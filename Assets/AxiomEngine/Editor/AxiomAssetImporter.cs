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
            
            string path = $"Assets/AxiomEngine/GameSpecific/SunEater/Data/Locations/{newLocation.LocationId}.asset";
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            
            AssetDatabase.CreateAsset(newLocation, path);
            
            // 2. Create a basic scene hierarchy if in an open scene
            GameObject root = new GameObject($"[LOCATION] {locationName}");
            new GameObject("Environment").transform.SetParent(root.transform);
            new GameObject("NPCs").transform.SetParent(root.transform);
            new GameObject("Triggers").transform.SetParent(root.transform);
            
            Debug.Log($"[Axiom] Created level template for {locationName} at {path}");
            
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newLocation;
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
