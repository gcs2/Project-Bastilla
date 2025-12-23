// ============================================================================
// RPGPlatform.Data - Progression Configuration
// ScriptableObject for defining Power Tiers and XP Curves
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatform.Data
{
    [CreateAssetMenu(fileName = "NewProgressionConfig", menuName = "RPG/Progression/Config")]
    public class ProgressionConfig : ScriptableObject
    {
        [Header("Leveling Settings")]
        [Tooltip("Maximum Level Cap")]
        public int MaxLevel = 50;

        [Tooltip("Base XP required for level 2")]
        public long BaseXP = 100;

        [Tooltip("Exponent for Logarithmic/Exponential curve. XP = Base * (Level^Exponent)")]
        public float XPExponent = 2.5f;

        [Header("Power Tiers")]
        public List<PowerTier> Tiers = new List<PowerTier>();

        public PowerTier GetTierForLevel(int level)
        {
            // Iterate backwards to find highest matching tier
            for (int i = Tiers.Count - 1; i >= 0; i--)
            {
                if (level >= Tiers[i].MinLevel)
                    return Tiers[i];
            }
            return null;
        }

        public long GetXPForLevel(int level)
        {
            if (level <= 1) return 0;
            // Formula: XP = Base * ((Level-1) ^ Exponent)
            // This creates a steep curve
            double xp = BaseXP * Math.Pow(level - 1, XPExponent);
            return (long)xp;
        }
    }

    [Serializable]
    public class PowerTier
    {
        public string TierId;          // "initiate", "knight"
        public string DisplayName;     // "Initiate", "Knight"
        public int MinLevel;           // Level required to enter tier
        public float StatMultiplier = 1.0f; // Multiplier to base stats
        
        // Future expansion:
        // public List<string> UnlockedAbilities;
    }
}
