// ============================================================================
// SunEater.Abilities - The Quiet
// Example of an ability gated by both Tier and Morality
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using RPGPlatform.Core;
using RPGPlatform.Systems.Progression;

namespace SunEater.Abilities
{
    public class TheQuietAbility : IAbility, IStatusEffect // Acts as both for simplicity in example
    {
        public string AbilityId => "SE_TheQuiet";
        public string DisplayName => "The Quiet";
        public string Description => "Slow time to a standstill. Requires Palatine rank and Humanist focus.";
        public Sprite Icon => null;
        public AbilityType Type => AbilityType.Utility;
        public TargetType Targeting => TargetType.AllEnemies;
        public DamageType DamageType => DamageType.Psychic;
        public StatType PrimaryStat => StatType.Wisdom;
        public int ResourceCost => 100;
        public int CooldownTurns => 10;
        public int CurrentCooldown { get; private set; }
        public int Range => 100;
        public int AreaOfEffect => 100;
        
        // Requirements
        public int RequiredLevel => 41; // Start of Palatine
        public string RequiredTierId => "palatine"; // Tier check
        public float? MinMoralityValue => 75f; // High Positive (Humanist)
        public float? MaxMoralityValue => null;
        public string RequiredMoralityAxis => "humanism";

        // Logic
        public bool CanUse(ICombatant user)
        {
            // 1. Check Tier
            var progression = ProgressionManager.Instance;
            if (progression != null) // If system exists
            {
                if (!progression.IsTierUnlocked(RequiredTierId))
                {
                    Debug.Log("Ability Locked: Must be Palatine Tier.");
                    return false;
                }
            }
            
            // 2. Check Morality (Usually handled by Decorator, but here for explicit checking)
            // (Assuming generic check passed or redundant check)
            
            return CurrentCooldown <= 0;
        }

        public bool IsValidTarget(ICombatant user, ICombatant target) => target.Team != user.Team;

        public void Use(ICombatant user)
        {
            Debug.Log(">>> THE QUIET ACTIVATED: TIME STOPS <<<");
            CurrentCooldown = CooldownTurns;
            // Apply "Time Stop" effect to all enemies...
        }

        public void OnUse() {} // Satisfy IAbility

        public void TickCooldown()
        {
            if (CurrentCooldown > 0) CurrentCooldown--;
        }

        // IStatusEffect stub
        public string DamageFormula => "";
        public float CritMultiplier => 1f;
        public List<StatusEffectData> AppliedEffects => null;
        // ... rest of interface stubs ...
        public string EffectId => "TimeStop";
        public int Duration => 3;
        public int StackCount => 1;
        public int MaxStacks => 1;
        public bool IsDebuff => true;
        public bool IsDispellable => false;
        public ICombatant Source { get; set; }
        public ICombatant Target { get; set; }
        public void OnApply() {}
        public void OnTick() {}
        public void OnRemove() {}
        public void AddStack() {}
        public CombatStats GetStatModifiers() => new CombatStats();
    }
}
