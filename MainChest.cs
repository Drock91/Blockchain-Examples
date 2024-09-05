using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Mirror;
namespace dragon.mirror{
public class MainChest : NetworkBehaviour, IPointerExitHandler, IPointerEnterHandler
{
    Match assignedMatch;
    [SerializeField] Animator animator;
    [SerializeField] Sprite openedChestSprite;
    [SerializeField] SpriteRenderer sRend;
    [SerializeField] List<Mob> AttachedGroup;
    [SyncVar]
    [SerializeField] public bool Full = true;
    [SerializeField] public static UnityEvent<string> ImproperCheckText = new UnityEvent<string>();
    [SerializeField] public static UnityEvent SendAmount = new UnityEvent();
    [SerializeField] public static UnityEvent<int> Speaker = new UnityEvent<int>();
    public static UnityEvent HoverNoise = new UnityEvent();
    private string NAME = "Main chest";
    [SerializeField] GameObject PopUpTextPrefab;
    int tier = 1;
    public int GetTier(){
        return tier;
    }
    private object chestLock = new object();

    [Server]
    public void SetOpened(){
        lock (chestLock)
        {
            Full = false;
            RpcOpenAnimator();
        }
        //RpcOpenAnimator();
    }
    bool notSent = true;
    [Server]
    public void SendChestAmount(float amount){
        if(notSent){
            notSent = false;
            print($"Sending main chest was asked");
            RpcSendAmount(amount);
        }
    }
    [ClientRpc]
    void RpcSendAmount(float amount){
        print($"Sending main chest was asked");
        GameObject spawnTextPopUp = Instantiate(PopUpTextPrefab, transform.position, Quaternion.identity);
        AbilityPopUp abilityDisplay = spawnTextPopUp.GetComponent<AbilityPopUp>();
        abilityDisplay.GoldPopUp(amount);
        Speaker.Invoke(450);
    }
    [ClientRpc]
    void RpcOpenAnimator(){
        animator.SetBool("IsOpened", true);
        SendAmount.Invoke();
    }
    [Server]
    public void SetMatch(Match match, int _tier){
        tier = _tier;
        assignedMatch = match;
    }
    [ClientRpc]
    void RpcSaveList(List<Mob> _AttachedGroup){
        AttachedGroup = _AttachedGroup;
    }
    IEnumerator SetChestTier(){
        yield return new WaitForSeconds(6f);
        RpcSetUpMCAnimator(tier);
        RpcSaveList(AttachedGroup);
    }
    [ClientRpc]
    void RpcSetUpMCAnimator(int _tier){
        if(_tier <= 0){
            _tier = 1;
        }
        animator.SetInteger("Tier", _tier);
    }
    [Server]
    public void FillOutMainChest(List<Mob> mobs, Match match){
        if(match == assignedMatch){
            AttachedGroup = mobs;
            StartCoroutine(SetChestTier());
        }
    }
    public void TryToOpen(){
        if(ScenePlayer.localPlayer.Combat){
            ImproperCheckText.Invoke($"Cannot interact until out of combat");
            return;
        }
        MovingObject selectedObject = CombatPartyView.instance.GetSelected();
        if (selectedObject && Full)
        {
            if (selectedObject.GetComponent<NetworkIdentity>().hasAuthority)
            {
                // Calculate the distance between the MovingObject and the door
                float distanceToDoor = Vector3.Distance(selectedObject.transform.position, transform.position);
                
                // Check if the distance is less than or equal to 3
                if (distanceToDoor <= 3f)
                {
                    AttachedGroup.RemoveAll(mob => mob == null);
                    bool allDeadOrNull = true;
                    foreach (var mob in AttachedGroup)
                    {
                        if (mob != null && !mob.Dying)
                        {
                            allDeadOrNull = false;
                            break; // Break out of the loop as soon as a live mob is found
                        }
                    }
                    if (allDeadOrNull && Full)
                    {
                        // All mobs are dead or null, so you can open the chest
                        // Add your code to open the chest here
                        PlayerCharacter PC = selectedObject.GetComponent<PlayerCharacter>();
                        if(PC){
                            print("Opening chest!!");
                            PC.CmdOpenMainChest(this);
                        }
                    }
                    else
                    {
                        // There is still at least one alive mob, so the chest remains locked
                        // Add any additional handling for the locked state here if needed
                        ImproperCheckText.Invoke($"There are still {AttachedGroup.Count} mobs attached to this chest");
                    }
                }
                else
                {
                    print("You are too far from the door to interact with it.");
                }
            }
       
        }

    }
     public void OnPointerEnter(PointerEventData eventData){
		if (isServer)
    	{
    	    return;
    	}
        if(ScenePlayer.localPlayer.CheckForUIHIt()){
			return;
		}
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if(spriteRenderer != null){
            if(!spriteRenderer.enabled){
                return;
            }
        }
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
    	mouseOverBox.InjectName(NAME);
    	mouseOverBox.transform.position = Input.mousePosition + new Vector3(100, 100, 0);
        HoverNoise.Invoke();
    }
     public void OnPointerExit(PointerEventData eventData){
		if(isServer){
			return;
		}
		this.transform.GetComponent<SpriteRenderer>().color = new Color32(255,255,255,255);
		MouseOverCombat mouseOverBox = GameObject.Find("MouseOverCombat").GetComponent<MouseOverCombat>();
		if(!mouseOverBox){
			return;
		}
    	Canvas mouseOverBoxCanvas = mouseOverBox.GetComponent<Canvas>();
		mouseOverBoxCanvas.enabled = false;
    }
}}
