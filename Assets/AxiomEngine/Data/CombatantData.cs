using System.Collections.Generic;
using UnityEngine;
using RPGPlatform.Core;

namespace RPGPlatform.Data
{
    [CreateAssetMenu(fileName = "NewCombatantData", menuName = "RPG/Combat/Combatant Data")]
    public class CombatantData : ScriptableObject
    {
        [Header("Identity")]
        public string Id;
        public string BaseName;
        public string Title;
        
        [Header("Stats")]
        public CombatStats BaseStats;
        
        [Header("Abilities")]
        public List<AbilityData> Abilities = new List<AbilityData>();
        
        [Header("Presentation")]
        public GameObject Prefab;
        public Sprite Portrait;
        
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(Id))
            {
                Id = name.ToLower().Replace(" ", "_");
            }
        }
    }
}
