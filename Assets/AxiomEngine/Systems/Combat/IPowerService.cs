// ============================================================================
// RPGPlatform.Combat - Power Service Interface
// Abstraction for ability execution with decorator support
// ============================================================================

using RPGPlatform.Core;

namespace RPGPlatform.Combat
{
    /// <summary>
    /// Service interface for executing abilities
    /// Supports decorator pattern for adding validation layers (morality, resources, etc.)
    /// </summary>
    public interface IPowerService
    {
        /// <summary>
        /// Check if an ability can be executed by a combatant
        /// </summary>
        /// <param name="user">The combatant attempting to use the ability</param>
        /// <param name="ability">The ability to check</param>
        /// <returns>True if the ability can be executed</returns>
        bool CanExecute(ICombatant user, IAbility ability);
        
        /// <summary>
        /// Execute an ability against a target
        /// </summary>
        /// <param name="user">The combatant using the ability</param>
        /// <param name="target">The target of the ability</param>
        /// <param name="ability">The ability to execute</param>
        /// <returns>Result of the ability execution</returns>
        CommandResult Execute(ICombatant user, ICombatant target, IAbility ability);
    }
}
