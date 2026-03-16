import { useState, useEffect } from 'react';
import api from '../api/client';

function Pagos() {
    const [pagos, setPagos] = useState([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [showModal, setShowModal] = useState(false);
    const [editingPago, setEditingPago] = useState(null);
    const [formData, setFormData] = useState({
        pedidoId: 0,
        monto: 0,
        metodoPago: '',
        estado: 1, // 1: Pendiente, 2: Completado, etc.
        referenciaTransaccion: ''
    });

    useEffect(() => {
        fetchPagos();
    }, []);

    const fetchPagos = async () => {
        try {
            const response = await api.get('/api/pagos'); // Endpoint asumido de PagosController
            setPagos(response.data);
        } catch (error) {
            console.error('Error fetching pagos:', error);
        } finally {
            setLoading(false);
        }
    };

    const handleOpenModal = (pago = null) => {
        if (pago) {
            setEditingPago(pago);
            setFormData(pago);
        } else {
            setEditingPago(null);
            setFormData({
                pedidoId: 0,
                monto: 0,
                metodoPago: '',
                estado: 1,
                referenciaTransaccion: ''
            });
        }
        setShowModal(true);
    };

    const handleCloseModal = () => {
        setShowModal(false);
        setEditingPago(null);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        try {
            const dataToSave = {
                id: editingPago ? Number(editingPago.id) : 0,
                pedidoId: Number(formData.pedidoId),
                monto: Number(formData.monto),
                metodoPago: formData.metodoPago,
                estado: Number(formData.estado),
                referenciaTransaccion: formData.referenciaTransaccion
            };

            if (editingPago) {
                await api.put(`/api/pagos/${editingPago.id}`, dataToSave);
                alert("¡Pago actualizado!");
            } else {
                await api.post('/api/pagos', dataToSave);
                alert("¡Pago agregado!");
            }

            fetchPagos();
            handleCloseModal();

        } catch (error) {
            console.error('Error:', error);
            alert('Error al guardar/modificar el pago');
        }
    };

    const handleDeletePermanently = async (id) => {
        if (!confirm('¿Estás seguro de eliminar este pago?')) return;

        try {
            await api.delete(`/api/pagos/${id}`);
            fetchPagos();
        } catch (error) {
            console.error('Error deleting pago:', error);
        }
    }

    const getEstadoText = (estadoEnum) => {
        switch (estadoEnum) {
            case 1: return 'Pendiente';
            case 2: return 'Completado';
            case 3: return 'Rechazado';
            case 4: return 'Cancelado';
            default: return 'Desconocido';
        }
    };

    const filteredPagos = pagos.filter(pago =>
        pago.referenciaTransaccion.toLowerCase().includes(searchTerm.toLowerCase()) ||
        pago.metodoPago.toLowerCase().includes(searchTerm.toLowerCase()) ||
        pago.monto.toString().includes(searchTerm)
    );

    if (loading) {
        return <div className="text-center py-12">Cargando pagos...</div>;
    }

    return (
        <div>
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-8">
                <div>
                    <h1 className="text-3xl font-bold text-gray-800">Pagos</h1>
                    <p className="text-gray-600 mt-2">Gestiona el historial de transacciones</p>
                </div>

                <div className="flex flex-col sm:flex-row w-full md:w-auto gap-4">
                    <div className="relative flex-1 sm:w-64">
                        <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400">🔍</span>
                        <input
                            type="text"
                            placeholder="Buscar pagos..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pl-10 pr-4 py-2 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 transition-all"
                        />
                    </div>
                    <button
                        onClick={() => handleOpenModal()}
                        className="bg-primary-600 text-white p-3 md:px-6 md:py-3 rounded-lg font-medium hover:bg-primary-700 transition-colors flex items-center justify-center whitespace-nowrap"
                        title="Nuevo Pago"
                    >
                        <span className="text-xl md:mr-2">➕</span>
                        <span className="hidden md:inline">Nuevo Pago</span>
                    </button>
                </div>
            </div>

            <div className="bg-white rounded-xl shadow-md overflow-x-auto">
                <table className="w-full">
                    <thead className="bg-gray-50 border-b">
                        <tr>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Pedido ID</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Monto</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Método</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Estado</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Referencia</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Acciones</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-200">
                        {filteredPagos.map((pago) => (
                            <tr key={pago.id} className="hover:bg-gray-50">
                                <td className="px-6 py-4 font-medium text-gray-900">{pago.pedidoId}</td>
                                <td className="px-6 py-4 text-gray-900">${pago.monto.toLocaleString('es-CO')}</td>
                                <td className="px-6 py-4 text-gray-900">{pago.metodoPago}</td>
                                <td className="px-6 py-4">
                                    <span className={`px-3 py-1 rounded-full text-sm font-medium ${pago.estado === 2 ? 'bg-green-100 text-green-800' :
                                        pago.estado === 1 ? 'bg-yellow-100 text-yellow-800' : 'bg-red-100 text-red-800'
                                        }`}>
                                        {getEstadoText(pago.estado)}
                                    </span>
                                </td>
                                <td className="px-6 py-4 text-gray-500 text-sm">{pago.referenciaTransaccion}</td>
                                <td className="px-6 py-4">
                                    <div className="flex space-x-2">
                                        <button
                                            onClick={() => handleOpenModal(pago)}
                                            className="p-2 text-primary-600 hover:bg-primary-50 rounded-lg transition-colors"
                                            title="Editar"
                                        >
                                            ✏️
                                        </button>
                                        <button
                                            onClick={() => handleDeletePermanently(pago.id)}
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
            </div>

            {/* Modal */}
            {showModal && (
                <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
                    <div className="bg-white rounded-xl p-8 max-w-md w-full max-h-[90vh] overflow-y-auto">
                        <h2 className="text-2xl text-gray-700 font-bold mb-6">
                            {editingPago ? 'Editar Pago' : 'Nuevo Pago'}
                        </h2>

                        <form onSubmit={handleSubmit} className="space-y-4">
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Pedido ID</label>
                                <input type="number" value={formData.pedidoId} onChange={(e) => setFormData({ ...formData, pedidoId: e.target.value })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500" required />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Monto</label>
                                <input type="number" step="0.01" value={formData.monto} onChange={(e) => setFormData({ ...formData, monto: e.target.value })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500" required />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Método de Pago</label>
                                <input type="text" value={formData.metodoPago || ''} onChange={(e) => setFormData({ ...formData, metodoPago: e.target.value })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500" required />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Estado</label>
                                <select value={formData.estado} onChange={(e) => setFormData({ ...formData, estado: Number(e.target.value) })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500" required>
                                    <option value={1}>Pendiente</option>
                                    <option value={2}>Completado</option>
                                    <option value={3}>Rechazado</option>
                                    <option value={4}>Cancelado</option>
                                </select>
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Referencia Transacción</label>
                                <input type="text" value={formData.referenciaTransaccion || ''} onChange={(e) => setFormData({ ...formData, referenciaTransaccion: e.target.value })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500" />
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

export default Pagos;
