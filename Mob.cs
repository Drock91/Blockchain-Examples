using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
using Mirror;
using PlayFab.ClientModels;
namespace dragon.mirror{
public class Mob : MovingObject
{
	[SerializeField] public int compendiumSerial;

    [SerializeField] GameObject castBarPrefab;
	[SyncVar]
	[SerializeField] public string NAME;
	public Match assignedMatch;
    [SyncVar]
	[SerializeField] public MovingObject activeTarget;
	[SerializeField] public float Vision = 5f;
	[SerializeField] public float Hearing = 5f;
	[SerializeField] public float Smell = 5f;
    [SyncVar]
	[SerializeField] public bool Resetting = false;
	[SerializeField] public bool InvisTrueSight;
	[SerializeField] public bool InvisUndeadTrueSight;
	[SerializeField] public bool SneakTrueSight;
	public Dictionary<MovingObject, float> threatList = new Dictionary<MovingObject, float>();
	public float MaxDistanceFromOrigin = 15f;
    //[SyncVar]
	[SerializeField] public string groupNumber;
    //[SyncVar]
	[SerializeField] public bool Aggro = false;
   // [SyncVar]
	[SerializeField] public bool Pulled = false;
	[SerializeField] public string MobClass;
	[SerializeField] CircleCollider2D RadiusDetectorCollider;

    [SerializeField] public static UnityEvent<MovingObject>  MobFogPosition = new UnityEvent<MovingObject>();
    [SerializeField] public static UnityEvent<MovingObject, Match>  MobDiedRemovePossibleTarget = new UnityEvent<MovingObject, Match>();
	[SyncVar]
	[SerializeField] public MovingObject charmedController;
	int StealAmount = 1;
	bool threatOn = false;
	Coroutine DamageRoutine;
	public void DamageOccured(){
		if(DamageRoutine != null){
			StopCoroutine(DamageRoutine);
			DamageRoutine = null;
		}
		DamageRoutine = StartCoroutine(DamageRoutineFull());
	}
	public float fadeDuration = 10f;
public float fadeToHalfDuration = 8f; // Duration to fade to 0.5
public float fadeToZeroDuration = 2f; // Duration to fade to 0

IEnumerator DamageRoutineFull()
{
    SpriteRenderer sRend = GetComponent<SpriteRenderer>();
    if (sRend)
    {
        if (!sRend.enabled)
        {
            DamageRoutine = null;
            yield break;
        }
    }

    if (healthBarTransformParent != null)
    {
        healthBarTransformParent.gameObject.SetActive(true);
    }
    if (magicPointBarTransformParent != null)
    {
        magicPointBarTransformParent.gameObject.SetActive(true);
    }

    CanvasGroup canvasGroupHP = healthBarTransformParent.gameObject.GetComponent<CanvasGroup>();
    CanvasGroup canvasGroupMP = magicPointBarTransformParent.gameObject.GetComponent<CanvasGroup>();
    float startTime = Time.time;
    canvasGroupHP.alpha = 1;
    canvasGroupMP.alpha = 1;

    // Fade from 1 to 0.5 over 8 seconds
    while (Time.time < startTime + fadeToHalfDuration)
    {
        float t = (Time.time - startTime) / fadeToHalfDuration;
        canvasGroupHP.alpha = Mathf.Lerp(1, 0.5f, t);
        canvasGroupMP.alpha = Mathf.Lerp(1, 0.5f, t);
        yield return null;
    }

    // Update startTime for the next phase
    startTime = Time.time;

    // Fade from 0.5 to 0 over 2 seconds
    while (Time.time < startTime + fadeToZeroDuration)
    {
        float t = (Time.time - startTime) / fadeToZeroDuration;
        canvasGroupHP.alpha = Mathf.Lerp(0.5f, 0, t);
        canvasGroupMP.alpha = Mathf.Lerp(0.5f, 0, t);
        yield return null;
    }

    canvasGroupHP.alpha = 0;
    canvasGroupMP.alpha = 0;
    healthBarTransformParent.gameObject.SetActive(false);
    magicPointBarTransformParent.gameObject.SetActive(false);

    DamageRoutine = null;
    yield break;
}
	public int GetStealAmount(){
		return StealAmount;
	}
	public bool CanSteal(){
		if(StealAmount > 0){
			StealAmount = 0;
			return true;
		} else {
			return false;
		}
	}
	public bool CanCast(){
		bool hasCD = false;
		if(!SpellQCoolDown){
			if(SpellQ != "None")
			hasCD = true;
		}
		if(!SpellECoolDown){
			if(SpellE != "None")
			hasCD = true;
		}
		if(!SpellRCoolDown){
			if(SpellR != "None")
			hasCD = true;
		}
		if(!SpellFCoolDown){
			if(SpellF != "None")
			hasCD = true;
		}
		return hasCD;
	}
	public bool DecideToCast(){
		//print($"Decide to cast is checking random check");
		float randomCheckAmount = UnityEngine.Random.Range(0f, 100f);

		//print($"Decide to cast is checking random check {randomCheckAmount} is amount");
		if(randomCheckAmount > 85f){
		//print($"{randomCheckAmount} is amount and passed");

			//figure out logic of class
			bool hasCD = false;
			if(!SpellQCoolDown){
				if(SpellQ != "None")
				hasCD = true;
			}
			if(!SpellECoolDown){
				if(SpellE != "None")
				hasCD = true;
			}
			if(!SpellRCoolDown){
				if(SpellR != "None")
				hasCD = true;
			}
			if(!SpellFCoolDown){
				if(SpellF != "None")
				hasCD = true;
			}
			if(hasCD){
		//print($"passed cd check in Decide to cast");

				return true;
			} else {
		//print($"failed cd check in Decide to cast");

				return false;
			}
		} else {
		//print($"{randomCheckAmount} is amount and failed");

			return false;
		}
	}
	
	public IMobState currentState;
    
	// State Interface and Classes
    public interface IMobState
    {
        void Execute(Mob mob, TurnManager turnManager);
    }
	public class DeathState : IMobState
    {
        public void Execute(Mob mob, TurnManager turnManager)
        {
		    // implement Death behavior here
			turnManager.RemoveMob(mob.assignedMatch, mob);
        }
    }
	public static Vector3 ModifyOriginRandomly(Vector3 origin)
    {
        // Randomly decide to modify the x, y, or both
        int choice = Random.Range(0, 3); // Returns 0, 1, or 2

        if (choice == 0) // Modify only x
        {
            origin.x += 0.25f;
        }
        else if (choice == 1) // Modify only y
        {
            origin.y += 0.25f;
        }
        else if (choice == 2) // Modify both x and y
        {
            origin.x += 0.25f;
            origin.y += 0.25f;
        }

        return origin;
    }
	public Vector3 ModifyOriginRandomlyNonStatic(Vector3 origin)
    {
        // Randomly decide to modify the x, y, or both
        int choice = Random.Range(0, 3); // Returns 0, 1, or 2

        if (choice == 0) // Modify only x
        {
            origin.x += 0.25f;
        }
        else if (choice == 1) // Modify only y
        {
            origin.y += 0.25f;
        }
        else if (choice == 2) // Modify both x and y
        {
            origin.x += 0.25f;
            origin.y += 0.25f;
        }

        return origin;
    }
   
    public class PatrolState : IMobState
    {
        public void Execute(Mob mob, TurnManager turnManager)
        {
			if(mob.Dying){
				return;
			}
			//print($"Running Execute on PatrolState");
		    // implement Patrol behavior here
        }
    }
	public class ResetState : IMobState
    {
        public void Execute(Mob mob, TurnManager turnManager)
        {
			if(mob.Dying){
				return;
			}
			mob.charmedController = null;
			mob.StopATTACKINGMob();
			mob.Resetting = true;
			mob.Aggro = false;
			mob.cur_hp = mob.max_hp;
			mob.cur_mp = mob.max_mp;
			mob.stamina = -100f;
			mob.Target = null;
			mob.threatList.Clear();
			mob.moving = true;
			mob.Stunned = false;
			mob.Snared = false;
			mob.Feared = false;
			mob.Mesmerized = false;
			mob.agent.isStopped = false;
			mob.agent.SetDestination(ModifyOriginRandomly(mob.Origin));
        }

    }
    public class EnemyActionState : IMobState
    {
        public void Execute(Mob mob, TurnManager turnManager)
        {
			if(mob.Dying){
				return;
			}
			if(mob.Resetting){
				return;
			}
			if(mob.GetATTACKING() != null){
				mob.StopATTACKINGMob();
			}
			bool failedToTarget = false;
				if(mob.Target == null || mob.FROZEN){
					failedToTarget = true;
				} else {
					if(mob.Target.FROZEN){
						failedToTarget = true;
					}
					if(mob.Target.Dying){
						failedToTarget = true;
					}
        			if(mob.Target.Hide && !mob.SneakTrueSight ){
						failedToTarget = true;
					}
					if(mob.Target.Sneak && !mob.SneakTrueSight){
						failedToTarget = true;
					}
					if(mob.Target.Invisibility && !mob.InvisTrueSight){
						failedToTarget = true;
					}
					if(mob.Target.InvisibilityUndead && !mob.InvisUndeadTrueSight){
						failedToTarget = true;
					}
				}
				if(!failedToTarget){
					
					mob.SetATTACKING(mob.Target, false);
				} else {
                	mob.TransitionToState(new OnGuardState(), "OnGuardState", mob.curatorTM); 
				}
        }
    }
	
	public class OnGuardState : IMobState
    {
		//Idle waiting
        public void Execute(Mob mob, TurnManager turnManager)
        {
			if(mob.Dying){
				return;
			}
			if(mob.Resetting){
				Vector2 mobV2 = new Vector2(mob.transform.position.x, mob.transform.position.y);
				// Check if mob is not standing on its point of origin (in the x and y directions).
				if (mobV2 != mob.Origin)
				{
					mob.ResettingMob();
				    // The mob is not on its origin point. You can put your logic here.
				} else {
					mob.Resetting = false;
					mob.Aggro = false;
					mob.Pulled = false;
				}
				return;
			}
        	MovingObject highestThreatPlayer = mob.GetHighestThreat();

			if(highestThreatPlayer != null){
				mob.Target = highestThreatPlayer;
				mob.StartCoroutine(mob.HandleAttackTarget());

			} else {
				//Lets ask for other mobs first to verify if we need to reset here
				print($"Resetting mob {mob.NAME} from guard position because our target is no longer able to be produced or we have no target");
				Dictionary<MovingObject, float> groupsList = new Dictionary<MovingObject, float>();
				groupsList = mob.curatorTM.GetGroupsThreatList(mob.groupNumber, mob);
				if(groupsList.Count > 0){
					mob.threatList = groupsList;
					MovingObject newHighThreat = mob.GetHighestThreat();

					if(newHighThreat != null){
					print($"Resetting mob {mob.NAME} from guard position found a new target! not fully resetting now");

						mob.Target = newHighThreat;
						mob.StartCoroutine(mob.HandleAttackTarget());
						return;
					}
				}
				mob.ResettingMob();
			}
        }
    }
	public void RemoveCharmedController(){
		charmedController = null;
	}
	public MovingObject GetCharmedController(){
		return charmedController;
	}
	public class ResState : IMobState
    {
        public void Execute(Mob mob, TurnManager turnManager)
        {
		    // implement Death behavior here
			turnManager.AddMob(mob.assignedMatch, mob);
			if(mob.GetDeathCountdown() != null){
				mob.ResStateDetected();
			}
            mob.TransitionToState(new OnGuardState(), "OnGuardState", mob.curatorTM); 
        }
    }
	public class CharmedState : IMobState
    {
        public void Execute(Mob mob, TurnManager turnManager)
        {
		    // implement Death behavior here
			if(mob.GetATTACKING() != null){
				mob.StopATTACKINGMob();
			}
			//mob.threatList.Clear();
        }
    }
	public class UncharmedState : IMobState
    {
		//Idle waiting
        public void Execute(Mob mob, TurnManager turnManager)
        {
			//mob.threatList.Clear();
			MovingObject puppetMaster = mob.GetCharmedController();
			if(puppetMaster){
				if(!mob.threatList.ContainsKey(puppetMaster))
				mob.threatList[puppetMaster] = 200f;
			}
        	MovingObject highestThreatPlayer = mob.GetHighestThreat();
			mob.RemoveCharmedController();
			if(highestThreatPlayer != null){
				mob.Target = highestThreatPlayer;
				mob.StartCoroutine(mob.HandleAttackTarget());

			} else {
				//Lets ask for other mobs first to verify if we need to reset here
				print($"Resetting mob {mob.NAME} from guard position because our target is no longer able to be produced or we have no target");
				Dictionary<MovingObject, float> groupsList = new Dictionary<MovingObject, float>();
				groupsList = mob.curatorTM.GetGroupsThreatList(mob.groupNumber, mob);
				if(groupsList.Count > 0){
					mob.threatList = groupsList;
					MovingObject newHighThreat = mob.GetHighestThreat();

					if(newHighThreat != null){
					print($"Resetting mob {mob.NAME} from guard position found a new target! not fully resetting now");

						mob.Target = newHighThreat;
						mob.StartCoroutine(mob.HandleAttackTarget());
						return;
					}
				}
				mob.ResettingMob();
			}
            // implement Enemy Action behavior here
			//print($"Running Execute on OnGuardState");

        }
    }
	public void CharmMobActivate(MovingObject charmingObject){
		charmedController = charmingObject;
		TransitionToState(new CharmedState(), "CharmedState", curatorTM);
	}
	public void CharmMobDeactivate(){
		StartCoroutine(PausedCharDeactivate());
	}
	IEnumerator PausedCharDeactivate(){
		yield return new WaitForSeconds(.1f);
		TransitionToState(new UncharmedState(), "UncharmedState", curatorTM);
		RpcCharmFaded();
	}
	public void ResMobMode(){
		TransitionToState(new ResState(), "ResState", curatorTM);
	}
	public void ExecuteCurrentState(TurnManager turnManager) {
	    if(currentState != null) {
	        currentState.Execute(this, turnManager);
	    }
	}
	public void TransitionToState(IMobState state, string mobState, TurnManager turnManager) {
        currentState = state;
		print($"{gameObject.name} transitioned to {mobState}");
		ExecuteCurrentState(turnManager);
    }
	//Transition states above
	protected override void Awake ()
    {
		if(isServer){
		}
		base.Awake();
    }
    protected override void Start()
    {
		if(isServer){
			UnStealthedChar.AddListener(ReAddToThreat);
		}
        base.Start();
    }
	public void SetMATCH(Match match){
		assignedMatch = match;
	}
	public override void OnStartServer()
	{	
		#if UNITY_SERVER
		PlayFabServer.ENDMATCHFULLY.AddListener(SelfDestruction);
		#endif
	}

	public override void OnStartClient()
	{	
		RadiusDetectorCollider.enabled = false;
		if (ctSliderTransformParent != null)
        {
            ctSliderTransformParent.gameObject.SetActive(false);
        }
		if (healthBarTransformParent != null)
        {
            healthBarTransformParent.gameObject.SetActive(false);
        }
        if (magicPointBarTransformParent != null)
        {
            magicPointBarTransformParent.gameObject.SetActive(false);
        }
	}
	
	//voided out
	void SelfDestruction(Match match){

		if(match == assignedMatch){
			Destroy(this.gameObject);
		}
	}
	protected override void Update()
    {
		base.Update();
    }
	void ReAddToThreat(MovingObject player, Match match){
		if(match == assignedMatch && !GetFriendly(player)){
			float _distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.y), 
        	new Vector2(player.transform.position.x, player.transform.position.y));
        	if (_distance <= Vision)
        	{
				if(HasLineOfSight(transform.position, player.transform.position)){
					if(!threatList.ContainsKey(player)){
						threatList.Add(player, 1f);
						if (!threatList.ContainsKey(player)){
        	    		    threatList.Add(player, 1f);
							for(int i = 0; i < curatorTM.MobGroups[groupNumber].Count; i++){
								if (curatorTM.MobGroups[groupNumber][i].Resetting || curatorTM.MobGroups[groupNumber][i].Dying){
									continue;
								}
								if(!GetFriendly(curatorTM.MobGroups[groupNumber][i])){
									continue;
								}
								if (!curatorTM.MobGroups[groupNumber][i].threatList.ContainsKey(player)){
        	    		    		curatorTM.MobGroups[groupNumber][i].threatList.Add(player, 1f);
								}
								if(!curatorTM.MobGroups[groupNumber][i].Target){
									curatorTM.MobGroups[groupNumber][i].DamageTaken();
								}
							}
						}
        	    		// Set the player as the target
        				MovingObject highestThreatPlayer = GetHighestThreat();
						if(highestThreatPlayer != null){
							if(Target == null){
								Target = highestThreatPlayer;
								StartCoroutine(HandleAttackTarget());
								return;
							} else {
								if(highestThreatPlayer != Target){
									Target = highestThreatPlayer;
									StartCoroutine(HandleAttackTarget());
									return;
								}
							}
						}
					}
				}
			}
		}
	}
	public bool CanReachTarget(GameObject target)
{
    NavMeshPath path = new NavMeshPath();
    bool agentOnNavMesh = IsOnNavMesh(agent.transform.position);
    bool targetOnNavMesh = IsOnNavMesh(target.transform.position);

    Debug.Log($"Agent on NavMesh: {agentOnNavMesh}, Target on NavMesh: {targetOnNavMesh}");

    if (agentOnNavMesh && targetOnNavMesh)
    {
		NavMeshHit hit;
        if (NavMesh.SamplePosition(agent.transform.position, out hit, 1.0f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }

        agent.CalculatePath(target.transform.position, path);

        switch (path.status)
        {
            case NavMeshPathStatus.PathComplete:
                Debug.Log($"Target was {target.name} and we are {NAME}, path COMPLETE we can walk to them");
                break;
            case NavMeshPathStatus.PathPartial:
                Debug.Log($"Target was {target.name} and we are {NAME}, path PARTIAL we can walk to them");
                break;
            case NavMeshPathStatus.PathInvalid:
                Debug.Log($"Target was {target.name} and we are {NAME}, path INVALID we can't walk to them");
                break;
        }

        return path.status == NavMeshPathStatus.PathComplete;
    }
    else
    {
        Debug.LogWarning("Either the agent or the target is not on the NavMesh.");
        return false;
    }
}

bool IsOnNavMesh(Vector3 position)
{
    NavMeshHit hit;
    return NavMesh.SamplePosition(position, out hit, 1.0f, NavMesh.AllAreas);
}
	public MovingObject GetHighestThreat(){
		float highestThreat = float.MinValue;
        MovingObject highestThreatPlayer = null;
        // Get the list of players from the threat dictionary.
        var players = new List<MovingObject>(threatList.Keys);
		List<MovingObject> removalList = new List<MovingObject>();
		for (int i = 0; i < players.Count; i++){
			if(players[i] == null){
				continue;
			}
			bool failedToTarget = false;
			if(players[i].FROZEN){
				failedToTarget = true;
			}
        	if(players[i].Hide && !SneakTrueSight ){
				failedToTarget = true;
			}
			if(players[i].Sneak && !SneakTrueSight){
				failedToTarget = true;
			}
			if(players[i].Invisibility && !InvisTrueSight){
				failedToTarget = true;
			}
			if(players[i].InvisibilityUndead && !InvisUndeadTrueSight){
				failedToTarget = true;
			}
			if(failedToTarget){
				removalList.Add(players[i]);
				continue;
			}
			if(players[i].Dying){
				removalList.Add(players[i]);
				continue;
			}
			if(GetFriendly(players[i])){
				removalList.Add(players[i]);
				continue;
			}
            // Get the threat for this player.
            float newThreat = threatList[players[i]];
            // Check if this threat is higher than the current highest threat.
            if (newThreat > highestThreat){
                // If so, update the highest threat and corresponding player.
                highestThreat = newThreat;
                highestThreatPlayer = players[i];
				//print($"{NAME} has a threat for {highestThreatPlayer.gameObject.name} for a threat value{highestThreat}");
            }
        }
		for (int i = 0; i < removalList.Count; i++){
			if(threatList.ContainsKey(removalList[i])){
				threatList.Remove(removalList[i]);
			}
		}
		if(highestThreatPlayer == null){
			for (int i = 0; i < curatorTM.MobGroups[groupNumber].Count; i++){
				if(curatorTM.MobGroups[groupNumber][i] == this){
					continue;
				}
				MovingObject possibleKillTarget = null;
				if(curatorTM.MobGroups[groupNumber][i].Target != null){
					possibleKillTarget = curatorTM.MobGroups[groupNumber][i].Target;
					if(possibleKillTarget.Dying){
						continue;
					}
					if(!GetFriendly(possibleKillTarget)){
						if(!threatList.ContainsKey(possibleKillTarget))
						threatList.Add(possibleKillTarget, 1f);
					}
				}
				if(possibleKillTarget != null){
					highestThreatPlayer = possibleKillTarget;
					break;
				}
			}
		}
		//if(highestThreatPlayer != null){
		//	CanReachTarget(highestThreatPlayer.gameObject);
		//}
		//print($"{highestThreatPlayer} is {NAME} highest threat for {highestThreat}");
		return highestThreatPlayer;
	}
	IEnumerator HandleAttackTarget()
	{
		yield return new WaitForSeconds(0.5f);
		if(Mesmerized || Stunned || Feared){
			yield break;
		}
		TransitionToState(new EnemyActionState(), "EnemyActionState", curatorTM);
	}
	/*

	[Server]
	public Vector3 CheckForStacking(MovingObject target, float attackRange){
		List<Mob> matchMobListCheck = curatorTM.GetENEMYList();
		List<PlayerCharacter> matchPlayerCharacterListCheck = curatorTM.GetPCList();
		List<Vector3> DONOTUSELIST = new List<Vector3>();
		float _distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.y), 
        new Vector2(target.transform.position.x, target.transform.position.y));
        if (_distance <= .55f)
        {
			DONOTUSELIST.Add(target.transform.position);
		}
    	foreach (var potentialStackingMember in matchMobListCheck)
    	{
			if (potentialStackingMember == null || potentialStackingMember.Dying || potentialStackingMember == this)
        	{
        	    continue; // Skip null, dying mobs, and the current mob itself
        	}

        	float distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.y), 
        	new Vector2(potentialStackingMember.transform.position.x, potentialStackingMember.transform.position.y));
        	if (distance <= .55f)
        	{
				DONOTUSELIST.Add(potentialStackingMember.transform.position);
        	    //print($"Other mob's name was {potentialStackingMember.NAME} and our name was {NAME}");
        	}
    	}
		Vector3 newPosition = Vector3.zero;
    	// If there are too many mobs near the target, find a new position
    	if (DONOTUSELIST.Count > 0) // Define SomeThreshold based on your game's mechanics
    	{
    	    newPosition = FindNewPosition(target.transform.position, attackRange, DONOTUSELIST);
    	}
		//print($"returning new position {newPosition}");
		return newPosition;
	} 
	// Method to find a new position around the target within the attack range
	Vector3 FindNewPosition(Vector3 targetPosition, float range, List<Vector3> DONOTUSELIST){
		if(!DONOTUSELIST.Contains(targetPosition)){
		    DONOTUSELIST.Add(targetPosition);
		}
	    const int attempts = 30; // Number of attempts to find a suitable position
	    agent = GetComponent<NavMeshAgent>();
	    for (int i = 0; i < attempts; i++)
	    {
	        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * range;
	        randomDirection += targetPosition;
	        NavMeshHit hit;
	        if (NavMesh.SamplePosition(randomDirection, out hit, range, agent.areaMask)) // Use agent.areaMask to match the agent's NavMesh area
	        {
	            // Check if the position is not too close to other mobs
            	bool tooClose = DONOTUSELIST.Any(donotUsePosition => 
            	    Vector3.Distance(hit.position, donotUsePosition) <= (donotUsePosition == targetPosition ? 0.8f : 0.6f));
            	if (!tooClose)
            	{
            	    // Found a suitable position that's not too close to DONOTUSELIST positions
            	    return hit.position;
            	}
	        }
	    }
	    return Vector3.zero; // No suitable position found
	}
	*/

	public void ResetCheckForNearByTargetsOrReset(){
		if(isServer && !Dying){
			Resetting = false;
        	// Check if the entering collider is a PlayerCharacter
			List<PlayerCharacter> pcList = curatorTM.GetPCList();
			List<PlayerCharacter> newThreatList = new List<PlayerCharacter>();
			for(int i = 0; i < pcList.Count; i++){
				if(pcList[i].Dying){
					continue;
				}
				float _distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.y), 
        		new Vector2(pcList[i].transform.position.x, pcList[i].transform.position.y));
        		if (_distance <= Vision)
        		{
					if(!HasLineOfSight(this.transform.position, pcList[i].transform.position)){
						print($"Does NOT have line of sight of {pcList[i].CharacterName}");
						continue;
					} else {
						print($"Does have line of sight of {pcList[i].CharacterName}");
					}
					newThreatList.Add(pcList[i]);
				}
			}
			for(int u = 0; u < newThreatList.Count; u++){
				if (!threatList.ContainsKey(newThreatList[u])){
        	        threatList.Add(newThreatList[u], 1f);
				}
			}
			for(int p = 0; p < curatorTM.MobGroups[groupNumber].Count; p++){
				if(curatorTM.MobGroups[groupNumber][p] == this){
					continue;
				}
				for(int v = 0; v < curatorTM.MobGroups[groupNumber][p].threatList.Count; v++){
					var threatItem = curatorTM.MobGroups[groupNumber][p].threatList.ElementAt(v);

					if(!threatList.ContainsKey(threatItem.Key)){
						threatList.Add(threatItem.Key, threatItem.Value);
					}
				}
			}
        	// Set the player as the target
        	MovingObject highestThreatPlayer = GetHighestThreat();
			if(highestThreatPlayer != null){
				if(Target == null){
					Target = highestThreatPlayer;
					TransitionToState(new EnemyActionState(), "EnemyActionState", curatorTM);
					return;
				} else {
					if(highestThreatPlayer != Target){
						Target = highestThreatPlayer;
						TransitionToState(new EnemyActionState(), "EnemyActionState", curatorTM);
						return;
					}
				}
			} else {
				ResettingMob();
				//TransitionToState(new ResetState(), "ResetState", curatorTM);
			}

        }
    }
	[Server]
	public void ResetCheckForNearByTargets(){
		if(isServer && !Dying){
			print($"Called ResetCheck to turn off reset and pulled on mob {NAME}");
			Resetting = false;
			Pulled = false;
			ServerResetTimer();
        	// Check if the entering collider is a PlayerCharacter
			List<PlayerCharacter> pcList = curatorTM.GetPCList();
			List<PlayerCharacter> newThreatList = new List<PlayerCharacter>();
			for(int i = 0; i < pcList.Count; i++){
				if(pcList[i].Dying){
					continue;
				}
				if(GetFriendly(pcList[i])){
					continue;
				}
				float _distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.y), 
        		new Vector2(pcList[i].transform.position.x, pcList[i].transform.position.y));
        		if (_distance <= Vision)
        		{
					if(!HasLineOfSight(this.transform.position, pcList[i].transform.position)){
						print($"Does NOT have line of sight of {pcList[i].CharacterName}");
						continue;
					} else {
						print($"Does have line of sight of {pcList[i].CharacterName}");
					}
					newThreatList.Add(pcList[i]);
				}
			}
			for(int u = 0; u < newThreatList.Count; u++){
				if (!threatList.ContainsKey(newThreatList[u])){
        	        threatList.Add(newThreatList[u], 1f);
				}
			}
        	// Set the player as the target
        	MovingObject highestThreatPlayer = GetHighestThreat();
			if(highestThreatPlayer != null){
				if(Target == null){
					Target = highestThreatPlayer;
					TransitionToState(new EnemyActionState(), "EnemyActionState", curatorTM);
					return;
				} else {
					if(highestThreatPlayer != Target){
						Target = highestThreatPlayer;
						TransitionToState(new EnemyActionState(), "EnemyActionState", curatorTM);
						return;
					}
				}
			}
        }
    }
	public void DamageTaken(){
		if(Mesmerized || Stunned || Feared){
			return;
		}
		MovingObject newTarget = GetHighestThreat();
		if(newTarget){
			Target = newTarget;
		}
		StartCoroutine(HandleAttackTarget());
	}
	public void ResettingMob(){
		
		curatorTM.ResetMobs(this);
	}
	[Server]
	public void ActivateThreatSystem(){
		StartCoroutine(StartActivateThreatSystem());
	}
	public IEnumerator StartActivateThreatSystem(){
        yield return new WaitForSeconds(2f);
		threatOn = true;
	}
	
	[Server]
	public void EnergySpark(MovingObject obj, string agentType){
		if(obj == this){
			//currentState = new OnGuardState();
			StartCoroutine(EnergyUpdaterMob(agentType));
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
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance; 
        agent.enabled = true;
    }
        public IEnumerator EnergyUpdaterMob(string agentType) {
			float rechargeTime = .5f;
			float haste = 0f;
			Energized = true;
	        RpcReadyToPlay(false);
        	StartCoroutine(SendRpcQueue());
        	stamina = 0f;
			int dodgeC = 0;
			dodgeC = dodge;
	        StartCoroutine(StartNavMesh(agentType));
			SetStatsServer();
            while (Energized && !Dying) {
                if (!Casting && !Mesmerized && !Stunned && !Feared && !Resetting) {
					if(GetAgility() > 101){
						haste = Mathf.Floor((GetAgility() - 100) / 2);
					} else {
						haste = 0f;
					}
					rechargeTime = .5f / (1f + (haste / 100f));
					stamina -= 5f;
        			stamina = Mathf.Clamp(stamina, -100f, 250f);
                    //if (stamina == -100f && SwitchFlip) {
                    //    if (PatrolPath == null) {
                    //        if (threatList.Count > 0 && !Dying){
                    //            SwitchFlip = false;
                    //            TransitionToState(new EnemyActionState(), "EnemyActionState", curatorTM); // New EnemyActionState
                    //        }
                    //    } else {
                    //        TransitionToState(new PatrolState(), "PatrolState", curatorTM); // New PatrolState
                    //    }
                    //}
                    yield return new WaitForSeconds(rechargeTime);
                } else {
                    yield return null;
                }
            }
        }
	[ClientRpc]
	public void ClientMobFog(){
		MobFogPosition.Invoke(this);
	}
	public void AddStaminaMob(float delay){
		if(stamina + delay > 250f){
			stamina = 250f;
		} else {
        	stamina += delay;
		}
    }
	
    [Server]
    public void Die()
    {
		Dying = true;
		Aggro = false;
		StopATTACKINGMob();
		StopCASTINGTOMOVE();
		Target = null;
		CircleCollider2D[] circleColliders = GetComponents<CircleCollider2D>();
    	foreach (CircleCollider2D circleCollider in circleColliders)
    	{
    	    circleCollider.enabled = false;
    	}
		//Energized = false;
		moving = false;
		agent.isStopped = true;
		agent.enabled = false;
		// If the agent has CircleColliders, disable them
		//RpcSpawnDeath();
        TransitionToState(new DeathState(), "DeathState", curatorTM); // New ResetState
    }
	
	[ClientRpc]
	void RpcRemoveMobCast(){
		CancelCast.Invoke(this);
	}
	[ClientRpc]
	public void RpcMobCasting(float castTime, string spell){
		GameObject castBarObject = Instantiate(castBarPrefab, new Vector3(transform.position.x, transform.position.y - .8f, 0f) , Quaternion.identity);
        CastbarEnemy castbar = castBarObject.GetComponent<CastbarEnemy>();
        castbar.SetMob(this, castTime, spell);
	}
	Coroutine deathCountDown;
	public Coroutine GetDeathCountdown(){
		return deathCountDown;
	}
	public void ResStateDetected(){
		if(deathCountDown != null){
			StopCoroutine(deathCountDown);
		}
	}
	[Server]
	public IEnumerator ProcessDeath(bool duelArena){
		float randomCheckAmount = UnityEngine.Random.Range(.1f, 1f);
		yield return new WaitForSeconds(randomCheckAmount);
		//if(!duelArena)
		//StartCoroutine(WaitForDeathDisplay());
		yield return new WaitForSeconds(.1f);
		if(deathCountDown != null){
			StopCoroutine(deathCountDown);
		}
		deathCountDown = StartCoroutine(WaitTwoMinutes());
		
	}
	IEnumerator WaitForDeathDisplay(){
		float randomCheckAmount = UnityEngine.Random.Range(.1f, 1f);
		yield return new WaitForSeconds(randomCheckAmount);
		RpcMobEXPCPDisplay(CLASSPOINTS, (int)EXPERIENCE, TIER, mobType);

	}
	IEnumerator WaitTwoMinutes(){
		yield return new WaitForSeconds(120);
		Destroy(this);
	}
	public void CheckHoverObject(){
		if(hoveringOver){
			hoveringOver = false;
			this.transform.GetComponent<SpriteRenderer>().color = new Color32(255,255,255,255);
			MouseOverCombat mouseOverBox = GameObject.Find("MouseOverCombat").GetComponent<MouseOverCombat>();
			if(!mouseOverBox){
				return;
			}
    		Canvas mouseOverBoxCanvas = mouseOverBox.GetComponent<Canvas>();
			mouseOverBoxCanvas.enabled = false;
		}
	}
	
	 public void OnMouseEnter(){
		if (isServer)
    	{
    	    return;
    	}
		if(ScenePlayer.localPlayer.CheckForUIHIt()){
			return;
		}
		if(Dying){
	 		hoveringOver = false;
			return;
		}
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if(spriteRenderer != null){
            if(!spriteRenderer.enabled){
                return;
            }
        }
	 	hoveringOver = true;
		this.transform.GetComponent<SpriteRenderer>().color = new Color32 (208,70,72, 255);
    	MouseOverCombat mouseOverBox = GameObject.Find("MouseOverCombat").GetComponent<MouseOverCombat>();
		if(!mouseOverBox){
			return;
		}
		SpriteRenderer sRend = GetComponent<SpriteRenderer>();
        if(sRend){
            if(!sRend.enabled){
				return;
            }
        }
    	Canvas mouseOverBoxCanvas = mouseOverBox.GetComponent<Canvas>();
    	mouseOverBoxCanvas.enabled = true;
		//if(DamageRoutine != null){
		//	StopCoroutine(DamageRoutine);
		//	DamageRoutine = null;
		//}
		//
		//CanvasGroup canvasGroupHP = healthBarTransformParent.gameObject.GetComponent<CanvasGroup>();
		//CanvasGroup canvasGroupMP = magicPointBarTransformParent.gameObject.GetComponent<CanvasGroup>();
		//canvasGroupHP.alpha = 1;
        //canvasGroupMP.alpha = 1;
		//if (healthBarTransformParent != null)
        //{
        //    healthBarTransformParent.gameObject.SetActive(true);
        //}
        //if (magicPointBarTransformParent != null)
        //{
        //    magicPointBarTransformParent.gameObject.SetActive(true);
        //}
		string mobName = StatAsset.Instance.IdentifiedBefore(compendiumSerial);
    	mouseOverBox.InjectName(mobName);
    	mouseOverBox.transform.position = Input.mousePosition + new Vector3(100, 100, 0);
        HoverNoise.Invoke();
    }
    public void OnMouseExit(){
		if(isServer){
			return;
		}
	 	hoveringOver = false;
		this.transform.GetComponent<SpriteRenderer>().color = new Color32(255,255,255,255);
		MouseOverCombat mouseOverBox = GameObject.Find("MouseOverCombat").GetComponent<MouseOverCombat>();
		if(!mouseOverBox){
			return;
		}
    	Canvas mouseOverBoxCanvas = mouseOverBox.GetComponent<Canvas>();
		mouseOverBoxCanvas.enabled = false;
		//if (healthBarTransformParent != null)
        //{
        //    healthBarTransformParent.gameObject.SetActive(false);
        //}
        //if (magicPointBarTransformParent != null)
        //{
        //    magicPointBarTransformParent.gameObject.SetActive(false);
        //}

		SpriteRenderer sRend = GetComponent<SpriteRenderer>();
        if(sRend){
            if(!sRend.enabled){
				return;
            }
        }

		//if(DamageRoutine != null){
		//	StopCoroutine(DamageRoutine);
		//	DamageRoutine = null;
		//}
		//DamageRoutine = StartCoroutine(DamageRoutineFull());
    }
	public void OnTriggerEnter2D(Collider2D other){
		if (other == null){
    	    //Debug.LogError("Collider is null");
    	    return;
    	}
		MovingObject moCheck = other.GetComponent<MovingObject>();
		if (moCheck == null){
    	    return;
    	}
		if(isServer && !Dying && !Resetting && !Pulled && Energized && threatOn){
        	// Check if the entering collider is a PlayerCharacter
        	
        	PlayerCharacter player = moCheck.GetComponent<PlayerCharacter>();
        	if (moCheck != null){
				if(GetFriendly(moCheck)){
					return;
				}
				if(moCheck.Dying){
					return;
				}
				if(moCheck.Hide && !SneakTrueSight ){
					return;
				}
				if(moCheck.Sneak && !SneakTrueSight){
					return;
				}
				if(moCheck.Invisibility && !InvisTrueSight){
					return;
				}
				if(moCheck.InvisibilityUndead && !InvisUndeadTrueSight){
					return;
				}
        		if (player != null){
					print($"Investigating {player.CharacterName} for this trigger enter check");
				}
				if(GetFriendly(moCheck)){
					return;
				}
				if(!HasLineOfSight(this.transform.position, moCheck.transform.position)){
					if (player != null){
						//print($"Does NOT have line of sight of {player.CharacterName}");
					}
					return;
				} else {
					if (player != null){
						//print($"Does have line of sight of {player.CharacterName}");
					}
				}
				Pulled = true;
				if (!threatList.ContainsKey(moCheck)){
        	        threatList.Add(moCheck, 1f);
					for(int i = 0; i < curatorTM.MobGroups[groupNumber].Count; i++){
						if (curatorTM.MobGroups[groupNumber][i].Resetting || curatorTM.MobGroups[groupNumber][i].Dying){
							continue;
						}
						if(curatorTM.MobGroups[groupNumber][i] == this){
							continue;
						}
						if(curatorTM.MobGroups[groupNumber][i].GetFriendly(moCheck)){
							continue;
						}
						curatorTM.MobGroups[groupNumber][i].Pulled = true;
						if (!curatorTM.MobGroups[groupNumber][i].threatList.ContainsKey(moCheck)){
        	        		curatorTM.MobGroups[groupNumber][i].threatList.Add(moCheck, 1f);
						}
						if(!curatorTM.MobGroups[groupNumber][i].Target){
							curatorTM.MobGroups[groupNumber][i].DamageTaken();
						}
					}
				}
        	    // Set the player as the target
        		MovingObject highestThreatPlayer = GetHighestThreat();
				if(highestThreatPlayer != null){
					if(Target == null){
						Target = highestThreatPlayer;
						StartCoroutine(HandleAttackTarget());
						return;
					} else {
						if(highestThreatPlayer != Target){
							Target = highestThreatPlayer;
							StartCoroutine(HandleAttackTarget());
							return;
						}
					}
				}
			}
        }
    }
	public void OnTriggerStay2D(Collider2D other){
		if (other == null){
    	    //Debug.LogError("Collider is null");
    	    return;
    	}
		MovingObject moCheck = other.GetComponent<MovingObject>();
		if (moCheck == null){
    	    return;
    	}
		if(isServer && !Dying && !Resetting && !Pulled && Energized && threatOn){
        	// Check if the entering collider is a PlayerCharacter
        	PlayerCharacter player = moCheck.GetComponent<PlayerCharacter>();
        	if (moCheck != null){
				if(GetFriendly(moCheck)){
					return;
				}
				if(moCheck.Dying){
					return;
				}
				if(moCheck.Hide && !SneakTrueSight ){
					return;
				}
				if(moCheck.Sneak && !SneakTrueSight){
					return;
				}
				if(moCheck.Invisibility && !InvisTrueSight){
					return;
				}
				if(moCheck.InvisibilityUndead && !InvisUndeadTrueSight){
					return;
				}
        		if (player != null){
					//print($"Investigating {player.CharacterName} for this trigger enter check");
				}
				if(GetFriendly(moCheck)){
					return;
				}
				if(!HasLineOfSight(this.transform.position, moCheck.transform.position)){
					if (player != null){
						//print($"Does NOT have line of sight of {player.CharacterName}");
					}
					return;
				} else {
					if (player != null){
						//print($"Does have line of sight of {player.CharacterName}");
					}
				}
				Pulled = true;
				if (!threatList.ContainsKey(moCheck)){
        	        threatList.Add(moCheck, 1f);
					for(int i = 0; i < curatorTM.MobGroups[groupNumber].Count; i++){
						if (curatorTM.MobGroups[groupNumber][i].Resetting || curatorTM.MobGroups[groupNumber][i].Dying){
							continue;
						}
						if(curatorTM.MobGroups[groupNumber][i] == this){
							continue;
						}
						if(curatorTM.MobGroups[groupNumber][i].GetFriendly(moCheck)){
							continue;
						}
						curatorTM.MobGroups[groupNumber][i].Pulled = true;
						if (!curatorTM.MobGroups[groupNumber][i].threatList.ContainsKey(moCheck)){
        	        		curatorTM.MobGroups[groupNumber][i].threatList.Add(moCheck, 1f);
						}
						if(!curatorTM.MobGroups[groupNumber][i].Target){
							curatorTM.MobGroups[groupNumber][i].DamageTaken();
						}
					}
				}
        	    // Set the player as the target
        		MovingObject highestThreatPlayer = GetHighestThreat();
				if(highestThreatPlayer != null){
					if(Target == null){
						Target = highestThreatPlayer;
						StartCoroutine(HandleAttackTarget());
						return;
					} else {
						if(highestThreatPlayer != Target){
							Target = highestThreatPlayer;
							StartCoroutine(HandleAttackTarget());
							return;
						}
					}
				}
			}
        }
    }
	public void OnTriggerExit2D(Collider2D other){
		if (other == null){
    	    //Debug.LogError("Collider is null");
    	    return;
    	}
		MovingObject moCheck = other.GetComponent<MovingObject>();
		if (moCheck == null){
    	    return;
    	}
		if(isServer && !Dying && !Resetting && !Pulled && Energized && threatOn){
        	// Check if the entering collider is a PlayerCharacter
        	PlayerCharacter player = moCheck.GetComponent<PlayerCharacter>();
        	if (moCheck != null){
				if(GetFriendly(moCheck)){
					return;
				}
				if(moCheck.Dying){
					return;
				}
				if(moCheck.Hide && !SneakTrueSight ){
					return;
				}
				if(moCheck.Sneak && !SneakTrueSight){
					return;
				}
				if(moCheck.Invisibility && !InvisTrueSight){
					return;
				}
				if(moCheck.InvisibilityUndead && !InvisUndeadTrueSight){
					return;
				}
        		if (player != null){
					//print($"Investigating {player.CharacterName} for this trigger enter check");
				}
				if(GetFriendly(moCheck)){
					return;
				}
				if(!HasLineOfSight(this.transform.position, moCheck.transform.position)){
					if (player != null){
						//print($"Does NOT have line of sight of {player.CharacterName}");
					}
					return;
				} else {
					if (player != null){
						//print($"Does have line of sight of {player.CharacterName}");
					}
				}
				Pulled = true;
				if (!threatList.ContainsKey(moCheck)){
        	        threatList.Add(moCheck, 1f);
					for(int i = 0; i < curatorTM.MobGroups[groupNumber].Count; i++){
						if (curatorTM.MobGroups[groupNumber][i].Resetting || curatorTM.MobGroups[groupNumber][i].Dying){
							continue;
						}
						if(curatorTM.MobGroups[groupNumber][i] == this){
							continue;
						}
						if(curatorTM.MobGroups[groupNumber][i].GetFriendly(moCheck)){
							continue;
						}
						curatorTM.MobGroups[groupNumber][i].Pulled = true;
						if (!curatorTM.MobGroups[groupNumber][i].threatList.ContainsKey(moCheck)){
        	        		curatorTM.MobGroups[groupNumber][i].threatList.Add(moCheck, 1f);
						}
						if(!curatorTM.MobGroups[groupNumber][i].Target){
							curatorTM.MobGroups[groupNumber][i].DamageTaken();
						}
					}
				}
        	    // Set the player as the target
        		MovingObject highestThreatPlayer = GetHighestThreat();
				if(highestThreatPlayer != null){
					if(Target == null){
						Target = highestThreatPlayer;
						StartCoroutine(HandleAttackTarget());
						return;
					} else {
						if(highestThreatPlayer != Target){
							Target = highestThreatPlayer;
							StartCoroutine(HandleAttackTarget());
							return;
						}
					}
				}
			}
        }
    }
}
}