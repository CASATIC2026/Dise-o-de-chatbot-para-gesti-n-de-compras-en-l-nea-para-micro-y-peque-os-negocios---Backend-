import { useState, useEffect } from 'react';
import api from '../api/client';

function Categoria() {
    const [categorias, setCategorias] = useState([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [showModal, setShowModal] = useState(false);
    const [editingCategoria, setEditingCategoria] = useState(null);
    const [formData, setFormData] = useState({
        nombre: '',
        descripcion: ''
    });

    useEffect(() => {
        fetchCategorias();
    }, []);

    const fetchCategorias = async () => {
        try {
            const response = await api.get('/admin/inventario/categorias');
            setCategorias(response.data);
        } catch (error) {
            console.error('Error fetching categorias:', error);
        } finally {
            setLoading(false);
        }
    };

    const handleOpenModal = (categoria = null) => {
        if (categoria) {
            setEditingCategoria(categoria);
            setFormData(categoria);
        } else {
            setEditingCategoria(null);
            setFormData({
                nombre: '',
                descripcion: '',
            });
        }
        setShowModal(true);
    };

    const handleCloseModal = () => {
        setShowModal(false);
        setEditingCategoria(null);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        try {
            const dataToSave = {
                id: editingCategoria ? Number(editingCategoria.id) : 0,
                nombre: formData.nombre,
                descripcion: formData.descripcion,
            };

            if (editingCategoria) {
                await api.put(`/admin/inventario/categorias/${editingCategoria.id}`, dataToSave);
                alert("¡Categoria actualizada!");
            } else {
                await api.post('/admin/inventario/categorias', dataToSave);
                alert("¡Categoria agregada!");
            }

            fetchCategorias();
            handleCloseModal();
        } catch (error) {
            console.error('Error saving categoria:', error);
            alert('Error al guardar/modificar el objeto');
        }
    };

    const handleDeletePermanently = async (id) => {
        if (!confirm('¿Estás seguro de eliminar esta categoría permanentemente?')) return;

        try {
            await api.delete(`/admin/inventario/categorias/${id}`);
            fetchCategorias();
        } catch (error) {
            console.error('Error deleting categoria:', error);
        }
    };

    const filteredCategorias = categorias.filter(cat =>
        cat.nombre.toLowerCase().includes(searchTerm.toLowerCase()) ||
        cat.descripcion.toLowerCase().includes(searchTerm.toLowerCase())
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
                    <h1 className="text-3xl font-bold text-neutral-900 tracking-tight">Categorías</h1>
                    <p className="text-neutral-500 mt-2">Gestiona las familias de tus productos</p>
                </div>

                <div className="flex flex-col sm:flex-row w-full md:w-auto gap-4">
                    <div className="relative flex-1 sm:w-72">
                        <span className="absolute left-3 top-1/2 -translate-y-1/2 text-neutral-400">🔍</span>
                        <input
                            type="text"
                            placeholder="Buscar categorías..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pl-10 pr-4 py-2.5 bg-white border border-neutral-200 rounded-xl focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all shadow-sm"
                        />
                    </div>
                    <button
                        onClick={() => handleOpenModal()}
                        className="bg-primary-600 text-white px-5 py-2.5 rounded-xl font-semibold shadow-sm shadow-primary-500/30 hover:bg-primary-700 hover:shadow-md hover:shadow-primary-500/40 transition-all flex items-center justify-center whitespace-nowrap gap-2"
                        title="Nueva Categoría"
                    >
                        <span className="text-lg">➕</span>
                        <span>Nueva Categoría</span>
                    </button>
                </div>
            </div>

            <div className="bg-white rounded-2xl shadow-sm border border-neutral-200 overflow-hidden">
                <div className="overflow-x-auto">
                    <table className="w-full text-left border-collapse">
                        <thead>
                            <tr className="bg-neutral-50/50 border-b border-neutral-200">
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 uppercase tracking-wider">Categoría</th>
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 uppercase tracking-wider">Descripción</th>
                                <th className="px-6 py-4 text-xs font-bold text-neutral-500 uppercase tracking-wider">Acciones</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-neutral-100">
                            {filteredCategorias.map((categoria) => (
                                <tr key={categoria.id} className="hover:bg-neutral-50/50 transition-colors">
                                    <td className="px-6 py-4">
                                        <div className="font-bold text-neutral-900">{categoria.nombre}</div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="text-sm font-medium text-neutral-600">{categoria.descripcion || <span className="text-neutral-400 italic">Sin descripción</span>}</div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="flex items-center gap-1.5">
                                            <button
                                                onClick={() => handleOpenModal(categoria)}
                                                className="p-1.5 text-primary-600 hover:bg-primary-50 rounded-lg transition-colors border border-transparent hover:border-primary-100"
                                                title="Editar"
                                            >
                                                ✏️
                                            </button>
                                            <button
                                                onClick={() => handleDeletePermanently(categoria.id)}
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

                    {filteredCategorias.length === 0 && (
                        <div className="flex flex-col justify-center items-center py-16 text-neutral-500">
                            <span className="text-5xl mb-4">🗂️</span>
                            <span className="font-medium">No se encontraron categorías.</span>
                        </div>
                    )}
                </div>
            </div>

            {/* Modal */}
            {showModal && (
                <div className="fixed inset-0 bg-neutral-900/40 backdrop-blur-sm flex items-center justify-center z-50 p-4 animate-fade-in">
                    <div className="bg-white rounded-2xl shadow-xl w-full max-w-md max-h-[90vh] flex flex-col overflow-hidden">
                        <div className="p-6 border-b border-neutral-100 flex justify-between items-center">
                            <h2 className="text-xl text-neutral-900 font-bold tracking-tight">
                                {editingCategoria ? 'Editar Categoría' : 'Nueva Categoría'}
                            </h2>
                            <button onClick={handleCloseModal} className="text-neutral-400 hover:text-neutral-600 bg-neutral-50 hover:bg-neutral-100 rounded-lg p-1.5 transition-colors">
                                ✕
                            </button>
                        </div>

                        <div className="p-6 overflow-y-auto">
                            <form id="categoriaForm" onSubmit={handleSubmit} className="space-y-5">
                                <div>
                                    <label className="block text-sm font-semibold text-neutral-700 mb-1.5">Nombre <span className="text-red-500">*</span></label>
                                    <input
                                        type="text"
                                        value={formData.nombre}
                                        onChange={(e) => setFormData({ ...formData, nombre: e.target.value })}
                                        className="w-full px-4 py-2.5 bg-neutral-50 border border-neutral-200 rounded-xl focus:bg-white focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all"
                                        placeholder="Ej. Electrónica"
                                        required
                                    />
                                </div>

                                <div>
                                    <label className="block text-sm font-semibold text-neutral-700 mb-1.5">Descripción <span className="text-red-500">*</span></label>
                                    <textarea
                                        value={formData.descripcion}
                                        onChange={(e) => setFormData({ ...formData, descripcion: e.target.value })}
                                        className="w-full px-4 py-2.5 bg-neutral-50 border border-neutral-200 rounded-xl focus:bg-white focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 transition-all resize-none"
                                        rows="3"
                                        placeholder="Descripción de la categoría..."
                                        required
                                    />
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
                                form="categoriaForm"
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

export default Categoria;
