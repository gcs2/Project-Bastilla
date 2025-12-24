// ============================================================================
// RPGPlatform.Systems.Dialogue - Dialogue Manager
// Core logic for running conversations
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RPGPlatform.Core;
using RPGPlatform.Core.Dialogue;
using RPGPlatform.Data;

namespace RPGPlatform.Systems.Dialogue
{
    public class DialogueManager : MonoBehaviour, IDialogueService
    {
        [Header("Services")]
        // Dependencies would usually be injected via ServiceLocator
        private IMoralityService _moralityService;
        private IQuestService _questService;
        private ISkillService _skillService;
        private IInfluenceService _influenceService;
        private IDialogueRepository _repository;

        [Header("State")]
        private ConversationData _currentConversation;
        private DialogueNode _currentNode;
        private ICombatant _player;
        private ICombatant _primarySpeaker;

        // Events
        public event Action<DialogueNode> OnNodeStart;
        public event Action OnConversationEnd;

        // Singleton for simple access in prototypes (ServiceLocator is better)
        public static DialogueManager Instance { get; private set; }

        public string CurrentConversationId => _currentConversation?.ConversationId ?? string.Empty;

        private void Awake()
        {
            Instance = this;
        }

        public void Initialize(IMoralityService morality, IQuestService quests, ISkillService skills, IInfluenceService influence, IDialogueRepository repository)
        {
            _moralityService = morality;
            _questService = quests;
            _skillService = skills;
            _influenceService = influence;
            _repository = repository ?? new ResourceDialogueRepository(); // Default fallback
        }

        public void StartConversation(string conversationId, ICombatant player, ICombatant speaker)
        {
            if (_repository == null) _repository = new ResourceDialogueRepository();
            
            var conversation = _repository.LoadConversation(conversationId);
            if (conversation == null)
            {
                Debug.LogError($"[DialogueManager] Conversation not found: {conversationId}");
                return;
            }

            StartConversation(conversation, player, speaker);
        }

        public void StartConversation(ConversationData data, ICombatant player, ICombatant speaker)
        {
            _currentConversation = data;
            _player = player;
            _primarySpeaker = speaker;
            
            Debug.Log($"[DialogueManager] Starting conversation '{data.ConversationId}' with {speaker.DisplayName}");
            
            SetNode(data.EntryNodeId);
        }

        private void SetNode(string nodeId)
        {
            if (_currentConversation == null) return;
            
            var node = _currentConversation.GetNode(nodeId);
            if (node == null)
            {
                EndConversation();
                return;
            }

            _currentNode = node;

            // Prepare Context
            var context = CreateContext();

            // Check for Auto responses (Interjections/Interrupts)
            foreach (var response in _currentNode.Responses)
            {
                if (response.DisplayType == ResponseDisplayType.Auto)
                {
                    if (EvaluateConditions(response.Conditions, context))
                    {
                        Debug.Log($"[DialogueManager] Auto-Interjection triggering: {response.Text}");
                        SetNode(response.NextNodeId);
                        return; // Immediate transition, do not show current node
                    }
                }
            }
            
            // Execute Node Effects
            foreach (var effect in _currentNode.Effects)
            {
                effect.Execute(context);
            }

            Debug.Log($"[DialogueManager] Node: {_currentNode.Text} (Speaker: {GetSpeakerName(node)})");
            OnNodeStart?.Invoke(_currentNode);
        }

        public void SelectResponse(int responseIndex)
        {
            if (_currentNode == null || responseIndex < 0 || responseIndex >= _currentNode.Responses.Count)
                return;

            var response = _currentNode.Responses[responseIndex];
            
            // Validate condition again just in case
            var context = CreateContext();
            if (!EvaluateConditions(response.Conditions, context))
            {
                Debug.LogWarning("Tried to select a locked response.");
                return;
            }

            Debug.Log($"[DialogueManager] Selected: {response.Text}");
            
            SetNode(response.NextNodeId);
        }
        
        /// <summary>
        /// Get valid responses for UI to display
        /// </summary>
        public List<DialogueResponse> GetValidResponses()
        {
            if (_currentNode == null) return new List<DialogueResponse>();
            
            var context = CreateContext();
            var valid = new List<DialogueResponse>();

            foreach (var response in _currentNode.Responses)
            {
                bool met = EvaluateConditions(response.Conditions, context);
                // IF display type is Hidden and not met, skip
                // IF display type is SkillCheck, maybe show grayed out?
                // For now, if met -> add. simple.
                if (met)
                {
                    valid.Add(response);
                }
            }
            return valid;
        }

        private void EndConversation()
        {
            Debug.Log("[DialogueManager] Conversation Ended");
            _currentConversation = null;
            _currentNode = null;
            OnConversationEnd?.Invoke();
        }

        private DialogueContext CreateContext()
        {
            var ctx = new DialogueContext
            {
                Player = _player,
                Morality = _moralityService
            };
            
            // Inject services into LocalState for conditions to find
            if (_questService != null) ctx.LocalState["QuestService"] = _questService;
            if (_skillService != null) ctx.LocalState["SkillService"] = _skillService;
            if (_influenceService != null) ctx.LocalState["InfluenceService"] = _influenceService;
            
            return ctx;
        }

        private bool EvaluateConditions(List<IDialogueCondition> conditions, DialogueContext context)
        {
            if (conditions == null || conditions.Count == 0) return true;
            foreach (var cond in conditions)
            {
                if (!cond.Evaluate(context)) return false;
            }
            return true;
        }

        private string GetSpeakerName(DialogueNode node)
        {
            if (!string.IsNullOrEmpty(node.SpeakerOverride))
                return node.SpeakerOverride;
            return _primarySpeaker != null ? _primarySpeaker.DisplayName : "Unknown";
        }
    }
}
