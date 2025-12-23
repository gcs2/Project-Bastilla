// ============================================================================
// Axiom RPG Engine - Dialogue Presenter
// Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.
// ============================================================================

using UnityEngine;
using UnityEngine.UI;
using RPGPlatform.Core.Dialogue;
using RPGPlatform.UI;

namespace RPGPlatform.UI.Presenters
{
    public class DialoguePresenter : MonoBehaviour
    {
        [Header("Logic Controller")]
        [SerializeField] private DialogueViewController _controller;

        [Header("UI References")]
        [SerializeField] private GameObject _dialoguePanel;
        [SerializeField] private Text _speakerNameText;
        [SerializeField] private Text _dialogueText;
        [SerializeField] private Transform _buttonContainer;
        [SerializeField] private GameObject _choiceButtonPrefab;

        private void Start()
        {
            if (_controller != null)
            {
                _controller.OnShowNode += DisplayNode;
                _controller.OnShowOptions += ShowChoices;
                _controller.OnEndConversation += CloseDialogue;
            }
            
            _dialoguePanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_controller != null)
            {
                _controller.OnShowNode -= DisplayNode;
                _controller.OnShowOptions -= ShowChoices;
                _controller.OnEndConversation -= CloseDialogue;
            }
        }

        private void DisplayNode(string speaker, string text)
        {
            _dialoguePanel.SetActive(true);
            if (_speakerNameText != null) _speakerNameText.text = speaker;
            if (_dialogueText != null) _dialogueText.text = text;
            
            Debug.Log($"[UI] Narrative: {speaker} says '{text}'");
        }

        private void ShowChoices(DialogueResponse[] options)
        {
            // Clear old buttons
            foreach (Transform child in _buttonContainer)
            {
                Destroy(child.gameObject);
            }

            // Create new buttons
            for (int i = 0; i < options.Length; i++)
            {
                int index = i;
                var buttonGO = Instantiate(_choiceButtonPrefab, _buttonContainer);
                var btn = buttonGO.GetComponent<Button>();
                var txt = buttonGO.GetComponentInChildren<Text>();
                
                if (txt != null) txt.text = options[i].Text;
                if (btn != null) btn.onClick.AddListener(() => _controller.SelectOption(index));
            }
        }

        private void CloseDialogue()
        {
            _dialoguePanel.SetActive(false);
        }
    }
}
