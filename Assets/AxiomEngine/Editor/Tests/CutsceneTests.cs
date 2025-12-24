using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using NUnit.Framework;
using RPGPlatform.Core;
using RPGPlatform.Core.Progression;
using RPGPlatform.Systems.Travel;
using RPGPlatform.Systems.Cutscene;
using RPGPlatform.Data;
using SunEater.Cutscenes;

namespace RPGPlatform.Tests
{
    [TestFixture]
    public class CutsceneTests
    {
        private GameObject _holder;
        private TravelManager _travel;
        private CutsceneController _cutscenes;
        private SunEaterCutsceneLogic _logic;

        [SetUp]
        public void Setup()
        {
            _holder = new GameObject("TestHolder");
            _travel = _holder.AddComponent<TravelManager>();
            _cutscenes = _holder.AddComponent<CutsceneController>();
            _logic = _holder.AddComponent<SunEaterCutsceneLogic>();
            
            _travel.Initialize(new MockSceneLoader(), new TestingCommon.MockProgressionService(), new TestingCommon.MockQuestService());
            _logic.Initialize(_travel, new TestingCommon.MockMoralityService(), _cutscenes);
        }

        [TearDown]
        public void Teardown()
        {
            UnityEngine.Object.DestroyImmediate(_holder);
        }

        [Test]
        public void Test_Travel_BasicDiscovery()
        {
            var loc = ScriptableObject.CreateInstance<LocationData>();
            loc.LocationId = "safe_planet";
            loc.DisplayName = "Safe Planet";
            loc.MinLevel = 1;
            loc.MainSceneName = "Scene_Safe";

            _travel.TravelTo(loc);
            
            Assert.AreEqual("safe_planet", _travel.CurrentLocation.LocationId);
        }

        [Test]
        public void Test_Cutscene_LogicBranching()
        {
            // Verify Vorgossos logic executes without error
            var loc = ScriptableObject.CreateInstance<LocationData>();
            loc.LocationId = "vorgossos";
            
            _travel.TravelTo(loc);
            
            Assert.Pass("Logic executed without exception");
        }

        // Mocks
        private class MockSceneLoader : ISceneLoader
        {
            public Task LoadSceneAsync(string name) => Task.CompletedTask;
        }
    }
}
