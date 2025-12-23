// ============================================================================
// RPGPlatform.Data - Genre Profile
// Defines the aesthetic/presentation layer for the current genre (Sun Eater vs Anime etc.)
// ============================================================================

using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Playables;

namespace RPGPlatform.Data
{
    [CreateAssetMenu(fileName = "NewGenreProfile", menuName = "RPG/Genre/Profile")]
    public class GenreProfile : ScriptableObject
    {
        [Header("Identity")]
        public string GenreId;         // "suneater", "anime_scifi"
        public string GenreName;       // "The Sun Eater"

        [Header("Default Cutscenes")]
        // Default assets to use if no specific override is found
        public VideoClip DefaultLandingVideo;
        public PlayableAsset DefaultLandingTimeline;
        
        [Header("UI Styler")]
        public Color PrimaryColor = Color.white;
        public Font MainFont;
        public Font DefaultFont;      // Compatibility field

        [Header("Audio")]
        public AudioClip GlobalMusic;
    }
}
