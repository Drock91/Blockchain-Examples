using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using Mirror;
using UnityEngine.Events;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using System.Net.Http;
using TMPro;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Text;
using WebSocketSharp;

namespace dragon.mirror
{
 

public class PlayFabClient : NetworkManager
{

    public static PlayFabClient Instance { get; private set; }
    public static UnityEvent<bool> TacticianBegin = new UnityEvent<bool>();
    public static UnityEvent<string, bool> XummMSG = new UnityEvent<string, bool>();
    public static UnityEvent CenterCamera = new UnityEvent();
    public static UnityEvent<string, Vector3> CenterCameraCombat = new UnityEvent<string, Vector3>();
    public static UnityEvent RandomCameraReset = new UnityEvent();
    public static UnityEvent BlockCLicker = new UnityEvent();
    public static UnityEvent NameDone = new UnityEvent();
    public static UnityEvent CharacterBegin = new UnityEvent();
    public static UnityEvent QRMusic = new UnityEvent();
    public static UnityEvent QRStopMusic = new UnityEvent();
    public static UnityEvent StartLoginMusicWaiting = new UnityEvent();
    public static UnityEvent EndLoginMusicWaiting = new UnityEvent();
    public static UnityEvent<int> StoreFrontMessage = new UnityEvent<int>();
    public static UnityEvent<string> VersionUpdate = new UnityEvent<string>();

    public DisconnectedEvent OnDisconnected = new DisconnectedEvent();
    public class DisconnectedEvent : UnityEvent<int?> {}
    [SerializeField] public static UnityEvent OnConnected = new UnityEvent();
    public string userName { get; private set; }
    public string PlayFabId;
    public string SessionTicket;
    public static string tactician;
    public string scene;
    GameObject settings;
    [SerializeField] GameObject LoadScreenGameObject;
    [SerializeField] Canvas LoadScreenCanvas;
    [SerializeField] Slider LoadScreenSlider;
    [SerializeField] GameObject LoadScreenSliderGameObject;
    List<GameObject> prefabs = new List<GameObject>();
    [SerializeField] GameObject XummQRCodePayment;
    [SerializeField] GameObject QRCODEGENERATOR;
    [SerializeField] GameObject ErrorQRCODE;
    [SerializeField] GameObject ConfirmQRCODE;
    [SerializeField] GameObject ExitButton;
    [SerializeField] GameObject InfoObject;
    [SerializeField] Image WaitingObject;

    [SerializeField] TextMeshProUGUI qrCodeDisplayText;
    [SerializeField] Canvas XummQRCodePaymentCanvas;
    public static UnityEvent OurNodeSet = new UnityEvent();
    [SerializeField] GameObject TipGO;
    [SerializeField] TextMeshProUGUI Tip;
    [SerializeField] string VERSION;
    //Coroutine ServerChecker;
    public bool showPing = true;
    float afkThreshold = 300f; // 5 minutes for AFK
    float logoutThreshold = 5400f; // 90 minutes for logout
    private float lastActivityTime;
    public override void Awake()
    {
        LoadNetworkPrefabs();
        base.Awake();
        Instance = this;
        lastActivityTime = Time.time;
    }
   
    public void Update(){
        if(ScenePlayer.localPlayer == null){
            return;
        }
        // Check for any key input or mouse movement
        if (Input.anyKeyDown || Input.anyKey || Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            lastActivityTime = Time.time;
            CheckIfAFK();
            //print($"Mouse moved! AFK next time is {300f}");

        }
        if (Input.anyKeyDown || Input.anyKey)
        {
            lastActivityTime = Time.time;
            CheckIfAFK();
            //print($"Button clicked or mouse clicked AFK next time is {300f}");
        }
        CheckAFKStatus();
    }
    bool AFK;
    void CheckIfAFK(){
        if(AFK){
            AFK = false;
            print("AFK ENDED");
            ScenePlayer.localPlayer.ReturnedFromAFK();
        }
    }
    void CheckAFKStatus()
    {
        if (Time.time - lastActivityTime > logoutThreshold)
        {
            LogoutPlayer();
        }
        else if (Time.time - lastActivityTime > afkThreshold)
        {
            if(!AFK){
                AFK = true;
                print("AFK STARTED");
                ScenePlayer.localPlayer.SetAFK();
            }
            // Set player state to AFK
            // This is where you could notify the server that the player is AFK
        } 
    }

     void LogoutPlayer()
    {
        print("LOGOUT STARTED");
        StartCoroutine(ClientLogoutDisconnected());
    }
    private void LoadNetworkPrefabs()
    {
        // Clear the list to not have duplicates
        // Load all prefabs in the 'Resources/Prefabs/Monsters' directory
        //prefabs = Resources.LoadAll<GameObject>("Prefabs/Monsters");
        prefabs.AddRange(Resources.LoadAll<GameObject>("Prefabs/Monsters"));
        prefabs.AddRange(Resources.LoadAll<GameObject>("Prefabs/Droppables"));

        foreach (GameObject prefab in prefabs)
        {
            if (prefab.GetComponent<NetworkIdentity>() != null)
            {
                spawnPrefabs.Add(prefab);
            }
            else
            {
                Debug.LogWarning($"{prefab.name} does not have a NetworkIdentity and was not added to the list of spawnable prefabs.");
            }
        }
    }
    List<string> tipsList = new List<string>
    {
        "Can't leave town with a fallen comrade or while someone is leveling up in your party.",
        "You can't learn new spells while leveling up a character. So pick wisely when to level up!",
        "Random battles await you outside the town of Arudine! Pack some potions and sharpen your sword.",
        "Don't forget to rest at the inn before venturing out to ensure your party is at full strength.",
        "Wizards never run out of mana; they're just recharging their arcane enthusiasm!",
        "Why did the rogue wear leather armor? Because it's made of hide!",
        "What's a paladin's favorite hangout? The 'Righteous Inn'!",
        "Why did the adventurer carry a map? To find the XP 'hot spots'!",
        "Did you hear about the priest who lost his faith? Now he's a 'non-prophet'!",
        "Blockchain transactions are irreversible, make sure to double-check all details in DragonKill.",
        "The DragonKill staff will never ask for your private crypto key or your password.",
        "There are 8 different themes to choose from in the settings menu to customize your game experience.",
        "Crafting in town can make your party of up to 6 stronger with better items and equipment.",
        "Join forces with other players in dungeons for better loot and unique treasures.",
        "You can trade with other players for supplies to help you continue your journey or aid you on your return.",
        "Manage your energy wisely! The max cap is 10,000, and you'll need it for all actions in DragonKill.",
        "Energy regenerates at 3,000 per day if you are at 0, up to 3,000. After that, it regenerates at 1,000 per day.",
        "Pace yourself and avoid overplaying to maintain a steady energy flow and maximize your play-to-earn potential.",
        "Use your characters' strengths and abilities to slay monsters for gold and valuable items.",
        "Trading in town can help you find essential items and supplies for your adventures.",
        "Houses in town can hold up to 6 items for sale. Secure a house to establish your own marketplace.",
        "The auction house offers a way to sell items but beware of the high fees involved.",
        "Prepare thoroughly before traveling far; the journey will be challenging and requires a lot of effort.",
        "Join dungeons with other players to earn better loot and experience.",
        "Raiding with large groups can lead to epic rewards and legendary status.",
        "In the city of Arudine, you'll find various crafting stations to enhance your items and equipment.",
        "Gather resources and craft powerful items to strengthen your party.",
        "Forming a balanced party with different classes can help you overcome diverse challenges.",
        "Use the trading system to exchange items and resources with other players.",
        "Maximize your play-to-earn potential by managing your energy and resources efficiently.",
        "Visit the auction house for rare items, but consider the high fees before placing your bid.",
        "Explore different parts of Gaia to discover hidden secrets and powerful artifacts.",
        "Your characters' synergy and teamwork can make a significant difference in tough battles.",
        "Keep an eye on your energy levels and plan your actions accordingly to avoid running out.",
        "Save energy by resting in town and participating in low-energy activities.",
        "Crafting high-quality items can give you an edge in both PvE and PvP encounters.",
        "Trading in town is a great way to find supplies and gear for your journey.",
        "Auction house fees can add up. Use houses for cost-effective trading.",
        "Each city in Gaia offers unique opportunities and challenges. Explore them all!",
        "Stay updated with the latest game events and updates to make the most of your play-to-earn experience.",
        "Collaborate with other players to tackle the hardest dungeons and raids for the best rewards.",
        "Challenging other tacticians does not effect your characters health and magic points after the duel, including death!."
    };
    void SetUserName(string _userName){
        userName = _userName;
    }
    private const string NAME_REGEX = @"^(?!.*([A-Za-z0-9])\1{2})(?=.*[a-z])(?=.{3,13})[A-Za-z0-9]+$";
    
    //void LoadOnSelected(){
    //    LoadScreenGameObject.SetActive(true);
    //    TipGO.SetActive(true);
    //    Tip.text = GetRandomTip();
    //    LoadScreenCanvas.enabled = true;
    //    LoadScreenSliderGameObject.SetActive(true);
    //}
    void OnEnable(){
        //SpeakerAntenna.LoadOn.AddListener(LoadOnSelected);
        ScenePlayer.MoveRequest.AddListener(MoveRequest);
        ContainerPlayerUI.TacticianReady.AddListener(NoobReady);
        ScenePlayer.OVMRequest.AddListener(OVMRequest);
        ScenePlayer.LoadbarOffToggle.AddListener(TurnOffLoadbar);
        ScenePlayer.LoadbarOnToggle.AddListener(TurnOnLoadbar);
        ScenePlayer.RegistrationFinished.AddListener(TurnOffRegister);

        NetworkClient.RegisterHandler<SendQRCodeUrlMessage>(RegisterPlayer);
        NetworkClient.RegisterHandler<XummMessage>(XummCode);
        NetworkClient.RegisterHandler<XummTransmute>(XUMMTRANSMUTERES);
        NetworkClient.RegisterHandler<XRPLTransmute>(XRPLTRANSMUTERES);
        NetworkClient.RegisterHandler<WaitingForLogout>(WaitingLogoutTip);
        NetworkClient.RegisterHandler<OffsetRecord>(OffSetReceived);
        ContainerPlayerUI.TacticianNameSet.AddListener(SetUserName);
        StoreFront.ReadyToTransmuteXUMMDKP.AddListener(PrepareXumm);
        StoreFront.ReadyToTransmuteXRPLGOLD.AddListener(PrepareXRPL);
        ScenePlayer.ToggleCloseEndMatch.AddListener(ClosedMatch);
        UILobby.LoadScreenDetected.AddListener(ClosedMatch);
        //NetworkClient.RegisterHandler<XUMMQRCODE>(ACTIVATEACCOUNT);
    }
    Coroutine animationRoutine;
    void ClosedMatch(){
        LoadScreenGameObject.SetActive(true);
            TipGO.SetActive(true);
            Tip.text = GetRandomTip();
            LoadScreenCanvas.enabled = true;
            LoadScreenSliderGameObject.SetActive(true);
            //animationRoutine = StartCoroutine(AnimationRoutine());
            
    }
    void CombatSceneRequested(string sceneName, Vector3 offsetPosition){
        CenterCameraCombat.Invoke(sceneName, offsetPosition);
    }
    IEnumerator AnimationRoutine()
    {
        // Generate a random number between 0 and 7
        int randomNumber = UnityEngine.Random.Range(0, 8); // Random.Range is inclusive of the first argument and exclusive of the second

        // Retrieve the sprite list for the random choice
        List<Sprite> spriteList = ItemAssets.Instance.GetWaitingSprites(randomNumber);

        // Loop indefinitely
        while (true)
        {
            // Iterate through each sprite in the list
            foreach (Sprite sprite in spriteList)
            {
                WaitingObject.sprite = sprite; // Change the sprite displayed by the SpriteRenderer
                yield return new WaitForSeconds(0.5f); // Wait for half a second before continuing
            }
        }
    }
    public void ExitPressedOnQR(){
        StoreFrontMessage.Invoke(10);
        XummQRCodePayment.SetActive(false);
        XummQRCodePaymentCanvas.enabled = false;
        ExitButton.SetActive(false);
    }
    void WaitingLogoutTip(WaitingForLogout msg){
        Tip.text = "Prior logout is still processessing, this could take a moment";
        StartLoginMusicWaiting.Invoke();
        //music for waiting
    }
    void XRPLTRANSMUTERES(XRPLTransmute msg){
            InfoObject.SetActive(false);

            //QRCODEGENERATOR.SetActive(false);
        int signal = 0;
        if(msg.code == "1"){
            signal = 1;
        }
        StoreFrontMessage.Invoke(signal);
    }
    void XUMMTRANSMUTERES(XummTransmute msg){
        if(msg.qrCodeUrl != null){
            InfoObject.SetActive(false);
            XummQRCodePayment.SetActive(true);
            XummQRCodePaymentCanvas.enabled = true;
            ErrorQRCODE.SetActive(false);
            ConfirmQRCODE.SetActive(false);
            //QRCODEGENERATOR.SetActive(true);
            qrCodeDisplayText.text = "Scan the QR Code to transmute the dkp xls-20 token into in-game gold currency";
            ExitButton.SetActive(true);
            QRCodeGenerator qrCode = QRCODEGENERATOR.GetComponent<QRCodeGenerator>();
            if(qrCode){
                qrCode.StartMe(msg.qrCodeUrl);
            }
        }
        if(msg.code != null){
            int signal = 2;
                //cancelled
                

            if(msg.code == "3"){
                signal = 3;
                //expired
            }
            if(msg.code == "4"){
                signal = 4;
                //validating
            }
            if(msg.code == "5"){
                signal = 5;
                //success!
            }
            if(msg.code == "6"){
                signal = 6;
                //failed!
            }
            if(msg.code == "15"){
                signal = 15;
                //failed!
            }
            if(msg.code == "20"){
                signal = 20;
                //failed!
            }
            if(msg.code == "25"){
                signal = 25;
                //failed!
            }
            XummQRCodePayment.SetActive(false);
            XummQRCodePaymentCanvas.enabled = false;
            StoreFrontMessage.Invoke(signal);
        }
        
    }
    void XummCode(XummMessage msg){
        bool quitting = false;
        if(msg.quit){
            quitting = true;
            OnClientDisconnect();
        }
        if(msg.code  == "Checking status of our market buy for DKP"){
            qrCodeDisplayText.text = "After trading XRP for DKP we can register our account, 50000 dkp is the requirement";
        }
        if(msg.code == "Trust set was sent to XRPL from Xumm"){
            qrCodeDisplayText.text = "Trust set tx was sent to the XRP Ledger awaiting a validator node response to confirm";
        }
        if(msg.code == "The trust set QR code was cancelled, trying again in a few moments."){
            qrCodeDisplayText.text = "Generating a new QR code, trying again in a few moments";
        }
        if(msg.code == "The trust set QR code has expired, trying again in a few seconds."){
            qrCodeDisplayText.text = "Generating a new QR code, trying again in a few moments";
        }
        if(msg.code == "No trust line detected, sending a trust set QR code in a few moments"){
            qrCodeDisplayText.text = "Processing a new QR code for setting a trustline with DKP, 2 XRP reserve is required for this, the XRP will not be consumed";
        }
        if(msg.code == "The registration QR code has expired, trying again in a few seconds."){
            qrCodeDisplayText.text = "Your registration code was expired, checking trust line, preparing another QR code";
        }
        if(msg.code == "The registration QR code was cancelled, trying again in a few seconds."){
            qrCodeDisplayText.text = "Your registration code was cancelled, checking trust line, preparing another QR code";
        }
        if(msg.code == "Registration request sent from XUMM to XRPL awaiting a validator node reponse"){
            qrCodeDisplayText.text = "Pending response from a validator node on the XRP Ledger, one moment please";
        }
        if(msg.code == "Trading XRP for DKP via the XRP Ledger's decentralized exchange one moment please"){
            qrCodeDisplayText.text = "Your trade will be completed shortly";
        }
        if(msg.code == "The wallet you have tried to sign with has already been registered to another DragonKill account, please try another."){
            qrCodeDisplayText.text = "Try another wallet, it seems that one has already been used before";
        }
        if(msg.code == "Error on payload, we will generate you a new QR code in just a few moments."){
            qrCodeDisplayText.text = "Generating another QR code";
        }
        if(msg.code == "Registration tx successfully validated on the XRP Ledger. Welcome to DragonKill!"){
            qrCodeDisplayText.text = "Welcome to DragonKill, congraduations on registering.";
        }
        if(msg.code == "Checking status one moment please"){
            qrCodeDisplayText.text = "Scan the QR code with your Xumm Wallet to activate your Dragonkill account for 100dkp.";
        }
        if(msg.code == "Checking status trust set status one moment please"){
            qrCodeDisplayText.text = "We are in the trust set part of the registration";
        }
        if(msg.code == "Trust line set for DKP but not enough liquidity, fetching the market order to accomplish registration please double check"){
            qrCodeDisplayText.text = "Please double check the QR code to verify it is an acceptable trade for XRP to DKP, this is based on the current market";
        }
        if(msg.code == "The registration QR code was cancelled, trying again in a few moments."){
            qrCodeDisplayText.text = "The registration QR code was cancelled, trying again in a few moments.";
        }
        
        
        if(!quitting && !msg.pending){
            //QRCODEGENERATOR.SetActive(false);
            QRCodeGenerator qrCode = QRCODEGENERATOR.GetComponent<QRCodeGenerator>();
            if(qrCode){
                qrCode.ToggleCanvas(false);
            }
            if(msg.error){
                ErrorQRCODE.SetActive(true);
                
                ConfirmQRCODE.SetActive(false);
            } else {
                ErrorQRCODE.SetActive(false);
                ConfirmQRCODE.SetActive(true);
            }
            XummMSG.Invoke(msg.code, msg.error);
        }
        if(msg.pending){
            XummMSG.Invoke(msg.code, msg.error);
        }
    }
    
    void NoobReady(BuildingTacticianHelper tactBuild){
        QRStopMusic.Invoke();
        LoadScreenGameObject.SetActive(true);
        TipGO.SetActive(true);
        Tip.text = GetRandomTip();
        LoadScreenCanvas.enabled = true;
        LoadScreenSliderGameObject.SetActive(true);
        NetworkClient.connection.Send<NoobToPlayer>(new NoobToPlayer
        {
            Sprite = tactBuild.Sprite,
            bonusStatStrength = tactBuild.bonusStatStrength,
            bonusStatAgility = tactBuild.bonusStatAgility,
            bonusStatFortitude = tactBuild.bonusStatFortitude,
            bonusStatArcana = tactBuild.bonusStatArcana,
            BirthDate = tactBuild.BirthDate,
            BodyStyle = tactBuild.BodyStyle,
            EyeColor = tactBuild.EyeColor,
            finished = true
        });
    }

    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling) {
        if (customHandling)
        {
            StartCoroutine(LoadScreenEngaged(newSceneName));
        }
    }
    IEnumerator LoadScreenEngaged(string newSceneName){
            CircleCollider2D boxCollider = ScenePlayer.localPlayer.GetComponent<CircleCollider2D>();

        if(newSceneName != "OVM" && newSceneName != "TOWNOFARUDINE"){
            boxCollider.enabled = false;
        } else {
            boxCollider.enabled = true;
        }
        BlockCLicker.Invoke();
        //print($" {newSceneName} was scene name we are supposed to be at");
        if(!newSceneName.Contains("Random")){
            LoadScreenGameObject.SetActive(true);
            TipGO.SetActive(true);
            Tip.text = GetRandomTip();
            LoadScreenCanvas.enabled = true;
            LoadScreenSliderGameObject.SetActive(true);
        }
        
        //load screen tip
        loadingSceneAsync = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
        loadingSceneAsync.allowSceneActivation = false;
        float lerpValue = 0.0f; 
        float startTime = Time.time;
        float duration = 2f;
        while(lerpValue < .75f){
            lerpValue = Mathf.Lerp(0f,.75f, (Time.time - startTime) / duration);
            LoadScreenSlider.value = lerpValue;
            yield return null;
        }
        loadingSceneAsync.allowSceneActivation = true;
        if(newSceneName.Contains("Random")){
            RandomCameraReset.Invoke();
        }
    }
    void TurnOffRegister(){
        registerProcess = false;
        LoadScreenCanvas.enabled = false;
        LoadScreenGameObject.SetActive(false);
        XummQRCodePayment.SetActive(false);
        XummQRCodePaymentCanvas.enabled = false;
        ErrorQRCODE.SetActive(false);
        ConfirmQRCODE.SetActive(false);
        //QRCODEGENERATOR.SetActive(true);
       // EndLoginMusicWaiting.Invoke();
        QRStopMusic.Invoke();


        //load tip here randomly for players
    }
    void TurnOnLoadbar(){
        LoadScreenGameObject.SetActive(true);
        TipGO.SetActive(true);
        Tip.text = GetRandomTip();
        LoadScreenCanvas.enabled = true;
        LoadScreenSliderGameObject.SetActive(true);
        //load tip here randomly for players
    }
    void TurnOffLoadbar(){
        
        StartCoroutine(WaitForLoading());
    }
    IEnumerator WaitForLoading()
    {
        while(!SceneManager.GetSceneByName(ScenePlayer.localPlayer.currentScene).isLoaded)
        {
           // Do something here
            yield return null;
        }
        CenterCamera.Invoke();
        EndLoginMusicWaiting.Invoke();
        OurNodeSet.Invoke();
        float lerpValue = .75f; 
        float startTime = Time.time;
        float duration = 2f;
        while(lerpValue < 1f){
            lerpValue = Mathf.Lerp(.75f,1f, (Time.time - startTime) / duration);
            LoadScreenSlider.value = lerpValue;
            yield return null;
        }
        yield return new WaitForSeconds(1f);
        LoadScreenCanvas.enabled = false;
        LoadScreenGameObject.SetActive(false);
        TipGO.SetActive(false);
        LoadScreenSliderGameObject.SetActive(false);
        //TurnOffLoadbarEXT();
    }
    void TurnOffLoadbarEXT(){
        CenterCamera.Invoke();
        EndLoginMusicWaiting.Invoke();
        OurNodeSet.Invoke();
        StartCoroutine(WaitCenterCamera());
    }
    IEnumerator WaitCenterCamera()
    {
        yield return new WaitForSeconds(1.5f);
        LoadScreenCanvas.enabled = false;
        LoadScreenGameObject.SetActive(false);
        TipGO.SetActive(false);
        LoadScreenSliderGameObject.SetActive(false);
    }
    void ResetAfterFailedConnect(){
        LoadScreenCanvas.enabled = false;
        LoadScreenGameObject.SetActive(false);
        TipGO.SetActive(false);
        LoadScreenSliderGameObject.SetActive(false);
        //UISignIn.instance.ResetLogin();
        StartCoroutine(ResetAfterFailedConnectionOrdiconnection());
        //if (!settings.GetComponent<Settings>().isLocalServer)
        //{
            if(ServerDataChecker == null){
                ServerDataChecker = StartCoroutine(ServerCheckINFO());
            }

            playerCountVariable = "Players online: 0";
            //RequestMultiplayerServerInfo(0);

            //StartCoroutine(ServerSniffer(0));

            playerCountText.text = playerCountVariable;
        //}
        //unfreeze the button
    }
    void ResetAfterFailedConnectSniff(){
        LoadScreenCanvas.enabled = false;
        LoadScreenGameObject.SetActive(false);
        TipGO.SetActive(false);
        LoadScreenSliderGameObject.SetActive(false);
    }
    IEnumerator ResetAfterFailedConnectionOrdiconnection(){
        yield return new WaitForSeconds(1f);
        if(UISignIn.instance != null){
            UISignIn.instance.ResetLogin();
        } else {
            print("Button was destroyed cant access UISignIn");
        }
    }
    public void CancelRegistrationClicked(){
        TurnOffRegister();
        ScenePlayer.localPlayer.RegisterWalletCancelationClient();
    }
    public string GetCurrentScene(){
            int countLoaded = SceneManager.sceneCount;
            print($"*******{countLoaded} is how many sceens are loaded on the server*******");
            Scene[] loadedScenes = new Scene[countLoaded];
            for (int i = 0; i < countLoaded; i++)
            {
                loadedScenes[i] = SceneManager.GetSceneAt(i);
                string sceneName = loadedScenes[i].name;
                if(sceneName == "Container")
                {
                    //change scene name
                    scene = sceneName;
                    break;
                }
                if(sceneName == "TOWNOFARUDINE")
                {
                    //change scene name
                    scene = sceneName;
                    break;
                }
                if(sceneName == "OVM")
                {
                    //change scene name
                    scene = sceneName;
                } 

            }
            return scene;
         
        }
    string GetRandomTip()
    {
        int randomIndex = UnityEngine.Random.Range(0, tipsList.Count);
        return tipsList[randomIndex];
    }
    string DetermineDirection()
{
    string direction = "";

    // Get positions
    Vector2 hostPosition = ScenePlayer.localPlayer.transform.position;
    Vector2 nodePosition = new Vector2(-36.354f, -8.491f);
    if(hostPosition.x > -35.56f){
        direction = "East";
    } else if(hostPosition.y < -9f){
        direction = "South";
    } else {
        direction = "West";
    }
/*
    // Calculate relative position
    Vector2 relativePosition = hostPosition - nodePosition;

    // Determine direction
    if (relativePosition.x > 0 && relativePosition.y > 0)
    {
        direction = "West";
    }
    else if (relativePosition.x < 0 && relativePosition.y > 0)
    {
        direction = "West";
    }
    else if (relativePosition.x > 0 && relativePosition.y < 0)
    {
        direction = "South";
    }
    else if (relativePosition.x < 0 && relativePosition.y < 0)
    {
        direction = "South";
    }
    else if (relativePosition.x > 0)
    {
        direction = "East";
    }
    else if (relativePosition.x < 0)
    {
        direction = "West";
    }
    else if (relativePosition.y > 0)
    {
        direction = "West";
    }
    else if (relativePosition.y < 0)
    {
        direction = "South";
    }
    */
    return direction;
}

    void MoveRequest(string newScene, string oldScene){
        LoadScreenGameObject.SetActive(true);
        TipGO.SetActive(true);
        Tip.text = GetRandomTip();
        LoadScreenCanvas.enabled = true;
        LoadScreenSliderGameObject.SetActive(true);
        string unloadScene = string.Empty;
        if (oldScene == null)
        {
            unloadScene = "Container";
        }else {
            unloadScene = oldScene;
        }
        string direction = DetermineDirection();
        NetworkClient.connection.Send<ClientRequestLoadScene>(new ClientRequestLoadScene {
            newScene = newScene,
            oldScene = unloadScene,
            direction = direction,
            login = false
        });
    }
    void OVMRequest(string currentScene)
    {
        LoadScreenGameObject.SetActive(true);
        TipGO.SetActive(true);
        Tip.text = GetRandomTip();
        LoadScreenCanvas.enabled = true;
        LoadScreenSliderGameObject.SetActive(true);
        //put in load tips
        //NetworkClient.connection.Send<ClientRequestLoadScene>(new ClientRequestLoadScene {
        //    newScene = "OVM",
        //    oldScene = "TOWNOFARUDINE",
        //    login = false
        //});
    }
    [SerializeField] TextMeshProUGUI VersionText;

    [SerializeField] Canvas ServerInfoStatus;
    [SerializeField] TextMeshProUGUI playerCountText;
    [SerializeField] TextMeshProUGUI offlineOnlineinfo;
    [SerializeField] Image ServerInfoStatusImage;
    [SerializeField] Sprite onlineSprite;
    [SerializeField] Sprite offlineSprite;
    [SerializeField] Canvas errorPage;
    [SerializeField] TextMeshProUGUI errorText;
    string playerCountVariable = "Players online: 0";
    public override void Start()
    {
        Startup.Reset.AddListener(ResetAfterFailedConnect);
        settings = GameObject.Find("SETTINGS");
        LoadVideoPlayer.VideoComplete.AddListener(StartRequestMultiplayersInfo);
        VersionUpdate.Invoke(VERSION);
        VersionText.text = "DragonKill beta V" + VERSION;
        //if (!settings.GetComponent<Settings>().isLocalServer)
        //{
            
        //}
    }
    Coroutine ServerDataChecker;
    void StartRequestMultiplayersInfo(){
        playerCountText.text = playerCountVariable;
        ServerDataChecker = StartCoroutine(ServerCheckINFO());
    }
    IEnumerator ServerCheckINFO(){
        while(true){
            //RequestMultiplayerServerInfo(0);
            StartCoroutine(ServerSniffer(0));

            yield return new WaitForSeconds(60f);
        }   
        
    }
    bool activeServers = false;
    Coroutine errorDisplayRoutine;
    IEnumerator ErrorDisplay(string errorMessage){
        errorPage.enabled = true;
        errorText.text = errorMessage;
        yield return new WaitForSeconds(10f);
        errorPage.enabled = false;
    }
    private async void RequestMultiplayerServerInfo(int counter)
    {
        ServerInfoStatus.enabled = true;
        ServerInfoStatusImage.sprite = offlineSprite;
        counter ++;
        if(counter > 5){
            return;
        }
        versionTest versionRequest = new versionTest
        {
            version = VERSION
        };
        // server offline, server online
        using (var httpClient = new HttpClient())
        {
            var jsonContent = JsonConvert.SerializeObject(versionRequest);
            var contentInject = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{loginServerURL}/clientServerSniff", contentInject);  
            if (response.IsSuccessStatusCode)
            {
                // Parse the JSON response
                string content = await response.Content.ReadAsStringAsync();
                LoginRequestResponse loginResponse = JsonConvert.DeserializeObject<LoginRequestResponse>(content);
                // Use the values from the response
                playerCountVariable = "Players online: " + loginResponse.meta.playerCountVariable;
                if(loginResponse.meta.activeServers != "0"){
                    ServerInfoStatusImage.sprite = onlineSprite;
                    offlineOnlineinfo.text = "Server status: online";
                } else {
                    offlineOnlineinfo.text = "Server status: offline";
                }
                playerCountText.text = playerCountVariable;
                // Successfully joined the queue
                print($"queue is ready lets go! active servers: {loginResponse.meta.activeServers} players online: {loginResponse.meta.playerCountVariable}");
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                Debug.LogError($"Failed to join the queue. Status code: {response.StatusCode}, Message: {errorContent}");

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                    Debug.LogError("Bad Request: " + errorResponse.message);
                    if (errorResponse.message == "Version is required")
                    {
                        // Handle missing version case
                        Debug.LogError("The request is missing the version.");
                        StartCoroutine(ErrorDisplay(errorResponse.message));
                        return;
                    }
                    else if (errorResponse.message.Contains("No servers available with the specified version"))
                    {
                        // Handle no servers with the specified version case
                        Debug.LogError("No servers available with the specified version. Please update your game client.");
                        StartCoroutine(ErrorDisplay("No servers available with the specified version. Please update your game client."));
                        return;
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    Debug.LogError("Service Unavailable: " + errorContent);
                }
                // Log the failure or take other actions
                Debug.LogError("Failed to get server info...");
                RequestMultiplayerServerInfo(counter);
                return;
            }
        }
    }
    public void ChoCho(string playfabID, string sessionId, bool newAccount){
        LoadScreenGameObject.SetActive(true);
        TipGO.SetActive(true);
        Tip.text = GetRandomTip();
        LoadScreenCanvas.enabled = true;
        if(!newAccount)
        {
            LoadScreenSliderGameObject.SetActive(true);
        }
        this.SessionTicket = sessionId;
        this.PlayFabId = playfabID;
    }
    public string checkAddress = "Not connected";
    void NoobPlayer(Noob netMsg){
        checkAddress = netMsg.Address;
        LoadScreenCanvas.enabled = false;
        LoadScreenGameObject.SetActive(false);
        TipGO.SetActive(false);
        XummQRCodePayment.SetActive(false);
        XummQRCodePaymentCanvas.enabled = false;
        QRMusic.Invoke();
        if(netMsg.finished == false){
            bool NameNotSet = true;
            if(netMsg.tactician == "NOTACTICIANNAMEPHASE"){
                NameNotSet = false;
            } else {
                userName = netMsg.tactician;
            }
            TacticianBegin.Invoke(NameNotSet);
        } 
    }
    bool registerProcess = false;
    public bool CheckRegisterProcess(){
        return registerProcess;
    }
    void RegisterPlayer(SendQRCodeUrlMessage netMsg){
        registerProcess = true;
        LoadScreenCanvas.enabled = true;
        LoadScreenGameObject.SetActive(true);
        TipGO.SetActive(false);
        XummQRCodePayment.SetActive(true);
        XummQRCodePaymentCanvas.enabled = true;
        ErrorQRCODE.SetActive(false);
        ConfirmQRCODE.SetActive(false);
        QRCodeGenerator qrCode = QRCODEGENERATOR.GetComponent<QRCodeGenerator>();
        if(qrCode){
            qrCode.StartMe(netMsg.qrCodeUrl);
        }
        QRMusic.Invoke();
    }
    void PrepareXumm(string amount){
        InfoObject.SetActive(false);
        XummQRCodePayment.SetActive(true);
        XummQRCodePaymentCanvas.enabled = true;
        ErrorQRCODE.SetActive(false);
        ConfirmQRCODE.SetActive(false);
        qrCodeDisplayText.text = $"Preparing your QR code to transmute {amount} dkp into gold";
        NetworkClient.connection.Send<XummTransmute>(new XummTransmute
        {
            amount = amount
        });
    }
    void PrepareXRPL(string amount){
        NetworkClient.connection.Send<XRPLTransmute>(new XRPLTransmute
        {
            amount = amount
        });
    }
    public void CancelQRCode(){
        XummQRCodePayment.SetActive(false);
        XummQRCodePaymentCanvas.enabled = false;
        ErrorQRCODE.SetActive(false);
        ConfirmQRCODE.SetActive(false);
    }
    public void LoadScreenOpen(){
        LoadScreenGameObject.SetActive(true);
        TipGO.SetActive(true);
        Tip.text = GetRandomTip();
        LoadScreenCanvas.enabled = true;
        LoadScreenSliderGameObject.SetActive(true);
    }
    void CharacterBuild(){
        CharacterBegin.Invoke();
    }
    public void OnReceivePlayerInfo(PlayerInfo netMsg)
    {
       // Debug.Log("client connected to the server");
        //OnConnected.Invoke();
        print($"Client connected to the server\nReceived PlayerInfo, sending playfabid: {this.PlayFabId} back to Server");

        //print($"Client connected to the server\nReceived PlayerInfo, sending playfabid: {this.PlayFabId} back to Server");
        NetworkClient.connection.Send<PlayerInfo>(new PlayerInfo
        {
            ConnectionId = netMsg.ConnectionId,
            SessionTicket = this.SessionTicket
        });
    }
    public void OffSetReceived(OffsetRecord netMsg)
    {
        CenterCameraCombat.Invoke(netMsg.SceneName, netMsg.SceneOffset);
    }
    public override void OnStopClient(){
        if (mode == NetworkManagerMode.ClientOnly)
            StartCoroutine(ClientUnloadSubScenes());
    }
    IEnumerator ClientUnloadSubScenes()
    {
        for (int index = 0; index < SceneManager.sceneCount; index++)
        {
            if (SceneManager.GetSceneAt(index) != SceneManager.GetActiveScene())
                yield return SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(index));
        }
    }
    IEnumerator ClientLogoutDisconnected(){
        base.OnClientDisconnect();
        yield return new WaitForSeconds(2f);
        ResetAfterFailedConnect();
        Debug.Log("client disconnected");
        OnDisconnected.Invoke(null);
    }
    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        ResetAfterFailedConnect();
        Debug.Log("client disconnected");
        OnDisconnected.Invoke(null);
    }
    public void Disagreed(){
          #if UNITY_EDITOR
         // Application.Quit() does not work in the editor so
         // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
         UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
        
    }
    /// Playfab Calls
    private string loginServerURL = "https://dkpgamingapp-d9f21223f33f.herokuapp.com";
    private string buildID = "";
    private string SessionID = "";
    private string port = "";
    private string injectedIPAddress = "";

    //private async void RequestMultiplayerServer(int counter)
    public class versionTest
    {
        public string version { get; set; }
    }
    public class UpdateMultiplayerRequest
    {
        public string ip { get; set; }
        public string port { get; set; }
        public string sessionID { get; set; }
    }
    public class ErrorResponse
    {
        public string message { get; set; }
    }
    IEnumerator ServerSniffer(int counter) {
        ServerInfoStatus.enabled = true;
        ServerInfoStatusImage.sprite = offlineSprite;
            counter ++;
            if(counter > 5){
                ResetAfterFailedConnect();
                yield break;
            }
            WWWForm form = new WWWForm();
            form.AddField("version", VERSION);
            using (UnityWebRequest www = UnityWebRequest.Post(loginServerURL + "/clientServerSniff", form)) {
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError) {
                    Debug.LogError($"Error pinging server: {www.error}");
                    string errorContent = www.downloadHandler.text;
                    Debug.LogError($"Failed to join the queue. Message: {errorContent}");
                    if (www.responseCode == 400) {
                        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                        Debug.LogError("Bad Request: " + errorResponse.message);
                        if (errorResponse.message == "Version is required") {
                            Debug.LogError("The request is missing the version.");
                            if(errorDisplayRoutine != null){
                                StopCoroutine(errorDisplayRoutine);
                                errorDisplayRoutine = null;
                            }
                            errorDisplayRoutine = StartCoroutine(ErrorDisplay(errorResponse.message));
                            ResetAfterFailedConnectSniff();
                            yield break;
                        } else if (errorResponse.message.Contains("No servers available with the specified version")) {
                            Debug.LogError("No servers available with the specified version. Please update your game client.");
                            if(errorDisplayRoutine != null){
                                StopCoroutine(errorDisplayRoutine);
                                errorDisplayRoutine = null;
                            }
                            errorDisplayRoutine = StartCoroutine(ErrorDisplay("No servers available with the specified version. Please update your game client."));
                            ResetAfterFailedConnectSniff();
                            yield break;
                        }
                    } else if (www.responseCode == 503) {
                        Debug.LogError("Service Unavailable: " + errorContent);
                    }
                    // Log the failure or take other actions
                    Debug.LogError("Failed to get server info...");
                    StartCoroutine(ServerSniffer(counter));
                    yield break;
                } else {
                    string content = www.downloadHandler.text;
                LoginRequestResponse loginResponse = JsonConvert.DeserializeObject<LoginRequestResponse>(content);
                // Use the values from the response
                playerCountVariable = "Players online: " + loginResponse.meta.playerCountVariable;
                if(loginResponse.meta.activeServers != "0"){
                    ServerInfoStatusImage.sprite = onlineSprite;
                    offlineOnlineinfo.text = "Server status: online";
                } else {
                    offlineOnlineinfo.text = "Server status: offline";
                }
                playerCountText.text = playerCountVariable;
                // Successfully joined the queue
               // print($"queue is ready lets go! active servers: {loginResponse.meta.activeServers} players online: {loginResponse.meta.playerCountVariable}");
                }
            }
        }
        IEnumerator ServerLogin(int counter) {
            counter ++;
            if(counter > 5){
                ResetAfterFailedConnect();
                yield break;
            }
            string id = this.PlayFabId;
            WWWForm form = new WWWForm();
            form.AddField("version", VERSION);
            form.AddField("id", id);
            using (UnityWebRequest www = UnityWebRequest.Post(loginServerURL + $"/clientlogin", form)) {
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError) {
                    Debug.LogError($"Error pinging server: {www.error}");
                    string errorContent = www.downloadHandler.text;
                    Debug.LogError($"Failed to join the queue. Message: {errorContent}");
                    if (www.responseCode == 400) {
                        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                        Debug.LogError("Bad Request: " + errorResponse.message);
                        if (errorResponse.message == "Version is required") {
                            Debug.LogError("The request is missing the version.");
                            if(errorDisplayRoutine != null){
                                StopCoroutine(errorDisplayRoutine);
                                errorDisplayRoutine = null;
                            }
                            errorDisplayRoutine = StartCoroutine(ErrorDisplay(errorResponse.message));
                            ResetAfterFailedConnectSniff();
                            yield break;
                        } else if (errorResponse.message.Contains("No servers available with the specified version")) {
                            Debug.LogError("No servers available with the specified version. Please update your game client.");
                            if(errorDisplayRoutine != null){
                                StopCoroutine(errorDisplayRoutine);
                                errorDisplayRoutine = null;
                            }
                            errorDisplayRoutine = StartCoroutine(ErrorDisplay("No servers available with the specified version. Please update your game client."));
                            ResetAfterFailedConnectSniff();
                            yield break;
                        }
                    } else if (www.responseCode == 503) {
                        Debug.LogError("Service Unavailable: " + errorContent);
                    }
                    // Log the failure or take other actions
                    Debug.LogError("Failed to get server info...");
                    StartCoroutine(ServerLogin(counter));
                    yield break;
                } else {
                    string content = www.downloadHandler.text;
                LoginRequestResponse loginResponse = JsonConvert.DeserializeObject<LoginRequestResponse>(content);
                // Use the values from the response
                buildID = loginResponse.meta.buildConnectID;
                SessionID = loginResponse.meta.sessionConnectID;
                print("queue is ready lets go! buildID: " + buildID + " sessionID: " + SessionID);
                if(buildID == "LoginConnect"){
                    //injectedIPAddress = loginResponse.meta.mirroredIPAddress;
                    NetworkClient.RegisterHandler<Noob>(NoobPlayer, false);
                    NetworkClient.RegisterHandler<PlayerInfo>(OnReceivePlayerInfo, false);
                    print($"response address was {loginResponse.meta.mirroredIPAddress} and the port was {loginResponse.meta.portString}");
                    this.networkAddress = loginResponse.meta.mirroredIPAddress;
                    this.GetComponent<TelepathyTransport>().port = ushort.Parse(loginResponse.meta.portString);
                    this.StartClient();
                } else {
                    RequestMultiplayerServerRequest requestData = new RequestMultiplayerServerRequest();
                    requestData.BuildId = buildID;
                    requestData.SessionId = SessionID;
                    requestData.PreferredRegions = new List<string>() { "EastUs" };
                    PlayFabMultiplayerAPI.RequestMultiplayerServer(requestData, OnRequestMultiplayerServer, OnRequestMultiplayerServerError);
                }
                }
            }
        }
    private async void RequestMultiplayerServer(int counter)
    {
        counter ++;
        if(counter > 5){
            ResetAfterFailedConnect();
            return;
        }
        string id = this.PlayFabId;
        versionTest versionRequest = new versionTest
        {
            version = VERSION
        };
        using (var httpClient = new HttpClient())
        {
            var jsonContent = JsonConvert.SerializeObject(versionRequest);
            var contentInject = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{loginServerURL}/clientlogin?id={id}", contentInject);  
            if (response.IsSuccessStatusCode)
            {
                // Parse the JSON response
                string content = await response.Content.ReadAsStringAsync();
                LoginRequestResponse loginResponse = JsonConvert.DeserializeObject<LoginRequestResponse>(content);
                // Use the values from the response
                buildID = loginResponse.meta.buildConnectID;
                SessionID = loginResponse.meta.sessionConnectID;
                print("queue is ready lets go! buildID: " + buildID + " sessionID: " + SessionID);
                if(buildID == "LoginConnect"){
                    NetworkClient.RegisterHandler<Noob>(NoobPlayer, false);
                    NetworkClient.RegisterHandler<PlayerInfo>(OnReceivePlayerInfo, false);
                    print($"response address was {loginResponse.meta.mirroredIPAddress} and the port was {loginResponse.meta.portString}");
                    this.networkAddress = loginResponse.meta.mirroredIPAddress;
                    this.GetComponent<TelepathyTransport>().port = ushort.Parse(loginResponse.meta.portString);
                    this.StartClient();
                } else {
                    RequestMultiplayerServerRequest requestData = new RequestMultiplayerServerRequest();
                    requestData.BuildId = buildID;
                    requestData.SessionId = SessionID;
                    requestData.PreferredRegions = new List<string>() { "EastUs" };
                    PlayFabMultiplayerAPI.RequestMultiplayerServer(requestData, OnRequestMultiplayerServer, OnRequestMultiplayerServerError);
                }
            }
            else
            {
                // Log the failure or take other actions based on status code and message
                string errorContent = await response.Content.ReadAsStringAsync();
                Debug.LogError($"Failed to join the queue. Status code: {response.StatusCode}, Message: {errorContent}");
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                    Debug.LogError("Bad Request: " + errorResponse.message);

                    if (errorResponse.message == "ID is required")
                    {
                        // Handle missing ID case
                        Debug.LogError("The request is missing the ID.");
                        StartCoroutine(ErrorDisplay(errorResponse.message));
                        return;
                    }
                    else if (errorResponse.message == "Version is required")
                    {
                        // Handle missing version case
                        Debug.LogError("The request is missing the version.");
                        StartCoroutine(ErrorDisplay(errorResponse.message));
                        return;
                    }
                    else if (errorResponse.message.Contains("No servers available with the specified version"))
                    {
                        // Handle no servers with the specified version case
                        Debug.LogError("No servers available with the specified version. Please update your game client.");
                        StartCoroutine(ErrorDisplay("No servers available with the specified version. Please update your game client."));
                        return;
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    Debug.LogError("Service Unavailable: " + errorContent);
                }
                RequestMultiplayerServer(counter);
            }
        }
    }
    private async void SuccessfullyCalledServerLogEfforts(int counter, string ip, string port, string sessionID)
    {
        counter ++;
        if(counter > 5){
            ResetAfterFailedConnect();
            return;
        }
        string id = this.PlayFabId;
        UpdateMultiplayerRequest  updateRequest = new UpdateMultiplayerRequest
        {
            ip = ip,
            port = port,
            sessionID = SessionID
        };
        using (var httpClient = new HttpClient())
        {
           var jsonContent = JsonConvert.SerializeObject(updateRequest);
            var contentInject = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{loginServerURL}/clientUpdateIP?id={id}", contentInject);  
            if (response.IsSuccessStatusCode)
            {
                print("Successfully logged our server we got from playfab to login");
            }
            else
            {
                // Log the failure or take other actions
                Debug.LogError("Failed to join the queue, retrying...");
                LoginServerMirrorAddress(counter);
                return;
                // Wait before the next retry
            }
        }
    }
    private void OnRequestMultiplayerServer(RequestMultiplayerServerResponse response)
    {
        NetworkClient.RegisterHandler<Noob>(NoobPlayer, false);
        NetworkClient.RegisterHandler<PlayerInfo>(OnReceivePlayerInfo, false);
        print($"response address was {response.IPV4Address}");
        this.networkAddress = response.IPV4Address;
        this.GetComponent<TelepathyTransport>().port = (ushort)response.Ports[0].Num;
        SuccessfullyCalledServerLogEfforts(0, response.IPV4Address, response.Ports[0].Num.ToString(), response.SessionId);
        this.StartClient();
    }
    private void OnRequestMultiplayerServerError(PlayFabError error)
    {
        Debug.Log(error.ErrorMessage);
        if (error != null && error.Error == PlayFabErrorCode.MultiplayerServerTooManyRequests)
        {
            // Handle 429 error (Rate Limit Exceeded)
            //ask login server for the proper info
            LoginServerMirrorAddress(0);
        }
        else
        {
            // Handle other PlayFab errors
            ResetAfterFailedConnect();
        }
    }
    private async void LoginServerMirrorAddress(int counter)
    {
        counter ++;
        if(counter > 5){
            ResetAfterFailedConnect();
            return;
        }
        string id = this.PlayFabId;
        using (var httpClient = new HttpClient())
        {
            var response = await httpClient.PostAsync($"{loginServerURL}/clientloginActiveState?id={id}", null);  
            if (response.IsSuccessStatusCode)
            {
                // Parse the JSON response
            string content = await response.Content.ReadAsStringAsync();
            LoginRequestResponse loginResponse = JsonConvert.DeserializeObject<LoginRequestResponse>(content);
            // Use the values from the response
            //buildID = loginResponse.meta.buildConnectID;
            //SessionID = loginResponse.meta.sessionConnectID;
            port = loginResponse.meta.portString;
            injectedIPAddress = loginResponse.meta.mirroredIPAddress;
            //NetworkClient.RegisterHandler<Noob>(NoobPlayer, false);
            //NetworkClient.RegisterHandler<PlayerInfo>(OnReceivePlayerInfo, false);
            //print($"response address was {injectedIPAddress} and the port was {port}");
            NetworkClient.RegisterHandler<Noob>(NoobPlayer);
            NetworkClient.RegisterHandler<PlayerInfo>(OnReceivePlayerInfo);
            this.networkAddress = injectedIPAddress;
            this.GetComponent<TelepathyTransport>().port = ushort.Parse(port);
            this.StartClient();
            }
            else
            {
                // Log the failure or take other actions
                Debug.LogError("Failed to join the queue, retrying...");
                LoginServerMirrorAddress(counter);
                return;
                // Wait before the next retry
            }
        }
    }
    public void ClientStarter(bool newPlayer){
        //ServerInfoStatus.enabled = false;
        if(ServerDataChecker != null){
            StopCoroutine(ServerDataChecker);
            ServerDataChecker = null;
        }
        ServerInfoStatus.enabled = false;
        if (!settings.GetComponent<Settings>().isLocalServer)
        {
            StartCoroutine(ServerLogin(LoginCounter));
        }
        else
        {
            NetworkClient.RegisterHandler<Noob>(NoobPlayer);
            NetworkClient.RegisterHandler<PlayerInfo>(OnReceivePlayerInfo);
            this.StartClient();
        }
        if(newPlayer){
            LoadScreenGameObject.SetActive(false);
            TipGO.SetActive(false);
        }
    }
    int LoginCounter = 0;
    async void LocalClientStart(int counter){
        string id = this.PlayFabId;
        counter ++;
        print(counter + " is our counter");
        using (var httpClient = new HttpClient())
        {
            var response = await httpClient.PostAsync($"{loginServerURL}/clientlogin?id={id}", null);  
            if (response.IsSuccessStatusCode)
            {
                // Parse the JSON response
            string content = await response.Content.ReadAsStringAsync();
            LoginRequestResponse loginResponse = JsonConvert.DeserializeObject<LoginRequestResponse>(content);

            // Use the values from the response
            buildID = loginResponse.meta.buildConnectID;
            SessionID = loginResponse.meta.sessionConnectID;
                // Successfully joined the queue
                print("queue is ready lets go!");
            }
            else
            {
                // Log the failure or take other actions
                Debug.LogError("Failed to join the queue, retrying...");
                if(counter < 5)
                LocalClientStart(counter);
                return;
                // Wait before the next retry
            }
        }
        NetworkClient.RegisterHandler<Noob>(NoobPlayer);
        NetworkClient.RegisterHandler<PlayerInfo>(OnReceivePlayerInfo);
        this.StartClient();
    }
    
}
}

