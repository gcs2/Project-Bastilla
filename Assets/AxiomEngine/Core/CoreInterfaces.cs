// ============================================================================
// Axiom RPG Engine - Core Interfaces
// Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGPlatform.Core
{
    #region Enumerations
    
    public enum DamageType
    {
        Physical,
        Energy,
        Psychic,
        Fire,
        Ice,
        Lightning,
        Void,
        Holy,
        Unholy
    }
    
    public enum SaveType
    {
        Fortitude,  // Physical resilience
        Reflex,     // Dodge/evasion
        Will        // Mental resistance
    }
    
    public enum AbilityType
    {
        Attack,
        Heal,
        Buff,
        Debuff,
        Utility,
        Passive
    }
    
    public enum TargetType
    {
        Self,
        SingleEnemy,
        SingleAlly,
        AllEnemies,
        AllAllies,
        Area,
        Line,
        Cone
    }
    
    public enum CombatPhase
    {
        NotStarted,
        RoundStart,
        Planning,       // Player/AI selects actions
        Execution,      // Actions are performed
        Resolution,     // Effects resolve, deaths processed
        RoundEnd,
        Victory,
        Defeat
    }
    
    public enum StatType
    {
        Strength,
        Dexterity,
        Constitution,
        Intelligence,
        Wisdom,
        Charisma
    }
    
    #endregion

    #region Data Structures
    
    /// <summary>
    /// Result of a d20 roll with all modifiers tracked
    /// </summary>
    [Serializable]
    public class RollResult
    {
        public int NaturalRoll;      // The raw d20 result (1-20)
        public int Modifier;          // Total modifiers applied
        public int Total;             // NaturalRoll + Modifier
        public int TargetNumber;      // DC or AC to beat
        public bool IsCriticalHit;    // Natural 20 (or within crit range)
        public bool IsCriticalMiss;   // Natural 1
        public bool IsSuccess;        // Total >= TargetNumber (with crit rules)
        
        public override string ToString()
        {
            string critText = IsCriticalHit ? " (CRITICAL!)" : IsCriticalMiss ? " (FUMBLE!)" : "";
            return $"Roll: {NaturalRoll} + {Modifier} = {Total} vs {TargetNumber} → {(IsSuccess ? "HIT" : "MISS")}{critText}";
        }
    }
    
    /// <summary>
    /// Detailed breakdown of damage dealt
    /// </summary>
    [Serializable]
    public class DamageResult
    {
        public int BaseDamage;
        public int BonusDamage;
        public float DamageMultiplier = 1f;
        public int Absorbed;          // Blocked by shields/armor
        public int Resisted;          // Reduced by resistances
        public DamageType Type;
        public bool WasCritical;
        
        public int TotalDamage => Mathf.Max(0, 
            Mathf.RoundToInt((BaseDamage + BonusDamage) * DamageMultiplier) - Absorbed - Resisted);

        public int FinalDamage => TotalDamage; // Compatibility alias
    }
    
    /// <summary>
    /// Result of executing a combat command
    /// </summary>
    public class CommandResult
    {
        public bool Success;
        public RollResult Roll;
        public DamageResult Damage;
        public List<string> Messages = new List<string>();
        public List<IStatusEffect> AppliedEffects = new List<IStatusEffect>();
        public string Message { get; set; } // Compatibility support
        public Dictionary<string, string> Metadata = new Dictionary<string, string>();
        
        public static CommandResult Hit(RollResult roll, DamageResult damage)
        {
            return new CommandResult { Success = true, Roll = roll, Damage = damage };
        }

        public static CommandResult Hit(ICombatant source, ICombatant target, IAbility ability, RollResult roll, DamageResult damage)
        {
            return new CommandResult { Success = true, Roll = roll, Damage = damage, Message = $"{source.DisplayName} hits {target.DisplayName} with {ability.DisplayName}" };
        }
        
        public static CommandResult Miss(RollResult roll)
        {
            return new CommandResult { Success = false, Roll = roll };
        }

        public static CommandResult Miss(ICombatant source, ICombatant target, IAbility ability, RollResult roll)
        {
            return new CommandResult { Success = false, Roll = roll, Message = $"{source.DisplayName} misses {target.DisplayName} with {ability.DisplayName}" };
        }
        
        public static CommandResult Failure(string reason)
        {
            return new CommandResult { Success = false, Messages = { reason } };
        }
    }
    
    /// <summary>
    /// Combat statistics for any combatant
    /// </summary>
    [Serializable]
    public class CombatStats
    {
        [Header("Core Stats")]
        public int Strength = 10;
        public int Dexterity = 10;
        public int Constitution = 10;
        public int Intelligence = 10;
        public int Wisdom = 10;
        public int Charisma = 10;
        
        [Header("Derived Stats")]
        public int MaxHealth = 100;
        public int CurrentHealth = 100;
        public int ArmorClass = 10;
        public int ProficiencyBonus = 2;
        public int InitiativeBonus = 0;
        
        [Header("Saving Throws")]
        public int FortitudeSave = 0;
        public int ReflexSave = 0;
        public int WillSave = 0;
        
        /// <summary>
        /// Calculate the modifier for a given stat (D&D style: (stat - 10) / 2)
        /// </summary>
        public int GetModifier(StatType stat)
        {
            int value = stat switch
            {
                StatType.Strength => Strength,
                StatType.Dexterity => Dexterity,
                StatType.Constitution => Constitution,
                StatType.Intelligence => Intelligence,
                StatType.Wisdom => Wisdom,
                StatType.Charisma => Charisma,
                _ => 10
            };
            return (value - 10) / 2;
        }
        
        public int GetSaveBonus(SaveType save)
        {
            return save switch
            {
                SaveType.Fortitude => FortitudeSave + GetModifier(StatType.Constitution),
                SaveType.Reflex => ReflexSave + GetModifier(StatType.Dexterity),
                SaveType.Will => WillSave + GetModifier(StatType.Wisdom),
                _ => 0
            };
        }
        
        public CombatStats Clone()
        {
            return (CombatStats)MemberwiseClone();
        }
    }
    
    /// <summary>
    /// Position in combat (supports both grid and free-form)
    /// </summary>
    [Serializable]
    public struct CombatPosition
    {
        public Vector3 WorldPosition;   // For free-form
        public Vector2Int GridPosition; // For grid-based
        
        public static CombatPosition FromWorld(Vector3 pos) => 
            new CombatPosition { WorldPosition = pos };
        
        public static CombatPosition FromGrid(int x, int y) => 
            new CombatPosition { GridPosition = new Vector2Int(x, y) };
    }
    
    #endregion

    #region Core Interfaces
    
    /// <summary>
    /// Any entity that can participate in combat
    /// </summary>
    public interface ICombatant
    {
        string Id { get; }
        string DisplayName { get; }
        string Name => DisplayName; // Alias for compatibility
        CombatStats Stats { get; }
        CombatPosition Position { get; set; }
        IReadOnlyList<IAbility> Abilities { get; }
        IReadOnlyList<IStatusEffect> ActiveEffects { get; }
        IResourcePool Resources { get; }
        
        bool IsAlive { get; }
        bool IsPlayerControlled { get; }
        bool CanMove { get; }
        bool CanAct { get; }
        int Team { get; }  // 0 = player party, 1+ = enemy factions
        
        void ApplyDamage(DamageResult damage);
        void TakeDamage(int amount, DamageType type); // Compatibility wrapper
        int Heal(int amount); // Returns actual healed amount
        void ApplyHealing(int amount);
        void AddStatusEffect(IStatusEffect effect);
        void RemoveStatusEffect(string effectId);
        IStatusEffect ApplyStatusEffect(StatusEffectTemplate template, ICombatant source);
        void TickEffects();  // Called each round
        
        
        // Events
        event Action<DamageResult> OnDamageReceived;
        event Action<int> OnHealingReceived;
        event Action<IStatusEffect> OnEffectAdded;
        event Action<IStatusEffect> OnEffectRemoved;
        event Action OnDefeated;
    }
    
    /// <summary>
    /// Generic resource pool (Energy/Mana/Force/Stamina)
    /// </summary>
    public interface IResourcePool
    {
        string ResourceName { get; }           // "Force", "Mana", "Stamina"
        int Current { get; }
        int Maximum { get; }
        float Percentage { get; }
        
        bool CanAfford(int cost);
        bool TrySpend(int cost);
        void Restore(int amount);
        void RestoreFull();
        void SetMaximum(int newMax, bool fillToNew = false);
        
        event Action<int, int> OnResourceChanged;  // (current, max)
    }
    
    /// <summary>
    /// Generic resource pool (Energy/Mana/Force/Stamina)
    /// </summary>
    public interface ITurnManager
    {
        int CurrentRound { get; }
        ICombatant CurrentCombatant { get; }
        List<ICombatant> TurnOrder { get; }

        event Action<int> OnRoundStart;
        event Action<ICombatant> OnTurnStart;
        event Action<ICombatant> OnTurnEnd;
        event Action<int> OnRoundEnd;

        void Initialize(ICombatResolver resolver);
        void StartCombat(List<ICombatant> combatants);
        void NextTurn();
        void Tick();
    }
    
    /// <summary>
    /// An ability/skill/spell that can be used in combat
    /// </summary>
    public interface IAbility
    {
        string AbilityId { get; }
        string DisplayName { get; }
        string Description { get; }
        Sprite Icon { get; }
        
        AbilityType Type { get; }
        TargetType Targeting { get; }
        DamageType DamageType { get; }
        StatType PrimaryStat { get; }
        
        int ResourceCost { get; }
        int CooldownTurns { get; }
        int CurrentCooldown { get; }
        int Range { get; }
        int AreaOfEffect { get; }
        
        // Requirements
        int RequiredLevel { get; }
        float? MinMoralityValue { get; }      // Null = no requirement
        float? MaxMoralityValue { get; }
        string RequiredMoralityAxis { get; }
        
        bool CanUse(ICombatant user);
        bool IsValidTarget(ICombatant user, ICombatant target);
        void Use(ICombatant user);  // Deduct resources, start cooldown
        void OnUse(); // Usage tracking for specific impls
        void TickCooldown();
        
        // Damage formula (e.g., "2d6 + STR")
        string DamageFormula { get; }
        float CritMultiplier { get; }
        List<StatusEffectData> AppliedEffects { get; }
    }
    
    /// <summary>
    /// A status effect (buff, debuff, DoT, HoT, etc.)
    /// </summary>
    public interface IStatusEffect
    {
        string EffectId { get; }
        string DisplayName { get; }
        Sprite Icon { get; }
        
        int Duration { get; }           // Turns remaining (-1 = permanent until dispelled)
        int StackCount { get; }
        int MaxStacks { get; }
        bool IsDebuff { get; }
        bool IsDispellable { get; }
        
        ICombatant Source { get; }
        ICombatant Target { get; }
        
        void OnApply();
        void OnTick();      // Called each turn
        void OnRemove();
        void AddStack();
        
        // Stat modifications
        CombatStats GetStatModifiers();
    }
    
    /// <summary>
    /// Handles combat math - dice rolls, damage calculation, etc.
    /// Strategy pattern: swap D20 for percentile, card-based, etc.
    /// </summary>
    public interface ICombatResolver
    {
        RollResult ResolveAttack(ICombatant attacker, ICombatant target, IAbility ability);
        DamageResult CalculateDamage(ICombatant attacker, ICombatant target, IAbility ability, RollResult roll);
        bool ResolveSavingThrow(ICombatant target, SaveType saveType, int dc);
        RollResult ResolveCheck(ICombatant source, StatType stat, int dc); // Core resolution check
        int RollInitiative(ICombatant combatant);
        int RollDice(int count, int sides);
        int RollDice(string formula);  // "2d6+3"
    }
    
    /// <summary>
    /// Handles positioning logic - grid or free-form
    /// </summary>
    public interface IPositioningSystem
    {
        bool IsGridBased { get; }
        float GetDistance(ICombatant a, ICombatant b);
        bool IsInRange(ICombatant attacker, ICombatant target, int range);
        bool IsValidPosition(CombatPosition position);
        List<ICombatant> GetCombatantsInArea(CombatPosition center, int radius, List<ICombatant> allCombatants);
        List<CombatPosition> GetValidMovePositions(ICombatant combatant, int movementRange);
        bool TryMove(ICombatant combatant, CombatPosition newPosition);
        void MoveCombatant(ICombatant combatant, CombatPosition position); // Simple move
    }
    
    /// <summary>
    /// Morality system interface (can be null service if disabled)
    /// </summary>
    public interface IMoralityService
    {
        bool HasMorality { get; }
        float GetAxisValue(string axisId);
        void ModifyAxis(string axisId, float delta);
        bool MeetsRequirement(string axisId, float? minValue, float? maxValue);
    }
    
    #endregion

    #region Command Pattern
    
    /// <summary>
    /// Base interface for all combat commands (Command Pattern)
    /// </summary>
    public interface ICombatCommand
    {
        string CommandName { get; }
        ICombatant Source { get; }
        int Priority { get; }  // For simultaneous action resolution
        
        bool CanExecute();
        CommandResult Execute();
        void Undo();
    }
    
    #endregion
    
    #region State Machine
    
    /// <summary>
    /// Interface for combat phase states
    /// </summary>
    public interface ICombatPhaseState
    {
        CombatPhase Phase { get; }
        void Enter(CombatContext context);
        void Update(CombatContext context);
        void Exit(CombatContext context);
        CombatPhase? GetNextPhase(CombatContext context);
    }
    
    /// <summary>
    /// Shared context passed between combat states
    /// </summary>
    public class CombatContext
    {
        public List<ICombatant> AllCombatants = new List<ICombatant>();
        public List<ICombatant> TurnOrder = new List<ICombatant>();
        public int CurrentTurnIndex;
        public int CurrentRound;
        public Queue<ICombatCommand> PendingCommands = new Queue<ICombatCommand>();
        public List<ICombatCommand> ExecutedCommands = new List<ICombatCommand>();
        public bool PlayerInputReceived;
        public ICombatCommand SelectedCommand;
        
        public ICombatant CurrentCombatant => 
            CurrentTurnIndex >= 0 && CurrentTurnIndex < TurnOrder.Count 
                ? TurnOrder[CurrentTurnIndex] 
                : null;
        
        public bool AllEnemiesDefeated => 
            AllCombatants.FindAll(c => c.Team != 0).TrueForAll(c => !c.IsAlive);
        
        public bool AllPlayersDefeated => 
            AllCombatants.FindAll(c => c.Team == 0).TrueForAll(c => !c.IsAlive);
    }
    
    #endregion

    #region ScriptableObject Data (for reference)
    
    /// <summary>
    /// Data container for status effect templates
    /// </summary>
    [Serializable]
    public class StatusEffectData
    {
        public string EffectId;
        public string DisplayName;
        public int Duration;
        public int MaxStacks;
        public bool IsDebuff;
        public bool IsDispellable;
        
        // Stat modifiers
        public int StrengthMod;
        public int DexterityMod;
        public int ConstitutionMod;
        public int ArmorClassMod;
        
        // Periodic effects
        public int DamagePerTurn;
        public int HealingPerTurn;
        public DamageType DamageType;
    }
    
    #endregion
    /// <summary>
    /// Base class for item-based effects
    /// </summary>
    [Serializable]
    public abstract class ItemEffect : ScriptableObject
    {
        public string ItemId;
        public string ItemName;
        public abstract void Apply(ICombatant target);
    }
}
