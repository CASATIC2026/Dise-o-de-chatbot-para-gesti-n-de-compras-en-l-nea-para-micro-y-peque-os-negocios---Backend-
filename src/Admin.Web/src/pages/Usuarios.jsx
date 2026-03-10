import { useState, useEffect } from 'react';
import api from '../api/client';

function Usuarios() {
    const [usuarios, setUsuarios] = useState([]);
    const [loading, setLoading] = useState(true);
    const [showModal, setShowModal] = useState(false);
    const [editingUsuario, setEditingUsuario] = useState(null);
    const [formData, setFormData] = useState({
        nombre: '',
        email: '',
        contrasenaHash: '',
        rol: 1,
        estado: true,
        telefono: ''
    });

    useEffect(() => {
        fetchUsuarios();
    }, []);

    const fetchUsuarios = async () => {
        try {
            const response = await api.get('/admin/inventario/usuarios'); // Asumiendo este endpoint
            setUsuarios(response.data);
        } catch (error) {
            console.error('Error fetching usuarios:', error);
        } finally {
            setLoading(false);
        }
    };

    const handleOpenModal = (usuario = null) => {
        if (usuario) {
            setEditingUsuario(usuario);
            setFormData(usuario);
        } else {
            setEditingUsuario(null);
            setFormData({
                nombre: '',
                email: '',
                contrasenaHash: '',
                rol: 1,
                estado: true,
                telefono: ''
            });
        }
        setShowModal(true);
    };

    const handleCloseModal = () => {
        setShowModal(false);
        setEditingUsuario(null);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        try {
            const dataToSave = {
                id: editingUsuario ? Number(editingUsuario.id) : 0,
                nombre: formData.nombre,
                email: formData.email,
                contrasenaHash: formData.contrasenaHash, // Idealmente esto se maneja distinto
                rol: Number(formData.rol),
                estado: Boolean(formData.estado),
                telefono: formData.telefono,
            };

            if (editingUsuario) {
                await api.put(`/admin/inventario/usuarios/${editingUsuario.id}`, dataToSave);
                alert("¡Usuario actualizado!");
            } else {
                await api.post('/admin/inventario/usuarios', dataToSave);
                alert("¡Usuario agregado!");
            }

            fetchUsuarios();
            handleCloseModal();

        } catch (error) {
            console.error('Error:', error);
            alert('Error al guardar/modificar el usuario');
        }
    };

    const getRolColor = (remitenteEnum) => {
        switch (remitenteEnum) {
            case 1: return 'bg-blue-100 text-blue-800';
            case 2: return 'bg-green-100 text-green-800';
            default: return 'bg-gray-100 text-gray-800';
        }
    };

    const getRolText = (remitenteEnum) => {
        switch (remitenteEnum) {
            case 1: return 'Administrador';
            case 2: return 'Vendedor';
        }
    };

    const handleDeletePermanently = async (id) => {
        if (!confirm('¿Estás seguro de eliminar este usuario?')) return;

        try {
            await api.delete(`/admin/inventario/usuarios/${id}`);
            fetchUsuarios();
        } catch (error) {
            console.error('Error deleting usuario:', error);
        }
    }

    if (loading) {
        return <div className="text-center py-12">Cargando usuarios...</div>;
    }

    return (
        <div>
            <div className="flex justify-between items-center mb-8">
                <div>
                    <h1 className="text-3xl font-bold text-gray-800">Usuarios</h1>
                    <p className="text-gray-600 mt-2">Gestiona los usuarios del sistema</p>
                </div>
                <button
                    onClick={() => handleOpenModal()}
                    className="bg-primary-600 text-white px-6 py-3 rounded-lg font-medium hover:bg-primary-700 transition-colors"
                >
                    + Nuevo Usuario
                </button>
            </div>

            <div className="bg-white rounded-xl shadow-md overflow-hidden">
                <table className="w-full">
                    <thead className="bg-gray-50 border-b">
                        <tr>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Nombre</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Email</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Rol</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Estado</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Acciones</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-200">
                        {usuarios.map((usuario) => (
                            <tr key={usuario.id} className="hover:bg-gray-50">
                                <td className="px-6 py-4 font-medium text-gray-900">{usuario.nombre}</td>
                                <td className="px-6 py-4 text-gray-900">{usuario.email}</td>
                                <td className="px-6 py-4 ">
                                    <span className={`px-3 py-1 rounded-full text-sm font-medium ${getRolColor(usuario.rol)}`}>
                                        {getRolText(usuario.rol)}
                                    </span>
                                </td>
                                <td className="px-6 py-4">
                                    <span className={`px-3 py-1 rounded-full text-sm font-medium ${usuario.estado ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                                        {usuario.estado ? 'Activo' : 'Inactivo'}
                                    </span>
                                </td>
                                <td className="px-6 py-4">
                                    <div className="flex space-x-2">
                                        <button onClick={() => handleOpenModal(usuario)} className="text-primary-600 hover:text-primary-800 font-medium">Editar</button>
                                        <button onClick={() => handleDeletePermanently(usuario.id)} className="text-red-600 hover:text-red-800 font-medium">Eliminar</button>
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
                            {editingUsuario ? 'Editar Usuario' : 'Nuevo Usuario'}
                        </h2>

                        <form onSubmit={handleSubmit} className="space-y-4">
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Nombre</label>
                                <input type="text" value={formData.nombre} onChange={(e) => setFormData({ ...formData, nombre: e.target.value })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500" required />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
                                <input type="email" value={formData.email} onChange={(e) => setFormData({ ...formData, email: e.target.value })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500" required />
                            </div>
                            {!editingUsuario && (
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">Contraseña</label>
                                    <input type="password" value={formData.contrasenaHash} onChange={(e) => setFormData({ ...formData, contrasenaHash: e.target.value })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500" required />
                                </div>
                            )}
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Rol</label>
                                <select value={formData.rol} onChange={(e) => setFormData({ ...formData, rol: Number(e.target.value) })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500" required>
                                    <option value={1}>Administrador</option>
                                    <option value={2}>Vendedor</option>
                                </select>
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Teléfono</label>
                                <input type="text" value={formData.telefono || ''} onChange={(e) => setFormData({ ...formData, telefono: e.target.value })} className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500" />
                            </div>
                            <div className="flex items-center">
                                <input type="checkbox" checked={formData.estado} onChange={(e) => setFormData({ ...formData, estado: e.target.checked })} className="w-4 h-4 text-primary-600" />
                                <label className="ml-2 text-sm text-gray-700">Usuario activo</label>
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

export default Usuarios;
