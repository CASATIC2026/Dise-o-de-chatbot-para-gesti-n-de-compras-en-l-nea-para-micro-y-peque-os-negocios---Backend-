function Pedidos() {
    const pedidosMock = [
        {
            id: 1,
            usuario: 'Juan Pérez',
            total: 45000,
            estado: 'Pagado',
            direccion: 'Calle 123 #45-67',
            fecha: '2024-02-13 10:30',
        },
        {
            id: 2,
            usuario: 'María García',
            total: 120000,
            estado: 'Pendiente',
            direccion: 'Carrera 5 #12-34',
            fecha: '2024-02-13 11:15',
        },
    ];

    const getEstadoColor = (estado) => {
        const colors = {
            Pendiente: 'bg-yellow-100 text-yellow-800',
            Confirmado: 'bg-blue-100 text-blue-800',
            Pagado: 'bg-green-100 text-green-800',
            Enviado: 'bg-purple-100 text-purple-800',
            Cancelado: 'bg-red-100 text-red-800',
        };
        return colors[estado] || 'bg-gray-100 text-gray-800';
    };

    return (
        <div>
            <div className="mb-8">
                <h1 className="text-3xl font-bold text-gray-800">Pedidos</h1>
                <p className="text-gray-600 mt-2">Gestiona los pedidos de tus clientes</p>
            </div>

            <div className="bg-white rounded-xl shadow-md overflow-hidden">
                <table className="w-full">
                    <thead className="bg-gray-50 border-b">
                        <tr>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">ID</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Cliente</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Dirección</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Total</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Estado</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Fecha</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-200">
                        {pedidosMock.map((pedido) => (
                            <tr key={pedido.id} className="hover:bg-gray-50">
                                <td className="px-6 py-4 font-medium text-gray-900">#{pedido.id}</td>
                                <td className="px-6 py-4 text-gray-900">{pedido.usuario}</td>
                                <td className="px-6 py-4 text-gray-600 text-sm">{pedido.direccion}</td>
                                <td className="px-6 py-4 text-gray-900 font-medium">
                                    ${pedido.total.toLocaleString('es-CO')}
                                </td>
                                <td className="px-6 py-4">
                                    <span className={`px-3 py-1 rounded-full text-sm font-medium ${getEstadoColor(pedido.estado)}`}>
                                        {pedido.estado}
                                    </span>
                                </td>
                                <td className="px-6 py-4 text-gray-600 text-sm">{pedido.fecha}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>

                {pedidosMock.length === 0 && (
                    <div className="text-center py-12 text-gray-500">
                        No hay pedidos registrados
                    </div>
                )}
            </div>
        </div>
    );
}

export default Pedidos;
