// ============================================================================
// Axiom RPG Engine - Turn Manager
// Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RPGPlatform.Core;

namespace RPGPlatform.Systems.Combat
{
    public class TurnManager : MonoBehaviour, ITurnManager
    {
        [Header("Debug")]
        [SerializeField] private int _currentRound = 0;
        [SerializeField] private int _turnIndex = -1;
        
        private List<ICombatant> _turnOrder = new List<ICombatant>();
        private ICombatResolver _combatResolver; // For rolling initiative

        public int CurrentRound => _currentRound;
        public List<ICombatant> TurnOrder => _turnOrder;
        
        public ICombatant CurrentCombatant => 
            (_turnIndex >= 0 && _turnIndex < _turnOrder.Count) ? _turnOrder[_turnIndex] : null;

        public event Action<int> OnRoundStart;
        public event Action<ICombatant> OnTurnStart;
        public event Action<ICombatant> OnTurnEnd;
        public event Action<int> OnRoundEnd;

        public void Initialize(ICombatResolver resolver)
        {
            _combatResolver = resolver;
        }

        public void StartCombat(List<ICombatant> combatants)
        {
            if (combatants == null || combatants.Count == 0)
            {
                Debug.LogError("[TurnManager] Cannot start combat with 0 combatants.");
                return;
            }

            Debug.Log("[TurnManager] Starting Combat...");
            _turnOrder = new List<ICombatant>(combatants);
            
            // Roll Initiative
            if (_combatResolver != null)
            {
                // Sort by Initiative Roll + Dex Mod usually
                // Use a stable sort or simple sort
                // We'll create a temporary list of (Combatant, InitiativeScore)
                var logic = combatants.Select(c => new 
                { 
                    C = c, 
                    Init = _combatResolver.RollInitiative(c) 
                }).OrderByDescending(x => x.Init).ToList();

                _turnOrder = logic.Select(x => x.C).ToList();
                
                foreach(var x in logic)
                    Debug.Log($"[Init] {x.C.DisplayName}: {x.Init}");
            }
            else
            {
                // Fallback sort by Dex if no resolver
                Debug.LogWarning("[TurnManager] No resolver, sorting by raw Dex stat.");
                _turnOrder.Sort((a, b) => b.Stats.Dexterity.CompareTo(a.Stats.Dexterity));
            }

            _currentRound = 0;
            StartRound();
        }

        public void NextTurn()
        {
            if (CurrentCombatant != null)
            {
                OnTurnEnd?.Invoke(CurrentCombatant);
            }

            _turnIndex++;

            // Check Round End
            if (_turnIndex >= _turnOrder.Count)
            {
                OnRoundEnd?.Invoke(_currentRound);
                
                // Cleanup / Check death
                _turnOrder.RemoveAll(c => !c.IsAlive);
                
                if (_turnOrder.Count == 0 || IsCombatOver()) // Simple check or external?
                {
                   // Let StateMachine handle "EndCombat" usually, but here we just loop or stop
                   // For now, loop to next round
                }

                StartRound();
            }
            else
            {
                 // Start next turn
                 CurrentCombatant?.TickEffects();
                 OnTurnStart?.Invoke(CurrentCombatant);
                 Debug.Log($"[Turn] {CurrentCombatant?.DisplayName}'s turn.");
            }
        }

        private void StartRound()
        {
            _currentRound++;
            _turnIndex = 0;
            
            // Refresh logic? (e.g. cooldown ticks) handled elsewhere usually
            
            Debug.Log($"[TurnManager] Round {_currentRound} Start");
            OnRoundStart?.Invoke(_currentRound);
            
            // Check if first combatant is alive? List cleaned at end of round.
            if (_turnOrder.Count > 0)
            {
                OnTurnStart?.Invoke(_turnOrder[_turnIndex]);
                Debug.Log($"[Turn] {_turnOrder[_turnIndex].DisplayName}'s turn.");
            }
        }

        public void EndCombat()
        {
            Debug.Log("[TurnManager] Combat Ended.");
            _turnOrder.Clear();
            _currentRound = 0;
            _turnIndex = -1;
        }

        public void Tick()
        {
            // Update cooldowns or periodic effects if needed
            // For now, NextTurn handles the main progression
        }

        private bool IsCombatOver()
        {
            // Simple check: Only one team remains?
            if (_turnOrder.Count == 0) return true;
            int firstTeam = _turnOrder[0].Team;
            return _turnOrder.All(c => c.Team == firstTeam);
        }
    }
}
