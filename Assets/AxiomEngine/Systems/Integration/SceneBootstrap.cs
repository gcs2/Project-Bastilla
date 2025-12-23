// ============================================================================
// Axiom RPG Engine - Scene Bootstrap
// Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.
// ============================================================================

using UnityEngine;
using RPGPlatform.Core;
using RPGPlatform.Systems.Combat;
using RPGPlatform.Systems.Dialogue;
using RPGPlatform.Core.Dialogue;
using RPGPlatform.Systems.Audio;
using RPGPlatform.Systems.Input;

namespace RPGPlatform.Systems.Integration
{
    /// <summary>
    /// Responsible for initializing core services and binding UI in a new scene.
    /// </summary>
    public class SceneBootstrap : MonoBehaviour
    {
        [Header("System Prefabs")]
        [SerializeField] private GameObject _audioManagerPrefab;
        [SerializeField] private GameObject _inputManagerPrefab;
        
        [Header("UI Canvases")]
        [SerializeField] private GameObject _combatHUDPrefab;
        [SerializeField] private GameObject _dialogueCanvasPrefab;

        private void Awake()
        {
            Debug.Log("[Axiom Engine] Bootstrapping Scene...");
            InitializeCoreServices();
            InitializeUI();
        }

        private void InitializeCoreServices()
        {
            // 1. Ensure Audio is present
            if (!ServiceLocator.IsRegistered<RPGPlatform.Core.Audio.IAudioService>())
            {
                if (_audioManagerPrefab != null) Instantiate(_audioManagerPrefab);
                else gameObject.AddComponent<AudioManager>();
            }

            // 2. Ensure Input is present
            if (InputManager.Instance == null)
            {
                if (_inputManagerPrefab != null) Instantiate(_inputManagerPrefab);
                else gameObject.AddComponent<InputManager>();
            }

            // 3. Register Logic Managers (singleton-lite for prototype)
            // Dialogue
            if (!ServiceLocator.IsRegistered<IDialogueService>())
            {
                var diag = gameObject.AddComponent<DialogueManager>();
                ServiceLocator.Register<IDialogueService>(diag);
            }

            // Combat Math
            if (!ServiceLocator.IsRegistered<ICombatResolver>())
            {
                var resolver = new D20CombatResolver(null); // Uses default config
                ServiceLocator.Register<ICombatResolver>(resolver);
            }
        }

        private void InitializeUI()
        {
            // In a production environment, we'd use Addressables or a Resource Loader
            if (_combatHUDPrefab != null) Instantiate(_combatHUDPrefab);
            if (_dialogueCanvasPrefab != null) Instantiate(_dialogueCanvasPrefab);
            
            Debug.Log("[Axiom Engine] UI Layers initialized.");
        }
    }
}
