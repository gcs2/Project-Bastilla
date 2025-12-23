// ============================================================================
// RPGPlatform.Systems.Dialogue - Repositories
// Concrete implementations of IDialogueRepository
// ============================================================================

using UnityEngine;
using RPGPlatform.Core.Dialogue;
using RPGPlatform.Data;

namespace RPGPlatform.Systems.Dialogue
{
    /// <summary>
    /// Loads conversations from the Resources folder.
    /// Default implementation.
    /// </summary>
    public class ResourceDialogueRepository : IDialogueRepository
    {
        private readonly string _basePath;

        public ResourceDialogueRepository(string basePath = "Conversations")
        {
            _basePath = basePath;
        }

        public ConversationData LoadConversation(string conversationId)
        {
            return Resources.Load<ConversationData>($"{_basePath}/{conversationId}");
        }
    }
}
