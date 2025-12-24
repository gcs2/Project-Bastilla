// ============================================================================
// RPGPlatform.Editor.Tests - Demo Bootstrapper Tests
// Verifies the initialization and gameplay loop of the Sun Eater Demo
// ============================================================================

using NUnit.Framework;
using UnityEngine;
using SunEater.Demo;
using RPGPlatform.Systems.Combat;
using RPGPlatform.Systems.Dialogue;
using RPGPlatform.Systems.Quests;
using RPGPlatform.Systems.Morality;
using RPGPlatform.Data;
using RPGPlatform.Core;
using RPGPlatform.Core.Dialogue;
using System.Collections.Generic;

namespace RPGPlatform.Tests
{
    [TestFixture]
    public class DemoBootstrapperTests
    {
        private GameObject _root;
        private PlayableDemoBootstrapper _bootstrapper;
        private GameObject _player;
        private GameObject _inquisitor;
        private ConversationData _dialogueData;
        private QuestData _questData;

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Clear();
            _root = new GameObject("TestRoot");
            
            // Add Managers
            _root.AddComponent<CombatManager>();
            _root.AddComponent<DialogueManager>();
            _root.AddComponent<QuestManager>();
            
            _bootstrapper = _root.AddComponent<PlayableDemoBootstrapper>();
            
            _player = new GameObject("Player");
            _player.AddComponent<Combatant>();
            
            _inquisitor = new GameObject("Inquisitor");
            _inquisitor.AddComponent<Combatant>();
            
            _dialogueData = ScriptableObject.CreateInstance<ConversationData>();
            _dialogueData.ConversationId = "inquisitor_spectacle";
            
            _questData = ScriptableObject.CreateInstance<QuestData>();
            _questData.QuestId = "vorgossos_heretic";
            
            _bootstrapper.Configure(_player, _inquisitor, _dialogueData, _questData);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            Object.DestroyImmediate(_player);
            Object.DestroyImmediate(_inquisitor);
        }

        [Test]
        public void Test_SystemInitialization()
        {
            // Act
            // We simulate Start() by calling the private method or just letting Unity do its thing 
            // but in EditMode we usually have to call init methods manually.
            // PlayableDemoBootstrapper calls InitializeSystems in Start.
            
            _bootstrapper.SendMessage("Start"); // Force Unity lifecycle

            // Assert
            Assert.IsTrue(ServiceLocator.IsRegistered<IMoralityService>(), "Morality service should be registered.");
            Assert.IsTrue(ServiceLocator.IsRegistered<IQuestService>(), "Quest service should be registered.");
            Assert.IsTrue(ServiceLocator.IsRegistered<IDialogueService>(), "Dialogue service should be registered.");
        }

        [Test]
        public void Test_DialogueToCombatTransition()
        {
            // Arrange
            _bootstrapper.SendMessage("Start");
            var dialogueManager = _root.GetComponent<DialogueManager>();
            var combatManager = _root.GetComponent<CombatManager>();
            
            bool combatStarted = false;
            combatManager.OnCombatStarted += () => combatStarted = true;

            // Act
            // Simulate dialogue ending on the spectacle ID
            _bootstrapper.SendMessage("OnDialogueEnded");

            // Assert
            Assert.IsTrue(combatStarted, "Combat should have started after dialogue ended on the spectacle ID.");
        }

        [Test]
        public void Test_QuestCompletionOnVictory()
        {
            // Arrange
            _bootstrapper.SendMessage("Start");
            var combatManager = _root.GetComponent<CombatManager>();
            var questManager = _root.GetComponent<QuestManager>();
            
            // Start combat first
            _bootstrapper.SendMessage("OnDialogueEnded");
            
            // Act
            // Simulate Victory
            combatManager.EndCombat(true);

            // Assert
            Assert.IsTrue(questManager.IsQuestCompleted(_questData.QuestId), "Quest should be completed upon combat victory.");
        }
    }
}
