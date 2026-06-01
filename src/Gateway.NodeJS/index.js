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
import axios from 'axios';

const app = express();
const PORT = process.env.PORT || 3000;

// Middleware
app.use(cors());
app.use(express.json());

// Initialize Telegram Bot
const TELEGRAM_BOT_URL = process.env.CHATBOT_SERVICE_URL || 'http://localhost:5003'
const TELEGRAM_BOT_TOKEN = process.env.TELEGRAM_BOT_TOKEN;
const INVENTARIO_URL = process.env.INVENTARIO_SERVICE_URL || 'http://localhost:5001';
const PAGOS_URL = process.env.PAGOS_SERVICE_URL || 'http://localhost:5002';

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
        // Usamos axios para mayor consistencia con el resto del archivo
        const response = await axios.post(`${INVENTARIO_URL}/api/auth/login`, req.body);
        res.status(response.status).json(response.data);
    } catch (error) {
        // Axios lanza errores automáticamente para códigos 4xx y 5xx
        console.error("❌ Error en Login Gateway:", error.message);
        const status = error.response?.status || 500;
        const message = error.response?.data?.message || "Error conectando con el servicio de identidad";
        res.status(status).json({ message });
    }
});

// State management debug endpoint (development only)
if (process.env.NODE_ENV === 'development') {
    app.get('/api/debug/sessions', (req, res) => {
        res.json(StateManager.getStats());
    });
}



if (!TELEGRAM_BOT_TOKEN) {
    console.error('❌ TELEGRAM_BOT_TOKEN is not set in environment variables');
    process.exit(1);
}

console.log('🤖 Initializing Telegram Bot...');
//const chatbot = new ChatbotEngine(TELEGRAM_BOT_TOKEN, INVENTARIO_URL, PAGOS_URL);
app.post('/api/webhook', async (req, res) => {
    try {
        const telegramUpdate = req.body;

        await axios.post(TELEGRAM_BOT_URL + '/api/bot', telegramUpdate, {
            headers: {
                'Content-Type': 'application/json',
                'X-Telegram-Bot-Api-Secret-Token': 'SOME-SECRET-STRING'
            }
        });
        res.sendStatus(200);
    } catch (error) {
        console.error("Error reenviando a .NET:", error);
        res.sendStatus(500);
    }
});

app.all('/api/pagos/*', async (req, res) => {
    const subPath = req.params[0];
    const targetUrl = `${PAGOS_URL}/api/pagos/${subPath}`;
    console.log(` Reenviando a: ${targetUrl}`);
        
    try {
        const response = await axios({
            method: req.method,
            url: targetUrl,
            data: req.body,
            params: req.query,
            headers: {
                //firmas de wompi
                'content-type': req.headers['content-type'] || 'application/json',
                'x-event-checksum': req.headers['x-event-checksum'],
                'user-agent': req.headers['user-agent']
            },
            maxRedirects: 0,
            validateStatus: (status) => status >= 200 && status < 400,
            timeout: 5000 // tiempo de solicitud
        });
        if(response.status >= 300 && response.status < 400){
            res.setHeader('Location', response.headers.location);
            return res.status(response.status).end();
        }

        res.status(response.status).json(response.data);
    } catch (error) {
        console.error(`Error ruteando al Pagos (${subPath}):`, error.message);
        res.status(error.response?.status || 500).json({
            message: "Error en el microservicio de pagos",
            details: error.message
        });
    }
});
/**
 * Proxy interno: El Bot llama aquí para obtener datos del Inventario
 * Ruta: /api/proxy/inventario/productos?page=0...
 */
app.get('/api/proxy/inventario/*', async (req, res) => {
    // Extraemos la parte de la URL después de /inventario/
    // Ejemplo: si el bot llama a /api/proxy/inventario/categorias, subPath es 'categorias'
    const subPath = req.params[0];
    const targetUrl = `${INVENTARIO_URL}/api/inventario/${subPath}`;
    console.log(`📡 Reenviando a: ${targetUrl}`);
    try {
        const response = await axios({
            method: 'GET',
            url: targetUrl,
            params: req.query, // Pasa los parámetros de paginación (?page=0&take=6)
            headers: {
                'Content-Type': 'application/json'
                // Aquí podrías añadir un token interno si quieres más seguridad
            }
        });
        res.status(response.status).json(response.data);
    } catch (error) {
        console.error(`❌ Error ruteando al Inventario (${subPath}):`, error.message);
        res.status(error.response?.status || 500).json({
            message: "Error en el microservicio de inventario",
            details: error.message
        });
    }
});

// Start server
app.listen(PORT, () => {
    console.log(`✅ Gateway server running on port ${PORT}`);
    console.log(`📦 Inventory Service: ${INVENTARIO_URL}`);
    console.log(`💳 Payments Service: ${PAGOS_URL}`);
    console.log(`🤖 Telegram Bot: ${TELEGRAM_BOT_URL}`);

});

// Graceful shutdown
process.on('SIGTERM', () => {
    console.log('SIGTERM received, shutting down gracefully...');
    process.exit(0);
});
