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
            
            _travel.Initialize(new MockSceneLoader(), new MockProgressionService(), new MockQuestService());
            _logic.Initialize(_travel, new MockMoralityService(), _cutscenes);
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
        
        private class MockProgressionService : IProgressionService
        {
            public int CurrentLevel => 50;
            public long CurrentXP => 0;
            public long XPToNextLevel => 100;
            public string CurrentTierId => "";
            public float CurrentStatMultiplier => 1f;
            public void AddXP(long a){}
            public bool IsTierUnlocked(string t) => true;
            public event System.Action<int> OnLevelUp;
            public event System.Action<string> OnTierChanged;
            public event System.Action<long, long> OnXPChanged;
        }

        private class MockQuestService : IQuestService
        {
            public bool GetFlag(string f) => true;
            public void SetFlag(string f, bool v) {}
            public int GetQuestStep(string q) => 100;
            public void SetQuestStep(string q, int s) {}
            public bool IsQuestCompleted(string q) => true;
        }

        private class MockMoralityService : IMoralityService
        {
             Dictionary<string, float> _vals = new Dictionary<string, float>();
             public bool HasMorality => true;
             public float GetAxisValue(string id) => _vals.ContainsKey(id) ? _vals[id] : 0;
             public void ModifyAxis(string id, float d) => _vals[id] = d;
             public bool MeetsRequirement(string id, float? min, float? max) => true;
        }
    }
}
