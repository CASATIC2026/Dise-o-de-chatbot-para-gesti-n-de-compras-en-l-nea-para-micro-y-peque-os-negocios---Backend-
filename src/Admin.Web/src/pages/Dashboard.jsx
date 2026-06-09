import { useEffect, useState } from 'react';
import { AreaChart, Area, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import api from '../api/client';
import { MoneyIcon, OrdersIcon, InventoryIcon, AlertIcon, CheckCircleIcon } from '../components/Icons';
import { getNotifColor, getNotifIcon } from '../utils/notifications';

function Dashboard({ notifications = [] }) {
    const [stats, setStats] = useState(null);
    const [loading, setLoading] = useState(true);
    const [widgets, setWidgets] = useState({
        Estadisticas: true,
        Ventas: true,
        Ingresos: true,
        Actividad: true
    });
    const [isDropdownOpen, setIsDropdownOpen] = useState(false);
    const userTimeZoneOffsetMinutes = new Date().getTimezoneOffset();

    useEffect(() => {
        fetchStats();
    }, []);

    const fetchStats = async () => {
        try {
            const response = await api.get('/admin/dashboard/stats', {
                params: {
                    timeZoneOffsetMinutes: userTimeZoneOffsetMinutes
                }
            });
            setStats(response.data);
        } catch (error) {
            console.error('[Dashboard] Error en stats:', error);
        } finally {
            setLoading(false);
        }
    };

    const toggleWidget = (widget) => {
        setWidgets((prev) => ({ ...prev, [widget]: !prev[widget] }));
    };

    const formatCurrency = (value) =>
        `$${Number(value || 0).toLocaleString('es-CO', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        })}`;

    if (loading) {
        return (
            <div className="flex items-center justify-center h-64">
                <div className="flex flex-col items-center">
                    <div className="w-10 h-10 border-4 border-primary-200 border-t-primary-600 rounded-full animate-spin mb-4"></div>
                    <div className="text-gray-500 font-medium tracking-wide">Cargando métricas...</div>
                </div>
            </div>
        );
    }

    const statCards = [
        {
            title: 'Ingresos Totales (Hoy)',
            value: formatCurrency(stats?.totalVentasHoy),
            icon: <MoneyIcon />,
            colorText: 'text-primary-600',
            bgIcon: 'bg-primary-50',
            trend: stats?.trends?.totalVentasHoy || '0%'
        },
        {
            title: 'Pedidos',
            value: stats?.totalPedidos ?? 0,
            icon: <OrdersIcon />,
            colorText: 'text-secondary-600',
            bgIcon: 'bg-secondary-50',
            trend: stats?.trends?.totalPedidos || '0%'
        },
        {
            title: 'Productos Activos',
            value: stats?.productosActivos ?? 0,
            icon: <InventoryIcon />,
            colorText: 'text-tertiary-600',
            bgIcon: 'bg-tertiary-50',
            trend: stats?.trends?.productosActivos || 'Estable'
        },
        {
            title: 'Stock Bajo',
            value: stats?.stockBajo ?? 0,
            icon: <AlertIcon />,
            colorText: 'text-red-500',
            bgIcon: 'bg-red-50',
            trend: stats?.trends?.stockBajo || '0%'
        },
    ];

    const salesData = stats?.charts?.salesData || [];
    const revenueData = stats?.charts?.revenueData || [];

    return (
        <div className="animate-fade-in">
            <div className="flex flex-col md:flex-row md:items-end justify-between mb-8 pb-4 border-b border-gray-200 dark:border-gray-700 gap-4">
                <div>
                    <h1 className="text-3xl font-bold text-gray-800 dark:text-gray-100 tracking-tight">Dashboard Financiero</h1>
                    <p className="text-gray-500 dark:text-gray-400 mt-1">Monitorea los KPIs de tu E-commerce en tiempo real</p>
                </div>

                {/* Opciones de personalizar (Desktop) */}
                <div className="hidden lg:flex items-center gap-2 bg-white dark:bg-gray-800 p-2 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm">
                    <span className="text-sm font-medium text-gray-500 dark:text-gray-300 px-2 border-r border-gray-200 dark:border-gray-600">Personalizar</span>
                    {Object.keys(widgets).map((key) => (
                        <button
                            key={key}
                            onClick={() => toggleWidget(key)}
                            className={`px-3 py-1.5 text-xs font-semibold rounded-lg transition-all ${widgets[key]
                                ? 'bg-primary-50 dark:bg-cyan-900/20 text-primary-600 dark:text-cyan-400'
                                : 'bg-gray-100 dark:bg-gray-700 text-gray-400 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-600'
                                }`}
                        >
                            {key}
                        </button>
                    ))}
                </div>

                {/* Opciones de personalizar (Mobile Collapsible) */}
                <div className="lg:hidden relative w-full sm:w-auto mt-2 md:mt-0">
                    <button 
                        onClick={() => setIsDropdownOpen(!isDropdownOpen)}
                        className="flex items-center justify-between w-full sm:w-64 bg-white dark:bg-gray-800 px-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm text-sm font-semibold text-gray-700 dark:text-gray-200 hover:bg-gray-50 dark:hover:bg-gray-700/80 transition-colors"
                    >
                        <span>Personalizar Widgets</span>
                        <svg className={`w-4 h-4 transition-transform duration-300 ${isDropdownOpen ? 'rotate-180' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" /></svg>
                    </button>
                    {isDropdownOpen && (
                        <div className="absolute top-full right-0 left-0 sm:left-auto sm:right-0 mt-2 sm:w-64 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-xl shadow-lg z-10 p-2 flex flex-col gap-1 animate-fade-in">
                            {Object.keys(widgets).map((key) => (
                                <button
                                    key={key}
                                    onClick={() => toggleWidget(key)}
                                    className={`flex items-center justify-between px-3 py-2.5 text-sm font-semibold rounded-lg transition-all ${widgets[key]
                                        ? 'bg-primary-50 dark:bg-cyan-900/20 text-primary-600 dark:text-cyan-400'
                                        : 'text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'
                                        }`}
                                >
                                    <span>{key}</span>
                                    <div className={`w-2.5 h-2.5 rounded-full shadow-sm ${widgets[key] ? 'bg-primary-500 dark:bg-cyan-400 shadow-primary-500/50' : 'bg-gray-300 dark:bg-gray-600'}`}></div>
                                </button>
                            ))}
                        </div>
                    )}
                </div>
            </div>

            {widgets.Estadisticas && (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
                    {statCards.map((stat) => (
                        <div
                            key={stat.title}
                            className="bg-white dark:bg-gray-800 rounded-2xl border border-gray-200 dark:border-gray-700 p-6 shadow-sm hover:shadow-md transition-all duration-300 group"
                        >
                            <div className="flex items-start justify-between">
                                <div>
                                    <p className="text-sm font-medium text-gray-500 dark:text-gray-400 mb-1">{stat.title}</p>
                                    <p className="text-3xl font-bold text-gray-800 dark:text-gray-100 tracking-tight mb-2">{stat.value}</p>
                                    <div className="flex items-center gap-1.5">
                                        <span className={`text-xs font-bold px-2 py-0.5 rounded-full ${stat.trend.startsWith('+')
                                            ? 'bg-green-100 text-green-700'
                                            : stat.trend.startsWith('-')
                                                ? 'bg-red-100 text-red-700'
                                                : 'bg-gray-100 text-gray-600'
                                            }`}>
                                            {stat.trend}
                                        </span>
                                        <span className="text-xs text-gray-400 dark:text-gray-500">
                                            {stat.title === 'Ingresos Totales (Hoy)' ? 'vs ayer' : 'comparativo actual'}
                                        </span>
                                    </div>
                                </div>
                                <div className={`${stat.bgIcon} ${stat.colorText} w-12 h-12 rounded-xl flex items-center justify-center text-2xl group-hover:scale-110 transition-transform opacity-80`}>
                                    {stat.icon}
                                </div>
                            </div>
                        </div>
                    ))}
                </div>
            )}

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-8">
                {widgets.Ingresos && (
                    <div className="bg-white dark:bg-gray-800 rounded-2xl border border-gray-200 dark:border-gray-700 shadow-sm p-6 lg:col-span-2">
                        <div className="flex justify-between items-center mb-6">
                            <h3 className="text-lg font-bold text-gray-800 dark:text-gray-100 tracking-tight">Ingresos Financieros (Hoy)</h3>
                            <span className="flex h-3 w-3 relative">
                                <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-green-400 opacity-75"></span>
                                <span className="relative inline-flex rounded-full h-3 w-3 bg-green-500"></span>
                            </span>
                        </div>
                        <div className="h-72">
                            <ResponsiveContainer width="100%" height="100%">
                                <AreaChart data={revenueData} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
                                    <defs>
                                        <linearGradient id="colorRevenue" x1="0" y1="0" x2="0" y2="1">
                                            <stop offset="5%" stopColor="#00C2CB" stopOpacity={0.3} />
                                            <stop offset="95%" stopColor="#00C2CB" stopOpacity={0} />
                                        </linearGradient>
                                    </defs>
                                    <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#e5e7eb" />
                                    <XAxis dataKey="time" axisLine={false} tickLine={false} tick={{ fill: '#868e96', fontSize: 12 }} dy={10} />
                                    <YAxis axisLine={false} tickLine={false} tick={{ fill: '#868e96', fontSize: 12 }} />
                                    <Tooltip
                                        labelFormatter={(label) => `Hora: ${label}`}
                                        formatter={(value) => [formatCurrency(value), 'Ingresos']}
                                        contentStyle={{ borderRadius: '12px', border: '1px solid #e5e7eb', background: '#fff', color: '#333' }}
                                        itemStyle={{ color: '#00C2CB', fontWeight: 600 }}
                                    />
                                    <Area type="monotone" dataKey="revenue" name="Ingresos" stroke="#00C2CB" strokeWidth={3} fillOpacity={1} fill="url(#colorRevenue)" />
                                </AreaChart>
                            </ResponsiveContainer>
                        </div>
                    </div>
                )}

                {widgets.Ventas && (
                    <div className="bg-white dark:bg-gray-800 rounded-2xl border border-gray-200 dark:border-gray-700 shadow-sm p-6">
                        <h3 className="text-lg font-bold text-gray-800 dark:text-gray-100 tracking-tight mb-6">Pedidos Semanales</h3>
                        <div className="h-72">
                            <ResponsiveContainer width="100%" height="100%">
                                <BarChart data={salesData} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
                                    <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#e5e7eb" />
                                    <XAxis dataKey="name" axisLine={false} tickLine={false} tick={{ fill: '#868e96', fontSize: 12 }} dy={10} />
                                    <YAxis axisLine={false} tickLine={false} tick={{ fill: '#868e96', fontSize: 12 }} />
                                    <Tooltip
                                        formatter={(value) => [value, 'Pedidos']}
                                        cursor={{ fill: '#f3f4f6' }}
                                        contentStyle={{ borderRadius: '12px', border: '1px solid #e5e7eb', background: '#fff', color: '#333' }}
                                    />
                                    <Bar dataKey="ventas" fill="#9492ff" radius={[4, 4, 0, 0]} barSize={24} />
                                </BarChart>
                            </ResponsiveContainer>
                        </div>
                    </div>
                )}
            </div>

            {widgets.Actividad && (
                <div className="bg-white dark:bg-gray-800 rounded-2xl border border-gray-200 dark:border-gray-700 shadow-sm p-6 w-full lg:w-1/2">
                    <div className="flex justify-between items-center mb-6">
                        <h3 className="text-lg font-bold text-gray-800 dark:text-gray-100 tracking-tight">Actividad Reciente</h3>
                    </div>
                    <div className="space-y-4 max-h-[300px] overflow-y-auto pr-2">
                        {notifications.length === 0 && <p className="text-center text-gray-400 dark:text-gray-500 py-4">Sin actividad reciente</p>}
                        {notifications.map((act) => {
                            const NotifIcon = getNotifIcon(act.icono);
                            return (
                                <div key={act.id} className={`flex items-start gap-3 p-3 rounded-xl border animate-fade-in-up ${getNotifColor(act.color)}`}>
                                    <span className="mt-0.5 text-gray-600 dark:text-gray-300 shrink-0">
                                        <NotifIcon className="w-5 h-5" />
                                    </span>
                                    <div className="flex-1">
                                        <p className="font-semibold text-gray-800 dark:text-gray-100 text-sm">{act.titulo}</p>
                                        <p className="text-sm text-gray-600 dark:text-gray-300">{act.mensaje}</p>
                                        <p className="text-xs text-gray-400 dark:text-gray-500 mt-1">{act.fecha}</p>
                                    </div>
                                </div>
                            );
                        })}
                    </div>
                </div>
            )}do
        </div>
    );
}

export default Dashboard;
