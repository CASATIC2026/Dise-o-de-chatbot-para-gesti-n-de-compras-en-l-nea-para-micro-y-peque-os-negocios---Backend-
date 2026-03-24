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
        estado: 0,
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
            const response = await api.get('/admin/inventario/pedidos');
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

    const getEstadoStyles = (estadoEnum) => {
        switch (estadoEnum) {
            case 0: return 'bg-yellow-50 text-yellow-700 border-yellow-200'; // Pendiente
            case 1: return 'bg-blue-50 text-blue-700 border-blue-200'; // Confirmado
            case 2: return 'bg-green-50 text-green-700 border-green-200'; // Pagado
            case 3: return 'bg-purple-50 text-purple-700 border-purple-200'; // Enviado
            case 4: return 'bg-red-50 text-red-700 border-red-200'; // Cancelado
            default: return 'bg-neutral-50 text-neutral-700 border-neutral-200';
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
        getEstadoText(pedido.estado).toLowerCase().includes(searchTerm.toLowerCase()) ||
        pedido.total.toString().includes(searchTerm)
    );

    if (loading) {
        return (
            <div className="flex justify-center items-center h-64">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
            </div>
        );
    }

    return (
        <div className="animate-fade-in">
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-8">
                <div>
                    <h1 className="text-3xl font-bold text-neutral-900 tracking-tight">Pedidos</h1>
                    <p className="text-neutral-500 mt-2">Gestiona las órdenes de compra y su estado en tiempo real</p>
                </div>

                <div className="flex flex-col sm:flex-row w-full md:w-auto gap-4">
                    <div className="relative flex-1 sm:w-72">
                        <span className="absolute left-3 top-1/2 -translate-y-1/2 text-neutral-400">🔍</span>
                        <input
                            type="text"
                            placeholder="Buscar pedidos..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pl-10 pr-4 py-2.5 bg-white border border-neutral-200 rounded-xl focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all shadow-sm"
                        />
                    </div>
                    <button
                        onClick={() => handleOpenModal()}
                        className="bg-primary-600 text-white px-5 py-2.5 rounded-xl font-semibold shadow-sm shadow-primary-500/30 hover:bg-primary-700 hover:shadow-md hover:shadow-primary-500/40 transition-all flex items-center justify-center whitespace-nowrap gap-2"
                        title="Nuevo Pedido"
                    >
                        <span className="text-lg">➕</span>
                        <span>Nuevo Pedido</span>
                    </button>
                </div>
            </div>

            <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 overflow-hidden">
                <div className="overflow-x-auto">
                    <table className="w-full text-left border-collapse">
                        <thead>
                            <tr className="bg-neutral-50/50 border-b border-neutral-200">
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 uppercase tracking-wider">ID</th>
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 uppercase tracking-wider">Cliente / Usuario</th>
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 uppercase tracking-wider">Dirección</th>
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 uppercase tracking-wider">Total</th>
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 uppercase tracking-wider">Estado</th>
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 uppercase tracking-wider">Acciones</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-neutral-100">
                            {filteredPedidos.map((pedido) => (
                                <tr key={pedido.id} className="hover:bg-neutral-50/50 transition-colors">
                                    <td className="px-6 py-4 font-bold text-neutral-900">
                                        #{pedido.id.toString().padStart(4, '0')}
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="font-semibold text-neutral-900">ID: {pedido.clienteId}</div>
                                        <div className="text-xs text-neutral-500">Usr: {pedido.usuarioId}</div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="text-sm font-medium text-neutral-700 truncate max-w-xs" title={pedido.direccionEntrega}>
                                            {pedido.direccionEntrega || <span className="text-neutral-400 italic">No especificada</span>}
                                        </div>
                                    </td>
                                    <td className="px-6 py-4 font-bold text-neutral-900">
                                        ${Number(pedido.total).toLocaleString('es-CO')}
                                    </td>
                                    <td className="px-6 py-4">
                                        <span className={`inline-flex px-2.5 py-1 rounded-md text-xs font-bold border ${getEstadoStyles(pedido.estado)}`}>
                                            {getEstadoText(pedido.estado)}
                                        </span>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="flex items-center gap-1.5">
                                            <button
                                                onClick={() => handleOpenModal(pedido)}
                                                className="p-1.5 text-primary-600 hover:bg-primary-50 rounded-lg transition-colors border border-transparent hover:border-primary-100"
                                                title="Editar"
                                            >
                                                ✏️
                                            </button>
                                            <button
                                                onClick={() => handleDeletePermanently(pedido.id)}
                                                className="p-1.5 text-red-600 hover:bg-red-50 rounded-lg transition-colors border border-transparent hover:border-red-100"
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

                    {filteredPedidos.length === 0 && (
                        <div className="flex flex-col justify-center items-center py-16 text-neutral-500">
                            <span className="text-5xl mb-4">📭</span>
                            <span className="font-medium">No se encontraron pedidos.</span>
                        </div>
                    )}
                </div>
            </div>

            {/* Modal */}
            {showModal && (
                <div className="fixed inset-0 bg-neutral-900/40 backdrop-blur-sm flex items-center justify-center z-50 p-4 animate-fade-in">
                    <div className="bg-white rounded-2xl shadow-xl w-full max-w-md max-h-[90vh] flex flex-col overflow-hidden">
                        <div className="p-6 border-b border-neutral-100 flex justify-between items-center">
                            <h2 className="text-xl text-neutral-900 font-bold tracking-tight">
                                {editingPedido ? 'Editar Pedido' : 'Nuevo Pedido'}
                            </h2>
                            <button onClick={handleCloseModal} className="text-neutral-400 hover:text-neutral-600 bg-neutral-50 hover:bg-neutral-100 rounded-lg p-1.5 transition-colors">
                                ✕
                            </button>
                        </div>

                        <div className="p-6 overflow-y-auto">
                            <form id="pedidoForm" onSubmit={handleSubmit} className="space-y-5">
                                <div className="grid grid-cols-2 gap-4">
                                    <div>
                                        <label className="block text-sm font-semibold text-neutral-700 mb-1.5">Usuario ID</label>
                                        <input
                                            type="number"
                                            value={formData.usuarioId}
                                            onChange={(e) => setFormData({ ...formData, usuarioId: e.target.value })}
                                            className="w-full px-4 py-2.5 bg-neutral-50 border border-neutral-200 rounded-xl focus:bg-white focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all"
                                            required
                                        />
                                    </div>
                                    <div>
                                        <label className="block text-sm font-semibold text-neutral-700 mb-1.5">Cliente ID</label>
                                        <input
                                            type="number"
                                            value={formData.clienteId}
                                            onChange={(e) => setFormData({ ...formData, clienteId: e.target.value })}
                                            className="w-full px-4 py-2.5 bg-neutral-50 border border-neutral-200 rounded-xl focus:bg-white focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all"
                                            required
                                        />
                                    </div>
                                </div>

                                <div>
                                    <label className="block text-sm font-semibold text-neutral-700 mb-1.5">Total ($)</label>
                                    <div className="relative">
                                        <span className="absolute left-3 top-1/2 -translate-y-1/2 text-neutral-400 font-medium">$</span>
                                        <input
                                            type="number"
                                            step="0.01"
                                            value={formData.total}
                                            onChange={(e) => setFormData({ ...formData, total: e.target.value })}
                                            className="w-full pl-8 pr-4 py-2.5 bg-neutral-50 border border-neutral-200 rounded-xl focus:bg-white focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all"
                                            required
                                        />
                                    </div>
                                </div>

                                <div>
                                    <label className="block text-sm font-semibold text-neutral-700 mb-1.5">Estado del Pedido</label>
                                    <select
                                        value={formData.estado}
                                        onChange={(e) => setFormData({ ...formData, estado: Number(e.target.value) })}
                                        className="w-full px-4 py-2.5 bg-neutral-50 border border-neutral-200 rounded-xl focus:bg-white focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all"
                                        required
                                    >
                                        <option value={0}>⏳ Pendiente</option>
                                        <option value={1}>✅ Confirmado</option>
                                        <option value={2}>💰 Pagado</option>
                                        <option value={3}>🚚 Enviado</option>
                                        <option value={4}>❌ Cancelado</option>
                                    </select>
                                </div>

                                <div>
                                    <label className="block text-sm font-semibold text-neutral-700 mb-1.5">Dirección de Entrega</label>
                                    <textarea
                                        value={formData.direccionEntrega || ''}
                                        onChange={(e) => setFormData({ ...formData, direccionEntrega: e.target.value })}
                                        className="w-full px-4 py-2.5 bg-neutral-50 border border-neutral-200 rounded-xl focus:bg-white focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all resize-none"
                                        rows="2"
                                        placeholder="Ingrese la dirección completa..."
                                        required
                                    />
                                </div>

                                <div>
                                    <label className="block text-sm font-semibold text-neutral-700 mb-1.5">Referencia Wompi</label>
                                    <input
                                        type="text"
                                        value={formData.referenciaWompi || ''}
                                        onChange={(e) => setFormData({ ...formData, referenciaWompi: e.target.value })}
                                        className="w-full px-4 py-2.5 bg-neutral-50 border border-neutral-200 rounded-xl focus:bg-white focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all"
                                        placeholder="Opcional..."
                                    />
                                </div>
                            </form>
                        </div>

                        <div className="p-6 border-t border-neutral-100 bg-neutral-50 rounded-b-2xl flex gap-3">
                            <button
                                type="button"
                                onClick={handleCloseModal}
                                className="flex-1 bg-white border border-neutral-200 text-neutral-700 py-2.5 rounded-xl font-semibold shadow-sm hover:bg-neutral-50 transition-colors"
                            >
                                Cancelar
                            </button>
                            <button
                                type="submit"
                                form="pedidoForm"
                                className="flex-1 bg-primary-600 text-white py-2.5 rounded-xl font-semibold shadow-sm hover:bg-primary-700 hover:shadow-primary-500/30 transition-all"
                            >
                                Guardar
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default Pedidos;
