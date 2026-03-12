import { useState, useEffect } from 'react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import api from '../api/client';

function Dashboard() {
    const [stats, setStats] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        fetchStats();
    }, []);

    const fetchStats = async () => {
        try {
            const response = await api.get('/admin/dashboard/stats');
            setStats(response.data);
        } catch (error) {
            console.error('Error fetching stats:', error);
        } finally {
            setLoading(false);
        }
    };

    if (loading) {
        return (
            <div className="flex items-center justify-center h-64">
                <div className="text-xl text-gray-600">Cargando estadísticas...</div>
            </div>
        );
    }

    const statCards = [
        {
            title: 'Total Productos',
            value: stats?.totalProductos || 0,
            icon: '📦',
            color: 'bg-blue-500',
        },
        {
            title: 'Productos Activos',
            value: stats?.productosActivos || 0,
            icon: '✅',
            color: 'bg-green-500',
        },
        {
            title: 'Total Pedidos',
            value: stats?.totalPedidos || 0,
            icon: '🛒',
            color: 'bg-purple-500',
        },
        {
            title: 'Stock Bajo',
            value: stats?.stockBajo || 0,
            icon: '⚠️',
            color: 'bg-red-500',
        },
    ];

    const chartData = [
        { name: 'Lun', ventas: 12 },
        { name: 'Mar', ventas: 19 },
        { name: 'Mié', ventas: 15 },
        { name: 'Jue', ventas: 25 },
        { name: 'Vie', ventas: 22 },
        { name: 'Sáb', ventas: 30 },
        { name: 'Dom', ventas: 18 },
    ];

    return (
        <div>
            <div className="mb-8">
                <h1 className="text-3xl font-bold text-gray-800">Dashboard</h1>
                <p className="text-gray-600 mt-2">Resumen general de tu negocio</p>
            </div>

            {/* Stats Cards */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
                {statCards.map((stat, index) => (
                    <div
                        key={index}
                        className="bg-white rounded-xl shadow-md p-6 hover:shadow-lg transition-shadow"
                    >
                        <div className="flex items-center justify-between">
                            <div>
                                <p className="text-sm text-gray-600 mb-1">{stat.title}</p>
                                <p className="text-3xl font-bold text-gray-800">{stat.value}</p>
                            </div>
                            <div className={`${stat.color} w-14 h-14 rounded-full flex items-center justify-center text-3xl`}>
                                {stat.icon}
                            </div>
                        </div>
                    </div>
                ))}
            </div>

            {/* Charts */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                <div className="bg-white rounded-xl shadow-md p-6">
                    <h3 className="text-lg font-semibold text-gray-800 mb-4">Ventas de la Semana</h3>
                    <ResponsiveContainer width="100%" height={300}>
                        <BarChart data={chartData}>
                            <CartesianGrid strokeDasharray="3 3" />
                            <XAxis dataKey="name" />
                            <YAxis />
                            <Tooltip />
                            <Legend />
                            <Bar dataKey="ventas" fill="#0ea5e9" />
                        </BarChart>
                    </ResponsiveContainer>
                </div>

                <div className="bg-white rounded-xl shadow-md p-6">
                    <h3 className="text-lg font-semibold text-gray-800 mb-4">Actividad Reciente</h3>
                    <div className="space-y-4">
                        <div className="flex items-start space-x-3 p-3 bg-green-50 rounded-lg">
                            <span className="text-2xl">✅</span>
                            <div>
                                <p className="font-medium text-gray-800">Nuevo pedido recibido</p>
                                <p className="text-sm text-gray-600">Hace 5 minutos</p>
                            </div>
                        </div>
                        <div className="flex items-start space-x-3 p-3 bg-blue-50 rounded-lg">
                            <span className="text-2xl">📦</span>
                            <div>
                                <p className="font-medium text-gray-800">Producto agregado al catálogo</p>
                                <p className="text-sm text-gray-600">Hace 1 hora</p>
                            </div>
                        </div>
                        <div className="flex items-start space-x-3 p-3 bg-yellow-50 rounded-lg">
                            <span className="text-2xl">⚠️</span>
                            <div>
                                <p className="font-medium text-gray-800">Stock bajo en 2 productos</p>
                                <p className="text-sm text-gray-600">Hace 3 horas</p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default Dashboard;
