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
    /// <summary>
    /// Tests for the Sun Eater demo bootstrapper.
    /// TODO: MIGRATE TO TAV - This test suite is brittle due to Unity Editor overhead.
    /// It will be replaced by the Axiom CLI / TAV Harness (AxiomShell.cs).
    /// </summary>
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
            var combatManager = _root.AddComponent<CombatManager>();
            combatManager.Initialize(); // Manual initialization for EditMode
            
            _root.AddComponent<DialogueManager>();
            _root.AddComponent<QuestManager>();
            
            _bootstrapper = _root.AddComponent<PlayableDemoBootstrapper>();
            
            Debug.Log($"[Tests] TurnManagers on root: {_root.GetComponents<TurnManager>().Length}");
            
            _player = new GameObject("Player");
            _player.AddComponent<Combatant>();
            
            _inquisitor = new GameObject("Inquisitor");
            _inquisitor.AddComponent<Combatant>();
            
            _dialogueData = ScriptableObject.CreateInstance<ConversationData>();
            _dialogueData.ConversationId = "inquisitor_spectacle";
            _dialogueData.EntryNodeId = "start";
            _dialogueData.Nodes = new List<DialogueNode> 
            { 
                new DialogueNode { NodeId = "start", Text = "Test Start" } 
            };
            
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
            Debug.Log("Test_SystemInitialization: Manually calling Initialize on Bootstrapper");
            _bootstrapper.Initialize(); 

            // Assert
            bool morality = ServiceLocator.IsRegistered<IMoralityService>();
            bool quest = ServiceLocator.IsRegistered<IQuestService>();
            bool dialogue = ServiceLocator.IsRegistered<IDialogueService>();
            
            Debug.Log($"Services Registered - Morality: {morality}, Quest: {quest}, Dialogue: {dialogue}");

            Assert.IsTrue(morality, "Morality service should be registered.");
            Assert.IsTrue(quest, "Quest service should be registered.");
            Assert.IsTrue(dialogue, "Dialogue service should be registered.");
        }

        [Test]
        public void Test_DialogueToCombatTransition()
        {
            // Arrange
            _bootstrapper.Initialize();
            var dialogueManager = _root.GetComponent<DialogueManager>();
            var combatManager = _root.GetComponent<CombatManager>();
            
            bool combatStarted = false;
            combatManager.OnCombatStarted += () => 
            {
                Debug.Log("Test_DialogueToCombatTransition: Combat Started Event Fired");
                combatStarted = true;
            };

            // Act
            // Manually set the conversation ID on the dialogue manager so the bootstrapper sees it
            Debug.Log("Test_DialogueToCombatTransition: Starting conversation to set state");
            dialogueManager.StartConversation(_dialogueData, _player.GetComponent<ICombatant>(), _inquisitor.GetComponent<ICombatant>());
            
            Debug.Log("Test_DialogueToCombatTransition: Simulating OnDialogueEnded");
            // Direct call to ensure it hits the method
            _bootstrapper.OnDialogueEnded();

            // Assert
            Assert.IsTrue(combatStarted, "Combat should have started after dialogue ended on the spectacle ID.");
        }

        [Test]
        public void Test_QuestCompletionOnVictory()
        {
            // Arrange
            _bootstrapper.Initialize();
            var combatManager = _root.GetComponent<CombatManager>();
            var questManager = _root.GetComponent<QuestManager>();
            var dialogueManager = _root.GetComponent<DialogueManager>();

            // Start combat first
            Debug.Log("Test_QuestCompletionOnVictory: Setting up Combat State");
            dialogueManager.StartConversation(_dialogueData, _player.GetComponent<ICombatant>(), _inquisitor.GetComponent<ICombatant>());
            _bootstrapper.OnDialogueEnded();
            
            // Act
            Debug.Log("Test_QuestCompletionOnVictory: Ending Combat with Victory");
            combatManager.EndCombat(true);

            // Assert
            bool completed = questManager.IsQuestCompleted(_questData.QuestId);
            Debug.Log($"Test_QuestCompletionOnVictory: Quest Completed? {completed}");
            Assert.IsTrue(completed, "Quest should be completed upon combat victory.");
        }

    }
}
