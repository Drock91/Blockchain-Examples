using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using UnityEngine.AI;
using Mirror;
namespace dragon.mirror{

public class Door : NetworkBehaviour
{
    Match assignedMatch;
    [SerializeField] private Canvas interactionCanvas; // Reference to the Canvas
    [SerializeField] private GameObject lockedMenu; // Reference to the Canvas
    [SerializeField] private GameObject goldLockedMenu; // Reference to the Canvas
    [SerializeField] private GameObject silverlockedMenu; // Reference to the Canvas
    [SerializeField] private GameObject unlockedMenu; // Reference to the Canvas
    [SerializeField] Animator animator;
    [SerializeField] NavMeshObstacle obstacle;
    [SerializeField] public static UnityEvent<Mob>  PCClickedDoor = new UnityEvent<Mob>();
    public static UnityEvent HoverNoise = new UnityEvent();
    [SyncVar]
    public bool isLocked = true;
    [SyncVar]
    public bool Gold = true;
    [SyncVar]
    public bool Silver = false;
    [SyncVar]
    public bool isDestroyed = false; // Add this to track the destroyed state
    [SyncVar]
    public bool ClosedDoor = true; // Add this to track the destroyed state
    private string NAME = "Door";
    public override void OnStartServer(){
        base.OnStartServer();
        TurnManager.OpenDoor.AddListener(OpenDoor);
        TurnManager.BreakDoor.AddListener(BreakDoor);
        TurnManager.UnlockDoor.AddListener(UnlockDoor);
        TurnManager.CloseDoor.AddListener(CloseDoor);

    }
        public override void OnStartClient()
        {
            base.OnStartClient();
            ScenePlayer.NewTarget.AddListener(TurnOffDoor);
        }
    public void TurnOffDoor(){
        if (interactionCanvas != null)
    {
        interactionCanvas.enabled = false;
        lockedMenu.SetActive(false);
        goldLockedMenu.SetActive(false);
        silverlockedMenu.SetActive(false);
        unlockedMenu.SetActive(false);
    }
    }
    public void ToggleInteractionPanel()
{
    print("Clicked door!!");
    if (interactionCanvas != null)
    {
        MovingObject selectedObject = CombatPartyView.instance.GetSelected();
        if (selectedObject)
        {
            if (selectedObject.GetComponent<NetworkIdentity>().hasAuthority)
            {
                // Calculate the distance between the MovingObject and the door
                float distanceToDoor = Vector3.Distance(selectedObject.transform.position, transform.position);
                
                // Check if the distance is less than or equal to 3
                if (distanceToDoor <= 3f)
                {
                    bool isCanvasEnabled = interactionCanvas.enabled;
                    interactionCanvas.enabled = !isCanvasEnabled;
                    // Reset all menus
                    lockedMenu.SetActive(false);
                    goldLockedMenu.SetActive(false);
                    silverlockedMenu.SetActive(false);
                    unlockedMenu.SetActive(false);
                    // Determine which menu to show based on the door's state
                    if (isDestroyed)
                    {
                        // Handle logic for destroyed door if needed
                        return;
                    }
                    if (isLocked)
                    {
                        if (Gold) goldLockedMenu.SetActive(!isCanvasEnabled);
                        else if (Silver) silverlockedMenu.SetActive(!isCanvasEnabled);
                        else lockedMenu.SetActive(!isCanvasEnabled);
                    }
                    else
                    {
                        unlockedMenu.SetActive(!isCanvasEnabled);
                    }
                }
                else
                {
                    print("You are too far from the door to interact with it.");
                }
            }
        }
    }
}
    IEnumerator InitializeClient(){
        yield return new WaitForSeconds(3f);
        RpcInitializeState();
    }
    [ClientRpc]
    private void RpcInitializeState(){
        animator.SetBool("IsLocked", isLocked); // Set the initial animator state.
        animator.SetBool("GoldDoor", Gold); // Set the initial animator state.
        animator.SetBool("SilverDoor", Silver); // Set the initial animator state.
        // You can also add logic here to enable/disable the lock image on the client.
    }
    [Server]
    public void SetMatch(Match match, bool locked, bool gold, bool silver){
        assignedMatch = match;
        isLocked = locked;
        Gold = gold;
        Silver = silver;
        StartCoroutine(InitializeClient());
    }
    [Server]
    void CloseDoor(Door door, Match match){
        if(door == this){
            ClosedDoor = true;
            obstacle.enabled = true;
            RpcCloseDoor();
        }
    }
    [Server]
    void UnlockDoor(Door door, Match match){
        if(door == this){
            isLocked = false; // Update the locked state.
            RpcUnlockDoor();
        }
    }
    [Server]
    void BreakDoor(Door door, Match match){
        if (door == this)
        {
            obstacle.enabled = false; // Disable on the server
            isDestroyed = true; // Set the destroyed state
            RpcBreakDoor(); 
        }
    }
    [Server]
    void OpenDoor(Door door, Match match){
        if(door == this && !isLocked){
            ClosedDoor = false;
            obstacle.enabled = false;
            RpcOpenDoor();
        }
    }
    [ClientRpc]
    private void RpcOpenDoor()
    {
        obstacle.enabled = false;
        animator.SetBool("IsOpened", true);
    }
    [ClientRpc]
    private void RpcBreakDoor()
    {
        obstacle.enabled = false; // Disable on the client
        animator.SetBool("IsDestroyed", true);
        isDestroyed = true; // Set the destroyed state on the client
    }
    [ClientRpc]
    private void RpcUnlockDoor()
    {
        animator.SetBool("IsLocked", false);
    }
    [ClientRpc]
    private void RpcCloseDoor()
    {
        obstacle.enabled = true;
        animator.SetBool("IsOpened", false);
    }
    public void ClientUnlockDoor()
    {
        PlayerCharacter selectedPlayer = CombatPartyView.instance.GetSelected().GetComponent<PlayerCharacter>();
        if(selectedPlayer){
            if(selectedPlayer.GetComponent<NetworkIdentity>().hasAuthority){
                selectedPlayer.CmdUnlockDoor(this, selectedPlayer.assignedMatch);
            }
        }
    }
    public void ClientBreakDoor()
    {
        PlayerCharacter selectedPlayer = CombatPartyView.instance.GetSelected().GetComponent<PlayerCharacter>();
        if(selectedPlayer){
            if(selectedPlayer.GetComponent<NetworkIdentity>().hasAuthority){
                selectedPlayer.CmdBreakDoor(this, selectedPlayer.assignedMatch);
            }
        }
    }
    public void ClientOpenDoor()
    {
        PlayerCharacter selectedPlayer = CombatPartyView.instance.GetSelected().GetComponent<PlayerCharacter>();
        if(selectedPlayer){
            if(selectedPlayer.GetComponent<NetworkIdentity>().hasAuthority){
                if(ClosedDoor){
                    selectedPlayer.CmdOpenDoor(this, selectedPlayer.assignedMatch);
                } else 
                {
                    selectedPlayer.CmdCloseDoor(this, selectedPlayer.assignedMatch);
                }
                TurnOffDoor();
            }
        }
    }
    public void ClientCloseDoor()
    {
        PlayerCharacter selectedPlayer = CombatPartyView.instance.GetSelected().GetComponent<PlayerCharacter>();
        if(selectedPlayer){
            if(selectedPlayer.GetComponent<NetworkIdentity>().hasAuthority){
                selectedPlayer.CmdCloseDoor(this, selectedPlayer.assignedMatch);
            }
            TurnOffDoor();
        }
    }
    public void OnMouseEnter(){
		if (isServer)
    	{
    	    return;
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
    public void OnMouseExit(){
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
