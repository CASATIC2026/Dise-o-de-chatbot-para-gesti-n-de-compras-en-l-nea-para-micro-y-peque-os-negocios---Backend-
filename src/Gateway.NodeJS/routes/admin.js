/**
 * Admin Routes - Proxy to microservices with authentication
 */
import express from 'express';
import axios from 'axios';
import { authenticateToken, requireRole } from '../middleware/Auth.js';

const router = express.Router();

// Si no hay .env, usara el nombre del servicio de Docker por defecto
const INVENTARIO_URL = process.env.INVENTARIO_SERVICE_URL || 'http://localhost:5041';
const PAGOS_URL = process.env.PAGOS_SERVICE_URL || 'http://pagos-service:8080';

// Aplicar autenticacion a todas las rutas de admin
router.use(authenticateToken);

const requireAdmin = requireRole('Administrador');
const requireAdminExceptGet = (req, res, next) => {
    if (req.method === 'GET') {
        return next();
    }

    return requireAdmin(req, res, next);
};

router.use('/inventario/usuarios', requireAdmin);
router.use('/inventario/conversaciones', requireAdmin);
router.use('/inventario/mensajes', requireAdmin);
router.use('/inventario/categorias', requireAdminExceptGet);

/**
 * Proxy generico para Inventario
 * Reenvia el token de autorizacion para superar la "doble validacion" en C#
 */
router.all('/inventario/*', async (req, res) => {
    try {
        const path = req.params[0];
        const url = `${INVENTARIO_URL}/api/inventario/${path}`;

        const response = await axios({
            method: req.method,
            url,
            data: req.body,
            params: req.query,
            headers: {
                'Content-Type': 'application/json',
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

const buildPagosTargetUrl = (path = '') => {
    const normalizedPath = path ? `/${path}` : '';
    const wompiPaths = [
        '/crear-enlace-automatico',
        '/webhook/wompi'
    ];

    const targetBaseUrl = wompiPaths.some(prefix => normalizedPath.startsWith(prefix))
        ? PAGOS_URL
        : INVENTARIO_URL;

    return `${targetBaseUrl}/api/pagos${normalizedPath}`;
};

const proxyPagosRequest = async (req, res, path = '') => {
    try {
        const url = buildPagosTargetUrl(path);

        const response = await axios({
            method: req.method,
            url,
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
};

/**
 * Proxy para Pagos:
 * - CRUD administrativo -> Inventario (/api/pagos)
 * - Integraciones Wompi -> Pagos (/api/pagos/crear-enlace-automatico, /webhook/wompi)
 */
router.all('/pagos', async (req, res) => {
    await proxyPagosRequest(req, res);
});

router.all('/pagos/*', async (req, res) => {
    await proxyPagosRequest(req, res, req.params[0]);
});

/**
 * Endpoint del Dashboard
 * Agrega datos de los microservicios enviando el token en cada peticion
 */
router.get('/dashboard/stats', async (req, res) => {
    try {
        const authHeader = req.headers.authorization;

        const [productosRes] = await Promise.all([
            axios.get(`${INVENTARIO_URL}/api/inventario/productos`, {
                headers: { 'Authorization': authHeader }
            }),
            Promise.resolve({ data: [] })
        ]);

        const productos = Array.isArray(productosRes.data)
            ? productosRes.data
            : (productosRes.data?.items || productosRes.data?.Items || []);
        const pedidos = [];

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
