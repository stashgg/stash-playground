## Get started

To use these examples, make sure you have Python 3 installed and then run:

1. `python3 -m venv .venv`
2. `. .venv/bin/activate`
3. `pip install -r requirements.txt`

This creates and activates a virtual environment with all the requirements needed to run the examples.

## `authorize-purchases.py`

This is an example endpoint you'd add to your backend to authorize Stash to complete purchases. During the purchase process, Stash verifies whether the player has enough funds to cover the purchase and then calls this endpoint. This allows you to do your own verification in regard to player eligibility. If you authorize the purchase, Stash continues with the payment flow. There's a [guide](https://docs.stash.gg/docs/create-purchase-authorization-endpoint) in our docs for building this endpoint, as well as a [specifications](https://docs.stash.gg/docs/pay-authorize-purchase-specifications) page with more details.

To try this example, run `flask --app authorize_purchases.py run --debug` and then use curl to test:

```curl
curl -X POST http://127.0.0.1:5000/purchase/player_id_123 \
     -H "Content-Type: application/json" \
     -H "x-api-key: cad1" \
     -d '{
          "transactionId": "abc123",
          "items": [
            { "id": "sword_123", "quantity": 1 },
            { "id": "shield_1234", "quantity": 1 }
          ]
        }'
```