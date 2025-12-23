// ============================================================================
// Axiom RPG Engine - Audio Manager
// Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.
// ============================================================================

using UnityEngine;
using System.Collections.Generic;
using RPGPlatform.Core;
using RPGPlatform.Core.Audio;

namespace RPGPlatform.Systems.Audio
{
    public class AudioManager : MonoBehaviour, IAudioService
    {
        [Header("Settings")]
        [SerializeField] private int _sfxSourceCount = 10;
        
        private AudioSource _musicSource;
        private List<AudioSource> _sfxPool = new List<AudioSource>();
        private AudioSource _voiceSource;

        private void Awake()
        {
            ServiceLocator.Register<IAudioService>(this);

            // Initialize Music Source
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;

            // Initialize Voice Source
            _voiceSource = gameObject.AddComponent<AudioSource>();
            _voiceSource.playOnAwake = false;

            // Initialize SFX Pool
            for (int i = 0; i < _sfxSourceCount; i++)
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                _sfxPool.Add(source);
            }
        }

        public void PlaySFX(AudioClip clip, Vector3? position = null, float volume = 1.0f)
        {
            if (clip == null) return;

            var source = GetAvailableSFXSource();
            if (source != null)
            {
                if (position.HasValue)
                {
                    source.spatialBlend = 1.0f; // 3D
                    source.transform.position = position.Value;
                }
                else
                {
                    source.spatialBlend = 0f; // 2D/UI
                }

                source.PlayOneShot(clip, volume);
            }
        }

        public void PlayMusic(AudioClip clip, bool loop = true, float fadeDuration = 1.0f)
        {
            if (clip == null) return;
            
            // Simplified: No crossfade yet, just swap
            _musicSource.clip = clip;
            _musicSource.loop = loop;
            _musicSource.Play();
            Debug.Log($"[Audio] Now playing music: {clip.name}");
        }

        public void StopMusic(float fadeDuration = 1.0f)
        {
            _musicSource.Stop();
        }

        public void PlayVoice(AudioClip clip, string characterId)
        {
            if (clip == null) return;
            
            _voiceSource.clip = clip;
            _voiceSource.Play();
            Debug.Log($"[Audio] Voice ({characterId}): Playing clip.");
        }

        public void SetVolume(string channel, float volume)
        {
            // Implementation for Master/Music/SFX channels using AudioMixer usually
            Debug.Log($"[Audio] Setting {channel} volume to {volume}");
        }

        private AudioSource GetAvailableSFXSource()
        {
            foreach (var source in _sfxPool)
            {
                if (!source.isPlaying) return source;
            }
            return _sfxPool[0]; // Recycle oldest
        }
    }
}
