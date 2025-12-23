// ============================================================================
// SunEater.GameData - Sun Eater Morality Configuration
// Game-specific morality setup for The Sun Eater
// Uses generic RPGPlatform systems with Sun Eater data
// ============================================================================
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RPGPlatform.Core;
using RPGPlatform.Data;
using RPGPlatform.Systems.Morality;

namespace SunEater.GameData
{
    /// <summary>
    /// Sun Eater game-specific morality configuration
    /// Manages the Humanist/Transhumanist axis using data-driven effect configs
    /// This is GAME DATA, not platform code
    /// </summary>
    public class SunEaterMoralityConfig : MonoBehaviour
    {
        [Header("Axis Configuration")]
        [SerializeField] private MoralityAxisConfig _humanismAxis;
        
        [Header("Transhumanist Effect Configs")]
        [Tooltip("Minor cybernetic enhancements (-25 to -49)")]
        [SerializeField] private MoralityEffectConfig _minorCybernetics;
        
        [Tooltip("Moderate cybernetic enhancements (-50 to -74)")]
        [SerializeField] private MoralityEffectConfig _moderateCybernetics;
        
        [Tooltip("Major cybernetic enhancements (-75 to -100)")]
        [SerializeField] private MoralityEffectConfig _majorCybernetics;
        
        [Header("Chantry Reputation Configs")]
        [Tooltip("Suspicious standing (-25 to -49)")]
        [SerializeField] private MoralityEffectConfig _chantrySuspicious;
        
        [Tooltip("Condemned standing (-50 to -74)")]
        [SerializeField] private MoralityEffectConfig _chantryCondemned;
        
        [Tooltip("Heretic standing (-75 to -100)")]
        [SerializeField] private MoralityEffectConfig _chantryHeretic;
        
        [Header("Systems")]
        private MoralityState _moralityState;
        private MoralityEffectManager _cyberneticEffects;
        private MoralityEffectManager _chantryReputation;
        
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo = true;
        
        /// <summary>
        /// Get the morality state
        /// </summary>
        public MoralityState MoralityState => _moralityState;
        
        /// <summary>
        /// Get the cybernetic effects manager
        /// </summary>
        public MoralityEffectManager CyberneticEffects => _cyberneticEffects;
        
        /// <summary>
        /// Get the Chantry reputation manager
        /// </summary>
        public MoralityEffectManager ChantryReputation => _chantryReputation;
        
        private void Awake()
        {
            InitializeSystems();
        }
        
        /// <summary>
        /// Initialize the morality systems
        /// </summary>
        private void InitializeSystems()
        {
            // Create default axis if not assigned
            if (_humanismAxis == null)
            {
                _humanismAxis = CreateDefaultHumanismAxis();
            }
            
            // Initialize morality state
            _moralityState = new MoralityState();
            _moralityState.Initialize(_humanismAxis);
            
            // Initialize cybernetic effects (if configs provided)
            if (_minorCybernetics != null || _moderateCybernetics != null || _majorCybernetics != null)
            {
                var cyberConfigs = new[] { _minorCybernetics, _moderateCybernetics, _majorCybernetics }
                    .Where(c => c != null).ToArray();
                    
                _cyberneticEffects = new MoralityEffectManager(_moralityState, "humanism", cyberConfigs);
            }
            
            // Initialize Chantry reputation (if configs provided)
            if (_chantrySuspicious != null || _chantryCondemned != null || _chantryHeretic != null)
            {
                var chantryConfigs = new[] { _chantrySuspicious, _chantryCondemned, _chantryHeretic }
                    .Where(c => c != null).ToArray();
                    
                _chantryReputation = new MoralityEffectManager(_moralityState, "humanism", chantryConfigs);
            }
            
            // Subscribe to events
            _moralityState.OnAxisChanged += OnMoralityChanged;
            
            if (_cyberneticEffects != null)
                _cyberneticEffects.OnEffectChanged += OnCyberneticEffectChanged;
                
            if (_chantryReputation != null)
                _chantryReputation.OnEffectChanged += OnChantryReputationChanged;
            
            if (_showDebugInfo)
            {
                Debug.Log("[SunEaterMoralitySetup] Initialized Sun Eater morality systems");
                Debug.Log($"  Axis: {_humanismAxis.DisplayName}");
                Debug.Log($"  Poles: {_humanismAxis.PositivePoleLabel} / {_humanismAxis.NegativePoleLabel}");
            }
        }
        
        /// <summary>
        /// Create default humanism axis configuration
        /// </summary>
        private MoralityAxisConfig CreateDefaultHumanismAxis()
        {
            var config = ScriptableObject.CreateInstance<MoralityAxisConfig>();
            config.AxisId = "humanism";
            config.DisplayName = "Humanist vs Transhumanist";
            config.PositivePoleLabel = "Humanist";
            config.NegativePoleLabel = "Transhumanist";
            config.MinValue = -100f;
            config.MaxValue = 100f;
            config.DefaultValue = 0f;
            config.PositiveColor = new Color(0.2f, 0.8f, 1f); // Cyan for Humanist
            config.NegativeColor = new Color(1f, 0.3f, 0.3f); // Red for Transhumanist
            
            return config;
        }
        
        /// <summary>
        /// Apply morality effects to a combatant
        /// </summary>
        public void ApplyMoralityEffects(ICombatant combatant)
        {
            if (combatant == null)
            {
                Debug.LogWarning("[SunEaterMoralitySetup] Cannot apply effects to null combatant");
                return;
            }
            
            // Apply cybernetic effects
            _cyberneticEffects?.ApplyEffects(combatant);
            
            // Chantry reputation doesn't modify stats, just access/social
            
            if (_showDebugInfo)
            {
                Debug.Log($"[SunEaterMoralitySetup] Applied morality effects to {combatant.Name}");
                Debug.Log($"  {_moralityState.GetStateDescription()}");
                if (_cyberneticEffects != null)
                    Debug.Log($"  Cybernetics: {_cyberneticEffects.GetStatusText()}");
                if (_chantryReputation != null)
                    Debug.Log($"  Chantry: {_chantryReputation.GetStatusText()}");
            }
        }
        
        /// <summary>
        /// Make a morality choice (positive = humanist, negative = transhumanist)
        /// </summary>
        public void MakeMoralityChoice(float delta, string choiceDescription = "")
        {
            if (!string.IsNullOrEmpty(choiceDescription))
            {
                Debug.Log($"[SunEaterMoralitySetup] Choice: {choiceDescription} ({delta:+0;-0})");
            }
            
            _moralityState.ModifyAxis("humanism", delta);
        }
        
        /// <summary>
        /// Get full status report
        /// </summary>
        public string GetStatusReport()
        {
            var report = $"=== Sun Eater Morality Status ===\n";
            report += $"{_moralityState.GetStateDescription()}\n\n";
            
            if (_cyberneticEffects != null)
            {
                report += "Cybernetic Enhancements:\n";
                report += $"{_cyberneticEffects.GetEffectDescription()}\n\n";
            }
            
            if (_chantryReputation != null)
            {
                report += "Chantry Standing:\n";
                report += $"{_chantryReputation.GetEffectDescription()}\n";
            }
            
            return report;
        }
        
        /// <summary>
        /// Handle morality changes
        /// </summary>
        private void OnMoralityChanged(string axisId, float newValue)
        {
            if (_showDebugInfo)
            {
                Debug.Log($"[SunEaterMoralitySetup] Morality changed: {newValue:F1}");
                Debug.Log($"  Alignment: {_humanismAxis.GetPoleLabel(newValue)}");
            }
        }
        
        /// <summary>
        /// Handle cybernetic effect changes
        /// </summary>
        private void OnCyberneticEffectChanged(MoralityEffectConfig newEffect)
        {
            if (_showDebugInfo)
            {
                Debug.Log($"[SunEaterMoralitySetup] Cybernetic tier changed: {newEffect?.EffectName ?? "None"}");
            }
        }
        
        /// <summary>
        /// Handle Chantry reputation changes
        /// </summary>
        private void OnChantryReputationChanged(MoralityEffectConfig newEffect)
        {
            if (_showDebugInfo)
            {
                Debug.Log($"[SunEaterMoralitySetup] Chantry standing changed: {newEffect?.EffectName ?? "Neutral"}");
            }
        }
        
        #region Context Menu Commands (for testing)
        
        [ContextMenu("Make Humanist Choice (+25)")]
        private void MakeHumanistChoice()
        {
            MakeMoralityChoice(25f, "Chose to preserve human nature");
        }
        
        [ContextMenu("Make Transhumanist Choice (-25)")]
        private void MakeTranshumanistChoice()
        {
            MakeMoralityChoice(-25f, "Chose cybernetic enhancement");
        }
        
        [ContextMenu("Show Status Report")]
        private void ShowStatusReport()
        {
            Debug.Log(GetStatusReport());
        }
        
        #endregion
    }
}
