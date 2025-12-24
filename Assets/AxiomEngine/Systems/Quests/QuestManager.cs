// ============================================================================
// Axiom RPG Engine - Quest Manager
// Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RPGPlatform.Core;
using RPGPlatform.Core.Progression; // For Rewards
using RPGPlatform.Data;

namespace RPGPlatform.Systems.Quests
{
    public class QuestManager : MonoBehaviour, IQuestService
    {
        // Internal State
        private List<string> _completedQuestIds = new List<string>();
        // QuestId -> Current Objective Index (Simple linear track for prototype)
        // Or QuestId -> Objective Progress Dict?
        // Let's stick to the Interface contract: GetQuestStep(questId) returns int step.
        private Dictionary<string, int> _questSteps = new Dictionary<string, int>();

        // We also need Flags
        private Dictionary<string, bool> _flags = new Dictionary<string, bool>();

        // In a real game, this would load from a SaveSystem.
        
        [Header("Configuration")]
        [SerializeField] private List<QuestData> _questDatabase; // For lookup if needed, or inject

        // Dependencies
        private IProgressionService _progression;

        public event Action<string> OnQuestStarted;
        public event Action<string> OnQuestCompleted;
        public event Action<string, int> OnQuestStepUpdated;

        public void Initialize(IProgressionService progression, List<QuestData> database = null)
        {
            _progression = progression;
            if (database != null) _questDatabase = database;
        }

        // --- IQuestService Implementation ---

        public bool GetFlag(string flagId)
        {
            return _flags.ContainsKey(flagId) && _flags[flagId];
        }

        public void SetFlag(string flagId, bool value)
        {
            _flags[flagId] = value;
            Debug.Log($"[QuestManager] SetFlag {flagId} = {value}");
        }

        public int GetQuestStep(string questId)
        {
            return _questSteps.ContainsKey(questId) ? _questSteps[questId] : 0;
        }

        public void SetQuestStep(string questId, int step)
        {
            if (!_questSteps.ContainsKey(questId))
            {
                // New Quest Started
                Debug.Log($"[QuestManager] Started Quest: {questId}");
                OnQuestStarted?.Invoke(questId);
            }

            _questSteps[questId] = step;
            OnQuestStepUpdated?.Invoke(questId, step);

            Debug.Log($"[QuestManager] Quest {questId} updated to Step {step}");

            // Basic completion check: usually we check against QuestData Max Steps
            // For now, let's assume if step >= 100 it's done? Or explicit Complete function?
            // The Interface doesn't have CompleteQuest(), usually handled by logic setting a "Completed" flag or step.
            // But we added IsQuestCompleted() to interface.
        }

        public bool IsQuestCompleted(string questId)
        {
            return _completedQuestIds.Contains(questId);
        }

        public bool CanStartQuest(string questId)
        {
            if (_questSteps.ContainsKey(questId) || IsQuestCompleted(questId))
                return false;

            var questData = _questDatabase?.FirstOrDefault(q => q.QuestId == questId);
            if (questData == null) return true; // Default to true if no data, or false? Let's say true for loose coupling.

            // Level Check
            if (_progression != null && _progression.CurrentLevel < questData.MinLevel)
                return false;

            // Prerequisite Quests
            foreach (var reqId in questData.RequiredQuestIds)
            {
                if (!IsQuestCompleted(reqId))
                    return false;
            }

            return true;
        }

        public void AcceptQuest(string questId)
        {
            if (CanStartQuest(questId))
            {
                SetQuestStep(questId, 1);
            }
            else
            {
                Debug.LogWarning($"[QuestManager] Cannot accept quest {questId}: Prerequisites not met.");
            }
        }

        // --- Public Helper API for Game Logic ---

        public void CompleteQuest(string questId)
        {
            if (_completedQuestIds.Contains(questId)) return;

            _completedQuestIds.Add(questId);
            Debug.Log($"[QuestManager] Completed Quest: {questId}");
            OnQuestCompleted?.Invoke(questId);

            // Grant Rewards
            var questData = _questDatabase?.FirstOrDefault(q => q.QuestId == questId);
            if (questData != null && _progression != null)
            {
                if (questData.XPReward > 0)
                {
                    _progression.AddXP(questData.XPReward);
                    Debug.Log($"[QuestManager] Granted {questData.XPReward} XP");
                }
            }
        }
    }
}
