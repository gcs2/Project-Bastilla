using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using RPGPlatform.Core;
using RPGPlatform.Systems.Combat;
using RPGPlatform.Data;

namespace RPGPlatform.Tests
{
    [TestFixture]
    public class CombatTests
    {
        private GameObject _holder;
        private TurnManager _turnManager;
        private CombatStateMachine _stateMachine;

        [SetUp]
        public void Setup()
        {
            _holder = new GameObject("TestHolder");
            _turnManager = _holder.AddComponent<TurnManager>();
            _stateMachine = _holder.AddComponent<CombatStateMachine>();
            _turnManager.Initialize(null); // No resolver for initiative tests
            _stateMachine.Initialize(_turnManager);
        }

        [TearDown]
        public void Teardown()
        {
            UnityEngine.Object.DestroyImmediate(_holder);
        }

        [Test]
        public void Test_InitiativeOrder()
        {
            var p1 = new MockCombatant("Player", 1, 10); // Dex 10
            var e1 = new MockCombatant("Enemy", 2, 18);  // Dex 18 (Fast)
            var p2 = new MockCombatant("SlowBro", 1, 8); // Dex 8

            _turnManager.StartCombat(new List<ICombatant> { p1, e1, p2 });

            // Order should be Enemy (18) -> Player (10) -> SlowBro (8)
            var order = _turnManager.TurnOrder;
            Assert.AreEqual("Enemy", order[0].DisplayName);
            Assert.AreEqual("Player", order[1].DisplayName);
            Assert.AreEqual("SlowBro", order[2].DisplayName);
        }

        [Test]
        public void Test_CombatStateMachine_Flow()
        {
            var hero = new MockCombatant("Hero", 0, 10);
            var villain = new MockCombatant("Villain", 1, 8);
            
            _stateMachine.StartCombat(new List<ICombatant> { hero, villain });

            // Phase should be Planning
            Assert.AreEqual(CombatPhase.Planning, _stateMachine.CurrentPhase);
            Assert.AreEqual("Hero", _turnManager.CurrentCombatant.DisplayName);

            // Verify Command Logic directly
            var resolver = new MockResolver();
            var abilityData = ScriptableObject.CreateInstance<AbilityData>();
            abilityData.DamageFormula = "10";
            var ability = new Ability(abilityData);
            
            var cmd = new AttackCommand(hero, villain, ability, resolver);
            var result = cmd.Execute();
            
            Assert.IsTrue(result.Success);
            Assert.IsTrue(villain.CurrentHealth < villain.MaxHealth);

            // Verify Turn Passing
            var endTurn = new EndTurnCommand(hero, _turnManager);
            endTurn.Execute();
            
            Assert.AreEqual("Villain", _turnManager.CurrentCombatant.DisplayName);
        }

        // Mocks
        private class MockCombatant : ICombatant
        {
            public string DisplayName { get; set; }
            public string Id => DisplayName;
            public int Team { get; set; }
            public CombatStats Stats { get; set; } = new CombatStats();
            public CombatPosition Position { get; set; }
            public IReadOnlyList<IAbility> Abilities => null;
            public IReadOnlyList<IStatusEffect> ActiveEffects => new List<IStatusEffect>();
            public IResourcePool Resources => null;
            public bool IsAlive => CurrentHealth > 0;
            public bool IsPlayerControlled => true;
            
            public int MaxHealth = 100;
            public int CurrentHealth = 100;

            public MockCombatant(string name, int team, int dex)
            {
                DisplayName = name;
                Team = team;
                Stats.Dexterity = dex;
            }

            public string Name => DisplayName;
            public void ApplyDamage(DamageResult d) { CurrentHealth -= d.FinalDamage; }
            public void TakeDamage(int a, DamageType t) { CurrentHealth -= a; }
            public int Heal(int a) { CurrentHealth += a; return a; }
            public void ApplyHealing(int a) { CurrentHealth += a; }
            public bool CanMove => true;
            public bool CanAct => true;
            public void AddStatusEffect(IStatusEffect effect) {}
            public void RemoveStatusEffect(string effectId) {}
            public IStatusEffect ApplyStatusEffect(StatusEffectTemplate template, ICombatant source) => null;
            public void TickEffects() {}
            public event System.Action<DamageResult> OnDamageReceived = delegate { };
            public event System.Action<int> OnHealingReceived = delegate { };
            public event System.Action<IStatusEffect> OnEffectAdded = delegate { };
            public event System.Action<IStatusEffect> OnEffectRemoved = delegate { };
            public event System.Action OnDefeated = delegate { };
        }

        private class MockResolver : ICombatResolver
        {
            public RollResult ResolveAttack(ICombatant a, ICombatant t, IAbility ability) 
            {
                return new RollResult { IsSuccess = true, Total = 20, NaturalRoll = 15 };
            }
            public DamageResult CalculateDamage(ICombatant a, ICombatant t, IAbility ability, RollResult r)
            {
                return new DamageResult { BaseDamage = 10 };
            }
            public bool ResolveSavingThrow(ICombatant t, SaveType s, int dc) => true;
            public RollResult ResolveCheck(ICombatant s, StatType st, int dc) => new RollResult { IsSuccess = true };
            public int RollInitiative(ICombatant c) => c.Stats.Dexterity;
            public int RollDice(int c, int s) => 5;
            public int RollDice(string f) => 5;
        }
    }
}
