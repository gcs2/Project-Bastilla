// ============================================================================
// RPGPlatform.Data - Location Data
// Defines a planet or area the player can travel to
// ============================================================================

using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatform.Data
{
    [CreateAssetMenu(fileName = "NewLocation", menuName = "RPG/World/Location")]
    public class LocationData : ScriptableObject
    {
        [Header("Identity")]
        public string LocationId;       // e.g. "vorgossos"
        public string DisplayName;      // e.g. "Vorgossos"
        public string Description;

        [Header("Travel Requirements")]
        public int MinLevel;
        public List<string> RequiredQuestIds = new List<string>();

        [Header("Scene Config")]
        public string MainSceneName;    // Unity Scene to load

        [Header("Atmosphere")]
        public AudioClip AmbientMusic;
    }
}
