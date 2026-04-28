import { useState, useEffect } from 'react';
import { AreaChart, Area, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import api from '../api/client';
import { MoneyIcon, OrdersIcon, InventoryIcon, AlertIcon, CheckCircleIcon, ClientsIcon } from '../components/Icons';

function Dashboard({ notifications = [] }) {
    const [stats, setStats] = useState(null);
    const [loading, setLoading] = useState(true);

    // Customizable dashboard state
    const [widgets, setWidgets] = useState({
        stats: true,
        revenue: true,
        sales: true,
        activity: true
    });

    useEffect(() => {
        fetchStats();
    }, []);

    const fetchStats = async () => {
        try {
            const response = await api.get('/admin/dashboard/stats');
            setStats(response.data);
        } catch (error) {
            console.error('❌ [Dashboard] Error en stats:', error);
        } finally {
            setLoading(false);
        }
    };

    const toggleWidget = (widget) => {
        setWidgets(prev => ({ ...prev, [widget]: !prev[widget] }));
    };

    if (loading) {
        return (
            <div className="flex items-center justify-center h-64">
                <div className="flex flex-col items-center">
                    <div className="w-10 h-10 border-4 border-primary-200 border-t-primary-600 rounded-full animate-spin mb-4"></div>
                    <div className="text-neutral-500 font-medium tracking-wide">Cargando métricas...</div>
                </div>
    useEffect(() => {
        fetchStats();
    }, []);

    if (loading) {
        return (
            <div className="flex flex-col items-center justify-center h-64">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500 mb-4"></div>
                <div className="text-xl text-gray-600">Cargando estadísticas...</div>
            </div>
        );
    }

    const statCards = [
        {
            title: 'Ingresos Totales (Hoy)',
            value: '$' + ((stats?.totalVentas || 12450) / 100).toFixed(2),
            icon: <MoneyIcon />,
            colorText: 'text-primary-600',
            bgIcon: 'bg-primary-50',
            trend: '+12.5%'
        },
        {
            title: 'Pedidos',
            value: stats?.totalPedidos || 0,
            icon: <OrdersIcon />,
            colorText: 'text-secondary-600',
            bgIcon: 'bg-secondary-50',
            trend: '+5.2%'
        },
        {
            title: 'Productos Activos',
            value: stats?.productosActivos || 0,
            icon: <InventoryIcon />,
            colorText: 'text-tertiary-600',
            bgIcon: 'bg-tertiary-50',
            trend: 'Estable'
        },
        {
            title: 'Stock Bajo',
            value: stats?.stockBajo || 0,
            icon: <AlertIcon />,
            colorText: 'text-red-500',
            bgIcon: 'bg-red-50',
            trend: '-2.1%'
        },
    ];

    const salesData = [
        { name: 'Lun', ventas: 12 }, { name: 'Mar', ventas: 19 },
        { name: 'Mié', ventas: 15 }, { name: 'Jue', ventas: 25 },
        { name: 'Vie', ventas: 22 }, { name: 'Sáb', ventas: 30 },
        { name: 'Dom', ventas: 18 },
    ];

    const revenueData = [
        { time: '08:00', revenue: 400 }, { time: '10:00', revenue: 900 },
        { time: '12:00', revenue: 1500 }, { time: '14:00', revenue: 1100 },
        { time: '16:00', revenue: 2300 }, { time: '18:00', revenue: 3200 },
    ];

    return (
        <div className="animate-fade-in">
            <div className="flex flex-col md:flex-row md:items-end justify-between mb-8 pb-4 border-b border-neutral-200 dark:border-dark-border gap-4">
                <div>
                    <h1 className="text-3xl font-bold text-neutral-900 dark:text-neutral-100 tracking-tight">Dashboard Financiero</h1>
                    <p className="text-neutral-500 dark:text-neutral-400 mt-1">Monitorea los KPIs de tu E-commerce en tiempo real</p>
                </div>

                {/* Customization Menu */}
                <div className="flex items-center gap-2 bg-white dark:bg-dark-surface p-2 rounded-xl border border-neutral-200 dark:border-dark-border shadow-sm dark:shadow-none">
                    <span className="text-sm font-medium text-neutral-500 dark:text-neutral-400 px-2 border-r border-neutral-200 dark:border-dark-border">Personalizar</span>
                    {Object.keys(widgets).map(key => (
                        <button
                            key={key}
                            onClick={() => toggleWidget(key)}
                            className={`px-3 py-1.5 text-xs font-semibold rounded-lg transition-all ${widgets[key]
                                    ? 'bg-primary-50 dark:bg-cyan-900/20 text-primary-600 dark:text-cyan-400'
                                    : 'bg-neutral-100 dark:bg-dark-input text-neutral-400 dark:text-neutral-500 hover:bg-neutral-200 dark:hover:bg-dark-border'
                                }`}
                        >
                            {key.charAt(0).toUpperCase() + key.slice(1)}
                        </button>
                    ))}
                </div>
            </div>

            {/* Stats Cards */}
            {widgets.stats && (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
                    {statCards.map((stat, index) => (
                        <div
                            key={index}
                            className="bg-white dark:bg-dark-surface rounded-2xl border border-neutral-200 dark:border-dark-border p-6 shadow-sm dark:shadow-none hover:shadow-md dark:hover:shadow-black/20 transition-all duration-300 group"
                        >
                            <div className="flex items-start justify-between">
                                <div>
                                    <p className="text-sm font-medium text-neutral-500 dark:text-neutral-400 mb-1">{stat.title}</p>
                                    <p className="text-3xl font-bold text-neutral-900 dark:text-neutral-100 tracking-tight mb-2">{stat.value}</p>
                                    <div className="flex items-center gap-1.5">
                                        <span className={`text-xs font-bold px-2 py-0.5 rounded-full ${stat.trend.startsWith('+') ? 'bg-green-100 dark:bg-green-900/20 text-green-700 dark:text-green-400' :
                                                stat.trend.startsWith('-') ? 'bg-red-100 dark:bg-red-900/20 text-red-700 dark:text-red-400' : 'bg-neutral-100 dark:bg-dark-input text-neutral-600 dark:text-neutral-400'
                                            }`}>
                                            {stat.trend}
                                        </span>
                                        <span className="text-xs text-neutral-400 dark:text-neutral-500">vs semana pasada</span>
                                    </div>
                                </div>
                                <div className={`${stat.bgIcon} ${stat.colorText} w-12 h-12 rounded-xl flex items-center justify-center text-2xl group-hover:scale-110 transition-transform opacity-80 dark:opacity-60`}>
                                    {stat.icon}
                                </div>
        <div className="p-6">
            <div className="mb-8">
                <h1 className="text-3xl font-bold text-gray-800">Dashboard</h1>
                <p className="text-gray-600 mt-2">Resumen general en tiempo real</p>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
                {statCards.map((stat, index) => (
                    <div key={index} className="bg-white rounded-xl shadow-md p-6 border border-gray-100">
                        <div className="flex items-center justify-between">
                            <div>
                                <p className="text-sm text-gray-600 mb-1">{stat.title}</p>
                                <p className="text-3xl font-bold text-gray-800">{stat.value}</p>
                            </div>
                            <div className={`${stat.color} w-14 h-14 rounded-full flex items-center justify-center text-3xl text-white`}>
                                {stat.icon}
                            </div>
                        </div>
                    ))}
                </div>
            )}

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-8">
                {/* Revenue Premium Chart */}
                {widgets.revenue && (
                    <div className="bg-white dark:bg-dark-surface rounded-2xl border border-neutral-200 dark:border-dark-border shadow-sm dark:shadow-none p-6 lg:col-span-2">
                        <div className="flex justify-between items-center mb-6">
                            <h3 className="text-lg font-bold text-neutral-900 dark:text-neutral-100 tracking-tight">Ingresos Financieros (Hoy)</h3>
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
                                    <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#2a2a2a" />
                                    <XAxis dataKey="time" axisLine={false} tickLine={false} tick={{ fill: '#868e96', fontSize: 12 }} dy={10} />
                                    <YAxis axisLine={false} tickLine={false} tick={{ fill: '#868e96', fontSize: 12 }} />
                                    <Tooltip
                                        contentStyle={{ borderRadius: '12px', border: '1px solid #2a2a2a', background: '#1a1a1a', color: '#f0f0f0' }}
                                        itemStyle={{ color: '#00C2CB', fontWeight: 600 }}
                                    />
                                    <Area type="monotone" dataKey="revenue" stroke="#00C2CB" strokeWidth={3} fillOpacity={1} fill="url(#colorRevenue)" />
                                </AreaChart>
                            </ResponsiveContainer>
                        </div>
                    </div>
                )}

                {/* Sales Chart */}
                {widgets.sales && (
                    <div className="bg-white dark:bg-dark-surface rounded-2xl border border-neutral-200 dark:border-dark-border shadow-sm dark:shadow-none p-6">
                        <h3 className="text-lg font-bold text-neutral-900 dark:text-neutral-100 tracking-tight mb-6">Pedidos Semanales</h3>
                        <div className="h-72">
                            <ResponsiveContainer width="100%" height="100%">
                                <BarChart data={salesData} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
                                    <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#2a2a2a" />
                                    <XAxis dataKey="name" axisLine={false} tickLine={false} tick={{ fill: '#868e96', fontSize: 12 }} dy={10} />
                                    <YAxis axisLine={false} tickLine={false} tick={{ fill: '#868e96', fontSize: 12 }} />
                                    <Tooltip cursor={{ fill: '#242424' }} contentStyle={{ borderRadius: '12px', border: '1px solid #2a2a2a', background: '#1a1a1a', color: '#f0f0f0' }} />
                                    <Bar dataKey="ventas" fill="#9492ff" radius={[4, 4, 0, 0]} barSize={24} />
                                </BarChart>
                            </ResponsiveContainer>
                        </div>
                    </div>
                )}
            </div>

            {/* Recent Activity */}
            {widgets.activity && (
                <div className="bg-white dark:bg-dark-surface rounded-2xl border border-neutral-200 dark:border-dark-border shadow-sm dark:shadow-none p-6 w-full lg:w-1/2">
                    <div className="flex justify-between items-center mb-6">
                        <h3 className="text-lg font-bold text-neutral-900 dark:text-neutral-100 tracking-tight">Actividad Reciente</h3>
                        <button className="text-sm font-semibold text-primary-600 dark:text-cyan-400 hover:text-primary-800 dark:hover:text-cyan-300 transition-colors">Ver Todo</button>
                    </div>
                    <div className="space-y-4">
                        <div className="flex items-start gap-4 p-4 hover:bg-neutral-50 dark:hover:bg-dark-input rounded-xl transition-colors border border-transparent hover:border-neutral-100 dark:hover:border-dark-border group">
                            <div className="w-10 h-10 rounded-full bg-green-50 dark:bg-green-900/20 flex items-center justify-center text-green-600 text-lg group-hover:scale-110 transition-transform"><CheckCircleIcon /></div>
                            <div className="flex-1">
                                <p className="font-semibold text-neutral-900 dark:text-neutral-100">Nuevo pedido recibido</p>
                                <p className="text-sm text-neutral-500 dark:text-neutral-400">Orden #2948 por $120.50</p>
                            </div>
                            <span className="text-xs font-medium text-neutral-400 dark:text-neutral-500">Hace 5m</span>
                        </div>
                        <div className="flex items-start gap-4 p-4 hover:bg-neutral-50 dark:hover:bg-dark-input rounded-xl transition-colors border border-transparent hover:border-neutral-100 dark:hover:border-dark-border group">
                            <div className="w-10 h-10 rounded-full bg-secondary-50 dark:bg-indigo-900/20 flex items-center justify-center text-secondary-600 dark:text-indigo-300 text-lg group-hover:scale-110 transition-transform"><InventoryIcon /></div>
                            <div className="flex-1">
                                <p className="font-semibold text-neutral-900 dark:text-neutral-100">Producto actualizado</p>
                                <p className="text-sm text-neutral-500 dark:text-neutral-400">Stock ajustado para "Teclado Mecánico"</p>
                            </div>
                            <span className="text-xs font-medium text-neutral-400 dark:text-neutral-500">Hace 1h</span>
                        </div>
                        <div className="flex items-start gap-4 p-4 hover:bg-neutral-50 dark:hover:bg-dark-input rounded-xl transition-colors border border-transparent hover:border-neutral-100 dark:hover:border-dark-border group">
                            <div className="w-10 h-10 rounded-full bg-tertiary-50 dark:bg-purple-900/20 flex items-center justify-center text-tertiary-600 dark:text-purple-300 text-lg group-hover:scale-110 transition-transform"><ClientsIcon /></div>
                            <div className="flex-1">
                                <p className="font-semibold text-neutral-900 dark:text-neutral-100">Nuevo cliente registrado</p>
                                <p className="text-sm text-neutral-500 dark:text-neutral-400">Carlos Mendoza ha creado una cuenta.</p>
                            </div>
                            <span className="text-xs font-medium text-neutral-400 dark:text-neutral-500">Hace 3h</span>
                        </div>
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                <div className="bg-white rounded-xl shadow-md p-6 border border-gray-100">
                    <h3 className="text-lg font-semibold text-gray-800 mb-4">Ventas de la Semana</h3>
                    <div className="h-[300px] w-full">
                        <ResponsiveContainer width="100%" height="100%">
                            <BarChart data={chartData}>
                                <CartesianGrid strokeDasharray="3 3" vertical={false} />
                                <XAxis dataKey="name" />
                                <YAxis />
                                <Tooltip />
                                <Legend />
                                <Bar dataKey="ventas" fill="#0ea5e9" radius={[4, 4, 0, 0]} />
                            </BarChart>
                        </ResponsiveContainer>
                    </div>
                </div>

                <div className="bg-white rounded-xl shadow-md p-6 border border-gray-100">
                    <h3 className="text-lg font-semibold text-gray-800 mb-4">Actividad Reciente</h3>
                    <div className="space-y-4 max-h-[300px] overflow-y-auto pr-2">
                        {notifications.length === 0 && <p className="text-center text-gray-400 py-4">Sin actividad reciente</p>}
                        {notifications.map((act) => (
                            <div key={act.id} className={`flex items-start space-x-3 p-3 ${act.color} rounded-lg border border-gray-100 animate-fade-in-down`}>
                                <span className="text-2xl">{act.icono}</span>
                                <div className="flex-1">
                                    <p className="font-medium text-gray-800 text-sm">{act.titulo}</p>
                                    <p className="text-xs text-gray-600">{act.mensaje}</p>
                                    <p className="text-[10px] text-gray-400 mt-1">{act.fecha}</p>
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
            )}
        </div>
    );
}

export default Dashboard;
