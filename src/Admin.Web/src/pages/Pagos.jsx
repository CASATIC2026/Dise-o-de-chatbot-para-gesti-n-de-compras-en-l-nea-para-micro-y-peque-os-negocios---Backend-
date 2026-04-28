import { useState, useEffect } from 'react';
import api from '../api/client';
import { SearchIcon, AddNewIcon, EditIcon, DeleteIcon, PaymentsIcon, CloseIcon } from '../components/Icons';

// Shared dark-mode input class
const inputCls = "w-full px-4 py-2.5 bg-neutral-50 dark:bg-dark-input border border-neutral-200 dark:border-dark-border text-neutral-900 dark:text-neutral-100 rounded-xl focus:bg-white dark:focus:bg-dark-surface focus:outline-none focus:border-primary-500 dark:focus:border-cyan-500 focus:ring-4 focus:ring-primary-500/10 dark:focus:ring-cyan-500/10 transition-all";
const labelCls = "block text-sm font-semibold text-neutral-700 dark:text-neutral-300 mb-1.5";

function Pagos() {
    const [pagos, setPagos] = useState([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [showModal, setShowModal] = useState(false);
    const [editingPago, setEditingPago] = useState(null);
    const [formData, setFormData] = useState({ pedidoId: 0, monto: 0, metodoPago: '', estado: 1, referenciaTransaccion: '' });

    useEffect(() => { fetchPagos(); }, []);

    const fetchPagos = async () => {
        try { const r = await api.get('/api/pagos'); setPagos(r.data); }
        catch (e) { console.error('Error fetching pagos:', e); }
        finally { setLoading(false); }
    };

    const handleOpenModal = (pago = null) => {
        if (pago) { setEditingPago(pago); setFormData(pago); }
        else { setEditingPago(null); setFormData({ pedidoId: 0, monto: 0, metodoPago: '', estado: 1, referenciaTransaccion: '' }); }
        setShowModal(true);
    };

    const handleCloseModal = () => { setShowModal(false); setEditingPago(null); };

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            const d = { id: editingPago ? Number(editingPago.id) : 0, pedidoId: Number(formData.pedidoId), monto: Number(formData.monto), metodoPago: formData.metodoPago, estado: Number(formData.estado), referenciaTransaccion: formData.referenciaTransaccion };
            if (editingPago) { await api.put(`/api/pagos/${editingPago.id}`, d); alert('¡Pago actualizado!'); }
            else { await api.post('/api/pagos', d); alert('¡Pago agregado!'); }
            fetchPagos(); handleCloseModal();
        } catch (e) { console.error('Error:', e); alert('Error al guardar/modificar el pago'); }
    };

    const handleDeletePermanently = async (id) => {
        if (!confirm('¿Estás seguro de eliminar este pago?')) return;
        try { await api.delete(`/api/pagos/${id}`); fetchPagos(); }
        catch (e) { console.error('Error deleting pago:', e); }
    };

    const getEstadoText = (s) => ({ 1: 'Pendiente', 2: 'Completado', 3: 'Rechazado', 4: 'Cancelado' }[s] || 'Desconocido');
    const getEstadoColor = (s) => ({ 1: 'bg-amber-100 dark:bg-amber-900/20 text-amber-700 dark:text-amber-400 border-amber-200 dark:border-amber-800/30', 2: 'bg-emerald-100 dark:bg-emerald-900/20 text-emerald-700 dark:text-emerald-400 border-emerald-200 dark:border-emerald-800/30', 3: 'bg-rose-100 dark:bg-rose-900/20 text-rose-700 dark:text-rose-400 border-rose-200 dark:border-rose-800/30', 4: 'bg-neutral-100 dark:bg-dark-input text-neutral-600 dark:text-neutral-400 border-neutral-200 dark:border-dark-border' }[s] || 'bg-neutral-100 dark:bg-dark-input text-neutral-600 dark:text-neutral-400 border-neutral-200 dark:border-dark-border');

    const filteredPagos = pagos.filter(p =>
        (p.referenciaTransaccion && p.referenciaTransaccion.toLowerCase().includes(searchTerm.toLowerCase())) ||
        (p.metodoPago && p.metodoPago.toLowerCase().includes(searchTerm.toLowerCase())) ||
        (p.monto && p.monto.toString().includes(searchTerm))
    );

    if (loading) return <div className="flex justify-center items-center h-64"><div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600 dark:border-cyan-500"></div></div>;

    return (
        <div className="animate-fade-in">
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-8">
                <div>
                    <h1 className="text-3xl font-bold text-neutral-900 dark:text-neutral-100 tracking-tight">Pagos</h1>
                    <p className="text-neutral-500 dark:text-neutral-400 mt-2">Gestiona el historial de transacciones</p>
                </div>
                <div className="flex flex-col sm:flex-row w-full md:w-auto gap-4">
                    <div className="relative flex-1 sm:w-72">
                        <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-neutral-400" />
                        <input type="text" placeholder="Buscar pagos..." value={searchTerm} onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pl-10 pr-4 py-2.5 bg-white dark:bg-dark-input border border-neutral-200 dark:border-dark-border text-neutral-900 dark:text-neutral-100 placeholder:text-neutral-400 dark:placeholder:text-neutral-600 rounded-xl focus:outline-none focus:border-primary-500 dark:focus:border-cyan-500 focus:ring-4 focus:ring-primary-500/10 dark:focus:ring-cyan-500/10 transition-all shadow-sm dark:shadow-none" />
                    </div>
                    <button onClick={() => handleOpenModal()} className="bg-primary-600 dark:bg-cyan-600 text-white px-5 py-2.5 rounded-xl font-semibold shadow-sm shadow-primary-500/30 dark:shadow-cyan-500/20 hover:bg-primary-700 dark:hover:bg-cyan-700 hover:shadow-md transition-all flex items-center justify-center whitespace-nowrap gap-2">
                        <AddNewIcon className="w-5 h-5" /><span>Nuevo Pago</span>
                    </button>
                </div>
            </div>

            <div className="bg-white dark:bg-dark-surface rounded-2xl shadow-sm dark:shadow-none border border-neutral-200 dark:border-dark-border overflow-hidden">
                <div className="overflow-x-auto">
                    <table className="w-full text-left border-collapse">
                        <thead>
                            <tr className="bg-neutral-50/50 dark:bg-dark-input/50 border-b border-neutral-200 dark:border-dark-border">
                                {['Pedido', 'Monto', 'Método', 'Estado', 'Referencia', 'Acciones'].map(h => (
                                    <th key={h} className="px-6 py-4 text-xs font-bold text-neutral-500 dark:text-neutral-500 uppercase tracking-wider">{h}</th>
                                ))}
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-neutral-100 dark:divide-dark-border">
                            {filteredPagos.map((pago) => (
                                <tr key={pago.id} className="hover:bg-neutral-50/50 dark:hover:bg-dark-input/50 transition-colors">
                                    <td className="px-6 py-4">
                                        <div className="font-bold text-neutral-900 dark:text-neutral-100 border border-neutral-200 dark:border-dark-border bg-white dark:bg-dark-input rounded-md px-2 py-1 inline-block text-sm">#{pago.pedidoId}</div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="font-bold text-emerald-600 dark:text-emerald-400">${pago.monto.toLocaleString('es-CO')}</div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="text-sm font-medium text-neutral-700 dark:text-neutral-300 flex items-center gap-1.5 border border-neutral-200 dark:border-dark-border bg-neutral-50 dark:bg-dark-input rounded-md px-2 py-1 w-max">
                                            <PaymentsIcon className="w-3.5 h-3.5" /> {pago.metodoPago}
                                        </div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <span className={`px-2.5 py-1 rounded-md text-xs font-bold border ${getEstadoColor(pago.estado)}`}>
                                            {getEstadoText(pago.estado)}
                                        </span>
                                    </td>
                                    <td className="px-6 py-4 text-neutral-500 dark:text-neutral-400 text-sm font-medium font-mono">
                                        {pago.referenciaTransaccion || <span className="text-neutral-300 dark:text-neutral-600 italic">Sin Ref.</span>}
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="flex items-center gap-1.5">
                                            <button onClick={() => handleOpenModal(pago)} title="Editar"
                                                className="p-1.5 text-primary-600 dark:text-cyan-400 hover:bg-primary-50 dark:hover:bg-cyan-900/20 rounded-lg transition-colors border border-transparent hover:border-primary-100 dark:hover:border-cyan-800/30">
                                                <EditIcon className="w-4 h-4" />
                                            </button>
                                            <button onClick={() => handleDeletePermanently(pago.id)} title="Eliminar"
                                                className="p-1.5 text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-lg transition-colors border border-transparent hover:border-red-100 dark:hover:border-red-800/30">
                                                <DeleteIcon className="w-4 h-4" />
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                    {filteredPagos.length === 0 && (
                        <div className="flex flex-col justify-center items-center py-16 text-neutral-500 dark:text-neutral-500">
                            <PaymentsIcon className="w-12 h-12 mb-4 opacity-40" />
                            <span className="font-medium">No se encontraron pagos.</span>
                        </div>
                    )}
                </div>
            </div>

            {showModal && (
                <div className="fixed inset-0 bg-neutral-900/40 dark:bg-black/60 backdrop-blur-sm flex items-center justify-center z-50 p-4 animate-fade-in">
                    <div className="bg-white dark:bg-dark-surface rounded-2xl shadow-xl dark:shadow-black/40 w-full max-w-md max-h-[90vh] flex flex-col overflow-hidden border border-neutral-100 dark:border-dark-border">
                        <div className="p-6 border-b border-neutral-100 dark:border-dark-border flex justify-between items-center">
                            <h2 className="text-xl text-neutral-900 dark:text-neutral-100 font-bold tracking-tight">{editingPago ? 'Editar Pago' : 'Nuevo Pago'}</h2>
                            <button onClick={handleCloseModal} className="text-neutral-400 hover:text-neutral-600 dark:hover:text-neutral-300 bg-neutral-50 dark:bg-dark-input hover:bg-neutral-100 dark:hover:bg-dark-border rounded-lg p-1.5 transition-colors"><CloseIcon className="w-4 h-4" /></button>
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
                        <div className="p-6 border-t border-neutral-100 dark:border-dark-border bg-neutral-50 dark:bg-dark-input rounded-b-2xl flex gap-3">
                            <button type="button" onClick={handleCloseModal} className="flex-1 bg-white dark:bg-dark-surface border border-neutral-200 dark:border-dark-border text-neutral-700 dark:text-neutral-300 py-2.5 rounded-xl font-semibold shadow-sm hover:bg-neutral-50 dark:hover:bg-dark-elevated transition-colors">Cancelar</button>
                            <button type="submit" form="pagoForm" className="flex-1 bg-primary-600 dark:bg-cyan-600 text-white py-2.5 rounded-xl font-semibold shadow-sm hover:bg-primary-700 dark:hover:bg-cyan-700 transition-all">Guardar</button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default Pagos;
