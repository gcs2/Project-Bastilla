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

### Environment Generation (Unity AI / Muse)
Unity AI (which includes Muse) is your primary tool for environment props and sprites.
1. **Open Assistant**: In Unity, go to `Window > AI > Assistant`.
2. **Interact**: Type your request (e.g., "Generate a sprite for a sci-fi market stall") or use the specific Muse Texture/Sprite generators if separate windows exist.
3. **Save to Project**: Drag generated assets into `Assets/AxiomEngine/GameSpecific/SunEater/Generated`.

#### Prompt: Vorgossos Marketplace
> "Low-poly sci-fi marketplace props, organic bio-architecture mixed with corroded metal, bioluminescent market stalls, dark greens and purples, grime-covered textures, Unity URP compatible."

### Character Generation (Rosebud AI)
Rosebud AI (rosebud.ai) handles character fabrication, rigging, and basic animations.
1. **Visit rosebud.ai**: Log in to their web portal. (Check pricing; they usually offer a free tier with limited credits).
2. **Text-to-Character**: Use their "Fabricator" tool and paste the character prompt.
3. **Rigging & Export**: Select **FBX** as the format and **Unity Humanoid** as the rig type before downloading.
4. **Import into Unity**: 
   - Drag the `.fbx` file into `Assets/AxiomEngine/GameSpecific/SunEater/Data/NPCs`.
   - In the FBX's **Inspector**, go to the **Rig** tab and set **Animation Type** to `Humanoid`. Hit **Apply**.
5. **Axiom Linking**: Open your `NPC_Inquisitor` asset (`CombatantData`) and drag the imported character prefab into the **Prefab** slot.

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
