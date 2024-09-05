using System.Linq;
using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Mirror;
using System.Text.RegularExpressions;
using UnityEngine.AI;
using TMPro;
namespace dragon.mirror{
    public class ScenePlayer : NetworkBehaviour
{
    public string x;
    public string y;
    [SyncVar]
    public bool GameMaster = false;
    [SyncVar]
    public bool CanTravel = false;
    [SyncVar] 
    public int TokenCount;
    [SyncVar]
    public bool Combat = false;
    [SyncVar] 
    [SerializeField] public long Gold;
    [SyncVar] 
    [SerializeField] public string matchID;
    [SyncVar] 
    [SerializeField] public int playerIndex;
    [SyncVar]
    [SerializeField] public bool inMatch = false;
    [SyncVar]
    [SerializeField] public bool inLobby = false;
    [SyncVar]
    [SerializeField] public string playerName;
    [SyncVar]
    [SerializeField] public string loadSprite;
    [SyncVar] 
    [SerializeField] public float Energy;
    [SyncVar] 
    [SerializeField] public bool matchLeader = false;
    public string lastScene;
    [SyncVar]
    public string currentScene;
    [SyncVar]
    [SerializeField] public Match currentMatch;
    [SyncVar]
	[SerializeField] public string SpellOne = "Empty";
    [SyncVar]
	[SerializeField] public bool TactSpellOne;
	[SyncVar]
	[SerializeField] public float CooldownSpellOne = 0f;
    [SyncVar]
	[SerializeField] public string SpellTwo = "Empty";
    [SyncVar]
	[SerializeField] public bool TactSpellTwo;
	[SyncVar]
	[SerializeField] public float CooldownSpellTwo = 0f;
    //[SyncVar]
	//[SerializeField] public bool MovingTact;
    public string currentNode;
    Transform cameraController;
    
    public SceneNode OurNode;
    [SerializeField] SpriteRenderer OVMsRend;
    [SerializeField] SpriteRenderer TOWNsRend;
    [SerializeField] TextMeshProUGUI TactName;
    [SerializeField] GameObject SpriteFlip;
    [SerializeField] GameObject rightClickPrefab;
    [SerializeField] GameObject sayPrefab;
    [SerializeField] GameObject levelUpPrefab;
    [SerializeField] Slider energySlider;

    bool playerOVMReady = false;
    //INVENTORY CODE
    //ENDINVENTORYCODE
    public static UnityEvent HideAll = new UnityEvent ();
    public static UnityEvent ShowAll = new UnityEvent ();

    public static UnityEvent<string> RepairCompletedOnItem = new UnityEvent<string> ();
    public static UnityEvent RepairCompletedAll = new UnityEvent ();
    public static UnityEvent<int> Speaker = new UnityEvent<int> ();
    public static UnityEvent<string> QuestFinished = new UnityEvent<string> ();
    public static UnityEvent<NetworkConnectionToClient, string> QuestCompleteRewardAccess = new UnityEvent<NetworkConnectionToClient, string>();
    public static UnityEvent NewWave = new UnityEvent();
    public static UnityEvent<List<Vector2>> BlockedWave = new UnityEvent<List<Vector2>>();
    [SerializeField] public static ScenePlayer localPlayer;
    [SerializeField] public static UnityEvent  PlayerMoving = new UnityEvent();
    [SerializeField] public static UnityEvent  ItemMoved = new UnityEvent();
    [SerializeField] public static UnityEvent<Vector3>  PlayerMovingClicked = new UnityEvent<Vector3>();
    [SerializeField] public static UnityEvent<ScenePlayer>  ClearSelectedTiles = new UnityEvent<ScenePlayer>();
    [SerializeField] public static UnityEvent<ScenePlayer>  SendUnselectedTarget = new UnityEvent<ScenePlayer>();
    [SerializeField] public static UnityEvent<MovingObject>  CancelCast = new UnityEvent<MovingObject>();
    [SerializeField] public static UnityEvent<Match, string>  PermissionToFinish = new UnityEvent<Match, string>();
    [SerializeField] public static UnityEvent<Match, string>  PermissionToFinishSewers = new UnityEvent<Match, string>();

    [SerializeField] public static UnityEvent  SendPlayers = new UnityEvent();
    [SerializeField] public static UnityEvent<int>  RefreshSpellEquipped = new UnityEvent<int>();
    [SerializeField] public static UnityEvent<PlayerCharacter> BuildCombatPlayerUI = new UnityEvent<PlayerCharacter>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient, string, string, string> SpellChange = new UnityEvent<NetworkConnectionToClient, string, string, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient, string, int> TactSpellChange = new UnityEvent<NetworkConnectionToClient, string, int>();

    [SerializeField] public static UnityEvent<NetworkConnectionToClient, LearnSpell, string> SpellPurchase = new UnityEvent<NetworkConnectionToClient, LearnSpell, string>();
    [SerializeField] public static UnityEvent<string, string> MoveRequest = new UnityEvent<string, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient, string, string, string> ServerCharacterBuildRequest = new UnityEvent<NetworkConnectionToClient, string, string, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient, string> ServerWyvernHatch = new UnityEvent<NetworkConnectionToClient, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient, string> FinalRequest = new UnityEvent<NetworkConnectionToClient, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient, string, string> ResetOVM = new UnityEvent<NetworkConnectionToClient, string, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient, string> PartyRemoval = new UnityEvent<NetworkConnectionToClient, string>();
    [SerializeField] public static UnityEvent<string, string, string, ScenePlayer> VoteNeed = new UnityEvent<string, string, string, ScenePlayer>();
    [SerializeField] public static UnityEvent<string, string, string, ScenePlayer> VoteGreed = new UnityEvent<string, string, string, ScenePlayer>();
    [SerializeField] public static UnityEvent<string, string, ScenePlayer> VotePass = new UnityEvent<string, string, ScenePlayer>();
    [SerializeField] public static UnityEvent<string> OVMRequest = new UnityEvent<string>();
    [SerializeField] public static UnityEvent<float> UIToggle = new UnityEvent<float>();
    [SerializeField] public static UnityEvent MapOn = new UnityEvent();
    [SerializeField] public static UnityEvent MapOff = new UnityEvent();
    [SerializeField] public static UnityEvent NewTarget = new UnityEvent();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient, float> EnergyUpdate = new UnityEvent<NetworkConnectionToClient, float>();
    [SerializeField] public static UnityEvent EnteringTown = new UnityEvent();
    [SerializeField] public static UnityEvent RandomMatchEventClient = new UnityEvent();
    [SerializeField] public static UnityEvent LeavingTown = new UnityEvent();
    [SerializeField] public static UnityEvent OpenWalletBuilder = new UnityEvent();
    [SerializeField] public static UnityEvent BuildInventories = new UnityEvent();
    [SerializeField] public static UnityEvent<string>  OnUI = new UnityEvent<string> ();
    [SerializeField] public static UnityEvent WalletAwake = new UnityEvent ();
    [SerializeField] public static UnityEvent RegistrationFinished = new UnityEvent ();
    [SerializeField] public static UnityEvent walletTransmute = new UnityEvent ();
    [SerializeField] public static UnityEvent<ScenePlayer> RemoveLobby = new UnityEvent<ScenePlayer>();
    [SerializeField] public static UnityEvent BeginGameClearLobby = new UnityEvent();
    [SerializeField] public static UnityEvent<ScenePlayer> CancelAllCastsOwned = new UnityEvent<ScenePlayer>();
    [SerializeField] public static UnityEvent PlayerDisconnectedFromMatchLobby = new UnityEvent();
    [SerializeField] public static UnityEvent<ScenePlayer, string> RemoveAdventurer = new UnityEvent<ScenePlayer, string>();
    [SerializeField] public static UnityEvent<List<MovingObject>> SendFogMobs = new UnityEvent<List<MovingObject>>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient> PurchaseCharacterToken = new UnityEvent<NetworkConnectionToClient>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient, string> DeleteCharacter = new UnityEvent<NetworkConnectionToClient, string>();
    [SerializeField] public bool menuOpened = false;
    public static UnityEvent<NetworkConnectionToClient, string, string, string> OnPlayerDataUpdateRequest = new UnityEvent<NetworkConnectionToClient, string, string, string>();
    public static UnityEvent<NetworkConnectionToClient, string, string> LevelUpStarted = new UnityEvent<NetworkConnectionToClient, string, string>();
    public static UnityEvent<NetworkConnectionToClient, string, string> LevelUpEnded = new UnityEvent<NetworkConnectionToClient, string, string>();
    [SerializeField] public static UnityEvent LevelUpEndedSound = new UnityEvent();
    [SerializeField] public static UnityEvent LevelUpStartedSound = new UnityEvent();
    public static UnityEvent<NetworkConnectionToClient> LogoutPlayer = new UnityEvent<NetworkConnectionToClient>();
    public static UnityEvent ServerLogoutPlayer = new UnityEvent();

    [SerializeField] public static UnityEvent innReset = new UnityEvent();
    [SerializeField] public static UnityEvent TargetHighlightReset = new UnityEvent();
    public static UnityEvent<NetworkConnectionToClient, StackingMessage> StackingItem = new UnityEvent<NetworkConnectionToClient, StackingMessage>();
    public static UnityEvent<NetworkConnectionToClient> HealPartyServer = new UnityEvent<NetworkConnectionToClient>();
    public static UnityEvent<NetworkConnectionToClient, string> ResCharacter = new UnityEvent<NetworkConnectionToClient, string>();
    public static UnityEvent PurchaseButtonAvailable = new UnityEvent();
    string charliesTicket; 
    Transform transformProcessing;
    //****************************
    //***********Energy***********
    //****************************
    [SerializeField] public static UnityEvent<float> Charging = new UnityEvent<float>();
    [SerializeField] public static UnityEvent<string> EndingServerMessage = new UnityEvent<string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,uint,string,int> ServerTransmitTX = new UnityEvent<NetworkConnectionToClient,uint,string,int>();
    [SerializeField] public static UnityEvent ToggleEndMatch = new UnityEvent();
    [SerializeField] public static UnityEvent CloseEndMatch = new UnityEvent();
    [SerializeField] public static UnityEvent ToggleCloseEndMatch = new UnityEvent();
    [SerializeField] public static UnityEvent ToggleSpellsOff = new UnityEvent();
    [SerializeField] public static UnityEvent LoadbarOnToggle = new UnityEvent();
    [SerializeField] public static UnityEvent LoadbarOffToggle = new UnityEvent();
    [SerializeField] public static UnityEvent<string> FinalWeight = new UnityEvent<string>();

    //Tactician Inventory
    [SerializeField] public static UnityEvent ContentSizer = new UnityEvent();
    [SerializeField] public static UnityEvent<CharacterInventoryListItem> PurchasedItem = new UnityEvent<CharacterInventoryListItem>();
    [SerializeField] public static UnityEvent<CharacterInventoryListItem> PickedUpItemTactician = new UnityEvent<CharacterInventoryListItem>();
    [SerializeField] public static UnityEvent<CharacterInventoryListItem, string, string> PickedUpItemCharacter = new UnityEvent<CharacterInventoryListItem, string, string>();
    [SerializeField] public static UnityEvent<ItemSelectable> StaffBuild = new UnityEvent<ItemSelectable>();
    [SerializeField] public static UnityEvent<string, int, string> BuildingItemDrop = new UnityEvent<string, int, string>();
    //trading server calls
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string> StashToTactInv = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string, string> StashToTactEquipped = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string> StashToTactSafetyBelt = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string, string> StashToCharInv = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string, EquippingData> StashToCharEquip = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string, EquippingData>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string, string> TactInvToTactEquip = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string> TactInvToTactBelt = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string, string> TactBeltToTactEquip = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string> TactBeltToTactInv = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string> TactEquipToTactInv = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string, string> TactEquipToTactEquip = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string> TactEquipToTactBelt = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string, string> TactEquipToCharInv = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string, string> TactInvToCharInv = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string, EquippingData> TactInvToCharEquip = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string, EquippingData>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string, string> TactSafetyBeltToCharInv = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string, EquippingData> TactSafetyBeltToCharEquip = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string, EquippingData>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string> TactInvToStash = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string> TactEquipToStash = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string> TactBeltToStash = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string, string> CharInvToTactInv = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string, string> CharEquipToTactInv = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string, string> CharInvToTactBelt = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string, string> CharEquipToTactBelt = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient, SalvageNetworkList> SalvageRefundItemList = new UnityEvent<NetworkConnectionToClient, SalvageNetworkList>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient, ItemSelectable> RepairSingleItemEVENT = new UnityEvent<NetworkConnectionToClient, ItemSelectable>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient> RepairAllItemsEVENT = new UnityEvent<NetworkConnectionToClient>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string> ServerConsumingItemFullyEvent = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string> ServerConsumingItemPartiallyEvent = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string>();

    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string> ServerDestroyItem = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string, EquippingData> CharInvToTactEquip = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string, EquippingData>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string, string> CharInvToStash = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string, string> CharEquipToStash = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string ,EquippingData> CharInvToCharInv = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string ,EquippingData>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string ,EquippingData> CharInvToCharEquip = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string ,EquippingData>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string ,EquippingData> CharEquipToCharInv = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string ,EquippingData>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string ,EquippingData> CharEquipToCharEquip = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string ,EquippingData>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string ,EquippingData> CharEquipToInvSame = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string ,EquippingData>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string ,EquippingData> CharInvToEquipSame = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string ,EquippingData>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, string ,EquippingData> CharEquipToEquipSame = new UnityEvent<NetworkConnectionToClient,ItemSelectable, string ,EquippingData>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, ItemSelectable, EquippingData> CharOneUnequipToCharEquip = new UnityEvent<NetworkConnectionToClient,ItemSelectable, ItemSelectable, EquippingData>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, ItemSelectable, EquippingData> CharTwoUnequipToCharEquip = new UnityEvent<NetworkConnectionToClient,ItemSelectable, ItemSelectable, EquippingData>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, ItemSelectable, EquippingData> CharOneUnequipToCharEquipSendTwo = new UnityEvent<NetworkConnectionToClient,ItemSelectable, ItemSelectable, EquippingData>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,ItemSelectable, ItemSelectable, EquippingData> CharTwoUnequipToCharEquipSendOne = new UnityEvent<NetworkConnectionToClient,ItemSelectable, ItemSelectable, EquippingData>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient,string, string, EquippingData> CharUnequipTactToCharEquip = new UnityEvent<NetworkConnectionToClient,string, string, EquippingData>();
    [SerializeField] public static UnityEvent<NewStackCreated> BuildStackableItem = new UnityEvent<NewStackCreated>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient, string> SendParty = new UnityEvent<NetworkConnectionToClient, string>();
    [SerializeField] public static UnityEvent GetCharacters = new UnityEvent();
    [SerializeField] public static UnityEvent<string> BuildItems = new UnityEvent<string>();
    [SerializeField] public static UnityEvent PartySpawned = new UnityEvent();
    [SerializeField] public static UnityEvent PartyRefresh = new UnityEvent();
    [SerializeField] public static UnityEvent<string> ResetSpring = new UnityEvent<string>();
    [SerializeField] public static UnityEvent<string> RefreshSheets = new UnityEvent<string>();
    [SerializeField] public static UnityEvent RefreshChain = new UnityEvent();

    [SerializeField] public static UnityEvent CloseBuildWindow = new UnityEvent();
    [SerializeField] public static UnityEvent ResetCompendium = new UnityEvent();
    [SerializeField] public static UnityEvent ResetTrackCheck = new UnityEvent();
    [SerializeField] public static UnityEvent<ScenePlayer, string> CheckLevelUpPrefabs = new UnityEvent<ScenePlayer, string>();
    [SerializeField] public static UnityEvent<ScenePlayer, string> RefreshEXP = new UnityEvent<ScenePlayer, string>();
    [SerializeField] public static UnityEvent<ScenePlayer, string> Ressurected = new UnityEvent<ScenePlayer, string>();
    [SerializeField] public static UnityEvent<string, string> RefreshAbilityPage = new UnityEvent<string, string>();
    [SerializeField] public static UnityEvent<ItemSelectable> ResetItemSelectable = new UnityEvent<ItemSelectable>();
    [SerializeField] public static UnityEvent<ScenePlayer, string> DeathBroadcast = new UnityEvent<ScenePlayer, string>();
    [SerializeField] public static UnityEvent<ScenePlayer, ItemSelectable> Refreshitem = new UnityEvent<ScenePlayer, ItemSelectable>();
    [SerializeField] public static UnityEvent<ItemSelectable> DestroyInventoryItem = new UnityEvent<ItemSelectable>();
    [SerializeField] public static UnityEvent<string> ImproperCheckText = new UnityEvent<string>();
    [SerializeField] public static UnityEvent<CombatLogEntry> GainEXPCP = new UnityEvent<CombatLogEntry>();
    //Swap spells
    [SerializeField] public static UnityEvent<string, ScenePlayer, SendSpellList> ChangingMOSpellsMatch = new UnityEvent<string, ScenePlayer, SendSpellList>();
    public static UnityEvent ResetTokens = new UnityEvent();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient, string, string, int> ServerCraftRequest = new UnityEvent<NetworkConnectionToClient, string, string, int>();
    [SerializeField] public static UnityEvent CraftReturn = new UnityEvent();
    [SerializeField] public static UnityEvent CloseEndMenu = new UnityEvent();
    //****************************
    //***********Characters*******
    //****************************6
    
    //event for opening combat UI
     public static UnityEvent<string> OpenCharSheetOneCombat = new UnityEvent<string>();
     public static UnityEvent<string> OpenCharSheetTwoCombat = new UnityEvent<string>();
    public static  UnityEvent<MoveUnits> OnCharactersMoved = new UnityEvent<MoveUnits>();
    public static  UnityEvent<MovingObject, List<MovingObject>, Match> OnCharactersFollow = new UnityEvent<MovingObject, List<MovingObject>, Match>();
    public static  UnityEvent<MovingObject, List<MovingObject>, Match> OnCharactersAttack = new UnityEvent<MovingObject, List<MovingObject>, Match>();
    public static  UnityEvent<Match, ScenePlayer, MovingObject, string> TactBuffSpell = new UnityEvent<Match, ScenePlayer, MovingObject, string>();
    public static  UnityEvent<Match, ScenePlayer, MovingObject, string> TactDamageSpell = new UnityEvent<Match, ScenePlayer, MovingObject, string>();
    public static  UnityEvent<Match, ScenePlayer, MovingObject, string> TactHealSpell = new UnityEvent<Match, ScenePlayer, MovingObject, string>();
    public static  UnityEvent<Match, ScenePlayer, Vector2, string> AoeCombatSpell = new UnityEvent<Match, ScenePlayer, Vector2, string>();
    public static UnityEvent<NetworkConnectionToClient> CancelRegistration = new UnityEvent<NetworkConnectionToClient>();

    public static UnityEvent<GameObject> selectedCharacterHighlight = new UnityEvent<GameObject>();
    public static UnityEvent<string, string> HideBuffRemoved = new UnityEvent<string, string>();
    public static UnityEvent AbsorbBuffRemoved = new UnityEvent();
    public static UnityEvent ClearTargetNonCombat = new UnityEvent();
    public static UnityEvent<ScenePlayer, ScenePlayer> requestChallengeServer = new UnityEvent<ScenePlayer, ScenePlayer>();
    public static UnityEvent<ScenePlayer> requestChallengeTarget = new UnityEvent<ScenePlayer>();
    public static UnityEvent<ScenePlayer> requestGroupTarget = new UnityEvent<ScenePlayer>();
    public static UnityEvent<ScenePlayer> requestGuildTarget = new UnityEvent<ScenePlayer>();
    public static UnityEvent<ScenePlayer> requestFriendTarget = new UnityEvent<ScenePlayer>();
    public static UnityEvent<ScenePlayer, ScenePlayer, bool> decisionChallengeServer = new UnityEvent<ScenePlayer, ScenePlayer, bool>();
    public static UnityEvent<ScenePlayer, ScenePlayer> cancelChallengeRequestServer = new UnityEvent<ScenePlayer, ScenePlayer>();

    public static UnityEvent TurnOffTactInteractionUI = new UnityEvent();

    public static UnityEvent<ScenePlayer, ScenePlayer> requestInspectServer = new UnityEvent<ScenePlayer, ScenePlayer>();



    [SerializeField] GameObject playerLobbyUI;
    public GameObject tilePrefab;
    //Movement
    private bool Loading = true;
    [SerializeField] GameObject castBarPrefab;
    [SerializeField] GameObject spellPrefab;
    GameObject SpellQ;
    GameObject SpellE;
    GameObject SpellR;
    GameObject SpellF;
    private const string CastingQ = "CastingQ";
    private const string CastingE = "CastingE";
    private const string CastingR = "CastingR";
    private const string CastingF = "CastingF";
    private const string Selected = "Selected";
    private const string Unselected = "Unselected";
    string MODE = Unselected;
    Coroutine movementCoroutine;
    Coroutine spellCoroutine;
    //Controlling characters
	//[SerializeField] public PlayerCharacter selectedPlayer;
	public Mob selectedMob;
    [SerializeField] public static UnityEvent<Mob>  selectedMobSend = new UnityEvent<Mob>();
    //client receiving characterData
    [SerializeField] public TacticianFullDataMessage TacticianInformationSheet;
    [SerializeField] public List<CharacterFullDataMessage> InformationSheets = new List<CharacterFullDataMessage>();
    [SerializeField] public List<string> ActivePartyList = new List<string>();
    [SerializeField] public List<string> MatchPartyList = new List<string>();
    public Dictionary<string, string> InspectParty = new Dictionary<string, string>();
    private Dictionary<string, SceneNode> sceneNodesDictionary = new Dictionary<string, SceneNode>();
    private Coroutine spriteSwap;
    [SerializeField] public static UnityEvent<NetworkConnectionToClient> DevShutingDownServer = new UnityEvent<NetworkConnectionToClient>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient, float> GetRandomCost = new UnityEvent<NetworkConnectionToClient, float>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient, CraftingListItem> CraftedItemPermissionToBuild = new UnityEvent<NetworkConnectionToClient, CraftingListItem>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient, CraftingListItem> CraftedItemCancel = new UnityEvent<NetworkConnectionToClient, CraftingListItem>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient, string, string> GameMasterCreateList = new UnityEvent<NetworkConnectionToClient, string, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient, string, string> GameMasterTeleport = new UnityEvent<NetworkConnectionToClient, string, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient> GameMasterHeal = new UnityEvent<NetworkConnectionToClient>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient> GameMasterBreakingPointReleased = new UnityEvent<NetworkConnectionToClient>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient, string, string, int> GameMasterCreateItem = new UnityEvent<NetworkConnectionToClient, string, string, int>();
    [SerializeField] public static UnityEvent<string, string, string, int> GameMasterCreateItemForPlayer = new UnityEvent<string, string, string, int>();

    [SerializeField] public static UnityEvent<CraftingListItem> CraftStartedBuildPrefab = new UnityEvent<CraftingListItem>();
    [SerializeField] public static UnityEvent<CraftingListItem> CraftRemovePrefab = new UnityEvent<CraftingListItem>();
    [SerializeField] public static UnityEvent ObjectiveUpdatedQSheet = new UnityEvent();
    [SerializeField] public static UnityEvent<int, string> ObjectiveUpdated = new UnityEvent<int, string>();
    [SerializeField] public static UnityEvent<int, string> QuestCompletedUpdate = new UnityEvent<int, string>();
    //[SerializeField] public static UnityEvent<NetworkConnectionToClient, string, string> FailedToLootItems = new UnityEvent<NetworkConnectionToClient, string, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient> RegisterWalletRequest = new UnityEvent<NetworkConnectionToClient>();
    [SerializeField] public static UnityEvent<ScenePlayer, int, float, float> PlayerJoinedGroup = new UnityEvent<ScenePlayer, int, float, float>();
    [SerializeField] public static UnityEvent<ScenePlayer> PlayerLeftGroup = new UnityEvent<ScenePlayer>();
    [SerializeField] public static UnityEvent<ScenePlayer, float, float> UpdateGroupPlayer = new UnityEvent<ScenePlayer, float, float>();
    [SerializeField] public static UnityEvent<ScenePlayer> PlayerStart = new UnityEvent<ScenePlayer>();
    [SerializeField] public static UnityEvent<ItemSelectable> SendOverflow = new UnityEvent<ItemSelectable>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient, ItemSelectable, string> SendBackOverflow= new UnityEvent<NetworkConnectionToClient, ItemSelectable, string>();
        
    [SerializeField] public static UnityEvent<Match, TurnManager> EndDuelServer = new UnityEvent<Match, TurnManager>();


    [SerializeField] public static UnityEvent<NetworkConnectionToClient, string> CreateGuild = new UnityEvent<NetworkConnectionToClient, string>();

    public static UnityEvent<string> PingUpdate = new UnityEvent<string>();
    [SerializeField] GameObject backgroundSpriteUI;
    [SerializeField] Animator animator;
    private Dictionary<int, List<ScenePlayer>> GroupDictionary = new Dictionary<int, List<ScenePlayer>>();
    public Dictionary<ScenePlayer, (int, int)> UIGroupNumericalReferences = new Dictionary<ScenePlayer, (int, int)>();
    [SerializeField] public static UnityEvent StartPlayerUIGROUP = new UnityEvent();
    [SerializeField] public static UnityEvent<GameObject> TargetSelectedNonCombat = new UnityEvent<GameObject>();
    [SerializeField] public static UnityEvent<float, float, GameObject> TargetSelectedNonCombatUpdateStats = new UnityEvent<float, float, GameObject>();
    [SerializeField] public static UnityEvent<ScenePlayer> PlayerConfirmedTrade = new UnityEvent<ScenePlayer>();
    [SerializeField] public static UnityEvent<ScenePlayer> TradeStarted = new UnityEvent<ScenePlayer>();
    [SerializeField] public static UnityEvent TradeCancelCompletely = new UnityEvent();
    [SerializeField] public static UnityEvent TradeCancelPartially = new UnityEvent();
    [SerializeField] public static UnityEvent<ScenePlayer, string> TradeGoldSetClient = new UnityEvent<ScenePlayer, string>();
    [SerializeField] public static UnityEvent TradeCompletedResetWindow = new UnityEvent();
    [SerializeField] public static UnityEvent TradePartnerAccepted = new UnityEvent();
    [SerializeField] public static UnityEvent<int> DenyRequest = new UnityEvent<int>();
    [SerializeField] public static UnityEvent<ScenePlayer, ScenePlayer> GetPlayerInfo = new UnityEvent<ScenePlayer, ScenePlayer>();
    [SerializeField] public static UnityEvent<ScenePlayer, ScenePlayer> RequestedTrade = new UnityEvent<ScenePlayer, ScenePlayer>();
    [SerializeField] public static UnityEvent<ScenePlayer, string> TradeGoldSet = new UnityEvent<ScenePlayer, string>();
    [SerializeField] public static UnityEvent<ScenePlayer, List<ItemSelectable>> TradeItemsSet = new UnityEvent<ScenePlayer, List<ItemSelectable>>();
    [SerializeField] public static UnityEvent<List<ItemSelectable>> TradeItemsFromPartner = new UnityEvent<List<ItemSelectable>>();
    [SerializeField] public static UnityEvent<ScenePlayer> TradeFinalRequest = new UnityEvent<ScenePlayer>();
    [SerializeField] public static UnityEvent<ScenePlayer> TradeCancelKeepOpen = new UnityEvent<ScenePlayer>();
    [SerializeField] public static UnityEvent<ScenePlayer> CancelTradeCompletely = new UnityEvent<ScenePlayer>();
    [SerializeField] public static UnityEvent<ScenePlayer, bool> TradeConfirm = new UnityEvent<ScenePlayer, bool>();
    [SerializeField] public static UnityEvent<ScenePlayer> UntargetPlayer = new UnityEvent<ScenePlayer>();
    //public Dictionary<int, List<ScenePlayer>> GetFullGroup(){
    //    return GroupDictionary;
    //}
    public NetworkConnectionToClient GetConnection(){
        return this.connectionToClient;
    }
    bool ableToTrade = true;

    bool inRaid = false;
    bool inGroup = false;
    [TargetRpc]
    public void TargetRpcAoeEffectValues(string attacker, string spellName, MovingObject caster, NetworkIdentity[] objectKeys, string[] damageTypes, int[] damageAmounts, bool[] crits){
        Dictionary<MovingObject, (string, int, bool)> objectsDamaged = new Dictionary<MovingObject, (string, int, bool)>();
        for (int i = 0; i < objectKeys.Length; i++){
            MovingObject mo = objectKeys[i].GetComponent<MovingObject>();
            objectsDamaged[mo] = (damageTypes[i], damageAmounts[i], crits[i]);
        }
        foreach(var obj in objectsDamaged){
            obj.Key.SpawnDmgEffect(spellName, attacker, obj.Value.Item2.ToString(), obj.Value.Item1, obj.Value.Item3);
        }
       // ItemAssets.Instance.CastingSpellAnimationAOE(spellName, caster, animationStart);
        // Now you can use objectsDamaged as needed
    }
    public void SetTracker(string charID, string tracking){
        CmdSetTracking(charID, tracking);
    }
    [Command]
    void CmdSetTracking(string charID, string tracking){
        Dictionary<string, string> possibleSelection = new Dictionary<string, string>();
        foreach(var sheet in InformationSheets){
            bool noMP = true;
            bool hasName = false;
            bool hasTrack = false;
            bool hasMP = false;
            bool hasDeath = false;
            bool dead = false;
            string charName = string.Empty;
            foreach(var stat in sheet.CharStatData){
                if(stat.Key == "Class"){
                    if(stat.Value == "Archer"){
                        foreach(var spell in sheet.CharSpellData){
                            if(spell.Key == "EastT2BottomSkill"){//track
                                hasTrack = true;
                                break;
                            }
                        }
                    } 
                }
                if(stat.Key == "DEATH"){
                    dead = true;
                    hasDeath = true;
                }
                if(stat.Key == "currentMP"){
                    if(int.Parse(stat.Value) > 0){
                        noMP = false;
                    }
                    hasMP = true;
                }
                if(stat.Key == "CharName"){
                    charName = stat.Value;
                    hasName = true;
                }
                if(hasName && hasMP && hasTrack && hasDeath){
                    break;
                }
            }
            if(dead){
                continue;
            }
            if(noMP){
                continue;
            }
            if(!hasTrack){
                continue;
            }
            bool onCD = false;
            foreach (var coolies in sheet.CharCooldownData){
                DateTime initialTime = DateTime.UtcNow;
                DateTime completedTime = DateTime.Parse(coolies.Value, null, System.Globalization.DateTimeStyles.RoundtripKind);
                if (initialTime < completedTime)
                {
                    var nameMatch = System.Text.RegularExpressions.Regex.Match(coolies.SpellnameFull, @"^\D*");
                    string spell = nameMatch.Value.Trim(); 
                    if(spell == "Track"){
                        onCD = true;
                        break;
                    }
                }
            }
            if(onCD){
                continue;
            }
            possibleSelection.Add(sheet.CharacterID, charName);
        }
        if(possibleSelection.ContainsKey(charID)){//validated tracker
            ServerSetTracking(charID, tracking);
        }
    }
    public void EndTracker(string charID){
        CmdEndTracker(charID);
    }
    [Command]
    public void CmdEndTracker(string charID){
        ServerEndTracker(charID);
    }
    [Server]
    void ServerEndTracker(string charID){
        CharacterStatListItem trackingContainer = (new CharacterStatListItem {
            Key = "trackingData",
            Value = "None"
        });
        int sheetIndex = -1;
        int HPIndex = -1;
        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                {
                    if (InformationSheets[i].CharStatData[s].Key == trackingContainer.Key)
                    {
                        HPIndex = s;
                        break;
                    }
                }
                if (HPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(HPIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(trackingContainer);

                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                break;
            }
        }
        TargetEndTracker(charID, trackingContainer);
    }
    [TargetRpc]
    void TargetEndTracker(string charID, CharacterStatListItem trackingContainer){
        int sheetIndex = -1;
        int HPIndex = -1;
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                {
                    if (InformationSheets[i].CharStatData[s].Key == trackingContainer.Key)
                    {
                        HPIndex = s;
                        break;
                    }
                }
                if (HPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(HPIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(trackingContainer);

                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                break;
            }
        }
        ResetTrackCheck.Invoke();
    }
    [Server]
    void ServerSetTracking(string charID, string tracking){
        string _spellName = string.Empty;
        float cdReduction = 0f;
        string mpvalue = string.Empty;
        foreach(var sheet in InformationSheets){
            if(sheet.CharacterID != charID){
                continue;
            }
            foreach(var stat in sheet.CharStatData){
                if(stat.Key == "currentMP"){
                    mpvalue = stat.Value;
                    break;
                }
            }
            foreach(var spellOption in sheet.CharSpellData){
                if(spellOption.Key == "EastT2BottomSkill"){//track
                    _spellName = spellOption.Value;
                }
                /*
                if(spellOption.Key == "SPELLQ"){
                   if(spellOption.Value != "None"){
                        var _nameMatch = Regex.Match(spellOption.Value, @"^\D*");
                        string spellQ = _nameMatch.Value.Trim();
                        if(spellQ == "Track"){
                            spellPosition = spellOption.Key;
                            PKeyPosition = "COOLDOWNQ";
                        }
                   }
                }
                if(spellOption.Key == "SPELLE"){
                   if(spellOption.Value != "None"){
                        var _nameMatch = Regex.Match(spellOption.Value, @"^\D*");
                        string spellE = _nameMatch.Value.Trim();
                        if(spellE == "Track"){
                            spellPosition = spellOption.Key;
                            PKeyPosition = "COOLDOWNE";
                        }
                   }
                }
                if(spellOption.Key == "SPELLR"){
                   if(spellOption.Value != "None"){
                        var _nameMatch = Regex.Match(spellOption.Value, @"^\D*");
                        string spellR = _nameMatch.Value.Trim();
                        if(spellR == "Track"){
                            spellPosition = spellOption.Key;
                            PKeyPosition = "COOLDOWNR";
                        }
                   }
                }
                if(spellOption.Key == "SPELLF"){
                   if(spellOption.Value != "None"){
                        var _nameMatch = Regex.Match(spellOption.Value, @"^\D*");
                        string spellF = _nameMatch.Value.Trim();
                        if(spellF == "Track"){
                            spellPosition = spellOption.Key;
                            PKeyPosition = "COOLDOWNF";
                        }
                   }
                }
                */
                if(spellOption.Key == "SouthT3EndSkill"){
                    var abilityRankString = Regex.Match(spellOption.Value, @"\d+$");
                    if (abilityRankString.Success) {
                        int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                        cdReduction = abilityRank * 0.2f;
                    }
                }
            }
        }
        int newMPValue = int.Parse(mpvalue) - 1;
        var nameMatch = Regex.Match(_spellName, @"^\D*");
        string spell = nameMatch.Value.Trim(); 
        int _spellRank = 1;
        // Extract spell rank
        var rankMatch = Regex.Match(_spellName, @"\d+$");
        if (rankMatch.Success) {
            _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
        }
        float duration = StatAsset.Instance.GetSpellCooldown(spell, _spellRank, cdReduction);
        string dateTimeWithZone = DateTime.UtcNow.ToString("o");
        DateTime timeNow = DateTime.Parse(dateTimeWithZone, null, System.Globalization.DateTimeStyles.RoundtripKind);
	    DateTime updatedTime = timeNow.AddSeconds(duration); // Add duration in seconds
	    //DateTime initialTime = DateTime.Parse(dateTimeWithZone); // Convert to DateTime object
	    //DateTime updatedTime = initialTime.AddSeconds(duration); // Add duration in seconds
	    string newDateTimeWithZone = updatedTime.ToString("o");
	    print("NEW DATA TIME IS " + newDateTimeWithZone + " FOR OUR COOLDOWN SPELL Q");
        
	    CharacterCooldownListItem coolie = (new CharacterCooldownListItem {
	    	SpellnameFull = _spellName,
	    	Value = newDateTimeWithZone
	    });
        ServerCooldownSave(charID, coolie);
        CharacterStatListItem trackingContainer = (new CharacterStatListItem {
            Key = "trackingData",
            Value = tracking
        });
        CharacterStatListItem MP = (new CharacterStatListItem{
            Key = "currentMP",
            Value = newMPValue.ToString()
        });
        int sheetIndex = -1;
        int HPIndex = -1;
        int MPIndex = -1;
        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                {
                    if (InformationSheets[i].CharStatData[s].Key == trackingContainer.Key)
                    {
                        HPIndex = s;
                        break;
                    }
                }
                if (HPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(HPIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(trackingContainer);

                for (int X = 0; X < InformationSheets[i].CharStatData.Count; X++)
                {
                    if (InformationSheets[i].CharStatData[X].Key == MP.Key)
                    {
                        MPIndex = X;
                        break;
                    }
                }
                if (MPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(MPIndex);
                }

                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(MP);
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                TargetUpdateTracking(charID, trackingContainer, MP);
                break;
            }
        }
        //tracking use this to save the mob name 
        
    }
    [TargetRpc]
    public void TargetUpdateTracking(string charID, CharacterStatListItem trackingContainer, CharacterStatListItem MP){
        int sheetIndex = -1;
        int HPIndex = -1;
        int MPIndex = -1;
        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                {
                    if (InformationSheets[i].CharStatData[s].Key == trackingContainer.Key)
                    {
                        HPIndex = s;
                        break;
                    }
                }
                if (HPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(HPIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(trackingContainer);

                for (int X = 0; X < InformationSheets[i].CharStatData.Count; X++)
                {
                    if (InformationSheets[i].CharStatData[X].Key == MP.Key)
                    {
                        MPIndex = X;
                        break;
                    }
                }
                if (MPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(MPIndex);
                }

                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(MP);
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                RefreshSheets.Invoke( charID);
                ResetTrackCheck.Invoke();
                break;
            }
        }
    }
    
    public void SplitCombinedValue(string combinedValue, out string value, out string spellNameFull)
    {
        string[] parts = combinedValue.Split(';'); // Replace with the delimiter you used

        if (parts.Length != 2)
        {
            throw new ArgumentException("Invalid combined value");
        }

        value = parts[0];
        spellNameFull = parts[1];
    }
    //miniMapClick
    public void MovePlayerPaused(Vector3 clickPosition, PointerEventData data, bool shift){
        StartCoroutine(PausedPlayerMove(clickPosition, data, shift));
    }
    IEnumerator PausedPlayerMove(Vector3 clickPosition, PointerEventData data, bool shift){
        yield return new WaitForSeconds(.1f);
        MovePlayerToPosition(clickPosition, data, shift);
    }
    public void SetPentagonFormation(){
        PlayerPrefs.SetString("FormationMode", "Pentagon");
    }
    public void SetTriangleFormation(){
        PlayerPrefs.SetString("FormationMode", "Triangle");
    }
    public void SetLineFormation(){
        PlayerPrefs.SetString("FormationMode", "Rectangle");
    }
    public void SetTeeFormation(){
        PlayerPrefs.SetString("FormationMode", "Tee");
    }
    string GetFormationMode(){
        string mode = "Pentagon";
        mode = PlayerPrefs.GetString("FormationMode", "Pentagon");
        return mode;
    }
    public bool CheckForCombatZone(string data){
        return false;
    }
    public void MoveCameraToPositionOfClickCombatZone(Vector3 clickPosition){
        cameraController.transform.position = clickPosition;
    }
    public void MovePlayerToPosition(Vector3 clickPosition, PointerEventData data, bool shift){
        //print($"Starting mouse click on minimap with position of {clickPosition}");
        if(StatAsset.Instance.CheckForCombatZone(currentScene)){
                bool mobOnly = false;
            bool _Green = true;
                Vector2 targetPos = clickPosition;

                if(selectedCharacters.Count > 0){
                    List<MovingObject> MOs = new List<MovingObject>();
                    Dictionary<MovingObject, int> siblingIndices = new Dictionary<MovingObject, int>();
                    foreach(var selectedChar in selectedCharacters){
                        MovingObject pc = selectedChar.GetComponent<MovingObject>();
                        
                        MOs.Add(pc);
                    }
                    foreach(Transform child in CombatPartyView.instance.transform){
                        CharacterCombatUI _char = child.GetComponent<CharacterCombatUI>();
                        MovingObject charOwner = _char.owner;
                        if (MOs.Contains(charOwner)) {
                            siblingIndices[charOwner] = child.GetSiblingIndex();
                        }
                    }
                    // Sort MOs based on sibling indices
                    MOs = MOs.OrderBy(mo => siblingIndices.ContainsKey(mo) ? siblingIndices[mo] : int.MaxValue).ToList();

                    //MOs = MOs.OrderBy(mo => siblingIndices[mo]).ToList();
                    RaycastHit2D hit = Physics2D.Raycast(targetPos, Vector2.zero, 0f, LayerMask.GetMask("movingObjects"));
                    if (hit.collider == null){
                        // The target position is not blocked
                        RaycastHit2D Floor = Physics2D.Raycast(targetPos, Vector2.zero, 0f, LayerMask.GetMask("Floor"));
                        if(Floor.collider != null){
                            RaycastHit2D Wall = Physics2D.Raycast(targetPos, Vector2.zero, 0f, LayerMask.GetMask("blockingLayer"));
                            if(Wall.collider == null){
                                //check if one or multiple
                                if (shift){
                                    foreach(var unit in MOs){
                                        PlayerCharacter moPC = unit.GetComponent<PlayerCharacter>();
                                        if(moPC){
                                            if(AwaitingHarvestDictionary.ContainsKey(moPC.CharID)){
                                                if(AwaitingHarvestDictionary[moPC.CharID] != null){
                                                    StopCoroutine(AwaitingHarvestDictionary[moPC.CharID]);
                                                }
                                                AwaitingHarvestDictionary[moPC.CharID] = null;
                                                AwaitingHarvestDictionary.Remove(moPC.CharID);
                                            }
                                        }
                                    }
                                    for(int units = 0; units < MOs.Count; units++){
                                        if(MOs.Count == 1){
                                            Mob premobChecker = MOs[units].GetComponent<Mob>();
                                            if(premobChecker){
                                                mobOnly = true;
                                            }
                                        }
                                        if(MOs[units].Dying || MOs[units].Feared || MOs[units].Stunned || MOs[units].Mesmerized || mobOnly){
                                            PlayerCharacter pcChecker = MOs[units].GetComponent<PlayerCharacter>();
                                            Mob mobChecker = MOs[units].GetComponent<Mob>();
                                            string nameCheck = string.Empty;
                                            if(pcChecker){
                                                nameCheck = pcChecker.CharacterName;
                                            }
                                            if(mobChecker){
                                                nameCheck = mobChecker.NAME;
                                            }
                                            if(MOs[units].Dying){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} while dead");
                                            }
                                            if(MOs[units].Feared){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} while feared");
                                            }
                                            if(MOs[units].Stunned){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} while stunned");
                                            }
                                            if(MOs[units].Mesmerized){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} while unconscious");
                                            }
                                            if(mobOnly){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} without the owners command");
                                            }
                                            MOs.Remove(MOs[units]);
                                            
                                            //add name of player characrter or mob name and reason why it cant move
                                        }
                                    }
                                    if(MOs.Count == 0){
                                        //no one can move
                                        return;
                                    }
                                    CmdMoveUnits(MOs, targetPos, GetFormationMode());
                                } else {
                                    MovingObject selectedMember = CombatPartyView.instance.GetSelected();
                                    if(selectedMember){
                                        if(selectedMember.GetComponent<NetworkIdentity>().hasAuthority){
                                            if(selectedCharacters.Contains(selectedMember.gameObject)){
                                                //we can operate now
                                                List<MovingObject> singleList = new List<MovingObject>();
                                                singleList.Add(selectedMember);
                                                foreach(var unit in singleList){
                                                    PlayerCharacter moPC = unit.GetComponent<PlayerCharacter>();
                                                    if(moPC){
                                                        if(AwaitingHarvestDictionary.ContainsKey(moPC.CharID)){
                                                            if(AwaitingHarvestDictionary[moPC.CharID] != null){
                                                                StopCoroutine(AwaitingHarvestDictionary[moPC.CharID]);
                                                            }
                                                            AwaitingHarvestDictionary[moPC.CharID] = null;
                                                            AwaitingHarvestDictionary.Remove(moPC.CharID);
                                                        }
                                                    }
                                                }
                                                for(int unit = 0; unit < singleList.Count; unit++){
                                                    if(singleList.Count == 1){
                                                    Mob premobChecker = singleList[unit].GetComponent<Mob>();
                                                        if(premobChecker){
                                                            mobOnly = true;
                                                        }
                                                    }
                                                    if(singleList[unit].Dying || singleList[unit].Feared || singleList[unit].Stunned || singleList[unit].Mesmerized || mobOnly){
                                                        PlayerCharacter pcChecker = singleList[unit].GetComponent<PlayerCharacter>();
                                                        Mob mobChecker = singleList[unit].GetComponent<Mob>();
                                                        string nameCheck = string.Empty;
                                                        if(pcChecker){
                                                            nameCheck = pcChecker.CharacterName;
                                                        }
                                                        if(mobChecker){
                                                            nameCheck = mobChecker.NAME;
                                                        }
                                                        if(singleList[unit].Dying){
                                                            ImproperCheckText.Invoke($"Cannot move {nameCheck} while dead");
                                                        }
                                                        if(singleList[unit].Feared){
                                                            ImproperCheckText.Invoke($"Cannot move {nameCheck} while feared");
                                                        }
                                                        if(singleList[unit].Stunned){
                                                            ImproperCheckText.Invoke($"Cannot move {nameCheck} while stunned");
                                                        }
                                                        if(singleList[unit].Mesmerized){
                                                            ImproperCheckText.Invoke($"Cannot move {nameCheck} while unconscious");
                                                        }
                                                        if(mobOnly){
                                                            ImproperCheckText.Invoke($"Cannot move {nameCheck} without the owners command");
                                                        }
                                                        singleList.Remove(singleList[unit]);
                                                    }
                                                }
                                                if(singleList.Count == 0){
                                                    //no one can move
                                                    return;
                                                }
                                                CmdMoveUnits(singleList, targetPos, GetFormationMode());
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    } else {
                        MovingObject  selectedTarget = hit.collider.gameObject.GetComponent<MovingObject>();
                        if(!selectedTarget){
                            return;
                        }
                        if(selectedTarget.Dying){
                            return;
                        }
                        if (shift){
                            foreach(var selectedChar in selectedCharacters){
                                MovingObject pc = selectedChar.GetComponent<MovingObject>();
                                if(selectedChar.GetComponent<NetworkIdentity>().hasAuthority)
                                pc.CmdSetTarget(selectedTarget);
                                //MOs.Add(pc);
                            }
                        }
                        if(hit.collider.gameObject.tag == "Character"){
                                //check if one or multiple
                            if (shift){
                                foreach(var unit in MOs){
                                        PlayerCharacter moPC = unit.GetComponent<PlayerCharacter>();
                                        if(moPC){
                                            if(AwaitingHarvestDictionary.ContainsKey(moPC.CharID)){
                                                if(AwaitingHarvestDictionary[moPC.CharID] != null){
                                                    StopCoroutine(AwaitingHarvestDictionary[moPC.CharID]);
                                                }
                                                AwaitingHarvestDictionary[moPC.CharID] = null;
                                                AwaitingHarvestDictionary.Remove(moPC.CharID);
                                            }
                                        }
                                    }
                                    for(int units = 0; units < MOs.Count; units++){
                                        if(MOs.Count == 1){
                                            Mob premobChecker = MOs[units].GetComponent<Mob>();
                                            if(premobChecker){
                                                mobOnly = true;
                                            }
                                        }
                                        if(MOs[units].Dying || MOs[units].Feared || MOs[units].Stunned || MOs[units].Mesmerized || mobOnly){
                                            PlayerCharacter pcChecker = MOs[units].GetComponent<PlayerCharacter>();
                                            Mob mobChecker = MOs[units].GetComponent<Mob>();
                                            string nameCheck = string.Empty;
                                            if(pcChecker){
                                                nameCheck = pcChecker.CharacterName;
                                            }
                                            if(mobChecker){
                                                nameCheck = mobChecker.NAME;
                                            }
                                            if(MOs[units].Dying){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} while dead");
                                            }
                                            if(MOs[units].Feared){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} while feared");
                                            }
                                            if(MOs[units].Stunned){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} while stunned");
                                            }
                                            if(MOs[units].Mesmerized){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} while unconscious");
                                            }
                                            if(mobOnly){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} without the owners command");
                                            }
                                            MOs.Remove(MOs[units]);
                                            
                                            //add name of player characrter or mob name and reason why it cant move
                                        }
                                    }
                                    if(MOs.Count == 0){
                                        //no one can move
                                        return;
                                    }
                                    //attack or move to
                                    MovingObject hitMO = hit.collider.gameObject.GetComponent<MovingObject>();
                                    if(hitMO){
                                        if(FriendlyList.Contains(hitMO)){
                                            CmdMoveUnits(MOs, targetPos, GetFormationMode());//change to follow
                                        } else {
                                            CmdAttackUnit(MOs, hitMO);//change to autoattack
                                        }

                                    }
                            } else {
                                MovingObject selectedMember = CombatPartyView.instance.GetSelected();
                                if(selectedMember){
                                    if(selectedMember.GetComponent<NetworkIdentity>().hasAuthority){
                                        if(selectedCharacters.Contains(selectedMember.gameObject)){
                                            //we can operate now
                                            List<MovingObject> singleList = new List<MovingObject>();
                                            singleList.Add(selectedMember);
                                            foreach(var unit in singleList){
                                                    PlayerCharacter moPC = unit.GetComponent<PlayerCharacter>();
                                                    if(moPC){
                                                        if(AwaitingHarvestDictionary.ContainsKey(moPC.CharID)){
                                                            if(AwaitingHarvestDictionary[moPC.CharID] != null){
                                                                StopCoroutine(AwaitingHarvestDictionary[moPC.CharID]);
                                                            }
                                                            AwaitingHarvestDictionary[moPC.CharID] = null;
                                                            AwaitingHarvestDictionary.Remove(moPC.CharID);
                                                        }
                                                    }
                                                }
                                            selectedMember.CmdSetTarget(selectedTarget);
                                            for(int unit = 0; unit < singleList.Count; unit++){
                                                if(singleList.Count == 1){
                                                    Mob premobChecker = singleList[unit].GetComponent<Mob>();
                                                    if(premobChecker){
                                                        mobOnly = true;
                                                    }
                                                }
                                                if(singleList[unit].Dying || singleList[unit].Feared || singleList[unit].Stunned || singleList[unit].Mesmerized || mobOnly){
                                                    PlayerCharacter pcChecker = singleList[unit].GetComponent<PlayerCharacter>();
                                                    Mob mobChecker = singleList[unit].GetComponent<Mob>();
                                                    string nameCheck = string.Empty;
                                                    if(pcChecker){
                                                        nameCheck = pcChecker.CharacterName;
                                                    }
                                                    if(mobChecker){
                                                        nameCheck = mobChecker.NAME;
                                                    }
                                                    if(singleList[unit].Dying){
                                                        ImproperCheckText.Invoke($"Cannot move {nameCheck} while dead");
                                                    }
                                                    if(singleList[unit].Feared){
                                                        ImproperCheckText.Invoke($"Cannot move {nameCheck} while feared");
                                                    }
                                                    if(singleList[unit].Stunned){
                                                        ImproperCheckText.Invoke($"Cannot move {nameCheck} while stunned");
                                                    }
                                                    if(singleList[unit].Mesmerized){
                                                        ImproperCheckText.Invoke($"Cannot move {nameCheck} while unconscious");
                                                    }
                                                    if(mobOnly){
                                                        ImproperCheckText.Invoke($"Cannot move {nameCheck} without the owners command");
                                                    }
                                                    singleList.Remove(singleList[unit]);
                                                }
                                            }
                                            if(singleList.Count == 0){
                                                //no one can move
                                                return;
                                            }
                                            MovingObject hitMO = hit.collider.gameObject.GetComponent<MovingObject>();
                                            if(hitMO){
                                                if(FriendlyList.Contains(hitMO)){
                                                    CmdMoveUnits(singleList, targetPos, GetFormationMode());
                                                } else {
                                                    CmdAttackUnit(singleList, hitMO);//change to autoattack
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        } else if(hit.collider.gameObject.tag == "Enemy"){
                            SpriteRenderer sRend = hit.collider.gameObject.GetComponent<SpriteRenderer>();
                            if(sRend){
                                if(sRend.enabled){
                                    _Green = false;
                                }
                            }
                            //check if one or multiple
                            if (shift){
                                foreach(var unit in MOs){
                                        PlayerCharacter moPC = unit.GetComponent<PlayerCharacter>();
                                        if(moPC){
                                            if(AwaitingHarvestDictionary.ContainsKey(moPC.CharID)){
                                                if(AwaitingHarvestDictionary[moPC.CharID] != null){
                                                    StopCoroutine(AwaitingHarvestDictionary[moPC.CharID]);
                                                }
                                                AwaitingHarvestDictionary[moPC.CharID] = null;
                                                AwaitingHarvestDictionary.Remove(moPC.CharID);
                                            }
                                        }
                                    }
                                    for(int units = 0; units < MOs.Count; units++){
                                        if(MOs.Count == 1){
                                            Mob premobChecker = MOs[units].GetComponent<Mob>();
                                            if(premobChecker){
                                                mobOnly = true;
                                            }
                                        }
                                        if(MOs[units].Dying || MOs[units].Feared || MOs[units].Stunned || MOs[units].Mesmerized || mobOnly){
                                            PlayerCharacter pcChecker = MOs[units].GetComponent<PlayerCharacter>();
                                            Mob mobChecker = MOs[units].GetComponent<Mob>();
                                            string nameCheck = string.Empty;
                                            if(pcChecker){
                                                nameCheck = pcChecker.CharacterName;
                                            }
                                            if(mobChecker){
                                                nameCheck = mobChecker.NAME;
                                            }
                                            if(MOs[units].Dying){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} while dead");
                                            }
                                            if(MOs[units].Feared){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} while feared");
                                            }
                                            if(MOs[units].Stunned){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} while stunned");
                                            }
                                            if(MOs[units].Mesmerized){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} while unconscious");
                                            }
                                            if(mobOnly){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} without the owners command");
                                            }
                                            MOs.Remove(MOs[units]);
                                            
                                            //add name of player characrter or mob name and reason why it cant move
                                        }
                                    }
                                    if(MOs.Count == 0){
                                        //no one can move
                                        return;
                                    }
                                    if(selectedTarget){
                                        if(FriendlyList.Contains(selectedTarget)){
                                            CmdMoveUnits(MOs, targetPos, GetFormationMode());
                                        } else {
                                            CmdAttackUnit(MOs, selectedTarget);//change to autoattack
                                        }
                                    }
                                    
                            } else {
                                MovingObject selectedMember = CombatPartyView.instance.GetSelected();
                                if(selectedMember){
                                    if(selectedMember.GetComponent<NetworkIdentity>().hasAuthority){
                                        if(selectedCharacters.Contains(selectedMember.gameObject)){
                                            //we can operate now
                                            List<MovingObject> singleList = new List<MovingObject>();
                                            singleList.Add(selectedMember);
                                            selectedMember.CmdSetTarget(selectedTarget);
                                            foreach(var unit in singleList){
                                                    PlayerCharacter moPC = unit.GetComponent<PlayerCharacter>();
                                                    if(moPC){
                                                        if(AwaitingHarvestDictionary.ContainsKey(moPC.CharID)){
                                                            if(AwaitingHarvestDictionary[moPC.CharID] != null){
                                                                StopCoroutine(AwaitingHarvestDictionary[moPC.CharID]);
                                                            }
                                                            AwaitingHarvestDictionary[moPC.CharID] = null;
                                                            AwaitingHarvestDictionary.Remove(moPC.CharID);
                                                        }
                                                    }
                                                }
                                            for(int unit = 0; unit < singleList.Count; unit++){
                                                if(singleList.Count == 1){
                                                    Mob premobChecker = singleList[unit].GetComponent<Mob>();
                                                    if(premobChecker){
                                                        mobOnly = true;
                                                    }
                                                }
                                                if(singleList[unit].Dying || singleList[unit].Feared || singleList[unit].Stunned || singleList[unit].Mesmerized || mobOnly){
                                                    PlayerCharacter pcChecker = singleList[unit].GetComponent<PlayerCharacter>();
                                                    Mob mobChecker = singleList[unit].GetComponent<Mob>();
                                                    string nameCheck = string.Empty;
                                                    if(pcChecker){
                                                        nameCheck = pcChecker.CharacterName;
                                                    }
                                                    if(mobChecker){
                                                        nameCheck = mobChecker.NAME;
                                                    }
                                                    if(singleList[unit].Dying){
                                                        ImproperCheckText.Invoke($"Cannot move {nameCheck} while dead");
                                                    }
                                                    if(singleList[unit].Feared){
                                                        ImproperCheckText.Invoke($"Cannot move {nameCheck} while feared");
                                                    }
                                                    if(singleList[unit].Stunned){
                                                        ImproperCheckText.Invoke($"Cannot move {nameCheck} while stunned");
                                                    }
                                                    if(singleList[unit].Mesmerized){
                                                        ImproperCheckText.Invoke($"Cannot move {nameCheck} while unconscious");
                                                    }
                                                    if(mobOnly){
                                                        ImproperCheckText.Invoke($"Cannot move {nameCheck} without the owners command");
                                                    }
                                                    singleList.Remove(singleList[unit]);
                                                }
                                            }
                                            if(singleList.Count == 0){
                                                //no one can move
                                                return;
                                            }
                                            if(selectedTarget){
                                                if(FriendlyList.Contains(selectedTarget)){
                                                    CmdMoveUnits(singleList, targetPos, GetFormationMode());
                                                } else {
                                                    CmdAttackUnit(singleList, selectedTarget);//change to autoattack
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //selectedTarget.TargettedMO();
                        //CombatPartyView.instance.Retargetter(selectedTarget);
                    }
                }
                Vector3 target3D = clickPosition;
                target3D.z += 10;
                //GameObject rightclick = Instantiate(rightClickPrefab, target3D, Quaternion.identity);
                GameObject rightclick = GetObject(target3D);
                RightClickAnimation RCA = rightclick.GetComponent<RightClickAnimation>();
                if(_Green){
                    RCA.StartGreenSequence();
                } else {
                    RCA.StartRedSequence();
                }
                NewTarget.Invoke();
            
        } else {
        print($"Starting mouse click on minimap with position of {clickPosition} non combat");
            
               
                //// Create a list to store the objects the raycast hits
                //List<RaycastResult> results = new List<RaycastResult>();
                //// Perform the raycast using the pointer event data
                //EventSystem.current.RaycastAll(data, results);
                //bool ditch = true;
                //// Go through the list and check for the UI object by name
                //foreach (RaycastResult result in results)
                //{
                //    SceneNode node = result.gameObject.GetComponent<SceneNode>();
                //    if(node){
                //        ditch = false;
                //        break;
                //    }
                //}
                //if(ditch){
                //    return;
                //}
            bool Green = true;
            Vector3 target3D = clickPosition;
            target3D.z += 10;
            // Perform a raycast to check for the clicked layer
            RaycastHit2D[] hitInfos = Physics2D.RaycastAll(clickPosition, Vector2.zero);

           // print($"Clicked position is {clickPosition}");

        bool isValidHit = true;
        string possibleFailedTag = string.Empty;
        foreach (RaycastHit2D hitInfo in hitInfos)
        {
            string clickedTag = hitInfo.collider.gameObject.tag;

            //print($"Targetting raycast tag for object {hitInfo.collider.gameObject.name} is tag: {clickedTag}");
            if (string.IsNullOrEmpty(clickedTag) || clickedTag == "Untagged" || clickedTag == "Node" || clickedTag == "WALL" || clickedTag == "GlorySeeker")
            {
                continue;
            }
            // Check if the clicked tag is one that allows entry
            if (!IsTagAllowedToEnter(clickedTag))
            {
                isValidHit = false;
                possibleFailedTag = clickedTag;
                break;
            }
        }
                // Check if the clicked layer is one that allows entry
                if (isValidHit)
                {
                    NavMeshHit hit;
                // Adjust the maxDistance as necessary for your game's scale
                if (NavMesh.SamplePosition(target3D, out hit, 1.0f, NavMesh.AllAreas)) {
                    target3D = hit.position; // Update target3D to the hit position if it's on the NavMesh
                    RaycastHit2D[] recheck = Physics2D.RaycastAll(target3D, Vector2.zero);
                    bool isValidHitCheck = true;
                    string failedTag = string.Empty;
                    foreach (RaycastHit2D hitInfo in recheck)
                    {
                        string recheckTag = hitInfo.collider.gameObject.tag;
                        //print($"Targetting raycast tag for object {hitInfo.collider.gameObject.name} is tag: {recheckTag}");
                        if (string.IsNullOrEmpty(recheckTag) || recheckTag == "Untagged" || recheckTag == "Node" || recheckTag == "WALL" || recheckTag == "GlorySeeker")
                        {
                            continue;
                        }
                        // Check if the clicked tag is one that allows entry
                        if (!IsTagAllowedToEnter(recheckTag))
                        {
                            isValidHitCheck = false;
                            failedTag = recheckTag;
                            break;
                        }
                    }
                    
                    if(!isValidHitCheck){
                        string failedZone = RestrictedName(failedTag);
                        if(failedZone == "Arudine Guild Plot"){
                            ImproperCheckText.Invoke($"You must own {failedZone} or be a member of the guild to enter that space");
                        } else {
                            ImproperCheckText.Invoke($"You must own {failedZone} or have permission from the owner to enter that space");
                        }
                        return;
                    }
                    //GameObject rightclick = Instantiate(rightClickPrefab, target3D, Quaternion.identity);
                    GameObject rightclick = GetObject(target3D);
                    RightClickAnimation RCA = rightclick.GetComponent<RightClickAnimation>();
                    PlayerMovingClicked.Invoke(target3D);
                    if(Green){
                        RCA.StartGreenSequence();
                    } else {
                        RCA.StartRedSequence();
                    }
                    //Vector2 direction = (target3D - transform.position).normalized;
                    //if(Mathf.Abs(direction.x) > Mathf.Abs(direction.y)){
                    //    if(direction.x > 0){
                    //        // Going right
                    //        animator.SetBool("right", true);
                    //        animator.SetBool("left", false);
                    //        animator.SetBool("up", false);
                    //        animator.SetBool("down", false);
                    //        animator.SetBool("stand", false);
                    //    } else {
                    //        // Going left
                    //        animator.SetBool("right", false);
                    //        animator.SetBool("left", true);
                    //        animator.SetBool("up", false);
                    //        animator.SetBool("down", false);
                    //        animator.SetBool("stand", false);
                    //    }
                    //} else {
                    //    if(direction.y > 0){
                    //        // Going up
                    //        animator.SetBool("right", false);
                    //        animator.SetBool("left", false);
                    //        animator.SetBool("up", true);
                    //        animator.SetBool("down", false);
                    //        animator.SetBool("stand", false);
                    //    } else {
                    //        // Going down
                    //        animator.SetBool("right", false);
                    //        animator.SetBool("left", false);
                    //        animator.SetBool("up", false);
                    //        animator.SetBool("down", true);
                    //        animator.SetBool("stand", false);
                    //    }
                    //}
                    //if(canMove){
                        //CmdMovePlayer(target3D);
                    //} 
                        MovementRequestedTactician.Enqueue(target3D);

//                    canMove = false; // Set the flag to prevent further moves.
                } else {
                    // Handle the case where no valid NavMesh point was found close to the clicked position
                    Debug.Log("Clicked point is not on a walkable NavMesh surface.");
                    ImproperCheckText.Invoke("Cannot move there");
                }
                return;
            } else {
                string restrictedZone = RestrictedName(possibleFailedTag);
                if(restrictedZone == "Arudine Guild Plot"){
                    ImproperCheckText.Invoke($"You must own {restrictedZone} or be a member of the guild to enter that space");
                } else {
                    ImproperCheckText.Invoke($"You must own {restrictedZone} or have permission from the owner to enter that space");
                }
                return;
            }
        }
    }
    public List<MovingObject> FriendlyList = new List<MovingObject>();
    public List<MovingObject>  GetFriendlyList(){
        return FriendlyList;
    }
    [Server]
	public void ServerClearFriendly(){
		FriendlyList.Clear();
	}
	[ClientRpc]
	public void RpcClearFriendly(){
		FriendlyList.Clear();
	}
    public bool GetFriendly(MovingObject moCheck){
		if(FriendlyList.Contains(moCheck)){
			//print($"{moCheck.name} was friendly for {this.name}");
			return true;
		}
			//print($"{moCheck.name} was not on our friendly list for {this.name}");
		return false;
	}
    [Server]
    public void ServerSetFriends(List<MovingObject> friendlyList){
        FriendlyList = friendlyList;
    }
    [ClientRpc]
    public void RpcSetFriends(List<MovingObject> friendlyList){
        FriendlyList = friendlyList;
    }
    [Server]
	public void ServerAddFriendly(MovingObject moCheck){
		if(!FriendlyList.Contains(moCheck)){
			FriendlyList.Add(moCheck);
		}
        RpcAddFriendly(moCheck);
	}
	[ClientRpc]
	public void RpcAddFriendly(MovingObject moCheck){
		if(!FriendlyList.Contains(moCheck)){
			FriendlyList.Add(moCheck);
		}
	}
	[Server]
	public void ServerRemoveFriendly(MovingObject moCheck){
		if(FriendlyList.Contains(moCheck)){
			FriendlyList.Remove(moCheck);
		}
        RpcRemoveFriendly(moCheck);
	}
	[ClientRpc]
	public void RpcRemoveFriendly(MovingObject moCheck){
		if(FriendlyList.Contains(moCheck)){
			FriendlyList.Remove(moCheck);
		}
	}
    [Server]
    public void ServerCompletedTradeResetWindows(){
        TargetCompletedTradeResetWindows();
    }
    [TargetRpc]
    void TargetCompletedTradeResetWindows(){
        TradeCompletedResetWindow.Invoke();
    }
    [Server]
    public void ServerTradeParterAccept(){
        TargetTradeParterAccept();
    }
    [TargetRpc]
    void TargetTradeParterAccept(){
        TradePartnerAccepted.Invoke();
    }
    [Server]
    public void ServerCancelTradeCompletely(){
        TargetCancelTradeCompletely();
    }
    [TargetRpc]
    void TargetCancelTradeCompletely(){
        Speaker.Invoke(101);
        TradeCancelCompletely.Invoke();
    }
    [Server]
    public void ServerCancelTradeCompletelyCannotTrade(){
        TargetCancelTradeCompletelyCannotTrade();
    }
    [TargetRpc]
    void TargetCancelTradeCompletelyCannotTrade(){
        Speaker.Invoke(101);
        TradeCancelCompletely.Invoke();
        ImproperCheckText.Invoke("Cannot trade with this target");
    }
     [Server]
    public void ServerCancelTradePartially(){
        TargetCancelTradePartially();
    }
    [TargetRpc]
    void TargetCancelTradePartially(){
        TradeCancelPartially.Invoke();
    }
    public bool TradesAcceptedCheck(){
        bool tradeChecker = false;
        if(TradeSwitch == 1){
            tradeChecker = true;
        }
        return tradeChecker;
    }
    public bool ChallengesAcceptedCheck(){
        bool ChallengeChecker = false;
        if(ChallengeSwitch == 1){
            ChallengeChecker = true;
        }
        return ChallengeChecker;
    }
    public bool InspectsAcceptedCheck(){
        bool InspectChecker = false;
        if(InspectSwitch == 1){
            InspectChecker = true;
        }
        return InspectChecker;
    }
    [TargetRpc]
    public void TargetSetGroup(bool toggle){
        inGroup = toggle;

    }
    [TargetRpc]
    public void TargetSetRaid(bool toggle){
        inRaid = toggle;
    }
    public bool GetRaidSetting(){
        return inRaid;
    }
    public bool GetGroupSetting(){
        return inGroup;
    }
    public Dictionary<ScenePlayer, (int, float, float)> GetFullGroups(){
        return GroupsDictionary;
    }
    //build the server methods to send this group info we need to make this magic happen
    private Dictionary<ScenePlayer, (int, float, float)> GroupsDictionary = new Dictionary<ScenePlayer, (int, float, float)>();
    //Get health mana info about a players party
    [Command]
    void CmdReturnInfoTarget(ScenePlayer targetPlayer, ScenePlayer ourPlayer){
        ServerReturnInfoTarget(targetPlayer, ourPlayer);
    }
    [Server]
    void ServerReturnInfoTarget(ScenePlayer targetPlayer, ScenePlayer ourPlayer){
        GetPlayerInfo.Invoke(targetPlayer, ourPlayer);
    }
    [Server]
    public void ServerFinalTargetReturn(ScenePlayer targetPlayer, float health, float mana){
        TargetFinalTargetReturn(targetPlayer, health, mana);
    }
    [TargetRpc]
    void TargetFinalTargetReturn(ScenePlayer targetPlayer, float health, float mana){
        TargetSelectedNonCombatUpdateStats.Invoke(health, mana, targetPlayer.gameObject);
    }
    //request trade
    public void AskToTradeWithPlayer(ScenePlayer targetPlayer){
        if(this != targetPlayer){
            CmdRequestedTrade(targetPlayer);
        }
    }
    [Command]
    void CmdRequestedTrade(ScenePlayer targetPlayer){
        ServerRequestedTrade(targetPlayer);
    }
    [Server]
    void ServerRequestedTrade(ScenePlayer targetPlayer){
        RequestedTrade.Invoke(targetPlayer, this);
    }
    //Started trade with target player
    [Server]
    public void ServerAskToConfirmTrade(ScenePlayer targetPlayer){
        TargetAskToConfirmTrade(targetPlayer);
    }
    [TargetRpc]
    void TargetAskToConfirmTrade(ScenePlayer targetPlayer){
        TradeStarted.Invoke(targetPlayer);
    }
    public void ConfirmTrade(bool confirmation){
        CmdConfirmTrade(confirmation);
    }
    [Command]
    void CmdConfirmTrade(bool confirmation){
        ServerConfirmTrade(confirmation);
    }
    [Server]
    void ServerConfirmTrade(bool confirmation){
        TradeConfirm.Invoke(this, confirmation);
    }
    //ask for the trade
    [Server]
    public void ServerAcceptTradeStart(ScenePlayer targetPlayer){
        TargetAcceptTradeStart(targetPlayer);
    }
    [TargetRpc]
    void TargetAcceptTradeStart(ScenePlayer targetPlayer){
        //tell them that the player is ready
        PlayerConfirmedTrade.Invoke(targetPlayer);
        //tell them to remove the wait for code
    }
    //ask for the tradeServerDenyTradeRequest
    [Server]
    public void ServerDenyTradeRequest(int request){
        TargetDenyTradeRequest(request);
    }
    [TargetRpc]
    void TargetDenyTradeRequest(int request){
        //tell them that the player is ready
        DenyRequest.Invoke(request);
        //tell them to remove the wait for code
    }
    //tell trader how much gold you will give them
    public void AskToSetGoldTradeValue(string goldValue){
        print($"player {playerName} is sending {goldValue} gold offer");
        CmdRequestedSetGoldTradeValue(goldValue);
    }
    [Command]
    void CmdRequestedSetGoldTradeValue(string goldValue){
        print($"player {playerName} is sending {goldValue} gold offer");

        ServerRequestedSetGoldTradeValue(goldValue);
    }
    [Server]
    void ServerRequestedSetGoldTradeValue(string goldValue){
        TradeGoldSet.Invoke(this, goldValue);
    }

    public void AskToSetTradeItems(List<ItemSelectable> itemsOffered){
        CmdRequestedSetTradeItems(itemsOffered);
    }
    [Command]
    void CmdRequestedSetTradeItems(List<ItemSelectable> itemsOffered){

        ServerRequestedSetTradeItems(itemsOffered);
    }
    [Server]
    void ServerRequestedSetTradeItems(List<ItemSelectable> itemsOffered){
        TradeItemsSet.Invoke(this, itemsOffered);
    }
    [Server]
    public void ServerSetGoldTradeValueFromPlayer(ScenePlayer targetPlayer, string goldValue){
        print($"player {targetPlayer.playerName} is sending {goldValue} gold offer");

        TargetSetGoldTradeValueFromPlayer(targetPlayer, goldValue);
    }
    [TargetRpc]
    public void TargetSetGoldTradeValueFromPlayer(ScenePlayer targetPlayer, string goldValue){
        print($"player {targetPlayer.playerName} is sending {goldValue} gold offer");

        TradeGoldSetClient.Invoke(targetPlayer, goldValue);
    }


    [Server]
    public void ServerSetPartnerItems(List<ItemSelectable> items){
        foreach(var item in items){
            print($"sending item {item.GetItemName()}");
        }
        TargetSetPartnerItems(items);
    }
    [TargetRpc]
    public void TargetSetPartnerItems(List<ItemSelectable> items){
        foreach(var item in items){
            print($"sending item {item.GetItemName()}");
        }
        TradeItemsFromPartner.Invoke(items);
    }

    public void AskToTradeFinal(){
        CmdRequestedTradeFinal();
    }
    [Command]
    void CmdRequestedTradeFinal(){
        ServerRequestedTradeFinal();
    }
    [Server]
    void ServerRequestedTradeFinal(){
        TradeFinalRequest.Invoke(this);
    }
    public void AskToCanelButKeepTrade(){
        CmdCancelButKeepTrade();
    }
    [Command]
    void CmdCancelButKeepTrade(){
        ServerCancelButKeepTrade();
    }
    [Server]
    void ServerCancelButKeepTrade(){
        TradeCancelKeepOpen.Invoke(this);
    }

    public void AskToCompletelyCancelTrade(){
        CmdCompletelyCancelTrade();
    }
    [Command]
    void CmdCompletelyCancelTrade(){
        ServerCompletelyCancelTrade();
    }
    [Server]
    void ServerCompletelyCancelTrade(){
        CancelTradeCompletely.Invoke(this);
    }
    public void EndDuelStart(){
        CloseEndMenu.Invoke();
        CmdEndDuelStart();
    }
    [Command]
    public void CmdEndDuelStart(){
        foreach(var mo in FriendlyList){
            if(mo != null){
                TurnManager tm = mo.curatorTM;
                ServerEndDuel(tm);
                return;
            }
        }
    }
    [Server]
    void ServerEndDuel(TurnManager curatorTM){
        EndDuelServer.Invoke(currentMatch, curatorTM);
    }
    public void AskServerForInformationAboutPlayer(ScenePlayer Splayer){
        float healthPercentage = 0;
        float manaPercentage = 0;
        
        if(Splayer == this){
            int tactFortBonus = 0;
            int tactArcanaBonus = 0;
            if(int.TryParse(TacticianInformationSheet.FortitudeBonus, out int parsedFortBonus)){
                tactFortBonus += parsedFortBonus;
            }
            if(int.TryParse(TacticianInformationSheet.ArcanaBonus, out int parsedArcanaBonus)){
                tactArcanaBonus += parsedArcanaBonus;
            }
            for(int i = 0; i < TacticianInformationSheet.TacticianInventoryData.Count; i++){
                if(TacticianInformationSheet.TacticianInventoryData[i].Value.Deleted || TacticianInformationSheet.TacticianInventoryData[i].Value.amount == 0){
                    continue;
                }
                if(TacticianInformationSheet.TacticianInventoryData[i].Value.EQUIPPED){
                    if (!string.IsNullOrEmpty(TacticianInformationSheet.TacticianInventoryData[i].Value.FORTITUDE_item)) {
                        if (int.Parse(TacticianInformationSheet.TacticianInventoryData[i].Value.FORTITUDE_item) > 0) {
                            tactFortBonus += int.Parse(TacticianInformationSheet.TacticianInventoryData[i].Value.FORTITUDE_item);
                        }
                    }
                    if (!string.IsNullOrEmpty(TacticianInformationSheet.TacticianInventoryData[i].Value.ARCANA_item)) {
                        if (int.Parse(TacticianInformationSheet.TacticianInventoryData[i].Value.ARCANA_item) > 0) {
                            tactArcanaBonus += int.Parse(TacticianInformationSheet.TacticianInventoryData[i].Value.ARCANA_item);
                        }
                    }
                }
            }
            if(GetParty().Count != 0){
             foreach(var member in GetParty()){
                foreach(var sheet in GetInformationSheets()){
                    if(sheet.CharacterID == member){
                        bool healthFound = false;
                        bool manaFound = false;
                        bool lvlFound = false;
                        bool classFound = false;
                        bool coreFound = false;

                        float health = 0;
                        float mana = 0;
                        int _level = 1;
                        string _core = "STANDARD";
                        string _class = string.Empty;
                        foreach(var stat in sheet.CharStatData){
                            if(healthFound && manaFound && lvlFound && classFound && coreFound){
                                break;
                            }
                            if(stat.Key == "currentHP"){
                                if(float.TryParse(stat.Value, out float parsedHealth)){
                                    health = parsedHealth;
                                }
                                healthFound = true;
                            }
                            if(stat.Key == "currentMP"){
                                 if(float.TryParse(stat.Value, out float parsedMana)){
                                    mana = parsedMana;
                                }
                                manaFound = true;
                            }
                            if (stat.Key == "Class") {
                                _class = stat.Value;
                            }
                            if (stat.Key == "LVL") {
                                _level = int.Parse(stat.Value);
                            }
                            if (stat.Key == "CORE") {
                                _core = stat.Value;
                            }
                        }
                        int equipHP = 0;
                        int equipArcana = 0;
                        //equipHP += tacticianBonusFortitude;
                        //equipArcana += tacticianBonusArcana;
                        var charInventoryDataList = sheet.CharInventoryData;
                        for (int k = 0; k < charInventoryDataList.Count; k++) {
                            var charItem = charInventoryDataList[k];
                            if (charItem.Value.EQUIPPED) {
                                if (!string.IsNullOrEmpty(charItem.Value.FORTITUDE_item)) {
                                    if (int.Parse(charItem.Value.FORTITUDE_item) > 0) {
                                        equipHP += int.Parse(charItem.Value.FORTITUDE_item);
                                    }
                                }
                                if (!string.IsNullOrEmpty(charItem.Value.ARCANA_item)) {
                                    if (int.Parse(charItem.Value.ARCANA_item) > 0) {
                                        equipArcana += int.Parse(charItem.Value.ARCANA_item);
                                    }
                                }
                            }
                        }
                        int PASSIVE_Agility = 0;
                        int PASSIVE_Arcana = 0;
                        int PASSIVE_Strength = 0;
                        int PASSIVE_Fortitude = 0;
                        int PASSIVE_Resist = 0;
                        float cdReductionPercentage = 0f;
                        for(int _char = 0; _char < GetInformationSheets().Count; _char++){
                            if(GetInformationSheets()[_char].CharacterID == member){
                                for(int ability = 0; ability < GetInformationSheets()[_char].CharSpellData.Count; ability++){
                                    if(GetInformationSheets()[_char].CharSpellData[ability].Key == "SouthT3EndSkill"){
                                        var abilityRankString = System.Text.RegularExpressions.Regex.Match(GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                                        if (abilityRankString.Success) {
                                            int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                                            cdReductionPercentage = abilityRank * 0.2f;
                                        }
                                    }
                                    if(GetInformationSheets()[_char].CharSpellData[ability].Key == "SouthT2LeftSkill"){
                                        var abilityRankString = System.Text.RegularExpressions.Regex.Match(GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                                        if (abilityRankString.Success) {
                                            int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                                            PASSIVE_Strength = abilityRank;
                                        }
                                    }
                                    if(GetInformationSheets()[_char].CharSpellData[ability].Key == "SouthT2MiddleSkill"){
                                        var abilityRankString = System.Text.RegularExpressions.Regex.Match(GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                                        if (abilityRankString.Success) {
                                            int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                                            PASSIVE_Agility = abilityRank;
                                        }
                                    }
                                    if(GetInformationSheets()[_char].CharSpellData[ability].Key == "SouthT2RightSkill"){
                                        var abilityRankString = System.Text.RegularExpressions.Regex.Match(GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                                        if (abilityRankString.Success) {
                                            int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                                            PASSIVE_Resist = abilityRank;
                                        }
                                    }
                                    if(GetInformationSheets()[_char].CharSpellData[ability].Key == "SouthT3LeftSkill"){
                                        var abilityRankString = System.Text.RegularExpressions.Regex.Match(GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                                        if (abilityRankString.Success) {
                                            int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                                            PASSIVE_Fortitude = abilityRank;
                                        }
                                    }
                                    if(GetInformationSheets()[_char].CharSpellData[ability].Key == "SouthT3RightSkill"){
                                        var abilityRankString = System.Text.RegularExpressions.Regex.Match(GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                                        if (abilityRankString.Success) {
                                            int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                                            PASSIVE_Arcana = abilityRank;
                                        }
                                    }
                                }
                                break;
                            }
                        }
                        (int baseStrength, int baseAgility, int baseFortitude, int baseArcana) = StatAsset.Instance.GetCharacterStats(_class, _level, _core);
                        float maxHP = equipHP + baseFortitude + tactFortBonus + PASSIVE_Fortitude;
                        float maxMP = (equipArcana + baseArcana + tactArcanaBonus + PASSIVE_Arcana) / 7;
                        //Get max HP and MP then divide the current by max
                        health /= maxHP;
                        mana /= maxMP;
                        healthPercentage += health;
                        manaPercentage += mana;
                        break;
                    }
                }
            }
            healthPercentage /= GetParty().Count;
            manaPercentage /= GetParty().Count;
            } else {
                healthPercentage = 0f;
                manaPercentage = 0f;
            }
            TargetSelectedNonCombatUpdateStats.Invoke(healthPercentage, manaPercentage, Splayer.gameObject);
            //send a message with the proper stuff for the target
        } else {
            CmdReturnInfoTarget(Splayer, this);
        }
    }
    [Server]
    public void ServerSetPlayerGroup(int groupNumber, ScenePlayer player, float overAllHealth, float overAllMana){//send custom class maybe
        TargetSetPlayerGroup(groupNumber, player, overAllHealth, overAllMana);
    }
    [TargetRpc]
    public void TargetSetPlayerGroup(int groupNumber, ScenePlayer player, float overAllHealth, float overAllMana){
        if(!GroupsDictionary.ContainsKey(player)){
            PlayerJoinedGroup.Invoke(player, groupNumber, overAllHealth, overAllMana);
        } else {
            UpdateGroupPlayer.Invoke(player, overAllHealth, overAllMana);
        }
        GroupsDictionary[player] = (groupNumber, overAllHealth, overAllMana);
    }
    [TargetRpc]
    public void TargetRemovePlayer(ScenePlayer removingPlayer){
        if(GroupsDictionary.ContainsKey(removingPlayer)){
            GroupsDictionary.Remove(removingPlayer);
        }
        PlayerLeftGroup.Invoke(removingPlayer);
    }
    public (float, float, float) GetPlayerGroupUIStats(ScenePlayer sPlayer){
        float healthPercentage = 0;
        float manaPercentage = 0;
        float sliderThreeValue = sPlayer.Energy;
        if(GroupsDictionary.Count > 0){
            foreach(var playerCard in GroupsDictionary){
                if(playerCard.Key == sPlayer){
                    healthPercentage = playerCard.Value.Item2;
                    manaPercentage = playerCard.Value.Item3;
                    break;
                }
            }
        } else {
            int tactFortBonus = 0;
            int tactArcanaBonus = 0;
            if(int.TryParse(TacticianInformationSheet.FortitudeBonus, out int parsedFortBonus)){
                tactFortBonus += parsedFortBonus;
            }
            if(int.TryParse(TacticianInformationSheet.ArcanaBonus, out int parsedArcanaBonus)){
                tactArcanaBonus += parsedArcanaBonus;
            }
            for(int i = 0; i < TacticianInformationSheet.TacticianInventoryData.Count; i++){
                if(TacticianInformationSheet.TacticianInventoryData[i].Value.Deleted || TacticianInformationSheet.TacticianInventoryData[i].Value.amount == 0){
                    continue;
                }
                if(TacticianInformationSheet.TacticianInventoryData[i].Value.EQUIPPED){
                    if (!string.IsNullOrEmpty(TacticianInformationSheet.TacticianInventoryData[i].Value.FORTITUDE_item)) {
                        if (int.Parse(TacticianInformationSheet.TacticianInventoryData[i].Value.FORTITUDE_item) > 0) {
                            tactFortBonus += int.Parse(TacticianInformationSheet.TacticianInventoryData[i].Value.FORTITUDE_item);
                        }
                    }
                    if (!string.IsNullOrEmpty(TacticianInformationSheet.TacticianInventoryData[i].Value.ARCANA_item)) {
                        if (int.Parse(TacticianInformationSheet.TacticianInventoryData[i].Value.ARCANA_item) > 0) {
                            tactArcanaBonus += int.Parse(TacticianInformationSheet.TacticianInventoryData[i].Value.ARCANA_item);
                        }
                    }
                }
            }
            if(GetParty().Count != 0){
             foreach(var member in GetParty()){
                foreach(var sheet in GetInformationSheets()){
                    if(sheet.CharacterID == member){
                        bool healthFound = false;
                        bool manaFound = false;
                        bool lvlFound = false;
                        bool classFound = false;
                        bool coreFound = false;

                        float health = 1;
                        float mana = 1;
                        int _level = 1;
                        string _core = "STANDARD";
                        string _class = string.Empty;
                        foreach(var stat in sheet.CharStatData){
                            if(healthFound && manaFound && lvlFound && classFound && coreFound){
                                break;
                            }
                            if(stat.Key == "currentHP"){
                                if(float.TryParse(stat.Value, out float parsedHealth)){
                                    health = parsedHealth;
                                }
                                healthFound = true;
                            }
                            if(stat.Key == "currentMP"){
                                 if(float.TryParse(stat.Value, out float parsedMana)){
                                    mana = parsedMana;
                                }
                                manaFound = true;
                            }
                            if (stat.Key == "Class") {
                                _class = stat.Value;
                                classFound = true;
                            }
                            if (stat.Key == "LVL") {
                                _level = int.Parse(stat.Value);
                                lvlFound = true;
                            }
                            if (stat.Key == "CORE") {
                                _core = stat.Value;
                                coreFound = true;
                            }
                        }
                        int equipHP = 0;
                        int equipArcana = 0;
                        var charInventoryDataList = sheet.CharInventoryData;
                        for (int k = 0; k < charInventoryDataList.Count; k++) {
                            var charItem = charInventoryDataList[k];
                            if (charItem.Value.EQUIPPED) {
                                if (!string.IsNullOrEmpty(charItem.Value.FORTITUDE_item)) {
                                    if (int.Parse(charItem.Value.FORTITUDE_item) > 0) {
                                        equipHP += int.Parse(charItem.Value.FORTITUDE_item);
                                    }
                                }
                                if (!string.IsNullOrEmpty(charItem.Value.ARCANA_item)) {
                                    if (int.Parse(charItem.Value.ARCANA_item) > 0) {
                                        equipArcana += int.Parse(charItem.Value.ARCANA_item);
                                    }
                                }
                            }
                        }
                        
                        int PASSIVE_Agility = 0;
                        int PASSIVE_Arcana = 0;
                        int PASSIVE_Strength = 0;
                        int PASSIVE_Fortitude = 0;
                        int PASSIVE_Resist = 0;
                        float cdReductionPercentage = 0f;
                        for(int _char = 0; _char < GetInformationSheets().Count; _char++){
                            if(GetInformationSheets()[_char].CharacterID == member){
                                for(int ability = 0; ability < GetInformationSheets()[_char].CharSpellData.Count; ability++){
                                    if(GetInformationSheets()[_char].CharSpellData[ability].Key == "SouthT3EndSkill"){
                                        var abilityRankString = System.Text.RegularExpressions.Regex.Match(GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                                        if (abilityRankString.Success) {
                                            int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                                            cdReductionPercentage = abilityRank * 0.2f;
                                        }
                                    }
                                    if(GetInformationSheets()[_char].CharSpellData[ability].Key == "SouthT2LeftSkill"){
                                        var abilityRankString = System.Text.RegularExpressions.Regex.Match(GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                                        if (abilityRankString.Success) {
                                            int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                                            PASSIVE_Strength = abilityRank;
                                        }
                                    }
                                    if(GetInformationSheets()[_char].CharSpellData[ability].Key == "SouthT2MiddleSkill"){
                                        var abilityRankString = System.Text.RegularExpressions.Regex.Match(GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                                        if (abilityRankString.Success) {
                                            int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                                            PASSIVE_Agility = abilityRank;
                                        }
                                    }
                                    if(GetInformationSheets()[_char].CharSpellData[ability].Key == "SouthT2RightSkill"){
                                        var abilityRankString = System.Text.RegularExpressions.Regex.Match(GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                                        if (abilityRankString.Success) {
                                            int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                                            PASSIVE_Resist = abilityRank;
                                        }
                                    }
                                    if(GetInformationSheets()[_char].CharSpellData[ability].Key == "SouthT3LeftSkill"){
                                        var abilityRankString = System.Text.RegularExpressions.Regex.Match(GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                                        if (abilityRankString.Success) {
                                            int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                                            PASSIVE_Fortitude = abilityRank;
                                        }
                                    }
                                    if(GetInformationSheets()[_char].CharSpellData[ability].Key == "SouthT3RightSkill"){
                                        var abilityRankString = System.Text.RegularExpressions.Regex.Match(GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                                        if (abilityRankString.Success) {
                                            int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                                            PASSIVE_Arcana = abilityRank;
                                        }
                                    }
                                }
                                break;
                            }
                        }
                        (int baseStrength, int baseAgility, int baseFortitude, int baseArcana) = StatAsset.Instance.GetCharacterStats(_class, _level, _core);
                        float maxHP = equipHP + baseFortitude + tactFortBonus + PASSIVE_Fortitude;
                        float maxMP = (equipArcana + baseArcana + tactArcanaBonus + PASSIVE_Arcana) / 7;
                        //Get max HP and MP then divide the current by max
                        health /= maxHP;
                        mana /= maxMP;
                        healthPercentage += health;
                        manaPercentage += mana;

                        break;
                    }
                }
            }
            healthPercentage /= GetParty().Count;
            manaPercentage /= GetParty().Count;
            } else {
                healthPercentage = 0f;
                manaPercentage = 0f;
            }
        }
        
        return (healthPercentage, manaPercentage, sliderThreeValue);
    }
    // Method to add or update a purchase data instance
    public List<ItemSelectable> OverflowItemsChecked = new List<ItemSelectable>();
    [Server]
    //public void ServerStorePurchaseData(string select, NetworkConnectionToClient nconn, string customID, PlayerInfo playerData, ItemSelectable item, Dictionary<string,string> statBookOne, Dictionary<string,string> statBookTwo, Dictionary<string,string> statBookThree, Dictionary<string,string> statBookFour, Dictionary<string,string> statBookFive, Dictionary<string,string> statBookSix, bool login, bool Unstacking)
    public void ServerStorePurchaseData(ItemSelectable item)
    {
        print($"ServerStorePurchaseData Built item {item.GetItemName()} because we had no more space");
        if(!OverflowItemsChecked.Contains(item)){
            OverflowItemsChecked.Add(item);
            TargetBuildItemOverflow(item);
        }
        //build item now
    }
    
    public void RegisterWalletCancelationClient(){
        print($"Starting RegisterWalletCancelationClient");
        CmdRegisterWalletCancelation();
    }
    [Command]
    void CmdRegisterWalletCancelation(){
        ServerRegisterWalletCancelation();
    }
    [Server]
    void ServerRegisterWalletCancelation(){
        print($"Starting ServerRegisterWalletCancelation");
        CancelRegistration.Invoke(this.connectionToClient);
    }
    public void RegisterWalletRequestClient(){
        CmdRegisterWalletRequest();
    }
    [Command]
    void CmdRegisterWalletRequest(){
        ServerRegisterWalletRequest();
    }
    [Server]
    void ServerRegisterWalletRequest(){
        RegisterWalletRequest.Invoke(this.connectionToClient);
    }
    [TargetRpc]
    void TargetBuildItemOverflow(ItemSelectable item){
        //Built item because we had no more space
        SendOverflow.Invoke(item);
        print("Built item because we had no more space");
    }
    public void OverflowLooted(ItemSelectable item, string serial){
        CmdOverflowLooted(item, serial);
    }
    [Command]
    void CmdOverflowLooted(ItemSelectable item, string serial){
        ServerOverflowLooted(item, serial);
    }
    [Server]
    void ServerOverflowLooted(ItemSelectable item, string serial){
        SendBackOverflow.Invoke(this.connectionToClient, item, serial);
    }
    public void OverflowRemoved(ItemSelectable item){
        CmdOverflowRemoved(item);
    }
    [Command]
    void CmdOverflowRemoved(ItemSelectable item){
        ServerOverflowRemoved(item);
    }
    [Server]
    void ServerOverflowRemoved(ItemSelectable item){
        if(OverflowItemsChecked.Contains(item)){
            OverflowItemsChecked.Remove(item);
        }
    }
    // Method to clear all stored purchase data
    [Server]
    public void ServerClearAllPurchaseData()
    {
        //foreach(var removingItem in overflowDataList){
        //    FailedToLootItems.Invoke(this.connectionToClient, removingItem.Value.select, removingItem.Key);
        //}
        OverflowItemsChecked.Clear();
    }

   
    [Server]
    void ServerAwaitingItemChoice(string select, NetworkConnectionToClient nconn, string InstanceID, PlayerInfo playerData, ItemSelectable item, Dictionary<string,string> statBookOne, Dictionary<string,string> statBookTwo, Dictionary<string,string> statBookThree, Dictionary<string,string> statBookFour, Dictionary<string,string> statBookFive, Dictionary<string,string> statBookSix, bool login, bool Unstacking){
        // this is where server gives us the info to make our struct called SignBook whatever in the netmessages script
        //dont forget to unline out the stuff in playfab server for transform into dragonkill method bottom part and then get
        //the removal methods good for if they dont get item in time or if it goes to someone else its going to need to have option to 
        //remove the item from the current owner and then send it to another sorta like a trade
        //this will also need a method to build the item clickable and then have it be able to tell server to build the item for real with the sign books
        // using an event to call the method under transforminto dragonkill method you will see it, just think it through with what we said and you will be fine warrior
    }
    [Server]
    public void ServerBuildTacticianSpellCooldownLogin(float duration, int keyPressed){
        if(keyPressed == 1){
            if(abilityOneCDRoutine != null){
                StopCoroutine(abilityOneCDRoutine);
                abilityOneCDRoutine = null;
            }
            abilityOneCDRoutine = StartCoroutine(SetAbilityCoolDownOne(duration));
        }
        if(keyPressed == 2){
            if(abilityTwoCDRoutine != null){
                StopCoroutine(abilityTwoCDRoutine);
                abilityTwoCDRoutine = null;
            }
            abilityTwoCDRoutine = StartCoroutine(SetAbilityCoolDownTwo(duration));
        }
    }
    [Server]
    public void ServerCheckAllItemsOnSceneSwap(){
        //print("Swapping scene lets do a server check on our items");
        //print("Starting StashInventoryData inv items");
        for(int i = 0; i < TacticianInformationSheet.StashInventoryData.Count; i++){
            if(TacticianInformationSheet.StashInventoryData[i].Value.Deleted || TacticianInformationSheet.StashInventoryData[i].Value.amount == 0){
                //print($"StashInventoryData inv itemm was DELETED {TacticianInformationSheet.StashInventoryData[i].Value.GetItemName()}");
                continue;
            }
            //print($"StashInventoryData inv itemm was not deleted {TacticianInformationSheet.StashInventoryData[i].Value.GetItemName()}");
        }
        //print("Starting TacticianInventoryData inv items");
        for(int o = 0; o < TacticianInformationSheet.TacticianInventoryData.Count; o++){
            if(TacticianInformationSheet.TacticianInventoryData[o].Value.Deleted || TacticianInformationSheet.TacticianInventoryData[o].Value.amount == 0){
                //print($"TacticianInventoryData inv itemm was DELETED {TacticianInformationSheet.TacticianInventoryData[o].Value.GetItemName()}");
                continue;
            }
            //print($"TacticianInventoryData inv itemm was not deleted {TacticianInformationSheet.TacticianInventoryData[o].Value.GetItemName()}");
        }
       // print("Starting InformationSheets inv items");
        for(int p = 0; p < InformationSheets.Count; p++){
            //print($"Starting Informationsheet, {InformationSheets[p].CharacterID}");
            for(int x = 0; x < InformationSheets[p].CharInventoryData.Count; x++){
                if(InformationSheets[p].CharInventoryData[x].Value.Deleted || InformationSheets[p].CharInventoryData[x].Value.amount == 0){
                    //print($"Char itemm was DELETED {InformationSheets[p].CharInventoryData[x].Value.GetItemName()}");
                    continue;
                }
                //print($"Char itemm was not deleted {InformationSheets[p].CharInventoryData[x].Value.GetItemName()}");
            }
        }
        StartCoroutine(SecondCheck());
    }
    [Command]
    void CmdDoubleCheckBTN(){
        ServerCheckAllItemsOnSceneSwapDoubleCheck();
    }
    [Server]
    public void ServerCheckAllItemsOnSceneSwapDoubleCheck(){
        //print("Swapping scene lets do a server check on our items");
        //print("Starting StashInventoryData inv items");
        for(int i = 0; i < TacticianInformationSheet.StashInventoryData.Count; i++){
            if(TacticianInformationSheet.StashInventoryData[i].Value.Deleted || TacticianInformationSheet.StashInventoryData[i].Value.amount == 0){
                //print($"StashInventoryData inv itemm was DELETED {TacticianInformationSheet.StashInventoryData[i].Value.GetItemName()}");
                continue;
            }
            //print($"StashInventoryData inv itemm was not deleted {TacticianInformationSheet.StashInventoryData[i].Value.GetItemName()}");
        }
        //print("Starting TacticianInventoryData inv items");
        for(int o = 0; o < TacticianInformationSheet.TacticianInventoryData.Count; o++){
            if(TacticianInformationSheet.TacticianInventoryData[o].Value.Deleted || TacticianInformationSheet.TacticianInventoryData[o].Value.amount == 0){
                //print($"TacticianInventoryData inv itemm was DELETED {TacticianInformationSheet.TacticianInventoryData[o].Value.GetItemName()}");
                continue;
            }
            //print($"TacticianInventoryData inv itemm was not deleted {TacticianInformationSheet.TacticianInventoryData[o].Value.GetItemName()}");
        }
        //print("Starting InformationSheets inv items");
        for(int p = 0; p < InformationSheets.Count; p++){
            //print($"Starting Informationsheet, {InformationSheets[p].CharacterID}");
            for(int x = 0; x < InformationSheets[p].CharInventoryData.Count; x++){
                if(InformationSheets[p].CharInventoryData[x].Value.Deleted || InformationSheets[p].CharInventoryData[x].Value.amount == 0){
                    //print($"Char itemm was DELETED {InformationSheets[p].CharInventoryData[x].Value.GetItemName()}");
                    continue;
                }
                //print($"Char itemm was not deleted {InformationSheets[p].CharInventoryData[x].Value.GetItemName()}");
            }
        }
    }
    IEnumerator SecondCheck(){
        yield return new WaitForSeconds(5f);
        ServerCheckAllItemsOnSceneSwapDoubleCheck();
    }
    [Server]
    public void ServerBuildTacticianSpellCooldown(int keyPressed){
        string dateTimeWithZone = DateTime.UtcNow.ToString("o");
        //build duration call
        string spell = string.Empty;
        if(keyPressed == 1){
            spell = SpellOne;
        }
        if(keyPressed == 2){
            spell = SpellTwo;
        }
        float duration = GetSpellTactCooldownDuration(spell);
        if(keyPressed == 1){
            if(abilityOneCDRoutine != null){
                StopCoroutine(abilityOneCDRoutine);
            }
            abilityOneCDRoutine = StartCoroutine(SetAbilityCoolDownOne(duration));
        }
        if(keyPressed == 2){
            if(abilityTwoCDRoutine != null){
                StopCoroutine(abilityTwoCDRoutine);
            }
            abilityTwoCDRoutine = StartCoroutine(SetAbilityCoolDownTwo(duration));
        }
        DateTime timeNow = DateTime.Parse(dateTimeWithZone, null, System.Globalization.DateTimeStyles.RoundtripKind);
		DateTime updatedTime = timeNow.AddSeconds(duration); // Add duration in seconds
		string newDateTimeWithZone = updatedTime.ToString("o");
        print($"Adding {newDateTimeWithZone} to our cooldown {spell} at ServerBuildTacticianSpellCooldown process");

		CharacterCooldownListItem coolie = (new CharacterCooldownListItem {
			Value = newDateTimeWithZone,
			//PKey = spell,
            SpellnameFull = spell
		});
        DateTime timeCooldown = DateTime.Parse(coolie.Value, null, System.Globalization.DateTimeStyles.RoundtripKind);

        print("CHECKING TIME: " + timeNow + " FOR OUR COOLDOWN SPELL Tact, WHICH SHOULD NOW BE " + timeCooldown);

		ServerCooldownSaveTactician(coolie);
    }
    (bool, int type) GetSpellTactType(string spell){
        bool reqFriendly = false;
        int spellType = 0;
        if (spell == "Ignite"){
            reqFriendly = false;
            spellType = 1;
        }
        if (spell == "Enthrall"){
            reqFriendly = false;
            spellType = 10;
        }
        if (spell == "Refresh"){
            reqFriendly = true;
            spellType = 3;
        }
        if (spell == "Absorb"){
            reqFriendly = true;
            spellType = 3;
        }
        if (spell == "Morale Boost"){
            reqFriendly = true;
            spellType = 3;
        }
        if (spell == "Lightning Bolt"){
            reqFriendly = false;
            spellType = 1;
        }
        if (spell == "Mend"){
            reqFriendly = true;
            spellType = 2;
        }
        if (spell == "Group Mend"){
            reqFriendly = true;
            spellType = 2;
        }
        if (spell == "Light"){
            reqFriendly = true;
            spellType = 3;
        }
        if (spell == "Stun"){
            reqFriendly = false;
            spellType = 3;
        }
        if (spell == "Antitdote"){
            reqFriendly = true;
            spellType = 2;
        }
        if (spell == "Focus"){
            reqFriendly = true;
            spellType = 3;
        }
        if (spell == "Chain Lightning"){
            reqFriendly = false;
            spellType = 1;
        }
        if (spell == "Harvest Boost"){
            reqFriendly = true;
            spellType = 0;
        }
        if (spell == "Alliance"){
            reqFriendly = false;
            spellType = 0;
        }
        if (spell == "Growth"){
            reqFriendly = true;
            spellType = 0;
        }
        if (spell == "Divine Resurrection"){
            reqFriendly = true;
            spellType = 4;
        }
        
        return (reqFriendly, spellType);
    }
    float GetSpellTactCooldownDuration(string spell){
        float duration = 0f;
        if (spell == "Ignite"){
            duration = 60f;//check for shrine if shrine make it 15
        }
        if (spell == "Enthrall"){
            duration = 60f;//check for shrine if shrine make it 15
        }
        if (spell == "Refresh"){
            duration = 60f;//check for shrine if shrine make it 15
        }
        if (spell == "Absorb"){
            duration = 60f;//check for shrine if shrine make it 15
        }
        if (spell == "Morale Boost"){
            duration = 1800f;
        }
        if (spell == "Lightning Bolt"){
            duration = 1800f;
        }
        if (spell == "Repel"){
            duration = 1800f;
        }
        if (spell == "Mend"){
            duration = 1800f;
        }
        if (spell == "Light"){
            duration = 1800f;
        }
        if (spell == "Stun"){
            duration = 1800f;
        }
        if (spell == "Antitdote"){
            duration = 1800f;
        }
        if (spell == "Fortification"){
            duration = 86400f;
        }
        if (spell == "Focus"){
            duration = 3600f;
        }
        if (spell == "Evacuate"){
            duration = 86400f;
        }
        if (spell == "Chain Lightning"){
            duration = 3600f;
        }
        if (spell == "Far Sight"){
            duration = 1800f;
        }
        if (spell == "Group Mend"){
            duration = 3600f;
        }
        if (spell == "Harvest Boost"){
            duration = 1800f;
        }
        if (spell == "Fertile Soil"){
            duration = 172800f;
        }
        if (spell == "Alliance"){
            duration = 1800f;
        }
        if (spell == "Growth"){
            duration = 86400f;
        }
        if (spell == "Return"){
            duration = 604800f;
        }
        if (spell == "Divine Resurrection"){
            duration = 604800f;
        }
        if (spell == "Rest"){
            duration = 604800f;
        }
        return duration;
    }
    [TargetRpc]
    public void TargetSendFailureMessageToClient(string message)
    {
    	ImproperCheckText.Invoke(message);
        // Implement code to send the message to the client here
        // This could be another RPC call or any other method of informing the client
    }
    public IEnumerator BuffManagerRoutine(){
        while(true){
            CharacterBuffListRemovalPackage packageRemoval = new CharacterBuffListRemovalPackage {
                removingBuffLists = new List<CharacterBuffListRemoval>()
            };
            foreach(var sheet in InformationSheets){
                DateTime currentTime = DateTime.UtcNow;
                List<CharacterBuffListItem> ourSheetRemovalList = new List<CharacterBuffListItem>();
                foreach(var buff in sheet.CharBuffData){
                    if(buff.Key == "Hide"){
                        continue;
                    }
                    if (DateTime.TryParseExact(buff.Time, "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime expirationTime)) {
                        Console.WriteLine($"Parsed Expiration Time (UTC): {expirationTime:O} for Buff: {buff.Key}");
                        if (expirationTime <= currentTime) {
                        Console.WriteLine($"added buff {buff.Key} for removal!");

                            ourSheetRemovalList.Add(buff);
                        }
                    } else {
                        Console.WriteLine($"added buff {buff.Key} for removal!");

                        ourSheetRemovalList.Add(buff);
                    }
                }
                if(ourSheetRemovalList.Count > 0){
                    CharacterBuffListRemoval ourBuffListRemoval = new CharacterBuffListRemoval{
                        Key = sheet.CharacterID,
                        removingBuffs = ourSheetRemovalList
                    };
                    packageRemoval.removingBuffLists.Add(ourBuffListRemoval);
                }
            }
            if(packageRemoval.removingBuffLists.Count > 0){
                CmdRemoveLists(packageRemoval);
            }
            yield return new WaitForSeconds(1f);
        }
    }
    [Command]
    public void CmdRemoveLists(CharacterBuffListRemovalPackage removalPackage){
        for(int i = 0; i < removalPackage.removingBuffLists.Count; i++){
            for(int x = 0; x < removalPackage.removingBuffLists[i].removingBuffs.Count; x++){
                        Console.WriteLine($"added buff {removalPackage.removingBuffLists[i].Key} for removal!");

                ServerRemoveBuff(removalPackage.removingBuffLists[i].Key, removalPackage.removingBuffLists[i].removingBuffs[x]);
            }
        }
    }
    public void SayChat(string speech){
        print($"Got to SayChat");
        CmdSayChat(speech);
    }
    [Command]
    public void CmdSayChat(string speech){
        print($"Got to SayChat");

        ServerSayChat(speech);
    }
    [Server]
    public void ServerSayChat(string speech){
        ClientSayChatLocal(speech);
    }
    [ClientRpc]
    void ClientSayChatLocal(string speech){
        print($"Got to ClientSayChatLocal");
        if(ScenePlayer.localPlayer == this){
            GameObject saySpawn = Instantiate(sayPrefab, transform.position, Quaternion.identity);
            PlayerLocalSpeech saySpeech = saySpawn.GetComponent<PlayerLocalSpeech>();
            saySpeech.PlaySpeech(speech);
        } else {
            // Check if localPlayer's transform exists and the distance is within 5 units.
            if (ScenePlayer.localPlayer != null && ScenePlayer.localPlayer.transform != null)
            {
                float distance = Vector3.Distance(ScenePlayer.localPlayer.transform.position, transform.position);
                if (distance <= 5f)
                {
                    GameObject saySpawn = Instantiate(sayPrefab, transform.position, Quaternion.identity);
                    PlayerLocalSpeech saySpeech = saySpawn.GetComponent<PlayerLocalSpeech>();
                    saySpeech.PlaySpeech(speech);
                }
            }
        }
    }
    [TargetRpc]
    public void TargetShuttingDownCall(string message){
        EndingServerMessage.Invoke(message);
    }
    public void DevShutdown(){
        CmdDevShutdown();
    }
    [Command]
    public void CmdDevShutdown(){
        ServerDevShutdown();
    }
    [Server]
    public void ServerDevShutdown(){
        DevShutingDownServer.Invoke(this.connectionToClient);
    }
    public void GameMasterBuildList(string itemType, string quality){
        CmdGameMasterBuildList(itemType, quality);
    }
    [Command]
    public void CmdGameMasterBuildList(string itemType, string quality){
        ServerGameMasterBuildList(itemType, quality);
    }
    [Server]
    public void ServerGameMasterBuildList(string itemType, string quality){
        GameMasterCreateList.Invoke(this.connectionToClient, itemType, quality);
    }
    public void GameMasterTeleportOVM(string nodename, string playerName){
        CmdGameMasterTeleportOVM(nodename, playerName);
    }
    [Command]
    public void CmdGameMasterTeleportOVM(string nodename, string playerName){
        ServerGameMasterTeleportOVM(nodename, playerName);
    }
    [Server]
    public void ServerGameMasterTeleportOVM(string nodename, string playerName){
        GameMasterTeleport.Invoke(this.connectionToClient, nodename, playerName);
    }
    public void ServerGameMasterheal(){
        CmdServerGameMasterheal();
    }
    [Command]
    public void CmdServerGameMasterheal(){
        ServerServerGameMasterheal();
    }
    [Server]
    public void ServerServerGameMasterheal(){
        GameMasterHeal.Invoke(this.connectionToClient);
    }
    public void BreakingPointCast(){
        CmdBreakingPointCast();
    }
    [Command]
    public void CmdBreakingPointCast(){
        ServerBreakingPointCast();
    }
    [Server]
    public void ServerBreakingPointCast(){
        GameMasterBreakingPointReleased.Invoke(this.connectionToClient);
    }
    public void GameMasterCreateGuild(string guildNameRequest){
        print($"{guildNameRequest} was our GameMasterCreateGuild request name");
        CmdGameMasterCreateGuild(guildNameRequest);
    }
    [Command]
    public void CmdGameMasterCreateGuild(string guildNameRequest){
        ServerGameMasterCreateGuild(guildNameRequest);
    }
    [Server]
    public void ServerGameMasterCreateGuild(string guildNameRequest){
        CreateGuild.Invoke(this.connectionToClient, guildNameRequest);
    }

     public void GameMasterBuildItem(string itemName, string quality, int quant){
        CmdGameMasterBuildItem(itemName, quality, quant);
    }
    [Command]
    public void CmdGameMasterBuildItem(string itemName, string quality, int quant){
        ServerGameMasterBuildItem(itemName, quality, quant);
    }
    [Server]
    public void ServerGameMasterBuildItem(string itemName, string quality, int quant){
        GameMasterCreateItem.Invoke(this.connectionToClient, itemName, quality, quant);
    }
    public void GameMasterBuildItemForPlayer(string _playerName, string itemName, string quality, int quant){
        CmdGameMasterBuildItemForPlayer(_playerName, itemName, quality, quant);
    }
    [Command]
    public void CmdGameMasterBuildItemForPlayer(string _playerName, string itemName, string quality, int quant){
        ServerGameMasterBuildItemForPlayer(_playerName, itemName, quality, quant);
    }
    [Server]
    public void ServerGameMasterBuildItemForPlayer(string _playerName, string itemName, string quality, int quant){
        GameMasterCreateItemForPlayer.Invoke(_playerName, itemName, quality, quant);
    }
    public void CancelBuildingCraftedItem(CraftingListItem craftingItem){
        CmdAskServerToCancelItem(craftingItem);
    }
    [Command]
    void CmdAskServerToCancelItem(CraftingListItem craftingItem){
        ServerToCancelItem(craftingItem);
    }
    [Server]
    void ServerToCancelItem(CraftingListItem craftingItem){
        CraftedItemCancel.Invoke(this.connectionToClient, craftingItem);
    }
    [Server]
    public void ServerDeleteCraftItem(CraftingListItem craftedItem){
        if(TacticianInformationSheet.CraftingItems.Contains(craftedItem)){
            TacticianInformationSheet.CraftingItems.Remove(craftedItem);
        }
        TargetDeleteCraftItem(craftedItem);
    }
    [TargetRpc]
    public void TargetLootedItem(){
        Speaker.Invoke(350);
    }
    [TargetRpc]
    public void TargetLootedQuestItem(){
        Speaker.Invoke(350);
        PingUpdate.Invoke("Tactician");
    }
    [TargetRpc]
    public void SalvageItemsLooted(){
        Speaker.Invoke(350);
        CraftReturn.Invoke();
    }
    [TargetRpc]
    public void TargetDeleteCraftItem(CraftingListItem craftedItem){
        if(TacticianInformationSheet.CraftingItems.Contains(craftedItem)){
            TacticianInformationSheet.CraftingItems.Remove(craftedItem);
        }
        CraftReturn.Invoke();
    }
    public void FinishBuildingCraftedItem(CraftingListItem craftingItem){
        CmdAskServerToFinishItem(craftingItem);
    }
    [Command]
    void CmdAskServerToFinishItem(CraftingListItem craftingItem){
        CraftedItemPermissionToBuild.Invoke(this.connectionToClient, craftingItem);
    }
    [Server]
    public void ServerUpdateWallet(string walletAddress, string dkpBalance, string xrpBalance){
        TacticianInformationSheet.Address = walletAddress;
        TacticianInformationSheet.DKPBalance = dkpBalance;
        TacticianInformationSheet.XRPBalance = xrpBalance;
        TargetUpdateWallet(walletAddress, dkpBalance, xrpBalance);
    }
    [TargetRpc]
    void TargetUpdateWallet(string walletAddress, string dkpBalance, string xrpBalance){
        TacticianInformationSheet.Address = walletAddress;
        TacticianInformationSheet.DKPBalance = dkpBalance;
        TacticianInformationSheet.XRPBalance = xrpBalance;
        WalletAwake.Invoke();
        RegistrationFinished.Invoke();
    }
    [Server]
    public void ServerFinishedCraftItem(CraftingListItem craftedItem, int newSkillLevel, float newSkillExp, string skillName){
        print($"Server finished craft item {craftedItem.ItemName} is being removed from our crafting list");
        if(TacticianInformationSheet.CraftingItems.Contains(craftedItem)){
            print($"Server finished craft item {craftedItem.ItemName} HAS been removed from our crafting list");
            TacticianInformationSheet.CraftingItems.Remove(craftedItem);
        } else {
            print($"Server finished craft item {craftedItem.ItemName} HAS NOT been removed from our crafting list");
            
        }
        //craftedItem.Finished = true;
        if(skillName == "weaponCraftingSkill"){
            TacticianInformationSheet.weaponCraftingSkill = newSkillLevel;
            TacticianInformationSheet.weaponCraftingExp = newSkillExp;
        }
        if(skillName == "armorCraftingSkill"){
            TacticianInformationSheet.armorCraftingSkill = newSkillLevel;
            TacticianInformationSheet.armorCraftingExp = newSkillExp;
        }
        if(skillName == "jewelCraftingSkill"){
            TacticianInformationSheet.jewelCraftingSkill = newSkillLevel;
            TacticianInformationSheet.jewelCraftingExp = newSkillExp;
        }
        if(skillName == "cookingSkill"){
            TacticianInformationSheet.cookingSkill = newSkillLevel;
            TacticianInformationSheet.cookingExp = newSkillExp;
        }
        if(skillName == "alchemySkill"){
            TacticianInformationSheet.alchemySkill = newSkillLevel;
            TacticianInformationSheet.alchemyExp = newSkillExp;
        }
        if(skillName == "refiningSkill"){
            TacticianInformationSheet.refiningSkill = newSkillLevel;
            TacticianInformationSheet.refiningExp = newSkillExp;
        }
        TargetFinishedCraftItem(craftedItem, newSkillLevel, newSkillExp, skillName);
        
    }
    [TargetRpc]
    public void TargetFinishedCraftItem(CraftingListItem craftedItem, int newSkillLevel, float newSkillExp, string skillName){
        print($"Target finished craft item {craftedItem.ItemName} is being removed from our crafting list");

        if(TacticianInformationSheet.CraftingItems.Contains(craftedItem)){
            print($"Target finished craft item {craftedItem.ItemName} HAS been removed from our crafting list");
            TacticianInformationSheet.CraftingItems.Remove(craftedItem);
        } else {
            print($"Target finished craft item {craftedItem.ItemName} HAS NOT been removed from our crafting list");
            
        }
        //craftedItem.Finished = true;
        if(skillName == "weaponCraftingSkill"){
            TacticianInformationSheet.weaponCraftingSkill = newSkillLevel;
            TacticianInformationSheet.weaponCraftingExp = newSkillExp;
        }
        if(skillName == "armorCraftingSkill"){
            TacticianInformationSheet.armorCraftingSkill = newSkillLevel;
            TacticianInformationSheet.armorCraftingExp = newSkillExp;
        }
        if(skillName == "jewelCraftingSkill"){
            TacticianInformationSheet.jewelCraftingSkill = newSkillLevel;
            TacticianInformationSheet.jewelCraftingExp = newSkillExp;
        }
        if(skillName == "cookingSkill"){
            TacticianInformationSheet.cookingSkill = newSkillLevel;
            TacticianInformationSheet.cookingExp = newSkillExp;
        }
        if(skillName == "alchemySkill"){
            TacticianInformationSheet.alchemySkill = newSkillLevel;
            TacticianInformationSheet.alchemyExp = newSkillExp;
        }
        if(skillName == "refiningSkill"){
            TacticianInformationSheet.refiningSkill = newSkillLevel;
            TacticianInformationSheet.refiningExp = newSkillExp;
        }
        CraftRemovePrefab.Invoke(craftedItem);
        
        RefreshSheets.Invoke( "Tactician");
        ContentSizer.Invoke();
        CraftReturn.Invoke();

    }
    [Server]
    public void ServerStartCraftItem(CraftingListItem craftedItem, List<CharacterInventoryListItem> changedItems)
    {
        print(craftedItem.Time + " is our time of start for ServerStartCraftItem");
        if(changedItems.Count > 0){
            for (int x = 0; x < changedItems.Count; x++){
                int itemIndex = -1;
                for (int j = 0; j < TacticianInformationSheet.TacticianInventoryData.Count; j++){
                    if (TacticianInformationSheet.TacticianInventoryData[j].Value.customID == changedItems[x].Value.customID){
                        itemIndex = j;
                        break;
                    }
                }
                // If the item is found, remove it
                if (itemIndex != -1){
                    TacticianInformationSheet.TacticianInventoryData.RemoveAt(itemIndex);
                }
                TacticianInformationSheet.TacticianInventoryData.Add(changedItems[x]);
                print("Added a changed item and its amount is " + changedItems[x].Value.amount + " and name is " + changedItems[x].Value.Item_Name);
            }
        }
        if(craftedItem.Mode != "transmuting"){
            if(!TacticianInformationSheet.CraftingItems.Contains(craftedItem)){
                TacticianInformationSheet.CraftingItems.Add(craftedItem);
            }
        }
        
        TargetStartCraftItem(craftedItem, changedItems);
    }
    [TargetRpc]
    public void TargetStartCraftItem(CraftingListItem craftedItem, List<CharacterInventoryListItem> changedItems)
    {
        print(craftedItem.Time + " is our time of start for TargetStartCraftItem");
        if(changedItems.Count > 0){
            for (int x = 0; x < changedItems.Count; x++){
                int itemIndex = -1;
                for (int j = 0; j < TacticianInformationSheet.TacticianInventoryData.Count; j++){
                    if (TacticianInformationSheet.TacticianInventoryData[j].Value.customID == changedItems[x].Value.customID){
                        itemIndex = j;
                        break;
                    }
                }
                // If the item is found, remove it
                if (itemIndex != -1){
                    TacticianInformationSheet.TacticianInventoryData.RemoveAt(itemIndex);
                }
                print("Added a changed item and its amount is " + changedItems[x].Value.amount + " and name is " + changedItems[x].Value.Item_Name);
                TacticianInformationSheet.TacticianInventoryData.Add(changedItems[x]);
                Refreshitem.Invoke(this, changedItems[x].Value);
                ResetItemSelectable.Invoke(changedItems[x].Value);
            }
        }
        //if(craftedItem.Mode != "transmuting"){
        if(!TacticianInformationSheet.CraftingItems.Contains(craftedItem)){
            TacticianInformationSheet.CraftingItems.Add(craftedItem);
        }
        CraftStartedBuildPrefab.Invoke(craftedItem);
        //}
        print($"Starting up the craft build for {craftedItem.ItemName}");
        RefreshSheets.Invoke( "Tactician");
        ContentSizer.Invoke();
        CraftReturn.Invoke();
        //ResetSpring.Invoke();
           
    }
    void StartTransmuteItem(CraftingListItem craftedItem){
        CmdTransmuteItem(craftedItem);
    }
    [Command]
    void CmdTransmuteItem(CraftingListItem craftedItem){
        CraftedItemPermissionToBuild.Invoke(this.connectionToClient, craftedItem);
    }
    public void AskToBuildItem(string itemName, string mode, int quant){
        CmdCraftItemRequest(itemName, mode, quant);
    }
    [Command]
    void CmdCraftItemRequest(string itemName, string mode, int quant){
        ServerCraftItemRequest(itemName, mode, quant);
    }
    [Server]
    void ServerCraftItemRequest(string itemName, string mode, int quant){
        ServerCraftRequest.Invoke(this.connectionToClient, itemName, mode, quant);
    }
    [TargetRpc]
    public void TargetCraftReturn(){
        CraftReturn.Invoke();
    }
    public string ReturnCraftBuildableValue(string item)
{
    int buildCount = 0; // This will count how many items can be built
    List<string> requiredIngredients = CraftingRef.Instance.FindIngredientList(item);
    
    // A dictionary to keep track of how many of each item is available in the inventory.
    Dictionary<string, int> inventory = new Dictionary<string, int>();

    // Populate the inventory dictionary with items from TacticianInventoryData,
    // Summing the total quantities for each item.
    foreach (var tactItem in TacticianInformationSheet.TacticianInventoryData)
    {
        if(tactItem.Value.Deleted || tactItem.Value.amount == 0){
            continue;
        }
        if (inventory.ContainsKey(tactItem.Value.Item_Name))
        {
            inventory[tactItem.Value.Item_Name] += tactItem.Value.amount; // Add to existing count for this item
        }
        else
        {
            inventory[tactItem.Value.Item_Name] = tactItem.Value.amount; // Add new item to inventory
        }
    }

    bool canBuild = true; // Flag to check if we can still build more items

    while(canBuild)
    {
        // Create a working copy of required ingredients for this iteration.
        List<string> required = new List<string>(requiredIngredients);

        foreach (string ingredient in requiredIngredients)
        {
            // Check if the ingredient is in the inventory and has enough quantity.
            if (inventory.ContainsKey(ingredient) && inventory[ingredient] > 0)
            {
                // Use one of the ingredient.
                inventory[ingredient]--;
                
                // Remove one instance of the ingredient from the working copy of required.
                required.Remove(ingredient);
            }
            else
            {
                // Can't build anymore as one of the required ingredients is not available or not enough.
                canBuild = false;
                break;
            }
        }

        // If all required items were used (meaning the required list is empty), increment build count.
        if (required.Count == 0)
        {
            buildCount++;
        }
        else
        {
            // We break out of the loop if we can't build anymore.
            break;
        }
    }

    // Return the count of buildable items as a string.
    return buildCount.ToString();
}
   void AddAllSceneNodesToDictionary()
    {
        SceneNode[] allNodes = FindObjectsOfType<SceneNode>();

        // clear dictionary before adding new elements
        sceneNodesDictionary.Clear();

        foreach (SceneNode node in allNodes)
        {
            // use the node's name as the key
            sceneNodesDictionary[node.nodeName] = node;
        }
    }
    public void LogoutClient(ScenePlayer sPlayer){
        if(sPlayer == ScenePlayer.localPlayer){
            print("Logging Cleint out now");
            CmdLogoutClient();
        }
    }
    [Command]
    void CmdLogoutClient(){
        LogoutPlayer.Invoke(this.connectionToClient);
    }
    [TargetRpc]
    public void TargetLogoutClient(){
        ServerLogoutPlayer.Invoke();
    }
    public List<CharacterFullDataMessage> GetInformationSheets(){
        return InformationSheets;
    }
    public List<string> GetParty(){
        return ActivePartyList;
    }
    public List<string> GetMatchParty(){
        return MatchPartyList;
    }
    public TacticianFullDataMessage GetTacticianSheet(){
        return TacticianInformationSheet;
    }
    [Server]
    public void GetTacticianLVLEXP(CharacterStatListItem LVL, CharacterStatListItem EXP){
        int sheetIndex = -1;
        int LVLIndex = -1;
        int EXPIndex = -1;
        int startLevel = 1;
        
        int endLevel = int.Parse(LVL.Value);
        for (int s = 0; s < TacticianInformationSheet.TacticianStatData.Count; s++)
        {
            if (TacticianInformationSheet.TacticianStatData[s].Key == LVL.Key)
            {
                startLevel = int.Parse(TacticianInformationSheet.TacticianStatData[s].Value);
                LVLIndex = s;
                break;
            }
        }
        if (LVLIndex != -1)
        {
            TacticianInformationSheet.TacticianStatData.RemoveAt(LVLIndex);
        }
        // Add the new CharacterInventoryListItem to the CharInventoryData list
        TacticianInformationSheet.TacticianStatData.Add(LVL);
        for (int X = 0; X < TacticianInformationSheet.TacticianStatData.Count; X++)
        {
            if (TacticianInformationSheet.TacticianStatData[X].Key == EXP.Key)
            {
                EXPIndex = X;
                break;
            }
        }
        if (EXPIndex != -1)
        {
            TacticianInformationSheet.TacticianStatData.RemoveAt(EXPIndex);
        }
        // Add the new CharacterInventoryListItem to the CharInventoryData list
        TacticianInformationSheet.TacticianStatData.Add(EXP);
        TargetGetTacticianLVLEXP(LVL, EXP);
        if(endLevel > startLevel){
            //learn new spell!
            CheckForNextTactSpellLevel(endLevel);
            ClientPlayLevelUpAnimation();
            //play the animation
        }
            
    }

    [Server]
void CheckForNextTactSpellLevel(int level)
{
    string spellToLearn = string.Empty;
    switch (level)
    {
        case 1:
            spellToLearn = "Morale Boost";
            break;
        case 2:
            spellToLearn = "Lightning Bolt";
            break;
        case 3:
            spellToLearn = "Repel";
            break;
        case 4:
            spellToLearn = "Mend";
            break;
        case 5:
            spellToLearn = "Light";
            break;
        case 6:
            spellToLearn = "Stun";
            break;
        case 7:
            spellToLearn = "Antidote";
            break;
        case 8:
            spellToLearn = "Fortification";
            break;
        case 9:
            spellToLearn = "Focus";
            break;
        case 10:
            spellToLearn = "Evacuate";
            break;
        case 11:
            spellToLearn = "Chain Lightning";
            break;
        case 12:
            spellToLearn = "Far Sight";
            break;
        case 13:
            spellToLearn = "Group Mend";
            break;
        case 14:
            spellToLearn = "Harvest Boost";
            break;
        case 15:
            spellToLearn = "Fertile Soil";
            break;
        case 16:
            spellToLearn = "Alliance";
            break;
        case 17:
            spellToLearn = "Growth";
            break;
        case 18:
            spellToLearn = "Return";
            break;
        case 19:
            spellToLearn = "Divine Resurrection";
            break;
        case 20:
            spellToLearn = "Rest";
            break;
        default:
            Debug.Log("Invalid level or spell not available for the given level.");
            break;
    }

    if (!string.IsNullOrEmpty(spellToLearn))
    {
        Debug.Log($"Spell to learn at level {level}: {spellToLearn}");
        TacticianSpellListItem LearningSpell = new TacticianSpellListItem {
            Key = spellToLearn
        };
        int SpellIndex = -1;
        for (int s = 0; s < TacticianInformationSheet.TacticianSpellData.Count; s++)
        {
            if (TacticianInformationSheet.TacticianSpellData[s].Key == LearningSpell.Key)
            {
                SpellIndex = s;
                break;
            }
        }
        if (SpellIndex != -1)
        {
            TacticianInformationSheet.TacticianSpellData.RemoveAt(SpellIndex);
        }
        TacticianInformationSheet.TacticianSpellData.Add(LearningSpell);
        TargetLearnSpell(LearningSpell);
    }
}
[TargetRpc]
void TargetLearnSpell(TacticianSpellListItem LearningSpell){
    int SpellIndex = -1;
    for (int s = 0; s < TacticianInformationSheet.TacticianSpellData.Count; s++)
    {
        if (TacticianInformationSheet.TacticianSpellData[s].Key == LearningSpell.Key)
        {
            SpellIndex = s;
            break;
        }
    }
    if (SpellIndex != -1)
    {
        TacticianInformationSheet.TacticianSpellData.RemoveAt(SpellIndex);
    }
    TacticianInformationSheet.TacticianSpellData.Add(LearningSpell);
    RefreshSheets.Invoke( "Tactician");
}
    [ClientRpc]
    public void ClientPlayLevelUpAnimation(){
        if(!StatAsset.Instance.CheckForCombatZone(currentScene)){
            GameObject leveledupPrefab = Instantiate(levelUpPrefab, this.transform.position, Quaternion.identity);
        }
        PingUpdate.Invoke("Tactician");
        Speaker.Invoke(390);
    }
     [TargetRpc]
    public void TargetGetTacticianLVLEXP(CharacterStatListItem LVL, CharacterStatListItem EXP){
        int sheetIndex = -1;
        int LVLIndex = -1;
        int EXPIndex = -1;
       
        for (int s = 0; s < TacticianInformationSheet.TacticianStatData.Count; s++)
        {
            if (TacticianInformationSheet.TacticianStatData[s].Key == LVL.Key)
            {
                LVLIndex = s;
                break;
            }
        }
        if (LVLIndex != -1)
        {
            TacticianInformationSheet.TacticianStatData.RemoveAt(LVLIndex);
        }
        // Add the new CharacterInventoryListItem to the CharInventoryData list
        TacticianInformationSheet.TacticianStatData.Add(LVL);
        for (int X = 0; X < TacticianInformationSheet.TacticianStatData.Count; X++)
        {
            if (TacticianInformationSheet.TacticianStatData[X].Key == EXP.Key)
            {
                EXPIndex = X;
                break;
            }
        }
        if (EXPIndex != -1)
        {
            TacticianInformationSheet.TacticianStatData.RemoveAt(EXPIndex);
        }
        // Add the new CharacterInventoryListItem to the CharInventoryData list
        TacticianInformationSheet.TacticianStatData.Add(EXP);
        RefreshSheets.Invoke( "Tactician");
        RefreshChain.Invoke();
        
    }
    [ClientRpc]
    public void RpcBuildPartyInspector(ClientPartyInformation savedList, Match match)
    {
        if(currentMatch == null){
            return;
        }
        if(currentMatch == match || ScenePlayer.localPlayer == this){
            foreach(var key in savedList.Party){
                // Only add if the key does not exist in InspectParty
                if (!InspectParty.ContainsKey(key.Key))//make the class to look at these in inspector
                {
                    InspectParty.Add(key.Key, key.Value);
                    //print($"added {key.Key} to inspectorlist and they have sprite {key.Value}");
                }
            }
        }

    }
    [Server]
    public void AddPartyServer(string ID){
         if(!ActivePartyList.Contains(ID)){
            ActivePartyList.Add(ID);
            TargetAddParty(ID);
        }
    }
    [TargetRpc]
    void TargetAddParty(string ID){
        ActivePartyList.Add(ID);
    }
    [Server]
    public void AddMatchPartyListServer(string ID){
        MatchPartyList.Add(ID);
        TargetAddMatchPartyList(ID);
    }
    [TargetRpc]
    void TargetAddMatchPartyList(string ID){
        MatchPartyList.Add(ID);
    }
    [Server]
    public void ServerRemovingPartymember(string ID){
        if(ActivePartyList.Contains(ID)){
            ActivePartyList.Remove(ID);
            TargetRemoveParty(ID);
        }
    }
    [TargetRpc]
    void TargetRemoveParty(string ID){
        if(ActivePartyList.Contains(ID)){
            ActivePartyList.Remove(ID);
        }
    }
    [TargetRpc]
    public void TargetSendMobList(List<MovingObject> mobs){
        SendFogMobs.Invoke(mobs);
    }
    //Inventory/Stat/Spell manipulation
    [Server]
    public void ServerPurchasedItemResult(CharacterInventoryListItem DATA){
        TacticianInformationSheet.StashInventoryData.Add(DATA);
        TargetPurchasedItemResult(DATA);
    }
    [TargetRpc]
    public void TargetPurchasedItemResult(CharacterInventoryListItem DATA){
        // Add the new CharacterInventoryListItem to the CharInventoryData list
        PingUpdate.Invoke("Stash");

        TacticianInformationSheet.StashInventoryData.Add(DATA);
        PurchasedItem.Invoke(DATA);
        ContentSizer.Invoke();
        CraftReturn.Invoke();

    }
     [Server]
    public void ServerTacticianItemResult(CharacterInventoryListItem DATA){
        TacticianInformationSheet.TacticianInventoryData.Add(DATA);
        TargetTacticianItemResult(DATA);
    }
    [TargetRpc]
    public void TargetTacticianItemResult(CharacterInventoryListItem DATA){
        // Add the new CharacterInventoryListItem to the CharInventoryData list
        TacticianInformationSheet.TacticianInventoryData.Add(DATA);
        PickedUpItemTactician.Invoke(DATA);
        CraftReturn.Invoke();

    }
    [Server]
    public void GetFullTacticianData(TacticianFullDataMessage DATA){
        TacticianInformationSheet = DATA;
        TargetTactInfoSheetAdd(DATA);
    }
    [TargetRpc]
    void TargetTactInfoSheetAdd(TacticianFullDataMessage DATA){
        SetTactSheetClient(DATA);
    }
    
    void SetTactSheetClient(TacticianFullDataMessage DATA){
        TacticianInformationSheet = DATA;
        
        
        //RefreshTactician.Invoke();
        //RebuildItems.Invoke("Tactician", null);
    }
    [Server]
    public void ServerSendDKPCD(string cd, string XRP, string DKP){
        TacticianInformationSheet.DKPCooldown = cd;
        TacticianInformationSheet.DKPBalance = DKP;
        TacticianInformationSheet.XRPBalance = XRP;
        TargetSendDKPCD(cd, XRP, DKP);
    }
    [TargetRpc]
    void TargetSendDKPCD(string cd, string XRP, string DKP){
        TacticianInformationSheet.DKPBalance = DKP;
        TacticianInformationSheet.XRPBalance = XRP;
        TacticianInformationSheet.DKPCooldown = cd;
        StartCoroutine(WalletWakeup());
        PingUpdate.Invoke("Wallet");

    }
    IEnumerator WalletWakeup(){
        yield return new WaitForSeconds(2f);
        WalletAwake.Invoke();
        walletTransmute.Invoke();
    }
    [Server]
    public void ServerSpawnItems(){
        TargetSpawnItems();
    }
    [TargetRpc]
    void TargetSpawnItems(){
        BuildInventories.Invoke();
    }
    [Server]
    public void ServerSpawnCraftBuilds(){
        TargetSpawnCraftBuilds();
    }
    [TargetRpc]
    void TargetSpawnCraftBuilds(){
        StartCoroutine(BuildCraftSpawnsOnStartUp());
    }
    IEnumerator BuildCraftSpawnsOnStartUp(){
        yield return new WaitForSeconds(5f);
        foreach(var item in TacticianInformationSheet.CraftingItems){
            print("Building item " + item.ItemName); 
            CraftStartedBuildPrefab.Invoke(item);
        }
    }
    [Server]
    public void GetTacticianDeletingItem(CharacterInventoryListItem DATA){
        int itemIndex = -1;
            print($"Starting Tact delete call on scene player");
        for (int j = 0; j < TacticianInformationSheet.TacticianInventoryData.Count; j++){
            if (TacticianInformationSheet.TacticianInventoryData[j].Value.customID == DATA.Value.customID){
                itemIndex = j;
                break;
            }
        }
        // If the item is found, remove it
        if (itemIndex != -1){
            TacticianInformationSheet.TacticianInventoryData.RemoveAt(itemIndex);
        }
            print($"Starting Tact delete call on scene player for target");

        TargetGetTacticianDeletingItem(DATA);
        DATA.Value.amount = 0;
        DATA.Value.Deleted = true;
        DATA.Value.Changed = true;
        // Add the new CharacterInventoryListItem to the CharInventoryData list
        TacticianInformationSheet.TacticianInventoryData.Add(DATA);
        print($"{DATA.Value.amount} is our amount for the item and changed is {DATA.Value.Changed} and Deleted is {DATA.Value.Deleted} that we added in delete call");

    }
    [TargetRpc]
    public void TargetGetTacticianDeletingItem(CharacterInventoryListItem DATA)
    {
            print($"Starting Tact delete call on scene player for target");
        print($"{DATA.Value.amount} is our amount for the item and changed is {DATA.Value.Changed} and Deleted is {DATA.Value.Deleted} on our tactician delete target before change");

        int itemIndex = -1;
        for (int j = 0; j < TacticianInformationSheet.TacticianInventoryData.Count; j++){
            if (TacticianInformationSheet.TacticianInventoryData[j].Value.customID == DATA.Value.customID){
                itemIndex = j;
                break;
            }
        }
        // If the item is found, remove it
        if (itemIndex != -1){
            TacticianInformationSheet.TacticianInventoryData.RemoveAt(itemIndex);
        }
        DATA.Value.amount = 0;
        DATA.Value.Deleted = true;
        DATA.Value.Changed = true;
        print($"{DATA.Value.amount} is our amount for the item and changed is {DATA.Value.Changed} and Deleted is {DATA.Value.Deleted} on our tactician delete target after change");
        // Add the new CharacterInventoryListItem to the CharInventoryData list
        TacticianInformationSheet.TacticianInventoryData.Add(DATA);
        Refreshitem.Invoke(this, DATA.Value);
        foreach(var sheet in InformationSheets){
            RefreshSheets.Invoke( sheet.CharacterID);
        }
        ContentSizer.Invoke();
        ItemMoved.Invoke();
        CraftReturn.Invoke();

        //RebuildItems.Invoke("Tactician", null);
        //refresh items just sent back
    }
    [Server]
    public void GetTacticianNewItem(CharacterInventoryListItem DATA){
        int itemIndex = -1;
        for (int j = 0; j < TacticianInformationSheet.TacticianInventoryData.Count; j++){
            if (TacticianInformationSheet.TacticianInventoryData[j].Value.customID == DATA.Value.customID){
                itemIndex = j;
                break;
            }
        }
        // If the item is found, remove it
        if (itemIndex != -1){
            TacticianInformationSheet.TacticianInventoryData.RemoveAt(itemIndex);
        }
        // Add the new CharacterInventoryListItem to the CharInventoryData list
        TacticianInformationSheet.TacticianInventoryData.Add(DATA);
        TargetGetTacticianNewItem(DATA);
    }
    [TargetRpc]
    public void TargetGetTacticianNewItem(CharacterInventoryListItem DATA)
    {
        int itemIndex = -1;
        for (int j = 0; j < TacticianInformationSheet.TacticianInventoryData.Count; j++){
            if (TacticianInformationSheet.TacticianInventoryData[j].Value.customID == DATA.Value.customID){
                itemIndex = j;
                break;
            }
        }
        // If the item is found, remove it
        if (itemIndex != -1){
            TacticianInformationSheet.TacticianInventoryData.RemoveAt(itemIndex);
        }
        // Add the new CharacterInventoryListItem to the CharInventoryData list
        TacticianInformationSheet.TacticianInventoryData.Add(DATA);
        Refreshitem.Invoke(this, DATA.Value);
        foreach(var sheet in InformationSheets){
            RefreshSheets.Invoke( sheet.CharacterID);
        }
        ContentSizer.Invoke();
                ItemMoved.Invoke();
        CraftReturn.Invoke();


        //RebuildItems.Invoke("Tactician", null);
        //refresh items just sent back
    }
    [Server]
    public void GetTacticianRemoveItem(CharacterInventoryListItem itemKey){
        int itemIndex = -1;
        for (int j = 0; j < TacticianInformationSheet.TacticianInventoryData.Count; j++){
            if (TacticianInformationSheet.TacticianInventoryData[j].Value.customID == itemKey.Value.customID){
                itemIndex = j;
                break;
            }
        }
        // If the item is found, remove it
        if (itemIndex != -1){
            TacticianInformationSheet.TacticianInventoryData.RemoveAt(itemIndex);
        }
        TargetGetTacticianRemoveItem(itemKey);
    }
    [TargetRpc]
    public void TargetGetTacticianRemoveItem(CharacterInventoryListItem itemKey){
        int itemIndex = -1;
        for (int j = 0; j < TacticianInformationSheet.TacticianInventoryData.Count; j++){
            if (TacticianInformationSheet.TacticianInventoryData[j].Value.customID == itemKey.Value.customID){
                itemIndex = j;
                break;
            }
        }
        // If the item is found, remove it
        if (itemIndex != -1){
            TacticianInformationSheet.TacticianInventoryData.RemoveAt(itemIndex);
        }
        ContentSizer.Invoke();
                ItemMoved.Invoke();
        CraftReturn.Invoke();

        //RebuildItems.Invoke("Tactician", null);

    }
    [Server]
    public void GetStashNewItems(List<CharacterInventoryListItem> DATAList){
        foreach(var DATA in DATAList){
            TacticianInformationSheet.StashInventoryData.Add(DATA);
        }
        // Add the new CharacterInventoryListItem to the CharInventoryData list
        TargetGetStashNewItems(DATAList);
    }
    [TargetRpc]
    public void TargetGetStashNewItems(List<CharacterInventoryListItem> DATAList)
    {
        foreach(var DATA in DATAList){
            TacticianInformationSheet.StashInventoryData.Add(DATA);
        }
        ContentSizer.Invoke();
        CraftReturn.Invoke();
        

    }
    void SharedPingMessage(){
        #if UNITY_SERVER
        //print("Server message from scene player called building tactician");
        #endif

        #if !UNITY_SERVER
        //print("Client message from scene player called building tactician");
        #endif
    }
    [TargetRpc]
    public void TargetSendInventoryItemSelectable(CharacterInventoryListItem DATA){
        ResetItemSelectable.Invoke(DATA.Value);
    }
    [Server]
    public void GetStashDeletingItem(CharacterInventoryListItem DATA){
        int itemIndex = -1;
        for (int j = 0; j < TacticianInformationSheet.StashInventoryData.Count; j++){
            if (TacticianInformationSheet.StashInventoryData[j].Value.customID == DATA.Value.customID){
                itemIndex = j;
                break;
            }
        }
        // If the item is found, remove it
        if (itemIndex != -1){
            TacticianInformationSheet.StashInventoryData.RemoveAt(itemIndex);
        }
        TargetGetStashDeletingItem(DATA);
        DATA.Value.amount = 0;
        DATA.Value.Deleted = true;
        DATA.Value.Changed = true;
        // Add the new CharacterInventoryListItem to the CharInventoryData list
        TacticianInformationSheet.StashInventoryData.Add(DATA);
        
    }
    [TargetRpc]
    public void TargetGetStashDeletingItem(CharacterInventoryListItem DATA)
    {
        int itemIndex = -1;
        for (int j = 0; j < TacticianInformationSheet.StashInventoryData.Count; j++){
            if (TacticianInformationSheet.StashInventoryData[j].Value.customID == DATA.Value.customID){
                itemIndex = j;
                break;
            }
        }
        // If the item is found, remove it
        if (itemIndex != -1){
            TacticianInformationSheet.StashInventoryData.RemoveAt(itemIndex);
        }
        DATA.Value.amount = 0;
        DATA.Value.Deleted = true;
        DATA.Value.Changed = true;
        // Add the new CharacterInventoryListItem to the CharInventoryData list
        TacticianInformationSheet.StashInventoryData.Add(DATA);
        Refreshitem.Invoke(this, DATA.Value);
        ContentSizer.Invoke();
        CraftReturn.Invoke();

        //WRONG
    }
    [Server]
    public void GetStashNewItem(CharacterInventoryListItem DATA){
        int itemIndex = -1;
        for (int j = 0; j < TacticianInformationSheet.StashInventoryData.Count; j++){
            if (TacticianInformationSheet.StashInventoryData[j].Value.customID == DATA.Value.customID){
                itemIndex = j;
                break;
            }
        }
        // If the item is found, remove it
        if (itemIndex != -1){
            TacticianInformationSheet.StashInventoryData.RemoveAt(itemIndex);
        }
        // Add the new CharacterInventoryListItem to the CharInventoryData list
        TacticianInformationSheet.StashInventoryData.Add(DATA);
        TargetGetStashNewItem(DATA);
    }
    [TargetRpc]
    public void TargetGetStashNewItem(CharacterInventoryListItem DATA)
    {
        int itemIndex = -1;
        for (int j = 0; j < TacticianInformationSheet.StashInventoryData.Count; j++){
            if (TacticianInformationSheet.StashInventoryData[j].Value.customID == DATA.Value.customID){
                itemIndex = j;
                break;
            }
        }
        // If the item is found, remove it
        if (itemIndex != -1){
            TacticianInformationSheet.StashInventoryData.RemoveAt(itemIndex);
        }
        // Add the new CharacterInventoryListItem to the CharInventoryData list
        TacticianInformationSheet.StashInventoryData.Add(DATA);
        Refreshitem.Invoke(this, DATA.Value);
        ContentSizer.Invoke();
        CraftReturn.Invoke();

        //WRONG
    }
    [Server]
    public void GetStashRemoveItem(CharacterInventoryListItem itemKey){
        int itemIndex = -1;
        for (int j = 0; j < TacticianInformationSheet.StashInventoryData.Count; j++){
            if (TacticianInformationSheet.StashInventoryData[j].Value.customID == itemKey.Value.customID){
                itemIndex = j;
                break;
            }
        }
        // If the item is found, remove it
        if (itemIndex != -1){
            TacticianInformationSheet.StashInventoryData.RemoveAt(itemIndex);
        }
        TargetGetStashRemoveItem(itemKey);
    }
    [TargetRpc]
    public void TargetGetStashRemoveItem(CharacterInventoryListItem itemKey){
        int itemIndex = -1;
        for (int j = 0; j < TacticianInformationSheet.StashInventoryData.Count; j++){
            if (TacticianInformationSheet.StashInventoryData[j].Value.customID == itemKey.Value.customID){
                itemIndex = j;
                break;
            }
        }
        // If the item is found, remove it
        if (itemIndex != -1){
            TacticianInformationSheet.StashInventoryData.RemoveAt(itemIndex);
        }
        ContentSizer.Invoke();
        CraftReturn.Invoke();

    }
    [Server]
    public void GetCharacterPickedUpItem(string charID, CharacterInventoryListItem DATA)
    {
        int sheetIndex = -1;
        int itemIndex = -1;
        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                // Find the index of the CharacterInventoryListItem with the matching key
                for (int j = 0; j < InformationSheets[i].CharInventoryData.Count; j++)
                {
                    if (InformationSheets[i].CharInventoryData[j].Value.customID == DATA.Value.customID)
                    {
                        itemIndex = j;
                        break;
                    }
                }
                // If the item is found, remove it
                if (itemIndex != -1)
                {
                    InformationSheets[i].CharInventoryData.RemoveAt(itemIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharInventoryData.Add(DATA);
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                TargetGetCharacterPickedUpItem(charID, DATA);
                break;
            }
        }
    }
    [TargetRpc]
    public void TargetGetCharacterPickedUpItem(string charID, CharacterInventoryListItem DATA)
    {
        int sheetIndex = -1;
        int itemIndex = -1;
        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                // Find the index of the CharacterInventoryListItem with the matching key
                for (int j = 0; j < InformationSheets[i].CharInventoryData.Count; j++)
                {
                    if (InformationSheets[i].CharInventoryData[j].Value.customID == DATA.Value.customID)
                    {
                        itemIndex = j;
                        break;
                    }
                }
                // If the item is found, remove it
                if (itemIndex != -1)
                {
                    InformationSheets[i].CharInventoryData.RemoveAt(itemIndex);
                }
                string _class = string.Empty;
                foreach(var stat in InformationSheets[i].CharStatData){
                    if(stat.Key == "Class"){
                        _class = stat.Value;
                        break;
                    }
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharInventoryData.Add(DATA);
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                PickedUpItemCharacter.Invoke(DATA, charID, _class);
                ItemMoved.Invoke();
                CraftReturn.Invoke();
                RefreshSheets.Invoke(charID);
                break;
            }
        }
    }
    [Server]
    public void GetCharacterDeletingItem(string charID, CharacterInventoryListItem DATA)
    {
        int sheetIndex = -1;
        int itemIndex = -1;
        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                // Find the index of the CharacterInventoryListItem with the matching key
                for (int j = 0; j < InformationSheets[i].CharInventoryData.Count; j++)
                {
                    if (InformationSheets[i].CharInventoryData[j].Value.customID == DATA.Value.customID)
                    {
                        itemIndex = j;
                        break;
                    }
                }
                // If the item is found, remove it
                if (itemIndex != -1)
                {
                    InformationSheets[i].CharInventoryData.RemoveAt(itemIndex);
                }
                TargetGetCharacterDeletingItem(charID, DATA);
                DATA.Value.amount = 0;
                DATA.Value.Deleted = true;
                DATA.Value.Changed = true;
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharInventoryData.Add(DATA);
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                break;
            }
        }
    }
    [TargetRpc]
    public void TargetGetCharacterDeletingItem(string charID, CharacterInventoryListItem DATA)
    {
        int sheetIndex = -1;
        int itemIndex = -1;
        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                // Find the index of the CharacterInventoryListItem with the matching key
                for (int j = 0; j < InformationSheets[i].CharInventoryData.Count; j++)
                {
                    if (InformationSheets[i].CharInventoryData[j].Value.customID == DATA.Value.customID)
                    {
                        itemIndex = j;
                        break;
                    }
                }
                // If the item is found, remove it
                if (itemIndex != -1)
                {
                    InformationSheets[i].CharInventoryData.RemoveAt(itemIndex);
                }
                DATA.Value.amount = 0;
                DATA.Value.Deleted = true;
                DATA.Value.Changed = true;
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharInventoryData.Add(DATA);
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                Refreshitem.Invoke(this, DATA.Value);
                RefreshSheets.Invoke( charID);
        CraftReturn.Invoke();

                ItemMoved.Invoke();
                break;
            }
        }
    }
    [Server]
    public void GetCharacterNewItem(string charID, CharacterInventoryListItem DATA)
    {
        //print($"custom id = {DATA.Value.customID} for item {DATA.Value.GetItemName()}");
        int sheetIndex = -1;
        int itemIndex = -1;
        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                // Find the index of the CharacterInventoryListItem with the matching key
                for (int j = 0; j < InformationSheets[i].CharInventoryData.Count; j++)
                {
                    //print($"custom id = {DATA.Value.customID} and item checked is {InformationSheets[i].CharInventoryData[j].Value.customID} for {InformationSheets[i].CharInventoryData[j].Value.GetItemName()}");

                    if (InformationSheets[i].CharInventoryData[j].Value.customID == DATA.Value.customID)
                    {
                        print($"{InformationSheets[i].CharInventoryData[j].Value.GetItemName()} was a match!");

                        itemIndex = j;
                        break;
                    }
                }
                // If the item is found, remove it
                if (itemIndex != -1)
                {
                    InformationSheets[i].CharInventoryData.RemoveAt(itemIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharInventoryData.Add(DATA);
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                TargetGetCharacterNewItem(charID, DATA);
                break;
            }
        }
    }
    [TargetRpc]
    public void TargetGetCharacterNewItem(string charID, CharacterInventoryListItem DATA)
    {
      //  print($"custom id = {DATA.Value.customID} for item {DATA.Value.GetItemName()} with a quantity of {DATA.Value.GetAmount()}");
        int sheetIndex = -1;
        int itemIndex = -1;
        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                // Find the index of the CharacterInventoryListItem with the matching key
                for (int j = 0; j < InformationSheets[i].CharInventoryData.Count; j++)
                {
                    //print($"custom id = {DATA.Value.customID} and item checked is {InformationSheets[i].CharInventoryData[j].Value.customID} for {InformationSheets[i].CharInventoryData[j].Value.GetItemName()}");
                    if (InformationSheets[i].CharInventoryData[j].Value.customID == DATA.Value.customID)
                    {
                        print($"{InformationSheets[i].CharInventoryData[j].Value.GetItemName()} was a match!");
                        itemIndex = j;
                        break;
                    }
                }
                // If the item is found, remove it
                if (itemIndex != -1)
                {
                    InformationSheets[i].CharInventoryData.RemoveAt(itemIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharInventoryData.Add(DATA);
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                Refreshitem.Invoke(this, DATA.Value);
                RefreshSheets.Invoke( charID);
                ItemMoved.Invoke();
        CraftReturn.Invoke();

                break;
            }
        }
    }
    [Server]
public void RemoveCharacterItem(string charID, CharacterInventoryListItem itemKey)
{
    int sheetIndex = -1;
    int itemIndex = -1;

    // Find the character sheet with the matching CharacterID
    for (int i = 0; i < InformationSheets.Count; i++)
    {
        if (InformationSheets[i].CharacterID == charID)
        {
            sheetIndex = i;

            // Find the index of the CharacterInventoryListItem with the matching key
            for (int j = 0; j < InformationSheets[i].CharInventoryData.Count; j++)
            {
                if (InformationSheets[i].CharInventoryData[j].Value.customID == itemKey.Value.customID)
                {
                    itemIndex = j;
                    break;
                }
            }

            // If the item is found, remove it
            if (itemIndex != -1)
            {
                InformationSheets[i].CharInventoryData.RemoveAt(itemIndex);
            }

            // Update the character sheet in the InformationSheets
            InformationSheets[i] = InformationSheets[sheetIndex];
            TargetRemoveCharacterItem(charID, itemKey);
            break;
        }
    }
}

[TargetRpc]
public void TargetRemoveCharacterItem(string charID, CharacterInventoryListItem itemKey)
{
    int sheetIndex = -1;
    int itemIndex = -1;

    // Find the character sheet with the matching CharacterID
    for (int i = 0; i < InformationSheets.Count; i++)
    {
        if (InformationSheets[i].CharacterID == charID)
        {
            sheetIndex = i;

            // Find the index of the CharacterInventoryListItem with the matching key
            for (int j = 0; j < InformationSheets[i].CharInventoryData.Count; j++)
            {
                if (InformationSheets[i].CharInventoryData[j].Value.customID == itemKey.Value.customID)
                {
                    itemIndex = j;
                    break;
                }
            }
            // If the item is found, remove it
            if (itemIndex != -1)
            {
                InformationSheets[i].CharInventoryData.RemoveAt(itemIndex);
            }
            // Update the character sheet in the InformationSheets
            InformationSheets[i] = InformationSheets[sheetIndex];
            RefreshSheets.Invoke( charID);
                ItemMoved.Invoke();
        CraftReturn.Invoke();


            break;
        }
    }
}
    [Server]
    public void GetCharacterSpellItemPurchase(string charID, CharacterSpellListItem SpellDATA, CharacterStatListItem StatDATA)
    {
        int sheetIndex = -1;
        int spellIndex = -1;
        int statIndex = -1;

        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                // Find the index of the CharacterInventoryListItem with the matching key
                for (int j = 0; j < InformationSheets[i].CharSpellData.Count; j++)
                {
                    if (InformationSheets[i].CharSpellData[j].Key == SpellDATA.Key)
                    {
                        spellIndex = j;
                        break;
                    }
                }
                // If the item is found, remove it
                if (spellIndex != -1)
                {
                    InformationSheets[i].CharSpellData.RemoveAt(spellIndex);
                }
                for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                {
                    if (InformationSheets[i].CharStatData[s].Key == StatDATA.Key)
                    {
                        statIndex = s;
                        break;
                    }
                }
                if (statIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(statIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharSpellData.Add(SpellDATA);
                InformationSheets[i].CharStatData.Add(StatDATA);
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                TargetGetCharacterSpellItemPurchase(charID, SpellDATA, StatDATA);
                print($"spell data was updated: {SpellDATA.Updated} and our new Classpoints value was: {StatDATA.Value}");
                break;
            }
        }
    }
    [TargetRpc]
    public void TargetGetCharacterSpellItemPurchase(string charID, CharacterSpellListItem SpellDATA, CharacterStatListItem StatDATA)
    {
        int sheetIndex = -1;
        int spellIndex = -1;
        int statIndex = -1;

        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                // Find the index of the CharacterInventoryListItem with the matching key
                for (int j = 0; j < InformationSheets[i].CharSpellData.Count; j++)
                {
                    if (InformationSheets[i].CharSpellData[j].Key == SpellDATA.Key)
                    {
                        spellIndex = j;
                        break;
                    }
                }
                // If the item is found, remove it
                if (spellIndex != -1)
                {
                    InformationSheets[i].CharSpellData.RemoveAt(spellIndex);
                }
                for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                {
                    if (InformationSheets[i].CharStatData[s].Key == StatDATA.Key)
                    {
                        statIndex = s;
                    }

                }
                if (statIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(statIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharSpellData.Add(SpellDATA);
                InformationSheets[i].CharStatData.Add(StatDATA);
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                print($"spell data was updated: {SpellDATA.Updated} and our new Classpoints value was: {StatDATA.Value}");

                RefreshSheets.Invoke( charID);
                ResetSpring.Invoke(charID);
                ResetCompendium.Invoke();
                break;
            }
        }
    }
    [Server]
    public void GetCharacterSpellChange(string charID, CharacterSpellListItem SpellUpdatedDATA, CharacterSpellListItem possibleChangeDATA, SendSpellList spellList)
    {
        int sheetIndex = -1;
        int spellNewIndex = -1;
        int spellOldIndex = -1;
        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                // Find the index of the CharacterInventoryListItem with the matching key
                for (int j = 0; j < InformationSheets[i].CharSpellData.Count; j++)
                {
                    if (InformationSheets[i].CharSpellData[j].Key == SpellUpdatedDATA.Key)
                    {
                        spellNewIndex = j;
                        break;
                    }
                }
                // If the item is found, remove it
                if (spellNewIndex != -1)
                {
                    InformationSheets[i].CharSpellData.RemoveAt(spellNewIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharSpellData.Add(SpellUpdatedDATA);
                if(possibleChangeDATA.Key != "Empty"){
                    for (int X = 0; X < InformationSheets[i].CharSpellData.Count; X++)
                    {
                        if (InformationSheets[i].CharSpellData[X].Key == possibleChangeDATA.Key)
                        {
                            spellOldIndex = X;
                            break;
                        }
                    }
                    // If the item is found, remove it
                    if (spellOldIndex != -1)
                    {
                        InformationSheets[i].CharSpellData.RemoveAt(spellOldIndex);
                    }
                        InformationSheets[i].CharSpellData.Add(possibleChangeDATA);

                }
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                break;
            }
        }
        //if(currentScene != "OVM" && currentScene != "TOWNOFARUDINE"){
        if(StatAsset.Instance.CheckForCombatZone(currentScene)){

            ChangingMOSpellsMatch.Invoke(charID, this, spellList);
        }
        TargetGetCharacterSpellChange(charID, SpellUpdatedDATA, possibleChangeDATA);
    }
    [TargetRpc]
    public void TargetGetCharacterSpellChange(string charID, CharacterSpellListItem SpellUpdatedDATA, CharacterSpellListItem possibleChangeDATA)
    {
        int sheetIndex = -1;
        int spellNewIndex = -1;
        int spellOldIndex = -1;
        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                // Find the index of the CharacterInventoryListItem with the matching key
                for (int j = 0; j < InformationSheets[i].CharSpellData.Count; j++)
                {
                    if (InformationSheets[i].CharSpellData[j].Key == SpellUpdatedDATA.Key)
                    {
                        spellNewIndex = j;
                        break;
                    }
                }
                // If the item is found, remove it
                if (spellNewIndex != -1)
                {
                    InformationSheets[i].CharSpellData.RemoveAt(spellNewIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharSpellData.Add(SpellUpdatedDATA);
                if(possibleChangeDATA.Key != "Empty"){
                    for (int X = 0; X < InformationSheets[i].CharSpellData.Count; X++)
                    {
                        if (InformationSheets[i].CharSpellData[X].Key == possibleChangeDATA.Key)
                        {
                            spellOldIndex = X;
                            break;
                        }
                    }
                    // If the item is found, remove it
                    if (spellOldIndex != -1)
                    {
                        InformationSheets[i].CharSpellData.RemoveAt(spellOldIndex);
                        InformationSheets[i].CharSpellData.Add(possibleChangeDATA);
                    }
                }
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                break;
            }
        }
        RefreshSheets.Invoke( charID);
    }
    //Save Game
    [Server]

    public void GetSavedGame(CharacterSaveData savedData){
        int sheetIndex = -1;
        int HPIndex = -1;
        int MPIndex = -1;
        int ClassPointsIndex = -1;
        int expIndex = -1;
        CharacterStatListItem HP = (new CharacterStatListItem{
            Key = "currentHP",
            Value = savedData.CharHealth.ToString()
        });
        CharacterStatListItem MP = (new CharacterStatListItem{
            Key = "currentMP",
            Value =  savedData.CharMana.ToString()
        });
        CharacterStatListItem EXP = (new CharacterStatListItem{
            Key = "EXP",
            Value =  savedData.CharExperience.ToString("F2")
        });
        CharacterStatListItem CLASSPOINTS = (new CharacterStatListItem{
            Key = "ClassPoints",
            Value =  savedData.CharClassPoints.ToString("F2")
        });


        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == savedData.CharID)
            {
                sheetIndex = i;
                for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                {
                    if (InformationSheets[i].CharStatData[s].Key == HP.Key)
                    {
                        HPIndex = s;
                        break;
                    }
                }
                if (HPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(HPIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(HP);

                for (int m = 0; m < InformationSheets[i].CharStatData.Count; m++)
                {
                    if (InformationSheets[i].CharStatData[m].Key == MP.Key)
                    {
                        MPIndex = m;
                        break;
                    }
                }
                if (MPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(MPIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(MP);

                for (int X = 0; X < InformationSheets[i].CharStatData.Count; X++)
                {
                    if (InformationSheets[i].CharStatData[X].Key == CLASSPOINTS.Key)
                    {
                        ClassPointsIndex = X;
                        break;
                    }
                }
                if (ClassPointsIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(ClassPointsIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(CLASSPOINTS);

                for (int R = 0; R < InformationSheets[i].CharStatData.Count; R++)
                {
                    if (InformationSheets[i].CharStatData[R].Key == EXP.Key)
                    {
                        expIndex = R;
                        break;
                    }
                }
                if (expIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(expIndex);
                }
                
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(EXP);

                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                TargetSavedGameR(savedData);
                break;
            }
        }
    }
    [TargetRpc]

    public void TargetSavedGameR(CharacterSaveData savedData){
        int sheetIndex = -1;
        int HPIndex = -1;
        int MPIndex = -1;
        int ClassPointsIndex = -1;
        int expIndex = -1;
        CharacterStatListItem HP = (new CharacterStatListItem{
            Key = "currentHP",
            Value = savedData.CharHealth.ToString()
        });
        CharacterStatListItem MP = (new CharacterStatListItem{
            Key = "currentMP",
            Value =  savedData.CharMana.ToString()
        });
        CharacterStatListItem EXP = (new CharacterStatListItem{
            Key = "EXP",
            Value =  savedData.CharExperience.ToString("F2")
        });
        CharacterStatListItem CLASSPOINTS = (new CharacterStatListItem{
            Key = "ClassPoints",
            Value =  savedData.CharClassPoints.ToString("F2")
        });


        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == savedData.CharID)
            {
                sheetIndex = i;
                for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                {
                    if (InformationSheets[i].CharStatData[s].Key == HP.Key)
                    {
                        HPIndex = s;
                        break;
                    }
                }
                if (HPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(HPIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(HP);

                for (int m = 0; m < InformationSheets[i].CharStatData.Count; m++)
                {
                    if (InformationSheets[i].CharStatData[m].Key == MP.Key)
                    {
                        MPIndex = m;
                        break;
                    }
                }
                if (MPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(MPIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(MP);

                for (int X = 0; X < InformationSheets[i].CharStatData.Count; X++)
                {
                    if (InformationSheets[i].CharStatData[X].Key == CLASSPOINTS.Key)
                    {
                        ClassPointsIndex = X;
                        break;
                    }
                }
                if (ClassPointsIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(ClassPointsIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(CLASSPOINTS);

                for (int R = 0; R < InformationSheets[i].CharStatData.Count; R++)
                {
                    if (InformationSheets[i].CharStatData[R].Key == EXP.Key)
                    {
                        expIndex = R;
                        break;
                    }
                }
                if (expIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(expIndex);
                }
                
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(EXP);

                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                RefreshSheets.Invoke( savedData.CharID);
                break;
            }
        }
    }
    [Server]

    public void GetDEATHCHARACTER(string charID, CharacterStatListItem DEATH, CharacterStatListItem EXP){
        int sheetIndex = -1;
        int HPIndex = -1;
        int MPIndex = -1;
        int DEATHIndex = -1;
        int expIndex = -1;
        CharacterStatListItem HP = (new CharacterStatListItem{
            Key = "currentHP",
            Value = "0"
        });
        //CharacterStatListItem MP = (new CharacterStatListItem{
        //    Key = "currentMP",
        //    Value = "0"
        //});

        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                {
                    if (InformationSheets[i].CharStatData[s].Key == HP.Key)
                    {
                        HPIndex = s;
                        break;
                    }
                }
                if (HPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(HPIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(HP);
                //for (int m = 0; m < InformationSheets[i].CharStatData.Count; m++)
                //{
                //    if (InformationSheets[i].CharStatData[m].Key == MP.Key)
                //    {
                //        MPIndex = m;
                //        break;
                //    }
                //}
                //if (MPIndex != -1)
                //{
                //    InformationSheets[i].CharStatData.RemoveAt(MPIndex);
                //}
                //// Add the new CharacterInventoryListItem to the CharInventoryData list
                //InformationSheets[i].CharStatData.Add(MP);

                for (int X = 0; X < InformationSheets[i].CharStatData.Count; X++)
                {
                    if (InformationSheets[i].CharStatData[X].Key == DEATH.Key)
                    {
                        DEATHIndex = X;
                        break;
                    }
                }
                if (DEATHIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(DEATHIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(DEATH);

                for (int R = 0; R < InformationSheets[i].CharStatData.Count; R++)
                {
                    if (InformationSheets[i].CharStatData[R].Key == EXP.Key)
                    {
                        expIndex = R;
                        break;
                    }
                }
                if (expIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(expIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(EXP);
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                TargetDEATHCHARACTER(charID, DEATH, EXP);
                break;
            }
        }
    }
    [TargetRpc]

    public void TargetDEATHCHARACTER(string charID, CharacterStatListItem DEATH, CharacterStatListItem EXP){
        int sheetIndex = -1;
        int HPIndex = -1;
        int MPIndex = -1;
        int DEATHIndex = -1;
        int expIndex = -1;

        CharacterStatListItem HP = (new CharacterStatListItem{
            Key = "currentHP",
            Value = "0"
        });
        //CharacterStatListItem MP = (new CharacterStatListItem{
        //    Key = "currentMP",
        //    Value = "0"
        //});
        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                {
                    if (InformationSheets[i].CharStatData[s].Key == HP.Key)
                    {
                        HPIndex = s;
                        break;
                    }
                }
                if (HPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(HPIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(HP);
                //for (int m = 0; m < InformationSheets[i].CharStatData.Count; m++)
                //{
                //    if (InformationSheets[i].CharStatData[m].Key == MP.Key)
                //    {
                //        MPIndex = m;
                //        break;
                //    }
                //}
                //if (MPIndex != -1)
                //{
                //    InformationSheets[i].CharStatData.RemoveAt(MPIndex);
                //}
                //// Add the new CharacterInventoryListItem to the CharInventoryData list
                //InformationSheets[i].CharStatData.Add(MP);

                for (int X = 0; X < InformationSheets[i].CharStatData.Count; X++)
                {
                    if (InformationSheets[i].CharStatData[X].Key == DEATH.Key)
                    {
                        DEATHIndex = X;
                        break;
                    }
                }
                if (DEATHIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(DEATHIndex);
                }
                
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(DEATH);
                for (int R = 0; R < InformationSheets[i].CharStatData.Count; R++)
                {
                    if (InformationSheets[i].CharStatData[R].Key == EXP.Key)
                    {
                        expIndex = R;
                        break;
                    }
                }
                if (expIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(expIndex);
                }
                InformationSheets[i].CharStatData.Add(EXP);

                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                RefreshSheets.Invoke( charID);
                DeathBroadcast.Invoke(this, charID);
                break;
            }
        }
    }
    [Server]

    public void ServerResurrectCharacterCombat(string charID, CharacterStatListItem HP, CharacterStatListItem MP){
        int sheetIndex = -1;
        int HPIndex = -1;
        int MPIndex = -1;
        int DeathIndex = -1;

        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                for (int v = 0; v < InformationSheets[i].CharStatData.Count; v++)
                {
                    if (InformationSheets[i].CharStatData[v].Key == MP.Key)
                    {
                        MPIndex = v;
                        break;
                    }
                }
                if (MPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(MPIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(MP);
                for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                {
                    if (InformationSheets[i].CharStatData[s].Key == HP.Key)
                    {
                        HPIndex = s;
                        break;
                    }
                }
                if (HPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(HPIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(HP);
                for (int X = 0; X < InformationSheets[i].CharStatData.Count; X++)
                {
                    if (InformationSheets[i].CharStatData[X].Key == "DEATH")
                    {
                        DeathIndex = X;
                        break;
                    }
                }
                if (DeathIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(DeathIndex);
                }
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                TargetResurrectCharacterCombat(charID, HP, MP);
                break;
            }
        }
    }
    [TargetRpc]

    public void TargetResurrectCharacterCombat(string charID, CharacterStatListItem HP, CharacterStatListItem MP){
        int sheetIndex = -1;
        int HPIndex = -1;
        int MPIndex = -1;
        int DeathIndex = -1;

        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                for (int v = 0; v < InformationSheets[i].CharStatData.Count; v++)
                {
                    if (InformationSheets[i].CharStatData[v].Key == MP.Key)
                    {
                        MPIndex = v;
                        break;
                    }
                }
                if (MPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(MPIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(MP);
                for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                {
                    if (InformationSheets[i].CharStatData[s].Key == HP.Key)
                    {
                        HPIndex = s;
                        break;
                    }
                }
                if (HPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(HPIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(HP);
                for (int X = 0; X < InformationSheets[i].CharStatData.Count; X++)
                {
                    if (InformationSheets[i].CharStatData[X].Key == "DEATH")
                    {
                        DeathIndex = X;
                        break;
                    }
                }
                if (DeathIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(DeathIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                RefreshSheets.Invoke( charID);
                Ressurected.Invoke(this, charID);
                break;
            }
        }
    }
     [Server]

    public void ServerResurrectCharacter(string charID, CharacterStatListItem HP){
        int sheetIndex = -1;
        int HPIndex = -1;
        int DeathIndex = -1;

        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                {
                    if (InformationSheets[i].CharStatData[s].Key == HP.Key)
                    {
                        HPIndex = s;
                        break;
                    }
                }
                if (HPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(HPIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(HP);
                for (int X = 0; X < InformationSheets[i].CharStatData.Count; X++)
                {
                    if (InformationSheets[i].CharStatData[X].Key == "DEATH")
                    {
                        DeathIndex = X;
                        break;
                    }
                }
                if (DeathIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(DeathIndex);
                }
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                TargetResurrectCharacter(charID, HP);
                break;
            }
        }
    }
    [TargetRpc]

    public void TargetResurrectCharacter(string charID, CharacterStatListItem HP){
        int sheetIndex = -1;
        int HPIndex = -1;
        int DeathIndex = -1;

        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                {
                    if (InformationSheets[i].CharStatData[s].Key == HP.Key)
                    {
                        HPIndex = s;
                        break;
                    }
                }
                if (HPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(HPIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(HP);
                for (int X = 0; X < InformationSheets[i].CharStatData.Count; X++)
                {
                    if (InformationSheets[i].CharStatData[X].Key == "DEATH")
                    {
                        DeathIndex = X;
                        break;
                    }
                }
                if (DeathIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(DeathIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                RefreshSheets.Invoke( charID);
                Ressurected.Invoke(this, charID);
                break;
            }
        }
    }
    [Server]

    public void GetCharacterUpdateDuraDeath(string charID, CharacterInventoryListItem damagedItem){
        int sheetIndex = -1;
        int DurabilityIndex = -1;

        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                if(damagedItem.Key != "LuckyRoll"){
                    for (int X = 0; X < InformationSheets[i].CharInventoryData.Count; X++)
                    {
                        if (InformationSheets[i].CharInventoryData[X].Value.customID == damagedItem.Value.customID)
                        {
                            DurabilityIndex = X;
                            break;
                        }
                    }
                    if (DurabilityIndex != -1)
                    {
                        InformationSheets[i].CharInventoryData.RemoveAt(DurabilityIndex);
                    }
                    // Add the new CharacterInventoryListItem to the CharInventoryData list
                    InformationSheets[i].CharInventoryData.Add(damagedItem);
                }
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                TargetCharacterUpdateDuraDeath(charID, damagedItem);
                break;
            }
        }
    }
    [TargetRpc]

    public void TargetCharacterUpdateDuraDeath(string charID, CharacterInventoryListItem damagedItem){
        int sheetIndex = -1;
        int DurabilityIndex = -1;

        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                if(damagedItem.Key != "LuckyRoll"){
                    for (int X = 0; X < InformationSheets[i].CharInventoryData.Count; X++)
                    {
                        if (InformationSheets[i].CharInventoryData[X].Value.customID == damagedItem.Value.customID)
                        {
                            DurabilityIndex = X;
                            break;
                        }
                    }
                    if (DurabilityIndex != -1)
                    {
                        InformationSheets[i].CharInventoryData.RemoveAt(DurabilityIndex);
                    }
                    // Add the new CharacterInventoryListItem to the CharInventoryData list
                    InformationSheets[i].CharInventoryData.Add(damagedItem);
                    ResetItemSelectable.Invoke(damagedItem.Value);
                }
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                RefreshSheets.Invoke( charID);
                break;
            }
        }
    }
    [Server]

    public void GetCharacterUpdateHarvestDurability(string charID, CharacterInventoryListItem damagedItem){
        int sheetIndex = -1;
        int DurabilityIndex = -1;

        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                if(damagedItem.Key != "LuckyRoll"){
                    for (int X = 0; X < InformationSheets[i].CharInventoryData.Count; X++)
                    {
                        if (InformationSheets[i].CharInventoryData[X].Value.customID == damagedItem.Value.customID)
                        {
                            DurabilityIndex = X;
                            break;
                        }
                    }
                    if (DurabilityIndex != -1)
                    {
                        InformationSheets[i].CharInventoryData.RemoveAt(DurabilityIndex);
                    }
                    // Add the new CharacterInventoryListItem to the CharInventoryData list
                    InformationSheets[i].CharInventoryData.Add(damagedItem);
                }
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                TargetCharacterUpdateHarvestDurability(charID, damagedItem);
                break;
            }
        }
    }
    [TargetRpc]

    public void TargetCharacterUpdateHarvestDurability(string charID, CharacterInventoryListItem damagedItem){
        int sheetIndex = -1;
        int DurabilityIndex = -1;

        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                if(damagedItem.Key != "LuckyRoll"){
                    for (int X = 0; X < InformationSheets[i].CharInventoryData.Count; X++)
                    {
                        if (InformationSheets[i].CharInventoryData[X].Value.customID == damagedItem.Value.customID)
                        {
                            DurabilityIndex = X;
                            break;
                        }
                    }
                    if (DurabilityIndex != -1)
                    {
                        InformationSheets[i].CharInventoryData.RemoveAt(DurabilityIndex);
                    }
                    // Add the new CharacterInventoryListItem to the CharInventoryData list
                    InformationSheets[i].CharInventoryData.Add(damagedItem);
                    ResetItemSelectable.Invoke(damagedItem.Value);
                }
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                RefreshSheets.Invoke( charID);
                break;
            }
        }
    }
    [Server]

    public void ServerRepairItem(string charID, CharacterInventoryListItem damagedItem){
        int sheetIndex = -1;
        int DurabilityIndex = -1;
        if(charID == "Tactician"){
            for (int p = 0; p < TacticianInformationSheet.TacticianInventoryData.Count; p++)
            {
                if (TacticianInformationSheet.TacticianInventoryData[p].Value.customID == damagedItem.Value.customID)
                {
                    DurabilityIndex = p;
                    break;
                }
            }
            if (DurabilityIndex != -1)
            {
                TacticianInformationSheet.TacticianInventoryData.RemoveAt(DurabilityIndex);
            }
            // Add the new CharacterInventoryListItem to the CharInventoryData list
            TacticianInformationSheet.TacticianInventoryData.Add(damagedItem);
            TargetRepairItem(charID, damagedItem);
        } else {
            // Find the character sheet with the matching CharacterID
            for (int i = 0; i < InformationSheets.Count; i++)
            {
                if (InformationSheets[i].CharacterID == charID)
                {
                    sheetIndex = i;

                    if(damagedItem.Key != "LuckyRoll"){
                        for (int X = 0; X < InformationSheets[i].CharInventoryData.Count; X++)
                        {
                            if (InformationSheets[i].CharInventoryData[X].Value.customID == damagedItem.Value.customID)
                            {
                                DurabilityIndex = X;
                                break;
                            }
                        }
                        if (DurabilityIndex != -1)
                        {
                            InformationSheets[i].CharInventoryData.RemoveAt(DurabilityIndex);
                        }
                        // Add the new CharacterInventoryListItem to the CharInventoryData list
                        InformationSheets[i].CharInventoryData.Add(damagedItem);
                    }
                    // Update the character sheet in the InformationSheets
                    InformationSheets[i] = InformationSheets[sheetIndex];
                    TargetRepairItem(charID, damagedItem);
                    break;
                }
            }
        }
        
    }
    [TargetRpc]
    public void TargetRepairedAll(){
        RepairCompletedAll.Invoke();
    }
    [TargetRpc]

    public void TargetRepairItem(string charID, CharacterInventoryListItem damagedItem){
        int sheetIndex = -1;
        int DurabilityIndex = -1;
        if(charID == "Tactician"){
            for (int p = 0; p < TacticianInformationSheet.TacticianInventoryData.Count; p++)
            {
                if (TacticianInformationSheet.TacticianInventoryData[p].Value.customID == damagedItem.Value.customID)
                {
                    DurabilityIndex = p;
                    break;
                }
            }
            if (DurabilityIndex != -1)
            {
                TacticianInformationSheet.TacticianInventoryData.RemoveAt(DurabilityIndex);
            }
            // Add the new CharacterInventoryListItem to the CharInventoryData list
            TacticianInformationSheet.TacticianInventoryData.Add(damagedItem);
            ResetItemSelectable.Invoke(damagedItem.Value);
            RefreshSheets.Invoke( charID);
            RepairCompletedOnItem.Invoke(damagedItem.Value.customID);
        } else {
            // Find the character sheet with the matching CharacterID
            for (int i = 0; i < InformationSheets.Count; i++)
            {
                if (InformationSheets[i].CharacterID == charID)
                {
                    sheetIndex = i;
                    if(damagedItem.Key != "LuckyRoll"){
                        for (int X = 0; X < InformationSheets[i].CharInventoryData.Count; X++)
                        {
                            if (InformationSheets[i].CharInventoryData[X].Value.customID == damagedItem.Value.customID)
                            {
                                DurabilityIndex = X;
                                break;
                            }
                        }
                        if (DurabilityIndex != -1)
                        {
                            InformationSheets[i].CharInventoryData.RemoveAt(DurabilityIndex);
                        }
                        // Add the new CharacterInventoryListItem to the CharInventoryData list
                        InformationSheets[i].CharInventoryData.Add(damagedItem);
                        ResetItemSelectable.Invoke(damagedItem.Value);
                    }
                    // Update the character sheet in the InformationSheets
                    InformationSheets[i] = InformationSheets[sheetIndex];
                    RefreshSheets.Invoke( charID);
                    RepairCompletedOnItem.Invoke(damagedItem.Value.customID);
                    break;
                }
            }
        }
    }
    [Server]

    public void GetCharacterUpdateHPDurability(string charID, CharacterStatListItem HP, CharacterInventoryListItem damagedItem){
        int sheetIndex = -1;
        int HPIndex = -1;
        int DurabilityIndex = -1;

        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                {
                    if (InformationSheets[i].CharStatData[s].Key == HP.Key)
                    {
                        HPIndex = s;
                        break;
                    }
                }
                if (HPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(HPIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(HP);
                if(damagedItem.Key != "LuckyRoll"){
                    for (int X = 0; X < InformationSheets[i].CharInventoryData.Count; X++)
                    {
                        if (InformationSheets[i].CharInventoryData[X].Value.customID == damagedItem.Value.customID)
                        {
                            DurabilityIndex = X;
                            break;
                        }
                    }
                    if (DurabilityIndex != -1)
                    {
                        InformationSheets[i].CharInventoryData.RemoveAt(DurabilityIndex);
                    }
                    // Add the new CharacterInventoryListItem to the CharInventoryData list
                    InformationSheets[i].CharInventoryData.Add(damagedItem);
                }
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                string customIDKey = string.Empty;
                string durability = string.Empty;
                if(damagedItem.Value != null){
                    customIDKey = damagedItem.Value.customID;
                    durability = damagedItem.Value.Durability;
                }
                TargetCharacterUpdateHPDurability(charID, HP.Value, damagedItem.Key, customIDKey, durability);
                break;
            }
        }
    }
    [TargetRpc]

    public void TargetCharacterUpdateHPDurability(string charID, string HPValue, string damagedItemCustomKey, string damagedItemCustomValue, string durabilityValue){
        int sheetIndex = -1;
        int HPIndex = -1;
        int DurabilityIndex = -1;
        CharacterStatListItem HP = (new CharacterStatListItem{
            Key = "currentHP",
            Value = HPValue
        });
        CharacterInventoryListItem damagedItem = (new CharacterInventoryListItem{
            Key = damagedItemCustomKey
        });
        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                {
                    if (InformationSheets[i].CharStatData[s].Key == HP.Key)
                    {
                        HPIndex = s;
                        break;
                    }
                }
                if (HPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(HPIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(HP);
                if(damagedItemCustomKey != "LuckyRoll"){
                    for (int X = 0; X < InformationSheets[i].CharInventoryData.Count; X++)
                    {
                        if (InformationSheets[i].CharInventoryData[X].Value.customID == damagedItemCustomValue)
                        {
                            damagedItem.Value = InformationSheets[i].CharInventoryData[X].Value;
                            damagedItem.Value.Durability = durabilityValue;
                            DurabilityIndex = X;
                            break;
                        }
                    }
                    if (DurabilityIndex != -1)
                    {
                        InformationSheets[i].CharInventoryData.RemoveAt(DurabilityIndex);
                    }
                    // Add the new CharacterInventoryListItem to the CharInventoryData list
                    InformationSheets[i].CharInventoryData.Add(damagedItem);
                    ResetItemSelectable.Invoke(damagedItem.Value);
                }
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                RefreshSheets.Invoke( charID);
                break;
            }
        }
    }
    [Server]

    public void ServerRemoveNonBuffSpell(string charID, string spellName){
        int sheetIndex = -1;
        int BuffIndex = -1;
         CharacterBuffListItem BUFF = new CharacterBuffListItem();
        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                    sheetIndex = i;
                    var sheet = InformationSheets[sheetIndex];
                    if(InformationSheets[i].CharBuffData == null) {
                        print("info sheet for this char was null!!!!!!!!!!!!");
                        // Ensure CharBuffData is not null
                        return;
                    }
                    for (int y = 0; y < sheet.CharBuffData.Count; y++)
                    {
                        if (sheet.CharBuffData[y].Key == spellName)
                        {
                            BUFF = sheet.CharBuffData[y];
                            break;
                        }
                    }
                    if(string.IsNullOrEmpty(BUFF.Key)){
                        return;
                    }
                    for (int s = 0; s < sheet.CharBuffData.Count; s++)
                    {
                        if (sheet.CharBuffData[s].Key == BUFF.Key)
                        {
                            BuffIndex = s;
                            break;
                        }
                    }
                    if (BuffIndex != -1)
                    {
                        sheet.CharBuffData.RemoveAt(BuffIndex);
                            print("CharBuffData was removed!!!!!!!!!!!!");

                    }

                InformationSheets[i] = sheet;

                // Update the character sheet in the InformationSheets
                //InformationSheets[i] = InformationSheets[sheetIndex];
                TargetRemoveNonBuffSpell(charID, BUFF);
                break;
            }
        }
    }
    [TargetRpc]

    public void TargetRemoveNonBuffSpell(string charID, CharacterBuffListItem BUFF){
        int sheetIndex = -1;
        int BuffIndex = -1;
        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                    sheetIndex = i;
                    var sheet = InformationSheets[sheetIndex];
                    if(InformationSheets[i].CharBuffData == null) {
                        print("info sheet for this char was null!!!!!!!!!!!!");
                        // Ensure CharBuffData is not null
                        return;
                    }
                    for (int s = 0; s < sheet.CharBuffData.Count; s++)
                    {
                        if (sheet.CharBuffData[s].Key == BUFF.Key)
                        {
                            BuffIndex = s;
                            break;
                        }
                    }
                    if (BuffIndex != -1)
                    {
                        sheet.CharBuffData.RemoveAt(BuffIndex);
                            print("CharBuffData was removed!!!!!!!!!!!!");

                    }

                InformationSheets[i] = sheet;
                RefreshSheets.Invoke( charID);
                //HideBuffRemoved.Invoke("Hide", "Stealthed");
                break;
            }
        }
    }
    [Server]

    public void ServerRemoveBuff(string charID, CharacterBuffListItem BUFF){
        int sheetIndex = -1;
        int BuffIndex = -1;
        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                    sheetIndex = i;
                    var sheet = InformationSheets[sheetIndex];
                    if(InformationSheets[i].CharBuffData == null) {
                        print("info sheet for this char was null!!!!!!!!!!!!");
                        // Ensure CharBuffData is not null
                        return;
                    }
                    for (int s = 0; s < sheet.CharBuffData.Count; s++)
                    {
                        if (sheet.CharBuffData[s].Key == BUFF.Key)
                        {
                            BuffIndex = s;
                            break;
                        }
                    }
                    if (BuffIndex != -1)
                    {
                        sheet.CharBuffData.RemoveAt(BuffIndex);
                            print("CharBuffData was removed!!!!!!!!!!!!");

                    }

                InformationSheets[i] = sheet;

                // Update the character sheet in the InformationSheets
                //InformationSheets[i] = InformationSheets[sheetIndex];
                TargetRemoveBuff(charID, BUFF);
                break;
            }
        }
    }
    [TargetRpc]

    public void TargetRemoveBuff(string charID, CharacterBuffListItem BUFF){
        int sheetIndex = -1;
        int BuffIndex = -1;
        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                    sheetIndex = i;
                    var sheet = InformationSheets[sheetIndex];
                    if(InformationSheets[i].CharBuffData == null) {
                        print("info sheet for this char was null!!!!!!!!!!!!");
                        // Ensure CharBuffData is not null
                        return;
                    }
                    for (int s = 0; s < sheet.CharBuffData.Count; s++)
                    {
                        if (sheet.CharBuffData[s].Key == BUFF.Key)
                        {
                            BuffIndex = s;
                            break;
                        }
                    }
                    if (BuffIndex != -1)
                    {
                        sheet.CharBuffData.RemoveAt(BuffIndex);
                            print("CharBuffData was removed!!!!!!!!!!!!");

                    }

                InformationSheets[i] = sheet;
                RefreshSheets.Invoke( charID);
                if(StatAsset.Instance.CheckForCombatZone(currentScene)){
                    CombatPartyView.instance.CheckMovingObjectIDCheck(charID);
                }
                break;
            }
        }
    }
    [Server]

    public void ServerAddBuff(string charID, CharacterBuffListItem BUFF){
        int sheetIndex = -1;
        int BuffIndex = -1;
        int FoodIndex = -1;
        int samePotionIndex = -1;
        bool samePotionHaste = false;
        bool samePotionDefense = false;
        if(BUFF.Key == "Defense Potion" || BUFF.Key == "Greater Defense Potion"){
            samePotionDefense  = true;
        }
        if(BUFF.Key == "Haste Potion" || BUFF.Key == "Greater Haste Potion"){
            samePotionHaste  = true;
        }

        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                    sheetIndex = i;
                    var sheet = InformationSheets[sheetIndex];
                    if(InformationSheets[i].CharBuffData == null) {
                        print("info sheet for this char was null!!!!!!!!!!!!");
                        // Ensure CharBuffData is not null
                        if (sheet.CharBuffData == null) {
                            sheet.CharBuffData = new List<CharacterBuffListItem>();
                            print("info sheet for this char was built!!!!!!!!!!!!");
                        }
                    }
                    if(samePotionHaste || samePotionDefense){
                        for (int o = 0; o < sheet.CharBuffData.Count; o++)
                        {
                            if(samePotionHaste){
                                if (sheet.CharBuffData[o].Key == "Greater Haste Potion" || sheet.CharBuffData[o].Key == "Haste Potion")
                                {
                                    samePotionIndex = o;
                                    break;
                                }
                            }
                            if(samePotionDefense){
                                if (sheet.CharBuffData[o].Key == "Greater Defense Potion" || sheet.CharBuffData[o].Key == "Defense Potion")
                                {
                                    samePotionIndex = o;
                                    break;
                                }
                            }
                        }
                        if (samePotionIndex != -1)
                        {
                            print($"{sheet.CharBuffData[samePotionIndex].Key} was removed!!!!!!!!!!!!");
                            sheet.CharBuffData.RemoveAt(samePotionIndex);

                        }
                    }
                    if(BUFF.FoodBuff){
                        for (int l = 0; l < sheet.CharBuffData.Count; l++)
                        {
                            if (sheet.CharBuffData[l].FoodBuff)
                            {
                                FoodIndex = l;
                                break;
                            }
                        }
                        if (FoodIndex != -1)
                        {
                            print($"{sheet.CharBuffData[FoodIndex].Key} was removed!!!!!!!!!!!!");
                            sheet.CharBuffData.RemoveAt(FoodIndex);
                        }
                    }
                    for (int s = 0; s < sheet.CharBuffData.Count; s++)
                    {
                        if (sheet.CharBuffData[s].Key == BUFF.Key)
                        {
                            BuffIndex = s;
                            break;
                        }
                    }
                    if (BuffIndex != -1)
                    {
                            print($"{sheet.CharBuffData[BuffIndex].Key} was removed!!!!!!!!!!!!");
                        sheet.CharBuffData.RemoveAt(BuffIndex);
                            print("CharBuffData was removed!!!!!!!!!!!!");

                    }

                // Add the new CharacterInventoryListItem to the CharInventoryData list
                print($"{BUFF.Key} CharBuffData was added!!!!!!!!!!!!");
                sheet.CharBuffData.Add(BUFF);
                InformationSheets[i] = sheet;

                // Update the character sheet in the InformationSheets
                //InformationSheets[i] = InformationSheets[sheetIndex];
                TargetAddBuff(charID, BUFF);
                break;
            }
        }
    }
    [TargetRpc]
    public void TargetAddBuff(string charID, CharacterBuffListItem BUFF){
        int sheetIndex = -1;
        int BuffIndex = -1;
        int FoodIndex = -1;
        int samePotionIndex = -1;
        bool samePotionHaste = false;
        bool samePotionDefense = false;
        
        if(BUFF.Key == "Defense Potion" || BUFF.Key == "Greater Defense Potion"){
            samePotionDefense  = true;
        }
        if(BUFF.Key == "Haste Potion" || BUFF.Key == "Greater Haste Potion"){
            samePotionHaste  = true;
        }

        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                    sheetIndex = i;
                    var sheet = InformationSheets[sheetIndex];
                    if(InformationSheets[i].CharBuffData == null) {
                        // Ensure CharBuffData is not null
                        if (sheet.CharBuffData == null) {
                            sheet.CharBuffData = new List<CharacterBuffListItem>();
                        }
                    }
                    if(samePotionHaste || samePotionDefense){
                        for (int o = 0; o < sheet.CharBuffData.Count; o++)
                        {
                            if(samePotionHaste){
                                if (sheet.CharBuffData[o].Key == "Greater Haste Potion" || sheet.CharBuffData[o].Key == "Haste Potion")
                                {
                                    samePotionIndex = o;
                                    break;
                                }
                            }
                            if(samePotionDefense){
                                if (sheet.CharBuffData[o].Key == "Greater Defense Potion" || sheet.CharBuffData[o].Key == "Defense Potion")
                                {
                                    samePotionIndex = o;
                                    break;
                                }
                            }

                        }
                        if (samePotionIndex != -1)
                        {
                            print($"{sheet.CharBuffData[samePotionIndex].Key} was removed!!!!!!!!!!!!");

                            sheet.CharBuffData.RemoveAt(samePotionIndex);
                        }
                    }
                    if(BUFF.FoodBuff){
                        for (int l = 0; l < sheet.CharBuffData.Count; l++)
                        {
                            if (sheet.CharBuffData[l].FoodBuff)
                            {
                                FoodIndex = l;
                                break;
                            }
                        }
                        if (FoodIndex != -1)
                        {
                            print($"{sheet.CharBuffData[FoodIndex].Key} was removed!!!!!!!!!!!!");

                            sheet.CharBuffData.RemoveAt(FoodIndex);
                        }
                    }
                    for (int s = 0; s < sheet.CharBuffData.Count; s++)
                    {
                        if (sheet.CharBuffData[s].Key == BUFF.Key)
                        {
                            BuffIndex = s;
                            break;
                        }
                    }
                    if (BuffIndex != -1)
                    {
                            print($"{sheet.CharBuffData[BuffIndex].Key} was removed!!!!!!!!!!!!");
                        sheet.CharBuffData.RemoveAt(BuffIndex);
                    }
                            print($"{BUFF.Key} was Added!!!!!!!!!!!!");
                    sheet.CharBuffData.Add(BUFF);
                // Add the new CharacterInventoryListItem to the CharInventoryData list

                // Update the character sheet in the InformationSheets
                //InformationSheets[i] = InformationSheets[sheetIndex];
                InformationSheets[i] = sheet;
                RefreshSheets.Invoke( charID);
                if(StatAsset.Instance.CheckForCombatZone(currentScene)){
                    CombatPartyView.instance.CheckMovingObjectIDCheck(charID);
                }
                break;
            }
        }
    }
    
    [Server]

    public void ServerCombatIdentifyEnemyUpdate(int mobID){
        CharacterStatListItem ID = new CharacterStatListItem {
            Key = "IdentifiedTargets"
        };
        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < TacticianInformationSheet.TacticianStatData.Count; i++)
        {
            if (TacticianInformationSheet.TacticianStatData[i].Key == "IdentifiedTargets")
            {
                //get the ids, if ours is in there we dont need to go further
                List<int> ourIDList = StatAsset.Instance.BreakStringIntoNumbers(TacticianInformationSheet.TacticianStatData[i].Value);
                if(ourIDList != null){
                    if(ourIDList.Contains(mobID)){
                        print($"Already had");
                        return;
                    } else {
                        string newString = StatAsset.Instance.InsertNumberIntoSortedString(TacticianInformationSheet.TacticianStatData[i].Value, mobID);
                        ID.Value = newString;
                        print($"Old ID value is {TacticianInformationSheet.TacticianStatData[i].Value}");
                        print($"New ID value is {ID.Value}");
                    }
                } else {
                    string newString = StatAsset.Instance.InsertNumberIntoSortedString(TacticianInformationSheet.TacticianStatData[i].Value, mobID);
                    ID.Value = newString;
                    print($"Old ID value is {TacticianInformationSheet.TacticianStatData[i].Value}");
                    print($"New ID value is {ID.Value}");
                }
                
                TacticianInformationSheet.TacticianStatData.RemoveAt(i);
                TacticianInformationSheet.TacticianStatData.Add(ID);
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                // Update the character sheet in the InformationSheets
                TargetCombatIdentifyEnemyUpdate(mobID);
                break;
            }
        }
    }
    [TargetRpc]

    public void TargetCombatIdentifyEnemyUpdate(int mobID){
        CharacterStatListItem ID = new CharacterStatListItem {
            Key = "IdentifiedTargets"
        };
        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < TacticianInformationSheet.TacticianStatData.Count; i++)
        {
            if (TacticianInformationSheet.TacticianStatData[i].Key == "IdentifiedTargets")
            {
                //get the ids, if ours is in there we dont need to go further
                List<int> ourIDList = StatAsset.Instance.BreakStringIntoNumbers(TacticianInformationSheet.TacticianStatData[i].Value);
                if(ourIDList != null){
                    if(ourIDList.Contains(mobID)){
                        print($"Already had");
                        return;
                    } else {
                        string newString = StatAsset.Instance.InsertNumberIntoSortedString(TacticianInformationSheet.TacticianStatData[i].Value, mobID);
                        ID.Value = newString;
                        print($"Old ID value is {TacticianInformationSheet.TacticianStatData[i].Value}");
                        print($"New ID value is {ID.Value}");
                    }
                    
                } else {
                    string newString = StatAsset.Instance.InsertNumberIntoSortedString(TacticianInformationSheet.TacticianStatData[i].Value, mobID);
                    ID.Value = newString;
                    print($"Old ID value is {TacticianInformationSheet.TacticianStatData[i].Value}");
                    print($"New ID value is {ID.Value}");
                }
                TacticianInformationSheet.TacticianStatData.RemoveAt(i);
                TacticianInformationSheet.TacticianStatData.Add(ID);
                ResetCompendium.Invoke();
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                // Update the character sheet in the InformationSheets
                break;
            }
        }
    }
    [Server]

    public void ServerCombatHPUpdate(string charID, CharacterStatListItem HP){
        int sheetIndex = -1;
        int HPIndex = -1;
        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                {
                    if (InformationSheets[i].CharStatData[s].Key == HP.Key)
                    {
                        HPIndex = s;
                        break;
                    }
                }
                if (HPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(HPIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(HP);

                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                TargetCombatHPUpdate(charID, HP);
                break;
            }
        }
    }
    [TargetRpc]

    public void TargetCombatHPUpdate(string charID, CharacterStatListItem HP){
        int sheetIndex = -1;
        int HPIndex = -1;
        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                {
                    if (InformationSheets[i].CharStatData[s].Key == HP.Key)
                    {
                        HPIndex = s;
                        break;
                    }
                }
                if (HPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(HPIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(HP);
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                RefreshSheets.Invoke( charID);
                break;
            }
        }
    }
    [Server]
    public void GetCharacterUpdateHPMP(string charID, CharacterStatListItem HP, CharacterStatListItem MP){
        int sheetIndex = -1;
        int HPIndex = -1;
        int MPIndex = -1;

        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                {
                    if (InformationSheets[i].CharStatData[s].Key == HP.Key)
                    {
                        HPIndex = s;
                        break;
                    }
                }
                if (HPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(HPIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(HP);

                for (int X = 0; X < InformationSheets[i].CharStatData.Count; X++)
                {
                    if (InformationSheets[i].CharStatData[X].Key == MP.Key)
                    {
                        MPIndex = X;
                        break;
                    }
                }
                if (MPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(MPIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(MP);
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                TargetCharacterUpdateHPMP(charID, HP, MP);
                break;
            }
        }
    }
    [TargetRpc]

    public void TargetCharacterUpdateHPMP(string charID, CharacterStatListItem HP, CharacterStatListItem MP){
        int sheetIndex = -1;
        int HPIndex = -1;
        int MPIndex = -1;

        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                {
                    if (InformationSheets[i].CharStatData[s].Key == HP.Key)
                    {
                        HPIndex = s;
                        break;
                    }
                }
                if (HPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(HPIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(HP);

                for (int X = 0; X < InformationSheets[i].CharStatData.Count; X++)
                {
                    if (InformationSheets[i].CharStatData[X].Key == MP.Key)
                    {
                        MPIndex = X;
                        break;
                    }
                }
                if (MPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(MPIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(MP);
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                RefreshSheets.Invoke( charID);
                break;
            }
        }
    }
    [Server]

    public void GetCharacterUpdateEXPLVL(string charID, CharacterStatListItem LVL, CharacterStatListItem exp){
        int sheetIndex = -1;
        int LVLIndex = -1;
        int expIndex = -1;

        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                {
                    if (InformationSheets[i].CharStatData[s].Key == LVL.Key)
                    {
                        LVLIndex = s;
                        break;
                    }
                }
                if (LVLIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(LVLIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(LVL);

                for (int X = 0; X < InformationSheets[i].CharStatData.Count; X++)
                {
                    if (InformationSheets[i].CharStatData[X].Key == exp.Key)
                    {
                        expIndex = X;
                        break;
                    }
                }
                if (expIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(expIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(exp);
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                TargetCharacterUpdateEXPLVL(charID, LVL, exp);
                break;
            }
        }
    }
    [TargetRpc]

    public void TargetCharacterUpdateEXPLVL(string charID, CharacterStatListItem LVL, CharacterStatListItem exp){
        int sheetIndex = -1;
        int LVLIndex = -1;
        int expIndex = -1;

        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                {
                    if (InformationSheets[i].CharStatData[s].Key == LVL.Key)
                    {
                        LVLIndex = s;
                        break;
                    }
                }
                if (LVLIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(LVLIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(LVL);

                for (int X = 0; X < InformationSheets[i].CharStatData.Count; X++)
                {
                    if (InformationSheets[i].CharStatData[X].Key == exp.Key)
                    {
                        expIndex = X;
                        break;
                    }
                }
                if (expIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(expIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(exp);
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                RefreshSheets.Invoke( charID);
                break;
            }
        }
    }
    [Server]
    public void GetCharacterUpdateLVLINGEXP(string charID, CharacterStatListItem LVLING, CharacterStatListItem EXP){
        int sheetIndex = -1;
        int lvlIndex = -1;
        int expIndex = -1;

        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                {
                    if (InformationSheets[i].CharStatData[s].Key == LVLING.Key)
                    {
                        lvlIndex = s;
                        break;
                    }
                }
                if (lvlIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(lvlIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(LVLING);

                for (int X = 0; X < InformationSheets[i].CharStatData.Count; X++)
                {
                    if (InformationSheets[i].CharStatData[X].Key == EXP.Key)
                    {
                        expIndex = X;
                        break;
                    }
                }
                if (expIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(expIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(EXP);
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                TargetCharacterUpdateLVLEXP(charID, LVLING, EXP);
                break;
            }
        }
    }
    [TargetRpc]
    public void TargetCharacterUpdateLVLEXP(string charID, CharacterStatListItem LVLING, CharacterStatListItem EXP){
        int sheetIndex = -1;
        int lvlIndex = -1;
        int expIndex = -1;
        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                {
                    if (InformationSheets[i].CharStatData[s].Key == LVLING.Key)
                    {
                        lvlIndex = s;
                        break;
                    }
                }
                if (lvlIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(lvlIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(LVLING);

                for (int X = 0; X < InformationSheets[i].CharStatData.Count; X++)
                {
                    if (InformationSheets[i].CharStatData[X].Key == EXP.Key)
                    {
                        expIndex = X;
                        break;
                    }
                }
                if (expIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(expIndex);
                }

                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(EXP);
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                RefreshSheets.Invoke( charID);
                LevelUpStartedSound.Invoke();
                //Add the event here to play music for level up start
                break;
            }
        }
    }
    [Server]
    public void GetCharacterUpdateLVL(string charID, CharacterStatListItem LVL, string LVLING){
        int sheetIndex = -1;
        int LVLIndex = -1;
        int LevelingIndex = -1;

        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                {
                    if (InformationSheets[i].CharStatData[s].Key == LVL.Key)
                    {
                        LVLIndex = s;
                        break;
                    }
                }
                if (LVLIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(LVLIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(LVL);

                for (int X = 0; X < InformationSheets[i].CharStatData.Count; X++)
                {
                    if (InformationSheets[i].CharStatData[X].Key == LVLING)
                    {
                        LevelingIndex = X;
                        break;
                    }
                }
                if (LevelingIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(LevelingIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                TargetGetCharacterUpdateLVL(charID, LVL, LVLING);
                break;
            }
        }
    }
    [TargetRpc]
    public void TargetGetCharacterUpdateLVL(string charID, CharacterStatListItem LVL, string LVLING){
        int sheetIndex = -1;
        int LVLIndex = -1;
        int LevelingIndex = -1;

        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                {
                    if (InformationSheets[i].CharStatData[s].Key == LVL.Key)
                    {
                        LVLIndex = s;
                        break;
                    }
                }
                if (LVLIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(LVLIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(LVL);

                for (int X = 0; X < InformationSheets[i].CharStatData.Count; X++)
                {
                    if (InformationSheets[i].CharStatData[X].Key == LVLING)
                    {
                        LevelingIndex = X;
                        break;
                    }
                }
                if (LevelingIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(LevelingIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                RefreshSheets.Invoke( charID);
                LevelUpEndedSound.Invoke();
                PingUpdate.Invoke("Army");

                //check char 
                foreach(var sheet in InformationSheets){
                    if(sheet.CharacterID == charID)
                    {
                        int _level = int.Parse(LVL.Value);
                        string _CORE = "";
                        float exp = 0f;
                        foreach(var stat in sheet.CharStatData)
                        {
                            switch(stat.Key)
                            {
                                case "CORE":
                                    _CORE = stat.Value;
                                    break;
                                case "EXP":
                                    exp = float.Parse(stat.Value);
                                    break;
                            }
                        }
                        (int ExpCost, int EnergyCost, float TimeCost, int GoldCost) = GetCharacterLevelUp(_level, _CORE);
                        if(ExpCost <= (int)exp){
                            print($"Char had enough exp for next level up telling ancient to build one!!!***************");
                            CheckLevelUpPrefabs.Invoke(this, charID);
                        }
                        break;
                    }
                }
                //Add the event here to play music for level up start
                break;
            }
        }
    }
    [Server]
    public void GetINNServer(string charID, CharacterStatListItem HP, CharacterStatListItem MP){
        int sheetIndex = -1;
        int HPIndex = -1;
        int MPIndex = -1;

        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                {
                    if (InformationSheets[i].CharStatData[s].Key == HP.Key)
                    {
                        HPIndex = s;
                        break;
                    }
                }
                if (HPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(HPIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(HP);

                for (int X = 0; X < InformationSheets[i].CharStatData.Count; X++)
                {
                    if (InformationSheets[i].CharStatData[X].Key == MP.Key)
                    {
                        MPIndex = X;
                        break;
                    }
                }
                if (MPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(MPIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(MP);
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                TargetGetINNClient(charID, HP, MP);
                break;
            }
        }
    }
    [TargetRpc]

    public void TargetGetINNClient(string charID, CharacterStatListItem HP, CharacterStatListItem MP){
        int sheetIndex = -1;
        int HPIndex = -1;
        int MPIndex = -1;
        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                {
                    if (InformationSheets[i].CharStatData[s].Key == HP.Key)
                    {
                        HPIndex = s;
                        break;
                    }
                }
                if (HPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(HPIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(HP);

                for (int X = 0; X < InformationSheets[i].CharStatData.Count; X++)
                {
                    if (InformationSheets[i].CharStatData[X].Key == MP.Key)
                    {
                        MPIndex = X;
                        break;
                    }
                }
                if (MPIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(MPIndex);
                }

                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(MP);
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                RefreshSheets.Invoke( charID);
                break;
            }
        }
    }
    /*
    [Server]
    public void GetTacticianEXP(CharacterStatListItem exp){//get exp and level and possibly promote level if over exp cap
        int expIndex = -1;

        // Find the character sheet with the matching CharacterID
        
        for (int X = 0; X < TacticianInformationSheet.TacticianStatData.Count; X++)
        {
            if (TacticianInformationSheet.TacticianStatData[X].Key == exp.Key)
            {
                expIndex = X;
                break;
            }
        }
        if (expIndex != -1)
        {
            TacticianInformationSheet.TacticianStatData.RemoveAt(expIndex);
        }
        // Add the new CharacterInventoryListItem to the CharInventoryData list
        TacticianInformationSheet.TacticianStatData.Add(exp);
        // Update the character sheet in the InformationSheets
        TargetGetTacticianEXP(exp);
    }
    [TargetRpc]
    public void TargetGetTacticianEXP(CharacterStatListItem exp){
        int expIndex = -1;

        // Find the character sheet with the matching CharacterID
        
        for (int X = 0; X < TacticianInformationSheet.TacticianStatData.Count; X++)
        {
            if (TacticianInformationSheet.TacticianStatData[X].Key == exp.Key)
            {
                expIndex = X;
                break;
            }
        }
        if (expIndex != -1)
        {
            TacticianInformationSheet.TacticianStatData.RemoveAt(expIndex);
        }
        // Add the new CharacterInventoryListItem to the CharInventoryData list
        TacticianInformationSheet.TacticianStatData.Add(exp);
        RefreshChain.Invoke();
        //Speaker.Invoke(253);
        // Update the character sheet in the InformationSheets
    }
    */
    [Server]
    public void GetCharacterUpdateEXP(string charID, CharacterStatListItem exp){
        int sheetIndex = -1;
        int expIndex = -1;

        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                for (int X = 0; X < InformationSheets[i].CharStatData.Count; X++)
                {
                    if (InformationSheets[i].CharStatData[X].Key == exp.Key)
                    {
                        expIndex = X;
                        break;
                    }
                }
                if (expIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(expIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(exp);
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                TargetGetCharacterUpdateEXP(charID, exp);
                break;
            }
        }
    }
    [TargetRpc]
    public void TargetGetCharacterUpdateEXP(string charID, CharacterStatListItem exp){
        int sheetIndex = -1;
        int expIndex = -1;

        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                sheetIndex = i;
                for (int X = 0; X < InformationSheets[i].CharStatData.Count; X++)
                {
                    if (InformationSheets[i].CharStatData[X].Key == exp.Key)
                    {
                        expIndex = X;
                        break;
                    }
                }
                if (expIndex != -1)
                {
                    InformationSheets[i].CharStatData.RemoveAt(expIndex);
                }
                // Add the new CharacterInventoryListItem to the CharInventoryData list
                InformationSheets[i].CharStatData.Add(exp);
                // Update the character sheet in the InformationSheets
                InformationSheets[i] = InformationSheets[sheetIndex];
                RefreshSheets.Invoke( charID);
                RefreshEXP.Invoke(this, charID);
                break;
            }
        }
    }
    /*
    public void UpdateCharacterSkill(int newSkillLevel, float newSkillExp, string skillName, string charId){
        CharacterFullDataMessage SheetModifying = new CharacterFullDataMessage();
        for (int i = 0; i < InformationSheets.Count; i++) {
            if (InformationSheets[i].CharacterID == charId) {
                SheetModifying = InformationSheets[i];
                break;
            }
        }
        if(InformationSheets.Contains(SheetModifying)){
            InformationSheets.Remove(SheetModifying);
        }
        if(skillName == "miningSkill"){
            SheetModifying.miningSkill = newSkillLevel;
            SheetModifying.miningExp = newSkillExp;
        }
        if(skillName == "prospectingSkill"){
            SheetModifying.prospectingSkill = newSkillLevel;
            SheetModifying.prospectingExp = newSkillExp;
        }
        if(skillName == "woodCuttingSkill"){
            SheetModifying.woodCuttingSkill = newSkillLevel;
            SheetModifying.woodCuttingExp = newSkillExp;
        }
        if(skillName == "foragingSkill"){
            SheetModifying.foragingSkill = newSkillLevel;
            SheetModifying.foragingExp = newSkillExp;
        }
        if(skillName == "skinningSkill"){
            SheetModifying.skinningSkill = newSkillLevel;
            SheetModifying.skinningExp = newSkillExp;
        }
        InformationSheets.Add(SheetModifying);
        TargetUpdateCharacterSkill(newSkillLevel, newSkillExp, skillName, charId);
    }








    if(type == "Dragon"){
                    //try to parse the repstat.Value to see if its >0, if its greater than zero, subtract 5 from this and then set GiantRepStat.Value to be the new value so we can add that back in later, if its not parseable or its less than 0 just make it 0 string thanks.
                    int currentRep = 0;

                    // Try to parse the current reputation value
                    if (int.TryParse(repstat.Value, out currentRep)) {
                        // Only modify the reputation if it's greater than zero
                        if (currentRep > 0) {
                            // Calculate the new reputation, subtract 5
                            int newRep = currentRep - 5;
                            // Ensure the new reputation is not less than zero
                            newRep = Math.Max(0, newRep);

                            // Set the new reputation value
                            DragonRepStat.Value = newRep.ToString();
                        } else {
                            // If current reputation is zero or negative, set to zero
                            DragonRepStat.Value = "0";
                        }
                    } else {
                        // If parsing fails, log an error or set the reputation to zero
                        Debug.LogError("Failed to parse GiantRep value: " + repstat.Value);
                    }                
                } else {
                    if(string.IsNullOrEmpty(DragonRepStat.Value)){
                        DragonRepStat.Value = repstat.Value;
                    }
                }





                if(type == "Lizard"){
                    //try to parse the repstat.Value to see if its >0, if its greater than zero, subtract 5 from this and then set GiantRepStat.Value to be the new value so we can add that back in later, if its not parseable or its less than 0 just make it 0 string thanks.
                    int currentRep = 0;

                    // Try to parse the current reputation value
                    if (int.TryParse(repstat.Value, out currentRep)) {
                        // Only modify the reputation if it's greater than zero
                        if (currentRep > 0) {
                            // Calculate the new reputation, subtract 5
                            int newRep = currentRep - 5;
                            // Ensure the new reputation is not less than zero
                            newRep = Math.Max(0, newRep);

                            // Set the new reputation value
                            LizardRepStat.Value = newRep.ToString();
                        } else {
                            // If current reputation is zero or negative, set to zero
                            LizardRepStat.Value = "0";
                        }
                    } else {
                        // If parsing fails, log an error or set the reputation to zero
                        Debug.LogError("Failed to parse LizardRepStat value: " + repstat.Value);
                    }

                    int currentRepOrc = 0;

                    // Try to parse the current reputation value for Orc
                    if (int.TryParse(repstat.Value, out currentRepOrc)) {
                        // Check if the reputation is below the maximum cap of 9999
                        if (currentRepOrc < 9999) {
                            // Increment the reputation since it is below the cap
                            int newRep = currentRepOrc + 1;
                            // Ensure that adding does not exceed the maximum allowed reputation
                            newRep = Math.Min(newRep, 9999);
                            // Set the new reputation value
                            OrcRepStat.Value = newRep.ToString();
                        } else {
                            // If the reputation is already 9999 or above, set it to 9999
                            OrcRepStat.Value = "9999";
                        }
                    } else {
                        // If parsing fails, log an error and set the reputation to zero as a fallback
                        Debug.LogError("Failed to parse OrcRep value: " + repstat.Value);
                    }   
                    int currentRepDwarf = 0;

                    // Try to parse the current reputation value for Dwarf
                    if (int.TryParse(repstat.Value, out currentRepDwarf)) {
                        // Check if the reputation is below the maximum cap of 9999
                        if (currentRepDwarf < 9999) {
                            // Increment the reputation since it is below the cap
                            int newRep = currentRepDwarf + 1;
                            // Ensure that adding does not exceed the maximum allowed reputation
                            newRep = Math.Min(newRep, 9999);
                            // Set the new reputation value
                            DwarfRepStat.Value = newRep.ToString();
                        } else {
                            // If the reputation is already 9999 or above, set it to 9999
                            DwarfRepStat.Value = "9999";
                        }
                    } else {
                        // If parsing fails, log an error and set the reputation to zero as a fallback
                        Debug.LogError("Failed to parse DwarfRepStat value: " + repstat.Value);
                    }
                    
                    int currentRepGnome = 0;

                    // Try to parse the current reputation value for Gnome
                    if (int.TryParse(repstat.Value, out currentRepGnome)) {
                        // Check if the reputation is below the maximum cap of 9999
                        if (currentRepGnome < 9999) {
                            // Increment the reputation since it is below the cap
                            int newRep = currentRepGnome + 1;
                            // Ensure that adding does not exceed the maximum allowed reputation
                            newRep = Math.Min(newRep, 9999);
                            // Set the new reputation value
                            GnomeRepStat.Value = newRep.ToString();
                        } else {
                            // If the reputation is already 9999 or above, set it to 9999
                            GnomeRepStat.Value = "9999";
                        }
                    } else {
                        // If parsing fails, log an error and set the reputation to zero as a fallback
                        Debug.LogError("Failed to parse GnomeRepStat value: " + repstat.Value);
                    }                  
                } else {
                    LizardRepStat.Value = repstat.Value;
                }



                if(type == "Orc"){
                    int currentRep = 0;

                    // Try to parse the current reputation value
                    if (int.TryParse(repstat.Value, out currentRep)) {
                        // Only modify the reputation if it's greater than zero
                        if (currentRep > 0) {
                            // Calculate the new reputation, subtract 5
                            int newRep = currentRep - 5;
                            // Ensure the new reputation is not less than zero
                            newRep = Math.Max(0, newRep);

                            // Set the new reputation value
                            OrcRepStat.Value = newRep.ToString();
                        } else {
                            // If current reputation is zero or negative, set to zero
                            OrcRepStat.Value = "0";
                        }
                    } else {
                        // If parsing fails, log an error or set the reputation to zero
                        Debug.LogError("Failed to parse OrcRepStat value: " + repstat.Value);
                    }
                    int currentRepElf = 0;

                    // Try to parse the current reputation value for Elf
                    if (int.TryParse(repstat.Value, out currentRepElf)) {
                        // Check if the reputation is below the maximum cap of 9999
                        if (currentRepElf < 9999) {
                            // Increment the reputation since it is below the cap
                            int newRep = currentRepElf + 1;
                            // Ensure that adding does not exceed the maximum allowed reputation
                            newRep = Math.Min(newRep, 9999);
                            // Set the new reputation value
                            ElfRepStat.Value = newRep.ToString();
                        } else {
                            // If the reputation is already 9999 or above, set it to 9999
                            ElfRepStat.Value = "9999";
                        }
                    } else {
                        // If parsing fails, log an error and set the reputation to zero as a fallback
                        Debug.LogError("Failed to parse ElfRepStat value: " + repstat.Value);
                    }
                    int currentRepLizard = 0;

                    // Try to parse the current reputation value for Lizard
                    if (int.TryParse(repstat.Value, out currentRepLizard)) {
                        // Check if the reputation is below the maximum cap of 9999
                        if (currentRepLizard < 9999) {
                            // Increment the reputation since it is below the cap
                            int newRep = currentRepLizard + 1;
                            // Ensure that adding does not exceed the maximum allowed reputation
                            newRep = Math.Min(newRep, 9999);
                            // Set the new reputation value
                            LizardRepStat.Value = newRep.ToString();
                        } else {
                            // If the reputation is already 9999 or above, set it to 9999
                            LizardRepStat.Value = "9999";
                        }
                    } else {
                        // If parsing fails, log an error and set the reputation to zero as a fallback
                        Debug.LogError("Failed to parse LizardRepStat value: " + repstat.Value);
                    }
                    int currentRepFaerie = 0;

                    // Try to parse the current reputation value for Faerie
                    if (int.TryParse(repstat.Value, out currentRepFaerie)) {
                        // Check if the reputation is below the maximum cap of 9999
                        if (currentRepFaerie < 9999) {
                            // Increment the reputation since it is below the cap
                            int newRep = currentRepFaerie + 1;
                            // Ensure that adding does not exceed the maximum allowed reputation
                            newRep = Math.Min(newRep, 9999);
                            // Set the new reputation value
                            FaerieStat.Value = newRep.ToString();
                        } else {
                            // If the reputation is already 9999 or above, set it to 9999
                            FaerieStat.Value = "9999";
                        }
                    } else {
                        // If parsing fails, log an error and set the reputation to zero as a fallback
                        Debug.LogError("Failed to parse FaerieRepStat value: " + repstat.Value);
                    }
                } else {
                    OrcRepStat.Value = repstat.Value;
                }

    */
    [Server]
    
    public void ServerUpdateReputation(int tier, string type){
        CharacterStatListItem GiantRepStat = (new CharacterStatListItem { Key = "GiantRep"});
        CharacterStatListItem DragonRepStat = (new CharacterStatListItem { Key = "DragonRep"});
        CharacterStatListItem LizardRepStat = (new CharacterStatListItem { Key = "LizardRep"});
        CharacterStatListItem OrcRepStat = (new CharacterStatListItem { Key = "OrcRep"});
        CharacterStatListItem FaerieStat = (new CharacterStatListItem { Key = "FaerieRep"});
        CharacterStatListItem ElfRepStat = (new CharacterStatListItem { Key = "ElfRep"});
        CharacterStatListItem DwarfRepStat = (new CharacterStatListItem { Key = "DwarfRep"});
        CharacterStatListItem GnomeRepStat = (new CharacterStatListItem { Key = "GnomeRep"});

        foreach(var repstat in TacticianInformationSheet.TacticianStatData){
            if(repstat.Key == "GiantRep"){
                GiantRepStat.Value = repstat.Value;
            }
            if(repstat.Key == "DragonRep"){
                DragonRepStat.Value = repstat.Value;
            }
            if(repstat.Key == "LizardRep"){
                LizardRepStat.Value = repstat.Value;
            }
            if(repstat.Key == "OrcRep"){
                OrcRepStat.Value = repstat.Value;
            }
            if(repstat.Key == "FaerieRep"){
                FaerieStat.Value = repstat.Value;
            }
            if(repstat.Key == "ElfRep"){
                ElfRepStat.Value = repstat.Value;
            }
            if(repstat.Key == "DwarfRep"){
                DwarfRepStat.Value = repstat.Value;
            }
            if(repstat.Key == "GnomeRep"){
               GnomeRepStat.Value = repstat.Value;
            }
        }
        if(type == "Undead"){
            if(tier < 3){
                // Try to parse the current reputation value for Faerie
                    int currentRepFaerie = 0;
                    if (int.TryParse(FaerieStat.Value, out currentRepFaerie)) {
                        // Check if the reputation is below the maximum cap of 9999
                        if (currentRepFaerie < 9999) {
                            // Increment the reputation since it is below the cap
                            int newRep = currentRepFaerie + 1;
                            // Ensure that adding does not exceed the maximum allowed reputation
                            newRep = Math.Min(newRep, 9999);
                            // Set the new reputation value
                            FaerieStat.Value = newRep.ToString();
                        } else {
                            // If the reputation is already 9999 or above, set it to 9999
                            FaerieStat.Value = "9999";
                        }
                    }
                    int currentRepElf = 0;

                    // Try to parse the current reputation value for Elf
                    if (int.TryParse(ElfRepStat.Value, out currentRepElf)) {
                        // Check if the reputation is below the maximum cap of 9999
                        if (currentRepElf < 9999) {
                            // Increment the reputation since it is below the cap
                            int newRep = currentRepElf + 1;
                            // Ensure that adding does not exceed the maximum allowed reputation
                            newRep = Math.Min(newRep, 9999);
                            // Set the new reputation value
                            ElfRepStat.Value = newRep.ToString();
                        } else {
                            // If the reputation is already 9999 or above, set it to 9999
                            ElfRepStat.Value = "9999";
                        }
                    } 
                    int currentRepDwarf = 0;

                    // Try to parse the current reputation value for Dwarf
                    if (int.TryParse(DwarfRepStat.Value, out currentRepDwarf)) {
                        // Check if the reputation is below the maximum cap of 9999
                        if (currentRepDwarf < 9999) {
                            // Increment the reputation since it is below the cap
                            int newRep = currentRepDwarf + 1;
                            // Ensure that adding does not exceed the maximum allowed reputation
                            newRep = Math.Min(newRep, 9999);
                            // Set the new reputation value
                            DwarfRepStat.Value = newRep.ToString();
                        } else {
                            // If the reputation is already 9999 or above, set it to 9999
                            DwarfRepStat.Value = "9999";
                        }
                    } 
                    int currentRepGnome = 0;

                    // Try to parse the current reputation value for Gnome
                    if (int.TryParse(GnomeRepStat.Value, out currentRepGnome)) {
                        // Check if the reputation is below the maximum cap of 9999
                        if (currentRepGnome < 9999) {
                            // Increment the reputation since it is below the cap
                            int newRep = currentRepGnome + 1;
                            // Ensure that adding does not exceed the maximum allowed reputation
                            newRep = Math.Min(newRep, 9999);
                            // Set the new reputation value
                            GnomeRepStat.Value = newRep.ToString();
                        } else {
                            // If the reputation is already 9999 or above, set it to 9999
                            GnomeRepStat.Value = "9999";
                        }
                    }                
                //text.text = $" <color=#{deathhexColor}>Death</color>\n<color=#{colorYellow}>EXP:</color> <color=#{colorYellow}>{EXP}</color>\n<color=#{colorYellow}>CP:</color> <color=#{colorYellow}>{CP}</color>\n<color=#{colorGreen}>+1 faerie, dwarf, elf and gnome rep</color>";
            } else {
                int currentRepDragon = 0;

                    // Try to parse the current reputation value for Dragon
                    if (int.TryParse(DragonRepStat.Value, out currentRepDragon)) {
                        // Check if the reputation is below the maximum cap of 9999
                        if (currentRepDragon < 9999) {
                            // Increment the reputation since it is below the cap
                            int newRep = currentRepDragon + 1;
                            // Ensure that adding does not exceed the maximum allowed reputation
                            newRep = Math.Min(newRep, 9999);
                            // Set the new reputation value
                            DragonRepStat.Value = newRep.ToString();
                        } else {
                            // If the reputation is already 9999 or above, set it to 9999
                            DragonRepStat.Value = "9999";
                        }
                    } 
                    int currentRepGiant = 0;

                    // Try to parse the current reputation value for Giant
                    if (int.TryParse(GiantRepStat.Value, out currentRepGiant)) {
                        // Check if the reputation is below the maximum cap of 9999
                        if (currentRepGiant < 9999) {
                            // Increment the reputation since it is below the cap
                            int newRep = currentRepGiant + 1;
                            // Ensure that adding does not exceed the maximum allowed reputation
                            newRep = Math.Min(newRep, 9999);
                            // Set the new reputation value
                            GiantRepStat.Value = newRep.ToString();
                        } else {
                            // If the reputation is already 9999 or above, set it to 9999
                            GiantRepStat.Value = "9999";
                        }
                    }  
                
                //text.text = $" <color=#{deathhexColor}>Death</color>\n<color=#{colorYellow}>EXP:</color> <color=#{colorYellow}>{EXP}</color>\n<color=#{colorYellow}>CP:</color> <color=#{colorYellow}>{CP}</color>\n<color=#{colorGreen}>+1 dragon and giant rep:</color>";
            }
        }
        if(type == "Lizard"){
            int currentRep = 0;
            // Try to parse the current reputation value
            if (int.TryParse(LizardRepStat.Value, out currentRep)) {
                // Only modify the reputation if it's greater than zero
                if (currentRep > 0) {
                    // Calculate the new reputation, subtract 5
                    int newRep = currentRep - 5;
                    // Ensure the new reputation is not less than zero
                    newRep = Math.Max(0, newRep);
                    // Set the new reputation value
                    LizardRepStat.Value = newRep.ToString();
                } else {
                    // If current reputation is zero or negative, set to zero
                    LizardRepStat.Value = "0";
                }
            }
            
                    int currentRepOrc = 0;

                    // Try to parse the current reputation value for Orc
                    if (int.TryParse(OrcRepStat.Value, out currentRepOrc)) {
                        // Check if the reputation is below the maximum cap of 9999
                        if (currentRepOrc < 9999) {
                            // Increment the reputation since it is below the cap
                            int newRep = currentRepOrc + 1;
                            // Ensure that adding does not exceed the maximum allowed reputation
                            newRep = Math.Min(newRep, 9999);
                            // Set the new reputation value
                            OrcRepStat.Value = newRep.ToString();
                        } else {
                            // If the reputation is already 9999 or above, set it to 9999
                            OrcRepStat.Value = "9999";
                        }
                    } 
                    int currentRepDwarf = 0;

                    // Try to parse the current reputation value for Dwarf
                    if (int.TryParse(DwarfRepStat.Value, out currentRepDwarf)) {
                        // Check if the reputation is below the maximum cap of 9999
                        if (currentRepDwarf < 9999) {
                            // Increment the reputation since it is below the cap
                            int newRep = currentRepDwarf + 1;
                            // Ensure that adding does not exceed the maximum allowed reputation
                            newRep = Math.Min(newRep, 9999);
                            // Set the new reputation value
                            DwarfRepStat.Value = newRep.ToString();
                        } else {
                            // If the reputation is already 9999 or above, set it to 9999
                            DwarfRepStat.Value = "9999";
                        }
                    } 
                    int currentRepGnome = 0;

                    // Try to parse the current reputation value for Gnome
                    if (int.TryParse(GnomeRepStat.Value, out currentRepGnome)) {
                        // Check if the reputation is below the maximum cap of 9999
                        if (currentRepGnome < 9999) {
                            // Increment the reputation since it is below the cap
                            int newRep = currentRepGnome + 1;
                            // Ensure that adding does not exceed the maximum allowed reputation
                            newRep = Math.Min(newRep, 9999);
                            // Set the new reputation value
                            GnomeRepStat.Value = newRep.ToString();
                        } else {
                            // If the reputation is already 9999 or above, set it to 9999
                            GnomeRepStat.Value = "9999";
                        }
                    } 
            //text.text = $" <color=#{deathhexColor}>Death</color>\n<color=#{colorYellow}>EXP:</color> <color=#{colorYellow}>{EXP}</color>\n<color=#{colorYellow}>CP:</color> <color=#{colorYellow}>{CP}</color>\n<color=#{colorGreen}>+1 orc, dwarf and gnome rep</color>\n<color=#{deathhexColor}>-5 lizard rep</color>";
        }
        if(type == "Giant"){
            int currentRep = 0;
            // Try to parse the current reputation value
            if (int.TryParse(GiantRepStat.Value, out currentRep)) {
                // Only modify the reputation if it's greater than zero
                if (currentRep > 0) {
                    // Calculate the new reputation, subtract 5
                    int newRep = currentRep - 5;
                    // Ensure the new reputation is not less than zero
                    newRep = Math.Max(0, newRep);
                    // Set the new reputation value
                    GiantRepStat.Value = newRep.ToString();
                } else {
                    // If current reputation is zero or negative, set to zero
                    GiantRepStat.Value = "0";
                }
            }
            
            //text.text = $" <color=#{deathhexColor}>Death</color>\n<color=#{colorYellow}>EXP:</color> <color=#{colorYellow}>{EXP}</color>\n<color=#{colorYellow}>CP:</color> <color=#{colorYellow}>{CP}</color>\n<color=#{deathhexColor}>-5 giant rep</color>";
        }
        if(type == "Orc"){
            int currentRep = 0;
            // Try to parse the current reputation value
            if (int.TryParse(OrcRepStat.Value, out currentRep)) {
                // Only modify the reputation if it's greater than zero
                if (currentRep > 0) {
                    // Calculate the new reputation, subtract 5
                    int newRep = currentRep - 5;
                    // Ensure the new reputation is not less than zero
                    newRep = Math.Max(0, newRep);
                    // Set the new reputation value
                    OrcRepStat.Value = newRep.ToString();
                } else {
                    // If current reputation is zero or negative, set to zero
                    OrcRepStat.Value = "0";
                }
            }
             int currentRepLizard = 0;

                    // Try to parse the current reputation value for Lizard
                    if (int.TryParse(LizardRepStat.Value, out currentRepLizard)) {
                        // Check if the reputation is below the maximum cap of 9999
                        if (currentRepLizard < 9999) {
                            // Increment the reputation since it is below the cap
                            int newRep = currentRepLizard + 1;
                            // Ensure that adding does not exceed the maximum allowed reputation
                            newRep = Math.Min(newRep, 9999);
                            // Set the new reputation value
                            LizardRepStat.Value = newRep.ToString();
                        } else {
                            // If the reputation is already 9999 or above, set it to 9999
                            LizardRepStat.Value = "9999";
                        }
                    } 
                    int currentRepFaerie = 0;

                    // Try to parse the current reputation value for Faerie
                    if (int.TryParse(FaerieStat.Value, out currentRepFaerie)) {
                        // Check if the reputation is below the maximum cap of 9999
                        if (currentRepFaerie < 9999) {
                            // Increment the reputation since it is below the cap
                            int newRep = currentRepFaerie + 1;
                            // Ensure that adding does not exceed the maximum allowed reputation
                            newRep = Math.Min(newRep, 9999);
                            // Set the new reputation value
                            FaerieStat.Value = newRep.ToString();
                        } else {
                            // If the reputation is already 9999 or above, set it to 9999
                            FaerieStat.Value = "9999";
                        }
                    } 
                    int currentRepElf = 0;

                    // Try to parse the current reputation value for Elf
                    if (int.TryParse(ElfRepStat.Value, out currentRepElf)) {
                        // Check if the reputation is below the maximum cap of 9999
                        if (currentRepElf < 9999) {
                            // Increment the reputation since it is below the cap
                            int newRep = currentRepElf + 1;
                            // Ensure that adding does not exceed the maximum allowed reputation
                            newRep = Math.Min(newRep, 9999);
                            // Set the new reputation value
                            ElfRepStat.Value = newRep.ToString();
                        } else {
                            // If the reputation is already 9999 or above, set it to 9999
                            ElfRepStat.Value = "9999";
                        }
                    }
            
            //text.text = $" <color=#{deathhexColor}>Death</color>\n<color=#{colorYellow}>EXP:</color> <color=#{colorYellow}>{EXP}</color>\n<color=#{colorYellow}>CP:</color> <color=#{colorYellow}>{CP}</color>\n<color=#{colorGreen}>+1 lizard, faerie and elf rep</color>\n<color=#{deathhexColor}>-5 orc rep</color>";
        }
        if(type == "Faerie"){
            int currentRep = 0;
            // Try to parse the current reputation value
            if (int.TryParse(FaerieStat.Value, out currentRep)) {
                // Only modify the reputation if it's greater than zero
                if (currentRep > 0) {
                    // Calculate the new reputation, subtract 5
                    int newRep = currentRep - 5;
                    // Ensure the new reputation is not less than zero
                    newRep = Math.Max(0, newRep);
                    // Set the new reputation value
                    FaerieStat.Value = newRep.ToString();
                } else {
                    // If current reputation is zero or negative, set to zero
                    FaerieStat.Value = "0";
                }
            }
            int currentRepOrc = 0;
                // Try to parse the current reputation value for Orc
                if (int.TryParse(OrcRepStat.Value, out currentRepOrc)) {
                    // Check if the reputation is below the maximum cap of 9999
                    if (currentRepOrc < 9999) {
                        // Increment the reputation since it is below the cap
                        int newRep = currentRepOrc + 1;
                        // Ensure that adding does not exceed the maximum allowed reputation
                        newRep = Math.Min(newRep, 9999);
                        // Set the new reputation value
                        OrcRepStat.Value = newRep.ToString();
                    } else {
                        // If the reputation is already 9999 or above, set it to 9999
                        OrcRepStat.Value = "9999";
                    }
                } 
            
            //text.text = $" <color=#{deathhexColor}>Death</color>\n<color=#{colorYellow}>EXP:</color> <color=#{colorYellow}>{EXP}</color>\n<color=#{colorYellow}>CP:</color> <color=#{colorYellow}>{CP}</color>\n<color=#{colorGreen}>+1 orc rep</color>\n<color=#{deathhexColor}>-5 faerie rep</color>";
        }
        if(type == "Elf"){
            int currentRep = 0;
            // Try to parse the current reputation value
            if (int.TryParse(ElfRepStat.Value, out currentRep)) {
                // Only modify the reputation if it's greater than zero
                if (currentRep > 0) {
                    // Calculate the new reputation, subtract 5
                    int newRep = currentRep - 5;
                    // Ensure the new reputation is not less than zero
                    newRep = Math.Max(0, newRep);
                    // Set the new reputation value
                    ElfRepStat.Value = newRep.ToString();
                } else {
                    // If current reputation is zero or negative, set to zero
                    ElfRepStat.Value = "0";
                }
            }
            int currentRepOrc = 0;
            // Try to parse the current reputation value for Orc
            if (int.TryParse(OrcRepStat.Value, out currentRepOrc)) {
                // Check if the reputation is below the maximum cap of 9999
                if (currentRepOrc < 9999) {
                    // Increment the reputation since it is below the cap
                    int newRep = currentRepOrc + 1;
                    // Ensure that adding does not exceed the maximum allowed reputation
                    newRep = Math.Min(newRep, 9999);
                    // Set the new reputation value
                    OrcRepStat.Value = newRep.ToString();
                } else {
                    // If the reputation is already 9999 or above, set it to 9999
                    OrcRepStat.Value = "9999";
                }
            } 
            
            //text.text = $" <color=#{deathhexColor}>Death</color>\n<color=#{colorYellow}>EXP:</color> <color=#{colorYellow}>{EXP}</color>\n<color=#{colorYellow}>CP:</color> <color=#{colorYellow}>{CP}</color>\n<color=#{colorGreen}>+1 orc rep</color>\n<color=#{deathhexColor}>-5 elf rep</color>";
        }
        if(type == "Dwarf"){
            int currentRep = 0;
            // Try to parse the current reputation value
            if (int.TryParse(DwarfRepStat.Value, out currentRep)) {
                // Only modify the reputation if it's greater than zero
                if (currentRep > 0) {
                    // Calculate the new reputation, subtract 5
                    int newRep = currentRep - 5;
                    // Ensure the new reputation is not less than zero
                    newRep = Math.Max(0, newRep);
                    // Set the new reputation value
                    DwarfRepStat.Value = newRep.ToString();
                } else {
                    // If current reputation is zero or negative, set to zero
                    DwarfRepStat.Value = "0";
                }
            }
            int currentRepLizard = 0;
                // Try to parse the current reputation value for Lizard
                if (int.TryParse(LizardRepStat.Value, out currentRepLizard)) {
                    // Check if the reputation is below the maximum cap of 9999
                    if (currentRepLizard < 9999) {
                        // Increment the reputation since it is below the cap
                        int newRep = currentRepLizard + 1;
                        // Ensure that adding does not exceed the maximum allowed reputation
                        newRep = Math.Min(newRep, 9999);
                        // Set the new reputation value
                        LizardRepStat.Value = newRep.ToString();
                    } else {
                        // If the reputation is already 9999 or above, set it to 9999
                        LizardRepStat.Value = "9999";
                    }
                } 
            
            //text.text = $" <color=#{deathhexColor}>Death</color>\n<color=#{colorYellow}>EXP:</color> <color=#{colorYellow}>{EXP}</color>\n<color=#{colorYellow}>CP:</color> <color=#{colorYellow}>{CP}</color>\n<color=#{colorGreen}>+1 lizard rep</color>\n<color=#{deathhexColor}>-5 dwarf rep</color>";
        }
        if(type == "Gnome"){
            int currentRep = 0;
            // Try to parse the current reputation value
            if (int.TryParse(GnomeRepStat.Value, out currentRep)) {
                // Only modify the reputation if it's greater than zero
                if (currentRep > 0) {
                    // Calculate the new reputation, subtract 5
                    int newRep = currentRep - 5;
                    // Ensure the new reputation is not less than zero
                    newRep = Math.Max(0, newRep);
                    // Set the new reputation value
                    GnomeRepStat.Value = newRep.ToString();
                } else {
                    // If current reputation is zero or negative, set to zero
                    GnomeRepStat.Value = "0";
                }
            }
            int currentRepLizard = 0;
                // Try to parse the current reputation value for Lizard
                if (int.TryParse(LizardRepStat.Value, out currentRepLizard)) {
                    // Check if the reputation is below the maximum cap of 9999
                    if (currentRepLizard < 9999) {
                        // Increment the reputation since it is below the cap
                        int newRep = currentRepLizard + 1;
                        // Ensure that adding does not exceed the maximum allowed reputation
                        newRep = Math.Min(newRep, 9999);
                        // Set the new reputation value
                        LizardRepStat.Value = newRep.ToString();
                    } else {
                        // If the reputation is already 9999 or above, set it to 9999
                        LizardRepStat.Value = "9999";
                    }
                }
            //text.text = $" <color=#{deathhexColor}>Death</color>\n<color=#{colorYellow}>EXP:</color> <color=#{colorYellow}>{EXP}</color>\n<color=#{colorYellow}>CP:</color> <color=#{colorYellow}>{CP}</color>\n<color=#{colorGreen}>+1 lizard rep</color>\n<color=#{deathhexColor}>-5 gnome rep</color>";
        }
        if(type == "Dragon"){
            int currentRep = 0;
            // Try to parse the current reputation value
            if (int.TryParse(DragonRepStat.Value, out currentRep)) {
                // Only modify the reputation if it's greater than zero
                if (currentRep > 0) {
                    // Calculate the new reputation, subtract 5
                    int newRep = currentRep - 5;
                    // Ensure the new reputation is not less than zero
                    newRep = Math.Max(0, newRep);
                    // Set the new reputation value
                    DragonRepStat.Value = newRep.ToString();
                } else {
                    // If current reputation is zero or negative, set to zero
                    DragonRepStat.Value = "0";
                }
            }
            
            //text.text = $" <color=#{deathhexColor}>Death</color>\n<color=#{colorYellow}>EXP:</color> <color=#{colorYellow}>{EXP}</color>\n<color=#{colorYellow}>CP:</color> <color=#{colorYellow}>{CP}</color>\n<color=#{deathhexColor}>-5 dragon rep</color>";
        }
        Dictionary<string, CharacterStatListItem> repChanges = new Dictionary<string, CharacterStatListItem>();
        repChanges.Add(GiantRepStat.Key, GiantRepStat);
        repChanges.Add(DragonRepStat.Key, DragonRepStat);
        repChanges.Add(LizardRepStat.Key, LizardRepStat);
        repChanges.Add(OrcRepStat.Key, OrcRepStat);
        repChanges.Add(FaerieStat.Key, FaerieStat);
        repChanges.Add(ElfRepStat.Key, ElfRepStat);
        repChanges.Add(DwarfRepStat.Key, DwarfRepStat);
        repChanges.Add(GnomeRepStat.Key, GnomeRepStat);
        
        for(int i = 0; i < TacticianInformationSheet.TacticianStatData.Count; i++){
            var key = TacticianInformationSheet.TacticianStatData[i].Key;
            if (repChanges.ContainsKey(key)) {
                // Remove the entry directly
                TacticianInformationSheet.TacticianStatData.RemoveAt(i);
                Debug.Log("Removed " + key + " from Tactician Stat Data.");
            }
        }
        List<CharacterStatListItem> serverSendList = new List<CharacterStatListItem>();
        foreach(var change in repChanges){
            serverSendList.Add(change.Value);
        }
        foreach(var listItem in serverSendList){
            TacticianInformationSheet.TacticianStatData.Add(listItem);
        }
        TargetUpdateReputation(serverSendList);
        RefreshSheets.Invoke( "Tactician");
    }
    [TargetRpc]
    void TargetUpdateReputation(List<CharacterStatListItem> repChanges){
        Dictionary<string, CharacterStatListItem> repChangesDictionary = new Dictionary<string, CharacterStatListItem>();
        foreach(var change in repChanges){
            repChangesDictionary.Add(change.Key, change);
        }
        for(int i = 0; i < TacticianInformationSheet.TacticianStatData.Count; i++){
            var key = TacticianInformationSheet.TacticianStatData[i].Key;
            if (repChangesDictionary.ContainsKey(key)) {
                // Remove the entry directly
                TacticianInformationSheet.TacticianStatData.RemoveAt(i);
                //Debug.Log("Removed " + key + " from Tactician Stat Data.");
            }
        }
        foreach(var listItem in repChanges){
            TacticianInformationSheet.TacticianStatData.Add(listItem);
        }
    }
    private Queue<RpcExpSpawner> EXPUpdateQueue = new Queue<RpcExpSpawner>();
    public void AddRpcCall(List<string> _charIDs, List<string> _classpoints, List<string> _exp, List<string> _ExpValue, List<string> _CpValue, string _mobName)
    {
        //print("Starting AddRpcCall for this unit");
        //foreach(var charID in _charIDs){
        //    print($"{charID} is one of the chars we are working on for EXP! {_ExpValue} was our exp value to give from {_mobName}");
        //}

        EXPUpdateQueue.Enqueue(new RpcExpSpawner(_charIDs,  _classpoints, _exp, _ExpValue, _CpValue, _mobName));
    }
    public Coroutine EXPCPRoutine;
    public IEnumerator SendRpcQueue()
    {
        print("Starting exp for this unit");
		float interval = 0.5f;
        while (true)
        {
            if (EXPUpdateQueue.Count > 0)
            {
                RpcExpSpawner expSpawner = EXPUpdateQueue.Dequeue();
                TargetCharacterUpdateEXPCP(expSpawner.charIDs, expSpawner.classpoints, expSpawner.exp, expSpawner.ExpValue, expSpawner.CpValue, expSpawner.mobName); // Ensure connectionToClient is valid and references the appropriate client
            }

            yield return new WaitForSeconds(interval);
        }
    }
    [Server]
    public void GetCharacterUpdateEXPCP(List<string> charIDs, List<string> classpoints, List<string> exp, List<string> expAmount, List<string> cpAmount, string mobName){
        for(int m = 0; m < charIDs.Count; m++){
            CharacterStatListItem ClassPointsItem = (new CharacterStatListItem{
                Key = "ClassPoints",
                Value = classpoints[m]
            });
            CharacterStatListItem EXPItem = (new CharacterStatListItem{
                Key = "EXP",
                Value = exp[m]
            });
            int sheetIndex = -1;
            int classPointsIndex = -1;
            int expIndex = -1;
            for (int i = 0; i < InformationSheets.Count; i++)
            {
                if (InformationSheets[i].CharacterID == charIDs[m])
                {
                    sheetIndex = i;
                    for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                    {
                        if (InformationSheets[i].CharStatData[s].Key == ClassPointsItem.Key)
                        {
                            classPointsIndex = s;
                            break;
                        }
                    }
                    if (classPointsIndex != -1)
                    {
                        InformationSheets[i].CharStatData.RemoveAt(classPointsIndex);
                    }
                    // Add the new CharacterInventoryListItem to the CharInventoryData list
                    InformationSheets[i].CharStatData.Add(ClassPointsItem);

                    for (int X = 0; X < InformationSheets[i].CharStatData.Count; X++)
                    {
                        if (InformationSheets[i].CharStatData[X].Key == EXPItem.Key)
                        {
                            expIndex = X;
                            break;
                        }
                    }
                    if (expIndex != -1)
                    {
                        InformationSheets[i].CharStatData.RemoveAt(expIndex);
                    }
                    // Add the new CharacterInventoryListItem to the CharInventoryData list
                    InformationSheets[i].CharStatData.Add(EXPItem);
                    // Update the character sheet in the InformationSheets
                    InformationSheets[i] = InformationSheets[sheetIndex];
                }
            }
        }
        //TargetCharacterUpdateEXPCP(charIDs, classpoints, exp, expAmount, cpAmount, mobName);
        AddRpcCall(charIDs, classpoints, exp, expAmount, cpAmount, mobName);
    }
    [TargetRpc]

    public void TargetCharacterUpdateEXPCP(List<string> charIDs, List<string> classpoints, List<string> exp, List<string> expAmount, List<string> cpAmount, string mobName){
        if(!string.IsNullOrEmpty(mobName)){
            string attacker = string.Empty;
            //string defender = mobName;
			string defender = StatAsset.Instance.GetMobName(StatAsset.Instance.GetEnemyToPrefabMapping(), mobName);
            string content = "attacked";
            string amount = $"";
		    int type = 666; 
		    int element = 5;
            CombatLogEntry entry = new CombatLogEntry(attacker, defender, content, amount, type, element, false);
		    GainEXPCP.Invoke(entry);
        }
        for(int m = 0; m < charIDs.Count; m++){
            CharacterStatListItem ClassPointsItem = (new CharacterStatListItem{
                Key = "ClassPoints",
                Value = classpoints[m]
            });
            CharacterStatListItem EXPItem = (new CharacterStatListItem{
                Key = "EXP",
                Value = exp[m]
            });
           // print($"Giving char {charIDs[m]} exp and cp for values {expAmount[m]}, {cpAmount[m]} from {mobName}");
            int sheetIndex = -1;
            int classPointsIndex = -1;
            int expIndex = -1;
            for (int i = 0; i < InformationSheets.Count; i++)
            {
                if (InformationSheets[i].CharacterID == charIDs[m])
                {
                    sheetIndex = i;
                    for (int s = 0; s < InformationSheets[i].CharStatData.Count; s++)
                    {
                        if (InformationSheets[i].CharStatData[s].Key == ClassPointsItem.Key)
                        {
                            classPointsIndex = s;
                            break;
                        }
                    }
                    if (classPointsIndex != -1)
                    {
                        InformationSheets[i].CharStatData.RemoveAt(classPointsIndex);
                    }
                    // Add the new CharacterInventoryListItem to the CharInventoryData list
                    InformationSheets[i].CharStatData.Add(ClassPointsItem);

                    for (int X = 0; X < InformationSheets[i].CharStatData.Count; X++)
                    {
                        if (InformationSheets[i].CharStatData[X].Key == EXPItem.Key)
                        {
                            expIndex = X;
                            break;
                        }
                    }
                    if (expIndex != -1)
                    {
                        InformationSheets[i].CharStatData.RemoveAt(expIndex);
                    }
                    InformationSheets[i].CharStatData.Add(EXPItem);
                    // Update the character sheet in the InformationSheets
                    InformationSheets[i] = InformationSheets[sheetIndex];
                    RefreshSheets.Invoke( charIDs[m]);
                    RefreshEXP.Invoke(this, charIDs[m]);
                    string attacker = string.Empty;
                    for (int P = 0; P < InformationSheets[i].CharStatData.Count; P++)
                    {
                        if (InformationSheets[i].CharStatData[P].Key == "CharName")
                        {
                            attacker = InformationSheets[i].CharStatData[P].Value;
                            break;
                        }
                    }
                    //string defender = mobName;
					string defender = StatAsset.Instance.GetMobName(StatAsset.Instance.GetEnemyToPrefabMapping(), mobName);
                    string content = "attacked";
                    string amount = $"EXP: {expAmount[m]} and CP: {cpAmount[m]}";
		            int type = 667; 
		            int element = 5;
                    CombatLogEntry entry = new CombatLogEntry(attacker, defender, content, amount, type, element, false);
		            GainEXPCP.Invoke(entry);
                    //Update combat log
                    //print($"This char gained this much exp {expAmount} and cp {cpAmount} from {mobName}");
                }
            }
        }
    }
    public float CheckForLevelUpSkill(int currentLevel)
        {
            const int BaseExp = 1000;
            const int AdditionalExpPerLevel = 200;
            // Calculate the experience required for the next level
            //float expForNextLevel = BaseExp + (currentLevel * AdditionalExpPerLevel);
            float expForNextLevel = BaseExp + ((currentLevel - 1) * AdditionalExpPerLevel);

            // Check if current experience is enough for leveling up
            return expForNextLevel;
        }
        /*
    public int GetExpForLevel(int targetLevel)
{
    const int MaxLevel = 100;
    const int BaseExp = 1000;
    const int AdditionalExpPerLevel = 200;

    // Ensure the target level is within the allowable range
    targetLevel = Mathf.Clamp(targetLevel, 1, MaxLevel);

    // Initialize total experience
    int totalExp = 0;

    // Calculate the total experience required to reach the target level from level 1
    for (int level = 0; level < targetLevel; level++)
    {
        totalExp += BaseExp + (level * AdditionalExpPerLevel);
    }

    return totalExp;
}*/
    [Server]
    public void UpdateCharacterSkill(int newSkillLevel, float newSkillExp, string skillName, string charId){
        CharacterFullDataMessage SheetModifying = new CharacterFullDataMessage();
        for (int i = 0; i < InformationSheets.Count; i++) {
            if (InformationSheets[i].CharacterID == charId) {
                SheetModifying = InformationSheets[i];
                break;
            }
        }
        if(InformationSheets.Contains(SheetModifying)){
            InformationSheets.Remove(SheetModifying);
        }
        if(skillName == "miningSkill"){
            SheetModifying.miningSkill = newSkillLevel;
            SheetModifying.miningExp = newSkillExp;
        }
        if(skillName == "prospectingSkill"){
            SheetModifying.prospectingSkill = newSkillLevel;
            SheetModifying.prospectingExp = newSkillExp;
        }
        if(skillName == "woodCuttingSkill"){
            SheetModifying.woodCuttingSkill = newSkillLevel;
            SheetModifying.woodCuttingExp = newSkillExp;
        }
        if(skillName == "foragingSkill"){
            SheetModifying.foragingSkill = newSkillLevel;
            SheetModifying.foragingExp = newSkillExp;
        }
        if(skillName == "skinningSkill"){
            SheetModifying.skinningSkill = newSkillLevel;
            SheetModifying.skinningExp = newSkillExp;
        }
        InformationSheets.Add(SheetModifying);
        TargetUpdateCharacterSkill(newSkillLevel, newSkillExp, skillName, charId);
    }
    [TargetRpc]
    public void TargetUpdateCharacterSkill(int newSkillLevel, float newSkillExp, string skillName, string charId){
        CharacterFullDataMessage SheetModifying = new CharacterFullDataMessage();
        for (int i = 0; i < InformationSheets.Count; i++) {
            if (InformationSheets[i].CharacterID == charId) {
                SheetModifying = InformationSheets[i];
                break;
            }
        }
        if(InformationSheets.Contains(SheetModifying)){
            InformationSheets.Remove(SheetModifying);
        }
        if(skillName == "miningSkill"){
            SheetModifying.miningSkill = newSkillLevel;
            SheetModifying.miningExp = newSkillExp;
        }
        if(skillName == "prospectingSkill"){
            SheetModifying.prospectingSkill = newSkillLevel;
            SheetModifying.prospectingExp = newSkillExp;
        }
        if(skillName == "woodCuttingSkill"){
            SheetModifying.woodCuttingSkill = newSkillLevel;
            SheetModifying.woodCuttingExp = newSkillExp;
        }
        if(skillName == "foragingSkill"){
            SheetModifying.foragingSkill = newSkillLevel;
            SheetModifying.foragingExp = newSkillExp;
        }
        if(skillName == "skinningSkill"){
            SheetModifying.skinningSkill = newSkillLevel;
            SheetModifying.skinningExp = newSkillExp;
        }
        InformationSheets.Add(SheetModifying);
        RefreshSheets.Invoke( charId);
    }
    [Server]
    public void GetFullCharacterData(CharacterFullDataMessage DATA){
        // If the character's data is already in the InformationSheets dictionary, update the CharStatData field
        CharacterFullDataMessage SheetRemoving = new CharacterFullDataMessage();
        foreach (var sheet in InformationSheets){
            if (sheet.CharacterID == DATA.CharacterID){
                SheetRemoving = sheet;
            }
        }
        if(InformationSheets.Contains(SheetRemoving)){
            InformationSheets.Remove(SheetRemoving);
        }
        InformationSheets.Add(DATA);
        // If the character's data was not already in the InformationSheets dictionary, add it now
        TargetGiveFullCharacterData(DATA);
    }
    [TargetRpc]
    void TargetGiveFullCharacterData(CharacterFullDataMessage DATA){
        ClientUpdateCharacter(DATA);
    }
    void ClientUpdateCharacter(CharacterFullDataMessage DATA){
        CharacterFullDataMessage SheetRemoving = new CharacterFullDataMessage();
        foreach (var sheet in InformationSheets){
            if (sheet.CharacterID == DATA.CharacterID){
                SheetRemoving = sheet;
            }
        }
        if(InformationSheets.Contains(SheetRemoving)){
            InformationSheets.Remove(SheetRemoving);
        }
        InformationSheets.Add(DATA);
        string _Class = string.Empty;
        foreach(var stat in DATA.CharStatData){
            if(stat.Key == "Class"){
                _Class = stat.Value;
            }
        }
        GetCharacters.Invoke();
        //RebuildItems.Invoke(DATA.CharacterID, _Class);
        RefreshSheets.Invoke( DATA.CharacterID);
    }
    [Server]
    public void ServerBuildCharacters(){
        TargetBuildCharacters();
    }
    [TargetRpc]
    void TargetBuildCharacters(){
        GetCharacters.Invoke();
        StartCoroutine(ReorderBuild());
    }
    IEnumerator ReorderBuild(){
        yield return new WaitForSeconds(1f);
        PartySpawned.Invoke();
    }
    [Server]
    public void GetFullCharacterDataNew(CharacterFullDataMessage DATA){
        // If the character's data is already in the InformationSheets dictionary, update the CharStatData field
        CharacterFullDataMessage SheetRemoving = new CharacterFullDataMessage();
        foreach (var sheet in InformationSheets){
            if (sheet.CharacterID == DATA.CharacterID){
                SheetRemoving = sheet;
            }
        }
        if(InformationSheets.Contains(SheetRemoving)){
            InformationSheets.Remove(SheetRemoving);
        }
        InformationSheets.Add(DATA);
        // If the character's data was not already in the InformationSheets dictionary, add it now
        TargetGetFullCharacterDataNew(DATA);
    }
    [TargetRpc]
    void TargetGetFullCharacterDataNew(CharacterFullDataMessage DATA){
        ClientGetFullCharacterDataNew(DATA);
    }
    void ClientGetFullCharacterDataNew(CharacterFullDataMessage DATA){
        CharacterFullDataMessage SheetRemoving = new CharacterFullDataMessage();
        foreach (var sheet in InformationSheets){
            if (sheet.CharacterID == DATA.CharacterID){
                SheetRemoving = sheet;
            }
        }
        if(InformationSheets.Contains(SheetRemoving)){
            InformationSheets.Remove(SheetRemoving);
        }
        InformationSheets.Add(DATA);
        string _Class = string.Empty;
        foreach(var stat in DATA.CharStatData){
            if(stat.Key == "Class"){
                _Class = stat.Value;
            }
        }
        PingUpdate.Invoke("Army");
        GetCharacters.Invoke();
        BuildItems.Invoke(DATA.CharacterID);
    }
    [Server]
    public void ServerDeleteChar(CharacterFullDataMessage DATA){
        // If the character's data is already in the InformationSheets dictionary, update the CharStatData field
        CharacterFullDataMessage SheetRemoving = new CharacterFullDataMessage();
        foreach (var sheet in InformationSheets){
            if (sheet.CharacterID == DATA.CharacterID){
                SheetRemoving = sheet;
            }
        }
        if(InformationSheets.Contains(SheetRemoving)){
            InformationSheets.Remove(SheetRemoving);
        }
        // If the character's data was not already in the InformationSheets dictionary, add it now
        TargetDeleteChar(DATA);
    }
    [TargetRpc]
    void TargetDeleteChar(CharacterFullDataMessage DATA){
        ClientDeleteChar(DATA);
    }
    void ClientDeleteChar(CharacterFullDataMessage DATA){
        CharacterFullDataMessage SheetRemoving = new CharacterFullDataMessage();
        foreach (var sheet in InformationSheets){
            if (sheet.CharacterID == DATA.CharacterID){
                SheetRemoving = sheet;
            }
        }
        if(InformationSheets.Contains(SheetRemoving)){
            InformationSheets.Remove(SheetRemoving);
        }
    }
    [Server]
    public void ServerCooldownRemove(string charID, CharacterCooldownListItem coolie){
        int sheetIndex = -1;
        int coolieIndex = -1;
        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                var sheet = InformationSheets[i];
                sheetIndex = i;
                if (sheet.CharCooldownData == null){
                    print("Nothing to delete so we skipped this part");
                } else {
                    for (int s = 0; s < InformationSheets[i].CharCooldownData.Count; s++)
                    {
                        if (InformationSheets[i].CharCooldownData[s].SpellnameFull == coolie.SpellnameFull)
                        {
                            coolieIndex = s;
                            break;
                        }
                    }
                    if (coolieIndex != -1)
                    {
                        InformationSheets[i].CharCooldownData.RemoveAt(coolieIndex);
                    }
                    // Add the new CharacterInventoryListItem to the CharInventoryData list
                    // Update the character sheet in the InformationSheets
                    InformationSheets[i] = InformationSheets[sheetIndex];
                    print($"{coolie.SpellnameFull} is the spell we are removing from cooldown");
                    TargetCooldownRemove(charID, coolie);
                }
                break;
            }
        }
    }
    [TargetRpc]
    public void TargetCooldownRemove(string charID, CharacterCooldownListItem coolie){
        int sheetIndex = -1;
        int coolieIndex = -1;
        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                var sheet = InformationSheets[i];
                sheetIndex = i;
                if (sheet.CharCooldownData == null){
                    print("Nothing to delete so we skipped this part");
                } else {
                    for (int s = 0; s < InformationSheets[i].CharCooldownData.Count; s++)
                    {
                        if (InformationSheets[i].CharCooldownData[s].SpellnameFull == coolie.SpellnameFull)
                        {
                            coolieIndex = s;
                            break;
                        }
                    }
                    if (coolieIndex != -1)
                    {
                        InformationSheets[i].CharCooldownData.RemoveAt(coolieIndex);
                    }
                    // Add the new CharacterInventoryListItem to the CharInventoryData list
                    // Update the character sheet in the InformationSheets
                    InformationSheets[i] = InformationSheets[sheetIndex];
                    print($"{coolie.SpellnameFull} is the spell we are removing from cooldown");
                    RefreshSheets.Invoke( charID);
                    break;
                }
            }
        }
    }
    [Server]

    public void ServerCooldownSaveTactician(CharacterCooldownListItem coolie){
        if(TacticianInformationSheet.TacticianCooldownData.Contains(coolie)){
            print($"Adding {coolie.SpellnameFull} was removed ServerCooldownSaveTactician process");
            TacticianInformationSheet.TacticianCooldownData.Remove(coolie);
        }
        TacticianInformationSheet.TacticianCooldownData.Add(coolie);
        print($"{coolie.SpellnameFull} is the spell we are putting on cooldown");
        TargetRpcCooldownSaveTactician(coolie);
    }
    [TargetRpc]

    public void TargetRpcCooldownSaveTactician(CharacterCooldownListItem coolie){
        if(TacticianInformationSheet.TacticianCooldownData.Contains(coolie)){
            print($"Adding {coolie.SpellnameFull} was removed TargetRpcCooldownSaveTactician process");
            TacticianInformationSheet.TacticianCooldownData.Remove(coolie);
        }
        TacticianInformationSheet.TacticianCooldownData.Add(coolie);
        print($"{coolie.SpellnameFull} is the spell we are putting on cooldown");
        RefreshSheets.Invoke( "Tactician");
    }
    [Server]
    public void ServerCooldownSave(string charID, CharacterCooldownListItem coolie){
        int sheetIndex = -1;
        int coolieIndex = -1;
        string dateTimeWithZone = DateTime.UtcNow.ToString("o");
				//DateTime initialTime = DateTime.Parse(dateTimeWithZone); // Convert to DateTime object
				//DateTime updatedTime = initialTime.AddSeconds(duration); // Add duration in seconds
				DateTime timeNow = DateTime.Parse(dateTimeWithZone, null, System.Globalization.DateTimeStyles.RoundtripKind);
				DateTime timeCooldown = DateTime.Parse(coolie.Value, null, System.Globalization.DateTimeStyles.RoundtripKind);
				print("CHECKING TIME: " + timeNow + $" FOR OUR COOLDOWN SPELL {coolie.SpellnameFull}, WHICH SHOULD NOW BE " + timeCooldown);
        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                var sheet = InformationSheets[i];
                sheetIndex = i;
                if (sheet.CharCooldownData == null){
                        List<CharacterCooldownListItem> cdList = new List<CharacterCooldownListItem>();
                        sheet.CharCooldownData = cdList; // Assign the newly created list
                        sheet.CharCooldownData.Add(coolie);
                        InformationSheets[i] = sheet;
                    } else {
                    for (int s = 0; s < InformationSheets[i].CharCooldownData.Count; s++)
                    {
                        if (InformationSheets[i].CharCooldownData[s].SpellnameFull == coolie.SpellnameFull)
                        {
                            coolieIndex = s;
                            break;
                        }
                    }
                    if (coolieIndex != -1)
                    {
                        InformationSheets[i].CharCooldownData.RemoveAt(coolieIndex);
                    }
                    // Add the new CharacterInventoryListItem to the CharInventoryData list
                    InformationSheets[i].CharCooldownData.Add(coolie);
                    // Update the character sheet in the InformationSheets
                    InformationSheets[i] = InformationSheets[sheetIndex];
                }
                print($"{coolie.SpellnameFull} is the spell we are putting on cooldown");
                TargetCooldownSave(charID, coolie);
                break;
            }
        }
    }
    [TargetRpc]

    public void TargetCooldownSave(string charID, CharacterCooldownListItem coolie){
        int sheetIndex = -1;
        int coolieIndex = -1;
        // Find the character sheet with the matching CharacterID
        for (int i = 0; i < InformationSheets.Count; i++)
        {
            if (InformationSheets[i].CharacterID == charID)
            {
                var sheet = InformationSheets[i];
                sheetIndex = i;
                if (sheet.CharCooldownData == null){
                    List<CharacterCooldownListItem> cdList = new List<CharacterCooldownListItem>();
                    sheet.CharCooldownData = cdList; // Assign the newly created list
                    sheet.CharCooldownData.Add(coolie);
                    InformationSheets[i] = sheet;
                    print($"{coolie.SpellnameFull} is the spell we are putting on cooldown");
                    RefreshSheets.Invoke( charID);
                    break;
                } else {
                    for (int s = 0; s < InformationSheets[i].CharCooldownData.Count; s++)
                    {
                        if (InformationSheets[i].CharCooldownData[s].SpellnameFull == coolie.SpellnameFull)
                        {
                            coolieIndex = s;
                            break;
                        }
                    }
                    if (coolieIndex != -1)
                    {
                        InformationSheets[i].CharCooldownData.RemoveAt(coolieIndex);
                    }
                    // Add the new CharacterInventoryListItem to the CharInventoryData list
                    InformationSheets[i].CharCooldownData.Add(coolie);
                    // Update the character sheet in the InformationSheets
                    InformationSheets[i] = InformationSheets[sheetIndex];
                  //  print($"{coolie.SpellnameFull} is the spell we are putting on cooldown");
                    RefreshSheets.Invoke( charID);
                    break;
                }
            }
        }
    }
    [TargetRpc]
    public void TargetInnReset(){
        ResetInnBools();
    }
    void ResetInnBools(){
        innReset.Invoke();
    }
    //Items
    [TargetRpc]
    public void TargetItemRoll(string itemName, int amount, string ID){
        BuildingItemDrop.Invoke(itemName, amount, ID);
         //print($"TargetItemRoll on client for {itemName}");
    }
    [TargetRpc]
    public void TargetCloseEndGameCanvas(){
        DeselectedCharacter();
        if(movementCoroutine != null){
            StopCoroutine(movementCoroutine);
        }
        if(spellCoroutine != null){
            StopCoroutine(spellCoroutine);
        }
        ScenePlayer.localPlayer.ClearSelected();
        CombatPartyView.instance.TurnOffCanvas();
        FormationScript.instance.TurnOffCanvas();
        ClosingWindow();
    }
    void ClosingWindow(){
        ToggleCloseEndMatch.Invoke();
    }
    [TargetRpc]
    public void TargetGatherSeekers(PlayerCharacter player){
        //FindSeekers(player);
        StartCoroutine(CastFindSeekers(player));

    }
    IEnumerator CastFindSeekers(PlayerCharacter player){
        yield return new WaitForSeconds(2f);
        //print("Casting find seekrs!");
        BuildCombatPlayerUI.Invoke(player);
    }
    void FindSeekers(PlayerCharacter player){
        BuildCombatPlayerUI.Invoke(player);

        //StartCoroutine(CastFindSeekers(player));
    }
    public void DeletingCharacter(string id){
        CmdDeleteCharacter(id);
    }
    [Command]
    void CmdDeleteCharacter(string id){
        DeleteCharacter.Invoke(this.connectionToClient, id);
    }
    public void BuyTokens(){
        CmdBuyTokens();
    }
    [Command]
    void CmdBuyTokens(){
        PurchaseCharacterToken.Invoke(this.connectionToClient);
    }
    [TargetRpc]
    public void TargetRefreshItems(ItemSelectable item){
        Refreshitem.Invoke(this, item);
    }
    [Server]
    public void GoldAmountSet(long newGold){
        SetGold(newGold);
    }
    [Server]
    public void SetGold(long newGold){
        Gold = newGold;
        TargetWalletAwake();
    }
    [Server]
    public void ServerRefreshWallet(){
        StartCoroutine(ServerRefreshWalletRoutine());
    }
    IEnumerator ServerRefreshWalletRoutine(){
        yield return new WaitForSeconds(1f);
        TargetWalletAwake();
    }
    public long GoldAmount(){
        return Gold;
    }
    
    [TargetRpc]
    public void TargetStaffBuild(ItemSelectable item){
        StaffBuild.Invoke(item);
    }
    //TradingItemsInInventory***************************************************************************************************************
    //Stacks
    void StackItemSend(StackingMessage message){
        CmdStackItemSend(message);
    }
    [Command]
    void CmdStackItemSend(StackingMessage message){
        ServerStackItemSend(message);
    }
    [Server]
    void ServerStackItemSend(StackingMessage message){
        StackingItem.Invoke(this.connectionToClient, message);
    }
    //Stash
    void StashToTactInventory(ItemSelectable item){
        CmdStashToTactInventory(item);
    }
    [Command]
    void CmdStashToTactInventory(ItemSelectable item){
        ServerStashToTactInventory(item);
    }
    [Server]
    void ServerStashToTactInventory(ItemSelectable item){
        string request = "StashToTactInventory";
        StashToTactInv.Invoke(this.connectionToClient, item, request);
    }
    void StashToTactEquipment(ItemSelectable item, string SlotName){
        CmdStashToTactEquip(item, SlotName);
    }
    [Command]
    void CmdStashToTactEquip(ItemSelectable item, string SlotName){
        ServerStashToTactEquip(item, SlotName);
    }
    [Server]
    void ServerStashToTactEquip(ItemSelectable item, string SlotName){
        string request = "StashToTactEquip";
        StashToTactEquipped.Invoke(this.connectionToClient, item, request, SlotName);
    }
    void StashToTactbelt(ItemSelectable item){
        CmdStashToTactbelt(item);
    }
    [Command]
    void CmdStashToTactbelt(ItemSelectable item){
        ServerStashToTactbelt(item);
    }
    [Server]
    void ServerStashToTactbelt(ItemSelectable item){
        string request = "StashToTactBelt";
        StashToTactSafetyBelt.Invoke(this.connectionToClient, item, request);
    }
    void StashToCharInventory(string character, ItemSelectable item){
        CmdStashToCharInventory(character, item);
    }
    [Command]
    void CmdStashToCharInventory(string character, ItemSelectable item){
        ServerStashToCharInventory(character, item);
    }
    [Server]
    void ServerStashToCharInventory(string character, ItemSelectable item){
        string request = "StashToCharInv";
        StashToCharInv.Invoke(this.connectionToClient, item, request, character);
    }
    void StashToCharEquipment(string character, ItemSelectable item, string SlotName){
        CmdStashToCharEquipment(character, item, SlotName);
    }
    [Command]
    void CmdStashToCharEquipment(string character, ItemSelectable item, string SlotName){
        ServerStashToCharEquipment(character, item, SlotName);
    }
    [Server]
    void ServerStashToCharEquipment(string character, ItemSelectable item, string SlotName){
        string request = "StashToCharEquip";
        EquippingData BuiltEquipData = new EquippingData { Slot = SlotName, CharacterSlot = character };
        StashToCharEquip.Invoke(this.connectionToClient, item, request, BuiltEquipData);
    }
    void TacticianInvToStash(ItemSelectable item){
        CmdTacticianInvToStash(item);
    }
    [Command]
    void CmdTacticianInvToStash(ItemSelectable item){
        ServerTacticianInvToStash(item);
    }
    [Server]
    void ServerTacticianInvToStash(ItemSelectable item){
        string request = "TactInventoryToStash";
        TactInvToStash.Invoke(this.connectionToClient, item, request);
    }
    void TacticianInvToTactEquip(ItemSelectable item, string SlotName){
        CmdTacticianInvToTactEquip(item, SlotName);
    }
    [Command]
    void CmdTacticianInvToTactEquip(ItemSelectable item, string SlotName){
        ServerTacticianInvToTactEquip(item, SlotName);
    }
    [Server]
    void ServerTacticianInvToTactEquip(ItemSelectable item, string SlotName){
        string request = "TacticianInvToTacticianEquip";
        TactInvToTactEquip.Invoke(this.connectionToClient, item, request, SlotName);
    }
    void TacticianInvToTactBelt(ItemSelectable item){
        CmdTacticianInvToTactBelt(item);
    }
    [Command]
    void CmdTacticianInvToTactBelt(ItemSelectable item){
        ServerTacticianInvToTactBelt(item);
    }
    [Server]
    void ServerTacticianInvToTactBelt(ItemSelectable item){
        string request = "TactInvToTactBelt";
        TactInvToTactBelt.Invoke(this.connectionToClient, item, request);
    }
    
    void TacticianInvToCharInv(string character, ItemSelectable item){
        CmdTactInvToCharInv(character, item);
    }
    [Command]
    void CmdTactInvToCharInv(string character, ItemSelectable item){
        ServerInvTactToCharInv(character, item);
    }
    [Server]
    void ServerInvTactToCharInv(string character, ItemSelectable item){
        string request = "TactInvCharInv";
        TactInvToCharInv.Invoke(this.connectionToClient, item, request, character);
    }
    void TactInvToCharEquipment(string character, ItemSelectable item, string SlotName){
        CmdTactInvToCharEquipment(character, item, SlotName);
    }
    [Command]
    void CmdTactInvToCharEquipment(string character, ItemSelectable item, string SlotName){
        ServerInvTactToCharEquipment(character, item, SlotName);
    }
    [Server]
    void ServerInvTactToCharEquipment(string character, ItemSelectable item, string SlotName){
        string request = "TactInvToCharEquip";
        EquippingData BuiltEquipData = new EquippingData { Slot = SlotName, CharacterSlot = character };
        TactInvToCharEquip.Invoke(this.connectionToClient, item, request, BuiltEquipData);
    }
    
    void TacticianEquipToStash(ItemSelectable item){
        CmdTacticianEquipToStash(item);
    }
    [Command]
    void CmdTacticianEquipToStash(ItemSelectable item){
        ServerTacticianEquipToStash(item);
    }
    [Server]
    void ServerTacticianEquipToStash(ItemSelectable item){
        string request = "TactEquipToStash";
        TactEquipToStash.Invoke(this.connectionToClient, item, request);
    }
    void TacticianEquipToTactInv(ItemSelectable item){
        CmdTacticianEquipToTactInv(item);
    }
    [Command]
    void CmdTacticianEquipToTactInv(ItemSelectable item){
        ServerTacticianEquipToTactInv(item);
    }
    [Server]
    void ServerTacticianEquipToTactInv(ItemSelectable item){
        string request = "TactEquipToTactInv";
        TactEquipToTactInv.Invoke(this.connectionToClient, item, request);
    }
    void TacticianEquipToTactEquip(ItemSelectable item, string SlotName){
        CmdTacticianEquipToTactEquip(item, SlotName);
    }
    [Command]
    void CmdTacticianEquipToTactEquip(ItemSelectable item, string SlotName){
        ServerTacticianEquipToTactEquip(item, SlotName);
    }
    [Server]
    void ServerTacticianEquipToTactEquip(ItemSelectable item, string SlotName){
        string request = "TacticianInvToTacticianEquip";
        TactEquipToTactEquip.Invoke(this.connectionToClient, item, request, SlotName);
    }
    void TacticianEquipToTactBelt(ItemSelectable item){
        CmdTacticianEquipToTactBelt(item);
    }
    [Command]
    void CmdTacticianEquipToTactBelt(ItemSelectable item){
        ServerTacticianEquipToTactBelt(item);
    }
    [Server]
    void ServerTacticianEquipToTactBelt(ItemSelectable item){
        string request = "TactEquipToTactBelt";
        TactEquipToTactBelt.Invoke(this.connectionToClient, item, request);
    }
    void TacticianEquipToCharInv(string character, ItemSelectable item){
        CmdTacticianEquipToCharInv(character, item);
    }
    [Command]
    void CmdTacticianEquipToCharInv(string character, ItemSelectable item){
        ServerTacticianEquipToCharInv(character, item);
    }
    [Server]
    void ServerTacticianEquipToCharInv(string character, ItemSelectable item){
        string request = "TactEquipCharInv";
        TactEquipToCharInv.Invoke(this.connectionToClient, item, request, character);
    }
    void TacticianBeltToStash(ItemSelectable item){
        CmdTacticianBeltToStash(item);
    }
    [Command]
    void CmdTacticianBeltToStash(ItemSelectable item){
        ServerTacticianBeltToStash(item);
    }
    [Server]
    void ServerTacticianBeltToStash(ItemSelectable item){
        string request = "TactBeltToStash";
        TactBeltToStash.Invoke(this.connectionToClient, item, request);
    }
    void TacticianBeltToTactInv(ItemSelectable item){
        CmdTacticianBeltToTactInv(item);
    }
    [Command]
    void CmdTacticianBeltToTactInv(ItemSelectable item){
        ServerTacticianBeltToTactInv(item);
    }
    [Server]
    void ServerTacticianBeltToTactInv(ItemSelectable item){
        string request = "TactBeltToTactInv";
        TactBeltToTactInv.Invoke(this.connectionToClient, item, request);
    }
    void TacticianBeltToTactEquip(ItemSelectable item, string SlotName){
        CmdTacticianBeltToTactEquip(item, SlotName);
    }
    [Command]
    void CmdTacticianBeltToTactEquip(ItemSelectable item, string SlotName){
        ServerTacticianBeltToTactEquip(item, SlotName);
    }
    [Server]
    void ServerTacticianBeltToTactEquip(ItemSelectable item, string SlotName){
        string request = "TactBeltToTactEquip";
        TactBeltToTactEquip.Invoke(this.connectionToClient, item, request, SlotName);
    }
    void TacticianBeltToCharInv(string character, ItemSelectable item){
        CmdTacticianBeltToCharInv(character, item);
    }
    [Command]
    void CmdTacticianBeltToCharInv(string character, ItemSelectable item){
        ServerTacticianBeltToCharInv(character, item);
    }
    [Server]
    void ServerTacticianBeltToCharInv(string character, ItemSelectable item){
        string request = "TactBeltCharInv";
        TactSafetyBeltToCharInv.Invoke(this.connectionToClient, item, request, character);
    }
    void TacticianBeltToCharEquip(string character, ItemSelectable item, string SlotName){
        CmdTacticianBeltToCharEquip(character, item, SlotName);
    }
    [Command]
    void CmdTacticianBeltToCharEquip(string character, ItemSelectable item, string SlotName){
        ServerTacticianBeltToCharEquip(character, item, SlotName);
    }
    [Server]
    void ServerTacticianBeltToCharEquip(string character, ItemSelectable item, string SlotName){
        string request = "TactBeltToCharEquip";
        EquippingData BuiltEquipData = new EquippingData { Slot = SlotName, CharacterSlot = character };
        TactSafetyBeltToCharEquip.Invoke(this.connectionToClient, item, request, BuiltEquipData);
    }
    void CharacterInvToStash(string character, ItemSelectable item){
        CmdCharacterInvToStash(character, item);
    }
    [Command]
    void CmdCharacterInvToStash(string character, ItemSelectable item){
        ServerCharacterInvToStash(character, item);
    }
    [Server]
    void ServerCharacterInvToStash(string character, ItemSelectable item){
        string request = "CharInvStash";
        CharInvToStash.Invoke(this.connectionToClient, item, request, character);
    }
    void CharacterInvToTactInventory(string character, ItemSelectable item){
        CmdCharacterInvToTactInventory(character, item);
    }
    [Command]
    void CmdCharacterInvToTactInventory(string character, ItemSelectable item){
        ServerCharacterInvToTactInventory(character, item);
    }
    [Server]
    void ServerCharacterInvToTactInventory(string character, ItemSelectable item){
        string request = "CharInvTactInv";
        CharInvToTactInv.Invoke(this.connectionToClient, item, request, character);
    }
    void CharacterInvToTactBelt(string character, ItemSelectable item){
        CmdCharacterInvToTactBelt(character, item);
    }
    [Command]
    void CmdCharacterInvToTactBelt(string character, ItemSelectable item){
        ServerCharacterInvToTactBelt(character, item);
    }
    [Server]
    void ServerCharacterInvToTactBelt(string character, ItemSelectable item){
        string request = "CharInvTactBelt";
        CharInvToTactBelt.Invoke(this.connectionToClient, item, request, character);
    }
    void CharacterInvToTactEquip(string character, ItemSelectable item, string SlotName){
        CmdCharacterInvToTactEquip(character, item, SlotName);
    }
    [Command]
    void CmdCharacterInvToTactEquip(string character, ItemSelectable item, string SlotName){
        ServerCharacterInvToTactEquip(character, item, SlotName);
    }
    [Server]
    void ServerCharacterInvToTactEquip(string character, ItemSelectable item, string SlotName){
        string request = "CharInvTactEquip";
        EquippingData BuiltEquipData = new EquippingData { Slot = SlotName, CharacterSlot = character };
        CharInvToTactEquip.Invoke(this.connectionToClient, item, request, BuiltEquipData);
    }
    
    void CharacterEquipToStash(string character, ItemSelectable item){
        CmdCharacterEquipToStash(character, item);
    }
    [Command]
    void CmdCharacterEquipToStash(string character, ItemSelectable item){
        ServerCharacterEquipToStash(character, item);
    }
    [Server]
    void ServerCharacterEquipToStash(string character, ItemSelectable item){
        string request = "CharEquipStash";
        CharEquipToStash.Invoke(this.connectionToClient, item, request, character);
    }
    void CharacterEquipToTacticianInv(string character, ItemSelectable item){
        CmdCharacterEquipToTacticianInv(character, item);
    }
    [Command]
    void CmdCharacterEquipToTacticianInv(string character, ItemSelectable item){
        ServerCharacterEquipToTacticianInv(character, item);
    }
    [Server]
    void ServerCharacterEquipToTacticianInv(string character, ItemSelectable item){
        string request = "CharEquipTactInv";
        CharEquipToTactInv.Invoke(this.connectionToClient, item, request, character);
    }
    void CharacterEquipToTacticianBelt(string character, ItemSelectable item){
        CmdCharacterEquipToTacticianBelt(character, item);
    }
    [Command]
    void CmdCharacterEquipToTacticianBelt(string character, ItemSelectable item){
        ServerCharacterEquipToTacticianBelt(character, item);
    }
    [Server]
    void ServerCharacterEquipToTacticianBelt(string character, ItemSelectable item){
        string request = "CharEquipTactBelt";
        CharEquipToTactBelt.Invoke(this.connectionToClient, item, request, character);
    }
    void RepairAllItems(){
        CmdRepairAllItems();
    }
    [Command]
    void CmdRepairAllItems(){
        ServerRepairAllItems();
    }
    [Server]
    void ServerRepairAllItems(){
        RepairAllItemsEVENT.Invoke(this.connectionToClient);
    }
    void RepairSingleItem(ItemSelectable itemSelectable){
        print($"Starting RepairSingleItem");

        CmdRepairSingleItem(itemSelectable);
    }
    [Command]
    void CmdRepairSingleItem(ItemSelectable itemSelectable){
        print($"Starting CmdRepairSingleItem");

        ServerRepairSingleItem(itemSelectable);
    }
    [Server]
    void ServerRepairSingleItem(ItemSelectable itemSelectable){
        print($"Starting ServerRepairSingleItem");
        RepairSingleItemEVENT.Invoke(this.connectionToClient, itemSelectable);
    }
    void SalvageRefundItems(SalvageNetworkList salvageRefundList){
        foreach(var salvageItem in salvageRefundList.RemovingTheseItems){
            DestroyInventoryItem.Invoke(salvageItem);
        }
        CmdSalvageRefundItems(salvageRefundList);
    }
    [Command]
    void CmdSalvageRefundItems(SalvageNetworkList salvageRefundList){
        ServerSalvageRefundItems(salvageRefundList);
    }
    [Server]
    void ServerSalvageRefundItems(SalvageNetworkList salvageRefundList){
        SalvageRefundItemList.Invoke(this.connectionToClient, salvageRefundList);
    }
    void DestroyingItem(string character, ItemSelectable item){
        CmdDestroyingItem(character, item);
    }
    [Command]
    void CmdDestroyingItem(string character, ItemSelectable item){
        ServerDestroyingItem(character, item);
    }
    [Server]
    void ServerDestroyingItem(string character, ItemSelectable item){
        ServerDestroyItem.Invoke(this.connectionToClient, item, character);
    }

    void ConsumingItemFully(string character, ItemSelectable item){
        CmdConsumingItemFully(character, item);
    }
    [Command]
    void CmdConsumingItemFully(string character, ItemSelectable item){
        ServerConsumingItemFully(character, item);
    }
    [Server]
    void ServerConsumingItemFully(string character, ItemSelectable item){
        ServerConsumingItemFullyEvent.Invoke(this.connectionToClient, item, character);
    }
    void ConsumingItemPartially(string character, ItemSelectable item){
        CmdConsumingItemPartially(character, item);
    }
    [Command]
    void CmdConsumingItemPartially(string character, ItemSelectable item){
        ServerConsumingItemPartially(character, item);
    }
    [Server]
    void ServerConsumingItemPartially(string character, ItemSelectable item){
        ServerConsumingItemPartiallyEvent.Invoke(this.connectionToClient, item, character);
    }

    void CharacterInvToCharInv(string characterOne, string characterTwo, ItemSelectable item){
        CmdCharacterInvToCharInv(characterOne, characterTwo, item);
    }
    [Command]
    void CmdCharacterInvToCharInv(string characterOne, string characterTwo,  ItemSelectable item){
        ServerCharacterInvToCharInv(characterOne, characterTwo, item);
    }
    [Server]
    void ServerCharacterInvToCharInv(string characterOne, string characterTwo,  ItemSelectable item){
        string request = "CharInvCharInv";
        EquippingData BuiltEquipData = new EquippingData { CharacterSlotOne = characterOne, CharacterSlotTwo = characterTwo};
        CharInvToCharInv.Invoke(this.connectionToClient, item, request, BuiltEquipData);
    }
    void CharacterInvToCharEquip(string characterOne, string characterTwo, ItemSelectable item, string SlotName){
        CmdCharacterInvToCharEquip(characterOne, characterTwo, item, SlotName);
    }
    [Command]
    void CmdCharacterInvToCharEquip(string characterOne, string characterTwo, ItemSelectable item, string SlotName){
        ServerCharacterInvToCharEquip(characterOne, characterTwo, item, SlotName);
    }
    [Server]
    void ServerCharacterInvToCharEquip(string characterOne, string characterTwo, ItemSelectable item, string SlotName){
        string request = "CharInvCharEquip";
        EquippingData BuiltEquipData = new EquippingData { CharacterSlotOne = characterOne, CharacterSlotTwo = characterTwo, Slot = SlotName};
        CharInvToCharEquip.Invoke(this.connectionToClient, item, request, BuiltEquipData);
    }
    void CharacterEquipToCharInv(string characterOne, string characterTwo, ItemSelectable item){
        CmdCharacterEquipToCharInv(characterOne, characterTwo, item);
    }
    [Command]
    void CmdCharacterEquipToCharInv(string characterOne, string characterTwo, ItemSelectable item){
        ServerCharacterEquipToCharInv(characterOne, characterTwo, item);
    }
    [Server]
    void ServerCharacterEquipToCharInv(string characterOne, string characterTwo, ItemSelectable item){
        string request = "CharEquipCharInv";
        EquippingData BuiltEquipData = new EquippingData { CharacterSlotOne = characterOne, CharacterSlotTwo = characterTwo};

        CharEquipToCharInv.Invoke(this.connectionToClient, item, request, BuiltEquipData);
    }
    void CharacterEquipToCharEquip(string characterOne, string characterTwo, ItemSelectable item, string SlotName){
        CmdCharacterEquipToCharEquip(characterOne, characterTwo, item, SlotName);
    }
    [Command]
    void CmdCharacterEquipToCharEquip(string characterOne, string characterTwo, ItemSelectable item, string SlotName){
        ServerCharacterEquipToCharEquip(characterOne, characterTwo, item, SlotName);
    }
    [Server]
    void ServerCharacterEquipToCharEquip(string characterOne, string characterTwo, ItemSelectable item, string SlotName){
        string request = "CharEquipCharEquip";
        EquippingData BuiltEquipData = new EquippingData { Slot = SlotName,  CharacterSlotOne = characterOne, CharacterSlotTwo = characterTwo};
        CharEquipToCharEquip.Invoke(this.connectionToClient, item, request, BuiltEquipData);
    }
    
    void CharacterEquiptoInvSame(string character, ItemSelectable item){
        CmdCharacterEquiptoInvSame(character, item);
    }
    [Command]
    void CmdCharacterEquiptoInvSame(string character, ItemSelectable item){
        ServerCharacterEquiptoInvSame(character, item);
    }
    [Server]
    void ServerCharacterEquiptoInvSame(string character, ItemSelectable item){
        string request = "CharEquipInvSame";
        EquippingData BuiltEquipData = new EquippingData { CharacterSlot = character };
        CharEquipToInvSame.Invoke(this.connectionToClient, item, request, BuiltEquipData);
    }
    void CharacterInvtoEquipSame(string character, ItemSelectable item, string SlotName){
        CmdCharacterInvtoEquipSame(character, item, SlotName);
    }
    [Command]
    void CmdCharacterInvtoEquipSame(string character, ItemSelectable item, string SlotName){
        ServerCharacterInvtoEquipSame(character, item, SlotName);
    }
    [Server]
    void ServerCharacterInvtoEquipSame(string character, ItemSelectable item, string SlotName){
        string request = "CharInvEquipSame";
        EquippingData BuiltEquipData = new EquippingData { Slot = SlotName, CharacterSlot = character };
        CharInvToEquipSame.Invoke(this.connectionToClient, item, request, BuiltEquipData);
    }
    void CharacterEquiptoEquipSame(string character, ItemSelectable item, string SlotName){
        CmdCharacterEquiptoEquipSame(character, item, SlotName);
    }
    [Command]
    void CmdCharacterEquiptoEquipSame(string character, ItemSelectable item, string SlotName){
        ServerCharacterEquiptoEquipSame(character, item, SlotName);
    }
    [Server]
    void ServerCharacterEquiptoEquipSame(string character, ItemSelectable item, string SlotName){
        string request = "CharEquipEquipSame";
        EquippingData BuiltEquipData = new EquippingData { Slot = SlotName, CharacterSlot = character };
        CharEquipToEquipSame.Invoke(this.connectionToClient, item, request, BuiltEquipData);
    }

    //UnequipEquipMethods
    void CharacterOneUnequipToCharEquipSendTwo( ItemSelectable itemOne, ItemSelectable itemTwo, string slot){
        string CharOne = CharacterSheet.Instance.GetSerial();
        string CharTwo = CharacterTwoSheet.Instance.GetSerial();
        CmdCharacterOneUnequipToCharEquipSendTwo(CharOne, slot, itemOne, itemTwo, CharTwo);
    }
    [Command]
    void CmdCharacterOneUnequipToCharEquipSendTwo(string characterOne, string slot, ItemSelectable itemOne, ItemSelectable itemTwo, string characterTwo){
        ServerCharacterOneUnequipToCharEquipSendTwo(characterOne, slot, itemOne, itemTwo, characterTwo);
    }
    [Server]
    void ServerCharacterOneUnequipToCharEquipSendTwo(string characterOne, string slot, ItemSelectable itemOne, ItemSelectable itemTwo, string characterTwo){
        string request = "CharacterOneUnequipToCharEquipSendTwo";
        EquippingData BuiltEquipData = new EquippingData { Request = request, CharacterSlotOne = characterOne, CharacterSlotTwo = characterTwo , Slot = slot};

        CharOneUnequipToCharEquipSendTwo.Invoke(this.connectionToClient, itemOne, itemTwo, BuiltEquipData);
    }
    void CharacterTwoUnequipToCharEquipSendOne(ItemSelectable itemOne, ItemSelectable itemTwo, string slot){
        string CharOne = CharacterSheet.Instance.GetSerial();
        string CharTwo = CharacterTwoSheet.Instance.GetSerial();
        CmdCharacterTwoUnequipToCharEquipSendOne(CharOne, slot, itemOne, itemTwo, CharTwo);
    }
    [Command]
    void CmdCharacterTwoUnequipToCharEquipSendOne(string characterOne, string slot, ItemSelectable itemOne, ItemSelectable itemTwo, string characterTwo){
        ServerCharacterTwoUnequipToCharEquipSendOne(characterOne, slot, itemOne, itemTwo, characterTwo);
    }
    [Server]
    void ServerCharacterTwoUnequipToCharEquipSendOne(string characterOne, string slot, ItemSelectable itemOne, ItemSelectable itemTwo, string characterTwo){
        string request = "CharacterTwoUnequipToCharEquipSendOne";
        EquippingData BuiltEquipData = new EquippingData { Request = request, CharacterSlotOne = characterOne, CharacterSlotTwo = characterTwo , Slot = slot};

        CharTwoUnequipToCharEquipSendOne.Invoke(this.connectionToClient, itemOne, itemTwo, BuiltEquipData);
    }
    void CharacterOneUnequipToCharEquip(string character, ItemSelectable itemOne, ItemSelectable itemTwo, string slot){
        CmdCharacterOneUnequipToCharEquip(character, slot, itemOne, itemTwo);
    }
    [Command]
    void CmdCharacterOneUnequipToCharEquip(string character, string slot, ItemSelectable itemOne, ItemSelectable itemTwo){
        ServerCharacterOneUnequipToCharEquip(character, slot, itemOne, itemTwo);
    }
    [Server]
    void ServerCharacterOneUnequipToCharEquip(string character, string slot, ItemSelectable itemOne, ItemSelectable itemTwo){
        string request = "CharUnequipCharEquipSame";
        EquippingData BuiltEquipData = new EquippingData { Request = request, CharacterSlot = character, Slot =  slot};

        CharOneUnequipToCharEquip.Invoke(this.connectionToClient, itemOne, itemTwo, BuiltEquipData);
    }
    void CharacterTwoUnequipToCharEquip(string character, ItemSelectable itemOne, ItemSelectable itemTwo, string slot){
        CmdCharacterTwoUnequipToCharEquip(character, slot, itemOne, itemTwo);
    }
    [Command]
    void CmdCharacterTwoUnequipToCharEquip(string character, string slot, ItemSelectable itemOne, ItemSelectable itemTwo){
        ServerCharacterTwoUnequipToCharEquip(character, slot, itemOne, itemTwo);
    }
    [Server]
    void ServerCharacterTwoUnequipToCharEquip(string character, string slot, ItemSelectable itemOne, ItemSelectable itemTwo){
        string request = "CharUnequipCharEquipSame";
        EquippingData BuiltEquipData = new EquippingData { Request = request, CharacterSlot = character , Slot = slot};

        CharTwoUnequipToCharEquip.Invoke(this.connectionToClient, itemOne, itemTwo, BuiltEquipData);
    }
     void CharacterUnequipToTactStash(string character, InventoryItem itemOne, InventoryItem itemTwo, string slot){
        string ItemOneID = itemOne.SeeSelectable().customID;
        string ItemTwoID = itemTwo.SeeSelectable().customID;
        if(ItemOneID == ItemTwoID){
            print("Rejected this call! ************************************ means we have an issue still");
            return;
        }
        bool inv = itemOne.GetTactInventory();
        bool belt = itemOne.GetTactBelt();
        bool stash = itemOne.GetStashSheet();
        bool NFT = itemOne.SeeSelectable().NFT;
        if(stash){
            inv = false;
        }
        print($"{inv} inv, {belt} belt, {stash} stash, {NFT} NFT, {ItemOneID} ItemOneID, {ItemTwoID} ItemTwoID, {itemOne.GetTacticianSheet()} itemOne.GetTacticianSheet, {itemTwo.GetTacticianSheet()} itemTwo.GetTacticianSheet, {itemOne.GetStashSheet()} itemOne.GetStashSheet, {itemTwo.GetStashSheet()} itemTwo.GetStashSheet,");
        CmdCharacterUnequipToTactStash(character, slot, ItemOneID, ItemTwoID, inv, belt, stash, NFT);
    }
    [Command]
    void CmdCharacterUnequipToTactStash(string character, string slot, string itemOne, string itemTwo, bool inv, bool belt, bool stash, bool NFT){
        ServerCharacterUnequipToTactStash(character, slot, itemOne, itemTwo, inv, belt, stash, NFT);
    }
    [Server]
    void ServerCharacterUnequipToTactStash(string character, string slot, string itemOne, string itemTwo, bool inv, bool belt, bool stash, bool NFT){
        //itemOne is the one going to tact, itemTwo is coming from stash tact inv or belt lets find out
        bool tactInv = inv;
        bool tactBelt = belt;
        bool NFTStash = NFT;
        bool Stash = stash;
        string tactOrStashID = "Tactician";
        if(stash){
            tactOrStashID = "Stash";
        }
        //if(tactInv || tactBelt){
        //    tactOrStashID = "Tactician";
        //} else {
        //    tactOrStashID = "Stash";
        //}
        string request = "CharacterUnequipToTactStash";
        print($"{tactOrStashID} is the tact or stash id ");
        EquippingData BuiltEquipData = new EquippingData { Request = request, CharacterSlot = character , Slot = slot, CharacterSlotOne = tactOrStashID , TactBelt = tactBelt, Stash = Stash, TactInv = tactInv };

        CharUnequipTactToCharEquip.Invoke(this.connectionToClient, itemOne, itemTwo, BuiltEquipData);
    }
    void CreateNewStackItem(NewStackCreated buildingStack){
        CmdCreateNewStackItem(buildingStack);
    }
    [Command]
    void CmdCreateNewStackItem(NewStackCreated buildingStack){
        ServerCreateNewStackItem(buildingStack);
    }
    [Server]
    void ServerCreateNewStackItem(NewStackCreated buildingStack){

        BuildStackableItem.Invoke(buildingStack);
    }
    //****************************
    //***********Characters*******
    //**************************** 
    [TargetRpc]
    public void TargetStartTokenUpdate(){
        StartCoroutine(TokenUpdaterStart());
        
    }
    [TargetRpc]
    public void TargetTokenUpdate(){
        StartCoroutine(TokenUpdater());
        
    }
    IEnumerator TokenUpdater(){
        yield return new WaitForSeconds(2f);
        PingUpdate.Invoke("Wallet");
        ResetTokens.Invoke();
    }
    IEnumerator TokenUpdaterStart(){
        yield return new WaitForSeconds(2f);
        StartPlayerUIGROUP.Invoke();
        ResetTokens.Invoke();
    }
    [Command]
    void CmdAddToParty(string charID){
        PartySelected(charID);
    }
    [Server]
    void PartySelected(string charID){
        if(ActivePartyList.Count < 6){
            SendParty.Invoke(this.connectionToClient, charID);
        }
    }
    [TargetRpc]
    public void TargetPopulateSelected(){
        PartySpawned.Invoke();
    }
    void AddPartyMember(string Id){
        if(ActivePartyList.Count > 5){
            return;
        }
        foreach(var sheet in InformationSheets){
            if(sheet.CharacterID == Id){
                if(!ActivePartyList.Contains(Id)){
                    CmdAddToParty(Id);
                }
            }
        }
    }
    void RemovePartyMember(string ID){
        if(ActivePartyList.Contains(ID)){
            ActivePartyList.Remove(ID);
            CmdRemovePartyMember(ID);
        }
    }
    [Command]
    void CmdRemovePartyMember(string ID){
        if(ActivePartyList.Contains(ID)){
            ServerRemovePartyMember(ID);
        }
    }
    [Server]
    void ServerRemovePartyMember(string ID){
        if(ActivePartyList.Contains(ID)){
            PartyRemoval.Invoke(this.connectionToClient, ID);
        }
        
    }
    void RollNeed(string itemName, string ID, string choice){
        CmdPassVoteNeed(itemName, ID, choice);
    }
    [Command]
    void CmdPassVoteNeed(string itemName, string ID, string choice){
        VoteNeed.Invoke(itemName, ID, choice, this);
    }
    void RollGreed(string itemName, string ID, string choice){
        CmdPassVoteGreed(itemName, ID, choice);
    }
    [Command]
    void CmdPassVoteGreed(string itemName, string ID, string choice){
        VoteGreed.Invoke(itemName, ID, choice, this);
    }
    void RollPass(string itemName, string ID){
        CmdPassVotePass(itemName, ID);
    }
    [Command]
    void CmdPassVotePass(string itemName, string ID){
        VotePass.Invoke(itemName, ID, this);
    }
    void HealPartyRequest(){
        CmdHealPartyRequest();
    }
    [Command]
    void CmdHealPartyRequest(){
        HealPartyServer.Invoke(this.connectionToClient);
    }
    void RequestToBuild(string _nameRequest, string _Type, string spri){
        //do check to make sure this is a valid use of a token and we hav eone
        //print("Ready to ping server");
        string fighterQuestID = "Arudine-A1-3";
        string fighterTrig = "Recruit a fighter from captain edmure";
        string priestQuestID = "Arudine-A1-4";
        string priestTrig = "Recruit a priest from captain edmure";
        if(!QuestProgression.Contains(fighterQuestID)){
            foreach(var curQuest in QuestInProgressData){
                if(curQuest.questID == fighterQuestID){
                    QuestTriggeredData(fighterQuestID, fighterTrig);
                    break;
                }
            }
        }
        if(!QuestProgression.Contains(priestQuestID)){
            foreach(var curQuest in QuestInProgressData){
                if(curQuest.questID == priestQuestID){
                    QuestTriggeredData(priestQuestID, priestTrig);
                    break;
                }
            }
        }
        CMDRecruitCharacter(_nameRequest, _Type, spri);
    }
    [Command]
    void CMDRecruitCharacter(string _nameRequest, string _Type, string spri){
        //if character token is in inventory process
        RecruitCharacter(_nameRequest, _Type, spri);

    }
    [Server]
    void RecruitCharacter(string _nameRequest, string _Type, string spri){
        //print("got to recruit character");

        ServerCharacterBuildRequest.Invoke(this.connectionToClient, _nameRequest, _Type, spri);
    }
    public void BuildWyvernGM(){
        if(GameMaster){
            CMDBuildWyvern("Cheesil");
        }
    }
    [Command]
    void CMDBuildWyvern(string _nameRequest){
        //if character token is in inventory process
        ServerBuildWyvern(_nameRequest);

    }
    [Server]
    void ServerBuildWyvern(string _nameRequest){
        //print("got to recruit character");

        ServerWyvernHatch.Invoke(this.connectionToClient, _nameRequest);
    }
    
    [Server]
    public void TokenCounted(int tokens){
        TokenCount = tokens;
        TargetTokenCheck(tokens);
    }
    [TargetRpc]
    void TargetTokenCheck(int tokens){
        if(tokens == 0){
            CloseBuildWindow.Invoke();
        }
    }
    [Server]
    public void ServerResetRandomChance(){
        movedDistance = 0f;
        lastRollTime = 0f;
    }
    Sprite Load( string imageName, string spriteName)
    {
        Sprite[] all = Resources.LoadAll<Sprite>( imageName);
 
        foreach( var s in all)
        {
            if (s.name == spriteName)
            {
                return s;
            }
        }
        return null;
    }
    public override void OnStartAuthority(){
        
    }
    [Server]
    public void ServerMapSelect(bool on){
        if(on){
            TargetOVMapOn();
        } else {
            TargetOVMapOff();
        }
    }
    [TargetRpc]
    public void TargetOVMapOn(){
        MapOn.Invoke();
    }
    [TargetRpc]
    public void TargetOVMapOff(){
        MapOff.Invoke();
    }
    [Server]
    public void SetPlayerData(PlayerInfo playerData){
        //print("printing at set player data");
        playerName = playerData.PlayerName;
        Energy = playerData.Energy;
        lastScene = playerData.CurrentScene;
        currentScene = playerData.CurrentScene;
        //currentNode = playerData.SavedNode;
        loadSprite = playerData.PlayerSprite;
        charliesTicket = playerData.SessionTicket;
        if(playerData.newPlayer){
            lastScene = "TOWNOFARUDINE";
            currentScene = "TOWNOFARUDINE";
            //Gold = 1000000;
        } 
        RpcSetTactname(playerData.PlayerName);
        RpcSetSpriteAnimator(loadSprite);
        //print("printing at end set player data");
        //print($"printing {currentNode}");
    }
    [ClientRpc]
    void RpcSetSpriteAnimator(string spriteType){
        animator.enabled = true;
        if(spriteType == "Male"){
            animator.SetBool("male", true);
        } else {
            animator.SetBool("male", false);
        }
    }
    [ClientRpc]
    void RpcSetTactname(string pName){
        //print($"Setting tact name to {pName}");
        TactName.text = pName;
    }
    [Server]
    public void SetOurNode(Dictionary<string, SceneNode> nodes){
       // StartCoroutine(PauseThenSetNode(current));
        sceneNodesDictionary = nodes;
    }
    IEnumerator PauseThenSetNode(string current){
        yield return new WaitForSeconds(2f);
        OurNode = FindNodeByName(current);
        //print($"printing {current}");
        //TargetSetOurNode(current);
    }
    
    void FindOurNode()
    {
        if(currentScene == "OVM"){
            //OurNode = FindNodeByName(currentNode);
            //print($"Finding node and setting ournode to {currentNode}");
        }
    }
    [TargetRpc]
    public void TargetCharge(float _energy)
    {
        Charging.Invoke(_energy);
    }
    [Server]
    public void EnergyTick(float energy){
        Energy = energy;
        //print($"Testing Energy: {Energy}");
        TargetUpdateEnergyDisplay(Energy);
    }
    [TargetRpc]
    public void TargetWalletAwake(){
        WalletAwake.Invoke();
    }
    [TargetRpc]
    public void TargetQuestCompleted(){
        Speaker.Invoke(252);
    }
    [TargetRpc]
    public void TargetOpenUI(string currentScene){
        OnUI.Invoke(currentScene);
        //WalletAwake.Invoke();
    }
    //[Server]
    //public void ServerTurnOffSprite(){
    //    TargetTurnOffSprite();
    //}
    //[TargetRpc]
    //void TargetTurnOffSprite(){
    //    TurnOffSprite();
    //}
    //void TurnOffSprite(){
    //    SpriteRenderer sRend = GetComponent<SpriteRenderer>();
    //    sRend.enabled = false;
    //}
    
    //[TargetRpc]
    //public void TargetToggleSprite(bool show, string loadSprite){
    //    SceneChangeSprite(show, loadSprite);
    //}
    [TargetRpc]
    public void TargetShowPartyCombatView(){
        if(selectedCharacters.Count > 0){
            //clear them all
            selectedCharacters.Clear();
        }
        CombatPartyView.instance.TurnOnCanvas();
        FormationScript.instance.TurnOnCanvas();
    }
    void SpriteAnimatorSet(){
        isDragging = false;
            RectTransform energyGO = energySlider.GetComponent<RectTransform>();
        //if(currentScene == "OVM"){
        //    energyGO.transform.position = new Vector3(energyGO.transform.position.x, -0.294f, energyGO.transform.position.z);
        //} else {
        //    energyGO.transform.position = new Vector3(energyGO.transform.position.x, -0.5f, energyGO.transform.position.z);
        //}
       // if (currentScene == "OVM") {
       //    energyGO.anchoredPosition = new Vector2(energyGO.anchoredPosition.x, -0.294f);
       //} else {
       //    energyGO.anchoredPosition = new Vector2(energyGO.anchoredPosition.x, -2.26f);
       //    //energyGO.anchoredPosition = new Vector2(energyGO.anchoredPosition.x, -0.5f);
       //}
        bool SpriteSet = false;
        if(!StatAsset.Instance.CheckForCombatZone(currentScene)){
            SpriteSet = true;
        }
        StartCoroutine(SceneChangeDelay(SpriteSet));
    }
   
   IEnumerator SceneChangeDelay(bool SpriteSet){
    TOWNsRend.enabled = false;
    OVMsRend.enabled = false;
    TactName.enabled = false;
    backgroundSpriteUI.SetActive(false);
    yield return new WaitForSeconds(1.25f);
    
    SceneChangeSprite(SpriteSet);
   }
    void SceneChangeSprite(bool show){
        Quaternion rotation = SpriteFlip.transform.rotation;
        if(rotation.x != 0f){
            rotation = Quaternion.Euler(0f, rotation.eulerAngles.y, rotation.eulerAngles.z);
            
            SpriteFlip.transform.rotation = rotation;
        }
        if(loadSprite == "Male"){
            animator.SetBool("male", true);
        } else {
            animator.SetBool("male", false);
        }
        animator.SetBool("stand", true);
        animator.SetBool("started", true);
        animator.SetBool("right", false);
        animator.SetBool("left", false);
        animator.SetBool("up", false);
        animator.SetBool("down", true);
        SpriteRenderer sRend = GetComponent<SpriteRenderer>();
        
        GameObject energyGO = energySlider.gameObject;

        RectTransform energyGORect = energySlider.GetComponent<RectTransform>();

        Quaternion rotationEnergy = energySlider.gameObject.transform.rotation;
        if(rotationEnergy.x != 0f){
            rotationEnergy = Quaternion.Euler(0f, rotationEnergy.eulerAngles.y, rotationEnergy.eulerAngles.z);
            
            energySlider.gameObject.transform.rotation = rotationEnergy;
        }
        if(!isLocalPlayer){
             print("Accessed the build sprite not on a local player");
             //energySlider.gameObject.transform.position = new Vector3(energySlider.gameObject.transform.position.x, energySlider.gameObject.transform.position.y, -0.56f);
             //energyGO.position = new Vector3(energyGO.position.x, energyGO.position.y, -0.56f);
             energyGORect.anchoredPosition3D = new Vector3(-.4f, -2.26f, -0.56f);
             if (currentScene == "OVM") {
                energyGORect.anchoredPosition3D = new Vector3(-.4f, -2.26f, -0.28f);
                } else {
//                    energyGORect.anchoredPosition = new Vector2(energyGORect.anchoredPosition.x, -0.5f);
                    energyGORect.anchoredPosition3D = new Vector3(-.4f, -2.26f, -0.56f);
                }
        } else {
            if(isLocalPlayer){
                if (currentScene == "OVM") {
                    energyGORect.anchoredPosition = new Vector2(energyGORect.anchoredPosition.x, -2f);
                } else {
//                    energyGORect.anchoredPosition = new Vector2(energyGORect.anchoredPosition.x, -0.5f);
                   energyGORect.anchoredPosition = new Vector2(energyGORect.anchoredPosition.x, -2.26f);
                }
            }
        }
        if(show)
        {
            sRend.enabled = true;
            //spriteSwap = StartCoroutine(SpriteSwapPlayer(playerSprite, scene));
            SpriteSwapOnPlayer();
            TOWNsRend.enabled = true;
            OVMsRend.enabled = true;
            TactName.enabled = true;
            backgroundSpriteUI.SetActive(true);
            energyGO.SetActive(true);
            StartCoroutine(ShowAllOthers());
            if(currentScene == "OVM"){
                Vector3 newScale = new Vector3(.5f, .5f, .5f);
                SpriteFlip.transform.localScale = newScale;
                energyGO.transform.localScale = newScale;
            } else {
                Vector3 newScale = new Vector3(1f, 1f, 1f);
                SpriteFlip.transform.localScale = newScale;
                energyGO.transform.localScale = newScale;
            }


        }else{
            sRend.enabled = false;
            backgroundSpriteUI.SetActive(false);
            energyGO.SetActive(false);
            TOWNsRend.enabled = false;
            OVMsRend.enabled = false;
            TactName.enabled = false;
            StartCoroutine(HideAllOthers());

            if(currentScene == "OVM"){
                Vector3 newScale = new Vector3(.5f, .5f, .5f);
                SpriteFlip.transform.localScale = newScale;
                energyGO.transform.localScale = newScale;
            } else {
                Vector3 newScale = new Vector3(1f, 1f, 1f);
                SpriteFlip.transform.localScale = newScale;
                energyGO.transform.localScale = newScale;
            }
        }
    }
    IEnumerator HideAllOthers(){
        yield return new WaitForSeconds(2f);
        HideAll.Invoke();
    }
    IEnumerator ShowAllOthers(){
        yield return new WaitForSeconds(2f);
        ShowAll.Invoke();
    }
    void HideReceiver(){
        SpriteRenderer sRend = GetComponent<SpriteRenderer>();
        GameObject energyGO = energySlider.gameObject;
        sRend.enabled = false;
        backgroundSpriteUI.SetActive(false);
        energyGO.SetActive(false);
        TOWNsRend.enabled = false;
        OVMsRend.enabled = false;
        TactName.enabled = false;
    }
    void ShowReceiver(){
        SpriteRenderer sRend = GetComponent<SpriteRenderer>();
        GameObject energyGO = energySlider.gameObject;
        SpriteSwapOnPlayer();
        TOWNsRend.enabled = true;
        OVMsRend.enabled = true;
        TactName.enabled = true;
        backgroundSpriteUI.SetActive(true);
        energyGO.SetActive(true);
    }
    void SpriteSwapOnPlayer(){
        SpriteRenderer sRend = GetComponent<SpriteRenderer>();
        sRend.enabled = true;
        //if(loadSprite == "Male"){
        //    animator.SetBool("male", true);
        //} else {
        //    animator.SetBool("male", false);
        //}
        //Quaternion rotation = SpriteFlip.transform.rotation;
        //if(rotation.x == 0f){
        //    rotation = Quaternion.Euler(90f, rotation.eulerAngles.y, rotation.eulerAngles.z);
        //    SpriteFlip.transform.rotation = rotation;
        //}
        //if(rotation.x > 90f){
        //    rotation = Quaternion.Euler(0, rotation.eulerAngles.y, rotation.eulerAngles.z);
        //}
        if(currentScene == "OVM"){
            Vector3 newScale = new Vector3(.5f, .5f, .5f);
            SpriteFlip.transform.localScale = newScale;
        } else {
            Vector3 newScale = new Vector3(1f, 1f, 1f);
            SpriteFlip.transform.localScale = newScale;
        }
        //TactName.enabled = true;
        //TOWNsRend.enabled = true;
    }
    /*
    IEnumerator SpriteSwapPlayer(string playerSprite, string scene){
        //SpriteRenderer sRend = GetComponent<SpriteRenderer>();
        //sRend.enabled = true;
        //Animator animator = SpriteFlip.GetComponent<Animator>();
        //animator.SetBool("male", true);
        Quaternion rotation = SpriteFlip.transform.rotation;
        if(rotation.x == 0f){
            rotation = Quaternion.Euler(90f, rotation.eulerAngles.y, rotation.eulerAngles.z);
            SpriteFlip.transform.rotation = rotation;
        }
        if(scene == "OVM"){
            Vector3 newScale = new Vector3(.5f, .5f, .5f);
            SpriteFlip.transform.localScale = newScale;
        } else {
            Vector3 newScale = new Vector3(1f, 1f, 1f);
            SpriteFlip.transform.localScale = newScale;
        }
        TactName.enabled = true;
        TOWNsRend.enabled = true;
        //OVMsRend.enabled = true;
        //string playerSpriteAlt = playerSprite.Replace("Player0", "Player1");
        //Sprite spriteOne = Load("Player0", playerSprite);
        //Sprite spriteTwo = Load("Player1", playerSpriteAlt);
        while(true){
            TOWNsRend.sprite = spriteOne;
            //OVMsRend.sprite = spriteOne;
            yield return new WaitForSeconds(.5f);
            TOWNsRend.sprite = spriteTwo;
            //OVMsRend.sprite = spriteTwo;
            yield return new WaitForSeconds(.5f);
        }
    }
    */
    [TargetRpc]
    public void TargetToggleLoadBarOff(){
        LoadbarOffToggle.Invoke();
        //if(currentScene != "OVM" && currentScene != "TOWNOFARUDINE"){
        if(StatAsset.Instance.CheckForCombatZone(currentScene)){
            SpriteFlip.SetActive(false);
        } else {
            SpriteFlip.SetActive(true);
        }
    }
    void RequestPurchase(uint price, string itemName, int quant){
        //print($"Item: {itemName} was purchased at price: {price}");
        if(price > 0 && price < 30001 && itemName != null){
            CmdPayForItem(price, itemName, quant);
        }
    }
    [Command]
    void CmdPayForItem(uint price, string itemName, int quant){
        if(price > 0 && price < 30001 && itemName != null){
        ServerTransmitPayment(price, itemName, quant);
        }

    }
    [Server]
    void ServerTransmitPayment(uint price, string itemName, int quant){
        ServerTransmitTX.Invoke(this.connectionToClient, price, itemName, quant);
    }
    [TargetRpc]
    public void TargetEnablePurchaseBtn(){
        EnablePurchaseBtn();
    }
    void EnablePurchaseBtn(){
        PurchaseButtonAvailable.Invoke();
    }
    void RequestOVM(string direction){
        if(!isLocalPlayer){ return; }
        if(currentScene == "TOWNOFARUDINE"){
            CmdRequestOVM(direction);
        }
    }
    [Command]
    void CmdRequestOVM(string direction){
        OurNode = FindNodeByName("TOWNOFARUDINE");
        RequestOVMCheck(direction);
        TargetSetTownNode();
        //add validation 
    }
    [TargetRpc]
    void TargetSetTownNode(){
        OurNode = FindNodeByName("TOWNOFARUDINE");
    }
    [Server]
    void RequestOVMCheck(string direction){
        ResetOVM.Invoke(this.connectionToClient, charliesTicket, direction);
        
    }
    [TargetRpc]
    public void TargetSendOVMRequest(){
        SendingOVMRequest();
    }
    void SendingOVMRequest(){
        OVMRequest.Invoke(currentScene);
    }
    [Command]
    void CmdProcessSpellPurchase(LearnSpell spell, string spellBook){
        SpellPurchase.Invoke(this.connectionToClient, spell, spellBook); 
    }
    [Command]
    void CmdProcessSpellChangeTact(string spellName, int slot){
        ServerProcessSpellChangeTact(spellName, slot);
    }
    [Server]
    public void ServerChangingScenesCheck(){
        ServerClearAllPurchaseData();
        ServerClearMovement();
        if(TactSpellOne && CooldownSpellOne > 0){
            ServerBuildTacticianSpellCooldownLogin(CooldownSpellOne, 1);
        }
        if(TactSpellTwo && CooldownSpellTwo > 0){
            ServerBuildTacticianSpellCooldownLogin(CooldownSpellTwo, 2);
        }
        //ServerCheckAllItemsOnSceneSwap();
        //StartCoroutine(ServerChangeScenesCheckCD());
    }
    IEnumerator ServerChangeScenesCheckCD(){
        //if(abilityOneCDRoutine != null){
        //    StopCoroutine(abilityOneCDRoutine);
        //}
        //if(abilityTwoCDRoutine != null){
        //    StopCoroutine(abilityTwoCDRoutine);
        //}
        yield return new WaitForSeconds(2f);
        if(SpellOne != "Empty" && SpellOne != "None"){
            ServerProcessSpellChangeTact(SpellOne, 1);
        }
        if(SpellTwo != "Empty" && SpellTwo != "None"){
            ServerProcessSpellChangeTact(SpellTwo, 2);
        }
    }
    [Server]
    public void ServerProcessSpellChangeTact(string spellName, int slot){

        //TactSpellChange.Invoke(this.connectionToClient, spellName, slot); //this is where we modify the SpellOne and spellTwo with their cooldown timers 
           bool onCD = false;
           string dateTimeWithZone = DateTime.UtcNow.ToString("o");
           int remainingSeconds = 0;
           
        DateTime initialTime = DateTime.Parse(dateTimeWithZone, null, System.Globalization.DateTimeStyles.RoundtripKind);
        print($"{initialTime} was intialTime ServerProcessSpellChangeTact");
           foreach(var cd in TacticianInformationSheet.TacticianCooldownData){
                if(cd.SpellnameFull.Contains(spellName)){
                    DateTime completedTime = DateTime.Parse(cd.Value, null, System.Globalization.DateTimeStyles.RoundtripKind);
        print($"{completedTime} was completedTime for {cd.SpellnameFull} ServerProcessSpellChangeTact");

                    TimeSpan timeLeft = completedTime - initialTime;
                    remainingSeconds = (int)timeLeft.TotalSeconds;
                    print($"{remainingSeconds} is our remaining seconds for {spellName}");
                    if (remainingSeconds > 0)
                    {

                        onCD = true;
                    } else {

                        remainingSeconds = 0;
                    }
                    break;
                }
           }
        if(slot == 1){
            SpellOne = spellName;
            

            if(onCD){
        print($"{remainingSeconds} was remainingSeconds and {spellName} was On cooldown!ServerProcessSpellChangeTact");

                if(abilityOneCDRoutine != null){
                    StopCoroutine(abilityOneCDRoutine);
                }
                abilityOneCDRoutine = StartCoroutine(SetAbilityCoolDownOne((float)remainingSeconds));
            } else {
        print($"{remainingSeconds} was remainingSeconds and {spellName} was off cooldown!ServerProcessSpellChangeTact");
        CooldownSpellOne = (float)remainingSeconds;
            TactSpellOne = false;
            }
            if(SpellTwo == SpellOne){
                if(abilityTwoCDRoutine != null){
                    StopCoroutine(abilityTwoCDRoutine);
                }
                SpellTwo = "Empty";
                CooldownSpellTwo = 0f;
                TactSpellTwo = false;
            }
        }
        if(slot == 2){
            SpellTwo = spellName;
            
            if(onCD){
        print($"{remainingSeconds} was remainingSeconds and {spellName} was On cooldown!ServerProcessSpellChangeTact");
                if(abilityTwoCDRoutine != null){
                    StopCoroutine(abilityTwoCDRoutine);
                }
                abilityTwoCDRoutine = StartCoroutine(SetAbilityCoolDownTwo((float)remainingSeconds));
            } else {
        print($"{remainingSeconds} was remainingSeconds and {spellName} was off cooldown!ServerProcessSpellChangeTact");
        CooldownSpellTwo = (float)remainingSeconds;
            TactSpellTwo = false;
            }
            if(SpellOne == SpellTwo){
                if(abilityOneCDRoutine != null){
                    StopCoroutine(abilityOneCDRoutine);
                }
                SpellOne = "Empty";
                CooldownSpellOne = 0f;
                TactSpellOne = false;
                
            }
        }
        if(StatAsset.Instance.CheckForCombatZone(currentScene)){
            ServerRefreshEquippedSpell(slot);
        }
        //then tell
    }
    
    [Server]
    void ServerRefreshEquippedSpell(int slot){
        TargetRefreshEquipped(slot);
    }
    [TargetRpc]
    void TargetRefreshEquipped(int slot){
        RefreshSpellEquipped.Invoke(slot); 

    }
    [Command]
    void CmdProcessSpellChange(string spellName, string spellBook, string slot){
        SpellChange.Invoke(this.connectionToClient, spellName, spellBook, slot); 
    }
    [Command]
    void CmdPermissionToEnd(string nodePlusExit){
        ServerPermissionToEnd(nodePlusExit);
        
    }
    [Server]
    void ServerPermissionToEnd(string nodePlusExit){
        PermissionToFinish.Invoke(currentMatch, nodePlusExit);
    }

    [Command]
    void CmdPermissionToEndSewers(string nodePlusExit){
        ServerPermissionToEndSewers(nodePlusExit);
        
    }
    [Server]
    void ServerPermissionToEndSewers(string nodePlusExit){
        PermissionToFinishSewers.Invoke(currentMatch, nodePlusExit);
    }
    void BlockClicker(){
        playerOVMReady = false;
    }
    void ClearClicker(){
        playerOVMReady = true;
        //GetCharacters.Invoke();
        //foreach(var character in InformationSheets){
        //    GetCharacters.Invoke(character.CharacterID);
        //}
        
        if(IsOVMSceneLoaded()){
            StartCoroutine(SendDelayedTileSweep());
        }
        //CmdClearMovement();
    }
    [Command]
    void CmdClearMovement(){
        if (RandomMatchMovementCoroutine != null) {
            StopCoroutine(RandomMatchMovementCoroutine);
        }
        movedDistance = 0f;
        lastPosition = transform.position; // Initialize lastPosition the first time
    }
    [Server]
    public void ServerClearMovement(){
        if (RandomMatchMovementCoroutine != null) {
            StopCoroutine(RandomMatchMovementCoroutine);
        }
        movedDistance = 0f;
        lastPosition = transform.position; // Initialize lastPosition the first time
        //StartCoroutine(SendRpcQueueMovement());
    }
    IEnumerator SendDelayedTileSweep(){
        yield return new WaitForSeconds(.75f);
        NewWave.Invoke();
    }
    public bool IsOVMSceneLoaded()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name == "OVM")
            {
                return true;
            }
        }
        return false;
    }
    private void ProcessResRequest(string ID){
        CmdProcessResRequest(ID);
    }
    [Command]
    void CmdProcessResRequest(string ID){
        ResCharacter.Invoke(connectionToClient, ID);
    }
    private void LevelUpStartHandler(string timestamp, string charID){
        Debug.Log("Level Up Started at: " + timestamp);
        CmdSendServerLevelUpStart(timestamp, charID);
    }
    
    [Command]
    void CmdSendServerLevelUpStart(string timestampString, string charID){
        LevelUpStarted.Invoke(connectionToClient, timestampString, charID);
    }
    private void LevelUpCompleteHandler(string timestamp, string charID){
        Debug.Log("Level Up Completed!");
        CmdSendServerLevelUpComplete(timestamp, charID);
    }
    [Command]
    void CmdSendServerLevelUpComplete(string timestampString, string charID){
        LevelUpEnded.Invoke(connectionToClient,timestampString, charID);
    }
    public void ClearTargetSpellChange(MovingObject mo){
        print("Clearing selected and unselecting");
        foreach(var character in selectedCharacters){
            PlayerCharacter pc = character.GetComponent<PlayerCharacter>();
            pc.UnselectedMO();
            pc.UnTargettedMO();
            pc.UnselectedUnit();
        }
        //if(SelectedMob != null){
        //    SelectedMob.UnselectedMO();
        //    SelectedMob = null;
        //}
        //if(TargetMob != null){
        //    TargetMob.UnTargettedMO();
        //    TargetMob = null;
        //}
        selectedCharacters.Clear();
        CombatPartyView.instance.TurnOffSelectedWindow();
        ToggleSpellsOff.Invoke();
    }
    void StartUpReconfig(List<string> charIDS){
        CmdReconfiguredTeam(charIDS);
    }
    [Command]
    void CmdReconfiguredTeam(List<string> charIDS){
        ServerReconfigTeam(charIDS);
    }
    [Server]
    void ServerReconfigTeam(List<string> charIDS){
        ActivePartyList = charIDS;
        TargetReconfigTeam(charIDS);
    }
    [TargetRpc]
    void TargetReconfigTeam(List<string> charIDS){
        ActivePartyList = charIDS;
    }
    void SewerRequestedByPlayer(int floor){
        float energyCost = 0;
        if(floor == 1){
            energyCost = 50;
        }
        if(floor == 2){
            energyCost = 100;
        }
        if(floor == 3){
            energyCost = 150;
        }
        if(floor == 4){
            energyCost = 200;
        }
        //from town
        if(Energy < energyCost){
            ImproperCheckText.Invoke("Not enough energy to enter sewers");
            return;
        }
        if(ActivePartyList.Count == 0){
            ImproperCheckText.Invoke("Empty party");
            return;
        }
        CmdRequestMatchMakerSewer(floor);
    }
    [Command]
    void CmdRequestMatchMakerSewer(int floor){
        ServerBuildMatchMakerSewer(floor);
    }
    [Server]
    void ServerBuildMatchMakerSewer(int floor){
        matchID = MatchMaker.GetRandomMatchID();
        float energyCost = 0;
        if(floor == 1){
            energyCost = 50;
        }
        if(floor == 2){
            energyCost = 100;
        }
        if(floor == 3){
            energyCost = 150;
        }
        if(floor == 4){
            energyCost = 200;
        }
        GetRandomCost.Invoke(this.connectionToClient, energyCost);
        //print("called CMDSOLOGAME ************* on server");
        if (MatchMaker.instance.SewersInstance(matchID, this, false, out playerIndex, false, floor)) {
            Debug.Log($"Game {matchID} Solo created successfully");
            TargetRandomStartScreenShot();
        }
    }
    [Server]
    public void ServerBuildMatchMakerSewerLogin(string sewerMap, bool login){
        matchID = MatchMaker.GetRandomMatchID();
        int floor = 1;
        if(sewerMap == "Sewers level 2 story"){
            floor = 2;
        }
        if(sewerMap == "Sewers level 3 story"){
            floor = 3;
        }
        if(sewerMap == "Sewers level 4 story"){
            floor = 4;
        }
        //print("called CMDSOLOGAME ************* on server");
        if (MatchMaker.instance.SewersInstance(matchID, this, false, out playerIndex, login, floor)) {
            Debug.Log($"Game {matchID} Solo created successfully");
            TargetRandomStartScreenShot();
        }
    }
    //List<string> CurrentQuests = new List<string>();
    public List<string> QuestProgression = new List<string>();//Completed Quests
    public List<string> RepeatQuestCD = new List<string>(); //Completed repeatquests on cooldown

    public List<string> FinishedQuestsReadyToTurnIn = new List<string>(); // finished quests ready
    public List<QuestSaveData> QuestInProgressData = new List<QuestSaveData>();
    [Server]
    public void ServerSpawnQuestSystem(List<string> QuestCompleted, List<string> QuestCurrent, List<string> RepeatQuestCompleted){
        RepeatQuestCD = RepeatQuestCompleted;
        QuestProgression = QuestCompleted;
        foreach (var questString in QuestCurrent) {
            // Split the string by underscores to separate the quest ID from its objectives
            var questParts = questString.Split('_');
            // The first part is the quest ID
            string questID = questParts[0];
            // The rest are the objectives, which we can get by skipping the first entry
            var objectives = questParts.Skip(1).ToList();
            // Now create the QuestSaveData instance and add it to the list
            List<string> objectivesToAdd = new List<string>();
            foreach(var objective in objectives){
                if(!string.IsNullOrEmpty(objective)){
                    objectivesToAdd.Add(objective);
                }
            }
            QuestSaveData newQuest = new QuestSaveData(questID, objectivesToAdd);
            foreach(var obj in newQuest.objectives){
                //print(obj + " was an objective");
            }
            QuestInProgressData.Add(newQuest);
            if( objectivesToAdd.Count == 0){ //objectives.Count == 1 ||
                //print(objectivesToAdd.Count + " was our objective count for " + questID);
                FinishedQuestsReadyToTurnIn.Add(newQuest.questID);
                //foreach(var check in objectives){
                //    if(string.IsNullOrEmpty(check)){
                //        FinishedQuestsReadyToTurnIn.Add(newQuest.questID);
                //        break;
                //    }
                //}
            }
        }
        TargetSpawnQuestSystem(QuestCompleted, QuestCurrent, RepeatQuestCompleted);
    }
    [TargetRpc]
    void TargetSpawnQuestSystem(List<string> QuestCompleted, List<string> QuestCurrent, List<string> RepeatQuestCompleted){
        RepeatQuestCD = RepeatQuestCompleted;
        QuestProgression = QuestCompleted;
        bool firstOne = false;
        foreach (var questString in QuestCurrent) {
            // Split the string by underscores to separate the quest ID from its objectives
            var questParts = questString.Split('_');
            // The first part is the quest ID
            string questID = questParts[0];
            // The rest are the objectives, which we can get by skipping the first entry
            var objectives = questParts.Skip(1).ToList();
            // Now create the QuestSaveData instance and add it to the list
            List<string> objectivesToAdd = new List<string>();
            foreach(var objective in objectives){
                if(!string.IsNullOrEmpty(objective)){
                    objectivesToAdd.Add(objective);
                }
            }
            QuestSaveData newQuest = new QuestSaveData(questID, objectivesToAdd);
            foreach(var obj in newQuest.objectives){
                //print(obj + " was an objective");
            }
            QuestInProgressData.Add(newQuest);
            if( objectivesToAdd.Count == 0){ //objectives.Count == 1 ||
                //print(objectivesToAdd.Count + " was our objective count for " + questID);
                FinishedQuestsReadyToTurnIn.Add(newQuest.questID);
                //foreach(var check in objectives){
                //    if(string.IsNullOrEmpty(check)){
                //        FinishedQuestsReadyToTurnIn.Add(newQuest.questID);
                //        break;
                //    }
                //}
            }
            if(!firstOne){
                firstOne = true;
                QuestSheet.Instance.SetStarterDisplay(questID);
            } else {
                QuestSheet.Instance.SpawnNewQuestInLog(questID);
            }
        }
    }
    public List<string> GetRepeatQuestCompletedFullyList(){
        return RepeatQuestCD;
    }
    public List<string> GetQuestCompletedFullyList(){
        return QuestProgression;
    }
    public List<QuestSaveData> GetQuestSavedStatus(){
        return QuestInProgressData;
    }
    public List<string> GetFinishedQuestsReadyToTurnIn(){
        return FinishedQuestsReadyToTurnIn;
    }
    //public bool GetReadyToCompleteQuests(List<string> questsAvail){
    //    foreach(var rQ in FinishedQuestsReadyToTurnIn){
    //        if(questsAvail.Contains(rQ)){
    //            CompletedQuest(rQ);
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    public bool GetReadyToCompleteQuests(List<string> questsAvail, out string completedQuestId){
    completedQuestId = null; // Initialize the out parameter

    foreach(var rQ in FinishedQuestsReadyToTurnIn){
        if(questsAvail.Contains(rQ)){
            completedQuestId = rQ; // Set the completed quest ID
            if(!CompletedQuest(rQ)){
                return false;
            }
            return true;
        }
    }
    return false;
}
    bool CompletedQuest(string questID){
        //we meed to start process to get rewards and stamp this quest as completed or on CD if repeatable
        Quest quest = QuestLog.Instance.GetQuestByName(questID);
        if(FinishedQuestsReadyToTurnIn.Contains(questID)){
            if(questID == "Arudine-A1-4"){
                int counter = 0;
                foreach(var invItem in TacticianInformationSheet.TacticianInventoryData){
                    if(invItem.Value.Deleted || invItem.Value.amount == 0){
                        continue;
                    }
                    if(invItem.Value.EQUIPPED == false){
                        counter++;
                    }
                }
                if(counter >= 19){
                    ImproperCheckText.Invoke("Not enough space to complete this quest");
                    QuestFinished.Invoke(questID);
                    return false;
                }
            }
            if(questID == "Arudine-A1-5"){
                int counter = 0;
                foreach(var invItem in TacticianInformationSheet.TacticianInventoryData){
                    if(invItem.Value.Deleted || invItem.Value.amount == 0){
                        continue;
                    }
                    if(invItem.Value.EQUIPPED == false){
                        counter++;
                    }
                }
                if(counter >= 20){
                    ImproperCheckText.Invoke("Not enough space to complete this quest");
                    QuestFinished.Invoke(questID);

                    return false;
                }
            }
            if(questID == "Arudine-A1-6"){
                int counter = 0;
                foreach(var invItem in TacticianInformationSheet.TacticianInventoryData){
                    if(invItem.Value.Deleted || invItem.Value.amount == 0){
                        continue;
                    }
                    if(invItem.Value.EQUIPPED == false){
                        counter++;
                    }
                }
                if(counter >= 20){
                    ImproperCheckText.Invoke("Not enough space to complete this quest");
                    QuestFinished.Invoke(questID);
                    return false;
                }
            }
            if(questID == "Arudine-A1-7"){
                int counter = 0;
                foreach(var invItem in TacticianInformationSheet.TacticianInventoryData){
                    if(invItem.Value.Deleted || invItem.Value.amount == 0){
                        continue;
                    }
                    if(invItem.Value.EQUIPPED == false){
                        counter++;
                    }
                }
                if(counter >= 20){
                    ImproperCheckText.Invoke("Not enough space to complete this quest");
                    QuestFinished.Invoke(questID);
                    return false;
                }
            }
            if(questID == "Arudine-A1-8"){
                int counter = 0;
                foreach(var invItem in TacticianInformationSheet.TacticianInventoryData){
                    if(invItem.Value.Deleted || invItem.Value.amount == 0){
                        continue;
                    }
                    if(invItem.Value.EQUIPPED == false){
                        counter++;
                    }
                }
                if(counter >= 20){
                    ImproperCheckText.Invoke("Not enough space to complete this quest");
                    QuestFinished.Invoke(questID);
                    return false;
                }
            }
            if(questID == "Arudine-A2-1"){
                int counter = 0;
                foreach(var invItem in TacticianInformationSheet.TacticianInventoryData){
                    if(invItem.Value.Deleted || invItem.Value.amount == 0){
                        continue;
                    }
                    if(invItem.Value.EQUIPPED == false){
                        counter++;
                    }
                }
                if(counter >= 16){
                    ImproperCheckText.Invoke("Not enough space to complete this quest");
                    QuestFinished.Invoke(questID);
                    return false;
                }
            }
            FinishedQuestsReadyToTurnIn.Remove(questID);
            QuestCompletedUpdate.Invoke(6000, quest.Name);
            QuestSheet.Instance.CompleteQuestTurnIn(questID);
            CmdCompletedQuest(questID);
        }
        // Iterate through the QuestInProgressData to find and remove the completed quest
        for(int i = 0; i < QuestInProgressData.Count; i++){
            if(QuestInProgressData[i].questID == questID){
                // Remove the quest from the QuestInProgressData
                QuestInProgressData.RemoveAt(i);
                // Break out of the loop as the quest is found and removed
                break;
            }
        }
        if(quest.Repeatable){
            return true;
        }
        if(!QuestProgression.Contains(questID)){
            QuestProgression.Add(questID);
        }
            return true;

    }
    [Command]
    void CmdCompletedQuest(string questID){
        if(FinishedQuestsReadyToTurnIn.Contains(questID)){
            FinishedQuestsReadyToTurnIn.Remove(questID);
        }
        // Iterate through the QuestInProgressData to find and remove the completed quest
        for(int i = 0; i < QuestInProgressData.Count; i++){
            if(QuestInProgressData[i].questID == questID){
                // Remove the quest from the QuestInProgressData
                QuestInProgressData.RemoveAt(i);
                // Break out of the loop as the quest is found and removed
                break;
            }
        }
        //tell server to get reward for this quest
        if(!QuestProgression.Contains(questID)){
            QuestProgression.Add(questID);
            QuestCompleteRewardAccess.Invoke(connectionToClient, questID);
            //send reward
        }
    }
    [TargetRpc]
    void TargetRefreshQuestsAfterTurnIn(){

    }
    public bool PrereqQuestCheck(string questID){
        bool quest = false;
        if(questID.Contains("Arudine-A1")){
            quest = true;
        }
        if(questID.Contains("Arudine-A2")){
            if(QuestProgression.Contains("Arudine-A1-8")){
                quest = true;
            }
        }
        if(questID.Contains("Arudine-A3")){
            if(QuestProgression.Contains("Arudine-A2-4")){
                quest = true;
            }
        }
        return quest;
    }
    public List<string> GetQuestProgression(List<string> questChainCheck){
        List<string> QuestsCompleted = new List<string>();
        foreach(var questId  in questChainCheck){
            bool isCompleted = QuestProgression.Contains(questId);
            // If the quest is not completed, add it to QuestsNotCompleteOrCurrent
            if (isCompleted)
            {
                QuestsCompleted.Add(questId);
            }
        }
        return QuestsCompleted;
    }
    public bool GiveQuest(string questID){
        bool currentContains = QuestInProgressData.Any(cQ => cQ.questID == questID);
        if(QuestProgression.Contains(questID) || currentContains){
            return false;
        } else {
            AddQuestToCurrent(questID);
            PingUpdate.Invoke("Quest");
            return true;
        }
    }
    public void PlayDeclineSound(){
        Speaker.Invoke(251);
    }
    void AddQuestToCurrent(string questID){
        bool currentContains = QuestInProgressData.Any(cQ => cQ.questID == questID);
        if(!currentContains){
            //create build quest saved data
            Quest quest = QuestLog.Instance.GetQuestByName(questID);
            List<string> questObjs = new List<string>();
            foreach(var obj in quest.Objectives){
                questObjs.Add(obj);
            }
            QuestSaveData creatingQuestSave = new QuestSaveData(questID, questObjs);
            print("Adding quest to current printing all of the objectives");
            foreach(var obj in quest.Objectives){
                print(obj + " was an objective");
            }
            QuestInProgressData.Add(creatingQuestSave);
            Speaker.Invoke(250);
            //CurrentQuests.Add(questID);
            CmdAddQuestToCurrent(questID);
            QuestSheet.Instance.SpawnNewQuestInLog(questID);
        }
    }
    [Command]
    void CmdAddQuestToCurrent(string questID){
        //bool currentContains = false;
        //foreach(var cQ in QuestInProgressData){
        //    if(cQ.questID == questID){
        //        currentContains = true;
        //        break;
        //    }
        //}
        bool currentContains = QuestInProgressData.Any(cQ => cQ.questID == questID);
        if(!currentContains){
            //CurrentQuests.Add(questID);
            Quest quest = QuestLog.Instance.GetQuestByName(questID);
            List<string> questObjs = new List<string>();
            foreach(var obj in quest.Objectives){
                questObjs.Add(obj);
            }
            QuestSaveData creatingQuestSave = new QuestSaveData(questID, questObjs);
            QuestInProgressData.Add(creatingQuestSave);
        }
    }
    public List<string> GetQuestsCurrent(List<string> questChainCheck){
        List<string> QuestsCurrent = new List<string>();
        foreach(var cQ in QuestInProgressData){
            QuestsCurrent.Add(cQ.questID);
        }
        //foreach(var questId  in questChainCheck){
        //    bool isCurrent = CurrentQuests.Contains(questId);
        //    // If the quest is not Current, add it to QuestsNotCompleteOrCurrent
        //    if (isCurrent)
        //    {
        //        QuestsCurrent.Add(questId);
        //    }
        //}
        return QuestsCurrent;
    }
    //We need to build this on the server so we can know their pref 

    int InspectSwitch = 0;
    public void SetInspectSwitch(){
        InspectSwitch = PlayerPrefs.GetInt("InspectSwitch", 1);
        CmdSetInspectSwitch(InspectSwitch);
    }
    [Command]
    void CmdSetInspectSwitch(int _InspectSwitch){
        InspectSwitch = _InspectSwitch;
    }
    public int GetInspectSwitch(){
        InspectSwitch = PlayerPrefs.GetInt("InspectSwitch", 1);
        return InspectSwitch;
    }
    int ChallengeSwitch = 0;
    public void SetChallengeSwitch(){
        ChallengeSwitch = PlayerPrefs.GetInt("ChallengeSwitch", 1);
        CmdSetChallengeSwitch(ChallengeSwitch);

    }
    public int GetChallengeSwitch(){
        ChallengeSwitch = PlayerPrefs.GetInt("ChallengeSwitch", 1);
        return ChallengeSwitch;
    }
    [Command]
    void CmdSetChallengeSwitch(int _ChallengeSwitch){
        ChallengeSwitch = _ChallengeSwitch;
    }
    int TradeSwitch = 0;
    public void SetTradeSwitch(){
        TradeSwitch = PlayerPrefs.GetInt("TradeSwitch", 1);
        CmdSetTradeSwitch(TradeSwitch);
    }
    public int GetTradeSwitch(){
        TradeSwitch = PlayerPrefs.GetInt("TradeSwitch", 1);
        return TradeSwitch;
    }
    [Command]
    void CmdSetTradeSwitch(int _TradeSwitch){
        TradeSwitch = _TradeSwitch;
    }
    int TutorialSwitch = 0;
    public void SetTutorialSwitch(){
        TutorialSwitch = PlayerPrefs.GetInt("TutorialSwitch", 0);
    }
    public int GetTutorialSwitch(){
        TutorialSwitch = PlayerPrefs.GetInt("TutorialSwitch", 0);
        return TutorialSwitch;
    }
    public int GetTutorialTip(string TipName){
        int TipReturn = PlayerPrefs.GetInt(TipName, 0);
        if(TipReturn == 0){
            SetTutorialTip(TipName);
        }
        return TipReturn;
    }
    public bool GetTutorialOnetimeCheck(){
        int OneTimeSwitch = PlayerPrefs.GetInt("TutorialOneTimeSwitch", 0);
        if(OneTimeSwitch == 0){
            return false;
        } else {
            return true;
        }
    }
    void SetTutorialTip(string TipName){
        PlayerPrefs.SetInt(TipName, 1);
        PlayerPrefs.Save();
    }

    
    IEnumerator BuildSpriteInput(float timer){
        TOWNsRend.enabled = false;
        OVMsRend.enabled = false;
        TactName.enabled = false;
        backgroundSpriteUI.SetActive(false);
        yield return new WaitForSeconds(timer);
        if(afkStatus){
            TactName.text =  "(AFK) " + playerName;
        } else {
            TactName.text =  playerName;
        }
        if(loadSprite == "Male"){
            animator.SetBool("male", true);
        }
        if(loadSprite == "Female"){
            animator.SetBool("male", false);
        }
        animator.SetBool("started", true);
        RectTransform energyGO = energySlider.GetComponent<RectTransform>();
        Quaternion rotationEnergy = energySlider.gameObject.transform.rotation;
        if(rotationEnergy.x != 0f){
            rotationEnergy = Quaternion.Euler(0f, rotationEnergy.eulerAngles.y, rotationEnergy.eulerAngles.z);
            energySlider.gameObject.transform.rotation = rotationEnergy;
        }
        //if(currentScene == "OVM"){
        //    energyGO.transform.position = new Vector3(energyGO.transform.position.x, -0.294f, energyGO.transform.position.z);
        //} else {
        //    energyGO.transform.position = new Vector3(energyGO.transform.position.x, -0.5f, energyGO.transform.position.z);
        //}
        //if (currentScene == "OVM") {
        //   energyGO.anchoredPosition = new Vector2(energyGO.anchoredPosition.x, 0.294f);
        //} else {
        //    energyGO.anchoredPosition = new Vector2(energyGO.anchoredPosition.x, 0.5f);
        //}
        //if(!isLocalPlayer){
        //     print("Accessed the build sprite not on a local player");
        //     //energySlider.gameObject.transform.position = new Vector3(energySlider.gameObject.transform.position.x, energySlider.gameObject.transform.position.y, -0.56f);
        //     //energyGO.position = new Vector3(energyGO.position.x, energyGO.position.y, -0.56f);
        //     energyGO.anchoredPosition3D = new Vector3(-.4f, -2.26f, -0.56f);
        //} else {
        //    if(isLocalPlayer){
        //        if (currentScene == "OVM") {
        //            energyGO.anchoredPosition = new Vector2(energyGO.anchoredPosition.x, -2f);
        //        } else {
//      //              energyGO.anchoredPosition = new Vector2(energyGO.anchoredPosition.x, -0.5f);
        //           energyGO.anchoredPosition = new Vector2(energyGO.anchoredPosition.x, -2.26f);
        //        }
        //    }
        //}
        //if (currentScene == "OVM") {
        //    energyGO.anchoredPosition = new Vector2(energyGO.anchoredPosition.x, -0.294f);
        //} else {
        //   energyGO.anchoredPosition = new Vector2(energyGO.anchoredPosition.x, -2.26f);
        //}
        TOWNsRend.enabled = true;
        OVMsRend.enabled = true;
        TactName.enabled = true;
        backgroundSpriteUI.SetActive(true);
        if(!StatAsset.Instance.CheckForCombatZone(currentScene)){
            SceneChangeSprite(true);
        } else {
            SceneChangeSprite(false);
        }
        //if(spriteSwap != null){
        //    StopCoroutine(spriteSwap);
        //}
        //spriteSwap = StartCoroutine(SpriteSwapPlayer(loadSprite, currentScene));
    }
    [SyncVar]
    public bool afkStatus = false;
    public void ReturnedFromAFK(){
        CmdReturnedFromAFK();
    }
    [Command]
    void CmdReturnedFromAFK(){
        ServerReturnedFromAFK();
        
    }
    [Server]
    void ServerReturnedFromAFK(){
        afkStatus = false;
        RpcUpdateAFKStatus(false);
    }
    [ClientRpc]
    void RpcUpdateAFKStatus(bool afk){
        if(afk){
            TactName.text =  "(AFK) " + playerName;
        } else {
            TactName.text =  playerName;
        }
    }
    public void SetAFK(){
        CmdSetAFK();
    }
    [Command]
    void CmdSetAFK(){
        ServerSetAFK();
    }
    [Server]
    void ServerSetAFK(){
        afkStatus = true;
        RpcUpdateAFKStatus(true);
    }
    public override void OnStartClient()
    {
        base.OnStartClient();

           //StartCoroutine(BuildSpriteInput());
           //SpriteAnimatorSet();
        if(!isLocalPlayer){
            StartCoroutine(BuildSpriteInput(5f));

           //StartCoroutine(BuildSpriteInput());

            //if(spriteSwap != null){
            //    StopCoroutine(spriteSwap);
            //}
            //spriteSwap = StartCoroutine(SpriteSwapPlayer(loadSprite, currentScene));
            //Quaternion rotation = SpriteFlip.transform.rotation;
            //if(rotation.x == 0f){
            //    rotation = Quaternion.Euler(90f, rotation.eulerAngles.y, rotation.eulerAngles.z);
            //    SpriteFlip.transform.rotation = rotation;
            //}
            //if(currentScene == "OVM"){
            //    Quaternion rotation = SpriteFlip.transform.rotation;
            //    rotation = Quaternion.Euler(0f, rotation.eulerAngles.y, rotation.eulerAngles.z);
            //    SpriteFlip.transform.rotation = rotation;
            //}
            //if(currentScene == "TOWNOFARUDINE"){
            //    Quaternion rotation = SpriteFlip.transform.rotation;
            //    rotation = Quaternion.Euler(90f, rotation.eulerAngles.y, rotation.eulerAngles.z);
            //    SpriteFlip.transform.rotation = rotation;
            //}
            PlayFabClient.CenterCamera.AddListener(SpriteAnimatorSet);

        }
        if(isLocalPlayer){
            InitializePool();
            localPlayer = this;
            TutorialSwitch = PlayerPrefs.GetInt("TutorialSwitch", 0);

            PlayFabClient.CenterCamera.AddListener(SpriteAnimatorSet);
            CraftingSheet.RepairAllSend.AddListener(RepairAllItems);
            RepairPrefab.RepairItem.AddListener(RepairSingleItem);
            SalvageManager.RefundList.AddListener(SalvageRefundItems);
            InteractableObject.SewerRequest.AddListener(SewerRequestedByPlayer);
            CharacterSelectedScript.ReconfigureParty.AddListener(StartUpReconfig);
            PlayerCharacter.ResetSpells.AddListener(ClearTargetSpellChange);
            //MovingObject.MovedToCast.AddListener(CharacterCastingTargetSpell);
            PlayFabClient.CenterCamera.AddListener(ClearClicker);
            PlayFabClient.CenterCamera.AddListener(StartUpdateFogWorld);
            PlayFabClient.BlockCLicker.AddListener(BlockClicker);
            LevelUpPrefab.OnLevelUpStart.AddListener(LevelUpStartHandler);
            LevelUpPrefab.OnLevelUpComplete.AddListener(LevelUpCompleteHandler);
            UILobby.RemoveAdventurer.AddListener(RemoveCharacterFromAdventureList);
            UILobby.PickingAdventurer.AddListener(AddCharacterToAdventureList);
            ContainerPlayerUI.FinishedMatch.AddListener(CmdPermissionToEnd);
            ContainerPlayerUI.FinishedSewerMatch.AddListener(CmdPermissionToEndSewers);

            AbilityRankController.PurchaseSkillLevel.AddListener(CmdProcessSpellPurchase);
            CharacterSheet.ChangingSpellRequest.AddListener(CmdProcessSpellChange);
            CharacterTwoSheet.ChangingSpellRequest.AddListener(CmdProcessSpellChange);
            TacticianSheet.ChangingSpellRequest.AddListener(CmdProcessSpellChangeTact);
            Castbar.CastingSpell.AddListener(CastSpell);
            Castbar.CastingHarvest.AddListener(CastHarvest);
            ContainerUIButtons.RequestOVM.AddListener(RequestOVM);
            ContainerUIButtons.PurchaseRequest.AddListener(RequestPurchase);
            ContainerUIButtons.StackingItem.AddListener(StackItemSend);
            ContainerUIButtons.StashToTactInv.AddListener(StashToTactInventory);
            ContainerUIButtons.StashToTactEquip.AddListener(StashToTactEquipment);
            ContainerUIButtons.StashToTactBelt.AddListener(StashToTactbelt);
            ContainerUIButtons.StashToCharInv.AddListener(StashToCharInventory);
            ContainerUIButtons.StashToCharEquip.AddListener(StashToCharEquipment);
            ContainerUIButtons.TactInvStash.AddListener(TacticianInvToStash);
            ContainerUIButtons.TactInvToTactEquip.AddListener(TacticianInvToTactEquip);
            ContainerUIButtons.TactInvToTactBelt.AddListener(TacticianInvToTactBelt);
            ContainerUIButtons.TactInvToCharInv.AddListener(TacticianInvToCharInv);
            ContainerUIButtons.TactInvToCharEquip.AddListener(TactInvToCharEquipment);
            ContainerUIButtons.TactEquipStash.AddListener(TacticianEquipToStash);
            ContainerUIButtons.TactEquipToTactInv.AddListener(TacticianEquipToTactInv);
            ContainerUIButtons.TactEquipToTactEquip.AddListener(TacticianEquipToTactEquip);
            ContainerUIButtons.TactEquipToTactBelt.AddListener(TacticianEquipToTactBelt);
            ContainerUIButtons.TactEquipToCharInv.AddListener(TacticianEquipToCharInv);
            ContainerUIButtons.TactBeltStash.AddListener(TacticianBeltToStash);
            ContainerUIButtons.TactBeltToTactInv.AddListener(TacticianBeltToTactInv);
            ContainerUIButtons.TactBeltToTactEquip.AddListener(TacticianBeltToTactEquip);
            ContainerUIButtons.TactBeltToCharInv.AddListener(TacticianBeltToCharInv);
            ContainerUIButtons.TactBeltToCharEquip.AddListener(TacticianBeltToCharEquip);

            ContainerUIButtons.CharInvToStash.AddListener(CharacterInvToStash);
            ContainerUIButtons.CharInvToTactInv.AddListener(CharacterInvToTactInventory);
            ContainerUIButtons.CharInvToTactBelt.AddListener(CharacterInvToTactBelt);
            ContainerUIButtons.CharInvToTactEquip.AddListener(CharacterInvToTactEquip);
            ContainerUIButtons.CharEquipToStash.AddListener(CharacterEquipToStash);
            ContainerUIButtons.CharEquipToTactInv.AddListener(CharacterEquipToTacticianInv);
            ContainerUIButtons.CharEquipToTactBelt.AddListener(CharacterEquipToTacticianBelt);
            ContainerUIButtons.DestroyThisItem.AddListener(DestroyingItem);
            ContainerUIButtons.ConsumeAll.AddListener(ConsumingItemFully);
            ContainerUIButtons.ConsumeSome.AddListener(ConsumingItemPartially);
            ContainerUIButtons.CharInvToCharInv.AddListener(CharacterInvToCharInv);
            ContainerUIButtons.CharInvToCharEquip.AddListener(CharacterInvToCharEquip);
            ContainerUIButtons.CharEquipToCharEquip.AddListener(CharacterEquipToCharEquip);
            ContainerUIButtons.CharEquipToCharInv.AddListener(CharacterEquipToCharInv);
            ContainerUIButtons.CharEquipToCharEquipSame.AddListener(CharacterEquiptoEquipSame);
            ContainerUIButtons.CharEquipToCharInvSame.AddListener(CharacterEquiptoInvSame);
            ContainerUIButtons.CharInvToCharEquipSame.AddListener(CharacterInvtoEquipSame);
            ContainerUIButtons.CharOneSWAPUnequipEquip.AddListener(CharacterOneUnequipToCharEquip);
            ContainerUIButtons.CharTwoSWAPUnequipEquip.AddListener(CharacterTwoUnequipToCharEquip);

            ContainerUIButtons.CharOneUnequipEquipSendCharTwo.AddListener(CharacterOneUnequipToCharEquipSendTwo);
            ContainerUIButtons.CharTwoUnequipEquipSendCharOne.AddListener(CharacterTwoUnequipToCharEquipSendOne);

            ContainerUIButtons.CharOneSWAPTactStashUnequipEquip.AddListener(CharacterUnequipToTactStash);
            ContainerUIButtons.CharTwoSWAPTactStashUnequipEquip.AddListener(CharacterUnequipToTactStash);
            ContainerUIButtons.BuildingNewStackItem.AddListener(CreateNewStackItem);
            //ContainerUIButtons.CharToTact.AddListener(StashToTactInventory);
            //ContainerUIButtons.TactToChar.AddListener(StashToTactInventory);
           // ContainerUIButtons.CharToChar.AddListener(StashToTactInventory);
            //ContainerPlayerUI.PartyRemove.AddListener(RemovePartyMember);
            ContainerPlayerUI.CharacterFinalize.AddListener(RequestToBuild);
            ContainerPlayerUI.SendingNeedRoll.AddListener(RollNeed);
            ContainerPlayerUI.SendingGreedRoll.AddListener(RollGreed);
            ContainerPlayerUI.SendingPassRoll.AddListener(RollPass);
            ContainerUIButtons.HealParty.AddListener(HealPartyRequest);
            CharacterSelectedScript.PartyMemberAdd.AddListener(AddPartyMember);
            CharacterSelectedScript.RemoveFromParty.AddListener(RemovePartyMember);
            DeathPrefab.ResFromDragon.AddListener(ProcessResRequest);
            PlayFabClient.OurNodeSet.AddListener(FindOurNode);
            cameraController = GameObject.Find("Camera Controller").transform;
            //UIEnergyToggle();
            //inventory = new InventoryManager();
            //uI_Inventory = GameObject.FindGameObjectWithTag("TacticianInventory").GetComponent<UI_Inventory>();
            //uI_Inventory.SetInventory(inventory);
            //LeavingTown.Invoke();
            Loading = false;
            StartCoroutine(BuffManagerRoutine());
            //if(currentScene == null || currentScene == string.Empty)
            //{
            //    return;
            //}
            
            //if(currentScene == "OVM"){
            //    //LeavingTown.Invoke();
            //    StartCoroutine(LocalSprite());
            //}
            //TacticianInventory.Callback += OnEquipmentChange;
            // Process initial SyncDictionary payload
            //foreach (KeyValuePair<string, ItemSelectable> kvp in TacticianInventory)
           //OnEquipmentChange(SyncDictionary<string, ItemSelectable>.Operation.OP_ADD, kvp.Key, kvp.Value);
            StartCoroutine(SendRpcQueueMovement());
            StartCoroutine(BuildSpriteInput(4f));


        } 
        
    }
    public override void OnStopClient(){
        Debug.Log($"Client Stopped");
        UntargetPlayer.Invoke(this);
        //ClientDisconnect();
    }
    public override void OnStopServer(){
        //NetworkServer.OnDisconnectedEvent.Invoke(connectionToClient);
        Debug.Log($"Client Stopped on Server");
        //ServerDisconnect();
        //Logout();
    }
    //IEnumerator LocalSprite(){
    //    SpriteRenderer sRend = GetComponent<SpriteRenderer>();
    //    if(loadSprite == null || loadSprite == string.Empty)
    //    {
    //        while(loadSprite == null || loadSprite == string.Empty)
    //        {
    //            yield return null;
    //        }
    //    }
    //    sRend.sprite = Load("Player0", loadSprite);
    //    if(sRend.sprite == null)
    //    {
    //        while(sRend.sprite == null)
    //        {
    //            yield return null;
    //        }
    //    }
    //    sRend.enabled = true;
    //}
    
    public void SpawnCharacterEnterUI(){
        ClientCharacterEnterSpawn();
    }
    [ClientRpc]
    void ClientCharacterEnterSpawn(){
        SpawnClientCharacterEnterUI();
    }
    void SpawnClientCharacterEnterUI(){
        // figure out which character this is supposed to be
    }
    [ClientRpc]
    public void RpcSpawnPlayerUI(string TacticianSprite, Match match){
        if(currentMatch == match || ScenePlayer.localPlayer == this){
            SpawnClientUI(TacticianSprite);
        }
    }
    void SpawnClientUI(string TacticianSprite){
        //use match to get all the players but be sure to clear them first in the instance of ui lobby on the player parent
            UILobby.instance.SpawnPlayerUIAlltogether(this, TacticianSprite);
    }
   [Server]
    public void UpdateQuestProgress(string questID, string trigger)
    {
        print("UpdateQuestProgress our quest " + questID + " and trigger data " + trigger);
        for (int i = 0; i < QuestInProgressData.Count; i++)
        {
            if (QuestInProgressData[i].questID == questID)
            {
                if (QuestInProgressData[i].objectives.Contains(trigger))
                {
                    QuestInProgressData[i].objectives.Remove(trigger); // Remove the objective
                    TargetUpdateQuestProgress(questID, trigger);
                    if(QuestInProgressData[i].objectives.Count == 0){ //QuestInProgressData[i].objectives.Count == 1 || 
                        print(QuestInProgressData[i].objectives.Count + " was our objective count for " + questID);
                        if(!FinishedQuestsReadyToTurnIn.Contains(questID)){
                            FinishedQuestsReadyToTurnIn.Add(questID);
                            print("Added " + questID + " to the finished quest list");
                        }
                        //foreach(var check in QuestInProgressData[i].objectives){
                        //    if(string.IsNullOrEmpty(check)){
                        //       if(!FinishedQuestsReadyToTurnIn.Contains(questID)){
                        //            FinishedQuestsReadyToTurnIn.Add(questID);
                        //            print("Added " + questID + " to the finished quest list");
                        //        }
                        //        break;
                        //    }
                        //}
                    }
                    print("Removed objective: " + trigger);
                }
                break; // Exit the loop as the quest has been found and updated
            }
        }
    } 
    [TargetRpc]
    public void TargetUpdateQuestProgress(string questID, string trigger)
    {
        for (int i = 0; i < QuestInProgressData.Count; i++)
        {
            if (QuestInProgressData[i].questID == questID)
            {
                if (QuestInProgressData[i].objectives.Contains(trigger))
                {
                    QuestInProgressData[i].objectives.Remove(trigger); // Remove the objective
                    if(QuestInProgressData[i].objectives.Count == 0){ //QuestInProgressData[i].objectives.Count == 1 || 
                        print(QuestInProgressData[i].objectives.Count + " was our objective count for " + questID);
                        if(!FinishedQuestsReadyToTurnIn.Contains(questID)){
                            FinishedQuestsReadyToTurnIn.Add(questID);
                            QuestFinished.Invoke(questID);
                            Speaker.Invoke(253);
                            PingUpdate.Invoke("Quest");
                            Quest ourQuest = QuestLog.Instance.GetQuestByName(questID);
                            BuildFinishQuest($"{ourQuest.Name} is completed go speak with {ourQuest.FinishNPC}!");
                            print("Added " + questID + " to the finished quest list");
                        }
                        //foreach(var check in QuestInProgressData[i].objectives){
                        //    if(string.IsNullOrEmpty(check)){
                        //       if(!FinishedQuestsReadyToTurnIn.Contains(questID)){
                        //            FinishedQuestsReadyToTurnIn.Add(questID);
                        //            print("Added " + questID + " to the finished quest list");
                        //        }
                        //        break;
                        //    }
                        //}
                    }
                    print("Removed objective: " + trigger);
                    QuestSheet.Instance.RefreshQuestObjectives(questID, trigger);
                    Speaker.Invoke(252);
                    //QuestObjectiveComplete.Instance.gameObject.SetActive(true);
                    //QuestObjectiveComplete.Instance.Title.text = "-" + trigger;
                    //QuestObjectiveComplete.Instance.SetTimer(2000);
                    ObjectiveUpdated.Invoke(6000, trigger);
                    ObjectiveUpdatedQSheet.Invoke();
                    PingUpdate.Invoke("Quest");

                    
                }
                break; // Exit the loop as the quest has been found and updated
            }
        }
    }
    void BuildFinishQuest(string quest){
        StartCoroutine(QuestCompleted(quest));
    }
    IEnumerator QuestCompleted(string quest){
        yield return new WaitForSeconds(8f);
        ObjectiveUpdated.Invoke(6000, quest);
        ObjectiveUpdatedQSheet.Invoke();

    }
    public void QuestTriggeredData(string questID, string trigger){
        print("Updating our quest " + questID + " and trigger data " + trigger);
        CmdQuestTriggeredData(questID, trigger);
    }
    [Command]
    public void CmdQuestTriggeredData(string questID, string trigger){
        print("CmdQuestTriggeredData our quest " + questID + " and trigger data " + trigger);
        UpdateQuestProgress(questID, trigger);
    }
    void OnTriggerEnter2D(Collider2D other){
        SceneNode node = other.GetComponent<SceneNode>();
        QuestExploreTownEnviornment questTrigger = other.GetComponent<QuestExploreTownEnviornment>();
        if(isServer){
            if(node)
            {
                //print($"Player object landed on node {other.gameObject.name}");
                currentNode = other.gameObject.name;
                RandomRepel = true;
            }
            if(questTrigger){
                string trigger = questTrigger.GetQuestTrigger();
                string questID = questTrigger.GetQuestID();
                if(QuestProgression.Contains(questID)){
                    return;
                }
                UpdateQuestProgress(questID, trigger);
            }
        }
        if(isLocalPlayer){
            if(node)
            {
                currentNode = node.nodeName;
                print($"Landed on Node: {other.gameObject.name}");
                //StopMovementOnServer();
            }
        }
    }
    void OnTriggerStay2D(Collider2D other){
        SceneNode node = other.GetComponent<SceneNode>();
        if(isServer){
            if(node)
            {
                //print($"Player object landed on node {other.gameObject.name}");
                currentNode = other.gameObject.name;
                RandomRepel = true;
            }
        }
        if(isLocalPlayer){
            if(node)
            {
                currentNode = node.nodeName;
                //print($"Landed on Node: {other.gameObject.name}");
            }
        }
    }
    void OnTriggerExit2D(Collider2D other){
        SceneNode node = other.GetComponent<SceneNode>();
        if(isServer)
        {
            if(node)
            {
                //print($"Player object landed on node {other.gameObject.name}");
                currentNode = "Exploring";
                RandomRepel = false;
            }
        }
        if(isLocalPlayer){
            if(node)
            {
                currentNode = "Exploring";
            }
        }
    }
    void OnStartTheGameSoloExit2D(Collider2D collider){
        if(isServer)
        {
            currentNode = "Exploring";
            return;
        }
        if(isLocalPlayer){
            // maninipulate menu now 
        //    //print($"Left Node: {collider.gameObject.name}");
            //StartCoroutine(EagleHasLanded(other.gameObject.name));
        }
    }
    public void TownRequest(){
        if(TradeSupervisor.instance.GetTradeWindow()){
            ImproperCheckText.Invoke($"Cannot do that while trading");
            return;
        }
        MoveRequest.Invoke("TOWNOFARUDINE", currentScene);
    }
    bool ValidCity(){
        if(!string.IsNullOrEmpty(currentNode)){
            if(currentNode == "City of Elves"){
                return true;
            }
            if(currentNode == "City of Gnome's"){
                return true;
            }
            if(currentNode == "City of Dwarve's"){
                return true;
            }
            if(currentNode == "City of Faerie's"){
                return true;
            }
            if(currentNode == "City of Dragon's"){
                return true;
            }
            if(currentNode == "City of Orc's"){
                return true;
            }
            if(currentNode == "City of Lizard's"){
                return true;
            }
            if(currentNode == "City of Giants"){
                return true;
            }
        }
        return false;
    }
    public void CityRequest(){
        if(ValidCity()){
            if(TradeSupervisor.instance.GetTradeWindow()){
                ImproperCheckText.Invoke($"Cannot do that while trading");
                return;
            }
            ImproperCheckText.Invoke($"{currentNode} is not available to enter yet, but it is coming soon!");
            return;
            MoveRequest.Invoke(currentNode, currentScene);
        }
    }
    bool ValidOutpost(){
        if(!string.IsNullOrEmpty(currentNode)){
            if(currentNode == "Outpost1"){
                return true;
            }
        }
        return false;
    }
    public void OutpostRequest(){
        if(ValidOutpost()){
            if(TradeSupervisor.instance.GetTradeWindow()){
                ImproperCheckText.Invoke($"Cannot do that while trading");
                return;
            }
            ImproperCheckText.Invoke($"{currentNode} is not available to enter yet, but it is coming soon!");
            return;
            MoveRequest.Invoke(currentNode, currentScene);
        }
    }
    // we need a mode to decipher if move grid should be shown or spell grid
    
    //if(spell == "Turn Undead"){
		//    curatorTM.UpdatePlayerCastedOffensiveSpell(match, this, target, spell, _spellRank, cost);
		//    return;
		//}
		//if(spell == "Undead Protection"){
		//    curatorTM.UpdatePlayerCastedOffensiveSpell(match, this, target, spell, _spellRank, cost);
		//    return;
		//}
		//if(spell == "Blizzard"){
		//    curatorTM.UpdatePlayerCastedOffensiveSpell(match, this, target, spell, _spellRank, cost);
		//    return;
		//}
		//if(spell == "Fireball"){
		//    curatorTM.UpdatePlayerCastedOffensiveSpell(match, this, target, spell, _spellRank, cost);
		//    return;
		//}
		//if(spell == "Group Heal"){
		//    curatorTM.UpdatePlayerCastedOffensiveSpell(match, this, target, spell, _spellRank, cost);
		//    return;
		//}
		//if(spell == "Ice Blast"){
		//    curatorTM.UpdatePlayerCastedOffensiveSpell(match, this, target, spell, _spellRank, cost);
		//    return;
		//}
		//if(spell == "Meteor Shower"){
		//    curatorTM.UpdatePlayerCastedOffensiveSpell(match, this, target, spell, _spellRank, cost);
		//    return;
		//}
		//if(spell == "Intimidating Roar"){
		//    curatorTM.UpdatePlayerCastedOffensiveSpell(match, this, target, spell, _spellRank, cost);
		//    return;
		//}

    
    [Command]
    void CmdCancelSpell(MovingObject pc){
        ServerCancelSpell(pc);
    }
    [Server]
    void ServerCancelSpell(MovingObject pc){
        pc.Casting = false;
        foreach(var player in currentMatch.players){
            TargetCancelSpell(pc);
        }
    }
    [TargetRpc]
    public void TargetCancelSpell(MovingObject pc){
        CancelCast.Invoke(pc);
        //foreach(var key in castingBars){
        //    if(pc == key.Key){
        //        key.Value.CancelingCast(pc);
        //        castingBars.Remove(pc);
        //    }
        //}
    }
    public bool isDragging = false;
    Vector2 startPoint;
    public float clickThreshold = 0.1f;
    private Vector2 boxStart;
    private Vector2 boxEnd;
    List<GameObject> selectedCharacters = new List<GameObject>();
    public bool CheckForTownZone(){
        bool checkZone = false;
        if(currentScene == "TOWNOFARUDINE"){
            checkZone = true;
        }
        return checkZone;
    }
    
    public Coroutine abilityOneCDRoutine;
    public Coroutine abilityTwoCDRoutine;
    public IEnumerator SetAbilityCoolDownOne(float duration){
        TactSpellOne = true;
        CooldownSpellOne = duration;
        while (CooldownSpellOne > 0f)
        {
            CooldownSpellOne -= Time.deltaTime;
            yield return null;
        }
        CooldownSpellOne = 0f;
        TactSpellOne = false;
        abilityOneCDRoutine = null;
    }
    public IEnumerator SetAbilityCoolDownTwo(float duration){
        TactSpellTwo = true;
        CooldownSpellTwo = duration;
        while (CooldownSpellTwo > 0f)
        {
            CooldownSpellTwo -= Time.deltaTime;
            yield return null;
        }
        CooldownSpellTwo = 0f;
        TactSpellTwo = false;
        abilityTwoCDRoutine = null;
    }
    public IEnumerator SetRepelRoutine(float duration){
        repelOn = true;
        while (duration > 0f)
        {
            duration -= Time.deltaTime;
            yield return null;
        }
        repelOn = false;
        repelRoutine = null;
    }
    Coroutine repelRoutine;
    bool repelOn = false;
    [Command]
    void CmdCastedSpellOutCombat(int keyPressed){
        string toBeSent = string.Empty;
        if(keyPressed == 1){
            toBeSent = SpellOne;
        }
        if(keyPressed == 2){
            toBeSent = SpellTwo;
        }
        if(toBeSent == "Repel"){
            if(repelRoutine != null){
                StopCoroutine(repelRoutine);
            }
            repelRoutine = StartCoroutine(SetRepelRoutine(120f));
        }
        if(toBeSent == "Rest"){

        }
        if(toBeSent == "Fertile Soil"){
            
        }
        ServerBuildTacticianSpellCooldown(keyPressed);
    }
    [Server]
    void  ServerClientLightSpell(PlayerCharacter pc){
        if(pc == null){
            return;
        }
        int energyCost = GetTactEnergyCost("Light");
        if(energyCost > 0){
            Energy -= energyCost;
            if(Energy < 0){
                Energy = 0;
            }
            TargetUpdateEnergyDisplay(Energy);
        }
        int lvl = 1;
        for(int o = 0; o < GetTacticianSheet().TacticianStatData.Count; o++){
            if(GetTacticianSheet().TacticianStatData[o].Key == "LVL"){
                lvl = int.Parse(GetTacticianSheet().TacticianStatData[o].Value);
                break;
            }
        }
        pc.SetClientLightSpell(false, lvl);
        //string attacker = string.Empty;
        //string defender = string.Empty;
        //string content = "Light";
        //string amount = "5" + "_" + "minutes";
        //attacker = playerName;
        //defender = pc.CharacterName;
        //CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, 3, 3, false);
        //pc.RpcSpawnCombatLog(cNet);
        string spell = "Light";
        if(spell == SpellOne){
            ServerBuildTacticianSpellCooldown(1);
        }
        if(spell == SpellTwo){
            ServerBuildTacticianSpellCooldown(2);
        }
        
    }
    [Command]
    void CmdBuildSpellSelected(MovingObject targetIfAny, int keyPressed, int type){
        string toBeSent = string.Empty;
        if(keyPressed == 1){
            toBeSent = SpellOne;
        }
        if(keyPressed == 2){
            toBeSent = SpellTwo;
        }
        if(StatAsset.Instance.CheckForCombatZone(currentScene)){
            if(targetIfAny != null){
                PlayerCharacter pcCheck = targetIfAny.GetComponent<PlayerCharacter>();

                if(pcCheck != null){
                    if(toBeSent == "Light"){
                        ServerClientLightSpell(pcCheck);
                        return;
                    }
                }
            }
            if(type == 1){
                if(targetIfAny != null){

                if(FriendlyList.Contains(targetIfAny)){
                    TargetSendFailureMessageToClient($"Must cast this on a enemy target");
                    return;
                }
                TactDamageSpell.Invoke(currentMatch, this, targetIfAny, toBeSent);
                }

            }
            if(type == 2){
                
                if(targetIfAny != null){
                    if(!FriendlyList.Contains(targetIfAny)){
                        TargetSendFailureMessageToClient($"Must cast this on a friendly target");
                        return;
                    }
                    TactHealSpell.Invoke(currentMatch, this, targetIfAny, toBeSent);
                }
            }
            if(type == 3){
                if(targetIfAny != null){
                    if(!FriendlyList.Contains(targetIfAny) && toBeSent != "Stun"){
                        TargetSendFailureMessageToClient($"Must cast this on a friendly target");
                        return;
                    }
                    TactBuffSpell.Invoke(currentMatch, this, targetIfAny, toBeSent);
                }
            }
            if(type == 10){
                if(targetIfAny != null){
                    if(FriendlyList.Contains(targetIfAny)){
                        TargetSendFailureMessageToClient($"Must cast this on a enemy target");
                        return;
                    }
                    TactBuffSpell.Invoke(currentMatch, this, targetIfAny, toBeSent);
                }
            }
        }
    }
    public void RemovePossibleClickSpell(){
        if(SavedClick != null){
            Destroy(SavedClick.radiusIndicator);
            SavedClick = null;
        }
    }
    void Update()
    {   
        if(isServer){
            return;
        }
        energySlider.value = Energy;
    	timeSinceLastUpdate += Time.deltaTime;
        cameraUpdate += Time.deltaTime;
        if(!isLocalPlayer){ 
            if(IsMoving()){
                if(animator.enabled){
                    if(animator.runtimeAnimatorController != null && animator.GetBool("stand")){
                        animator.SetBool("stand", false);
                    }
                    if(animator.runtimeAnimatorController != null && !animator.GetBool("stand")){
                        Vector2 direction = (transform.position - lastPositionClientMovement).normalized;
                        SetAnimatorState(GetDirectionValue(direction)); 
                    }   
                }
            } else {
                if(animator.enabled){
                    if(animator.runtimeAnimatorController != null && !animator.GetBool("stand")){
                        NonRPCSetDirection("Standing");
                    }   
                }
            }
            if(currentScene == "OVM"){
                if(timeSinceLastUpdate >= updateCooldown && CanUpdateOVM){
                    GameObject energyGO = energySlider.gameObject;
                    UpdateFogWorldOtherPlayer(SpriteFlip, energyGO);
			    	timeSinceLastUpdate = 0;
			    }
            }
            lastPositionClientMovement = transform.position;
            return; 
        }
        if(Loading){
            return;
        }
        if(!playerOVMReady){
            return;
        }
        if(ScenePlayer.localPlayer == null){
            return;
        }
        //if (Input.GetKeyDown(KeyCode.Alpha9) && !ContainerUIButtons.Instance.GetChat()){
        //    CmdDoubleCheckBTN();
        //}
        if(!StatAsset.Instance.CheckForCombatZone(currentScene)){
            if (Input.GetKeyDown(KeyCode.Escape)){
                if(SavedClick != null){
                    Destroy(SavedClick.radiusIndicator);
                    SavedClick = null;
                }
                ClearTargetNonCombat.Invoke();
            }
        }   
        if (Input.GetKeyDown(KeyCode.I) && StatAsset.Instance.CheckForCombatZone(currentScene) && !ContainerUIButtons.Instance.GetChat())
		{
            string id = string.Empty;
            MovingObject selected = CombatPartyView.instance.GetSelected();
            if(selected){
                PlayerCharacter pc = selected.GetComponent<PlayerCharacter>();
                if(pc){
                    if(!pc.GetComponent<NetworkIdentity>().hasAuthority){
                        return;
                    }
                    id = pc.CharID;
                } else {
                    return;
                }
            } else {
                return;
            }
            if(string.IsNullOrEmpty(id)){
                print($"Null or empty ID for {id}");
                return;
            }
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)){
                //ShiftX char sheet two
                if(CharacterTwoSheet.Instance.GetSerial() == id){
                    if(CharacterTwoSheet.Instance.GetComponent<Canvas>().enabled){
                        CharacterTwoSheet.Instance.ClosedSheet();
                        return;
                    } else {
                        OpenCharSheetTwoCombat.Invoke(id);
                        return;
                    }
                } else {
                    OpenCharSheetTwoCombat.Invoke(id);
                }
            } else {
                if(CharacterSheet.Instance.GetSerial() == id){
                    if(CharacterSheet.Instance.GetComponent<Canvas>().enabled){
                        CharacterSheet.Instance.ClosedSheet();
                        return;
                    } else {
                        OpenCharSheetOneCombat.Invoke(id);
                        return;
                    }
                } else {
                    OpenCharSheetOneCombat.Invoke(id);
                }
            }

        }
        /*
        if (Input.GetKeyDown(KeyCode.Alpha1) && !StatAsset.Instance.CheckForCombatZone(currentScene) && !ContainerUIButtons.Instance.GetChat())
		{
            if(TactSpellOne){
                ImproperCheckText.Invoke($"Spell is on cooldown");
                return;
            }
            if(SpellOne == "Empty"){
                return;
            }
            if(SpellOne == "Rest" || SpellOne == "Repel"){
                CmdCastedSpellOutCombat(1);
                return;
            }
            if(SpellOne == "Fertile Soil"){
                CmdCastedSpellOutCombat(1);
                return;
            }
            ImproperCheckText.Invoke($"This spell does not work here");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && !StatAsset.Instance.CheckForCombatZone(currentScene) && !ContainerUIButtons.Instance.GetChat())
		{
            if(TactSpellTwo){
                ImproperCheckText.Invoke($"Spell is on cooldown");
                return;
            }
            if(SpellTwo == "Empty"){
                ImproperCheckText.Invoke($"Equip a spell here first");
                return;
            }
            if(SpellTwo == "Rest" || SpellTwo == "Repel"){
                CmdCastedSpellOutCombat(2);
                return;
            }
            if(SpellTwo == "Fertile Soil"){
                CmdCastedSpellOutCombat(2);
                return;
            }
            ImproperCheckText.Invoke($"This spell does not work here");
        }
        */
        if (Input.GetKeyDown(KeyCode.Alpha1) && StatAsset.Instance.CheckForCombatZone(currentScene) && !ContainerUIButtons.Instance.GetChat())
		{
            PressedTactOneCombat();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && StatAsset.Instance.CheckForCombatZone(currentScene) && !ContainerUIButtons.Instance.GetChat())
		{
            PressedTactTwoCombat();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) && StatAsset.Instance.CheckForCombatZone(currentScene) && !ContainerUIButtons.Instance.GetChat())
		{
            PressedPotionCombat();
		}
        if (Input.GetKeyDown(KeyCode.Alpha4) && StatAsset.Instance.CheckForCombatZone(currentScene) && !ContainerUIButtons.Instance.GetChat())
		{
            PressedFoodCombat();
		}
        if (Input.GetKeyDown(KeyCode.Q) && StatAsset.Instance.CheckForCombatZone(currentScene) && !ContainerUIButtons.Instance.GetChat())
		{
            if(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)){
                PressedAltQCombat();
            } else {
                PressedQCombat();
            }
		}
        if (Input.GetKeyDown(KeyCode.E) && StatAsset.Instance.CheckForCombatZone(currentScene) && !ContainerUIButtons.Instance.GetChat())
		{
            if(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)){
                PressedAltECombat();
            } else {
                PressedECombat();
            }
        }
        if (Input.GetKeyDown(KeyCode.R) && StatAsset.Instance.CheckForCombatZone(currentScene) && !ContainerUIButtons.Instance.GetChat())
		{
            if(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)){
                PressedAltRCombat();
            } else {
                PressedRCombat();
            }
        }
        if (Input.GetKeyDown(KeyCode.F) && StatAsset.Instance.CheckForCombatZone(currentScene) && !ContainerUIButtons.Instance.GetChat())
		{
            if(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)){
                PressedAltFCombat();
            } else {
                PressedFCombat();
            }
        }
        if(StatAsset.Instance.CheckForCombatZone(currentScene)){
            if(Input.GetMouseButtonDown(1)){
                RightClickCombat();
            }
            if(Input.GetKeyDown(KeyCode.Z) && !ContainerUIButtons.Instance.GetChat()){
                if(SavedClick != null){
                    Destroy(SavedClick.radiusIndicator);
                    SavedClick = null;
                }
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)){
                    CancelAllCasts();
                } else {
                    CancelLastSelectedCast();
                }
            }
            if (Input.GetKeyDown(KeyCode.Space) && !ContainerUIButtons.Instance.GetChat()){
                if(SavedClick != null){
                    Destroy(SavedClick.radiusIndicator);
                    SavedClick = null;
                }
                ClearTarget();
                TargetHighlightReset.Invoke();
                NewTarget.Invoke();
            }
            if (Input.GetKeyDown(KeyCode.Escape)){
                
                RemoveTarget();
                NewTarget.Invoke();
            }
            bool aoeClick = false;
            if (Input.GetMouseButtonDown(0))
            {
                if(SavedClick == null){
                    LeftClickDown();
                } else {
                    if(HaveAoeSpellClickSaved()){
                        aoeClick = true;
                    }
                } 
            }
            if (Input.GetMouseButtonUp(0) && !aoeClick)
            {
                LeftClickUp();
            }
            if(SavedClick != null){
                startPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition); // changed to Input.mousePosition
                    //print($"Set start: {startPoint}");

                //print("Dragging started");
                boxStart = Input.mousePosition;
                    //print($"Set boxStart: {boxStart}");
            }
            if (isDragging)
            {
                boxEnd = Input.mousePosition;
            }
        } else {
            Vector2 checkPos = Input.mousePosition;
            RaycastHit2D hitMiniMap = Physics2D.Raycast(checkPos, Vector2.zero, 0f, LayerMask.GetMask("MiniMap"));
            bool miniHit = false;
            if (hitMiniMap.collider != null){
                miniHit = true;
            }
            if(miniHit){
                return;
            }
            if(IsMoving()){
                if(animator.enabled){
                    Vector2 direction = (transform.position - lastPositionClientMovement).normalized;
                    SetAnimatorState(GetDirectionValue(direction)); 
                }
            } else {
                if(animator.enabled){
                    if(animator.runtimeAnimatorController != null && !animator.GetBool("stand")){
                        NonRPCSetDirection("Standing");
                    }   
                }
            }
            MovePlayer();
            if(timeSinceLastUpdate >= updateCooldown){
                if(currentScene == "OVM" && CanUpdateOVM){
                    UpdateFogWorld(transform.position);
                }
				timeSinceLastUpdate = 0;
			}
            lastPositionClientMovement = transform.position;
        }
    }
    bool IsMoving()
{
    if(transform.position == lastPositionClientMovement){
        return false;
    } else {
        return true;
    }
    //float distanceMoved = Vector3.Distance(transform.position, lastPositionClientMovement);
    //return distanceMoved > 0.01f; // Adjust the threshold as needed
}
    private float updateCooldown = .1f; // 1 second cooldown
    private float timeSinceLastUpdate = 0.0f;
    Vector3 lastPositionClientMovement;
    bool clientStopped = true;
    public Canvas ConnectCanvas;
    void CancelLastSelectedCast(){
        MovingObject lastSelected = CombatPartyView.instance.GetSelected();
        if(lastSelected != null){
            if(lastSelected.GetComponent<NetworkIdentity>().hasAuthority){
                lastSelected.CmdCancelSpell();
            }
        }
    }
    void CancelAllCasts(){
        CancelAllCastsOwned.Invoke(this);
    }
    
    public void RemoveSelectedCharacter(GameObject deadChar){
        if(selectedCharacters.Contains(deadChar)){
            selectedCharacters.Remove(deadChar);
        }
    }
    public void CheckAuthorityOfCharacter(MovingObject character, MovingObject MobInstance){
        if(character.GetComponent<NetworkIdentity>().hasAuthority){
            foreach(var selected in selectedCharacters){
                MovingObject charCheck = selected.GetComponent<MovingObject>();
                if(charCheck.Target != null){
                    if(charCheck.Target == MobInstance){
                        charCheck.CmdRemoveTarget();
                    }
                }
            }
        }
    }
    void RemoveTarget(){
        foreach(var character in selectedCharacters){
            MovingObject pc = character.GetComponent<MovingObject>();
            pc.UnTargettedMO();
            pc.CmdClearAllStopMoving();
        }
        //if(TargetMob != null){
        //    TargetMob.UnTargettedMO();
        //    TargetMob = null;
        //}
    }
    public void ClearTarget(){
//        print("Clearing selected and unselecting");
        foreach(var character in selectedCharacters){
            MovingObject pc = character.GetComponent<MovingObject>();
            pc.UnselectedMO();
            pc.UnTargettedMO();
            pc.UnselectedUnit();
        }
        //if(SelectedMob != null){
        //    SelectedMob.UnselectedMO();
        //    SelectedMob = null;
        //}
        //if(TargetMob != null){
        //    TargetMob.UnTargettedMO();
        //    TargetMob = null;
        //}
        selectedCharacters.Clear();
        CombatPartyView.instance.TurnOffSelectedWindow();
        ToggleSpellsOff.Invoke();
    }
    [Command]
    void CmdMoveUnits(List<MovingObject> characters, Vector2 targetPos, string mode){
        MoveUnits movementMsg = new MoveUnits(targetPos, characters, currentMatch, mode, this);
        OnCharactersMoved.Invoke(movementMsg);
    }
    [Command]
    void CmdFollowFriendlyUnit(List<MovingObject> characters, MovingObject target){
        OnCharactersFollow.Invoke(target, characters, currentMatch);
    }
    [Command]
    void CmdAttackUnit(List<MovingObject> characters, MovingObject target){
        List<MovingObject> ownedchars = new List<MovingObject>();
        foreach(var mo in characters){
            if(mo.GetComponent<NetworkIdentity>().hasAuthority){
                ownedchars.Add(mo);
            }
        }
        OnCharactersAttack.Invoke(target, characters, currentMatch);
    }
    void OnGUI()
{
    if (isDragging)
    {
        // Create a rect object
        Rect rect = GetScreenRect(boxStart, boxEnd); // use GetScreenRect

        // Draw the rect
        GUI.color = new Color(1, 1, 1, 0.5f);
        GUI.Box(rect, "", new GUIStyle { normal = new GUIStyleState { background = Texture2D.whiteTexture } });
        GUI.color = Color.white;
    }
}
void clearUI(){
    isDragging = false;

}

private Rect GetScreenRect(Vector2 screenPosition1, Vector2 screenPosition2)
{
    // Move origin from bottom-left to top-left
    screenPosition1.y = Screen.height - screenPosition1.y;
    screenPosition2.y = Screen.height - screenPosition2.y;
    // Calculate corners
    Vector2 topLeft = Vector2.Min(screenPosition1, screenPosition2);
    Vector2 bottomRight = Vector2.Max(screenPosition1, screenPosition2);
    // Create Rect
    return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
}

    void CheckCharactersInSelectionArea(Vector2 startPoint, Vector2 endPoint)
    {
        bool haveTargetPrior = false;
        if(selectedCharacters.Count > 0){
            haveTargetPrior = true;
        }
        MovingObject priorTarget = CombatPartyView.instance.GetSelected();
        if(priorTarget){
            haveTargetPrior = true;
        }
        // Ensure the start point is the bottom-left and the end point is the top-right
        Vector2 min = Vector2.Min(startPoint, endPoint);
        Vector2 max = Vector2.Max(startPoint, endPoint);

        // Get all characters within the selection area
        Collider2D[] charactersInArea = Physics2D.OverlapAreaAll(min, max, LayerMask.GetMask("movingObjects"));
        bool triggered = false;
        // Loop over all characters in the area and add those owned by the player to the selected characters list
        foreach (Collider2D characterCollider in charactersInArea)
        {
            GameObject character = characterCollider.gameObject;
            if (!character.GetComponent<NetworkIdentity>())//character.CompareTag("Character") && 
            {
                continue;
            }
            Mob mob = character.GetComponent<Mob>();
            if(mob){
                continue;
            }

            if (character.GetComponent<NetworkIdentity>().hasAuthority)//character.CompareTag("Character") && 
            {
                if(!triggered){
//                    print("In triggered for checking so it thinks we were dragging to select");
                    ClearTarget();
                    triggered = true;
                }
                MovingObject pc = character.GetComponent<MovingObject>();
                if(pc.Dying){
                    continue;
                }
                selectedCharacters.Add(character);
                if(pc != CombatPartyView.instance.GetSelected()){
                    pc.SelectedMO();
                }
                if(character == null){
                    print($"Character not found!! THIS WAS THE BUG*******");

                    return;
                }
                selectedCharacterHighlight.Invoke(character);
                //print($"Pc {pc.CharacterName} was selected in our drag method!");
            }
        }
        if(selectedCharacters.Count > 0){
            List<MovingObject> selected = new List<MovingObject>();
            foreach(var sChar in selectedCharacters){
                MovingObject mo = sChar.GetComponent<MovingObject>();
                if(mo){
                    selected.Add(mo);
                    //foreach(Transform child in CombatPartyView.instance.transform){
                    //    CharacterCombatUI combatCharUI = child.GetComponent<CharacterCombatUI>();
                    //    if(combatCharUI.owner == mo){
                    //        combatCharUI.Selected();
                    //        break;
                    //    }
                    //}
                }
            }
            CombatPartyView.instance.TurnOnSelectedWindow(selected);
            //NewTarget.Invoke();
        }
    }
   
private SceneNode FindNodeByName(string nodeName)
{
    if(!isServer)
    {
        AddAllSceneNodesToDictionary();
    }
    if (sceneNodesDictionary.TryGetValue(nodeName, out SceneNode node))
    {
        return node;
    }
    
    // if no matching node is found, return null
    return null;
}
    [TargetRpc]
    public void TargetUpdateEnergyDisplay(float _energy){
        //print($"{_energy} is our energy for player {playerName} at time: {Time.time}");
        UIToggle.Invoke(_energy);
    }
    [TargetRpc]
    public void TargetHarvestContinous(PlayerCharacter pc, GameObject node){
        HarvestContinous(pc, node);
        //if(pc.stamina >= 0){
        //    //ImproperCheckText.Invoke($"Not enough staminia to perform this action");
        //    HarvestContinous(pc, node);
        //    return;
        //} else {
        //    CharacterCastingHarvest(pc, node);
        //}        
    }
    void HarvestContinous(PlayerCharacter pc, GameObject node){
        if(AwaitingHarvestDictionary.ContainsKey(pc.CharID)){
            if(AwaitingHarvestDictionary[pc.CharID] != null){
                StopCoroutine(AwaitingHarvestDictionary[pc.CharID]);
            }
            AwaitingHarvestDictionary[pc.CharID] = null;
            AwaitingHarvestDictionary.Remove(pc.CharID);
        }
        AwaitingHarvestDictionary.Add(pc.CharID, StartCoroutine(ContinousHarvestingCheck(pc, node)));
    }
    Dictionary<string,Coroutine> AwaitingHarvestDictionary = new Dictionary<string, Coroutine>();
    IEnumerator ContinousHarvestingCheck(PlayerCharacter pc, GameObject node){
        while(pc.stamina >= 0){
            yield return null; // Wait for the next frame
        }
        if(!pc.Dying && node != null){
            CharacterCastingHarvest(pc, node);
        }
        if(AwaitingHarvestDictionary.ContainsKey(pc.CharID)){
            AwaitingHarvestDictionary[pc.CharID] = null;
            AwaitingHarvestDictionary.Remove(pc.CharID);
        }
    }
    private float speed = 400f;

    private float lerpSpeed = 0.025f;  // Adjust this value as needed
    public float Dialspeed = 400f;

    public float DiallerpSpeed = 0.025f;  // Adjust this value as needed

void LocalCamera() {
    if(ContainerUIButtons.Instance == null){
        return;
    }
    if (!ContainerUIButtons.Instance.GetChat()) {
        //Vector3 targetPosition = cameraController.position + new Vector3(
        //    Input.GetKey(KeyCode.A) ? -speed * Time.deltaTime : (Input.GetKey(KeyCode.D) ? speed * Time.deltaTime : 0),
        //    Input.GetKey(KeyCode.W) ? speed * Time.deltaTime : (Input.GetKey(KeyCode.S) ? -speed * Time.deltaTime : 0),
        //    0
        //);
        Vector3 targetPosition = cameraController.position + new Vector3(
            (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) ? -speed * Time.deltaTime : (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) ? speed * Time.deltaTime : 0,
            (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) ? speed * Time.deltaTime : (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) ? -speed * Time.deltaTime : 0,
            0
        );
        cameraController.position = Vector3.Lerp(cameraController.position, targetPosition, lerpSpeed);
    }
}
public int initialPoolSize = 10;
    private Queue<GameObject> pool = new Queue<GameObject>();
private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = Instantiate(rightClickPrefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }
    public GameObject GetObject(Vector3 position)
    {
        GameObject obj;
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            obj = Instantiate(rightClickPrefab);
        }
        obj.transform.position = position;
        obj.SetActive(true);
        return obj;
    }
    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
float cameraUpdate = 0f;
    void LateUpdate(){
        if(!isServer){
            if(isLocalPlayer){
                if(Loading){
                    return;
                }
    	        //cameraUpdate += Time.deltaTime;
                //if(cameraUpdate > .01f){
                    if(!StatAsset.Instance.CheckForCombatZone(currentScene)){
                        //if(cameraController.transform.position != transform.position)
                        cameraController.transform.position = transform.position;
                    } else {
                        LocalCamera();
                    }
                //    cameraUpdate = 0f;
                //}
                //if(currentScene == "TOWNOFARUDINE" || currentScene == "OVM"){
                //    cameraController.transform.position = transform.position;
                //} else {
                //    LocalCamera();
                //}
            }
        }
    }
        public float delayTime = 1f;  // Delay in seconds
    GameObject lobby = null;
    GameObject FogWorld = null;
    
    [TargetRpc]
    public void TargetUpdateFogWorldTeleport(Vector3 characterPosition){
        UpdateFogWorldTeleport(characterPosition);
    }
    void StartUpdateFogWorld(){
        if(currentScene == "OVM"){
            StartCoroutine(PausedUpdate());
        }
    }
    IEnumerator PausedUpdate(){
        yield return new WaitForSeconds(2.5f);
        SendWorldFogSegments(transform.position);
    }
    [Server]
    public void ServerSetSegment(List<int> segments){
        SegmentData = segments;
        TargetSetSegment(segments);
    }
    [TargetRpc]
    void TargetSetSegment(List<int> segments){
        SegmentData = segments;
    }
    private List<int> SegmentData = new List<int>();
    public List<int> GetSegmentlistWorldMap(){
        return SegmentData;
    }
    public void GetOVMSegmentData(int segment){
        if(SegmentData.Contains(segment)){
            return;
        } else {
            Speaker.Invoke(300);
            SegmentData.Add(segment);
            CmdGetNewList(SegmentData);
        }
    }
    [Command]
    void CmdGetNewList(List<int> segments){
        SegmentData = segments;
    }
    [SyncVar] public bool CanUpdateOVM = false;
    void SendWorldFogSegments(Vector3 characterPosition){
        if(!FogWorld){
            FogWorld = GameObject.Find("OVMFogMachine");
        }
        if(FogWorld == null){
            return;
        }
        FogWorldMap fogofWar = FogWorld.GetComponent<FogWorldMap>();
		//print($"{posCheck} is the new target position that should be the char pos in foggywar");
        fogofWar.AddSegmentsOnStart(SegmentData);
       // UpdateFogWorld(characterPosition);
    }
    
    void UpdateFogWorldOtherPlayer(GameObject spriteObject, GameObject energyBar){
		if(!FogWorld){
            FogWorld = GameObject.Find("OVMFogMachine");
        }
        if(FogWorld == null){
            return;
        }
        FogWorldMap fogofWar = FogWorld.GetComponent<FogWorldMap>();
        fogofWar.UpdateOtherPlayer(spriteObject, energyBar);
    }
    void UpdateFogWorld(Vector3 characterPosition){
		if(!FogWorld){
            FogWorld = GameObject.Find("OVMFogMachine");
        }
        if(FogWorld == null){
            return;
        }
        FogWorldMap fogofWar = FogWorld.GetComponent<FogWorldMap>();
        fogofWar.UpdateFogOfWar(characterPosition);
    }
    void UpdateFogWorldTeleport(Vector3 characterPosition){
		if(!FogWorld){
            FogWorld = GameObject.Find("OVMFogMachine");
        }
        if(FogWorld == null){
            return;
        }
        FogWorldMap fogofWar = FogWorld.GetComponent<FogWorldMap>();
        fogofWar.UpdateFogOfWarTele(characterPosition);
    }
    private bool canMove = true;
private float moveCooldownTimer = 0.0f;
private float moveCooldownDuration = 0.15f; // 0.25 seconds cooldown
void CheckMove(){
    if (!canMove)
    {
        moveCooldownTimer += Time.deltaTime;

        // Check if the cooldown timer has expired.
        if (moveCooldownTimer >= moveCooldownDuration)
        {
            canMove = true; // Reset the flag when the cooldown is over.
            moveCooldownTimer = 0.0f; // Reset the timer.
        }
    }
}
string RestrictedName(string tag)
{
    // Check for each tag and return the corresponding house name
    if (tag == "ArudineHouse1")
    {
        return "Arudine House 1";
    }
    if (tag == "ArudineHouse2")
    {
        return "Arudine House 2";
    }
    if (tag == "ArudineHouse3")
    {
        return "Arudine House 3";
    }
    if (tag == "ArudineHouse4")
    {
        return "Arudine House 4";
    }
    if (tag == "ArudineHouse5")
    {
        return "Arudine House 5";
    }
    if (tag == "ArudineHouse6")
    {
        return "Arudine House 6";
    }
    if (tag == "ArudineHouse7")
    {
        return "Arudine House 7";
    }
    if (tag == "ArudineHouse8")
    {
        return "Arudine House 8";
    }
    if (tag == "ArudineHouse9")
    {
        return "Arudine House 9";
    }
    if (tag == "ArudineHouse10")
    {
        return "Arudine House 10";
    }
    if (tag == "ArudineGuild")
    {
        return "Arudine Guild Plot";
    }

    // If the tag does not match any of the known tags, return null or an empty string
    return "Restricted zone";
}
 bool IsTagAllowedToEnter(string tag)
{
    // Define the tags that allow entry based on conditions
    List<string> allowedTags = new List<string>();
    // Add conditionally allowed tags
        allowedTags.Add("ArudineHouse1");
        allowedTags.Add("ArudineGuild");

    if (TacticianInformationSheet.TacticianAddress == "Arudine House 1")
    {
        allowedTags.Add("ArudineHouse1");
    }
    if (TacticianInformationSheet.TacticianAddress == "Arudine House 2")
    {
        allowedTags.Add("ArudineHouse2");
    }
    if (TacticianInformationSheet.TacticianAddress == "Arudine House 3")
    {
        allowedTags.Add("ArudineHouse3");
    }
    if (TacticianInformationSheet.TacticianAddress == "Arudine House 4")
    {
        allowedTags.Add("ArudineHouse4");
    }
    if (TacticianInformationSheet.TacticianAddress == "Arudine House 5")
    {
        allowedTags.Add("ArudineHouse5");
    }
    if (TacticianInformationSheet.TacticianAddress == "Arudine House 6")
    {
        allowedTags.Add("ArudineHouse6");
    }
    if (TacticianInformationSheet.TacticianAddress == "Arudine House 7")
    {
        allowedTags.Add("ArudineHouse7");
    }
    if (TacticianInformationSheet.TacticianAddress == "Arudine House 8")
    {
        allowedTags.Add("ArudineHouse8");
    }
    if (TacticianInformationSheet.TacticianAddress == "Arudine House 9")
    {
        allowedTags.Add("ArudineHouse9");
    }
    if (TacticianInformationSheet.TacticianAddress == "Arudine House 10")
    {
        allowedTags.Add("ArudineHouse10");
    }

    // Check if the clicked tag is in the allowed tags
    return allowedTags.Contains(tag);
}
void GetDirection(Vector3 LastDirectionGiven){
     Vector2 direction = (LastDirectionGiven - transform.position).normalized;
    if(Mathf.Abs(direction.x) > Mathf.Abs(direction.y)){
    if(direction.x > 0){
            // Going right
            animator.SetBool("right", true);
            animator.SetBool("left", false);
            animator.SetBool("up", false);
            animator.SetBool("down", false);
            animator.SetBool("stand", false);
        } else {
            // Going left
            animator.SetBool("right", false);
            animator.SetBool("left", true);
            animator.SetBool("up", false);
            animator.SetBool("down", false);
            animator.SetBool("stand", false);
        }
    } else {
        if(direction.y > 0){
            // Going up
            animator.SetBool("right", false);
            animator.SetBool("left", false);
            animator.SetBool("up", true);
            animator.SetBool("down", false);
            animator.SetBool("stand", false);
        } else {
            // Going down
            animator.SetBool("right", false);
            animator.SetBool("left", false);
            animator.SetBool("up", false);
            animator.SetBool("down", true);
            animator.SetBool("stand", false);
        }
    }
}

    void MovePlayer(){
        if(currentScene == "OVM"){
            if(ConnectCanvas == null)
                lobby = GameObject.Find("Lobby");
                if(lobby == null){
                    return;
                }
                ConnectCanvas = lobby.GetComponent<Canvas>();
                if(ConnectCanvas == null){
                    return;
                }
            if(ConnectCanvas.enabled){
                return;
            }
        }
        if(Input.GetMouseButtonDown(1) && !ContainerUIButtons.Instance.GetChat() && !TradeSupervisor.instance.GetTradeWindow()){
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            print($"{worldPosition} is our worldPosition, mouse potion is {Input.mousePosition}");
            RaycastHit2D hitUI = Physics2D.Raycast(worldPosition, Vector2.zero, 0f, LayerMask.GetMask("UI"));
            bool UIHit = false;
            
            if (hitUI.collider != null){
                UIHit = true;
            }
            if(UIHit){
                return;
            }
            if (EventSystem.current.IsPointerOverGameObject())
            {
                PointerEventData pointerData = new PointerEventData(EventSystem.current)
                    {
                        position = Input.mousePosition
                    };
                    // Create a list to store the objects the raycast hits
                    List<RaycastResult> results = new List<RaycastResult>();
                    // Perform the raycast using the pointer event data
                    EventSystem.current.RaycastAll(pointerData, results);
                    bool ditch = true;
                    // Go through the list and check for the UI object by name
                    foreach (RaycastResult result in results)
                    {
                        SceneNode node = result.gameObject.GetComponent<SceneNode>();
                        if(node){
                            ditch = false;
                            break;
                        }
                    }
                    if(ditch){
                        return;
                    }
            }
            bool Green = true;
            Vector3 target3D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            target3D.z += 10;
            // Perform a raycast to check for the clicked layer
            RaycastHit2D[] hitInfos = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);


        bool isValidHit = true;
        string possibleFailedTag = string.Empty;
        foreach (RaycastHit2D hitInfo in hitInfos)
        {
            string clickedTag = hitInfo.collider.gameObject.tag;

            //print($"Targetting raycast tag for object {hitInfo.collider.gameObject.name} is tag: {clickedTag}");
            if (string.IsNullOrEmpty(clickedTag) || clickedTag == "Untagged" || clickedTag == "Node" || clickedTag == "WALL" || clickedTag == "GlorySeeker")
            {
                continue;
            }
            // Check if the clicked tag is one that allows entry
            if (!IsTagAllowedToEnter(clickedTag))
            {
                isValidHit = false;
                possibleFailedTag = clickedTag;
                break;
            }
        }
                // Check if the clicked layer is one that allows entry
                if (isValidHit)
                {
                    NavMeshHit hit;
                // Adjust the maxDistance as necessary for your game's scale
                    if (NavMesh.SamplePosition(target3D, out hit, 1.0f, NavMesh.AllAreas)) {
                        target3D = hit.position; // Update target3D to the hit position if it's on the NavMesh
                        RaycastHit2D[] recheck = Physics2D.RaycastAll(target3D, Vector2.zero);


                        bool isValidHitCheck = true;
                        string failedTag = string.Empty;
                        foreach (RaycastHit2D hitInfo in recheck)
                        {
                            string recheckTag = hitInfo.collider.gameObject.tag;

                            //print($"Targetting raycast tag for object {hitInfo.collider.gameObject.name} is tag: {recheckTag}");
                            if (string.IsNullOrEmpty(recheckTag) || recheckTag == "Untagged" || recheckTag == "Node" || recheckTag == "WALL" || recheckTag == "GlorySeeker")
                            {
                                continue;
                            }
                            // Check if the clicked tag is one that allows entry
                            if (!IsTagAllowedToEnter(recheckTag))
                            {
                                isValidHitCheck = false;
                                failedTag = recheckTag;
                                break;
                            }
                        }
                        
                        if(!isValidHitCheck){
                            string failedZone = RestrictedName(failedTag);
                            if(failedZone == "Arudine Guild Plot"){
                                ImproperCheckText.Invoke($"You must own {failedZone} or be a member of the guild to enter that space");
                            } else {
                                ImproperCheckText.Invoke($"You must own {failedZone} or have permission from the owner to enter that space");
                            }
                            return;
                        }
                        //GameObject rightclick = Instantiate(rightClickPrefab, target3D, Quaternion.identity);
                        GameObject rightclick = GetObject(target3D);
                        RightClickAnimation RCA = rightclick.GetComponent<RightClickAnimation>();
                        PlayerMovingClicked.Invoke(target3D);
                        if(Green){
                            RCA.StartGreenSequence();
                        } else {
                            RCA.StartRedSequence();
                        }
                        Vector2 direction = (target3D - transform.position).normalized;
                        /*
                         if(Mathf.Abs(direction.x) > Mathf.Abs(direction.y)){
                             if(direction.x > 0){
                                 // Going right
                                 animator.SetBool("right", true);
                                 animator.SetBool("left", false);
                                 animator.SetBool("up", false);
                                 animator.SetBool("down", false);
                                 animator.SetBool("stand", false);
                             } else {
                                 // Going left
                                 animator.SetBool("right", false);
                                 animator.SetBool("left", true);
                                 animator.SetBool("up", false);
                                 animator.SetBool("down", false);
                                 animator.SetBool("stand", false);
                             }
                         } else {
                             if(direction.y > 0){
                                 // Going up
                                 animator.SetBool("right", false);
                                 animator.SetBool("left", false);
                                 animator.SetBool("up", true);
                                 animator.SetBool("down", false);
                                 animator.SetBool("stand", false);
                             } else {
                                 // Going down
                                 animator.SetBool("right", false);
                                 animator.SetBool("left", false);
                                 animator.SetBool("up", false);
                                 animator.SetBool("down", true);
                                 animator.SetBool("stand", false);
                             }
                         }
                         */
                        MovementRequestedTactician.Enqueue(target3D);

                        //CmdMovePlayer(target3D);
                        //canMove = false; // Set the flag to prevent further moves.
                    } else {
                        // Handle the case where no valid NavMesh point was found close to the clicked position
                        Debug.Log("Clicked point is not on a walkable NavMesh surface.");
                        ImproperCheckText.Invoke("Cannot move there");
                    }
                    return;
                } else {
                    string restrictedZone = RestrictedName(possibleFailedTag);
                    if(restrictedZone == "Arudine Guild Plot"){
                        ImproperCheckText.Invoke($"You must own {restrictedZone} or be a member of the guild to enter that space");
                    } else {
                        ImproperCheckText.Invoke($"You must own {restrictedZone} or have permission from the owner to enter that space");
                    }
                    return;
                }
            }
        }
            bool SoundPopped = true;
            Vector3 StepVector;
            /*
        void PlaySound(){
            if(!SoundPopped){
            } else {
                float distanceTraveledPing = 1.0f;
                if(ScenePlayer.localPlayer.currentScene == "OVM"){
                    distanceTraveledPing = .05f;
                }
                if (Vector3.Distance(StepVector, transform.position) >= distanceTraveledPing){
                    string typeOfStep = "grass";
                    if(currentScene == "OVM"){
                        if(MatchMaker.instance != null){
                           typeOfStep = MatchMaker.instance.RandomNodeSelector(transform.position);
                        }
                    } else {
                        
                    }
                    AudioClip FirstClip = RandomCD(typeOfStep);
                    UIAudio.PlayOneShot(FirstClip);
                    SoundPopped = false;
                    StepVector = transform.position;
                }
            }
            
        }

        AudioClip RandomCD(string soundRequested){
           AudioClip[] clips = null;
            switch (soundRequested.ToLower())
            {
                case "grass":
                    clips = new AudioClip[] { tacticianMovingOneGrass, tacticianMovingTwoGrass, tacticianMovingThreeGrass };
                    break;
                case "wood":
                    clips = new AudioClip[] { tacticianMovingOneWood, tacticianMovingTwoWood, tacticianMovingThreeWood };
                    break;
                case "stone":
                    clips = new AudioClip[] { tacticianMovingOneStone, tacticianMovingTwoStone, tacticianMovingThreeStone };
                    break;
                case "sand":
                    clips = new AudioClip[] { tacticianMovingOneSand, tacticianMovingTwoSand, tacticianMovingThreeSand };
                    break;
                default:
                    Debug.LogWarning("Invalid sound requested.");
                    return null;
            }
            int randomIndex;
            randomIndex = UnityEngine.Random.Range(0, clips.Length);
            return clips[randomIndex];
        }    
        */
        void SetAnimatorState(string direction)
{
    switch (direction)
    {
        case "right":
            animator.SetBool("right", true);
            animator.SetBool("left", false);
            animator.SetBool("up", false);
            animator.SetBool("down", false);
            animator.SetBool("stand", false);
            break;
        case "left":
            animator.SetBool("right", false);
            animator.SetBool("left", true);
            animator.SetBool("up", false);
            animator.SetBool("down", false);
            animator.SetBool("stand", false);
            break;
        case "up":
            animator.SetBool("right", false);
            animator.SetBool("left", false);
            animator.SetBool("up", true);
            animator.SetBool("down", false);
            animator.SetBool("stand", false);
            break;
        case "down":
            animator.SetBool("right", false);
            animator.SetBool("left", false);
            animator.SetBool("up", false);
            animator.SetBool("down", true);
            animator.SetBool("stand", false);
            break;
        default:
            animator.SetBool("right", false);
            animator.SetBool("left", false);
            animator.SetBool("up", false);
            animator.SetBool("down", false);
            animator.SetBool("stand", true);
            break;
    }
}
    string GetDirectionValue(Vector3 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            return direction.x > 0 ? "right" : "left";
        }
        else
        {
            return direction.y > 0 ? "up" : "down";
        }
    }
void StopMovementOnServer(){
    CmdStopMovement();
}
[Command]
void CmdStopMovement(){
    NavMeshAgent navMeshAgent = GetComponent<NavMeshAgent>();
    navMeshAgent.updateRotation = false;  // Prevent agent from updating rotation
    navMeshAgent.isStopped = true;
}
// Overloaded Command for mouse-click movement
float movedDistance = 0f;
bool RandomRepel = false;
        //StartCoroutine(SendRpcQueueMovement());

[Command]
void CmdMovePlayer(Vector3 targetPosition){
    if (RandomMatchMovementCoroutine != null) {
        StopCoroutine(RandomMatchMovementCoroutine);
    }
    RandomMatchMovementCoroutine = StartCoroutine(MoveAndRoll(targetPosition));
}
private Queue<Vector3> MovementRequestedTactician = new Queue<Vector3>();
public IEnumerator SendRpcQueueMovement()
    {
		float interval = 0.25f;
        while (true)
        {
            if (MovementRequestedTactician.Count > 1)
            {
                // Remove all items except the last one
                while (MovementRequestedTactician.Count > 1)
                {
                    MovementRequestedTactician.Dequeue();
                }
            }

            if (MovementRequestedTactician.Count == 1)
            {
                Vector3 startingVector = MovementRequestedTactician.Dequeue();
                CmdMovePlayer(startingVector);
            }

            yield return new WaitForSeconds(interval);
        }
    }
Coroutine RandomMatchMovementCoroutine;

Vector3 lastPosition;
void OnArrivalAtDestination() {
    RpcSetDirection("Standing");
}

IEnumerator MoveAndRoll(Vector3 targetPosition) {
    float directionUpdateInterval = .15f; // Time in seconds between direction updates
    float lastDirectionUpdateTime = Time.time; // Track the last update time
    //MovingTact = true;
    NavMeshAgent navMeshAgent = GetComponent<NavMeshAgent>();
    navMeshAgent.updateRotation = false;  // Prevent agent from updating rotation
    navMeshAgent.isStopped = false;
    Vector3 newTargetPosition = transform.position;
    if(currentScene != "OVM"){
    //if(currentScene == "TOWNOFARUDINE"){
        navMeshAgent.speed = 3.5f;
        navMeshAgent.SetDestination(targetPosition);
        //UpdateDirection();
        // Wait until the NavMeshAgent has reached the destination
        //while (navMeshAgent.remainingDistance > 0.1f) {
        //while (Vector3.Distance(transform.position, targetPosition) >= 0.15f) {
        ////while (transform.position != targetPosition){
        //    if (Time.time - lastDirectionUpdateTime >= directionUpdateInterval) {
        //        if(transform.position == newTargetPosition){
        //            Debug.Log("Arrived at destination!");
        //            //OnArrivalAtDestination();
        //            //MovingTact = false;
        //            RandomMatchMovementCoroutine = null;
        //            yield break;
        //        } else {
        //            newTargetPosition = transform.position;
        //        }
        //        //UpdateDirection(); // Update the direction based on current position
        //        lastDirectionUpdateTime = Time.time; // Reset the last update time
        //    }
        //    yield return null; // Wait for the next frame
        //}
        //Debug.Log("Arrived at destination!");
        //MovingTact = false;
        //OnArrivalAtDestination();
        RandomMatchMovementCoroutine = null;
    } else {
        navMeshAgent.speed = .15f;
        navMeshAgent.SetDestination(targetPosition);
        //UpdateDirection();
        lastPosition = transform.position;
        //while (Vector3.Distance(transform.position, targetPosition) >= 0.15f) {
            while (transform.position != targetPosition){

            //if (Time.time - lastDirectionUpdateTime >= directionUpdateInterval) {
            //    if(transform.position == newTargetPosition){
            //        Debug.Log("Arrived at destination!");
            //        //OnArrivalAtDestination();
            //        RandomMatchMovementCoroutine = null;
            //        yield break;
            //    } else {
            //        newTargetPosition = transform.position;
            //    }
            //   // UpdateDirection(); // Update the direction based on current position
            //    lastDirectionUpdateTime = Time.time; // Reset the last update time
            //}
            // Calculate distance moved since last frame
            if(!RandomRepel){
                float distanceThisFrame = Vector3.Distance(transform.position, lastPosition);
                movedDistance += distanceThisFrame;
                // If moved more than 1 unit, roll
                if (movedDistance >= 1f) {
                    //TargetUpdateFogWorld(lastPosition);
                    print("Starting random roll we moved 1f");
                    if (RollForRandomMatch()) {
                        StartCoroutine(SavedRandomRoll());
                        //ServerRandomMatchSoloGame("Random_Forest_1_1", false);
                        //movedDistance = 0f;
                        //lastPosition = transform.position; // update last position
                        ////MovingTact = false;
                        //if (RandomMatchMovementCoroutine != null) {
                        //    StopCoroutine(RandomMatchMovementCoroutine);
                        //    RandomMatchMovementCoroutine = null;
                        //}
                        yield break;
                        //send random match
                    }
                    movedDistance = 0f; // reset moved distance
                }
                lastPosition = transform.position; // update last position
            }
            yield return null; // wait for next frame
        }
        //Debug.Log("Arrived at destination!");
       // OnArrivalAtDestination();
       //MovingTact = false;
        RandomMatchMovementCoroutine = null;
    }
}
IEnumerator SavedRandomRoll(){
    CanUpdateOVM = false;
    yield return new WaitForSeconds(1f);
    ServerRandomMatchSoloGame("Random_Forest_1_1", false);
    movedDistance = 0f;
    lastPosition = transform.position; // update last position
    //MovingTact = false;
    if (RandomMatchMovementCoroutine != null) {
        StopCoroutine(RandomMatchMovementCoroutine);
        RandomMatchMovementCoroutine = null;
    }
}
void UpdateDirection() {
    // Check if we have a path and it has at least one corner
    NavMeshAgent navMeshAgent = GetComponent<NavMeshAgent>();
    if (navMeshAgent.hasPath && navMeshAgent.path.corners.Length > 1) {
        // Use the second corner to look ahead. The first corner is the current position.
        Vector3 nextCorner = navMeshAgent.path.corners[1];
        Vector3 directionToNextCorner = (nextCorner - transform.position).normalized;

        // Here, adapt this part to your needs. This is just a simple directional check.
        if(Mathf.Abs(directionToNextCorner.x) > Mathf.Abs(directionToNextCorner.y)) {
            RpcSetDirection(directionToNextCorner.x > 0 ? "Right" : "Left");
        } else {
            RpcSetDirection(directionToNextCorner.y > 0 ? "Top" : "Bottom");
        }
    }
}

private float lastRollTime = 0f;
private float rollCooldown = 1f; // 1 second cooldown
/*
[Server]
public void ServerRandomCharge(float amount){
    CharacterStatListItem EXPItem = (new CharacterStatListItem{
        Key = "EXP",
        Value = ""
    });
    Energy -= amount;
    float EXP = 0f;
    foreach(var stat in TacticianInformationSheet.TacticianStatData){
        if(stat.Key == "EXP"){
            EXP = float.Parse(stat.Value);
        }
    }
    EXP += amount;
    EXPItem.Value = Math.Round(EXP, 2).ToString("F2");
    TargetUpdateEnergyDisplay(Energy);
    GetTacticianEXP(EXPItem);
}
*/
private bool RollForRandomMatch() {
    if(repelOn){
        return false;
    }
    float currentTime = Time.time;
    if (currentTime - lastRollTime < rollCooldown) {
        return false; // Cooldown not yet expired
    }
    GetRandomCost.Invoke(this.connectionToClient, 2);
    int roll = UnityEngine.Random.Range(1, 101);
    print($"Got a {roll} roll for a random match");
    lastRollTime = currentTime; // Update the last roll time
    int rollChance = 15;
    for(int _char = 0; _char < ActivePartyList.Count; _char++){
        for(int sheet = 0; sheet < InformationSheets.Count; sheet++){
            if(InformationSheets[sheet].CharacterID == ActivePartyList[_char]){
                for(int stat = 0; stat < InformationSheets[sheet].CharStatData.Count; stat++){
                    if (InformationSheets[sheet].CharStatData[stat].Key == "Class") {
                        if(InformationSheets[sheet].CharStatData[stat].Value == "Rogue"){
                            for(int ability = 0; ability < InformationSheets[sheet].CharSpellData.Count; ability++){
                                if(InformationSheets[sheet].CharSpellData[ability].Key == "EastT3TopSkill"){
                                    var abilityRankString = System.Text.RegularExpressions.Regex.Match(InformationSheets[sheet].CharSpellData[ability].Value, @"\d+$");
                                    if (abilityRankString.Success) {
                                        int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                                        rollChance += abilityRank;
                                    }
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
                break;
            }
        }
    }
    if (roll <= rollChance) {
        print($"Got a random match to spawn at this time {DateTime.Now}");
        return true;
    }

    return false;
}
private float lastRandomMatchTime = 0f;
private const float RANDOM_MATCH_COOLDOWN = 5f;  // 5 second cooldown

    [Server]
    public void ServerRandomMatchSoloGame(string biome, bool login){
        string randomMatch = string.Empty;
        if(login){
            randomMatch = biome;
        } else {
            randomMatch = MatchMaker.instance.RandomNodeSelector(transform.position);
        }
        print($"Found random match with our new formula! it was {randomMatch}");
        float currentTime = Time.time;
        // Check if enough time has elapsed since the last random match was made
        if (currentTime - lastRandomMatchTime >= RANDOM_MATCH_COOLDOWN)
        {
            // Set the last random match time to now
            lastRandomMatchTime = currentTime;
            string finalMatchName = randomMatch;
            NavMeshAgent navMeshAgent = GetComponent<NavMeshAgent>();
            navMeshAgent.isStopped = true;
            // Generate a random number between 1 and 5
            //int randomNumber = UnityEngine.Random.Range(1, 6);
            // Append the random number to the biome name
            //finalMatchName = $"{biome}_{randomNumber}";
            matchID = MatchMaker.GetRandomMatchID();
            //print("called CMDSOLOGAME ************* on server");
            if (MatchMaker.instance.RandomMatch(matchID, this, false, out playerIndex, finalMatchName, login)) {
                Debug.Log($"Game {matchID} Solo created successfully");
                TargetRandomStartScreenShot();
            }
            if (RandomMatchMovementCoroutine != null) {
                StopCoroutine(RandomMatchMovementCoroutine);
                RandomMatchMovementCoroutine = null;
            }
        }  else {
            Debug.Log("ServerRandomMatchSoloGame is on cooldown.");
        }
    }
    [TargetRpc]
    void TargetRandomStartScreenShot(){
        RandomMatchEventClient.Invoke();
    }
    public void NonRPCSetDirection(string direction){
    
    if(direction == "Left"){
        animator.SetBool("right", false);
        animator.SetBool("left", true);
        animator.SetBool("up", false);
        animator.SetBool("down", false);
        animator.SetBool("stand", false);
    }
    if(direction == "Right"){
        // Going right
        animator.SetBool("right", true);
        animator.SetBool("left", false);
        animator.SetBool("up", false);
        animator.SetBool("down", false);
        animator.SetBool("stand", false);
    }
    if(direction == "Top"){
        // Going left
        animator.SetBool("right", false);
        animator.SetBool("left", false);
        animator.SetBool("up", true);
        animator.SetBool("down", false);
        animator.SetBool("stand", false);
    }
    if(direction == "Bottom"){
        // Going up
        animator.SetBool("right", false);
        animator.SetBool("left", false);
        animator.SetBool("up", false);
        animator.SetBool("down", true);
        animator.SetBool("stand", false);
    }
    if(direction == "Standing"){
        // Going up
        animator.SetBool("right", false);
        animator.SetBool("left", false);
        animator.SetBool("up", false);
        animator.SetBool("down", false);
        animator.SetBool("stand", true);
    }
    /*
    Animator animator = SpriteFlip.GetComponent<Animator>();
    // Reset all directions to false
    animator.SetBool("down", false);
    animator.SetBool("right", false);
    animator.SetBool("left", false);
    animator.SetBool("up", false);
    
    // Set the correct direction to true based on the direction string
    switch (direction)
    {
        case "North":
            animator.SetBool("up", true);
            break;
        case "South":
            animator.SetBool("down", true);
            break;
        case "East":
            animator.SetBool("right", true);
            break;
        case "West":
            animator.SetBool("left", true);
            break;
        case "North-East":
            animator.SetBool("right", true); // You might have a dedicated animation for this
            break;
        case "North-West":
            animator.SetBool("left", true); // You might have a dedicated animation for this
            break;
        case "South-East":
            animator.SetBool("right", true); // You might have a dedicated animation for this
            break;
        case "South-West":
            animator.SetBool("left", true); // You might have a dedicated animation for this
            break;
        default:
            break;
    }
    */
}
[ClientRpc]
public void RpcSetDirection(string direction){
    
    if(direction == "Left"){
        animator.SetBool("right", false);
        animator.SetBool("left", true);
        animator.SetBool("up", false);
        animator.SetBool("down", false);
        animator.SetBool("stand", false);
    }
    if(direction == "Right"){
        // Going right
        animator.SetBool("right", true);
        animator.SetBool("left", false);
        animator.SetBool("up", false);
        animator.SetBool("down", false);
        animator.SetBool("stand", false);
    }
    if(direction == "Top"){
        // Going left
        animator.SetBool("right", false);
        animator.SetBool("left", false);
        animator.SetBool("up", true);
        animator.SetBool("down", false);
        animator.SetBool("stand", false);
    }
    if(direction == "Bottom"){
        // Going up
        animator.SetBool("right", false);
        animator.SetBool("left", false);
        animator.SetBool("up", false);
        animator.SetBool("down", true);
        animator.SetBool("stand", false);
    }
    if(direction == "Standing"){
        // Going up
        animator.SetBool("right", false);
        animator.SetBool("left", false);
        animator.SetBool("up", false);
        animator.SetBool("down", false);
        animator.SetBool("stand", true);
    }
    /*
    Animator animator = SpriteFlip.GetComponent<Animator>();
    // Reset all directions to false
    animator.SetBool("down", false);
    animator.SetBool("right", false);
    animator.SetBool("left", false);
    animator.SetBool("up", false);
    
    // Set the correct direction to true based on the direction string
    switch (direction)
    {
        case "North":
            animator.SetBool("up", true);
            break;
        case "South":
            animator.SetBool("down", true);
            break;
        case "East":
            animator.SetBool("right", true);
            break;
        case "West":
            animator.SetBool("left", true);
            break;
        case "North-East":
            animator.SetBool("right", true); // You might have a dedicated animation for this
            break;
        case "North-West":
            animator.SetBool("left", true); // You might have a dedicated animation for this
            break;
        case "South-East":
            animator.SetBool("right", true); // You might have a dedicated animation for this
            break;
        case "South-West":
            animator.SetBool("left", true); // You might have a dedicated animation for this
            break;
        default:
            break;
    }
    */
}
//*******************************************
	//*********NODE LOBBY CONTROLS***************
	//*******************************************

     /*
        HOST MATCH
    */
    void RemoveCharacterFromAdventureList(string charSlot){
        CmdRemoveCharacterFromAdventureList(charSlot);
    }
    [Command]
    void CmdRemoveCharacterFromAdventureList(string charSlot){
        ServerRemoveCharacterFromAdventureList(charSlot);
    }
    [Server]
    void ServerRemoveCharacterFromAdventureList(string charSlot){
        //print("Got to server remove character");
        MatchMaker.instance.RemoveCharacterVote(this, charSlot, currentMatch);

    }
    [TargetRpc]
    public void TargetRemoveAdventurer(ScenePlayer sPlayer, string serial){
        //UILobby.instance.SpawnAdventurer(sPlayer,selectedCharacter, spriteName, serial);
        //if(sPlayer.currentMatch == ScenePlayer.localPlayer.currentMatch){
            RemoveAdventurer.Invoke(sPlayer, serial);
        //}
    }
    void AddCharacterToAdventureList(string charName, string spriteName, string charSlot){
        CmdAddCharacterToAdventureList(charName, spriteName, charSlot);
        print("MADE IT TO  AddCharacterToAdventureList");
    }
    [Command]
    void CmdAddCharacterToAdventureList(string charName, string spriteName, string charSlot){
        ServerAddCharacterToAdventureList(charName, spriteName, charSlot);
    }
    [Server]
    void ServerAddCharacterToAdventureList(string charName, string spriteName, string charSlot){
        MatchMaker.instance.CastCharacterVote(this, charSlot, currentMatch, charName, spriteName);
    }
    [TargetRpc]
    public void TargetSpawnAdventurer(ScenePlayer sPlayer, string selectedCharacter, string spriteName, string serial){
        UILobby.instance.SpawnAdventurer(sPlayer,selectedCharacter, spriteName, serial);
    }
    public void HostGame(bool publicMatch){
        CmdHostGame(publicMatch);
    }

    [Command]
    void CmdHostGame(bool publicMatch){
        matchID = MatchMaker.GetRandomMatchID();
        
        if (MatchMaker.instance.HostGame(matchID, this, publicMatch, out int _playerIndex)) {
            Debug.Log($"Game {matchID} hosted successfully, Public game?: {publicMatch}");
             playerIndex = _playerIndex;
            //networkMatchChecker.matchId = _matchID.ToGuid();
            List<SavedPartyList> savedParty = new List<SavedPartyList>();
            foreach(var member in GetParty()){
                    foreach(var sheet in GetInformationSheets()){
                        if(sheet.CharacterID == member){
                            string charName = string.Empty;
                            string charSprite = string.Empty;
                            bool nameFound = false;
                            bool spriteFound = false;
                            foreach(var stat in sheet.CharStatData){
                                if(stat.Key == "CharName"){
                                    charName = stat.Value;
                                    nameFound = true;
                                }
                                if(stat.Key == "CharacterSprite"){
                                    charSprite = stat.Value;
                                    spriteFound = true;
                                }
                                if(nameFound && spriteFound){
                                    break;
                                }
                            }
                            SavedPartyList KVP = (new SavedPartyList{
                                Key = charName,
                                Value = charSprite
                            });
                            savedParty.Add(KVP);
                        }
                    }
                }
                ClientPartyInformation partyList = (new ClientPartyInformation
                {
                    Party = savedParty,
                    owner = this
                });
            TargetHostGame(true, matchID, playerIndex, loadSprite, partyList);
           // StartCoroutine(SendPartyInfoToClient(partyList));
        } else {
            Debug.Log($"<color=red>Game hosted failed:</color>");
            TargetHostGame(false, matchID, playerIndex, loadSprite, new ClientPartyInformation());

        }

    }

    [TargetRpc]
    void TargetHostGame(bool success, string _matchID, int _playerindex, string tactSprite, ClientPartyInformation savedList){
        if(!success){
            return;
        }
        foreach(var key in savedList.Party){
            // Only add if the key does not exist in InspectParty
            if (!InspectParty.ContainsKey(key.Key))//make the class to look at these in inspector
            {
                InspectParty.Add(key.Key, key.Value);
                //print($"added {key.Key} to inspectorlist and they have sprite {key.Value}");
            }
        }
        playerIndex = _playerindex;
        matchID = _matchID;
        //isMatchLeader = true;
        Debug.Log($"Match ID: {matchID} == {_matchID}");
        UILobby.instance.HostSuccess (success, _matchID, tactSprite);

    }
    [Server]
    //IEnumerator SendPartyInfoToClient(ClientPartyInformation partyList){
    //    yield return new WaitForSeconds(2f);
    //    RpcBuildPartyInspector(partyList);
    //}
    /*
        JOIN MATCH
    */
    public void JoinGame (string _inputID) {
        CmdJoinGame (_inputID);
    }
    [Command]
    void CmdJoinGame (string _matchID) {
        matchID = _matchID;
        ScenePlayer Host;
        if (MatchMaker.instance.JoinGame(_matchID, this, out int _playerIndex, out Host)) {
            Debug.Log($"Game hosted successfully CMDJOINGAME");
            playerIndex = _playerIndex;
            matchID = _matchID;
            CanUpdateOVM = false;
            //networkMatchChecker.matchId = _matchID.ToGuid();
            TargetJoinGame(true, _matchID, _playerIndex);
            /*
            foreach(var partyMember in currentMatch.players){
                List<SavedPartyList> savedParty = new List<SavedPartyList>();
                foreach(var member in partyMember.GetParty()){
                    foreach(var sheet in partyMember.GetInformationSheets()){
                        if(sheet.CharacterID == member){
                            string charName = string.Empty;
                            string charSprite = string.Empty;
                            bool nameFound = false;
                            bool spriteFound = false;
                            foreach(var stat in sheet.CharStatData){
                                if(stat.Key == "CharName"){
                                    charName = stat.Value;
                                    nameFound = true;
                                }
                                if(stat.Key == "CharacterSprite"){
                                    charSprite = stat.Value;
                                    spriteFound = true;
                                }
                                if(nameFound && spriteFound){
                                    break;
                                }
                            }
                            SavedPartyList KVP = (new SavedPartyList{
                                Key = charName,
                                Value = charSprite
                            });
                            savedParty.Add(KVP);
                        }
                    }
                }
                ClientPartyInformation partyList = (new ClientPartyInformation
                {
                    Party = savedParty,
                    owner = this
                });
                StartCoroutine(SendPartyInfoToClient(partyList));
            }
                */

        } else {
            Debug.Log($"<color=red>Game hosted failed:</color>");
            TargetJoinGame(false, _matchID, playerIndex);
        }
    }
    [TargetRpc]
    void TargetJoinGame(bool success, string _matchID, int _playerIndex){
        Debug.Log($"Match ID: {matchID} == {_matchID}");
        UILobby.instance.JoiningSucces (success, _matchID);
    }
    /*
        SEARCH MATCH
    */

    public void SearchGame() {
        CmdSearchGame();
    }
    [Command]
    void CmdSearchGame () {
        if (MatchMaker.instance.SearchGame(this, out int _playerIndex, out string _matchID)) {
            Debug.Log($"Game found CMDSEARCHGAME");
            playerIndex = _playerIndex;
            matchID = _matchID;
            //networkMatchChecker.matchId = matchID.ToGuid();
            CanUpdateOVM = false;
            TargetSearchGame(true, matchID, playerIndex);
            //Host
            if (isServer && playerLobbyUI != null) {
                playerLobbyUI.SetActive(true);
            }
        } else {
            Debug.Log($"<color=red>Game not found</color>");
            TargetSearchGame(false, matchID, playerIndex);
        }
    }
    [TargetRpc]
    void TargetSearchGame(bool success, string _matchID, int _playerIndex){
        playerIndex = _playerIndex;
        matchID = _matchID;
        Debug.Log($"Match ID: {matchID} == {_matchID}");
        UILobby.instance.SearchSuccess (success, _matchID);
    }
    
    
    /*
        BEGIN MATCH
    */
    public void BeginGame() {
        //print("Beginning game from start game");
        CmdBeginGame(ScenePlayer.localPlayer);
    }
    

    [Command]
    void CmdBeginGame(ScenePlayer player) {
        ServerBeginGame(player);
    }
    [Server]
    void ServerBeginGame(ScenePlayer player){
        Debug.Log($"Game Beginning CMDBEGINGAME");
        if (matchID == string.Empty)
        { 
            //print("Bugged out from match maker disconnect");
        }
        foreach(var sPlayer in player.currentMatch.players){
            TargetRemoveLobby();
        }
        MatchMaker.instance.CreateGame (player, matchID, currentMatch);
    }
    [TargetRpc]
    void TargetRemoveLobby(){
        BeginGameClearLobby.Invoke();
    }
    
    [TargetRpc]
    public void TargetGetReadyForStart(){
        //lets try to turn off screen here with a call to load bar
        if(UILobby.instance != null){
            UILobby.instance.RemoveLobby();
        }
        LoadbarOnToggle.Invoke();
    }
    public void InspectTactician(ScenePlayer p2){
        CmdInspectTactician(p2);
    }
    [Command]
    void CmdInspectTactician(ScenePlayer p2){
        requestInspectServer.Invoke(this, p2);
    }
    public void ArenaDuelAccepted(ScenePlayer p2){
        CmdChallengePlayer(p2);
        PlayerClickInteraction pci = GetComponent<PlayerClickInteraction>();
        pci.ChallengerSubmit(p2);
    }
    [Command]
    void CmdChallengePlayer(ScenePlayer p2){
        requestChallengeServer.Invoke(this, p2);
    }
    [TargetRpc]
    public void TargetAskChallenge(ScenePlayer challenger){
        //challenger ask
        //requestChallengeTarget.Invoke(challenger);
        PlayerClickInteraction pci = GetComponent<PlayerClickInteraction>();
        pci.ChallengeRequest(challenger);
    }
    public void ChallengeResponse(bool accepted, ScenePlayer challenger){
        if(accepted){
            CmdChallengePlayerAccepted(challenger);
        } else {
            CmdChallengePlayerDenied(challenger);
        }
    }
    public void CancelChallengeRequest(ScenePlayer opponentRequest){
        if(opponentRequest != null){
            CmdChallengeRequest(opponentRequest);
        }
    }
    [Command]
    void CmdChallengeRequest(ScenePlayer opponentRequest){
        cancelChallengeRequestServer.Invoke(opponentRequest, this);
    }
    [Command]
    void CmdChallengePlayerAccepted(ScenePlayer challenger){
        decisionChallengeServer.Invoke(this, challenger, true);
    }
    [Command]
    void CmdChallengePlayerDenied(ScenePlayer challenger){
        decisionChallengeServer.Invoke(this, challenger, false);
    }
    [TargetRpc]
    public void TargetDeniedResponse(){
        PlayerClickInteraction pci = GetComponent<PlayerClickInteraction>();
        pci.DeniedChallengeRequest();
    }
    [Server]
    public void ServerStartArenaDuel(ScenePlayer pTwo){
        matchID = MatchMaker.GetRandomMatchID();
        //print("called CMDSOLOGAME ************* on server");
        if (MatchMaker.instance.HostArenaDuel(matchID, this, pTwo, false)) {
            Debug.Log($"Game {matchID} Solo created successfully");
            MatchMaker.instance.CreateDuelMatch(matchID, currentMatch, this, pTwo);
        } 
        TargetTurnOffWaiting();
    }
    [TargetRpc]
    void TargetTurnOffWaiting(){
        TurnOffTactInteractionUI.Invoke();
    }
    public void LeftLobby(){
        CmdLeftLobby();
    }
    [Command]
    void CmdLeftLobby(){
        CanUpdateOVM = true;
    }
    public void JoinedLobby(){
        CmdJoinedLobby();
    }
    [Command]
    void CmdJoinedLobby(){
        CanUpdateOVM = false;
    }
    public void SoloGame(){
        CmdSoloGame();
    }
    [Command]
    void CmdSoloGame(){
        matchID = MatchMaker.GetRandomMatchID();
        //print("called CMDSOLOGAME ************* on server");
        if (MatchMaker.instance.HostSoloGame(matchID, this, false, out playerIndex)) {
            Debug.Log($"Game {matchID} Solo created successfully");
        } 
    }
    /*
        DISCONNECT MATCH
    */
    [TargetRpc]
    public void TargetPassLeader(bool leader){
        Debug.Log($"Server passed our client leadership of the match");
        UILobby.instance.LeaderToggle(leader);
    }
    

    public void DisconnectMatchLobby(){
        CmdDisconnectFromMatchLobby();
    }
    [Command]
    void CmdDisconnectFromMatchLobby(){
        MatchMaker.instance.PlayerDisconnectedFromLobby(this, matchID);
        if(currentScene == "OVM")
        CanUpdateOVM = true;
    
        //ServerDisconnectFromMatchLobby();
    }
    [Server]
    void ServerDisconnectFromMatchLobby(){
        //matchID = string.Empty;
        //playerIndex = 0;
        //RpcDisconnectFromMatchLobby();
    }
    [TargetRpc]
    public void TargetDisconnectFromMatchLobby(ScenePlayer sPlayer){
        DisconnectFromMatchLobby(sPlayer);
    }
    [ClientRpc]
    public void rpcDisconnectFromMatchLobby(){
        DisconnectFromMatchLobby(this);
    }
    void DisconnectFromMatchLobby(ScenePlayer sPlayer){
        RemoveLobby.Invoke(sPlayer);
        PlayerDisconnectedFromMatchLobby.Invoke();
    }
    [Server]
    void Logout(){
        DateTime LogoutTime = DateTime.Now;
        string time = LogoutTime.ToString();
        float x = this.gameObject.transform.position.x;
        float y = this.gameObject.transform.position.y;
    }
    [TargetRpc]
    public void TargetCloseGameCompletely(){
        Application.Quit();
    }
    [ClientRpc]
    void RpcDisconnectGame(){
        ClientDisconnect();
    }
    void ClientDisconnect(){
        if(playerLobbyUI != null){
            if (!isServer){
                Destroy(playerLobbyUI);
            } else {
                playerLobbyUI.SetActive(false);
            }
        }
    }
    [ClientRpc]
    public void RpcCastBar(MovingObject pc, float duration, string mode, MovingObject target, ScenePlayer owner, string spell){
        //print($"Clicked cast location is {mousePosition}");
        AudioMgr audiomgr = pc.GetComponent<AudioMgr>();
        string soundRequest = string.Empty;
        //if(spell == "Aimed Shot"){
        //    soundRequest = "BowDrawSFX";
        //} else {
        //    soundRequest = "casting";
        //}
        //audiomgr.PlaySound(soundRequest);
        GameObject tileObject = Instantiate(tilePrefab, new Vector3(target.transform.position.x, target.transform.position.y, 0f), Quaternion.identity);
        MoveableTile castTile = tileObject.GetComponent<MoveableTile>();
        GameObject castBarObject = Instantiate(castBarPrefab, new Vector3(pc.transform.position.x, pc.transform.position.y - .8f, 0f) , Quaternion.identity);
        Castbar castbar = castBarObject.GetComponent<Castbar>();
        (int type, int element) = StatAsset.Instance.GetSpellType(spell);
        // type 0 = auto attack
        // type 1 = burn spell
        // type 2 = heal spell
        // type 3 = buff spell
        // type 4 = debuff spell
        // type 5 = sefl casted
        // type 6 = cc
        string colorHex = string.Empty;
        switch (type)
        {
            case 1:
                colorHex = "FF0000"; // Red
                break;
            case 2:
                colorHex = "00FF00"; // Green
                break;
            case 3:
                colorHex = "0000FF"; // Blue
                break;
            case 4:
                colorHex = "FFC0CB"; // Pink
                break;
            case 6:
                colorHex = "FFFF00"; // Yellow
                break;
            case 8:
                colorHex = "FF0000"; // Red
                break;
            case 9:
                colorHex = "00FF00"; // Green
                break;
        }
        castTile.StartCoroutine(castTile.SelectedAbilityTile(duration, colorHex));
        castbar.SetMob(pc, duration, mode, target, castTile, owner);
        //pc.PlayCastSound();
    }
    [ClientRpc]
    public void RpcCastBarAOESPELL(MovingObject pc, float duration, string mode, Vector2 target, ScenePlayer owner){
        //print($"Clicked cast location is {mousePosition}");
        
        string Spell = string.Empty;
        if(mode == CastingQ){
            Spell = pc.SpellQ;
        }
        if(mode == CastingE){
            Spell = pc.SpellE;
        }
        if(mode == CastingR){
            Spell = pc.SpellR;
        }
        if(mode == CastingF){
            Spell = pc.SpellF;
        }
        var nameMatch = Regex.Match(Spell, @"^\D*");
        string spell = nameMatch.Value.Trim(); 
        int _spellRank = 1;
        // Extract spell rank
        var rankMatch = Regex.Match(Spell, @"\d+$");
        if (rankMatch.Success) {
            _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
        }
        float radius = AoeRadius(spell);
        (int type, int element) = StatAsset.Instance.GetSpellType(spell);
        // type 0 = auto attack
        // type 1 = burn spell
        // type 2 = heal spell
        // type 3 = buff spell
        // type 4 = debuff spell
        // type 5 = sefl casted
        // type 6 = cc
        float selfCast = StatAsset.Instance.SelfCasted(spell);
        bool selfCasted = false;
        if(selfCast == 1){
            target = pc.transform.position;
        }
        string colorHex = string.Empty;
        switch (type)
        {
            case 1:
                colorHex = "FF0000"; // Red
                break;
            case 2:
                colorHex = "00FF00"; // Green
                break;
            case 3:
                colorHex = "0000FF"; // Blue
                break;
            case 4:
                colorHex = "FFC0CB"; // Pink
                break;
            case 6:
                colorHex = "FFFF00"; // Yellow
                break;
        }
        GameObject tileObject = Instantiate(tilePrefab, target, Quaternion.identity);
        MoveableTile castTile = tileObject.GetComponent<MoveableTile>();
        GameObject castBarObject = Instantiate(castBarPrefab, new Vector3(pc.transform.position.x, pc.transform.position.y - .8f, 0f) , Quaternion.identity);
        Castbar castbar = castBarObject.GetComponent<Castbar>();
        if(spell == "Tail Whip" || spell == "Fire Breath"){
            castTile.StartCoroutine(castTile.SelectedAbilityTileAoeCone(duration, radius, colorHex, target));
        } else {
            castTile.StartCoroutine(castTile.SelectedAbilityTileAoe(duration, radius, colorHex));
        }
        castbar.SetTargetPositionAOESPELL(pc, duration, mode, target, castTile, owner);
        //pc.PlayCastSound();
    }
    float AoeRadius(string spell){
        float radius = 0f;
        if(spell == "Fireball"){
            radius = 3f;
            return radius;
        }
        if(spell == "Ice Blast"){
            radius = 3f;
            return radius;
        }
        if(spell == "Gravity Stun"){
            radius = 4f;
            return radius;
        }
        if(spell == "Group Heal"){
            radius = 3f;
            return radius;
        }
        if(spell == "Teleport"){
            radius = 1f;
            return radius;
        }
        if(spell == "Meteor Shower"){
            radius = 5f;
            return radius;
        }
        if(spell == "Blizzard"){
            radius = 3f;
            return radius;
        }
        if(spell == "Tornado"){
            radius = 1f;
            return radius;
        }
        if(spell == "Dash"){
            radius = 6f;
            return radius;
        }
        if (spell == "Solar Flare"){
            radius = 4f;
            return radius;
        }
        if (spell == "Undead Protection"){
            radius = 7f;
            return radius;
        }
        if(spell == "Intimidating Roar"){
            radius = 4f;
            return radius;
        }
        if (spell == "Resist Magic"){
            radius = 4f;
            return radius;
        }
        if(spell == "Turn Undead"){
            radius = 5f;
            return radius;
        }
        return radius;
    }
    
  
    [ClientRpc]
    public void  RpcCastBarHarvesting(MovingObject pc, float duration, string mode, ScenePlayer owner, GameObject harvestDrop, string type){
    	pc.NonRPCCancelCastAnimation();
        //if(pc != null){
    	//	CancelCast.Invoke(pc);
        //}
        //print($"Clicked cast location is {mousePosition}");
        GameObject castBarObject = Instantiate(castBarPrefab, new Vector3(pc.transform.position.x, pc.transform.position.y - 1.5f, 0f) , Quaternion.identity);
        Castbar castbar = castBarObject.GetComponent<Castbar>();
        castbar.SetHarvest(pc, mode, duration, harvestDrop, owner);
        pc.PlayerSetBool(type, true);
        
        //pc.PlayCastSound();
    }
    [Command]
    void CmdBuildHarvesterCastBar(MovingObject pc, string harvestMode, ScenePlayer owner, GameObject harvestingNode, bool rightFace){
        //get pc stats for everything or atlest just agil for casttime
        //if(pc.Casting){
        //    pc.ServerStopCasting();
        //}
        if(harvestingNode == null){
            return;
        }
        pc.HarvestStart();
        string boolOperation = string.Empty;
        if(harvestMode == "Harvest wood"){
            boolOperation = "chopping";
        }
        if(harvestMode == "Harvest stone"){
            boolOperation = "prospecting";
        }
        if(harvestMode == "Harvest ore"){
            boolOperation = "mining";
        }
        if(harvestMode == "Harvest fiber"){
            boolOperation = "foraging";
        }
        if(harvestMode == "Harvest hide"){
            boolOperation = "skinning";
        }
        int agility = pc.GetAgility();
        int strength = pc.GetStrength();
        int fort = pc.GetFortitude();
        int arcana = pc.GetArcana();
        int mining = 1;
        int skinning = 1;
        int prospecting = 1;
        int foraging = 1;
        int woodCutting = 1;
        PlayerCharacter playerChar = pc.GetComponent<PlayerCharacter>();
        if(playerChar){
            foreach(var sheet in owner.GetInformationSheets()){
                if(sheet.CharacterID == playerChar.CharID){
                    mining = sheet.miningSkill;
                    skinning = sheet.skinningSkill;
                    prospecting = sheet.prospectingSkill;
                    foraging = sheet.foragingSkill;
                    woodCutting = sheet.woodCuttingSkill;
                    break;
                }
            }
            int skill = 1;
            if(harvestMode == "Harvest wood"){
                skill = woodCutting;
            }
            if(harvestMode == "Harvest stone"){
                skill = prospecting;
            }
            if(harvestMode == "Harvest ore"){
                skill = mining;
            }
            if(harvestMode == "Harvest fiber"){
                skill = foraging;
            }
            if(harvestMode == "Harvest hide"){
                skill = skinning;
            }
            int PASSIVE_Agility = 0;
            int harvestHasteLvl = 0;
            for(int _char = 0; _char < GetInformationSheets().Count; _char++){
                if(GetInformationSheets()[_char].CharacterID == playerChar.CharID){
                    for(int ability = 0; ability < GetInformationSheets()[_char].CharSpellData.Count; ability++){
                        if(GetInformationSheets()[_char].CharSpellData[ability].Key == "SouthT2MiddleSkill"){
                            var abilityRankString = System.Text.RegularExpressions.Regex.Match(GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                            if (abilityRankString.Success) {
                                int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                                PASSIVE_Agility = abilityRank;
                            }
                        }
                        if(GetInformationSheets()[_char].CharSpellData[ability].Key == "SouthT1Skill"){
                            var abilityRankString = System.Text.RegularExpressions.Regex.Match(GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                            if (abilityRankString.Success) {
                                int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                                harvestHasteLvl = abilityRank;
                            }
                        }
                    }
                    break;
                }
            }
            agility += PASSIVE_Agility;
            //rare chance
            pc.Casting = true;
            float skillVariant = 1-((float)skill/200) - ((float)agility/750);
            float duration = 10 * skillVariant;
            //muultiplay by harvest buff
            Vector3 movementDirection = harvestingNode.transform.position - pc.transform.position;
            //Vector2 direction = new Vector2(targetPosition.x - transform.position.x, targetPosition.y - transform.position.y);
        float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
        // Determine the direction string based on the angle
        string directionString = pc.GetDirectionString(angle);
        Debug.Log("Facing direction: " + directionString);
        if(pc.GetFacingDirection() != directionString){
            pc.SetFacingDirection(directionString);
	    }
           
            pc.RpcTurnToFaceTarget(harvestingNode.transform.position);
		    //bool newRightFace = movementDirection.x >= 0;
		    //if (newRightFace != rightFace)
		    //{
		    //	rightFace = newRightFace;
		    //	pc.RpcUpdateFacingDirection(newRightFace);
		    //}
            if(harvestHasteLvl > 0){
                //UtilityDescription.text = $"Increases harvest speed by {(50f + (2f * rankModifier)).ToString("F2")} %";
                float hasteHarvest = 50f + (2f * harvestHasteLvl);
                hasteHarvest /= 100f;
                
                float newDuration = duration / hasteHarvest;
                duration = newDuration;
            }

            RpcCastBarHarvesting(pc, duration, harvestMode, owner, harvestingNode, boolOperation);
            //pc.ClientSetBool(boolOperation, true);

        }
        
    }
    
    // we need to build item and destroy the items effected
    [Command]
    void CmdBuildCastBar(MovingObject pc, string mode, MovingObject target, ScenePlayer owner, Vector2 MousePosition){
        if(pc.Invisibility){
		    pc.ServerRemoveStatus("Stealthed", "Invisibility", true);
        }
        string _spell = string.Empty;
        if(mode == CastingQ){
            _spell = pc.SpellQ;
        }
        if(mode == CastingE){
            _spell = pc.SpellE;
        }
        if(mode == CastingR){
            _spell = pc.SpellR;
        }
        if(mode == CastingF){
            _spell = pc.SpellF;
        }
        pc.Casting = true;
        var nameMatch = System.Text.RegularExpressions.Regex.Match(_spell, @"^\D*");
        string spell = nameMatch.Value.Trim(); 
        int _spellRank = 1;
        // Extract spell rank
        var rankMatch = System.Text.RegularExpressions.Regex.Match(_spell, @"\d+$");
        if (rankMatch.Success) {
            _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
        }
        float selfCast = StatAsset.Instance.SelfCasted(spell);
        bool selfCasted = false;
        if(selfCast == 1){
            selfCasted = true;
        }
        int lvl = 1;
        string ownerName = string.Empty;
        string targetName = string.Empty;
        PlayerCharacter pcCheck = pc.GetComponent<PlayerCharacter>();
        if(pcCheck){
            lvl = pcCheck.Level;
           ownerName = pcCheck.CharacterName;
        } else {
            Mob mobCheck = pc.GetComponent<Mob>();
			ownerName = StatAsset.Instance.GetMobName(StatAsset.Instance.GetEnemyToPrefabMapping(), mobCheck.NAME);
        }
        if(target != null){
            PlayerCharacter pcChecktarget = target.GetComponent<PlayerCharacter>();
            if(pcChecktarget){
               targetName = pcChecktarget.CharacterName;
            } else {
                Mob mobCheckTarget = target.GetComponent<Mob>();
		    	targetName = StatAsset.Instance.GetMobName(StatAsset.Instance.GetEnemyToPrefabMapping(), mobCheckTarget.NAME);
            }
        }
        if(!selfCasted){
            if(target == null){
                Vector2 direction = new Vector2(pc.transform.position.x - MousePosition.x, pc.transform.position.y - MousePosition.y);
    	        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                string directionString = pc.GetDirectionString(angle);
                if(pc.GetFacingDirection() != directionString){
		        	pc.SetFacingDirection(directionString);
		        	//pc.RpcSetDirectionFacing(directionString);
		        }
                pc.RpcTurnToFaceTarget(MousePosition);
            } else {
                Vector2 direction = new Vector2(pc.transform.position.x - target.transform.position.x, pc.transform.position.y - target.transform.position.y);
    	        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                string directionString = pc.GetDirectionString(angle);
                if(pc.GetFacingDirection() != directionString){
		        	pc.SetFacingDirection(directionString);
		        	//pc.RpcSetDirectionFacing(directionString);
		        }
                pc.RpcTurnToFaceTarget(target.transform.position);
            }
        }
        
        if(!StatAsset.Instance.SkillShot(spell)){
            MousePosition = Vector2.zero;
        }
        pc.RpcCastingSpell(spell);
        //Vector2 direction = new Vector2(MousePosition.x - pc.transform.position.x, MousePosition.y - pc.transform.position.y);
        //float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        // Determine the direction string based on the angle
        
		//string ourName = string.Empty;
		//Mob mob = GetComponent<Mob>();
		//PlayerCharacter pc = GetComponent<PlayerCharacter>();
		//if(mob){
		//	ourName = mob.NAME;
		//}
		//if(pc){
		//	ourName = pc.CharacterName;
		//}
        //Debug.Log(ourName + " is facing direction: " + directionString);
		
		//pc.AddRpcCall(null, null, false, true, spell, null, false, ownerName, targetName, pc, target, MousePosition);
        float castTime = StatAsset.Instance.GetCastTime(spell, lvl, _spellRank);
        //print($"building cast bar for {pc.CharacterName} who is casting spell on keybind {mode}");
        if(mode == CastingQ){
            if(pc.cur_mp >= StatAsset.Instance.GetSpellCost(spell)){
                pc.Casting = true;
                //if aoe do this 
                if(StatAsset.Instance.SkillShot(spell) || selfCasted){
                    RpcCastBarAOESPELL(pc, castTime, mode, MousePosition, owner);
                    return;
                }
                RpcCastBar(pc, castTime, mode, target, owner, spell);
                return;
            }
        }
        if(mode == CastingE){
            if(pc.cur_mp >= StatAsset.Instance.GetSpellCost(spell)){
                pc.Casting = true;
                //if aoe do this 
                if(StatAsset.Instance.SkillShot(spell) || selfCasted){
                    RpcCastBarAOESPELL(pc, castTime, mode, MousePosition, owner);
                    return;
                }
                RpcCastBar(pc, castTime, mode, target, owner, spell);
                return;
            }
        }
        if(mode == CastingR){
            if(pc.cur_mp >= StatAsset.Instance.GetSpellCost(spell)){
                pc.Casting = true;
                //if aoe do this 
                if(StatAsset.Instance.SkillShot(spell) || selfCasted){
                    RpcCastBarAOESPELL(pc, castTime, mode, MousePosition, owner);
                    return;
                }
                RpcCastBar(pc, castTime, mode, target, owner, spell);
                return;
            }
        }
        if(mode == CastingF){
            if(pc.cur_mp >= StatAsset.Instance.GetSpellCost(spell)){
                pc.Casting = true;
                //if aoe do this 
                if(StatAsset.Instance.SkillShot(spell) || selfCasted){
                    RpcCastBarAOESPELL(pc, castTime, mode, MousePosition, owner);
                    return;
                }
                RpcCastBar(pc, castTime, mode, target, owner, spell);
                return;
            }
        }
    }
    void CastHarvest(CastingSpellTargetsOrMouse casting){
        if(casting.character.GetComponent<NetworkIdentity>().hasAuthority){
            if(casting.owner == this){
                //Process the harvest now
                PlayerCharacter pcCheck = casting.character.GetComponent<PlayerCharacter>();
                if(pcCheck == null){
                    return;
                }
                if(casting.HarvestObject == null){
                    print("CASTING HARVEST OBJECT NODE DIDNT EXIST TURNING OFF ANIMATION FOR IT");
                    casting.character.NoHitsLeftOnNode();
                    return;
                }
                if(casting.mode == "Harvest hide"){
                    LeatherNodeDrop leatherNode = casting.HarvestObject.GetComponent<LeatherNodeDrop>();
                    if(leatherNode){
                        pcCheck.CmdHarvestLeather(leatherNode);
                    }
                }
                if(casting.mode == "Harvest fiber"){
                    ClothNodeDrop ClothNode = casting.HarvestObject.GetComponent<ClothNodeDrop>();
                    if(ClothNode){
                        pcCheck.CmdHarvestCloth(ClothNode);
                    }
                }
                if(casting.mode == "Harvest ore"){
                    OreNodeDrop oreNode = casting.HarvestObject.GetComponent<OreNodeDrop>();
                    if(oreNode){
                        pcCheck.CmdHarvestOre(oreNode);
                    }
                }
                if(casting.mode == "Harvest stone"){
                    StoneNodeDrop StoneNode = casting.HarvestObject.GetComponent<StoneNodeDrop>();
                    if(StoneNode){
                        pcCheck.CmdHarvestStone(StoneNode);
                    }
                }
                if(casting.mode == "Harvest wood"){
                    TreeNodeDrop TreeNode = casting.HarvestObject.GetComponent<TreeNodeDrop>();
                    if(TreeNode){
                        pcCheck.CmdHarvestTree(TreeNode);
                    }
                }
            }
        }
    }
    void CastSpell(CastingSpellTargetsOrMouse casting){
        if(casting.character.GetComponent<NetworkIdentity>().hasAuthority){
            if(casting.owner == this){
                PlayerCharacter pcCheck = casting.character.GetComponent<PlayerCharacter>();
                if(casting.AOE){
                    if(pcCheck){
                        pcCheck.CmdCastAOESpell(casting.mode, casting.Clicked);
                    } else {
                        casting.character.CmdCastAOESpellMOB(casting.mode, casting.Clicked);
                    }
                } else {
                    if(StatAsset.Instance.InSpellRange(casting.character, casting.target, casting.mode, new Vector2(), out float finalRange)){
                        if(pcCheck){
                            pcCheck.CmdCastSpell(casting.mode, casting.target);
                        } else {
                            casting.character.CmdCastSpellMOB(casting.mode, casting.target);
                        }   
                    }
                }
            }
        }
    }
    public (string, int) SeparateSpell(string spell)
{
    var match = Regex.Match(spell, @"(.*\D)(\d+)$");

    string spellName = spell;  // default values, in case the regex doesn't match
    int level = 1;

    if (match.Success) 
    {
        spellName = match.Groups[1].Value.Trim();
        level = int.Parse(match.Groups[2].Value);
    }

    return (spellName, level);
}
public void AutoCastForCharacter(MovingObject castingCharacter, MovingObject target, string mode, Vector2 mousePosition){
    StartCoroutine(AutoCastingRangeFinder(castingCharacter, target, mode, mousePosition));
}
IEnumerator AutoCastingRangeFinder(MovingObject castingCharacter, MovingObject target, string mode, Vector2 mousePosition){
    yield return new WaitForSeconds(.1f);
print($"Auto casting for character with {mode} as the mode for {castingCharacter.gameObject.name}");
    if(castingCharacter.Casting){
        castingCharacter.CmdCancelSpell();
    }
    PlayerCharacter pc = castingCharacter.GetComponent<PlayerCharacter>();
    string _spell = string.Empty;
    
    if(mode == CastingQ){
        _spell = castingCharacter.SpellQ;
    }
    if(mode == CastingE){
        _spell = castingCharacter.SpellE;
    }
    if(mode == CastingR){
        _spell = castingCharacter.SpellR;
    }
    if(mode == CastingF){
        _spell = castingCharacter.SpellF;
    }
    var nameMatch = System.Text.RegularExpressions.Regex.Match(_spell, @"^\D*");
    string spell = nameMatch.Value.Trim(); 
    int _spellRank = 1;
    // Extract spell rank
    var rankMatch = System.Text.RegularExpressions.Regex.Match(_spell, @"\d+$");
    if (rankMatch.Success) {
        _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
    }
    if(castingCharacter.cur_mp < StatAsset.Instance.GetSpellCost(spell)){
        ImproperCheckText.Invoke($"Not enough magic points to cast {spell}");
        yield break;

    }
    if(target){
       if(!HasLineOfSight(castingCharacter.transform.position, target.transform.position)){
            ImproperCheckText.Invoke($"{spell} location is not in line of sight");
            yield break;
        }
        if(!StatAsset.Instance.InSpellRange(castingCharacter, target, mode, mousePosition, out float finalRange)){
            print($"{finalRange} is max range of this spell");
            castingCharacter.CmdMoveToCast(target, mode, finalRange, mousePosition);
            yield break;
        } 
    } else {
        if(!HasLineOfSight(castingCharacter.transform.position, mousePosition)){
            ImproperCheckText.Invoke($"{spell} location is not in line of sight");
            yield break;
        }
        if(!StatAsset.Instance.InSpellRange(castingCharacter, null, mode, mousePosition, out float finalRange)){
            print($"{finalRange} is max range of this spell");
            castingCharacter.CmdMoveToCast(target, mode, finalRange, mousePosition);
            yield break;
        }
    }
    //friendly spell
    float castTime = StatAsset.Instance.GetCastTime(spell, pc.Level, _spellRank);
    if(castTime == 0f){
        if(target){
            pc.CmdInstantCastSpell(mode, target);
        } else {
            if(StatAsset.Instance.SkillShot(spell)){
                //CmdBuildCastBar(castingCharacter, mode, null, this, mousePosition);
                pc.CmdInstantCastSpellSelfCast(mode, mousePosition);
            }
        }
    } else {
        if(StatAsset.Instance.SkillShot(spell)){
            CmdBuildCastBar(castingCharacter, mode, null, this, mousePosition);
        } else {
            CmdBuildCastBar(castingCharacter, mode, target, this, mousePosition);
        }
    }
}
public void CharacterCastingHarvest(MovingObject castingCharacter, GameObject targetNode){
    TreeNodeDrop treeNode = targetNode.GetComponent<TreeNodeDrop>();
    StoneNodeDrop StoneNode = targetNode.GetComponent<StoneNodeDrop>();
    OreNodeDrop OreNode = targetNode.GetComponent<OreNodeDrop>();
    ClothNodeDrop ClothNode = targetNode.GetComponent<ClothNodeDrop>();
    LeatherNodeDrop LeatherNode = targetNode.GetComponent<LeatherNodeDrop>();
        PlayerCharacter PC = castingCharacter.GetComponent<PlayerCharacter>();
    if(targetNode == null){
        return;
    }
    string harvestType = string.Empty;
    if(treeNode){
        harvestType = "Harvest wood";
         if(PC){
                        bool hasAxe = false;
                        int itemCount = 0;
                        foreach(var sheet in InformationSheets){
                            if(sheet.CharacterID == PC.CharID){
                            
                                float _currentWeight = 0f;
                                int gearStrength = 0;
                                string _class = string.Empty;
                                int _level = 1;
                                string _CORE = string.Empty;
                                if(PC.stamina >= 0){
                                    ImproperCheckText.Invoke($"Not enough staminia to perform this action");
                                    return;
                                }
                                foreach(var inventoryItem in sheet.CharInventoryData){
                                    if(inventoryItem.Value.Deleted){
                                        continue;
                                    }
                                    _currentWeight += float.Parse(inventoryItem.Value.GetWeight()) * inventoryItem.Value.amount;
                                    if(inventoryItem.Value.EQUIPPED){
                                        if(inventoryItem.Value.GetItemType() == ItemSelectable.ItemType.CharacterBag){
                                            if(inventoryItem.Value.GetItemName() == "Bag"){
                                                gearStrength += 30;
                                            }
                                        }
                                        if(!string.IsNullOrEmpty(inventoryItem.Value.STRENGTH_item)){
                                            if(int.Parse(inventoryItem.Value.STRENGTH_item) > 0){
                                                gearStrength += int.Parse(inventoryItem.Value.STRENGTH_item);
                                            }
                                        }
                                    }
                                }
                                foreach(var stat in sheet.CharStatData){
                                    if(stat.Key == "CORE"){
                                        _CORE = stat.Value;
                                    }
                                    if(stat.Key == "Class"){
                                        _class = stat.Value; 
                                    }
                                    if(stat.Key == "LVL"){
                                        _level = int.Parse(stat.Value); 
                                    }
                                }
                                int PASSIVE_Strength = 0;
                                for(int _char = 0; _char < GetInformationSheets().Count; _char++){
                                    if(GetInformationSheets()[_char].CharacterID == PC.CharID){
                                        for(int ability = 0; ability < GetInformationSheets()[_char].CharSpellData.Count; ability++){
                                            if(GetInformationSheets()[_char].CharSpellData[ability].Key == "SouthT2LeftSkill"){
                                                var abilityRankString = System.Text.RegularExpressions.Regex.Match(GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                                                if (abilityRankString.Success) {
                                                    int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                                                    PASSIVE_Strength = abilityRank;
                                                    break;
                                                }
                                            }
                                        }
                                        break;
                                    }
                                }
                                (int baseStrength, int baseAgility, int baseFortitude, int baseArcana) = StatAsset.Instance.GetCharacterStats(_class, _level, _CORE);
                                gearStrength += baseStrength + PASSIVE_Strength;    
                                if(_currentWeight >= gearStrength){
                                    //Need to put error text here ****
                                    ImproperCheckText.Invoke("This character cannot carry anymore weight");
                                    return;
                                }
                                foreach(var item in sheet.CharInventoryData){
                                    if(item.Value.Deleted || item.Value.amount == 0){
                                        continue;
                                    }
                                    if(item.Value.Item_Name == "Lumberjack Axe" && int.Parse(item.Value.Durability) >= 1){
                                        hasAxe = true;
                                    }
                                    if(!item.Value.EQUIPPED){//Hide Ore Stone Wood
                                        itemCount ++;
                                    }
                                }
                                if(itemCount <= 18){
                                    //can build the rare now lets roll to see if we get
                                } else {
                                    ImproperCheckText.Invoke($"Need atleast 2 empty inventory spaces to chop this tree");
                                    return; //no room
                                }
                                break;
                            }
                        }
                        if(hasAxe){
                        } else {
                            ImproperCheckText.Invoke($"A useable lumberjack axe is needed to cut this tree down");
                            return;
                        }
                    }
    }
    if(StoneNode){
        harvestType = "Harvest stone";
        if(PC){
                        bool hasStoneHammer = false;
                        int itemCount = 0;
                        foreach(var sheet in InformationSheets){
                            if(sheet.CharacterID == PC.CharID){
                                float _currentWeight = 0f;
                                int gearStrength = 0;
                                string _class = string.Empty;
                                int _level = 1;
                                string _CORE = string.Empty;
                                if(PC.stamina >= 0){
                                    ImproperCheckText.Invoke($"Not enough staminia to perform this action");
                                    return;
                                }
                                foreach(var inventoryItem in sheet.CharInventoryData){
                                    if(inventoryItem.Value.Deleted){
                                        continue;
                                    }
                                    _currentWeight += float.Parse(inventoryItem.Value.GetWeight()) * inventoryItem.Value.amount;
                                    if(inventoryItem.Value.EQUIPPED){
                                        if(inventoryItem.Value.GetItemType() == ItemSelectable.ItemType.CharacterBag){
                                            if(inventoryItem.Value.GetItemName() == "Bag"){
                                                gearStrength += 30;
                                            }
                                        }
                                        if(!string.IsNullOrEmpty(inventoryItem.Value.STRENGTH_item)){
                                            if(int.Parse(inventoryItem.Value.STRENGTH_item) > 0){
                                                gearStrength += int.Parse(inventoryItem.Value.STRENGTH_item);
                                            }
                                        }
                                    }
                                }
                                foreach(var stat in sheet.CharStatData){
                                    if(stat.Key == "CORE"){
                                        _CORE = stat.Value;
                                    }
                                    if(stat.Key == "Class"){
                                        _class = stat.Value; 
                                    }
                                    if(stat.Key == "LVL"){
                                        _level = int.Parse(stat.Value); 
                                    }
                                }
                                int PASSIVE_Strength = 0;
                                for(int _char = 0; _char < GetInformationSheets().Count; _char++){
                                    if(GetInformationSheets()[_char].CharacterID == PC.CharID){
                                        for(int ability = 0; ability < GetInformationSheets()[_char].CharSpellData.Count; ability++){
                                            if(GetInformationSheets()[_char].CharSpellData[ability].Key == "SouthT2LeftSkill"){
                                                var abilityRankString = System.Text.RegularExpressions.Regex.Match(GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                                                if (abilityRankString.Success) {
                                                    int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                                                    PASSIVE_Strength = abilityRank;
                                                    break;
                                                }
                                            }
                                        }
                                        break;
                                    }
                                }
                                (int baseStrength, int baseAgility, int baseFortitude, int baseArcana) = StatAsset.Instance.GetCharacterStats(_class, _level, _CORE);
                                gearStrength += baseStrength + PASSIVE_Strength;             
                                if(_currentWeight >= gearStrength){
                                    //Need to put error text here ****
                                    ImproperCheckText.Invoke("This character cannot carry anymore weight");
                                    return;
                                }
                                foreach(var item in sheet.CharInventoryData){
                                    if(item.Value.Deleted || item.Value.amount == 0){
                                        continue;
                                    }
                                    if(item.Value.Item_Name == "Stone Hammer" && int.Parse(item.Value.Durability) >= 1){
                                        hasStoneHammer = true;
                                    }
                                if(!item.Value.EQUIPPED){//Hide Ore Stone Wood
                                        itemCount ++;
                                    }
                                }
                                if(itemCount <= 18){
                                    //can build the rare now lets roll to see if we get
                                } else {
                                    ImproperCheckText.Invoke($"Need atleast 2 empty inventory spaces to harvest this stone");
                                    return; //no room
                                }
                                break;
                            }
                        }
                        if(hasStoneHammer){
                        } else {
                            ImproperCheckText.Invoke($"A usable stone hammer is required to harvest this stone rock");
                            return;
                        }
                    }
    }
    if(OreNode){
        harvestType = "Harvest ore";
        if(PC){
                        bool hasPickaxe = false;
                        int itemCount = 0;
                        foreach(var sheet in InformationSheets){
                            if(sheet.CharacterID == PC.CharID){
                                float _currentWeight = 0f;
                                int gearStrength = 0;
                                string _class = string.Empty;
                                int _level = 1;
                                string _CORE = string.Empty;
                                if(PC.stamina >= 0){
                                    ImproperCheckText.Invoke($"Not enough staminia to perform this action");
                                    return;
                                }
                                foreach(var inventoryItem in sheet.CharInventoryData){
                                    if(inventoryItem.Value.Deleted || inventoryItem.Value.amount == 0){
                                        continue;
                                    }
                                    
                                    _currentWeight += float.Parse(inventoryItem.Value.GetWeight()) * inventoryItem.Value.amount;
                                    if(inventoryItem.Value.EQUIPPED){
                                        if(inventoryItem.Value.GetItemType() == ItemSelectable.ItemType.CharacterBag){
                                            if(inventoryItem.Value.GetItemName() == "Bag"){
                                                gearStrength += 30;
                                            }
                                        }
                                        if(!string.IsNullOrEmpty(inventoryItem.Value.STRENGTH_item)){
                                            if(int.Parse(inventoryItem.Value.STRENGTH_item) > 0){
                                                gearStrength += int.Parse(inventoryItem.Value.STRENGTH_item);
                                            }
                                        }
                                    }
                                }
                                foreach(var stat in sheet.CharStatData){
                                    if(stat.Key == "CORE"){
                                        _CORE = stat.Value;
                                    }
                                    if(stat.Key == "Class"){
                                        _class = stat.Value; 
                                    }
                                    if(stat.Key == "LVL"){
                                        _level = int.Parse(stat.Value); 
                                    }
                                }
                                int PASSIVE_Strength = 0;
                                for(int _char = 0; _char < GetInformationSheets().Count; _char++){
                                    if(GetInformationSheets()[_char].CharacterID == PC.CharID){
                                        for(int ability = 0; ability < GetInformationSheets()[_char].CharSpellData.Count; ability++){
                                            if(GetInformationSheets()[_char].CharSpellData[ability].Key == "SouthT2LeftSkill"){
                                                var abilityRankString = System.Text.RegularExpressions.Regex.Match(GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                                                if (abilityRankString.Success) {
                                                    int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                                                    PASSIVE_Strength = abilityRank;
                                                    break;
                                                }
                                            }
                                        }
                                        break;
                                    }
                                }
                                (int baseStrength, int baseAgility, int baseFortitude, int baseArcana) = StatAsset.Instance.GetCharacterStats(_class, _level, _CORE);
                                gearStrength += baseStrength + PASSIVE_Strength;            
                                if(_currentWeight >= gearStrength){
                                    //Need to put error text here ****
                                    ImproperCheckText.Invoke("This character cannot carry anymore weight");
                                    return;
                                }
                                foreach(var item in sheet.CharInventoryData){
                                    if(item.Value.Deleted || item.Value.amount == 0){
                                        continue;
                                    }
                                    if(item.Value.Item_Name == "Pickaxe" && int.Parse(item.Value.Durability) >= 1){
                                        hasPickaxe = true;
                                    }
                                if(!item.Value.EQUIPPED){//Hide Ore Stone Wood
                                        itemCount ++;
                                    }
                                }
                                if(itemCount <= 18){
                                    //can build the rare now lets roll to see if we get
                                } else {
                                    ImproperCheckText.Invoke($"Need atleast 2 empty inventory spaces to mine this ore");
                                    return; //no room
                                }
                            }
                        }
                        if(hasPickaxe){
                        } else {
                            ImproperCheckText.Invoke($"A useable pickaxe is required to mine this ore deposit");
                            return;
                        }
                    }
    }
    if(ClothNode){
        harvestType = "Harvest fiber";
                    if(PC){
                        bool hasScythe = false;
                        int itemCount = 0;
                        foreach(var sheet in InformationSheets){
                            if(sheet.CharacterID == PC.CharID){
                                float _currentWeight = 0f;
                                int gearStrength = 0;
                                string _class = string.Empty;
                                int _level = 1;
                                string _CORE = string.Empty;
                                if(PC.stamina >= 0){
                                    ImproperCheckText.Invoke($"Not enough staminia to perform this action");
                                    return;
                                }
                                foreach(var inventoryItem in sheet.CharInventoryData){
                                    if(inventoryItem.Value.Deleted){
                                        continue;
                                    }
                                    _currentWeight += float.Parse(inventoryItem.Value.GetWeight()) * inventoryItem.Value.amount;
                                    if(inventoryItem.Value.EQUIPPED){
                                        if(inventoryItem.Value.GetItemType() == ItemSelectable.ItemType.CharacterBag){
                                            if(inventoryItem.Value.GetItemName() == "Bag"){
                                                gearStrength += 30;
                                            }
                                        }
                                        if(!string.IsNullOrEmpty(inventoryItem.Value.STRENGTH_item)){
                                            if(int.Parse(inventoryItem.Value.STRENGTH_item) > 0){
                                                gearStrength += int.Parse(inventoryItem.Value.STRENGTH_item);
                                            }
                                        }
                                    }
                                }
                                foreach(var stat in sheet.CharStatData){
                                    if(stat.Key == "CORE"){
                                        _CORE = stat.Value;
                                    }
                                    if(stat.Key == "Class"){
                                        _class = stat.Value; 
                                    }
                                    if(stat.Key == "LVL"){
                                        _level = int.Parse(stat.Value); 
                                    }
                                }
                                int PASSIVE_Strength = 0;
                                for(int _char = 0; _char < GetInformationSheets().Count; _char++){
                                    if(GetInformationSheets()[_char].CharacterID == PC.CharID){
                                        for(int ability = 0; ability < GetInformationSheets()[_char].CharSpellData.Count; ability++){
                                            if(GetInformationSheets()[_char].CharSpellData[ability].Key == "SouthT2LeftSkill"){
                                                var abilityRankString = System.Text.RegularExpressions.Regex.Match(GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                                                if (abilityRankString.Success) {
                                                    int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                                                    PASSIVE_Strength = abilityRank;
                                                    break;
                                                }
                                            }
                                        }
                                        break;
                                    }
                                }
                                (int baseStrength, int baseAgility, int baseFortitude, int baseArcana) = StatAsset.Instance.GetCharacterStats(_class, _level, _CORE);
                                gearStrength += baseStrength + PASSIVE_Strength;         
                                if(_currentWeight >= gearStrength){
                                    //Need to put error text here ****
                                    ImproperCheckText.Invoke("This character cannot carry anymore weight");
                                    return;
                                }
                                foreach(var item in sheet.CharInventoryData){
                                    if(item.Value.Deleted || item.Value.amount == 0){
                                        continue;
                                    }
                                    if(item.Value.Item_Name == "Scythe" && int.Parse(item.Value.Durability) >= 1){
                                        hasScythe = true;
                                        break;
                                    }
                                if(!item.Value.EQUIPPED){//Hide Ore Stone Wood
                                        itemCount ++;
                                    }
                                }
                                if(itemCount <= 18){
                                    //can build the rare now lets roll to see if we get
                                } else {
                                    ImproperCheckText.Invoke($"Need atleast 2 empty inventory spaces to forage this plant");
                                    return; //no room
                                }
                                break;
                            }
                        }
                        if(hasScythe){
                        } else {
                            ImproperCheckText.Invoke($"A usable scythe is required to forage this plant fiber");
                            return;
                        }
                    }
    }
    if(LeatherNode){
        harvestType = "Harvest hide";
        if(PC){
                        bool hasKnife = false;
                        int itemCount = 0;
                        foreach(var sheet in InformationSheets){
                            if(sheet.CharacterID == PC.CharID){
                                float _currentWeight = 0f;
                                int gearStrength = 0;
                                string _class = string.Empty;
                                int _level = 1;
                                string _CORE = string.Empty;
                                if(PC.stamina >= 0){
                                    ImproperCheckText.Invoke($"Not enough staminia to perform this action");
                                    return;
                                }
                                foreach(var inventoryItem in sheet.CharInventoryData){
                                    if(inventoryItem.Value.Deleted){
                                        continue;
                                    }
                                    _currentWeight += float.Parse(inventoryItem.Value.GetWeight()) * inventoryItem.Value.amount;
                                    if(inventoryItem.Value.EQUIPPED){
                                        if(inventoryItem.Value.GetItemType() == ItemSelectable.ItemType.CharacterBag){
                                            if(inventoryItem.Value.GetItemName() == "Bag"){
                                                gearStrength += 30;
                                            }
                                        }
                                        if(!string.IsNullOrEmpty(inventoryItem.Value.STRENGTH_item)){
                                            if(int.Parse(inventoryItem.Value.STRENGTH_item) > 0){
                                                gearStrength += int.Parse(inventoryItem.Value.STRENGTH_item);
                                            }
                                        }
                                    }
                                }
                                foreach(var stat in sheet.CharStatData){
                                    if(stat.Key == "CORE"){
                                        _CORE = stat.Value;
                                    }
                                    if(stat.Key == "Class"){
                                        _class = stat.Value; 
                                    }
                                    if(stat.Key == "LVL"){
                                        _level = int.Parse(stat.Value); 
                                    }
                                }
                                int PASSIVE_Strength = 0;
                                for(int _char = 0; _char < GetInformationSheets().Count; _char++){
                                    if(GetInformationSheets()[_char].CharacterID == PC.CharID){
                                        for(int ability = 0; ability < GetInformationSheets()[_char].CharSpellData.Count; ability++){
                                            if(GetInformationSheets()[_char].CharSpellData[ability].Key == "SouthT2LeftSkill"){
                                                var abilityRankString = System.Text.RegularExpressions.Regex.Match(GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                                                if (abilityRankString.Success) {
                                                    int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                                                    PASSIVE_Strength = abilityRank;
                                                    break;
                                                }
                                            }
                                        }
                                        break;
                                    }
                                }
                                (int baseStrength, int baseAgility, int baseFortitude, int baseArcana) = StatAsset.Instance.GetCharacterStats(_class, _level, _CORE);
                                gearStrength += baseStrength + PASSIVE_Strength;              
                                if(_currentWeight >= gearStrength){
                                    //Need to put error text here ****
                                    ImproperCheckText.Invoke("This character cannot carry anymore weight");
                                    return;
                                }
                                foreach(var item in sheet.CharInventoryData){
                                    if(item.Value.Deleted || item.Value.amount == 0){
                                        continue;
                                    }
                                    if(item.Value.Item_Name == "Skinning Knife" && int.Parse(item.Value.Durability) >= 1){
                                        hasKnife = true;
                                    }
                                    if(!item.Value.EQUIPPED){//Hide Ore Stone Wood
                                        itemCount ++;
                                    }
                                }
                                if(itemCount <= 18){
                                    //can build the rare now lets roll to see if we get
                                } else {
                                    ImproperCheckText.Invoke($"Need atleast 2 empty inventory spaces to skin this hide");
                                    return; //no room
                                }
                                break;
                            }
                        }
                        if(hasKnife){
                        } else {
                            ImproperCheckText.Invoke($"A usable skinning knife is required to harvest this raw hide");
                            return;
                        }
                        //if has axe is true we need to chop the wood now and set a timer to all, if they get hit it cancels it. then durability skill up and yield etc
                    }
    }
	bool rightFace = castingCharacter.GetComponent<SpriteRenderer>().flipX;
    CmdBuildHarvesterCastBar(castingCharacter, harvestType, this, targetNode, rightFace);
}
void CharacterCastingTargetSpell(MovingObject castingCharacter, MovingObject target, string mode, Vector2 mousePosition){
    if(castingCharacter.Casting){
        castingCharacter.CmdCancelSpell();
    }
    PlayerCharacter pc = castingCharacter.GetComponent<PlayerCharacter>();
    int lvl = 1;
    if(pc){
        lvl = pc.Level;
    }
    string _spell = string.Empty;
    
    if(mode == CastingQ){
        _spell = castingCharacter.SpellQ;
    }
    if(mode == CastingE){
        _spell = castingCharacter.SpellE;
    }
    if(mode == CastingR){
        _spell = castingCharacter.SpellR;
    }
    if(mode == CastingF){
        _spell = castingCharacter.SpellF;
    }
    var nameMatch = System.Text.RegularExpressions.Regex.Match(_spell, @"^\D*");
    string spell = nameMatch.Value.Trim(); 
    int _spellRank = 1;
    // Extract spell rank
    var rankMatch = System.Text.RegularExpressions.Regex.Match(_spell, @"\d+$");
    if (rankMatch.Success) {
        _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
    }
    if(castingCharacter.cur_mp < StatAsset.Instance.GetSpellCost(spell)){
        ImproperCheckText.Invoke($"Not enough magic points to cast {spell}");
        return;
    }
    //Check if we are self casted
        float selfCast = StatAsset.Instance.SelfCasted(spell);
        bool selfCasted = false;
        if(selfCast == 1){
            selfCasted = true;
            mousePosition = castingCharacter.transform.position;
        }
    if(StatAsset.Instance.SkillShot(spell)){
        //this book mark is for line of sight on skill shots
//        print(mousePosition + " was our mouse position for skill shot");
        //if(!HasLineOfSight(castingCharacter.transform.position, mousePosition) && !selfCasted){
        //    
        //    ImproperCheckText.Invoke($"{spell} location is not in line of sight");
        //    return;
        //}
        if(!StatAsset.Instance.InSpellRange(castingCharacter, null, mode, mousePosition, out float finalRange) && !selfCasted){
            print($"{finalRange} is max range of this spell");
            castingCharacter.CmdMoveToCast(target, mode, finalRange, mousePosition);
            return;
        } else {
            if(!HasLineOfSight(castingCharacter.transform.position, mousePosition) && !selfCasted){
                float rangeUsed = StatAsset.Instance.InSpellRangeTM(spell, _spellRank, pc);
                castingCharacter.CmdMoveToCast(target, mode, rangeUsed, mousePosition);
                return;
            }
        }
    } else {

        if(target && !selfCasted){
        //this book mark is for line of sight on non skill shots
            //if(!HasLineOfSight(castingCharacter.transform.position, target.transform.position) && !selfCasted){
            //    ImproperCheckText.Invoke($"{spell} location is not in line of sight");
            //    return;
            //}
            if(!StatAsset.Instance.InSpellRange(castingCharacter, target, mode, mousePosition, out float finalRange) && !selfCasted){
                print($"{finalRange} is max range of this spell");
                
                castingCharacter.CmdMoveToCast(target, mode, finalRange, mousePosition);
                return;
            } else {
                if(!HasLineOfSight(castingCharacter.transform.position, target.transform.position) && !selfCasted){
                    float rangeUsed = StatAsset.Instance.InSpellRangeTM(spell, _spellRank, pc);
                    castingCharacter.CmdMoveToCast(target, mode, rangeUsed, target.transform.position);
                    return;
                }
            }
        } 
    }
    /*
    if(target){
       if(!HasLineOfSight(castingCharacter.transform.position, target.transform.position)){
            ImproperCheckText.Invoke($"{spell} location is not in line of sight");
            return;
        }
        if(!StatAsset.Instance.InSpellRange(castingCharacter, target, mode, mousePosition)){
            print($"{finalRange} is max range of this spell");
            castingCharacter.CmdMoveToCast(target, mode, finalRange, mousePosition);
            return;
        } 
    } 
    */
    //friendly spell
    float castTime = StatAsset.Instance.GetCastTime(spell, lvl, _spellRank);
    if(castTime == 0f){
        if(target && !selfCasted){
            if(pc){
                pc.CmdInstantCastSpell(mode, target);
            } else {
                castingCharacter.CmdInstantCastSpellMOB(mode, target);
            }
        } else {
             if(StatAsset.Instance.SkillShot(spell) || selfCasted){
                //CmdBuildCastBar(castingCharacter, mode, null, this, mousePosition);
                if(pc){
                    pc.CmdInstantCastSpellSelfCast(mode, mousePosition);
                } else {
                    castingCharacter.CmdInstantCastSpellSelfCastMOB(mode, mousePosition);
                }
            }
        }
        //else {
        //    pc.CmdCastAOESpell(mode, mousePosition);
        //}
    } else {
        if(StatAsset.Instance.SkillShot(spell) || selfCasted){
                    //RpcCastBarAOESPELL(pc, castTime, mode, MousePosition, owner);

            //pc.CmdCastAOESpell(mode, mousePosition);
            CmdBuildCastBar(castingCharacter, mode, null, this, mousePosition);

        } else {
            CmdBuildCastBar(castingCharacter, mode, target, this, mousePosition);
        }
        //castingCharacter.CmdCancelMovementToCast();
    }
}
int GetTactEnergyCost(string spell){
        int value = 0;
        if (spell == "Ignite"){
            value = 0;
        }
        if (spell == "Enthrall"){
            value = 0;
        }
        if (spell == "Refresh"){
            value = 0;
        }
        if (spell == "Absorb"){
            value = 0;
        }
        if (spell == "Morale Boost"){
            value = 100;
        }
        if (spell == "Lightning Bolt"){
            value = 100;
        }
        if (spell == "Repel"){
            value = 100;
        }
        if (spell == "Mend"){
            value = 150;
        }
        if (spell == "Light"){
            value = 100;
        }
        if (spell == "Stun"){
            value = 100;
        }
        if (spell == "Antitdote"){
            value = 100;
        }
        if (spell == "Fortification"){
            value = 500;
        }
        if (spell == "Focus"){
            value = 200;
        }
        if (spell == "Evacuate"){
            value = 500;
        }
        if (spell == "Chain Lightning"){
            value = 300;
        }
        if (spell == "Far Sight"){
            value = 200;
        }
        if (spell == "Group Mend"){
            value = 400;
        }
        if (spell == "Harvest Boost"){
            value = 200;
        }
        if (spell == "Fertile Soil"){
            value = 500;
        }
        if (spell == "Alliance"){
            value = 100;
        }
        if (spell == "Growth"){
            value = 500;
        }
        if (spell == "Return"){
            value = 1000;
        }
        if (spell == "Divine Resurrection"){
            value = 3000;
        }
        if (spell == "Rest"){
            value = 1000;
        }
        return value;
    }
List<Vector2> reservationWalls;
public bool HasLineOfSight(Vector2 start, Vector2 end)
    {
        int x0 = Mathf.FloorToInt(start.x);
        int y0 = Mathf.FloorToInt(start.y);
        int x1 = Mathf.FloorToInt(end.x);
        int y1 = Mathf.FloorToInt(end.y);
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;
        while (true)
        {
            Vector2 current = new Vector2(x0 + .5f, y0 + .5f);
            if (reservationWalls.Contains(current)){
                print(current + " was the current location that did not have line of sight");
                return false;
            }
            if (x0 == x1 && y0 == y1){
                return true;
            }
            int e2 = 2 * err;
            if (e2 > -dy){
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx){
                err += dx;
                y0 += sy;
            }
        }
    }
    [TargetRpc]
    public void TargetWalls(List<Vector2> targetList){
        reservationWalls = targetList;
    }
    public void OwnerClickedCharacterUI(PlayerCharacter pcClicked, bool ClearSelected){
        if(ClearSelected){
            ClearTarget();
        }
        ClickedCharacter(pcClicked);
    }
    void ClickedCharacter(PlayerCharacter SelectedChar){
        //print($"Owned character was clicked, {SelectedChar.CharacterName} is ready to be used");
        if(!selectedCharacters.Contains(SelectedChar.gameObject)){
            selectedCharacters.Add(SelectedChar.gameObject);
        }
        SelectedChar.SelectedUnit();
    }
    void ClickedMob(Mob _SelectedMob){
        _SelectedMob.SelectedMO();
    }
    void ClickedMobOwned(Mob _SelectedMob){
        if(_SelectedMob.CharmedOWNER != null){
            List<MovingObject> selected = new List<MovingObject>();
            selected.Add(_SelectedMob.CharmedOWNER);
            _SelectedMob.CharmedOWNER.SelectedMO();
            //_SelectedMob.SelectedMO();
            if(!selectedCharacters.Contains(_SelectedMob.CharmedOWNER.gameObject)){
                selectedCharacters.Add(_SelectedMob.CharmedOWNER.gameObject);
            }
            CombatPartyView.instance.TurnOnSelectedWindow(selected);
        }
        
    }
    void ClickedTargetMob(Mob _TargetMob){
        _TargetMob.TargettedMO();
        //TargetMob = _TargetMob;
    }
   
    
    void DeselectedCharacter(){
        //selectedPlayer = null;
        selectedCharacters.Clear();
        CombatPartyView.instance.TurnOffSelectedWindow();
        //if(SpellQ != null){
        //    Destroy(SpellQ);
        //}
        //if(SpellE != null){
        //    Destroy(SpellE);
        //}
        //if(SpellR != null){
        //    Destroy(SpellR);
        //}
        //if(SpellF != null){
        //    Destroy(SpellF);
        //}
        ToggleSpellsOff.Invoke();
    }
    public void ClearSelected(){
        //if(SelectedMob != null){
        //    SelectedMob = null;
        //}
        //if(TargetMob != null){
        //    TargetMob = null;
        //}
        selectedCharacters.Clear();
        CombatPartyView.instance.TurnOffSelectedWindow();
        ToggleSpellsOff.Invoke();
    }
    [TargetRpc]
    public void TargetEndMatchCanvas(){
        ToggleEndMatch.Invoke();
    }
    public void PortalOVM(){
        ToggleEndMatch.Invoke();
    }
    public void PortalOVMClose(){
        CloseEndMatch.Invoke();
    }
    public int GetCharacterResCost(int _level){
        if(_level > 20 || _level < 0){
            return 0;
        }
        //int[] resCosts = {
        //    2000,  3000,  6000, 10000, 13000,
        //    17000, 20000, 25000, 30000, 35000,
        //    40000, 50000, 60000, 70000, 80000,
        //    90000, 100000, 115000, 150000, 200000
        //};
        int[] resCosts = {
            500,  600,  700,  800,  900,
            1500, 2000, 2500, 3000, 3500,
            4000, 5000, 6000, 7000, 9000,
            11000, 13000, 15000, 20000, 25000
        };
        return resCosts[_level - 1];
    }
    public (int, int, float, int) GetCharacterLevelUp(int _level, string CORE){
            int ExpCost = 0;
            int EnergyCost = 0;
            float TimeCost = 0f;
            int GoldCost = 0;
            switch (CORE) {
                case "STANDARD":
                    switch (_level) {
                        case 1:
                            ExpCost = 4000;
                            EnergyCost = 100;
                            TimeCost = 60f; // 1 minute in seconds
                            GoldCost = 500;
                            break;
                        case 2:
                            ExpCost = 8000;
                            EnergyCost = 200;
                            TimeCost = 300f; // 5 minutes in seconds
                            GoldCost = 750;
                            break;
                        case 3:
                            ExpCost = 12000;
                            EnergyCost = 300;
                            TimeCost = 900f; // 15 minutes in seconds
                            GoldCost = 1000;
                            break;
                        case 4:
                            ExpCost = 20000;
                            EnergyCost = 400;
                            TimeCost = 3600f; // 1 hour in seconds
                            GoldCost = 1500;
                            break;
                        case 5:
                            ExpCost = 30000;
                            EnergyCost = 500;
                            TimeCost = 10800f; // 3 hours in seconds
                            GoldCost = 2000;
                            break;
                        case 6:
                            ExpCost = 55000;
                            EnergyCost = 600;
                            TimeCost = 18000f; // 5 hours in seconds
                            GoldCost = 2500;
                            break;
                        case 7:
                            ExpCost = 75000;
                            EnergyCost = 700;
                            TimeCost = 21600f; // 6 hours in seconds
                            GoldCost = 3000;
                            break;
                        case 8:
                            ExpCost = 100000;
                            EnergyCost = 800;
                            TimeCost = 25200f; // 7 hours in seconds
                            GoldCost = 3500;
                            break;
                        case 9:
                            ExpCost = 145000;
                            EnergyCost = 900;
                            TimeCost = 28800f; // 8 hours in seconds
                            GoldCost = 4000;
                            break;
                        case 10:
                            ExpCost = 180000;
                            EnergyCost = 1000;
                            TimeCost = 32400f; // 9 hours in seconds
                            GoldCost = 4500;
                            break;
                        case 11:
                            ExpCost = 220000;
                            EnergyCost = 1200;
                            TimeCost = 39600f; // 11 hours in seconds
                            GoldCost = 5000;
                            break;
                        case 12:
                            ExpCost = 265000;
                            EnergyCost = 1400;
                            TimeCost = 46800f; // 13 hours in seconds
                            GoldCost = 5500;
                            break;
                        case 13:
                            ExpCost = 300000;
                            EnergyCost = 1600;
                            TimeCost = 54000f; // 15 hours in seconds
                            GoldCost = 6000;
                            break;
                        case 14:
                            ExpCost = 350000;
                            EnergyCost = 1800;
                            TimeCost = 61200f; // 17 hours in seconds
                            GoldCost = 6500;
                            break;
                        case 15:
                            ExpCost = 425000;
                            EnergyCost = 2000;
                            TimeCost = 68400f; // 19 hours in seconds
                            GoldCost = 7000;
                            break;
                        case 16:
                            ExpCost = 500000;
                            EnergyCost = 2200;
                            TimeCost = 75600f; // 21 hours in seconds
                            GoldCost = 7500;
                            break;
                        case 17:
                            ExpCost = 600000;
                            EnergyCost = 2400;
                            TimeCost = 82800f; // 23 hours in seconds
                            GoldCost = 8000;
                            break;
                        case 18:
                            ExpCost = 700000;
                            EnergyCost = 2600;
                            TimeCost = 90000f; // 25 hours in seconds
                            GoldCost = 9000;
                            break;
                        case 19:
                            ExpCost = 800000;
                            EnergyCost = 2800;
                            TimeCost = 97200f; // 27 hours in seconds
                            GoldCost = 10000;
                            break;
                        default:
                            break;
                    }
                    break;
                case "HARDCORE":
                    switch (_level) {
                        case 1:
                            ExpCost = 8000;
                            EnergyCost = 200;
                            TimeCost = 3600f;
                            GoldCost = 800;
                            break;
                        case 2:
                            ExpCost = 16000;
                            EnergyCost = 400;
                            TimeCost = 7200f;
                            GoldCost = 2500;
                            break;
                        case 3:
                            ExpCost = 24000;
                            EnergyCost = 600;
                            TimeCost = 14400f;
                            GoldCost = 5000;
                            break;
                        case 4:
                            ExpCost = 40000;
                            EnergyCost = 800;
                            TimeCost = 21600f;
                            GoldCost = 10000;
                            break;
                        case 5:
                            ExpCost = 60000;
                            EnergyCost = 1000;
                            TimeCost = 28800f;
                            GoldCost = 15000;
                            break;
                        case 6:
                            ExpCost = 110000;
                            EnergyCost = 1200;
                            TimeCost = 36000f;
                            GoldCost = 20000;
                            break;
                        case 7:
                            ExpCost = 150000;
                            EnergyCost = 1400;
                            TimeCost = 43200f;
                            GoldCost = 25000;
                            break;
                        case 8:
                            ExpCost = 200000;
                            EnergyCost = 1600;
                            TimeCost = 50400f;
                            GoldCost = 30000;
                            break;
                        case 9:
                            ExpCost = 290000;
                            EnergyCost = 1800;
                            TimeCost = 57600f;
                            GoldCost = 35000;
                            break;
                        case 10:
                            ExpCost = 360000;
                            EnergyCost = 2000;
                            TimeCost = 64800f;
                            GoldCost = 40000;
                            break;
                        case 11:
                            ExpCost = 440000;
                            EnergyCost = 2200;
                            TimeCost = 72000f;
                            GoldCost = 45000;
                            break;
                        case 12:
                            ExpCost = 530000;
                            EnergyCost = 2400;
                            TimeCost = 79200f;
                            GoldCost = 50000;
                            break;
                        case 13:
                            ExpCost = 600000;
                            EnergyCost = 2600;
                            TimeCost = 86400f;
                            GoldCost = 55000;
                            break;
                        case 14:
                            ExpCost = 700000;
                            EnergyCost = 2800;
                            TimeCost = 93600f;
                            GoldCost = 60000;
                            break;
                        case 15:
                            ExpCost = 850000;
                            EnergyCost = 3000;
                            TimeCost = 100800f;
                            GoldCost = 70000;
                            break;
                        case 16:
                            ExpCost = 1000000;
                            EnergyCost = 3200;
                            TimeCost = 108000f;
                            GoldCost = 80000;
                            break;
                        case 17:
                            ExpCost = 1200000;
                            EnergyCost = 3400;
                            TimeCost = 115200f;
                            GoldCost = 90000;
                            break;
                        case 18:
                            ExpCost = 1400000;
                            EnergyCost = 3600;
                            TimeCost = 122400f;
                            GoldCost = 100000;
                            break;
                        case 19:
                            ExpCost = 1600000;
                            EnergyCost = 3800;
                            TimeCost = 129600f;
                            GoldCost = 125000;
                            break;
                        case 20:
                            ExpCost = 2000000;
                            EnergyCost = 4000;
                            TimeCost = 136800f;
                            GoldCost = 150000;
                            break;
                        default:
                            break;
                    }
                    break;
                case "HERO":
                    switch (_level) {
                        case 1:
                            ExpCost = 16000;
                            EnergyCost = 300;
                            TimeCost = 7200f;
                            GoldCost = 1500;
                            break;
                        case 2:
                            ExpCost = 32000;
                            EnergyCost = 600;
                            TimeCost = 18000f;
                            GoldCost = 4000;
                            break;
                        case 3:
                            ExpCost = 48000;
                            EnergyCost = 900;
                            TimeCost = 28800f;
                            GoldCost = 10000;
                            break;
                        case 4:
                            ExpCost = 80000;
                            EnergyCost = 1200;
                            TimeCost = 39600f;
                            GoldCost = 16000;
                            break;
                        case 5:
                            ExpCost = 120000;
                            EnergyCost = 1500;
                            TimeCost = 50400f;
                            GoldCost = 25000;
                            break;
                        case 6:
                            ExpCost = 220000;
                            EnergyCost = 1800;
                            TimeCost = 61200f;
                            GoldCost = 35000;
                            break;
                        case 7:
                            ExpCost = 300000;
                            EnergyCost = 2100;
                            TimeCost = 72000f;
                            GoldCost = 45000;
                            break;
                        case 8:
                            ExpCost = 400000;
                            EnergyCost = 2400;
                            TimeCost = 82800f;
                            GoldCost = 55000;
                            break;
                        case 9:
                            ExpCost = 580000;
                            EnergyCost = 2700;
                            TimeCost = 93600f;
                            GoldCost = 65000;
                            break;
                        case 10:
                            ExpCost = 720000;
                            EnergyCost = 3000;
                            TimeCost = 104400f;
                            GoldCost = 75000;
                            break;
                        case 11:
                            ExpCost = 880000;
                            EnergyCost = 3300;
                            TimeCost = 115200f;
                            GoldCost = 100000;
                            break;
                        case 12:
                            ExpCost = 1060000;
                            EnergyCost = 3600;
                            TimeCost = 126000f;
                            GoldCost = 125000;
                            break;
                        case 13:
                            ExpCost = 1200000;
                            EnergyCost = 3900;
                            TimeCost = 136800f;
                            GoldCost = 150000;
                            break;
                        case 14:
                            ExpCost = 1400000;
                            EnergyCost = 4200;
                            TimeCost = 147600f;
                            GoldCost = 175000;
                            break;
                        case 15:
                            ExpCost = 1800000;
                            EnergyCost = 4500;
                            TimeCost = 158400f;
                            GoldCost = 200000;
                            break;
                        case 16:
                            ExpCost = 2100000;
                            EnergyCost = 4800;
                            TimeCost = 169200f;
                            GoldCost = 225000;
                            break;
                        case 17:
                            ExpCost = 2400000;
                            EnergyCost = 5100;
                            TimeCost = 180000f;
                            GoldCost = 250000;
                            break;
                        case 18:
                            ExpCost = 2800000;
                            EnergyCost = 5400;
                            TimeCost = 190800f;
                            GoldCost = 275000;
                            break;
                        case 19:
                            ExpCost = 3200000;
                            EnergyCost = 5700;
                            TimeCost = 201600f;
                            GoldCost = 350000;
                            break;
                        case 20:
                            ExpCost = 4000000;
                            EnergyCost = 6000;
                            TimeCost = 212400f;
                            GoldCost = 500000;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        return (ExpCost, EnergyCost, TimeCost, GoldCost);
    }
    void PressedAltQCombat(){
        MovingObject petowner = CombatPartyView.instance.GetSelected();
        if(!petowner){
            return;
        }
        MovingObject selectedObject = petowner.CharmedPET;
            if(selectedObject == null){
                ImproperCheckText.Invoke($"No target set");
                return;
            }
            if(!selectedObject.GetComponent<NetworkIdentity>().hasAuthority){
                return;
            }
            GameObject spellQGO = SpellManager.instance.GetPETSpellQ();
            if(spellQGO == null){
                return;
            }
            Spell spellq = spellQGO.GetComponent<Spell>();
            if(spellq == null){
                return;
            }
            if(selectedObject.Silenced){
                ImproperCheckText.Invoke($"Cannot cast while silenced");
                return;
            }
            //PlayerCharacter selectedPlayer = selectedObject.GetComponent<PlayerCharacter>();
            
            if(selectedObject.SpellQ == "None"){
                ImproperCheckText.Invoke($"Unit does not have a spell selected for that key");
                return;
            }
            
            if(selectedObject.SpellQCoolDown){
                ImproperCheckText.Invoke($"{selectedObject.SpellQ} is on cooldown");
            }
            if(selectedObject.stamina > 0){
                ImproperCheckText.Invoke($"Unit does not have enough stamina to cast {selectedObject.SpellQ}");
                return;
            }
            var nameMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellQ, @"^\D*");
            string spell = nameMatch.Value.Trim(); 
            int _spellRank = 1;
            // Extract spell rank
            var rankMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellQ, @"\d+$");
            if (rankMatch.Success) {
                _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
            }
            if(!selectedObject.shield && spell == "Bash" || !selectedObject.shield && spell == "Shield Bash"){
                ImproperCheckText.Invoke($"Must have a shield for that");
                return;
            }
            
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mouseWorldPosition2D = new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);

            if(selectedObject.cur_mp < StatAsset.Instance.GetSpellCost(spell)){
                ImproperCheckText.Invoke($"Unit does not have enough MP for {selectedObject.SpellQ}");
                return;
            }
            if(!spellq.GetCastable()){
                ImproperCheckText.Invoke($"Unit's spell {selectedObject.SpellQ} is on cooldown");
                return;
            }
            if(selectedObject.moving){
                //tell server to stop moving
                selectedObject.CmdStopMoving();
            }
            float selfCast = StatAsset.Instance.SelfCasted(spell);
            bool selfCasted = false;
            if(selfCast == 1){
                selfCasted = true;
            }
            if(StatAsset.Instance.SkillShot(spell)){
                if(SavedClick != null){
                    Destroy(SavedClick.radiusIndicator);
                    SavedClick = null;
                }
                BuildAoeClick(selectedObject, CastingQ);
                return;
            }
            if(selfCasted){
                CharacterCastingTargetSpell(selectedObject, null, CastingQ, mouseWorldPosition2D);
                return;
            }
            bool friendlySpell = StatAsset.Instance.DetermineFriendly(spell);
            //check if target required
            if(petowner.Target != null){
                if(spell == "Resurrect" && !petowner.Target.Dying){
                    ImproperCheckText.Invoke("Can only cast that on the dead");
                    return;
                }
                if(spell == "Purge" || spell == "Dispel"){
                    CharacterCastingTargetSpell(selectedObject, petowner.Target, CastingQ, mouseWorldPosition2D);
                    return;
                }
                if(selectedObject.GetFriendly(petowner.Target) && friendlySpell){
                    if(petowner.Target.Dying && spell != "Resurrect"){
                        ImproperCheckText.Invoke($"{selectedObject.SpellQ} cannot be cast on a dead target");
                        return;
                    }
                    CharacterCastingTargetSpell(selectedObject, petowner.Target, CastingQ, mouseWorldPosition2D);
                    return;
                } else if(selectedObject.GetFriendly(petowner.Target) && !friendlySpell){
                    ImproperCheckText.Invoke($"{selectedObject.SpellQ} cannot be cast on a friendly target");
                    return;
                }
                if(!selectedObject.GetFriendly(petowner.Target) && !friendlySpell){
                    if(petowner.Target.Dying){
                        ImproperCheckText.Invoke($"{selectedObject.SpellQ} cannot be cast on a dead target");
                        return;
                    }
                    if(spell == "Backstab"){
                        if(selectedObject.IsBehindAnotherObject(petowner.Target.transform.position, petowner.Target.GetFacingDirection())){
                            ImproperCheckText.Invoke($"We are behind target nice!");
                        } else {
                            ImproperCheckText.Invoke($"Must be behind target for that");
                            return;
                        }
                    }
                    CharacterCastingTargetSpell(selectedObject, petowner.Target, CastingQ, mouseWorldPosition2D);
                } else if(!selectedObject.GetFriendly(petowner.Target) && friendlySpell){
                    ImproperCheckText.Invoke($"{selectedObject.SpellQ} cannot be cast on a hostile target");
                    return;
                }
                //clean this up and make it so that we can see if we need to cast immediately or later
            } else {
                //try to cast aoe on the mouse position?
            }
    }
    void PressedAltECombat(){
        MovingObject petowner = CombatPartyView.instance.GetSelected();
        if(!petowner){
            return;
        }
        //use combatpartywindow
            MovingObject selectedObject = petowner.CharmedPET;
            
            if(selectedObject == null){
                ImproperCheckText.Invoke($"No target set");
                return;
            }
            if(!selectedObject.GetComponent<NetworkIdentity>().hasAuthority){
                return;
            }
            GameObject spellEGO = SpellManager.instance.GetPETSpellE();
            if(spellEGO == null){
                return;
            }
            Spell spellE = spellEGO.GetComponent<Spell>();
            if(spellE == null){
                return;
            }
            if(selectedObject.Silenced){
                ImproperCheckText.Invoke($"Cannot cast while silenced");
                return;
            }
            //PlayerCharacter selectedPlayer = selectedObject.GetComponent<PlayerCharacter>();
            
            if(selectedObject.SpellE == "None"){
                ImproperCheckText.Invoke($"Unit does not have a spell selected for that key");
                return;
            }
            if(selectedObject.SpellECoolDown){
                ImproperCheckText.Invoke($"{selectedObject.SpellE} is on cooldown");
            }
            if(selectedObject.stamina > 0)
            {
                ImproperCheckText.Invoke($"Unit does not have enough stamina to cast {selectedObject.SpellE}");
                return;
            }
            var nameMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellE, @"^\D*");
            string spell = nameMatch.Value.Trim(); 
            int _spellRank = 1;
            // Extract spell rank
            var rankMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellE, @"\d+$");
            if (rankMatch.Success) {
                _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
            }
            if(!selectedObject.shield && spell == "Bash" || !selectedObject.shield && spell == "Shield Bash"){
                ImproperCheckText.Invoke($"Must have a shield for that");
                return;
            }
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mouseWorldPosition2D = new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);

            if(selectedObject.cur_mp < StatAsset.Instance.GetSpellCost(spell)){
                ImproperCheckText.Invoke($"Unit does not have enough MP for {selectedObject.SpellE}");
                return;
            }
            if(!spellE.GetCastable()){
                ImproperCheckText.Invoke($"Unit's spell {selectedObject.SpellE} is on cooldown");
                return;
            }
            if(selectedObject.moving){
                //tell server to stop moving
                selectedObject.CmdStopMoving();
            }
            float selfCast = StatAsset.Instance.SelfCasted(spell);
            bool selfCasted = false;
            if(selfCast == 1){
                selfCasted = true;
            }
            if(StatAsset.Instance.SkillShot(spell)){
                if(SavedClick != null){
                    Destroy(SavedClick.radiusIndicator);
                    SavedClick = null;
                }
                BuildAoeClick(selectedObject, CastingE);
                return;
            }
            if(selfCasted){
                CharacterCastingTargetSpell(selectedObject, null, CastingE, mouseWorldPosition2D);
                return;
            }
            bool friendlySpell = StatAsset.Instance.DetermineFriendly(spell);
            //check if target required
            if(petowner.Target != null){
                if(spell == "Resurrect" && !petowner.Target.Dying){
                    ImproperCheckText.Invoke("Can only cast that on the dead");
                    return;
                }
                if(spell == "Purge" || spell == "Dispel"){
                    CharacterCastingTargetSpell(selectedObject, petowner.Target, CastingE, mouseWorldPosition2D);
                    return;
                }
                if(selectedObject.GetFriendly(petowner.Target) && friendlySpell){
                    if(petowner.Target.Dying && spell != "Resurrect"){
                        ImproperCheckText.Invoke($"{selectedObject.SpellE} cannot be cast on a dead target");
                        return;
                    }
                    CharacterCastingTargetSpell(selectedObject, petowner.Target, CastingE, mouseWorldPosition2D);
                    return;
                } else if(selectedObject.GetFriendly(petowner.Target) && !friendlySpell){
                    ImproperCheckText.Invoke($"{selectedObject.SpellE} cannot be cast on a friendly target");
                    return;
                }
                
                if(!selectedObject.GetFriendly(petowner.Target) && !friendlySpell){
                    if(selectedObject.Target.Dying){
                        ImproperCheckText.Invoke($"{selectedObject.SpellE} cannot be cast on a dead target");
                        return;
                    }
                    if(spell == "Backstab"){
                        if(selectedObject.IsBehindAnotherObject(petowner.Target.transform.position, petowner.Target.GetFacingDirection())){
                            ImproperCheckText.Invoke($"We are behind target nice!");
                        } else {
                            ImproperCheckText.Invoke($"Must be behind target for that");
                            return;
                        }
                    }
                    CharacterCastingTargetSpell(selectedObject, petowner.Target, CastingE, mouseWorldPosition2D);
                } else if(!selectedObject.GetFriendly(petowner.Target) && friendlySpell){
                    ImproperCheckText.Invoke($"{selectedObject.SpellE} cannot be cast on a hostile target");
                    return;
                }
                //clean this up and make it so that we can see if we need to cast immediately or later
            } else {
                //try to cast aoe on the mouse position?
            }
    }
    void PressedAltRCombat(){
        MovingObject petowner = CombatPartyView.instance.GetSelected();
        if(!petowner){
            return;
        }
        MovingObject selectedObject = petowner.CharmedPET;
            
            if(selectedObject == null){
                ImproperCheckText.Invoke($"No target set");
                return;
            }
            if(!selectedObject.GetComponent<NetworkIdentity>().hasAuthority){
                return;
            }
            GameObject spellRGO = SpellManager.instance.GetPETSpellR();
            if(spellRGO == null){
                return;
            }
            Spell spellR = spellRGO.GetComponent<Spell>();
            if(spellR == null){
                return;
            }
            if(selectedObject.Silenced){
                ImproperCheckText.Invoke($"Cannot cast while silenced");
                return;
            }
            //PlayerCharacter selectedPlayer = selectedObject.GetComponent<PlayerCharacter>();
            
            if(selectedObject.SpellR == "None"){
                ImproperCheckText.Invoke($"Unit does not have a spell selected for that key");
                return;
            }
            if(selectedObject.SpellRCoolDown){
                ImproperCheckText.Invoke($"{selectedObject.SpellR} is on cooldown");
            }
            if(selectedObject.stamina > 0)
            {
                ImproperCheckText.Invoke($"Unit does not have enough stamina to cast {selectedObject.SpellR}");
                return;
            }
            var nameMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellR, @"^\D*");
            string spell = nameMatch.Value.Trim(); 
            int _spellRank = 1;
            // Extract spell rank
            var rankMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellR, @"\d+$");
            if (rankMatch.Success) {
                _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
            }
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mouseWorldPosition2D = new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);
            if(!selectedObject.shield && spell == "Bash" || !selectedObject.shield && spell == "Shield Bash"){
                ImproperCheckText.Invoke($"Must have a shield for that");
                return;
            }
            if(selectedObject.cur_mp < StatAsset.Instance.GetSpellCost(spell)){
                ImproperCheckText.Invoke($"Unit does not have enough MP for {selectedObject.SpellR}");
                return;
            }
            if(!spellR.GetCastable()){
                ImproperCheckText.Invoke($"Unit's spell {selectedObject.SpellR} is on cooldown");
                return;
            }
            if(selectedObject.moving){
                //tell server to stop moving
                selectedObject.CmdStopMoving();
            }
            float selfCast = StatAsset.Instance.SelfCasted(spell);
            bool selfCasted = false;
            if(selfCast == 1){
                selfCasted = true;
            }
            //var nameMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellR, @"^\D*");
            //string spellNameShaved = nameMatch.Value.Trim(); 
            //float targetable = SelfCasted(spell);
            //if(targetable != 0f){
            //    //cast the spell its a self target
            //    selectedObject.CmdSelfTarget(selectedObject.SpellR, CastingR);
            //    //possibly clear Mode to deselect character?
            //    return;
            //}
            
            if(StatAsset.Instance.SkillShot(spell)){
                if(SavedClick != null){
                    Destroy(SavedClick.radiusIndicator);
                    SavedClick = null;
                }
                BuildAoeClick(selectedObject, CastingR);
                return;
            }
            if(selfCasted){
                CharacterCastingTargetSpell(selectedObject, null, CastingR, mouseWorldPosition2D);
                return;
            }
            //if(AnyTarget(spell)){
            //    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingR, mouseWorldPosition2D);
            //}
            bool friendlySpell = StatAsset.Instance.DetermineFriendly(spell);
            //check if target required
            if(petowner.Target != null){
                if(spell == "Resurrect" && !petowner.Target.Dying){
                    ImproperCheckText.Invoke("Can only cast that on the dead");
                    return;
                }
                if(spell == "Purge" || spell == "Dispel"){
                    CharacterCastingTargetSpell(selectedObject, petowner.Target, CastingR, mouseWorldPosition2D);
                    return;
                }
                if(selectedObject.GetFriendly(petowner.Target) && friendlySpell){
                    if(petowner.Target.Dying && spell != "Resurrect"){
                        ImproperCheckText.Invoke($"{selectedObject.SpellR} cannot be cast on a dead target");
                        return;
                    }
                    CharacterCastingTargetSpell(selectedObject, petowner.Target, CastingR, mouseWorldPosition2D);
                    return;
                } else if(selectedObject.GetFriendly(petowner.Target) && !friendlySpell){
                    ImproperCheckText.Invoke($"{selectedObject.SpellR} cannot be cast on a friendly target");
                    return;
                }
                
                if(!selectedObject.GetFriendly(petowner.Target) && !friendlySpell){
                    if(petowner.Target.Dying){
                        ImproperCheckText.Invoke($"{selectedObject.SpellR} cannot be cast on a dead target");
                        return;
                    }
                    if(spell == "Backstab"){
                        if(selectedObject.IsBehindAnotherObject(petowner.Target.transform.position, petowner.Target.GetFacingDirection())){
                            ImproperCheckText.Invoke($"We are behind target nice!");
                        } else {
                            ImproperCheckText.Invoke($"Must be behind target for that");
                            return;
                        }
                    }
                    CharacterCastingTargetSpell(selectedObject, petowner.Target, CastingR, mouseWorldPosition2D);
                } else if(!selectedObject.GetFriendly(petowner.Target) && friendlySpell){
                    ImproperCheckText.Invoke($"{selectedObject.SpellR} cannot be cast on a hostile target");
                    return;
                }
                //clean this up and make it so that we can see if we need to cast immediately or later
            } else {
                //try to cast aoe on the mouse position?
            }
    }
    void PressedAltFCombat(){
        MovingObject petowner = CombatPartyView.instance.GetSelected();
        if(!petowner){
            return;
        }
        MovingObject selectedObject = petowner.CharmedPET;
            if(selectedObject == null){
                ImproperCheckText.Invoke($"No target set");
                return;
            }
            if(!selectedObject.GetComponent<NetworkIdentity>().hasAuthority){
                return;
            }
            GameObject spellFGO = SpellManager.instance.GetPETSpellF();
            if(spellFGO == null){
                return;
            }
            Spell spellf = spellFGO.GetComponent<Spell>();
            if(spellf == null){
                return;
            }
            if(selectedObject.Silenced){
                ImproperCheckText.Invoke($"Cannot cast while silenced");
                return;
            }
            if(selectedObject.SpellF == "None"){
                ImproperCheckText.Invoke($"Unit does not have a spell selected for that key");
                return;
            }
            if(selectedObject.SpellFCoolDown){
                ImproperCheckText.Invoke($"{selectedObject.SpellF} is on cooldown");
            }
            if(selectedObject.stamina > 0)
            {
                ImproperCheckText.Invoke($"Unit does not have enough stamina to cast {selectedObject.SpellF}");
                return;
            }
            var nameMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellF, @"^\D*");
            string spell = nameMatch.Value.Trim(); 
            int _spellRank = 1;
            // Extract spell rank
            var rankMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellF, @"\d+$");
            if (rankMatch.Success) {
                _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
            }
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mouseWorldPosition2D = new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);
            if(!selectedObject.shield && spell == "Bash" || !selectedObject.shield && spell == "Shield Bash"){
                ImproperCheckText.Invoke($"Must have a shield for that");
                return;
            }
            if(selectedObject.cur_mp < StatAsset.Instance.GetSpellCost(spell)){
                ImproperCheckText.Invoke($"Unit does not have enough MP for {selectedObject.SpellF}");
                return;
            }
            if(!spellf.GetCastable()){
                ImproperCheckText.Invoke($"Unit's spell {selectedObject.SpellF} is on cooldown");
                return;
            }
            if(selectedObject.moving){
                //tell server to stop moving
                selectedObject.CmdStopMoving();
            }
            
            //var nameMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellF, @"^\D*");
            //string spellNameShaved = nameMatch.Value.Trim(); 
            //float targetable = SelfCasted(spell);
            //if(targetable != 0f){
            //    //cast the spell its a self target
            //    selectedObject.CmdSelfTarget(selectedObject.SpellF, CastingF);
            //    //possibly clear Mode to deselect character?
            //    return;
            //}
            float selfCast = StatAsset.Instance.SelfCasted(spell);
            bool selfCasted = false;
            if(selfCast == 1){
                selfCasted = true;
            }
            if(StatAsset.Instance.SkillShot(spell)){
                if(SavedClick != null){
                    Destroy(SavedClick.radiusIndicator);
                    SavedClick = null;
                }
                BuildAoeClick(selectedObject, CastingF);
                return;
            }
            if(selfCasted){
                CharacterCastingTargetSpell(selectedObject, null, CastingF, mouseWorldPosition2D);
                return;
            }
            //if(AnyTarget(spell)){
            //    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingF, mouseWorldPosition2D);
            //}
            bool friendlySpell = StatAsset.Instance.DetermineFriendly(spell);
            //check if target required
            if(petowner.Target != null){
                if(spell == "Resurrect" && !petowner.Target.Dying){
                    ImproperCheckText.Invoke("Can only cast that on the dead");
                    return;
                }
                if(spell == "Purge" || spell == "Dispel"){
                    CharacterCastingTargetSpell(selectedObject, petowner.Target, CastingF, mouseWorldPosition2D);
                    return;
                }
                if(selectedObject.GetFriendly(petowner.Target) && friendlySpell){
                    if(petowner.Target.Dying && spell != "Resurrect"){
                        ImproperCheckText.Invoke($"{selectedObject.SpellF} cannot be cast on a dead target");
                        return;
                    }
                    CharacterCastingTargetSpell(selectedObject, petowner.Target, CastingF, mouseWorldPosition2D);
                    return;
                } else if(selectedObject.GetFriendly(petowner.Target) && !friendlySpell){
                    ImproperCheckText.Invoke($"{selectedObject.SpellF} cannot be cast on a friendly target");
                    return;
                }
                
                if(!selectedObject.GetFriendly(petowner.Target) && !friendlySpell){
                    if(petowner.Target.Dying){
                        ImproperCheckText.Invoke($"{selectedObject.SpellF} cannot be cast on a dead target");
                        return;
                    }
                    if(spell == "Backstab"){
                        if(selectedObject.IsBehindAnotherObject(petowner.Target.transform.position, petowner.Target.GetFacingDirection())){
                            ImproperCheckText.Invoke($"We are behind target nice!");
                        } else {
                            ImproperCheckText.Invoke($"Must be behind target for that");
                            return;
                        }
                    }
                    CharacterCastingTargetSpell(selectedObject, petowner.Target, CastingF, mouseWorldPosition2D);
                } else if(!selectedObject.GetFriendly(petowner.Target) && friendlySpell){
                    ImproperCheckText.Invoke($"{selectedObject.SpellF} cannot be cast on a hostile target");
                    return;
                }
                //clean this up and make it so that we can see if we need to cast immediately or later
            } else {
                //try to cast aoe on the mouse position?
            }
    }
    void PressedQCombat(){
        MovingObject selectedObject = CombatPartyView.instance.GetSelected();
            if(selectedObject == null){
                ImproperCheckText.Invoke($"No target set");
                return;
            }
            if(!selectedObject.GetComponent<NetworkIdentity>().hasAuthority){
                return;
            }
            GameObject spellQGO = SpellManager.instance.GetSpellQ();
            if(spellQGO == null){
                return;
            }
            Spell spellq = spellQGO.GetComponent<Spell>();
            if(spellq == null){
                return;
            }
            if(selectedObject.Silenced){
                ImproperCheckText.Invoke($"Cannot cast while silenced");
                return;
            }
            //PlayerCharacter selectedPlayer = selectedObject.GetComponent<PlayerCharacter>();
            
            if(selectedObject.SpellQ == "None"){
                ImproperCheckText.Invoke($"Unit does not have a spell selected for that key");
                return;
            }
            
            if(selectedObject.SpellQCoolDown){
                ImproperCheckText.Invoke($"{selectedObject.SpellQ} is on cooldown");
            }
            if(selectedObject.stamina > 0){
                ImproperCheckText.Invoke($"Unit does not have enough stamina to cast {selectedObject.SpellQ}");
                return;
            }
            var nameMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellQ, @"^\D*");
            string spell = nameMatch.Value.Trim(); 
            int _spellRank = 1;
            // Extract spell rank
            var rankMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellQ, @"\d+$");
            if (rankMatch.Success) {
                _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
            }
            if(!selectedObject.shield && spell == "Bash" || !selectedObject.shield && spell == "Shield Bash"){
                ImproperCheckText.Invoke($"Must have a shield for that");
                return;
            }
            
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mouseWorldPosition2D = new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);

            if(selectedObject.cur_mp < StatAsset.Instance.GetSpellCost(spell)){
                ImproperCheckText.Invoke($"Unit does not have enough MP for {selectedObject.SpellQ}");
                return;
            }
            if(!spellq.GetCastable()){
                ImproperCheckText.Invoke($"Unit's spell {selectedObject.SpellQ} is on cooldown");
                return;
            }
            if(selectedObject.moving){
                //tell server to stop moving
                selectedObject.CmdStopMoving();
            }
            float selfCast = StatAsset.Instance.SelfCasted(spell);
            bool selfCasted = false;
            if(selfCast == 1){
                selfCasted = true;
            }
            if(StatAsset.Instance.SkillShot(spell)){
                if(SavedClick != null){
                    Destroy(SavedClick.radiusIndicator);
                    SavedClick = null;
                }
                BuildAoeClick(selectedObject, CastingQ);
                return;
            }
            if(selfCasted){
                CharacterCastingTargetSpell(selectedObject, null, CastingQ, mouseWorldPosition2D);
                return;
            }
            bool friendlySpell = StatAsset.Instance.DetermineFriendly(spell);
            //check if target required
            if(selectedObject.Target != null){
                if(spell == "Resurrect" && !selectedObject.Target.Dying){
                    ImproperCheckText.Invoke("Can only cast that on the dead");
                    return;
                }
                if(spell == "Purge" || spell == "Dispel"){
                    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingQ, mouseWorldPosition2D);
                    return;
                }
                if(selectedObject.GetFriendly(selectedObject.Target) && friendlySpell){
                    if(selectedObject.Target.Dying && spell != "Resurrect"){
                        ImproperCheckText.Invoke($"{selectedObject.SpellQ} cannot be cast on a dead target");
                        return;
                    }
                    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingQ, mouseWorldPosition2D);
                    return;
                } else if(selectedObject.GetFriendly(selectedObject.Target) && !friendlySpell){
                    ImproperCheckText.Invoke($"{selectedObject.SpellQ} cannot be cast on a friendly target");
                    return;
                }
                if(!selectedObject.GetFriendly(selectedObject.Target) && !friendlySpell){
                    if(selectedObject.Target.Dying){
                        ImproperCheckText.Invoke($"{selectedObject.SpellQ} cannot be cast on a dead target");
                        return;
                    }
                    if(spell == "Backstab"){
                        if(selectedObject.IsBehindAnotherObject(selectedObject.Target.transform.position, selectedObject.Target.GetFacingDirection())){
                            ImproperCheckText.Invoke($"We are behind target nice!");
                        } else {
                            ImproperCheckText.Invoke($"Must be behind target for that");
                            return;
                        }
                    }
                    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingQ, mouseWorldPosition2D);
                } else if(!selectedObject.GetFriendly(selectedObject.Target) && friendlySpell){
                    ImproperCheckText.Invoke($"{selectedObject.SpellQ} cannot be cast on a hostile target");
                    return;
                }
                //clean this up and make it so that we can see if we need to cast immediately or later
            } else {
                //try to cast aoe on the mouse position?
            }
    }
    void PressedECombat(){
        //use combatpartywindow
            MovingObject selectedObject = CombatPartyView.instance.GetSelected();
            
            if(selectedObject == null){
                ImproperCheckText.Invoke($"No target set");
                return;
            }
            if(!selectedObject.GetComponent<NetworkIdentity>().hasAuthority){
                return;
            }
            GameObject spellEGO = SpellManager.instance.GetSpellE();
            if(spellEGO == null){
                return;
            }
            Spell spellE = spellEGO.GetComponent<Spell>();
            if(spellE == null){
                return;
            }
            if(selectedObject.Silenced){
                ImproperCheckText.Invoke($"Cannot cast while silenced");
                return;
            }
            //PlayerCharacter selectedPlayer = selectedObject.GetComponent<PlayerCharacter>();
            
            if(selectedObject.SpellE == "None"){
                ImproperCheckText.Invoke($"Unit does not have a spell selected for that key");
                return;
            }
            if(selectedObject.SpellECoolDown){
                ImproperCheckText.Invoke($"{selectedObject.SpellE} is on cooldown");
            }
            if(selectedObject.stamina > 0)
            {
                ImproperCheckText.Invoke($"Unit does not have enough stamina to cast {selectedObject.SpellE}");
                return;
            }
            var nameMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellE, @"^\D*");
            string spell = nameMatch.Value.Trim(); 
            int _spellRank = 1;
            // Extract spell rank
            var rankMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellE, @"\d+$");
            if (rankMatch.Success) {
                _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
            }
            if(!selectedObject.shield && spell == "Bash" || !selectedObject.shield && spell == "Shield Bash"){
                ImproperCheckText.Invoke($"Must have a shield for that");
                return;
            }
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mouseWorldPosition2D = new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);

            if(selectedObject.cur_mp < StatAsset.Instance.GetSpellCost(spell)){
                ImproperCheckText.Invoke($"Unit does not have enough MP for {selectedObject.SpellE}");
                return;
            }
            if(!spellE.GetCastable()){
                ImproperCheckText.Invoke($"Unit's spell {selectedObject.SpellE} is on cooldown");
                return;
            }
            if(selectedObject.moving){
                //tell server to stop moving
                selectedObject.CmdStopMoving();
            }
            float selfCast = StatAsset.Instance.SelfCasted(spell);
            bool selfCasted = false;
            if(selfCast == 1){
                selfCasted = true;
            }
            if(StatAsset.Instance.SkillShot(spell)){
                if(SavedClick != null){
                    Destroy(SavedClick.radiusIndicator);
                    SavedClick = null;
                }
                BuildAoeClick(selectedObject, CastingE);
                return;
            }
            if(selfCasted){
                CharacterCastingTargetSpell(selectedObject, null, CastingE, mouseWorldPosition2D);
                return;
            }
            bool friendlySpell = StatAsset.Instance.DetermineFriendly(spell);
            //check if target required
            if(selectedObject.Target != null){
                if(spell == "Resurrect" && !selectedObject.Target.Dying){
                    ImproperCheckText.Invoke("Can only cast that on the dead");
                    return;
                }
                if(spell == "Purge" || spell == "Dispel"){
                    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingE, mouseWorldPosition2D);
                    return;
                }
                if(selectedObject.GetFriendly(selectedObject.Target) && friendlySpell){
                    if(selectedObject.Target.Dying && spell != "Resurrect"){
                        ImproperCheckText.Invoke($"{selectedObject.SpellE} cannot be cast on a dead target");
                        return;
                    }
                    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingE, mouseWorldPosition2D);
                    return;
                } else if(selectedObject.GetFriendly(selectedObject.Target) && !friendlySpell){
                    ImproperCheckText.Invoke($"{selectedObject.SpellE} cannot be cast on a friendly target");
                    return;
                }
                
                if(!selectedObject.GetFriendly(selectedObject.Target) && !friendlySpell){
                    if(selectedObject.Target.Dying){
                        ImproperCheckText.Invoke($"{selectedObject.SpellE} cannot be cast on a dead target");
                        return;
                    }
                    if(spell == "Backstab"){
                        if(selectedObject.IsBehindAnotherObject(selectedObject.Target.transform.position, selectedObject.Target.GetFacingDirection())){
                            ImproperCheckText.Invoke($"We are behind target nice!");
                        } else {
                            ImproperCheckText.Invoke($"Must be behind target for that");
                            return;
                        }
                    }
                    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingE, mouseWorldPosition2D);
                } else if(!selectedObject.GetFriendly(selectedObject.Target) && friendlySpell){
                    ImproperCheckText.Invoke($"{selectedObject.SpellE} cannot be cast on a hostile target");
                    return;
                }
                //clean this up and make it so that we can see if we need to cast immediately or later
            } else {
                //try to cast aoe on the mouse position?
            }
    }
    void PressedRCombat(){
        MovingObject selectedObject = CombatPartyView.instance.GetSelected();
            
            if(selectedObject == null){
                ImproperCheckText.Invoke($"No target set");
                return;
            }
            if(!selectedObject.GetComponent<NetworkIdentity>().hasAuthority){
                return;
            }
            GameObject spellRGO = SpellManager.instance.GetSpellR();
            if(spellRGO == null){
                return;
            }
            Spell spellR = spellRGO.GetComponent<Spell>();
            if(spellR == null){
                return;
            }
            if(selectedObject.Silenced){
                ImproperCheckText.Invoke($"Cannot cast while silenced");
                return;
            }
            //PlayerCharacter selectedPlayer = selectedObject.GetComponent<PlayerCharacter>();
            
            if(selectedObject.SpellR == "None"){
                ImproperCheckText.Invoke($"Unit does not have a spell selected for that key");
                return;
            }
            if(selectedObject.SpellRCoolDown){
                ImproperCheckText.Invoke($"{selectedObject.SpellR} is on cooldown");
            }
            if(selectedObject.stamina > 0)
            {
                ImproperCheckText.Invoke($"Unit does not have enough stamina to cast {selectedObject.SpellR}");
                return;
            }
            var nameMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellR, @"^\D*");
            string spell = nameMatch.Value.Trim(); 
            int _spellRank = 1;
            // Extract spell rank
            var rankMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellR, @"\d+$");
            if (rankMatch.Success) {
                _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
            }
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mouseWorldPosition2D = new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);
            if(!selectedObject.shield && spell == "Bash" || !selectedObject.shield && spell == "Shield Bash"){
                ImproperCheckText.Invoke($"Must have a shield for that");
                return;
            }
            if(selectedObject.cur_mp < StatAsset.Instance.GetSpellCost(spell)){
                ImproperCheckText.Invoke($"Unit does not have enough MP for {selectedObject.SpellR}");
                return;
            }
            if(!spellR.GetCastable()){
                ImproperCheckText.Invoke($"Unit's spell {selectedObject.SpellR} is on cooldown");
                return;
            }
            if(selectedObject.moving){
                //tell server to stop moving
                selectedObject.CmdStopMoving();
            }
            float selfCast = StatAsset.Instance.SelfCasted(spell);
            bool selfCasted = false;
            if(selfCast == 1){
                selfCasted = true;
            }
            //var nameMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellR, @"^\D*");
            //string spellNameShaved = nameMatch.Value.Trim(); 
            //float targetable = SelfCasted(spell);
            //if(targetable != 0f){
            //    //cast the spell its a self target
            //    selectedObject.CmdSelfTarget(selectedObject.SpellR, CastingR);
            //    //possibly clear Mode to deselect character?
            //    return;
            //}
            
            if(StatAsset.Instance.SkillShot(spell)){
                if(SavedClick != null){
                    Destroy(SavedClick.radiusIndicator);
                    SavedClick = null;
                }
                BuildAoeClick(selectedObject, CastingR);
                return;
            }
            if(selfCasted){
                CharacterCastingTargetSpell(selectedObject, null, CastingR, mouseWorldPosition2D);
                return;
            }
            //if(AnyTarget(spell)){
            //    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingR, mouseWorldPosition2D);
            //}
            bool friendlySpell = StatAsset.Instance.DetermineFriendly(spell);
            //check if target required
            if(selectedObject.Target != null){
                if(spell == "Resurrect" && !selectedObject.Target.Dying){
                    ImproperCheckText.Invoke("Can only cast that on the dead");
                    return;
                }
                if(spell == "Purge" || spell == "Dispel"){
                    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingR, mouseWorldPosition2D);
                    return;
                }
                if(selectedObject.GetFriendly(selectedObject.Target) && friendlySpell){
                    if(selectedObject.Target.Dying && spell != "Resurrect"){
                        ImproperCheckText.Invoke($"{selectedObject.SpellR} cannot be cast on a dead target");
                        return;
                    }
                    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingR, mouseWorldPosition2D);
                    return;
                } else if(selectedObject.GetFriendly(selectedObject.Target) && !friendlySpell){
                    ImproperCheckText.Invoke($"{selectedObject.SpellR} cannot be cast on a friendly target");
                    return;
                }
                
                if(!selectedObject.GetFriendly(selectedObject.Target) && !friendlySpell){
                    if(selectedObject.Target.Dying){
                        ImproperCheckText.Invoke($"{selectedObject.SpellR} cannot be cast on a dead target");
                        return;
                    }
                    if(spell == "Backstab"){
                        if(selectedObject.IsBehindAnotherObject(selectedObject.Target.transform.position, selectedObject.Target.GetFacingDirection())){
                            ImproperCheckText.Invoke($"We are behind target nice!");
                        } else {
                            ImproperCheckText.Invoke($"Must be behind target for that");
                            return;
                        }
                    }
                    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingR, mouseWorldPosition2D);
                } else if(!selectedObject.GetFriendly(selectedObject.Target) && friendlySpell){
                    ImproperCheckText.Invoke($"{selectedObject.SpellR} cannot be cast on a hostile target");
                    return;
                }
                //clean this up and make it so that we can see if we need to cast immediately or later
            } else {
                //try to cast aoe on the mouse position?
            }
    }
    void PressedFCombat(){
        MovingObject selectedObject = CombatPartyView.instance.GetSelected();
            if(selectedObject == null){
                ImproperCheckText.Invoke($"No target set");
                return;
            }
            if(!selectedObject.GetComponent<NetworkIdentity>().hasAuthority){
                return;
            }
            GameObject spellFGO = SpellManager.instance.GetSpellF();
            if(spellFGO == null){
                return;
            }
            Spell spellf = spellFGO.GetComponent<Spell>();
            if(spellf == null){
                return;
            }
            if(selectedObject.Silenced){
                ImproperCheckText.Invoke($"Cannot cast while silenced");
                return;
            }
            if(selectedObject.SpellF == "None"){
                ImproperCheckText.Invoke($"Unit does not have a spell selected for that key");
                return;
            }
            if(selectedObject.SpellFCoolDown){
                ImproperCheckText.Invoke($"{selectedObject.SpellF} is on cooldown");
            }
            if(selectedObject.stamina > 0)
            {
                ImproperCheckText.Invoke($"Unit does not have enough stamina to cast {selectedObject.SpellF}");
                return;
            }
            var nameMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellF, @"^\D*");
            string spell = nameMatch.Value.Trim(); 
            int _spellRank = 1;
            // Extract spell rank
            var rankMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellF, @"\d+$");
            if (rankMatch.Success) {
                _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
            }
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mouseWorldPosition2D = new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);
            if(!selectedObject.shield && spell == "Bash" || !selectedObject.shield && spell == "Shield Bash"){
                ImproperCheckText.Invoke($"Must have a shield for that");
                return;
            }
            if(selectedObject.cur_mp < StatAsset.Instance.GetSpellCost(spell)){
                ImproperCheckText.Invoke($"Unit does not have enough MP for {selectedObject.SpellF}");
                return;
            }
            if(!spellf.GetCastable()){
                ImproperCheckText.Invoke($"Unit's spell {selectedObject.SpellF} is on cooldown");
                return;
            }
            if(selectedObject.moving){
                //tell server to stop moving
                selectedObject.CmdStopMoving();
            }
            
            //var nameMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellF, @"^\D*");
            //string spellNameShaved = nameMatch.Value.Trim(); 
            //float targetable = SelfCasted(spell);
            //if(targetable != 0f){
            //    //cast the spell its a self target
            //    selectedObject.CmdSelfTarget(selectedObject.SpellF, CastingF);
            //    //possibly clear Mode to deselect character?
            //    return;
            //}
            float selfCast = StatAsset.Instance.SelfCasted(spell);
            bool selfCasted = false;
            if(selfCast == 1){
                selfCasted = true;
            }
            if(StatAsset.Instance.SkillShot(spell)){
                if(SavedClick != null){
                    Destroy(SavedClick.radiusIndicator);
                    SavedClick = null;
                }
                BuildAoeClick(selectedObject, CastingF);
                return;
            }
            if(selfCasted){
                CharacterCastingTargetSpell(selectedObject, null, CastingF, mouseWorldPosition2D);
                return;
            }
            //if(AnyTarget(spell)){
            //    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingF, mouseWorldPosition2D);
            //}
            bool friendlySpell = StatAsset.Instance.DetermineFriendly(spell);
            //check if target required
            if(selectedObject.Target != null){
                if(spell == "Resurrect" && !selectedObject.Target.Dying){
                    ImproperCheckText.Invoke("Can only cast that on the dead");
                    return;
                }
                if(spell == "Purge" || spell == "Dispel"){
                    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingF, mouseWorldPosition2D);
                    return;
                }
                if(selectedObject.GetFriendly(selectedObject.Target) && friendlySpell){
                    if(selectedObject.Target.Dying && spell != "Resurrect"){
                        ImproperCheckText.Invoke($"{selectedObject.SpellF} cannot be cast on a dead target");
                        return;
                    }
                    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingF, mouseWorldPosition2D);
                    return;
                } else if(selectedObject.GetFriendly(selectedObject.Target) && !friendlySpell){
                    ImproperCheckText.Invoke($"{selectedObject.SpellF} cannot be cast on a friendly target");
                    return;
                }
                
                if(!selectedObject.GetFriendly(selectedObject.Target) && !friendlySpell){
                    if(selectedObject.Target.Dying){
                        ImproperCheckText.Invoke($"{selectedObject.SpellF} cannot be cast on a dead target");
                        return;
                    }
                    if(spell == "Backstab"){
                        if(selectedObject.IsBehindAnotherObject(selectedObject.Target.transform.position, selectedObject.Target.GetFacingDirection())){
                            ImproperCheckText.Invoke($"We are behind target nice!");
                        } else {
                            ImproperCheckText.Invoke($"Must be behind target for that");
                            return;
                        }
                    }
                    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingF, mouseWorldPosition2D);
                } else if(!selectedObject.GetFriendly(selectedObject.Target) && friendlySpell){
                    ImproperCheckText.Invoke($"{selectedObject.SpellF} cannot be cast on a hostile target");
                    return;
                }
                //clean this up and make it so that we can see if we need to cast immediately or later
            } else {
                //try to cast aoe on the mouse position?
            }
    }
    void PressedPotionCombat(){
        MovingObject selectedObject = CombatPartyView.instance.GetSelected();
            if(selectedObject == null){
                ImproperCheckText.Invoke($"No target set");
                return;
            }
            if(!selectedObject.GetComponent<NetworkIdentity>().hasAuthority){
                return;
            }
            GameObject potionSpellCheck = SpellManager.instance.GetPotionSpell();
            if(potionSpellCheck == null){
                return;
            }
            Spell potionSpell = potionSpellCheck.GetComponent<Spell>();
            if(potionSpell == null){
                return;
            }
            PlayerCharacter selectedPlayer = selectedObject.GetComponent<PlayerCharacter>();
            if(!selectedPlayer){
                ImproperCheckText.Invoke($"No target set");
                return;
            }
            
            if(selectedPlayer.Casting){
                selectedPlayer.CmdCancelSpell();
            }
            foreach(var sheet in InformationSheets){
                if(sheet.CharacterID == selectedPlayer.CharID){
                    foreach(var consumedItemCheck in sheet.CharInventoryData){
                        if(consumedItemCheck.Value.Deleted || consumedItemCheck.Value.amount == 0){
                            continue;
                        }
                        if(consumedItemCheck.Value.EQUIPPED && consumedItemCheck.Value.GetItemType() == ItemSelectable.ItemType.PotionT1 || consumedItemCheck.Value.EQUIPPED && consumedItemCheck.Value.GetItemType() == ItemSelectable.ItemType.PotionT2){
                            if(consumedItemCheck.Value.amount == 1){
                                ConsumingItemFully(selectedPlayer.CharID, consumedItemCheck.Value);
                                SpellManager.instance.FinishedConsumablePotion();
                                return;
                            }
                            if(consumedItemCheck.Value.amount > 1){
                                SpellManager.instance.PartiallyConsumablePotion(consumedItemCheck.Value.amount);
                                ConsumingItemPartially(selectedPlayer.CharID, consumedItemCheck.Value);
                                return;
                            }
                        }
                    }
                    break;
                }
            }
            ImproperCheckText.Invoke($"{selectedPlayer.CharacterName} does not have a potion selected");
    }
    void PressedFoodCombat(){
        MovingObject selectedObject = CombatPartyView.instance.GetSelected();
            if(selectedObject == null){
                ImproperCheckText.Invoke($"No target set");
                return;
            }
            if(!selectedObject.GetComponent<NetworkIdentity>().hasAuthority){
                return;
            }
            GameObject foodSpellCheck = SpellManager.instance.GetFoodSpell();
            if(foodSpellCheck == null){
                return;
            }
            Spell foodSpell = foodSpellCheck.GetComponent<Spell>();
            if(foodSpell == null){
                return;
            }
            PlayerCharacter selectedPlayer = selectedObject.GetComponent<PlayerCharacter>();
            if(!selectedPlayer){
                ImproperCheckText.Invoke($"No target set");
                return;
            }
            
            if(selectedPlayer.Casting){
                selectedPlayer.CmdCancelSpell();
            }
            foreach(var sheet in InformationSheets){
                if(sheet.CharacterID == selectedPlayer.CharID){
                    foreach(var consumedItemCheck in sheet.CharInventoryData){
                        if(consumedItemCheck.Value.Deleted || consumedItemCheck.Value.amount == 0){
                            continue;
                        }
                        if(consumedItemCheck.Value.EQUIPPED && consumedItemCheck.Value.GetItemType() == ItemSelectable.ItemType.FoodT1){
                            if(consumedItemCheck.Value.amount == 1){
                                ConsumingItemFully(selectedPlayer.CharID, consumedItemCheck.Value);
                                SpellManager.instance.FinishedConsumableFood();
                                return;
                            }
                            if(consumedItemCheck.Value.amount > 1){
                                //SpellManager.instance.PartiallyConsumableFood(consumedItemCheck.Value.amount);
                                ConsumingItemPartially(selectedPlayer.CharID, consumedItemCheck.Value);
                                return;
                            }
                        }
                    }
                    break;
                }
            }
            ImproperCheckText.Invoke($"{selectedPlayer.CharacterName} does not have a food selected");
    }
    void PressedTactOneCombat(){
        if(TactSpellOne){
                ImproperCheckText.Invoke($"Spell is on cooldown");
                return;
            }
            if(SpellOne == "Empty"){
                ImproperCheckText.Invoke($"Equip a spell here first");
                return;
            }
            if(SpellOne == "None"){
                ImproperCheckText.Invoke($"Equip a spell here first");
                return;
            }
            if(SpellOne == "Rest" || SpellOne == "Repel"){
                ImproperCheckText.Invoke($"Must be on the world map to use this ability");
                return;
            }
            if(SpellOne == "Fertile Soil"){
                ImproperCheckText.Invoke($"Must be in a town or city to use this ability");
                return;
            }
            if(SpellOne == "Far Sight"){
                
                //handle far sight later
            }
            if(SpellOne == "Evacuate"){
                foreach(var mo in FriendlyList){
                    if(mo != null){
                        if(mo.GetComponent<NetworkIdentity>().hasAuthority){
                            mo.EvacuateSpellCastTact(this);
                            return;
                        }
                    }
                }
                //handle far sight later
                
                return;
            }
            MovingObject selectedObject = CombatPartyView.instance.GetSelected();
            //Get required Type, basically if we need a target or if its a utility spell
            (bool friendlyRequired, int type) = GetSpellTactType(SpellOne);
            if(selectedObject == null){
                if(friendlyRequired){
                    ImproperCheckText.Invoke($"Select an friendly target for that spell");
                    return;
                } else {
                    ImproperCheckText.Invoke($"Select an enemy target for that spell");
                    return;
                }
            }
            PlayerCharacter pcSelectedCheck = selectedObject.GetComponent<PlayerCharacter>();
            if(friendlyRequired && !FriendlyList.Contains(selectedObject)){
                ImproperCheckText.Invoke($"Select an friendly target for that spell");
                return;
            }
            if(!friendlyRequired && FriendlyList.Contains(selectedObject)){
                MovingObject selectedTarget = selectedObject.Target;
                if(selectedTarget == null){
                    ImproperCheckText.Invoke($"Select an enemy target for that spell");
                    return;
                }
                
                if(FriendlyList.Contains(selectedTarget)){
                    ImproperCheckText.Invoke($"Select an enemy target for that spell");
                    return;
                } else {
                    if(SpellOne == "Lightning Bolt"){
                        Speaker.Invoke(601);
                    }
                    if(SpellOne == "Enthrall"){
                        Speaker.Invoke(602);
                    }
                    if(SpellOne == "Morale Boost"){
                        Speaker.Invoke(603);
                    }
                    if(SpellOne == "Refresh"){
                        Speaker.Invoke(604);
                    }
                    if(SpellOne == "Ignite"){
                        Speaker.Invoke(605);
                    }
                    if(SpellOne == "Repel"){
                        Speaker.Invoke(606);
                    }
                    if(SpellOne == "Mend"){
                        Speaker.Invoke(607);
                    }
                    if(SpellOne == "Absorb"){
                        Speaker.Invoke(608);
                    }
                    if(SpellOne == "Light"){
                        Speaker.Invoke(609);
                    }
                    CmdBuildSpellSelected(selectedTarget, 1, type);
                    return;
                }
            }
            if(SpellOne == "Lightning Bolt"){
                Speaker.Invoke(601);
            }
            if(SpellOne == "Enthrall"){
                Speaker.Invoke(602);
            }
            if(SpellOne == "Morale Boost"){
                Speaker.Invoke(603);
            }
            if(SpellOne == "Refresh"){
                Speaker.Invoke(604);
            }
            if(SpellOne == "Ignite"){
                Speaker.Invoke(605);
            }
            if(SpellOne == "Repel"){
                Speaker.Invoke(606);
            }
            if(SpellOne == "Mend"){
                Speaker.Invoke(607);
            }
            if(SpellOne == "Absorb"){
                Speaker.Invoke(608);
            }
            if(SpellOne == "Light"){
                Speaker.Invoke(609);
            }
            CmdBuildSpellSelected(selectedObject, 1, type);
    }
    void PressedTactTwoCombat(){
        if(TactSpellTwo){
                ImproperCheckText.Invoke($"Spell is on cooldown");
                return;
            }
            if(SpellTwo == "Empty"){
                ImproperCheckText.Invoke($"Equip a spell here first");
                return;
            }
            if(SpellTwo == "None"){
                ImproperCheckText.Invoke($"Equip a spell here first");
                return;
            }
            if(SpellTwo == "Rest" || SpellTwo == "Repel"){
                ImproperCheckText.Invoke($"Must be on the world map to use this ability");
                return;
            }
            if(SpellTwo == "Fertile Soil"){
                ImproperCheckText.Invoke($"Must be in a town or city to use this ability");
                return;
            }
            if(SpellTwo == "Far Sight"){
                Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 mouseWorldPosition2D = new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);
                //handle far sight later
            }
            if(SpellTwo == "Evacuate"){
                foreach(var mo in FriendlyList){
                    if(mo != null){
                        if(mo.GetComponent<NetworkIdentity>().hasAuthority){
                            mo.EvacuateSpellCastTact(this);
                            return;
                        }
                    }
                }
            }
            MovingObject selectedObject = CombatPartyView.instance.GetSelected();
            //Get required Type, basically if we need a target or if its a utility spell
            (bool friendlyRequired, int type) = GetSpellTactType(SpellTwo);
            if(selectedObject == null){
                if(friendlyRequired){
                    ImproperCheckText.Invoke($"Select an friendly target for that spell");
                    return;
                } else {
                    ImproperCheckText.Invoke($"Select an enemy target for that spell");
                    return;
                }
            }
            if(friendlyRequired && !FriendlyList.Contains(selectedObject)){
                ImproperCheckText.Invoke($"Select an friendly target for that spell");
                return;
            }
            if(!friendlyRequired &&  !FriendlyList.Contains(selectedObject)){
                MovingObject selectedTarget = selectedObject.Target;
                if(selectedTarget == null){
                    ImproperCheckText.Invoke($"Select an enemy target for that spell");
                    return;
                }
                if(FriendlyList.Contains(selectedTarget)){
                    ImproperCheckText.Invoke($"Select an enemy target for that spell");
                    return;
                } else {
                    if(SpellTwo == "Lightning Bolt"){
                        Speaker.Invoke(601);
                    }
                    if(SpellTwo == "Enthrall"){
                        Speaker.Invoke(602);
                    }
                    if(SpellTwo == "Morale Boost"){
                        Speaker.Invoke(603);
                    }
                    if(SpellTwo == "Refresh"){
                        Speaker.Invoke(604);
                    }
                    if(SpellTwo == "Ignite"){
                        Speaker.Invoke(605);
                    }
                    if(SpellTwo == "Repel"){
                        Speaker.Invoke(606);
                    }
                    if(SpellTwo == "Mend"){
                        Speaker.Invoke(607);
                    }
                    if(SpellTwo == "Absorb"){
                        Speaker.Invoke(608);
                    }
                    if(SpellTwo == "Light"){
                        Speaker.Invoke(609);
                    }
                    CmdBuildSpellSelected(selectedTarget, 2, type);
                    return;
                }
            }
            if(SpellTwo == "Lightning Bolt"){
                Speaker.Invoke(601);
            }
            if(SpellTwo == "Enthrall"){
                Speaker.Invoke(602);
            }
            if(SpellTwo == "Morale Boost"){
                Speaker.Invoke(603);
            }
            if(SpellTwo == "Refresh"){
                Speaker.Invoke(604);
            }
            if(SpellTwo == "Ignite"){
                Speaker.Invoke(605);
            }
            if(SpellTwo == "Repel"){
                Speaker.Invoke(606);
            }
            if(SpellTwo == "Mend"){
                Speaker.Invoke(607);
            }
            if(SpellTwo == "Absorb"){
                Speaker.Invoke(608);
            }
            if(SpellTwo == "Light"){
                Speaker.Invoke(609);
            }
            CmdBuildSpellSelected(selectedObject, 2, type);
    }
    public class AoeSpellClickSave {
        public MovingObject caster{ get; private set; }
        public bool PET{ get; private set; }
        public string spellSlot{ get; private set; }
        public GameObject radiusIndicator{ get; private set; }
        public AoeSpellClickSave(MovingObject mo, string SpellSlot, GameObject aoeRadiusIndicatorCursor, bool pet){
            caster = mo;
            spellSlot = SpellSlot;
            radiusIndicator = aoeRadiusIndicatorCursor;
            PET = pet;
        }
        public void ExecuteClick(){//add a way to tell if its a pet? we need the alt Q for the spell grab from spell manager
            if(spellSlot == CastingQ){
                ScenePlayer.localPlayer.PressedQCombatClick(caster, PET);
            }
            if(spellSlot == CastingE){
                ScenePlayer.localPlayer.PressedECombatClick(caster, PET);
            }
            if(spellSlot == CastingR){
                ScenePlayer.localPlayer.PressedRCombatClick(caster, PET);
            }
            if(spellSlot == CastingF){
                ScenePlayer.localPlayer.PressedFCombatClick(caster, PET);
            }
        }
    }
    void BuildAoeClick(MovingObject mo, string SpellSlot){
        
        GameObject aoeRadiusIndicatorCursor = Instantiate(tilePrefab);
        MoveableTile tile = aoeRadiusIndicatorCursor.GetComponent<MoveableTile>();
        string Spell = string.Empty;
        if(SpellSlot == CastingQ){
            Spell = mo.SpellQ;
        }
        if(SpellSlot == CastingE){
            Spell = mo.SpellE;
        }
        if(SpellSlot == CastingR){
            Spell = mo.SpellR;
        }
        if(SpellSlot == CastingF){
            Spell = mo.SpellF;
        }
        var nameMatch = Regex.Match(Spell, @"^\D*");
        string spell = nameMatch.Value.Trim(); 
        int _spellRank = 1;
        // Extract spell rank
        var rankMatch = Regex.Match(Spell, @"\d+$");
        if (rankMatch.Success) {
            _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
        }
        float radius = AoeRadius(spell);
        (int type, int element) = StatAsset.Instance.GetSpellType(spell);
        string colorHex = string.Empty;
        switch (type)
        {
            case 1:
                colorHex = "FF0000"; // Red
                break;
            case 2:
                colorHex = "00FF00"; // Green
                break;
            case 3:
                colorHex = "0000FF"; // Blue
                break;
            case 4:
                colorHex = "FFC0CB"; // Pink
                break;
            case 6:
                colorHex = "FFFF00"; // Yellow
                break;
        }
        tile.StartCoroutine(tile.SelectedAbilityTileAoeClick(radius, colorHex));
        bool isPet = false;
        if(mo.CharmedOWNER != null){
            isPet = true;
        }
        SavedClick = new AoeSpellClickSave(mo, SpellSlot, aoeRadiusIndicatorCursor, isPet);
    }
    AoeSpellClickSave SavedClick;
    bool HaveAoeSpellClickSaved(){
        if(SavedClick != null){
            isDragging = false;
            SavedClick.ExecuteClick();
            Destroy(SavedClick.radiusIndicator);
            SavedClick = null;
            return true;
        } else {
            return false;
        }
    }
    public bool CheckForUIHIt(){
        bool UIHit = false;
        List<RectTransform> uiRectTransforms = new List<RectTransform>();
        Vector2 checkPos = Input.mousePosition;
        GameObject spellQGOPET = SpellManager.instance.GetPETSpellQ();
        if(spellQGOPET != null){
            RectTransform PETspellQRect = spellQGOPET.GetComponent<RectTransform>();
            uiRectTransforms.Add(PETspellQRect);
        }
        GameObject spellEGOPET = SpellManager.instance.GetPETSpellE();
        if(spellEGOPET != null){
            RectTransform PETspellERect = spellEGOPET.GetComponent<RectTransform>();
            uiRectTransforms.Add(PETspellERect);
        }
        GameObject spellRGOPET = SpellManager.instance.GetPETSpellR();
        if(spellRGOPET != null){
            RectTransform PETspellRRect = spellRGOPET.GetComponent<RectTransform>();
            uiRectTransforms.Add(PETspellRRect);
        }
        GameObject spellFGOPET = SpellManager.instance.GetPETSpellF();
        if(spellFGOPET != null){
            RectTransform PETspellFRect = spellFGOPET.GetComponent<RectTransform>();
            uiRectTransforms.Add(PETspellFRect);
        }
        GameObject spellQGO = SpellManager.instance.GetSpellQ();
        if(spellQGO != null){
            RectTransform spellQRect = spellQGO.GetComponent<RectTransform>();
            uiRectTransforms.Add(spellQRect);
        }
        GameObject spellEGO = SpellManager.instance.GetSpellE();
        if(spellEGO != null){
            RectTransform spellERect = spellEGO.GetComponent<RectTransform>();
            uiRectTransforms.Add(spellERect);
        }
        GameObject spellRGO = SpellManager.instance.GetSpellR();
        if(spellRGO != null){
            RectTransform spellRRect = spellRGO.GetComponent<RectTransform>();
            uiRectTransforms.Add(spellRRect);
        }
        GameObject spellFGO = SpellManager.instance.GetSpellF();
        if(spellFGO != null){
            RectTransform spellFRect = spellFGO.GetComponent<RectTransform>();
            uiRectTransforms.Add(spellFRect);
        }
        GameObject potionGO = SpellManager.instance.GetPotionSpell();
        if(potionGO != null){
            RectTransform PotionRect = potionGO.GetComponent<RectTransform>();
            uiRectTransforms.Add(PotionRect);
        }
        GameObject FoodGO = SpellManager.instance.GetFoodSpell();
        if(FoodGO != null){
            RectTransform FoodRect = FoodGO.GetComponent<RectTransform>();
            uiRectTransforms.Add(FoodRect);
        }
        GameObject TacticianSpellOne = SpellManager.instance.GetTactSpellOne();
        if(TacticianSpellOne != null){
            RectTransform spellOneRect = TacticianSpellOne.GetComponent<RectTransform>();
            uiRectTransforms.Add(spellOneRect);
        }
        GameObject TacticianSpellTwo = SpellManager.instance.GetTactSpellTwo();
        if(TacticianSpellTwo != null){
            RectTransform spellTwoRect = TacticianSpellTwo.GetComponent<RectTransform>();
            uiRectTransforms.Add(spellTwoRect);
        }
        RectTransform chatRectTransform = ChatPlayerUI.instance.GetComponent<RectTransform>();
        if(ChatPlayerUI.instance.GetComponent<Canvas>().enabled){
            uiRectTransforms.Add(chatRectTransform);
        }
        RectTransform settingsRectTransform = SettingsMenu.Instance.GetComponent<RectTransform>();
        if(SettingsMenu.Instance.GetComponent<Canvas>().enabled){
            uiRectTransforms.Add(settingsRectTransform);
        }
        RectTransform groupRectTransform = GroupSelectSheet.Instance.GetComponent<RectTransform>();
        if(GroupSelectSheet.Instance.GetComponent<Canvas>().enabled){
            uiRectTransforms.Add(groupRectTransform);
        }
        RectTransform charOneRectTransform = CharacterSheet.Instance.GetComponent<RectTransform>();
        if(CharacterSheet.Instance.GetComponent<Canvas>().enabled){
            uiRectTransforms.Add(charOneRectTransform);
        }
        RectTransform charTwoRectTransform = CharacterTwoSheet.Instance.GetComponent<RectTransform>();
        if(CharacterTwoSheet.Instance.GetComponent<Canvas>().enabled){
            uiRectTransforms.Add(charTwoRectTransform);
        }
        RectTransform TactRectTransform = TacticianSheet.Instance.GetComponent<RectTransform>();
        if(TacticianSheet.Instance.GetComponent<Canvas>().enabled){
            uiRectTransforms.Add(TactRectTransform);
        }
        RectTransform StashRectTransform = StashSheet.Instance.GetComponent<RectTransform>();
        if(StashSheet.Instance.GetComponent<Canvas>().enabled){
            uiRectTransforms.Add(StashRectTransform);
        }
        RectTransform dragonRectTransform = AncientDragonShrineSheet.Instance.GetComponent<RectTransform>();
        if(AncientDragonShrineSheet.Instance.GetComponent<Canvas>().enabled){
            uiRectTransforms.Add(dragonRectTransform);
        }
        RectTransform StoreRectTransform = StoreFront.Instance.GetComponent<RectTransform>();
        if(StoreFront.Instance.GetComponent<Canvas>().enabled){
            uiRectTransforms.Add(StoreRectTransform);
        }
        RectTransform ItemShopRectTransform = ArudineItemShopSheet.Instance.GetComponent<RectTransform>();
        if(ArudineItemShopSheet.Instance.GetComponent<Canvas>().enabled){
            uiRectTransforms.Add(ItemShopRectTransform);
        }
        RectTransform MarketRectTransform = MarketSheet.Instance.GetComponent<RectTransform>();
        if(MarketSheet.Instance.GetComponent<Canvas>().enabled){
            uiRectTransforms.Add(MarketRectTransform);
        }
        RectTransform CraftRectTransform = CraftingSheet.instance.GetComponent<RectTransform>();
        if(CraftingSheet.instance.GetComponent<Canvas>().enabled){
            uiRectTransforms.Add(CraftRectTransform);
        }
        RectTransform QuestRectTransform = QuestSheet.Instance.GetComponent<RectTransform>();
        if(QuestSheet.Instance.GetComponent<Canvas>().enabled){
            uiRectTransforms.Add(QuestRectTransform);
        }
        RectTransform CompendiumRectTransform = CompendiumSheet.Instance.GetComponent<RectTransform>();
        if(CompendiumSheet.Instance.GetComponent<Canvas>().enabled){
            uiRectTransforms.Add(CompendiumRectTransform);
        }
        RectTransform tradeRectTransform = TradeSupervisor.instance.GetComponent<RectTransform>();
        if(TradeSupervisor.instance.GetComponent<Canvas>().enabled){
            uiRectTransforms.Add(tradeRectTransform);
        }
        foreach (RectTransform uiRectTransform in uiRectTransforms)
        {
           // Debug.Log($"UI Element: {uiRectTransform.name}, Mouse Position: {checkPos} Rect: {uiRectTransform.rect}");

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(uiRectTransform, checkPos, null, out Vector2 localPoint))
            {
              //  Debug.Log($"Local Point: {localPoint}");

                if (uiRectTransform.rect.Contains(localPoint))
                {
                    Spell spellCheck = uiRectTransform.gameObject.GetComponent<Spell>();
                    if(spellCheck){
                        
                        MovingObject mo = spellCheck.GetOwner();
                        string spellCasted = spellCheck.GetSPELLSLOT();
                        string spell = spellCheck.GetSPELLNAME();
                        bool AoeCheck = false;
                        if(StatAsset.Instance.SkillShot(spell)){
                            AoeCheck = true;
                        }
                        if(spellCasted == "Q"){
                            if(AoeCheck){
                                BuildAoeClick(mo, CastingQ);
                            } else {
                                PressedQCombat();
                            }
                        }
                        if(spellCasted == "E"){
                            if(AoeCheck){
                                BuildAoeClick(mo, CastingE);
                            } else {
                                PressedECombat();
                            }
                        }
                        if(spellCasted == "R"){
                            if(AoeCheck){
                                BuildAoeClick(mo, CastingR);
                            } else {
                                PressedRCombat();
                            }
                        }
                        if(spellCasted == "F"){
                            if(AoeCheck){
                                BuildAoeClick(mo, CastingF);
                            } else {
                                PressedFCombat();
                            }
                        }
                        if(spellCasted == "AltQ"){
                            if(AoeCheck){
                                BuildAoeClick(mo, CastingQ);
                            } else {
                                PressedAltQCombat();
                            }
                        }
                        if(spellCasted == "AltE"){
                            if(AoeCheck){
                                BuildAoeClick(mo, CastingE);
                            } else {
                                PressedAltECombat();
                            }
                        }
                        if(spellCasted == "AltR"){
                            if(AoeCheck){
                                BuildAoeClick(mo, CastingR);
                            } else {
                                PressedAltRCombat();
                            }
                        }
                        if(spellCasted == "AltF"){
                            if(AoeCheck){
                                BuildAoeClick(mo, CastingF);
                            } else {
                                PressedAltFCombat();
                            }
                        }
                        if(spellCasted == "1"){
                            if(AoeCheck){
                                BuildAoeClick(mo, CastingQ);
                            } else {
                                PressedTactOneCombat();
                            }
                        }
                        if(spellCasted == "2"){
                            if(AoeCheck){
                                BuildAoeClick(mo, CastingQ);
                            } else {
                                PressedTactTwoCombat();
                            }
                        }
                        if(spellCasted == "3"){
                            PressedPotionCombat();
                        }
                        if(spellCasted == "4"){
                            PressedFoodCombat();
                        }
                    }
                    return true; // Return true as soon as a hit is detected
                }
            }
        }
        return UIHit;
    }
    IEnumerator QuickRelease(){
        yield return new WaitForSeconds(.1f);
    }
    public void SendPetAttack(MovingObject petowner, MovingObject pet, MovingObject target){
        if(petowner.GetFriendly(target)){
            ImproperCheckText.Invoke("Cannot send your pet to attack a friendly target");
            return;
        } else {
            List<MovingObject> MOs = new List<MovingObject>();
            MOs.Add(pet);
            CmdAttackUnit(MOs, target);
        }
    }
    public void SendPetHeel(MovingObject petowner, MovingObject pet){
        List<MovingObject> MOs = new List<MovingObject>();
        MOs.Add(pet);
        Vector2 targetPos = new Vector2(petowner.transform.position.x -.75f, petowner.transform.position.y);
        CmdMoveUnits(MOs, targetPos, GetFormationMode());
    }
    void RightClickCombat(){
        if(HaveAoeSpellClickSaved()){
            return;
        }
        Vector2 checkPos = Input.mousePosition;
        RaycastHit2D hitMiniMap = Physics2D.Raycast(checkPos, Vector2.zero, 0f, LayerMask.GetMask("MiniMap"));
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
       // print($"{worldPosition} is our worldPosition, mouse potion is {Input.mousePosition}");
        RaycastHit2D hitUI = Physics2D.Raycast(worldPosition, Vector2.zero, 0f, LayerMask.GetMask("UI"));
        if (hitMiniMap.collider != null){
            return;
        }
        if (hitUI.collider != null){
            return;
        }
        if(CheckForUIHIt()){
            return;
        }
                        bool mobOnly = false;

                //print("Right clicked!");
                bool Green = true;
                Vector2 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                if(selectedCharacters.Count > 0){
                    List<MovingObject> MOs = new List<MovingObject>();
                    Dictionary<MovingObject, int> siblingIndices = new Dictionary<MovingObject, int>();
                    foreach(var selectedChar in selectedCharacters){
                        
                        MovingObject pc = selectedChar.GetComponent<MovingObject>();
                        if(pc.GetComponent<NetworkIdentity>().hasAuthority)
                        MOs.Add(pc);
                    }
                    foreach(Transform child in CombatPartyView.instance.transform){
                        CharacterCombatUI _char = child.GetComponent<CharacterCombatUI>();
                        MovingObject charOwner = _char.owner;
                        if (MOs.Contains(charOwner)) {
                            siblingIndices[charOwner] = child.GetSiblingIndex();
                        }
                    }
                    // Sort MOs based on sibling indices
                    MOs = MOs.OrderBy(mo => siblingIndices.ContainsKey(mo) ? siblingIndices[mo] : int.MaxValue).ToList();

                    //MOs = MOs.OrderBy(mo => siblingIndices[mo]).ToList();
                    RaycastHit2D hit = Physics2D.Raycast(targetPos, Vector2.zero, 0f, LayerMask.GetMask("movingObjects"));
                    if (hit.collider == null){
                        // The target position is not blocked
                        RaycastHit2D Floor = Physics2D.Raycast(targetPos, Vector2.zero, 0f, LayerMask.GetMask("Floor"));
                        if(Floor.collider != null){
                            RaycastHit2D Wall = Physics2D.Raycast(targetPos, Vector2.zero, 0f, LayerMask.GetMask("blockingLayer"));
                            if(Wall.collider == null){
                                //check if one or multiple
                                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)){
                                    foreach(var unit in MOs){
                                        PlayerCharacter moPC = unit.GetComponent<PlayerCharacter>();
                                        if(moPC){
                                            if(AwaitingHarvestDictionary.ContainsKey(moPC.CharID)){
                                                if(AwaitingHarvestDictionary[moPC.CharID] != null){
                                                    StopCoroutine(AwaitingHarvestDictionary[moPC.CharID]);
                                                }
                                                AwaitingHarvestDictionary[moPC.CharID] = null;
                                                AwaitingHarvestDictionary.Remove(moPC.CharID);
                                            }
                                        }
                                    }
                                    for(int units = 0; units < MOs.Count; units++){
                                        if(MOs.Count == 1){
                                            Mob premobChecker = MOs[units].GetComponent<Mob>();
                                            if(premobChecker){
                                                mobOnly = true;
                                            }
                                        }
                                        if(MOs[units].Dying || MOs[units].Feared || MOs[units].Stunned || MOs[units].Mesmerized || mobOnly){
                                            PlayerCharacter pcChecker = MOs[units].GetComponent<PlayerCharacter>();
                                            Mob mobChecker = MOs[units].GetComponent<Mob>();
                                            string nameCheck = string.Empty;
                                            if(pcChecker){
                                                nameCheck = pcChecker.CharacterName;
                                            }
                                            if(mobChecker){
                                                nameCheck = mobChecker.NAME;
                                            }
                                            if(MOs[units].Dying){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} while dead");
                                            }
                                            if(MOs[units].Feared){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} while feared");
                                            }
                                            if(MOs[units].Stunned){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} while stunned");
                                            }
                                            if(MOs[units].Mesmerized){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} while unconscious");
                                            }
                                            if(mobOnly){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} without the owners command");
                                            }
                                            MOs.Remove(MOs[units]);
                                            
                                            //add name of player characrter or mob name and reason why it cant move
                                        }
                                    }
                                    if(MOs.Count == 0){
                                        //no one can move
                                        return;
                                    }
                                    CmdMoveUnits(MOs, targetPos, GetFormationMode());
                                } else {
                                    MovingObject selectedMember = CombatPartyView.instance.GetSelected();
                                    if(selectedMember){
                                        if(selectedMember.GetComponent<NetworkIdentity>().hasAuthority){
                                            if(selectedCharacters.Contains(selectedMember.gameObject)){
                                                //we can operate now
                                                List<MovingObject> singleList = new List<MovingObject>();
                                                singleList.Add(selectedMember);
                                                foreach(var unit in singleList){
                                                    PlayerCharacter moPC = unit.GetComponent<PlayerCharacter>();
                                                    if(moPC){
                                                        if(AwaitingHarvestDictionary.ContainsKey(moPC.CharID)){
                                                            if(AwaitingHarvestDictionary[moPC.CharID] != null){
                                                                StopCoroutine(AwaitingHarvestDictionary[moPC.CharID]);
                                                            }
                                                            AwaitingHarvestDictionary[moPC.CharID] = null;
                                                            AwaitingHarvestDictionary.Remove(moPC.CharID);
                                                        }
                                                    }
                                                }
                                                for(int unit = 0; unit < singleList.Count; unit++){
                                                    if(singleList.Count == 1){
                                                        Mob premobChecker = singleList[unit].GetComponent<Mob>();
                                                        if(premobChecker){
                                                            mobOnly = true;
                                                        }
                                                    }
                                                    if(singleList[unit].Dying || singleList[unit].Feared || singleList[unit].Stunned || singleList[unit].Mesmerized || mobOnly){
                                                        PlayerCharacter pcChecker = singleList[unit].GetComponent<PlayerCharacter>();
                                                        Mob mobChecker = singleList[unit].GetComponent<Mob>();
                                                        string nameCheck = string.Empty;
                                                        if(pcChecker){
                                                            nameCheck = pcChecker.CharacterName;
                                                        }
                                                        if(mobChecker){
                                                            nameCheck = mobChecker.NAME;
                                                        }
                                                        if(singleList[unit].Dying){
                                                            ImproperCheckText.Invoke($"Cannot move {nameCheck} while dead");
                                                        }
                                                        if(singleList[unit].Feared){
                                                            ImproperCheckText.Invoke($"Cannot move {nameCheck} while feared");
                                                        }
                                                        if(singleList[unit].Stunned){
                                                            ImproperCheckText.Invoke($"Cannot move {nameCheck} while stunned");
                                                        }
                                                        if(singleList[unit].Mesmerized){
                                                            ImproperCheckText.Invoke($"Cannot move {nameCheck} while unconscious");
                                                        }
                                                        if(mobOnly){
                                                            ImproperCheckText.Invoke($"Cannot move {nameCheck} without the owners command");
                                                        }
                                                        singleList.Remove(singleList[unit]);
                                                    }
                                                }
                                                if(singleList.Count == 0){
                                                    //no one can move
                                                    return;
                                                }
                                                CmdMoveUnits(singleList, targetPos, GetFormationMode());
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    } else {
                        MovingObject  selectedTarget = hit.collider.gameObject.GetComponent<MovingObject>();
                        if(!selectedTarget){
                            return;
                        }
                        if(selectedTarget.Dying){
                            return;
                        }
                        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)){
                            foreach(var selectedChar in selectedCharacters){
                                MovingObject pc = selectedChar.GetComponent<MovingObject>();
                                if(selectedChar.GetComponent<NetworkIdentity>().hasAuthority)
                                pc.CmdSetTarget(selectedTarget);
                                //MOs.Add(pc);
                            }
                        }
                        if(hit.collider.gameObject.tag == "Character"){
                                //check if one or multiple
                            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)){
                                foreach(var unit in MOs){
                                        PlayerCharacter moPC = unit.GetComponent<PlayerCharacter>();
                                        if(moPC){
                                            if(AwaitingHarvestDictionary.ContainsKey(moPC.CharID)){
                                                if(AwaitingHarvestDictionary[moPC.CharID] != null){
                                                    StopCoroutine(AwaitingHarvestDictionary[moPC.CharID]);
                                                }
                                                AwaitingHarvestDictionary[moPC.CharID] = null;
                                                AwaitingHarvestDictionary.Remove(moPC.CharID);
                                            }
                                        }
                                    }
                                    for(int units = 0; units < MOs.Count; units++){
                                        if(MOs.Count == 1){
                                            Mob premobChecker = MOs[units].GetComponent<Mob>();
                                            if(premobChecker){
                                                mobOnly = true;
                                            }
                                        }
                                        if(MOs[units].Dying || MOs[units].Feared || MOs[units].Stunned || MOs[units].Mesmerized || mobOnly){
                                            PlayerCharacter pcChecker = MOs[units].GetComponent<PlayerCharacter>();
                                            Mob mobChecker = MOs[units].GetComponent<Mob>();
                                            string nameCheck = string.Empty;
                                            if(pcChecker){
                                                nameCheck = pcChecker.CharacterName;
                                            }
                                            if(mobChecker){
                                                nameCheck = mobChecker.NAME;
                                            }
                                            if(MOs[units].Dying){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} while dead");
                                            }
                                            if(MOs[units].Feared){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} while feared");
                                            }
                                            if(MOs[units].Stunned){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} while stunned");
                                            }
                                            if(MOs[units].Mesmerized){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} while unconscious");
                                            }
                                            if(mobOnly){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} without the owners command");
                                            }
                                            MOs.Remove(MOs[units]);
                                            
                                            //add name of player characrter or mob name and reason why it cant move
                                        }
                                    }
                                    if(MOs.Count == 0){
                                        //no one can move
                                        return;
                                    }
                                    //attack or move to
                                    MovingObject hitMO = hit.collider.gameObject.GetComponent<MovingObject>();
                                    if(hitMO){
                                        if(FriendlyList.Contains(hitMO)){
                                            CmdMoveUnits(MOs, targetPos, GetFormationMode());//change to follow
                                        } else {
                                            CmdAttackUnit(MOs, hitMO);//change to autoattack
                                        }

                                    }
                            } else {
                                MovingObject selectedMember = CombatPartyView.instance.GetSelected();
                                if(selectedMember){
                                    if(selectedMember.GetComponent<NetworkIdentity>().hasAuthority){
                                        if(selectedCharacters.Contains(selectedMember.gameObject)){
                                            //we can operate now
                                            List<MovingObject> singleList = new List<MovingObject>();
                                            singleList.Add(selectedMember);
                                            foreach(var unit in singleList){
                                                    PlayerCharacter moPC = unit.GetComponent<PlayerCharacter>();
                                                    if(moPC){
                                                        if(AwaitingHarvestDictionary.ContainsKey(moPC.CharID)){
                                                            if(AwaitingHarvestDictionary[moPC.CharID] != null){
                                                                StopCoroutine(AwaitingHarvestDictionary[moPC.CharID]);
                                                            }
                                                            AwaitingHarvestDictionary[moPC.CharID] = null;
                                                            AwaitingHarvestDictionary.Remove(moPC.CharID);
                                                        }
                                                    }
                                                }
                                            selectedMember.CmdSetTarget(selectedTarget);
                                            for(int unit = 0; unit < singleList.Count; unit++){
                                                if(singleList.Count == 1){
                                                    Mob premobChecker = singleList[unit].GetComponent<Mob>();
                                                    if(premobChecker){
                                                        mobOnly = true;
                                                    }
                                                }
                                                if(singleList[unit].Dying || singleList[unit].Feared || singleList[unit].Stunned || singleList[unit].Mesmerized || mobOnly){
                                                    PlayerCharacter pcChecker = singleList[unit].GetComponent<PlayerCharacter>();
                                                    Mob mobChecker = singleList[unit].GetComponent<Mob>();
                                                    string nameCheck = string.Empty;
                                                    if(pcChecker){
                                                        nameCheck = pcChecker.CharacterName;
                                                    }
                                                    if(mobChecker){
                                                        nameCheck = mobChecker.NAME;
                                                    }
                                                    if(singleList[unit].Dying){
                                                        ImproperCheckText.Invoke($"Cannot move {nameCheck} while dead");
                                                    }
                                                    if(singleList[unit].Feared){
                                                        ImproperCheckText.Invoke($"Cannot move {nameCheck} while feared");
                                                    }
                                                    if(singleList[unit].Stunned){
                                                        ImproperCheckText.Invoke($"Cannot move {nameCheck} while stunned");
                                                    }
                                                    if(singleList[unit].Mesmerized){
                                                        ImproperCheckText.Invoke($"Cannot move {nameCheck} while unconscious");
                                                    }
                                                    if(mobOnly){
                                                        ImproperCheckText.Invoke($"Cannot move {nameCheck} without the owners command");
                                                    }
                                                    singleList.Remove(singleList[unit]);
                                                }
                                            }
                                            if(singleList.Count == 0){
                                                //no one can move
                                                return;
                                            }
                                            MovingObject hitMO = hit.collider.gameObject.GetComponent<MovingObject>();
                                            if(hitMO){
                                                if(FriendlyList.Contains(hitMO)){
                                                    CmdMoveUnits(singleList, targetPos, GetFormationMode());
                                                } else {
                                                    CmdAttackUnit(singleList, hitMO);//change to autoattack
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        } else if(hit.collider.gameObject.tag == "Enemy"){
                            SpriteRenderer sRend = hit.collider.gameObject.GetComponent<SpriteRenderer>();
                            if(sRend){
                                if(sRend.enabled){
                                    Green = false;
                                }
                            }
                            //check if one or multiple
                            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)){
                                foreach(var unit in MOs){
                                        PlayerCharacter moPC = unit.GetComponent<PlayerCharacter>();
                                        if(moPC){
                                            if(AwaitingHarvestDictionary.ContainsKey(moPC.CharID)){
                                                if(AwaitingHarvestDictionary[moPC.CharID] != null){
                                                    StopCoroutine(AwaitingHarvestDictionary[moPC.CharID]);
                                                }
                                                AwaitingHarvestDictionary[moPC.CharID] = null;
                                                AwaitingHarvestDictionary.Remove(moPC.CharID);
                                            }
                                        }
                                    }
                                    for(int units = 0; units < MOs.Count; units++){
                                        if(MOs.Count == 1){
                                            Mob premobChecker = MOs[units].GetComponent<Mob>();
                                            if(premobChecker){
                                                mobOnly = true;
                                            }
                                        }
                                        if(MOs[units].Dying || MOs[units].Feared || MOs[units].Stunned || MOs[units].Mesmerized || mobOnly){
                                            PlayerCharacter pcChecker = MOs[units].GetComponent<PlayerCharacter>();
                                            Mob mobChecker = MOs[units].GetComponent<Mob>();
                                            string nameCheck = string.Empty;
                                            if(pcChecker){
                                                nameCheck = pcChecker.CharacterName;
                                            }
                                            if(mobChecker){
                                                nameCheck = mobChecker.NAME;
                                            }
                                            if(MOs[units].Dying){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} while dead");
                                            }
                                            if(MOs[units].Feared){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} while feared");
                                            }
                                            if(MOs[units].Stunned){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} while stunned");
                                            }
                                            if(MOs[units].Mesmerized){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} while unconscious");
                                            }
                                            if(mobOnly){
                                                ImproperCheckText.Invoke($"Cannot move {nameCheck} without the owners command");
                                            }
                                            MOs.Remove(MOs[units]);
                                            
                                            //add name of player characrter or mob name and reason why it cant move
                                        }
                                    }
                                    if(MOs.Count == 0){
                                        //no one can move
                                        return;
                                    }
                                    if(selectedTarget){
                                        if(FriendlyList.Contains(selectedTarget)){
                                            CmdMoveUnits(MOs, targetPos, GetFormationMode());
                                        } else {
                                            CmdAttackUnit(MOs, selectedTarget);//change to autoattack
                                        }
                                    }
                                    
                            } else {
                                MovingObject selectedMember = CombatPartyView.instance.GetSelected();
                                if(selectedMember){
                                    if(selectedMember.GetComponent<NetworkIdentity>().hasAuthority){
                                        if(selectedCharacters.Contains(selectedMember.gameObject)){
                                            //we can operate now
                                            List<MovingObject> singleList = new List<MovingObject>();
                                            singleList.Add(selectedMember);
                                            selectedMember.CmdSetTarget(selectedTarget);
                                            foreach(var unit in singleList){
                                                    PlayerCharacter moPC = unit.GetComponent<PlayerCharacter>();
                                                    if(moPC){
                                                        if(AwaitingHarvestDictionary.ContainsKey(moPC.CharID)){
                                                            if(AwaitingHarvestDictionary[moPC.CharID] != null){
                                                                StopCoroutine(AwaitingHarvestDictionary[moPC.CharID]);
                                                            }
                                                            AwaitingHarvestDictionary[moPC.CharID] = null;
                                                            AwaitingHarvestDictionary.Remove(moPC.CharID);
                                                        }
                                                    }
                                                }
                                            for(int unit = 0; unit < singleList.Count; unit++){
                                                if(singleList.Count == 1){
                                                    Mob premobChecker = singleList[unit].GetComponent<Mob>();
                                                    if(premobChecker){
                                                        mobOnly = true;
                                                    }
                                                }
                                                if(singleList[unit].Dying || singleList[unit].Feared || singleList[unit].Stunned || singleList[unit].Mesmerized || mobOnly){
                                                    PlayerCharacter pcChecker = singleList[unit].GetComponent<PlayerCharacter>();
                                                    Mob mobChecker = singleList[unit].GetComponent<Mob>();
                                                    string nameCheck = string.Empty;
                                                    if(pcChecker){
                                                        nameCheck = pcChecker.CharacterName;
                                                    }
                                                    if(mobChecker){
                                                        nameCheck = mobChecker.NAME;
                                                    }
                                                    if(singleList[unit].Dying){
                                                        ImproperCheckText.Invoke($"Cannot move {nameCheck} while dead");
                                                    }
                                                    if(singleList[unit].Feared){
                                                        ImproperCheckText.Invoke($"Cannot move {nameCheck} while feared");
                                                    }
                                                    if(singleList[unit].Stunned){
                                                        ImproperCheckText.Invoke($"Cannot move {nameCheck} while stunned");
                                                    }
                                                    if(singleList[unit].Mesmerized){
                                                        ImproperCheckText.Invoke($"Cannot move {nameCheck} while unconscious");
                                                    }
                                                    if(mobOnly){
                                                        ImproperCheckText.Invoke($"Cannot move {nameCheck} without the owners command");
                                                    }
                                                    singleList.Remove(singleList[unit]);
                                                }
                                            }
                                            if(singleList.Count == 0){
                                                //no one can move
                                                return;
                                            }
                                            if(selectedTarget){
                                                if(FriendlyList.Contains(selectedTarget)){
                                                    CmdMoveUnits(singleList, targetPos, GetFormationMode());
                                                } else {
                                                    CmdAttackUnit(singleList, selectedTarget);//change to autoattack
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //selectedTarget.TargettedMO();
                        //CombatPartyView.instance.Retargetter(selectedTarget);
                    }
                }
                Vector3 target3D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                target3D.z += 10;
                //GameObject rightclick = Instantiate(rightClickPrefab, target3D, Quaternion.identity);
                GameObject rightclick = GetObject(target3D);
                RightClickAnimation RCA = rightclick.GetComponent<RightClickAnimation>();
                if(Green){
                    RCA.StartGreenSequence();
                } else {
                    RCA.StartRedSequence();
                }
                NewTarget.Invoke();
    }
    void LeftClickUp(){
        
        if(SavedClick != null){
            //print("We had a saved CLick on the up part of mouse");
            startPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    //print($"Set start: {startPoint}");

            isDragging = false;
            return;
        }
        Vector2 endPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                // If mouse movement is smaller than the threshold, consider it a click.
                if (Vector2.Distance(startPoint, endPoint) < clickThreshold)
                {
//                    print("thinks we werent dragging");
                    if(selectedCharacters.Count > 0){} else {
                        if(CheckForUIHIt()){
                          isDragging = false;
                          return;
                        }
                        RaycastHit2D MovingObjectCheck = Physics2D.Raycast(startPoint, Vector2.zero, 0f, LayerMask.GetMask("movingObjects"));
                        if(MovingObjectCheck.collider != null){
                            List<MovingObject> selected = new List<MovingObject>();
                            //if(MovingObjectCheck.collider.GetComponent<MovingObject>().Dying){
                            //    return;
                            //}
                            selected.Add(MovingObjectCheck.collider.GetComponent<MovingObject>());
                            if (MovingObjectCheck.collider.gameObject.tag == "Character"){// && MovingObjectCheck.collider.GetComponent<NetworkIdentity>().hasAuthority){
                                PlayerCharacter pc = MovingObjectCheck.collider.gameObject.GetComponent<PlayerCharacter>();
                                ClickedCharacter(pc);
                                if(pc.Target != null){
                                    SpriteRenderer sRend = pc.Target.GetComponent<SpriteRenderer>();
                                    //CombatPartyView.instance.TurnOnSelectedWindow(selected);
                                    //NewTarget.Invoke();
                                    if(!sRend.enabled){
                                        if(MovingObjectCheck.collider.GetComponent<NetworkIdentity>().hasAuthority){
                                            pc.CmdRemoveTarget();
                                        }
                                    } 
                                }
                                if(MovingObjectCheck.collider.GetComponent<NetworkIdentity>().hasAuthority){
                                    selectedCharacterHighlight.Invoke(pc.gameObject);
                                }
                                CombatPartyView.instance.TurnOnSelectedWindow(selected);
                               // NewTarget.Invoke();
                            }
                            if(MovingObjectCheck.collider.gameObject.tag == "Enemy"){
                                Mob mob = MovingObjectCheck.collider.gameObject.GetComponent<Mob>();
                                if(MovingObjectCheck.collider.GetComponent<NetworkIdentity>().hasAuthority){
                                    ClickedMobOwned(mob);
                                } else {
                                    ClickedMob(mob);
                                    CombatPartyView.instance.TurnOnSelectedWindow(selected);
                                }
                              //  NewTarget.Invoke();
                            }
                        }
                    }
                }
                else
                {
                    print($"triggered checkCharacterInSectionArea at start: {startPoint} and end: {endPoint}");
                    CheckCharactersInSelectionArea(startPoint, endPoint);
                    //print("Dragging ended");
                }
                isDragging = false;
    }
    /*
    if (EventSystem.current.IsPointerOverGameObject())
                {
                    PointerEventData pointerData = new PointerEventData(EventSystem.current)
                    {
                        position = Input.mousePosition
                    };
                    
                    // Create a list to store the objects the raycast hits
                    List<RaycastResult> results = new List<RaycastResult>();

                    // Perform the raycast using the pointer event data
                    EventSystem.current.RaycastAll(pointerData, results);
                    bool ditch = false;
                    bool castedSpell = false;
                    string spellCasted = string.Empty;
                    // Go through the list and check for the UI object by name
                    foreach (RaycastResult result in results)
                    {
                        ContainerUIButtons uiContainer = result.gameObject.GetComponent<ContainerUIButtons>();
                        if(uiContainer){
                            ditch = true;
                        }
                        Spell possibleSpell = result.gameObject.GetComponent<Spell>();
                        if(possibleSpell){
                            spellCasted = possibleSpell.GetSPELLSLOT();
                            castedSpell = true;
                        }
                        ChatPlayerUI chat = result.gameObject.GetComponent<ChatPlayerUI>();
                        if(chat){
                            ditch = true;
                        }
                        Castbar castBar = result.gameObject.GetComponent<Castbar>();
                        if(castBar){
                            ditch = true;
                        }
                        CastbarEnemy castBarenemy = result.gameObject.GetComponent<CastbarEnemy>();
                        if(castBarenemy){
                            ditch = true;
                        }
                        TutorialTip tips = result.gameObject.GetComponent<TutorialTip>();
                        if(tips){
                            ditch = true;
                        }
                        DragAndDropMenu dragMenu = result.gameObject.GetComponent<DragAndDropMenu>();
                        if(dragMenu){
                            ditch = true;
                        }
                        InventoryItem posInvItem = result.gameObject.GetComponent<InventoryItem>();
                        if(posInvItem){
                            ditch = true;
                        }
                        
                    }
                    if(ditch){
                        return;
                    }
                    if(castedSpell){
                        if(spellCasted == "Q"){
                            PressedQCombat();
                            return;
                        }
                        if(spellCasted == "E"){
                            PressedECombat();
                            return;
                        }
                        if(spellCasted == "R"){
                            PressedRCombat();
                            return;
                        }
                        if(spellCasted == "F"){
                            PressedFCombat();
                            return;
                        }
                        if(spellCasted == "1"){
                            PressedTactOneCombat();
                            return;
                        }
                        if(spellCasted == "2"){
                            PressedTactTwoCombat();
                            return;
                        }
                        if(spellCasted == "3"){
                            PressedPotionCombat();
                            return;
                        }
                        if(spellCasted == "4"){
                            PressedFoodCombat();
                            return;
                        }
                    }
                }
                */
    void LeftClickDown(){
        if(CheckForUIHIt()){
            return;
        }
        startPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition); // changed to Input.mousePosition
                    //print($"Set start: {startPoint}");

                //print("Dragging started");
                boxStart = Input.mousePosition;
                    //print($"Set boxStart: {boxStart}");

                if(selectedCharacters.Count > 0){
                    Vector2 _worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    RaycastHit2D _hitUI = Physics2D.Raycast(_worldPosition, Vector2.zero, 0f, LayerMask.GetMask("UI"));
                    bool _UIHit = false;
                    if (_hitUI.collider != null){
                        _UIHit = true;
                    }
                    if(_UIHit){
                        isDragging = false;
                        return;
                    }
                    RaycastHit2D MovingObjectCheck = Physics2D.Raycast(startPoint, Vector2.zero, 0f, LayerMask.GetMask("movingObjects"));
                    if(MovingObjectCheck.collider != null){
                        MovingObject selectedTarget = MovingObjectCheck.collider.gameObject.GetComponent<MovingObject>();
                        Mob mob = selectedTarget.GetComponent<Mob>();
                        if(mob){
                            ClickedTargetMob(mob);
                        } 
                        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)){
                            foreach(var selectedChar in selectedCharacters){
                                if(selectedChar.GetComponent<NetworkIdentity>().hasAuthority){
                                    MovingObject pc = selectedChar.GetComponent<MovingObject>();
                                    pc.CmdSetTarget(selectedTarget);
                                }
                            }
                        } else {
                            MovingObject selectedMember = CombatPartyView.instance.GetSelected();
                            if(selectedMember){
                                if(selectedMember.GetComponent<NetworkIdentity>().hasAuthority){
                                    if(selectedCharacters.Contains(selectedMember.gameObject)){
                                        //we can operate now
                                        selectedMember.CmdSetTarget(selectedTarget);
                                    }
                                }
                            }
                        }
                        
                        //foreach(var selectedChar in selectedCharacters){
                        //    MovingObject pc = selectedChar.GetComponent<MovingObject>();
                        //    pc.CmdSetTarget(selectedTarget);
                        //    MOs.Add(pc);
                        //}
                        
                        CombatPartyView.instance.Retargetter(selectedTarget);
                    }
                } else {
                    ClearTarget();
                }
                isDragging = true;
    }
    public void PressedQCombatClick(MovingObject selectedObject, bool Pet){
            if(selectedObject == null){
                ImproperCheckText.Invoke($"No target set");
                return;
            }
            if(!selectedObject.GetComponent<NetworkIdentity>().hasAuthority){
                return;
            }
            GameObject spellQGO = SpellManager.instance.GetSpellQ();
            if(Pet){
                spellQGO = SpellManager.instance.GetPETSpellQ();
            }
            if(spellQGO == null){
                return;
            }
            Spell spellq = spellQGO.GetComponent<Spell>();
            if(spellq == null){
                return;
            }
            if(selectedObject.Silenced){
                ImproperCheckText.Invoke($"Cannot cast while silenced");
                return;
            }
            //PlayerCharacter selectedPlayer = selectedObject.GetComponent<PlayerCharacter>();
            
            if(selectedObject.SpellQ == "None"){
                ImproperCheckText.Invoke($"Unit does not have a spell selected for that key");
                return;
            }
            
            if(selectedObject.SpellQCoolDown){
                ImproperCheckText.Invoke($"{selectedObject.SpellQ} is on cooldown");
            }
            if(selectedObject.stamina > 0){
                ImproperCheckText.Invoke($"Unit does not have enough stamina to cast {selectedObject.SpellQ}");
                return;
            }
            var nameMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellQ, @"^\D*");
            string spell = nameMatch.Value.Trim(); 
            int _spellRank = 1;
            // Extract spell rank
            var rankMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellQ, @"\d+$");
            if (rankMatch.Success) {
                _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
            }
            if(!selectedObject.shield && spell == "Bash" || !selectedObject.shield && spell == "Shield Bash"){
                ImproperCheckText.Invoke($"Must have a shield for that");
                return;
            }
            
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mouseWorldPosition2D = new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);

            if(selectedObject.cur_mp < StatAsset.Instance.GetSpellCost(spell)){
                ImproperCheckText.Invoke($"Unit does not have enough MP for {selectedObject.SpellQ}");
                return;
            }
            if(!spellq.GetCastable()){
                ImproperCheckText.Invoke($"Unit's spell {selectedObject.SpellQ} is on cooldown");
                return;
            }
            if(selectedObject.moving){
                //tell server to stop moving
                selectedObject.CmdStopMoving();
            }
            float selfCast = StatAsset.Instance.SelfCasted(spell);
            bool selfCasted = false;
            if(selfCast == 1){
                selfCasted = true;
            }
            if(StatAsset.Instance.SkillShot(spell) || selfCasted){
                CharacterCastingTargetSpell(selectedObject, null, CastingQ, mouseWorldPosition2D);
                return;
            }
            bool friendlySpell = StatAsset.Instance.DetermineFriendly(spell);
            //check if target required
            if(selectedObject.Target != null){
                if(spell == "Resurrect" && !selectedObject.Target.Dying){
                    ImproperCheckText.Invoke("Can only cast that on the dead");
                    return;
                }
                if(spell == "Purge" || spell == "Dispel"){
                    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingQ, mouseWorldPosition2D);
                    return;
                }
                if(selectedObject.GetFriendly(selectedObject.Target) && friendlySpell){
                    if(selectedObject.Target.Dying && spell != "Resurrect"){
                        ImproperCheckText.Invoke($"{selectedObject.SpellQ} cannot be cast on a dead target");
                        return;
                    }
                    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingQ, mouseWorldPosition2D);
                    return;
                } else if(selectedObject.GetFriendly(selectedObject.Target) && !friendlySpell){
                    ImproperCheckText.Invoke($"{selectedObject.SpellQ} cannot be cast on a friendly target");
                    return;
                }
                if(!selectedObject.GetFriendly(selectedObject.Target) && !friendlySpell){
                    if(selectedObject.Target.Dying){
                        ImproperCheckText.Invoke($"{selectedObject.SpellQ} cannot be cast on a dead target");
                        return;
                    }
                    if(spell == "Backstab"){
                        if(selectedObject.IsBehindAnotherObject(selectedObject.Target.transform.position, selectedObject.Target.GetFacingDirection())){
                            ImproperCheckText.Invoke($"We are behind target nice!");
                        } else {
                            ImproperCheckText.Invoke($"Must be behind target for that");
                            return;
                        }
                    }
                    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingQ, mouseWorldPosition2D);
                } else if(!selectedObject.GetFriendly(selectedObject.Target) && friendlySpell){
                    ImproperCheckText.Invoke($"{selectedObject.SpellQ} cannot be cast on a hostile target");
                    return;
                }
                //clean this up and make it so that we can see if we need to cast immediately or later
            } else {
                //try to cast aoe on the mouse position?
            }
    }
    public void PressedECombatClick(MovingObject selectedObject, bool Pet){
        //use combatpartywindow
            if(selectedObject == null){
                ImproperCheckText.Invoke($"No target set");
                return;
            }
            if(!selectedObject.GetComponent<NetworkIdentity>().hasAuthority){
                return;
            }
            GameObject spellEGO = SpellManager.instance.GetSpellE();
            if(Pet){
                spellEGO = SpellManager.instance.GetPETSpellE();
            }
            if(spellEGO == null){
                return;
            }
            Spell spellE = spellEGO.GetComponent<Spell>();
            if(spellE == null){
                return;
            }
            if(selectedObject.Silenced){
                ImproperCheckText.Invoke($"Cannot cast while silenced");
                return;
            }
            //PlayerCharacter selectedPlayer = selectedObject.GetComponent<PlayerCharacter>();
            
            if(selectedObject.SpellE == "None"){
                ImproperCheckText.Invoke($"Unit does not have a spell selected for that key");
                return;
            }
            if(selectedObject.SpellECoolDown){
                ImproperCheckText.Invoke($"{selectedObject.SpellE} is on cooldown");
            }
            if(selectedObject.stamina > 0)
            {
                ImproperCheckText.Invoke($"Unit does not have enough stamina to cast {selectedObject.SpellE}");
                return;
            }
            var nameMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellE, @"^\D*");
            string spell = nameMatch.Value.Trim(); 
            int _spellRank = 1;
            // Extract spell rank
            var rankMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellE, @"\d+$");
            if (rankMatch.Success) {
                _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
            }
            if(!selectedObject.shield && spell == "Bash" || !selectedObject.shield && spell == "Shield Bash"){
                ImproperCheckText.Invoke($"Must have a shield for that");
                return;
            }
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mouseWorldPosition2D = new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);

            if(selectedObject.cur_mp < StatAsset.Instance.GetSpellCost(spell)){
                ImproperCheckText.Invoke($"Unit does not have enough MP for {selectedObject.SpellE}");
                return;
            }
            if(!spellE.GetCastable()){
                ImproperCheckText.Invoke($"Unit's spell {selectedObject.SpellE} is on cooldown");
                return;
            }
            if(selectedObject.moving){
                //tell server to stop moving
                selectedObject.CmdStopMoving();
            }
            float selfCast = StatAsset.Instance.SelfCasted(spell);
            bool selfCasted = false;
            if(selfCast == 1){
                selfCasted = true;
            }
            if(StatAsset.Instance.SkillShot(spell) || selfCasted){
                CharacterCastingTargetSpell(selectedObject, null, CastingE, mouseWorldPosition2D);
                return;
            }
            bool friendlySpell = StatAsset.Instance.DetermineFriendly(spell);
            //check if target required
            if(selectedObject.Target != null){
                if(spell == "Resurrect" && !selectedObject.Target.Dying){
                    ImproperCheckText.Invoke("Can only cast that on the dead");
                    return;
                }
                if(spell == "Purge" || spell == "Dispel"){
                    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingE, mouseWorldPosition2D);
                    return;
                }
                if(selectedObject.GetFriendly(selectedObject.Target) && friendlySpell){
                    if(selectedObject.Target.Dying && spell != "Resurrect"){
                        ImproperCheckText.Invoke($"{selectedObject.SpellE} cannot be cast on a dead target");
                        return;
                    }
                    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingE, mouseWorldPosition2D);
                    return;
                } else if(selectedObject.GetFriendly(selectedObject.Target) && !friendlySpell){
                    ImproperCheckText.Invoke($"{selectedObject.SpellE} cannot be cast on a friendly target");
                    return;
                }
                
                if(!selectedObject.GetFriendly(selectedObject.Target) && !friendlySpell){
                    if(selectedObject.Target.Dying){
                        ImproperCheckText.Invoke($"{selectedObject.SpellE} cannot be cast on a dead target");
                        return;
                    }
                    if(spell == "Backstab"){
                        if(selectedObject.IsBehindAnotherObject(selectedObject.Target.transform.position, selectedObject.Target.GetFacingDirection())){
                            ImproperCheckText.Invoke($"We are behind target nice!");
                        } else {
                            ImproperCheckText.Invoke($"Must be behind target for that");
                            return;
                        }
                    }
                    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingE, mouseWorldPosition2D);
                } else if(!selectedObject.GetFriendly(selectedObject.Target) && friendlySpell){
                    ImproperCheckText.Invoke($"{selectedObject.SpellE} cannot be cast on a hostile target");
                    return;
                }
                //clean this up and make it so that we can see if we need to cast immediately or later
            } else {
                //try to cast aoe on the mouse position?
            }
    }
    public void PressedRCombatClick(MovingObject selectedObject, bool Pet){
            if(selectedObject == null){
                ImproperCheckText.Invoke($"No target set");
                return;
            }
            if(!selectedObject.GetComponent<NetworkIdentity>().hasAuthority){
                return;
            }
            GameObject spellRGO = SpellManager.instance.GetSpellR();
            if(Pet){
                spellRGO = SpellManager.instance.GetPETSpellR();
            }
            if(spellRGO == null){
                return;
            }
            
            Spell spellR = spellRGO.GetComponent<Spell>();
            if(spellR == null){
                return;
            }
            if(selectedObject.Silenced){
                ImproperCheckText.Invoke($"Cannot cast while silenced");
                return;
            }
            //PlayerCharacter selectedPlayer = selectedObject.GetComponent<PlayerCharacter>();
            
            if(selectedObject.SpellR == "None"){
                ImproperCheckText.Invoke($"Unit does not have a spell selected for that key");
                return;
            }
            if(selectedObject.SpellRCoolDown){
                ImproperCheckText.Invoke($"{selectedObject.SpellR} is on cooldown");
            }
            if(selectedObject.stamina > 0)
            {
                ImproperCheckText.Invoke($"Unit does not have enough stamina to cast {selectedObject.SpellR}");
                return;
            }
            var nameMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellR, @"^\D*");
            string spell = nameMatch.Value.Trim(); 
            int _spellRank = 1;
            // Extract spell rank
            var rankMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellR, @"\d+$");
            if (rankMatch.Success) {
                _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
            }
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mouseWorldPosition2D = new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);
            if(!selectedObject.shield && spell == "Bash" || !selectedObject.shield && spell == "Shield Bash"){
                ImproperCheckText.Invoke($"Must have a shield for that");
                return;
            }
            if(selectedObject.cur_mp < StatAsset.Instance.GetSpellCost(spell)){
                ImproperCheckText.Invoke($"Unit does not have enough MP for {selectedObject.SpellR}");
                return;
            }
            if(!spellR.GetCastable()){
                ImproperCheckText.Invoke($"Unit's spell {selectedObject.SpellR} is on cooldown");
                return;
            }
            if(selectedObject.moving){
                //tell server to stop moving
                selectedObject.CmdStopMoving();
            }
            float selfCast = StatAsset.Instance.SelfCasted(spell);
            bool selfCasted = false;
            if(selfCast == 1){
                selfCasted = true;
            }
            //var nameMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellR, @"^\D*");
            //string spellNameShaved = nameMatch.Value.Trim(); 
            //float targetable = SelfCasted(spell);
            //if(targetable != 0f){
            //    //cast the spell its a self target
            //    selectedObject.CmdSelfTarget(selectedObject.SpellR, CastingR);
            //    //possibly clear Mode to deselect character?
            //    return;
            //}
            if(StatAsset.Instance.SkillShot(spell) || selfCasted){
                CharacterCastingTargetSpell(selectedObject, null, CastingR, mouseWorldPosition2D);
                return;
            }
            //if(AnyTarget(spell)){
            //    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingR, mouseWorldPosition2D);
            //}
            bool friendlySpell = StatAsset.Instance.DetermineFriendly(spell);
            //check if target required
            if(selectedObject.Target != null){
                if(spell == "Resurrect" && !selectedObject.Target.Dying){
                    ImproperCheckText.Invoke("Can only cast that on the dead");
                    return;
                }
                if(spell == "Purge" || spell == "Dispel"){
                    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingR, mouseWorldPosition2D);
                    return;
                }
                if(selectedObject.GetFriendly(selectedObject.Target) && friendlySpell){
                    if(selectedObject.Target.Dying && spell != "Resurrect"){
                        ImproperCheckText.Invoke($"{selectedObject.SpellR} cannot be cast on a dead target");
                        return;
                    }
                    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingR, mouseWorldPosition2D);
                    return;
                } else if(selectedObject.GetFriendly(selectedObject.Target) && !friendlySpell){
                    ImproperCheckText.Invoke($"{selectedObject.SpellR} cannot be cast on a friendly target");
                    return;
                }
                
                if(!selectedObject.GetFriendly(selectedObject.Target) && !friendlySpell){
                    if(selectedObject.Target.Dying){
                        ImproperCheckText.Invoke($"{selectedObject.SpellR} cannot be cast on a dead target");
                        return;
                    }
                    if(spell == "Backstab"){
                        if(selectedObject.IsBehindAnotherObject(selectedObject.Target.transform.position, selectedObject.Target.GetFacingDirection())){
                            ImproperCheckText.Invoke($"We are behind target nice!");
                        } else {
                            ImproperCheckText.Invoke($"Must be behind target for that");
                            return;
                        }
                    }
                    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingR, mouseWorldPosition2D);
                } else if(!selectedObject.GetFriendly(selectedObject.Target) && friendlySpell){
                    ImproperCheckText.Invoke($"{selectedObject.SpellR} cannot be cast on a hostile target");
                    return;
                }
                //clean this up and make it so that we can see if we need to cast immediately or later
            } else {
                //try to cast aoe on the mouse position?
            }
    }
    public void PressedFCombatClick(MovingObject selectedObject, bool Pet){
            if(selectedObject == null){
                ImproperCheckText.Invoke($"No target set");
                return;
            }
            if(!selectedObject.GetComponent<NetworkIdentity>().hasAuthority){
                return;
            }
            GameObject spellFGO = SpellManager.instance.GetSpellF();
            if(Pet){
                spellFGO = SpellManager.instance.GetPETSpellF();
            }
            if(spellFGO == null){
                return;
            }
            Spell spellf = spellFGO.GetComponent<Spell>();
            if(spellf == null){
                return;
            }
            if(selectedObject.Silenced){
                ImproperCheckText.Invoke($"Cannot cast while silenced");
                return;
            }
            if(selectedObject.SpellF == "None"){
                ImproperCheckText.Invoke($"Unit does not have a spell selected for that key");
                return;
            }
            if(selectedObject.SpellFCoolDown){
                ImproperCheckText.Invoke($"{selectedObject.SpellF} is on cooldown");
            }
            if(selectedObject.stamina > 0)
            {
                ImproperCheckText.Invoke($"Unit does not have enough stamina to cast {selectedObject.SpellF}");
                return;
            }
            var nameMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellF, @"^\D*");
            string spell = nameMatch.Value.Trim(); 
            int _spellRank = 1;
            // Extract spell rank
            var rankMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellF, @"\d+$");
            if (rankMatch.Success) {
                _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
            }
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mouseWorldPosition2D = new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);
            if(!selectedObject.shield && spell == "Bash" || !selectedObject.shield && spell == "Shield Bash"){
                ImproperCheckText.Invoke($"Must have a shield for that");
                return;
            }
            if(selectedObject.cur_mp < StatAsset.Instance.GetSpellCost(spell)){
                ImproperCheckText.Invoke($"Unit does not have enough MP for {selectedObject.SpellF}");
                return;
            }
            if(!spellf.GetCastable()){
                ImproperCheckText.Invoke($"Unit's spell {selectedObject.SpellF} is on cooldown");
                return;
            }
            if(selectedObject.moving){
                //tell server to stop moving
                selectedObject.CmdStopMoving();
            }
            
            //var nameMatch = System.Text.RegularExpressions.Regex.Match(selectedObject.SpellF, @"^\D*");
            //string spellNameShaved = nameMatch.Value.Trim(); 
            //float targetable = SelfCasted(spell);
            //if(targetable != 0f){
            //    //cast the spell its a self target
            //    selectedObject.CmdSelfTarget(selectedObject.SpellF, CastingF);
            //    //possibly clear Mode to deselect character?
            //    return;
            //}
            float selfCast = StatAsset.Instance.SelfCasted(spell);
            bool selfCasted = false;
            if(selfCast == 1){
                selfCasted = true;
            }
            if(StatAsset.Instance.SkillShot(spell) || selfCasted){
                CharacterCastingTargetSpell(selectedObject, null, CastingF, mouseWorldPosition2D);
                return;
            }
            //if(AnyTarget(spell)){
            //    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingF, mouseWorldPosition2D);
            //}
            bool friendlySpell = StatAsset.Instance.DetermineFriendly(spell);
            //check if target required
            if(selectedObject.Target != null){
                if(spell == "Resurrect" && !selectedObject.Target.Dying){
                    ImproperCheckText.Invoke("Can only cast that on the dead");
                    return;
                }
                if(spell == "Purge" || spell == "Dispel"){
                    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingF, mouseWorldPosition2D);
                    return;
                }
                if(selectedObject.GetFriendly(selectedObject.Target) && friendlySpell){
                    if(selectedObject.Target.Dying && spell != "Resurrect"){
                        ImproperCheckText.Invoke($"{selectedObject.SpellF} cannot be cast on a dead target");
                        return;
                    }
                    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingF, mouseWorldPosition2D);
                    return;
                } else if(selectedObject.GetFriendly(selectedObject.Target) && !friendlySpell){
                    ImproperCheckText.Invoke($"{selectedObject.SpellF} cannot be cast on a friendly target");
                    return;
                }
                
                if(!selectedObject.GetFriendly(selectedObject.Target) && !friendlySpell){
                    if(selectedObject.Target.Dying){
                        ImproperCheckText.Invoke($"{selectedObject.SpellF} cannot be cast on a dead target");
                        return;
                    }
                    if(spell == "Backstab"){
                        if(selectedObject.IsBehindAnotherObject(selectedObject.Target.transform.position, selectedObject.Target.GetFacingDirection())){
                            ImproperCheckText.Invoke($"We are behind target nice!");
                        } else {
                            ImproperCheckText.Invoke($"Must be behind target for that");
                            return;
                        }
                    }
                    CharacterCastingTargetSpell(selectedObject, selectedObject.Target, CastingF, mouseWorldPosition2D);
                } else if(!selectedObject.GetFriendly(selectedObject.Target) && friendlySpell){
                    ImproperCheckText.Invoke($"{selectedObject.SpellF} cannot be cast on a hostile target");
                    return;
                }
                //clean this up and make it so that we can see if we need to cast immediately or later
            } else {
                //try to cast aoe on the mouse position?
            }
    }
    //public bool HasLineOfSight(Vector2 start, Vector2 end, PlayerCharacter pc)
    //{
    //    int x0 = Mathf.FloorToInt(start.x);
    //    int y0 = Mathf.FloorToInt(start.y);
    //    int x1 = Mathf.FloorToInt(end.x);
    //    int y1 = Mathf.FloorToInt(end.y);
    //    int dx = Mathf.Abs(x1 - x0);
    //    int dy = Mathf.Abs(y1 - y0);
    //    int sx = x0 < x1 ? 1 : -1;
    //    int sy = y0 < y1 ? 1 : -1;
    //    int err = dx - dy;
    //    while (true)
    //    {
    //        Vector2 current = new Vector2(x0 + .5f, y0 + .5f);
    //        RaycastHit2D floorCheck = Physics2D.Raycast(current, Vector2.zero, 0f, LayerMask.GetMask("Floor"));
    //        if(floorCheck.collider != null){
    //            RaycastHit2D obstructionCheck = Physics2D.Raycast(current, Vector2.zero, 0f, LayerMask.GetMask("blockingLayer"));
    //            if(obstructionCheck.collider != null){
    //                return false;
    //            }
    //        } else {
    //            return false;
    //        }
    //        if (x0 == x1 && y0 == y1){
    //            return true;
    //        }
    //        int e2 = 2 * err;
    //        if (e2 > -dy){
    //            err -= dy;
    //            x0 += sx;
    //        }
    //        if (e2 < dx){
    //            err += dx;
    //            y0 += sy;
    //        }
    //    }
    //}
    /*
    if (EventSystem.current.IsPointerOverGameObject())
                {
                    PointerEventData pointerData = new PointerEventData(EventSystem.current)
                    {
                        position = Input.mousePosition
                    };
                    
                    // Create a list to store the objects the raycast hits
                    List<RaycastResult> results = new List<RaycastResult>();

                    // Perform the raycast using the pointer event data
                    EventSystem.current.RaycastAll(pointerData, results);
                    bool ditch = false;
                    bool castedSpell = false;
                    string spellCasted = string.Empty;
                    // Go through the list and check for the UI object by name
                    foreach (RaycastResult result in results)
                    {
                        ContainerUIButtons uiContainer = result.gameObject.GetComponent<ContainerUIButtons>();
                        if(uiContainer){
                            ditch = true;
                        }
                        Spell possibleSpell = result.gameObject.GetComponent<Spell>();
                        if(possibleSpell){
                            spellCasted = possibleSpell.GetSPELLSLOT();
                            castedSpell = true;
                        }
                        ChatPlayerUI chat = result.gameObject.GetComponent<ChatPlayerUI>();
                        if(chat){
                            ditch = true;
                        }
                        Castbar castBar = result.gameObject.GetComponent<Castbar>();
                        if(castBar){
                            ditch = true;
                        }
                        CastbarEnemy castBarenemy = result.gameObject.GetComponent<CastbarEnemy>();
                        if(castBarenemy){
                            ditch = true;
                        }
                        TutorialTip tips = result.gameObject.GetComponent<TutorialTip>();
                        if(tips){
                            ditch = true;
                        }
                        DragAndDropMenu dragMenu = result.gameObject.GetComponent<DragAndDropMenu>();
                        if(dragMenu){
                            ditch = true;
                        }
                        InventoryItem posInvItem = result.gameObject.GetComponent<InventoryItem>();
                        if(posInvItem){
                            ditch = true;
                        }
                    }
                    if(ditch){
                        return;
                    }
                    if(castedSpell){
                        if(spellCasted == "Q"){
                            PressedQCombat();
                            return;
                        }
                        if(spellCasted == "E"){
                            PressedECombat();
                            return;
                        }
                        if(spellCasted == "R"){
                            PressedRCombat();
                            return;
                        }
                        if(spellCasted == "F"){
                            PressedFCombat();
                            return;
                        }
                        if(spellCasted == "1"){
                            PressedTactOneCombat();
                            return;
                        }
                        if(spellCasted == "2"){
                            PressedTactTwoCombat();
                            return;
                        }
                        if(spellCasted == "3"){
                            PressedPotionCombat();
                            return;
                        }
                        if(spellCasted == "4"){
                            PressedFoodCombat();
                            return;
                        }
                    }
                }
                */
}
}

