// ============================================================================
// RPGPlatform.Systems.Combat - Ability System
// Abilities with resource costs, cooldowns, and alignment requirements
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using RPGPlatform.Core;
using RPGPlatform.Data;

namespace RPGPlatform.Systems.Combat
{
    
    /// <summary>
    /// Runtime instance of an ability
    /// </summary>
    public class Ability : IAbility
    {
        private readonly AbilityData _data;
        private int _currentCooldown;
        private int _usesRemaining;
        
        #region IAbility Implementation
        
        public string AbilityId => _data.AbilityId;
        public string DisplayName => _data.DisplayName;
        public string Description => _data.Description;
        public Sprite Icon => _data.Icon;
        
        public AbilityType Type => _data.Type;
        public TargetType Targeting => _data.Targeting;
        public DamageType DamageType => _data.DamageType;
        public StatType PrimaryStat => _data.PrimaryStat;
        
        public int ResourceCost => _data.ResourceCost;
        public int CooldownTurns => _data.CooldownTurns;
        public int CurrentCooldown => _currentCooldown;
        public int Range => _data.Range;
        public int AreaOfEffect => _data.AreaOfEffect;
        
        public int RequiredLevel => _data.RequiredLevel;
        public float? MinMoralityValue => 
            float.IsNegativeInfinity(_data.MinMoralityValue) ? null : _data.MinMoralityValue;
        public float? MaxMoralityValue => 
            float.IsPositiveInfinity(_data.MaxMoralityValue) ? null : _data.MaxMoralityValue;
        public string RequiredMoralityAxis => _data.RequiredMoralityAxis;
        
        public string DamageFormula => _data.DamageFormula;
        public float CritMultiplier => _data.CritMultiplier;
        public List<StatusEffectData> AppliedEffects => new List<StatusEffectData>();
        
        #endregion
        
        public AbilityData Data => _data;
        
        public Ability(AbilityData data)
        {
            _data = data;
            _currentCooldown = 0;
            _usesRemaining = data.UsesPerCombat;
        }
        
        /// <summary>
        /// Check if the ability can be used by this combatant
        /// </summary>
        public bool CanUse(ICombatant user)
        {
            // Check if alive
            if (!user.IsAlive)
            {
                return false;
            }
            
            // Check cooldown
            if (_currentCooldown > 0)
            {
                return false;
            }
            
            // Check uses remaining
            if (_usesRemaining == 0)
            {
                return false;
            }
            
            // Check resource cost
            if (user.Resources != null && !user.Resources.CanAfford(ResourceCost))
            {
                return false;
            }
            
            // Check status effects (stunned, silenced, etc.)
            if (user is Combatant combatant && !combatant.CanAct)
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Check if a target is valid for this ability
        /// </summary>
        public bool IsValidTarget(ICombatant user, ICombatant target)
        {
            if (target == null || !target.IsAlive)
            {
                return Targeting == TargetType.Self; // Self-targeting doesn't need a live target
            }
            
            switch (Targeting)
            {
                case TargetType.Self:
                    return target == user;
                    
                case TargetType.SingleEnemy:
                    return target.Team != user.Team;
                    
                case TargetType.SingleAlly:
                    return target.Team == user.Team && target != user;
                    
                case TargetType.AllEnemies:
                case TargetType.AllAllies:
                case TargetType.Area:
                case TargetType.Line:
                case TargetType.Cone:
                    return true; // Multi-target validation is handled elsewhere
                    
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// Use the ability (deduct resources, start cooldown)
        /// </summary>
        public void Use(ICombatant user)
        {
            // Spend resource
            if (user.Resources != null && ResourceCost > 0)
            {
                user.Resources.TrySpend(ResourceCost);
            }
            
            // Start cooldown
            _currentCooldown = CooldownTurns;
            
            // Decrement uses
            if (_usesRemaining > 0)
            {
                _usesRemaining--;
            }
            
            Debug.Log($"[Ability] {user.DisplayName} uses {DisplayName}. " +
                     $"Cooldown: {_currentCooldown}, Uses: {_usesRemaining}");
        }

        public virtual void OnUse() { } // Track usage state if needed
        
        /// <summary>
        /// Reduce cooldown by 1 (called each turn)
        /// </summary>
        public void TickCooldown()
        {
            if (_currentCooldown > 0)
            {
                _currentCooldown--;
            }
        }
        
        /// <summary>
        /// Reset for new combat
        /// </summary>
        public void ResetForCombat()
        {
            _currentCooldown = 0;
            _usesRemaining = _data.UsesPerCombat;
        }
        
        /// <summary>
        /// Get reason why ability can't be used
        /// </summary>
        public string GetCannotUseReason(ICombatant user)
        {
            if (!user.IsAlive)
                return "You are defeated";
            
            if (_currentCooldown > 0)
                return $"On cooldown ({_currentCooldown} turns remaining)";
            
            if (_usesRemaining == 0)
                return "No uses remaining";
            
            if (user.Resources != null && !user.Resources.CanAfford(ResourceCost))
                return $"Not enough {user.Resources.ResourceName} (need {ResourceCost})";
            
            if (user is Combatant combatant && !combatant.CanAct)
                return "Cannot act (stunned)";
            
            return "Unknown reason";
        }
    }
    
    /// <summary>
    /// Validates ability morality requirements
    /// </summary>
    public static class AbilityMoralityValidator
    {
        public static bool MeetsMoralityRequirements(IAbility ability, IMoralityService morality)
        {
            if (ability == null) return true;
            if (morality == null) return true;
            if (!morality.HasMorality) return true;

            if (string.IsNullOrEmpty(ability.RequiredMoralityAxis)) return true;

            return morality.MeetsRequirement(
                ability.RequiredMoralityAxis, 
                ability.MinMoralityValue, 
                ability.MaxMoralityValue
            );
        }
    
        /// <summary>
        /// Get description of morality requirement
        /// </summary>
        public static string GetMoralityRequirementText(IAbility ability)
        {
            if (string.IsNullOrEmpty(ability.RequiredMoralityAxis))
                return "";
            
            string axis = ability.RequiredMoralityAxis;
            
            if (ability.MinMoralityValue.HasValue && ability.MaxMoralityValue.HasValue)
            {
                return $"Requires {axis} between {ability.MinMoralityValue} and {ability.MaxMoralityValue}";
            }
            else if (ability.MinMoralityValue.HasValue)
            {
                return $"Requires {axis} ≥ {ability.MinMoralityValue}";
            }
            else if (ability.MaxMoralityValue.HasValue)
            {
                return $"Requires {axis} ≤ {ability.MaxMoralityValue}";
            }
            
            return "";
        }
    }
}
