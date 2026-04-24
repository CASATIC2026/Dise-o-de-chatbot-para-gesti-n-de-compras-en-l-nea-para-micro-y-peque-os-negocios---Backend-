import { useEffect, useState } from 'react';
import api from '../api/client';

function ActionIcon({ name }) {
    const commonProps = {
        className: 'w-4 h-4',
        viewBox: '0 0 24 24',
        fill: 'none',
        stroke: 'currentColor',
        strokeWidth: 2,
        strokeLinecap: 'round',
        strokeLinejoin: 'round',
        'aria-hidden': true,
    };

    switch (name) {
        case 'edit':
            return (
                <svg {...commonProps}>
                    <path d="M12 20h9" />
                    <path d="M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4 12.5-12.5Z" />
                </svg>
            );
        case 'delete':
            return (
                <svg {...commonProps}>
                    <path d="M3 6h18" />
                    <path d="M8 6V4h8v2" />
                    <path d="M19 6l-1 14H6L5 6" />
                    <path d="M10 11v6M14 11v6" />
                </svg>
            );
        case 'search':
        default:
            return (
                <svg {...commonProps}>
                    <circle cx="11" cy="11" r="7" />
                    <path d="m20 20-3.5-3.5" />
                </svg>
            );
    }
}

const ROLES = {
    ADMINISTRADOR: 1,
    VENDEDOR: 2,
};

const initialFormData = {
    nombre: '',
    email: '',
    contrasenaHash: '',
    rol: ROLES.VENDEDOR,
    estado: true,
    telefono: '',
};

function Usuarios() {
    const [usuarios, setUsuarios] = useState([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [showModal, setShowModal] = useState(false);
    const [editingUsuario, setEditingUsuario] = useState(null);
    const [formData, setFormData] = useState(initialFormData);

    useEffect(() => {
        fetchUsuarios();
    }, []);

    const fetchUsuarios = async () => {
        try {
            const response = await api.get('/admin/inventario/usuarios');
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
            setFormData({
                ...usuario,
                contrasenaHash: '',
            });
        } else {
            setEditingUsuario(null);
            setFormData(initialFormData);
        }

        setShowModal(true);
    };

    const handleCloseModal = () => {
        setShowModal(false);
        setEditingUsuario(null);
        setFormData(initialFormData);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        try {
            const dataToSave = {
                id: editingUsuario ? Number(editingUsuario.id) : 0,
                nombre: formData.nombre,
                email: formData.email,
                contrasenaHash: formData.contrasenaHash,
                rol: Number(formData.rol),
                estado: Boolean(formData.estado),
                telefono: formData.telefono,
            };

            if (editingUsuario) {
                await api.put(`/admin/inventario/usuarios/${editingUsuario.id}`, dataToSave);
                alert('Usuario actualizado');
            } else {
                await api.post('/admin/inventario/usuarios', dataToSave);
                alert('Usuario agregado');
            }

            fetchUsuarios();
            handleCloseModal();
        } catch (error) {
            console.error('Error saving usuario:', error);
            alert('Error al guardar o modificar el usuario');
        }
    };

    const getRolColor = (rol) => {
        switch (rol) {
            case ROLES.ADMINISTRADOR:
                return 'bg-blue-100 text-blue-800';
            case ROLES.VENDEDOR:
                return 'bg-emerald-100 text-emerald-800';
            default:
                return 'bg-gray-100 text-gray-800';
        }
    };

    const getRolText = (rol) => {
        switch (rol) {
            case ROLES.ADMINISTRADOR:
                return 'Administrador';
            case ROLES.VENDEDOR:
                return 'Vendedor';
            default:
                return 'Sin rol';
        }
    };

    const handleDeletePermanently = async (id) => {
        if (!window.confirm('Esta seguro de eliminar este usuario?')) {
            return;
        }

        try {
            await api.delete(`/admin/inventario/usuarios/${id}`);
            fetchUsuarios();
        } catch (error) {
            console.error('Error deleting usuario:', error);
        }
    };

    const filteredUsuarios = usuarios.filter((user) =>
        user.nombre.toLowerCase().includes(searchTerm.toLowerCase()) ||
        user.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
        getRolText(user.rol).toLowerCase().includes(searchTerm.toLowerCase())
    );

    const administradores = filteredUsuarios.filter((usuario) => usuario.rol === ROLES.ADMINISTRADOR);
    const vendedores = filteredUsuarios.filter((usuario) => usuario.rol === ROLES.VENDEDOR);

    const renderTablaUsuarios = (listaUsuarios, emptyMessage) => (
        <div className="bg-white rounded-xl shadow-md overflow-x-auto">
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
                    {listaUsuarios.length > 0 ? (
                        listaUsuarios.map((usuario) => (
                            <tr key={usuario.id} className="hover:bg-gray-50">
                                <td className="px-6 py-4 font-medium text-gray-900">{usuario.nombre}</td>
                                <td className="px-6 py-4 text-gray-900">{usuario.email}</td>
                                <td className="px-6 py-4">
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
                                        <button
                                            onClick={() => handleOpenModal(usuario)}
                                            className="p-2 text-primary-600 hover:bg-primary-50 rounded-lg transition-colors inline-flex items-center gap-2"
                                            title="Editar"
                                        >
                                            <ActionIcon name="edit" />
                                            Editar
                                        </button>
                                        <button
                                            onClick={() => handleDeletePermanently(usuario.id)}
                                            className="p-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors inline-flex items-center gap-2"
                                            title="Eliminar"
                                        >
                                            <ActionIcon name="delete" />
                                            Eliminar
                                        </button>
                                    </div>
                                </td>
                            </tr>
                        ))
                    ) : (
                        <tr>
                            <td colSpan="5" className="px-6 py-8 text-center text-gray-500">
                                {emptyMessage}
                            </td>
                        </tr>
                    )}
                </tbody>
            </table>
        </div>
    );

    if (loading) {
        return <div className="text-center py-12">Cargando usuarios...</div>;
    }

    return (
        <div>
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-8">
                <div>
                    <h1 className="text-3xl font-bold text-gray-800">Usuarios</h1>
                    <p className="text-gray-600 mt-2">Gestiona administradores y vendedores del panel</p>
                </div>

                <div className="flex flex-col sm:flex-row w-full md:w-auto gap-4">
                    <div className="relative flex-1 sm:w-64">
                        <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400">
                            <ActionIcon name="search" />
                        </span>
                        <input
                            type="text"
                            placeholder="Buscar usuarios..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pl-10 pr-4 py-2 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 transition-all"
                        />
                    </div>
                    <button
                        onClick={() => handleOpenModal()}
                        className="bg-primary-600 text-white p-3 md:px-6 md:py-3 rounded-lg font-medium hover:bg-primary-700 transition-colors flex items-center justify-center whitespace-nowrap"
                        title="Nuevo Usuario"
                    >
                        Nuevo Usuario
                    </button>
                </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
                <div className="bg-white rounded-xl shadow-md p-5 border-l-4 border-blue-500">
                    <p className="text-sm text-gray-500">Administradores</p>
                    <p className="text-3xl font-bold text-gray-800 mt-2">{administradores.length}</p>
                </div>
                <div className="bg-white rounded-xl shadow-md p-5 border-l-4 border-emerald-500">
                    <p className="text-sm text-gray-500">Vendedores</p>
                    <p className="text-3xl font-bold text-gray-800 mt-2">{vendedores.length}</p>
                </div>
                <div className="bg-white rounded-xl shadow-md p-5 border-l-4 border-gray-400">
                    <p className="text-sm text-gray-500">Total usuarios panel</p>
                    <p className="text-3xl font-bold text-gray-800 mt-2">{filteredUsuarios.length}</p>
                </div>
            </div>

            <div className="space-y-8">
                <section>
                    <div className="mb-4">
                        <h2 className="text-xl font-semibold text-gray-800">Vendedores</h2>
                        <p className="text-sm text-gray-600 mt-1">Usuarios con acceso operativo limitado respecto al administrador.</p>
                    </div>
                    {renderTablaUsuarios(vendedores, 'No hay vendedores para mostrar.')}
                </section>

                <section>
                    <div className="mb-4">
                        <h2 className="text-xl font-semibold text-gray-800">Administradores</h2>
                        <p className="text-sm text-gray-600 mt-1">Estos usuarios mantienen acceso completo al panel.</p>
                    </div>
                    {renderTablaUsuarios(administradores, 'No hay administradores para mostrar.')}
                </section>
            </div>

            {showModal && (
                <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
                    <div className="bg-white rounded-xl p-8 max-w-md w-full max-h-[90vh] overflow-y-auto">
                        <h2 className="text-2xl text-gray-700 font-bold mb-6">
                            {editingUsuario ? 'Editar Usuario' : 'Nuevo Usuario'}
                        </h2>

                        <form onSubmit={handleSubmit} className="space-y-4">
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Nombre</label>
                                <input
                                    type="text"
                                    value={formData.nombre}
                                    onChange={(e) => setFormData({ ...formData, nombre: e.target.value })}
                                    className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500"
                                    required
                                />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
                                <input
                                    type="email"
                                    value={formData.email}
                                    onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                                    className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500"
                                    required
                                />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">
                                    {editingUsuario ? 'Nueva contrasena (opcional)' : 'Contrasena'}
                                </label>
                                <input
                                    type="password"
                                    value={formData.contrasenaHash}
                                    onChange={(e) => setFormData({ ...formData, contrasenaHash: e.target.value })}
                                    className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500"
                                    required={!editingUsuario}
                                />
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Rol</label>
                                <select
                                    value={formData.rol}
                                    onChange={(e) => setFormData({ ...formData, rol: Number(e.target.value) })}
                                    className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500"
                                    required
                                >
                                    <option value={ROLES.VENDEDOR}>Vendedor</option>
                                    <option value={ROLES.ADMINISTRADOR}>Administrador</option>
                                </select>
                            </div>
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Telefono</label>
                                <input
                                    type="text"
                                    value={formData.telefono || ''}
                                    onChange={(e) => setFormData({ ...formData, telefono: e.target.value })}
                                    className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500"
                                />
                            </div>
                            <div className="flex items-center">
                                <input
                                    type="checkbox"
                                    checked={formData.estado}
                                    onChange={(e) => setFormData({ ...formData, estado: e.target.checked })}
                                    className="w-4 h-4 text-primary-600"
                                />
                                <label className="ml-2 text-sm text-gray-700">Usuario activo</label>
                            </div>
                            <div className="flex space-x-3 pt-4">
                                <button type="submit" className="flex-1 bg-primary-600 text-white py-2 rounded-lg font-medium hover:bg-primary-700">
                                    Guardar
                                </button>
                                <button type="button" onClick={handleCloseModal} className="flex-1 bg-gray-200 text-gray-800 py-2 rounded-lg font-medium hover:bg-gray-300">
                                    Cancelar
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
}

export default Usuarios;
