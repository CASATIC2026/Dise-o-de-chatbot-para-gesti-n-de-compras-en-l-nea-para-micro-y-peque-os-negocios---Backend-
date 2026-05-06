import { useEffect, useState } from 'react';
import api from '../api/client';
import { SearchIcon, AddNewIcon, EditIcon, DeleteIcon, PhoneIcon, ShieldIcon, CrossIcon } from '../components/Icons';
import Pagination from '../components/Pagination';

const inputCls = "w-full px-4 py-2.5 bg-neutral-50 dark:bg-dark-input border border-neutral-200 dark:border-dark-border text-neutral-900 dark:text-neutral-100 rounded-xl placeholder:text-neutral-400 dark:placeholder:text-neutral-600 focus:bg-white dark:focus:bg-dark-surface focus:outline-none focus:border-primary-500 dark:focus:border-cyan-500 focus:ring-4 focus:ring-primary-500/10 dark:focus:ring-cyan-500/10 transition-all";
const labelCls = "block text-sm font-semibold text-neutral-700 dark:text-neutral-300 mb-1.5";

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
    const [totalCount, setTotalCount] = useState(0);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [currentPage, setCurrentPage] = useState(1);
    const [showModal, setShowModal] = useState(false);
    const [editingUsuario, setEditingUsuario] = useState(null);
    const [formData, setFormData] = useState({ nombre: '', email: '', contrasenaHash: '', rol: 1, estado: true, telefono: '' });

    const ITEMS_PER_PAGE = 10;

    useEffect(() => { 
        fetchUsuarios(); 
    }, [currentPage, searchTerm]);

    const fetchUsuarios = async () => {
        try {
            setLoading(true);
            const response = await api.get('/admin/inventario/usuarios/paged', {
                params: {
                    page: currentPage,
                    pageSize: ITEMS_PER_PAGE,
                    search: searchTerm
                }
            });
            setUsuarios(response.data.items);
            setTotalCount(response.data.totalCount);
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
                return 'bg-blue-100 dark:bg-blue-900/20 text-blue-800 dark:text-blue-400';
            case ROLES.VENDEDOR:
                return 'bg-emerald-100 dark:bg-emerald-900/20 text-emerald-800 dark:text-emerald-400';
            default:
                return 'bg-gray-100 dark:bg-gray-700 text-gray-800 dark:text-gray-300';
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

    const totalPages = Math.max(1, Math.ceil(totalCount / ITEMS_PER_PAGE));
    const handleSearch = (v) => { setSearchTerm(v); setCurrentPage(1); };


    if (loading) return <div className="flex justify-center items-center h-64"><div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600 dark:border-cyan-500"></div></div>;

    return (
        <div className="animate-fade-in">
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-8">
                <div>
                    <h1 className="text-3xl font-bold dark:text-gray-100 tracking-tight">Usuarios</h1>
                    <p className="text-gray-600 dark:text-gray-400 mt-2">Gestiona administradores y vendedores del panel</p>
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
                            onChange={(e) => handleSearch(e.target.value)}
                            className="w-full pl-10 pr-4 py-2 bg-white dark:bg-dark-input border border-gray-200 dark:border-dark-border text-gray-900 dark:text-gray-100 rounded-lg placeholder:text-gray-400 dark:placeholder:text-gray-500 focus:outline-none focus:ring-2 focus:ring-primary-500 dark:focus:ring-cyan-500 transition-all"
                        />
                    </div>
                    <button
                        onClick={() => handleOpenModal()}
                        className="bg-primary-600 dark:bg-cyan-600 text-white p-3 md:px-6 md:py-3 rounded-lg font-medium hover:bg-primary-700 dark:hover:bg-cyan-700 transition-colors flex items-center justify-center whitespace-nowrap"
                        title="Nuevo Usuario"
                    >
                        Nuevo Usuario
                    </button>
                </div>
            </div>

            <div className="bg-white dark:bg-dark-surface rounded-xl shadow-md dark:shadow-none border border-gray-200 dark:border-dark-border overflow-hidden">
                <div className="overflow-x-auto">
                    <table className="w-full min-w-[920px]">
                        <thead className="bg-gray-50 dark:bg-dark-input border-b border-gray-200 dark:border-dark-border">
                            <tr>
                                <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Nombre</th>
                                <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Email</th>
                                <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Rol</th>
                                <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Estado</th>
                                <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Acciones</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-200 dark:divide-dark-border">
                            {usuarios.length > 0 ? (
                                usuarios.map((usuario) => (
                                    <tr key={usuario.id} className="hover:bg-gray-50 dark:hover:bg-dark-input/50">
                                        <td className="px-6 py-4 font-medium text-gray-900 dark:text-gray-100">{usuario.nombre}</td>
                                        <td className="px-6 py-4 text-gray-900 dark:text-gray-200">{usuario.email}</td>
                                        <td className="px-6 py-4">
                                            <span className={`px-3 py-1 rounded-full text-sm font-medium ${getRolColor(usuario.rol)}`}>
                                                {getRolText(usuario.rol)}
                                            </span>
                                        </td>
                                        <td className="px-6 py-4">
                                            <span className={`px-3 py-1 rounded-full text-sm font-medium ${usuario.estado ? 'bg-green-100 dark:bg-green-900/20 text-green-800 dark:text-green-400' : 'bg-red-100 dark:bg-red-900/20 text-red-800 dark:text-red-400'}`}>
                                                {usuario.estado ? 'Activo' : 'Inactivo'}
                                            </span>
                                        </td>
                                        <td className="px-6 py-4">
                                            <div className="flex space-x-2">
                                                <button
                                                    onClick={() => handleOpenModal(usuario)}
                                                    className="p-2 text-primary-600 dark:text-cyan-400 hover:bg-primary-50 dark:hover:bg-cyan-900/20 rounded-lg transition-colors inline-flex items-center gap-2"
                                                    title="Editar"
                                                >
                                                    <ActionIcon name="edit" />
                                                    <span className="hidden sm:inline">Editar</span>
                                                </button>
                                                <button
                                                    onClick={() => handleDeletePermanently(usuario.id)}
                                                    className="p-2 text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-lg transition-colors inline-flex items-center gap-2"
                                                    title="Eliminar"
                                                >
                                                    <ActionIcon name="delete" />
                                                    <span className="hidden sm:inline">Eliminar</span>
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                ))
                            ) : (
                                <tr>
                                    <td colSpan="5" className="px-6 py-8 text-center text-gray-500 dark:text-gray-400">
                                        No se encontraron usuarios.
                                    </td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                </div>
                <Pagination 
                    currentPage={currentPage}
                    totalPages={totalPages}
                    totalItems={totalCount}
                    itemsPerPage={ITEMS_PER_PAGE}
                    onChange={setCurrentPage}
                />
            </div>

            {showModal && (
                <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-50 p-4">
                    <div className="bg-white dark:bg-dark-surface rounded-xl p-8 max-w-md w-full max-h-[90vh] overflow-y-auto border border-gray-200 dark:border-dark-border">
                        <h2 className="text-2xl text-gray-700 dark:text-gray-100 font-bold mb-6">
                            {editingUsuario ? 'Editar Usuario' : 'Nuevo Usuario'}
                        </h2>

                        <form onSubmit={handleSubmit} className="space-y-4">
                            <div>
                                <label className={labelCls}>Nombre</label>
                                <input
                                    type="text"
                                    value={formData.nombre}
                                    onChange={(e) => setFormData({ ...formData, nombre: e.target.value })}
                                    className={inputCls}
                                    required
                                />
                            </div>
                            <div>
                                <label className={labelCls}>Email</label>
                                <input
                                    type="email"
                                    value={formData.email}
                                    onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                                    className={inputCls}
                                    required
                                />
                            </div>
                            <div>
                                <label className={labelCls}>
                                    {editingUsuario ? 'Nueva contrasena (opcional)' : 'Contrasena'}
                                </label>
                                <input
                                    type="password"
                                    value={formData.contrasenaHash}
                                    onChange={(e) => setFormData({ ...formData, contrasenaHash: e.target.value })}
                                    className={inputCls}
                                    required={!editingUsuario}
                                />
                            </div>
                            <div>
                                <label className={labelCls}>Rol</label>
                                <select
                                    value={formData.rol}
                                    onChange={(e) => setFormData({ ...formData, rol: Number(e.target.value) })}
                                    className={inputCls}
                                    required
                                >
                                    <option value={ROLES.VENDEDOR}>Vendedor</option>
                                    <option value={ROLES.ADMINISTRADOR}>Administrador</option>
                                </select>
                            </div>
                            <div>
                                <label className={labelCls}>Telefono</label>
                                <input
                                    type="text"
                                    value={formData.telefono || ''}
                                    onChange={(e) => setFormData({ ...formData, telefono: e.target.value })}
                                    className={inputCls}
                                />
                            </div>
                            <div className="flex items-center">
                                <input
                                    type="checkbox"
                                    checked={formData.estado}
                                    onChange={(e) => setFormData({ ...formData, estado: e.target.checked })}
                                    className="w-4 h-4 text-primary-600"
                                />
                                <label className="ml-2 text-sm text-gray-700 dark:text-gray-300">Usuario activo</label>
                            </div>
                            <div className="flex space-x-3 pt-4">
                                <button type="submit" className="flex-1 bg-primary-600 dark:bg-cyan-600 text-white py-2 rounded-lg font-medium hover:bg-primary-700 dark:hover:bg-cyan-700">
                                    Guardar
                                </button>
                                <button type="button" onClick={handleCloseModal} className="flex-1 bg-gray-200 dark:bg-gray-700 text-gray-800 dark:text-gray-200 py-2 rounded-lg font-medium hover:bg-gray-300 dark:hover:bg-gray-600">
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
