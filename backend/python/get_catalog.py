from flask import Flask, request, jsonify

app = Flask(__name__)

# Securely store API keys outside of this file.
# For example purposes only, a sample API key is
# used below.

API_KEY = "cad1"

# For example purposes, the catalog data is hard coded below.
# In a production scenario, this information would likely come
# from a database or another API.

catalog_data = {
    "rows": [
        {
            "header": "Featured",
            "items": [
                {
                    "id": "sword_123",
                    "name": "My cool sword",
                    "description": "This is really flashy",
                    "max_purchasable": 1,
                    "expiration_time_seconds": 1721319873,
                    "price": {
                        "currency": "USD",
                        "amount": "11.2"
                    },
                    "image": "https://my.server/image/sword_123.png"
                }
            ]
        },
        {
            "items": [
                {
                    "id": "shield_1234",
                    "name": "My cool shield",
                    "description": "This is really sturdy",
                    "price": {
                        "currency": "USD",
                        "amount": "20.3"
                    },
                    "image": "https://my.server/image/shield_1234.png"
                }
            ]
        }
    ]
}

@app.route('/get-catalog', methods=['GET'])
def get_catalog():
    # Retrieve the player ID and language code from
    # query parameters. You can use the player ID to
    # customize the items players see in the shop.
    # The language code is used for localization.
    player_id = request.args.get('player_id')
    language_code = request.args.get('language_code')

    # Validate the API key before processing the call
    api_key = request.headers.get('x-api-key')
    if not api_key or api_key != API_KEY:
        return jsonify({"error": "Unauthorized"}), 401

    # Check for required parameters
    if not player_id or not language_code:
        return jsonify({"error": "Missing player_id or language_code parameters"}), 400

    # Return the catalog after validating the request
    return jsonify(catalog_data), 200

if __name__ == '__main__':
    app.run(debug=True)
