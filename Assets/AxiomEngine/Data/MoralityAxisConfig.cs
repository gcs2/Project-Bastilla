// ============================================================================
// RPGPlatform.Data - Morality Axis Configuration
// Configurable morality axes for alignment-based gameplay
// ScriptableObject for data-driven design
// ============================================================================

using System;
using UnityEngine;

namespace RPGPlatform.Data
{
    /// <summary>
    /// Defines a morality axis with configurable poles and value ranges
    /// Example: Humanist (+100) <-> Transhumanist (-100)
    /// This is PLATFORM DATA, not game-specific
    /// </summary>
    [CreateAssetMenu(fileName = "MoralityAxis", menuName = "RPG/Morality/Axis Config")]
    public class MoralityAxisConfig : ScriptableObject
    {
        [Header("Axis Identity")]
        [Tooltip("Unique identifier for this axis (e.g., 'humanism')")]
        public string AxisId = "alignment";
        
        [Tooltip("Display name for UI (e.g., 'Humanist vs Transhumanist')")]
        public string DisplayName = "Alignment";
        
        [Header("Pole Labels")]
        [Tooltip("Label for positive values (e.g., 'Humanist')")]
        public string PositivePoleLabel = "Good";
        
        [Tooltip("Label for negative values (e.g., 'Transhumanist')")]
        public string NegativePoleLabel = "Evil";
        
        [Header("Value Range")]
        [Tooltip("Minimum value (typically -100)")]
        public float MinValue = -100f;
        
        [Tooltip("Maximum value (typically 100)")]
        public float MaxValue = 100f;
        
        [Tooltip("Starting value for new characters")]
        public float DefaultValue = 0f;
        
        [Header("Visual Settings")]
        [Tooltip("Color for positive pole")]
        public Color PositiveColor = Color.cyan;
        
        [Tooltip("Color for negative pole")]
        public Color NegativeColor = Color.red;
        
        /// <summary>
        /// Get the label for a specific value on this axis
        /// </summary>
        public string GetPoleLabel(float value)
        {
            if (value > 0)
                return PositivePoleLabel;
            else if (value < 0)
                return NegativePoleLabel;
            else
                return "Neutral";
        }
        
        /// <summary>
        /// Get the color for a specific value on this axis
        /// </summary>
        public Color GetColor(float value)
        {
            if (value > 0)
                return Color.Lerp(Color.white, PositiveColor, Mathf.Abs(value) / MaxValue);
            else if (value < 0)
                return Color.Lerp(Color.white, NegativeColor, Mathf.Abs(value) / Mathf.Abs(MinValue));
            else
                return Color.white;
        }
        
        /// <summary>
        /// Clamp a value to this axis's valid range
        /// </summary>
        public float ClampValue(float value)
        {
            return Mathf.Clamp(value, MinValue, MaxValue);
        }
        
        /// <summary>
        /// Get normalized value (-1 to 1)
        /// </summary>
        public float GetNormalizedValue(float value)
        {
            if (value > 0)
                return value / MaxValue;
            else if (value < 0)
                return value / Mathf.Abs(MinValue);
            else
                return 0f;
        }
    }
}
