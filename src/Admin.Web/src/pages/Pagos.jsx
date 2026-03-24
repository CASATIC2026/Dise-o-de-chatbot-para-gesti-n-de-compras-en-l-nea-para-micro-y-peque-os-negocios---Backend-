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

    const getEstadoColor = (estadoEnum) => {
        switch (estadoEnum) {
            case 1: return 'bg-amber-100 text-amber-700 border-amber-200';
            case 2: return 'bg-emerald-100 text-emerald-700 border-emerald-200';
            case 3: return 'bg-rose-100 text-rose-700 border-rose-200';
            case 4: return 'bg-neutral-100 text-neutral-600 border-neutral-200';
            default: return 'bg-neutral-100 text-neutral-600 border-neutral-200';
        }
    };

    const filteredPagos = pagos.filter(pago =>
        (pago.referenciaTransaccion && pago.referenciaTransaccion.toLowerCase().includes(searchTerm.toLowerCase())) ||
        (pago.metodoPago && pago.metodoPago.toLowerCase().includes(searchTerm.toLowerCase())) ||
        (pago.monto && pago.monto.toString().includes(searchTerm))
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
                    <h1 className="text-3xl font-bold text-neutral-900 tracking-tight">Pagos</h1>
                    <p className="text-neutral-500 mt-2">Gestiona el historial de transacciones</p>
                </div>

                <div className="flex flex-col sm:flex-row w-full md:w-auto gap-4">
                    <div className="relative flex-1 sm:w-72">
                        <span className="absolute left-3 top-1/2 -translate-y-1/2 text-neutral-400">🔍</span>
                        <input
                            type="text"
                            placeholder="Buscar pagos..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pl-10 pr-4 py-2.5 bg-white border border-neutral-200 rounded-xl focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all shadow-sm"
                        />
                    </div>
                    <button
                        onClick={() => handleOpenModal()}
                        className="bg-primary-600 text-white px-5 py-2.5 rounded-xl font-semibold shadow-sm shadow-primary-500/30 hover:bg-primary-700 hover:shadow-md hover:shadow-primary-500/40 transition-all flex items-center justify-center whitespace-nowrap gap-2"
                        title="Nuevo Pago"
                    >
                        <span className="text-lg">➕</span>
                        <span>Nuevo Pago</span>
                    </button>
                </div>
            </div>

            <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 overflow-hidden">
                <div className="overflow-x-auto">
                    <table className="w-full text-left border-collapse">
                        <thead>
                            <tr className="bg-neutral-50/50 border-b border-neutral-200">
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 uppercase tracking-wider">Pedido</th>
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 uppercase tracking-wider">Monto</th>
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 uppercase tracking-wider">Método</th>
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 uppercase tracking-wider">Estado</th>
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 uppercase tracking-wider">Referencia</th>
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 uppercase tracking-wider">Acciones</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-neutral-100">
                            {filteredPagos.map((pago) => (
                                <tr key={pago.id} className="hover:bg-neutral-50/50 transition-colors">
                                    <td className="px-6 py-4">
                                        <div className="font-bold text-neutral-900 border border-neutral-200 bg-white rounded-md px-2 py-1 inline-block text-sm">
                                            #{pago.pedidoId}
                                        </div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="font-bold text-emerald-600">${pago.monto.toLocaleString('es-CO')}</div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="text-sm font-medium text-neutral-700 flex items-center gap-1.5 border border-neutral-200 bg-neutral-50 rounded-md px-2 py-1 fit-content">
                                            <span>💳</span> {pago.metodoPago}
                                        </div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <span className={`px-2.5 py-1 rounded-md text-xs font-bold border ${getEstadoColor(pago.estado)}`}>
                                            {getEstadoText(pago.estado)}
                                        </span>
                                    </td>
                                    <td className="px-6 py-4 text-neutral-500 text-sm font-medium font-mono">
                                        {pago.referenciaTransaccion || <span className="text-neutral-300 italic">Sin Ref.</span>}
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="flex items-center gap-1.5">
                                            <button
                                                onClick={() => handleOpenModal(pago)}
                                                className="p-1.5 text-primary-600 hover:bg-primary-50 rounded-lg transition-colors border border-transparent hover:border-primary-100"
                                                title="Editar"
                                            >
                                                ✏️
                                            </button>
                                            <button
                                                onClick={() => handleDeletePermanently(pago.id)}
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

                    {filteredPagos.length === 0 && (
                        <div className="flex flex-col justify-center items-center py-16 text-neutral-500">
                            <span className="text-5xl mb-4">💳</span>
                            <span className="font-medium">No se encontraron pagos.</span>
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
                                {editingPago ? 'Editar Pago' : 'Nuevo Pago'}
                            </h2>
                            <button onClick={handleCloseModal} className="text-neutral-400 hover:text-neutral-600 bg-neutral-50 hover:bg-neutral-100 rounded-lg p-1.5 transition-colors">
                                ✕
                            </button>
                        </div>

                        <div className="p-6 overflow-y-auto">
                            <form id="pagoForm" onSubmit={handleSubmit} className="space-y-5">
                                <div className="grid grid-cols-2 gap-4">
                                    <div>
                                        <label className="block text-sm font-semibold text-neutral-700 mb-1.5">Pedido ID <span className="text-red-500">*</span></label>
                                        <input
                                            type="number"
                                            value={formData.pedidoId}
                                            onChange={(e) => setFormData({ ...formData, pedidoId: e.target.value })}
                                            className="w-full px-4 py-2.5 bg-neutral-50 border border-neutral-200 rounded-xl focus:bg-white focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all"
                                            required
                                        />
                                    </div>
                                    <div>
                                        <label className="block text-sm font-semibold text-neutral-700 mb-1.5">Monto <span className="text-red-500">*</span></label>
                                        <input
                                            type="number"
                                            step="0.01"
                                            value={formData.monto}
                                            onChange={(e) => setFormData({ ...formData, monto: e.target.value })}
                                            className="w-full px-4 py-2.5 bg-neutral-50 border border-neutral-200 rounded-xl focus:bg-white focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all font-mono"
                                            required
                                        />
                                    </div>
                                </div>
                                <div className="grid grid-cols-2 gap-4">
                                    <div>
                                        <label className="block text-sm font-semibold text-neutral-700 mb-1.5">Método de Pago <span className="text-red-500">*</span></label>
                                        <input
                                            type="text"
                                            value={formData.metodoPago || ''}
                                            onChange={(e) => setFormData({ ...formData, metodoPago: e.target.value })}
                                            className="w-full px-4 py-2.5 bg-neutral-50 border border-neutral-200 rounded-xl focus:bg-white focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all"
                                            placeholder="Ej. Tarjeta"
                                            required
                                        />
                                    </div>
                                    <div>
                                        <label className="block text-sm font-semibold text-neutral-700 mb-1.5">Estado <span className="text-red-500">*</span></label>
                                        <select
                                            value={formData.estado}
                                            onChange={(e) => setFormData({ ...formData, estado: Number(e.target.value) })}
                                            className="w-full px-4 py-2.5 bg-neutral-50 border border-neutral-200 rounded-xl focus:bg-white focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all font-medium"
                                            required
                                        >
                                            <option value={1}>Pendiente</option>
                                            <option value={2}>Completado</option>
                                            <option value={3}>Rechazado</option>
                                            <option value={4}>Cancelado</option>
                                        </select>
                                    </div>
                                </div>
                                <div>
                                    <label className="block text-sm font-semibold text-neutral-700 mb-1.5">Referencia Transacción</label>
                                    <input
                                        type="text"
                                        value={formData.referenciaTransaccion || ''}
                                        onChange={(e) => setFormData({ ...formData, referenciaTransaccion: e.target.value })}
                                        className="w-full px-4 py-2.5 bg-neutral-50 border border-neutral-200 rounded-xl focus:bg-white focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all font-mono text-sm"
                                        placeholder="Ej. TX-123456789"
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
                                form="pagoForm"
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

export default Pagos;
