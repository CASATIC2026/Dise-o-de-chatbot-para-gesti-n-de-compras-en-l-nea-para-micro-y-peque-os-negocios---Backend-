import { useState, useEffect } from 'react';
import { AreaChart, Area, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
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
                    <div className="text-gray-500 font-medium tracking-wide">Cargando métricas...</div>
                </div>
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
            <div className="flex flex-col md:flex-row md:items-end justify-between mb-8 pb-4 border-b border-gray-200 dark:border-gray-700 gap-4">
                <div>
                    <h1 className="text-3xl font-bold text-gray-800 dark:text-gray-100 tracking-tight">Dashboard Financiero</h1>
                    <p className="text-gray-500 dark:text-gray-400 mt-1">Monitorea los KPIs de tu E-commerce en tiempo real</p>
                </div>

                {/* Customization Menu */}
                <div className="flex items-center gap-2 bg-white dark:bg-gray-800 p-2 rounded-xl border border-gray-200 dark:border-gray-700 shadow-sm">
                    <span className="text-sm font-medium text-gray-500 dark:text-gray-300 px-2 border-r border-gray-200 dark:border-gray-600">Personalizar</span>
                    {Object.keys(widgets).map(key => (
                        <button
                            key={key}
                            onClick={() => toggleWidget(key)}
                            className={`px-3 py-1.5 text-xs font-semibold rounded-lg transition-all ${widgets[key]
                                    ? 'bg-primary-50 text-primary-600'
                                    : 'bg-gray-100 dark:bg-gray-700 text-gray-400 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-600'
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
                            className="bg-white dark:bg-gray-800 rounded-2xl border border-gray-200 dark:border-gray-700 p-6 shadow-sm hover:shadow-md transition-all duration-300 group"
                        >
                            <div className="flex items-start justify-between">
                                <div>
                                    <p className="text-sm font-medium text-gray-500 dark:text-gray-400 mb-1">{stat.title}</p>
                                    <p className="text-3xl font-bold text-gray-800 dark:text-gray-100 tracking-tight mb-2">{stat.value}</p>
                                    <div className="flex items-center gap-1.5">
                                        <span className={`text-xs font-bold px-2 py-0.5 rounded-full ${stat.trend.startsWith('+') ? 'bg-green-100 text-green-700' :
                                                stat.trend.startsWith('-') ? 'bg-red-100 text-red-700' : 'bg-gray-100 text-gray-600'
                                            }`}>
                                            {stat.trend}
                                        </span>
                                        <span className="text-xs text-gray-400 dark:text-gray-500">vs semana pasada</span>
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
                {/* Revenue Premium Chart */}
                {widgets.revenue && (
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
                                        contentStyle={{ borderRadius: '12px', border: '1px solid #e5e7eb', background: '#fff', color: '#333' }}
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
                    <div className="bg-white dark:bg-gray-800 rounded-2xl border border-gray-200 dark:border-gray-700 shadow-sm p-6">
                        <h3 className="text-lg font-bold text-gray-800 dark:text-gray-100 tracking-tight mb-6">Pedidos Semanales</h3>
                        <div className="h-72">
                            <ResponsiveContainer width="100%" height="100%">
                                <BarChart data={salesData} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
                                    <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#e5e7eb" />
                                    <XAxis dataKey="name" axisLine={false} tickLine={false} tick={{ fill: '#868e96', fontSize: 12 }} dy={10} />
                                    <YAxis axisLine={false} tickLine={false} tick={{ fill: '#868e96', fontSize: 12 }} />
                                    <Tooltip cursor={{ fill: '#f3f4f6' }} contentStyle={{ borderRadius: '12px', border: '1px solid #e5e7eb', background: '#fff', color: '#333' }} />
                                    <Bar dataKey="ventas" fill="#9492ff" radius={[4, 4, 0, 0]} barSize={24} />
                                </BarChart>
                            </ResponsiveContainer>
                        </div>
                    </div>
                )}
            </div>

            {/* Recent Activity — fed by real-time notifications */}
            {widgets.activity && (
                <div className="bg-white dark:bg-gray-800 rounded-2xl border border-gray-200 dark:border-gray-700 shadow-sm p-6 w-full lg:w-1/2">
                    <div className="flex justify-between items-center mb-6">
                        <h3 className="text-lg font-bold text-gray-800 dark:text-gray-100 tracking-tight">Actividad Reciente</h3>
                    </div>
                    <div className="space-y-4 max-h-[300px] overflow-y-auto pr-2">
                        {notifications.length === 0 && <p className="text-center text-gray-400 dark:text-gray-500 py-4">Sin actividad reciente</p>}
                        {notifications.map((act) => (
                            <div key={act.id} className={`flex items-start gap-3 p-3 ${act.color} rounded-xl border border-gray-100 animate-fade-in-up`}>
                                <span className="text-2xl">{act.icono}</span>
                                <div className="flex-1">
                                    <p className="font-semibold text-gray-800 dark:text-gray-100 text-sm">{act.titulo}</p>
                                    <p className="text-sm text-gray-600 dark:text-gray-300">{act.mensaje}</p>
                                    <p className="text-xs text-gray-400 dark:text-gray-500 mt-1">{act.fecha}</p>
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
