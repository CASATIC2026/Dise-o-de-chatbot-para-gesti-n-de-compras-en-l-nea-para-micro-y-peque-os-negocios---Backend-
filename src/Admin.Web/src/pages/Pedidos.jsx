import { useState, useEffect } from 'react';
import api from '../api/client';

function Pedidos() {
    const [pedidos, setPedidos] = useState([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [showModal, setShowModal] = useState(false);
    const [editingPedido, setEditingPedido] = useState(null);
    const [formData, setFormData] = useState({
        usuarioId: 0,
        clienteId: 0,
        estado: 0, // 0: Pendiente, 1: Confirmado, 2: Pagado, 3: Enviado, 4: Cancelado
        total: 0,
        direccionEntrega: '',
        detallesJson: '[]',
        referenciaWompi: ''
    });

    useEffect(() => {
        fetchPedidos();
    }, []);

    const fetchPedidos = async () => {
        try {
            const response = await api.get('/admin/inventario/pedidos'); // Endpoint asumido
            setPedidos(response.data);
        } catch (error) {
            console.error('Error fetching pedidos:', error);
        } finally {
            setLoading(false);
        }
    };

    const handleOpenModal = (pedido = null) => {
        if (pedido) {
            setEditingPedido(pedido);
            setFormData(pedido);
        } else {
            setEditingPedido(null);
            setFormData({
                usuarioId: 0,
                clienteId: 0,
                estado: 0,
                total: 0,
                direccionEntrega: '',
                detallesJson: '[]',
                referenciaWompi: ''
            });
        }
        setShowModal(true);
    };

    const handleCloseModal = () => {
        setShowModal(false);
        setEditingPedido(null);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        try {
            const dataToSave = {
                id: editingPedido ? Number(editingPedido.id) : 0,
                usuarioId: Number(formData.usuarioId),
                clienteId: Number(formData.clienteId),
                estado: Number(formData.estado),
                total: Number(formData.total),
                direccionEntrega: formData.direccionEntrega,
                detallesJson: formData.detallesJson,
                referenciaWompi: formData.referenciaWompi
            };

            if (editingPedido) {
                await api.put(`/admin/inventario/pedidos/${editingPedido.id}`, dataToSave);
                alert("¡Pedido actualizado!");
            } else {
                await api.post('/admin/inventario/pedidos', dataToSave);
                alert("¡Pedido agregado!");
            }

            fetchPedidos();
            handleCloseModal();

        } catch (error) {
            console.error('Error:', error);
            alert('Error al guardar/modificar el pedido');
        }
    };

    const handleDeletePermanently = async (id) => {
        if (!confirm('¿Estás seguro de eliminar este pedido?')) return;

        try {
            await api.delete(`/admin/inventario/pedidos/${id}`);
            fetchPedidos();
        } catch (error) {
            console.error('Error deleting pedido:', error);
        }
    }

    const getEstadoColor = (estadoEnum) => {
        switch (estadoEnum) {
            case 0: return 'bg-yellow-100 text-yellow-800'; // Pendiente
            case 1: return 'bg-blue-100 text-blue-800'; // Confirmado
            case 2: return 'bg-green-100 text-green-800'; // Pagado
            case 3: return 'bg-purple-100 text-purple-800'; // Enviado
            case 4: return 'bg-red-100 text-red-800'; // Cancelado
            default: return 'bg-gray-100 text-gray-800';
        }
    };

    const getEstadoText = (estadoEnum) => {
        switch (estadoEnum) {
            case 0: return 'Pendiente';
            case 1: return 'Confirmado';
            case 2: return 'Pagado';
            case 3: return 'Enviado';
            case 4: return 'Cancelado';
            default: return 'Desconocido';
        }
    };

    const filteredPedidos = pedidos.filter(pedido =>
        pedido.clienteId.toString().includes(searchTerm) ||
        pedido.estado.toLowerCase().includes(searchTerm.toLowerCase()) ||
        pedido.total.toString().includes(searchTerm)
    );

    if (loading) {
        return <div className="text-center py-12">Cargando pedidos...</div>;
    }

    return (
        <div>
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-8">
                <div>
                    <h1 className="text-3xl font-bold text-gray-800">Pedidos</h1>
                    <p className="text-gray-600 mt-2">Gestiona las órdenes de compra de tus clientes</p>
                </div>

                <div className="flex flex-col sm:flex-row w-full md:w-auto gap-4">
                    <div className="relative flex-1 sm:w-64">
                        <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400">🔍</span>
                        <input
                            type="text"
                            placeholder="Buscar pedidos..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pl-10 pr-4 py-2 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 transition-all"
                        />
                    </div>
                    <button
                        onClick={() => handleOpenModal()}
                        className="bg-primary-600 text-white p-3 md:px-6 md:py-3 rounded-lg font-medium hover:bg-primary-700 transition-colors flex items-center justify-center whitespace-nowrap"
                        title="Nuevo Pedido"
                    >
                        <span className="text-xl md:mr-2">➕</span>
                        <span className="hidden md:inline">Nuevo Pedido</span>
                    </button>
                </div>
            </div>

            <div className="bg-white rounded-xl shadow-md overflow-x-auto">
                <table className="w-full">
                    <thead className="bg-gray-50 border-b">
                        <tr>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">ID</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Usuario ID</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Cliente ID</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Dirección</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Total</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Estado</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Acciones</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-200">
                        {filteredPedidos.map((pedido) => (
                            <tr key={pedido.id} className="hover:bg-gray-50">
                                <td className="px-6 py-4 font-medium text-gray-900">#{pedido.id}</td>
                                <td className="px-6 py-4 text-gray-900">{pedido.usuarioId}</td>
                                <td className="px-6 py-4 text-gray-900">{pedido.clienteId}</td>
                                <td className="px-6 py-4 text-gray-600 text-sm truncate max-w-xs">{pedido.direccionEntrega}</td>
                                <td className="px-6 py-4 text-gray-900 font-medium">
                                    ${Number(pedido.total).toLocaleString('es-CO')}
                                </td>
                                <td className="px-6 py-4">
                                    <span className={`px-3 py-1 rounded-full text-sm font-medium ${getEstadoColor(pedido.estado)}`}>
                                        {getEstadoText(pedido.estado)}
                                    </span>
                                </td>
                                <td className="px-6 py-4">
                                    <div className="flex space-x-2">
                                        <button
                                            onClick={() => handleOpenModal(pedido)}
                                            className="p-2 text-primary-600 hover:bg-primary-50 rounded-lg transition-colors"
                                            title="Editar"
                                        >
                                            ✏️
                                        </button>
                                        <button
                                            onClick={() => handleDeletePermanently(pedido.id)}
                                            className="p-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                                            title="Eliminar"
                                        >
                                            🗑️
                                        </button>
                                    </div>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>

                {pedidos.length === 0 && (
                    <div className="text-center py-12 text-gray-500">
                        No hay pedidos registrados
                    </div>
                )}
            </div>

            {/* Modal */}
            {showModal && (
                <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
                    <div className="bg-white rounded-xl p-8 max-w-md w-full max-h-[90vh] overflow-y-auto">
                        <h2 className="text-2xl text-gray-700 font-bold mb-6">
                            {editingPedido ? 'Editar Pedido' : 'Nuevo Pedido'}
                        </h2>

                        <form onSubmit={handleSubmit} className="space-y-4">
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">Usuario ID</label>
                                    <input type="number" value={formData.usuarioId} onChange={(e) => setFormData({ ...formData, usuarioId: e.target.value })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500" required />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">Cliente ID</label>
                                    <input type="number" value={formData.clienteId} onChange={(e) => setFormData({ ...formData, clienteId: e.target.value })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500" required />
                                </div>
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Total</label>
                                <input type="number" step="0.01" value={formData.total} onChange={(e) => setFormData({ ...formData, total: e.target.value })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500" required />
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Estado</label>
                                <select value={formData.estado} onChange={(e) => setFormData({ ...formData, estado: Number(e.target.value) })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500" required>
                                    <option value={0}>Pendiente</option>
                                    <option value={1}>Confirmado</option>
                                    <option value={2}>Pagado</option>
                                    <option value={3}>Enviado</option>
                                    <option value={4}>Cancelado</option>
                                </select>
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Dirección Entrega</label>
                                <textarea value={formData.direccionEntrega || ''} onChange={(e) => setFormData({ ...formData, direccionEntrega: e.target.value })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500" rows="2" required />
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Referencia Wompi</label>
                                <input type="text" value={formData.referenciaWompi || ''} onChange={(e) => setFormData({ ...formData, referenciaWompi: e.target.value })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500" />
                            </div>

                            <div className="flex space-x-3 pt-4">
                                <button type="submit" className="flex-1 bg-primary-600 text-white py-2 rounded-lg font-medium hover:bg-primary-700">Guardar</button>
                                <button type="button" onClick={handleCloseModal} className="flex-1 bg-gray-200 text-gray-800 py-2 rounded-lg font-medium hover:bg-gray-300">Cancelar</button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
}

export default Pedidos;
