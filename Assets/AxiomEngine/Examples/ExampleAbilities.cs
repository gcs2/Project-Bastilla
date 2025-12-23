// ============================================================================
// RPGPlatform.Examples - Example Ability Implementations
// Demonstrates how to create abilities using the system
// ============================================================================

using UnityEngine;
using RPGPlatform.Core;
using RPGPlatform.Systems.Combat;
using RPGPlatform.Data;
using SunEater.GameData;

namespace RPGPlatform.Examples
{
    /// <summary>
    /// Example ability data - create these as ScriptableObject assets
    /// </summary>
    public static class ExampleAbilities
    {
        /// <summary>
        /// Creates a basic melee attack ability
        /// </summary>
        public static AbilityData CreateBasicAttack()
        {
            var ability = ScriptableObject.CreateInstance<AbilityData>();
            ability.AbilityId = "basic_attack";
            ability.DisplayName = "Strike";
            ability.Description = "A basic melee attack.";
            ability.Type = AbilityType.Attack;
            ability.Targeting = TargetType.SingleEnemy;
            ability.DamageType = DamageType.Physical;
            ability.PrimaryStat = StatType.Strength;
            ability.ResourceCost = 0;
            ability.CooldownTurns = 0;
            ability.Range = 1;
            ability.DamageFormula = "1d6";
            ability.CritMultiplier = 2f;
            return ability;
        }
        
        /// <summary>
        /// Creates a power attack with higher damage but resource cost
        /// </summary>
        public static AbilityData CreatePowerAttack()
        {
            var ability = ScriptableObject.CreateInstance<AbilityData>();
            ability.AbilityId = "power_attack";
            ability.DisplayName = "Power Strike";
            ability.Description = "A devastating blow that deals extra damage.";
            ability.Type = AbilityType.Attack;
            ability.Targeting = TargetType.SingleEnemy;
            ability.DamageType = DamageType.Physical;
            ability.PrimaryStat = StatType.Strength;
            ability.ResourceCost = 15;
            ability.CooldownTurns = 2;
            ability.Range = 1;
            ability.DamageFormula = "2d8+4";
            ability.CritMultiplier = 3f;
            return ability;
        }
        
        /// <summary>
        /// Creates a ranged energy attack (Force/Magic style)
        /// </summary>
        public static AbilityData CreateEnergyBolt()
        {
            var ability = ScriptableObject.CreateInstance<AbilityData>();
            ability.AbilityId = "energy_bolt";
            ability.DisplayName = "Energy Bolt";
            ability.Description = "Launch a bolt of pure energy at your target.";
            ability.Type = AbilityType.Attack;
            ability.Targeting = TargetType.SingleEnemy;
            ability.DamageType = DamageType.Energy;
            ability.PrimaryStat = StatType.Intelligence;
            ability.ResourceCost = 10;
            ability.CooldownTurns = 0;
            ability.Range = 6;
            ability.DamageFormula = "1d8+2";
            ability.CritMultiplier = 2f;
            return ability;
        }
        
        /// <summary>
        /// Creates an AoE attack
        /// </summary>
        public static AbilityData CreateShockwave()
        {
            var ability = ScriptableObject.CreateInstance<AbilityData>();
            ability.AbilityId = "shockwave";
            ability.DisplayName = "Shockwave";
            ability.Description = "Release a wave of force that damages all nearby enemies.";
            ability.Type = AbilityType.Attack;
            ability.Targeting = TargetType.Area;
            ability.DamageType = DamageType.Physical;
            ability.PrimaryStat = StatType.Strength;
            ability.ResourceCost = 25;
            ability.CooldownTurns = 3;
            ability.Range = 0;  // Centered on self
            ability.AreaOfEffect = 2;
            ability.DamageFormula = "2d6";
            ability.CritMultiplier = 1.5f;
            return ability;
        }
        
        /// <summary>
        /// Creates a healing ability
        /// </summary>
        public static AbilityData CreateHeal()
        {
            var ability = ScriptableObject.CreateInstance<AbilityData>();
            ability.AbilityId = "heal";
            ability.DisplayName = "Healing Touch";
            ability.Description = "Restore health to yourself or an ally.";
            ability.Type = AbilityType.Buff;
            ability.Targeting = TargetType.SingleAlly;
            ability.DamageType = DamageType.Holy;  // Or could be separate HealingType
            ability.PrimaryStat = StatType.Wisdom;
            ability.ResourceCost = 20;
            ability.CooldownTurns = 1;
            ability.Range = 3;
            ability.DamageFormula = "2d6+3";  // Healing amount
            ability.CritMultiplier = 1f;
            return ability;
        }
        
        /// <summary>
        /// Creates a buff ability
        /// </summary>
        public static AbilityData CreateBattleCry()
        {
            var ability = ScriptableObject.CreateInstance<AbilityData>();
            ability.AbilityId = "battle_cry";
            ability.DisplayName = "Battle Cry";
            ability.Description = "Inspire all allies, increasing their strength.";
            ability.Type = AbilityType.Buff;
            ability.Targeting = TargetType.AllAllies;
            ability.PrimaryStat = StatType.Charisma;
            ability.ResourceCost = 30;
            ability.CooldownTurns = 5;
            ability.Range = 0;
            // Would have AppliedEffects list with strength buff
            return ability;
        }
        
        /// <summary>
        /// Creates a debuff ability
        /// </summary>
        public static AbilityData CreateWeaken()
        {
            var ability = ScriptableObject.CreateInstance<AbilityData>();
            ability.AbilityId = "weaken";
            ability.DisplayName = "Weaken";
            ability.Description = "Sap the target's strength, reducing their damage.";
            ability.Type = AbilityType.Debuff;
            ability.Targeting = TargetType.SingleEnemy;
            ability.PrimaryStat = StatType.Intelligence;
            ability.ResourceCost = 15;
            ability.CooldownTurns = 2;
            ability.Range = 4;
            // Would have AppliedEffects list with strength debuff
            return ability;
        }
        
        /// <summary>
        /// Creates a morality-gated ability (Light side)
        /// </summary>
        public static AbilityData CreateDivineSmite()
        {
            var ability = ScriptableObject.CreateInstance<AbilityData>();
            ability.AbilityId = "divine_smite";
            ability.DisplayName = "Divine Smite";
            ability.Description = "Channel righteous fury into a devastating holy attack. Requires high Humanity alignment.";
            ability.Type = AbilityType.Attack;
            ability.Targeting = TargetType.SingleEnemy;
            ability.DamageType = DamageType.Holy;
            ability.PrimaryStat = StatType.Charisma;
            ability.ResourceCost = 35;
            ability.CooldownTurns = 3;
            ability.Range = 1;
            ability.DamageFormula = "3d8+5";
            ability.CritMultiplier = 2f;
            
            // Morality requirement: Humanism >= 50
            ability.RequiredMoralityAxis = "humanism";
            ability.MinMoralityValue = 50f;
            ability.MaxMoralityValue = float.PositiveInfinity;
            
            return ability;
        }
        
        /// <summary>
        /// Creates a morality-gated ability (Dark side)
        /// </summary>
        public static AbilityData CreateDrainLife()
        {
            var ability = ScriptableObject.CreateInstance<AbilityData>();
            ability.AbilityId = "drain_life";
            ability.DisplayName = "Drain Life";
            ability.Description = "Steal the life force from your enemy. Requires embracing Transhumanism.";
            ability.Type = AbilityType.Attack;
            ability.Targeting = TargetType.SingleEnemy;
            ability.DamageType = DamageType.Void;
            ability.PrimaryStat = StatType.Intelligence;
            ability.ResourceCost = 25;
            ability.CooldownTurns = 2;
            ability.Range = 3;
            ability.DamageFormula = "2d6";
            ability.CritMultiplier = 1.5f;
            
            // Morality requirement: Transhumanism >= 50 (negative Humanism)
            ability.RequiredMoralityAxis = "humanism";
            ability.MinMoralityValue = float.NegativeInfinity;
            ability.MaxMoralityValue = -50f;
            
            return ability;
        }
    }
    
    /// <summary>
    /// Example status effect templates
    /// </summary>
    public static class ExampleStatusEffects
    {
        public static StatusEffectTemplate CreateBurning()
        {
            var effect = ScriptableObject.CreateInstance<StatusEffectTemplate>();
            effect.EffectId = "burning";
            effect.DisplayName = "Burning";
            effect.Description = "Taking fire damage each turn.";
            effect.BaseDuration = 3;
            effect.MaxStacks = 3;
            effect.IsDebuff = true;
            effect.IsDispellable = true;
            effect.DamagePerTurn = 5;
            effect.PeriodicDamageType = DamageType.Fire;
            return effect;
        }
        
        public static StatusEffectTemplate CreatePoisoned()
        {
            var effect = ScriptableObject.CreateInstance<StatusEffectTemplate>();
            effect.EffectId = "poisoned";
            effect.DisplayName = "Poisoned";
            effect.Description = "Taking poison damage and reduced constitution.";
            effect.BaseDuration = 5;
            effect.MaxStacks = 1;
            effect.IsDebuff = true;
            effect.IsDispellable = true;
            effect.DamagePerTurn = 3;
            effect.ConstitutionMod = -2;
            return effect;
        }
        
        public static StatusEffectTemplate CreateStrengthened()
        {
            var effect = ScriptableObject.CreateInstance<StatusEffectTemplate>();
            effect.EffectId = "strengthened";
            effect.DisplayName = "Strengthened";
            effect.Description = "Increased strength and damage.";
            effect.BaseDuration = 3;
            effect.MaxStacks = 1;
            effect.IsDebuff = false;
            effect.IsDispellable = true;
            effect.StrengthMod = 4;
            return effect;
        }
        
        public static StatusEffectTemplate CreateDefending()
        {
            var effect = ScriptableObject.CreateInstance<StatusEffectTemplate>();
            effect.EffectId = "defending";
            effect.DisplayName = "Defending";
            effect.Description = "Increased armor class from defensive stance.";
            effect.BaseDuration = 1;
            effect.MaxStacks = 1;
            effect.IsDebuff = false;
            effect.IsDispellable = false;
            effect.ArmorClassMod = 4;
            return effect;
        }
        
        public static StatusEffectTemplate CreateStunned()
        {
            var effect = ScriptableObject.CreateInstance<StatusEffectTemplate>();
            effect.EffectId = "stunned";
            effect.DisplayName = "Stunned";
            effect.Description = "Cannot take actions.";
            effect.BaseDuration = 1;
            effect.MaxStacks = 1;
            effect.IsDebuff = true;
            effect.IsDispellable = true;
            effect.PreventsActions = true;
            return effect;
        }
        
        public static StatusEffectTemplate CreateRooted()
        {
            var effect = ScriptableObject.CreateInstance<StatusEffectTemplate>();
            effect.EffectId = "rooted";
            effect.DisplayName = "Rooted";
            effect.Description = "Cannot move.";
            effect.BaseDuration = 2;
            effect.MaxStacks = 1;
            effect.IsDebuff = true;
            effect.IsDispellable = true;
            effect.PreventsMovement = true;
            return effect;
        }
        
        public static StatusEffectTemplate CreateRegeneration()
        {
            var effect = ScriptableObject.CreateInstance<StatusEffectTemplate>();
            effect.EffectId = "regeneration";
            effect.DisplayName = "Regeneration";
            effect.Description = "Healing each turn.";
            effect.BaseDuration = 5;
            effect.MaxStacks = 1;
            effect.IsDebuff = false;
            effect.IsDispellable = true;
            effect.HealingPerTurn = 5;
            return effect;
        }
    }
    
    /// <summary>
    /// Example combat setup demonstrating the full system
    /// </summary>
    public class ExampleCombatSetup : MonoBehaviour
    {
        [SerializeField] private CombatManager _combatManager;
        
        [ContextMenu("Start Example Combat")]
        public void StartExampleCombat()
        {
            // Create player character
            var playerStats = new CombatStats
            {
                Strength = 14,
                Dexterity = 12,
                Constitution = 14,
                Intelligence = 10,
                Wisdom = 12,
                Charisma = 10,
                MaxHealth = 50,
                CurrentHealth = 50,
                ArmorClass = 15,
                ProficiencyBonus = 2
            };
            
            var playerAbilities = new System.Collections.Generic.List<AbilityData>
            {
                ExampleAbilities.CreateBasicAttack(),
                ExampleAbilities.CreatePowerAttack(),
                ExampleAbilities.CreateHeal()
            };
            
            var player = CombatantFactory.CreateBasic("Hero", playerStats, Vector3.zero, true, 0);
            
            // Create enemy
            var enemyStats = new CombatStats
            {
                Strength = 12,
                Dexterity = 10,
                Constitution = 12,
                Intelligence = 8,
                Wisdom = 8,
                Charisma = 6,
                MaxHealth = 30,
                CurrentHealth = 30,
                ArmorClass = 12,
                ProficiencyBonus = 1
            };
            
            var enemy = CombatantFactory.CreateBasic("Goblin", enemyStats, new Vector3(5, 0, 0), false, 1);
            
            // Start combat
            var combatants = new System.Collections.Generic.List<ICombatant> { player, enemy };
            _combatManager.StartCombat(combatants);
        }
    }
}
