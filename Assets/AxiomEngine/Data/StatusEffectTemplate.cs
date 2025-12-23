// ============================================================================
// RPGPlatform.Data - Status Effect Template
// ScriptableObject wrapper for StatusEffectData
// ============================================================================

using UnityEngine;
using RPGPlatform.Core;

namespace RPGPlatform.Core
{
    /// <summary>
    /// ScriptableObject template for status effects
    /// Allows designers to create effects like "Burning", "Stunned" as assets
    /// </summary>
    [CreateAssetMenu(fileName = "NewStatusEffect", menuName = "RPG/Combat/Status Effect")]
    public class StatusEffectTemplate : ScriptableObject
    {
        [Header("Identity")]
        public string EffectId;
        public string DisplayName;
        [TextArea] public string Description;
        public Sprite Icon;

        [Header("Duration & Stacking")]
        public int BaseDuration = 3;    // -1 for infinite
        public int MaxStacks = 1;
        public bool IsDebuff = true;
        public bool IsDispellable = true;

        [Header("Stats & Limits")]
        public bool PreventsActions = false;
        public bool PreventsMovement = false;
        
        [Header("Stat Modifiers")]
        public int StrengthMod = 0;
        public int DexterityMod = 0;
        public int ConstitutionMod = 0;
        public int ArmorClassMod = 0;

        [Header("Periodic Effects")]
        public int DamagePerTurn = 0;
        public int HealingPerTurn = 0;
        public DamageType PeriodicDamageType = DamageType.Physical;

        /// <summary>
        /// Convert this template to a serializable data structure
        /// </summary>
        public StatusEffectData ToData()
        {
            return new StatusEffectData
            {
                EffectId = string.IsNullOrEmpty(EffectId) ? name.ToLower() : EffectId,
                DisplayName = DisplayName,
                Duration = BaseDuration,
                MaxStacks = MaxStacks,
                IsDebuff = IsDebuff,
                IsDispellable = IsDispellable,
                StrengthMod = StrengthMod,
                DexterityMod = DexterityMod,
                ConstitutionMod = ConstitutionMod,
                ArmorClassMod = ArmorClassMod,
                DamagePerTurn = DamagePerTurn,
                HealingPerTurn = HealingPerTurn,
                DamageType = PeriodicDamageType
            };
        }
    }
}
