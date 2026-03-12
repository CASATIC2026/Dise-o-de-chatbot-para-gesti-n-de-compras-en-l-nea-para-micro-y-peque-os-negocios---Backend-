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
        return <div className="text-center py-12">Cargando categorías...</div>;
    }

    return (
        <div>
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-8">
                <div>
                    <h1 className="text-3xl font-bold text-gray-800">Categorías</h1>
                    <p className="text-gray-600 mt-2">Gestiona las familias de tus productos</p>
                </div>

                <div className="flex flex-col sm:flex-row w-full md:w-auto gap-4">
                    <div className="relative flex-1 sm:w-64">
                        <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400">🔍</span>
                        <input
                            type="text"
                            placeholder="Buscar categorías..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pl-10 pr-4 py-2 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 transition-all"
                        />
                    </div>
                    <button
                        onClick={() => handleOpenModal()}
                        className="bg-primary-600 text-white p-3 md:px-6 md:py-3 rounded-lg font-medium hover:bg-primary-700 transition-colors flex items-center justify-center whitespace-nowrap"
                        title="Nueva Categoria"
                    >
                        <span className="text-xl md:mr-2">➕</span>
                        <span className="hidden md:inline">Nueva Categoria</span>
                    </button>
                </div>
            </div>

            <div className="bg-white rounded-xl shadow-md overflow-x-auto">
                <table className="w-full">
                    <thead className="bg-gray-50 border-b">
                        <tr>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Categoria</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Descripcion</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Acciones</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-200">
                        {filteredCategorias.map((categoria) => (
                            <tr key={categoria.id} className="hover:bg-gray-50">
                                <td className="px-6 py-4">
                                    <div className="font-medium text-gray-900">{categoria.nombre}</div>
                                </td>
                                <td className="px-6 py-4 text-gray-900">
                                    <div className="text-sm text-gray-500">{categoria.descripcion}</div>
                                </td>
                                <td className="px-6 py-4">
                                    <div className="flex space-x-2">
                                        <button
                                            onClick={() => handleOpenModal(categoria)}
                                            className="p-2 text-primary-600 hover:bg-primary-50 rounded-lg transition-colors"
                                            title="Editar"
                                        >
                                            ✏️
                                        </button>
                                        <button
                                            onClick={() => handleDeletePermanently(categoria.id)}
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

            {showModal && (
                <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
                    <div className="bg-white rounded-xl p-8 max-w-md w-full max-h-[90vh] overflow-y-auto">
                        <h2 className="text-2xl text-gray-700 font-bold mb-6">
                            {editingCategoria ? 'Editar Categoria' : 'Nueva Categoria'}
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
                                <label className="block text-sm font-medium text-gray-700 mb-1">Descripción</label>
                                <textarea
                                    value={formData.descripcion}
                                    onChange={(e) => setFormData({ ...formData, descripcion: e.target.value })}
                                    className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500"
                                    rows="3"
                                    required
                                />
                            </div>

                            <div className="flex space-x-3 pt-4">
                                <button
                                    type="submit"
                                    className="flex-1 bg-primary-600 text-white py-2 rounded-lg font-medium hover:bg-primary-700"
                                >
                                    Guardar
                                </button>
                                <button
                                    type="button"
                                    onClick={handleCloseModal}
                                    className="flex-1 bg-gray-200 text-gray-800 py-2 rounded-lg font-medium hover:bg-gray-300"
                                >
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

export default Categoria;
