using System.Runtime.InteropServices;
using System.IO;
using System.Security.Cryptography;
using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEngine.Events;
using UnityEngine.Tilemaps;

namespace dragon.mirror
{
    [System.Serializable]
    public class PlayerSlotPair
    {
        public ScenePlayer player;
        public string slot;
        public PlayerSlotPair() { }
        public PlayerSlotPair(ScenePlayer player, string slot)
        {
            this.player = player;
            this.slot = slot;
        }
    }
    [System.Serializable]
    public class Match {
        public string matchID;
        public string matchNode;
        public bool publicMatch;
        public int matchSerial;
        public ScenePlayer matchLeader;
        public void SetMatchLeader(ScenePlayer leader){
            matchLeader = leader;
        }
        public ScenePlayer GetMatchLeader(){
            return matchLeader;
        }
        public void AddPlayerSlotPair(PlayerSlotPair newSlot){
            Debug.Log($"adding player slot {newSlot.player} player and {newSlot.slot} slot");
            if(!playerSlotPairs.Contains(newSlot)){
                playerSlotPairs.Add(newSlot);
            }
            //UpdatePlayerSlots();
        }
        public void RemovePlayerSlotPair(PlayerSlotPair removingSlot){
            Debug.Log($"removing player slot {removingSlot.player} player and {removingSlot.slot} slot");

            if(playerSlotPairs.Contains(removingSlot)){
                playerSlotPairs.Remove(removingSlot);
            }
            //UpdatePlayerSlots();
        }
        void UpdatePlayerSlots(){
            foreach(var player in players){
                player.currentMatch = this;
            }
        }
        public List<ScenePlayer> players = new List<ScenePlayer> ();
        public List<PlayerSlotPair> playerSlotPairs = new List<PlayerSlotPair>();
        public void AddPlayerAfterCreate(ScenePlayer playerAdding){
            if(!players.Contains(playerAdding)){
                players.Add(playerAdding);
            }
        }
        public Match(string matchID, ScenePlayer player) {
            this.matchID = matchID;
            players.Add (player);
        }
        public Match () { }
    }
public class MatchMaker : NetworkBehaviour
{
    [SerializeField] Tilemap BiomeFloor;
    public TileBase desertTile; // Drag your desert tile here
    public TileBase forestTile; // Drag your forest tile here
    public TileBase tundraTile; // Drag your forest tile here
    public TileBase bridgeTile; // Drag your forest tile here
    public TileBase swampTile; // Drag your forest tile here
    public TileBase caveTile; // Drag your forest tile here
    public TileBase darkForestTile; // Drag your forest tile here
    public TileBase waterTile; // Drag your forest tile here
    [SerializeField] public static UnityEvent<Match, List<ScenePlayer>>  FullWipe = new UnityEvent<Match, List<ScenePlayer>>();
    public static MatchMaker instance;
    public static UnityEvent<string, int, Match> CLEARTHEMATCH = new UnityEvent<string, int, Match>();
    public static UnityEvent<PlayerCharacter, ScenePlayer, Match, string> MakeCharacters = new UnityEvent<PlayerCharacter, ScenePlayer, Match, string>();
    public static UnityEvent<ChatManagerNode> MoveChatNodeOVM = new UnityEvent<ChatManagerNode>();
    public static UnityEvent<ChatManagerNode> MoveChatNodeTOWNOFARUDINE = new UnityEvent<ChatManagerNode>();
    //public static UnityEvent<Match, TurnManager, PlayerCharacter> SendCuratorTMToPlayer = new UnityEvent<Match, TurnManager, PlayerCharacter>();
    [SerializeField] public static UnityEvent<MovingObject>  Energize = new UnityEvent<MovingObject>();
    public static UnityEvent<TurnManager, int, Match> moveTurnManager = new UnityEvent<TurnManager, int, Match>();
    public static UnityEvent<ChatManagerNode, int, Match> moveChatManagerNode = new UnityEvent<ChatManagerNode, int, Match>();
    public static UnityEvent<Mob, Match> moveMob = new UnityEvent<Mob, Match>();
    public static UnityEvent<ScenePlayer, float> EnterNodeCost = new UnityEvent<ScenePlayer, float>();
    public readonly SyncList<Match> matches = new SyncList<Match>();
    public readonly SyncList<String> matchIDs = new SyncList<String>();
    Dictionary<ChatManagerNode, Match> ChatManagerNodesDictionary = new Dictionary<ChatManagerNode, Match>();
    //ChatManagers
    [SerializeField] GameObject ChatNodePrefab;
    //Characters
    [SerializeField] GameObject turnManagerPrefab;

[System.Serializable]
public class BiomeTierScenes
{
    public string biomeName;
    [Scene] public List<string> tier1Scenes = new List<string>();
    [Scene] public List<string> tier2Scenes = new List<string>();
    [Scene] public List<string> tier3Scenes = new List<string>();
    [Scene] public List<string> tier4Scenes = new List<string>();
}

public List<BiomeTierScenes> biomeScenes = new List<BiomeTierScenes>();
private Dictionary<TileBase, string> tileToBiomeMapping;
    public string RandomNodeSelector(Vector3 playerPosition){
        Vector3 NewPlayerPosition = new Vector3(playerPosition.x, playerPosition.y, 0);
        Vector3Int cellPosition = BiomeFloor.WorldToCell(NewPlayerPosition);
Debug.Log($"Player Position: {NewPlayerPosition}");
    Debug.Log($"Calculated Cell Position: {cellPosition}");
        // Get the tile at the cell position.
        TileBase tile = BiomeFloor.GetTile(cellPosition);
        // Debug the tile type
    if (tile != null)
        Debug.Log($"Tile Type: {tile.name}");
    else
        Debug.Log("Tile is null!");
        // Determine the tile type and set the biome name.
        string biomeName = "";
        if (tile == desertTile)
            biomeName = "Desert";
        else if (tile == forestTile)
            biomeName = "Forest";
        else if (tile == tundraTile)
            biomeName = "Tundra";
        else if (tile == bridgeTile)
            biomeName = "Bridge";
        else if (tile == swampTile)
            biomeName = "Swamp";
        else if (tile == caveTile)
            biomeName = "Cave";
        else if (tile == waterTile)
            biomeName = "Water";
        else if (tile == darkForestTile)
            biomeName = "Forest";

            BiomeTierScenes selectedBiome = biomeScenes.Find(b => b.biomeName == biomeName);
        if (selectedBiome != null && selectedBiome.tier1Scenes.Count > 0)
    {
        // Select a random scene from the tier1 list. Modify this for other tiers as needed.
        int randomIndex = UnityEngine.Random.Range(0, selectedBiome.tier1Scenes.Count);
        string fullPath = selectedBiome.tier1Scenes[randomIndex];
        string sceneName = Path.GetFileNameWithoutExtension(fullPath);
        return sceneName;

        //return selectedBiome.tier1Scenes[randomIndex];
    }
    else
    {
        Debug.LogWarning("No scenes found for the given biome or biome not defined.");
        return "";  // Return an empty string or some default value.
    }
        //float cellWidth = 60.2f;
        //float cellHeight = 49f;
        //int randomSelected = 1;
        //int col = Mathf.FloorToInt((playerPosition.x + 180.5f) / cellWidth);
        //int row = Mathf.FloorToInt((playerPosition.y + 99.5f) / cellHeight);
        ////int[,] tiers = {
        ////    { 4, 3, 4, 3, 4 },
        ////    { 3, 2, 1, 2, 3 },
        ////    { 3, 3, 2, 3, 2 },
        ////    { 4, 2, 3, 3, 4 }
        ////};
        ////biomeName = biomeName + "_" + tiers[row, col].ToString();
        //return biomeName + "_" + randomSelected;
    }
    public int DetermineTier(float x, float y)
{
    float cellWidth = 60.2f;
    float cellHeight = 49f;

    int col = Mathf.FloorToInt((x + 180.5f) / cellWidth);
    int row = Mathf.FloorToInt((y + 99.5f) / cellHeight);

    int[,] tiers = {
        { 4, 3, 4, 3, 4 },
        { 3, 2, 1, 2, 3 },
        { 3, 3, 2, 3, 2 },
        { 4, 2, 3, 3, 4 }
    };

    return tiers[row, col];
}
    void Start()
    {
        instance = this;
    }
    public bool HostGame(string _matchID, ScenePlayer _player, bool publicMatch, out int playerIndex){
        playerIndex = -1;
        if (!matchIDs.Contains(_matchID)) {
            matchIDs.Add (_matchID);
            // Lets build the ChatNode
            GameObject newChatNode = Instantiate (ChatNodePrefab);
            NetworkServer.Spawn(newChatNode);
            print("Spawned new chat node");
            //we need to give chatNode a link to us with the match thats how we will control it maybe?
            Match match = new Match (_matchID, _player);
            match.publicMatch = publicMatch;
            match.matchNode = _player.currentNode;
            match.matchSerial = matches.Count;
            
            match.SetMatchLeader(_player);
            matches.Add (match);
            _player.inLobby = true;
            _player.inMatch = false; 
            _player.currentMatch = match;
            _player.matchLeader = true;
            ChatManagerNode chatManager = newChatNode.GetComponent<ChatManagerNode>();
            chatManager.SetMatchSerial(match.matchSerial, match);
            chatManager.AddPlayer(_player);
            MoveChatNodeOVM.Invoke(chatManager);
            ChatManagerNodesDictionary.Add(chatManager, match);
            playerIndex = 1;
            Debug.Log($"Match generated");
            return true;
        } else {
            Debug.Log($"Match ID already exists");
            return false;
        }
    }
    public bool HostSoloGame(string _matchID, ScenePlayer _player, bool publicMatch, out int playerIndex){
        playerIndex = -1;
        if (!matchIDs.Contains(_matchID)) {
            matchIDs.Add (_matchID);
            Match match = new Match (_matchID, _player);
            match.publicMatch = publicMatch;
            match.matchNode = _player.currentNode;
            match.matchSerial = matches.Count;
            match.SetMatchLeader(_player);
            _player.matchLeader = true;
            _player.inLobby = false;
            _player.inMatch = true;
            _player.currentMatch = match;
            playerIndex = 1;
            Debug.Log($"Match generated");
            foreach(var character in _player.GetParty()){
                
                PlayerSlotPair newPair = new PlayerSlotPair(_player, character);
                //match.playerSlotPairs.Add(newPair);
                match.AddPlayerSlotPair(newPair);
            }
            matches.Add (match);
            CreateSoloGame(_player, _matchID, match);
            return true;
        } else {
            Debug.Log($"Match ID already exists");
            return false;
        }
    }
    public bool HostArenaDuel(string _matchID, ScenePlayer playerOne, ScenePlayer playerTwo, bool publicMatch){
        if (!matchIDs.Contains(_matchID)) {
            matchIDs.Add (_matchID);
            // Lets build the ChatNode
            GameObject newChatNode = Instantiate (ChatNodePrefab);
            NetworkServer.Spawn(newChatNode);
            print("Spawned new chat node");
            //we need to give chatNode a link to us with the match thats how we will control it maybe?
            Match match = new Match (_matchID, playerOne);
            match.AddPlayerAfterCreate(playerTwo);
            match.publicMatch = publicMatch;
            match.matchNode = "ArenaDuelFriendlyMode";
            match.matchSerial = matches.Count;
            
            match.SetMatchLeader(playerOne);
            playerOne.inMatch = true; 
            playerOne.currentMatch = match;
            playerOne.matchLeader = true;
            playerTwo.inMatch = true; 
            playerTwo.currentMatch = match;
            ChatManagerNode chatManager = newChatNode.GetComponent<ChatManagerNode>();
            chatManager.SetMatchSerial(match.matchSerial, match);
            chatManager.AddPlayer(playerOne);
            chatManager.AddPlayer(playerTwo);
            foreach(var character in playerOne.GetParty()){
                
                PlayerSlotPair newPair = new PlayerSlotPair(playerOne, character);
                //match.playerSlotPairs.Add(newPair);
                match.AddPlayerSlotPair(newPair);
            }
            foreach(var character in playerTwo.GetParty()){
                
                PlayerSlotPair newPair = new PlayerSlotPair(playerTwo, character);
                //match.playerSlotPairs.Add(newPair);
                match.AddPlayerSlotPair(newPair);
            }
            matches.Add (match);
            MoveChatNodeTOWNOFARUDINE.Invoke(chatManager);
            ChatManagerNodesDictionary.Add(chatManager, match);
            Debug.Log($"Match generated");
            return true;
        } else {
            Debug.Log($"Match ID already exists");
            return false;
        }
    }
    public void CreateSoloGame(ScenePlayer host, string _matchID, Match currentMatch){
        #if UNITY_SERVER
        PlayFabServer.instance.StartTheGameSolo(host, matches.Count, host.currentNode, currentMatch, false, false); 
        #endif
        //StartCoroutine(SetUpSoloMatch(host, _matchID, currentMatch));
    }
    public void CreateDuelMatch(string _matchID, Match currentMatch, ScenePlayer playerOne, ScenePlayer playerTwo){
        #if UNITY_SERVER
        ArenaDuel arenaDuelScript = new ArenaDuel(playerOne, playerTwo);
        PlayFabServer.instance.PVPDuelMatch(matches.Count,currentMatch, _matchID, arenaDuelScript); 
        #endif
        //StartCoroutine(SetUpSoloMatch(host, _matchID, currentMatch));
    }
    public bool RandomMatch(string _matchID, ScenePlayer _player, bool publicMatch, out int playerIndex, string random, bool login){
        playerIndex = -1;
        if (!matchIDs.Contains(_matchID)) {
            matchIDs.Add (_matchID);
            Match match = new Match (_matchID, _player);
            match.publicMatch = publicMatch;
           // match.matchNode = _player.currentNode;
            match.matchSerial = matches.Count;
            _player.inLobby = false;
            _player.inMatch = true;
            _player.currentMatch = match;
            playerIndex = 1;
            Debug.Log($"Match generated");
            foreach(var character in _player.GetParty()){
                
                PlayerSlotPair newPair = new PlayerSlotPair(_player, character);
                //match.playerSlotPairs.Add(newPair);
                match.AddPlayerSlotPair(newPair);
            }
            matches.Add (match);
            CreateRandomMatch(_player, _matchID, match, random, login);
            return true;
        } else {
            Debug.Log($"Match ID already exists");
            return false;
        }
    }
    public bool SewersInstance(string _matchID, ScenePlayer _player, bool publicMatch, out int playerIndex, bool login, int sewerFloor){
        string sewerMap = "Sewers level 1 story";
        if(sewerFloor == 2){
            sewerMap = "Sewers level 2 story";
        }
        if(sewerFloor == 3){
            sewerMap = "Sewers level 3 story";
        }
        if(sewerFloor == 4){
            sewerMap = "Sewers level 4 story";
        }
        playerIndex = -1;
        if (!matchIDs.Contains(_matchID)) {
            matchIDs.Add (_matchID);
            Match match = new Match (_matchID, _player);
            match.publicMatch = publicMatch;
           // match.matchNode = _player.currentNode;
            match.matchSerial = matches.Count;
            match.matchLeader = _player;
            _player.matchLeader = true;
            _player.inLobby = false;
            _player.inMatch = true;
            _player.currentMatch = match;
            playerIndex = 1;
            Debug.Log($"Match generated");
            foreach(var character in _player.GetParty()){
                
                PlayerSlotPair newPair = new PlayerSlotPair(_player, character);
                //match.playerSlotPairs.Add(newPair);
                match.AddPlayerSlotPair(newPair);
            }
            matches.Add (match);
            #if UNITY_SERVER
            PlayFabServer.instance.StartTheGameSolo(_player, matches.Count, sewerMap, match, false, login); 
            #endif
            return true;
        } else {
            Debug.Log($"Match ID already exists");
            return false;
        }
    }
    public void CreateRandomMatch(ScenePlayer host, string _matchID, Match currentMatch, string random, bool login){
        #if UNITY_SERVER
        PlayFabServer.instance.StartTheGameSolo(host, matches.Count, random, currentMatch, true, login); 
        #endif
        //StartCoroutine(SetUpSoloMatch(host, _matchID, currentMatch));
    }
    public bool JoinGame(string _matchID, ScenePlayer _player, out int playerIndex, out ScenePlayer Host){
        playerIndex = -1;
        //ChatManagerNode chatNode = ChatManagerNodesDictionary
        int matchNumber = -1;
        Match match = null;
        Match matchRef = null;

        bool changedMatch = false;
        if(matchIDs.Contains(_matchID)) {
            for (int i = 0; i < matches.Count; i++){
                if (matches[i].matchID == _matchID && matches[i].matchNode == _player.currentNode){
                    matchNumber = i;
                    matchRef = matches[i];
                    match = matches[i];
                    match.players.Add(_player);
                    //matches[i].matchLeader.currentMatch.players.Add(_player);
                    _player.inLobby = true;
                    _player.currentMatch = match;
                    playerIndex = match.players.Count;
                    ChatManagerNode targetChatManagerNode = null;
                    foreach (KeyValuePair<ChatManagerNode, Match> entry in ChatManagerNodesDictionary)
                    {
                        if (entry.Value.Equals(match))
                        {
                            targetChatManagerNode = entry.Key;
                            break;
                        }
                    }
                    if (targetChatManagerNode != null)
                    {
                        targetChatManagerNode.AddPlayer(_player);
                        // Found the ChatManagerNode associated with the targetMatch
                    }
                    else
                    {
                        // ChatManagerNode not found for the targetMatch
                    }
                    StartCoroutine(SplitPacketForClear(match));
                    foreach(var splayer in match.players){
                        //if(splayer == match.matchLeader){
                        //    continue;
                        //}
                        if(splayer.matchLeader){
                            continue;
                        }
                        splayer.currentMatch = match;
                        splayer.currentMatch.playerSlotPairs.Clear();
                    }
                    changedMatch = true;
                    break;
                }
            }
            if(changedMatch){
                if(matches.Contains(match)){
                    matches.Remove(matchRef);
                }
            }
            matches.Add(match);
            //for (int x = 0; x < matches.Count; x++){
            //    if (matches[x].matchID == _matchID){
            //        matches[x].playerSlotPairs.Clear();// Clear the playerSlotPairs list here                    // updates all the players to include the new player
            //        foreach(var sPlayer in matches[x].players){
            //            sPlayer.currentMatch = matches[x];
            //        }
            //        StartCoroutine(SplitPacketForClear(matches[x]));
            //        break;
            //    }
            //}
            ScenePlayer _Host = null;
            Host = _Host;
            foreach(var _matchPlayer in matches[matchNumber].players)
            {
                if(_matchPlayer.matchLeader){
                    Host = _matchPlayer;
                }
            }            
            //Host = matches[matchNumber].matchLeader;
            Debug.Log($"Match joined");
            return true;
        } else {
            Debug.Log($"Match ID does not exist");
            Host = null;
            return false;
        }
    }
    IEnumerator SplitPacketForClear(Match match){
        yield return new WaitForEndOfFrame();
        foreach(var partyMember in match.players){
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
                        break;
                    }
                }
            }
            ClientPartyInformation partyList = (new ClientPartyInformation
            {
                Party = savedParty,
                owner = partyMember
            });
            partyMember.currentMatch = match;
            partyMember.RpcBuildPartyInspector(partyList, match);
            partyMember.RpcSpawnPlayerUI(partyMember.loadSprite, match);
        }
    }
    public bool SearchGame(ScenePlayer _player, out int playerIndex, out string matchID){
        playerIndex = -1;
        matchID = string.Empty;
        //bool partyLead = false;
        for (int i = 0; i < matches.Count; i++) {
            if (matches[i].players.Count < 6 && matches[i].matchNode == _player.currentNode && matches[i].GetMatchLeader().inLobby){
                matchID = matches[i].matchID;
                if(JoinGame(matchID, _player, out playerIndex, out ScenePlayer Host)) {
                    return true;
                } 
            }
        }
        return false;
    }
    [Server]
    public void RemoveCharacterVote(ScenePlayer sPlayer, string characterSlot, Match match){
        for (int i = 0; i < matches.Count; i++){
            if (matches[i].matchID == match.matchID && matches[i].matchNode == sPlayer.currentNode){
                PlayerSlotPair pairToRemove = matches[i].matchLeader.currentMatch.playerSlotPairs.FirstOrDefault(pair => pair.player == sPlayer && pair.slot == characterSlot);
                // If pairToRemove is not null, remove the pair and notify the players
                if(pairToRemove != null){
                    // Remove the pair
                    matches[i].matchLeader.currentMatch.playerSlotPairs.Remove(pairToRemove);
                    // Send updates to each player
                    foreach(var player in match.players){
                        player.TargetRemoveAdventurer(sPlayer, characterSlot);
                        if(player != matches[i].matchLeader){
                            player.currentMatch = matches[i].matchLeader.currentMatch;  // Manually update currentMatch to trigger a sync
                        }
                    }
                }
            }
        }
        /*
        foreach(var pairkey in match.playerSlotPairs){
            if(pairkey.player == sPlayer && pairkey.slot == characterSlot){
                foreach(var player in match.players){
                    player.TargetRemoveAdventurer(sPlayer, characterSlot);
                }
                //match.playerSlotPairs.Remove(pairkey);
                match.RemovePlayerSlotPair(pairkey);

            }
        }
        PlayerSlotPair pairToRemove = match.playerSlotPairs.FirstOrDefault(pair => pair.player == sPlayer && pair.slot == characterSlot);
        // If pairToRemove is not null, remove the pair and notify the players
        if(pairToRemove != null){
            // Remove the pair
            match.RemovePlayerSlotPair(pairToRemove);
            // Send updates to each player
            foreach(var player in match.players){
                player.TargetRemoveAdventurer(sPlayer, characterSlot);
                player.currentMatch = match;  // Manually update currentMatch to trigger a sync
            }
        }
        */

    }
    public int GetNodeCharacterAllowancePerPlayer(string nodeName){
        int nodeTier = 1;
        //tier 1
        if(nodeName == "Random_Forest_1_1"){
            nodeTier = 1;
        }
        if(nodeName == "Random_Forest_1_2"){
            nodeTier = 1;
        }
        if(nodeName == "Random_Forest_1_3"){
            nodeTier = 1;
        }
        if(nodeName == "Lake Arudine"){
            nodeTier = 1;
        }
        if(nodeName == "Spider Caverns"){
            nodeTier = 1;
        }
        if(nodeName == "Sequoia Forest"){
            nodeTier = 1;
        }
        //tier 2
        if(nodeName == "Spider Caverns South" || nodeName == "Spider Caverns North"){
            nodeTier = 2;
        }
        //tier 3 is 2 max
        //tier 4 is 1 max
        //tier 5 is 1 max
        //tier 6 is 1 max and is the raid tier for 12 chars
        //tier 7 is 1 max and is the raid tier for 24 chars
        //tier 8 is 1 max and is the raid tier for 48 chars

        return nodeTier;
    }
    [Server]
    public void CastCharacterVote(ScenePlayer sPlayer, string characterSlot, Match match, string charName, string spriteName)
    {
        print("MADE IT TO CastCharacterVote 1");
        for (int i = 0; i < matches.Count; i++){
            print("MADE IT TO CastCharacterVote 2");

            if (matches[i].matchID == match.matchID && matches[i].matchNode == sPlayer.currentNode){
                print("MADE IT TO CastCharacterVote 3");
                int index = sPlayer.playerIndex;
                int playerCount = match.players.Count;
                int activeCharCount = 0;
                int nodeTier = GetNodeCharacterAllowancePerPlayer(sPlayer.currentNode);
                int allowedCharAmount = 6;
                bool raid = false;
                int maxAllowedRaidAmount = 12;
                if(nodeTier == 2){
                    allowedCharAmount = 3;
                }
                if(nodeTier == 3){
                    allowedCharAmount = 2;
                }
                if(nodeTier == 4){
                    allowedCharAmount = 1;
                }
                if(nodeTier == 5){
                    allowedCharAmount = 1;
                }
                if(nodeTier >= 6){
                    if(nodeTier == 7){
                        maxAllowedRaidAmount = 24;
                    }
                    if(nodeTier == 8){
                        maxAllowedRaidAmount = 48;
                    }
                    raid = true;
                    allowedCharAmount = 1;
                }
                if(!raid && matches[i].matchLeader.currentMatch.playerSlotPairs.Count == 6){
                    print("MADE IT TO CastCharacterVote but we have a full party! remove a character first");
                    return;
                }
                if(raid && matches[i].matchLeader.currentMatch.playerSlotPairs.Count == maxAllowedRaidAmount){
                    print("MADE IT TO CastCharacterVote but we have a full raid! remove a character first");
                    return;
                }
                foreach (PlayerSlotPair pair in matches[i].matchLeader.currentMatch.playerSlotPairs)
                {
                    if (pair.player == sPlayer)
                    {
                        print("MADE IT TO CastCharacterVote 4");
                        activeCharCount++;
                        // Check if the characterSlot already exists for this sPlayer
                        if (pair.slot == characterSlot)
                        {
                            print("MADE IT TO CastCharacterVote bad area");
                            return;
                        }
                    }
                }
                if(!raid){
                    if(nodeTier == 1){
                        if (playerCount == 1)
                        {
                            allowedCharAmount = 6;
                        }
                        else if (playerCount == 2)
                        {
                            allowedCharAmount = 3;
                        }
                        else if (playerCount == 3)
                        {
                            allowedCharAmount = 2;
                        }
                        else if (playerCount == 4)
                        {
                            if (index == 1 || index == 2)
                            {
                                allowedCharAmount = 2;
                            }
                            else
                            {
                                allowedCharAmount = 1;
                            }
                        }
                        else if (playerCount == 5)
                        {
                            if (index == 1)
                            {
                                allowedCharAmount = 2;
                            }
                            else
                            {
                                allowedCharAmount = 1;
                            }
                        }
                        else if (playerCount == 6)
                        {
                            allowedCharAmount = 1;
                        }
                    }
                }
                print($"{playerCount} is our player count and {allowedCharAmount} is our allowed char amount with an index of {index}");
                /*
                if (playerCount == 1)
                {
                    allowedCharAmount = 6;
                }
                else if (playerCount == 2)
                {
                    allowedCharAmount = 3;
                }
                else if (playerCount == 3)
                {
                    allowedCharAmount = 2;
                }
                else if (playerCount == 4)
                {
                    if (index == 1 || index == 2)
                    {
                        allowedCharAmount = 2;
                    }
                    else
                    {
                        allowedCharAmount = 1;
                    }
                }
                else if (playerCount == 5)
                {
                    if (index == 1)
                    {
                        allowedCharAmount = 2;
                    }
                    else
                    {
                        allowedCharAmount = 1;
                    }
                }
                else if (playerCount == 6)
                {
                    allowedCharAmount = 1;
                }
                */
                        print("MADE IT TO CastCharacterVote 5");

                if (activeCharCount < allowedCharAmount)
                {
        print("MADE IT TO CastCharacterVote 6 make sure to double check here for later it may have some werid issues with the second target spawn adventurer");

                    PlayerSlotPair newPair = new PlayerSlotPair(sPlayer, characterSlot);

                    matches[i].matchLeader.currentMatch.playerSlotPairs.Add(newPair);
                    //match.playerSlotPairs.Add(newPair);
                    foreach(var activePlayer in matches[i].matchLeader.currentMatch.players){
                        if(activePlayer == matches[i].matchLeader){
                            activePlayer.TargetSpawnAdventurer(newPair.player, charName, spriteName, characterSlot);
                            print($"{charName}, {spriteName} for leader");

                            continue;
                        }
                        activePlayer.currentMatch = matches[i].matchLeader.currentMatch;
                        activePlayer.TargetSpawnAdventurer(newPair.player, charName, spriteName, characterSlot);
                        print($"{charName}, {spriteName}");
                    }
                }
                    }
                }
        /*
        int index = sPlayer.playerIndex;
        int activeCharCount = 0;
        int allowedCharAmount = 0;
        foreach (PlayerSlotPair pair in match.playerSlotPairs)
        {
            if (pair.player == sPlayer)
            {
                activeCharCount++;
                // Check if the characterSlot already exists for this sPlayer
                if (pair.slot == characterSlot)
                {
                    return;
                }
            }
        }
        int playerCount = match.players.Count;
        if (playerCount == 1)
        {
            allowedCharAmount = 6;
        }
        else if (playerCount == 2)
        {
            allowedCharAmount = 3;
        }
        else if (playerCount == 3)
        {
            allowedCharAmount = 2;
        }
        else if (playerCount == 4)
        {
            if (index == 1 || index == 2)
            {
                allowedCharAmount = 2;
            }
            else
            {
                allowedCharAmount = 1;
            }
        }
        else if (playerCount == 5)
        {
            if (index == 1)
            {
                allowedCharAmount = 2;
            }
            else
            {
                allowedCharAmount = 1;
            }
        }
        else if (playerCount == 6)
        {
            allowedCharAmount = 1;
        }
        if (activeCharCount < allowedCharAmount)
        {
            PlayerSlotPair newPair = new PlayerSlotPair(sPlayer, characterSlot);
            
            matches[i].AddPlayerSlotPair(newPair);
            match.AddPlayerSlotPair(newPair);
            //match.playerSlotPairs.Add(newPair);
            foreach(var activePlayer in match.players){
                activePlayer.TargetSpawnAdventurer(newPair.player, charName, spriteName, characterSlot);
            }
        }
        */
    }
    public ChatManagerNode GetChatManagerNodeByMatch(Match match)
    {
        foreach (var pair in ChatManagerNodesDictionary)
        {
            if (pair.Value == match)
            {
                return pair.Key;
            }
        }
        print("Did not find a chat manager this is null ************ DOINK!!!!!!!!!!");
        return null;
    }
    public void CreateGame(ScenePlayer host, string _matchID, Match currentMatch){
        List<ScenePlayer> playerScripts = new List<ScenePlayer>();
        for (int i = 0; i < matches.Count; i++){
            if (matches[i].matchID == _matchID){
                foreach(var _player in matches[i].players){
                    _player.inLobby = false;
                    _player.inMatch = true;
                    //_player.currentMatch = match;
                    EnterNodeCost.Invoke(_player, 50f);
                    _player.TargetGetReadyForStart();
                    playerScripts.Add(_player);
                }
            }
        }
        #if UNITY_SERVER
        PlayFabServer.instance.StartTheGame(matches.Count, host.currentNode, currentMatch, host, _matchID, playerScripts); 
        #endif
        //StartCoroutine(SetUpMatch(host, _matchID, currentMatch, playerScripts));
        Debug.Log($"Game Beginning CREATEGAME ************");

    }
    public static string GetRandomMatchID(){
        string _id = string.Empty;
        for (int i = 0; i < 5; i++)
        {
            int random = UnityEngine.Random.Range(0,36);
            if (random < 26){
                _id += (char)(random + 65);
            } else {
                _id += (random - 26).ToString();
            }
        }
        Debug.Log($"Random Match ID: {_id}");
        return _id;
    }
    public void PlayerDisconnectedFromLobby(ScenePlayer player, string _matchID){
    for (int i = 0; i < matches.Count; i++){
        if (matches[i].matchID == _matchID){
            Match targetMatch = matches[i];
            
            ChatManagerNode targetChatManagerNode = null;
            foreach (KeyValuePair<ChatManagerNode, Match> entry in ChatManagerNodesDictionary)
            {
                if (entry.Value.Equals(targetMatch))
                {
                    targetChatManagerNode = entry.Key;
                    break;
                }
            }
            if (targetChatManagerNode != null)
            {
                targetChatManagerNode.RemovePlayer(player);
                if(targetChatManagerNode.partyPlayers.Count == 0){
                    if(ChatManagerNodesDictionary.ContainsKey(targetChatManagerNode))
                    ChatManagerNodesDictionary.Remove(targetChatManagerNode);
                    Destroy(targetChatManagerNode.gameObject);
                }
                // Found the ChatManagerNode associated with the targetMatch
            }
            else
            {
                // ChatManagerNode not found for the targetMatch
            }
            int playerIndex = targetMatch.players.IndexOf(player);
            targetMatch.players.RemoveAt(playerIndex);
            Debug.Log($"Player disconnected from match {_matchID} | {matches[i].players.Count} players remaining");
            //Match match;
            bool isLeader = player.matchLeader;
            player.matchLeader = false;
            
            // Clear playerSlotPairs if needed
            targetMatch.playerSlotPairs.Clear();
            if(player.currentMatch != null) {
                player.currentMatch.playerSlotPairs.Clear();
            }

            if(matches[i].players.Count == 0) {
                // Handle case with no more players in the match
                //match = matches[i];
                matches.RemoveAt(i);
                matchIDs.Remove(_matchID);
                //match = null;
                player.inLobby = false;
                player.inMatch = false;
            } else {
                // Update player indices and assign new match leader if necessary
                UpdatePlayerIndicesAndAssignLeader(targetMatch, player, isLeader);
            }

            player.rpcDisconnectFromMatchLobby();
            player.currentMatch = null;
            break;     
        }
    }
}

private void UpdatePlayerIndicesAndAssignLeader(Match targetMatch, ScenePlayer disconnectedPlayer, bool wasLeader) {
    for (int j = 0; j < targetMatch.players.Count; j++) {
        targetMatch.players[j].playerIndex = j; // Update playerIndex based on the current position in the list
    }

    if (wasLeader) {
        // Assign the next available player as the match leader
        ScenePlayer newLeader = targetMatch.players.FirstOrDefault();
        if (newLeader != null) {
            newLeader.matchLeader = true;
            // Notify the new leader if needed, for example:
            targetMatch.SetMatchLeader(newLeader);
            newLeader.TargetPassLeader(true);
            // Ensure all players have updated match information
            foreach (var player in targetMatch.players) {
                player.currentMatch = targetMatch;
                player.currentMatch.matchLeader = newLeader;
            }
            Debug.Log($"Passing match leader to player {newLeader.playerName}");
        }
    } else {
        // Ensure the match leader flag is correctly set for all players
        foreach (var player in targetMatch.players) {
            player.currentMatch = targetMatch;
        }
    }
}
/*
    public void PlayerDisconnectedFromLobby(ScenePlayer player, string _matchID){
        for (int i = 0; i < matches.Count; i++){
            if (matches[i].matchID == _matchID){
                Match targetMatch = matches[i];
                ChatManagerNode targetChatManagerNode = null;
                foreach (KeyValuePair<ChatManagerNode, Match> entry in ChatManagerNodesDictionary)
                {
                    if (entry.Value.Equals(targetMatch))
                    {
                        targetChatManagerNode = entry.Key;
                        break;
                    }
                }
                if (targetChatManagerNode != null)
                {
                    targetChatManagerNode.RemovePlayer(player);
                    if(targetChatManagerNode.partyPlayers.Count == 0){
                        if(ChatManagerNodesDictionary.ContainsKey(targetChatManagerNode))
                        ChatManagerNodesDictionary.Remove(targetChatManagerNode);
                        Destroy(targetChatManagerNode.gameObject);
                    }
                    // Found the ChatManagerNode associated with the targetMatch
                }
                else
                {
                    // ChatManagerNode not found for the targetMatch
                }
                int playerIndex = matches[i].players.IndexOf(player);
                //matches[i].players.RemoveAt(playerIndex);
                targetMatch.players.RemoveAt(playerIndex);
                Debug.Log($"Player disconnected from match {_matchID} | {matches[i].players.Count} players remaining");
                //player.currentMatch = null;
                Match match;
                bool isLeader = false;
                if(player == player.currentMatch.GetMatchLeader() && player.matchLeader){
                    isLeader = true;
                }
                player.matchLeader = false;
                targetMatch.playerSlotPairs.Clear();
                player.currentMatch.playerSlotPairs.Clear(); 
                if(matches[i].players.Count == 0) {
                    Debug.Log($"No more players in Match. Terminating {_matchID}");
                    match = matches[i];
                    matches.RemoveAt(i);
                    matchIDs.Remove(_matchID);
                    match = null;
                    player.inLobby = false;
                    player.inMatch = false;
                } else{
                    Debug.Log($"Trying to pass the match leader");
                    if(isLeader){
                        foreach(var splayer in targetMatch.players){
                            if(splayer == player){
                                continue;
                            }
                            matches.RemoveAt(i);
                            targetMatch.matchLeader = splayer;
                            splayer.currentMatch = targetMatch;
                            splayer.currentMatch.matchLeader = splayer;
                            splayer.matchLeader = true;
                            splayer.TargetPassLeader(true);
                            matches.Add(splayer.currentMatch);

                            Debug.Log($"Passing match leader to player {splayer.playerName} ");
                            foreach(var _player in splayer.currentMatch.players){
                                _player.currentMatch = targetMatch;
                            }
                            break;
                        }
                    } else {
                        matches.RemoveAt(i);
                        matches.Add(targetMatch);
                        foreach(var splayer in targetMatch.players){
                            splayer.currentMatch = targetMatch;
                            splayer.currentMatch.playerSlotPairs.Clear();
                        }
                    }
                    //foreach(var ssPlayer in matches[i].players){
                    //    ssPlayer.TargetDisconnectFromMatchLobby(player);
                    //}
                }
                player.rpcDisconnectFromMatchLobby();
                player.currentMatch = null;
                //now we need to reassign the players index for building
                break;     
            }
        }
    }
    */
    public void PlayerDisconnected (ScenePlayer player, string _matchID){
        if(player.currentMatch == null)
        {
            return;
        }
        for (int i = 0; i < matches.Count; i++){
            if (matches[i].matchID == _matchID){
                Match targetMatch = matches[i];
                ChatManagerNode targetChatManagerNode = null;
                foreach (KeyValuePair<ChatManagerNode, Match> entry in ChatManagerNodesDictionary)
                {
                    if (entry.Value.Equals(targetMatch))
                    {
                        targetChatManagerNode = entry.Key;
                        break;
                    }
                }
                if (targetChatManagerNode != null)
                {
                    targetChatManagerNode.RemovePlayer(player);
                    if(targetChatManagerNode.partyPlayers.Count == 0 || matches[i].matchNode == "ArenaDuelFriendlyMode"){
                        if(ChatManagerNodesDictionary.ContainsKey(targetChatManagerNode))
                        ChatManagerNodesDictionary.Remove(targetChatManagerNode);
                        Destroy(targetChatManagerNode.gameObject);
                    }
                    // Found the ChatManagerNode associated with the targetMatch
                }
                else
                {
                    // ChatManagerNode not found for the targetMatch
                }
                if(matches[i].matchNode == "ArenaDuelFriendlyMode"){
                    FullWipe.Invoke(matches[i], matches[i].players);
                    return;
                }
                int playerIndex = matches[i].players.IndexOf(player);
                matches[i].players.RemoveAt(playerIndex);
                print($"{playerIndex} is the players index that was removed");
                Debug.Log($"Player disconnected from match {_matchID} | {matches[i].players.Count} players remaining");
                bool isLeader = false;
                if(player == player.currentMatch.GetMatchLeader() && player.matchLeader){
                    isLeader = true;
                }
                if(matches[i].players.Count == 0) {
                    int serial = matches[i].matchSerial;
                    if(player.inMatch){
                        CLEARTHEMATCH.Invoke(_matchID, player.currentMatch.matchSerial, matches[i]);
                    }
                    Debug.Log($"No more players in Match. Terminating {_matchID}");
                    matches.RemoveAt(i);
                    matchIDs.Remove(_matchID);
                } else{
                    Debug.Log($"Trying to pass the match leader");
                    if(isLeader){
                        int leaderIndex = matches[i].players.Count;
                        int randomLeader = UnityEngine.Random.Range(0, leaderIndex);
                        ScenePlayer newLeader = matches[i].players[randomLeader].GetComponent<ScenePlayer>();
                        print($"Player: {newLeader.playerName} , is now the party leader out of {leaderIndex} players in the match: {_matchID}");
                        matches[i].SetMatchLeader(newLeader);
                        newLeader.matchLeader = true;
                        Debug.Log($"Passing match leader to player {newLeader.playerName} ");
                    }
                }   
            }
        }
    }
    /*
    public void FinishedMatch (Match match){
        print("Got to Finished Match in matchmaker call");
        if(!matches.Contains(match))
        {
            return;
        }
        for (int i = 0; i < matches.Count; i++){
            if (matches[i].matchID == match.matchID){
                //player.currentMatch.inMatch = false;
                foreach(var playerP in matches[i].players){
                    playerP.currentMatch = null;
                }
                int serial = matches[i].matchSerial;
                matchIDs.Remove(match.matchID);
                matches.RemoveAt(i);
                match = null;
                return;
            }
        }
    }
    */
    public void FinishedMatch (Match match, List<ScenePlayer> players) {
    print("Got to Finished Match in matchmaker call");
    
    // Check if match is null
    if (match == null) {
        Debug.LogError("Match is null");
        return;
    }
    ChatManagerNode targetChatManagerNode = null;
    foreach (KeyValuePair<ChatManagerNode, Match> entry in ChatManagerNodesDictionary)
    {
        if (entry.Value.Equals(match))
        {
            targetChatManagerNode = entry.Key;
            break;
        }
    }
    if (targetChatManagerNode != null)
    {
        if(ChatManagerNodesDictionary.ContainsKey(targetChatManagerNode))
        ChatManagerNodesDictionary.Remove(targetChatManagerNode);
        Destroy(targetChatManagerNode.gameObject);
    }
    else
    {
        // ChatManagerNode not found for the targetMatch
    }
    if (!matches.Contains(match)) {
        return;
    }

    for (int i = 0; i < matches.Count; i++) {
        // Check if matches[i] is null
        if (matches[i] == null) {
            Debug.LogError("matches[" + i + "] is null");
            continue;
        }
        
        if (matches[i].matchID == match.matchID) {
            // Check if matches[i].players is null
            foreach (var playerP in players) {
                // Check if playerP is null
                if (playerP == null) {
                    Debug.LogError("playerP is null");
                    continue;
                }
                playerP.currentMatch = null;
            }
            matchIDs.Remove(match.matchID);
            matches.RemoveAt(i);
            match = null;
        }
    }
}

    }
public static class MatchExtensions {
    public static Guid ToGuid (this string id) {
        MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider ();
        byte[] inputBytes = Encoding.Default.GetBytes (id);
        byte[] hashBytes = provider.ComputeHash(inputBytes);
        return new Guid (hashBytes);
    }
}
}
