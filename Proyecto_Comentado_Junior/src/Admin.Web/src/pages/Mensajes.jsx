import { useState, useEffect } from 'react';
import api from '../api/client';

function Mensajes() {
    const [mensajes, setMensajes] = useState([]);
    const [loading, setLoading] = useState(true);
    const [showModal, setShowModal] = useState(false);
    const [editingMensaje, setEditingMensaje] = useState(null);
    const [formData, setFormData] = useState({
        conversacionId: 0,
        contenido: '',
        role: '' // "User" o "Assistant"
    });

    useEffect(() => {
        fetchMensajes();
    }, []);

    const fetchMensajes = async () => {
        try {
            const response = await api.get('/admin/mensajes'); // Endpoint asumido
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
                role: 'User'
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
                role: formData.role
            };

            if (editingMensaje) {
                await api.put(`/admin/mensajes/${editingMensaje.id}`, dataToSave);
                alert("¡Mensaje actualizado!");
            } else {
                await api.post('/admin/mensajes', dataToSave);
                alert("¡Mensaje agregado!");
            }

            fetchMensajes();
            handleCloseModal();

        } catch (error) {
            console.error('Error:', error);
            alert('Error al guardar/modificar el mensaje');
        }
    };

    const handleDeletePermanently = async (id) => {
        if (!confirm('¿Estás seguro de eliminar este mensaje?')) return;

        try {
            await api.delete(`/admin/mensajes/${id}`);
            fetchMensajes();
        } catch (error) {
            console.error('Error deleting mensaje:', error);
        }
    }

    if (loading) {
        return <div className="text-center py-12">Cargando mensajes...</div>;
    }

    return (
        <div>
            <div className="flex justify-between items-center mb-8">
                <div>
                    <h1 className="text-3xl font-bold text-gray-800">Mensajes</h1>
                    <p className="text-gray-600 mt-2">Explora los mensajes individuales del sistema</p>
                </div>
                <button
                    onClick={() => handleOpenModal()}
                    className="bg-primary-600 text-white px-6 py-3 rounded-lg font-medium hover:bg-primary-700 transition-colors"
                >
                    + Nuevo Mensaje
                </button>
            </div>

            <div className="bg-white rounded-xl shadow-md overflow-hidden">
                <table className="w-full">
                    <thead className="bg-gray-50 border-b">
                        <tr>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Conversación ID</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Rol</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Contenido</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Acciones</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-200">
                        {mensajes.map((msg) => (
                            <tr key={msg.id} className="hover:bg-gray-50">
                                <td className="px-6 py-4 font-medium text-gray-900">{msg.conversacionId}</td>
                                <td className="px-6 py-4">
                                    <span className={`px-3 py-1 rounded-full text-sm font-medium ${msg.role === 'User' ? 'bg-blue-100 text-blue-800' : 'bg-purple-100 text-purple-800'}`}>
                                        {msg.role}
                                    </span>
                                </td>
                                <td className="px-6 py-4 text-gray-900 truncate max-w-xs">{msg.contenido}</td>
                                <td className="px-6 py-4">
                                    <div className="flex space-x-2">
                                        <button onClick={() => handleOpenModal(msg)} className="text-primary-600 hover:text-primary-800 font-medium">Editar</button>
                                        <button onClick={() => handleDeletePermanently(msg.id)} className="text-red-600 hover:text-red-800 font-medium">Eliminar</button>
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
                                <label className="block text-sm font-medium text-gray-700 mb-1">Rol</label>
                                <select value={formData.role} onChange={(e) => setFormData({ ...formData, role: e.target.value })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500" required>
                                    <option value="User">User</option>
                                    <option value="Assistant">Assistant</option>
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
