// ============================================================================
// RPGPlatform.Examples - Morality System Examples
// Demonstrates usage of morality system, decorator pattern, and Sun Eater configuration
// ============================================================================

using UnityEngine;
using RPGPlatform.Core;
using RPGPlatform.Combat;
using RPGPlatform.Data;
using RPGPlatform.Systems.Dialogue;
using RPGPlatform.Core.Dialogue;
using RPGPlatform.Systems.Audio;
using RPGPlatform.Systems.Combat;
using RPGPlatform.Systems.Morality;
using System.Collections.Generic;
using SunEater.GameData;

namespace RPGPlatform.Examples
{
    /// <summary>
    /// Example demonstrating the complete morality system implementation
    /// Shows MoralityState, PowerService decorator, and Sun Eater configuration
    /// </summary>
    public class ExampleMoralitySystems : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CombatManager _combatManager;
        
        [Header("Configuration")]
        [SerializeField] private MoralityAxisConfig _humanismAxis;
        
        private MoralityState _moralityState;
        private SunEaterMoralityConfig _sunEaterConfig;
        private IPowerService _powerService;
        
        [ContextMenu("Example 1: Basic Morality State")]
        public void Example1_BasicMoralityState()
        {
            Debug.Log("=== Example 1: Basic Morality State ===");
            
            // Create morality state
            var morality = new MoralityState();
            
            // Create axis config
            var axis = CreateHumanismAxis();
            
            // Initialize with axis
            morality.Initialize(axis);
            
            // Get initial value
            float value = morality.GetAxisValue("humanism");
            Debug.Log($"Initial humanism value: {value}");
            
            // Make some choices
            morality.ModifyAxis("humanism", 25f);  // Humanist choice
            Debug.Log($"After humanist choice: {morality.GetAxisValue("humanism")}");
            
            morality.ModifyAxis("humanism", -50f); // Transhumanist choice
            Debug.Log($"After transhumanist choice: {morality.GetAxisValue("humanism")}");
            
            // Check requirements
            bool meetsHumanist = morality.MeetsRequirement("humanism", 50f, null);
            bool meetsTranshumanist = morality.MeetsRequirement("humanism", null, -50f);
            
            Debug.Log($"Meets humanist requirement (>= 50): {meetsHumanist}");
            Debug.Log($"Meets transhumanist requirement (<= -50): {meetsTranshumanist}");
        }
        
        [ContextMenu("Example 2: PowerService Decorator Pattern")]
        public void Example2_PowerServiceDecorator()
        {
            Debug.Log("=== Example 2: PowerService Decorator Pattern ===");
            
            // Setup morality
            var morality = new MoralityState();
            morality.Initialize(CreateHumanismAxis());
            morality.SetAxisValue("humanism", -75f); // High transhumanist
            
            // Create base power service
            var resolver = new D20CombatResolver(CreateResolverConfig());
            IPowerService baseService = new PowerService(resolver);
            
            // Wrap with morality decorator
            IPowerService decoratedService = new MoralityCheckDecorator(baseService, morality);
            
            // Create test combatant
            var combatant = CreateTestCombatant();
            
            // Create transhumanist-only ability
            var drainLife = ExampleAbilities.CreateDrainLife();
            
            // Test execution
            bool canExecute = decoratedService.CanExecute(combatant, new Ability(drainLife));
            Debug.Log($"Can execute Drain Life (requires transhumanist): {canExecute}");
            
            // Create humanist-only ability
            var divineSmite = CreateDivineSmite();
            
            bool canExecuteDrain = decoratedService.CanExecute(combatant, new Ability(drainLife));
            bool canExecuteSmite = decoratedService.CanExecute(combatant, new Ability(divineSmite));
            Debug.Log($"Can execute Divine Smite (requires humanist): {canExecuteSmite}");
        }
        
        [ContextMenu("Example 3: Sun Eater Configuration")]
        public void Example3_SunEaterConfiguration()
        {
            Debug.Log("=== Example 3: Sun Eater Configuration ===");
            
            // Create Sun Eater config
            var config = gameObject.AddComponent<SunEaterMoralityConfig>();
            
            // Create test combatant
            var combatant = CreateTestCombatant();
            
            Debug.Log($"Initial stats: STR {combatant.Stats.Strength}, DEX {combatant.Stats.Dexterity}, INT {combatant.Stats.Intelligence}");
            
            // Make transhumanist choices
            config.MakeMoralityChoice(-30f, "Installed neural implant");
            config.MakeMoralityChoice(-30f, "Replaced organic limbs with cybernetics");
            config.MakeMoralityChoice(-30f, "Uploaded consciousness backup");
            
            // Apply effects
            config.ApplyMoralityEffects(combatant);
            
            Debug.Log($"After cybernetic enhancements: STR {combatant.Stats.Strength}, DEX {combatant.Stats.Dexterity}, INT {combatant.Stats.Intelligence}");
            
            // Show full status
            Debug.Log(config.GetStatusReport());
        }
        
        [ContextMenu("Example 4: Transhumanist Buffs")]
        public void Example4_TranshumanistBuffs()
        {
            Debug.Log("=== Example 4: Transhumanist Buffs ===");
            
            var morality = new MoralityState();
            morality.Initialize(CreateHumanismAxis());
            
            var buffSystem = new TranshumanistBuffSystem(morality, "humanism");
            var combatant = CreateTestCombatant();
            
            // Test different tiers
            Debug.Log("--- Minor Transhumanist (-30) ---");
            morality.SetAxisValue("humanism", -30f);
            buffSystem.ApplyBuffs(combatant);
            Debug.Log(buffSystem.GetBuffDescription());
            
            Debug.Log("\n--- Moderate Transhumanist (-60) ---");
            morality.SetAxisValue("humanism", -60f);
            buffSystem.ApplyBuffs(combatant);
            Debug.Log(buffSystem.GetBuffDescription());
            
            Debug.Log("\n--- Major Transhumanist (-90) ---");
            morality.SetAxisValue("humanism", -90f);
            buffSystem.ApplyBuffs(combatant);
            Debug.Log(buffSystem.GetBuffDescription());
        }
        
        [ContextMenu("Example 5: Chantry Infamy")]
        public void Example5_ChantryInfamy()
        {
            Debug.Log("=== Example 5: Chantry Infamy ===");
            
            var morality = new MoralityState();
            morality.Initialize(CreateHumanismAxis());
            
            var infamySystem = new ChantryInfamySystem(morality, "humanism");
            
            // Test different levels
            Debug.Log("--- Neutral (0) ---");
            morality.SetAxisValue("humanism", 0f);
            Debug.Log(infamySystem.GetInfamyDescription());
            Debug.Log($"Has Chantry access: {infamySystem.HasChantryAccess()}");
            Debug.Log($"Can enter temples: {infamySystem.CanEnterTemples()}");
            
            Debug.Log("\n--- Suspicious (-30) ---");
            morality.SetAxisValue("humanism", -30f);
            Debug.Log(infamySystem.GetInfamyDescription());
            Debug.Log($"Social penalty: {infamySystem.GetSocialPenalty()}");
            Debug.Log($"Price multiplier: {infamySystem.GetPriceMultiplier()}x");
            
            Debug.Log("\n--- Condemned (-60) ---");
            morality.SetAxisValue("humanism", -60f);
            Debug.Log(infamySystem.GetInfamyDescription());
            Debug.Log($"Has Chantry access: {infamySystem.HasChantryAccess()}");
            
            Debug.Log("\n--- Heretic (-90) ---");
            morality.SetAxisValue("humanism", -90f);
            Debug.Log(infamySystem.GetInfamyDescription());
            Debug.Log($"Can enter temples: {infamySystem.CanEnterTemples()}");
        }
        
        [ContextMenu("Example 6: Service Registration")]
        public void Example6_ServiceRegistration()
        {
            Debug.Log("=== Example 6: Service Registration ===");
            
            // Create services
            var morality = new MoralityState();
            morality.Initialize(CreateHumanismAxis());
            
            var resolver = new D20CombatResolver(CreateResolverConfig());
            IPowerService powerService = new PowerService(resolver);
            powerService = new MoralityCheckDecorator(powerService, morality);
            
            // Register services
            ServiceLocator.Register<IMoralityService>(morality);
            ServiceLocator.Register<IPowerService>(powerService);
            
            Debug.Log($"Registered services: {ServiceLocator.Count}");
            
            // Retrieve services
            var retrievedMorality = ServiceLocator.Get<IMoralityService>();
            var retrievedPower = ServiceLocator.Get<IPowerService>();
            
            Debug.Log($"Retrieved morality service: {retrievedMorality != null}");
            Debug.Log($"Retrieved power service: {retrievedPower != null}");
            
            // Use services
            retrievedMorality.ModifyAxis("humanism", -50f);
            Debug.Log($"Morality value: {retrievedMorality.GetAxisValue("humanism")}");
        }
        
        [ContextMenu("Example 7: Complete Integration")]
        public void Example7_CompleteIntegration()
        {
            Debug.Log("=== Example 7: Complete Integration ===");
            
            // Setup Sun Eater configuration
            var sunEater = gameObject.AddComponent<SunEaterMoralityConfig>();
            
            // Register morality service
            ServiceLocator.Register<IMoralityService>(sunEater.MoralityState);
            
            // Create power service with decorator
            var resolver = new D20CombatResolver(CreateResolverConfig());
            IPowerService powerService = new PowerService(resolver);
            powerService = new MoralityCheckDecorator(powerService, sunEater.MoralityState);
            ServiceLocator.Register<IPowerService>(powerService);
            
            // Create combatant
            var player = CreateTestCombatant();
            
            // Make transhumanist choices
            sunEater.MakeMoralityChoice(-80f, "Fully embraced cybernetic augmentation");
            
            // Apply effects
            sunEater.ApplyMoralityEffects(player);
            
            // Test abilities
            var drainLife = ExampleAbilities.CreateDrainLife();
            var divineSmite = CreateDivineSmite();
            
            Debug.Log($"\nTesting abilities with high transhumanist alignment:");
            Debug.Log($"Can use Drain Life (transhumanist): {powerService.CanExecute(player, new Ability(drainLife))}");
            Debug.Log($"Can use Divine Smite (humanist): {powerService.CanExecute(player, new Ability(divineSmite))}");
            
            // Show full status
            Debug.Log($"\n{sunEater.GetStatusReport()}");
        }
        
        #region Helper Methods
        
        private MoralityAxisConfig CreateHumanismAxis()
        {
            if (_humanismAxis != null)
                return _humanismAxis;
            
            var config = ScriptableObject.CreateInstance<MoralityAxisConfig>();
            config.AxisId = "humanism";
            config.DisplayName = "Humanist vs Transhumanist";
            config.PositivePoleLabel = "Humanist";
            config.NegativePoleLabel = "Transhumanist";
            config.MinValue = -100f;
            config.MaxValue = 100f;
            config.DefaultValue = 0f;
            
            return config;
        }
        
        private ICombatant CreateTestCombatant()
        {
            var stats = new CombatStats
            {
                Strength = 12,
                Dexterity = 10,
                Constitution = 12,
                Intelligence = 10,
                Wisdom = 10,
                Charisma = 10,
                MaxHealth = 40,
                CurrentHealth = 40,
                ArmorClass = 14,
                ProficiencyBonus = 2
            };
            
            return CombatantFactory.CreateBasic("Test Character", stats, Vector3.zero, true, 1);
        }
        
        private AbilityData CreateDivineSmite()
        {
            var ability = ScriptableObject.CreateInstance<AbilityData>();
            ability.AbilityId = "divine_smite";
            ability.DisplayName = "Divine Smite";
            ability.Description = "Channel divine energy to smite your foe. Requires Humanist alignment.";
            ability.Type = AbilityType.Attack;
            ability.Targeting = TargetType.SingleEnemy;
            ability.DamageType = DamageType.Holy;
            ability.PrimaryStat = StatType.Wisdom;
            ability.ResourceCost = 25;
            ability.CooldownTurns = 2;
            ability.Range = 1;
            ability.DamageFormula = "3d6";
            ability.CritMultiplier = 2f;
            
            // Morality requirement: Humanist >= 50
            ability.RequiredMoralityAxis = "humanism";
            ability.MinMoralityValue = 50f;
            ability.MaxMoralityValue = float.PositiveInfinity;
            
            return ability;
        }
        
        private D20ResolverConfig CreateResolverConfig()
        {
            var config = ScriptableObject.CreateInstance<D20ResolverConfig>();
            config.CriticalThreshold = 20;
            config.UseCriticalMisses = true;
            
            return config;
        }
        
        #endregion
    }

    /// <summary>
    /// Wrapper for TranshumanistBuffSystem using MoralityEffectManager
    /// </summary>
    public class TranshumanistBuffSystem
    {
        private MoralityEffectManager _manager;
        public TranshumanistBuffSystem(MoralityState state, string axis) 
        {
            _manager = new MoralityEffectManager(state, axis);
        }
        public void ApplyBuffs(ICombatant target) => _manager.ApplyEffects(target);
        public string GetBuffDescription() => _manager.GetEffectDescription();
    }

    /// <summary>
    /// Wrapper for ChantryInfamySystem using MoralityEffectManager
    /// </summary>
    public class ChantryInfamySystem
    {
        private MoralityEffectManager _manager;
        public ChantryInfamySystem(MoralityState state, string axis)
        {
            _manager = new MoralityEffectManager(state, axis);
        }
        public string GetInfamyDescription() => _manager.GetEffectDescription();
        public bool HasChantryAccess() => _manager.AllowsFactionAccess();
        public bool CanEnterTemples() => _manager.AllowsTerritoryAccess();
        public int GetSocialPenalty() => _manager.GetSocialPenalty();
        public float GetPriceMultiplier() => _manager.GetPriceMultiplier();
    }
}
