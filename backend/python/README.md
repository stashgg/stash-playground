## Get started

To use these examples, make sure you have Python 3 installed and then run:

1. `python3 -m venv .venv`
2. `. .venv/bin/activate`
3. `pip install -r requirements.txt`

This creates and activates a virtual environment with all the requirements needed to run the examples.

## `authorize_purchases.py`

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

## `get_catalog.py`

This is an example endpoint you'd add to your backend to pass your web shop catalog to Stash. Defining the endpoint on your end allows you to update the catalog whenever you want. Stash calls this endpoint every time the web shop is loaded, so players always see the latest version of your catalog. See the [specifications](https://docs.stash.gg/docs/get-catalog-specifications) page for more information about this endpoint.

To try this example, run `flask --app get_catalog.py run --debug` and then use curl to test:

```curl
curl -X GET "http://localhost:5000/get-catalog?player_id=1234&language_code=en" \
     -H "x-api-key: cad1"
```