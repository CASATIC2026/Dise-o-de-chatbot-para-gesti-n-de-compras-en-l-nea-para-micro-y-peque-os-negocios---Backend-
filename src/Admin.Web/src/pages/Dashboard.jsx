import { useState, useEffect } from 'react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import * as signalR from '@microsoft/signalr'; // Importamos SignalR
import api from '../api/client';

function Dashboard() {
    const [stats, setStats] = useState(null);
    const [loading, setLoading] = useState(true);
    
    // 1. Estado para las notificaciones reales
    const [actividades, setActividades] = useState([
        { id: 1, icono: '🚀', titulo: 'Sistema en línea', mensaje: 'Escuchando nuevas notificaciones...', color: 'bg-blue-50', fecha: 'Ahora' }
    ]);

    useEffect(() => {
        fetchStats();

        // 2. CONFIGURACIÓN DE SIGNALR
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("http://localhost:5001/notificationHub", {
                skipNegotiation: true,
                transport: signalR.HttpTransportType.WebSockets
            })
            .withAutomaticReconnect()
            .build();

        // Iniciar conexión
        connection.start()
            .then(() => console.log("Conectado al Hub de Notificaciones"))
            .catch(err => console.error("Error de conexión SignalR: ", err));

        // 3. ESCUCHAR NOTIFICACIONES DEL BACKEND
        connection.on("ReceiveNotification", (notificacion) => {
            // Mapeamos el color según el tipo que viene del backend
            const colorMap = {
                'success': 'bg-green-50',
                'warning': 'bg-yellow-50',
                'error': 'bg-red-50',
                'info': 'bg-blue-50'
            };

            const iconoMap = {
                'success': '✅',
                'warning': '⚠️',
                'error': '❌',
                'info': 'ℹ️'
            };

            const nuevaActividad = {
                id: Date.now(),
                titulo: notificacion.titulo,
                mensaje: notificacion.mensaje,
                icono: iconoMap[notificacion.tipo] || '🔔',
                color: colorMap[notificacion.tipo] || 'bg-gray-50',
                fecha: 'Recién'
            };

            // Agregamos al inicio de la lista
            setActividades(prev => [nuevaActividad, ...prev]);
        });

        return () => {
            connection.stop();
        };
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
        { title: 'Total Productos', value: stats?.totalProductos || 0, icon: '📦', color: 'bg-blue-500' },
        { title: 'Productos Activos', value: stats?.productosActivos || 0, icon: '✅', color: 'bg-green-500' },
        { title: 'Total Pedidos', value: stats?.totalPedidos || 0, icon: '🛒', color: 'bg-purple-500' },
        { title: 'Stock Bajo', value: stats?.stockBajo || 0, icon: '⚠️', color: 'bg-red-500' },
    ];

    const chartData = [
        { name: 'Lun', ventas: 12 }, { name: 'Mar', ventas: 19 }, { name: 'Mié', ventas: 15 },
        { name: 'Jue', ventas: 25 }, { name: 'Vie', ventas: 22 }, { name: 'Sáb', ventas: 30 }, { name: 'Dom', ventas: 18 },
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
                    <div key={index} className="bg-white rounded-xl shadow-md p-6 hover:shadow-lg transition-shadow">
                        <div className="flex items-center justify-between">
                            <div>
                                <p className="text-sm text-gray-600 mb-1">{stat.title}</p>
                                <p className="text-3xl font-bold text-gray-800">{stat.value}</p>
                            </div>
                            <div className={`${stat.color} w-14 h-14 rounded-full flex items-center justify-center text-3xl text-white`}>
                                {stat.icon}
                            </div>
                        </div>
                    </div>
                ))}
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                {/* Gráfica */}
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

                {/* --- ACTIVIDAD RECIENTE DINÁMICA --- */}
                <div className="bg-white rounded-xl shadow-md p-6">
                    <h3 className="text-lg font-semibold text-gray-800 mb-4">Actividad Reciente</h3>
                    <div className="space-y-4 max-h-[400px] overflow-y-auto pr-2">
                        {actividades.map((act) => (
                            <div key={act.id} className={`flex items-start space-x-3 p-3 ${act.color} rounded-lg border border-gray-100 transition-all animate-in fade-in slide-in-from-right-4`}>
                                <span className="text-2xl">{act.icono}</span>
                                <div>
                                    <p className="font-medium text-gray-800">{act.titulo}</p>
                                    <p className="text-sm text-gray-600">{act.mensaje}</p>
                                    <p className="text-xs text-gray-400 mt-1">{act.fecha}</p>
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
            </div>
        </div>
    );
}

export default Dashboard;