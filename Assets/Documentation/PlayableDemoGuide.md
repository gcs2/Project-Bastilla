# Sun Eater Playable Demo Guide

Welcome to the Sun Eater development guide. This document details how to use the Axiom engine to build a KOTOR-inspired RPG set in the Sun Eater universe.

## 1. Project Setup

### Unity Configuration
- **Render Pipeline**: Ensure you are using the **Universal Render Pipeline (URP)**.
- **Packages**: Install the following from the Package Manager:
    - **Cinemachine** (for cinematic cameras)
    - **Input System** (for modern controls)
    - **ProBuilder** (for rapid environment whiteboxing)

### Axiom Engine Integration
The engine is located in `Assets/AxiomEngine`. All your game-specific data should reside in `Assets/AxiomEngine/GameSpecific/SunEater`.

---

## 2. Automation Workflow (Vibe Coding)

### Environment Generation (Unity Muse / ProBuilder)
Instead of manual modeling, use the following "chunks" to generate your environment assets.

#### Prompt: Vorgossos Marketplace
> "Low-poly sci-fi marketplace props, organic bio-architecture mixed with corroded metal, bioluminescent market stalls, dark greens and purples, grime-covered textures, Unity URP compatible."

#### Integration Step:
1. Open the **Axiom Asset Importer** (`Axiom > Generate Level Template`).
2. Paste the generated assets into the `Generated` folder.
3. Apply the `SunEater_Theme` materials.

### Character Generation (Rosebud AI / MetaHuman)
#### Prompt: Chantry Inquisitor
> "Humanoid character, monastic black robes with crimson trim, cybernetic eye, stern expression, rigged for Unity Humanoid skeleton."

#### Prompt: Palatine Elite
> "Elegant tall humanoid, chrome-plated cybernetic limbs, translucent flowing cape, aristocratic bearing, high-detail sci-fi armor."

---

## 3. Game Systems Architecture

### Alignment: Humanism vs. Transhumanism
The Sun Eater demo uses a dual-axis morality system:
- **Humanism**: Adherence to the Chantry's laws, preservation of the human form.
- **Transhumanism**: Forbidden tech, radical augmentations, power at any cost.

### Combat: High-Matter Swords
Combat is turn-based d20. 
- **Weapon Stats**: High-matter swords use the `Energy` damage type and scale with `Finesse`.
- **Abilities**: Use the `SunEater_Abilities` ScriptableObjects to define neural-link powers.

---

## 4. Next Steps
1. Run the `Axiom > Generate Level Template` command to create your first scene.
2. Use the prompts above to populate the scene with AI-generated assets.
3. Hook up the `DialogueManager` to a `ConversationData` asset for your first NPC interaction.
