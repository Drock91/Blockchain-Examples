//Getting market price of a specific token from the XRP ledger with node.js 


app.post('/GetMarketPrice', async (req, res) => {
    const apiKeyFromRequest = req.headers['x-api-key'];
    const apiKeyFromEnv = process.env.CONVO_KEY; //keys come from databases with physical security keys to access and travel via HTTPS
      if (apiKeyFromRequest !== apiKeyFromEnv) {
          res.status(401).json({message: 'Unauthorized'});
          return;
      }
      // Parse the desired amount from the request body and convert it to a float
    const desiredAmount = parseFloat(req.body.amount);  // Assuming amount is passed as a string
      // Initialize an XRPL client and connect to it
    const client = new xrpl.Client('wss://xrplcluster.com');
    await client.connect();
     // Request the order book for XRP to DKP
     const orderBook = await client.request({
      command: 'book_offers',
      taker_pays: {
        currency: 'XRP'
      },
      taker_gets: {
        currency: 'DKP',
        issuer: 'rM7zpZQBfz9y2jEkDrKcXiYPitJx9YTS1J'
      },
      limit: 1000
    });
    await client.disconnect();
    // Sort the offers by rate in ascending order
    orderBook.result.offers.sort((a, b) => parseFloat(a.quality) - parseFloat(b.quality));
      // Initialize variables to hold aggregate liquidity and best rate
    let aggregateLiquidity = 0;
    let bestRate = 0;
      // Loop through sorted offers to find the best rate that can fulfill the desired amount
    for (const offer of orderBook.result.offers) {
      const availableDKP = parseFloat(offer.TakerGets.value);
      aggregateLiquidity += availableDKP;
      if (aggregateLiquidity >= desiredAmount) {
        bestRate = parseFloat(offer.quality);
        break;
      }
    }
     // Check if there's enough liquidity to fulfill the request
    if (bestRate === 0) {
      console.log('Not enough liquidity to fulfill 50,000 DKP.');
      res.json({ meta: { error: 'Not enough liquidity' } });
      return;
    }
    const bestMarketPrice = ((bestRate / 1000000 )* desiredAmount).toString();
    console.log(bestMarketPrice + " was our best market price");
    //sending the best market price for the amount we have requested, this wil be created in the game server now via xumm rest api
    const xummDetailedResponse = {
      meta: {
        bestMarketPrice: bestMarketPrice,
      }
    };
    res.json(xummDetailedResponse);
    //game server takes this request and generates a QR code to scan for the purchase and price. The example here was a registration fee of 50,000 DKP XLS-20 tokens
  });


  // this is an api route to send dkp from the game wallet to a players wallet via the XRPL 

  app.post('/dkpsend', async (req, res, next) => {
    console.log('We are listening')
    const apiKeyFromRequest = req.headers['x-api-key'];
      const apiKeyFromEnv = process.env.CONVO_KEY;
  
      if (apiKeyFromRequest !== apiKeyFromEnv) {
          res.status(401).json({message: 'Unauthorized'});
          return;
      }
    if (!req.body.userId || !req.body.walletAddress || !req.body.goldAmount) {
      res.status(400).send("Bad Request: Missing required parameters.");
      return;
    }
    const client = new xrpl.Client("wss://xrplcluster.com");
    await client.connect();
    const dkpWallet = xrpl.Wallet.fromSeed(process.env.SENDER_SEED);
    const currency_code = "DKP"
    // Send token ----------------------------------------------------------------
    const issue_quantity = req.body.goldAmount
    const send_token_tx = {
      "TransactionType": "Payment",
      "Account": process.env.SENDER_PUBLIC,
      "Amount": {
        "currency": currency_code,
        "value": req.body.goldAmount,
        "issuer": "rM7zpZQBfz9y2jEkDrKcXiYPitJx9YTS1J"
      },
      "Destination": req.body.walletAddress
    }
    const pay_prepared = await client.autofill(send_token_tx)
    const pay_signed = dkpWallet.sign(pay_prepared)
    const pay_result = await client.submitAndWait(pay_signed.tx_blob)
    console.log(pay_result);
    if (pay_result.result.meta.TransactionResult == "tesSUCCESS") {
      console.log(`Transaction succeeded: https://mainnet.xrpl.org/transactions/${pay_signed.hash}`)
      let xrpBalance = await client.getXrpBalance(req.body.walletAddress);
      let dkpBalance = "0";
      const balances = await client.getBalances(req.body.walletAddress);
      for (const balance of balances) {
        if (balance.currency === 'DKP') {
          dkpBalance = balance.value;
        }
      }
      const responseObj = {
        success: 'true',
        message: 'Transaction and gold removal successful',
        details: {
          userId: req.body.userId,
          goldAmount: req.body.goldAmount,
          walletAddress: req.body.walletAddress,
          xrpBalance: xrpBalance,
          dkpBalance: dkpBalance
        }
      }   
      res.status(200).json(responseObj);
    console.log(responseObj);
  } else {
    console.log(responseObj);
    console.log("failed");
    const responseObj = {
      success: 'false',
      message: 'Transaction and gold removal successful',
      details: {
          userId: userId,
          goldAmount: goldAmount,
          walletAddress: req.body.walletAddress
          //xrpBalance: xrpBalance,  // Added XRP balance
          //dkpBalance: dkpBalance   // Added DKP balance
      }
    };
    res.status(200).json(responseObj);
    console.log(responseObj);
    throw `Error sending transaction: ${pay_result.result.meta.TransactionResult}`
  }
  client.disconnect()
  });


  //custom Xaman (Xumm) xrp wallet integration
  // this is our callback for when a player makes an input on their xumm app
app.post('/xumm-webhook', async (req, res) => {
    console.log("starting webhook code")
    const timestamp = req.headers['x-xumm-request-timestamp'] || '';
    const json = req.body;
    const hmac = crypto.createHmac('sha1', process.env.XUMM_PRIVATE.replace('-', ''))
      .update(timestamp + JSON.stringify(json))
      .digest('hex');

    if (hmac !== req.headers['x-xumm-request-signature']) {
      console.warn('Signature mismatch. Possible tampering detected.');
      return res.status(401).send('Unauthorized');
    }
    const payloadId = req.body.payloadResponse.payload_uuidv4;
    console.log(req.body.payloadResponse.payload_uuidv4 + " this was our payloadResponse!")
    const _timestamp = Date.now();
    const isSigned = req.body.payloadResponse.signed;
    const customMetablob = req.body.custom_meta.blob;
    const txid = req.body.payloadResponse.txid;
    // You can push additional information to your pendingPayloadIds array if needed.
    console.log("adding to pendingPayloads !! ************************ LOOK FOR THIS IN LOG")
    pendingPayloadIds.push({ payloadId, _timestamp, isSigned, customMetablob, txid});
    res.status(200).send("OK");
});
// Cleanup old entries every 5 minutes
setInterval(() => {
  const fiveMinutesAgo = Date.now() - (5 * 60 * 1000);
  pendingPayloadIds = pendingPayloadIds.filter(item => item._timestamp > fiveMinutesAgo);
}, 5 * 60 * 1000);
async function getPayloadInfo(payloadId) {
  try {
    const response = await axios.get(`https://xumm.app/api/v1/platform/payload/${payloadId}`, { headers });
    if (response.status === 200) {
      return response;
    } else {
      return null;
    }
  } catch (error) {
    console.error(error);
    return null;
  }
}

//custom XRP Ledger NFT raffle logic

const xrpl = require('xrpl');
const serverURL = "wss://xrplcluster.com";
// XRP Ledger WebSocket server URL
const issuerWallet = 'rM7zpZQBfz9y2jEkDrKcXiYPitJx9YTS1J'; // DKP Issuer's wallet
const dkpCurrencyCode = 'DKP'; // Currency code for DKP
let wallets = new Map(); // To store wallets and their DKP balances
function AddRaffleWallets(){
}
AddRaffleWallets()
// Connect to the XRP Ledger
async function main() {
    const client = new xrpl.Client(serverURL);
    await client.connect();
    // Fetch and update DKP balances for each wallet
    for (let [wallet] of wallets) {
        const dkpBalance = await getDkpBalance(wallet, client);
        wallets.set(wallet, parseFloat(dkpBalance));
    }
    console.log("Wallets with their DKP balances:", wallets);
    await client.disconnect();
    // Roll dice to determine the winner
    rollDice(wallets);
}
// Function to fetch DKP balance for a wallet
async function getDkpBalance(wallet, client) {
    const response = await client.request({
        "command": "account_lines",
        "account": wallet,
        "ledger_index": "validated",
        "peer": issuerWallet
    });
    const lines = response.result.lines;
    const dkpLine = lines.find(line => line.currency === dkpCurrencyCode);
    return dkpLine ? dkpLine.balance : "0"; // Return 0 if DKP line doesn't exist
}
// Function to get the total number of votes
function getTotalVotes(wallets) {
    let total = 0;
    for (let balance of wallets.values()) {
        total += balance;
    }
    return total;
}
// Function to roll the dice and determine the winner
function rollDice(wallets) {
    let totalVotes = getTotalVotes(wallets);
    let roll = Math.floor(Math.random() * totalVotes);
    let currentRange = 0;
    for (const [wallet, dkp] of wallets.entries()) {
        if (roll < currentRange + dkp) {
            console.log(`Winner is wallet ${wallet} with a roll of ${roll}`);
            return wallet;
        }
        currentRange += dkp;
    }
}
main().catch(console.error);