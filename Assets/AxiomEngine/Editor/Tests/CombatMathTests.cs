// ============================================================================
// Axiom RPG Engine - Combat Math Tests
// Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.
// ============================================================================

using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using RPGPlatform.Core;
using RPGPlatform.Systems.Combat;
using RPGPlatform.Data;

namespace RPGPlatform.Tests
{
    [TestFixture]
    public class CombatMathTests
    {
        private GameObject _holder;
        private D20CombatResolver _resolver;
        private Combatant _attacker;
        private Combatant _target;

        [SetUp]
        public void Setup()
        {
            _holder = new GameObject("TestHolder");
            _resolver = new D20CombatResolver(null); // No config needed for basic math tests

            // Setup Attacker
            var attackerStats = new CombatStats { Strength = 14, ProficiencyBonus = 2 }; // Mod +2
            _attacker = CombatantFactory.CreateBasic("Attacker", attackerStats, Vector3.zero, true, 0);

            // Setup Target
            var targetStats = new CombatStats { ArmorClass = 15, MaxHealth = 50, CurrentHealth = 50 };
            _target = CombatantFactory.CreateBasic("Target", targetStats, Vector3.right, false, 1);

            // Setup Dummy Ability for tests
            _dummyAbility = ScriptableObject.CreateInstance<AbilityData>();
            _dummyAbility.DamageFormula = "1d6";
            _dummyAbility.PrimaryStat = StatType.Strength;
            _dummyAbility.DamageType = DamageType.Physical;
        }

        private AbilityData _dummyAbility;

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_holder);
            Object.DestroyImmediate(_attacker.gameObject);
            Object.DestroyImmediate(_target.gameObject);
        }

        [Test]
        public void Test_ResolveAttack_Hit()
        {
            // With Str 14 (+2) and Prof (+2), total mod is +4. 
            // Target AC 15 needs a natural roll of 11+.
            // Since it's random, we'll loop until we get a hit to verify logic.
            // Or we check the RollResult for mathematical consistency.
            
            var result = _resolver.ResolveAttack(_attacker, _target, new Ability(_dummyAbility));
            
            Assert.AreEqual(result.NaturalRoll + result.Modifier, result.Total);
            if (result.NaturalRoll == 20) Assert.IsTrue(result.IsCriticalHit);
            if (result.NaturalRoll >= 11) Assert.IsTrue(result.IsSuccess, $"Roll {result.NaturalRoll} + 4 should hit AC 15");
        }

        [Test]
        public void Test_DamageCalculation()
        {
            var roll = new RollResult { IsSuccess = true, IsCriticalHit = false };
            var damage = _resolver.CalculateDamage(_attacker, _target, new Ability(_dummyAbility), roll);
            
            // Basic attack default is 1d6 + StrMod
            // Str 14 -> +2 mod.
            // 1d6 is 1-6. Total should be 3-8.
            Assert.GreaterOrEqual(damage.TotalDamage, 3);
            Assert.LessOrEqual(damage.TotalDamage, 8);
        }

        [Test]
        public void Test_CriticalDamage()
        {
            var roll = new RollResult { IsSuccess = true, IsCriticalHit = true };
            var damage = _resolver.CalculateDamage(_attacker, _target, new Ability(_dummyAbility), roll);
            
            // Default 1d6 + StrMod. If Crit, BaseDamage * 2.
            // (1-6)*2 + 2 = 4-14.
            Assert.GreaterOrEqual(damage.TotalDamage, 4);
            Assert.LessOrEqual(damage.TotalDamage, 14);
            Assert.IsTrue(damage.WasCritical);
        }

        [Test]
        public void Test_StatusEffect_DamageOverTime()
        {
            var poisonTemplate = ScriptableObject.CreateInstance<StatusEffectTemplate>();
            poisonTemplate.EffectId = "Poison";
            poisonTemplate.DisplayName = "Poison";
            poisonTemplate.BaseDuration = 2;
            poisonTemplate.DamagePerTurn = 5;
            poisonTemplate.PeriodicDamageType = DamageType.Physical;
            poisonTemplate.IsDebuff = true;

            _target.ApplyStatusEffect(poisonTemplate, _attacker);
            Assert.IsTrue(_target.HasEffect("Poison"));

            int initialHP = _target.Stats.CurrentHealth;
            _target.TickEffects(); // Turn 1 tick

            Assert.AreEqual(initialHP - 5, _target.Stats.CurrentHealth);
            Assert.AreEqual(1, _target.ActiveEffects[0].Duration);

            _target.TickEffects(); // Turn 2 tick (expires)
            Assert.AreEqual(initialHP - 10, _target.Stats.CurrentHealth);
            Assert.IsFalse(_target.HasEffect("Poison"));
        }

        [Test]
        public void Test_StatusEffect_StatBuff()
        {
            var hasteTemplate = ScriptableObject.CreateInstance<StatusEffectTemplate>();
            hasteTemplate.EffectId = "Haste";
            hasteTemplate.DisplayName = "Haste";
            hasteTemplate.BaseDuration = 3;
            hasteTemplate.DexterityMod = 4;
            hasteTemplate.ArmorClassMod = 2;

            int initialDex = _target.Stats.Dexterity;
            int initialAC = _target.Stats.ArmorClass;

            _target.ApplyStatusEffect(hasteTemplate, _attacker);

            Assert.AreEqual(initialDex + 4, _target.Stats.Dexterity);
            Assert.AreEqual(initialAC + 2, _target.Stats.ArmorClass);

            _target.TickEffects();
            _target.TickEffects();
            _target.TickEffects(); // Expired

            Assert.AreEqual(initialDex, _target.Stats.Dexterity);
            Assert.AreEqual(initialAC, _target.Stats.ArmorClass);
        }
    }
}
