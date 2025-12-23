// ============================================================================
// RPGPlatform.Systems.Combat - Combat Manager
// Main combat orchestration class that ties all systems together
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using RPGPlatform.Core;
using RPGPlatform.Data;

namespace RPGPlatform.Systems.Combat
{
    /// <summary>
    /// Configuration for combat encounters
    /// </summary>
    [CreateAssetMenu(fileName = "CombatConfig", menuName = "RPG/Combat/Combat Config")]
    public class CombatConfig : ScriptableObject
    {
        [Header("Positioning")]
        public bool UseGridPositioning = true;
        public int GridWidth = 10;
        public int GridHeight = 10;
        public float CellSize = 2f;
        public float FreeFormArenaRadius = 30f;
        
        [Header("Rules")]
        public int MaxRounds = 100;  // Prevent infinite combat
        public bool AllowFleeing = true;
        public int FleeDC = 15;
        
        [Header("Default Effects")]
        public StatusEffectTemplate DefendingEffect;
    }
    
    /// <summary>
    /// Main combat manager - orchestrates all combat systems
    /// </summary>
    public class CombatManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private CombatConfig _config;
        [SerializeField] private D20ResolverConfig _resolverConfig;
        
        // Systems
        private ICombatResolver _resolver;
        private IPositioningSystem _positioning;
        private CombatStateMachine _stateMachine;
        private IMoralityService _morality;
        private ITurnManager _turnManager;
        
        // State
        private List<ICombatant> _combatants = new List<ICombatant>();
        private Stack<ICombatCommand> _commandHistory = new Stack<ICombatCommand>();
        
        // Events
        public event Action OnCombatStarted;
        public event Action<bool> OnCombatEnded;  // true = victory
        public event Action<CombatPhase> OnPhaseChanged;
        public event Action<ICombatant> OnTurnStarted;
        public event Action<ICombatCommand, CommandResult> OnCommandExecuted;
        public event Action<ICombatant, DamageResult> OnDamageDealt;
        public event Action<ICombatant> OnCombatantDefeated;
        
        // Properties
        public CombatPhase CurrentPhase => _stateMachine?.CurrentPhase ?? CombatPhase.NotStarted;
        public ICombatant CurrentCombatant => _stateMachine?.Context?.CurrentCombatant;
        public int CurrentRound => _stateMachine?.Context?.CurrentRound ?? 0;
        public IReadOnlyList<ICombatant> Combatants => _combatants;
        public IReadOnlyList<ICombatant> TurnOrder => _stateMachine?.Context?.TurnOrder;
        public bool IsInCombat => _stateMachine != null && !_stateMachine.IsCombatOver;
        public ICombatResolver Resolver => _resolver;
        public IPositioningSystem Positioning => _positioning;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            InitializeSystems();
        }
        
        private void Update()
        {
            if (!IsInCombat) return;
            
            // State machine handles its own updates via Unity lifecycle
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeSystems()
        {
            // Initialize combat resolver
            _resolver = new D20CombatResolver(_resolverConfig);
            
            // Initialize positioning system
            if (_config != null && _config.UseGridPositioning)
            {
                _positioning = new GridPositioningSystem(
                    _config.GridWidth, 
                    _config.GridHeight, 
                    _config.CellSize
                );
            }
            else
            {
                _positioning = new FreeFormPositioningSystem(
                    _config?.FreeFormArenaRadius ?? 30f
                );
            }
            
            // Initialize turn manager
            _turnManager = gameObject.AddComponent<TurnManager>();
            _turnManager.Initialize(_resolver);
            
            // State machine will be created when combat starts
        }
        
        /// <summary>
        /// Inject a morality service (for ability requirements)
        /// </summary>
        public void SetMoralityService(IMoralityService morality)
        {
            _morality = morality;
        }
        
        /// <summary>
        /// Override the combat resolver
        /// </summary>
        public void SetResolver(ICombatResolver resolver)
        {
            _resolver = resolver;
        }
        
        /// <summary>
        /// Override the positioning system
        /// </summary>
        public void SetPositioningSystem(IPositioningSystem positioning)
        {
            _positioning = positioning;
        }
        
        #endregion
        
        #region Combat Flow
        
        /// <summary>
        /// Start a new combat encounter
        /// </summary>
        public void StartCombat(List<ICombatant> combatants)
        {
            if (IsInCombat)
            {
                Debug.LogWarning("[CombatManager] Combat already in progress!");
                return;
            }
            
            _combatants = new List<ICombatant>(combatants);
            _commandHistory.Clear();
            
            // Subscribe to combatant events
            foreach (var combatant in _combatants)
            {
                combatant.OnDamageReceived += damage => OnDamageDealt?.Invoke(combatant, damage);
                combatant.OnDefeated += () => HandleCombatantDefeated(combatant);
            }
            
            // Place combatants on positioning system
            PlaceCombatants();
            
            // Create and initialize state machine
            _stateMachine = gameObject.AddComponent<CombatStateMachine>();
            _stateMachine.OnPhaseChanged += phase => HandlePhaseChanged(phase);
            _stateMachine.Initialize(_turnManager);
            
            Debug.Log($"[CombatManager] Starting combat with {_combatants.Count} combatants");
            OnCombatStarted?.Invoke();
            
            // Start the combat
            _stateMachine.StartCombat(_combatants);
        }
        
        /// <summary>
        /// Submit a command for the current combatant
        /// </summary>
        public bool SubmitCommand(ICombatCommand command)
        {
            if (!IsInCombat || CurrentPhase != CombatPhase.Planning)
            {
                Debug.LogWarning("[CombatManager] Cannot submit command - not in planning phase");
                return false;
            }
            
            if (command.Source != CurrentCombatant)
            {
                Debug.LogWarning("[CombatManager] Command source doesn't match current combatant");
                return false;
            }
            
            _stateMachine.SubmitCommand(command);
            _commandHistory.Push(command);
            
            return true;
        }
        
        /// <summary>
        /// Create and submit an attack command
        /// </summary>
        public bool Attack(ICombatant target, Ability ability)
        {
            if (CurrentCombatant == null) return false;
            
            // Check morality requirements
            if (!AbilityMoralityValidator.MeetsMoralityRequirements(ability, _morality))
            {
                Debug.Log($"[CombatManager] {ability.DisplayName} requires specific alignment");
                return false;
            }
            
            var command = new AttackCommand(CurrentCombatant, target, (IAbility)ability, _resolver, _positioning);
            return SubmitCommand(command);
        }
        
        /// <summary>
        /// Defend (enter defensive stance)
        /// </summary>
        public bool Defend()
        {
            if (CurrentCombatant == null) return false;
            
            var command = new DefendCommand(CurrentCombatant, _config?.DefendingEffect);
            return SubmitCommand(command);
        }
        
        /// <summary>
        /// Move to a new position
        /// </summary>
        public bool Move(CombatPosition targetPosition)
        {
            if (CurrentCombatant == null) return false;
            
            var command = new MoveCommand(CurrentCombatant, targetPosition, _positioning);
            return SubmitCommand(command);
        }
        
        /// <summary>
        /// Use an item
        /// </summary>
        public bool UseItem(ICombatant target, ItemEffect item)
        {
            if (CurrentCombatant == null) return false;
            
            var command = new UseItemCommand(CurrentCombatant, target, item);
            return SubmitCommand(command);
        }
        
        /// <summary>
        /// Pass the turn
        /// </summary>
        public bool Pass()
        {
            if (CurrentCombatant == null) return false;
            
            var command = new PassCommand(CurrentCombatant);
            return SubmitCommand(command);
        }
        
        /// <summary>
        /// Attempt to flee
        /// </summary>
        public bool Flee()
        {
            if (CurrentCombatant == null) return false;
            if (_config != null && !_config.AllowFleeing) return false;
            
            var command = new FleeCommand(CurrentCombatant, _resolver, _config?.FleeDC ?? 15);
            return SubmitCommand(command);
        }
        
        /// <summary>
        /// Undo the last command (if supported)
        /// </summary>
        public void UndoLastCommand()
        {
            if (_commandHistory.Count > 0)
            {
                var command = _commandHistory.Pop();
                command.Undo();
                Debug.Log($"[CombatManager] Undid: {command.CommandName}");
            }
        }
        
        /// <summary>
        /// End combat immediately
        /// </summary>
        public void EndCombat(bool victory)
        {
            if (!IsInCombat) return;
            
            _stateMachine.TransitionTo(victory ? CombatPhase.Victory : CombatPhase.Defeat);
        }
        
        #endregion
        
        #region Queries
        
        /// <summary>
        /// Get all enemies of a combatant
        /// </summary>
        public List<ICombatant> GetEnemies(ICombatant combatant)
        {
            return _combatants.FindAll(c => c.IsAlive && c.Team != combatant.Team);
        }
        
        /// <summary>
        /// Get all allies of a combatant
        /// </summary>
        public List<ICombatant> GetAllies(ICombatant combatant)
        {
            return _combatants.FindAll(c => c.IsAlive && c.Team == combatant.Team && c != combatant);
        }
        
        /// <summary>
        /// Get valid targets for an ability
        /// </summary>
        public List<ICombatant> GetValidTargets(ICombatant user, IAbility ability)
        {
            var targets = new List<ICombatant>();
            
            foreach (var combatant in _combatants)
            {
                if (combatant.IsAlive && ability.IsValidTarget(user, combatant))
                {
                    if (_positioning.IsInRange(user, combatant, ability.Range))
                    {
                        targets.Add(combatant);
                    }
                }
            }
            
            return targets;
        }
        
        /// <summary>
        /// Get usable abilities for the current combatant
        /// </summary>
        public List<IAbility> GetUsableAbilities()
        {
            if (CurrentCombatant == null) return new List<IAbility>();
            
            var usable = new List<IAbility>();
            
            foreach (var ability in CurrentCombatant.Abilities)
            {
                if (ability.CanUse(CurrentCombatant))
                {
                    // Check morality requirements
                    if (AbilityMoralityValidator.MeetsMoralityRequirements(ability, _morality))
                    {
                        usable.Add(ability);
                    }
                }
            }
            
            return usable;
        }
        
        #endregion
        
        #region Private Methods
        
        private void PlaceCombatants()
        {
            if (_positioning is GridPositioningSystem grid)
            {
                // Place players on left side, enemies on right
                int playerRow = 0;
                int enemyRow = 0;
                
                foreach (var combatant in _combatants)
                {
                    if (combatant.Team == 0) // Player team
                    {
                        grid.PlaceCombatant(combatant, new Vector2Int(1, 2 + playerRow));
                        playerRow++;
                    }
                    else // Enemy team
                    {
                        grid.PlaceCombatant(combatant, new Vector2Int(grid.Width - 2, 2 + enemyRow));
                        enemyRow++;
                    }
                }
            }
            else
            {
                // Free-form: place in a circle
                float radius = 10f;
                int count = _combatants.Count;
                
                for (int i = 0; i < count; i++)
                {
                    float angle = (2 * Mathf.PI * i) / count;
                    var pos = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                    _combatants[i].Position = CombatPosition.FromWorld(pos);
                }
            }
        }
        
        private void HandlePhaseChanged(CombatPhase newPhase)
        {
            Debug.Log($"[CombatManager] Phase changed to {newPhase}");
            OnPhaseChanged?.Invoke(newPhase);
            
            if (newPhase == CombatPhase.Planning && CurrentCombatant != null)
            {
                OnTurnStarted?.Invoke(CurrentCombatant);
            }
            
            if (newPhase == CombatPhase.Victory)
            {
                OnCombatEnded?.Invoke(true);
                CleanupCombat();
            }
            else if (newPhase == CombatPhase.Defeat)
            {
                OnCombatEnded?.Invoke(false);
                CleanupCombat();
            }
        }
        
        private void HandleCombatantDefeated(ICombatant combatant)
        {
            OnCombatantDefeated?.Invoke(combatant);
            Debug.Log($"[CombatManager] {combatant.DisplayName} defeated!");
        }
        
        private void CleanupCombat()
        {
            // Unsubscribe from events
            foreach (var combatant in _combatants)
            {
                // Would need to store delegates to properly unsubscribe
            }
            
            Debug.Log("[CombatManager] Combat ended");
        }
        
        #endregion
        
        #region Debug
        
        [ContextMenu("Debug - Print State")]
        private void DebugPrintState()
        {
            Debug.Log($"Phase: {CurrentPhase}");
            Debug.Log($"Round: {CurrentRound}");
            Debug.Log($"Current: {CurrentCombatant?.DisplayName ?? "None"}");
            Debug.Log($"Combatants: {_combatants.Count}");
        }
        
        #endregion
    }
    
    /// <summary>
    /// Static service locator for combat manager (optional pattern)
    /// </summary>
    public static class CombatService
    {
        private static CombatManager _instance;
        
        public static CombatManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = UnityEngine.Object.FindFirstObjectByType<CombatManager>();
                }
                return _instance;
            }
        }
        
        public static void SetInstance(CombatManager manager)
        {
            _instance = manager;
        }
    }
}
