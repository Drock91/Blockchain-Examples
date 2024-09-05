using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
using Mirror;
using TMPro;
using PlayFab.MultiplayerModels;
namespace dragon.mirror{
[System.Serializable]
public class CharacterSaveData {
    public int CharHealth;
    public int CharMana;
    public float CharExperience;
    public float CharClassPoints;
    public string CharID;
    public CharacterSaveData() { }
    public CharacterSaveData(int health, int mana,float exp, float classPoints, string ID)
    {
        CharHealth = health;
        CharMana = mana;
        CharExperience = exp;
        CharClassPoints = classPoints;
        CharID = ID;
    }
}

public class PlayerCharacter : MovingObject
{
    public static UnityEvent<PlayerCharacter, ScenePlayer, Match, int> CharacterSprite = new UnityEvent<PlayerCharacter, ScenePlayer, Match, int>();
    public static UnityEvent<MovingObject> ResetSpells = new UnityEvent<MovingObject>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient, CharacterSaveData>  SaveCharacter = new UnityEvent<NetworkConnectionToClient, CharacterSaveData>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient, int, string>  TrapMP = new UnityEvent<NetworkConnectionToClient, int, string>();
    public static UnityEvent<string> CombatRefresh = new UnityEvent<string>();
	[SyncVar]
	[SerializeField] public string CharacterName;
	[SyncVar]
	[SerializeField] public string ClassType;
    [SyncVar]
    [SerializeField] public bool Bow = false;
    [SyncVar]
	[SerializeField] public int Level;
	[SerializeField] public bool SoFire = false;
	[SerializeField] public bool Torch = false;
	[SyncVar]
	[SerializeField] public string CharID;
	[SerializeField] Canvas OurPlayer;
	[SerializeField] Canvas TheirPlayer;
    float PLAYEREXPERIENCE;
    float PLAYERCLASSPOINTS;
    float PLAYEREXPBONUS = 1f;
    [SerializeField] public static UnityEvent<MovingObject>  EnemyPlayerFogPosition = new UnityEvent<MovingObject>();
	public Match assignedMatch;
	[SyncVar] [SerializeField] public ScenePlayer assignedPlayer;
	private const string CastingQ = "CastingQ";
    private const string CastingE = "CastingE";
    private const string CastingR = "CastingR";
    private const string CastingF = "CastingF";
    private const string Selected = "Selected";
    [SerializeField] GameObject CombatPartyMemberBuilt;
    [SerializeField] GameObject CombatPartyMemberPrefab;
    [SerializeField] GameObject AbilityPopUpPrefab;

    
	protected override void Awake(){
		base.Awake();
	}
    protected override void Start(){
        base.Start();
		if(isServer){
            #if UNITY_SERVER
            PlayFabServer.SendEXPCP.AddListener(AcceptCPEXP);
            PlayFabServer.ENDMATCHMAKER.AddListener(SaveUnit);
            #endif
            ScenePlayer.ChangingMOSpellsMatch.AddListener(ChangingSpells);
			return;
		}
		ScenePlayer.BuildCombatPlayerUI.AddListener(BuildPlayerCharacterUICombat);
    }
    [ClientRpc]
    public void RpcGetPCName(string playersName){
        if (GetComponent<NetworkIdentity>().hasAuthority){
            OurPlayer.enabled = true;
            OurPlayer.sortingOrder = 25;
            TextMeshProUGUI OurPlayerText = OurPlayer.GetComponentInChildren<TextMeshProUGUI>();
            if(OurPlayerText)
            OurPlayerText.text = playersName;
        } else {
            TheirPlayer.enabled = true;
            TheirPlayer.sortingOrder = 25;
            TextMeshProUGUI TheirPlayerText = TheirPlayer.GetComponentInChildren<TextMeshProUGUI>();
            if(TheirPlayerText){
                TheirPlayerText.text = playersName;
                string nameText = TheirPlayerText.text;
                if(!ScenePlayer.localPlayer.GetFriendly(this)){
                   TheirPlayerText.text = $"<color=#8B0000>{nameText}</color>";
                } else {
                   TheirPlayerText.text = $"<color=#00E3FF>{nameText}</color>";
                }
            }
            
        }
    }
    [TargetRpc]
    public void TargetUpdateHarvestFinish(bool success, string amount, string energyCost, string rareName){
        GameObject harvestPopUp = Instantiate(AbilityPopUpPrefab, new Vector3(transform.position.x, transform.position.y + 1f), Quaternion.identity);
        HarvestPopUp harvestDisplay = harvestPopUp.GetComponent<HarvestPopUp>();
        harvestDisplay.HarvestPop(amount, rareName, success, energyCost);
                //abilityDisplay.AbilityPopUpBuild(Spell, null);
    }
    [TargetRpc]
    public void TargetImmune(){
        ImproperCheckText.Invoke("Target was immune to poison");
    }
    [Server]
    public void ServerTrap(int hp, int mp, bool debuff){
        if (Dying)
		return;
        if(Invisibility){
    		ServerRemoveStatus("Stealthed", "Invisibility", true);
        }
        if(InvisibilityUndead){

        }
        if(Hide){
            ServerRemoveStatus("Stealthed", "Hide", true);
        }
        if(Sneak){
            ServerRemoveStatus("Stealthed", "Sneak", true);        
        }
        if(cur_hp > 0){
            cur_hp -= hp;
        }
        if(cur_mp > 0){
            cur_mp -= mp;
        }
        if (cur_hp <= 0){
                ServerStopCasting();
	    		cur_hp = 0;
	    			if(!Dying){
	    				Dying = true;
	    				DeathEXP();
	    				StopATTACKINGMob();
	    				agent.isStopped = true;
	    				//agent.enabled = false;
	    				CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
	    				circleCollider.enabled = false;
	    				DeathCharacter.Invoke(assignedPlayer.connectionToClient, CharID, curatorTM);
	    				RemoveCharReserve.Invoke(this);
	    				//RevokePlayerAuthority();
	    				RpcCharDeath();
	    				DeadChar.Invoke(this, assignedMatch);
	    				Target = null;
	    			}
	    } else {
            if(hp > 0){
                if(!curatorTM.GetDuelMode())
	            TakeDamageCharacter.Invoke(assignedPlayer.connectionToClient, cur_hp, CharID);
            }
            if(mp > 0){
                if(!curatorTM.GetDuelMode())
	            TrapMP.Invoke(assignedPlayer.connectionToClient, cur_mp, CharID);
            }
            if(hp > 0 || mp > 0){
                RpcSpawnTrapDmg(hp, mp);
            }
	    }
    }
    [Server]
    void ChangingSpells(string ID, ScenePlayer CharOwner, SendSpellList spellList){
        if(ID == CharID && CharOwner == assignedPlayer){
            SpellQ = spellList.SpellQ;
            SpellE = spellList.SpellE;
            SpellR = spellList.SpellR;
            SpellF = spellList.SpellF;
            bool CheckedSpellQ = false;
            bool CheckedSpellE = false;
            bool CheckedSpellR = false;
            bool CheckedSpellF = false;
            foreach(var sheet in assignedPlayer.GetInformationSheets()){
                if(sheet.CharacterID == CharID){
                    foreach (var coolies in sheet.CharCooldownData){
                        DateTime initialTime = DateTime.UtcNow;
                        DateTime completedTime = DateTime.Parse(coolies.Value, null, System.Globalization.DateTimeStyles.RoundtripKind);
                        if (initialTime < completedTime)
                        {
                            TimeSpan timeDifference = completedTime - initialTime;
                            float timeDifferenceInSeconds = (float)timeDifference.TotalSeconds;

                            if(coolies.SpellnameFull.Contains(spellList.SpellQ)){
                                CheckedSpellQ = true;
                                RunSetAbilityCooldownX(timeDifferenceInSeconds, true, "Q");
                            }
                            if(coolies.SpellnameFull.Contains(spellList.SpellE)){
                                CheckedSpellE = true;
                                RunSetAbilityCooldownX(timeDifferenceInSeconds, true, "E");
                            }
                            if(coolies.SpellnameFull.Contains(spellList.SpellR)){
                                CheckedSpellR = true;
                                RunSetAbilityCooldownX(timeDifferenceInSeconds, true, "R");
                            }
                            if(coolies.SpellnameFull.Contains(spellList.SpellF)){
                                CheckedSpellF = true;
                                RunSetAbilityCooldownX(timeDifferenceInSeconds, true, "F");
                            }
                            print($"cooldown added to player character {completedTime.ToString() + ";" + coolies.SpellnameFull} was our cooldown data value");
                        } else {
                            if(coolies.SpellnameFull.Contains(spellList.SpellQ)){
                                CheckedSpellQ = true;
                                RunSetAbilityCooldownX(0f, true, "Q");
                            }
                            if(coolies.SpellnameFull.Contains(spellList.SpellE)){
                                CheckedSpellE = true;
                                RunSetAbilityCooldownX(0f, true, "E");
                            }
                            if(coolies.SpellnameFull.Contains(spellList.SpellR)){
                                CheckedSpellR = true;
                                RunSetAbilityCooldownX(0f, true, "R");
                            }
                            if(coolies.SpellnameFull.Contains(spellList.SpellF)){
                                CheckedSpellF = true;
                                RunSetAbilityCooldownX(0f, true, "F");
                            }
                            //cooldownData.Add(coolies.PKey, null);
                            print($"{coolies.SpellnameFull} was not on CD");
                        }
                    }
                }
            }
            if(SpellQ == "None" || SpellQ == "Empty" || !CheckedSpellQ){
                RunSetAbilityCooldownX(0f, true, "Q");
            }
            if(SpellE == "None" || SpellE == "Empty" || !CheckedSpellE){
                RunSetAbilityCooldownX(0f, true, "E");
            }
            if(SpellR == "None" || SpellR == "Empty" || !CheckedSpellR){
                RunSetAbilityCooldownX(0f, true, "R");
            }
            if(SpellF == "None" || SpellF == "Empty" || !CheckedSpellF){
                RunSetAbilityCooldownX(0f, true, "F");
            }
            print($"{CharacterName} Q spell is {SpellQ}");
            print($"{CharacterName} E spell is {SpellE}");
            print($"{CharacterName} R spell is {SpellR}");
            print($"{CharacterName} F spell is {SpellF}");
            TargetResetSpell(spellList);
        }
    }
    [TargetRpc]
    void TargetResetSpell(SendSpellList spellList){
        ResetSpells.Invoke(this);
    }
    [Server]
    public void ServerEXPResurrectRestoration(int resurrectLevel){
         // Calculate the restoration rate
        float expRestorationRate = 50f + ((resurrectLevel - 1) * 0.5f);
        // Calculate the percentage to restore
        float restorationPercentage = expRestorationRate / 100f;
        // Calculate the experience lost during death
        float expLost = PLAYEREXPERIENCE / 0.9f - PLAYEREXPERIENCE;
        // Calculate the experience to restore
        float expToRestore = expLost * restorationPercentage;
        // Restore the experience
        PLAYEREXPERIENCE += expToRestore;
    }
    public float GetPLAYEREXPERIENCE(){
        return PLAYEREXPERIENCE;
    }
    [Server]
    void AcceptCPEXP(Match match, float cp, float exp){
        if(Dying){
            return;
        }
        if(match == assignedMatch && !curatorTM.GetDuelMode() ){
            PLAYERCLASSPOINTS += cp;
            PLAYEREXPERIENCE += (exp * PLAYEREXPBONUS);
        }
    }
    [Server]
    public void ServerResetPEXPBONUS(float expBonus){
        PLAYEREXPBONUS = 1;
        PLAYEREXPBONUS += expBonus;
    }
    [Command]
    public void CmdUnlockDoor(Door door, Match match)
    {
        curatorTM.CuratorUnlockDoor(door, match);
    }
    [Command]
    public void CmdCloseDoor(Door door, Match match)
    {
        curatorTM.CuratorCloseDoor(door, match);
    }
    [Command]
    public void CmdBreakDoor(Door door, Match match)
    {
        curatorTM.CuratorBreakDoor(door, match);
    }

    [Command]
    public void CmdOpenDoor(Door door, Match match)
    {
        curatorTM.CuratorOpenDoor(door, match);
    }
    [Command]
    public void CmdOpenMainChest(MainChest mainchest){
        curatorTM.OpenChest(mainchest, assignedMatch);
    }
    [Command]
    public void CmdOpenMiniChest(MiniChest miniChest){
        curatorTM.LootMiniChest(miniChest, assignedMatch);
    }
    [Command]
    public void CmdPickUpArmor(ArmorDrop armorDrop){
        curatorTM.LootArmorRack(armorDrop, assignedMatch);
    }
    [Command]
    public void CmdPickUpWeapon(WeaponDrop weaponDrop){
        curatorTM.LootWeaponRack(weaponDrop, assignedMatch);
    }
    [Command]
    public void CmdHarvestLeather(LeatherNodeDrop LeatherDrop){
        if(LeatherDrop == null){
            return;
        }
        if(Casting){
			Casting = false;
        	RpcCancelCastAnimation();
		}
        if (agent == null) 
    	{
    	    Debug.LogWarning("NavMeshAgent not found.");
			agent = GetComponent<NavMeshAgent>();
			if (agent == null) 
    		{
    		    Debug.LogWarning("NavMeshAgent not found again.");
    		    return;
    		}
    	}
        agent.ResetPath();
    	agent.isStopped = false;
        curatorTM.LootLeatherRack(LeatherDrop, this, assignedMatch);
        //RpcCancelCastAnimation();
    }
    [Command]
    public void CmdHarvestCloth(ClothNodeDrop ClothDrop){
        if(ClothDrop == null){
            return;
        }
        if(Casting){
			Casting = false;
        	RpcCancelCastAnimation();
		}
        if (agent == null) 
    	{
    	    Debug.LogWarning("NavMeshAgent not found.");
			agent = GetComponent<NavMeshAgent>();
			if (agent == null) 
    		{
    		    Debug.LogWarning("NavMeshAgent not found again.");
    		    return;
    		}
    	}
    	agent.ResetPath();
        agent.isStopped = false;
        curatorTM.LootClothRack(ClothDrop, this, assignedMatch);
        //RpcCancelCastAnimation();
    }
    [Command]
    public void CmdHarvestTree(TreeNodeDrop TreeNodeDrop){
        if(TreeNodeDrop == null){
            return;
        }
        Casting = false;
    	if (agent == null) 
    	{
    	    Debug.LogWarning("NavMeshAgent not found.");
			agent = GetComponent<NavMeshAgent>();
			if (agent == null) 
    		{
    		    Debug.LogWarning("NavMeshAgent not found again.");
    		    return;
    		}
    	}
    	agent.ResetPath();
        agent.isStopped = false;
        curatorTM.LootTreeRack(TreeNodeDrop, this, assignedMatch);
        //RpcCancelCastAnimation();
    }
    [Command]
    public void CmdHarvestOre(OreNodeDrop OreNodeDrop){
        if(OreNodeDrop == null){
            return;
        }
        if(Casting){
			Casting = false;
        	RpcCancelCastAnimation();
		}
        if (agent == null) 
    	{
    	    Debug.LogWarning("NavMeshAgent not found.");
			agent = GetComponent<NavMeshAgent>();
			if (agent == null) 
    		{
    		    Debug.LogWarning("NavMeshAgent not found again.");
    		    return;
    		}
    	}
    	agent.ResetPath();
        agent.isStopped = false;
        curatorTM.LootOreRack(OreNodeDrop, this, assignedMatch);
        //RpcCancelCastAnimation();
    }
    [Command]
    public void CmdHarvestStone(StoneNodeDrop StoneNodeDrop){
        if(StoneNodeDrop == null){
            return;
        }
        if(Casting){
			Casting = false;
        	RpcCancelCastAnimation();
		}
        if (agent == null) 
    	{
    	    Debug.LogWarning("NavMeshAgent not found.");
			agent = GetComponent<NavMeshAgent>();
			if (agent == null) 
    		{
    		    Debug.LogWarning("NavMeshAgent not found again.");
    		    return;
    		}
    	}
    	agent.ResetPath();
        agent.isStopped = false;
        curatorTM.LootStoneRack(StoneNodeDrop, this, assignedMatch);
        //RpcCancelCastAnimation();
    }
    bool Inside = false;
    public bool GetInsidePC(){
        return Inside;
    }
	[Server]
	public void EnergySpark(MovingObject obj, string agentTypeName){
		if(obj == this){
            bool inside = curatorTM.GetInside();
			StartCoroutine(EnergyUpdaterCharacter(inside, agentTypeName, SoFire, Torch));
		}
	}
	public IEnumerator StartNavMesh(string agentTypeName){
        yield return new WaitForSeconds(1f);
        print($"Starting agent now on {gameObject.name}");
        int agentTypeID = -1;
        for (int i = 0; i < NavMesh.GetSettingsCount(); i++)
        {
            var id = NavMesh.GetSettingsByIndex(i).agentTypeID;
            var name = NavMesh.GetSettingsNameFromID(id);
            print($"agentType is {agentTypeName} and the navmesh iD is {name}");
            if (name == agentTypeName)
            {
                agentTypeID = id;
                break;
            }
        }
        if (agentTypeID != -1)
        {
            // Now you can set the agentTypeID on your NavMeshAgent
            agent.agentTypeID = agentTypeID;
        }
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance; 
        agent.enabled = true;
    }
    
	[Server]
	public IEnumerator EnergyUpdaterCharacter(bool inside, string agentTypeName, bool SoF, bool torch){
		////print($"ENERGIZEDDDDDDD this character {gameObject.name}");
		float rechargeTime = .5f;
		float haste = 0f;
		Energized = true;
        bool deadChar = false;
            //instantiate the players object and pass it the information to do what it needs to do
            foreach(var key in assignedPlayer.GetInformationSheets()){
                if(key.CharacterID == CharID){
                    foreach(var KVP in key.CharStatData){
                        if(KVP.Key == "DEATH"){
                            deadChar = true;
                            break;
                        }
                    }
                    break;
                }
            }
        RpcReadyToPlay(deadChar);
        StartCoroutine(SendRpcQueue());
        stamina = 0f;
        Inside = inside;
        StartCoroutine(StartNavMesh(agentTypeName));
		ClientSparkVision(inside, SoF, torch);
		SetStatsServer();
        //StartCoroutine(AdjustStartCooldowns(assignedPlayer, CharID));
        //StartCoroutine(UpdateAgentRoutine());
		while (Energized)
    	{
    	    if (!moving && !Casting && !Mesmerized && !Stunned && !Feared)
    	    {
					if(GetAgility() > 101){
						haste = Mathf.Floor((GetAgility() - 100) / 2);
					} else {
						haste = 0f;
					}
				rechargeTime = .5f / (1f + (haste / 100f));
                float stamRecovered = 5f;
                if(Sneak){
                    stamRecovered = 3.5f;
                }
				stamina -= stamRecovered;
        		stamina = Mathf.Clamp(stamina, -100f, 250f);
				//if(stamina <= 0){
    	        //    RpcClientCheckColor();
    	        //} else {
				//	RpcChangeToGray();
				//}
				yield return new WaitForSeconds(rechargeTime);
    	    } else {
				yield return null;
			}
    	}
    }

    public void DeathEXP(){
        if(!curatorTM.GetDuelMode())
        PLAYEREXPERIENCE *= .9f;
    }
    IEnumerator UpdateAgentRoutine() {
        while (true) {
            if(isServer && Energized)
            {
                //if(transform.position.z != 0){
                //    transform.position = new Vector3(transform.position.x, transform.position.y, 0);
                //}
                if(!agent.enabled){
                    yield return new WaitForSeconds(.25f);
                    continue;
                }
                if (!agent.pathPending && agent.enabled)
                {
                    if(moving && Casting){
                        ServerStopCasting();
                    }
                    if (agent.remainingDistance <= GetAttacKRange() && agent.enabled)
                    {
                        //RadiusLock = false;
                        if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f && agent.enabled)
                        {
                            if (moving) // If the agent was previously moving
                            {
                                moving = false;

                                //agent.radius *= 2;
                                //syncAgentRadius = agent.radius;
                                // If agent has moved at least 1 unit since last update
                                if (accumulatedDistance >= 1f)
                                {

                                    accumulatedDistance = 0; // Reset the accumulated distance
                                    Vector3 updateLocation = new Vector3(Mathf.FloorToInt(transform.position.x) + 0.5f, Mathf.FloorToInt(transform.position.y) + 0.5f, 0);
                                    NewFogUpdate(updateLocation); // Run NewFogUpdate

                                }

                            }
                        }
                    }
                    else
                    {
                        //if (!moving)
                        //{
                        //    moving = true;
                        //    // Reduce the baseOffset by half when agent starts moving
                        //    agent.radius /= 2;
                        //    syncAgentRadius = agent.radius;
                        //}
                        //moving = true;
                        accumulatedDistance += Vector3.Distance(agent.transform.position, lastPosition); // Update the accumulated distance
                        if (accumulatedDistance >= 1f)
                        {
                            accumulatedDistance = 0; // Reset the accumulated distance
                            Vector3 updateLocation = new Vector3(Mathf.FloorToInt(transform.position.x) + 0.5f, Mathf.FloorToInt(transform.position.y) + 0.5f, 0);
                            NewFogUpdate(updateLocation); // Run NewFogUpdate
                        }
                        lastPosition = agent.transform.position; // Update the last position
                    }
                }

            }
            yield return new WaitForSeconds(.25f);
        }
    }
    [ClientRpc]
	public void ClientSparkVision(bool inside, bool SoF, bool torch){
		StartCoroutine(VisionSpark(inside, SoF, torch));
	}
    IEnumerator VisionSpark(bool inside, bool SoF, bool torch){
		yield return new WaitForSeconds(3f);
		float RevealingRange = inside ? 4f : 7f;
		if (SoF) {
		    RevealingRange = inside ? 6f : 10f;
		}
        if (torch) {
		    RevealingRange = inside ? 5f : 8f;
		}
		FoggyWar fogofWar = GameObject.Find("FogMachine").GetComponent<FoggyWar>();
		if(!fogofWar){
			yield break;
		}
		//print($"Sending AddCharacter to {CharacterName} in the fog machaine for {RevealingRange} vision!!");
        MovingObject mo = GetComponent<MovingObject>();
        if(ScenePlayer.localPlayer.GetFriendlyList().Contains(mo)){
		    fogofWar.AddCharacter(this.gameObject, RevealingRange);
		    fogofWar.UpdateFogOfWar(this.gameObject, transform.position);
        } else {
            fogofWar.AddEnemyPlayerCharacterToEnemies(mo);
        }
		//print($"Sending visionspark to {GetComponent<PlayerCharacter>().CharacterName}");
		//FogUpdate.Invoke(transform.position, RevealingRange);
		
	}
    IEnumerator LateTargetShield(){
        yield return new WaitForSeconds(6f);
        TargetRpcStartShield();
    }
    [TargetRpc]
    public void TargetRpcStartShield(){
        shield = true;
    }
    
    [TargetRpc]
    public void TargetRpcSetShield(bool _shield){
        shield = _shield;
    }
    [Server]
    public void  SetClientLightSpell(bool charCasted, int spellRank){
        RpcAnimateTacticianSpell("Light", this);
        if(charCasted){
            StartCoroutine(LightSpellServerMonitorWizardCasted(Inside, spellRank));
        } else {
            StartCoroutine(LightSpellServerMonitor(Inside, spellRank));
        }
    }
    IEnumerator LightSpellServerMonitor(bool inside, int spellRank){
        ClientLightVisionUpdate(inside, true);
        int lvl = 1;
        for(int o = 0; o < assignedPlayer.GetTacticianSheet().TacticianStatData.Count; o++){
            if(assignedPlayer.GetTacticianSheet().TacticianStatData[o].Key == "LVL"){
                lvl = int.Parse(assignedPlayer.GetTacticianSheet().TacticianStatData[o].Value);
                break;
            }
        }
        ClientUpdateStatChangesAdd("Light", 300f, 300f, 1, "Tactician's Light", true, spellRank, false);
		yield return new WaitForSeconds(300f);
        ClientUpdateStatChangesRemove("Light", "Tactician's Light", true);
        ClientLightVisionUpdate(inside, false);
    }
    IEnumerator LightSpellServerMonitorWizardCasted(bool inside, int spellRank){
        ClientLightVisionUpdate(inside, true);
        ClientUpdateStatChangesAdd("Light", 300f, 300f, 1, "Light", true, spellRank, false);
		yield return new WaitForSeconds(300f);
        ClientUpdateStatChangesRemove("Light", "Light", true);
        ClientLightVisionUpdate(inside, false);
    }
    [ClientRpc]

    void ClientLightVisionUpdate(bool inside, bool Added){
        float RevealingRange = inside ? 6f : 10f;
        if(!Added){
            RevealingRange = inside ? 4f : 7f;
        }
		//float RevealingRange = inside ? 4f : 7f;
		FoggyWar fogofWar = GameObject.Find("FogMachine").GetComponent<FoggyWar>();
		if(!fogofWar){
			return;
		}
		//print($"Sending AddCharacter to {CharacterName} in the fog machaine for {RevealingRange} vision!!");
		MovingObject mo = GetComponent<MovingObject>();
        if(ScenePlayer.localPlayer.GetFriendlyList().Contains(mo)){
		    fogofWar.AddCharacter(this.gameObject, RevealingRange);
		    fogofWar.UpdateFogOfWar(this.gameObject, transform.position);
        } else {
            fogofWar.AddEnemyPlayerCharacterToEnemies(mo);
        }
    }
    
    IEnumerator LightSpellBuff(bool inside){
		float RevealingRange = inside ? 6f : 10f;
		//float RevealingRange = inside ? 4f : 7f;
		FoggyWar fogofWar = GameObject.Find("FogMachine").GetComponent<FoggyWar>();
		if(!fogofWar){
			yield break;
		}
		//print($"Sending AddCharacter to {CharacterName} in the fog machaine for {RevealingRange} vision!!");
        MovingObject mo = GetComponent<MovingObject>();
        if(ScenePlayer.localPlayer.GetFriendlyList().Contains(mo)){
		    fogofWar.AddCharacter(this.gameObject, RevealingRange);
		    fogofWar.UpdateFogOfWar(this.gameObject, transform.position);
        } else {
            fogofWar.AddEnemyPlayerCharacterToEnemies(mo);
        }
		yield return new WaitForSeconds(300f);
		RevealingRange = inside ? 4f : 7f;
        FoggyWar fogofWarCheckAgain = GameObject.Find("FogMachine").GetComponent<FoggyWar>();
		if(!fogofWar){
			yield break;
		}
		//print($"Sending AddCharacter to {CharacterName} in the fog machaine for {RevealingRange} vision!!");
        if(ScenePlayer.localPlayer.GetFriendlyList().Contains(mo)){
		    fogofWarCheckAgain.AddCharacter(this.gameObject, RevealingRange);
		    fogofWarCheckAgain.UpdateFogOfWar(this.gameObject, transform.position);
        } else {
            fogofWarCheckAgain.AddEnemyPlayerCharacterToEnemies(mo);
        }

		//print($"Sending visionspark to {GetComponent<PlayerCharacter>().CharacterName}");
		//FogUpdate.Invoke(transform.position, RevealingRange);
		
	}
	[Server]
	public void AssignedPayerAndMatch(ScenePlayer player, Match match, string number, bool multiplayer){
		////print("assigning playercharacter its stats");
		assignedMatch = match;
		assignedPlayer = player;
             // Calculate the raw arcana value from the MPBonus

		//StartCoroutine(SetUpPrefab(player, number));
		PrefabCharacterSetup(player, number, multiplayer);

	}
    [Server]
    public void StartProcessBeginningBuffsServer(){
        StartCoroutine(ProcessBeginningBuffs(assignedPlayer, CharID));
    }
    IEnumerator ProcessBeginningBuffs(ScenePlayer player, string charID){
        yield return new WaitForSeconds(1f);
        List<CharacterFullDataMessage> characterSheets = player.GetInformationSheets();
        List<CharacterCooldownListItem> GarbageList = new List<CharacterCooldownListItem>();
        Dictionary<string, float> SetAbilityCooldownsDictionaryStartMatch = new Dictionary<string, float>();
        foreach(var sheet in characterSheets){
            if(sheet.CharacterID == charID){
                if(sheet.CharCooldownData != null){
                    DateTime initialTime = DateTime.UtcNow;
                    foreach(var coolies in sheet.CharCooldownData){
                        print($"{coolies.Value} is when this cooldown {coolies.SpellnameFull} is due to expire");
                        if(coolies.SpellnameFull == SpellQ){
                            DateTime completedTime = DateTime.Parse(coolies.Value, null, System.Globalization.DateTimeStyles.RoundtripKind);
                            TimeSpan timeLeft = completedTime - initialTime;
                            int remainingSeconds = (int)timeLeft.TotalSeconds;
                            if (remainingSeconds > 0) {
                                SetAbilityCooldownsDictionaryStartMatch.Add("Q", (float)remainingSeconds);
                            } else {
                                print($"COOLDOWNQ was off cooldown");
                                GarbageList.Add(coolies);
                                CooldownQ = 0f;
                                SpellQCoolDown = false;
                            }
                        }
                        if(coolies.SpellnameFull == SpellE){
                            DateTime completedTime = DateTime.Parse(coolies.Value, null, System.Globalization.DateTimeStyles.RoundtripKind);
                            TimeSpan timeLeft = completedTime - initialTime;
                            int remainingSeconds = (int)timeLeft.TotalSeconds;
                            if (remainingSeconds > 0) {
                                SetAbilityCooldownsDictionaryStartMatch.Add("E", (float)remainingSeconds);
                            } else {
                                print($"COOLDOWNE was off cooldown");
                                GarbageList.Add(coolies);
                                CooldownE = 0f;
                                SpellECoolDown = false;
                            }
                        }
                        if(coolies.SpellnameFull == SpellR){
                            DateTime completedTime = DateTime.Parse(coolies.Value, null, System.Globalization.DateTimeStyles.RoundtripKind);
                            TimeSpan timeLeft = completedTime - initialTime;
                            int remainingSeconds = (int)timeLeft.TotalSeconds;
                            if (remainingSeconds > 0) {
                                SetAbilityCooldownsDictionaryStartMatch.Add("R", (float)remainingSeconds);
                            } else {
                                print($"COOLDOWNR was off cooldown");
                                GarbageList.Add(coolies);
                                CooldownR = 0f;
                                SpellRCoolDown = false;
                            }
                        }
                        if(coolies.SpellnameFull == SpellF){
                            DateTime completedTime = DateTime.Parse(coolies.Value, null, System.Globalization.DateTimeStyles.RoundtripKind);
                            TimeSpan timeLeft = completedTime - initialTime;
                            int remainingSeconds = (int)timeLeft.TotalSeconds;
                            if (remainingSeconds > 0) {
                                SetAbilityCooldownsDictionaryStartMatch.Add("F", (float)remainingSeconds);
                            } else {
                                print($"COOLDOWNF was off cooldown");
                                GarbageList.Add(coolies);
                                CooldownF = 0f;
                                SpellFCoolDown = false;
                            }
                        }
                    }
                }
                if(sheet.CharBuffData != null){
                    foreach(var buff in sheet.CharBuffData){
                        ServerBuildingBuff(CharID, assignedPlayer, assignedMatch, buff, true);
                    }
                }
            }
        }
        for (int i = 0; i < GarbageList.Count; i++)
        {
            assignedPlayer.ServerCooldownRemove(CharID, GarbageList[i]);
        }
        foreach(var cooldown in SetAbilityCooldownsDictionaryStartMatch){
            RunSetAbilityCooldownX(cooldown.Value, true, cooldown.Key);
            //if(cooldown.Key == "Q"){
            //    StartCoroutine(SetAbilityCoolDownQ(cooldown.Value, true));
            //}
            //if(cooldown.Key == "E"){
            //    StartCoroutine(SetAbilityCoolDownE(cooldown.Value, true));
            //}
            //if(cooldown.Key == "R"){
            //    StartCoroutine(SetAbilityCoolDownR(cooldown.Value, true));
            //}
            //if(cooldown.Key == "F"){
            //    StartCoroutine(SetAbilityCoolDownF(cooldown.Value, true));
            //}
        }
        
    }
	[Server]
	void PrefabCharacterSetup(ScenePlayer player, string charID, bool multiplayer){
        string _class = string.Empty;
        string _CORE = string.Empty;
        int level = 1;
        float currentWeight = 0f;
        List<CharacterFullDataMessage> characterSheets = player.GetInformationSheets();
        List<string> SetBonusItems = new List<string>();
        foreach(var sheet in characterSheets){
            if(sheet.CharacterID == charID){
                foreach(var spellItem in sheet.CharSpellData){
                    if(spellItem.Key == "SPELLQ"){
                        SpellQ = spellItem.Value;
                    }
                    if(spellItem.Key == "SPELLE"){
                        SpellE = spellItem.Value;
                    }
                    if(spellItem.Key == "SPELLR"){
                        SpellR = spellItem.Value;
                    }
                    if(spellItem.Key == "SPELLF"){
                        SpellF = spellItem.Value;
                    }
                }
                foreach(var stat in sheet.CharStatData){
                    if(stat.Key == "CORE"){
                        _CORE = stat.Value;
                    }
                    if(stat.Key == "Class"){
                        _class = stat.Value;
                        ClassType = stat.Value;
                    }
                    if(stat.Key == "currentHP"){
                        cur_hp += int.Parse(stat.Value);
                    }
                    if(stat.Key == "currentMP"){
                        cur_mp += int.Parse(stat.Value);
                    }
                    if(stat.Key == "LVL"){
                        Level = int.Parse(stat.Value);
                        level = int.Parse(stat.Value);
                    }
                    if(stat.Key == "CharName"){
			        	CharacterName = stat.Value;
			        }
                    if(stat.Key == "EXP"){
			        	PLAYEREXPERIENCE = float.Parse(stat.Value);
			        }
                    if(stat.Key == "ClassPoints"){
			        	PLAYERCLASSPOINTS = float.Parse(stat.Value);
			        }
                    if(stat.Key == "CharacterID"){
			        	CharID = stat.Value;
			        }
                }
                bool sofPresent = false;
                foreach(var charItem in sheet.CharInventoryData){
                    if(charItem.Value.GetEQUIPPED()){
                        if(charItem.Value.Deleted || charItem.Value.amount == 0 || !charItem.Value.NFT && charItem.Value.Durability == "0"){
                            continue;
                        }
                        if(StatAsset.Instance.IsItemInSet(charItem.Value.GetItemName())){
                            SetBonusItems.Add(charItem.Value.GetItemName());
                        }
                        if(charItem.Value.GetItemSpecificClass() == "Bow"){
                            //print($"{CharacterName} Had Bow");
                            Bow = true;
                        }
                        if(charItem.Value.GetItemName() == "Sword Of Fire"){
                            //print($"{CharacterName} Had SoFire");

                            sofPresent = true;
                            BonusFireWeapon = true;
                            BonusFireEffect = 1 * level;
                        }
                        if(charItem.Value.GetItemName() == "Acidic Axe"){
                            //print($"Had Axe");

                            BonusPoisonWeapon = true;
                            BonusPoisonEffect = 1 * level;
                        }
                        if(charItem.Value.GetItemName() == "Bow Of Power"){
                            //print($"Had Bow");

                            BonusMagicWeapon = true;
                            BonusMagicEffect = 1 * level;
                            Bow = true;
                        }
                        if(charItem.Value.GetItemName() == "Frozen Greatsword"){
                            //print($"Had BonusColdWeapon");

                            BonusColdWeapon = true;
                            BonusColdEffect = 2 * level;
                        }
                        if(charItem.Value.GetItemName() == "Greatspear Of Dragonslaying"){
                            //print($"Had BonusDragonWeapon");

                            BonusDragonWeapon = true;
                            BonusDragonEffect = 5 * level;
                        }
                        if(charItem.Value.GetItemName() == "Mace Of Healing"){
                            healingIncrease = 1 * level;
                            print($"Had healingIncrease of {healingIncrease} because its wearing a mace of healing nft!!");
                        }
                        if(charItem.Value.GetItemName() == "Spear Of Dragonslaying"){
                            //print($"Had BonusDragonWeapon");

                            BonusDragonWeapon = true;
                            BonusDragonEffect = 2 * level;
                        }
                        if(charItem.Value.GetItemName() == "Staff Of Protection"){
                            //print($"Had armor increase");

                            armor += (1 * level);
                        }
                        if(charItem.Value.GetItemName() == "Thunder Infused Greathammer"){
                            //print($"Had BonusMagicWeapon");

                            BonusMagicWeapon = true;
                            BonusMagicEffect = 2 * level;
                        }
                        if(charItem.Value.GetItemName() == "Vampiric Dagger"){
                            //print($"Had leech!!");
                            
                            BonusLeechWeapon = true;
                            BonusLeechEffect = 1 + (level - 1) / 3;
                        }
                        if(charItem.Value.GetItemName() == "Venomous Greataxe"){
                            //print($"Had BonusPoisonWeapon and some big ass damage!!");

                            BonusPoisonWeapon = true;
                            BonusPoisonEffect = 2 * level;
                        }
                        if(charItem.Value.GetSTRENGTH_item() != null)
                        {
                            strength += int.Parse(charItem.Value.GetSTRENGTH_item());
                        }
                        if(charItem.Value.GetAGILITY_item() != null)
                        {
                            agility += int.Parse(charItem.Value.GetAGILITY_item());
                        }
                        if(charItem.Value.GetFORTITUDE_item() != null)
                        {
                            fortitude +=  int.Parse(charItem.Value.GetFORTITUDE_item());
                        }
                        if(charItem.Value.GetARCANA_item() != null)
                        {
                            arcana += int.Parse(charItem.Value.GetARCANA_item());
                        }
                        if(charItem.Value.GetArmor_item() != null)
                        {
                            armor += int.Parse(charItem.Value.GetArmor_item());
                        }
                        if(charItem.Value.GetMagicResist_item() != null)
                        {
                            MagicResist += int.Parse(charItem.Value.GetMagicResist_item());
                        }
                        if(charItem.Value.GetFireResist_item() != null)
                        {
                            FireResist += int.Parse(charItem.Value.GetFireResist_item());
                        }
                        if(charItem.Value.GetColdResist_item() != null)
                        {
                            ColdResist += int.Parse(charItem.Value.GetColdResist_item());
                        }
                        if(charItem.Value.GetDiseaseResist_item() != null)
                        {
                            DiseaseResist += int.Parse(charItem.Value.GetDiseaseResist_item());
                        }
                        if(charItem.Value.GetPoisonResist_item() != null)
                        {
                            PoisonResist += int.Parse(charItem.Value.GetPoisonResist_item());
                        }
                        if(charItem.Value.GetItemSpecificClass() == "Shield"){
                            shield = true;
                            StartCoroutine(LateTargetShield());
                        }
                        if(charItem.Value.GetBlockChance() != "0" && !string.IsNullOrEmpty(charItem.Value.GetBlockChance())){
                            //shield = true;
                            //TargetRpcStartShield();
                            //TargetRpcSetShield(shield);
                            shieldChance = int.Parse(charItem.Value.GetBlockChance());
                        }
                        if(charItem.Value.GetBlockValue() != "0" && !string.IsNullOrEmpty(charItem.Value.GetBlockValue())){
                            //shield = true;
                            shieldValue = int.Parse(charItem.Value.GetBlockValue());
                            ThreatMod = true;
                            ThreatModifier = 3.5f;
                        }
                        if(charItem.Value.GetDamageMin() != "0" && !string.IsNullOrEmpty(charItem.Value.GetDamageMin())){
                            if(charItem.Value.GetEQUIPPEDSLOT() == "Main-Hand"){
                                minDmgMH = int.Parse(charItem.Value.GetDamageMin());
                                weaponType = charItem.Value.GetItemSpecificClass();
                                if(ClassType == "Druid" && weaponType == "Staff"){
                                    StaffDruid = true;
                                }
                            }
                        }
                        if(charItem.Value.GetDamageMax() != "0" && !string.IsNullOrEmpty(charItem.Value.GetDamageMax())){
                            if(charItem.Value.GetEQUIPPEDSLOT() == "Main-Hand"){
                                maxDmgMH = int.Parse(charItem.Value.GetDamageMax());
                                if(!string.IsNullOrEmpty(charItem.Value.GetAttackDelay()))
                                {
                                    attackDelay += float.Parse(charItem.Value.GetAttackDelay());
                                }
                                if(!string.IsNullOrEmpty(charItem.Value.GetPenetration()))
                                {
                                    penetration = int.Parse(charItem.Value.GetPenetration());
                                }
                                if(!string.IsNullOrEmpty(charItem.Value.GetParry()))
                                {
                                    parry += int.Parse(charItem.Value.GetParry());
                                }
                            }
                        }
                        if(charItem.Value.GetDamageMin() != "0" && !string.IsNullOrEmpty(charItem.Value.GetDamageMin())){
                            if(charItem.Value.GetEQUIPPEDSLOT() == "Off-Hand"){
                                duelWielding = true;
                                minDmgOH = int.Parse(charItem.Value.GetDamageMin());
                                weaponTypeOH = charItem.Value.GetItemSpecificClass();

                            }
                        }
                        if(charItem.Value.GetDamageMax() != "0" && !string.IsNullOrEmpty(charItem.Value.GetDamageMax())){
                            if(charItem.Value.GetEQUIPPEDSLOT() == "Off-Hand"){
                                duelWielding = true;
                                maxDmgOH = int.Parse(charItem.Value.GetDamageMax());
                                if(!string.IsNullOrEmpty(charItem.Value.GetAttackDelay()))
                                {
                                    attackDelayOH += float.Parse(charItem.Value.GetAttackDelay());
                                }
                                if(!string.IsNullOrEmpty(charItem.Value.GetPenetration()))
                                {
                                    penetrationOH = int.Parse(charItem.Value.GetPenetration());
                                }
                                if(!string.IsNullOrEmpty(charItem.Value.GetParry()))
                                {
                                    parry += int.Parse(charItem.Value.GetParry());
                                }
                            }
                        }
                    }
                    currentWeight = currentWeight + (float.Parse(charItem.Value.GetWeight()) * charItem.Value.GetAmount());
                }
                if(sofPresent){
                    SoFire = true;
                } else {
                    SoFire = false;
                }
                //print($"{SoFire} is what SoFire is set to on {CharacterName}");
                break;
            }
        }
        PLAYEREXPBONUS += StatAsset.Instance.CalculateExpBonus(SetBonusItems);
        print($"{PLAYEREXPBONUS} is our new Player exp bonus after armor set calculations!!!***************");
        if(_class == "Wyvern"){
            (float pen, float delay, float _parry) = StatAsset.Instance.GetWyvernStats(level, _CORE);
            attackDelay = delay;
            penetration = (int)pen;
            parry = (int)_parry;
        }
        if(attackDelay == 0){
            attackDelay = 60;
            weaponType = "Fist";
        }
        (int baseStrength, int baseAgility, int baseFortitude, int baseArcana) = StatAsset.Instance.GetCharacterStats(_class, level, _CORE);
        int PASSIVE_Agility = 0;
        int PASSIVE_Arcana = 0;
        int PASSIVE_Strength = 0;
        int PASSIVE_Fortitude = 0;
        int PASSIVE_Resist = 0;
        //if(pcTargetCheck.ClassType == "Fighter"){
		//		for(int _char = 0; _char < pcTargetCheck.assignedPlayer.GetInformationSheets().Count; _char++){
        //    		if(pcTargetCheck.assignedPlayer.GetInformationSheets()[_char].CharacterID == pcTargetCheck.CharID){
        //    		    for(int ability = 0; ability < pcTargetCheck.assignedPlayer.GetInformationSheets()[_char].CharSpellData.Count; ability++){
		//					if(pcTargetCheck.assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Key == "EastT3EndSkill"){
		//						HasRiposted = true;
		//						var abilityRankString = System.Text.RegularExpressions.Regex.Match(pcTargetCheck.assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
        //                		if (abilityRankString.Success) {
        //                		    riposteLvl = int.Parse(abilityRankString.Value); // Parse the rank number
		//							break;
        //                		}
		//					}
		//				}
		//				break;
		//			}
		//		}
		//	}
        for(int _char = 0; _char < assignedPlayer.GetInformationSheets().Count; _char++){
            if(assignedPlayer.GetInformationSheets()[_char].CharacterID == CharID){
                for(int ability = 0; ability < assignedPlayer.GetInformationSheets()[_char].CharSpellData.Count; ability++){
                    if(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Key == "EastT3BottomSkill" && ClassType == "Rogue"){
                        var abilityRankString = System.Text.RegularExpressions.Regex.Match(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                        if (abilityRankString.Success) {
                            int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                            float reductionPercent = 0.25f + (abilityRank * 0.25f);
                            // Convert the reduction percentage to a factor
                            float reductionFactor = (100f - reductionPercent) / 100f;
                            // Apply the reduction factor to the attack delay
                            attackDelay *= reductionFactor;
                            if(duelWielding)
                            attackDelayOH *= reductionFactor;

                        }
                    }
                    if(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Key == "EastT3EndSkill" && ClassType == "Fighter"){
						var abilityRankString = System.Text.RegularExpressions.Regex.Match(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                    	if (abilityRankString.Success) {
                    	    riposteLvl = int.Parse(abilityRankString.Value); // Parse the rank number
                            riposteChance += riposteLvl * .2f;
                    	}
					}
                    if(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Key == "WestT3TopSkill" && ClassType == "Wyvern"){
						var abilityRankString = System.Text.RegularExpressions.Regex.Match(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                    	if (abilityRankString.Success) {
                    	    doubleAttackLvl = int.Parse(abilityRankString.Value); // Parse the rank number
                            doubleAttackChance += doubleAttackLvl * .2f;
                    	}
					}
                    if(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Key == "WestT3TopSkill" && ClassType == "Druid"){
						var abilityRankString = System.Text.RegularExpressions.Regex.Match(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                    	if (abilityRankString.Success) {
                    	    int staffLevel = int.Parse(abilityRankString.Value); // Parse the rank number
                            StaffSteroid = 1 + ((staffLevel - 1) * .1f);
                    	}
					}
                    if(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Key == "EastT3TopSkill" && ClassType == "Wyvern"){
						var abilityRankString = System.Text.RegularExpressions.Regex.Match(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                    	if (abilityRankString.Success) {
                    	    criticalStrikeMeleeLvl = int.Parse(abilityRankString.Value); // Parse the rank number
                            criticalStrikeMeleeChance += 3f;
				            criticalStrikeMeleeChance += (criticalStrikeMeleeLvl * .2f);
                    	}
					}
                    if(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Key == "WestT3EndSkill" && ClassType == "Fighter" || assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Key == "WestT3EndSkill" && ClassType == "Rogue"
                    || assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Key == "WestT3EndSkill" && ClassType == "Paladin"){
						var abilityRankString = System.Text.RegularExpressions.Regex.Match(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                    	if (abilityRankString.Success) {
                    	    doubleAttackLvl = int.Parse(abilityRankString.Value); // Parse the rank number
                            doubleAttackChance += doubleAttackLvl * .2f;
                    	}
					}
					if(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Key == "WestT3BottomSkill" && ClassType == "Fighter"){
						var abilityRankString = System.Text.RegularExpressions.Regex.Match(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                    	if (abilityRankString.Success) {
                    	    criticalStrikeMeleeLvl = int.Parse(abilityRankString.Value); // Parse the rank number
                            criticalStrikeMeleeChance += 3f;
				            criticalStrikeMeleeChance += (criticalStrikeMeleeLvl * .2f);
                    	}
					}
                    if(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Key == "WestT2TopSkill" && ClassType == "Paladin"){
						var abilityRankString = System.Text.RegularExpressions.Regex.Match(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                    	if (abilityRankString.Success) {
                    	    criticalStrikeMeleeUndeadLvl = int.Parse(abilityRankString.Value); // Parse the rank number
                            criticalStrikeMeleeUndeadChance += 3f;
				            criticalStrikeMeleeUndeadChance += (criticalStrikeMeleeLvl * .2f);
                    	}
					}
                    if(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Key == "WestT2MiddleSkill" && ClassType == "Priest"){
						var abilityRankString = System.Text.RegularExpressions.Regex.Match(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                    	if (abilityRankString.Success) {
                    	    criticalStrikeHealSpellLvl = int.Parse(abilityRankString.Value); // Parse the rank number
                            criticalStrikeHealSpellChance += criticalStrikeHealSpellLvl * .1f;
                    	}
					}
                    if(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Key == "EastT2TopSkill" && ClassType == "Wizard"){
						var abilityRankString = System.Text.RegularExpressions.Regex.Match(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                    	if (abilityRankString.Success) {
                    	    criticalStrikeDmgSpellLvl = int.Parse(abilityRankString.Value); // Parse the rank number
                            criticalStrikeDmgSpellChance += criticalStrikeDmgSpellLvl * .1f;
                    	}
					}
                    if(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Key == "SouthT2LeftSkill"){
                        var abilityRankString = System.Text.RegularExpressions.Regex.Match(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                        if (abilityRankString.Success) {
                            int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                            PASSIVE_Strength = abilityRank;
                        }
                    }
                    if(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Key == "SouthT2MiddleSkill"){
                        var abilityRankString = System.Text.RegularExpressions.Regex.Match(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                        if (abilityRankString.Success) {
                            int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                            PASSIVE_Agility = abilityRank;
                        }
                    }
                    if(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Key == "SouthT2RightSkill"){
                        var abilityRankString = System.Text.RegularExpressions.Regex.Match(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                        if (abilityRankString.Success) {
                            int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                            PASSIVE_Resist = abilityRank;
                        }
                    }
                    if(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Key == "SouthT3LeftSkill"){
                        var abilityRankString = System.Text.RegularExpressions.Regex.Match(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                        if (abilityRankString.Success) {
                            int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                            PASSIVE_Fortitude = abilityRank;
                        }
                    }
                    if(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Key == "SouthT3RightSkill"){
                        var abilityRankString = System.Text.RegularExpressions.Regex.Match(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                        if (abilityRankString.Success) {
                            int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                            PASSIVE_Arcana = abilityRank;
                        }
                    }
                }
                break;
            }
        }
        MagicResist += PASSIVE_Resist;
        FireResist += PASSIVE_Resist;
        ColdResist += PASSIVE_Resist;
        DiseaseResist += PASSIVE_Resist;
        PoisonResist += PASSIVE_Resist;
        strength = strength + baseStrength + PASSIVE_Strength;
        agility = agility + baseAgility + PASSIVE_Agility;
        fortitude = fortitude + baseFortitude + PASSIVE_Fortitude;
        arcana = arcana + baseArcana + PASSIVE_Arcana;
        dodge = agility / 20;
        max_hp = fortitude;
        max_mp = arcana / 7;
		if(maxDmgMH == 0){
			maxDmgMH = 7 + (strength/ 20);
		}
		if(minDmgMH == 0){
			minDmgMH = 3;
		}
        if(_class == "Wyvern"){
            minDmgMH = 1;
            maxDmgMH = 3;
        }
        if(multiplayer)
        StartCoroutine(SetCombatPlayerName(player.playerName));
	}
    IEnumerator SetCombatPlayerName(string playersName){
        yield return new WaitForSeconds(5f);
        RpcGetPCName(playersName);
    }
    public int ApplyAgilityReduction(int agility, int strength, float carriedWeight)
    {
        float agilityReduction = CalculateAgilityReduction(strength, carriedWeight);
        float reducedAgility = agility * (1 - agilityReduction);
        return (int)reducedAgility;
    }
    public float CalculateAgilityReduction(int strength, float carriedWeight)
    {
        float maxCarryCapacity = strength; // 1 strength = 1 pound carry cap max
        float agilityReduction = 0f;

        if (carriedWeight > maxCarryCapacity * 1.5) // Over 150% max weight cap
        {
            agilityReduction = 0.75f; // 75% reduction
        }
        else if (carriedWeight > maxCarryCapacity * 1.25) // Over 125% max weight cap
        {
            agilityReduction = 0.5f; // 50% reduction
        }
        else if (carriedWeight > maxCarryCapacity) // Over 100% max weight cap
        {
            agilityReduction = 0.25f; // 25% reduction
        }

        return agilityReduction;
    }
    
	public void OnMouseEnter(){
		if (isServer)
    	{
    	    return;
    	}
        if(ScenePlayer.localPlayer.CheckForUIHIt()){
			return;
		}
    	    MouseOverCombat mouseOverBox = GameObject.Find("MouseOverCombat").GetComponent<MouseOverCombat>();
    	    Canvas mouseOverBoxCanvas = mouseOverBox.GetComponent<Canvas>();
    	    mouseOverBoxCanvas.enabled = true;
    	    mouseOverBox.InjectName(CharacterName);
    	    mouseOverBox.transform.position = Input.mousePosition + new Vector3(100, 100, 0);
            HoverNoise.Invoke();
    }
    	public void OnMouseExit(){

		if(isServer){
			return;
		}
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if(spriteRenderer != null){
            if(!spriteRenderer.enabled){
                return;
            }
        }
		MouseOverCombat mouseOverBox = GameObject.Find("MouseOverCombat").GetComponent<MouseOverCombat>();
       	Canvas mouseOverBoxCanvas = mouseOverBox.GetComponent<Canvas>();
	   	mouseOverBoxCanvas.enabled = false;
    	}
	    float finalRange;
    
    [Command]
    public void CmdCastAOESpell(string mode, Vector2 mousePosition){
        if(Casting){
			Casting = false;
        	RpcCancelCastAnimation();
		}
        
        string _spell = string.Empty;
        if(mode == CastingQ){
            _spell = SpellQ;
            if(SpellQCoolDown){
                return;
            }
        }
        if(mode == CastingE){
            _spell = SpellE;
            if(SpellECoolDown){
                return;
            }
        }
        if(mode == CastingR){
            _spell = SpellR;
            if(SpellRCoolDown){
                return;
            }
        }
        if(mode == CastingF){
            _spell = SpellF;
            if(SpellFCoolDown){
                return;
            }
        }
        var nameMatch = System.Text.RegularExpressions.Regex.Match(_spell, @"^\D*");
        string spell = nameMatch.Value.Trim(); // Trim any trailing spaces
        int _spellRank = 1;
        // Extract spell rank
        var rankMatch = System.Text.RegularExpressions.Regex.Match(_spell, @"\d+$");
        if (rankMatch.Success) {
            _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
        }
        int cost = StatAsset.Instance.GetSpellCost(spell);
        float cdReductionPercentage = 0;
        for(int _char = 0; _char < assignedPlayer.GetInformationSheets().Count; _char++){
            if(assignedPlayer.GetInformationSheets()[_char].CharacterID == CharID){
                for(int ability = 0; ability < assignedPlayer.GetInformationSheets()[_char].CharSpellData.Count; ability++){
                    if(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Key == "SouthT3EndSkill"){
                        var abilityRankString = System.Text.RegularExpressions.Regex.Match(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                        if (abilityRankString.Success) {
                            int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                            cdReductionPercentage = abilityRank * 0.2f;
                        }
                        break;
                    }
                }
                break;
            }
        }
        
        float cooldown = StatAsset.Instance.GetSpellCooldown(spell, _spellRank, cdReductionPercentage);
        if(cur_mp < cost){
			return;
		}
        //print($"about to set the cooldown time {cooldown} for {CharacterName} using {spell} and the mode is {mode}");
        if(mode == CastingQ){
            RunSetAbilityCooldownX(cooldown, false, "Q");
        }
        if(mode == CastingE){
            RunSetAbilityCooldownX(cooldown, false, "E");
        }
        if(mode == CastingR){
            RunSetAbilityCooldownX(cooldown, false, "R");
        }
        if(mode == CastingF){
            RunSetAbilityCooldownX(cooldown, false, "F");
        }
        bool hostile = !StatAsset.Instance.DetermineFriendly(spell);
        ProcessAOESpell(spell, _spellRank, cost, mousePosition, hostile);
    }
    
    [Command]
	public void CmdInstantCastSpell(string mode, MovingObject target){
        // all single target or instant cast spells
		if(Casting){
			Casting = false;
        	RpcCancelCastAnimation();
		}
        if(Invisibility){
		    ServerRemoveStatus("Stealthed", "Invisibility", true);
        }
        string _spell = string.Empty;
        if(mode == CastingQ){
            _spell = SpellQ;
            if(SpellQCoolDown){
                return;
            }
        }
        if(mode == CastingE){
            _spell = SpellE;
            if(SpellECoolDown){
                return;
            }
        }
        if(mode == CastingR){
            _spell = SpellR;
            if(SpellRCoolDown){
                return;
            }
        }
        if(mode == CastingF){
            _spell = SpellF;
            if(SpellFCoolDown){
                return;
            }
        }
        Vector3 movementDirection = target.transform.position - transform.position;
        //Vector2 direction = new Vector2(targetPosition.x - transform.position.x, targetPosition.y - transform.position.y);
        float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
        // Determine the direction string based on the angle
        string directionString = GetDirectionString(angle);
        Debug.Log("Facing direction: " + directionString);
		if(GetFacingDirection() != directionString){
            SetFacingDirection(directionString);
	    	RpcSetDirectionFacing(directionString);
	    }
	    //bool newRightFace = movementDirection.x >= 0;
	    //if (newRightFace != rightFace)
	    //{
	    //	rightFace = newRightFace;
	    //	RpcUpdateFacingDirection(newRightFace);
	    //}
        
        
        
        print($"Made it to CmdInstantCastSpell casting {_spell}");
        var nameMatch = System.Text.RegularExpressions.Regex.Match(_spell, @"^\D*");
        string spell = nameMatch.Value.Trim(); // Trim any trailing spaces
        int _spellRank = 1;
        // Extract spell rank
        var rankMatch = System.Text.RegularExpressions.Regex.Match(_spell, @"\d+$");
        if (rankMatch.Success) {
            _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
        }
        if(spell != "Resurrect"){
            if(target.Dying){
                TargetCannotCastOnDead();
                return;
            }
        }
        RpcCastingSpell(spell);
        
        float selfCast = StatAsset.Instance.SelfCasted(spell);
        bool selfCasted = false;
        if(selfCast == 1){
            selfCasted = true;
        }
        if(!StatAsset.Instance.InSpellRange(this, target, mode, new Vector2(), out float finalRange) && !selfCasted){
            print($"target {target.gameObject.name} was out of range of spell {_spell}");
            return;
        }
		int cost = StatAsset.Instance.GetSpellCost(spell);
        float cdReductionPercentage = 0;
        for(int _char = 0; _char < assignedPlayer.GetInformationSheets().Count; _char++){
            if(assignedPlayer.GetInformationSheets()[_char].CharacterID == CharID){
                for(int ability = 0; ability < assignedPlayer.GetInformationSheets()[_char].CharSpellData.Count; ability++){
                    if(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Key == "SouthT3EndSkill"){
                        var abilityRankString = System.Text.RegularExpressions.Regex.Match(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                        if (abilityRankString.Success) {
                            int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                            cdReductionPercentage = abilityRank * 0.2f;
                        }
                        break;
                    }
                }
                break;
            }
        }
        float cooldown = StatAsset.Instance.GetSpellCooldown(spell, _spellRank, cdReductionPercentage);
        if(cur_mp < cost){
			return;
		}
        //print($"about to set the cooldown time {cooldown} for {CharacterName} using {spell} and the mode is {mode}");
        if(mode == CastingQ){
            RunSetAbilityCooldownX(cooldown, false, "Q");
        }
        if(mode == CastingE){
            RunSetAbilityCooldownX(cooldown, false, "E");
        }
        if(mode == CastingR){
            RunSetAbilityCooldownX(cooldown, false, "R");
        }
        if(mode == CastingF){
            RunSetAbilityCooldownX(cooldown, false, "F");
        }
		ProcessSpellCast(mode, this, target, cost);
        if(spell != "Charge" && spell != "Dive"){
            //RpcCastingSpell(spell, null);
	        Mob mobTarget = target.GetComponent<Mob>();
	        PlayerCharacter charTarget = target.GetComponent<PlayerCharacter>();
	        string ownerName = string.Empty;
            string targetName = string.Empty;
               ownerName = CharacterName;
            if(charTarget){
               targetName = charTarget.CharacterName;
            } else {
	        	targetName = StatAsset.Instance.GetMobName(StatAsset.Instance.GetEnemyToPrefabMapping(), mobTarget.NAME);
            }
			//AddRpcCall(null, null, false, true, spell, null, false, ownerName, targetName);

        }

		//AOE
		//Aoe list enchanter GravityStun 5x5, ResistMagic 5x5
		//Aoe list priest turn undead 6x6, Groupheal 2x2
		//aoe list wizard IceBlast 3x3, IceSpear 1x3, FireBall 3x3, Meteor shower 6x6, Blizzard 4x4
		//aoe list archer 
		//aoe list fighter
		//aoe list rogue 
		//Single target
		//curatorTM.UpdatePlayerCastedDmgSpell();
		//Find the spell cost of each spell d
		//RpcAnimateSpell(spell, mousePosition);

	}
    
    [Command]
	public void CmdInstantCastSpellSelfCast(string mode, Vector3 target){
        // all single target or instant cast spells
		if(Casting){
			Casting = false;
        	RpcCancelCastAnimation();
		}
        if(Invisibility){
		    ServerRemoveStatus("Stealthed", "Invisibility", true);
        }
        string _spell = string.Empty;
        if(mode == CastingQ){
            _spell = SpellQ;
            if(SpellQCoolDown){
                return;
            }
        }
        if(mode == CastingE){
            _spell = SpellE;
            if(SpellECoolDown){
                return;
            }
        }
        if(mode == CastingR){
            _spell = SpellR;
            if(SpellRCoolDown){
                return;
            }
        }
        if(mode == CastingF){
            _spell = SpellF;
            if(SpellFCoolDown){
                return;
            }
        }
        Vector3 movementDirection = target - transform.position;
        //Vector2 direction = new Vector2(targetPosition.x - transform.position.x, targetPosition.y - transform.position.y);
        float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
        // Determine the direction string based on the angle
        string directionString = GetDirectionString(angle);
        Debug.Log("Facing direction: " + directionString);
        if(GetFacingDirection() != directionString){
            SetFacingDirection(directionString);
	    	RpcSetDirectionFacing(directionString);
	    }
	    //bool newRightFace = movementDirection.x >= 0;
	    //if (newRightFace != rightFace)
	    //{
	    //	rightFace = newRightFace;
	    //	RpcUpdateFacingDirection(newRightFace);
	    //}
        //if(!StatAsset.Instance.InSpellRange(this, target, mode)){
        //    print($"target {target.gameObject.name} was out of range of spell {_spell}");
        //    return;
        //}
        print($"Made it to CmdInstantCastSpellSelfCast casting {_spell}");
        var nameMatch = System.Text.RegularExpressions.Regex.Match(_spell, @"^\D*");
        string spell = nameMatch.Value.Trim(); // Trim any trailing spaces
        int _spellRank = 1;
        // Extract spell rank
        var rankMatch = System.Text.RegularExpressions.Regex.Match(_spell, @"\d+$");
        if (rankMatch.Success) {
            _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
        }
        RpcCastingSpell(spell);
		int cost = StatAsset.Instance.GetSpellCost(spell);
        float cdReductionPercentage = 0;
        for(int _char = 0; _char < assignedPlayer.GetInformationSheets().Count; _char++){
            if(assignedPlayer.GetInformationSheets()[_char].CharacterID == CharID){
                for(int ability = 0; ability < assignedPlayer.GetInformationSheets()[_char].CharSpellData.Count; ability++){
                    if(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Key == "SouthT3EndSkill"){
                        var abilityRankString = System.Text.RegularExpressions.Regex.Match(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                        if (abilityRankString.Success) {
                            int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                            cdReductionPercentage = abilityRank * 0.2f;
                        }
                        break;
                    }
                }
                break;
            }
        }
        float cooldown = StatAsset.Instance.GetSpellCooldown(spell, _spellRank, cdReductionPercentage);
        if(cur_mp < cost){
			return;
		}
        //print($"about to set the cooldown time {cooldown} for {CharacterName} using {spell} and the mode is {mode}");
        if(mode == CastingQ){
            RunSetAbilityCooldownX(cooldown, false, "Q");
        }
        if(mode == CastingE){
            RunSetAbilityCooldownX(cooldown, false, "E");
        }
        if(mode == CastingR){
            RunSetAbilityCooldownX(cooldown, false, "R");
        }
        if(mode == CastingF){
            RunSetAbilityCooldownX(cooldown, false, "F");
        }
        bool hostile = !StatAsset.Instance.DetermineFriendly(spell);
		ProcessAOESpell(spell, _spellRank, cost, target, hostile);
		//AOE
		//Aoe list enchanter GravityStun 5x5, ResistMagic 5x5
		//Aoe list priest turn undead 6x6, Groupheal 2x2
		//aoe list wizard IceBlast 3x3, IceSpear 1x3, FireBall 3x3, Meteor shower 6x6, Blizzard 4x4
		//aoe list archer 
		//aoe list fighter
		//aoe list rogue 
		//Single target
		//curatorTM.UpdatePlayerCastedDmgSpell();
		//Find the spell cost of each spell d
		//RpcAnimateSpell(spell, mousePosition);

	}
	[Command]
	public void CmdCastSpell(string mode, MovingObject target){
        // all single target or instant cast spells
		if(Casting){
			Casting = false;
        	RpcCancelCastAnimation();
		}
        string _spell = string.Empty;
        if(mode == CastingQ){
            _spell = SpellQ;
            if(SpellQCoolDown){
                return;
            }
        }
        if(mode == CastingE){
            _spell = SpellE;
            if(SpellECoolDown){
                return;
            }
        }
        if(mode == CastingR){
            _spell = SpellR;
            if(SpellRCoolDown){
                return;
            }
        }
        if(mode == CastingF){
            _spell = SpellF;
            if(SpellFCoolDown){
                return;
            }
        }
        Vector3 movementDirection = target.transform.position - transform.position;
        //Vector2 direction = new Vector2(targetPosition.x - transform.position.x, targetPosition.y - transform.position.y);
        float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
        // Determine the direction string based on the angle
        string directionString = GetDirectionString(angle);
        Debug.Log("Facing direction: " + directionString);
        if(GetFacingDirection() != directionString){
            SetFacingDirection(directionString);
	    	RpcSetDirectionFacing(directionString);
	    }
	    //bool newRightFace = movementDirection.x >= 0;
	    //if (newRightFace != rightFace)
	    //{
	    //	rightFace = newRightFace;
	    //	RpcUpdateFacingDirection(newRightFace);
	    //}
        if(!StatAsset.Instance.InSpellRange(this, target, mode, new Vector2(), out float finalRange)){
            print($"target {target.gameObject.name} was out of range of spell {_spell}");
            return;
        }
        print($"Made it to CmdCastSpell casting {_spell}");
        var nameMatch = System.Text.RegularExpressions.Regex.Match(_spell, @"^\D*");
        string spell = nameMatch.Value.Trim(); // Trim any trailing spaces
        int _spellRank = 1;
        // Extract spell rank
        var rankMatch = System.Text.RegularExpressions.Regex.Match(_spell, @"\d+$");
        if (rankMatch.Success) {
            _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
        }
        if(spell != "Resurrect"){
            if(target.Dying){
                TargetCannotCastOnDead();
                return;
            }
        }
		int cost = StatAsset.Instance.GetSpellCost(spell);
        float cdReductionPercentage = 0;
        for(int _char = 0; _char < assignedPlayer.GetInformationSheets().Count; _char++){
            if(assignedPlayer.GetInformationSheets()[_char].CharacterID == CharID){
                for(int ability = 0; ability < assignedPlayer.GetInformationSheets()[_char].CharSpellData.Count; ability++){
                    if(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Key == "SouthT3EndSkill"){
                        var abilityRankString = System.Text.RegularExpressions.Regex.Match(assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                        if (abilityRankString.Success) {
                            int abilityRank = int.Parse(abilityRankString.Value); // Parse the rank number
                            cdReductionPercentage = abilityRank * 0.2f;
                        }
                        break;
                    }
                }
                break;
            }
        }
        float cooldown = StatAsset.Instance.GetSpellCooldown(spell, _spellRank, cdReductionPercentage);
        if(cur_mp < cost){
			return;
		}
        //print($"about to set the cooldown time {cooldown} for {CharacterName} using {spell} and the mode is {mode}");
            if(mode == CastingQ){
                RunSetAbilityCooldownX(cooldown, false, "Q");
            }
            if(mode == CastingE){
                RunSetAbilityCooldownX(cooldown, false, "E");
            }
            if(mode == CastingR){
                RunSetAbilityCooldownX(cooldown, false, "R");
            }
            if(mode == CastingF){
                RunSetAbilityCooldownX(cooldown, false, "F");
            }
		ProcessSpellCast(mode, this, target, cost);
	}
	void BuildPlayerCharacterUICombat(PlayerCharacter player){
		if(player != this){
			return;
		}
		SpawnCombatPartyMemberPrefab();
        CombatPartyView.instance.AddOurUnits(this);
	}
    public GameObject SpawnCombatPartyMemberPrefab() {
		//print($"Building {CharacterName}'s combat UI build");
        CombatPartyMemberBuilt = Instantiate(CombatPartyMemberPrefab, CombatPartyView.instance.CombatPartyParent);
        CombatPartyMemberBuilt.GetComponent<CharacterCombatUI>().SetCharacter(this);
        //newUICombatCharacter.transform.SetSiblingIndex(player.playerIndex - 1);
        return CombatPartyMemberBuilt;
    }
    
	public void PlayCastSound(){
		AudioMgr sound = GetComponent<AudioMgr>();
		sound.PlayCastChantSound();
	}
    // Update is called once per frame
    protected override void Update()
    {
		base.Update();

    }
    //void OnTriggerEnter2D(Collider2D other)
    //{
    //    if(isServer){
    //        return;
    //    }
    //    PortalOVM portalOVM = other.gameObject.GetComponent<PortalOVM>();
    //    if(portalOVM){
    //        string exitLocation = portalOVM.GetNodePlusExit();
    //        ExitStringPass.Invoke(exitLocation);
    //        assignedPlayer.PortalOVM();
    //    }
    //    TutorialTip tip = other.gameObject.GetComponent<TutorialTip>();
    //    if(tip){
    //        
    //    }
    //}
    
    public void UpdateCombatUI(string stat, float duration, int value, string spellName, bool buff){
        //this is the new buff we want to build
        // its coming from playercharacter which came from turn manager
        // lets build the stamps here for the buffs and debuffs that wear off over time and fade out
        
        CombatPartyMemberBuilt.GetComponent<CharacterCombatUI>().ReceiveBuff(stat, duration, value, spellName, buff);
    }
    void SaveUnit(Match match){
		if(match == assignedMatch && !curatorTM.GetDuelMode()){
            CharacterSaveData savingGame = new CharacterSaveData(cur_hp, cur_mp, PLAYEREXPERIENCE, PLAYERCLASSPOINTS, CharID);
            //print($"Killing {CharacterName}, sending its data to be saved at server!! EXP {PLAYEREXPERIENCE} CP {PLAYERCLASSPOINTS} HP {cur_hp} MP {cur_mp}");
            SaveCharacter.Invoke(assignedPlayer.connectionToClient, savingGame);
			//Destroy(this.gameObject);
		}
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


    public void AskToMove(){

    }
}}
