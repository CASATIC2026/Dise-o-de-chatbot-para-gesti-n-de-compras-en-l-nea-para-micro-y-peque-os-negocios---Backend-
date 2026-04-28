import { useState, useEffect } from 'react';
import api from '../api/client';
import { SearchIcon, AddNewIcon, EditIcon, DeleteIcon, EyeIcon, UnavailableIcon, ImageIcon, AlertIcon, CrossIcon } from '../components/Icons';

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
        stock: 0,
        imagenUrl: '',
        activo: true
    });
    const [categorias, setCategorias] = useState([]);
    const [wasSubmitted, setWasSubmitted] = useState(false);

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
                stock: 0,
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
        setWasSubmitted(true);
        try {
            if (!formData.categoriaId) {
                return;
            }
            const dataToSave = {
                id: editingProduct ? Number(editingProduct.id) : Number(0),
                nombre: formData.nombre,
                descripcion: formData.descripcion,
                precio: Number(formData.precio),
                stock: Number(formData.stock),
                imagenUrl: formData.imagenUrl,
                activo: Boolean(formData.activo),
                categoriaId: Number(formData.categoriaId)
            };

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
        return (
            <div className="flex justify-center items-center h-64">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600 dark:border-cyan-500"></div>
            </div>
        );
    }
    const hasError = wasSubmitted && !formData.categoriaId;

    return (
        <div className="animate-fade-in">
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-8">
                <div>
                    <h1 className="text-3xl font-bold text-neutral-900 dark:text-neutral-100 tracking-tight">Inventario</h1>
                    <p className="text-neutral-500 dark:text-neutral-400 mt-2">Gestiona tu catálogo de productos con facilidad</p>
                </div>

                <div className="flex flex-col sm:flex-row w-full md:w-auto gap-4">
                    <div className="relative flex-1 sm:w-72">
                        <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-neutral-400" />
                        <input
                            type="text"
                            placeholder="Buscar productos..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pl-10 pr-4 py-2.5 bg-white dark:bg-dark-input border border-neutral-200 dark:border-dark-border text-neutral-900 dark:text-neutral-100 placeholder:text-neutral-400 dark:placeholder:text-neutral-600 rounded-xl focus:outline-none focus:border-primary-500 dark:focus:border-cyan-500 focus:ring-4 focus:ring-primary-500/10 dark:focus:ring-cyan-500/10 transition-all shadow-sm dark:shadow-none"
                        />
                    </div>
                    <button
                        onClick={() => handleOpenModal()}
                        className="bg-primary-600 dark:bg-cyan-600 text-white px-5 py-2.5 rounded-xl font-semibold shadow-sm shadow-primary-500/30 dark:shadow-cyan-500/20 hover:bg-primary-700 dark:hover:bg-cyan-700 hover:shadow-md transition-all flex items-center justify-center whitespace-nowrap gap-2"
                        title="Nuevo Producto"
                    >
                        <AddNewIcon className="w-5 h-5" />
                        <span>Nuevo Producto</span>
                    </button>
                </div>
            </div>

            <div className="bg-white dark:bg-dark-surface rounded-2xl shadow-sm dark:shadow-none border border-neutral-200 dark:border-dark-border overflow-hidden">
                <div className="overflow-x-auto">
                    <table className="w-full text-left border-collapse">
                        <thead>
                            <tr className="bg-neutral-50/50 dark:bg-dark-input/50 border-b border-neutral-200 dark:border-dark-border">
                                {['Producto', 'Precio', 'Stock', 'Categoría', 'Estado', 'Acciones'].map(h => (
                                    <th key={h} className="px-6 py-4 text-xs font-bold text-neutral-500 dark:text-neutral-500 uppercase tracking-wider">{h}</th>
                                ))}
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-neutral-100 dark:divide-dark-border">
                            {filteredProductos.map((producto) => (
                                <tr key={producto.id} className="hover:bg-neutral-50/50 dark:hover:bg-dark-input/50 transition-colors">
                                    <td className="px-6 py-4">
                                        <div className="flex items-center gap-3">
                                            {producto.imagenUrl ? (
                                                <img src={producto.imagenUrl} alt={producto.nombre} className="w-10 h-10 rounded-lg object-cover border border-neutral-200 dark:border-dark-border" />
                                            ) : (
                                                <div className="w-10 h-10 rounded-lg bg-neutral-100 dark:bg-dark-input flex items-center justify-center border border-neutral-200 dark:border-dark-border text-neutral-400">
                                                    <ImageIcon className="w-5 h-5 opacity-40" />
                                                </div>
                                            )}
                                            <div>
                                                <div className="font-semibold text-neutral-900 dark:text-neutral-100">{producto.nombre}</div>
                                                <div className="text-xs text-neutral-500 dark:text-neutral-500 font-medium truncate max-w-[200px]">{producto.descripcion}</div>
                                            </div>
                                        </div>
                                    </td>
                                    <td className="px-6 py-4 font-medium text-neutral-900 dark:text-neutral-100">
                                        ${producto.precio.toLocaleString('es-CO')}
                                    </td>
                                    <td className="px-6 py-4">
                                        <span className={`px-2.5 py-1 rounded-md text-xs font-bold border ${producto.stock < 10 ? 'bg-red-50 dark:bg-red-900/20 text-red-700 dark:text-red-400 border-red-200 dark:border-red-800/30' : 'bg-neutral-50 dark:bg-dark-input text-neutral-700 dark:text-neutral-400 border-neutral-200 dark:border-dark-border'}` }>
                                            {producto.stock} uds
                                        </span>
                                    </td>
                                    <td className="px-6 py-4">
                                        <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-secondary-50 dark:bg-indigo-900/20 text-secondary-700 dark:text-indigo-400 border border-secondary-100 dark:border-indigo-800/30">
                                            {producto.categoria.nombre}
                                        </span>
                                    </td>
                                    <td className="px-6 py-4">
                                        <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${producto.activo ? 'bg-green-50 dark:bg-green-900/20 text-green-700 dark:text-green-400 border-green-200 dark:border-green-800/30' : 'bg-neutral-100 dark:bg-dark-input text-neutral-600 dark:text-neutral-400 border-neutral-200 dark:border-dark-border'}` }>
                                            <span className={`w-1.5 h-1.5 rounded-full mr-1.5 ${producto.activo ? 'bg-green-500' : 'bg-neutral-400'}`}></span>
                                            {producto.activo ? 'Activo' : 'Inactivo'}
                                        </span>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="flex items-center gap-1.5">
                                            <button onClick={() => handleOpenImagePreview(producto.imagenUrl)} title="Ver Imagen"
                                                className="p-1.5 text-secondary-600 dark:text-indigo-400 hover:bg-secondary-50 dark:hover:bg-indigo-900/20 rounded-lg transition-colors border border-transparent hover:border-secondary-100 dark:hover:border-indigo-800/30">
                                                <EyeIcon className="w-4 h-4" />
                                            </button>
                                            <button onClick={() => handleOpenModal(producto)} title="Editar"
                                                className="p-1.5 text-primary-600 dark:text-cyan-400 hover:bg-primary-50 dark:hover:bg-cyan-900/20 rounded-lg transition-colors border border-transparent hover:border-primary-100 dark:hover:border-cyan-800/30">
                                                <EditIcon className="w-4 h-4" />
                                            </button>
                                            <button onClick={() => handleDelete(producto.id)} title="Desactivar"
                                                className="p-1.5 text-neutral-500 dark:text-neutral-400 hover:bg-neutral-100 dark:hover:bg-dark-input rounded-lg transition-colors border border-transparent hover:border-neutral-200 dark:hover:border-dark-border">
                                                <UnavailableIcon className="w-4 h-4" />
                                            </button>
                                            <button onClick={() => handleDeletePermanently(producto.id)} title="Eliminar"
                                                className="p-1.5 text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-lg transition-colors border border-transparent hover:border-red-100 dark:hover:border-red-800/30">
                                                <DeleteIcon className="w-4 h-4" />
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            </div>

            {/* Modal */}
            {showModal && (
                <div className="fixed inset-0 bg-neutral-900/40 dark:bg-black/60 backdrop-blur-sm flex items-center justify-center z-50 p-4 animate-fade-in">
                    <div className="bg-white dark:bg-dark-surface rounded-2xl shadow-xl dark:shadow-black/40 w-full max-w-md max-h-[90vh] flex flex-col overflow-hidden border border-neutral-100 dark:border-dark-border">
                        <div className="p-6 border-b border-neutral-100 dark:border-dark-border flex justify-between items-center">
                            <h2 className="text-xl text-neutral-900 dark:text-neutral-100 font-bold tracking-tight">
                                {editingProduct ? 'Editar Producto' : 'Nuevo Producto'}
                            </h2>
                            <button onClick={handleCloseModal} className="text-neutral-400 hover:text-neutral-600 dark:hover:text-neutral-300 bg-neutral-50 dark:bg-dark-input hover:bg-neutral-100 dark:hover:bg-dark-border rounded-lg p-1.5 transition-colors">
                                <CrossIcon className="w-4 h-4" />
                            </button>
                        </div>

                        <div className="p-6 overflow-y-auto">
                            <form id="productForm" onSubmit={handleSubmit} className="space-y-5">
                                <div>
                                    <label className="block text-sm font-semibold text-neutral-700 dark:text-neutral-300 mb-1.5">Categoría <span className="text-red-500">*</span></label>
                                    <select
                                        className={`w-full px-4 py-2.5 bg-neutral-50 dark:bg-dark-input border ${hasError ? 'border-red-300 dark:border-red-700 ring-1 ring-red-500/20' : 'border-neutral-200 dark:border-dark-border'} text-neutral-900 dark:text-neutral-100 rounded-xl focus:bg-white dark:focus:bg-dark-surface focus:outline-none focus:border-primary-500 dark:focus:border-cyan-500 focus:ring-4 focus:ring-primary-500/10 dark:focus:ring-cyan-500/10 transition-all`}
                                        value={formData.categoriaId || ''}
                                        onChange={(e) => setFormData({ ...formData, categoriaId: e.target.value })}
                                    >
                                        <option value="">Selecciona una categoría</option>
                                        {categorias.map((categoria) => (
                                            <option key={categoria.id} value={categoria.id}>{categoria.nombre}</option>
                                        ))}
                                    </select>
                                    {hasError && (
                                        <p className="mt-1.5 text-xs text-red-600 dark:text-red-400 font-medium flex items-center gap-1">
                                            <AlertIcon className="w-4 h-4" /> Debe seleccionar una categoría para continuar.
                                        </p>
                                    )}
                                </div>
                                <div>
                                    <label className="block text-sm font-semibold text-neutral-700 dark:text-neutral-300 mb-1.5">Nombre <span className="text-red-500">*</span></label>
                                    <input type="text" value={formData.nombre} onChange={(e) => setFormData({ ...formData, nombre: e.target.value })}
                                        className="w-full px-4 py-2.5 bg-neutral-50 dark:bg-dark-input border border-neutral-200 dark:border-dark-border text-neutral-900 dark:text-neutral-100 rounded-xl focus:bg-white dark:focus:bg-dark-surface focus:outline-none focus:border-primary-500 dark:focus:border-cyan-500 focus:ring-4 focus:ring-primary-500/10 dark:focus:ring-cyan-500/10 transition-all"
                                        placeholder="Ej. Teclado Mecánico" required />
                                </div>
                                <div>
                                    <label className="block text-sm font-semibold text-neutral-700 dark:text-neutral-300 mb-1.5">Descripción <span className="text-red-500">*</span></label>
                                    <textarea value={formData.descripcion} onChange={(e) => setFormData({ ...formData, descripcion: e.target.value })}
                                        className="w-full px-4 py-2.5 bg-neutral-50 dark:bg-dark-input border border-neutral-200 dark:border-dark-border text-neutral-900 dark:text-neutral-100 rounded-xl focus:bg-white dark:focus:bg-dark-surface focus:outline-none focus:border-primary-500 dark:focus:border-cyan-500 focus:ring-4 focus:ring-primary-500/10 dark:focus:ring-cyan-500/10 transition-all resize-none"
                                        rows="3" placeholder="Características del producto..." required />
                                </div>
                                <div className="grid grid-cols-2 gap-4">
                                    <div>
                                        <label className="block text-sm font-semibold text-neutral-700 dark:text-neutral-300 mb-1.5">Precio <span className="text-red-500">*</span></label>
                                        <div className="relative">
                                            <span className="absolute left-3 top-1/2 -translate-y-1/2 text-neutral-400 dark:text-neutral-600 font-medium">$</span>
                                            <input type="number" value={formData.precio}
                                                onKeyDown={(e) => ["e", "E", "+", "-"].includes(e.key) && e.preventDefault()}
                                                onChange={(e) => { const val = e.target.value; if (val === "" || /^\d*\.?\d{0,2}$/.test(val)) setFormData({ ...formData, precio: val }); }}
                                                className="w-full pl-8 pr-4 py-2.5 bg-neutral-50 dark:bg-dark-input border border-neutral-200 dark:border-dark-border text-neutral-900 dark:text-neutral-100 rounded-xl focus:bg-white dark:focus:bg-dark-surface focus:outline-none focus:border-primary-500 dark:focus:border-cyan-500 focus:ring-4 focus:ring-primary-500/10 dark:focus:ring-cyan-500/10 transition-all"
                                                placeholder="0.00" min="1" step="0.01" required />
                                        </div>
                                    </div>
                                    <div>
                                        <label className="block text-sm font-semibold text-neutral-700 dark:text-neutral-300 mb-1.5">Stock <span className="text-red-500">*</span></label>
                                        <input type="number" value={formData.stock}
                                            onKeyDown={(e) => ["e", "E", "+", "-", ",", "."].includes(e.key) && e.preventDefault()}
                                            onChange={(e) => { const val = e.target.value; if (val === "" || /^\d+$/.test(val)) setFormData({ ...formData, stock: val }); }}
                                            className="w-full px-4 py-2.5 bg-neutral-50 dark:bg-dark-input border border-neutral-200 dark:border-dark-border text-neutral-900 dark:text-neutral-100 rounded-xl focus:bg-white dark:focus:bg-dark-surface focus:outline-none focus:border-primary-500 dark:focus:border-cyan-500 focus:ring-4 focus:ring-primary-500/10 dark:focus:ring-cyan-500/10 transition-all"
                                            min="0" step="1" placeholder="0" required />
                                    </div>
                                </div>
                                <div>
                                    <label className="block text-sm font-semibold text-neutral-700 dark:text-neutral-300 mb-1.5">URL de Imagen</label>
                                    <input type="url" value={formData.imagenUrl} onChange={(e) => setFormData({ ...formData, imagenUrl: e.target.value })}
                                        className="w-full px-4 py-2.5 bg-neutral-50 dark:bg-dark-input border border-neutral-200 dark:border-dark-border text-neutral-900 dark:text-neutral-100 rounded-xl focus:bg-white dark:focus:bg-dark-surface focus:outline-none focus:border-primary-500 dark:focus:border-cyan-500 focus:ring-4 focus:ring-primary-500/10 dark:focus:ring-cyan-500/10 transition-all"
                                        placeholder="https://..." />
                                </div>
                                <div className="flex items-center bg-neutral-50 dark:bg-dark-input p-3 rounded-xl border border-neutral-200 dark:border-dark-border">
                                    <input type="checkbox" id="activoCheck" checked={formData.activo} onChange={(e) => setFormData({ ...formData, activo: e.target.checked })}
                                        className="w-5 h-5 text-primary-600 dark:text-cyan-500 border-neutral-300 dark:border-dark-border rounded focus:ring-primary-500 dark:focus:ring-cyan-500 focus:ring-2 accent-primary-600 dark:accent-cyan-500" />
                                    <label htmlFor="activoCheck" className="ml-3 text-sm font-semibold text-neutral-700 dark:text-neutral-300 select-none cursor-pointer">Producto activo (visible en tienda)</label>
                                </div>
                            </form>
                        </div>

                        <div className="p-6 border-t border-neutral-100 dark:border-dark-border bg-neutral-50 dark:bg-dark-input rounded-b-2xl flex gap-3">
                            <button type="button" onClick={handleCloseModal}
                                className="flex-1 bg-white dark:bg-dark-surface border border-neutral-200 dark:border-dark-border text-neutral-700 dark:text-neutral-300 py-2.5 rounded-xl font-semibold shadow-sm hover:bg-neutral-50 dark:hover:bg-dark-elevated transition-colors">Cancelar</button>
                            <button type="submit" form="productForm"
                                className="flex-1 bg-primary-600 dark:bg-cyan-600 text-white py-2.5 rounded-xl font-semibold shadow-sm hover:bg-primary-700 dark:hover:bg-cyan-700 transition-all">Guardar</button>
                        </div>
                    </div>
                </div>
            )}

            {/* Image Preview Modal */}
            {showImagePreview && (
                <div className="fixed inset-0 bg-neutral-900/80 dark:bg-black/90 backdrop-blur-sm flex items-center justify-center z-[60] p-4 animate-fade-in" onClick={handleCloseImagePreview}>
                    <div className="relative bg-white dark:bg-dark-surface rounded-2xl p-2 max-w-2xl w-full shadow-2xl dark:shadow-black/60 border border-neutral-100 dark:border-dark-border" onClick={e => e.stopPropagation()}>
                        <button className="absolute -top-12 right-0 text-white/70 hover:text-white transition-colors bg-white/10 hover:bg-white/20 rounded-full w-10 h-10 flex items-center justify-center" onClick={handleCloseImagePreview}>
                            <CrossIcon className="w-6 h-6" />
                        </button>
                        {previewImageUrl ? (
                            <img src={previewImageUrl} alt="Vista previa del producto" className="w-full h-auto max-h-[80vh] object-contain rounded-xl" />
                        ) : (
                            <div className="p-16 text-center text-neutral-500 dark:text-neutral-500 flex flex-col items-center">
                                <ImageIcon className="w-12 h-12 mb-3 opacity-30" />
                                <span className="font-medium">No hay imagen disponible para este producto</span>
                            </div>
                        )}
                    </div>
                </div>
            )}
        </div>
    );
}

export default Inventario;
