/**
 * Main Gateway Server with Telegram Bot
 */
import 'dotenv/config';
import express from 'express';
import cors from 'cors';
import ChatbotEngine from './services/ChatbotEngine.js';
import StateManager from './services/StateManager.js';
import adminRoutes from './routes/admin.js';
import { generateToken } from './middleware/Auth.js';

const app = express();
const PORT = process.env.PORT || 3000;

// Middleware
app.use(cors());
app.use(express.json());

// Health check
app.get('/health', (req, res) => {
    res.json({
        status: 'healthy',
        service: 'gateway',
        activeSessions: StateManager.getStats().activeSessions
    });
});

// Admin routes
app.use('/api/admin', adminRoutes);

// Auth endpoint (simplified - in production use proper user authentication)
/*app.post('/api/auth/login', (req, res) => {
    const { username, password } = req.body;

    // TODO: Implement proper authentication with database
    if (username === 'admin' && password === 'admin123') {
        const token = generateToken({ username, role: 'admin' });
        res.json({ token, username });
    } else {
        res.status(401).json({ message: 'Invalid credentials' });
    }
});*/
app.post('/api/auth/login', async (req, res) => {
    try {
        const response = await fetch(`${INVENTARIO_URL}/api/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(req.body)
        });
        const data = await response.json();
        res.status(response.status).json(data);
    } catch (error) {
        res.status(500).json({ message: "Error conectando con el servicio de identidad" });
    }
});

// State management debug endpoint (development only)
if (process.env.NODE_ENV === 'development') {
    app.get('/api/debug/sessions', (req, res) => {
        res.json(StateManager.getStats());
    });
}

// Initialize Telegram Bot
const TELEGRAM_BOT_TOKEN = process.env.TELEGRAM_BOT_TOKEN;
const INVENTARIO_URL = process.env.INVENTARIO_SERVICE_URL || 'http://localhost:5001';
const PAGOS_URL = process.env.PAGOS_SERVICE_URL || 'http://localhost:5002';

if (!TELEGRAM_BOT_TOKEN) {
    console.error('❌ TELEGRAM_BOT_TOKEN is not set in environment variables');
    process.exit(1);
}

console.log('🤖 Initializing Telegram Bot...');
const chatbot = new ChatbotEngine(TELEGRAM_BOT_TOKEN, INVENTARIO_URL, PAGOS_URL);

// Start server
app.listen(PORT, () => {
    console.log(`✅ Gateway server running on port ${PORT}`);
    console.log(`📦 Inventory Service: ${INVENTARIO_URL}`);
    console.log(`💳 Payments Service: ${PAGOS_URL}`);
    console.log(`🤖 Telegram Bot: Active`);
});

// Graceful shutdown
process.on('SIGTERM', () => {
    console.log('SIGTERM received, shutting down gracefully...');
    process.exit(0);
});
