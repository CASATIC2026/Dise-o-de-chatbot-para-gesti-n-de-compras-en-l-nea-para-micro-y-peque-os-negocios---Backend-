import { useState, useEffect } from 'react';
import api from '../api/client';

function Conversaciones() {
    const [conversaciones, setConversaciones] = useState([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [showModal, setShowModal] = useState(false);
    const [editingConversacion, setEditingConversacion] = useState(null);
    const [formData, setFormData] = useState({
        clienteId: 0,
        activa: false // 0: Activa, 1: Cerrada, etc.
    });

    useEffect(() => {
        fetchConversaciones();
    }, []);

    const fetchConversaciones = async () => {
        try {
            const response = await api.get('/admin/inventario/conversaciones'); // Endpoint asumido
            setConversaciones(response.data);
        } catch (error) {
            console.error('Error fetching conversaciones:', error);
        } finally {
            setLoading(false);
        }
    };

    const handleOpenModal = (conversacion = null) => {
        if (conversacion) {
            setEditingConversacion(conversacion);
            setFormData(conversacion);
        } else {
            setEditingConversacion(null);
            setFormData({
                clienteId: 0,
                activa: true
            });
        }
        setShowModal(true);
    };

    const handleCloseModal = () => {
        setShowModal(false);
        setEditingConversacion(null);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        try {
            const dataToSave = {

                id: editingConversacion ? Number(editingConversacion.id) : 0,
                clienteId: Number(formData.clienteId),
                activa: formData.activa === "true" || formData.activa === true ? true : false
            };

            if (editingConversacion) {
                await api.put(`/admin/inventario/conversaciones/${editingConversacion.id}`, dataToSave);
                alert("¡Conversación actualizada!");
            } else {
                await api.post('/admin/inventario/conversaciones', dataToSave);
                alert("¡Conversación agregada!");
            }
            console.log("Datos enviados:", dataToSave, formData);
            fetchConversaciones();
            handleCloseModal();

        } catch (error) {
            console.error('Error:', error);
            alert('Error al guardar/modificar la conversación');
        }
    };

    const handleDeletePermanently = async (id) => {
        if (!confirm('¿Estás seguro de eliminar esta conversación?')) return;

        try {
            await api.delete(`/admin/inventario/conversaciones/${id}`);
            fetchConversaciones();
        } catch (error) {
            console.error('Error deleting conversacion:', error);
        }
    }

    const getEstadoText = (estadoEnum) => {
        switch (estadoEnum) {
            case true: return 'Activa';
            case false: return 'Cerrada';
            default: return 'Desconocida';
        }
    };

    const filteredConversaciones = conversaciones.filter(conv =>
        conv.clienteId.toString().includes(searchTerm)
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
                    <h1 className="text-3xl font-bold text-neutral-900 tracking-tight">Conversaciones</h1>
                    <p className="text-neutral-500 mt-2">Gestiona el historial de chat de los clientes</p>
                </div>

                <div className="flex flex-col sm:flex-row w-full md:w-auto gap-4">
                    <div className="relative flex-1 sm:w-72">
                        <span className="absolute left-3 top-1/2 -translate-y-1/2 text-neutral-400">🔍</span>
                        <input
                            type="text"
                            placeholder="Buscar por Cliente ID..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pl-10 pr-4 py-2.5 bg-white border border-neutral-200 rounded-xl focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all shadow-sm"
                        />
                    </div>
                    <button
                        onClick={() => handleOpenModal()}
                        className="bg-primary-600 text-white px-5 py-2.5 rounded-xl font-semibold shadow-sm shadow-primary-500/30 hover:bg-primary-700 hover:shadow-md hover:shadow-primary-500/40 transition-all flex items-center justify-center whitespace-nowrap gap-2"
                        title="Nueva Conversación"
                    >
                        <span className="text-lg">➕</span>
                        <span>Nueva Conversación</span>
                    </button>
                </div>
            </div>

            <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 overflow-hidden">
                <div className="overflow-x-auto">
                    <table className="w-full text-left border-collapse">
                        <thead>
                            <tr className="bg-neutral-50/50 border-b border-neutral-200">
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 uppercase tracking-wider">Cliente ID</th>
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 uppercase tracking-wider">Estado</th>
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 uppercase tracking-wider">Acciones</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-neutral-100">
                            {filteredConversaciones.map((conv) => (
                                <tr key={conv.id} className="hover:bg-neutral-50/50 transition-colors">
                                    <td className="px-6 py-4">
                                        <div className="font-bold text-neutral-900 border border-neutral-200 bg-white rounded-md px-2 py-1 inline-block text-sm">
                                            {conv.clienteId}
                                        </div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="flex items-center gap-1.5">
                                            <div className={`w-2 h-2 rounded-full ${conv.activa ? 'bg-emerald-500 shadow-[0_0_8px_rgba(16,185,129,0.5)]' : 'bg-neutral-400'}`}></div>
                                            <span className={`text-sm font-semibold ${conv.activa ? 'text-emerald-700' : 'text-neutral-600'}`}>
                                                {getEstadoText(conv.activa)}
                                            </span>
                                        </div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="flex items-center gap-1.5">
                                            <button
                                                onClick={() => handleOpenModal(conv)}
                                                className="p-1.5 text-primary-600 hover:bg-primary-50 rounded-lg transition-colors border border-transparent hover:border-primary-100"
                                                title="Editar"
                                            >
                                                ✏️
                                            </button>
                                            <button
                                                onClick={() => handleDeletePermanently(conv.id)}
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

                    {filteredConversaciones.length === 0 && (
                        <div className="flex flex-col justify-center items-center py-16 text-neutral-500">
                            <span className="text-5xl mb-4">💬</span>
                            <span className="font-medium">No se encontraron conversaciones.</span>
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
                                {editingConversacion ? 'Editar Conversación' : 'Nueva Conversación'}
                            </h2>
                            <button onClick={handleCloseModal} className="text-neutral-400 hover:text-neutral-600 bg-neutral-50 hover:bg-neutral-100 rounded-lg p-1.5 transition-colors">
                                ✕
                            </button>
                        </div>

                        <div className="p-6 overflow-y-auto">
                            <form id="conversacionForm" onSubmit={handleSubmit} className="space-y-5">
                                <div>
                                    <label className="block text-sm font-semibold text-neutral-700 mb-1.5">Cliente ID <span className="text-red-500">*</span></label>
                                    <input
                                        type="number"
                                        min="1"
                                        step="1"
                                        value={formData.clienteId}
                                        onChange={(e) => setFormData({ ...formData, clienteId: e.target.value })}
                                        className="w-full px-4 py-2.5 bg-neutral-50 border border-neutral-200 rounded-xl focus:bg-white focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all font-mono"
                                        required
                                    />
                                </div>
                                <div className="pt-2">
                                    <label className="relative inline-flex items-center cursor-pointer group">
                                        <input
                                            type="checkbox"
                                            className="sr-only peer"
                                            checked={formData.activa === "true" || formData.activa === true}
                                            onChange={(e) => setFormData({ ...formData, activa: e.target.checked })}
                                        />
                                        <div className="w-11 h-6 bg-neutral-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-primary-500/20 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-emerald-500 transition-colors"></div>
                                        <span className="ml-3 text-sm font-semibold text-neutral-700">Conversación Activa</span>
                                    </label>
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
                                form="conversacionForm"
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

export default Conversaciones;
