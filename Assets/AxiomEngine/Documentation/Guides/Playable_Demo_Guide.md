# 🎮 Axiom RPG Engine: Playable Demo Guide
**Target Scenario:** Vorgossos Arrival Vertical Slice

This guide provides every detail you need to go from a code folder to a playable executable using "Vibe Coding."

---

## 🛠️ Step 1: Environment Setup
1. **Download Unity Hub**: Go to [unity.com](https://unity.com/download) and install Unity Hub.
2. **Install Unity 6**: Inside Unity Hub, go to **Installs > Install Editor** and select **Unity 6 (6000.0.x LTS)**.
3. **Download VS Code Extensions**: Ensure you have the `C# Dev Kit` and `Unity` extensions installed in VS Code.

---

## 🏗️ Step 2: Project Initialization & Engine Foundation
### How the Engine Works in Unity
Unity treats C# scripts in the `Assets/` folder as the "Foundation." On import, Unity compiles all `.cs` files into an assembly. You then "instantiate" the engine by attaching these scripts to **GameObjects**.

### Import Every System (Full Encapsulation)
Instead of just individual folders, you should copy the **entire source tree**:
1. **Create Source Folder**: In your Unity Project, create `Assets/AxiomEngine/`.
2. **Copy All Code**: Copy everything from the current `CombatSystem/CombatSystem/` directory (which contains `Core`, `Systems`, `Data`, `UI`, and `Editor`) into `Assets/AxiomEngine/`.
3. **Unity Compilation**: Once copied, Unity will take a few seconds to compile. If you see the **Axiom Engine** menu item at the top of your editor, the foundation is active!

---

## 🎨 Step 3: Visuals & Aesthetics (Making it "Real")
You don't need to be an artist to make the demo look great.
1. **The Whitebox**: Use Unity's `GameObject > 3D Object > Cube/Plane` to build the "Vorgossos Gate."
2. **ProBuilder (Recommended)**: Go to **Window > Package Manager**, search for **ProBuilder**, and install it. This allows you to "model" simple buildings and stairs directly in Unity.
3. **Asset Store (Free Graphics)**: Go to the [Unity Asset Store](https://assetstore.unity.com/) and search for "Free Sci-Fi Starter" or "Stylized Fantasy" to get high-quality character models and environment textures.
4. **Materials**: Create a new Material (`Right-Click > Create > Material`) to give your cubes color, glow, or metallic sheen.

---

## 🪄 Step 4: Vibe Coding (Content Generation)
Use these prompts with your preferred LLM to generate the data for the demo.

### Prompt A: The Narrative (Dialogue)
> "Act as a Narrative Designer for the Axiom RPG Engine. Using the schema in `LLM_DATA_GUIDELINE.md`, generate a dialogue script for the 'Vorgossos Arrival'. 
> - **Speaker**: Chantry Gatekeeper (Cold, suspicious).
> - **Player Choice 1**: [Diplomacy] 'I come on urgent business from the Emperor.' (DC 12 Charisma check).
> - **Player Choice 2**: [Hostility] 'Step aside or I remove you.' (Triggers Combat).
> - **Format**: Text script compatible with the Axiom `DialogueParser`."

---

## 🚀 Step 5: Creating the Executable
1. **Build Settings**: Go to **File > Build Settings > Add Open Scenes**.
2. **Target**: Select your OS (Windows/Mac).
3. **Build**: Click **Build**. Your engine logic, your visuals, and your Vibe-Coded content are now a standalone game!
