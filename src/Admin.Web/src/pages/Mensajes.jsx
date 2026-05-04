import { useState, useEffect } from 'react';
import api from '../api/client';
import { SearchIcon, AddNewIcon, EditIcon, DeleteIcon, UserIcon, HeadsetIcon, BotIcon, MessagesIcon, CloseIcon } from '../components/Icons';

function Mensajes() {
    const [mensajes, setMensajes] = useState([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [showModal, setShowModal] = useState(false);
    const [editingMensaje, setEditingMensaje] = useState(null);
    const [formData, setFormData] = useState({ conversacionId: 0, contenido: '', remitente: 1 });

    useEffect(() => { fetchMensajes(); }, []);

    const fetchMensajes = async () => {
        try {
            const response = await api.get('/admin/inventario/mensajes');
            setMensajes(response.data);
        } catch (error) { console.error('Error fetching mensajes:', error); }
        finally { setLoading(false); }
    };

    const handleOpenModal = (mensaje = null) => {
        if (mensaje) { setEditingMensaje(mensaje); setFormData(mensaje); }
        else { setEditingMensaje(null); setFormData({ conversacionId: 0, contenido: '', remitente: 1 }); }
        setShowModal(true);
    };

    const handleCloseModal = () => { setShowModal(false); setEditingMensaje(null); };

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            const dataToSave = { id: editingMensaje ? Number(editingMensaje.id) : 0, conversacionId: Number(formData.conversacionId), contenido: formData.contenido, remitente: Number(formData.remitente) };
            if (editingMensaje) { await api.put(`/admin/inventario/mensajes/${editingMensaje.id}`, dataToSave); alert('¡Mensaje actualizado!'); }
            else { await api.post('/admin/inventario/mensajes', dataToSave); alert('¡Mensaje agregado!'); }
            fetchMensajes(); handleCloseModal();
        } catch (error) { console.error('Error:', error); alert('Error al guardar/modificar el mensaje'); }
    };

    const handleDeletePermanently = async (id) => {
        if (!confirm('¿Estás seguro de eliminar este mensaje?')) return;
        try { await api.delete(`/admin/inventario/mensajes/${id}`); fetchMensajes(); }
        catch (error) { console.error('Error deleting mensaje:', error); }
    };

    const getRemitenteColor = (r) => ({ 1: 'bg-amber-100 dark:bg-amber-900/20 text-amber-700 dark:text-amber-400 border-amber-200 dark:border-amber-800/30', 2: 'bg-primary-100 dark:bg-cyan-900/20 text-primary-700 dark:text-cyan-400 border-primary-200 dark:border-cyan-800/30', 3: 'bg-emerald-100 dark:bg-emerald-900/20 text-emerald-700 dark:text-emerald-400 border-emerald-200 dark:border-emerald-800/30' }[r] || 'bg-neutral-100 dark:bg-dark-input text-neutral-600 dark:text-neutral-400 border-neutral-200 dark:border-dark-border');
    const getRemitenteIcon = (r) => ({ 1: <UserIcon className="w-3.5 h-3.5" />, 2: <HeadsetIcon className="w-3.5 h-3.5" />, 3: <BotIcon className="w-3.5 h-3.5" /> }[r] || null);
    const getRemitenteText = (r) => ({ 1: 'Cliente', 2: 'Soporte', 3: 'Sistema' }[r] || 'Desconocido');

    const filteredMensajes = mensajes.filter(m =>
        (m.contenido && m.contenido.toLowerCase().includes(searchTerm.toLowerCase())) ||
        (m.remitente && m.remitente.toString().includes(searchTerm))
    );

    if (loading) return (
        <div className="flex justify-center items-center h-64">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600 dark:border-cyan-500"></div>
        </div>
    );

    return (
        <div className="animate-fade-in">
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-8">
                <div>
                    <h1 className="text-3xl font-bold text-neutral-900 dark:text-neutral-100 tracking-tight">Mensajes</h1>
                    <p className="text-neutral-500 dark:text-neutral-400 mt-2">Bandeja de entrada y salida del bot</p>
                </div>
                <div className="flex flex-col sm:flex-row w-full md:w-auto gap-4">
                    <div className="relative flex-1 sm:w-72">
                        <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-neutral-400" />
                        <input type="text" placeholder="Buscar mensajes..." value={searchTerm} onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pl-10 pr-4 py-2.5 bg-white dark:bg-dark-input border border-neutral-200 dark:border-dark-border text-neutral-900 dark:text-neutral-100 placeholder:text-neutral-400 dark:placeholder:text-neutral-600 rounded-xl focus:outline-none focus:border-primary-500 dark:focus:border-cyan-500 focus:ring-4 focus:ring-primary-500/10 dark:focus:ring-cyan-500/10 transition-all shadow-sm dark:shadow-none" />
                    </div>
                    <button onClick={() => handleOpenModal()}
                        className="bg-primary-600 dark:bg-cyan-600 text-white px-5 py-2.5 rounded-xl font-semibold shadow-sm shadow-primary-500/30 dark:shadow-cyan-500/20 hover:bg-primary-700 dark:hover:bg-cyan-700 hover:shadow-md transition-all flex items-center justify-center whitespace-nowrap gap-2">
                        <AddNewIcon className="w-5 h-5" /><span>Nuevo Mensaje</span>
                    </button>
                </div>
            </div>

            <div className="bg-white dark:bg-dark-surface rounded-2xl shadow-sm dark:shadow-none border border-neutral-200 dark:border-dark-border overflow-hidden">
                <div className="overflow-x-auto">
                    <table className="w-full min-w-[980px] text-left border-collapse">
                        <thead>
                            <tr className="bg-neutral-50/50 dark:bg-dark-input/50 border-b border-neutral-200 dark:border-dark-border">
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 dark:text-neutral-500 uppercase tracking-wider whitespace-nowrap">Conversación</th>
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 dark:text-neutral-500 uppercase tracking-wider">Remitente</th>
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 dark:text-neutral-500 uppercase tracking-wider w-1/2">Contenido</th>
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 dark:text-neutral-500 uppercase tracking-wider text-right">Acciones</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-neutral-100 dark:divide-dark-border">
                            {filteredMensajes.map((msg) => (
                                <tr key={msg.id} className="hover:bg-neutral-50/50 dark:hover:bg-dark-input/50 transition-colors">
                                    <td className="px-6 py-4">
                                        <div className="font-bold text-neutral-900 dark:text-neutral-100 border border-neutral-200 dark:border-dark-border bg-white dark:bg-dark-input rounded-md px-2 py-1 inline-block text-sm">#{msg.conversacionId}</div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <span className={`px-2.5 py-1 rounded-md text-xs font-bold border flex items-center gap-1.5 w-max ${getRemitenteColor(msg.remitente)}`}>
                                            <span>{getRemitenteIcon(msg.remitente)}</span>{getRemitenteText(msg.remitente)}
                                        </span>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="text-neutral-700 dark:text-neutral-300 text-sm max-w-lg line-clamp-2 bg-neutral-50 dark:bg-dark-input p-2 rounded-lg border border-neutral-100 dark:border-dark-border">{msg.contenido}</div>
                                    </td>
                                    <td className="px-6 py-4 text-right">
                                        <div className="flex items-center justify-end gap-1.5">
                                            <button onClick={() => handleOpenModal(msg)} title="Editar"
                                                className="p-1.5 text-primary-600 dark:text-cyan-400 hover:bg-primary-50 dark:hover:bg-cyan-900/20 rounded-lg transition-colors border border-transparent hover:border-primary-100 dark:hover:border-cyan-800/30">
                                                <EditIcon className="w-4 h-4" />
                                            </button>
                                            <button onClick={() => handleDeletePermanently(msg.id)} title="Eliminar"
                                                className="p-1.5 text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-lg transition-colors border border-transparent hover:border-red-100 dark:hover:border-red-800/30">
                                                <DeleteIcon className="w-4 h-4" />
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                    {filteredMensajes.length === 0 && (
                        <div className="flex flex-col justify-center items-center py-16 text-neutral-500 dark:text-neutral-500">
                            <MessagesIcon className="w-12 h-12 mb-4 opacity-40" />
                            <span className="font-medium">No se encontraron mensajes.</span>
                        </div>
                    )}
                </div>
            </div>

            {showModal && (
                <div className="fixed inset-0 bg-neutral-900/40 dark:bg-black/60 backdrop-blur-sm flex items-center justify-center z-50 p-4 animate-fade-in">
                    <div className="bg-white dark:bg-dark-surface rounded-2xl shadow-xl dark:shadow-black/40 w-full max-w-lg max-h-[90vh] flex flex-col overflow-hidden border border-neutral-100 dark:border-dark-border">
                        <div className="p-6 border-b border-neutral-100 dark:border-dark-border flex justify-between items-center">
                            <h2 className="text-xl text-neutral-900 dark:text-neutral-100 font-bold tracking-tight">{editingMensaje ? 'Editar Mensaje' : 'Nuevo Mensaje'}</h2>
                            <button onClick={handleCloseModal} className="text-neutral-400 hover:text-neutral-600 dark:hover:text-neutral-300 bg-neutral-50 dark:bg-dark-input hover:bg-neutral-100 dark:hover:bg-dark-border rounded-lg p-1.5 transition-colors"><CloseIcon className="w-4 h-4" /></button>
                        </div>
                        <div className="p-6 overflow-y-auto">
                            <form id="mensajeForm" onSubmit={handleSubmit} className="space-y-5">
                                <div className="grid grid-cols-2 gap-4">
                                    <div>
                                        <label className="block text-sm font-semibold text-neutral-700 dark:text-neutral-300 mb-1.5">Conversación ID <span className="text-red-500">*</span></label>
                                        <input type="number" value={formData.conversacionId} onChange={(e) => setFormData({ ...formData, conversacionId: e.target.value })}
                                            className="w-full px-4 py-2.5 bg-neutral-50 dark:bg-dark-input border border-neutral-200 dark:border-dark-border text-neutral-900 dark:text-neutral-100 rounded-xl focus:bg-white dark:focus:bg-dark-surface focus:outline-none focus:border-primary-500 dark:focus:border-cyan-500 focus:ring-4 focus:ring-primary-500/10 dark:focus:ring-cyan-500/10 transition-all font-mono" required />
                                    </div>
                                    <div>
                                        <label className="block text-sm font-semibold text-neutral-700 dark:text-neutral-300 mb-1.5">Remitente <span className="text-red-500">*</span></label>
                                        <select value={formData.remitente} onChange={(e) => setFormData({ ...formData, remitente: Number(e.target.value) })}
                                            className="w-full px-4 py-2.5 bg-neutral-50 dark:bg-dark-input border border-neutral-200 dark:border-dark-border text-neutral-900 dark:text-neutral-100 rounded-xl focus:bg-white dark:focus:bg-dark-surface focus:outline-none focus:border-primary-500 dark:focus:border-cyan-500 focus:ring-4 focus:ring-primary-500/10 dark:focus:ring-cyan-500/10 transition-all font-medium" required>
                                            <option value={1}>Cliente</option>
                                            <option value={2}>Soporte</option>
                                            <option value={3}>Sistema</option>
                                        </select>
                                    </div>
                                </div>
                                <div>
                                    <label className="block text-sm font-semibold text-neutral-700 dark:text-neutral-300 mb-1.5">Contenido <span className="text-red-500">*</span></label>
                                    <textarea value={formData.contenido} onChange={(e) => setFormData({ ...formData, contenido: e.target.value })}
                                        className="w-full px-4 py-3 bg-neutral-50 dark:bg-dark-input border border-neutral-200 dark:border-dark-border text-neutral-900 dark:text-neutral-100 rounded-xl focus:bg-white dark:focus:bg-dark-surface focus:outline-none focus:border-primary-500 dark:focus:border-cyan-500 focus:ring-4 focus:ring-primary-500/10 dark:focus:ring-cyan-500/10 transition-all resize-y min-h-[120px]"
                                        placeholder="Escribe el mensaje aquí..." rows="4" required />
                                </div>
                            </form>
                        </div>
                        <div className="p-6 border-t border-neutral-100 dark:border-dark-border bg-neutral-50 dark:bg-dark-input rounded-b-2xl flex gap-3">
                            <button type="button" onClick={handleCloseModal}
                                className="flex-1 bg-white dark:bg-dark-surface border border-neutral-200 dark:border-dark-border text-neutral-700 dark:text-neutral-300 py-2.5 rounded-xl font-semibold shadow-sm hover:bg-neutral-50 dark:hover:bg-dark-elevated transition-colors">Cancelar</button>
                            <button type="submit" form="mensajeForm"
                                className="flex-1 bg-primary-600 dark:bg-cyan-600 text-white py-2.5 rounded-xl font-semibold shadow-sm hover:bg-primary-700 dark:hover:bg-cyan-700 transition-all">Guardar</button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default Mensajes;
