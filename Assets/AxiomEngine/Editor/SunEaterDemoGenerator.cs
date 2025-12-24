// ============================================================================
// RPGPlatform.Editor - Sun Eater Demo Generator
// "One-Click" Visual Spectacle & Gameplay Loop Initialization
// ============================================================================

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using RPGPlatform.Core;
using RPGPlatform.Core.Dialogue;
using RPGPlatform.Data;
using SunEater.Demo;
using System.IO;
using System.Collections.Generic;

namespace RPGPlatform.Editor
{
    public class SunEaterDemoGenerator : EditorWindow
    {
        [MenuItem("Axiom/[DEMO] Initialize Vorgossos Incursion")]
        public static void BuildDemo()
        {
            // 1. Create/Initialize Scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            scene.name = "SunEater_Vorgossos_Demo";

            // 2. Setup Sun Eater Data Directory
            string dataPath = "Assets/AxiomEngine/GameSpecific/SunEater/Data/Demo";
            Directory.CreateDirectory(dataPath);

            // 3. Generate Persistent Assets
            var ability = CreateAbility(dataPath);
            var quest = CreateQuest(dataPath);
            var dialogue = CreateDialogue(dataPath);
            var inquisitorData = CreateInquisitorData(dataPath, ability);

            // 4. Construct Environment
            GameObject root = new GameObject("[DEMO] Vorgossos Incursion");
            GameObject env = SetupEnvironment(root);
            
            // 5. Setup Combat & Dialogue Managers
            var combatManager = root.AddComponent<RPGPlatform.Systems.Combat.CombatManager>();
            var dialogueManager = root.AddComponent<RPGPlatform.Systems.Dialogue.DialogueManager>();
            var questManager = root.AddComponent<RPGPlatform.Systems.Quests.QuestManager>();

            // 6. Apply Visual Spectacle (URP)
            SetupVisualSpectacle(root);

            // 7. Spawn Characters
            var inquisitor = SpawnInquisitor(root, inquisitorData);
            var player = SpawnPlayer(root);

            // 8. Attach & Configure Bootstrapper
            var bootstrapper = root.AddComponent<PlayableDemoBootstrapper>();
            bootstrapper.Configure(player, inquisitor, dialogue, quest);
            
            Debug.Log("<color=cyan>[Axiom]</color> VISUAL SPECTACLE INITIALIZED: The Chantry has arrived on Vorgossos.");
            AssetDatabase.SaveAssets();
        }

        private static AbilityData CreateAbility(string path)
        {
            var asset = ScriptableObject.CreateInstance<AbilityData>();
            asset.AbilityId = "high_matter_sweep";
            asset.DisplayName = "High-Matter Sweep";
            asset.DamageFormula = "3d10+STR";
            asset.DamageType = DamageType.Energy;
            
            AssetDatabase.CreateAsset(asset, $"{path}/Ability_HighMatterSweep.asset");
            return asset;
        }

        private static QuestData CreateQuest(string path)
        {
            var asset = ScriptableObject.CreateInstance<QuestData>();
            asset.QuestId = "vorgossos_heretic";
            asset.DisplayName = "The Vorgossos Heretic";
            asset.XPReward = 2500;
            asset.Objectives.Add(new QuestObjectiveData { Description = "Decide the fate of the Chantry Inquisitor." });
            
            AssetDatabase.CreateAsset(asset, $"{path}/Quest_VorgossosHeretic.asset");
            return asset;
        }

        private static ConversationData CreateDialogue(string path)
        {
            var asset = ScriptableObject.CreateInstance<ConversationData>();
            asset.ConversationId = "inquisitor_spectacle";
            asset.EntryNodeId = "root";
            
            var root = new DialogueNode { NodeId = "root", Text = "Stop, Traveler. I sense the hum of forbidden silicon beneath your skin. The Chantry does not tolerate such... incompatibility." };
            var response1 = new DialogueResponse { Text = "I am a seeker of knowledge, not a heretic.", NextNodeId = "logic" };
            var response2 = new DialogueResponse { Text = "[Unsheathe High-Matter Sword] Then let the Silence take you.", NextNodeId = "combat" };
            root.Responses = new List<DialogueResponse> { response1, response2 };

            asset.Nodes = new List<DialogueNode> { root, new DialogueNode { NodeId = "logic", Text = "Knowledge is the path to ruin without Faith." }, new DialogueNode { NodeId = "combat", Text = "So be it. The Emperor's justice is swift." } };
            
            AssetDatabase.CreateAsset(asset, $"{path}/Dialogue_Inquisitor.asset");
            return asset;
        }

        private static CombatantData CreateInquisitorData(string path, AbilityData ability)
        {
            var asset = ScriptableObject.CreateInstance<CombatantData>();
            asset.BaseName = "Chantry Inquisitor";
            asset.BaseStats = new CombatStats { MaxHealth = 250, Strength = 18, Intelligence = 15 };
            // asset.Abilities.Add(ability); // Assuming CombatantData has a list of abilities
            
            AssetDatabase.CreateAsset(asset, $"{path}/Combatant_Inquisitor.asset");
            return asset;
        }

        private static GameObject SetupEnvironment(GameObject root)
        {
            GameObject env = new GameObject("Environment");
            env.transform.SetParent(root.transform);

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Vorgossos_Market_Floor";
            floor.transform.SetParent(env.transform);
            floor.transform.localScale = new Vector3(10, 1, 10);

            // Dark Violet Material
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", new Color(0.1f, 0.05f, 0.2f));
            floor.GetComponent<Renderer>().sharedMaterial = mat;

            return env;
        }

        private static void SetupVisualSpectacle(GameObject root)
        {
            // 1. Post-Processing Volume
            GameObject volObj = new GameObject("Vibe_Volume");
            volObj.transform.SetParent(root.transform);
            var vol = volObj.AddComponent<Volume>();
            vol.isGlobal = true;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "Vorgossos_Spectacle_Profile";
            
            var bloom = profile.Add<Bloom>();
            bloom.intensity.Override(5f);
            bloom.threshold.Override(1f);
            bloom.tint.Override(new Color(0f, 0.8f, 0.8f)); // Teal bloom

            var vignette = profile.Add<Vignette>();
            vignette.intensity.Override(0.45f);
            vignette.smoothness.Override(1f);

            AssetDatabase.CreateAsset(profile, "Assets/AxiomEngine/GameSpecific/SunEater/Data/Demo/Spectacle_Profile.asset");
            vol.sharedProfile = profile;

            // 2. Atmospheric Lighting
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.1f, 0.2f, 0.2f);
            RenderSettings.fogDensity = 0.05f;
            RenderSettings.ambientLight = new Color(0.05f, 0.05f, 0.1f);

            GameObject sun = GameObject.Find("Directional Light");
            if (sun != null)
            {
                sun.GetComponent<Light>().color = new Color(0.3f, 0.1f, 0.4f); // Purple sun
                sun.GetComponent<Light>().intensity = 0.5f;
            }
        }

        private static GameObject SpawnInquisitor(GameObject root, CombatantData data)
        {
            GameObject npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            npc.name = "NPC_Chantry_Inquisitor";
            npc.transform.SetParent(root.transform);
            npc.transform.position = new Vector3(0, 1, 8);
            
            var combatant = npc.AddComponent<RPGPlatform.Systems.Combat.Combatant>();
            combatant.Initialize(data, false, 1); // Team 1 = Enemy

            Material incMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            incMat.SetColor("_BaseColor", Color.black);
            incMat.SetColor("_EmissionColor", Color.red * 2f);
            incMat.EnableKeyword("_EMISSION");
            npc.GetComponent<Renderer>().sharedMaterial = incMat;

            // Add a backlight for silhouette
            GameObject backlight = new GameObject("Inquisitor_Backlight");
            backlight.transform.SetParent(npc.transform);
            backlight.transform.position = npc.transform.position + new Vector3(0, 2, 2);
            var l = backlight.AddComponent<Light>();
            l.color = new Color(0f, 1f, 1f); // Teal rim light
            l.intensity = 20f;
            l.range = 5f;

            return npc;
        }

        private static GameObject SpawnPlayer(GameObject root)
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player_Combatant";
            player.transform.SetParent(root.transform);
            player.transform.position = new Vector3(0, 1, 0);

            var combatant = player.AddComponent<RPGPlatform.Systems.Combat.Combatant>();
            var stats = new CombatStats { MaxHealth = 100, Strength = 14, Dexterity = 16 };
            combatant.Initialize(stats, new List<AbilityData>(), true, 0); // Team 0 = Player

            return player;
        }
    }
}
