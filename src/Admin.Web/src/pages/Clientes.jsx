import { useState, useEffect } from 'react';
import api from '../api/client';

function Clientes() {
    const [clientes, setClientes] = useState([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [showModal, setShowModal] = useState(false);
    const [editingCliente, setEditingCliente] = useState(null);
    const [formData, setFormData] = useState({
        nombre: '',
        telefono: '',
        email: '',
        direccion: ''
    });

    useEffect(() => {
        fetchClientes();
    }, []);

    const fetchClientes = async () => {
        try {
            const response = await api.get('/admin/inventario/clientes'); // Asumiendo este endpoint
            setClientes(response.data);
        } catch (error) {
            console.error('Error fetching clientes:', error);
        } finally {
            setLoading(false);
        }
    };

    const handleOpenModal = (cliente = null) => {
        if (cliente) {
            setEditingCliente(cliente);
            setFormData(cliente);
        } else {
            setEditingCliente(null);
            setFormData({
                nombre: '',
                telefono: '',
                email: '',
                direccion: '',
            });
        }
        setShowModal(true);
    };

    const handleCloseModal = () => {
        setShowModal(false);
        setEditingCliente(null);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        try {
            const dataToSave = {
                id: editingCliente ? Number(editingCliente.id) : 0,
                nombre: formData.nombre,
                telefono: formData.telefono,
                email: formData.email,
                direccion: formData.direccion,
            };

            if (editingCliente) {
                await api.put(`/admin/inventario/clientes/${editingCliente.id}`, dataToSave);
                alert("¡Cliente actualizado!");
            } else {
                await api.post('/admin/inventario/clientes', dataToSave);
                alert("¡Cliente agregado!");
            }

            fetchClientes();
            handleCloseModal();

        } catch (error) {
            console.error('Error:', error);
            alert('Error al guardar/modificar el cliente');
        }
    };

    const handleDeletePermanently = async (id) => {
        if (!confirm('¿Estás seguro de eliminar este cliente?')) return;

        try {
            await api.delete(`/admin/inventario/clientes/${id}`);
            fetchClientes();
        } catch (error) {
            console.error('Error deleting cliente:', error);
        }
    }

    const filteredClientes = clientes.filter(cliente =>
        cliente.nombre.toLowerCase().includes(searchTerm.toLowerCase()) ||
        cliente.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
        cliente.telefono.toLowerCase().includes(searchTerm.toLowerCase())
    );

    if (loading) {
        return <div className="text-center py-12">Cargando clientes...</div>;
    }

    return (
        <div>
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-8">
                <div>
                    <h1 className="text-3xl font-bold text-gray-800">Clientes</h1>
                    <p className="text-gray-600 mt-2">Gestiona tu base de datos de clientes</p>
                </div>

                <div className="flex flex-col sm:flex-row w-full md:w-auto gap-4">
                    <div className="relative flex-1 sm:w-64">
                        <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400">🔍</span>
                        <input
                            type="text"
                            placeholder="Buscar clientes..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pl-10 pr-4 py-2 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 transition-all"
                        />
                    </div>
                    <button
                        onClick={() => handleOpenModal()}
                        className="bg-primary-600 text-white p-3 md:px-6 md:py-3 rounded-lg font-medium hover:bg-primary-700 transition-colors flex items-center justify-center whitespace-nowrap"
                        title="Nuevo Cliente"
                    >
                        <span className="text-xl md:mr-2">➕</span>
                        <span className="hidden md:inline">Nuevo Cliente</span>
                    </button>
                </div>
            </div>

            <div className="bg-white rounded-xl shadow-md overflow-x-auto">
                <table className="w-full">
                    <thead className="bg-gray-50 border-b">
                        <tr>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Nombre</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Teléfono</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Email</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Dirección</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Acciones</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-200">
                        {filteredClientes.map((cliente) => (
                            <tr key={cliente.id} className="hover:bg-gray-50">
                                <td className="px-6 py-4 font-medium text-gray-900">{cliente.nombre}</td>
                                <td className="px-6 py-4 text-gray-900">{cliente.telefono}</td>
                                <td className="px-6 py-4 text-gray-900">{cliente.email}</td>
                                <td className="px-6 py-4 text-gray-500 text-sm">{cliente.direccion}</td>
                                <td className="px-6 py-4">
                                    <div className="flex space-x-2">
                                        <button
                                            onClick={() => handleOpenModal(cliente)}
                                            className="p-2 text-primary-600 hover:bg-primary-50 rounded-lg transition-colors"
                                            title="Editar"
                                        >
                                            ✏️
                                        </button>
                                        <button
                                            onClick={() => handleDeletePermanently(cliente.id)}
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
                            {editingCliente ? 'Editar Cliente' : 'Nuevo Cliente'}
                        </h2>

                        <form onSubmit={handleSubmit} className="space-y-4">
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Nombre</label>
                                <input type="text" value={formData.nombre} onChange={(e) => setFormData({ ...formData, nombre: e.target.value })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500" required />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Teléfono</label>
                                <input type="text" value={formData.telefono || ''} onChange={(e) => setFormData({ ...formData, telefono: e.target.value })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500" />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
                                <input type="email" value={formData.email || ''} onChange={(e) => setFormData({ ...formData, email: e.target.value })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500" />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Dirección</label>
                                <textarea value={formData.direccion || ''} onChange={(e) => setFormData({ ...formData, direccion: e.target.value })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500" rows="3" />
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

export default Clientes;
