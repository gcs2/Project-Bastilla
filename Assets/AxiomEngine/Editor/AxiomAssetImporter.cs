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
            floor.transform.localScale = new Vector3(5, 1, 5); // 50m x 50m

            // Try to find the AI-generated texture
            Texture2D floorTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AxiomEngine/GameSpecific/SunEater/Data/New Texture.png");
            if (floorTex != null)
            {
                Material floorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                floorMat.mainTexture = floorTex;
                floorMat.SetColor("_BaseColor", new Color(0.6f, 0.8f, 0.7f)); // Tint for extra vibe
                
                string matPath = $"Assets/AxiomEngine/GameSpecific/SunEater/Data/{locationName}_Floor.mat";
                AssetDatabase.CreateAsset(floorMat, matPath);
                floor.GetComponent<Renderer>().sharedMaterial = floorMat;
            }

            // 4. Cinematic Lighting (Sun Eater Palette)
            GameObject lightGrid = new GameObject("Vibe_Lighting");
            lightGrid.transform.SetParent(root.transform);
            
            // Teal Spotlight
            GameObject tealLight = new GameObject("Light_Teal_Bloom");
            tealLight.transform.SetParent(lightGrid.transform);
            tealLight.transform.position = new Vector3(0, 10, 0);
            Light l1 = tealLight.AddComponent<Light>();
            l1.type = LightType.Spot;
            l1.color = new Color(0f, 0.8f, 0.8f);
            l1.intensity = 50f;
            l1.range = 30f;
            l1.spotAngle = 60f;

            // 5. Primitive Market Stalls (Structural Vibe)
            for (int i = 0; i < 3; i++)
            {
                GameObject stall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stall.name = $"Market_Stall_{i}";
                stall.transform.SetParent(env.transform);
                stall.transform.position = new Vector3(Mathf.Sin(i * 2) * 8, 1.5f, Mathf.Cos(i * 2) * 8);
                stall.transform.localScale = new Vector3(3, 3, 3);
                
                if (floor.GetComponent<Renderer>().sharedMaterial != null)
                    stall.GetComponent<Renderer>().sharedMaterial = floor.GetComponent<Renderer>().sharedMaterial;
            }

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
