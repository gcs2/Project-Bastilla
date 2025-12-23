// ============================================================================
// RPGPlatform.Core.Progression - Progression Definitions
// Interfaces and Data Structures for XP, Leveling, and Tiers
// ============================================================================

using System;
using System.Collections.Generic;

namespace RPGPlatform.Core.Progression
{
    public interface IProgressionService
    {
        int CurrentLevel { get; }
        long CurrentXP { get; }
        long XPToNextLevel { get; }
        string CurrentTierId { get; }
        float CurrentStatMultiplier { get; }

        void AddXP(long amount);
        bool IsTierUnlocked(string tierId);
        
        event Action<int> OnLevelUp;
        event Action<string> OnTierChanged;
        event Action<long, long> OnXPChanged;
    }

    [Serializable]
    public class LevelUpResult
    {
        public int NewLevel;
        public List<string> UnlockedAbilities;
        public List<string> UnlockedPerks;
        public int StatPointsGained;
    }
}
