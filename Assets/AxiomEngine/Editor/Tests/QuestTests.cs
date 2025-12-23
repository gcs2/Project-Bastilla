// ============================================================================
// RPGPlatform.Tests - Quest Tests
// Verifies Quest Acceptance, Objective Updates, and Completion
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using RPGPlatform.Core;
using RPGPlatform.Core.Progression;
using RPGPlatform.Data;
using RPGPlatform.Systems.Quests;

namespace RPGPlatform.Tests
{
    [TestFixture]
    public class QuestTests
    {
        private GameObject _holder;
        private QuestManager _manager;
        private MockProgressionService _progression;

        [SetUp]
        public void Setup()
        {
            _holder = new GameObject("TestHolder");
            _manager = _holder.AddComponent<QuestManager>();
            _progression = new MockProgressionService();
        }

        [TearDown]
        public void Teardown()
        {
            UnityEngine.Object.DestroyImmediate(_holder);
        }

        [Test]
        public void Test_QuestCycle_Flow()
        {
            // Setup Quest Data
            var quest = ScriptableObject.CreateInstance<QuestData>();
            quest.QuestId = "q_kill_rats";
            quest.XPReward = 500;
            
            _manager.Initialize(_progression, new List<QuestData> { quest });

            // 1. Start Quest
            _manager.SetQuestStep("q_kill_rats", 1);
            Assert.AreEqual(1, _manager.GetQuestStep("q_kill_rats"));

            // 2. Update Progress
            _manager.SetQuestStep("q_kill_rats", 2);
            Assert.AreEqual(2, _manager.GetQuestStep("q_kill_rats"));

            // 3. Complete Quest
            _manager.CompleteQuest("q_kill_rats");
            
            Assert.IsTrue(_manager.IsQuestCompleted("q_kill_rats"));
            Assert.AreEqual(500, _progression.CurrentXP);

            // 4. Double Completion Check
            _manager.CompleteQuest("q_kill_rats");
            Assert.AreEqual(500, _progression.CurrentXP);
        }

        private class MockProgressionService : IProgressionService
        {
            public long CurrentXP { get; set; } = 0;
            public int CurrentLevel => 1;
            public long XPToNextLevel => 1000;
            public string CurrentTierId => "initiate";
            public float CurrentStatMultiplier => 1f;

            public void AddXP(long amount) 
            {
                CurrentXP += amount;
            }

            public bool IsTierUnlocked(string t) => true;

            public event System.Action<int> OnLevelUp;
            public event System.Action<string> OnTierChanged;
            public event System.Action<long, long> OnXPChanged;
        }
    }
}
