// ============================================================================
// Axiom RPG Engine - Vorgossos Incursion TAV Scenario
// Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.
// ============================================================================

using UnityEditor;
using UnityEngine;
using RPGPlatform.Core;
using RPGPlatform.Core.Dialogue;

namespace RPGPlatform.Editor.TAV
{
    /// <summary>
    /// Test Scenario: The Vorgossos Incursion.
    /// Steps through the demo logic purely via the AxiomShell.
    /// </summary>
    public static class VorgossosScenario
    {
        [MenuItem("Axiom Engine/TAV/Run Vorgossos Incursion")]
        public static void Run()
        {
            Debug.Log("--- STARTING TAV SCENARIO: VORGOSSOS INCURSION ---");
            
            // 1. Setup
            AxiomShell.Initialize();
            
            // 2. Start Conversation
            Debug.Log("[Step 1] Initializing Inquisitor Dialogue...");
            AxiomShell.Execute("talk inquisitor_spectacle");
            
            // 3. Choose the Aggressive Path
            Debug.Log("[Step 2] Selecting Aggressive Response (Index 1)...");
            AxiomShell.Execute("choose 1");
            
            // 4. End Dialogue to trigger Combat
            AxiomShell.Execute("end");
            
            // 5. Verify Combat Trigger (Manual or logic check)
            if (AxiomShell.Combat.IsInCombat)
            {
                Debug.Log("[Step 3] SUCCESS: Combat state engaged.");
            }
            else
            {
                Debug.LogError("[Step 3] FAILURE: Combat state not engaged after selection.");
            }
            
            // 5. Check Morality (Should be 50 by default)
            AxiomShell.Execute("stat");

            Debug.Log("--- TAV SCENARIO COMPLETED ---");
        }
    }
}
