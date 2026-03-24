import { useState, useEffect } from 'react';
import api from '../api/client';

function Mensajes() {
    const [mensajes, setMensajes] = useState([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [showModal, setShowModal] = useState(false);
    const [editingMensaje, setEditingMensaje] = useState(null);
    const [formData, setFormData] = useState({
        conversacionId: 0,
        contenido: '',
        remitente: 0 // 1 - "Cliente" , 2 - "Soporte", 3 - "Sistema"
    });

    useEffect(() => {
        fetchMensajes();
    }, []);

    const fetchMensajes = async () => {
        try {
            const response = await api.get('/admin/inventario/mensajes'); // Endpoint asumido
            setMensajes(response.data);
        } catch (error) {
            console.error('Error fetching mensajes:', error);
        } finally {
            setLoading(false);
        }
    };

    const handleOpenModal = (mensaje = null) => {
        if (mensaje) {
            setEditingMensaje(mensaje);
            setFormData(mensaje);
        } else {
            setEditingMensaje(null);
            setFormData({
                conversacionId: 0,
                contenido: '',
                remitente: 0
            });
        }
        setShowModal(true);
    };

    const handleCloseModal = () => {
        setShowModal(false);
        setEditingMensaje(null);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        try {
            const dataToSave = {
                id: editingMensaje ? Number(editingMensaje.id) : 0,
                conversacionId: Number(formData.conversacionId),
                contenido: formData.contenido,
                remitente: Number(formData.remitente)
            };

            if (editingMensaje) {
                await api.put(`/admin/inventario/mensajes/${editingMensaje.id}`, dataToSave);
                alert("¡Mensaje actualizado!");
            } else {
                await api.post('/admin/inventario/mensajes', dataToSave);
                alert("¡Mensaje agregado!");
            }

            fetchMensajes();
            handleCloseModal();

        } catch (error) {
            console.error('Error:', error);
            alert('Error al guardar/modificar el mensaje');
        }
    };

    const getRemitenteColor = (remitenteEnum) => {
        switch (remitenteEnum) {
            case 1: return 'bg-amber-100 text-amber-700 border-amber-200';
            case 2: return 'bg-primary-100 text-primary-700 border-primary-200';
            case 3: return 'bg-emerald-100 text-emerald-700 border-emerald-200';
            default: return 'bg-neutral-100 text-neutral-600 border-neutral-200';
        }
    };

    const getRemitenteIcon = (remitenteEnum) => {
        switch (remitenteEnum) {
            case 1: return '👤';
            case 2: return '🎧';
            case 3: return '🤖';
            default: return '❓';
        }
    }

    const getRemitenteText = (remitenteEnum) => {
        switch (remitenteEnum) {
            case 1: return 'Cliente';
            case 2: return 'Soporte';
            case 3: return 'Sistema';
            default: return 'Desconocido';
        }
    };

    const handleDeletePermanently = async (id) => {
        if (!confirm('¿Estás seguro de eliminar este mensaje?')) return;

        try {
            await api.delete(`/admin/inventario/mensajes/${id}`);
            fetchMensajes();
        } catch (error) {
            console.error('Error deleting mensaje:', error);
        }
    }

    const filteredMensajes = mensajes.filter(msg =>
        (msg.contenido && msg.contenido.toLowerCase().includes(searchTerm.toLowerCase())) ||
        (msg.remitente && msg.remitente.toString().toLowerCase().includes(searchTerm.toLowerCase()))
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
                    <h1 className="text-3xl font-bold text-neutral-900 tracking-tight">Mensajes</h1>
                    <p className="text-neutral-500 mt-2">Bandeja de entrada y salida del bot</p>
                </div>

                <div className="flex flex-col sm:flex-row w-full md:w-auto gap-4">
                    <div className="relative flex-1 sm:w-72">
                        <span className="absolute left-3 top-1/2 -translate-y-1/2 text-neutral-400">🔍</span>
                        <input
                            type="text"
                            placeholder="Buscar mensajes..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pl-10 pr-4 py-2.5 bg-white border border-neutral-200 rounded-xl focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all shadow-sm"
                        />
                    </div>
                    <button
                        onClick={() => handleOpenModal()}
                        className="bg-primary-600 text-white px-5 py-2.5 rounded-xl font-semibold shadow-sm shadow-primary-500/30 hover:bg-primary-700 hover:shadow-md hover:shadow-primary-500/40 transition-all flex items-center justify-center whitespace-nowrap gap-2"
                        title="Nuevo Mensaje"
                    >
                        <span className="text-lg">➕</span>
                        <span>Nuevo Mensaje</span>
                    </button>
                </div>
            </div>

            <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 overflow-hidden">
                <div className="overflow-x-auto">
                    <table className="w-full text-left border-collapse">
                        <thead>
                            <tr className="bg-neutral-50/50 border-b border-neutral-200">
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 uppercase tracking-wider whitespace-nowrap">Conversación</th>
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 uppercase tracking-wider">Remitente</th>
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 uppercase tracking-wider w-1/2">Contenido</th>
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 uppercase tracking-wider text-right">Acciones</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-neutral-100">
                            {filteredMensajes.map((msg) => (
                                <tr key={msg.id} className="hover:bg-neutral-50/50 transition-colors">
                                    <td className="px-6 py-4">
                                        <div className="font-bold text-neutral-900 border border-neutral-200 bg-white rounded-md px-2 py-1 inline-block text-sm">
                                            #{msg.conversacionId}
                                        </div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <span className={`px-2.5 py-1 rounded-md text-xs font-bold border flex items-center gap-1.5 w-max ${getRemitenteColor(msg.remitente)}`}>
                                            <span>{getRemitenteIcon(msg.remitente)}</span>
                                            {getRemitenteText(msg.remitente)}
                                        </span>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="text-neutral-700 text-sm max-w-lg line-clamp-2 bg-neutral-50 p-2 rounded-lg border border-neutral-100">
                                            {msg.contenido}
                                        </div>
                                    </td>
                                    <td className="px-6 py-4 text-right">
                                        <div className="flex items-center justify-end gap-1.5">
                                            <button
                                                onClick={() => handleOpenModal(msg)}
                                                className="p-1.5 text-primary-600 hover:bg-primary-50 rounded-lg transition-colors border border-transparent hover:border-primary-100"
                                                title="Editar"
                                            >
                                                ✏️
                                            </button>
                                            <button
                                                onClick={() => handleDeletePermanently(msg.id)}
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

                    {filteredMensajes.length === 0 && (
                        <div className="flex flex-col justify-center items-center py-16 text-neutral-500">
                            <span className="text-5xl mb-4">📨</span>
                            <span className="font-medium">No se encontraron mensajes.</span>
                        </div>
                    )}
                </div>
            </div>

            {/* Modal */}
            {showModal && (
                <div className="fixed inset-0 bg-neutral-900/40 backdrop-blur-sm flex items-center justify-center z-50 p-4 animate-fade-in">
                    <div className="bg-white rounded-2xl shadow-xl w-full max-w-lg max-h-[90vh] flex flex-col overflow-hidden">
                        <div className="p-6 border-b border-neutral-100 flex justify-between items-center">
                            <h2 className="text-xl text-neutral-900 font-bold tracking-tight">
                                {editingMensaje ? 'Editar Mensaje' : 'Nuevo Mensaje'}
                            </h2>
                            <button onClick={handleCloseModal} className="text-neutral-400 hover:text-neutral-600 bg-neutral-50 hover:bg-neutral-100 rounded-lg p-1.5 transition-colors">
                                ✕
                            </button>
                        </div>

                        <div className="p-6 overflow-y-auto">
                            <form id="mensajeForm" onSubmit={handleSubmit} className="space-y-5">
                                <div className="grid grid-cols-2 gap-4">
                                    <div>
                                        <label className="block text-sm font-semibold text-neutral-700 mb-1.5">Conversación ID <span className="text-red-500">*</span></label>
                                        <input
                                            type="number"
                                            value={formData.conversacionId}
                                            onChange={(e) => setFormData({ ...formData, conversacionId: e.target.value })}
                                            className="w-full px-4 py-2.5 bg-neutral-50 border border-neutral-200 rounded-xl focus:bg-white focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all font-mono"
                                            required
                                        />
                                    </div>
                                    <div>
                                        <label className="block text-sm font-semibold text-neutral-700 mb-1.5">Remitente <span className="text-red-500">*</span></label>
                                        <select
                                            value={formData.remitente}
                                            onChange={(e) => setFormData({ ...formData, remitente: Number(e.target.value) })}
                                            className="w-full px-4 py-2.5 bg-neutral-50 border border-neutral-200 rounded-xl focus:bg-white focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all font-medium"
                                            required
                                        >
                                            <option value={1}>👤 Cliente</option>
                                            <option value={2}>🎧 Soporte</option>
                                            <option value={3}>🤖 Sistema</option>
                                        </select>
                                    </div>
                                </div>

                                <div>
                                    <label className="block text-sm font-semibold text-neutral-700 mb-1.5">Contenido <span className="text-red-500">*</span></label>
                                    <textarea
                                        value={formData.contenido}
                                        onChange={(e) => setFormData({ ...formData, contenido: e.target.value })}
                                        className="w-full px-4 py-3 bg-neutral-50 border border-neutral-200 rounded-xl focus:bg-white focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all resize-y min-h-[120px]"
                                        placeholder="Escribe el mensaje aquí..."
                                        rows="4"
                                        required
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
                                form="mensajeForm"
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

export default Mensajes;
