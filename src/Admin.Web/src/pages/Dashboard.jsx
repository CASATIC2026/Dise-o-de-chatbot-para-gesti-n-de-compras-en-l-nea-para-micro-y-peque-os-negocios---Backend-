import { useState, useEffect } from 'react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import * as signalR from '@microsoft/signalr';
import api from '../api/client';

function Dashboard() {
    const [stats, setStats] = useState(null);
    const [loading, setLoading] = useState(true);
    const [actividades, setActividades] = useState([
        { id: 1, icono: '🚀', titulo: 'Sistema en línea', mensaje: 'Escuchando nuevas notificaciones...', color: 'bg-blue-50', fecha: 'Ahora' }
    ]);

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

    useEffect(() => {
        let isMounted = true;
        fetchStats();

        // 1. Crear la conexión una sola vez
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("https://xpf10vmg-5001.use2.devtunnels.ms/notificationHub", {
                headers: { "X-Tunnel-Skip-AntiPhishing-Page": "true" } // <--- ESTO ES CLAVE
            })
            .withAutomaticReconnect()
            .build();

        // 2. Definir la función de inicio
        const startConnection = async () => {
            if (connection.state === signalR.HubConnectionState.Disconnected) {
                try {
                    await connection.start();
                    console.log("🚀 SignalR Conectado con éxito");
                } catch (err) {
                    console.error("❌ Error al conectar SignalR:", err);
                    setTimeout(startConnection, 5000);
                }
            }
        };

        // 3. Configurar el receptor de eventos
        connection.on("ReceiveNotification", (notificacion) => {
            console.log("🔔 DATOS RECIBIDOS DEL HUB:", notificacion);

            if (!isMounted) return;

            const colorMap = { 'success': 'bg-green-50', 'warning': 'bg-yellow-50', 'error': 'bg-red-50', 'info': 'bg-blue-50' };
            const iconoMap = { 'success': '✅', 'warning': '⚠️', 'error': '❌', 'info': 'ℹ️' };

            // IMPORTANTE: .NET envía las propiedades en camelCase (primera letra minúscula)
            const nuevaActividad = {
                id: Date.now(),
                titulo: notificacion.titulo || "Notificación",
                mensaje: notificacion.mensaje || "Sin mensaje",
                icono: iconoMap[notificacion.tipo] || '🔔',
                color: colorMap[notificacion.tipo] || 'bg-gray-50',
                fecha: 'Recién'
            };

            setActividades(prev => [nuevaActividad, ...prev]);
        });

        startConnection();

        // 4. Limpieza al desmontar el componente
        return () => {
            isMounted = false;
            if (connection) {
                connection.stop();
                console.log("📡 SignalR Desconectado");
            }
        };
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
                    </div>
                ))}
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
                        {actividades.length === 0 && <p className="text-center text-gray-400 py-4">Sin actividad reciente</p>}
                        {actividades.map((act) => (
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
            </div>
        </div>
    );
}

export default Dashboard;