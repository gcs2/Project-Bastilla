// ============================================================================
// RPGPlatform.Core.Dialogue - Core Definitions
// Interfaces and Data Structures for the Dialogue System (TDD Section 5)
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using RPGPlatform.Core;
using RPGPlatform.Data;

namespace RPGPlatform.Core.Dialogue
{
    // ------------------------------------------------------------------------
    // Context & Interfaces
    // ------------------------------------------------------------------------

    /// <summary>
    /// Context passed to evaluating conditions
    /// </summary>
    public class DialogueContext
    {
        public ICombatant Player;
        public IMoralityService Morality;
        // Add Quest/Flag service interface here when available
        // public IQuestService Quests; 
        
        // Key-Value store for temporary conversation state
        public Dictionary<string, object> LocalState = new Dictionary<string, object>();
    }

    /// <summary>
    /// Interface for conditions that guard dialogue nodes or responses
    /// </summary>
    public interface IDialogueCondition
    {
        bool Evaluate(DialogueContext context);
        string GetFailureReason();
    }
    
    /// <summary>
    /// Interface for effects that run when a node is visited
    /// </summary>
    public interface IDialogueEffect
    {
        void Execute(DialogueContext context);
    }
    
    /// <summary>
    /// Main service interface for the Dialogue System
    /// </summary>
    public interface IDialogueService
    {
        void StartConversation(string conversationId, ICombatant player, ICombatant speaker);
        void SelectResponse(int responseIndex);
        event Action<DialogueNode> OnNodeStart;
        event Action OnConversationEnd;
    }

    /// <summary>
    /// Abstract the method of loading conversation data (Resources, Addressables, Database, etc.)
    /// </summary>
    public interface IDialogueRepository
    {
        ConversationData LoadConversation(string conversationId);
    }

    // ------------------------------------------------------------------------
    // Serializable Data Classes
    // ------------------------------------------------------------------------

    [Serializable]
    public class DialogueNode
    {
        public string NodeId;
        public string SpeakerID; // Added for compatibility
        public string SpeakerOverride;
        public string Text; // Simplified from LocalizedString for now
        public AudioClip VoiceOver;
        
        [SerializeReference] // Polymorphic serialization for ScriptableObjects/JSON
        public List<IDialogueCondition> Conditions = new List<IDialogueCondition>();
        
        [SerializeReference]
        public List<IDialogueEffect> Effects = new List<IDialogueEffect>();
        
        public List<DialogueResponse> Responses = new List<DialogueResponse>();
    }

    [Serializable]
    public class DialogueResponse
    {
        public string ResponseId;
        public string Text;
        public string NextNodeId;
        
        [SerializeReference]
        public List<IDialogueCondition> Conditions = new List<IDialogueCondition>();
        
        public ResponseDisplayType DisplayType = ResponseDisplayType.Normal;
    }

    public enum ResponseDisplayType
    {
        Normal,
        SkillCheck,
        AlignmentCheck,
        Hidden,
        Auto
    }
}
