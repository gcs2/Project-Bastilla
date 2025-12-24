using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using RPGPlatform.Core;
using RPGPlatform.Systems.Progression;
using RPGPlatform.Data;
using SunEater.Items;

namespace RPGPlatform.Tests
{
    [TestFixture]
    public class ProgressionTests
    {
        private GameObject _holder;
        private ProgressionManager _manager;
        private ProgressionConfig _config;

        [SetUp]
        public void Setup()
        {
            _holder = new GameObject("TestHolder");
            _config = ScriptableObject.CreateInstance<ProgressionConfig>();
            _config.MaxLevel = 50;
            _config.BaseXP = 100;
            _config.XPExponent = 2.0f;
            _config.Tiers = new List<PowerTier>
            {
                new PowerTier { TierId = "initiate", MinLevel = 1, StatMultiplier = 1.0f },
                new PowerTier { TierId = "knight", MinLevel = 11, StatMultiplier = 1.5f },
                new PowerTier { TierId = "champion", MinLevel = 26, StatMultiplier = 2.0f },
                new PowerTier { TierId = "palatine", MinLevel = 41, StatMultiplier = 3.0f }
            };

            _manager = _holder.AddComponent<ProgressionManager>();
            _manager.Initialize(_config);
        }

        [TearDown]
        public void Teardown()
        {
            UnityEngine.Object.DestroyImmediate(_holder);
        }

        [Test]
        public void Test_Leveling_Curve()
        {
            Assert.AreEqual(1, _manager.CurrentLevel);
            Assert.AreEqual("initiate", _manager.CurrentTierId);

            _manager.AddXP(100);
            Assert.AreEqual(2, _manager.CurrentLevel);

            _manager.AddXP(9900); // Total 10000
            Assert.AreEqual(11, _manager.CurrentLevel);
            Assert.AreEqual("knight", _manager.CurrentTierId);
            Assert.IsTrue(Mathf.Approximately(1.5f, _manager.CurrentStatMultiplier));
        }

        [Test]
        public void Test_SunEater_ItemGeneration()
        {
            var dumbFighter = new MockCombatant { Stats = { Intelligence = 8, Wisdom = 8 } };
            var sword1 = HighMatterSwordGenerator.Generate(dumbFighter);
            Assert.AreEqual("Unstable", sword1.Rarity);

            var godMind = new MockCombatant { Stats = { Intelligence = 24, Wisdom = 20 } };
            var sword2 = HighMatterSwordGenerator.Generate(godMind);
            Assert.AreEqual("Ascendant", sword2.Rarity);
            Assert.AreEqual(12, sword2.BaseDamage);
        }

        private class MockCombatant : ICombatant 
        { 
            public string DisplayName => "Mock";
            public string Name => DisplayName;
            public string Id => "mock";
            public CombatStats Stats { get; set; } = new CombatStats();
            public CombatPosition Position { get; set; }
            public IReadOnlyList<IAbility> Abilities => null;
            public IReadOnlyList<IStatusEffect> ActiveEffects => new List<IStatusEffect>();
            public IResourcePool Resources => null;
            public bool IsAlive => true; public bool IsPlayerControlled => true; public int Team => 0;
            public bool CanMove => true; public bool CanAct => true;
            public void ApplyDamage(DamageResult d) {} 
            public void TakeDamage(int a, DamageType t) {}
            public int Heal(int a) => a;
            public void ApplyHealing(int a) {} 
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
    }
}
