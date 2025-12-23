using UnityEngine;
using UnityEditor;
using RPGPlatform.Core;
using RPGPlatform.Data;
using RPGPlatform.Core.Dialogue;
using System.Collections.Generic;
using System.IO;

namespace RPGPlatform.Editor
{
    public static class AxiomDemoGenerator
    {
        private const string RootPath = "Assets/AxiomEngine/GameSpecific/SunEater/Data";

        [MenuItem("Axiom/Demo/Generate Playable Demo Data")]
        public static void GenerateAll()
        {
            Debug.Log("[Axiom] Starting Playable Demo Data Generation...");
            
            EnsureDirectories();
            
            // 0. Setup Genre/Morality (from AxiomAssetImporter)
            AxiomAssetImporter.SetupSunEater();
            
            var profile = GenerateGenreAssets();
            var ability = GenerateHighMatterAbility();
            var inquisitor = GenerateInquisitor(ability);
            var player = GeneratePlayer(ability);
            
            GenerateIntroConversation(inquisitor);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("[Axiom] Demo Generation Complete! Assets are located in " + RootPath);
        }

        private static void EnsureDirectories()
        {
            string[] subdirs = { "Locations", "Abilities", "NPCs", "Dialogue" };
            foreach (var dir in subdirs)
            {
                string path = Path.Combine(RootPath, dir);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            }
        }

        private static GenreProfile GenerateGenreAssets()
        {
            string path = $"{RootPath}/SunEater_GenreProfile.asset";
            var profile = AssetDatabase.LoadAssetAtPath<GenreProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<GenreProfile>();
                profile.GenreId = "suneater";
                profile.GenreName = "The Sun Eater";
                profile.PrimaryColor = new Color(0.1f, 0.8f, 0.4f);
                AssetDatabase.CreateAsset(profile, path);
            }
            return profile;
        }

        private static AbilityData GenerateHighMatterAbility()
        {
            var ability = ScriptableObject.CreateInstance<AbilityData>();
            ability.AbilityId = "high_matter_swing";
            ability.DisplayName = "High-Matter Swing";
            ability.Description = "A sweeping strike with a blade of condensed matter.";
            ability.Type = AbilityType.Attack;
            ability.Targeting = TargetType.SingleEnemy;
            ability.DamageType = DamageType.Energy;
            ability.PrimaryStat = StatType.Dexterity;
            ability.DamageFormula = "2d8 + 4";
            ability.Range = 1;
            
            string path = $"{RootPath}/Abilities/Ability_HighMatterSwing.asset";
            AssetDatabase.CreateAsset(ability, path);
            return ability;
        }

        private static CombatantData GenerateInquisitor(AbilityData weaponAbility)
        {
            var npc = ScriptableObject.CreateInstance<CombatantData>();
            npc.Id = "chantry_inquisitor";
            npc.BaseName = "Chantry Inquisitor";
            npc.Title = "Hand of the Chantry";
            npc.BaseStats = new CombatStats { 
                Strength = 12, Dexterity = 14, Constitution = 12, 
                Intelligence = 14, Wisdom = 16, Charisma = 15,
                MaxHealth = 80, CurrentHealth = 80, ArmorClass = 16 
            };
            npc.Abilities = new List<AbilityData> { weaponAbility };
            
            string path = $"{RootPath}/NPCs/NPC_Inquisitor.asset";
            AssetDatabase.CreateAsset(npc, path);
            return npc;
        }

        private static CombatantData GeneratePlayer(AbilityData weaponAbility)
        {
            var pc = ScriptableObject.CreateInstance<CombatantData>();
            pc.Id = "vorgossos_player";
            pc.BaseName = "Traveler";
            pc.BaseStats = new CombatStats { 
                Strength = 10, Dexterity = 16, Constitution = 12, 
                Intelligence = 12, Wisdom = 10, Charisma = 14,
                MaxHealth = 100, CurrentHealth = 100, ArmorClass = 15 
            };
            pc.Abilities = new List<AbilityData> { weaponAbility };
            
            string path = $"{RootPath}/NPCs/NPC_Player_Template.asset";
            AssetDatabase.CreateAsset(pc, path);
            return pc;
        }

        private static void GenerateIntroConversation(CombatantData inquisitor)
        {
            var convo = ScriptableObject.CreateInstance<ConversationData>();
            convo.ConversationId = "vorgossos_intro";
            convo.SpeakerId = inquisitor.Id;
            
            var rootNode = new DialogueNode
            {
                NodeId = "start",
                Text = "Halt, traveler. The Chantry has questions regarding your recent neural-link upgrades. Your soul reeks of heretical silicon.",
                Responses = new List<DialogueResponse>
                {
                    new DialogueResponse { Text = "[Faith] I serve the Chantry. These upgrades are sanctioned.", NextNodeId = "end_peaceful" },
                    new DialogueResponse 
                    { 
                        Text = "[Heresy] My body is my own, Inquisitor. Stand aside.", 
                        NextNodeId = "end_hostile"
                    }
                }
            };
            
            var peacefulEnd = new DialogueNode { NodeId = "end_peaceful", Text = "Very well. But the Red Company will be watching your form closely." };
            var hostileEnd = new DialogueNode { NodeId = "end_hostile", Text = "Blasphemy! By the Emperor's light, you shall be purged!" };

            convo.Nodes = new List<DialogueNode> { rootNode, peacefulEnd, hostileEnd };
            convo.EntryNodeId = "start";

            string path = $"{RootPath}/Dialogue/Conversation_VorgossosIntro.asset";
            AssetDatabase.CreateAsset(convo, path);
        }
    }
}
