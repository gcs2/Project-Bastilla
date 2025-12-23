# Axiom RPG Engine: LLM Data Guidelines 🤖📜

To enable "Vibe Coding" and rapid content generation, use the following schemas when prompting an LLM (like Gemini, Claude, or ChatGPT) to generate game data for the Axiom engine.

---

## 1. Dialogue Schema (Text Script)
The Axiom `DialogueParser` reads simplified text scripts.
**LLM Prompt Requirement**: 
"Generate a dialogue script for the Axiom RPG Engine using the following format:
- `[NodeID]`: Unique identifier.
- `Speaker`: Name of the NPC.
- `Text`: The dialogue text.
- `Options`: comma-separated list of `[Text] -> [TargetNodeID]`.
- `Condition` (Optional): `Morality:[Axis]:[Value]` or `Skill:[Type]:[DC]`."

**Example Logic**:
```text
[Vorgossos_Arrival]
Speaker: Gatekeeper
Text: Halt. State your business at the Chantry core. 
Options: [Diplomacy] We seek council. -> [Council_Request], [Threat] Move or die. -> [Hostile_Start]
Conditions: [Diplomacy] Morality:Humanism:20, [Threat] Skill:Intimidate:15
```

---

## 2. Ability Schema (JSON)
**LLM Prompt Requirement**:
"Generate an Axiom Ability in JSON format:
```json
{
  "AbilityId": "fireball_01",
  "DisplayName": "Neural Overheat",
  "BaseDamage": 15,
  "DamageType": "Energy",
  "ResourceCost": 20,
  "Range": 10.0,
  "AppliedEffects": ["burning_status"]
}
```

---

## 3. Best Practices for Vibe Coding
1. **Context Injection**: Always upload `SYSTEM_RULES.md` and `CoreInterfaces.cs` to the LLM's context window.
2. **Batch Generation**: Ask for "5 variations of low-level Chantry soldiers" to get diverse stat blocks.
3. **Validation**: Use the `Axiom > Data Importer` window in Unity to verify the generated JSON structure.

---
**Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.**
