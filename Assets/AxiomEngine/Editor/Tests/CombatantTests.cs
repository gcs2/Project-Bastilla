using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using RPGPlatform.Core;
using RPGPlatform.Systems.Combat;
using RPGPlatform.Data;

namespace RPGPlatform.Tests
{
    [TestFixture]
    public class CombatantTests
    {
        private GameObject _holder;
        private Combatant _combatant;

        [SetUp]
        public void Setup()
        {
            _holder = new GameObject("CombatantTestHolder");
            _combatant = _holder.AddComponent<Combatant>();
            
            var stats = new CombatStats { MaxHealth = 100, CurrentHealth = 100 };
            _combatant.Initialize(stats, new List<AbilityData>(), false, 1);
        }

        [TearDown]
        public void Teardown()
        {
            UnityEngine.Object.DestroyImmediate(_holder);
        }

        [Test]
        public void Test_DamageAndHealth()
        {
            Assert.AreEqual(100, _combatant.Stats.CurrentHealth);
            Assert.IsTrue(_combatant.IsAlive);

            _combatant.TakeDamage(30, DamageType.Physical);
            Assert.AreEqual(70, _combatant.Stats.CurrentHealth);
            Assert.IsTrue(_combatant.IsAlive);

            _combatant.TakeDamage(80, DamageType.Energy);
            Assert.AreEqual(0, _combatant.Stats.CurrentHealth);
            Assert.IsFalse(_combatant.IsAlive);
        }

        [Test]
        public void Test_Healing()
        {
            _combatant.TakeDamage(50, DamageType.Physical);
            Assert.AreEqual(50, _combatant.Stats.CurrentHealth);

            int healed = _combatant.Heal(30);
            Assert.AreEqual(30, healed);
            Assert.AreEqual(80, _combatant.Stats.CurrentHealth);

            // Cap at max health
            healed = _combatant.Heal(50);
            Assert.AreEqual(20, healed);
            Assert.AreEqual(100, _combatant.Stats.CurrentHealth);
        }

        [Test]
        public void Test_DefeatTrigger()
        {
            bool defeatedEventFired = false;
            _combatant.OnDefeated += () => defeatedEventFired = true;

            _combatant.TakeDamage(100, DamageType.Physical);
            Assert.IsTrue(defeatedEventFired);
            Assert.IsFalse(_combatant.IsAlive);
        }

        [Test]
        public void Test_InitializationWithData()
        {
            var data = ScriptableObject.CreateInstance<CombatantData>();
            data.Id = "test_npc";
            data.BaseName = "Test NPC";
            data.BaseStats = new CombatStats { MaxHealth = 200, CurrentHealth = 200 };
            
            var combatant = _holder.AddComponent<Combatant>();
            combatant.Initialize(data, false, 2);

            Assert.AreEqual("test_npc", combatant.Id);
            Assert.AreEqual("Test NPC", combatant.DisplayName);
            Assert.AreEqual(200, combatant.Stats.MaxHealth);
            Assert.AreEqual(2, combatant.Team);
        }
    }
}
