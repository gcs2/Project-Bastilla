// ============================================================================
// RPGPlatform.Editor - Axiom Asset Importer
// Automates the creation of level templates and asset ingestion for Sun Eater
// ============================================================================

using UnityEngine;
using UnityEditor;
using RPGPlatform.Data;
using System.IO;

namespace RPGPlatform.Editor
{
    public class AxiomAssetImporter : EditorWindow
    {
        private string locationName = "New Vorgossos Area";
        private EnvironmentProfile environmentProfile;
        private GameObject npcPrefab;
        private string promptText = "Generate sci-fi marketplace props...";

        [MenuItem("Axiom/Generate Level Template")]
        public static void ShowWindow()
        {
            GetWindow<AxiomAssetImporter>("Axiom Importer");
        }

        private void OnGUI()
        {
            GUILayout.Label("Sun Eater Level Generator", EditorStyles.boldLabel);
            
            locationName = EditorGUILayout.TextField("Location Name", locationName);
            environmentProfile = (EnvironmentProfile)EditorGUILayout.ObjectField("Environment Profile", environmentProfile, typeof(EnvironmentProfile), false);
            npcPrefab = (GameObject)EditorGUILayout.ObjectField("NPC Prefab (Inquisitor)", npcPrefab, typeof(GameObject), false);

            if (environmentProfile == null)
            {
                EditorGUILayout.HelpBox("Assign an Environment Profile to enable modular generation!", MessageType.Info);
            }
            
            GUILayout.Space(10);
            GUILayout.Label("AI Prompt Helper (Copy/Paste to Muse/Rosebud)", EditorStyles.miniLabel);
            promptText = EditorGUILayout.TextArea(promptText, GUILayout.Height(100));

            if (GUILayout.Button("Create Level Template"))
            {
                CreateTemplate();
            }
        }

        private void CreateTemplate()
        {
            // 1. Create LocationData ScriptableObject
            LocationData newLocation = ScriptableObject.CreateInstance<LocationData>();
            newLocation.LocationId = locationName.ToLower().Replace(" ", "_");
            newLocation.DisplayName = locationName;
            
            string dataPath = $"Assets/AxiomEngine/GameSpecific/SunEater/Data/Locations/{newLocation.LocationId}.asset";
            Directory.CreateDirectory(Path.GetDirectoryName(dataPath));
            AssetDatabase.CreateAsset(newLocation, dataPath);
            
            // 2. Create Scene Hierarchy
            GameObject root = new GameObject($"[LOCATION] {locationName}");
            GameObject env = new GameObject("Environment");
            env.transform.SetParent(root.transform);
            
            // 3. Automated Aesthetic: The "Vibe Floor"
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "MarketFloor_Auto";
            floor.transform.SetParent(env.transform);
            floor.transform.localScale = environmentProfile != null ? environmentProfile.FloorScale : new Vector3(5, 1, 5);

            // Try to find the AI-generated texture
            Texture2D floorTex = environmentProfile != null && environmentProfile.FloorTexture != null 
                ? environmentProfile.FloorTexture 
                : AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AxiomEngine/GameSpecific/SunEater/Data/VibeFloor_Diffuse.png");

            if (floorTex != null)
            {
                Material floorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                floorMat.mainTexture = floorTex;
                floorMat.SetColor("_BaseColor", environmentProfile != null ? environmentProfile.FloorTint : new Color(0.6f, 0.8f, 0.7f));
                
                string matPath = $"Assets/AxiomEngine/GameSpecific/SunEater/Data/{locationName}_Floor.mat";
                AssetDatabase.CreateAsset(floorMat, matPath);
                floor.GetComponent<Renderer>().sharedMaterial = floorMat;
            }

            // 4. Cinematic Lighting
            GameObject lightGrid = new GameObject("Vibe_Lighting");
            lightGrid.transform.SetParent(root.transform);
            
            GameObject tealLight = new GameObject("PrimaryVibeLight");
            tealLight.transform.SetParent(lightGrid.transform);
            tealLight.transform.position = new Vector3(0, 10, 0);
            Light l1 = tealLight.AddComponent<Light>();
            l1.type = LightType.Spot;
            l1.color = environmentProfile != null ? environmentProfile.PrimaryLightColor : new Color(0f, 0.8f, 0.8f);
            l1.intensity = environmentProfile != null ? environmentProfile.LightIntensity : 50f;
            l1.range = environmentProfile != null ? environmentProfile.LightRange : 30f;
            l1.spotAngle = 60f;

            // 5. Modular Props
            int sCount = environmentProfile != null ? environmentProfile.StallCount : 3;
            float sSpread = environmentProfile != null ? environmentProfile.StallSpread : 8f;

            for (int i = 0; i < sCount; i++)
            {
                GameObject stall;
                if (environmentProfile != null && environmentProfile.StallPrefab != null)
                    stall = (GameObject)PrefabUtility.InstantiatePrefab(environmentProfile.StallPrefab);
                else
                    stall = GameObject.CreatePrimitive(PrimitiveType.Cube);

                stall.name = $"Market_Prop_{i}";
                stall.transform.SetParent(env.transform);
                stall.transform.position = new Vector3(Mathf.Sin(i * (6.28f / sCount)) * sSpread, 1.5f, Mathf.Cos(i * (6.28f / sCount)) * sSpread);
                stall.transform.localScale = new Vector3(3, 3, 3);
                
                if (floor.GetComponent<Renderer>().sharedMaterial != null && environmentProfile?.StallPrefab == null)
                    stall.GetComponent<Renderer>().sharedMaterial = floor.GetComponent<Renderer>().sharedMaterial;
            }

            // 6. Character Injection
            if (npcPrefab != null)
            {
                GameObject npc = (GameObject)PrefabUtility.InstantiatePrefab(npcPrefab);
                npc.name = "NPC_Inquisitor";
                npc.transform.SetParent(root.transform);
                npc.transform.position = new Vector3(2, 0, 5); // Facing player
                npc.transform.rotation = Quaternion.Euler(0, 180, 0);
            }

            // 7. Demo Bootstrapper
            var bootstrapper = root.AddComponent<SunEater.Demo.PlayableDemoBootstrapper>();
            // Auto-assign player search logic could go here

            Debug.Log($"[Axiom] LEVEL BOOTSTRAP COMPLETE: {locationName}");
            Debug.Log($"Generated data at: {dataPath}");
            
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = root;
        }

        [MenuItem("Axiom/Setup Sun Eater Genre")]
        public static void SetupSunEater()
        {
            // Create GenreProfile
            GenreProfile sunEaterProfile = ScriptableObject.CreateInstance<GenreProfile>();
            sunEaterProfile.GenreId = "suneater";
            sunEaterProfile.PrimaryColor = new Color(0.1f, 0.8f, 0.4f); // Emerald green for Sun Eater
            
            string profilePath = "Assets/AxiomEngine/GameSpecific/SunEater/Data/SunEater_GenreProfile.asset";
            Directory.CreateDirectory(Path.GetDirectoryName(profilePath));
            AssetDatabase.CreateAsset(sunEaterProfile, profilePath);

            // Create Morality Axes
            MoralityAxisConfig humanism = ScriptableObject.CreateInstance<MoralityAxisConfig>();
            humanism.AxisId = "humanism";
            humanism.DisplayName = "Faith vs Heresy";
            humanism.PositivePoleLabel = "Humanist";
            humanism.NegativePoleLabel = "Transhumanist";
            
            string moralityPath = "Assets/AxiomEngine/GameSpecific/SunEater/Data/Morality_Humanism.asset";
            AssetDatabase.CreateAsset(humanism, moralityPath);

            Debug.Log("[Axiom] Sun Eater Genre and Morality Axes have been initialized.");
            AssetDatabase.SaveAssets();
        }
    }
}
