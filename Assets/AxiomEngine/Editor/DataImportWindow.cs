// ============================================================================
// Axiom RPG Engine - Data Import Window
// Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.
// ============================================================================

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using RPGPlatform.Systems.Combat;
using RPGPlatform.Data;

namespace RPGPlatform.Editor
{
    public class DataImportWindow : EditorWindow
    {
        private TextAsset _dataFile;
        private string _targetFolder = "Assets/GameData/Items";
        private DataType _dataType = DataType.Ability;

        public enum DataType { Ability, Quest }

        [MenuItem("Axiom Engine/Data Importer")]
        public static void ShowWindow()
        {
            GetWindow<DataImportWindow>("Axiom Data Importer");
        }

        private void OnGUI()
        {
            GUILayout.Label("Import Game Data", EditorStyles.boldLabel);
            
            _dataType = (DataType)EditorGUILayout.EnumPopup("Data Type", _dataType);
            _dataFile = (TextAsset)EditorGUILayout.ObjectField("Source File (JSON/CSV)", _dataFile, typeof(TextAsset), false);
            _targetFolder = EditorGUILayout.TextField("Target Folder", _targetFolder);

            if (GUILayout.Button("Process Import"))
            {
                RunImport();
            }
        }

        private void RunImport()
        {
            if (_dataFile == null)
            {
                Debug.LogError("[DataImporter] No source file selected.");
                return;
            }

            if (!Directory.Exists(_targetFolder))
            {
                Directory.CreateDirectory(_targetFolder);
            }

            string content = _dataFile.text;
            
            // Simplified: Assume JSON array for this prototype
            if (_dataType == DataType.Ability)
            {
                ImportAbilities(content);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Axiom Importer", "Import Complete!", "OK");
        }

        private void ImportAbilities(string json)
        {
            // Note: In production, use a robust JSON parser (Newtonsoft or Unity's JsonUtility with wrapper)
            // For this prototype, we mock the creation of one SO for demonstration
            // since Unity's JsonUtility doesn't support top-level arrays easily without a wrapper.
            
            Debug.Log("[DataImporter] Parsing Ability Data...");
            
            // Mocking a single asset creation based on the idea
            var newAbility = CreateInstance<AbilityData>();
            newAbility.DisplayName = "Imported Ability";
            newAbility.AbilityId = "imported_01";
            
            string path = $"{_targetFolder}/{newAbility.AbilityId}.asset";
            AssetDatabase.CreateAsset(newAbility, path);
            Debug.Log($"[DataImporter] Created asset at {path}");
        }
    }
}
