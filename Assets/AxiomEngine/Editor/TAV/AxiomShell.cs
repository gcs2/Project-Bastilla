// ============================================================================
// Axiom RPG Engine - Axiom Shell (TAV Harness)
// Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using RPGPlatform.Core;
using RPGPlatform.Core.Dialogue;
using RPGPlatform.Systems.Combat;
using RPGPlatform.Systems.Dialogue;
using RPGPlatform.Systems.Quests;
using RPGPlatform.Systems.Morality;
using RPGPlatform.Data;
using RPGPlatform.Tests;
using UnityEditor;
using System.Linq;

namespace RPGPlatform.Editor.TAV
{
    /// <summary>
    /// The central harness for Text Adventure Verification (TAV).
    /// Orchestrates systems and provides a CLI-like interface for logic testing.
    /// </summary>
    public static class AxiomShell
    {
        private static bool _initialized;
        private static GameObject _harnessRoot;
        
        // Systems
        public static CombatManager Combat { get; private set; }
        public static DialogueManager Dialogue { get; private set; }
        public static QuestManager Quests { get; private set; }
        public static MoralityState Morality { get; private set; }

        // Mock Entities
        public static TestingCommon.MockCombatant Player { get; private set; }
        public static TestingCommon.MockCombatant Inquisitor { get; private set; }

        public static void Initialize()
        {
            if (_initialized) Cleanup();

            _harnessRoot = new GameObject("TAV_Harness");
            _harnessRoot.hideFlags = HideFlags.HideAndDontSave;

            // Setup Systems
            Combat = _harnessRoot.AddComponent<CombatManager>();
            Dialogue = _harnessRoot.AddComponent<DialogueManager>();
            Quests = _harnessRoot.AddComponent<QuestManager>();
            Morality = new MoralityState();

            // Configure Dialogue with Editor Repository
            Dialogue.Initialize(Morality, Quests, null, null, new EditorDialogueRepository());

            // Configure Combat
            Combat.Initialize();

            // Setup Mock Combatants
            Player = new TestingCommon.MockCombatant();
            Player.DisplayName = "Player";
            Player.Team = 0;
            Player.Stats.CurrentHealth = 100;
            
            Inquisitor = new TestingCommon.MockCombatant();
            Inquisitor.DisplayName = "Inquisitor";
            Inquisitor.Team = 1;
            Inquisitor.Stats.CurrentHealth = 100;

            // Configure Morality (Default Axis for Sun Eater)
            var config = ScriptableObject.CreateInstance<MoralityAxisConfig>();
            config.AxisId = "humanism";
            config.DefaultValue = 50f;
            Morality.Initialize(config);
            
            // Register Services
            ServiceLocator.Register<IMoralityService>(Morality);
            ServiceLocator.Register<IQuestService>(Quests);
            ServiceLocator.Register<IDialogueService>(Dialogue);

            // Subscribe to Events for Logging & Demo Glue
            Dialogue.OnNodeStart += log => Debug.Log($"[TAV] DIALOGUE: {log.Text}");
            Dialogue.OnConversationEnd += OnDialogueEnded;
            
            Combat.OnCombatStarted += () => Debug.Log("[TAV] COMBAT STARTED");
            Combat.OnCombatEnded += victory => Debug.Log($"[TAV] COMBAT ENDED: {(victory ? "VICTORY" : "DEFEAT")}");
            Combat.OnDamageDealt += (target, result) => Debug.Log($"[TAV] COMBAT: {target.DisplayName} takes {result.TotalDamage} damage.");

            _initialized = true;
            Debug.Log("[AxiomShell] Text Adventure Verification Harness Initialized.");
        }

        public static void Cleanup()
        {
            if (_harnessRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(_harnessRoot);
            }
            _initialized = false;
        }

        /// <summary>
        /// Execute a text command against the engine logic.
        /// </summary>
        public static void Execute(string command)
        {
            if (!_initialized) Initialize();

            string[] parts = command.ToLower().Split(' ');
            if (parts.Length == 0) return;

            switch (parts[0])
            {
                case "talk":
                    // talk inquisitor_spectacle
                    if (parts.Length > 1) Dialogue.StartConversation(parts[1], null, null);
                    break;
                case "choose":
                    // choose 0
                    if (parts.Length > 1 && int.TryParse(parts[1], out int index))
                    {
                        Dialogue.SelectResponse(index);
                    }
                    break;
                case "end":
                    Dialogue.EndConversation();
                    break;
                case "stat":
                    Debug.Log($"[TAV] Morality (Humanism): {Morality.GetAxisValue("humanism")}");
                    break;
                case "help":
                    Debug.Log("[TAV] Commands: talk [id], choose [index], stat, help");
                    break;
                default:
                    Debug.LogWarning($"[TAV] Unknown command: {parts[0]}");
                    break;
            }
        }

        private static void OnDialogueEnded()
        {
            Debug.Log("[TAV] DIALOGUE ENDED");
            if (Dialogue.CurrentConversationId == "inquisitor_spectacle")
            {
                Debug.Log("[TAV] Transitioning to Combat (Demo Glue)...");
                Combat.StartCombat(new List<ICombatant> { Player, Inquisitor });
            }
        }

        private class EditorDialogueRepository : IDialogueRepository
        {
            public RPGPlatform.Data.ConversationData LoadConversation(string conversationId)
            {
                // Try direct search by name first
                var guids = AssetDatabase.FindAssets($"t:ConversationData {conversationId}");
                if (guids.Length == 0)
                {
                    // Fallback: search all ConversationData and check internal ID
                    guids = AssetDatabase.FindAssets("t:ConversationData");
                }

                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<ConversationData>(path);
                    if (asset != null && asset.ConversationId == conversationId)
                        return asset;
                }
                
                Debug.LogError($"[EditorDialogueRepository] FAILED to resolve ConversationId: {conversationId}");
                return null;
            }
        }
    }
}
