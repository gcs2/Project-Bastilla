// ============================================================================
// Axiom RPG Engine - Audio Service Interface
// Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.
// ============================================================================

using UnityEngine;

namespace RPGPlatform.Core.Audio
{
    /// <summary>
    /// Service for managing audio playback across the engine.
    /// Handles SFX, Background Music, and Voice Acting clips.
    /// </summary>
    public interface IAudioService
    {
        /// <summary>
        /// Plays a sound effect once at a given position.
        /// </summary>
        void PlaySFX(AudioClip clip, Vector3? position = null, float volume = 1.0f);

        /// <summary>
        /// Plays/Transitions to a new music track.
        /// </summary>
        void PlayMusic(AudioClip clip, bool loop = true, float fadeDuration = 1.0f);

        /// <summary>
        /// Stops the current music track with optional fade.
        /// </summary>
        void StopMusic(float fadeDuration = 1.0f);

        /// <summary>
        /// Plays a voice acting clip (prioritized over other sfx).
        /// </summary>
        void PlayVoice(AudioClip clip, string characterId);

        /// <summary>
        /// Sets master/channel volume (0.0 to 1.0).
        /// </summary>
        void SetVolume(string channel, float volume);
    }
}
