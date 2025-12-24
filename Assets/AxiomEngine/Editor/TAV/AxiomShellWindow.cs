// ============================================================================
// Axiom RPG Engine - Axiom Shell Editor Window
// The official "Text Adventure" interface for logic verification.
// ============================================================================

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using RPGPlatform.Editor.TAV;

namespace RPGPlatform.Editor.TAV
{
    public class AxiomShellWindow : EditorWindow
    {
        private string _currentCommand = "";
        private List<string> _log = new List<string>();
        private Vector2 _scrollPosition;
        private GUIStyle _logStyle;
        private bool _autoScroll = true;

        [MenuItem("Axiom Engine/Axiom Shell (TAV)")]
        public static void ShowWindow()
        {
            var window = GetWindow<AxiomShellWindow>("Axiom Shell");
            window.minSize = new Vector2(500, 400);
            AxiomShell.Initialize();
        }

        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
            if (AxiomShell.Dialogue == null) AxiomShell.Initialize();
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (logString.StartsWith("[TAV]") || logString.StartsWith("[AxiomShell]"))
            {
                _log.Add(logString);
                Repaint();
            }
        }

        private void OnGUI()
        {
            InitStyles();

            // Header
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("AXIOM SHELL - TEXT ADVENTURE VERIFICATION", EditorStyles.boldLabel);
            GUILayout.Label("Verification Mode: VORGOSSOS INCURSION", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();

            // Log Area
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true));
            foreach (var entry in _log)
            {
                var color = Color.white;
                if (entry.Contains("FAILURE") || entry.Contains("ERR")) color = Color.red;
                if (entry.Contains("SUCCESS") || entry.Contains("VICTORY")) color = Color.green;
                if (entry.Contains("COMBAT")) color = Color.yellow;
                
                var entryStyle = new GUIStyle(_logStyle);
                entryStyle.normal.textColor = color;
                
                EditorGUILayout.LabelField(entry, entryStyle);
            }
            if (_autoScroll && Event.current.type == EventType.Repaint)
            {
                _scrollPosition.y = float.MaxValue;
            }
            EditorGUILayout.EndScrollView();

            // Input Area
            EditorGUILayout.BeginHorizontal();
            GUI.SetNextControlName("CommandInput");
            _currentCommand = EditorGUILayout.TextField(_currentCommand, GUILayout.ExpandWidth(true));
            
            if (GUILayout.Button("SEND", GUILayout.Width(60)) || (Event.current.isKey && Event.current.keyCode == KeyCode.Return))
            {
                ExecuteCommand();
            }
            EditorGUILayout.EndHorizontal();

            // Footer / Toolbar
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Clear Log", EditorStyles.toolbarButton)) _log.Clear();
            if (GUILayout.Button("Reset Harness", EditorStyles.toolbarButton)) AxiomShell.Initialize();
            if (GUILayout.Button("Run Vorgossos Script", EditorStyles.toolbarButton)) VorgossosScenario.Run();
            _autoScroll = GUILayout.Toggle(_autoScroll, "Auto-Scroll", EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();

            // Focus on input
            if (Event.current.type == EventType.Layout)
            {
                GUI.FocusControl("CommandInput");
            }
        }

        private void InitStyles()
        {
            if (_logStyle == null)
            {
                _logStyle = new GUIStyle(EditorStyles.label);
                _logStyle.wordWrap = true;
                _logStyle.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/AxiomEngine/UI/Fonts/Consolas.ttf"); // Fallback to default if not found
                _logStyle.fontSize = 12;
            }
        }

        private void ExecuteCommand()
        {
            if (string.IsNullOrWhiteSpace(_currentCommand)) return;
            
            _log.Add($"> {_currentCommand}");
            AxiomShell.Execute(_currentCommand);
            _currentCommand = "";
            GUI.FocusControl("CommandInput");
        }
    }
}
