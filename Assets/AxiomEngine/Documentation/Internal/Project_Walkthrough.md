# Walkthrough - Axiom RPG Engine Project 🏛️

This walkthrough summarizes the full transformation of the project into the **Axiom RPG Engine**, a robust, genre-agnostic RPG platform.

## 1. Project Rebranding & Identity
- **New Name**: Axiom RPG Engine (formerly Sovereign).
- **Copyright**: (c) Geoffrey Salmon 2025.
- **Architecture**: Strict Clean Architecture with Layered Concerns.

## 2. Core Systems Implemented

### ⚔️ Combat Engine (Phase 5-7)
- **D20 Logic**: Implemented `D20CombatResolver` for real tabletop-style math (Attack rolls vs AC, Saving Throws, Criticals).
- **Status Effects**: A scalable `StatusEffectSystem` supporting ticking debuffs (Poison), buffs (Haste), and stat overrides.
- **Command Pattern**: All actions (Attack, UseAbility) are encapsulated as undoable commands.

### 🎭 Narrative & Dialogue (Phase 2)
- **DialogueManager**: Data-driven branching conversations with conditions (Morality, Skill Checks).
- **Repository Pattern**: Pluggable data sources for conversation trees.

### ⚖️ Morality & Progression (Phase 1, 3)
- **Generic Morality**: Configurable axes (Humanism vs. Transhumanism).
- **Logarithmic XP**: Professional scaling system for character growth.

### 🌍 Travel & Presentation (Phase 4, 8)
- **TravelManager**: Handles location prerequisites and scene transitions.
- **Presentation Layer**: `AudioManager` for spatial sound and music; UI Presenters to bind logic to TextMeshPro and Sliders.

## 3. "Vibe Coding" Pipeline (Phase 9)
- **LLM Data Guidelines**: A comprehensive schema for generating game content using AI.
- **Automated Tools**: `DataImportWindow` for CSV/JSON imports and `DialogueParser` for text scripts.

## 4. Final Verification: Vertical Slice (Phase 10)
Implemented the **Vorgossos Arrival** scenario:
1.  **Bootstrap**: Initializes Audio, Input, and UI layers.
2.  **Cutscene**: Mocked arrival sequence.
3.  **Dialogue**: Interaction with the "Gatekeeper" NPC.
4.  **Combat**: Seamless transition from dialogue into a D20-based combat encounter.

---
**The Axiom RPG Engine is now ready for full-scale development and content injection.**
