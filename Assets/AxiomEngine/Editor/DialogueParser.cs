// ============================================================================
// Axiom RPG Engine - Dialogue Parser
// Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.
// ============================================================================

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using RPGPlatform.Core.Dialogue;

namespace RPGPlatform.Editor
{
    public class DialogueParser : EditorWindow
    {
        private TextAsset _scriptFile;
        private string _targetPath = "Assets/GameData/Dialogues";

        [MenuItem("Axiom Engine/Dialogue Parser")]
        public static void ShowWindow()
        {
            GetWindow<DialogueParser>("Dialogue Parser");
        }

        private void OnGUI()
        {
            GUILayout.Label("Parse Dialogue Script", EditorStyles.boldLabel);
            _scriptFile = (TextAsset)EditorGUILayout.ObjectField("Source Text", _scriptFile, typeof(TextAsset), false);
            _targetPath = EditorGUILayout.TextField("Target Folder", _targetPath);

            if (GUILayout.Button("Parse Script"))
            {
                ParseScript();
            }
        }

        private void ParseScript()
        {
            if (_scriptFile == null) return;

            string[] lines = _scriptFile.text.Split('\n');
            // Mock parsing logic: Look for [Speaker]: [Text]
            DEBUG_LogParsing(lines);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Axiom Parser", "Dialogue Script Parsed!", "OK");
        }

        private void DEBUG_LogParsing(string[] lines)
        {
            foreach (var line in lines)
            {
                if (line.Contains(":"))
                {
                    string[] parts = line.Split(':');
                    string speaker = parts[0].Trim();
                    string text = parts[1].Trim();
                    Debug.Log($"[DialogueParser] Found Node: {speaker} -> {text}");
                    
                    // In a real implementation, we would create a ScriptableObject here
                    // containing DialogueNode data.
                }
            }
        }
    }
}
