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
                activa: formData.activa === "true" ? true : false
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
        return <div className="text-center py-12">Cargando conversaciones...</div>;
    }

    return (
        <div>
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-8">
                <div>
                    <h1 className="text-3xl font-bold text-gray-800">Conversaciones</h1>
                    <p className="text-gray-600 mt-2">Gestiona el historial de chat de los clientes</p>
                </div>

                <div className="flex flex-col sm:flex-row w-full md:w-auto gap-4">
                    <div className="relative flex-1 sm:w-64">
                        <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400">🔍</span>
                        <input
                            type="text"
                            placeholder="Buscar por Cliente ID..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pl-10 pr-4 py-2 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 transition-all"
                        />
                    </div>
                    <button
                        onClick={() => handleOpenModal()}
                        className="bg-primary-600 text-white p-3 md:px-6 md:py-3 rounded-lg font-medium hover:bg-primary-700 transition-colors flex items-center justify-center whitespace-nowrap"
                        title="Nueva Conversación"
                    >
                        <span className="text-xl md:mr-2">➕</span>
                        <span className="hidden md:inline">Nueva Conversación</span>
                    </button>
                </div>
            </div>

            <div className="bg-white rounded-xl shadow-md overflow-x-auto">
                <table className="w-full">
                    <thead className="bg-gray-50 border-b">
                        <tr>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Cliente ID</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Estado</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Acciones</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-200">
                        {filteredConversaciones.map((conv) => (
                            <tr key={conv.id} className="hover:bg-gray-50">
                                <td className="px-6 py-4 font-medium text-gray-900">{conv.clienteId}</td>
                                <td className="px-6 py-4">
                                    <span className={`px-3 py-1 rounded-full text-sm font-medium ${conv.activa === true ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'}`}>
                                        {getEstadoText(conv.activa)}
                                    </span>
                                </td>
                                <td className="px-6 py-4">
                                    <div className="flex space-x-2">
                                        <button
                                            onClick={() => handleOpenModal(conv)}
                                            className="p-2 text-primary-600 hover:bg-primary-50 rounded-lg transition-colors"
                                            title="Editar"
                                        >
                                            ✏️
                                        </button>
                                        <button
                                            onClick={() => handleDeletePermanently(conv.id)}
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
                            {editingConversacion ? 'Editar Conversación' : 'Nueva Conversación'}
                        </h2>

                        <form onSubmit={handleSubmit} className="space-y-4">
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Cliente ID</label>
                                <input type="number" min="1" step="1" value={formData.clienteId} onChange={(e) => setFormData({ ...formData, clienteId: e.target.value })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500" required />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Estado</label>
                                <select value={formData.activa} onChange={(e) => setFormData({ ...formData, activa: e.target.value }, console.log(formData.activa, e.target.value))} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500" required>
                                    <option key={true} value={true}>Activa</option>
                                    <option key={false} value={false}>Cerrada</option>
                                </select>
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

export default Conversaciones;
