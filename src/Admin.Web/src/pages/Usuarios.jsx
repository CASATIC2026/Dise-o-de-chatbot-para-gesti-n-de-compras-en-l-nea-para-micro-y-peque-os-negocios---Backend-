import { useState, useEffect } from 'react';
import api from '../api/client';

function Usuarios() {
    const [usuarios, setUsuarios] = useState([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
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
            case 1: return 'bg-primary-100 text-primary-800 border-primary-200';
            case 2: return 'bg-tertiary-100 text-tertiary-800 border-tertiary-200';
            default: return 'bg-neutral-100 text-neutral-800 border-neutral-200';
        }
    };

    const getRolText = (remitenteEnum) => {
        switch (remitenteEnum) {
            case 1: return 'Administrador';
            case 2: return 'Vendedor';
            default: return 'Desconocido';
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

    const filteredUsuarios = usuarios.filter(user =>
        (user.nombre && user.nombre.toLowerCase().includes(searchTerm.toLowerCase())) ||
        (user.email && user.email.toLowerCase().includes(searchTerm.toLowerCase())) ||
        (user.rol && user.rol.toString().includes(searchTerm))
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
                    <h1 className="text-3xl font-bold text-neutral-900 tracking-tight">Usuarios</h1>
                    <p className="text-neutral-500 mt-2">Gestiona los accesos al panel administrativo</p>
                </div>

                <div className="flex flex-col sm:flex-row w-full md:w-auto gap-4">
                    <div className="relative flex-1 sm:w-72">
                        <span className="absolute left-3 top-1/2 -translate-y-1/2 text-neutral-400">🔍</span>
                        <input
                            type="text"
                            placeholder="Buscar usuarios..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pl-10 pr-4 py-2.5 bg-white border border-neutral-200 rounded-xl focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all shadow-sm"
                        />
                    </div>
                    <button
                        onClick={() => handleOpenModal()}
                        className="bg-primary-600 text-white px-5 py-2.5 rounded-xl font-semibold shadow-sm shadow-primary-500/30 hover:bg-primary-700 hover:shadow-md hover:shadow-primary-500/40 transition-all flex items-center justify-center whitespace-nowrap gap-2"
                        title="Nuevo Usuario"
                    >
                        <span className="text-lg">➕</span>
                        <span>Nuevo Usuario</span>
                    </button>
                </div>
            </div>

            <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 overflow-hidden">
                <div className="overflow-x-auto">
                    <table className="w-full text-left border-collapse">
                        <thead>
                            <tr className="bg-neutral-50/50 border-b border-neutral-200">
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 uppercase tracking-wider">Usuario</th>
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 uppercase tracking-wider">Contacto</th>
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 uppercase tracking-wider">Rol</th>
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 uppercase tracking-wider">Estado</th>
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 uppercase tracking-wider">Acciones</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-neutral-100">
                            {filteredUsuarios.map((usuario) => (
                                <tr key={usuario.id} className="hover:bg-neutral-50/50 transition-colors">
                                    <td className="px-6 py-4">
                                        <div className="flex items-center gap-3">
                                            <div className="w-10 h-10 rounded-full bg-neutral-100 text-primary-600 flex items-center justify-center font-bold text-lg">
                                                {usuario.nombre ? usuario.nombre.charAt(0).toUpperCase() : 'U'}
                                            </div>
                                            <div className="font-bold text-neutral-900">{usuario.nombre}</div>
                                        </div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="text-sm font-medium text-neutral-600 mb-0.5">{usuario.email}</div>
                                        <div className="text-xs text-neutral-500"><span className="mr-1">📞</span>{usuario.telefono || 'N/A'}</div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <span className={`px-2.5 py-1 rounded-md text-xs font-bold border ${getRolColor(usuario.rol)}`}>
                                            {getRolText(usuario.rol)}
                                        </span>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="flex items-center gap-1.5">
                                            <div className={`w-2 h-2 rounded-full ${usuario.estado ? 'bg-emerald-500 shadow-[0_0_8px_rgba(16,185,129,0.5)]' : 'bg-rose-500 shadow-[0_0_8px_rgba(244,63,94,0.5)]'}`}></div>
                                            <span className={`text-sm font-semibold ${usuario.estado ? 'text-emerald-700' : 'text-rose-700'}`}>
                                                {usuario.estado ? 'Activo' : 'Inactivo'}
                                            </span>
                                        </div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="flex items-center gap-1.5">
                                            <button
                                                onClick={() => handleOpenModal(usuario)}
                                                className="p-1.5 text-primary-600 hover:bg-primary-50 rounded-lg transition-colors border border-transparent hover:border-primary-100"
                                                title="Editar"
                                            >
                                                ✏️
                                            </button>
                                            <button
                                                onClick={() => handleDeletePermanently(usuario.id)}
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

                    {filteredUsuarios.length === 0 && (
                        <div className="flex flex-col justify-center items-center py-16 text-neutral-500">
                            <span className="text-5xl mb-4">🛡️</span>
                            <span className="font-medium">No se encontraron usuarios.</span>
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
                                {editingUsuario ? 'Editar Usuario' : 'Nuevo Usuario'}
                            </h2>
                            <button onClick={handleCloseModal} className="text-neutral-400 hover:text-neutral-600 bg-neutral-50 hover:bg-neutral-100 rounded-lg p-1.5 transition-colors">
                                ✕
                            </button>
                        </div>

                        <div className="p-6 overflow-y-auto">
                            <form id="usuarioForm" onSubmit={handleSubmit} className="space-y-5">
                                <div className="grid grid-cols-2 gap-4">
                                    <div>
                                        <label className="block text-sm font-semibold text-neutral-700 mb-1.5">Nombre <span className="text-red-500">*</span></label>
                                        <input
                                            type="text"
                                            value={formData.nombre}
                                            onChange={(e) => setFormData({ ...formData, nombre: e.target.value })}
                                            className="w-full px-4 py-2.5 bg-neutral-50 border border-neutral-200 rounded-xl focus:bg-white focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all"
                                            placeholder="Ej. Ana Gómez"
                                            required
                                        />
                                    </div>
                                    <div>
                                        <label className="block text-sm font-semibold text-neutral-700 mb-1.5">Email <span className="text-red-500">*</span></label>
                                        <input
                                            type="email"
                                            value={formData.email}
                                            onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                                            className="w-full px-4 py-2.5 bg-neutral-50 border border-neutral-200 rounded-xl focus:bg-white focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all"
                                            placeholder="admin@ejemplo.com"
                                            required
                                        />
                                    </div>
                                </div>

                                <div className="grid grid-cols-2 gap-4">
                                    {!editingUsuario && (
                                        <div>
                                            <label className="block text-sm font-semibold text-neutral-700 mb-1.5">Contraseña <span className="text-red-500">*</span></label>
                                            <input
                                                type="password"
                                                value={formData.contrasenaHash}
                                                onChange={(e) => setFormData({ ...formData, contrasenaHash: e.target.value })}
                                                className="w-full px-4 py-2.5 bg-neutral-50 border border-neutral-200 rounded-xl focus:bg-white focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all font-mono"
                                                required
                                            />
                                        </div>
                                    )}
                                    <div>
                                        <label className="block text-sm font-semibold text-neutral-700 mb-1.5">Teléfono</label>
                                        <input
                                            type="text"
                                            value={formData.telefono || ''}
                                            onChange={(e) => setFormData({ ...formData, telefono: e.target.value })}
                                            className="w-full px-4 py-2.5 bg-neutral-50 border border-neutral-200 rounded-xl focus:bg-white focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all"
                                            placeholder="+57..."
                                        />
                                    </div>
                                </div>

                                <div className="grid grid-cols-2 gap-4">
                                    <div>
                                        <label className="block text-sm font-semibold text-neutral-700 mb-1.5">Rol <span className="text-red-500">*</span></label>
                                        <select
                                            value={formData.rol}
                                            onChange={(e) => setFormData({ ...formData, rol: Number(e.target.value) })}
                                            className="w-full px-4 py-2.5 bg-neutral-50 border border-neutral-200 rounded-xl focus:bg-white focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all font-medium"
                                            required
                                        >
                                            <option value={1}>Administrador</option>
                                            <option value={2}>Vendedor</option>
                                        </select>
                                    </div>
                                    <div className="flex flex-col justify-end pb-2">
                                        <label className="relative inline-flex items-center cursor-pointer group">
                                            <input
                                                type="checkbox"
                                                className="sr-only peer"
                                                checked={formData.estado}
                                                onChange={(e) => setFormData({ ...formData, estado: e.target.checked })}
                                            />
                                            <div className="w-11 h-6 bg-neutral-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-primary-500/20 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-primary-600 transition-colors"></div>
                                            <span className="ml-3 text-sm font-semibold text-neutral-700">Usuario Activo</span>
                                        </label>
                                    </div>
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
                                form="usuarioForm"
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

export default Usuarios;
