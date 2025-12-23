using System;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using RPGPlatform.Core;
using RPGPlatform.Core.Dialogue;
using RPGPlatform.Systems.Morality;
using RPGPlatform.Systems.Combat;
using RPGPlatform.Systems.Dialogue;
using RPGPlatform.Data;

namespace RPGPlatform.Tests
{
    [TestFixture]
    public class SystemTests
    {
        private GameObject _holder;

        [SetUp]
        public void Setup()
        {
            _holder = new GameObject("SystemTestHolder");
        }

        [TearDown]
        public void Teardown()
        {
            UnityEngine.Object.DestroyImmediate(_holder);
        }

        [Test]
        public void Test_Morality_Scaling_And_Clamping()
        {
            var config = ScriptableObject.CreateInstance<MoralityAxisConfig>();
            config.AxisId = "humanism";
            config.MinValue = -100;
            config.MaxValue = 100;
            config.DefaultValue = 0;

            var state = new MoralityState();
            state.Initialize(config);

            Assert.AreEqual(0, state.GetAxisValue("humanism"));

            state.ModifyAxis("humanism", 50);
            Assert.AreEqual(50, state.GetAxisValue("humanism"));

            state.ModifyAxis("humanism", 100); 
            Assert.AreEqual(100, state.GetAxisValue("humanism"));

            state.ModifyAxis("humanism", -250);
            Assert.AreEqual(-100, state.GetAxisValue("humanism"));
        }

        [Test]
        public void Test_Dialogue_Alignment_Gating()
        {
            var mockRepo = new MockDialogueRepository();
            var manager = _holder.AddComponent<DialogueManager>();
            var mockMorality = new MockMoralityService();
            manager.Initialize(mockMorality, null, null, null, mockRepo);

            var conversation = ScriptableObject.CreateInstance<ConversationData>();
            conversation.ConversationId = "unit_test_convo";
            conversation.Nodes = new List<DialogueNode>
            {
                new DialogueNode 
                { 
                    NodeId = "root", Text = "Root Node",
                    Responses = new List<DialogueResponse> 
                    {
                        new DialogueResponse { Text = "Normal", NextNodeId = "end" },
                        new DialogueResponse 
                        { 
                            Text = "Locked", NextNodeId = "end",
                            Conditions = new List<IDialogueCondition> 
                            { 
                                new MoralityCondition { AxisId = "humanism", RequiredValue = 50, Comparison = ComparisonType.Greater } 
                            }
                        }
                    }
                },
                new DialogueNode { NodeId = "end", Text = "End" }
            };
            conversation.EntryNodeId = "root";
            mockRepo.Conversations["unit_test_convo"] = conversation;

            manager.StartConversation("unit_test_convo", null, new MockCombatant());
            
            // Initially 1 response
            Assert.AreEqual(1, manager.GetValidResponses().Count);

            // Level up morality
            mockMorality.Set("humanism", 100);
            Assert.AreEqual(2, manager.GetValidResponses().Count);
        }

        // Mocks
        private class MockDialogueRepository : IDialogueRepository
        {
            public Dictionary<string, ConversationData> Conversations = new Dictionary<string, ConversationData>();
            public ConversationData LoadConversation(string id) => Conversations.ContainsKey(id) ? Conversations[id] : null;
        }

        private class MockMoralityService : IMoralityService
        {
            private Dictionary<string, float> _vals = new Dictionary<string, float>();
            public bool HasMorality => true;
            public float GetAxisValue(string id) => _vals.ContainsKey(id) ? _vals[id] : 0;
            public void ModifyAxis(string id, float d) => _vals[id] = GetAxisValue(id) + d;
            public bool MeetsRequirement(string id, float? min, float? max) => true; 
            public void Set(string id, float v) => _vals[id] = v;
        }

        private class MockCombatant : ICombatant 
        { 
            public string DisplayName => "Mock NPC";
            public string Name => DisplayName;
            public string Id => "mock";
            public CombatStats Stats => new CombatStats();
            public CombatPosition Position { get; set; }
            public IReadOnlyList<IAbility> Abilities => null;
            public IReadOnlyList<IStatusEffect> ActiveEffects => new List<IStatusEffect>();
            public IResourcePool Resources => null;
            public bool IsAlive => true; public bool IsPlayerControlled => false; public int Team => 1;
            public bool CanMove => true; public bool CanAct => true;
            public void ApplyDamage(DamageResult d) {} 
            public void TakeDamage(int a, DamageType t) {}
            public int Heal(int a) => a;
            public void ApplyHealing(int a) {} 
            public void AddStatusEffect(IStatusEffect e) {}
            public void RemoveStatusEffect(string e) {}
            public IStatusEffect ApplyStatusEffect(StatusEffectTemplate t, ICombatant s) => null;
            public void TickEffects() {}
            public event System.Action<DamageResult> OnDamageReceived;
            public event System.Action<int> OnHealingReceived; 
            public event System.Action<IStatusEffect> OnEffectAdded; 
            public event System.Action<IStatusEffect> OnEffectRemoved; 
            public event System.Action OnDefeated;
        }
    }
}
