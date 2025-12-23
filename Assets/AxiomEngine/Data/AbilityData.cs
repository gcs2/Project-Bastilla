// ============================================================================
// RPGPlatform.Data - Ability Data
// ScriptableObject definition for combat abilities
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using RPGPlatform.Core;

namespace RPGPlatform.Data
{
    [CreateAssetMenu(fileName = "NewAbility", menuName = "RPG/Combat/Ability")]
    public class AbilityData : ScriptableObject
    {
        [Header("Identity")]
        public string AbilityId;
        public string DisplayName;
        [TextArea] public string Description;
        public Sprite Icon;
        
        [Header("Classification")]
        public AbilityType Type = AbilityType.Attack;
        public TargetType Targeting = TargetType.SingleEnemy;
        public DamageType DamageType = DamageType.Physical;
        public StatType PrimaryStat = StatType.Strength;
        public List<string> Tags = new List<string>();
        
        [Header("Costs & Cooldowns")]
        public int ResourceCost = 10;
        public int CooldownTurns = 0;
        public int UsesPerCombat = -1;  // -1 = unlimited
        
        [Header("Range & Area")]
        public int Range = 1;           // 1 = melee, higher = ranged
        public int AreaOfEffect = 0;    // 0 = single target
        
        [Header("Damage")]
        public string DamageFormula = "1d6";
        public float CritMultiplier = 2f;
        
        [Header("Status Effects")]
        public List<StatusEffectData> AppliedEffects = new List<StatusEffectData>();
        public List<StatusEffectData> SelfEffects = new List<StatusEffectData>();
        
        [Header("Requirements")]
        public int RequiredLevel = 1;
        public string RequiredMoralityAxis;
        public float MinMoralityValue = float.NegativeInfinity;
        public float MaxMoralityValue = float.PositiveInfinity;
        
        [Header("Presentation")]
        public AnimationClip UseAnimation;
        public GameObject VFXPrefab;
        public AudioClip SFX;
        public float AnimationDuration = 1f;
        
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(AbilityId))
            {
                AbilityId = name.ToLower().Replace(" ", "_");
            }
        }
    }
}
