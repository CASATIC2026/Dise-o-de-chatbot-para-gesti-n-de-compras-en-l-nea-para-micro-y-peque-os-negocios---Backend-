import { useState, useEffect } from 'react';
import api from '../api/client';
import { SearchIcon, AddNewIcon, EditIcon, DeleteIcon, PhoneIcon, ShieldIcon, CrossIcon } from '../components/Icons';

const inputCls = "w-full px-4 py-2.5 bg-neutral-50 dark:bg-dark-input border border-neutral-200 dark:border-dark-border text-neutral-900 dark:text-neutral-100 rounded-xl focus:bg-white dark:focus:bg-dark-surface focus:outline-none focus:border-primary-500 dark:focus:border-cyan-500 focus:ring-4 focus:ring-primary-500/10 dark:focus:ring-cyan-500/10 transition-all";
const labelCls = "block text-sm font-semibold text-neutral-700 dark:text-neutral-300 mb-1.5";

function Usuarios() {
    const [usuarios, setUsuarios] = useState([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [showModal, setShowModal] = useState(false);
    const [editingUsuario, setEditingUsuario] = useState(null);
    const [formData, setFormData] = useState({ nombre: '', email: '', contrasenaHash: '', rol: 1, estado: true, telefono: '' });

    useEffect(() => { fetchUsuarios(); }, []);

    const fetchUsuarios = async () => {
        try { const r = await api.get('/admin/inventario/usuarios'); setUsuarios(r.data); }
        catch (e) { console.error('Error fetching usuarios:', e); }
        finally { setLoading(false); }
    };

    const handleOpenModal = (usuario = null) => {
        if (usuario) { setEditingUsuario(usuario); setFormData(usuario); }
        else { setEditingUsuario(null); setFormData({ nombre: '', email: '', contrasenaHash: '', rol: 1, estado: true, telefono: '' }); }
        setShowModal(true);
    };

    const handleCloseModal = () => { setShowModal(false); setEditingUsuario(null); };

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            const d = { id: editingUsuario ? Number(editingUsuario.id) : 0, nombre: formData.nombre, email: formData.email, contrasenaHash: formData.contrasenaHash, rol: Number(formData.rol), estado: Boolean(formData.estado), telefono: formData.telefono };
            if (editingUsuario) { await api.put(`/admin/inventario/usuarios/${editingUsuario.id}`, d); alert('¡Usuario actualizado!'); }
            else { await api.post('/admin/inventario/usuarios', d); alert('¡Usuario agregado!'); }
            fetchUsuarios(); handleCloseModal();
        } catch (e) { console.error('Error:', e); alert('Error al guardar/modificar el usuario'); }
    };

    const handleDeletePermanently = async (id) => {
        if (!confirm('¿Estás seguro de eliminar este usuario?')) return;
        try { await api.delete(`/admin/inventario/usuarios/${id}`); fetchUsuarios(); }
        catch (e) { console.error('Error deleting usuario:', e); }
    };

    const getRolColor = (r) => ({ 1: 'bg-primary-100 dark:bg-cyan-900/20 text-primary-800 dark:text-cyan-400 border-primary-200 dark:border-cyan-800/30', 2: 'bg-purple-100 dark:bg-purple-900/20 text-purple-800 dark:text-purple-400 border-purple-200 dark:border-purple-800/30' }[r] || 'bg-neutral-100 dark:bg-dark-input text-neutral-800 dark:text-neutral-400 border-neutral-200 dark:border-dark-border');
    const getRolText = (r) => ({ 1: 'Administrador', 2: 'Vendedor' }[r] || 'Desconocido');

    const filteredUsuarios = usuarios.filter(u =>
        (u.nombre && u.nombre.toLowerCase().includes(searchTerm.toLowerCase())) ||
        (u.email && u.email.toLowerCase().includes(searchTerm.toLowerCase())) ||
        (u.rol && u.rol.toString().includes(searchTerm))
    );

    if (loading) return <div className="flex justify-center items-center h-64"><div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600 dark:border-cyan-500"></div></div>;

    return (
        <div className="animate-fade-in">
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-8">
                <div>
                    <h1 className="text-3xl font-bold text-neutral-900 dark:text-neutral-100 tracking-tight">Usuarios</h1>
                    <p className="text-neutral-500 dark:text-neutral-400 mt-2">Gestiona los accesos al panel administrativo</p>
                </div>
                <div className="flex flex-col sm:flex-row w-full md:w-auto gap-4">
                    <div className="relative flex-1 sm:w-72">
                        <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-neutral-400" />
                        <input type="text" placeholder="Buscar usuarios..." value={searchTerm} onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pl-10 pr-4 py-2.5 bg-white dark:bg-dark-input border border-neutral-200 dark:border-dark-border text-neutral-900 dark:text-neutral-100 placeholder:text-neutral-400 dark:placeholder:text-neutral-600 rounded-xl focus:outline-none focus:border-primary-500 dark:focus:border-cyan-500 focus:ring-4 focus:ring-primary-500/10 dark:focus:ring-cyan-500/10 transition-all shadow-sm dark:shadow-none" />
                    </div>
                    <button onClick={() => handleOpenModal()} className="bg-primary-600 dark:bg-cyan-600 text-white px-5 py-2.5 rounded-xl font-semibold shadow-sm shadow-primary-500/30 dark:shadow-cyan-500/20 hover:bg-primary-700 dark:hover:bg-cyan-700 hover:shadow-md transition-all flex items-center justify-center whitespace-nowrap gap-2">
                        <AddNewIcon className="w-5 h-5" /><span>Nuevo Usuario</span>
                    </button>
                </div>
            </div>

            <div className="bg-white dark:bg-dark-surface rounded-2xl shadow-sm dark:shadow-none border border-neutral-200 dark:border-dark-border overflow-hidden">
                <div className="overflow-x-auto">
                    <table className="w-full text-left border-collapse">
                        <thead>
                            <tr className="bg-neutral-50/50 dark:bg-dark-input/50 border-b border-neutral-200 dark:border-dark-border">
                                {['Usuario', 'Contacto', 'Rol', 'Estado', 'Acciones'].map(h => (
                                    <th key={h} className="px-6 py-4 text-xs font-bold text-neutral-500 dark:text-neutral-500 uppercase tracking-wider">{h}</th>
                                ))}
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-neutral-100 dark:divide-dark-border">
                            {filteredUsuarios.map((usuario) => (
                                <tr key={usuario.id} className="hover:bg-neutral-50/50 dark:hover:bg-dark-input/50 transition-colors">
                                    <td className="px-6 py-4">
                                        <div className="flex items-center gap-3">
                                            <div className="w-10 h-10 rounded-full bg-neutral-100 dark:bg-dark-input text-primary-600 dark:text-cyan-400 flex items-center justify-center font-bold text-lg">
                                                {usuario.nombre ? usuario.nombre.charAt(0).toUpperCase() : 'U'}
                                            </div>
                                            <div className="font-bold text-neutral-900 dark:text-neutral-100">{usuario.nombre}</div>
                                        </div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="text-sm font-medium text-neutral-600 dark:text-neutral-300 mb-0.5">{usuario.email}</div>
                                        <div className="text-xs text-neutral-500 dark:text-neutral-500 flex items-center">
                                            <PhoneIcon className="w-3.5 h-3.5 mr-1 opacity-70" />
                                            {usuario.telefono || 'N/A'}
                                        </div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <span className={`px-2.5 py-1 rounded-md text-xs font-bold border ${getRolColor(usuario.rol)}`}>{getRolText(usuario.rol)}</span>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="flex items-center gap-1.5">
                                            <div className={`w-2 h-2 rounded-full ${usuario.estado ? 'bg-emerald-500 shadow-[0_0_8px_rgba(16,185,129,0.5)]' : 'bg-rose-500 shadow-[0_0_8px_rgba(244,63,94,0.5)]'}`}></div>
                                            <span className={`text-sm font-semibold ${usuario.estado ? 'text-emerald-700 dark:text-emerald-400' : 'text-rose-700 dark:text-rose-400'}`}>
                                                {usuario.estado ? 'Activo' : 'Inactivo'}
                                            </span>
                                        </div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="flex items-center gap-1.5">
                                            <button onClick={() => handleOpenModal(usuario)} title="Editar"
                                                className="p-1.5 text-primary-600 dark:text-cyan-400 hover:bg-primary-50 dark:hover:bg-cyan-900/20 rounded-lg transition-colors border border-transparent hover:border-primary-100 dark:hover:border-cyan-800/30">
                                                <EditIcon className="w-4 h-4" />
                                            </button>
                                            <button onClick={() => handleDeletePermanently(usuario.id)} title="Eliminar"
                                                className="p-1.5 text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-lg transition-colors border border-transparent hover:border-red-100 dark:hover:border-red-800/30">
                                                <DeleteIcon className="w-4 h-4" />
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                    {filteredUsuarios.length === 0 && (
                        <div className="flex flex-col justify-center items-center py-16 text-neutral-500 dark:text-neutral-500">
                            <ShieldIcon className="w-12 h-12 mb-4 opacity-40" />
                            <span className="font-medium">No se encontraron usuarios.</span>
                        </div>
                    )}
                </div>
            </div>

            {showModal && (
                <div className="fixed inset-0 bg-neutral-900/40 dark:bg-black/60 backdrop-blur-sm flex items-center justify-center z-50 p-4 animate-fade-in">
                    <div className="bg-white dark:bg-dark-surface rounded-2xl shadow-xl dark:shadow-black/40 w-full max-w-lg max-h-[90vh] flex flex-col overflow-hidden border border-neutral-100 dark:border-dark-border">
                        <div className="p-6 border-b border-neutral-100 dark:border-dark-border flex justify-between items-center">
                            <h2 className="text-xl text-neutral-900 dark:text-neutral-100 font-bold tracking-tight">{editingUsuario ? 'Editar Usuario' : 'Nuevo Usuario'}</h2>
                            <button onClick={handleCloseModal} className="text-neutral-400 hover:text-neutral-600 dark:hover:text-neutral-300 bg-neutral-50 dark:bg-dark-input hover:bg-neutral-100 dark:hover:bg-dark-border rounded-lg p-1.5 transition-colors">
                                <CrossIcon className="w-4 h-4" />
                            </button>
                        </div>
                        <div className="p-6 overflow-y-auto">
                            <form id="usuarioForm" onSubmit={handleSubmit} className="space-y-5">
                                <div className="grid grid-cols-2 gap-4">
                                    <div><label className={labelCls}>Nombre <span className="text-red-500">*</span></label><input type="text" value={formData.nombre} onChange={(e) => setFormData({ ...formData, nombre: e.target.value })} className={inputCls} placeholder="Ej. Ana Gómez" required /></div>
                                    <div><label className={labelCls}>Email <span className="text-red-500">*</span></label><input type="email" value={formData.email} onChange={(e) => setFormData({ ...formData, email: e.target.value })} className={inputCls} placeholder="admin@ejemplo.com" required /></div>
                                </div>
                                <div className="grid grid-cols-2 gap-4">
                                    {!editingUsuario && (
                                        <div><label className={labelCls}>Contraseña <span className="text-red-500">*</span></label><input type="password" value={formData.contrasenaHash} onChange={(e) => setFormData({ ...formData, contrasenaHash: e.target.value })} className={`${inputCls} font-mono`} required /></div>
                                    )}
                                    <div><label className={labelCls}>Teléfono</label><input type="text" value={formData.telefono || ''} onChange={(e) => setFormData({ ...formData, telefono: e.target.value })} className={inputCls} placeholder="+57..." /></div>
                                </div>
                                <div className="grid grid-cols-2 gap-4">
                                    <div>
                                        <label className={labelCls}>Rol <span className="text-red-500">*</span></label>
                                        <select value={formData.rol} onChange={(e) => setFormData({ ...formData, rol: Number(e.target.value) })} className={`${inputCls} font-medium`} required>
                                            <option value={1}>Administrador</option>
                                            <option value={2}>Vendedor</option>
                                        </select>
                                    </div>
                                    <div className="flex flex-col justify-end pb-2">
                                        <label className="relative inline-flex items-center cursor-pointer group">
                                            <input type="checkbox" className="sr-only peer" checked={formData.estado} onChange={(e) => setFormData({ ...formData, estado: e.target.checked })} />
                                            <div className="w-11 h-6 bg-neutral-200 dark:bg-dark-border peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-primary-500/20 dark:peer-focus:ring-cyan-500/20 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-primary-600 dark:peer-checked:bg-cyan-500 transition-colors"></div>
                                            <span className="ml-3 text-sm font-semibold text-neutral-700 dark:text-neutral-300">Usuario Activo</span>
                                        </label>
                                    </div>
                                </div>
                            </form>
                        </div>
                        <div className="p-6 border-t border-neutral-100 dark:border-dark-border bg-neutral-50 dark:bg-dark-input rounded-b-2xl flex gap-3">
                            <button type="button" onClick={handleCloseModal} className="flex-1 bg-white dark:bg-dark-surface border border-neutral-200 dark:border-dark-border text-neutral-700 dark:text-neutral-300 py-2.5 rounded-xl font-semibold shadow-sm hover:bg-neutral-50 dark:hover:bg-dark-elevated transition-colors">Cancelar</button>
                            <button type="submit" form="usuarioForm" className="flex-1 bg-primary-600 dark:bg-cyan-600 text-white py-2.5 rounded-xl font-semibold shadow-sm hover:bg-primary-700 dark:hover:bg-cyan-700 transition-all">Guardar</button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default Usuarios;
