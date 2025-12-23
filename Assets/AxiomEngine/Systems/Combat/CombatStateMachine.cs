// ============================================================================
// Axiom RPG Engine - Combat State Machine
// Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using RPGPlatform.Core;

namespace RPGPlatform.Systems.Combat
{
    public class CombatStateMachine : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private CombatPhase _currentPhase;
        
        private CombatContext _context;
        private Dictionary<CombatPhase, ICombatPhaseState> _states = new Dictionary<CombatPhase, ICombatPhaseState>();
        private ITurnManager _turnManager;

        public CombatPhase CurrentPhase => _currentPhase;
        public CombatContext Context => _context;
        public bool IsCombatOver => _context.AllPlayersDefeated || _context.AllEnemiesDefeated;
        
        public event Action<CombatPhase> OnPhaseChanged;

        public void Initialize(ITurnManager turnManager)
        {
            _turnManager = turnManager;
            _context = new CombatContext();
            
            // Register States
            _states[CombatPhase.Planning] = new PlanningState();
            _states[CombatPhase.Execution] = new ExecutionState();
            /* _states[CombatPhase.Resolution] = new ResolutionState(); */ // Merged for simplicity
            
            _currentPhase = CombatPhase.NotStarted;
        }

        public void StartCombat(List<ICombatant> combatants)
        {
            _context.AllCombatants = combatants;
            _turnManager.StartCombat(combatants); // This sets turn order
            _context.TurnOrder = _turnManager.TurnOrder;
            
            ChangePhase(CombatPhase.Planning);
        }

        private void Update()
        {
            if (_states.ContainsKey(_currentPhase))
            {
                _states[_currentPhase].Update(_context);
                
                var next = _states[_currentPhase].GetNextPhase(_context);
                if (next.HasValue && next.Value != _currentPhase)
                {
                    ChangePhase(next.Value);
                }
            }
        }

        public void TransitionTo(CombatPhase newPhase)
        {
            if (_states.ContainsKey(_currentPhase))
                 _states[_currentPhase].Exit(_context);

            _currentPhase = newPhase;
            Debug.Log($"[CombatState] Entering {_currentPhase}");
            OnPhaseChanged?.Invoke(_currentPhase);

            if (_states.ContainsKey(_currentPhase))
                 _states[_currentPhase].Enter(_context);
        }
        
        private void ChangePhase(CombatPhase newPhase) => TransitionTo(newPhase);
        
        // Public Input API for Planing
        public void SubmitCommand(ICombatCommand command)
        {
             if (_currentPhase != CombatPhase.Planning) return;
             _context.SelectedCommand = command;
             _context.PlayerInputReceived = true;
        }
    }

    // --- Concrete States ---

    public class PlanningState : ICombatPhaseState
    {
        public CombatPhase Phase => CombatPhase.Planning;

        public void Enter(CombatContext context)
        {
            context.PlayerInputReceived = false;
            context.SelectedCommand = null;
            
            // If AI Turn, trigger AI logic (Mock)
            if (context.CurrentCombatant != null && !context.CurrentCombatant.IsPlayerControlled)
            {
                Debug.Log("[Planning] AI Thinking...");
                // Simulating AI input delay
                // In real system, AIController would call SubmitCommand
                
                // Hack for prototype: immediately pass
                // context.SelectedCommand = new EndTurnCommand(context.CurrentCombatant, ...);
                context.PlayerInputReceived = true; // Skip for now or need ref to TurnManager
            }
        }

        public void Update(CombatContext context) { }

        public void Exit(CombatContext context) 
        {
             if (context.SelectedCommand != null)
                 context.PendingCommands.Enqueue(context.SelectedCommand);
        }

        public CombatPhase? GetNextPhase(CombatContext context)
        {
            if (context.PlayerInputReceived) 
                return CombatPhase.Execution;
            return null;
        }
    }

    public class ExecutionState : ICombatPhaseState
    {
        public CombatPhase Phase => CombatPhase.Execution;

        public void Enter(CombatContext context) { }

        public void Update(CombatContext context)
        {
            // Process Queue
            while (context.PendingCommands.Count > 0)
            {
                var cmd = context.PendingCommands.Dequeue();
                if (cmd.CanExecute())
                {
                    Debug.Log($"[Execute] {cmd.CommandName}");
                    cmd.Execute();
                    context.ExecutedCommands.Add(cmd);
                }
            }
        }

        public void Exit(CombatContext context) { }

        public CombatPhase? GetNextPhase(CombatContext context)
        {
             // If queue empty, go back to planning (next turn logic handled in EndTurnCommand usually)
             // Actually, if TurnManager was advanced, the CurrentCombatant changed.
             // We return to Planning for the NEW combatant.
             
             if (context.PendingCommands.Count == 0)
                 return CombatPhase.Planning;
             return null;
        }
    }
}
