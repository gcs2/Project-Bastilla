// ============================================================================
// Axiom RPG Engine - Combat View Controller
// Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.
// ============================================================================

using UnityEngine;
using System;
using RPGPlatform.Core;
using RPGPlatform.Systems.Combat;

namespace RPGPlatform.UI
{
    public class CombatViewController : MonoBehaviour
    {
        // Dependencies
        private ITurnManager _turnManager;
        private CombatStateMachine _stateMachine;

        // UI Events (View components subscribe to these)
        public event Action<string> OnShowTurnBanner;
        public event Action<ICombatant> OnUpdateHealth;
        public event Action<ICombatant> OnShowActionMenu;
        public event Action OnHideActionMenu;

        public void Initialize(ITurnManager turnManager, CombatStateMachine stateMachine)
        {
            _turnManager = turnManager;
            _stateMachine = stateMachine;

            // Subscribe
            if (_turnManager != null)
            {
                _turnManager.OnTurnStart += HandleTurnStart;
                _turnManager.OnTurnEnd += HandleTurnEnd;
            }
        }

        private void OnDestroy()
        {
            if (_turnManager != null)
            {
                _turnManager.OnTurnStart -= HandleTurnStart;
                _turnManager.OnTurnEnd -= HandleTurnEnd;
            }
        }

        private void HandleTurnStart(ICombatant combatant)
        {
            // Show Banner
            OnShowTurnBanner?.Invoke($"{combatant.DisplayName}'s Turn");

            // If Player, Show Menu (Check if input needed)
            // Simplified: If it's Player Team (0), show menu logic
            if (combatant.IsPlayerControlled)
            {
                OnShowActionMenu?.Invoke(combatant);
            }
            else
            {
                OnHideActionMenu?.Invoke();
            }
        }

        private void HandleTurnEnd(ICombatant combatant)
        {
            OnHideActionMenu?.Invoke();
        }

        // Called by Combatant events (if we subscribed to them)
        public void UpdateHealthUI(ICombatant combatant)
        {
            OnUpdateHealth?.Invoke(combatant);
        }
    }
}
