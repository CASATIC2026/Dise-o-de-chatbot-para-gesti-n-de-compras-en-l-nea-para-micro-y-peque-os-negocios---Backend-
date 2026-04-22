/**
 * Admin Routes - Proxy to microservices with authentication
 */
import express from 'express';
import axios from 'axios';
import { authenticateToken } from '../middleware/Auth.js';

const router = express.Router();

// Si no hay .env, usará el nombre del servicio de Docker por defecto
const INVENTARIO_URL = process.env.INVENTARIO_SERVICE_URL || 'http://inventario-service:8080';
const PAGOS_URL = process.env.PAGOS_SERVICE_URL || 'http://pagos-service:8080';

// Aplicar autenticación a todas las rutas de admin
router.use(authenticateToken);

/**
 * Proxy genérico para Inventario
 * Reenvía el token de autorización para superar la "doble validación" en C#
 */
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
                'Content-Type': 'application/json',
                // ENVIAR EL TOKEN AL MICROSERVICIO
                'Authorization': req.headers.authorization 
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

/**
 * Proxy genérico para Pagos
 */
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
                'Content-Type': 'application/json',
                'Authorization': req.headers.authorization
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

/**
 * Endpoint del Dashboard
 * Agrega datos de los microservicios enviando el token en cada petición
 */
router.get('/dashboard/stats', async (req, res) => {
    try {
        const authHeader = req.headers.authorization;

        // Llamada al microservicio de C# (Inventario)
        const [productosRes] = await Promise.all([
            axios.get(`${INVENTARIO_URL}/api/inventario/productos`, {
                headers: { 'Authorization': authHeader }
            }),
            // Espacio para futuros microservicios (Pedidos, etc.)
            Promise.resolve({ data: [] })
        ]);

        const productos = Array.isArray(productosRes.data)
            ? productosRes.data
            : (productosRes.data?.items || productosRes.data?.Items || []);
        const pedidos = []; // Placeholder

        res.json({
            totalProductos: productos.length,
            productosActivos: productos.filter(p => p.activo !== false).length,
            totalPedidos: pedidos.length,
            stockBajo: productos.filter(p => {
                const stock = p.stock ?? p.stockTotal ?? p.Stock ?? p.StockTotal ?? 0;
                return stock < 10;
            }).length
        });
    } catch (error) {
        console.error('[Admin] Error fetching dashboard stats:', error.message);
        res.status(500).json({ 
            message: 'Error fetching statistics',
            details: error.response?.data || error.message 
        });
    }
});

export default router;
