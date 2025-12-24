using System;
using System.Collections.Generic;
using UnityEngine;
using RPGPlatform.Core;
using RPGPlatform.Core.Dialogue;
using RPGPlatform.Systems.Combat;
using RPGPlatform.Systems.Morality;
using RPGPlatform.Core.Progression;
using RPGPlatform.Data;

namespace RPGPlatform.Tests
{
    public static class TestingCommon
    {
        public class MockCombatant : ICombatant
        {
            public string DisplayName { get; set; } = "Mock Combatant";
            public string Name => DisplayName;
            public string Id { get; set; } = "mock_id";
            public CombatStats Stats { get; set; } = new CombatStats();
            public CombatPosition Position { get; set; }
            public List<IAbility> InternalAbilities = new List<IAbility>();
            public IReadOnlyList<IAbility> Abilities => InternalAbilities;
            public List<IStatusEffect> InternalEffects = new List<IStatusEffect>();
            public IReadOnlyList<IStatusEffect> ActiveEffects => InternalEffects;
            public IResourcePool Resources { get; set; }
            public bool IsAlive { get; set; } = true;
            public bool IsPlayerControlled { get; set; } = false;
            public int Team { get; set; } = 1;
            public bool CanMove { get; set; } = true;
            public bool CanAct { get; set; } = true;

            public event Action<DamageResult> OnDamageReceived;
            public event Action<int> OnHealingReceived;
            public event Action<IStatusEffect> OnEffectAdded;
            public event Action<IStatusEffect> OnEffectRemoved;
            public event Action OnDefeated;

            public void ApplyDamage(DamageResult d) { OnDamageReceived?.Invoke(d); }
            public void TakeDamage(int a, DamageType t) { ApplyDamage(new DamageResult { BaseDamage = a, Type = t }); }
            public int Heal(int a) { OnHealingReceived?.Invoke(a); return a; }
            public void ApplyHealing(int a) { Heal(a); }
            public void AddStatusEffect(IStatusEffect e) { InternalEffects.Add(e); OnEffectAdded?.Invoke(e); }
            public void RemoveStatusEffect(string id) { InternalEffects.RemoveAll(e => e.EffectId == id); }
            public IStatusEffect ApplyStatusEffect(StatusEffectTemplate t, ICombatant s) => null;
            public void TickEffects() { }
            public void TriggerDefeat() { IsAlive = false; OnDefeated?.Invoke(); }
        }

        public class MockMoralityService : IMoralityService
        {
            public Dictionary<string, float> Values = new Dictionary<string, float>();
            public bool HasMorality => true;
            public float GetAxisValue(string id) => Values.ContainsKey(id) ? Values[id] : 0;
            public void ModifyAxis(string id, float delta) => Values[id] = GetAxisValue(id) + delta;
            public bool MeetsRequirement(string id, float? min, float? max) 
            {
                float val = GetAxisValue(id);
                if (min.HasValue && val < min.Value) return false;
                if (max.HasValue && val > max.Value) return false;
                return true;
            }
            public void Set(string id, float v) => Values[id] = v;
        }

        public class MockProgressionService : IProgressionService
        {
            public long CurrentXP { get; set; }
            public int CurrentLevel { get; set; } = 1;
            public long XPToNextLevel => 1000;
            public string CurrentTierId => "rookie";
            public float CurrentStatMultiplier => 1f;
            public void AddXP(long amount) { CurrentXP += amount; OnXPChanged?.Invoke(CurrentXP, XPToNextLevel); }
            public bool IsTierUnlocked(string t) => true;
            public event Action<int> OnLevelUp;
            public event Action<string> OnTierChanged;
            public event Action<long, long> OnXPChanged;
        }

        public class MockDialogueRepository : IDialogueRepository
        {
            public Dictionary<string, ConversationData> Conversations = new Dictionary<string, ConversationData>();
            public ConversationData LoadConversation(string id) => Conversations.ContainsKey(id) ? Conversations[id] : null;
        }

        public class MockQuestService : IQuestService
        {
            public Dictionary<string, bool> Flags = new Dictionary<string, bool>();
            public Dictionary<string, int> Steps = new Dictionary<string, int>();
            public HashSet<string> Completed = new HashSet<string>();

            public bool GetFlag(string id) => Flags.ContainsKey(id) ? Flags[id] : false;
            public void SetFlag(string id, bool v) => Flags[id] = v;
            public int GetQuestStep(string id) => Steps.ContainsKey(id) ? Steps[id] : 0;
            public void SetQuestStep(string id, int s) => Steps[id] = s;
            public bool IsQuestCompleted(string id) => Completed.Contains(id);
            public bool CanStartQuest(string id) => true;
            public void AcceptQuest(string id) => SetQuestStep(id, 1);
        }
    }
}
