import { useState, useEffect } from 'react';
import api from '../api/client';

function Categoria() {
    const [categorias, setCategorias] = useState([]);
    const [loading, setLoading] = useState(true);
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
            //console.log('Categorias:', response.data);
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
            // CONSTRUYE UN OBJETO NUEVO SÓLO CON LOS DATOS NECESARIOS
            const dataToSave = {
                id: editingCategoria ? Number(editingCategoria.id) : Number(0), // Solo envía el ID si es edición
                nombre: formData.nombre,
                descripcion: formData.descripcion,

            };

            console.log("Enviando datos limpios:", dataToSave);

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
            console.error('Error completo objeto:', error);
            if (error.response) {
                // El servidor respondió con algo (400, 500, etc)
                console.log('Datos del servidor:', error.response.data);
            } else if (error.request) {
                // La petición se hizo pero no hubo respuesta (El servidor se cayó o CORS)
                console.log('No se recibió respuesta del servidor. Revisa si el microservicio de Inventario está caído.');
            } else {
                console.log('Error de configuración:', error.message);
            }
            alert('Error al guardar/modificar el objecto');
        }
    };
    const handleDeletePermanently = async (id) => {
        if (!confirm('¿Estás seguro de eliminar esta categoria?')) return;

        try {

            await api.delete(`/admin/inventario/categorias/${id}`);
            fetchCategorias();
        } catch (error) {
            console.error('Error deleting categoria:', error);
        }
    }


    if (loading) {
        return <div className="text-center py-12">Cargando productos...</div>;
    }

    return (
        <div>
            <div className="flex justify-between items-center mb-8">
                <div>
                    <h1 className="text-3xl font-bold text-gray-800">Inventario</h1>
                    <p className="text-gray-600 mt-2">Gestiona tu catálogo de categorias</p>
                </div>
                <button
                    onClick={() => handleOpenModal()}
                    className="bg-primary-600 text-white px-6 py-3 rounded-lg font-medium hover:bg-primary-700 transition-colors"
                >
                    + Nueva Categoria
                </button>
            </div>

            <div className="bg-white rounded-xl shadow-md overflow-hidden">
                <table className="w-full">
                    <thead className="bg-gray-50 border-b">
                        <tr>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Categoria</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Descripcion</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Acciones</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-200">
                        {categorias.map((categoria) => (
                            <tr key={categoria.id} className="hover:bg-gray-50">
                                <td className="px-6 py-4">
                                    <div>
                                        <div className="font-medium text-gray-900">{categoria.nombre}</div>
                                    </div>
                                </td>
                                <td className="px-6 py-4 text-gray-900">
                                    <div className="text-sm text-gray-500">{categoria.descripcion}</div>
                                </td>
                                <td className="px-6 py-4">
                                    <div className="flex space-x-2">
                                        <button
                                            onClick={() => handleOpenModal(categoria)}
                                            className="text-primary-600 hover:text-primary-800 font-medium"
                                        >
                                            Editar
                                        </button>
                                        <button
                                            onClick={() => handleDeletePermanently(categoria.id)}
                                            className="text-red-600 hover:text-red-800 font-medium"
                                        >
                                            Eliminar
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
