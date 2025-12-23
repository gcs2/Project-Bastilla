# Axiom RPG Engine - Unity Implementation

A complete turn-based combat and narrative system for Unity following Clean Architecture principles, implementing the Command Pattern, State Machine, and composition-based design.

## Architecture Overview

[Axiom Engine Documentation](file:///c:/Users/zephy/Downloads/kotorturnbasedsystem/CombatSystem/CombatSystem/Documentation/)
- 📘 [Engineering Design](file:///c:/Users/zephy/Downloads/kotorturnbasedsystem/CombatSystem/CombatSystem/Documentation/Architecture/Engineering_Design.md)
- 🏛️ [Senior Eng Walkthrough](file:///c:/Users/zephy/Downloads/kotorturnbasedsystem/CombatSystem/CombatSystem/Documentation/Internal/Senior_Eng_Walkthrough.md)
- 👾 [Playable Demo Guide](file:///c:/Users/zephy/Downloads/kotorturnbasedsystem/CombatSystem/CombatSystem/Documentation/Guides/Playable_Demo_Guide.md)
- ⚙️ [System Rules](file:///c:/Users/zephy/Downloads/kotorturnbasedsystem/CombatSystem/CombatSystem/Documentation/Architecture/System_Rules.md)

```
┌─────────────────────────────────────────────────────────────────┐
│                      AXIOM CORE & SYSTEMS                       │
│   Orchestrates all systems, handles events, manages flow         │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐  │
│  │    STATE     │  │   COMMAND    │  │      TURN            │  │
│  │   MACHINE    │  │   HISTORY    │  │      QUEUE           │  │
│  │              │  │              │  │                      │  │
│  │ Planning     │  │ AttackCmd    │  │ Initiative-sorted    │  │
│  │ Execution    │  │ DefendCmd    │  │ combatant order      │  │
│  │ Resolution   │  │ MoveCmd      │  │                      │  │
│  │              │  │              │  │                      │  │
│  └──────────────┘  └──────────────┘  └──────────────────────┘  │
│                                                                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐  │
│  │    D20       │  │  POSITIONING │  │     RESOURCE         │  │
│  │  RESOLVER    │  │   SYSTEM     │  │      POOL            │  │
│  │ (Pluggable)  │  │ (Grid/Free)  │  │ (Force/Stamina)      │  │
│  └──────────────┘  └──────────────┘  └──────────────────────┘  │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

## Folder Structure

```
AxiomEngine/
├── Core/
│   ├── Audio/                     # Audio service interfaces
│   ├── Dialogue/                  # Narrative definitions
│   └── CoreInterfaces.cs          # Central contracts & enums
│
├── Systems/
│   ├── Combat/                    # D20 Resolver, TurnMgr, Stats
│   ├── Dialogue/                  # Conversation runtime
│   ├── Morality/                  # Choice & Consequence state
│   ├── Audio/                     # AudioManager (MonoBehaviour)
│   └── Input/                     # Centralized InputManager
│
├── Data/                          # ScriptableObject templates
├── UI/
│   └── Presenters/                # Binding Logic to Unity Assets
└── Editor/                        # Data Importers & Tools
```

## Core Systems

### 1. Combat Resolver (Strategy Pattern)
Pluggable combat resolution—swap between d20, percentile, or custom card-based systems easily.

### 2. Narrative Engine
Branching dialogue with alignment gating and skill-check requirements integrated directly into the `DialogueViewController`.

### 3. "Vibe Coding" Pipeline
- **LLM Data Guidelines**: Pre-defined schemas for AI content generation.
- **Data Importers**: Editor tools to batch-inject content directly into Unity.

---
**Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.**

# SYSTEM_RULES.md (v1.0)
**Project:** Axiom RPG Engine (Target: The Sun Eater)
**Tech Stack:** Unity 2025 | C# | .NET Standard 2.1
**Architecture:** Layered | Data-Driven (ScriptableObjects) | Command Pattern | Composition

---

## 1. THE GOLDEN RULES (Architectural Pillars)
*Violating these rules causes technical debt. Reject any generation that ignores them.*

### 1.1 Composition Over Inheritance
* **Do Not:** Create deep inheritance trees like `Player > Human > Combatant`.
* **Do:** Use the `ICombatant` interface and the `Combatant` MonoBehaviour as a container for components (`AbilityManager`, `StatusEffectManager`, `ResourcePool`).
* **Rule:** Game logic must rely on Interfaces (`IAbility`, `ICombatResolver`), not concrete implementations.

### 1.2 Data-Driven Design
* **Rule:** Behavior is Code; Content is Data.
* **Implementation:** All static game data (Abilities, Classes, Items, Quests, NPCs) **MUST** be defined as `ScriptableObject`.
* **Constraint:** Hard-coded values (damage numbers, strings, mana costs) are forbidden in C# classes. They must be loaded from `Config` objects.

### 1.3 The Command Pattern (Combat)
* **Rule:** No direct state modification during the `Planning` phase.
* **Implementation:** All combat actions (Attack, Move, Defend) must implement `ICombatCommand`.
* **Flow:** `CombatManager` receives input -> creates Command -> pushes to `CommandHistory` -> Command calls `Execute()`.
* **Requirement:** All Commands must support `Undo()`.

### 1.4 The Observer Pattern (UI & State)
* **Rule:** UI Code never "polls" data in `Update()`.
* **Implementation:** UI updates **ONLY** via C# Events (`Action<T>`).
* **Example:** `HealthBar` subscribes to `ICombatant.OnDamageReceived`, it does not check `currentHealth` every frame.

---

## 2. NAMESPACE & FOLDER STRUCTURE
*Maintain strict separation of concerns.*

* **`AxiomEngine.Core`**: Interfaces, Enums, Event definitions, and pure Data Structures (`DamageResult`, `RollResult`). *No Unity-specific logic if possible.*
* **`AxiomEngine.Systems`**: Concrete logic implementations.
    * `.Combat`: `CombatManager`, `D20Resolver`, `TurnQueue`.
    * `.Morality`: `AlignmentService`, `AxisState`.
    * `.Dialogue`: `DialogueManager`, `NodeParser`.
    * `.Progression`: `LevelingService`, `XPManager`.
* **`AxiomEngine.Data`**: ScriptableObject definitions (`AbilityData`, `ItemData`).
* **`AxiomEngine.UI`**: View-layer logic (Presenters).

---

## 3. CODING STANDARDS (For AI Generation)

* **Formatting:** K&R style braces. 4-space indentation.
* **Naming:** `PascalCase` for public methods/properties. `_camelCase` for private fields.
* **Serialization:** Use `[SerializeField] private` for Inspector variables. Never `public` fields.
* **Documentation:** All public interfaces and methods must have XML `/// <summary>` comments.
* **Error Handling:** Fail gracefully. Use `Debug.LogError` for missing references but do not crash the game loop.

---

## 4. DOMAIN RULES: THE SUN EATER (Genre Profile)
*Apply these "Flavor" rules when generating content or mechanics.*

### 4.1 Morality System (The "Alignment")
* **Axis:** `Humanism` (Positive) vs. `Transhumanism` (Negative).
* **Logic:**
    * **Humanist:** Rejects AI, uses kinetic/energy weapons, supported by Chantry. Buffs: Defense, Charisma, "Faith".
    * **Transhumanist:** Embraces cybernetics/genetics. Buffs: Speed, Intelligence, "Dark Tech".
    * **Mechanic:** `MoralityState` must inject modifiers into `SocialCheck` and `AbilityUnlock` logic.

### 4.2 Power Scaling (The "Math")
* **Progression:** Logarithmic (1-50 Levels).
* **Tiers:**
    1.  **Initiate:** Basic ballistics/blades.
    2.  **Knight:** Energy weapons, minor genetic buffs.
    3.  **Champion:** High-matter swords, command abilities.
    4.  **Palatine:** Reality-warping (The Quiet), time manipulation.
* **Constraint:** Damage numbers must verify against the `ProgressionConfig` curve.

### 4.3 Combat Physics
* **Resolver:** D20-based (Default).
* **Initiative:** `d20 + DEX + Alertness`.
* **Resources:** `NeuralStrain` (Mana equivalent) or `Stamina`.

---

## 5. EXISTING INTERFACE CONTRACTS
*The AI must respect these existing definitions found in the codebase.*

* `ICombatant`: Must implement `ApplyDamage(DamageResult)`, `Stats { get; }`, `Team { get; }`.
* `IAbility`: Must implement `Use(ICombatant user)`, `ResourceCost { get; }`.
* `ICombatResolver`: Must implement `ResolveAttack(...)` returning `RollResult`.
* `IPositioningSystem`: Must support `IsInRange(...)` and `GetDistance(...)`.

---

## 6. PROMPT INSTRUCTIONS
*When acting as the Coding Assistant:*

1.  **Check the Context:** Before writing code, verify if the Interface already exists in `AxiomEngine.Core`.
2.  **Mock Dependencies:** If a required system (e.g., `Inventory`) is missing, define its Interface first, then inject it.
3.  **Testability:** generated logic must be decoupled enough to run a NUnit test (e.g., separating Math from `MonoBehaviour`).