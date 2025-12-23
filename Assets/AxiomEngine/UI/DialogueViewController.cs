// ============================================================================
// Axiom RPG Engine - Dialogue View Controller
// Copyright (c) Geoffrey Salmon 2025. All Rights Reserved.
// ============================================================================

using UnityEngine;
using System;
using RPGPlatform.Core.Dialogue;
using RPGPlatform.Systems.Dialogue;

namespace RPGPlatform.UI
{
    public class DialogueViewController : MonoBehaviour
    {
        private IDialogueService _dialogueService;

        // View Events
        public event Action<string, string> OnShowNode; // Speaker, Text
        public event Action<DialogueResponse[]> OnShowOptions;
        public event Action OnEndConversation;

        public void Initialize(IDialogueService service)
        {
            _dialogueService = service;

            if (_dialogueService != null)
            {
                _dialogueService.OnNodeStart += HandleNodeStart;
                _dialogueService.OnConversationEnd += HandleConversationEnd;
            }
        }

        private void OnDestroy()
        {
            if (_dialogueService != null)
            {
                _dialogueService.OnNodeStart -= HandleNodeStart;
                _dialogueService.OnConversationEnd -= HandleConversationEnd;
            }
        }

        private void HandleNodeStart(DialogueNode node)
        {
            // Update Text
            OnShowNode?.Invoke(node.SpeakerID, node.Text);

            // Update Options
            // (Assuming node.Responses deals with availability logic internal or here)
            // The DialogueManager usually filters responses.
            // But DialogueNode struct usually has all of them.
            // Let's pass them to UI which handles rendering based on their state (if implementation allows)
            // Or usually the Service provides "CurrentResponses".
            // Checking DialogueDefinitions.cs... Response logic is inside DialogueManager selecting valid ones.
            // But OnNodeStart passes the raw node.
            // A robust system would pass the "Valid Responses" separately or Node would contain them.
            // For prototype, we pass node.Responses.
            
            if (node.Responses != null)
            {
                OnShowOptions?.Invoke(node.Responses.ToArray());
            }
        }

        private void HandleConversationEnd()
        {
            OnEndConversation?.Invoke();
        }
        
        // Input Bridge
        public void SelectOption(int index)
        {
            _dialogueService?.SelectResponse(index);
        }
    }
}
