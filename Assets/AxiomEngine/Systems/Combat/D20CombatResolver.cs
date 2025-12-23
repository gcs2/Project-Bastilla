// ============================================================================
// Axiom RPG Engine - D20 Combat Resolver
// Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.
// ============================================================================

using System;
using System.Text.RegularExpressions;
using UnityEngine;
using RPGPlatform.Core;

namespace RPGPlatform.Systems.Combat
{
    /// <summary>
    /// Configuration for the D20 combat system
    /// </summary>
    [CreateAssetMenu(fileName = "D20Config", menuName = "RPG/Combat/D20 Config")]
    public class D20ResolverConfig : ScriptableObject
    {
        [Header("Critical Hits")]
        [Range(1, 20)]
        public int CriticalThreshold = 20;      // Roll this or higher = crit
        public float CriticalMultiplier = 2f;
        
        [Header("Critical Misses")]
        public bool UseCriticalMisses = true;   // Natural 1 = auto-miss
        
        [Header("Attack Bonuses")]
        public bool UseProficiencyBonus = true;
        public bool UseStatModifiers = true;
        
        [Header("Armor Class")]
        public int BaseAC = 10;                 // AC when wearing nothing
        public bool DexterityAffectsAC = true;
    }
    
    /// <summary>
    /// D20-based combat resolver (like D&D/Pathfinder)
    /// Roll d20 + modifiers vs AC/DC
    /// </summary>
    public class D20CombatResolver : ICombatResolver
    {
        private readonly D20ResolverConfig _config;
        private readonly System.Random _rng;
        
        // Events for UI/logging
        public event Action<string> OnRollMade;
        
        public D20CombatResolver(D20ResolverConfig config)
        {
            _config = config ?? CreateDefaultConfig();
            _rng = new System.Random();
        }
        
        private static D20ResolverConfig CreateDefaultConfig()
        {
            var config = ScriptableObject.CreateInstance<D20ResolverConfig>();
            config.CriticalThreshold = 20;
            config.CriticalMultiplier = 2f;
            config.UseCriticalMisses = true;
            config.UseProficiencyBonus = true;
            config.UseStatModifiers = true;
            config.BaseAC = 10;
            config.DexterityAffectsAC = true;
            return config;
        }
        
        /// <summary>
        /// Resolve an attack roll
        /// </summary>
        public RollResult ResolveAttack(ICombatant attacker, ICombatant target, IAbility ability)
        {
            int naturalRoll = RollD20();
            int modifier = CalculateAttackModifier(attacker, ability);
            int total = naturalRoll + modifier;
            int targetAC = CalculateEffectiveAC(target);
            
            // Determine hit/miss with critical rules
            bool isCriticalHit = naturalRoll >= _config.CriticalThreshold;
            bool isCriticalMiss = _config.UseCriticalMisses && naturalRoll == 1;
            
            // Natural 20 always hits, natural 1 always misses
            bool isSuccess;
            if (isCriticalHit)
                isSuccess = true;
            else if (isCriticalMiss)
                isSuccess = false;
            else
                isSuccess = total >= targetAC;
            
            var result = new RollResult
            {
                NaturalRoll = naturalRoll,
                Modifier = modifier,
                Total = total,
                TargetNumber = targetAC,
                IsCriticalHit = isCriticalHit && isSuccess,  // Only crit if hit
                IsCriticalMiss = isCriticalMiss,
                IsSuccess = isSuccess
            };
            
            OnRollMade?.Invoke($"{attacker.DisplayName} attacks {target.DisplayName}: {result}");
            
            return result;
        }
        
        /// <summary>
        /// Calculate damage for a successful hit
        /// </summary>
        public DamageResult CalculateDamage(ICombatant attacker, ICombatant target, 
                                             IAbility ability, RollResult roll)
        {
            // Parse and roll the damage formula
            int baseDamage = RollDice(ability.DamageFormula);
            
            // Add stat modifier to damage
            int statBonus = 0;
            if (_config.UseStatModifiers)
            {
                statBonus = attacker.Stats.GetModifier(ability.PrimaryStat);
            }
            
            // Calculate multiplier (criticals)
            float multiplier = 1f;
            if (roll.IsCriticalHit)
            {
                multiplier = ability.CritMultiplier > 0 ? ability.CritMultiplier : _config.CriticalMultiplier;
            }
            
            // Calculate resistance/absorption
            int resistance = CalculateResistance(target, ability.DamageType);
            
            var result = new DamageResult
            {
                BaseDamage = baseDamage,
                BonusDamage = statBonus,
                DamageMultiplier = multiplier,
                Resisted = resistance,
                Absorbed = 0,  // Could be expanded for shield mechanics
                Type = ability.DamageType,
                WasCritical = roll.IsCriticalHit
            };
            
            OnRollMade?.Invoke($"Damage: {baseDamage} + {statBonus} × {multiplier} - {resistance} resistance = {result.TotalDamage}");
            
            return result;
        }
        
        /// <summary>
        /// Roll a saving throw
        /// </summary>
        public bool ResolveSavingThrow(ICombatant target, SaveType saveType, int dc)
        {
            int naturalRoll = RollD20();
            int modifier = target.Stats.GetSaveBonus(saveType);
            int total = naturalRoll + modifier;
            
            // Natural 20 always succeeds, natural 1 always fails
            bool success;
            if (naturalRoll == 20)
                success = true;
            else if (naturalRoll == 1)
                success = false;
            else
                success = total >= dc;
            
            OnRollMade?.Invoke($"{target.DisplayName} {saveType} save: {naturalRoll} + {modifier} = {total} vs DC {dc} → {(success ? "SUCCESS" : "FAILURE")}");
            
            return success;
        }

        /// <summary>
        /// Roll a general attribute check
        /// </summary>
        public RollResult ResolveCheck(ICombatant source, StatType stat, int dc)
        {
            int naturalRoll = RollD20();
            int modifier = source.Stats.GetModifier(stat);
            int total = naturalRoll + modifier;
            bool isSuccess = total >= dc;

            var result = new RollResult 
            {
                NaturalRoll = naturalRoll,
                Modifier = modifier,
                Total = total,
                TargetNumber = dc,
                IsSuccess = isSuccess
            };

            OnRollMade?.Invoke($"{source.DisplayName} {stat} check: {result}");
            return result;
        }
        
        /// <summary>
        /// Roll initiative for turn order
        /// </summary>
        public int RollInitiative(ICombatant combatant)
        {
            int roll = RollD20();
            int modifier = combatant.Stats.GetModifier(StatType.Dexterity) + combatant.Stats.InitiativeBonus;
            int total = roll + modifier;
            
            OnRollMade?.Invoke($"{combatant.DisplayName} initiative: {roll} + {modifier} = {total}");
            
            return total;
        }
        
        /// <summary>
        /// Roll multiple dice of the same type
        /// </summary>
        public int RollDice(int count, int sides)
        {
            int total = 0;
            for (int i = 0; i < count; i++)
            {
                total += _rng.Next(1, sides + 1);
            }
            return total;
        }
        
        /// <summary>
        /// Roll dice from a formula string like "2d6+3" or "1d8+STR"
        /// </summary>
        public int RollDice(string formula)
        {
            if (string.IsNullOrEmpty(formula))
                return 0;
            
            // Pattern: XdY+Z or XdY-Z or XdY
            var match = Regex.Match(formula.ToLower(), @"(\d+)d(\d+)([+-]\d+)?");
            
            if (!match.Success)
            {
                // Try to parse as plain number
                if (int.TryParse(formula, out int plainValue))
                    return plainValue;
                return 0;
            }
            
            int count = int.Parse(match.Groups[1].Value);
            int sides = int.Parse(match.Groups[2].Value);
            int modifier = 0;
            
            if (match.Groups[3].Success)
            {
                modifier = int.Parse(match.Groups[3].Value);
            }
            
            int rollTotal = RollDice(count, sides);
            
            OnRollMade?.Invoke($"Rolling {formula}: {rollTotal} + {modifier} = {rollTotal + modifier}");
            
            return rollTotal + modifier;
        }
        
        #region Private Helpers
        
        private int RollD20()
        {
            return _rng.Next(1, 21);
        }
        
        private int CalculateAttackModifier(ICombatant attacker, IAbility ability)
        {
            int modifier = 0;
            
            if (_config.UseStatModifiers)
            {
                modifier += attacker.Stats.GetModifier(ability.PrimaryStat);
            }
            
            if (_config.UseProficiencyBonus)
            {
                modifier += attacker.Stats.ProficiencyBonus;
            }
            
            // Add bonuses from status effects
            foreach (var effect in attacker.ActiveEffects)
            {
                var mods = effect.GetStatModifiers();
                if (mods != null)
                {
                    // Could add attack bonus tracking here
                }
            }
            
            return modifier;
        }
        
        private int CalculateEffectiveAC(ICombatant target)
        {
            int ac = target.Stats.ArmorClass;
            
            // Apply status effect modifiers
            foreach (var effect in target.ActiveEffects)
            {
                var mods = effect.GetStatModifiers();
                if (mods != null)
                {
                    ac += mods.ArmorClass;
                }
            }
            
            return ac;
        }
        
        private int CalculateResistance(ICombatant target, DamageType damageType)
        {
            // This could be expanded with a resistance system
            // For now, return 0
            return 0;
        }
        
        #endregion
    }
}
