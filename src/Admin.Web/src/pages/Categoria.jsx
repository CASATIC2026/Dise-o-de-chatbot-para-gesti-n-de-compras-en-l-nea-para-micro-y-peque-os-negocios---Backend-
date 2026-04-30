import { useState, useEffect } from 'react';
import api from '../api/client';
import { SearchIcon, AddNewIcon, EditIcon, DeleteIcon, CategoriesIcon, CrossIcon } from '../components/Icons';

function Categoria() {
    const [categorias, setCategorias] = useState([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [showModal, setShowModal] = useState(false);
    const [editingCategoria, setEditingCategoria] = useState(null);
    const [formData, setFormData] = useState({ nombre: '', descripcion: '' });

    useEffect(() => { fetchCategorias(); }, []);

    const fetchCategorias = async () => {
        try {
            const response = await api.get('/admin/inventario/categorias');
            setCategorias(response.data);
        } catch (error) {
            console.error('Error fetching categorias:', error);
        } finally { setLoading(false); }
    };

    const handleOpenModal = (categoria = null) => {
        if (categoria) { setEditingCategoria(categoria); setFormData(categoria); }
        else { setEditingCategoria(null); setFormData({ nombre: '', descripcion: '' }); }
        setShowModal(true);
    };

    const handleCloseModal = () => { setShowModal(false); setEditingCategoria(null); };

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            const dataToSave = { id: editingCategoria ? Number(editingCategoria.id) : 0, nombre: formData.nombre, descripcion: formData.descripcion };
            if (editingCategoria) { await api.put(`/admin/inventario/categorias/${editingCategoria.id}`, dataToSave); alert('¡Categoria actualizada!'); }
            else { await api.post('/admin/inventario/categorias', dataToSave); alert('¡Categoria agregada!'); }
            fetchCategorias(); handleCloseModal();
        } catch (error) { console.error('Error saving categoria:', error); alert('Error al guardar/modificar el objeto'); }
    };

    const handleDeletePermanently = async (id) => {
        if (!confirm('¿Estás seguro de eliminar esta categoría permanentemente?')) return;
        try { await api.delete(`/admin/inventario/categorias/${id}`); fetchCategorias(); }
        catch (error) { console.error('Error deleting categoria:', error); }
    };

    const filteredCategorias = categorias.filter(cat =>
        cat.nombre.toLowerCase().includes(searchTerm.toLowerCase()) ||
        cat.descripcion.toLowerCase().includes(searchTerm.toLowerCase())
    );

    if (loading) return (
        <div className="flex justify-center items-center h-64">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600 dark:border-cyan-500"></div>
        </div>
    );

    return (
        <div className="animate-fade-in">
            {/* Header */}
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-8">
                <div>
                    <h1 className="text-3xl font-bold text-neutral-900 dark:text-neutral-100 tracking-tight">Categorías</h1>
                    <p className="text-neutral-500 dark:text-neutral-400 mt-2">Gestiona las familias de tus productos</p>
                </div>
                <div className="flex flex-col sm:flex-row w-full md:w-auto gap-4">
                    <div className="relative flex-1 sm:w-72">
                        <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-neutral-400" />
                        <input type="text" placeholder="Buscar categorías..." value={searchTerm} onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pl-10 pr-4 py-2.5 bg-white dark:bg-dark-input border border-neutral-200 dark:border-dark-border text-neutral-900 dark:text-neutral-100 placeholder:text-neutral-400 dark:placeholder:text-neutral-600 rounded-xl focus:outline-none focus:border-primary-500 dark:focus:border-cyan-500 focus:ring-4 focus:ring-primary-500/10 dark:focus:ring-cyan-500/10 transition-all shadow-sm dark:shadow-none" />
                    </div>
                    <button onClick={() => handleOpenModal()}
                        className="bg-primary-600 dark:bg-cyan-600 text-white px-5 py-2.5 rounded-xl font-semibold shadow-sm shadow-primary-500/30 dark:shadow-cyan-500/20 hover:bg-primary-700 dark:hover:bg-cyan-700 hover:shadow-md transition-all flex items-center justify-center whitespace-nowrap gap-2">
                        <AddNewIcon className="w-5 h-5" /><span>Nueva Categoría</span>
                    </button>
                </div>
            </div>

            {/* Table */}
            <div className="bg-white dark:bg-dark-surface rounded-2xl shadow-sm dark:shadow-none border border-neutral-200 dark:border-dark-border overflow-hidden">
                <div className="overflow-x-auto">
                    <table className="w-full text-left border-collapse">
                        <thead>
                            <tr className="bg-neutral-50/50 dark:bg-dark-input/50 border-b border-neutral-200 dark:border-dark-border">
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 dark:text-neutral-500 uppercase tracking-wider">Categoría</th>
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 dark:text-neutral-500 uppercase tracking-wider">Descripción</th>
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 dark:text-neutral-500 uppercase tracking-wider">Acciones</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-neutral-100 dark:divide-dark-border">
                            {filteredCategorias.map((categoria) => (
                                <tr key={categoria.id} className="hover:bg-neutral-50/50 dark:hover:bg-dark-input/50 transition-colors">
                                    <td className="px-6 py-4">
                                        <div className="font-bold text-neutral-900 dark:text-neutral-100">{categoria.nombre}</div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="text-sm font-medium text-neutral-600 dark:text-neutral-400">{categoria.descripcion || <span className="text-neutral-400 dark:text-neutral-600 italic">Sin descripción</span>}</div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="flex items-center gap-1.5">
                                            <button onClick={() => handleOpenModal(categoria)} title="Editar"
                                                className="p-1.5 text-primary-600 dark:text-cyan-400 hover:bg-primary-50 dark:hover:bg-cyan-900/20 rounded-lg transition-colors border border-transparent hover:border-primary-100 dark:hover:border-cyan-800/30">
                                                <EditIcon className="w-4 h-4" />
                                            </button>
                                            <button onClick={() => handleDeletePermanently(categoria.id)} title="Eliminar"
                                                className="p-1.5 text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-lg transition-colors border border-transparent hover:border-red-100 dark:hover:border-red-800/30">
                                                <DeleteIcon className="w-4 h-4" />
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                    {filteredCategorias.length === 0 && (
                        <div className="flex flex-col justify-center items-center py-16 text-neutral-500 dark:text-neutral-500">
                            <CategoriesIcon className="w-12 h-12 mb-4 opacity-40" />
                            <span className="font-medium">No se encontraron categorías.</span>
                        </div>
                    )}
                </div>
            </div>

            {/* Modal */}
            {showModal && (
                <div className="fixed inset-0 bg-neutral-900/40 dark:bg-black/60 backdrop-blur-sm flex items-center justify-center z-50 p-4 animate-fade-in">
                    <div className="bg-white dark:bg-dark-surface rounded-2xl shadow-xl dark:shadow-black/40 w-full max-w-md max-h-[90vh] flex flex-col overflow-hidden border border-neutral-100 dark:border-dark-border">
                        <div className="p-6 border-b border-neutral-100 dark:border-dark-border flex justify-between items-center">
                            <h2 className="text-xl text-neutral-900 dark:text-neutral-100 font-bold tracking-tight">
                                {editingCategoria ? 'Editar Categoría' : 'Nueva Categoría'}
                            </h2>
                            <button onClick={handleCloseModal} className="text-neutral-400 hover:text-neutral-600 dark:hover:text-neutral-300 bg-neutral-50 dark:bg-dark-input hover:bg-neutral-100 dark:hover:bg-dark-border rounded-lg p-1.5 transition-colors"><CrossIcon className="w-4 h-4" /></button>
                        </div>
                        <div className="p-6 overflow-y-auto">
                            <form id="categoriaForm" onSubmit={handleSubmit} className="space-y-5">
                                <div>
                                    <label className="block text-sm font-semibold text-neutral-700 dark:text-neutral-300 mb-1.5">Nombre <span className="text-red-500">*</span></label>
                                    <input type="text" value={formData.nombre} onChange={(e) => setFormData({ ...formData, nombre: e.target.value })}
                                        className="w-full px-4 py-2.5 bg-neutral-50 dark:bg-dark-input border border-neutral-200 dark:border-dark-border text-neutral-900 dark:text-neutral-100 rounded-xl focus:bg-white dark:focus:bg-dark-surface focus:outline-none focus:border-primary-500 dark:focus:border-cyan-500 focus:ring-4 focus:ring-primary-500/10 dark:focus:ring-cyan-500/10 transition-all"
                                        placeholder="Ej. Electrónica" required />
                                </div>
                                <div>
                                    <label className="block text-sm font-semibold text-neutral-700 dark:text-neutral-300 mb-1.5">Descripción <span className="text-red-500">*</span></label>
                                    <textarea value={formData.descripcion} onChange={(e) => setFormData({ ...formData, descripcion: e.target.value })}
                                        className="w-full px-4 py-2.5 bg-neutral-50 dark:bg-dark-input border border-neutral-200 dark:border-dark-border text-neutral-900 dark:text-neutral-100 rounded-xl focus:bg-white dark:focus:bg-dark-surface focus:outline-none focus:border-primary-500 dark:focus:border-cyan-500 focus:ring-4 focus:ring-primary-500/10 dark:focus:ring-cyan-500/10 transition-all resize-none"
                                        rows="3" placeholder="Descripción de la categoría..." required />
                                </div>
                            </form>
                        </div>
                        <div className="p-6 border-t border-neutral-100 dark:border-dark-border bg-neutral-50 dark:bg-dark-input rounded-b-2xl flex gap-3">
                            <button type="button" onClick={handleCloseModal}
                                className="flex-1 bg-white dark:bg-dark-surface border border-neutral-200 dark:border-dark-border text-neutral-700 dark:text-neutral-300 py-2.5 rounded-xl font-semibold shadow-sm hover:bg-neutral-50 dark:hover:bg-dark-elevated transition-colors">
                                Cancelar
                            </button>
                            <button type="submit" form="categoriaForm"
                                className="flex-1 bg-primary-600 dark:bg-cyan-600 text-white py-2.5 rounded-xl font-semibold shadow-sm hover:bg-primary-700 dark:hover:bg-cyan-700 transition-all">
                                Guardar
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default Categoria;
