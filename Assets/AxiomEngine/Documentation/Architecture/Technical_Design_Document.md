

# **Multi-Genre RPG Platform**

Technical Design Document

Unity C\# Implementation with SOLID Principles

Version 1.0 | December 2024

# **Table of Contents**

# **1\. Executive Overview**

This document defines the technical architecture for a genre-agnostic RPG platform. The initial implementation targets a Sun Eater game inspired by KOTOR's gameplay mechanics, but the architecture supports complete genre reskinning for anime, fantasy, sci-fi, or any other setting.

## **1.1 Design Philosophy**

* **Composition over Inheritance:** Components define behavior; data drives configuration  
* **Pluggable Systems:** Combat resolution, morality, and progression systems are swappable per-game  
* **Data-Driven Design:** ScriptableObjects define all game content; code defines behavior  
* **SOLID Principles:** Single responsibility, open/closed, dependency injection throughout

## **1.2 Core Requirements Summary**

| Requirement | Implementation Approach |
| :---- | :---- |
| Turn-based Combat | Command Pattern with pluggable resolution strategies |
| d20 Resolution (Default) | ICombatResolver interface; D20Resolver as default implementation |
| Dual-Axis Morality | Configurable axis system via MoralitySystemConfig SO |
| Power Scaling | Tier-based progression with data-driven curves |
| Planet-Hopping | Location system with scene management abstraction |
| Genre Agnostic | Core logic separated from GenreDataPack ScriptableObjects |

# **2\. System Architecture Overview**

## **2.1 Layer Architecture**

The platform follows a strict layered architecture ensuring separation of concerns and enabling genre swapping without touching core game logic.

┌─────────────────────────────────────────────────────────────┐

│                    PRESENTATION LAYER                       │

│    UI Controllers | Animation | Visual Effects | Audio      │

├─────────────────────────────────────────────────────────────┤

│                    GENRE DATA LAYER                         │

│  GenreDataPack | Themes | Labels | Visuals | Audio Assets   │

├─────────────────────────────────────────────────────────────┤

│                    GAME SYSTEMS LAYER                       │

│     Combat | Dialogue | Progression | Quests | Inventory    │

├─────────────────────────────────────────────────────────────┤

│                    CORE ENGINE LAYER                        │

│   Interfaces | Events | Commands | State | Serialization    │

└─────────────────────────────────────────────────────────────┘

## **2.2 Namespace Organization**

RPGPlatform.Core           // Interfaces, base types, events

RPGPlatform.Core.Combat    // Combat interfaces and commands

RPGPlatform.Core.Morality  // Morality system interfaces

RPGPlatform.Core.Dialogue  // Dialogue system interfaces

RPGPlatform.Systems        // Concrete system implementations

RPGPlatform.Data           // ScriptableObject definitions

RPGPlatform.Genres         // Genre-specific data packs

RPGPlatform.UI             // Presentation layer

# **3\. Combat System Architecture**

## **3.1 Class Hierarchy Overview**

The combat system uses a Command Pattern for actions, a Strategy Pattern for resolution, and an Observer Pattern for state changes. This enables complete flexibility in combat mechanics while maintaining clean separation.

### **3.1.1 Core Combat Interfaces**

// Core combatant interface \- all entities implement this

public interface ICombatant

{

    string Id { get; }

    string DisplayName { get; }

    CombatStats Stats { get; }

    IReadOnlyList\<IAbility\> Abilities { get; }

    IReadOnlyList\<IStatusEffect\> ActiveEffects { get; }

    bool IsAlive { get; }

    int Initiative { get; }

    

    void ApplyDamage(DamageResult damage);

    void ApplyHealing(int amount);

    void AddStatusEffect(IStatusEffect effect);

    void RemoveStatusEffect(string effectId);

}

// Pluggable resolution system \- swap d20 for other systems

public interface ICombatResolver

{

    RollResult ResolveAttack(ICombatant attacker, ICombatant target, IAbility ability);

    DamageResult CalculateDamage(ICombatant attacker, ICombatant target, 

                                  IAbility ability, RollResult roll);

    bool ResolveSavingThrow(ICombatant target, SaveType saveType, int dc);

    int RollInitiative(ICombatant combatant);

}

### **3.1.2 D20 Combat Resolver (Default Implementation)**

public class D20CombatResolver : ICombatResolver

{

    private readonly D20ResolverConfig \_config;

    private readonly System.Random \_rng;

    

    public D20CombatResolver(D20ResolverConfig config)

    {

        \_config \= config;

        \_rng \= new System.Random();

    }

    

    public RollResult ResolveAttack(ICombatant attacker, ICombatant target, 

                                     IAbility ability)

    {

        int naturalRoll \= \_rng.Next(1, 21);

        int modifier \= CalculateAttackModifier(attacker, ability);

        int total \= naturalRoll \+ modifier;

        int targetAC \= target.Stats.ArmorClass;

        

        return new RollResult

        {

            NaturalRoll \= naturalRoll,

            Modifier \= modifier,

            Total \= total,

            TargetNumber \= targetAC,

            IsCriticalHit \= naturalRoll \>= \_config.CriticalThreshold,

            IsCriticalMiss \= naturalRoll \== 1,

            IsSuccess \= naturalRoll \== 20 || (naturalRoll \!= 1 && total \>= targetAC)

        };

    }

    

    private int CalculateAttackModifier(ICombatant attacker, IAbility ability)

    {

        var stat \= ability.PrimaryStat;

        int baseModifier \= attacker.Stats.GetModifier(stat);

        int proficiencyBonus \= attacker.Stats.ProficiencyBonus;

        return baseModifier \+ proficiencyBonus;

    }

}

## **3.2 Turn Queue and Command Pattern**

### **3.2.1 Turn Queue Manager**

public class TurnQueueManager

{

    public event Action\<ICombatant\> OnTurnStarted;

    public event Action\<ICombatant\> OnTurnEnded;

    public event Action OnRoundStarted;

    public event Action OnCombatEnded;

    

    private readonly PriorityQueue\<ICombatant, int\> \_turnQueue;

    private readonly List\<ICombatant\> \_allCombatants;

    private readonly ICombatResolver \_resolver;

    

    public ICombatant CurrentCombatant { get; private set; }

    public int CurrentRound { get; private set; }

    public CombatState State { get; private set; }

    

    public TurnQueueManager(ICombatResolver resolver)

    {

        \_resolver \= resolver;

        \_turnQueue \= new PriorityQueue\<ICombatant, int\>();

        \_allCombatants \= new List\<ICombatant\>();

        State \= CombatState.NotStarted;

    }

    

    public void InitializeCombat(IEnumerable\<ICombatant\> combatants)

    {

        \_allCombatants.Clear();

        \_allCombatants.AddRange(combatants);

        RollInitiativeForAll();

        CurrentRound \= 1;

        State \= CombatState.Active;

        OnRoundStarted?.Invoke();

        AdvanceToNextTurn();

    }

    

    public void AdvanceToNextTurn()

    {

        if (CurrentCombatant \!= null)

            OnTurnEnded?.Invoke(CurrentCombatant);

        

        if (\_turnQueue.Count \== 0\)

        {

            StartNewRound();

            return;

        }

        

        do { CurrentCombatant \= \_turnQueue.Dequeue(); }

        while (\!CurrentCombatant.IsAlive && \_turnQueue.Count \> 0);

        

        if (CurrentCombatant.IsAlive)

            OnTurnStarted?.Invoke(CurrentCombatant);

        else

            CheckCombatEnd();

    }

    

    private void RollInitiativeForAll()

    {

        foreach (var combatant in \_allCombatants)

        {

            int initiative \= \_resolver.RollInitiative(combatant);

            \_turnQueue.Enqueue(combatant, \-initiative); // Negative for descending

        }

    }

}

### **3.2.2 Combat Command Pattern**

// Base command interface with undo support

public interface ICombatCommand

{

    string CommandName { get; }

    ICombatant Source { get; }

    bool CanExecute();

    CommandResult Execute();

    void Undo();

}

// Concrete attack command

public class AttackCommand : ICombatCommand

{

    private readonly ICombatResolver \_resolver;

    private readonly ICombatant \_target;

    private readonly IAbility \_ability;

    private DamageResult \_appliedDamage;

    

    public string CommandName \=\> $"Attack with {\_ability.DisplayName}";

    public ICombatant Source { get; }

    

    public AttackCommand(ICombatant source, ICombatant target, 

                          IAbility ability, ICombatResolver resolver)

    {

        Source \= source;

        \_target \= target;

        \_ability \= ability;

        \_resolver \= resolver;

    }

    

    public bool CanExecute() \=\> Source.IsAlive && \_target.IsAlive && 

                                 \_ability.CanUse(Source);

    

    public CommandResult Execute()

    {

        var roll \= \_resolver.ResolveAttack(Source, \_target, \_ability);

        

        if (\!roll.IsSuccess)

            return CommandResult.Miss(roll);

        

        \_appliedDamage \= \_resolver.CalculateDamage(Source, \_target, \_ability, roll);

        \_target.ApplyDamage(\_appliedDamage);

        

        return CommandResult.Hit(roll, \_appliedDamage);

    }

    

    public void Undo()

    {

        if (\_appliedDamage \!= null)

            \_target.ApplyHealing(\_appliedDamage.TotalDamage);

    }

}

### **3.2.3 Combat Manager**

public class CombatManager

{

    private readonly TurnQueueManager \_turnQueue;

    private readonly ICombatResolver \_resolver;

    private readonly Stack\<ICombatCommand\> \_commandHistory;

    

    public event Action\<ICombatCommand, CommandResult\> OnCommandExecuted;

    public event Action\<CombatEndResult\> OnCombatEnded;

    

    public CombatManager(ICombatResolver resolver)

    {

        \_resolver \= resolver;

        \_turnQueue \= new TurnQueueManager(resolver);

        \_commandHistory \= new Stack\<ICombatCommand\>();

        \_turnQueue.OnCombatEnded \+= HandleCombatEnded;

    }

    

    public void ExecuteCommand(ICombatCommand command)

    {

        if (\!command.CanExecute()) return;

        

        var result \= command.Execute();

        \_commandHistory.Push(command);

        OnCommandExecuted?.Invoke(command, result);

    }

    

    public void UndoLastCommand()

    {

        if (\_commandHistory.Count \> 0\)

            \_commandHistory.Pop().Undo();

    }

}

# **4\. Morality and Alignment System**

The morality system is fully configurable per-game, supporting no morality, single-axis, dual-axis, or multi-dimensional alignment systems. The system is static per game instance but completely reconfigurable between games.

## **4.1 Data Structures**

### **4.1.1 Axis Definition**

\[System.Serializable\]

public class MoralityAxis

{

    public string AxisId;           // Internal identifier

    public string DisplayName;      // Shown in UI

    public string NegativeLabel;    // e.g., "Humanism"

    public string PositiveLabel;    // e.g., "Transhumanism"

    public float MinValue \= \-100f;

    public float MaxValue \= 100f;

    public float DefaultValue \= 0f;

    public Gradient ColorGradient;  // Visual representation

    public Sprite NegativeIcon;

    public Sprite PositiveIcon;

}

### **4.1.2 System Configuration**

\[CreateAssetMenu(fileName \= "MoralityConfig", menuName \= "RPG/Morality Config")\]

public class MoralitySystemConfig : ScriptableObject

{

    \[Header("System Configuration")\]

    public MoralitySystemType SystemType;  // None, SingleAxis, DualAxis, Custom

    public bool IsVisible \= true;           // Show in UI?

    public bool AffectsDialogue \= true;     // Unlock dialogue options?

    public bool AffectsAbilities \= true;    // Gate abilities?

    

    \[Header("Axes")\]

    public List\<MoralityAxis\> Axes \= new List\<MoralityAxis\>();

    

    \[Header("Thresholds")\]

    public List\<MoralityThreshold\> Thresholds;  // Named tiers like "Paragon"

    

    public int AxisCount \=\> Axes.Count;

    public bool HasMorality \=\> SystemType \!= MoralitySystemType.None;

}

public enum MoralitySystemType { None, SingleAxis, DualAxis, Custom }

### **4.1.3 Player Morality State**

\[System.Serializable\]

public class MoralityState

{

    private readonly Dictionary\<string, float\> \_axisValues;

    private readonly MoralitySystemConfig \_config;

    

    public event Action\<string, float, float\> OnAxisChanged;

    public event Action\<MoralityThreshold\> OnThresholdCrossed;

    

    public MoralityState(MoralitySystemConfig config)

    {

        \_config \= config;

        \_axisValues \= new Dictionary\<string, float\>();

        

        foreach (var axis in config.Axes)

            \_axisValues\[axis.AxisId\] \= axis.DefaultValue;

    }

    

    public float GetValue(string axisId) \=\> 

        \_axisValues.TryGetValue(axisId, out var val) ? val : 0f;

    

    public void ModifyAxis(string axisId, float delta)

    {

        if (\!\_axisValues.ContainsKey(axisId)) return;

        

        var axis \= \_config.Axes.First(a \=\> a.AxisId \== axisId);

        float oldValue \= \_axisValues\[axisId\];

        float newValue \= Mathf.Clamp(oldValue \+ delta, axis.MinValue, axis.MaxValue);

        

        \_axisValues\[axisId\] \= newValue;

        OnAxisChanged?.Invoke(axisId, oldValue, newValue);

        CheckThresholds(axisId, oldValue, newValue);

    }

    

    public MoralityThreshold GetCurrentThreshold(string axisId) { /\* ... \*/ }

    public Vector2 GetDualAxisPosition() { /\* For 2-axis systems \*/ }

}

## **4.2 Example Configurations**

### **4.2.1 Sun Eater (Dual-Axis)**

| Axis | Negative Pole | Positive Pole |
| :---- | :---- | :---- |
| Primary | Humanism | Transhumanism |
| Secondary | Compassion | Ruthlessness |

### **4.2.2 Fantasy Reskin (Single-Axis)**

| Axis | Negative Pole | Positive Pole |
| :---- | :---- | :---- |
| Alignment | Shadow | Light |

### **4.2.3 Anime Reskin (No Morality)**

SystemType \= MoralitySystemType.None — All morality-gated content becomes available; dialogue choices have no alignment consequences.

# **5\. Dialogue System**

## **5.1 Dialogue Data Structures**

\[CreateAssetMenu(fileName \= "Conversation", menuName \= "RPG/Dialogue/Conversation")\]

public class ConversationData : ScriptableObject

{

    public string ConversationId;

    public string SpeakerId;

    public List\<DialogueNode\> Nodes;

    public string EntryNodeId;

}

\[System.Serializable\]

public class DialogueNode

{

    public string NodeId;

    public string SpeakerOverride;      // If different from conversation speaker

    public LocalizedString Text;        // Supports localization

    public AudioClip VoiceOver;

    public List\<DialogueResponse\> Responses;

    public List\<DialogueCondition\> Conditions;  // Prerequisites to show

    public List\<DialogueEffect\> Effects;         // Executed when node plays

}

\[System.Serializable\]

public class DialogueResponse

{

    public string ResponseId;

    public LocalizedString Text;

    public string NextNodeId;           // Where this leads

    public List\<DialogueCondition\> Conditions;

    public List\<MoralityShift\> MoralityEffects;  // Alignment changes

    public ResponseDisplayType DisplayType;      // Normal, Skill, Morality

}

## **5.2 Condition System**

public interface IDialogueCondition

{

    bool Evaluate(DialogueContext context);

    string GetFailureReason();  // For grayed-out options

}

public class MoralityCondition : IDialogueCondition

{

    public string AxisId;

    public ComparisonType Comparison;

    public float RequiredValue;

    

    public bool Evaluate(DialogueContext ctx) \=\> 

        Compare(ctx.PlayerMorality.GetValue(AxisId), RequiredValue, Comparison);

}

public class SkillCheckCondition : IDialogueCondition

{

    public string SkillId;

    public int RequiredLevel;

    public bool ShowEvenIfFailed \= true;  // Gray out vs hide

}

# **6\. Progression System**

## **6.1 Power Tier Architecture**

The progression system supports scaling from basic fighters to legendary warriors (Palatine-tier in Sun Eater). Tiers are data-driven and fully configurable per genre.

\[CreateAssetMenu(fileName \= "ProgressionConfig", menuName \= "RPG/Progression Config")\]

public class ProgressionConfig : ScriptableObject

{

    public List\<PowerTier\> Tiers;

    public AnimationCurve XPCurve;         // XP required per level

    public int MaxLevel \= 50;

    public List\<StatScalingCurve\> StatCurves;

}

\[System.Serializable\]

public class PowerTier

{

    public string TierId;

    public string DisplayName;       // "Initiate", "Knight", "Palatine"

    public int MinLevel;

    public int MaxLevel;

    public List\<string\> UnlockedAbilityIds;

    public List\<string\> UnlockedPerkIds;

    public float StatMultiplier \= 1f;

    public Sprite TierIcon;

    public Color TierColor;

}

## **6.2 Example Power Tiers (Sun Eater)**

| Tier | Levels | Abilities | Stat Multiplier |
| :---- | :---- | :---- | :---- |
| Initiate | 1-10 | Basic combat, minor powers | 1.0x |
| Knight | 11-25 | Advanced powers, leadership | 1.5x |
| Champion | 26-40 | Elite powers, army command | 2.0x |
| Palatine | 41-50 | Legendary powers, reality-altering | 3.0x |

# **7\. ScriptableObject Templates**

## **7.1 Ability Template**

\[CreateAssetMenu(fileName \= "NewAbility", menuName \= "RPG/Abilities/Ability")\]

public class AbilityData : ScriptableObject

{

    \[Header("Identity")\]

    public string AbilityId;

    public LocalizedString DisplayName;

    public LocalizedString Description;

    public Sprite Icon;

    

    \[Header("Classification")\]

    public AbilityType Type;          // Attack, Defense, Buff, Debuff, Utility

    public TargetType Targeting;      // Self, Single, Area, All

    public DamageType DamageType;     // Physical, Energy, Psychic, etc.

    public List\<string\> Tags;         // For filtering: "fire", "healing", etc.

    

    \[Header("Costs & Cooldowns")\]

    public int ActionPointCost \= 1;

    public int ResourceCost;          // Mana, Energy, etc.

    public string ResourceType;       // Which resource to consume

    public int CooldownTurns;

    public int UsesPerCombat \= \-1;    // \-1 \= unlimited

    

    \[Header("Effects")\]

    public string BaseDamageFormula;  // e.g., "2d6 \+ STR"

    public float CritMultiplier \= 2f;

    public List\<StatusEffectApplication\> AppliedEffects;

    

    \[Header("Requirements")\]

    public int RequiredLevel;

    public string RequiredTierId;

    public List\<MoralityRequirement\> MoralityRequirements;

    public List\<string\> PrerequisiteAbilityIds;

    

    \[Header("Presentation")\]

    public AnimationClip UseAnimation;

    public GameObject VFXPrefab;

    public AudioClip SFX;

}

## **7.2 Item Template**

\[CreateAssetMenu(fileName \= "NewItem", menuName \= "RPG/Items/Item")\]

public class ItemData : ScriptableObject

{

    \[Header("Identity")\]

    public string ItemId;

    public LocalizedString DisplayName;

    public LocalizedString Description;

    public Sprite Icon;

    public ItemRarity Rarity;

    

    \[Header("Classification")\]

    public ItemType Type;             // Weapon, Armor, Consumable, Quest, Misc

    public EquipmentSlot Slot;        // For equipment only

    public List\<string\> Tags;

    

    \[Header("Stats")\]

    public List\<StatModifier\> StatModifiers;

    public List\<string\> GrantedAbilityIds;

    

    \[Header("Economy")\]

    public int BaseValue;

    public int MaxStack \= 1;

    public bool IsSellable \= true;

    

    \[Header("Requirements")\]

    public int RequiredLevel;

    public List\<StatRequirement\> StatRequirements;

}

## **7.3 Quest Template**

\[CreateAssetMenu(fileName \= "NewQuest", menuName \= "RPG/Quests/Quest")\]

public class QuestData : ScriptableObject

{

    \[Header("Identity")\]

    public string QuestId;

    public LocalizedString DisplayName;

    public LocalizedString Description;

    public QuestType Type;            // Main, Side, Companion, Faction

    public string LocationId;         // Planet/area where quest begins

    

    \[Header("Structure")\]

    public List\<QuestObjective\> Objectives;

    public List\<QuestBranch\> Branches;     // Mutually exclusive paths

    

    \[Header("Prerequisites")\]

    public int MinLevel;

    public List\<string\> RequiredCompletedQuestIds;

    public List\<MoralityRequirement\> MoralityRequirements;

    

    \[Header("Rewards")\]

    public int XPReward;

    public int CurrencyReward;

    public List\<ItemReward\> ItemRewards;

    public List\<MoralityShift\> MoralityRewards;

    public List\<string\> UnlockedQuestIds;   // Chains to next quests

}

\[System.Serializable\]

public class QuestObjective

{

    public string ObjectiveId;

    public LocalizedString Description;

    public ObjectiveType Type;        // Kill, Collect, Talk, Reach, Custom

    public string TargetId;           // Enemy type, item id, NPC id, location

    public int RequiredCount \= 1;

    public bool IsOptional;

    public bool IsHidden;             // Reveal on discovery

}

# **8\. Genre Separation Architecture**

The platform achieves genre-agnosticism through a clean separation between core systems and genre-specific data. All themed content is encapsulated in GenreDataPack ScriptableObjects.

## **8.1 Genre Data Pack**

\[CreateAssetMenu(fileName \= "NewGenre", menuName \= "RPG/Genre/Data Pack")\]

public class GenreDataPack : ScriptableObject

{

    \[Header("Identity")\]

    public string GenreId;

    public string DisplayName;           // "Sun Eater", "Mystic Realms"

    public string Description;

    public Sprite GenreIcon;

    

    \[Header("Systems Configuration")\]

    public MoralitySystemConfig MoralityConfig;

    public ProgressionConfig ProgressionConfig;

    public CombatResolverType DefaultResolver;  // D20, Percentile, Custom

    

    \[Header("Terminology")\]

    public TerminologyMap Terminology;   // Generic \-\> Genre-specific labels

    

    \[Header("Content")\]

    public List\<LocationData\> Locations;

    public List\<CharacterArchetype\> Archetypes;

    public List\<AbilityData\> Abilities;

    public List\<ItemData\> Items;

    public List\<QuestData\> Quests;

    

    \[Header("Presentation")\]

    public ThemeAssets Theme;            // Colors, fonts, UI sprites

    public AudioAssets Audio;            // Music, ambient, UI sounds

}

## **8.2 Terminology Mapping**

\[System.Serializable\]

public class TerminologyMap

{

    // Maps generic terms to genre-specific labels

    public Dictionary\<string, string\> Terms;

    

    // Example mappings for Sun Eater:

    // "mana" \-\> "Essence"

    // "spell" \-\> "Technique"

    // "level" \-\> "Rank"

    // "experience" \-\> "Glory"

    // "currency" \-\> "Marks"

    

    public string Localize(string genericTerm)

    {

        return Terms.TryGetValue(genericTerm.ToLower(), out var localized)

            ? localized

            : genericTerm;

    }

}

## **8.3 Theme Assets**

\[System.Serializable\]

public class ThemeAssets

{

    \[Header("Colors")\]

    public Color PrimaryColor;

    public Color SecondaryColor;

    public Color AccentColor;

    public Color BackgroundColor;

    public Color TextColor;

    

    \[Header("Fonts")\]

    public TMP\_FontAsset HeaderFont;

    public TMP\_FontAsset BodyFont;

    public TMP\_FontAsset UIFont;

    

    \[Header("UI Elements")\]

    public Sprite PanelBackground;

    public Sprite ButtonNormal;

    public Sprite ButtonHovered;

    public Sprite ButtonPressed;

    public Sprite HealthBarFill;

    public Sprite ResourceBarFill;

    

    \[Header("Icons")\]

    public Sprite\[\] RarityFrames;      // Common, Uncommon, Rare, Epic, Legendary

    public Sprite\[\] DamageTypeIcons;

    public Sprite\[\] AbilityTypeIcons;

}

# **9\. Location and Planet-Hopping System**

## **9.1 Location Data Structure**

\[CreateAssetMenu(fileName \= "NewLocation", menuName \= "RPG/World/Location")\]

public class LocationData : ScriptableObject

{

    \[Header("Identity")\]

    public string LocationId;

    public LocalizedString DisplayName;

    public LocalizedString Description;

    public LocationType Type;           // Planet, Station, Ship, Dungeon

    

    \[Header("World Map")\]

    public Vector2 GalaxyMapPosition;

    public Sprite MapIcon;

    public Sprite BackgroundArt;

    

    \[Header("Scenes")\]

    public string MainSceneName;

    public List\<SubLocation\> SubLocations;  // Areas within the location

    

    \[Header("Content")\]

    public List\<string\> AvailableQuestIds;

    public List\<string\> NPCIds;

    public List\<string\> ShopIds;

    public List\<EncounterTable\> RandomEncounters;

    

    \[Header("Requirements")\]

    public int MinLevel;

    public List\<string\> RequiredQuestIds;     // Must complete to unlock

    public List\<string\> RequiredItemIds;      // Keys, passes, etc.

    

    \[Header("Atmosphere")\]

    public AudioClip AmbientMusic;

    public AudioClip AmbientSFX;

    public Color LightingTint;

}

## **9.2 Travel Manager**

public class TravelManager

{

    private readonly ISceneLoader \_sceneLoader;

    private readonly IProgressionService \_progression;

    private readonly IQuestService \_quests;

    

    public event Action\<LocationData\> OnTravelStarted;

    public event Action\<LocationData\> OnTravelCompleted;

    public event Action\<LocationData, string\> OnTravelBlocked;

    

    public LocationData CurrentLocation { get; private set; }

    public List\<LocationData\> DiscoveredLocations { get; }

    

    public bool CanTravelTo(LocationData destination)

    {

        if (\_progression.CurrentLevel \< destination.MinLevel)

            return false;

        

        foreach (var questId in destination.RequiredQuestIds)

            if (\!\_quests.IsCompleted(questId))

                return false;

        

        return true;

    }

    

    public async Task TravelTo(LocationData destination)

    {

        if (\!CanTravelTo(destination))

        {

            OnTravelBlocked?.Invoke(destination, GetBlockReason(destination));

            return;

        }

        

        OnTravelStarted?.Invoke(destination);

        await \_sceneLoader.LoadScene(destination.MainSceneName);

        CurrentLocation \= destination;

        OnTravelCompleted?.Invoke(destination);

    }

}

# **10\. Dependency Injection and Service Locator**

The platform uses a lightweight dependency injection container for managing system dependencies, enabling easy testing and system swapping.

## **10.1 Service Container**

public class GameServiceContainer

{

    private readonly Dictionary\<Type, object\> \_services;

    private readonly Dictionary\<Type, Func\<object\>\> \_factories;

    

    public void RegisterSingleton\<TInterface, TImpl\>(TImpl instance)

        where TImpl : TInterface

    {

        \_services\[typeof(TInterface)\] \= instance;

    }

    

    public void RegisterFactory\<TInterface\>(Func\<TInterface\> factory)

    {

        \_factories\[typeof(TInterface)\] \= () \=\> factory();

    }

    

    public TInterface Resolve\<TInterface\>()

    {

        if (\_services.TryGetValue(typeof(TInterface), out var service))

            return (TInterface)service;

        

        if (\_factories.TryGetValue(typeof(TInterface), out var factory))

            return (TInterface)factory();

        

        throw new InvalidOperationException(

            $"Service {typeof(TInterface).Name} not registered");

    }

}

## **10.2 Game Bootstrap**

public class GameBootstrap : MonoBehaviour

{

    \[SerializeField\] private GenreDataPack \_genreData;

    

    private void Awake()

    {

        var container \= new GameServiceContainer();

        

        // Register combat resolver based on genre config

        ICombatResolver resolver \= \_genreData.DefaultResolver switch

        {

            CombatResolverType.D20 \=\> new D20CombatResolver(

                                          \_genreData.D20Config),

            CombatResolverType.Percentile \=\> new PercentileResolver(

                                                 \_genreData.PercentileConfig),

            \_ \=\> new D20CombatResolver(\_genreData.D20Config)

        };

        container.RegisterSingleton\<ICombatResolver, ICombatResolver\>(resolver);

        

        // Register morality system (or null service if None)

        if (\_genreData.MoralityConfig.HasMorality)

        {

            container.RegisterSingleton\<IMoralityService, MoralityService\>(

                new MoralityService(\_genreData.MoralityConfig));

        }

        else

        {

            container.RegisterSingleton\<IMoralityService, NullMoralityService\>(

                new NullMoralityService());

        }

        

        // Register other core services...

        container.RegisterSingleton\<ICombatManager, CombatManager\>(

            new CombatManager(resolver));

        container.RegisterSingleton\<IProgressionService, ProgressionService\>(

            new ProgressionService(\_genreData.ProgressionConfig));

        

        ServiceLocator.Initialize(container);

    }

}

# **Appendix A: Core Enumerations**

public enum CombatState { NotStarted, Active, Paused, Victory, Defeat }

public enum AbilityType { Attack, Defense, Buff, Debuff, Utility, Passive }

public enum TargetType { Self, SingleEnemy, SingleAlly, AllEnemies, AllAllies, Area }

public enum DamageType { Physical, Energy, Psychic, Fire, Ice, Lightning, Void }

public enum SaveType { Fortitude, Reflex, Will }

public enum ItemType { Weapon, Armor, Accessory, Consumable, Quest, Material }

public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary, Unique }

public enum EquipmentSlot { MainHand, OffHand, Head, Body, Hands, Feet, Accessory1, Accessory2 }

public enum QuestType { Main, Side, Companion, Faction, Daily }

public enum ObjectiveType { Kill, Collect, Talk, Reach, Escort, Defend, Custom }

public enum LocationType { Planet, SpaceStation, Ship, Outpost, Dungeon, City }

public enum ComparisonType { GreaterThan, LessThan, GreaterOrEqual, LessOrEqual, Equal }

public enum ResponseDisplayType { Normal, SkillCheck, MoralityGated, Locked }

# **Appendix B: Recommended Project Structure**

Assets/

├── Scripts/

│   ├── Core/

│   │   ├── Interfaces/

│   │   │   ├── ICombatant.cs

│   │   │   ├── ICombatResolver.cs

│   │   │   ├── ICombatCommand.cs

│   │   │   ├── IMoralityService.cs

│   │   │   └── IDialogueCondition.cs

│   │   ├── Commands/

│   │   │   ├── AttackCommand.cs

│   │   │   ├── DefendCommand.cs

│   │   │   └── UseAbilityCommand.cs

│   │   ├── Events/

│   │   │   └── GameEvents.cs

│   │   └── DataStructures/

│   │       ├── RollResult.cs

│   │       ├── DamageResult.cs

│   │       └── CommandResult.cs

│   ├── Systems/

│   │   ├── Combat/

│   │   │   ├── D20CombatResolver.cs

│   │   │   ├── TurnQueueManager.cs

│   │   │   └── CombatManager.cs

│   │   ├── Morality/

│   │   │   ├── MoralityService.cs

│   │   │   └── NullMoralityService.cs

│   │   ├── Dialogue/

│   │   │   └── DialogueManager.cs

│   │   ├── Progression/

│   │   │   └── ProgressionManager.cs

│   │   └── Travel/

│   │       └── TravelManager.cs

│   ├── Data/

│   │   ├── AbilityData.cs

│   │   ├── ItemData.cs

│   │   ├── QuestData.cs

│   │   ├── LocationData.cs

│   │   └── ConversationData.cs

│   └── UI/

│       ├── CombatUI.cs

│       ├── DialogueUI.cs

│       └── InventoryUI.cs

├── ScriptableObjects/

│   ├── Genres/

│   │   ├── SunEater/

│   │   │   ├── SunEaterGenre.asset

│   │   │   ├── Abilities/

│   │   │   ├── Items/

│   │   │   └── Quests/

│   │   └── FantasyReskin/

│   └── Config/

│       ├── D20Config.asset

│       └── MoralityConfigs/

└── Prefabs/

    ├── Combat/

    ├── UI/

    └── Characters/

# **Appendix C: Alternative Combat Resolvers**

The ICombatResolver interface allows complete replacement of the d20 system. Below are example alternative implementations.

## **C.1 Percentile Resolver**

public class PercentileCombatResolver : ICombatResolver

{

    private readonly PercentileConfig \_config;

    

    public RollResult ResolveAttack(ICombatant attacker, ICombatant target,

                                     IAbility ability)

    {

        int roll \= Random.Range(1, 101);

        int hitChance \= CalculateHitChance(attacker, target, ability);

        

        return new RollResult

        {

            NaturalRoll \= roll,

            TargetNumber \= hitChance,

            IsCriticalHit \= roll \<= \_config.CriticalThreshold,

            IsCriticalMiss \= roll \>= \_config.FumbleThreshold,

            IsSuccess \= roll \<= hitChance

        };

    }

}

## **C.2 Card-Based Resolver**

public class CardCombatResolver : ICombatResolver

{

    private readonly Deck \_deck;

    

    public RollResult ResolveAttack(ICombatant attacker, ICombatant target,

                                     IAbility ability)

    {

        var card \= \_deck.Draw();

        int effectiveValue \= card.Value \+ GetSuitBonus(card.Suit, ability);

        

        return new RollResult

        {

            NaturalRoll \= card.Value,

            Modifier \= GetSuitBonus(card.Suit, ability),

            Total \= effectiveValue,

            IsCriticalHit \= card.Value \>= 11,  // Face cards

            IsSuccess \= effectiveValue \> target.Stats.Defense

        };

    }

}

# **Document Revision History**

| Version | Date | Changes |
| :---- | :---- | :---- |
| 1.0 | December 2024 | Initial release \- Core architecture, combat, morality, progression, dialogue, quest, and genre separation systems |

*— End of Document —*