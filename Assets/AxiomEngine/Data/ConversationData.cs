// ============================================================================
// RPGPlatform.Data - Conversation Data
// ScriptableObject definition for storing dialogue trees
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using RPGPlatform.Core.Dialogue;

namespace RPGPlatform.Data
{
    [CreateAssetMenu(fileName = "NewConversation", menuName = "RPG/Dialogue/Conversation")]
    public class ConversationData : ScriptableObject
    {
        [Tooltip("Unique ID for this conversation")]
        public string ConversationId;

        [Tooltip("Default speaker ID (can be overridden per node)")]
        public string SpeakerId;

        [Tooltip("ID of the starting node")]
        public string EntryNodeId;

        [Tooltip("All nodes in this conversation")]
        public List<DialogueNode> Nodes = new List<DialogueNode>();

        /// <summary>
        /// Helper to get a node by ID
        /// </summary>
        public DialogueNode GetNode(string nodeId)
        {
            return Nodes.Find(n => n.NodeId == nodeId);
        }
    }
}
