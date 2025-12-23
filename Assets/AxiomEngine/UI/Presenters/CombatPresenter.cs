// ============================================================================
// Axiom RPG Engine - Combat Presenter
// Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.
// ============================================================================

using UnityEngine;
using UnityEngine.UI;
using RPGPlatform.Core;
using RPGPlatform.UI;

namespace RPGPlatform.UI.Presenters
{
    /// <summary>
    /// Binds the logic Layer (CombatViewController) to Unity UI components.
    /// Works with TextMeshPro and standard UI Sliders/Panels.
    /// </summary>
    public class CombatPresenter : MonoBehaviour
    {
        [Header("Logic Controller")]
        [SerializeField] private CombatViewController _controller;

        [Header("UI References")]
        [SerializeField] private GameObject _actionMenuPanel;
        [SerializeField] private Text _turnBannerText; // Swap for TMP_Text in production
        [SerializeField] private GameObject _healthBarPrefab;
        [SerializeField] private Transform _healthBarContainer;

        private void Start()
        {
            if (_controller != null)
            {
                _controller.OnShowTurnBanner += ShowTurnBanner;
                _controller.OnShowActionMenu += ShowActionMenu;
                _controller.OnHideActionMenu += HideActionMenu;
                _controller.OnUpdateHealth += UpdateHealthDisplay;
            }
        }

        private void OnDestroy()
        {
            if (_controller != null)
            {
                _controller.OnShowTurnBanner -= ShowTurnBanner;
                _controller.OnShowActionMenu -= ShowActionMenu;
                _controller.OnHideActionMenu -= HideActionMenu;
                _controller.OnUpdateHealth -= UpdateHealthDisplay;
            }
        }

        private void ShowTurnBanner(string message)
        {
            if (_turnBannerText != null)
            {
                _turnBannerText.text = message;
                Debug.Log($"[UI Presenter] Turn Banner: {message}");
            }
        }

        private void ShowActionMenu(ICombatant combatant)
        {
            if (_actionMenuPanel != null)
            {
                _actionMenuPanel.SetActive(true);
                Debug.Log($"[UI Presenter] Showing Action Menu for {combatant.DisplayName}");
            }
        }

        private void HideActionMenu()
        {
            if (_actionMenuPanel != null)
            {
                _actionMenuPanel.SetActive(false);
            }
        }

        private void UpdateHealthDisplay(ICombatant combatant)
        {
            // Logic to find/update Sliders for health
            Debug.Log($"[UI Presenter] Updating Health for {combatant.DisplayName}: {combatant.Stats.CurrentHealth}");
        }
    }
}
