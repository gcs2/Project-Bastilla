// ============================================================================
// RPGPlatform.Data - Morality Effect Configuration
// Data-driven configuration for morality-based buffs and penalties
// ============================================================================

using System;
using UnityEngine;
using RPGPlatform.Core;

namespace RPGPlatform.Data
{
    /// <summary>
    /// ScriptableObject defining stat buffs/penalties based on morality value ranges
    /// Follows data-driven design: behavior is code, content is data
    /// </summary>
    [CreateAssetMenu(fileName = "MoralityEffect", menuName = "RPG/Morality/Effect Config")]
    public class MoralityEffectConfig : ScriptableObject
    {
        [Header("Trigger Conditions")]
        [Tooltip("Axis this effect applies to (e.g., 'humanism')")]
        public string AxisId = "alignment";
        
        [Tooltip("Minimum value to trigger this effect (inclusive)")]
        public float MinValue = -100f;
        
        [Tooltip("Maximum value to trigger this effect (inclusive)")]
        public float MaxValue = -50f;
        
        [Header("Effect Identity")]
        [Tooltip("Display name for this effect tier")]
        public string EffectName = "Minor Effect";
        
        [Tooltip("Description of what this effect does")]
        [TextArea(2, 4)]
        public string Description = "";
        
        [Header("Stat Modifiers")]
        public int StrengthModifier = 0;
        public int DexterityModifier = 0;
        public int ConstitutionModifier = 0;
        public int IntelligenceModifier = 0;
        public int WisdomModifier = 0;
        public int CharismaModifier = 0;
        public int HealthModifier = 0;
        public int ArmorClassModifier = 0;
        
        [Header("Social Modifiers")]
        [Tooltip("Modifier to social checks with affected factions")]
        public int SocialPenalty = 0;
        
        [Tooltip("Price multiplier for services (1.0 = normal, 2.0 = double)")]
        public float PriceMultiplier = 1.0f;
        
        [Header("Access Restrictions")]
        [Tooltip("Areas or services blocked by this effect")]
        public string[] RestrictedAreas = new string[0];
        
        [Tooltip("Ability IDs that are blocked")]
        public string[] BlockedAbilities = new string[0];
        
        [Tooltip("Can access faction services")]
        public bool AllowsFactionAccess = true;
        
        [Tooltip("Can enter faction territories")]
        public bool AllowsTerritoryAccess = true;
        
        /// <summary>
        /// Check if a value falls within this effect's range
        /// </summary>
        public bool IsInRange(float value)
        {
            return value >= MinValue && value <= MaxValue;
        }
        
        /// <summary>
        /// Apply stat modifiers to combat stats
        /// </summary>
        public void ApplyToStats(CombatStats stats)
        {
            stats.Strength += StrengthModifier;
            stats.Dexterity += DexterityModifier;
            stats.Constitution += ConstitutionModifier;
            stats.Intelligence += IntelligenceModifier;
            stats.Wisdom += WisdomModifier;
            stats.Charisma += CharismaModifier;
            stats.MaxHealth += HealthModifier;
            stats.ArmorClass += ArmorClassModifier;
        }
        
        /// <summary>
        /// Remove stat modifiers from combat stats
        /// </summary>
        public void RemoveFromStats(CombatStats stats)
        {
            stats.Strength -= StrengthModifier;
            stats.Dexterity -= DexterityModifier;
            stats.Constitution -= ConstitutionModifier;
            stats.Intelligence -= IntelligenceModifier;
            stats.Wisdom -= WisdomModifier;
            stats.Charisma -= CharismaModifier;
            stats.MaxHealth -= HealthModifier;
            stats.ArmorClass -= ArmorClassModifier;
        }
        
        /// <summary>
        /// Check if an ability is blocked by this effect
        /// </summary>
        public bool IsAbilityBlocked(string abilityId)
        {
            foreach (var blocked in BlockedAbilities)
            {
                if (abilityId.Contains(blocked))
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// Get formatted description with stat modifiers
        /// </summary>
        public string GetFullDescription()
        {
            var desc = $"{EffectName}\n{Description}\n\n";
            
            // Stat modifiers
            if (HasStatModifiers())
            {
                desc += "Stat Modifiers:\n";
                if (StrengthModifier != 0) desc += $"  STR {StrengthModifier:+0;-0}\n";
                if (DexterityModifier != 0) desc += $"  DEX {DexterityModifier:+0;-0}\n";
                if (ConstitutionModifier != 0) desc += $"  CON {ConstitutionModifier:+0;-0}\n";
                if (IntelligenceModifier != 0) desc += $"  INT {IntelligenceModifier:+0;-0}\n";
                if (WisdomModifier != 0) desc += $"  WIS {WisdomModifier:+0;-0}\n";
                if (CharismaModifier != 0) desc += $"  CHA {CharismaModifier:+0;-0}\n";
                if (HealthModifier != 0) desc += $"  HP {HealthModifier:+0;-0}\n";
                if (ArmorClassModifier != 0) desc += $"  AC {ArmorClassModifier:+0;-0}\n";
            }
            
            // Social penalties
            if (SocialPenalty != 0)
            {
                desc += $"\nSocial Penalty: {SocialPenalty}\n";
            }
            
            if (PriceMultiplier != 1.0f)
            {
                desc += $"Price Multiplier: {PriceMultiplier}x\n";
            }
            
            // Restrictions
            if (!AllowsFactionAccess || !AllowsTerritoryAccess || BlockedAbilities.Length > 0)
            {
                desc += "\nRestrictions:\n";
                if (!AllowsFactionAccess) desc += "  - Cannot access faction services\n";
                if (!AllowsTerritoryAccess) desc += "  - Cannot enter faction territories\n";
                if (BlockedAbilities.Length > 0) desc += $"  - {BlockedAbilities.Length} abilities blocked\n";
            }
            
            return desc;
        }
        
        private bool HasStatModifiers()
        {
            return StrengthModifier != 0 || DexterityModifier != 0 || ConstitutionModifier != 0 ||
                   IntelligenceModifier != 0 || WisdomModifier != 0 || CharismaModifier != 0 ||
                   HealthModifier != 0 || ArmorClassModifier != 0;
        }
    }
}
