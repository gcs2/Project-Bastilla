// ============================================================================
// Axiom RPG Engine - Status Effect System
// Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.
// ============================================================================

using System;
using UnityEngine;
using RPGPlatform.Core;

namespace RPGPlatform.Systems.Combat
{
    /// <summary>
    /// Runtime instance of a status effect
    /// </summary>
    public class StatusEffect : IStatusEffect
    {
        private StatusEffectData _data;
        
        public string EffectId => _data.EffectId;
        public string DisplayName => _data.DisplayName;
        public Sprite Icon => null; // ToDo: Load from Resources/Addressables
        
        public int Duration { get; private set; }
        public int StackCount { get; private set; }
        public int MaxStacks => _data.MaxStacks;
        public bool IsDebuff => _data.IsDebuff;
        public bool IsDispellable => _data.IsDispellable;
        
        public ICombatant Source { get; private set; }
        public ICombatant Target { get; private set; }
        
        public StatusEffect(StatusEffectData data, ICombatant source, ICombatant target)
        {
            _data = data;
            Source = source;
            Target = target;
            Duration = data.Duration;
            StackCount = 1;
        }

        public void OnApply()
        {
            Debug.Log($"[Effect] {DisplayName} applied to {Target.DisplayName}");
            // Initial stat mod calculation happens via GetStatModifiers
        }

        public void OnTick()
        {
            if (Duration > 0) Duration--;
            
            // Apply DoT/HoT
            if (_data.DamagePerTurn > 0)
            {
                int dmg = _data.DamagePerTurn * StackCount;
                Target.ApplyDamage(new DamageResult 
                { 
                    BaseDamage = dmg, 
                    Type = _data.DamageType,
                    // Treat DoT as unresistable for now or add logic
                    DamageMultiplier = 1f 
                });
            }
            
            if (_data.HealingPerTurn > 0)
            {
                Target.ApplyHealing(_data.HealingPerTurn * StackCount);
            }
        }

        public void OnRemove()
        {
            Debug.Log($"[Effect] {DisplayName} removed from {Target.DisplayName}");
        }

        public void AddStack()
        {
            if (StackCount < MaxStacks) StackCount++;
            Duration = _data.Duration; // Refresh duration
        }

        public CombatStats GetStatModifiers()
        {
            var mods = new CombatStats();
            // Initialize with 0s (since it's a modifier, not base stats)
            // CombatStats defaults to 10s... we just want deltas.
            // Using a simpler struct or just zeroing out would be better.
            // For now, let's create a clean "Modifier" object if CombatStats allows.
            // Actually, CombatStats has 10 base. We should probably use a separate Modifier struct
            // But interface says `CombatStats GetStatModifiers()`.
            // Let's assume the consumer knows 10 is the baseline? 
            // Or better: CombatStats shouldn't be used for deltas.
            // The interface `CombatStats GetStatModifiers()` implies we return a stats object.
            // If the system adds these stats, it needs to handle the "10" base relative to 0.
            
            // Hack: manually set default 10s to 0 for the modifier object return
            mods.Strength = 0;
            mods.Dexterity = 0;
            mods.Constitution = 0;
            mods.Intelligence = 0;
            mods.Wisdom = 0;
            mods.Charisma = 0;
            mods.ArmorClass = 0;
            // ...
            
            // Now apply actual mods * Stacks
            mods.Strength += _data.StrengthMod * StackCount;
            mods.Dexterity += _data.DexterityMod * StackCount;
            mods.Constitution += _data.ConstitutionMod * StackCount;
            mods.ArmorClass += _data.ArmorClassMod * StackCount;
            
            return mods;
        }
    }
}
