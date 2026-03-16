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
            case 1: return 'bg-yellow-100 text-yellow-800'; // Pendiente
            case 2: return 'bg-blue-100 text-blue-800'; // Confirmado
            case 3: return 'bg-green-100 text-green-800'; // Pagado            
            default: return 'bg-gray-100 text-gray-800';
        }
    };

    const getRemitenteText = (remitenteEnum) => {
        switch (remitenteEnum) {
            case 1: return 'Cliente';
            case 2: return 'Soporte';
            case 3: return 'Sistema';
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
        msg.contenido.toLowerCase().includes(searchTerm.toLowerCase()) ||
        msg.remitente.toLowerCase().includes(searchTerm.toLowerCase())
    );

    if (loading) {
        return <div className="text-center py-12">Cargando mensajes...</div>;
    }

    return (
        <div>
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-8">
                <div>
                    <h1 className="text-3xl font-bold text-gray-800">Mensajes</h1>
                    <p className="text-gray-600 mt-2">Bandeja de entrada y salida del bot</p>
                </div>

                <div className="flex flex-col sm:flex-row w-full md:w-auto gap-4">
                    <div className="relative flex-1 sm:w-64">
                        <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400">🔍</span>
                        <input
                            type="text"
                            placeholder="Buscar mensajes..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pl-10 pr-4 py-2 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 transition-all"
                        />
                    </div>
                    <button
                        onClick={() => handleOpenModal()}
                        className="bg-primary-600 text-white p-3 md:px-6 md:py-3 rounded-lg font-medium hover:bg-primary-700 transition-colors flex items-center justify-center whitespace-nowrap"
                        title="Nuevo Mensaje"
                    >
                        <span className="text-xl md:mr-2">➕</span>
                        <span className="hidden md:inline">Nuevo Mensaje</span>
                    </button>
                </div>
            </div>

            <div className="bg-white rounded-xl shadow-md overflow-x-auto">
                <table className="w-full">
                    <thead className="bg-gray-50 border-b">
                        <tr>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Conversación ID</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Remitente</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Contenido</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Acciones</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-200">
                        {filteredMensajes.map((msg) => (
                            <tr key={msg.id} className="hover:bg-gray-50">
                                <td className="px-6 py-4 font-medium text-gray-900">{msg.conversacionId}</td>
                                <td className="px-6 py-4">
                                    <span className={`px-3 py-1 rounded-full text-sm font-medium ${getRemitenteColor(msg.remitente)}`}>
                                        {getRemitenteText(msg.remitente)}
                                    </span>
                                </td>
                                <td className="px-6 py-4 text-gray-900 truncate max-w-xs">{msg.contenido}</td>
                                <td className="px-6 py-4">
                                    <div className="flex space-x-2">
                                        <button
                                            onClick={() => handleOpenModal(msg)}
                                            className="p-2 text-primary-600 hover:bg-primary-50 rounded-lg transition-colors"
                                            title="Editar"
                                        >
                                            ✏️
                                        </button>
                                        <button
                                            onClick={() => handleDeletePermanently(msg.id)}
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
                            {editingMensaje ? 'Editar Mensaje' : 'Nuevo Mensaje'}
                        </h2>

                        <form onSubmit={handleSubmit} className="space-y-4">
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Conversación ID</label>
                                <input type="number" value={formData.conversacionId} onChange={(e) => setFormData({ ...formData, conversacionId: e.target.value })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500" required />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Remitente</label>
                                <select value={formData.remitente} onChange={(e) => setFormData({ ...formData, remitente: Number(e.target.value) })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500" required>

                                    <option value={1}>Usuario</option>
                                    <option value={2}>Soporte</option>
                                    <option value={3}>Sistema</option>

                                </select>
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Contenido</label>
                                <textarea value={formData.contenido} onChange={(e) => setFormData({ ...formData, contenido: e.target.value })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500" rows="4" required />
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

export default Mensajes;
