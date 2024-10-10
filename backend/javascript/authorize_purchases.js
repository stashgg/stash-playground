const express = require('express');
const app = express();
const PORT = 3000;

// Middleware to parse JSON bodies.
app.use(express.json());

// Validate the API key before processing the call.
// Securely store API keys outside of this file.
// For example purposes only, a sample API key is
// used below.
const apiKeyValidation = (req, res, next) => {
    const apiKey = req.header('x-api-key');
    if (!apiKey || apiKey !== 'abc123') {
        return res.status(401).json({ error: 'Invalid API key' });
    }
    next();
};

// Endpoint to handle purchase requests.
app.post('/purchase/:playerId', apiKeyValidation, (req, res) => {
    const playerId = req.params.playerId;
    const transactionData = req.body;

    // Basic error handling for missing or invalid JSON.
    if (!transactionData || !transactionData.transactionId || !transactionData.items) {
        return res.status(400).json({ error: 'Invalid or missing JSON data' });
    }

    // This is where you'd trigger logic for granting
    // the player their items.
    // This example prints the player ID and transaction data.

    console.log(`Player ID: ${playerId}`);
    console.log('Transaction Data:', transactionData);

    res.status(200).json({ message: 'Purchase processed successfully' });
});

// Error handling middleware for invalid JSON.
app.use((err, req, res, next) => {
    if (err instanceof SyntaxError && err.status === 400 && 'body' in err) {
        return res.status(400).json({ error: 'Invalid JSON' });
    }
    next();
});

// Start the server
app.listen(PORT, () => {
    console.log(`Server is running on port ${PORT}`);
});