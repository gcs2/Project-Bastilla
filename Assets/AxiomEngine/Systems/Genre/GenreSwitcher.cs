// ============================================================================
// Axiom RPG Engine - Genre Switcher
// Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.
// ============================================================================

using UnityEngine;
using RPGPlatform.Data;
using System;

namespace RPGPlatform.Systems.Genre
{
    /// <summary>
    /// Handles the swapping of visual and audio identity for the engine.
    /// Bridges the gap between generic systems and specific game "Vibes".
    /// </summary>
    public class GenreSwitcher : MonoBehaviour
    {
        public static GenreSwitcher Instance { get; private set; }

        [SerializeField] private GenreProfile _activeProfile;
        
        public event Action<GenreProfile> OnGenreChanged;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void SwitchGenre(GenreProfile newProfile)
        {
            if (newProfile == null) return;
            
            _activeProfile = newProfile;
            Debug.Log($"[GenreSwitcher] Engine updated to: {_activeProfile.GenreName}");
            
            OnGenreChanged?.Invoke(_activeProfile);
        }

        public GenreProfile GetActiveProfile() => _activeProfile;

        // Helper methods for UI/Audio to fetch assets
        public Font GetCommonFont() => _activeProfile?.DefaultFont;
        public AudioClip GetDefaultMusic() => _activeProfile?.GlobalMusic;
    }
}
