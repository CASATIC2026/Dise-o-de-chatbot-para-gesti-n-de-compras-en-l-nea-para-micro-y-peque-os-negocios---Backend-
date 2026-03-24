import axios from 'axios';
import { useState, useEffect } from 'react';
import api from '../api/client';

function Pagos() {
    const [pagos, setPagos] = useState([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [showModal, setShowModal] = useState(false);
    const [editingPago, setEditingPago] = useState(null);
    const [formData, setFormData] = useState({
        pedidoId: '',
        monto: '',
        metodoPago: '',
        estado: 1, // 1: Pendiente, 2: Completado, 3: Rechazado, 4: Cancelado
        referenciaTransaccion: ''
    });

    useEffect(() => {
        fetchPagos();
    }, []);

    const fetchPagos = async () => {
        try {
            setLoading(true);
            // Usamos axios directo con la URL que confirmamos que funciona
            // saltándonos el "client.js" solo para esta prueba
            const response = await axios.get('http://localhost:5001/api/pagos', {
                headers: {
                    Authorization: `Bearer ${localStorage.getItem('token')}`
                }
            });
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
            setFormData({
                pedidoId: pago.pedidoId,
                monto: pago.monto,
                metodoPago: pago.metodoPago,
                estado: pago.estado,
                referenciaTransaccion: pago.referenciaTransaccion || ''
            });
        } else {
            setEditingPago(null);
            setFormData({
                pedidoId: '',
                monto: '',
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
                await api.put(`/pagos/${editingPago.id}`, dataToSave);
            } else {
                await api.post('/pagos', dataToSave);
            }

            fetchPagos();
            handleCloseModal();
            alert("¡Inserción exitosa!");
        } catch (error) {
            console.error('Error al guardar:', error);
            alert('Error al procesar la solicitud');
        }
    };


    const obtenerEstadoInfo = (estado) => {
        const estados = {
            1: { texto: 'Pendiente', clase: 'bg-yellow-100 text-yellow-800' },
            2: { texto: 'Completado', clase: 'bg-green-100 text-green-800' },
            3: { texto: 'Rechazado', clase: 'bg-red-100 text-red-800' },
            4: { texto: 'Cancelado', clase: 'bg-gray-100 text-gray-800' }
        };
        return estados[estado] || { texto: 'Desconocido', clase: 'bg-gray-100 text-gray-400' };
    };

    const handleDeletePermanently = async (id) => {
        if (!confirm('¿Estás seguro de eliminar este registro?')) return;
        try {
            await api.delete(`/pagos/${id}`);
            fetchPagos();
        } catch (error) {
            console.error('Error deleting pago:', error);
        }
    };

    const filteredPagos = pagos.filter(pago =>
        (pago.referenciaTransaccion?.toLowerCase() || "").includes(searchTerm.toLowerCase()) ||
        pago.metodoPago.toLowerCase().includes(searchTerm.toLowerCase()) ||
        pago.pedidoId.toString().includes(searchTerm)
    );

    if (loading) return <div className="text-center py-12 font-medium">Cargando historial de pagos...</div>;

    return (
        <div className="p-4">
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-8">
                <div>
                    <h1 className="text-3xl font-bold text-gray-800">Pagos</h1>
                    <p className="text-gray-600 mt-1">Control de transacciones y estados de pedidos</p>
                </div>

                <div className="flex flex-col sm:flex-row w-full md:w-auto gap-4">
                    <div className="relative flex-1 sm:w-64">
                        <span className="absolute left-3 top-1/2 -translate-y-1/2">🔍</span>
                        <input
                            type="text"
                            placeholder="Buscar referencia o pedido..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pl-10 pr-4 py-2 border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none"
                        />
                    </div>
                    <button
                        onClick={() => handleOpenModal()}
                        className="bg-blue-600 text-white px-6 py-2 rounded-lg font-medium hover:bg-blue-700 transition-all flex items-center justify-center"
                    >
                        <span className="mr-2">+</span> Nuevo Pago
                    </button>
                </div>
            </div>

            <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-x-auto">
                <table className="w-full text-left">
                    <thead className="bg-gray-50 border-b border-gray-100">
                        <tr>
                            <th className="px-6 py-4 text-xs font-semibold text-gray-500 uppercase tracking-wider">Pedido ID</th>
                            <th className="px-6 py-4 text-xs font-semibold text-gray-500 uppercase tracking-wider">Monto</th>
                            <th className="px-6 py-4 text-xs font-semibold text-gray-500 uppercase tracking-wider">Método</th>
                            <th className="px-6 py-4 text-xs font-semibold text-gray-500 uppercase tracking-wider">Estado</th>
                            <th className="px-6 py-4 text-xs font-semibold text-gray-500 uppercase tracking-wider">Referencia</th>
                            <th className="px-6 py-4 text-xs font-semibold text-gray-500 uppercase tracking-wider text-center">Acciones</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-100">
                        {filteredPagos.map((pago) => {
                            const estadoInfo = obtenerEstadoInfo(pago.estado);
                            return (
                                <tr key={pago.id} className="hover:bg-gray-50/50 transition-colors">
                                    <td className="px-6 py-4 font-bold text-gray-700">#{pago.pedidoId}</td>
                                    <td className="px-6 py-4 text-gray-900 font-semibold">
                                        ${pago.monto.toLocaleString('es-CO', { minimumFractionDigits: 2 })}
                                    </td>
                                    <td className="px-6 py-4 text-gray-600 text-sm uppercase">{pago.metodoPago}</td>
                                    <td className="px-6 py-4">
                                        <span className={`px-3 py-1 rounded-full text-xs font-bold ${estadoInfo.clase}`}>
                                            {estadoInfo.texto}
                                        </span>
                                    </td>
                                    <td className="px-6 py-4 text-gray-500 text-sm font-mono">
                                        {pago.referenciaTransaccion || '---'}
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="flex justify-center space-x-3">
                                            <button onClick={() => handleOpenModal(pago)} className="text-blue-500 hover:text-blue-700" title="Editar">✏️</button>
                                            <button onClick={() => handleDeletePermanently(pago.id)} className="text-red-500 hover:text-red-700" title="Eliminar">🗑️</button>
                                        </div>
                                    </td>
                                </tr>
                            );
                        })}
                    </tbody>
                </table>
                {filteredPagos.length === 0 && (
                    <div className="py-12 text-center text-gray-400">No se encontraron registros de pagos.</div>
                )}
            </div>

            {/* Modal */}
            {showModal && (
                <div className="fixed inset-0 bg-black/40 backdrop-blur-sm flex items-center justify-center z-50 p-4">
                    <div className="bg-white rounded-2xl shadow-2xl max-w-md w-full p-8 overflow-y-auto max-h-screen">
                        <h2 className="text-2xl font-bold text-gray-800 mb-6 border-b pb-4">
                            {editingPago ? 'Actualizar Pago' : 'Registrar Nuevo Pago'}
                        </h2>

                        <form onSubmit={handleSubmit} className="space-y-5">
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-xs font-bold text-gray-500 uppercase mb-1">ID Pedido</label>
                                    <input type="number" value={formData.pedidoId} onChange={(e) => setFormData({ ...formData, pedidoId: e.target.value })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 outline-none" required />
                                </div>
                                <div>
                                    <label className="block text-xs font-bold text-gray-500 uppercase mb-1">Monto</label>
                                    <input type="number" step="0.01" value={formData.monto} onChange={(e) => setFormData({ ...formData, monto: e.target.value })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 outline-none" required />
                                </div>
                            </div>

                            <div>
                                <label className="block text-xs font-bold text-gray-500 uppercase mb-1">Método de Pago</label>
                                <input type="text" placeholder="Ej: Tarjeta, Efectivo, Transferencia" value={formData.metodoPago} onChange={(e) => setFormData({ ...formData, metodoPago: e.target.value })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 outline-none" required />
                            </div>

                            <div>
                                <label className="block text-xs font-bold text-gray-500 uppercase mb-1">Estado del Pago</label>
                                <select value={formData.estado} onChange={(e) => setFormData({ ...formData, estado: e.target.value })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 outline-none">
                                    <option value={1}>Pendiente</option>
                                    <option value={2}>Completado</option>
                                    <option value={3}>Rechazado</option>
                                    <option value={4}>Cancelado</option>
                                </select>
                            </div>

                            <div>
                                <label className="block text-xs font-bold text-gray-500 uppercase mb-1">Referencia / Comprobante</label>
                                <input type="text" value={formData.referenciaTransaccion} onChange={(e) => setFormData({ ...formData, referenciaTransaccion: e.target.value })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 outline-none" />
                            </div>

                            <div className="flex space-x-3 pt-6">
                                <button type="button" onClick={handleCloseModal} className="flex-1 px-4 py-2.5 bg-gray-100 text-gray-600 rounded-xl font-bold hover:bg-gray-200 transition-all">Cancelar</button>
                                <button type="submit" className="flex-1 px-4 py-2.5 bg-blue-600 text-white rounded-xl font-bold hover:bg-blue-700 shadow-lg shadow-blue-200 transition-all">Guardar Pago</button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
}

export default Pagos;