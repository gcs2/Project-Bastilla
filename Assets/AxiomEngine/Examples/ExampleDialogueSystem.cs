// ============================================================================
// ExampleDialogueSystem.cs
// Demonstrates the Dialogue System with Skill, Alignment, and Influence checks
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using RPGPlatform.Core;
using RPGPlatform.Core.Dialogue;
using RPGPlatform.Data;
using RPGPlatform.Systems.Dialogue;

namespace RPGPlatform.Examples
{
    public class ExampleDialogueSystem : MonoBehaviour
    {
        // Mocks
        private class MockCombatant : ICombatant 
        { 
            public string DisplayName { get; set; } = "Player";
            public string Id => "player";
            public CombatStats Stats => new CombatStats();
            public CombatPosition Position { get; set; }
            public IReadOnlyList<IAbility> Abilities => null;
            public IReadOnlyList<IStatusEffect> ActiveEffects => null;
            public IResourcePool Resources => null;
            public bool IsAlive => true; public bool IsPlayerControlled => true; public int Team => 0;
            public string Name => DisplayName;
            public void ApplyDamage(DamageResult d) {}
            public void TakeDamage(int a, DamageType t) {}
            public int Heal(int a) => a;
            public void ApplyHealing(int a) {} 
            public bool CanMove => true;
            public bool CanAct => true;
            public void AddStatusEffect(IStatusEffect e) {} 
            public void RemoveStatusEffect(string e) {}
            public IStatusEffect ApplyStatusEffect(StatusEffectTemplate t, ICombatant s) => null;
            public void TickEffects() {}
            public event System.Action<DamageResult> OnDamageReceived = delegate { };
            public event System.Action<int> OnHealingReceived = delegate { }; 
            public event System.Action<IStatusEffect> OnEffectAdded = delegate { }; 
            public event System.Action<IStatusEffect> OnEffectRemoved = delegate { }; 
            public event System.Action OnDefeated = delegate { };
        }

        private class MockQuestService : IQuestService
        {
            public bool GetFlag(string f) => false;
            public void SetFlag(string f, bool v) {}
            public int GetQuestStep(string q) => 0;
            public void SetQuestStep(string q, int s) {}
            public bool IsQuestCompleted(string q) => false;
            public bool CanStartQuest(string q) => true;
            public void AcceptQuest(string q) {}
        }

        class MockDialogueRepository : IDialogueRepository
        {
            public Dictionary<string, ConversationData> Conversations = new Dictionary<string, ConversationData>();
            public ConversationData LoadConversation(string id) => Conversations.ContainsKey(id) ? Conversations[id] : null;
        }

        class MockSkills : ISkillService
        {
            public int GetSkillLevel(string id) => 5;
            public bool PerformCheck(string id, int dc) => true;
            public int GetSkillLevel(ICombatant t, string s) => 5;
            public bool PerformSkillCheck(ICombatant t, string s, int dc) => true;
        }

        class MockInfluence : IInfluenceService
        {
            public int GetInfluence(string NPCId) => 50;
            public void ModifyInfluence(string NPCId, int delta) {}
        }

        class MockMorality : IMoralityService
        {
            public bool HasMorality => true;
            public float GetAxisValue(string id) => 50;
            public void ModifyAxis(string id, float d) {}
            public bool MeetsRequirement(string id, float? min, float? max) => true;
        }

        private IMoralityService _morality;
        private IQuestService _quests;
        private ISkillService _skills;
        private IInfluenceService _influence;
        private IDialogueRepository _repository;

        private void Start()
        {
            // Setup Services
            _morality = new MockMorality();
            _skills = new MockSkills();
            _influence = new MockInfluence();
            _quests = new MockQuestService();
            _repository = new MockDialogueRepository();

            var player = new MockCombatant { DisplayName = "Zephy" };
            var npc = new MockCombatant { DisplayName = "Merchant" };

            DialogueManager.Instance.Initialize(_morality, _quests, _skills, _influence, _repository);

            // Create Conversation Data
            var convo = ScriptableObject.CreateInstance<ConversationData>();
            convo.ConversationId = "test_convo";
            convo.EntryNodeId = "start";
            
            // Nodes
            var startNode = new DialogueNode 
            { 
                NodeId = "start", 
                Text = "Hello there. What do you want?", 
                Responses = new List<DialogueResponse>() 
            };
            
            // Option 1: Normal
            startNode.Responses.Add(new DialogueResponse 
            { 
                Text = "Just browsing.", 
                NextNodeId = "browsing" 
            });

            // Option 2: Skill Check (Persuade > 5)
            var persuadeResp = new DialogueResponse 
            { 
                Text = "[Persuade] Give me a discount.", 
                NextNodeId = "discount_success",
                DisplayType = ResponseDisplayType.SkillCheck
            };
            persuadeResp.Conditions.Add(new SkillCondition { SkillId = "Persuade", DifficultyClass = 5 });
            startNode.Responses.Add(persuadeResp);

            // Option 3: Alignment (Humanist > 50)
            var humanistResp = new DialogueResponse
            {
                Text = "[Humanist] You should help the needy.",
                NextNodeId = "charity",
                DisplayType = ResponseDisplayType.AlignmentCheck
            };
            humanistResp.Conditions.Add(new MoralityCondition { AxisId = "humanism", Comparison = ComparisonType.Greater, RequiredValue = 50 });
            startNode.Responses.Add(humanistResp);
            
            // Interjection: Companion "Vara" interrupts if Influence > 10
            // We model this as an AUTO response on the Start Node
            var interjectionResp = new DialogueResponse
            {
                Text = "", // Auto, usually no text shown or specific indicator
                NextNodeId = "vara_interrupt",
                DisplayType = ResponseDisplayType.Auto
            };
            interjectionResp.Conditions.Add(new CompanionInfluenceCondition { CompanionId = "Vara", MinInfluence = 10 });
            startNode.Responses.Add(interjectionResp);

            convo.Nodes.Add(startNode);
            convo.Nodes.Add(new DialogueNode { NodeId = "browsing", Text = "Look around then." });
            convo.Nodes.Add(new DialogueNode { NodeId = "discount_success", Text = "Fine, 10% off." });
            convo.Nodes.Add(new DialogueNode { NodeId = "charity", Text = "Bless you." });
            convo.Nodes.Add(new DialogueNode { NodeId = "vara_interrupt", SpeakerOverride = "Vara", Text = "Don't listen to him, he's a cheat!" });

            // RUN TEST 1: Basic
            Debug.Log("--- Test 1: Basic ---");
            DialogueManager.Instance.StartConversation(convo, player, npc);
            // Should see "Just browsing"
            // Persuade (0 vs 5) -> Fail/Hidden?
            // Humanist (0 vs 50) -> Fail
            // Vara (0 vs 10) -> Fail
            
            // RUN TEST 2: High Influence (Interjection)
            Debug.Log("--- Test 2: Vara Interjection ---");
            _influence.ModifyInfluence("Vara", 20); // Now 20
            DialogueManager.Instance.StartConversation(convo, player, npc);
            // Should verify that Auto response triggers
            
        }
    }
}
