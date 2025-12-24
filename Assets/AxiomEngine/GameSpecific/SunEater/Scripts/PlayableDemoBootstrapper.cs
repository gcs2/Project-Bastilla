// ============================================================================
// SunEater.Demo - Playable Demo Bootstrapper
// Orchestrates the "Vorgossos Incursion" gameplay slice
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using RPGPlatform.Core;
using RPGPlatform.Core.Dialogue;
using RPGPlatform.Systems.Combat;
using RPGPlatform.Systems.Dialogue;
using RPGPlatform.Systems.Quests;
using RPGPlatform.Systems.Morality;
using RPGPlatform.Data;

namespace SunEater.Demo
{
    public class PlayableDemoBootstrapper : MonoBehaviour
    {
        [Header("Scene Configuration")]
        [SerializeField] private GameObject _playerObj;
        [SerializeField] private GameObject _inquisitorObj;

        [Header("Demo Data")]
        [SerializeField] private ConversationData _introDialogue;
        [SerializeField] private QuestData _vorgossosQuest;

        private Combatant _player;
        private Combatant _inquisitor;
        private DialogueManager _dialogue;
        private CombatManager _combat;
        private QuestManager _quest;

        public void Configure(GameObject player, GameObject inquisitor, ConversationData dialogue, QuestData quest)
        {
            _playerObj = player;
            _inquisitorObj = inquisitor;
            _introDialogue = dialogue;
            _vorgossosQuest = quest;
        }

        private void Start()
        {
            Initialize();
            
            // Auto-start the demo loop after a brief delay
            Invoke(nameof(StartInquisitorDialogue), 2f);
        }

        public void Initialize()
        {
            _player = _playerObj.GetComponent<Combatant>();
            _inquisitor = _inquisitorObj.GetComponent<Combatant>();
            _dialogue = GetComponent<DialogueManager>() ?? FindFirstObjectByType<DialogueManager>();
            _combat = GetComponent<CombatManager>() ?? FindFirstObjectByType<CombatManager>();
            _quest = GetComponent<QuestManager>() ?? FindFirstObjectByType<QuestManager>();

            // Ensure Morality is registered
            if (!ServiceLocator.IsRegistered<IMoralityService>())
                ServiceLocator.Register<IMoralityService>(new MoralityState());

            // Initialize Managers
            _quest.Initialize(null, new List<QuestData> { _vorgossosQuest });
            ServiceLocator.Register<IQuestService>(_quest);

            _dialogue.Initialize(ServiceLocator.Get<IMoralityService>(), _quest, null, null, null);
            ServiceLocator.Register<IDialogueService>(_dialogue);
            _dialogue.OnConversationEnd += OnDialogueEnded;
            _dialogue.OnNodeStart += OnDialogueNodeStart;

            Debug.Log("[PlayableDemo] Sun Eater Systems Synchronized.");
        }

        private void StartInquisitorDialogue()
        {
            Debug.Log("[PlayableDemo] Inquisitor silhouettes against the toxic sky...");
            _dialogue.StartConversation(_introDialogue, null, (ICombatant)_inquisitor);
        }

        private void OnDialogueNodeStart(DialogueNode node)
        {
            Debug.Log($"<color=red><b>INQUISITOR:</b></color> {node.Text}");
        }

        public void OnDialogueEnded()
        {
            // Simple logic: if we hit the 'combat' node, we fight. 
            // In a real system, we'd check node flags.
            // For now, let's trigger based on the last node's content or external state.
            // Assuming the simple intro dialogue structure:
            if (_dialogue.CurrentConversationId == "inquisitor_spectacle")
            {
                // We'll simulate checking for the combat path
                // For the demo, we'll just start combat if they chose the confrontational path
                TriggerCombatEncounter();
            }
        }

        private void TriggerCombatEncounter()
        {
            Debug.Log("<color=red>[PlayableDemo]</color> BATTLE JOINED: The Chantry's blade hums with energy.");
            
            var combatants = new List<ICombatant> { (ICombatant)_player, (ICombatant)_inquisitor };
            _combat.StartCombat(combatants);

            // Hook into victory
            _combat.OnCombatEnded += (victory) => 
            {
                if (victory)
                {
                    Debug.Log("<color=green>[PlayableDemo]</color> VICTORY: The Inquisitor falls. Vorgossos is silent once more.");
                    _quest.SetQuestStep(_vorgossosQuest.QuestId, 100); // Complete
                }
            };
        }
    }
}
