// ============================================================================
// Axiom RPG Engine - Travel Manager
// Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.
// ============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using RPGPlatform.Core;
using RPGPlatform.Core.Progression;
using RPGPlatform.Data;

namespace RPGPlatform.Systems.Travel
{
    public class TravelManager : MonoBehaviour
    {
        private ISceneLoader _sceneLoader;
        private IProgressionService _progression;
        private IQuestService _questService;

        public event Action<LocationData> OnTravelStarted;
        public event Action<LocationData> OnTravelCompleted;
        public event Action<LocationData, string> OnTravelBlocked;

        public LocationData CurrentLocation { get; private set; }

        public static TravelManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void Initialize(ISceneLoader loader, IProgressionService progression, IQuestService quests)
        {
            _sceneLoader = loader;
            _progression = progression;
            _questService = quests;
        }

        public bool CanTravelTo(LocationData destination, out string blockReason)
        {
            blockReason = string.Empty;

            if (destination == null)
            {
                blockReason = "Invalid Destination";
                return false;
            }

            // check level
            if (_progression != null && _progression.CurrentLevel < destination.MinLevel)
            {
                blockReason = $"Level {destination.MinLevel} Required";
                return false;
            }

            // check quests
            if (_questService != null)
            {
                foreach (var qId in destination.RequiredQuestIds)
                {
                    if (!_questService.IsQuestCompleted(qId))
                    {
                        blockReason = $"Quest Required: {qId}";
                        return false;
                    }
                }
            }

            return true;
        }

        public async void TravelTo(LocationData destination)
        {
            if (!CanTravelTo(destination, out string reason))
            {
                Debug.LogWarning($"Travel Blocked: {reason}");
                OnTravelBlocked?.Invoke(destination, reason);
                return;
            }

            Debug.Log($"[TravelManager] departing for {destination.DisplayName}...");
            OnTravelStarted?.Invoke(destination);

            if (_sceneLoader != null)
            {
                await _sceneLoader.LoadSceneAsync(destination.MainSceneName);
            }
            else
            {
                // Fallback / Mock behavior
                await Task.Delay(100); 
            }

            CurrentLocation = destination;
            Debug.Log($"[TravelManager] arrived at {destination.DisplayName}");
            OnTravelCompleted?.Invoke(destination);
        }
    }
}
