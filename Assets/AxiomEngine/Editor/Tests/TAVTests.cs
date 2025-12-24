// ============================================================================
// Axiom RPG Engine - TAV Automated Tests
// Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.
// ============================================================================

using NUnit.Framework;
using RPGPlatform.Editor.TAV;
using UnityEngine;

namespace RPGPlatform.Tests
{
    [TestFixture]
    public class TAVTests
    {
        [SetUp]
        public void SetUp()
        {
            AxiomShell.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            AxiomShell.Cleanup();
        }

        [Test]
        public void Test_VorgossosIncursion_PureLogic()
        {
            // This runs the full scenario and asserts internally if needed
            // For now, we'll just run it and ensure no exceptions
            VorgossosScenario.Run();
            
            // Additional verification
            Assert.IsTrue(AxiomShell.Combat != null, "CombatManager should be initialized");
            Assert.IsTrue(AxiomShell.Dialogue != null, "DialogueManager should be initialized");
        }
    }
}
