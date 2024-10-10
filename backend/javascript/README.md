## Get started

To use these examples, make sure you have Node.js and Express.js. After that, clone this repo or copy the contents of this folder and run:

```bash
npm init -y
```

This creates and activates a virtual environment with all the requirements needed to run the examples.

## `authorize-purchases.js`

This is an example endpoint you'd add to your backend to authorize Stash to complete purchases. During the purchase process, Stash verifies whether the player has enough funds to cover the purchase and then calls this endpoint. This allows you to do your own verification in regard to player eligibility. If you authorize the purchase, Stash continues with the payment flow. There's a [guide](https://docs.stash.gg/docs/create-purchase-authorization-endpoint) in our docs for building this endpoint, as well as a [specifications](https://docs.stash.gg/docs/pay-authorize-purchase-specifications) page with more details.

To try this example, run `node authorize_purchases.js` and then use curl to test:

```curl
curl -X POST http://localhost:3000/purchase/player_id_123 -H "x-api-key: abc123" -H "Content-Type: application/json" -d '{
  "transactionId": "abc123",
  "items": [
    { "id": "sword_123", "quantity": 1 },
    { "id": "shield_1234", "quantity": 1 }
  ]
}'
```