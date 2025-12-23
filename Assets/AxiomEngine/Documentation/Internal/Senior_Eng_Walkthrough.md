# Axiom RPG Engine: Technical Lead Walkthrough 🏛️👨‍💻

**To: Junior Engineering Team**  
**From: Tech Lead (Antigravity)**  
**Subject: Engine Architecture & Standards**

Welcome to the **Axiom RPG Engine**. This codebase isn't just a collection of scripts; it's a modular platform designed for scalability, testability, and "vibe-centric" development. This walkthrough explains *how* we built it and *why* we made certain architectural decisions.

---

## 1. The Core Architecture (Clean Layering)
We follow a strict **Layered Architecture**. Look at the folder structure:
- **`RPGPlatform.Core`**: These are our "Contracts." They contain Interfaces (`ICombatant`, `IAbility`) and pure Data Structures (`RollResult`). Core should **never** depend on other layers.
- **`RPGPlatform.Systems`**: This is where logic lives (`TurnManager`, `D20CombatResolver`). Systems implement Core interfaces but shouldn't know about UI.
- **`RPGPlatform.UI`**: The "Presentation Layer." We use **Presenters** to bind logic events to Unity components (TextMeshPro). 

> [!TIP]
> **Why?** If we decide to swap Unity UI for a different UI system next year, we only change the `UI` folder. The combat math stays identical.

---

## 2. Dependency Management: The Service Locator
Avoid using `FindObjectOfType` or `GameObject.Find`. It's slow and brittle. Instead, we use `ServiceLocator.cs`.
- **How to register**: `ServiceLocator.Register<IMoralityService>(this);`
- **How to use**: `var morality = ServiceLocator.Get<IMoralityService>();`

---

## 3. Designing with Data (ScriptableObjects)
In Axiom, **Behavior is Code; Content is Data**.
- **`AbilityData.cs`**: This isn't just a container; it's a template. Designers can create 100 different fireballs in the Inspector without writing a single line of C#.
- **`LLM_DATA_GUIDELINE.md`**: We've optimized our data schemas so that AI (LLMs) can generate valid content for us. This is "Vibe Coding" at its peak.

---

## 4. Design Patterns in Use
- **Command Pattern**: Every action in combat (`AttackCommand`) is an object. This allows us to support `Undo()` and `Command History` for complex tactical replays.
- **Observer Pattern**: We use C# Actions (Events). The logic "shouts" when something happens (`OnDamageReceived`), and the UI "listens." 

---

## 5. Areas for Growth (Your Next Tasks)
While the engine is robust, here is where we can improve for a "Triple-A" feel:
1. **Addressables**: Currently, we reference some assets directly. Converting to `Addressables` will reduce memory usage and allow for DLC/Patching.
2. **Object Pooling**: Our `AudioManager` uses basic pooling, but `VFX` and `Combatants` should also be pooled to avoid GC spikes.
3. **Custom Inspectors**: Our `DataImportWindow` is a good start. Expanding this with a visual **Node Graph Editor** for Dialogue would be a huge win for the Narrative team.
4. **Networking**: The engine is currently single-player. Decoupling the `TurnManager` further from local state would allow for a multi-player "Synched Turn" mode.

---
**Standard Operating Procedure (SOP):** 
1. **Interfaces First**: Never write a concrete system without a Core Interface.
2. **NUnit Tests**: If you change the math in `D20CombatResolver`, run the `CombatMathTests` immediately.
3. **Serialize Wisely**: Keep private fields private. Use `[SerializeField]`.

**Code with integrity. Build for the future.**
