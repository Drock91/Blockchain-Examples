using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Mirror;
namespace dragon.mirror{
public class WeaponDrop : NetworkBehaviour, IPointerExitHandler, IPointerEnterHandler
{
    Match assignedMatch;
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer sRend;
    [SyncVar]
    [SerializeField] public bool Full = true;
    [SerializeField] public static UnityEvent<string> ImproperCheckText = new UnityEvent<string>();
    public static UnityEvent WeaponRackNoise = new UnityEvent();
    public static UnityEvent HoverNoise = new UnityEvent();
    private string NAME = "Weapon rack";
    int tier = 1;
    public int GetTier(){
        return tier;
    }
    private object weaponLock = new object();

    [Server]
    public void SetOpened(){
        lock (weaponLock)
        {
            Full = false;
            RpcOpenAnimator();
        }
        //RpcOpenAnimator();
    }
    [ClientRpc]
    void RpcOpenAnimator(){
        animator.SetBool("IsOpened", true);
        WeaponRackNoise.Invoke();
    }
    [Server]
    public void SetMatch(Match match, int _tier){
        tier = _tier;
        assignedMatch = match;
    }
    IEnumerator SetWeaponRackTier(){
        yield return new WaitForSeconds(6f);
        RpcSetUpWRAnimator(tier);
    }
    [ClientRpc]
    void RpcSetUpWRAnimator(int _tier){
        if(_tier <= 0){
            _tier = 1;
        }
        animator.SetInteger("Tier", _tier);
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
                    PlayerCharacter PC = selectedObject.GetComponent<PlayerCharacter>();
                    if(PC){
                        print("Opening chest!!");
                        PC.CmdPickUpWeapon(this);
                    }
                }
                else
                {
                    ImproperCheckText.Invoke("You are too far from the rack to interact with it.");
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
}
}