# Morality System Documentation

## Overview

The morality system provides configurable alignment tracking with gameplay effects. It supports multiple morality axes (e.g., Humanist/Transhumanist, Light/Dark) with buffs, penalties, and ability gating.

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    MORALITY SYSTEM                           │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────────┐  ┌──────────────────┐                │
│  │  MoralityState   │  │  AxisConfig      │                │
│  │                  │  │                  │                │
│  │ - Tracks values  │  │ - Pole labels    │                │
│  │ - Multiple axes  │  │ - Min/max values │                │
│  │ - Events         │  │ - Visual settings│                │
│  └──────────────────┘  └──────────────────┘                │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │           PowerService Decorator Pattern             │  │
│  │                                                       │  │
│  │  ┌────────────┐  wraps  ┌──────────────────────┐    │  │
│  │  │ Morality   │ ──────→ │   Base PowerService  │    │  │
│  │  │ Decorator  │         │                      │    │  │
│  │  └────────────┘         └──────────────────────┘    │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────┐  ┌──────────────────┐                │
│  │ Transhumanist    │  │ Chantry Infamy   │                │
│  │ Buff System      │  │ System           │                │
│  │                  │  │                  │                │
│  │ - Cybernetic     │  │ - Reputation     │                │
│  │   stat buffs     │  │ - Access control │                │
│  │ - Tiered scaling │  │ - Social penalties│               │
│  └──────────────────┘  └──────────────────┘                │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## File Structure

```
Systems/Morality/
├── MoralityAxisConfig.cs         # ScriptableObject for axis configuration
├── MoralityState.cs              # Core morality tracking (implements IMoralityService)
├── TranshumanistBuffSystem.cs    # Cybernetic stat buffs
├── ChantryInfamySystem.cs        # Religious faction penalties
└── SunEaterMoralityConfig.cs     # Sun Eater game integration

Systems/Combat/
├── IPowerService.cs              # Ability execution interface
├── PowerService.cs               # Base ability execution
└── MoralityCheckDecorator.cs     # Decorator for morality validation

Core/
└── ServiceLocator.cs             # Service registration (TDD Section 4 & 10)

Examples/
└── ExampleMoralitySystems.cs     # Complete usage examples
```

## Core Components

### 1. MoralityState Class

Implements `IMoralityService` and tracks morality values from -100 to 100 on configurable axes.

```csharp
// Create morality state
var morality = new MoralityState();

// Create axis configuration
var humanismAxis = ScriptableObject.CreateInstance<MoralityAxisConfig>();
humanismAxis.AxisId = "humanism";
humanismAxis.PositivePoleLabel = "Humanist";
humanismAxis.NegativePoleLabel = "Transhumanist";
humanismAxis.MinValue = -100f;
humanismAxis.MaxValue = 100f;

// Initialize
morality.Initialize(humanismAxis);

// Make choices
morality.ModifyAxis("humanism", 25f);  // Humanist choice (+25)
morality.ModifyAxis("humanism", -50f); // Transhumanist choice (-50)

// Get current value
float value = morality.GetAxisValue("humanism");

// Check requirements
bool meetsRequirement = morality.MeetsRequirement("humanism", 50f, null);
```

**Features:**
- Multiple axis support
- Value clamping to configured ranges
- Event notifications on value changes
- Requirement checking for abilities

### 2. PowerService Decorator Pattern

The decorator pattern allows layering validation logic on ability execution without modifying the base service.

```csharp
// Create base power service
var resolver = new D20CombatResolver(config);
IPowerService baseService = new PowerService(resolver, positioning);

// Wrap with morality decorator
IPowerService decoratedService = new MoralityCheckDecorator(baseService, moralityService);

// Check if ability can be executed (includes morality check)
bool canExecute = decoratedService.CanExecute(user, ability);

// Execute abilities (morality is validated automatically)
CommandResult result = decoratedService.Execute(user, target, ability);
```

**Benefits:**
- Separation of concerns
- Easy to add/remove validation layers
- Follows Gang of Four Decorator Pattern
- No modification to base PowerService

### 3. Sun Eater Game Configuration

Game-specific implementation for the Sun Eater setting with Humanist/Transhumanist axis.

```csharp
// Setup Sun Eater morality
var sunEater = gameObject.AddComponent<SunEaterMoralityConfig>();

// Make transhumanist choices
sunEater.MakeMoralityChoice(-30f, "Installed neural implant");
sunEater.MakeMoralityChoice(-30f, "Replaced limbs with cybernetics");
sunEater.MakeMoralityChoice(-20f, "Uploaded consciousness backup");

// Apply effects to combatant
sunEater.ApplyMoralityEffects(combatant);

// Check status
Debug.Log(sunEater.GetStatusReport());
```

## Sun Eater Systems

### Transhumanist Buffs

High transhumanist values grant cybernetic stat buffs that scale with alignment:

| Tier | Range | DEX | INT | CON | HP |
|------|-------|-----|-----|-----|----|
| None | 0 to -24 | - | - | - | - |
| Minor | -25 to -49 | +1 | +1 | - | - |
| Moderate | -50 to -74 | +2 | +2 | +1 | +5 |
| Major | -75 to -100 | +4 | +4 | +2 | +10 |

**Usage:**
```csharp
var buffSystem = new TranshumanistBuffSystem(moralityState, "humanism");

// Apply buffs based on current alignment
buffSystem.ApplyBuffs(combatant);

// Get current tier
TranshumanistTier tier = buffSystem.GetCurrentTier();

// Get description
string description = buffSystem.GetBuffDescription();
```

### Chantry Infamy Penalties

High transhumanist values impose penalties with the Chantry (religious faction):

| Level | Range | Social Penalty | Price Multiplier | Restrictions |
|-------|-------|----------------|------------------|--------------|
| Neutral | 0 to -24 | - | 1.0x | None |
| Suspicious | -25 to -49 | -2 | 1.5x | Watched closely |
| Condemned | -50 to -74 | -5 | 3.0x | Some areas restricted |
| Heretic | -75 to -100 | -10 | ∞ | No Chantry access, holy abilities blocked |

**Usage:**
```csharp
var infamySystem = new ChantryInfamySystem(moralityState, "humanism");

// Check access
bool hasAccess = infamySystem.HasChantryAccess();
bool canEnter = infamySystem.CanEnterTemples();

// Get penalties
int socialPenalty = infamySystem.GetSocialPenalty();
float priceMultiplier = infamySystem.GetPriceMultiplier();

// Check ability restrictions
bool isRestricted = infamySystem.IsAbilityRestricted("divine_smite");

// Get description
string status = infamySystem.GetInfamyDescription();
```

## Service Registration

Following TDD Section 4 and 10, use the ServiceLocator for dependency injection:

```csharp
// Create morality state
var morality = new MoralityState();
morality.Initialize(humanismAxis);

// Register morality service
ServiceLocator.Register<IMoralityService>(morality);

// Create power service with decorator
var resolver = new D20CombatResolver(config);
IPowerService powerService = new PowerService(resolver, positioning);
powerService = new MoralityCheckDecorator(powerService, morality);
ServiceLocator.Register<IPowerService>(powerService);

// Retrieve services anywhere in code
var moralityService = ServiceLocator.Get<IMoralityService>();
var power = ServiceLocator.Get<IPowerService>();
```

## Integration with CombatManager

The CombatManager already supports morality through `IMoralityService`:

```csharp
// Inject morality service
combatManager.SetMoralityService(moralityService);

// Abilities are automatically validated
combatManager.Attack(target, ability); // Checks morality requirements
```

## Creating Morality-Gated Abilities

Define abilities with morality requirements:

```csharp
// Humanist ability (requires >= 50)
var divineSmite = ScriptableObject.CreateInstance<AbilityData>();
divineSmite.AbilityId = "divine_smite";
divineSmite.DisplayName = "Divine Smite";
divineSmite.RequiredMoralityAxis = "humanism";
divineSmite.MinMoralityValue = 50f;

// Transhumanist ability (requires <= -50)
var drainLife = ScriptableObject.CreateInstance<AbilityData>();
drainLife.AbilityId = "drain_life";
drainLife.DisplayName = "Drain Life";
drainLife.RequiredMoralityAxis = "humanism";
drainLife.MaxMoralityValue = -50f;
```

## Examples

See `ExampleMoralitySystems.cs` for complete working examples:

1. **Example 1:** Basic MoralityState usage
2. **Example 2:** PowerService decorator pattern
3. **Example 3:** Sun Eater configuration
4. **Example 4:** Transhumanist buffs at different tiers
5. **Example 5:** Chantry infamy penalties
6. **Example 6:** Service registration
7. **Example 7:** Complete integration

Run examples from Unity Editor using the Context Menu on the component.

## Extending the System

### Add New Morality Axis

```csharp
var lawChaosAxis = ScriptableObject.CreateInstance<MoralityAxisConfig>();
lawChaosAxis.AxisId = "law_chaos";
lawChaosAxis.PositivePoleLabel = "Lawful";
lawChaosAxis.NegativePoleLabel = "Chaotic";

morality.AddAxis(lawChaosAxis);
```

### Add Custom Decorator

```csharp
public class ResourceCheckDecorator : IPowerService
{
    private readonly IPowerService _wrapped;
    
    public ResourceCheckDecorator(IPowerService wrapped)
    {
        _wrapped = wrapped;
    }
    
    public bool CanExecute(ICombatant user, IAbility ability)
    {
        // Custom validation
        if (!HasEnoughResources(user, ability))
            return false;
            
        return _wrapped.CanExecute(user, ability);
    }
    
    public CommandResult Execute(ICombatant user, ICombatant target, IAbility ability)
    {
        return _wrapped.Execute(user, target, ability);
    }
}

// Chain decorators
IPowerService service = new PowerService(resolver);
service = new MoralityCheckDecorator(service, morality);
service = new ResourceCheckDecorator(service);
```

## Testing

The morality system is designed for testability:

```csharp
[Test]
public void TestMoralityValueClamping()
{
    var morality = new MoralityState();
    morality.Initialize(humanismAxis);
    
    morality.ModifyAxis("humanism", 150f); // Should clamp to 100
    Assert.AreEqual(100f, morality.GetAxisValue("humanism"));
}

[Test]
public void TestDecoratorPattern()
{
    var morality = new MoralityState();
    morality.Initialize(humanismAxis);
    morality.SetAxisValue("humanism", -75f); // High transhumanist
    
    IPowerService service = new PowerService(resolver);
    service = new MoralityCheckDecorator(service, morality);
    
    var transhumanistAbility = CreateTranshumanistAbility();
    Assert.IsTrue(service.CanExecute(combatant, transhumanistAbility));
    
    var humanistAbility = CreateHumanistAbility();
    Assert.IsFalse(service.CanExecute(combatant, humanistAbility));
}
```

## Best Practices

1. **Use ServiceLocator for global services** - Register morality and power services once at game start
2. **Create axis configs as ScriptableObjects** - Allows designer configuration in Unity Editor
3. **Subscribe to morality events** - React to alignment changes for UI updates
4. **Layer decorators carefully** - Order matters (morality → resources → cooldowns)
5. **Test with examples** - Use the provided examples to verify integration

**Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.**
