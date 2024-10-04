from flask import Flask, request, jsonify

app = Flask(__name__)

# Securely store API keys outside of this file.
# For example purposes only, a sample API key is
# used below.

API_KEY = "cad1"

@app.route('/purchase/<player_id>', methods=['POST'])
def handle_transaction(player_id):
    # Validate the API key before processing the call
    api_key = request.headers.get('x-api-key')
    if not api_key or api_key != API_KEY:
        return jsonify({"error": "Unauthorized"}), 401
    if request.is_json:
        try:
            # Parse the JSON content from the request
            data = request.get_json()
            # Access transactionId and items from the JSON object
            transaction_id = data['transactionId']
            items = data['items']
            # This is where you'd trigger logic for granting
            # the player their items.
            # This example just prints the player ID, transaction ID,
            # and the items purchased.
            print(f'Player ID: {player_id}')
            print(f'Transaction ID: {transaction_id}')
            for item in items:
                print(f'Item ID: {item["id"]}, Quantity: {item["quantity"]}')
            # Respond with a 200 OK status after you grant the player their
            # items. This notifies Stash that we can charge the player.
            return jsonify({"message": "Transaction processed successfully"}), 200
        except KeyError as e:
            # If required keys are missing, respond with a 400 Bad Request
            return jsonify({"error": f"Missing key: {str(e)}"}), 400
    else:
        # If the request content-type is not JSON, respond with a 400 Bad Request
        return jsonify({"error": "Request must be JSON"}), 400

if __name__ == '__main__':
    app.run(debug=True)
