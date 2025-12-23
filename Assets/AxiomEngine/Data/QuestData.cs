// ============================================================================
// Axiom RPG Engine - Quest Data
// Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using RPGPlatform.Core;

namespace RPGPlatform.Data
{
    [CreateAssetMenu(fileName = "NewQuest", menuName = "RPG/Quests/QuestData")]
    public class QuestData : ScriptableObject
    {
        [Header("Identity")]
        public string QuestId;
        public string DisplayName;
        [TextArea] public string Description;

        [Header("Objectives")]
        public List<QuestObjectiveData> Objectives = new List<QuestObjectiveData>();

        [Header("Prerequisites")]
        public int MinLevel;
        public List<string> RequiredQuestIds = new List<string>();

        [Header("Rewards")]
        public int XPReward;
        public int CurrencyReward;
    }

    [Serializable]
    public class QuestObjectiveData
    {
        public string Description;
        public int TargetCount = 1; // e.g. Kill 5 rats
        // Valid for simple "Kill/Collect" or "Talk To" types
    }
}
