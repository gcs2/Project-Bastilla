# Unity Assistant & Prompt Engineering Guide

This guide provides the "Axiom Way" for communicating with the Unity AI Assistant for game content generation.

## 1. The Prompting Context
The Unity Assistant works best when you provide **Context -> Specifics -> Format**.

### The Context Pattern
Always frame your request within the Sun Eater universe:
> "In my sci-fi RPG (The Sun Eater), which uses a dark, grimy bio-architecture aesthetic, I need..."

## 2. Best Practices for Sun Eater Assets

### 2.1 Textures & Sprites
Avoid generic terms like "sci-fi wall". Use descriptive sensory language.
- **Resolution**: Aim for 1024x1024 (standard) or 512x512 for UI icons.
- **Style**: "Low-poly", "painterly", "photo-realistic", or "oil-rubbed".
- **Keywords**: "Grime", "Bioluminescence", "Corroded Metal", "Pulsing Veins".

### 2.2 Example Prompts
- **Merchant Stall**: "Generate a 2D isometric sprite of a sci-fi market stall. Materials: Weathered green tarp over a rusty steel frame. Details: Glowing neon tubes in teal, boxes of bioluminescent mushrooms in the corner. Style: Gritty cyberpunk."
- **Chantry Floor**: "Seamless PBR texture for a cathedral floor. Materials: Black obsidian tiles with veins of gold circuitry. Style: Hyper-detailed, worn from centuries of pilgrim footsteps."

## 3. Iterative Prompting
If the first result is "off", use the Assistant to refine:
1. "Make it darker and add more purple bioluminescence."
2. "Reduce the metal shine; make it look more organic and fleshy."

## 4. Integration
Once generated, save all assets to `Assets/AxiomEngine/GameSpecific/SunEater/Generated/`.
Refer to `PlayableDemoGuide.md` for how to link these to `CombatantData`.
