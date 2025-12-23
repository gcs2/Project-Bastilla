// ============================================================================
// RPGPlatform.Systems.Morality - Morality Effect Manager
// Generic system for applying morality-based effects using data-driven configs
// ============================================================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RPGPlatform.Core;
using RPGPlatform.Data;

namespace RPGPlatform.Systems.Morality
{
    /// <summary>
    /// Generic system for managing morality-based effects
    /// Uses data-driven MoralityEffectConfig ScriptableObjects
    /// Replaces hardcoded TranshumanistBuffSystem and ChantryInfamySystem
    /// </summary>
    public class MoralityEffectManager
    {
        private readonly MoralityState _moralityState;
        private readonly string _axisId;
        private readonly List<MoralityEffectConfig> _effectConfigs;
        
        private MoralityEffectConfig _currentEffect;
        private ICombatant _affectedCombatant;
        
        public event System.Action<MoralityEffectConfig> OnEffectChanged;
        
        public MoralityEffectManager(MoralityState moralityState, string axisId, params MoralityEffectConfig[] effectConfigs)
        {
            _moralityState = moralityState;
            _axisId = axisId;
            _effectConfigs = new List<MoralityEffectConfig>(effectConfigs);
            
            // Sort by min value (descending) for proper tier matching
            _effectConfigs.Sort((a, b) => b.MinValue.CompareTo(a.MinValue));
            
            // Subscribe to morality changes
            _moralityState.OnAxisChanged += OnMoralityChanged;
        }
        
        /// <summary>
        /// Get the current active effect based on morality value
        /// </summary>
        public MoralityEffectConfig GetCurrentEffect()
        {
            float value = _moralityState.GetAxisValue(_axisId);
            
            foreach (var config in _effectConfigs)
            {
                if (config.IsInRange(value))
                    return config;
            }
            
            return null;
        }
        
        /// <summary>
        /// Apply effects to a combatant
        /// </summary>
        public void ApplyEffects(ICombatant combatant)
        {
            if (combatant == null)
            {
                Debug.LogWarning("[MoralityEffectManager] Cannot apply effects to null combatant");
                return;
            }
            
            // Remove previous effect if any
            if (_currentEffect != null && _affectedCombatant != null)
            {
                _currentEffect.RemoveFromStats(_affectedCombatant.Stats);
            }
            
            // Apply new effect
            _affectedCombatant = combatant;
            _currentEffect = GetCurrentEffect();
            
            if (_currentEffect != null)
            {
                _currentEffect.ApplyToStats(combatant.Stats);
                Debug.Log($"[MoralityEffectManager] Applied '{_currentEffect.EffectName}' to {combatant.Name}");
            }
        }
        
        /// <summary>
        /// Remove effects from the current combatant
        /// </summary>
        public void RemoveEffects()
        {
            if (_currentEffect != null && _affectedCombatant != null)
            {
                _currentEffect.RemoveFromStats(_affectedCombatant.Stats);
                Debug.Log($"[MoralityEffectManager] Removed '{_currentEffect.EffectName}' from {_affectedCombatant.Name}");
            }
            
            _currentEffect = null;
            _affectedCombatant = null;
        }
        
        /// <summary>
        /// Check if faction access is allowed
        /// </summary>
        public bool AllowsFactionAccess()
        {
            var effect = GetCurrentEffect();
            return effect == null || effect.AllowsFactionAccess;
        }
        
        /// <summary>
        /// Check if territory access is allowed
        /// </summary>
        public bool AllowsTerritoryAccess()
        {
            var effect = GetCurrentEffect();
            return effect == null || effect.AllowsTerritoryAccess;
        }
        
        /// <summary>
        /// Get social penalty modifier
        /// </summary>
        public int GetSocialPenalty()
        {
            var effect = GetCurrentEffect();
            return effect?.SocialPenalty ?? 0;
        }
        
        /// <summary>
        /// Get price multiplier
        /// </summary>
        public float GetPriceMultiplier()
        {
            var effect = GetCurrentEffect();
            return effect?.PriceMultiplier ?? 1.0f;
        }
        
        /// <summary>
        /// Check if an ability is blocked
        /// </summary>
        public bool IsAbilityBlocked(string abilityId)
        {
            var effect = GetCurrentEffect();
            return effect != null && effect.IsAbilityBlocked(abilityId);
        }
        
        /// <summary>
        /// Get description of current effect
        /// </summary>
        public string GetEffectDescription()
        {
            var effect = GetCurrentEffect();
            return effect?.GetFullDescription() ?? "No active effects";
        }
        
        /// <summary>
        /// Get short status text
        /// </summary>
        public string GetStatusText()
        {
            var effect = GetCurrentEffect();
            return effect?.EffectName ?? "Neutral";
        }
        
        /// <summary>
        /// Handle morality changes
        /// </summary>
        private void OnMoralityChanged(string axisId, float newValue)
        {
            if (axisId != _axisId)
                return;
            
            var newEffect = GetCurrentEffect();
            
            // Effect changed
            if (newEffect != _currentEffect)
            {
                // Remove old effect
                if (_currentEffect != null && _affectedCombatant != null)
                {
                    _currentEffect.RemoveFromStats(_affectedCombatant.Stats);
                }
                
                // Apply new effect
                if (newEffect != null && _affectedCombatant != null)
                {
                    newEffect.ApplyToStats(_affectedCombatant.Stats);
                }
                
                _currentEffect = newEffect;
                OnEffectChanged?.Invoke(newEffect);
                
                Debug.Log($"[MoralityEffectManager] Effect changed to: {newEffect?.EffectName ?? "None"}");
            }
        }
    }
}
