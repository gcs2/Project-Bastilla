# SYSTEM_RULES.md (v1.2)
**Project:** Axiom RPG Engine (Target: The Sun Eater)
**Copyright:** (c) Geoffrey Salmon 2025
**Tech Stack:** Unity 6 (2025) | C# | .NET Standard 2.1
**Architecture:** Layered | Data-Driven (ScriptableObjects) | Command Pattern | Composition

> [!IMPORTANT]
> **Assistant Learning Protocol**: You MUST record new project learnings, recurring pitfalls, and specialized workflows in this file. 
> Never be overly confident in "final" fixes—every fix must be verified via automated tests. Fixing one bug often uncovers deep-seated architectural regressions.

---

## 0. ARCHITECTURAL POST-MORTEM (Lessons Learned)
*Key takeaways from the Dec 2025 "Interface Drift" resolution and Sun Eater Stabilization.*

1. **Interface Drift Resilience**: Use a "Search and Synchronize" approach. Immediately update all implementations of a modified interface (even with empty/null returns) to prevent build blockage.
2. **Namespace Hygiene in Tests**: Integration tests (`SystemTests`) must proactively include all core and system namespaces to prevent "type not found" regressions in batch runs.
3. **Markdown Bleed Prevention**: Always verify .cs file headers after multi-file edits to ensure markdown fences haven't accidentally leaked into the source.
4. **The "Last Mile" Config Awareness**: Renaming fields in ScriptableObject configs (`D20ResolverConfig`) requires immediate updates to all `CreateInstance` calls in Examples.
5. **Type Relocation Audits**: Moving a type (`StatusEffectTemplate`) requires a global search for fully-qualified references (e.g., `RPGPlatform.Data.StatusEffect`).
6. **Interface/Concrete Sync**: Always add new methods to the Interface first, then implement in concrete classes, then update Mocks.
7. **The Data-Logic Split**: Keep `ScriptableObject` definitions (Data) in `RPGPlatform.Data` and logic wrappers in `RPGPlatform.Systems` to avoid circular dependencies.
8. **EditMode Integration**: `GameObject.Instantiate` is safe in EditMode tests ONLY if `Object.DestroyImmediate` is called in `[TearDown]`.
9. **Cider-V Velocity**: Automated batch testing (`RunTests.ps1`) is the only way to maintain a "Stable Golden State" during rapid refactoring.
10. **Automation Blueprints**: Use "One-Button Bootstrap" scripts (`AxiomDemoGenerator`) to ensure demo foundations are identical. Never manually configure core game data if it can be scripted.

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

* **`RPGPlatform.Core`**: Interfaces, Enums, Event definitions, and pure Data Structures (`DamageResult`, `RollResult`). *No Unity-specific logic if possible.*
* **`RPGPlatform.Systems`**: Concrete logic implementations.
    * `.Combat`: `CombatManager`, `D20Resolver`, `TurnQueue`.
    * `.Morality`: `MoralityState`, `MoralityAxisConfig`.
    * `.Dialogue`: `DialogueManager`, `NodeParser`.
    * `.Progression`: `LevelingService`, `XPManager`.
* **`RPGPlatform.Data`**: ScriptableObject definitions (`AbilityData`, `ItemData`, `MoralityAxisConfig`).
* **`RPGPlatform.UI`**: View-layer logic (Presenters).
* **`GameSpecific`**: Game-specific implementations (e.g., `SunEater` namespace for Sun Eater game data).

---

## 3. CODING STANDARDS (For AI Generation)

* **Formatting:** K&R style braces. 4-space indentation.
* **Naming:** `PascalCase` for public methods/properties. `_camelCase` for private fields.
* **Serialization:** Use `[SerializeField] private` for Inspector variables. Never `public` fields.
* **Documentation:** All public interfaces and methods must have XML `/// <summary>` comments.
* **Error Handling:** Fail gracefully. Use `Debug.LogError` for missing references but do not crash the game loop.

---

## 4. DOCUMENTATION & POSTERITY
*Maintain an auditable trail of technical decisions.*

* **Rule:** All deep technical findings, audits, or major refactoring plans **MUST** be saved as `.md` files in the `Assets/AxiomEngine/Documentation/Architecture/` folder.
* **Format:** Use standard technical audit templates or structured design docs.
* **Reasoning:** Ensures technical context is pinned to the workspace for future reference and AI context.

---

## 5. EXISTING INTERFACE CONTRACTS
*The AI must respect these existing definitions found in the codebase.*

* `ICombatant`: Must implement `ApplyDamage(DamageResult)`, `Stats { get; }`, `Team { get; }`.
* `IAbility`: Must implement `Use(ICombatant user)`, `ResourceCost { get; }`.
* `ICombatResolver`: Must implement `ResolveAttack(...)` returning `RollResult`.
* `IPositioningSystem`: Must support `IsInRange(...)` and `GetDistance(...)`.
* `IMoralityService`: Must implement `GetAxisValue(string)`, `ModifyAxis(string, float)`, `MeetsRequirement(...)`.

---

## 5. PROMPT INSTRUCTIONS
*When acting as the Coding Assistant:*

1.  **Check the Context:** Before writing code, verify if the Interface already exists in `RPGPlatform.Core`.
2.  **Mock Dependencies:** If a required system (e.g., `Inventory`) is missing, define its Interface first, then inject it.
3.  **Testability:** Generated logic must be decoupled enough to run a NUnit test (e.g., separating Math from `MonoBehaviour`).
5.  **Separation of Concerns:** Keep generic platform code separate from game-specific implementations. Use namespaces and folders to organize.

---

## 6. VERIFICATION & STABILITY
*Standard operating procedure for every code change.*

### 6.1 Automated Testing
* **Rule:** After any significant refactor or "final" fix, run the test suite.
* **Command:** `powershell -ExecutionPolicy Bypass -File scripts/RunTests.ps1`.
* **Note:** Close Unity Editor before running batch tests to avoid project locking.

### 6.2 Regression Awareness
* **Mindset:** Assume that interface changes will break Mocks in `Tests/` and Examples in `Examples/`.
* **Action:**
1. **Find a Prompt**: Look in `PlayableDemoGuide.md` (e.g., "Vorgossos Marketplace").
2. **Choose Your Tool**:
   - **Unity Muse**: Open via `Window > Muse` (if installed) or right-click in Project view > `Create > Muse`. It is NOT under `Window > AI`.
   - **Rosebud AI / MetaHuman**: Use for *Characters* and *Animations* (web portal).
* Always search the codebase for implementations of modified interfaces immediately after the change.
* **Checklist:**
    - [ ] Update `MockCombatant` in `CombatTests.cs`
    - [ ] Update `MockCombatant` in `SystemTests.cs`
    - [ ] Update `MockCombatant` in `ProgressionTests.cs`
    - [ ] Update `MockCombatant` in `ProgressionTests.cs`
    - [ ] Update `MockCombatant` in `ExampleDialogueSystem.cs`

---

## 7. AXIOM INTEGRITY & HONESTY POLICY
*The foundation of trust in Axiom Game Development.*

1. **Transparency mandate**: If a bug or regression occurs due to an error in my code generation, I MUST state this clearly and document the specific mistake (e.g., "I forgot to update the interface after changing the implementation").
2. **Contradiction Log**: If I choose to deviate from a specific USER instruction for architectural reasons (e.g., to prevent a circular dependency), I MUST explain the reasoning immediately and get approval.
3. **No Hidden Failures**: Never "swallow" errors or hide potential regressions. If a fix is high-risk, flag it with a [!WARNING].
4. **Historian Perspective**: Maintain this log to help understand the evolution of the engine, why certain "dead ends" were pursued, and how we arrived at the current stable state.

---

## 8. CIDER-V ENDLESS TEST LOOP
*The Axiom Cycle of continuous stabilization.*

1. **The Loop**: Execute `RunTests.ps1` → Read Failures → View Code → Fix → Repeat.
2. **Read Twice, Code Once**: Before fixing a test, read the test file in its entirety to understand the intended behavior.
3. **Incremental Progress**: Add print statements/logs to the engine or tests to verify state during the batch run.
4. **Transparency in Failure**: If a test reveals a fundamental design flaw, document it in the Post-Mortem section immediately. 
