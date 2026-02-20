import { useState, useEffect } from 'react';
import api from '../api/client';

function Inventario() {
    const [productos, setProductos] = useState([]);
    const [loading, setLoading] = useState(true);
    const [showModal, setShowModal] = useState(false);
    const [editingProduct, setEditingProduct] = useState(null);
    const [formData, setFormData] = useState({
        nombre: '',
        descripcion: '',
        precio: '',
        stock: '',
        imagenUrl: '',
        activo: true,
    });

    useEffect(() => {
        fetchProductos();
    }, []);

    const fetchProductos = async () => {
        try {
            const response = await api.get('/admin/inventario/productos?soloActivos=false');
            setProductos(response.data);
        } catch (error) {
            console.error('Error fetching productos:', error);
        } finally {
            setLoading(false);
        }
    };

    const handleOpenModal = (producto = null) => {
        if (producto) {
            setEditingProduct(producto);
            setFormData(producto);
        } else {
            setEditingProduct(null);
            setFormData({
                nombre: '',
                descripcion: '',
                precio: '',
                stock: '',
                imagenUrl: '',
                activo: true,
            });
        }
        setShowModal(true);
    };

    const handleCloseModal = () => {
        setShowModal(false);
        setEditingProduct(null);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        try {
            if (editingProduct) {
                await api.put(`/admin/inventario/productos/${editingProduct.id}`, {
                    ...formData,
                    id: editingProduct.id,
                });
            } else {
                await api.post('/admin/inventario/productos', formData);
            }

            fetchProductos();
            handleCloseModal();
        } catch (error) {
            console.error('Error saving producto:', error);
            alert('Error al guardar el producto');
        }
    };

    const handleDelete = async (id) => {
        if (!confirm('¿Estás seguro de desactivar este producto?')) return;

        try {
            await api.delete(`/admin/inventario/productos/${id}`);
            fetchProductos();
        } catch (error) {
            console.error('Error deleting producto:', error);
        }
    };

    if (loading) {
        return <div className="text-center py-12">Cargando productos...</div>;
    }

    return (
        <div>
            <div className="flex justify-between items-center mb-8">
                <div>
                    <h1 className="text-3xl font-bold text-gray-800">Inventario</h1>
                    <p className="text-gray-600 mt-2">Gestiona tu catálogo de productos</p>
                </div>
                <button
                    onClick={() => handleOpenModal()}
                    className="bg-primary-600 text-white px-6 py-3 rounded-lg font-medium hover:bg-primary-700 transition-colors"
                >
                    + Nuevo Producto
                </button>
            </div>

            <div className="bg-white rounded-xl shadow-md overflow-hidden">
                <table className="w-full">
                    <thead className="bg-gray-50 border-b">
                        <tr>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Producto</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Precio</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Stock</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Estado</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Acciones</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-200">
                        {productos.map((producto) => (
                            <tr key={producto.id} className="hover:bg-gray-50">
                                <td className="px-6 py-4">
                                    <div>
                                        <div className="font-medium text-gray-900">{producto.nombre}</div>
                                        <div className="text-sm text-gray-500">{producto.descripcion}</div>
                                    </div>
                                </td>
                                <td className="px-6 py-4 text-gray-900">
                                    ${producto.precio.toLocaleString('es-CO')}
                                </td>
                                <td className="px-6 py-4">
                                    <span className={`px-3 py-1 rounded-full text-sm font-medium ${producto.stock < 10 ? 'bg-red-100 text-red-800' : 'bg-green-100 text-green-800'
                                        }`}>
                                        {producto.stock} unidades
                                    </span>
                                </td>
                                <td className="px-6 py-4">
                                    <span className={`px-3 py-1 rounded-full text-sm font-medium ${producto.activo ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'
                                        }`}>
                                        {producto.activo ? 'Activo' : 'Inactivo'}
                                    </span>
                                </td>
                                <td className="px-6 py-4">
                                    <div className="flex space-x-2">
                                        <button
                                            onClick={() => handleOpenModal(producto)}
                                            className="text-primary-600 hover:text-primary-800 font-medium"
                                        >
                                            Editar
                                        </button>
                                        <button
                                            onClick={() => handleDelete(producto.id)}
                                            className="text-red-600 hover:text-red-800 font-medium"
                                        >
                                            Desactivar
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
                        <h2 className="text-2xl font-bold mb-6">
                            {editingProduct ? 'Editar Producto' : 'Nuevo Producto'}
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

                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">Precio</label>
                                    <input
                                        type="number"
                                        value={formData.precio}
                                        onChange={(e) => setFormData({ ...formData, precio: e.target.value })}
                                        className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500"
                                        required
                                    />
                                </div>

                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">Stock</label>
                                    <input
                                        type="number"
                                        value={formData.stock}
                                        onChange={(e) => setFormData({ ...formData, stock: e.target.value })}
                                        className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500"
                                        required
                                    />
                                </div>
                            </div>

                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">URL de Imagen</label>
                                <input
                                    type="url"
                                    value={formData.imagenUrl}
                                    onChange={(e) => setFormData({ ...formData, imagenUrl: e.target.value })}
                                    className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500"
                                    placeholder="https://..."
                                />
                            </div>

                            <div className="flex items-center">
                                <input
                                    type="checkbox"
                                    checked={formData.activo}
                                    onChange={(e) => setFormData({ ...formData, activo: e.target.checked })}
                                    className="w-4 h-4 text-primary-600"
                                />
                                <label className="ml-2 text-sm text-gray-700">Producto activo</label>
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

export default Inventario;
