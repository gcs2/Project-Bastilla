# SYSTEM_RULES.md (v1.2)
**Project:** Axiom RPG Engine (Target: The Sun Eater)
**Copyright:** (c) Geoffrey Salmon 2025
**Repository:** https://github.com/gcs2/Project-Bastilla/
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
11. **Unity AI Toolkit Conflict**: The Unity AI Assistant (Muse) may throw `ArgumentException` if it encounters unknown categories like `Textures` during indexing. Ensure generated assets follow standard naming and search conventions to minimize toolkit friction.
12. **Terminal Safety**: Avoid recursive `Remove-Item` or `dotnet build` on the root without explicit need. **NEVER** run or attempt to run commands on terminal that will result in deletions or data corruption risk.
13. **The Spectacle Gap**: Architectural stability (tests/logic) does not equal a "Visual Spectacle." Every demo generator must prioritize silhouette, volumetric depth, and Environmental "Noise" to meet Sun Eater fidelity. (Ref: The Pill Incident).
14. **Zero-Warning Protocol**: When implementing Mocks or Interfaces that require events which are unused in that specific class, always initialize them (e.g., `event Action OnX = delegate { };`) to silence compiler warnings (CS0067) while satisfying the contract.

---

## 1. THE GOLDEN RULES (Architectural Pillars)
*Violating these rules causes technical debt. Reject any generation that ignores them.*

### 1.1 Composition Over Inheritance
* **Do Not:** Create deep inheritance trees like `Player > Human > Combatant`.
* **Do:** Use the `ICombatant` interface and the `Combatant` MonoBehaviour as a container for components (`AbilityManager`, `StatusEffectManager`, `ResourcePool`).
* **Rule:** Game logic must rely on Interfaces (`IAbility`, `ICombatResolver`), not concrete implementations.

### 1.2 Data-Driven Design
* **Rule:** All game data (stats, dialogue, quests) must reside in `ScriptableObjects` (`RPGPlatform.Data`).
* **Rule:** Runtime logic (`RPGPlatform.Systems`) acts upon data but never owns its "master copy".
* **Goal:** 100% of game content should be modifiable without re-compiling C# code.
* **Immutable Test Rule:** All new functionality MUST be verified via the Text Adventure Verification (TAV) harness. If logic cannot be verified via text commands, it is too coupled to the View.

### 1.3 Authoritative Knowledge Map
When performing a task, always refer to the specific document governing that topic:

| Topic Area | Source of Truth (Document) | Role |
| :--- | :--- | :--- |
| **Architectural Rules** | `System_Rules.md` | Core patterns, naming, and engineering policy. |
| **Demo Content** | `PlayableDemoGuide.md` | Sun Eater lore, scene setup, and asset lists. |
| **Technical Design** | `Technical_Design_Document.md`| Foundational logic and original platform specs. |
| **Project Vision** | `KotOR_Vision_Guide.txt` | The original "Knights of the Old Republic" inspiration. |
| **AI Content Workflow**| `Unity_Assistant_Guide.md` | Prompt engineering & AI tool usage. |
| **API / Implementation**| `Engineering_Design.md` | Deep technical specs for subsystem logic. |
| **Project Progress** | `Project_Walkthrough.md` | History of changes and stabilized state. |

> [!IMPORTANT]
> If a task spans multiple topics, you must load and synthesize all relevant docs from the table above.
* **Constraint:** Hard-coded values (damage numbers, strings, mana costs) are forbidden in C# classes. They must be loaded from `Config` objects.

### 1.4 The Command Pattern (Combat)
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
6.  **Test Mandate:** Always verify that your changes are covered by a test in `Assets/AxiomEngine/Editor/Tests/`.

---

## 6. VERIFICATION & STABILITY
*Standard operating procedure for every code change.*

### 6.1 Text Adventure Verification (TAV)
*   **Rule:** The primary verification method is the **Axiom CLI** (Text Adventure Mode).
*   **Concept:** The entire game logic (Combat, Dialogue, Questing) must be playable via a text-based console without rendering a single frame.
*   **Action:** Create a TAV Scenario script (e.g., `VorgossosScenario.cs`) that steps through the user story using pure C# logic calls.
*   **Deprecation Notice:** The legacy Unity Test Runner (`RunTests.ps1`) is DEPRECATED for daily logic verification due to excessive overhead and brittleness. It is reserved for nightly build integrity only.

### 6.2 Terminal Safety Mandate
* **Rule:** Never run or attempt to run commands on the terminal that result in deletions or data corruption risk (e.g., `rm -rf`, recursive `Remove-Item` without filters, force-cleaning the repository).
* **Action:** Use safe wrappers or targeted removals only for known temporary files (e.g., `unity_test_log.txt`).

### 6.3 Regression Awareness
* **Mindset:** Assume that interface changes will break Mocks in `Tests/` and Examples in `Examples/`.
* **Action:**
1. **Find a Prompt**: Look in `PlayableDemoGuide.md` (e.g., "Vorgossos Marketplace").
2. **Choose Your Tool**:
   - **Unity AI (Muse)**: Open via `Window > AI > Assistant`. Use the chat interface to generate textures/sprites or get help with asset search.
   - **Rosebud AI**: Use the web portal for *Characters* and *Animations*.
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

1. **The Loop:** Write TAV Scenario → Run Axiom CLI → Verify Output → Fix → Repeat.
2. **Read Twice, Code Once:** Before fixing a bug, reproduce it in the Text Adventure harness.
3. **Incremental Progress:** Use the CLI output to verify state changes (e.g., "Health: 100 -> 90").
4. **Transparency in Failure**: If a test reveals a fundamental design flaw, document it in the Post-Mortem section immediately. 

---

## 9. VISUAL SPECTACLE STANDARDS
*The engine must not only work; it must look Sun Eater ready.*

1. **No Bare Primitives**: Demos must never rely on pure Capsules or Cubes without specific emissive/shader overrides or particle effects.
2. **Atmospheric Depth**: Every generated scene MUST include a configured `Volume` with Bloom and Vignette, and `RenderSettings.fog` enabled.
3. **Prop Ingestion**: Level generators must prioritize spawning modular prop clusters (Stalls, Pillars, Debris) over flat geometry.
4. **Cinematic Lighting**: Use three-point lighting or rim-light offsets for primary NPCs to establish silhouette depth.
5. **Texture Density**: Avoid default Unity materials. Generators must attempt to map AI-generated or mottled textures to break up surface repetition.
