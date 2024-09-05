# Code portfolio of DragonKill
## TABLE OF CONTENTS
- [Overview](#overview)
- [Features](#features)
- [Technologies Used](#technologies-used)
- [Architecture](#architecture)
- [Future Plans](#future-plans)
- [Getting Started](#getting-started)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

OVERVIEW
-
Welcome to DragonKill, an innovative 2D fantasy Massively Multiplayer Online Role-Playing Game (MMORPG) that incorporates Play-to-Earn mechanisms and blockchain technology. Designed with a decentralized architecture, our metaverse offers a secure and reliable gaming experience. In DragonKill, players have the opportunity to earn DKP tokens, which are seamlessly tradeable on the XRP Ledger decentralized exchange for a variety of currencies. Additionally, DKP can be converted into in-game gold, allowing players to engage with our in-game marketplace and cover associated gameplay costs.


FEATURES
  -

* Server-Authority Mechanism: Enabled by Mirror networking, ensuring synchronized and trustworthy game states.

* Currency and Assets: Utilize playfab.com-generated currencies that integrate with XRP Ledger.

* Multi-platform Support: Aimed for browser-based gaming experiences, -currently standalone windows.

TECHNOLOGIES USED
-

* Heroku App: Central controller for game servers. Responsible for scaling and load balancing.

* Unity 2020.3: The game engine behind the metaverse.

* Mirror Networking: Handles server authority for reliable information synchronization.

* XRP Ledger & Xumm Wallet: For cryptocurrency and asset management.

* XRPscan API: Fetches balances and NFT data.

* PlasticSCM: Version control

ARCHITECTURE
-
* Heroku node.js App - Contains index.js-Includes necessary XRPL libraries for crypto signing 
  
  - [xrplintegration.js](xrplintegration.js) contains market order purchasing for registration if player needs to purchase DKP, it also has Xumm webhooks and other various calls to the XRP Ledger to include sending DKP to the player for transmuting in-game gold currency generated by playfab to DKP

* Unity 2020.3 - Game engine where the metaverse is constructed
  
  - [PlayFabClient.cs](PlayFabClient.cs) - network manager for client
  
  - [PlayFabServer.cs](PlayFabServer.cs) network manager for game server
  
  - [ScenePlayer.cs](ScenePlayer.cs) networked player script

  - [PlayerCharacter.cs](PlayerCharacter.cs) networked character script

  - [Mob.cs](Mob.cs) networked enemy script

  - [MovingObject.cs](MovingObject.cs) inheritted class from PlayerCharacter and Mob so they can speak to each other easily and share same qualities with polymorphism

  - [MatchMaking.cs](MatchMaking.css) script for generating different matches with players in it on the server

  - [Door.cs](Door.cs) script for opening networked doors

  - [ArmorDrop.cs](ArmorDrop.cs) used for looting armor stands that are networked

  - [MainChest.cs](MainChest.cs) script used to deliver in-game currency to players so they can convert to DKP XLS-20 token or spend on the market in-game.

  - [WeaponDrop.cs](WeaponDrop.cs) used for looting weapon stands that are networked

  - [TrapDrop.cs](TrapDrop.cs) networked code for damaging characters

* Mirror Networking - Ensures server authority and information reliability

* dragonkill.online Website - Placeholder website currently, will houses the WebGL build for browser-based gameplay (future)

* XRP Ledger & Xumm Wallet - Manage and store game currencies

  - XRP Ledger code can be found below in the following scripts and methods
  - [PlayFabServer.cs](PlayFabServer.cs)
    -
    - LINE 469 Method VerifyPlayerPayment - used to determine if a player has already registered with an XRP wallet before registering a wallet. 
    - LINE 496 Method GetTransactionHistory - used with VerifyPlayerPayment to get tx history of our registration xrp address to check if their wallet has paid           before for registration. if it has its rejected
    - LINE 584 Method RegisterTrustSet - used for setting a trust line with DKP on registration into the game
    - LINE 991 Method PurchaseDKPMarketPriceRegistration - used to get best price of DKP from our index.js script on our heroku app using xrpl library.
    - LINE 1536 Method SubmitBlobToXRPL - used to submit a blob for processing in the XRPL
    - LINE 1728 Method CheckXummStatusAPP - used to poll our heroku app for the Xumm webhook callback. 
    - LINE 1752 Method DKPTOGOLDTRANSMUTE - used to transmute DKP XLS-20 token to in-game gold the playfab virtual currency. 
  - [Index.js](Index.js)
    -
    - LINE 43 post/GetMarketPrice - gets best avail price of DKP from the XRP Ledger and sends to game server for processing their order
    - LINE 106 post/dkpsend - sends the XLS-20 DKP token to their Xumm wallet and tells the game server to burn the appropriate amount of gold from their playfab account
    - LINE 220 get/check-payload/:payloadId/:walletAddress - polling method from game servers to check if a payload has been completed and sends information to game server for processing
    - LINE 520 async function checkTrustline - checks trustline of account
    - LINE 543 post/balance - not in use anymore we used to use this to check their balances
  


FUTURE PLANS
-
Transition from Windows containers to Linux containers for better transport interoperability.
WebGL build to enable browser-based gameplay. House and guild plot NFT integration into main town, sandbox marketplace to link multiple game servers for one world effect. There is so much more to come!


CONTRIBUTING
-
We appreciate all contributions. See  [CONTRIBUTING.md](CONTRIBUTING.md) for details.


LICENSE
-
This project is licensed under the EULA License - see the [LICENSE.md](LICENSE.md) file for details.


CONTACT
-
Project Maintainer: [Derek Heinrichs]

Email: [customersupport@dragonkill.online]

Project Link: [dragonkill.online](https://www.dragonkill.online)
