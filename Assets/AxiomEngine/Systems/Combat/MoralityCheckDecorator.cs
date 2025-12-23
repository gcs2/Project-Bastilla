// ============================================================================
// RPGPlatform.Combat - Morality Check Decorator
// Decorator pattern implementation for morality-based ability gating
// ============================================================================

using UnityEngine;
using RPGPlatform.Core;

namespace RPGPlatform.Combat
{
    /// <summary>
    /// Decorator that adds morality requirement checking to ability execution
    /// Implements the Gang of Four Decorator Pattern
    /// </summary>
    public class MoralityCheckDecorator : IPowerService
    {
        private readonly IPowerService _wrappedService;
        private readonly IMoralityService _moralityService;
        
        public MoralityCheckDecorator(IPowerService wrappedService, IMoralityService moralityService)
        {
            _wrappedService = wrappedService;
            _moralityService = moralityService;
        }
        
        /// <summary>
        /// Check if ability can be executed, including morality requirements
        /// </summary>
        public bool CanExecute(ICombatant user, IAbility ability)
        {
            // First check base requirements
            if (!_wrappedService.CanExecute(user, ability))
            {
                return false;
            }
            
            // Then check morality requirements
            if (!MeetsMoralityRequirements(ability))
            {
                Debug.Log($"[MoralityCheckDecorator] {ability.DisplayName} requires specific alignment");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Execute ability if morality requirements are met
        /// </summary>
        public CommandResult Execute(ICombatant user, ICombatant target, IAbility ability)
        {
            // Check morality requirements before execution
            if (!MeetsMoralityRequirements(ability))
            {
                string requirement = GetMoralityRequirementText(ability);
                return CommandResult.Failure($"{ability.DisplayName} requires {requirement}");
            }
            
            // Delegate to wrapped service
            return _wrappedService.Execute(user, target, ability);
        }
        
        /// <summary>
        /// Check if morality requirements are met
        /// </summary>
        private bool MeetsMoralityRequirements(IAbility ability)
        {
            // No morality system = all abilities available
            if (_moralityService == null || !_moralityService.HasMorality)
                return true;
            
            // No axis requirement
            if (string.IsNullOrEmpty(ability.RequiredMoralityAxis))
                return true;
            
            return _moralityService.MeetsRequirement(
                ability.RequiredMoralityAxis,
                ability.MinMoralityValue,
                ability.MaxMoralityValue
            );
        }
        
        /// <summary>
        /// Get human-readable description of morality requirement
        /// </summary>
        private string GetMoralityRequirementText(IAbility ability)
        {
            if (string.IsNullOrEmpty(ability.RequiredMoralityAxis))
                return "";
            
            string axis = ability.RequiredMoralityAxis;
            
            if (ability.MinMoralityValue.HasValue && ability.MaxMoralityValue.HasValue)
            {
                return $"{axis} between {ability.MinMoralityValue} and {ability.MaxMoralityValue}";
            }
            else if (ability.MinMoralityValue.HasValue)
            {
                return $"{axis} ≥ {ability.MinMoralityValue}";
            }
            else if (ability.MaxMoralityValue.HasValue)
            {
                return $"{axis} ≤ {ability.MaxMoralityValue}";
            }
            
            return "";
        }
    }
}
