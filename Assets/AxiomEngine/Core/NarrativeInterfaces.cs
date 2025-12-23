// ============================================================================
// RPGPlatform.Core - Narrative Interfaces
// Mock interfaces for Quest and Skill systems generic to the platform
// ============================================================================

using System;
using System.Collections.Generic;

namespace RPGPlatform.Core
{
    /// <summary>
    /// Service for checking global flags and quest states
    /// </summary>
    public interface IQuestService
    {
        bool GetFlag(string flagId);
        void SetFlag(string flagId, bool value);
        int GetQuestStep(string questId);
        void SetQuestStep(string questId, int step);
        bool IsQuestCompleted(string questId);
    }

    /// <summary>
    /// Service for checking player skills outside of combat
    /// </summary>
    public interface ISkillService
    {
        int GetSkillLevel(ICombatant target, string skillId);
        bool PerformSkillCheck(ICombatant target, string skillId, int difficultyClass);
    }

    /// <summary>
    /// Service for tracking companion interactions
    /// </summary>
    public interface IInfluenceService
    {
        int GetInfluence(string companionId);
        void ModifyInfluence(string companionId, int delta);
    }
}
