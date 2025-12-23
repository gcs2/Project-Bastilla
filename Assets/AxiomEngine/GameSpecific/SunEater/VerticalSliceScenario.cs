// ============================================================================
// Axiom RPG Engine - Vertical Slice Scenario (Vorgossos Arrival)
// Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.
// ============================================================================

using UnityEngine;
using System.Collections;
using RPGPlatform.Core;
using RPGPlatform.Core.Dialogue;
using RPGPlatform.Systems.Combat;
using RPGPlatform.Systems.Travel;

namespace RPGPlatform.GameSpecific.SunEater
{
    /// <summary>
    /// Orchestrates the "Vorgossos Arrival" vertical slice.
    /// Manages the sequence: Cutscene -> Dialogue -> Combat Encounter.
    /// </summary>
    public class VerticalSliceScenario : MonoBehaviour
    {
        [Header("Dialogue Content")]
        [SerializeField] private string _arrivalConversationId = "vorgossos_gatekeeper";
        
        [Header("Combat Setup")]
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private Transform[] _spawnPoints;

        private IDialogueService _dialogueService;
        private ICombatResolver _combatResolver;
        private ITurnManager _turnManager;

        private void Start()
        {
            // Resolve services
            _dialogueService = ServiceLocator.Get<IDialogueService>();
            _combatResolver = ServiceLocator.Get<ICombatResolver>();
            _turnManager = ServiceLocator.Get<ITurnManager>();

            StartCoroutine(RunSequence());
        }

        private IEnumerator RunSequence()
        {
            Debug.Log("[Scenario] Starting Vorgossos Arrival Sequence...");

            // 1. Cutscene Logic (Mocked)
            yield return new WaitForSeconds(1f);
            Debug.Log("[Scenario] Playing Arrival Cutscene...");
            yield return new WaitForSeconds(2f);

            // 2. Start Initial Dialogue
            Debug.Log("[Scenario] Triggering Gatekeeper Dialogue...");
            if (_dialogueService != null)
            {
                // In a real scenario, we'd find the NPC combatant for the Gatekeeper
                _dialogueService.StartConversation(_arrivalConversationId, null, null);
                
                // Wait for dialogue to end (hooked into events in production)
                // For this mock, we wait 3 seconds
                yield return new WaitForSeconds(3f);
            }

            // 3. Trigger Combat Encounter
            Debug.Log("[Scenario] Dialogue leads to hostility! Starting Combat...");
            SetupCombat();
        }

        private void SetupCombat()
        {
            // Initialize mock combatants
            var player = CombatantFactory.CreateBasic("Player", new CombatStats(), Vector3.zero, true, 0);
            var enemy = CombatantFactory.CreateBasic("Chantry Guard", new CombatStats { ArmorClass = 12 }, Vector3.forward * 5, false, 1);

            var combatants = new System.Collections.Generic.List<ICombatant> { player, enemy };
            
            if (_turnManager != null)
            {
                _turnManager.StartCombat(combatants);
            }
        }
    }
}
