// ============================================================================
// SunEater.Items - High-Matter Sword Generator
// Generates weapon stats based on Player Attributes (Neural-Link)
// ============================================================================

using UnityEngine;
using RPGPlatform.Core;

namespace SunEater.Items
{
    public class HighMatterSwordStats
    {
        public int BaseDamage;
        public int BonusDamage;
        public string DamageFormula; // e.g. "1d10 + 5"
        public string Rarity;
    }

    public static class HighMatterSwordGenerator
    {
        /// <summary>
        /// Generates a High-Matter Sword tailored to the player's Neural-Link.
        /// Neural-Link is conceptually derived from Intelligence + Wisdom.
        /// </summary>
        public static HighMatterSwordStats Generate(ICombatant player)
        {
            // Calculate Neural-Link Score
            int intel = player.Stats.Intelligence;
            int wis = player.Stats.Wisdom;
            int neuralLink = (intel + wis) / 2;

            var sword = new HighMatterSwordStats();

            // Logic: Damage scales with Neural-Link steps
            if (neuralLink < 12)
            {
                sword.Rarity = "Unstable";
                sword.BaseDamage = 4;
                sword.DamageFormula = "1d6";
            }
            else if (neuralLink < 16)
            {
                sword.Rarity = "Stabilized";
                sword.BaseDamage = 6;
                sword.BonusDamage = neuralLink - 10; // Simple scaling
                sword.DamageFormula = $"1d8 + {sword.BonusDamage}";
            }
            else if (neuralLink < 20)
            {
                sword.Rarity = "Resonant";
                sword.BaseDamage = 8;
                sword.BonusDamage = (neuralLink - 10) * 2;
                sword.DamageFormula = $"1d10 + {sword.BonusDamage}";
            }
            else
            {
                sword.Rarity = "Ascendant";
                sword.BaseDamage = 12;
                sword.BonusDamage = (neuralLink - 10) * 3;
                sword.DamageFormula = $"2d8 + {sword.BonusDamage}";
            }

            Debug.Log($"[HighMatterSword] Generated '{sword.Rarity}' sword for Neural-Link {neuralLink}. Dmg: {sword.DamageFormula}");
            return sword;
        }
    }
}
