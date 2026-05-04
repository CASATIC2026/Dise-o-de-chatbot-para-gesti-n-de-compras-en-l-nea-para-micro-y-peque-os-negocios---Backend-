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

const normalizeArray = (payload) => {
    if (Array.isArray(payload)) {
        return payload;
    }

    if (Array.isArray(payload?.items)) {
        return payload.items;
    }

    if (Array.isArray(payload?.Items)) {
        return payload.Items;
    }

    return [];
};

const toNumber = (value) => {
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : 0;
};

const parseDate = (value) => {
    if (!value) {
        return null;
    }

    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? null : date;
};

const isSameDay = (left, right) =>
    left.getUTCFullYear() === right.getUTCFullYear() &&
    left.getUTCMonth() === right.getUTCMonth() &&
    left.getUTCDate() === right.getUTCDate();

const formatWeekday = (date) =>
    date.toLocaleDateString('es-CO', { weekday: 'short', timeZone: 'UTC' })
        .replace('.', '')
        .replace(/^./, (char) => char.toUpperCase());

const buildLastSevenDaysSeries = (items, getDate, getValue, labelKey, valueKey) => {
    const today = new Date();
    const days = Array.from({ length: 7 }, (_, index) => {
        const date = new Date(Date.UTC(
            today.getUTCFullYear(),
            today.getUTCMonth(),
            today.getUTCDate() - (6 - index)
        ));

        return {
            date,
            [labelKey]: formatWeekday(date),
            [valueKey]: 0
        };
    });

    items.forEach((item) => {
        const itemDate = parseDate(getDate(item));
        if (!itemDate) {
            return;
        }

        const utcDate = new Date(Date.UTC(
            itemDate.getUTCFullYear(),
            itemDate.getUTCMonth(),
            itemDate.getUTCDate()
        ));

        const bucket = days.find((day) => isSameDay(day.date, utcDate));
        if (!bucket) {
            return;
        }

        bucket[valueKey] += toNumber(getValue(item));
    });

    return days.map(({ date, ...rest }) => rest);
};

const buildTodayHourlyRevenue = (pagos) => {
    const today = new Date();
    const buckets = Array.from({ length: 6 }, (_, index) => {
        const hour = 8 + (index * 2);
        return {
            hour,
            time: `${hour.toString().padStart(2, '0')}:00`,
            revenue: 0
        };
    });

    pagos.forEach((pago) => {
        const pagoDate = parseDate(pago.fechaPago ?? pago.FechaPago ?? pago.creadoEn ?? pago.CreadoEn);
        if (!pagoDate || !isSameDay(pagoDate, today)) {
            return;
        }

        const hour = pagoDate.getUTCHours();
        const bucketIndex = Math.max(0, Math.min(buckets.length - 1, Math.floor((hour - 8) / 2)));
        const bucket = buckets[bucketIndex];

        if (!bucket || hour < 8) {
            return;
        }

        bucket.revenue += toNumber(pago.monto ?? pago.Monto);
    });

    return buckets.map(({ hour, ...rest }) => rest);
};

const calculateTrend = (current, previous) => {
    if (!previous) {
        if (!current) {
            return '0%';
        }

        return '+100%';
    }

    const percentage = ((current - previous) / previous) * 100;
    const rounded = Math.abs(percentage).toFixed(1);

    if (percentage === 0) {
        return '0%';
    }

    return `${percentage > 0 ? '+' : '-'}${rounded}%`;
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

        const [productosRes, pedidosRes, pagosRes, clientesRes] = await Promise.all([
            axios.get(`${INVENTARIO_URL}/api/inventario/productos`, {
                params: { soloActivos: false },
                headers: { 'Authorization': authHeader }
            }),
            axios.get(`${INVENTARIO_URL}/api/inventario/pedidos`, {
                headers: { 'Authorization': authHeader }
            }),
            axios.get(`${INVENTARIO_URL}/api/pagos`, {
                headers: { 'Authorization': authHeader }
            }),
            axios.get(`${INVENTARIO_URL}/api/inventario/clientes`, {
                headers: { 'Authorization': authHeader }
            })
        ]);

        const productos = normalizeArray(productosRes.data);
        const pedidos = normalizeArray(pedidosRes.data);
        const pagos = normalizeArray(pagosRes.data);
        const clientes = normalizeArray(clientesRes.data);
        const now = new Date();

        const totalVentasHoy = pagos
            .filter((pago) => {
                const pagoDate = parseDate(pago.fechaPago ?? pago.FechaPago ?? pago.creadoEn ?? pago.CreadoEn);
                return pagoDate && isSameDay(pagoDate, now);
            })
            .reduce((sum, pago) => sum + toNumber(pago.monto ?? pago.Monto), 0);

        const pedidosSemanaActual = buildLastSevenDaysSeries(
            pedidos,
            (pedido) => pedido.creadoEn ?? pedido.CreadoEn,
            () => 1,
            'name',
            'ventas'
        );

        const ingresosHoy = buildTodayHourlyRevenue(pagos);

        const currentWeekOrders = pedidosSemanaActual.reduce((sum, item) => sum + item.ventas, 0);
        const previousWeekOrders = pedidos
            .filter((pedido) => {
                const date = parseDate(pedido.creadoEn ?? pedido.CreadoEn);
                if (!date) {
                    return false;
                }

                const diffDays = Math.floor((now - date) / (1000 * 60 * 60 * 24));
                return diffDays >= 7 && diffDays < 14;
            })
            .length;

        const todayRevenueYesterday = pagos
            .filter((pago) => {
                const date = parseDate(pago.fechaPago ?? pago.FechaPago ?? pago.creadoEn ?? pago.CreadoEn);
                if (!date) {
                    return false;
                }

                const yesterday = new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate() - 1));
                const candidate = new Date(Date.UTC(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate()));
                return isSameDay(candidate, yesterday);
            })
            .reduce((sum, pago) => sum + toNumber(pago.monto ?? pago.Monto), 0);

        const productosActivos = productos.filter((p) => p.activo !== false && p.Activo !== false).length;
        const productosInactivos = Math.max(productos.length - productosActivos, 0);
        const stockBajo = productos.filter((p) => {
            const stock = p.stock ?? p.stockTotal ?? p.Stock ?? p.StockTotal ?? 0;
            return stock < 10;
        }).length;

        res.json({
            totalClientes: clientes.length,
            totalProductos: productos.length,
            productosActivos,
            productosInactivos,
            totalPedidos: pedidos.length,
            totalPagos: pagos.length,
            totalVentasHoy,
            stockBajo,
            trends: {
                totalVentasHoy: calculateTrend(totalVentasHoy, todayRevenueYesterday),
                totalPedidos: calculateTrend(currentWeekOrders, previousWeekOrders),
                productosActivos: productosInactivos > 0 ? `${productosActivos}/${productos.length}` : 'Estable',
                stockBajo: productos.length ? `${((stockBajo / productos.length) * 100).toFixed(1)}%` : '0%'
            },
            charts: {
                salesData: pedidosSemanaActual,
                revenueData: ingresosHoy
            }
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
