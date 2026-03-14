/**
 * Admin Routes - Proxy to microservices with authentication
 */
import express from 'express';
import axios from 'axios';
import { authenticateToken } from '../middleware/Auth.js';

const router = express.Router();

const INVENTARIO_URL = process.env.INVENTARIO_SERVICE_URL || 'http://localhost:5001';
const PAGOS_URL = process.env.PAGOS_SERVICE_URL || 'http://localhost:5002';

// Apply authentication to all admin routes
router.use(authenticateToken);

// Proxy to Inventory Service
router.all('/inventario/*', async (req, res) => {
    try {
        const path = req.params[0];
        const url = `${INVENTARIO_URL}/api/inventario/${path}`;

        const response = await axios({
            method: req.method,
            url: url,
            data: req.body,
            params: req.query,
            headers: {
                'Content-Type': 'application/json'
            }
        });

        res.status(response.status).json(response.data);
    } catch (error) {
        console.error('[Admin] Error proxying to Inventario:', error.message);
        res.status(error.response?.status || 500).json({
            message: 'Error communicating with Inventory service',
            error: error.response?.data || error.message
        });
    }
});

// Proxy to Payments Service
router.all('/pagos/*', async (req, res) => {
    try {
        const path = req.params[0];
        const url = `${PAGOS_URL}/api/pagos/${path}`;

        const response = await axios({
            method: req.method,
            url: url,
            data: req.body,
            params: req.query,
            headers: {
                'Content-Type': 'application/json'
            }
        });

        res.status(response.status).json(response.data);
    } catch (error) {
        console.error('[Admin] Error proxying to Pagos:', error.message);
        res.status(error.response?.status || 500).json({
            message: 'Error communicating with Payments service',
            error: error.response?.data || error.message
        });
    }
});

// Dashboard statistics endpoint
router.get('/dashboard/stats', async (req, res) => {
    try {
        // Aggregate data from multiple services
        const [productosRes, pedidosRes] = await Promise.all([
            axios.get(`${INVENTARIO_URL}/api/inventario/productos`),
            // Add pedidos endpoint when implemented
            Promise.resolve({ data: [] })
        ]);

        const productos = productosRes.data;
        const pedidos = pedidosRes.data;

        res.json({
            totalProductos: productos.length,
            productosActivos: productos.filter(p => p.activo).length,
            totalPedidos: pedidos.length,
            stockBajo: productos.filter(p => p.stock < 10).length
        });
    } catch (error) {
        console.error('[Admin] Error fetching dashboard stats:', error.message);
        res.status(500).json({ message: 'Error fetching statistics' });
    }
});

export default router;
