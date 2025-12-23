// ============================================================================
// Axiom RPG Engine - Status Effect Manager
// Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RPGPlatform.Core;
using RPGPlatform.Data;

namespace RPGPlatform.Systems.Combat
{
    /// <summary>
    /// Manages the collection of status effects for a single combatant
    /// </summary>
    public class StatusEffectManager
    {
        private ICombatant _owner;
        private List<IStatusEffect> _effects = new List<IStatusEffect>();
        
        public IReadOnlyList<IStatusEffect> Effects => _effects;
        
        public event Action<IStatusEffect> OnEffectAdded;
        public event Action<IStatusEffect> OnEffectRemoved;
        
        public StatusEffectManager(ICombatant owner)
        {
            _owner = owner;
        }
        
        public StatusEffect ApplyEffect(StatusEffectTemplate template, ICombatant source)
        // Wait, Combatant.cs calls ApplyEffect(StatusEffectTemplate, ICombatant). 
        // CoreInterfaces has StatusEffectData class, not Template. Let's assume Template = Data or wrapper.
        // CoreInterfaces line 451: public class StatusEffectData
        // Combatant.cs line 186 calls ApplyStatusEffect(StatusEffectTemplate template, ...)
        // This implies Combatant.cs has a typo or I need StatusEffectTemplate class.
        // Let's assume StatusEffectData is the correct one.
        // I will change signature here to StatusEffectData.
        { 
            // Check if stackable exists
            var existing = _effects.FirstOrDefault(e => e.EffectId == template.EffectId);
            if (existing != null)
            {
                existing.AddStack();
                Debug.Log($"[Effect] Stack added to {existing.DisplayName} on {_owner.DisplayName} ({existing.StackCount} stacks)");
                return existing as StatusEffect;
            }
            else
            {
                var newEffect = new StatusEffect(template.ToData(), source, _owner);
                _effects.Add(newEffect);
                newEffect.OnApply();
                OnEffectAdded?.Invoke(newEffect);
                return newEffect;
            }
        }
        
        // Overload for simply adding an instance (manual)
        public void AddEffect(IStatusEffect effect)
        {
             _effects.Add(effect);
             effect.OnApply();
             OnEffectAdded?.Invoke(effect);
        }

        public void RemoveEffect(string effectId)
        {
            var effect = _effects.FirstOrDefault(e => e.EffectId == effectId);
            if (effect != null)
            {
                effect.OnRemove();
                _effects.Remove(effect);
                OnEffectRemoved?.Invoke(effect);
            }
        }
        
        public void ClearAll()
        {
            // Reverse loop
            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                _effects[i].OnRemove();
                OnEffectRemoved?.Invoke(_effects[i]);
            }
            _effects.Clear();
        }

        public void TickAll()
        {
            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                var effect = _effects[i];
                effect.OnTick();
                
                // Remove expired
                if (effect.Duration == 0)
                {
                    RemoveEffect(effect.EffectId);
                }
            }
        }
        
        public CombatStats GetTotalModifiers()
        {
            var total = new CombatStats();
            // Start with 0s because CombatStats defaults to 10.
            // A helper helper to zero out stats is needed or manual.
            total.Strength = 0;
            total.Dexterity = 0;
            total.Constitution = 0;
            total.Intelligence = 0;
            total.Wisdom = 0;
            total.Charisma = 0;
            total.ArmorClass = 0;
            total.ProficiencyBonus = 0;
            total.InitiativeBonus = 0;
            total.MaxHealth = 0; 
            // Note: CurrentHealth shouldn't be mod directly usually? Or max health buff? 
            // We usually modify Max. 
            
            foreach (var effect in _effects)
            {
                var mods = effect.GetStatModifiers();
                if (mods != null)
                {
                    total.Strength += mods.Strength;
                    total.Dexterity += mods.Dexterity;
                    total.Constitution += mods.Constitution;
                    total.Intelligence += mods.Intelligence;
                    total.Wisdom += mods.Wisdom;
                    total.Charisma += mods.Charisma;
                    total.ArmorClass += mods.ArmorClass;
                    // ... others if defined in CombatStats/Effect
                }
            }
            return total;
        }

        public bool HasEffect(string effectId)
        {
            return _effects.Any(e => e.EffectId == effectId);
        }
        
        public bool IsStunned()
        {
            // Need a convention for "Stunned". 
            // Maybe check for ID "Stun"? Or add Property to StatusEffectData?
            // For now, check ID.
            return HasEffect("Stun");
        }
        
        public bool IsRooted()
        {
            return HasEffect("Root") || HasEffect("Stun");
        }
    }
}
