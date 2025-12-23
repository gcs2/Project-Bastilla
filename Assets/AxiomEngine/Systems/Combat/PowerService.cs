// ============================================================================
// RPGPlatform.Combat - Base Power Service Implementation
// Core ability execution logic
// ============================================================================

using UnityEngine;
using RPGPlatform.Core;

namespace RPGPlatform.Combat
{
    /// <summary>
    /// Base implementation of IPowerService
    /// Handles core ability execution logic including resource costs and cooldowns
    /// </summary>
    public class PowerService : IPowerService
    {
        private readonly ICombatResolver _resolver;
        private readonly IPositioningSystem _positioning;
        
        public PowerService(ICombatResolver resolver, IPositioningSystem positioning = null)
        {
            _resolver = resolver;
            _positioning = positioning;
        }
        
        /// <summary>
        /// Check if an ability can be executed
        /// </summary>
        public virtual bool CanExecute(ICombatant user, IAbility ability)
        {
            if (user == null || ability == null)
            {
                Debug.LogWarning("[PowerService] Null user or ability");
                return false;
            }
            
            // Check if ability can be used (handles cooldowns, uses remaining, etc.)
            if (!ability.CanUse(user))
            {
                return false;
            }
            
            // Check resource cost
            if (ability.ResourceCost > 0)
            {
                if (user.Resources == null || !user.Resources.CanAfford(ability.ResourceCost))
                {
                    Debug.Log($"[PowerService] {user.DisplayName} cannot afford {ability.DisplayName} (cost: {ability.ResourceCost})");
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Execute an ability against a target
        /// </summary>
        public virtual CommandResult Execute(ICombatant user, ICombatant target, IAbility ability)
        {
            if (!CanExecute(user, ability))
            {
                return CommandResult.Failure($"{ability.DisplayName} cannot be used");
            }
            
            // Spend resources
            if (ability.ResourceCost > 0 && user.Resources != null)
            {
                user.Resources.TrySpend(ability.ResourceCost);
            }
            
            // Execute the ability based on type
            CommandResult result;
            
            switch (ability.Type)
            {
                case AbilityType.Attack:
                    result = ExecuteAttack(user, target, ability);
                    break;
                    
                case AbilityType.Buff:
                case AbilityType.Heal:
                    result = ExecuteBuff(user, target, ability);
                    break;
                    
                case AbilityType.Debuff:
                    result = ExecuteDebuff(user, target, ability);
                    break;
                    
                default:
                    result = CommandResult.Failure($"Unknown ability type: {ability.Type}");
                    break;
            }
            
            // Mark ability as used
            ability.OnUse();
            
            return result;
        }
        
        /// <summary>
        /// Execute an attack ability
        /// </summary>
        protected virtual CommandResult ExecuteAttack(ICombatant user, ICombatant target, IAbility ability)
        {
            if (target == null)
            {
                return CommandResult.Failure("No target specified");
            }
            
            // Check range if positioning system is available
            if (_positioning != null && ability.Range > 0)
            {
                float distance = _positioning.GetDistance(user, target);
                if (distance > ability.Range)
                {
                    return CommandResult.Failure($"Target out of range (distance: {distance:F1}, range: {ability.Range})");
                }
            }
            
            // Resolve attack
            var rollResult = _resolver.ResolveAttack(user, target, ability);
            
            if (!rollResult.IsSuccess)
            {
                return CommandResult.Miss(user, target, ability, rollResult);
            }
            
            // Calculate and apply damage
            var damageResult = _resolver.CalculateDamage(user, target, ability, rollResult);
            target.TakeDamage(damageResult.TotalDamage, ability.DamageType);
            
            return CommandResult.Hit(user, target, ability, rollResult, damageResult);
        }
        
        /// <summary>
        /// Execute a buff or healing ability
        /// </summary>
        protected virtual CommandResult ExecuteBuff(ICombatant user, ICombatant target, IAbility ability)
        {
            if (target == null)
            {
                target = user; // Self-buff if no target
            }
            
            // For healing abilities, use damage formula as healing amount
            if (ability.Type == AbilityType.Heal || ability.Type == AbilityType.Buff)
            {
                // Parse healing amount from damage formula
                int healAmount = ParseHealingAmount(ability.DamageFormula);
                
                if (healAmount > 0)
                {
                    int actualHealed = target.Heal(healAmount);
                    Debug.Log($"[PowerService] {ability.DisplayName} healed {target.Name} for {actualHealed} HP");
                }
            }
            
            // Apply status effects if any
            // (This would integrate with the status effect system)
            
            return new CommandResult
            {
                Success = true,
                Message = $"{user.DisplayName} used {ability.DisplayName} on {target.DisplayName}"
            };
        }
        
        /// <summary>
        /// Execute a debuff ability
        /// </summary>
        protected virtual CommandResult ExecuteDebuff(ICombatant user, ICombatant target, IAbility ability)
        {
            if (target == null)
            {
                return CommandResult.Failure("No target specified");
            }
            
            // Resolve attack (debuffs can be resisted)
            var rollResult = _resolver.ResolveAttack(user, target, ability);
            
            if (!rollResult.IsSuccess)
            {
                return CommandResult.Miss(user, target, ability, rollResult);
            }
            
            // Apply status effects
            // (This would integrate with the status effect system)
            
            return new CommandResult
            {
                Success = true,
                Message = $"{user.DisplayName} debuffed {target.DisplayName} with {ability.DisplayName}"
            };
        }
        
        /// <summary>
        /// Parse healing amount from damage formula (simplified)
        /// </summary>
        private int ParseHealingAmount(string formula)
        {
            if (string.IsNullOrEmpty(formula))
                return 0;
            
            // Simple parser for "XdY+Z" format
            // For production, use the dice roller from combat resolver
            try
            {
                if (formula.Contains("d"))
                {
                    var parts = formula.Split('d');
                    int numDice = int.Parse(parts[0]);
                    int diceSize = int.Parse(parts[1].Split('+')[0]);
                    int bonus = 0;
                    
                    if (parts[1].Contains("+"))
                    {
                        bonus = int.Parse(parts[1].Split('+')[1]);
                    }
                    
                    // Roll dice
                    int total = 0;
                    for (int i = 0; i < numDice; i++)
                    {
                        total += Random.Range(1, diceSize + 1);
                    }
                    
                    return total + bonus;
                }
                else
                {
                    return int.Parse(formula);
                }
            }
            catch
            {
                Debug.LogWarning($"[PowerService] Could not parse healing formula: {formula}");
                return 0;
            }
        }
    }
}
