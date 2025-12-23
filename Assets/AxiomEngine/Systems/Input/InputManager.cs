// ============================================================================
// Axiom RPG Engine - Input Manager
// Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.
// ============================================================================

using UnityEngine;
using System;

namespace RPGPlatform.Systems.Input
{
    /// <summary>
    /// Centralized input handler. In a production environment, this wraps 
    /// Unity's New Input System. For this prototype, it provides a stable API.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        public event Action OnConfirm;
        public event Action OnCancel;
        public event Action<Vector2> OnMove;
        
        [Header("State")]
        [SerializeField] private bool _inputEnabled = true;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Update()
        {
            if (!_inputEnabled) return;

            // Simple legacy input mapping for prototype
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space) || UnityEngine.Input.GetKeyDown(KeyCode.Return))
            {
                OnConfirm?.Invoke();
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                OnCancel?.Invoke();
            }

            float horizontal = UnityEngine.Input.GetAxisRaw("Horizontal");
            float vertical = UnityEngine.Input.GetAxisRaw("Vertical");
            
            if (horizontal != 0 || vertical != 0)
            {
                OnMove?.Invoke(new Vector2(horizontal, vertical));
            }
        }

        public void SetInputEnabled(bool enabled)
        {
            _inputEnabled = enabled;
            Debug.Log($"[Input] Input Enabled: {enabled}");
        }
    }
}
