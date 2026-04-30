import { useEffect, useState } from 'react';
import api from '../api/client';
import { SearchIcon, AddNewIcon, EditIcon, DeleteIcon, PaymentsIcon, CloseIcon } from '../components/Icons';

// Shared input class
const inputCls = "w-full px-4 py-2.5 bg-gray-50 border border-gray-200 text-gray-900 rounded-xl focus:bg-white focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all";
const labelCls = "block text-sm font-semibold text-gray-700 mb-1.5";

function Pagos() {
    const [pagos, setPagos] = useState([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [showModal, setShowModal] = useState(false);
    const [editingPago, setEditingPago] = useState(null);
    const [formData, setFormData] = useState({ pedidoId: '', monto: '', metodoPago: '', estado: 1, referenciaTransaccion: '' });

    useEffect(() => { fetchPagos(); }, []);

    const fetchPagos = async () => {
        try {
            setLoading(true);
            const response = await api.get('/admin/pagos');
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

    const handleCloseModal = () => { setShowModal(false); setEditingPago(null); };

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
                await api.put(`/admin/pagos/${editingPago.id}`, dataToSave);
            } else {
                await api.post('/admin/pagos', dataToSave);
            }

            await fetchPagos();
            handleCloseModal();
            alert(editingPago ? 'Pago actualizado correctamente' : 'Pago registrado correctamente');
        } catch (error) {
            console.error('Error al guardar:', error);
            alert('Error al procesar la solicitud');
        }
    };

    const handleDeletePermanently = async (id) => {
        if (!confirm('¿Estás seguro de eliminar este registro?')) return;
        try {
            await api.delete(`/admin/pagos/${id}`);
            await fetchPagos();
        } catch (error) {
            console.error('Error deleting pago:', error);
        }
    };

    const getEstadoText = (s) => ({ 1: 'Pendiente', 2: 'Completado', 3: 'Rechazado', 4: 'Cancelado' }[s] || 'Desconocido');
    const getEstadoColor = (s) => ({
        1: 'bg-amber-100 text-amber-700 border-amber-200',
        2: 'bg-emerald-100 text-emerald-700 border-emerald-200',
        3: 'bg-rose-100 text-rose-700 border-rose-200',
        4: 'bg-gray-100 text-gray-600 border-gray-200'
    }[s] || 'bg-gray-100 text-gray-600 border-gray-200');

    const filteredPagos = pagos.filter(pago =>
        (pago.referenciaTransaccion?.toLowerCase() || '').includes(searchTerm.toLowerCase()) ||
        pago.metodoPago.toLowerCase().includes(searchTerm.toLowerCase()) ||
        pago.pedidoId.toString().includes(searchTerm)
    );

    if (loading) return <div className="flex justify-center items-center h-64"><div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div></div>;

    return (
        <div className="animate-fade-in">
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-8">
                <div>
                    <h1 className="text-3xl font-bold text-gray-800 tracking-tight">Pagos</h1>
                    <p className="text-gray-500 mt-1">Control de transacciones y estados de pedidos</p>
                </div>
                <div className="flex flex-col sm:flex-row w-full md:w-auto gap-4">
                    <div className="relative flex-1 sm:w-72">
                        <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                        <input type="text" placeholder="Buscar pagos..." value={searchTerm} onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pl-10 pr-4 py-2.5 bg-white border border-gray-200 text-gray-900 placeholder:text-gray-400 rounded-xl focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all shadow-sm" />
                    </div>
                    <button onClick={() => handleOpenModal()} className="bg-primary-600 text-white px-5 py-2.5 rounded-xl font-semibold shadow-sm shadow-primary-500/30 hover:bg-primary-700 hover:shadow-md transition-all flex items-center justify-center whitespace-nowrap gap-2">
                        <AddNewIcon className="w-5 h-5" /><span>Nuevo Pago</span>
                    </button>
                </div>
            </div>

            <div className="bg-white rounded-2xl shadow-sm border border-gray-200 overflow-hidden">
                <div className="overflow-x-auto">
                    <table className="w-full text-left border-collapse">
                        <thead>
                            <tr className="bg-gray-50/50 border-b border-gray-200">
                                {['Pedido', 'Monto', 'Método', 'Estado', 'Referencia', 'Acciones'].map(h => (
                                    <th key={h} className="px-6 py-4 text-xs font-bold text-gray-500 uppercase tracking-wider">{h}</th>
                                ))}
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-100">
                            {filteredPagos.map((pago) => (
                                <tr key={pago.id} className="hover:bg-gray-50/50 transition-colors">
                                    <td className="px-6 py-4">
                                        <div className="font-bold text-gray-900 border border-gray-200 bg-white rounded-md px-2 py-1 inline-block text-sm">#{pago.pedidoId}</div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="font-bold text-emerald-600">${pago.monto.toLocaleString('es-CO')}</div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="text-sm font-medium text-gray-700 flex items-center gap-1.5 border border-gray-200 bg-gray-50 rounded-md px-2 py-1 w-max">
                                            <PaymentsIcon className="w-3.5 h-3.5" /> {pago.metodoPago}
                                        </div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <span className={`px-2.5 py-1 rounded-md text-xs font-bold border ${getEstadoColor(pago.estado)}`}>
                                            {getEstadoText(pago.estado)}
                                        </span>
                                    </td>
                                    <td className="px-6 py-4 text-gray-500 text-sm font-medium font-mono">
                                        {pago.referenciaTransaccion || <span className="text-gray-300 italic">Sin Ref.</span>}
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="flex items-center gap-1.5">
                                            <button onClick={() => handleOpenModal(pago)} title="Editar"
                                                className="p-1.5 text-primary-600 hover:bg-primary-50 rounded-lg transition-colors border border-transparent hover:border-primary-100">
                                                <EditIcon className="w-4 h-4" />
                                            </button>
                                            <button onClick={() => handleDeletePermanently(pago.id)} title="Eliminar"
                                                className="p-1.5 text-red-600 hover:bg-red-50 rounded-lg transition-colors border border-transparent hover:border-red-100">
                                                <DeleteIcon className="w-4 h-4" />
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                    {filteredPagos.length === 0 && (
                        <div className="flex flex-col justify-center items-center py-16 text-gray-500">
                            <PaymentsIcon className="w-12 h-12 mb-4 opacity-40" />
                            <span className="font-medium">No se encontraron pagos.</span>
                        </div>
                    )}
                </div>
            </div>

            {showModal && (
                <div className="fixed inset-0 bg-black/40 backdrop-blur-sm flex items-center justify-center z-50 p-4 animate-fade-in">
                    <div className="bg-white rounded-2xl shadow-xl w-full max-w-md max-h-[90vh] flex flex-col overflow-hidden border border-gray-100">
                        <div className="p-6 border-b border-gray-100 flex justify-between items-center">
                            <h2 className="text-xl text-gray-900 font-bold tracking-tight">{editingPago ? 'Editar Pago' : 'Nuevo Pago'}</h2>
                            <button onClick={handleCloseModal} className="text-gray-400 hover:text-gray-600 bg-gray-50 hover:bg-gray-100 rounded-lg p-1.5 transition-colors"><CloseIcon className="w-4 h-4" /></button>
                        </div>
                        <div className="p-6 overflow-y-auto">
                            <form id="pagoForm" onSubmit={handleSubmit} className="space-y-5">
                                <div className="grid grid-cols-2 gap-4">
                                    <div>
                                        <label className={labelCls}>Pedido ID <span className="text-red-500">*</span></label>
                                        <input type="number" value={formData.pedidoId} onChange={(e) => setFormData({ ...formData, pedidoId: e.target.value })} className={inputCls} required />
                                    </div>
                                    <div>
                                        <label className={labelCls}>Monto <span className="text-red-500">*</span></label>
                                        <input type="number" step="0.01" value={formData.monto} onChange={(e) => setFormData({ ...formData, monto: e.target.value })} className={`${inputCls} font-mono`} required />
                                    </div>
                                </div>
                                <div className="grid grid-cols-2 gap-4">
                                    <div>
                                        <label className={labelCls}>Método de Pago <span className="text-red-500">*</span></label>
                                        <input type="text" value={formData.metodoPago || ''} onChange={(e) => setFormData({ ...formData, metodoPago: e.target.value })} className={inputCls} placeholder="Ej. Tarjeta" required />
                                    </div>
                                    <div>
                                        <label className={labelCls}>Estado <span className="text-red-500">*</span></label>
                                        <select value={formData.estado} onChange={(e) => setFormData({ ...formData, estado: Number(e.target.value) })} className={inputCls} required>
                                            <option value={1}>Pendiente</option>
                                            <option value={2}>Completado</option>
                                            <option value={3}>Rechazado</option>
                                            <option value={4}>Cancelado</option>
                                        </select>
                                    </div>
                                </div>
                                <div>
                                    <label className={labelCls}>Referencia Transacción</label>
                                    <input type="text" value={formData.referenciaTransaccion || ''} onChange={(e) => setFormData({ ...formData, referenciaTransaccion: e.target.value })} className={`${inputCls} font-mono text-sm`} placeholder="Ej. TX-123456789" />
                                </div>
                            </form>
                        </div>
                        <div className="p-6 border-t border-gray-100 bg-gray-50 rounded-b-2xl flex gap-3">
                            <button type="button" onClick={handleCloseModal} className="flex-1 bg-white border border-gray-200 text-gray-700 py-2.5 rounded-xl font-semibold shadow-sm hover:bg-gray-50 transition-colors">Cancelar</button>
                            <button type="submit" form="pagoForm" className="flex-1 bg-primary-600 text-white py-2.5 rounded-xl font-semibold shadow-sm hover:bg-primary-700 transition-all">Guardar</button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default Pagos;
