// ============================================================================
// RPGPlatform.Systems.Combat - Resource Pool System
// Generic resource management (Energy/Mana/Force/Stamina)
// ============================================================================

using System;
using UnityEngine;
using RPGPlatform.Core;

namespace RPGPlatform.Systems.Combat
{
    /// <summary>
    /// Configuration for a resource type
    /// </summary>
    [CreateAssetMenu(fileName = "ResourceConfig", menuName = "RPG/Combat/Resource Config")]
    public class ResourceConfig : ScriptableObject
    {
        public string ResourceId = "energy";
        public string DisplayName = "Energy";
        public Color ResourceColor = Color.blue;
        public Sprite Icon;
        
        [Header("Regeneration")]
        public bool RegeneratesInCombat = true;
        public int RegenPerTurn = 5;
        public bool RegeneratesOutOfCombat = true;
        public float OutOfCombatRegenRate = 10f;  // Per second
        
        [Header("Scaling")]
        public int BaseMaximum = 100;
        public int MaximumPerLevel = 5;
        public StatType ScalingStat = StatType.Constitution;
        public int MaximumPerStatPoint = 2;
    }
    
    /// <summary>
    /// Concrete implementation of IResourcePool
    /// Can represent any resource type: Mana, Energy, Force, Stamina, etc.
    /// </summary>
    [Serializable]
    public class ResourcePool : IResourcePool
    {
        [SerializeField] private string _resourceName = "Energy";
        [SerializeField] private int _current;
        [SerializeField] private int _maximum;
        [SerializeField] private int _regenPerTurn;
        
        public string ResourceName => _resourceName;
        public int Current => _current;
        public int Maximum => _maximum;
        public float Percentage => _maximum > 0 ? (float)_current / _maximum : 0f;
        public int RegenPerTurn => _regenPerTurn;
        
        public event Action<int, int> OnResourceChanged;
        
        public ResourcePool(string name, int maximum, int regenPerTurn = 0)
        {
            _resourceName = name;
            _maximum = maximum;
            _current = maximum;
            _regenPerTurn = regenPerTurn;
        }
        
        public ResourcePool(ResourceConfig config, int level, int scalingStat)
        {
            _resourceName = config.DisplayName;
            _maximum = config.BaseMaximum 
                     + (config.MaximumPerLevel * level)
                     + (config.MaximumPerStatPoint * ((scalingStat - 10) / 2));
            _current = _maximum;
            _regenPerTurn = config.RegeneratesInCombat ? config.RegenPerTurn : 0;
        }
        
        public bool CanAfford(int cost)
        {
            return _current >= cost;
        }
        
        public bool TrySpend(int cost)
        {
            if (!CanAfford(cost))
                return false;
            
            int oldValue = _current;
            _current -= cost;
            OnResourceChanged?.Invoke(_current, _maximum);
            return true;
        }
        
        public void Restore(int amount)
        {
            int oldValue = _current;
            _current = Mathf.Min(_current + amount, _maximum);
            
            if (_current != oldValue)
                OnResourceChanged?.Invoke(_current, _maximum);
        }
        
        public void RestoreFull()
        {
            if (_current != _maximum)
            {
                _current = _maximum;
                OnResourceChanged?.Invoke(_current, _maximum);
            }
        }
        
        public void SetMaximum(int newMax, bool fillToNew = false)
        {
            int oldMax = _maximum;
            _maximum = Mathf.Max(1, newMax);
            
            if (fillToNew && newMax > oldMax)
            {
                _current += (newMax - oldMax);
            }
            
            _current = Mathf.Min(_current, _maximum);
            OnResourceChanged?.Invoke(_current, _maximum);
        }
        
        /// <summary>
        /// Called at the start of each turn for regeneration
        /// </summary>
        public void Tick()
        {
            if (_regenPerTurn > 0)
            {
                Restore(_regenPerTurn);
            }
        }
        
        /// <summary>
        /// Force set current value (for loading saves, etc.)
        /// </summary>
        public void SetCurrent(int value)
        {
            _current = Mathf.Clamp(value, 0, _maximum);
            OnResourceChanged?.Invoke(_current, _maximum);
        }
        
        public override string ToString()
        {
            return $"{_resourceName}: {_current}/{_maximum}";
        }
    }
    
    /// <summary>
    /// Component wrapper for ResourcePool to use on GameObjects
    /// </summary>
    public class ResourcePoolComponent : MonoBehaviour, IResourcePool
    {
        [SerializeField] private ResourceConfig _config;
        [SerializeField] private int _level = 1;
        
        private ResourcePool _pool;
        private CombatStats _stats;
        
        public string ResourceName => _pool?.ResourceName ?? "Energy";
        public int Current => _pool?.Current ?? 0;
        public int Maximum => _pool?.Maximum ?? 0;
        public float Percentage => _pool?.Percentage ?? 0f;
        
        public event Action<int, int> OnResourceChanged;
        
        private void Awake()
        {
            Initialize();
        }
        
        public void Initialize(CombatStats stats = null, int level = -1)
        {
            _stats = stats;
            if (level > 0) _level = level;
            
            if (_config != null)
            {
                int scalingStat = _stats?.GetModifier(_config.ScalingStat) ?? 0;
                _pool = new ResourcePool(_config, _level, scalingStat);
            }
            else
            {
                _pool = new ResourcePool("Energy", 100, 5);
            }
            
            _pool.OnResourceChanged += (cur, max) => OnResourceChanged?.Invoke(cur, max);
        }
        
        public bool CanAfford(int cost) => _pool?.CanAfford(cost) ?? false;
        public bool TrySpend(int cost) => _pool?.TrySpend(cost) ?? false;
        public void Restore(int amount) => _pool?.Restore(amount);
        public void RestoreFull() => _pool?.RestoreFull();
        public void SetMaximum(int newMax, bool fillToNew = false) => _pool?.SetMaximum(newMax, fillToNew);
        public void Tick() => _pool?.Tick();
    }
}
