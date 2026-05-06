import { useState, useEffect } from 'react';
import api from '../api/client';
import { SearchIcon, AddNewIcon, EditIcon, DeleteIcon, ConversationsIcon, CloseIcon } from '../components/Icons';
import Pagination from '../components/Pagination';

const ITEMS_PER_PAGE = 10;

function Conversaciones() {
    const [conversaciones, setConversaciones] = useState([]);
    const [totalCount, setTotalCount] = useState(0);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [currentPage, setCurrentPage] = useState(1);
    const [showModal, setShowModal] = useState(false);
    const [editingConversacion, setEditingConversacion] = useState(null);
    const [formData, setFormData] = useState({ clienteId: 0, activa: false });

    useEffect(() => { 
        fetchConversaciones(); 
    }, [currentPage, searchTerm]);

    const fetchConversaciones = async () => {
        try {
            const response = await api.get('/admin/inventario/conversaciones/paged', {
                params: {
                    page: currentPage,
                    pageSize: ITEMS_PER_PAGE,
                    search: searchTerm
                }
            });
            setConversaciones(response.data.items);
            setTotalCount(response.data.totalCount);
        } catch (error) { 
            console.error('Error fetching conversaciones:', error); 
        } finally { setLoading(false); }
    };

    const handleOpenModal = (conversacion = null) => {
        if (conversacion) { setEditingConversacion(conversacion); setFormData(conversacion); }
        else { setEditingConversacion(null); setFormData({ clienteId: 0, activa: true }); }
        setShowModal(true);
    };

    const handleCloseModal = () => { setShowModal(false); setEditingConversacion(null); };

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            const dataToSave = {
                id: editingConversacion ? Number(editingConversacion.id) : 0,
                clienteId: Number(formData.clienteId),
                activa: formData.activa === 'true' || formData.activa === true
            };
            if (editingConversacion) { await api.put(`/admin/inventario/conversaciones/${editingConversacion.id}`, dataToSave); alert('¡Conversación actualizada!'); }
            else { await api.post('/admin/inventario/conversaciones', dataToSave); alert('¡Conversación agregada!'); }
            fetchConversaciones(); handleCloseModal();
        } catch (error) { console.error('Error:', error); alert('Error al guardar/modificar la conversación'); }
    };

    const handleDeletePermanently = async (id) => {
        if (!confirm('¿Estás seguro de eliminar esta conversación?')) return;
        try { await api.delete(`/admin/inventario/conversaciones/${id}`); fetchConversaciones(); }
        catch (error) { console.error('Error deleting conversacion:', error); }
    };

    const totalPages = Math.max(1, Math.ceil(totalCount / ITEMS_PER_PAGE));
    const handleSearch = (v) => { setSearchTerm(v); setCurrentPage(1); };

    if (loading) return (
        <div className="flex justify-center items-center h-64">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600 dark:border-cyan-500"></div>
        </div>
    );

    return (
        <div className="animate-fade-in">
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-8">
                <div>
                    <h1 className="text-3xl font-bold text-neutral-900 dark:text-neutral-100 tracking-tight">Conversaciones</h1>
                    <p className="text-neutral-500 dark:text-neutral-400 mt-2">Gestiona el historial de chat de los clientes</p>
                </div>
                <div className="flex flex-col sm:flex-row w-full md:w-auto gap-4">
                    <div className="relative flex-1 sm:w-72">
                        <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-neutral-400" />
                        <input type="text" placeholder="Buscar por Cliente ID..." value={searchTerm} onChange={(e) => handleSearch(e.target.value)}
                            className="w-full pl-10 pr-4 py-2.5 bg-white dark:bg-dark-input border border-neutral-200 dark:border-dark-border text-neutral-900 dark:text-neutral-100 placeholder:text-neutral-400 dark:placeholder:text-neutral-600 rounded-xl focus:outline-none focus:border-primary-500 dark:focus:border-cyan-500 focus:ring-4 focus:ring-primary-500/10 dark:focus:ring-cyan-500/10 transition-all shadow-sm dark:shadow-none" />
                    </div>
                    <button onClick={() => handleOpenModal()}
                        className="bg-primary-600 dark:bg-cyan-600 text-white px-5 py-2.5 rounded-xl font-semibold shadow-sm shadow-primary-500/30 dark:shadow-cyan-500/20 hover:bg-primary-700 dark:hover:bg-cyan-700 hover:shadow-md transition-all flex items-center justify-center whitespace-nowrap gap-2">
                        <AddNewIcon className="w-5 h-5" /><span>Nueva Conversación</span>
                    </button>
                </div>
            </div>

            <div className="bg-white dark:bg-dark-surface rounded-2xl shadow-sm dark:shadow-none border border-neutral-200 dark:border-dark-border overflow-hidden">
                <div className="overflow-x-auto">
                    <table className="w-full min-w-[700px] text-left border-collapse">
                        <thead>
                            <tr className="bg-neutral-50/50 dark:bg-dark-input/50 border-b border-neutral-200 dark:border-dark-border">
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 dark:text-neutral-500 uppercase tracking-wider">Cliente ID</th>
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 dark:text-neutral-500 uppercase tracking-wider">Estado</th>
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 dark:text-neutral-500 uppercase tracking-wider">Acciones</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-neutral-100 dark:divide-dark-border">
                            {conversaciones.map((conv) => (
                                <tr key={conv.id} className="hover:bg-neutral-50/50 dark:hover:bg-dark-input/50 transition-colors">
                                    <td className="px-6 py-4">
                                        <div className="font-bold text-neutral-900 dark:text-neutral-100 border border-neutral-200 dark:border-dark-border bg-white dark:bg-dark-input rounded-md px-2 py-1 inline-block text-sm">
                                            {conv.clienteId}
                                        </div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="flex items-center gap-1.5">
                                            <div className={`w-2 h-2 rounded-full ${conv.activa ? 'bg-emerald-500 shadow-[0_0_8px_rgba(16,185,129,0.5)]' : 'bg-neutral-400'}`}></div>
                                            <span className={`text-sm font-semibold ${conv.activa ? 'text-emerald-700 dark:text-emerald-400' : 'text-neutral-600 dark:text-neutral-400'}`}>
                                                {conv.activa ? 'Activa' : 'Cerrada'}
                                            </span>
                                        </div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="flex items-center gap-1.5">
                                            <button onClick={() => handleOpenModal(conv)} title="Editar"
                                                className="p-1.5 text-primary-600 dark:text-cyan-400 hover:bg-primary-50 dark:hover:bg-cyan-900/20 rounded-lg transition-colors border border-transparent hover:border-primary-100 dark:hover:border-cyan-800/30">
                                                <EditIcon className="w-4 h-4" />
                                            </button>
                                            <button onClick={() => handleDeletePermanently(conv.id)} title="Eliminar"
                                                className="p-1.5 text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-lg transition-colors border border-transparent hover:border-red-100 dark:hover:border-red-800/30">
                                                <DeleteIcon className="w-4 h-4" />
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                    {conversaciones.length === 0 && (
                        <div className="flex flex-col justify-center items-center py-16 text-neutral-500 dark:text-neutral-500">
                            <ConversationsIcon className="w-12 h-12 mb-4 opacity-40" />
                            <span className="font-medium">No se encontraron conversaciones.</span>
                        </div>
                    )}
                </div>
                <Pagination
                    currentPage={currentPage}
                    totalPages={totalPages}
                    totalItems={totalCount}
                    itemsPerPage={ITEMS_PER_PAGE}
                    onChange={setCurrentPage}
                />
            </div>

            {showModal && (
                <div className="fixed inset-0 bg-neutral-900/40 dark:bg-black/60 backdrop-blur-sm flex items-center justify-center z-50 p-4 animate-fade-in">
                    <div className="bg-white dark:bg-dark-surface rounded-2xl shadow-xl dark:shadow-black/40 w-full max-w-md max-h-[90vh] flex flex-col overflow-hidden border border-neutral-100 dark:border-dark-border">
                        <div className="p-6 border-b border-neutral-100 dark:border-dark-border flex justify-between items-center">
                            <h2 className="text-xl text-neutral-900 dark:text-neutral-100 font-bold tracking-tight">
                                {editingConversacion ? 'Editar Conversación' : 'Nueva Conversación'}
                            </h2>
                            <button onClick={handleCloseModal} className="text-neutral-400 hover:text-neutral-600 dark:hover:text-neutral-300 bg-neutral-50 dark:bg-dark-input hover:bg-neutral-100 dark:hover:bg-dark-border rounded-lg p-1.5 transition-colors"><CloseIcon className="w-4 h-4" /></button>
                        </div>
                        <div className="p-6 overflow-y-auto">
                            <form id="conversacionForm" onSubmit={handleSubmit} className="space-y-5">
                                <div>
                                    <label className="block text-sm font-semibold text-neutral-700 dark:text-neutral-300 mb-1.5">Cliente ID <span className="text-red-500">*</span></label>
                                    <input type="number" min="1" step="1" value={formData.clienteId}
                                        onChange={(e) => setFormData({ ...formData, clienteId: e.target.value })}
                                        className="w-full px-4 py-2.5 bg-neutral-50 dark:bg-dark-input border border-neutral-200 dark:border-dark-border text-neutral-900 dark:text-neutral-100 rounded-xl focus:bg-white dark:focus:bg-dark-surface focus:outline-none focus:border-primary-500 dark:focus:border-cyan-500 focus:ring-4 focus:ring-primary-500/10 dark:focus:ring-cyan-500/10 transition-all font-mono" required />
                                </div>
                                <div className="pt-2">
                                    <label className="relative inline-flex items-center cursor-pointer group">
                                        <input type="checkbox" className="sr-only peer"
                                            checked={formData.activa === 'true' || formData.activa === true}
                                            onChange={(e) => setFormData({ ...formData, activa: e.target.checked })} />
                                        <div className="w-11 h-6 bg-neutral-200 dark:bg-dark-border peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-primary-500/20 dark:peer-focus:ring-cyan-500/20 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-emerald-500 transition-colors"></div>
                                        <span className="ml-3 text-sm font-semibold text-neutral-700 dark:text-neutral-300">Conversación Activa</span>
                                    </label>
                                </div>
                            </form>
                        </div>
                        <div className="p-6 border-t border-neutral-100 dark:border-dark-border bg-neutral-50 dark:bg-dark-input rounded-b-2xl flex gap-3">
                            <button type="button" onClick={handleCloseModal}
                                className="flex-1 bg-white dark:bg-dark-surface border border-neutral-200 dark:border-dark-border text-neutral-700 dark:text-neutral-300 py-2.5 rounded-xl font-semibold shadow-sm hover:bg-neutral-50 dark:hover:bg-dark-elevated transition-colors">Cancelar</button>
                            <button type="submit" form="conversacionForm"
                                className="flex-1 bg-primary-600 dark:bg-cyan-600 text-white py-2.5 rounded-xl font-semibold shadow-sm hover:bg-primary-700 dark:hover:bg-cyan-700 transition-all">Guardar</button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default Conversaciones;
