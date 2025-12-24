using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using RPGPlatform.Core;
using RPGPlatform.Core.Dialogue;
using RPGPlatform.Systems.Combat;
using RPGPlatform.Systems.Quests;
using RPGPlatform.Systems.Dialogue;
using RPGPlatform.Systems.Morality;
using RPGPlatform.Data;

namespace RPGPlatform.Tests
{
    [TestFixture]
    public class SubsystemInDepthTests
    {
        private GameObject _holder;

        [SetUp]
        public void Setup()
        {
            _holder = new GameObject("InDepthTestHolder");
        }

        [TearDown]
        public void Teardown()
        {
            UnityEngine.Object.DestroyImmediate(_holder);
        }

        [Test]
        public void Test_StatusEffect_StackingAndDuration()
        {
            var combatant = _holder.AddComponent<Combatant>();
            combatant.Initialize(new CombatStats { MaxHealth = 100 }, new List<AbilityData>(), false, 1);
            
            var template = ScriptableObject.CreateInstance<StatusEffectTemplate>();
            template.EffectId = "bleed";
            template.BaseDuration = 3;
            template.MaxStacks = 5;

            // 1. Initial Apply
            combatant.ApplyStatusEffect(template, combatant);
            Assert.AreEqual(1, combatant.ActiveEffects.Count);
            Assert.AreEqual(1, combatant.ActiveEffects[0].StackCount);
            Assert.AreEqual(3, combatant.ActiveEffects[0].Duration);

            // 2. Stack Apply
            combatant.ApplyStatusEffect(template, combatant);
            Assert.AreEqual(1, combatant.ActiveEffects.Count, "Should stack, not add new entry");
            Assert.AreEqual(2, combatant.ActiveEffects[0].StackCount);

            // 3. Ticking
            combatant.TickEffects();
            Assert.AreEqual(2, combatant.ActiveEffects[0].Duration);
            
            combatant.TickEffects();
            combatant.TickEffects();
            Assert.AreEqual(0, combatant.ActiveEffects.Count, "Effect should be removed at 0 duration");
        }

        [Test]
        public void Test_ResourcePool_SpendAndRegen()
        {
            var config = ScriptableObject.CreateInstance<ResourceConfig>();
            config.ResourceId = "mana";
            config.DisplayName = "Mana";
            config.BaseMaximum = 100;
            config.MaximumPerLevel = 0;
            config.RegenPerTurn = 10;
            config.RegeneratesInCombat = true;

            var pool = new ResourcePool(config, 1, 10); // Level 1, 10 Constitution (0 bonus)
            
            Assert.AreEqual(100, pool.Current);

            bool spent = pool.TrySpend(40);
            Assert.IsTrue(spent);
            Assert.AreEqual(60, pool.Current);

            bool spentTooMuch = pool.TrySpend(70);
            Assert.IsFalse(spentTooMuch);
            Assert.AreEqual(60, pool.Current);

            pool.Tick();
            Assert.AreEqual(70, pool.Current);
        }

        [Test]
        public void Test_Quest_EventsAndRewards()
        {
            var manager = _holder.AddComponent<QuestManager>();
            var progression = new TestingCommon.MockProgressionService();
            
            var quest = ScriptableObject.CreateInstance<QuestData>();
            quest.QuestId = "test_quest";
            quest.XPReward = 1000;

            manager.Initialize(progression, new List<QuestData> { quest });

            bool startedCalled = false;
            bool completedCalled = false;
            manager.OnQuestStarted += (id) => startedCalled = true;
            manager.OnQuestCompleted += (id) => completedCalled = true;

            manager.SetQuestStep("test_quest", 1);
            Assert.IsTrue(startedCalled);

            manager.CompleteQuest("test_quest");
            Assert.IsTrue(completedCalled);
            Assert.AreEqual(1000, progression.CurrentXP);

            // Prevent double reward
            manager.CompleteQuest("test_quest");
            Assert.AreEqual(1000, progression.CurrentXP);
        }

        [Test]
        public void Test_Dialogue_Interjections()
        {
            var manager = _holder.AddComponent<DialogueManager>();
            var mor = new TestingCommon.MockMoralityService();
            var repo = new TestingCommon.MockDialogueRepository();
            manager.Initialize(mor, null, null, null, repo);

            var convo = ScriptableObject.CreateInstance<ConversationData>();
            convo.ConversationId = "interject_convo";
            convo.Nodes = new List<DialogueNode>
            {
                new DialogueNode 
                { 
                    NodeId = "start", Text = "Hello",
                    Responses = new List<DialogueResponse>
                    {
                        new DialogueResponse 
                        { 
                            Text = "Interjection", NextNodeId = "secret", 
                            DisplayType = ResponseDisplayType.Auto,
                            Conditions = new List<IDialogueCondition>
                            {
                                new MoralityCondition { AxisId = "humanism", RequiredValue = 10, Comparison = ComparisonType.Greater }
                            }
                        },
                        new DialogueResponse { Text = "Normal", NextNodeId = "end" }
                    }
                },
                new DialogueNode { NodeId = "secret", Text = "You found me!" },
                new DialogueNode { NodeId = "end", Text = "Bye" }
            };
            convo.EntryNodeId = "start";
            repo.Conversations["interject_convo"] = convo;

            // Scenario 1: No interjection
            mor.ModifyAxis("humanism", 0);
            DialogueNode receivedNode = null;
            manager.OnNodeStart += (n) => receivedNode = n;

            manager.StartConversation("interject_convo", null, new TestingCommon.MockCombatant());
            Assert.AreEqual("Hello", receivedNode.Text);

            // Scenario 2: Interjection happens automatically
            mor.Set("humanism", 50);
            manager.StartConversation("interject_convo", null, new TestingCommon.MockCombatant());
            Assert.AreEqual("You found me!", receivedNode.Text, "Dialogue should have skipped 'Hello' node due to Auto response");
        }

        [Test]
        public void Test_MoralityState_EventsAndClamping()
        {
            var morality = new MoralityState();
            var config = ScriptableObject.CreateInstance<MoralityAxisConfig>();
            config.AxisId = "light_dark";
            config.DisplayName = "Alignment";
            config.MinValue = 0;
            config.MaxValue = 100;
            config.DefaultValue = 50;

            morality.Initialize(config);
            
            string changedAxis = null;
            float changedValue = -1f;
            morality.OnAxisChanged += (id, val) => {
                changedAxis = id;
                changedValue = val;
            };

            // 1. Test Modification
            morality.ModifyAxis("light_dark", 10);
            Assert.AreEqual("light_dark", changedAxis);
            Assert.AreEqual(60f, changedValue);
            Assert.AreEqual(60f, morality.GetAxisValue("light_dark"));

            // 2. Test Clamping
            morality.ModifyAxis("light_dark", 100);
            Assert.AreEqual(100f, morality.GetAxisValue("light_dark"));

            morality.ModifyAxis("light_dark", -200);
            Assert.AreEqual(0f, morality.GetAxisValue("light_dark"));
        }

        [Test]
        public void Test_ServiceLocator_Flow()
        {
            ServiceLocator.Clear();
            var mockMorality = new TestingCommon.MockMoralityService();
            
            // 1. Register
            ServiceLocator.Register<IMoralityService>(mockMorality);
            Assert.IsTrue(ServiceLocator.IsRegistered<IMoralityService>());
            Assert.AreEqual(1, ServiceLocator.Count);

            // 2. Resolve
            var resolved = ServiceLocator.Get<IMoralityService>();
            Assert.AreSame(mockMorality, resolved);

            // 3. Unregister
            ServiceLocator.Unregister<IMoralityService>();
            Assert.IsFalse(ServiceLocator.IsRegistered<IMoralityService>());
            Assert.AreEqual(0, ServiceLocator.Count);
        }
    }
}
