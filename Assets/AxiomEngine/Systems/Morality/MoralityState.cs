// ============================================================================
// RPGPlatform.Morality - Morality State Implementation
// Tracks morality values across multiple configurable axes
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using RPGPlatform.Core;
using RPGPlatform.Data;

namespace RPGPlatform.Systems.Morality
{
    /// <summary>
    /// Tracks morality values on multiple axes (e.g., Humanist/Transhumanist, Light/Dark)
    /// Implements IMoralityService for integration with ability system
    /// </summary>
    [Serializable]
    public class MoralityState : IMoralityService
    {
        [SerializeField]
        private List<AxisState> _axes = new List<AxisState>();
        
        private Dictionary<string, AxisState> _axisLookup = new Dictionary<string, AxisState>();
        
        public bool HasMorality => _axes.Count > 0;
        
        /// <summary>
        /// Event fired when any axis value changes
        /// </summary>
        public event Action<string, float> OnAxisChanged;
        
        /// <summary>
        /// Internal storage for a single axis value
        /// </summary>
        [Serializable]
        private class AxisState
        {
            public string AxisId;
            public float Value;
            public MoralityAxisConfig Config;
            
            public AxisState(string axisId, float value, MoralityAxisConfig config)
            {
                AxisId = axisId;
                Value = value;
                Config = config;
            }
        }
        
        /// <summary>
        /// Initialize the morality state with default axes
        /// </summary>
        public void Initialize(params MoralityAxisConfig[] configs)
        {
            _axes.Clear();
            _axisLookup.Clear();
            
            foreach (var config in configs)
            {
                if (config == null)
                {
                    Debug.LogWarning("[MoralityState] Null config provided, skipping");
                    continue;
                }
                
                var axisState = new AxisState(config.AxisId, config.DefaultValue, config);
                _axes.Add(axisState);
                _axisLookup[config.AxisId] = axisState;
            }
        }
        
        /// <summary>
        /// Add a new axis at runtime
        /// </summary>
        public void AddAxis(MoralityAxisConfig config)
        {
            if (config == null)
            {
                Debug.LogError("[MoralityState] Cannot add null config");
                return;
            }
            
            if (_axisLookup.ContainsKey(config.AxisId))
            {
                Debug.LogWarning($"[MoralityState] Axis '{config.AxisId}' already exists");
                return;
            }
            
            var axisState = new AxisState(config.AxisId, config.DefaultValue, config);
            _axes.Add(axisState);
            _axisLookup[config.AxisId] = axisState;
        }
        
        /// <summary>
        /// Get the current value of an axis
        /// </summary>
        public float GetAxisValue(string axisId)
        {
            if (_axisLookup.TryGetValue(axisId, out var axis))
            {
                return axis.Value;
            }
            
            Debug.LogWarning($"[MoralityState] Axis '{axisId}' not found, returning 0");
            return 0f;
        }
        
        /// <summary>
        /// Modify an axis value by a delta amount
        /// </summary>
        public void ModifyAxis(string axisId, float delta)
        {
            if (!_axisLookup.TryGetValue(axisId, out var axis))
            {
                Debug.LogWarning($"[MoralityState] Cannot modify unknown axis '{axisId}'");
                return;
            }
            
            float oldValue = axis.Value;
            axis.Value = axis.Config.ClampValue(axis.Value + delta);
            
            if (Mathf.Abs(oldValue - axis.Value) > 0.001f)
            {
                OnAxisChanged?.Invoke(axisId, axis.Value);
                Debug.Log($"[MoralityState] {axisId}: {oldValue:F1} -> {axis.Value:F1} (delta: {delta:F1})");
            }
        }
        
        /// <summary>
        /// Set an axis to a specific value
        /// </summary>
        public void SetAxisValue(string axisId, float value)
        {
            if (!_axisLookup.TryGetValue(axisId, out var axis))
            {
                Debug.LogWarning($"[MoralityState] Cannot set unknown axis '{axisId}'");
                return;
            }
            
            float oldValue = axis.Value;
            axis.Value = axis.Config.ClampValue(value);
            
            if (Mathf.Abs(oldValue - axis.Value) > 0.001f)
            {
                OnAxisChanged?.Invoke(axisId, axis.Value);
            }
        }
        
        /// <summary>
        /// Check if the current morality meets the specified requirements
        /// </summary>
        public bool MeetsRequirement(string axisId, float? minValue, float? maxValue)
        {
            if (string.IsNullOrEmpty(axisId))
                return true;
            
            if (!_axisLookup.TryGetValue(axisId, out var axis))
            {
                Debug.LogWarning($"[MoralityState] Axis '{axisId}' not found for requirement check");
                return false;
            }
            
            float value = axis.Value;
            
            // Check minimum requirement
            if (minValue.HasValue && value < minValue.Value)
                return false;
            
            // Check maximum requirement
            if (maxValue.HasValue && value > maxValue.Value)
                return false;
            
            return true;
        }
        
        /// <summary>
        /// Get the configuration for an axis
        /// </summary>
        public MoralityAxisConfig GetAxisConfig(string axisId)
        {
            if (_axisLookup.TryGetValue(axisId, out var axis))
            {
                return axis.Config;
            }
            
            return null;
        }
        
        /// <summary>
        /// Get all axis IDs
        /// </summary>
        public IEnumerable<string> GetAllAxisIds()
        {
            return _axisLookup.Keys;
        }
        
        /// <summary>
        /// Get a description of the current morality state
        /// </summary>
        public string GetStateDescription()
        {
            if (_axes.Count == 0)
                return "No morality axes defined";
            
            var descriptions = new List<string>();
            foreach (var axis in _axes)
            {
                string label = axis.Config.GetPoleLabel(axis.Value);
                descriptions.Add($"{axis.Config.DisplayName}: {axis.Value:F0} ({label})");
            }
            
            return string.Join(", ", descriptions);
        }
    }
}
