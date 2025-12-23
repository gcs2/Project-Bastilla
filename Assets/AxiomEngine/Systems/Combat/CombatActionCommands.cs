// ============================================================================
// RPGPlatform.Systems.Combat - Combat Action Commands
// Command Pattern Implementations for Standard Actions
// ============================================================================

using UnityEngine;
using RPGPlatform.Core;

namespace RPGPlatform.Systems.Combat
{
    public class EndTurnCommand : ICombatCommand
    {
        public string CommandName => "End Turn";
        public ICombatant Source { get; }
        public int Priority => 0;

        private ITurnManager _turnManager;

        public EndTurnCommand(ICombatant source, ITurnManager turnManager)
        {
            Source = source;
            _turnManager = turnManager;
        }

        public bool CanExecute() => true;

        public CommandResult Execute()
        {
            _turnManager.NextTurn();
            return new CommandResult { Success = true, Message = "Turn Ended" };
        }

        public void Undo() { /* Cannot undo time usually */ }
    }

    public class AttackCommand : ICombatCommand
    {
        public string CommandName => "Attack";
        public ICombatant Source { get; }
        public ICombatant Target { get; }
        public int Priority => 10;

        private ICombatResolver _resolver;

        // Undo state
        private int _damageDealt;

        private IAbility _ability;
        private IPositioningSystem _positioning;

        public AttackCommand(ICombatant source, ICombatant target, IAbility ability, ICombatResolver resolver, IPositioningSystem positioning = null)
        {
            Source = source;
            Target = target;
            _ability = ability;
            _resolver = resolver;
            _positioning = positioning;
        }

        public bool CanExecute()
        {
            return Source.IsAlive && Target.IsAlive;
        }

        public CommandResult Execute()
        {
            if (_resolver == null) return CommandResult.Failure("No Resolver");

            // 1. Roll to Hit
            var roll = _resolver.ResolveAttack(Source, Target, _ability);

            if (roll.IsSuccess)
            {
                // 2. Calc Damage
                var dmgResult = _resolver.CalculateDamage(Source, Target, _ability, roll);
                
                // 3. Apply
                Target.TakeDamage(dmgResult.TotalDamage, _ability?.DamageType ?? DamageType.Physical);
                _damageDealt = dmgResult.TotalDamage;
                
                return CommandResult.Hit(Source, Target, _ability, roll, dmgResult);
            }
            else
            {
                return CommandResult.Miss(Source, Target, _ability, roll);
            }
        }

        public void Undo()
        {
            if (_damageDealt > 0 && Target != null)
            {
                Target.ApplyHealing(_damageDealt);
            }
        }
    }

    public class UseAbilityCommand : ICombatCommand
    {
        public string CommandName => _ability.DisplayName;
        public ICombatant Source { get; }
        public ICombatant Target { get; }
        public int Priority => 20;

        private IAbility _ability;
        private ICombatResolver _resolver;
        
        // Undo State? Complex for abilities (effects etc). 
        // For prototype, we might skip deep undo logic for abilities.

        public UseAbilityCommand(ICombatant source, ICombatant target, IAbility ability, ICombatResolver resolver)
        {
            Source = source;
            Target = target;
            _ability = ability;
            _resolver = resolver;
        }

        public bool CanExecute()
        {
            return Source.IsAlive && _ability.CanUse(Source) && _ability.IsValidTarget(Source, Target);
        }

        public CommandResult Execute()
        {
             if (!_ability.CanUse(Source)) return new CommandResult { Success = false, Message = "Cannot use ability" };

             // 1. Pay Costs
             _ability.Use(Source);

             // 2. Resolve Effect
             // If it's an attack ability:
             if (_ability is IAbility attackAbility) // AbilityType check?
             {
                 // Mock logic: If it has damage, resolve attack
                if (!string.IsNullOrEmpty(_ability.DamageFormula))
                {
                    var roll = _resolver.ResolveAttack(Source, Target, _ability);
                    if (roll.IsSuccess)
                    {
                        var dmg = _resolver.CalculateDamage(Source, Target, _ability, roll);
                        Target.TakeDamage(dmg.TotalDamage, _ability.DamageType);
                        return CommandResult.Hit(Source, Target, _ability, roll, dmg);
                    }
                    else
                    {
                        return CommandResult.Miss(Source, Target, _ability, roll);
                    }
                }
                else
                {
                    // Utility / Buff
                    return new CommandResult { Success = true, Message = $"Used {_ability.DisplayName}" };
                }
             }
             
             return new CommandResult { Success = true };
        }

        public void Undo()
        {
            // Not implemented for complex abilities yet
        }
    }

    public class DefendCommand : ICombatCommand
    {
        public string CommandName => "Defend";
        public ICombatant Source { get; }
        public int Priority => 5;

        private RPGPlatform.Core.StatusEffectTemplate _defendEffect;

        public DefendCommand(ICombatant source, RPGPlatform.Core.StatusEffectTemplate defendEffect)
        {
            Source = source;
            _defendEffect = defendEffect;
        }

        public bool CanExecute() => Source.IsAlive;

        public CommandResult Execute()
        {
            if (_defendEffect != null)
            {
                Source.ApplyStatusEffect(_defendEffect, Source);
            }
            return new CommandResult { Success = true, Message = $"{Source.DisplayName} takes a defensive stance" };
        }

        public void Undo()
        {
            if (_defendEffect != null)
            {
                Source.RemoveStatusEffect(_defendEffect.EffectId);
            }
        }
    }

    public class MoveCommand : ICombatCommand
    {
        public string CommandName => "Move";
        public ICombatant Source { get; }
        public int Priority => 30;

        private CombatPosition _targetPosition;
        private CombatPosition _oldPosition;
        private bool _hasOldPosition;
        private IPositioningSystem _positioning;

        public MoveCommand(ICombatant source, CombatPosition target, IPositioningSystem positioning)
        {
            Source = source;
            _targetPosition = target;
            _positioning = positioning;
        }

        public bool CanExecute() => Source.IsAlive && Source.CanMove;

        public CommandResult Execute()
        {
            _oldPosition = Source.Position;
            _hasOldPosition = true;
            _positioning.MoveCombatant(Source, _targetPosition);
            return new CommandResult { Success = true, Message = $"{Source.DisplayName} moved to {_targetPosition}" };
        }

        public void Undo()
        {
            if (_hasOldPosition)
            {
                _positioning.MoveCombatant(Source, _oldPosition);
            }
        }
    }

    public class PassCommand : ICombatCommand
    {
        public string CommandName => "Pass";
        public ICombatant Source { get; }
        public int Priority => 0;

        public PassCommand(ICombatant source)
        {
            Source = source;
        }

        public bool CanExecute() => true;

        public CommandResult Execute() => new CommandResult { Success = true, Message = $"{Source.DisplayName} passes" };

        public void Undo() { }
    }

    public class FleeCommand : ICombatCommand
    {
        public string CommandName => "Flee";
        public ICombatant Source { get; }
        public int Priority => 100;

        private ICombatResolver _resolver;
        private int _dc;

        public FleeCommand(ICombatant source, ICombatResolver resolver, int dc = 15)
        {
            Source = source;
            _resolver = resolver;
            _dc = dc;
        }

        public bool CanExecute() => Source.IsAlive;

        public CommandResult Execute()
        {
            var roll = _resolver.ResolveCheck(Source, StatType.Dexterity, _dc);
            if (roll.IsSuccess)
            {
                var result = new CommandResult { Success = true, Message = $"{Source.DisplayName} fled from combat!" };
                result.Metadata["Fled"] = "true";
                return result;
            }
            return new CommandResult { Success = false, Message = $"{Source.DisplayName} failed to flee" };
        }

        public void Undo() { }
    }

    public class UseItemCommand : ICombatCommand
    {
        public string CommandName => "Use Item";
        public ICombatant Source { get; }
        public ICombatant Target { get; }
        public int Priority => 15;

        private ItemEffect _item;

        public UseItemCommand(ICombatant source, ICombatant target, ItemEffect item)
        {
            Source = source;
            Target = target;
            _item = item;
        }

        public bool CanExecute() => Source.IsAlive && _item != null;

        public CommandResult Execute()
        {
            _item.Apply(Target);
            return new CommandResult { Success = true, Message = $"Used {_item.ItemName} on {Target.DisplayName}" };
        }

        public void Undo() { }
    }
}
