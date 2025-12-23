# Sun Eater ScriptableObject Asset Templates

These are templates for creating ScriptableObject assets in Unity Editor. Use these as reference when creating the actual assets.

---

## Humanist/Transhumanist Axis

**File:** `Assets/GameData/SunEater/Axes/HumanismAxis.asset`

```yaml
AxisId: humanism
DisplayName: Humanist vs Transhumanist
PositivePoleLabel: Humanist
NegativePoleLabel: Transhumanist
MinValue: -100
MaxValue: 100
DefaultValue: 0
PositiveColor: RGB(51, 204, 255)  # Cyan
NegativeColor: RGB(255, 77, 77)   # Red
```

---

## Cybernetic Enhancement Effects

### Minor Cybernetics
**File:** `Assets/GameData/SunEater/Effects/MinorCybernetics.asset`

```yaml
EffectName: Minor Cybernetic Enhancements
AxisId: humanism
MinValue: -49
MaxValue: -25
Description: Basic neural interface and optical enhancements

# Stat Modifiers
DexterityModifier: 1
IntelligenceModifier: 1
ConstitutionModifier: 0
HealthModifier: 0

# Social Impact
SocialPenalty: 0
PriceMultiplier: 1.0
AllowsFactionAccess: true
AllowsTerritoryAccess: true
```

### Moderate Cybernetics
**File:** `Assets/GameData/SunEater/Effects/ModerateCybernetics.asset`

```yaml
EffectName: Moderate Cybernetic Enhancements
AxisId: humanism
MinValue: -74
MaxValue: -50
Description: Cybernetic limbs, enhanced reflexes, memory augmentation

# Stat Modifiers
DexterityModifier: 2
IntelligenceModifier: 2
ConstitutionModifier: 1
HealthModifier: 5

# Social Impact
SocialPenalty: 0
PriceMultiplier: 1.0
AllowsFactionAccess: true
AllowsTerritoryAccess: true
```

### Major Cybernetics
**File:** `Assets/GameData/SunEater/Effects/MajorCybernetics.asset`

```yaml
EffectName: Major Cybernetic Enhancements
AxisId: humanism
MinValue: -100
MaxValue: -75
Description: Full-body conversion, AI integration, genetic rewriting

# Stat Modifiers
DexterityModifier: 4
IntelligenceModifier: 4
ConstitutionModifier: 2
HealthModifier: 10

# Social Impact
SocialPenalty: 0
PriceMultiplier: 1.0
AllowsFactionAccess: true
AllowsTerritoryAccess: true
```

---

## Chantry Reputation Effects

### Suspicious
**File:** `Assets/GameData/SunEater/Effects/ChantrySuspicious.asset`

```yaml
EffectName: Chantry: Suspicious
AxisId: humanism
MinValue: -49
MaxValue: -25
Description: The Chantry views your cybernetic modifications with concern

# Social Impact
SocialPenalty: -2
PriceMultiplier: 1.5
AllowsFactionAccess: true
AllowsTerritoryAccess: true
BlockedAbilities: []
```

### Condemned
**File:** `Assets/GameData/SunEater/Effects/ChantryCondemned.asset`

```yaml
EffectName: Chantry: Condemned
AxisId: humanism
MinValue: -74
MaxValue: -50
Description: The Chantry has publicly denounced your transhumanist ways

# Social Impact
SocialPenalty: -5
PriceMultiplier: 3.0
AllowsFactionAccess: false
AllowsTerritoryAccess: true
RestrictedAreas:
  - Chantry_Cathedral
  - Chantry_Seminary
BlockedAbilities: []
```

### Heretic
**File:** `Assets/GameData/SunEater/Effects/ChantryHeretic.asset`

```yaml
EffectName: Chantry: Heretic
AxisId: humanism
MinValue: -100
MaxValue: -75
Description: You are declared an enemy of the faith

# Social Impact
SocialPenalty: -10
PriceMultiplier: Infinity
AllowsFactionAccess: false
AllowsTerritoryAccess: false
RestrictedAreas:
  - Chantry_Cathedral
  - Chantry_Seminary
  - Chantry_Outpost
  - Holy_District
BlockedAbilities:
  - holy
  - divine
  - blessing
  - prayer
```

---

## How to Create in Unity

1. **Right-click in Project window** → Create → RPG → Morality → Axis Config
2. **Name the asset** (e.g., "HumanismAxis")
3. **Fill in the Inspector** using values from templates above
4. **Repeat for Effect Configs** → Create → RPG → Morality → Effect Config
5. **Assign to SunEaterMoralitySetup** component in scene

---

## Asset Organization

Recommended folder structure:
```
Assets/
└── GameData/
    └── SunEater/
        ├── Axes/
        │   └── HumanismAxis.asset
        │
        ├── Effects/
        │   ├── Cybernetics/
        │   │   ├── MinorCybernetics.asset
        │   │   ├── ModerateCybernetics.asset
        │   │   └── MajorCybernetics.asset
        │   │
        │   └── Chantry/
        │       ├── ChantrySuspicious.asset
        │       ├── ChantryCondemned.asset
        │       └── ChantryHeretic.asset
        │
        └── Abilities/
            ├── DrainLife.asset
            └── DivineSmite.asset
```

---

## Testing the Setup

Once assets are created:

1. Add `SunEaterMoralitySetup` component to a GameObject
2. Assign the `HumanismAxis` asset
3. Assign cybernetic effect assets (Minor, Moderate, Major)
4. Assign Chantry reputation assets (Suspicious, Condemned, Heretic)
5. Use Context Menu: "Make Transhumanist Choice (-25)"
6. Use Context Menu: "Show Status Report"
7. Verify buffs and penalties are applied correctly
