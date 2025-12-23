// ============================================================================
// RPGPlatform.Core.Dialogue - Condition Types
// Concrete implementations of IDialogueCondition
// ============================================================================

using System;
using UnityEngine;
using RPGPlatform.Core;

namespace RPGPlatform.Core.Dialogue
{
    public enum ComparisonType
    {
        Equals,
        NotEquals,
        Greater,
        Less,
        GreaterOrEqual,
        LessOrEqual
    }

    /// <summary>
    /// Checks a specific axis value from the MoralityService
    /// </summary>
    [Serializable]
    public class MoralityCondition : IDialogueCondition
    {
        public string AxisId;
        public ComparisonType Comparison;
        public float RequiredValue;

        public bool Evaluate(DialogueContext context)
        {
            if (context.Morality == null) return true; // Default to true if system disabled
            
            float currentValue = context.Morality.GetAxisValue(AxisId);
            return Compare(currentValue, RequiredValue, Comparison);
        }

        public string GetFailureReason() => $"Requires {AxisId} {FormatComparison(Comparison)} {RequiredValue}";

        private bool Compare(float a, float b, ComparisonType type)
        {
            return type switch
            {
                ComparisonType.Equals => Mathf.Approximately(a, b),
                ComparisonType.NotEquals => !Mathf.Approximately(a, b),
                ComparisonType.Greater => a > b,
                ComparisonType.Less => a < b,
                ComparisonType.GreaterOrEqual => a >= b,
                ComparisonType.LessOrEqual => a <= b,
                _ => false
            };
        }
        
        private string FormatComparison(ComparisonType type)
        {
            return type switch
            {
                ComparisonType.Equals => "==",
                ComparisonType.NotEquals => "!=",
                ComparisonType.Greater => ">",
                ComparisonType.Less => "<",
                ComparisonType.GreaterOrEqual => ">=",
                ComparisonType.LessOrEqual => "<=",
                _ => "?"
            };
        }
    }

    /// <summary>
    /// Checks a player skill level
    /// </summary>
    [Serializable]
    public class SkillCondition : IDialogueCondition
    {
        public string SkillId; // "Persuade", "Intimidate"
        public int DifficultyClass;
        
        // If true, evaluate via SkillService.PerformSkillCheck (dice roll)
        // If false, just check static Skill Level >= DifficultyClass
        public bool IsActiveCheck = false; 

        public bool Evaluate(DialogueContext context)
        {
            // We need to access a SkillService from context
            // context.LocalState could hold service references if not explicit
            // For now, let's assume one is available or we fail gracefully
            
            ISkillService skills = null;
            if (context.LocalState.TryGetValue("SkillService", out var obj))
                skills = obj as ISkillService;

            if (skills == null) return true; // Fail safe

            if (IsActiveCheck)
                return skills.PerformSkillCheck(context.Player, SkillId, DifficultyClass);
            else
                return skills.GetSkillLevel(context.Player, SkillId) >= DifficultyClass;
        }

        public string GetFailureReason() => $"Requires {SkillId} {DifficultyClass}";
    }

    /// <summary>
    /// Checks global quest flags
    /// </summary>
    [Serializable]
    public class QuestFlagCondition : IDialogueCondition
    {
        public string FlagId;
        public bool RequiredState = true;

        public bool Evaluate(DialogueContext context)
        {
            IQuestService quests = null;
            if (context.LocalState.TryGetValue("QuestService", out var obj))
                quests = obj as IQuestService;

            if (quests == null) return true;

            return quests.GetFlag(FlagId) == RequiredState;
        }

        public string GetFailureReason() => $"Flag {FlagId} must be {RequiredState}";
    }
    
    /// <summary>
    /// Sun Eater: Companion Interjections (Red Company)
    /// Checks if a companion has enough influence to interrupt/interject
    /// </summary>
    [Serializable]
    public class CompanionInfluenceCondition : IDialogueCondition
    {
        public string CompanionId;
        public int MinInfluence;
        
        public bool Evaluate(DialogueContext context)
        {
            IInfluenceService influence = null;
            if (context.LocalState.TryGetValue("InfluenceService", out var obj))
                influence = obj as IInfluenceService;
                
            if (influence == null) return false; // Default false for interjections?
            
            return influence.GetInfluence(CompanionId) >= MinInfluence;
        }
        
        public string GetFailureReason() => $"{CompanionId} Influence < {MinInfluence}";
    }
}
