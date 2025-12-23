// ============================================================================
// RPGPlatform.Systems.Progression - Progression Manager
// Logic for XP tracking, Leveling up, and calculating Tiers
// ============================================================================

using System;
using UnityEngine;
using RPGPlatform.Core.Progression;
using RPGPlatform.Data;

namespace RPGPlatform.Systems.Progression
{
    public class ProgressionManager : MonoBehaviour, IProgressionService
    {
        [Header("Configuration")]
        [SerializeField] private ProgressionConfig _config;

        [Header("State")]
        [SerializeField] private int _currentLevel = 1;
        [SerializeField] private long _currentXP = 0;

        public int CurrentLevel => _currentLevel;
        public long CurrentXP => _currentXP;
        public long XPToNextLevel => _config != null ? _config.GetXPForLevel(_currentLevel + 1) : 0;

        public string CurrentTierId 
        {
            get
            {
                var tier = _config?.GetTierForLevel(_currentLevel);
                return tier != null ? tier.TierId : "unknown";
            }
        }

        public float CurrentStatMultiplier
        {
            get
            {
                var tier = _config?.GetTierForLevel(_currentLevel);
                return tier != null ? tier.StatMultiplier : 1.0f;
            }
        }

        public event Action<int> OnLevelUp;
        public event Action<string> OnTierChanged;
        public event Action<long, long> OnXPChanged;

        // Singleton for prototype ease
        public static ProgressionManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void Initialize(ProgressionConfig config, int startLevel = 1, long startXP = 0)
        {
            _config = config;
            _currentLevel = startLevel;
            _currentXP = startXP;
            
            // Validate config
            if (_config == null)
            {
                Debug.LogError("[ProgressionManager] No config provided!");
            }
        }

        public void AddXP(long amount)
        {
            if (_config == null) return;
            if (_currentLevel >= _config.MaxLevel) return;

            long oldXP = _currentXP;
            _currentXP += amount;
            
            OnXPChanged?.Invoke(_currentXP, XPToNextLevel);

            CheckLevelUp();
        }

        public bool IsTierUnlocked(string tierId)
        {
            // Simple check: Is current tier equal or higher than required?
            // This requires Tiers to be ordered or have a Rank.
            // For now, let's look up the required tier in config and compare min levels.
            
            if (_config == null) return true;

            var currentTier = _config.GetTierForLevel(_currentLevel);
            var requiredTier = _config.Tiers.Find(t => t.TierId == tierId);

            if (requiredTier == null) return true; // Required tier doesn't exist? validation needed.
            if (currentTier == null) return false;

            return _currentLevel >= requiredTier.MinLevel;
        }

        private void CheckLevelUp()
        {
            if (_config == null) return;

            bool leveledUp = false;
            string oldTierId = CurrentTierId;

            while (_currentLevel < _config.MaxLevel && _currentXP >= _config.GetXPForLevel(_currentLevel + 1))
            {
                _currentLevel++;
                leveledUp = true;
                Debug.Log($"[ProgressionManager] Leveled Up! Now Level {_currentLevel}");
                OnLevelUp?.Invoke(_currentLevel);
            }

            if (leveledUp)
            {
                 // Check if tier changed
                 string newTierId = CurrentTierId;
                 if (newTierId != oldTierId)
                 {
                     Debug.Log($"[ProgressionManager] Tier Changed! {oldTierId} -> {newTierId}");
                     OnTierChanged?.Invoke(newTierId);
                 }
            }
        }
    }
}
