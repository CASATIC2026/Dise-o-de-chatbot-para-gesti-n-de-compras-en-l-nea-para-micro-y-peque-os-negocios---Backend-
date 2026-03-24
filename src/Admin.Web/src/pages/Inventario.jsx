import { useState, useEffect } from 'react';
import api from '../api/client';

function Inventario() {
    const [productos, setProductos] = useState([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [showModal, setShowModal] = useState(false);
    const [showImagePreview, setShowImagePreview] = useState(false);
    const [previewImageUrl, setPreviewImageUrl] = useState('');
    const [editingProduct, setEditingProduct] = useState(null);
    const [formData, setFormData] = useState({
        nombre: '',
        descripcion: '',
        precio: 0,
        stockTotal: 0,
        imagenUrl: '',
        activo: true

    });
    const [categorias, setCategorias] = useState([]);
    const [wasSubmitted, setWasSubmitted] = useState(false); // Nuevo estado para controlar si se ha intentado enviar el formulario

    useEffect(() => {
        fetchProductos();
        fetchCategorias();
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

    const handleOpenModal = (producto = null) => {
        if (producto) {
            setEditingProduct(producto);
            setFormData(producto);
        } else {
            setEditingProduct(null);
            setFormData({
                nombre: '',
                descripcion: '',
                precio: 0,
                stockTotal: 0,
                imagenUrl: '',
                activo: true

            });
        }
        setShowModal(true);
    };

    const handleCloseModal = () => {
        setShowModal(false);
        setEditingProduct(null);
    };

    const handleOpenImagePreview = (url) => {
        setPreviewImageUrl(url);
        setShowImagePreview(true);
    };

    const handleCloseImagePreview = () => {
        setShowImagePreview(false);
        setPreviewImageUrl('');
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setWasSubmitted(true); // Se activa la visualización de errores
        try {
            // VALIDACIÓN ADICIONAL: Asegurarse de que se ha seleccionado una categoría antes de enviar
            if (!formData.categoriaId) {
                return;
            }
            // CONSTRUYE UN OBJETO NUEVO SÓLO CON LOS DATOS NECESARIOS
            const dataToSave = {
                id: editingProduct ? Number(editingProduct.id) : Number(0), // Solo envía el ID si es edición
                nombre: formData.nombre,
                descripcion: formData.descripcion,
                precio: Number(formData.precio),
                stockTotal: Number(formData.stockTotal),
                imagenUrl: formData.imagenUrl,
                activo: Boolean(formData.activo),
                categoriaId: Number(formData.categoriaId)
            };

            console.log("Enviando datos limpios:", dataToSave);

            if (editingProduct) {
                await api.put(`/admin/inventario/productos/${editingProduct.id}`, dataToSave);
                alert("¡Producto actualizado!");
            } else {
                await api.post('/admin/inventario/productos', dataToSave);
                alert("¡Producto agregado!");
            }

            fetchProductos();
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
            alert('Error al guardar/modificar el producto' + error.message);
        }
    };
    const handleDeletePermanently = async (id) => {
        if (!confirm('¿Estás seguro de eliminar este producto?')) return;

        try {

            await api.delete(`/admin/inventario/productos/${id}`);
            fetchProductos();
        } catch (error) {
            console.error('Error deleting producto:', error);
        }
    }
    const handleDelete = async (id) => {
        if (!confirm('¿Estás seguro de desactivar este producto?')) return;

        try {

            await api.delete(`/admin/inventario/productos/soft-delete/${id}`);
            fetchProductos();
        } catch (error) {
            console.error('Error deleting producto:', error);
        }
    };

    const filteredProductos = productos.filter(producto =>
        producto.nombre.toLowerCase().includes(searchTerm.toLowerCase()) ||
        producto.descripcion.toLowerCase().includes(searchTerm.toLowerCase()) ||
        producto.categoriaId.toString().includes(searchTerm)
    );

    if (loading) {
        return <div className="text-center py-12">Cargando productos...</div>;
    }
    const hasError = wasSubmitted && !formData.categoriaId; // Solo muestra el error si se ha intentado enviar el formulario y no se ha seleccionado categoría
    return (
        <div>
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-8">
                <div>
                    <h1 className="text-3xl font-bold text-gray-800">Inventario</h1>
                    <p className="text-gray-600 mt-2">Gestiona tu catálogo de productos</p>
                </div>

                <div className="flex flex-col sm:flex-row w-full md:w-auto gap-4">
                    <div className="relative flex-1 sm:w-64">
                        <span className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400">🔍</span>
                        <input
                            type="text"
                            placeholder="Buscar productos..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pl-10 pr-4 py-2 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 transition-all"
                        />
                    </div>
                    <button
                        onClick={() => handleOpenModal()}
                        className="bg-primary-600 text-white p-3 md:px-6 md:py-3 rounded-lg font-medium hover:bg-primary-700 transition-colors flex items-center justify-center whitespace-nowrap"
                        title="Nuevo Producto"
                    >
                        <span className="text-xl md:mr-2">➕</span>
                        <span className="hidden md:inline">Nuevo Producto</span>
                    </button>
                </div>
            </div>

            <div className="bg-white rounded-xl shadow-md overflow-x-auto">
                <table className="w-full">
                    <thead className="bg-gray-50 border-b">
                        <tr>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Producto</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Precio</th>                            
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Disponible</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Reservado</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Total</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Categoria</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Estado</th>
                            <th className="px-6 py-4 text-left text-xs font-medium text-gray-500 uppercase">Acciones</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-200">
                        {filteredProductos.map((producto) => (
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
                                <td className="px-6 py-4 text-gray-900">
                                    {producto.stockDisponible}
                                </td>
                                <td className="px-6 py-4 text-gray-900">
                                    {producto.stockReservado}
                                </td>
                                <td className="px-6 py-4">
                                    <span className={`px-3 py-1 rounded-full text-sm font-medium ${producto.stockTotal < 10 ? 'bg-red-100 text-red-800' : 'bg-green-100 text-green-800'
                                        }`}>
                                        {producto.stockTotal}
                                    </span>                                    
                                </td>
                                <td className="px-6 py-4 text-gray-900">
                                    {producto.categoria.nombre}
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
                                            onClick={() => handleOpenImagePreview(producto.imagenUrl)}
                                            className="p-2 text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                                            title="Ver Imagen"
                                        >
                                            👁️
                                        </button>
                                        <button
                                            onClick={() => handleOpenModal(producto)}
                                            className="p-2 text-primary-600 hover:bg-primary-50 rounded-lg transition-colors"
                                            title="Editar"
                                        >
                                            ✏️
                                        </button>
                                        <button
                                            onClick={() => handleDelete(producto.id)}
                                            className="p-2 text-gray-500 hover:bg-gray-100 rounded-lg transition-colors"
                                            title="Desactivar"
                                        >
                                            🚫
                                        </button>
                                        <button
                                            onClick={() => handleDeletePermanently(producto.id)}
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

            {/* Modal */}
            {showModal && (
                <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
                    <div className="bg-white rounded-xl p-8 max-w-md w-full max-h-[90vh] overflow-y-auto">
                        <h2 className="text-2xl text-gray-700 font-bold mb-6">
                            {editingProduct ? 'Editar Producto' : 'Nuevo Producto'}
                        </h2>

                        <form onSubmit={handleSubmit} className="space-y-4">
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-1">Categoria</label>
                                <select className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500"
                                    value={formData.categoriaId || ''}
                                    onChange={(e) => setFormData({ ...formData, categoriaId: e.target.value })} >
                                    <option value="">Selecciona una categoria</option>
                                    {categorias.map((categoria) => (
                                        <option key={categoria.id} value={categoria.id}>
                                            {categoria.nombre}
                                        </option>
                                    ))}
                                </select>
                                {/* El downlabel solo aparece tras el clic en submit y si el valor es "" */}
                                {hasError && (
                                    <p className="mt-1 text-xs text-red-600 font-semibold flex items-center">
                                        <span className="mr-1">⚠️</span> Debe seleccionar una categoría para continuar.
                                    </p>
                                )}
                            </div>
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
                                        //onChange={(e) => setFormData({ ...formData, precio: e.target.value })}
                                        onKeyDown={(e) => ["e", "E", "+", "-"].includes(e.key) && e.preventDefault()}// Evita la entrada de caracteres no numéricos
                                        onChange={(e) => {
                                            const val = e.target.value;
                                            // Regex: Permite números enteros o con hasta 2 decimales
                                            // ^\d* : Empieza con 0 o más dígitos
                                            // (\.?\d{0,2}) : Opcionalmente un punto seguido de 0 a 2 dígitos
                                            if (val === "" || /^\d*\.?\d{0,2}$/.test(val)) {
                                                setFormData({ ...formData, precio: val });
                                                ///^\d*\.?\d{0,2}$/ test(val) -> true para: "123", "123.4", "123.45", "" (vacío)
                                            }// Si el valor no coincide con el formato, no se actualiza el estado
                                        }}
                                        className="w-full px-4 py-2 border rounded-lg focus:ring-2 focus:ring-primary-500"
                                        min="1"
                                        step="0.01"
                                        required
                                    />
                                </div>

                                <div>
                                    <label className="block text-sm font-medium text-gray-700 mb-1">Stock</label>
                                    <input
                                        type="number"
                                        value={formData.stockTotal}
                                        onKeyDown={(e) => ["e", "E", "+", "-", ",", "."].includes(e.key) && e.preventDefault()}// Evita la entrada de caracteres no numéricos
                                        onChange={(e) => {
                                            const val = e.target.value;
                                            // Permite solo números enteros positivos
                                            if (val === "" || /^\d+$/.test(val)) {
                                                setFormData({ ...formData, stockTotal: val });
                                            }
                                        }}
                                        min="0"
                                        step="1"
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
            {/* Image Preview Modal */}
            {showImagePreview && (
                <div
                    className="fixed inset-0 bg-black bg-opacity-75 flex items-center justify-center z-[60] p-4"
                    onClick={handleCloseImagePreview}
                >
                    <div className="relative bg-white rounded-xl p-2 max-w-2xl w-full">
                        <button
                            className="absolute -top-10 right-0 text-white text-3xl"
                            onClick={handleCloseImagePreview}
                        >
                            ×
                        </button>
                        {previewImageUrl ? (
                            <img
                                src={previewImageUrl}
                                alt="Vista previa del producto"
                                className="w-full h-auto max-h-[80vh] object-contain rounded-lg shadow-2xl"
                            />
                        ) : (
                            <div className="p-12 text-center text-gray-500">
                                No hay imagen disponible para este producto
                            </div>
                        )}
                    </div>
                </div>
            )}
        </div>
    );
}

export default Inventario;
