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
        private TestingCommon.MockProgressionService _progression;

        [SetUp]
        public void Setup()
        {
            _holder = new GameObject("TestHolder");
            _manager = _holder.AddComponent<QuestManager>();
            _progression = new TestingCommon.MockProgressionService();
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

        [Test]
        public void Test_QuestPrerequisites()
        {
            var q1 = ScriptableObject.CreateInstance<QuestData>();
            q1.QuestId = "q1";
            
            var q2 = ScriptableObject.CreateInstance<QuestData>();
            q2.QuestId = "q2";
            q2.RequiredQuestIds.Add("q1");

            _manager.Initialize(_progression, new List<QuestData> { q1, q2 });

            // 1. Try start Q2 (should fail)
            _manager.AcceptQuest("q2");
            Assert.AreEqual(0, _manager.GetQuestStep("q2"));

            // 2. Complete Q1
            _manager.AcceptQuest("q1");
            _manager.CompleteQuest("q1");

            // 3. Try start Q2 again (should succeed)
            _manager.AcceptQuest("q2");
            Assert.AreEqual(1, _manager.GetQuestStep("q2"));
        }
    }
}
