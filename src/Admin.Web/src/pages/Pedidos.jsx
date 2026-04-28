import { useState, useEffect } from 'react';
import api from '../api/client';
import { SearchIcon, AddNewIcon, EditIcon, DeleteIcon, OrdersIcon, ClockIcon, CheckCircleIcon, MoneyIcon, TruckIcon, CloseIcon } from '../components/Icons';

const inputCls = "w-full px-4 py-2.5 bg-neutral-50 dark:bg-dark-input border border-neutral-200 dark:border-dark-border text-neutral-900 dark:text-neutral-100 rounded-xl focus:bg-white dark:focus:bg-dark-surface focus:outline-none focus:border-primary-500 dark:focus:border-cyan-500 focus:ring-4 focus:ring-primary-500/10 dark:focus:ring-cyan-500/10 transition-all";
const labelCls = "block text-sm font-semibold text-neutral-700 dark:text-neutral-300 mb-1.5";

function Pedidos() {
    const [pedidos, setPedidos] = useState([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [showModal, setShowModal] = useState(false);
    const [editingPedido, setEditingPedido] = useState(null);
    const [formData, setFormData] = useState({ usuarioId: 0, clienteId: 0, estado: 0, total: 0, direccionEntrega: '', detallesJson: '[]', referenciaWompi: '' });

    useEffect(() => { fetchPedidos(); }, []);

    const fetchPedidos = async () => {
        try { const r = await api.get('/admin/inventario/pedidos'); setPedidos(r.data); }
        catch (e) { console.error('Error fetching pedidos:', e); }
        finally { setLoading(false); }
    };

    const handleOpenModal = (pedido = null) => {
        if (pedido) { setEditingPedido(pedido); setFormData(pedido); }
        else { setEditingPedido(null); setFormData({ usuarioId: 0, clienteId: 0, estado: 0, total: 0, direccionEntrega: '', detallesJson: '[]', referenciaWompi: '' }); }
        setShowModal(true);
    };

    const handleCloseModal = () => { setShowModal(false); setEditingPedido(null); };

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            const d = { id: editingPedido ? Number(editingPedido.id) : 0, usuarioId: Number(formData.usuarioId), clienteId: Number(formData.clienteId), estado: Number(formData.estado), total: Number(formData.total), direccionEntrega: formData.direccionEntrega, detallesJson: formData.detallesJson, referenciaWompi: formData.referenciaWompi };
            if (editingPedido) { await api.put(`/admin/inventario/pedidos/${editingPedido.id}`, d); alert('¡Pedido actualizado!'); }
            else { await api.post('/admin/inventario/pedidos', d); alert('¡Pedido agregado!'); }
            fetchPedidos(); handleCloseModal();
        } catch (e) { console.error('Error:', e); alert('Error al guardar/modificar el pedido'); }
    };

    const handleDeletePermanently = async (id) => {
        if (!confirm('¿Estás seguro de eliminar este pedido?')) return;
        try { await api.delete(`/admin/inventario/pedidos/${id}`); fetchPedidos(); }
        catch (e) { console.error('Error deleting pedido:', e); }
    };

    const getEstadoStyles = (s) => ({ 0: 'bg-yellow-50 dark:bg-yellow-900/20 text-yellow-700 dark:text-yellow-400 border-yellow-200 dark:border-yellow-800/30', 1: 'bg-blue-50 dark:bg-blue-900/20 text-blue-700 dark:text-blue-400 border-blue-200 dark:border-blue-800/30', 2: 'bg-green-50 dark:bg-green-900/20 text-green-700 dark:text-green-400 border-green-200 dark:border-green-800/30', 3: 'bg-purple-50 dark:bg-purple-900/20 text-purple-700 dark:text-purple-400 border-purple-200 dark:border-purple-800/30', 4: 'bg-red-50 dark:bg-red-900/20 text-red-700 dark:text-red-400 border-red-200 dark:border-red-800/30' }[s] || 'bg-neutral-50 dark:bg-dark-input text-neutral-700 dark:text-neutral-400 border-neutral-200 dark:border-dark-border');
    const getEstadoText = (s) => ({ 0: 'Pendiente', 1: 'Confirmado', 2: 'Pagado', 3: 'Enviado', 4: 'Cancelado' }[s] || 'Desconocido');

    const filteredPedidos = pedidos.filter(p =>
        p.clienteId.toString().includes(searchTerm) ||
        getEstadoText(p.estado).toLowerCase().includes(searchTerm.toLowerCase()) ||
        p.total.toString().includes(searchTerm)
    );

    if (loading) return <div className="flex justify-center items-center h-64"><div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600 dark:border-cyan-500"></div></div>;

    return (
        <div className="animate-fade-in">
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-8">
                <div>
                    <h1 className="text-3xl font-bold text-neutral-900 dark:text-neutral-100 tracking-tight">Pedidos</h1>
                    <p className="text-neutral-500 dark:text-neutral-400 mt-2">Gestiona las órdenes de compra y su estado en tiempo real</p>
                </div>
                <div className="flex flex-col sm:flex-row w-full md:w-auto gap-4">
                    <div className="relative flex-1 sm:w-72">
                        <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-neutral-400" />
                        <input type="text" placeholder="Buscar pedidos..." value={searchTerm} onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pl-10 pr-4 py-2.5 bg-white dark:bg-dark-input border border-neutral-200 dark:border-dark-border text-neutral-900 dark:text-neutral-100 placeholder:text-neutral-400 dark:placeholder:text-neutral-600 rounded-xl focus:outline-none focus:border-primary-500 dark:focus:border-cyan-500 focus:ring-4 focus:ring-primary-500/10 dark:focus:ring-cyan-500/10 transition-all shadow-sm dark:shadow-none" />
                    </div>
                    <button onClick={() => handleOpenModal()} className="bg-primary-600 dark:bg-cyan-600 text-white px-5 py-2.5 rounded-xl font-semibold shadow-sm shadow-primary-500/30 dark:shadow-cyan-500/20 hover:bg-primary-700 dark:hover:bg-cyan-700 hover:shadow-md transition-all flex items-center justify-center whitespace-nowrap gap-2">
                        <AddNewIcon className="w-5 h-5" /><span>Nuevo Pedido</span>
                    </button>
                </div>
            </div>

            <div className="bg-white dark:bg-dark-surface rounded-2xl shadow-sm dark:shadow-none border border-neutral-200 dark:border-dark-border overflow-hidden">
                <div className="overflow-x-auto">
                    <table className="w-full text-left border-collapse">
                        <thead>
                            <tr className="bg-neutral-50/50 dark:bg-dark-input/50 border-b border-neutral-200 dark:border-dark-border">
                                {['ID', 'Cliente / Usuario', 'Dirección', 'Total', 'Estado', 'Acciones'].map(h => (
                                    <th key={h} className="px-6 py-4 text-xs font-bold text-neutral-500 dark:text-neutral-500 uppercase tracking-wider">{h}</th>
                                ))}
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-neutral-100 dark:divide-dark-border">
                            {filteredPedidos.map((pedido) => (
                                <tr key={pedido.id} className="hover:bg-neutral-50/50 dark:hover:bg-dark-input/50 transition-colors">
                                    <td className="px-6 py-4 font-bold text-neutral-900 dark:text-neutral-100">#{pedido.id.toString().padStart(4, '0')}</td>
                                    <td className="px-6 py-4">
                                        <div className="font-semibold text-neutral-900 dark:text-neutral-100">ID: {pedido.clienteId}</div>
                                        <div className="text-xs text-neutral-500 dark:text-neutral-500">Usr: {pedido.usuarioId}</div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="text-sm font-medium text-neutral-700 dark:text-neutral-400 truncate max-w-xs" title={pedido.direccionEntrega}>
                                            {pedido.direccionEntrega || <span className="text-neutral-400 dark:text-neutral-600 italic">No especificada</span>}
                                        </div>
                                    </td>
                                    <td className="px-6 py-4 font-bold text-neutral-900 dark:text-neutral-100">${Number(pedido.total).toLocaleString('es-CO')}</td>
                                    <td className="px-6 py-4">
                                        <span className={`inline-flex px-2.5 py-1 rounded-md text-xs font-bold border ${getEstadoStyles(pedido.estado)}`}>{getEstadoText(pedido.estado)}</span>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="flex items-center gap-1.5">
                                            <button onClick={() => handleOpenModal(pedido)} title="Editar"
                                                className="p-1.5 text-primary-600 dark:text-cyan-400 hover:bg-primary-50 dark:hover:bg-cyan-900/20 rounded-lg transition-colors border border-transparent hover:border-primary-100 dark:hover:border-cyan-800/30">
                                                <EditIcon className="w-4 h-4" />
                                            </button>
                                            <button onClick={() => handleDeletePermanently(pedido.id)} title="Eliminar"
                                                className="p-1.5 text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-lg transition-colors border border-transparent hover:border-red-100 dark:hover:border-red-800/30">
                                                <DeleteIcon className="w-4 h-4" />
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                    {filteredPedidos.length === 0 && (
                        <div className="flex flex-col justify-center items-center py-16 text-neutral-500 dark:text-neutral-500">
                            <OrdersIcon className="w-12 h-12 mb-4 opacity-40" />
                            <span className="font-medium">No se encontraron pedidos.</span>
                        </div>
                    )}
                </div>
            </div>

            {showModal && (
                <div className="fixed inset-0 bg-neutral-900/40 dark:bg-black/60 backdrop-blur-sm flex items-center justify-center z-50 p-4 animate-fade-in">
                    <div className="bg-white dark:bg-dark-surface rounded-2xl shadow-xl dark:shadow-black/40 w-full max-w-md max-h-[90vh] flex flex-col overflow-hidden border border-neutral-100 dark:border-dark-border">
                        <div className="p-6 border-b border-neutral-100 dark:border-dark-border flex justify-between items-center">
                            <h2 className="text-xl text-neutral-900 dark:text-neutral-100 font-bold tracking-tight">{editingPedido ? 'Editar Pedido' : 'Nuevo Pedido'}</h2>
                            <button onClick={handleCloseModal} className="text-neutral-400 hover:text-neutral-600 dark:hover:text-neutral-300 bg-neutral-50 dark:bg-dark-input hover:bg-neutral-100 dark:hover:bg-dark-border rounded-lg p-1.5 transition-colors"><CloseIcon className="w-4 h-4" /></button>
                        </div>
                        <div className="p-6 overflow-y-auto">
                            <form id="pedidoForm" onSubmit={handleSubmit} className="space-y-5">
                                <div className="grid grid-cols-2 gap-4">
                                    <div><label className={labelCls}>Usuario ID</label><input type="number" value={formData.usuarioId} onChange={(e) => setFormData({ ...formData, usuarioId: e.target.value })} className={inputCls} required /></div>
                                    <div><label className={labelCls}>Cliente ID</label><input type="number" value={formData.clienteId} onChange={(e) => setFormData({ ...formData, clienteId: e.target.value })} className={inputCls} required /></div>
                                </div>
                                <div>
                                    <label className={labelCls}>Total ($)</label>
                                    <div className="relative">
                                        <span className="absolute left-3 top-1/2 -translate-y-1/2 text-neutral-400 dark:text-neutral-600 font-medium">$</span>
                                        <input type="number" step="0.01" value={formData.total} onChange={(e) => setFormData({ ...formData, total: e.target.value })} className={`${inputCls} pl-8`} required />
                                    </div>
                                </div>
                                <div>
                                    <label className={labelCls}>Estado del Pedido</label>
                                    <select value={formData.estado} onChange={(e) => setFormData({ ...formData, estado: Number(e.target.value) })} className={inputCls} required>
                                        <option value={0}>Pendiente</option>
                                        <option value={1}>Confirmado</option>
                                        <option value={2}>Pagado</option>
                                        <option value={3}>Enviado</option>
                                        <option value={4}>Cancelado</option>
                                    </select>
                                </div>
                                <div>
                                    <label className={labelCls}>Dirección de Entrega</label>
                                    <textarea value={formData.direccionEntrega || ''} onChange={(e) => setFormData({ ...formData, direccionEntrega: e.target.value })} className={`${inputCls} resize-none`} rows="2" placeholder="Dirección completa..." required />
                                </div>
                                <div>
                                    <label className={labelCls}>Referencia Wompi</label>
                                    <input type="text" value={formData.referenciaWompi || ''} onChange={(e) => setFormData({ ...formData, referenciaWompi: e.target.value })} className={inputCls} placeholder="Opcional..." />
                                </div>
                            </form>
                        </div>
                        <div className="p-6 border-t border-neutral-100 dark:border-dark-border bg-neutral-50 dark:bg-dark-input rounded-b-2xl flex gap-3">
                            <button type="button" onClick={handleCloseModal} className="flex-1 bg-white dark:bg-dark-surface border border-neutral-200 dark:border-dark-border text-neutral-700 dark:text-neutral-300 py-2.5 rounded-xl font-semibold shadow-sm hover:bg-neutral-50 dark:hover:bg-dark-elevated transition-colors">Cancelar</button>
                            <button type="submit" form="pedidoForm" className="flex-1 bg-primary-600 dark:bg-cyan-600 text-white py-2.5 rounded-xl font-semibold shadow-sm hover:bg-primary-700 dark:hover:bg-cyan-700 transition-all">Guardar</button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default Pedidos;
