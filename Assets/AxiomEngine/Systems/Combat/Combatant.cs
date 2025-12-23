// ============================================================================
// RPGPlatform.Systems.Combat - Combatant Implementation
// Unit/Character base class with modular components
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using RPGPlatform.Core;
using RPGPlatform.Data;

namespace RPGPlatform.Systems.Combat
{
    /// <summary>
    /// Base combatant implementation using composition
    /// Can represent players, enemies, NPCs, or destructible objects
    /// </summary>
    public class Combatant : MonoBehaviour, ICombatant
    {
        [Header("Identity")]
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private bool _isPlayerControlled;
        [SerializeField] private int _team;
        
        [Header("Stats")]
        [SerializeField] private CombatStats _baseStats = new CombatStats();
        
        [Header("Abilities")]
        [SerializeField] private List<AbilityData> _abilityData = new List<AbilityData>();
        
        [Header("Resources")]
        [SerializeField] private ResourceConfig _resourceConfig;
        
        // Runtime state
        private CombatStats _currentStats;
        private List<IAbility> _abilities = new List<IAbility>();
        private StatusEffectManager _effectManager;
        private ResourcePool _resources;
        private CombatPosition _position;
        
        #region ICombatant Implementation
        
        public string Id => _id;
        public string DisplayName => _displayName;
        public CombatStats Stats => GetEffectiveStats();
        public CombatPosition Position 
        { 
            get => _position; 
            set => _position = value; 
        }
        public IReadOnlyList<IAbility> Abilities => _abilities;
        public IReadOnlyList<IStatusEffect> ActiveEffects => 
            (IReadOnlyList<IStatusEffect>)_effectManager?.Effects ?? new List<IStatusEffect>();
        public IResourcePool Resources => _resources;
        
        public bool IsAlive => _currentStats.CurrentHealth > 0;
        public bool IsPlayerControlled => _isPlayerControlled;
        public bool CanMove => IsAlive && (_effectManager == null || !_effectManager.IsRooted());
        public bool CanAct => IsAlive && (_effectManager == null || !_effectManager.IsStunned());
        public int Team => _team;
        
        public event Action<DamageResult> OnDamageReceived;
        public event Action<int> OnHealingReceived;
        public event Action<IStatusEffect> OnEffectAdded;
        public event Action<IStatusEffect> OnEffectRemoved;
        public event Action OnDefeated;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            Initialize();
        }
        
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_id))
            {
                _id = System.Guid.NewGuid().ToString();
            }
        }
        
        #endregion
        
        #region Initialization
        
        public void Initialize()
        {
            // Clone base stats to working stats
            _currentStats = _baseStats.Clone();
            
            // Initialize status effect manager
            _effectManager = new StatusEffectManager(this);
            _effectManager.OnEffectAdded += effect => OnEffectAdded?.Invoke(effect);
            _effectManager.OnEffectRemoved += effect => OnEffectRemoved?.Invoke(effect);
            
            // Initialize resource pool
            if (_resourceConfig != null)
            {
                int scalingStat = _currentStats.GetModifier(_resourceConfig.ScalingStat);
                _resources = new ResourcePool(_resourceConfig, 1, scalingStat);
            }
            else
            {
                _resources = new ResourcePool("Energy", 100, 5);
            }
            
            // Initialize abilities from data
            InitializeAbilities();
            
            // Set initial position
            _position = CombatPosition.FromWorld(transform.position);
        }
        
        public void Initialize(CombatantData data, bool isPlayer = false, int team = 1)
        {
            _id = data.Id;
            _displayName = data.BaseName;
            _baseStats = data.BaseStats.Clone();
            _abilityData = data.Abilities;
            _isPlayerControlled = isPlayer;
            _team = team;
            Initialize();
        }

        public void Initialize(CombatStats stats, List<AbilityData> abilities, bool isPlayer, int team)
        {
            _baseStats = stats;
            _abilityData = abilities;
            _isPlayerControlled = isPlayer;
            _team = team;
            Initialize();
        }
        
        private void InitializeAbilities()
        {
            _abilities.Clear();
            foreach (var data in _abilityData)
            {
                if (data != null)
                {
                    _abilities.Add(new Ability(data));
                }
            }
        }
        
        #endregion
        
        #region Combat Actions
        
        public void ApplyDamage(DamageResult damage)
        {
            if (!IsAlive) return;
            
            int previousHealth = _currentStats.CurrentHealth;
            _currentStats.CurrentHealth = Mathf.Max(0, _currentStats.CurrentHealth - damage.TotalDamage);
            
            Debug.Log($"[Combatant] {DisplayName} takes {damage.TotalDamage} {damage.Type} damage. " +
                     $"Health: {previousHealth} → {_currentStats.CurrentHealth}");
            
            OnDamageReceived?.Invoke(damage);
            
            if (!IsAlive)
            {
                HandleDefeat();
            }
        }
        
        public void TakeDamage(int amount, DamageType type)
        {
            ApplyDamage(new DamageResult { BaseDamage = amount, Type = type });
        }

        public int Heal(int amount)
        {
            if (!IsAlive) return 0;
            
            int previousHealth = _currentStats.CurrentHealth;
            _currentStats.CurrentHealth = Mathf.Min(_currentStats.MaxHealth, _currentStats.CurrentHealth + amount);
            int actualHealing = _currentStats.CurrentHealth - previousHealth;
            
            if (actualHealing > 0)
            {
                OnHealingReceived?.Invoke(actualHealing);
            }
            return actualHealing;
        }

        public void ApplyHealing(int amount)
        {
            Heal(amount);
        }
        
        public void AddStatusEffect(IStatusEffect effect)
        {
            if (effect is StatusEffect se)
            {
                _effectManager.AddEffect(se);
            }
        }
        
        /// <summary>
        /// Apply a status effect from a template
        /// </summary>
        public IStatusEffect ApplyStatusEffect(StatusEffectTemplate template, ICombatant source)
        {
            return _effectManager.ApplyEffect(template, source);
        }
        
        public void RemoveStatusEffect(string effectId)
        {
            _effectManager.RemoveEffect(effectId);
        }
        
        public void TickEffects()
        {
            _effectManager.TickAll();
            _resources?.Tick();
            
            // Tick ability cooldowns
            foreach (var ability in _abilities)
            {
                ability.TickCooldown();
            }
        }
        
        #endregion
        
        #region Stat Calculation
        
        /// <summary>
        /// Get effective stats (base + equipment + buffs/debuffs)
        /// </summary>
        private CombatStats GetEffectiveStats()
        {
            var effective = _currentStats.Clone();
            
            // Apply status effect modifiers
            if (_effectManager != null)
            {
                var mods = _effectManager.GetTotalModifiers();
                effective.Strength += mods.Strength;
                effective.Dexterity += mods.Dexterity;
                effective.Constitution += mods.Constitution;
                effective.Intelligence += mods.Intelligence;
                effective.Wisdom += mods.Wisdom;
                effective.Charisma += mods.Charisma;
                effective.ArmorClass += mods.ArmorClass;
            }
            
            return effective;
        }
        
        #endregion
        
        #region State Queries
        
        // Deprecated methods replaced by properties
        public bool IsStunned() => _effectManager?.IsStunned() ?? false;
        public bool IsRooted() => _effectManager?.IsRooted() ?? false;
        
        public bool HasEffect(string effectId)
        {
            return _effectManager.HasEffect(effectId);
        }
        
        #endregion
        
        #region Private Methods
        
        private void HandleDefeat()
        {
            Debug.Log($"[Combatant] {DisplayName} has been defeated!");
            _effectManager.ClearAll();
            OnDefeated?.Invoke();
        }
        
        #endregion
        
        #region Editor/Debug
        
        [ContextMenu("Debug - Take 10 Damage")]
        private void DebugTakeDamage()
        {
            ApplyDamage(new DamageResult { BaseDamage = 10, Type = DamageType.Physical });
        }
        
        [ContextMenu("Debug - Heal 10")]
        private void DebugHeal()
        {
            ApplyHealing(10);
        }
        
        [ContextMenu("Debug - Print Stats")]
        private void DebugPrintStats()
        {
            var stats = GetEffectiveStats();
            Debug.Log($"[{DisplayName}] HP: {stats.CurrentHealth}/{stats.MaxHealth}, " +
                     $"AC: {stats.ArmorClass}, STR: {stats.Strength}, DEX: {stats.Dexterity}");
        }
        
        #endregion
    }
    
    /// <summary>
    /// Factory for creating combatants from data
    /// </summary>
    public static class CombatantFactory
    {
        public static Combatant CreateFromData(
            CombatantData data,
            Vector3 position,
            bool isPlayer,
            int team)
        {
            var prefab = data.Prefab != null ? data.Prefab : new GameObject(data.BaseName);
            var go = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);
            var combatant = go.GetComponent<Combatant>() ?? go.AddComponent<Combatant>();
            
            combatant.Initialize(data, isPlayer, team);
            
            return combatant;
        }

        public static Combatant CreateFromData(
            GameObject prefab,
            CombatStats stats,
            List<AbilityData> abilities,
            Vector3 position,
            bool isPlayer,
            int team,
            string displayName)
        {
            var go = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);
            var combatant = go.GetComponent<Combatant>() ?? go.AddComponent<Combatant>();
            
            combatant.Initialize(stats, abilities, isPlayer, team);
            
            return combatant;
        }
        
        public static Combatant CreateBasic(
            string name,
            CombatStats stats,
            Vector3 position,
            bool isPlayer,
            int team)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            
            var combatant = go.AddComponent<Combatant>();
            combatant.Initialize(stats, new List<AbilityData>(), isPlayer, team);
            
            return combatant;
        }
    }
}
