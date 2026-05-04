import { useEffect, useState } from 'react';
import QRCode from 'qrcode';
import api from '../api/client';
import { SearchIcon, AddNewIcon, EditIcon, DeleteIcon, PaymentsIcon, CloseIcon } from '../components/Icons';

// Shared input class
const inputCls = "w-full px-4 py-2.5 bg-gray-50 dark:bg-gray-700 border border-gray-200 dark:border-gray-600 text-gray-900 dark:text-gray-100 rounded-xl placeholder:text-gray-400 dark:placeholder:text-gray-400 focus:bg-white dark:focus:bg-gray-600 focus:outline-none focus:border-primary-500 dark:focus:border-primary-400 focus:ring-4 focus:ring-primary-500/10 transition-all";
const labelCls = "block text-sm font-semibold text-gray-700 dark:text-gray-300 mb-1.5";

const QrIcon = ({ className = 'w-5 h-5', ...props }) => (
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className={className} aria-hidden="true" {...props}>
        <rect width="5" height="5" x="3" y="3" rx="1" />
        <rect width="5" height="5" x="16" y="3" rx="1" />
        <rect width="5" height="5" x="3" y="16" rx="1" />
        <path d="M16 16h.01" />
        <path d="M21 16h-2v3" />
        <path d="M16 21h3" />
        <path d="M21 21h.01" />
        <path d="M12 7v3" />
        <path d="M12 14h.01" />
        <path d="M7 12h3" />
        <path d="M14 12h1" />
    </svg>
);

const getErrorMessage = (error) => {
    const payload = error.response?.data;
    if (typeof payload === 'string') return payload;
    if (typeof payload?.error === 'string') return payload.error;
    if (typeof payload?.error?.message === 'string') return payload.error.message;
    if (typeof payload?.error?.mensaje === 'string') return payload.error.mensaje;
    return payload?.message || payload?.mensaje || error.message || 'No se pudo generar el QR de pago';
};

function Pagos() {
    const [pagos, setPagos] = useState([]);
    const [loading, setLoading] = useState(true);
    const [searchTerm, setSearchTerm] = useState('');
    const [showModal, setShowModal] = useState(false);
    const [editingPago, setEditingPago] = useState(null);
    const [formData, setFormData] = useState({ pedidoId: '', monto: '', metodoPago: '', estado: 1, referenciaTransaccion: '' });
    const [generatingQrId, setGeneratingQrId] = useState(null);
    const [qrModal, setQrModal] = useState(null);

    useEffect(() => { fetchPagos(); }, []);

    const fetchPagos = async () => {
        try {
            setLoading(true);
            const response = await api.get('/admin/pagos');
            setPagos(response.data);
        } catch (error) {
            console.error('Error fetching pagos:', error);
        } finally {
            setLoading(false);
        }
    };

    const handleOpenModal = (pago = null) => {
        if (pago) {
            setEditingPago(pago);
            setFormData({
                pedidoId: pago.pedidoId,
                monto: pago.monto,
                metodoPago: pago.metodoPago,
                estado: pago.estado,
                referenciaTransaccion: pago.referenciaTransaccion || ''
            });
        } else {
            setEditingPago(null);
            setFormData({
                pedidoId: '',
                monto: '',
                metodoPago: '',
                estado: 1,
                referenciaTransaccion: ''
            });
        }
        setShowModal(true);
    };

    const handleCloseModal = () => { setShowModal(false); setEditingPago(null); };

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            const dataToSave = {
                id: editingPago ? Number(editingPago.id) : 0,
                pedidoId: Number(formData.pedidoId),
                monto: Number(formData.monto),
                metodoPago: formData.metodoPago,
                estado: Number(formData.estado),
                referenciaTransaccion: formData.referenciaTransaccion
            };

            if (editingPago) {
                await api.put(`/admin/pagos/${editingPago.id}`, dataToSave);
            } else {
                await api.post('/admin/pagos', dataToSave);
            }

            await fetchPagos();
            handleCloseModal();
            alert(editingPago ? 'Pago actualizado correctamente' : 'Pago registrado correctamente');
        } catch (error) {
            console.error('Error al guardar:', error);
            alert('Error al procesar la solicitud');
        }
    };

    const handleDeletePermanently = async (id) => {
        if (!confirm('¿Estás seguro de eliminar este registro?')) return;
        try {
            await api.delete(`/admin/pagos/${id}`);
            await fetchPagos();
        } catch (error) {
            console.error('Error deleting pago:', error);
        }
    };

    const handleGenerateQr = async (pago) => {
        if (!pago?.pedidoId) {
            alert('Este pago no tiene un pedido asociado.');
            return;
        }

        try {
            setGeneratingQrId(pago.id);
            const response = await api.post(`/admin/pagos/crear-enlace-automatico/${pago.pedidoId}`);
            const paymentUrl = response.data?.url;

            if (!paymentUrl) {
                throw new Error('El servicio no devolvio un enlace de pago para generar el QR.');
            }

            const qrDataUrl = await QRCode.toDataURL(paymentUrl, {
                width: 320,
                margin: 2,
                color: {
                    dark: '#111827',
                    light: '#ffffff'
                }
            });

            setQrModal({
                pedidoId: pago.pedidoId,
                referencia: response.data?.referencia || pago.referenciaTransaccion || '',
                url: paymentUrl,
                qrDataUrl
            });
            await fetchPagos();
        } catch (error) {
            console.error('Error generando QR de pago:', error);
            alert(getErrorMessage(error));
        } finally {
            setGeneratingQrId(null);
        }
    };

    const getEstadoText = (s) => ({ 1: 'Pendiente', 2: 'Completado', 3: 'Rechazado', 4: 'Cancelado' }[s] || 'Desconocido');
const getEstadoColor = (s) => ({
        1: 'bg-amber-100 dark:bg-amber-900/20 text-amber-700 dark:text-amber-400 border-amber-200 dark:border-amber-800/30',
        2: 'bg-emerald-100 dark:bg-emerald-900/20 text-emerald-700 dark:text-emerald-400 border-emerald-200 dark:border-emerald-800/30',
        3: 'bg-rose-100 dark:bg-rose-900/20 text-rose-700 dark:text-rose-400 border-rose-200 dark:border-rose-800/30',
        4: 'bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-300 border-gray-200 dark:border-gray-600'
    }[s] || 'bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-300 border-gray-200 dark:border-gray-600');

    const filteredPagos = pagos.filter(pago =>
        (pago.referenciaTransaccion?.toLowerCase() || '').includes(searchTerm.toLowerCase()) ||
        pago.metodoPago.toLowerCase().includes(searchTerm.toLowerCase()) ||
        pago.pedidoId.toString().includes(searchTerm)
    );

    if (loading) return <div className="flex justify-center items-center h-64"><div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div></div>;

    return (
        <div className="animate-fade-in">
            <div className={qrModal ? 'blur-sm pointer-events-none select-none transition duration-200' : 'transition duration-200'}>
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-8">
                <div>
                    <h1 className="text-3xl font-bold text-gray-800 dark:text-gray-100 tracking-tight">Pagos</h1>
                    <p className="text-gray-500 dark:text-gray-400 mt-1">Control de transacciones y estados de pedidos</p>
                </div>
                <div className="flex flex-col sm:flex-row w-full md:w-auto gap-4">
                    <div className="relative flex-1 sm:w-72">
                        <SearchIcon className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
                        <input type="text" placeholder="Buscar pagos..." value={searchTerm} onChange={(e) => setSearchTerm(e.target.value)}
                            className="w-full pl-10 pr-4 py-2.5 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-600 text-gray-900 dark:text-gray-100 placeholder:text-gray-400 rounded-xl focus:outline-none focus:border-primary-500 dark:focus:border-primary-400 focus:ring-4 focus:ring-primary-500/10 transition-all shadow-sm" />
                    </div>
                    <button onClick={() => handleOpenModal()} className="bg-primary-600 text-white px-5 py-2.5 rounded-xl font-semibold shadow-sm shadow-primary-500/30 hover:bg-primary-700 hover:shadow-md transition-all flex items-center justify-center whitespace-nowrap gap-2">
                        <AddNewIcon className="w-5 h-5" /><span>Nuevo Pago</span>
                    </button>
                </div>
            </div>

            <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden">
                <div className="overflow-x-auto">
                    <table className="w-full min-w-[880px] text-left border-collapse">
                        <thead>
                            <tr className="bg-gray-50/50 dark:bg-gray-700 border-b border-gray-200 dark:border-gray-600">
                                {['Pedido', 'Monto', 'Método', 'Estado', 'Referencia', 'Acciones'].map(h => (
                                    <th key={h} className="px-6 py-4 text-xs font-bold text-gray-500 dark:text-gray-300 uppercase tracking-wider">{h}</th>
                                ))}
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-100 dark:divide-gray-700">
                            {filteredPagos.map((pago) => (
                                <tr key={pago.id} className="hover:bg-gray-50/50 dark:hover:bg-gray-700/50 transition-colors">
                                    <td className="px-6 py-4">
                                        <div className="font-bold text-gray-900 dark:text-gray-100 border border-gray-200 dark:border-gray-600 bg-white dark:bg-gray-700 rounded-md px-2 py-1 inline-block text-sm">#{pago.pedidoId}</div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="font-bold text-emerald-600">${pago.monto.toLocaleString('es-CO')}</div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="text-sm font-medium text-gray-700 dark:text-gray-200 flex items-center gap-1.5 border border-gray-200 dark:border-gray-600 bg-gray-50 dark:bg-gray-700 rounded-md px-2 py-1 w-max">
                                            <PaymentsIcon className="w-3.5 h-3.5" /> {pago.metodoPago}
                                        </div>
                                    </td>
                                    <td className="px-6 py-4">
                                        <span className={`px-2.5 py-1 rounded-md text-xs font-bold border ${getEstadoColor(pago.estado)}`}>
                                            {getEstadoText(pago.estado)}
                                        </span>
                                    </td>
                                    <td className="px-6 py-4 text-gray-500 dark:text-gray-300 text-sm font-medium font-mono">
                                        {pago.referenciaTransaccion || <span className="text-gray-300 dark:text-gray-500 italic">Sin Ref.</span>}
                                    </td>
                                    <td className="px-6 py-4">
                                        <div className="flex items-center gap-1.5">
                                            <button onClick={() => handleGenerateQr(pago)} title="Generar QR" disabled={generatingQrId === pago.id}
                                                className="p-1.5 text-emerald-600 hover:bg-emerald-50 disabled:opacity-50 disabled:cursor-wait rounded-lg transition-colors border border-transparent hover:border-emerald-100">
                                                {generatingQrId === pago.id ? (
                                                    <span className="block w-4 h-4 rounded-full border-2 border-emerald-200 border-t-emerald-600 animate-spin" />
                                                ) : (
                                                    <QrIcon className="w-4 h-4" />
                                                )}
                                            </button>
                                            <button onClick={() => handleOpenModal(pago)} title="Editar"
                                                className="p-1.5 text-primary-600 hover:bg-primary-50 rounded-lg transition-colors border border-transparent hover:border-primary-100">
                                                <EditIcon className="w-4 h-4" />
                                            </button>
                                            <button onClick={() => handleDeletePermanently(pago.id)} title="Eliminar"
                                                className="p-1.5 text-red-600 hover:bg-red-50 rounded-lg transition-colors border border-transparent hover:border-red-100">
                                                <DeleteIcon className="w-4 h-4" />
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                    {filteredPagos.length === 0 && (
                        <div className="flex flex-col justify-center items-center py-16 text-gray-500 dark:text-gray-400">
                            <PaymentsIcon className="w-12 h-12 mb-4 opacity-40" />
                            <span className="font-medium">No se encontraron pagos.</span>
                        </div>
                    )}
                </div>
            </div>

            {showModal && (
                <div className="fixed inset-0 bg-black/40 backdrop-blur-sm flex items-center justify-center z-50 p-4 animate-fade-in">
                    <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-xl w-full max-w-md max-h-[90vh] flex flex-col overflow-hidden border border-gray-100 dark:border-gray-700">
                        <div className="p-6 border-b border-gray-100 dark:border-gray-700 flex justify-between items-center">
                            <h2 className="text-xl text-gray-900 dark:text-gray-100 font-bold tracking-tight">{editingPago ? 'Editar Pago' : 'Nuevo Pago'}</h2>
                            <button onClick={handleCloseModal} className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 bg-gray-50 dark:bg-gray-700 hover:bg-gray-100 dark:hover:bg-gray-600 rounded-lg p-1.5 transition-colors"><CloseIcon className="w-4 h-4" /></button>
                        </div>
                        <div className="p-6 overflow-y-auto">
                            <form id="pagoForm" onSubmit={handleSubmit} className="space-y-5">
                                <div className="grid grid-cols-2 gap-4">
                                    <div>
                                        <label className={labelCls}>Pedido ID <span className="text-red-500">*</span></label>
                                        <input type="number" value={formData.pedidoId} onChange={(e) => setFormData({ ...formData, pedidoId: e.target.value })} className={inputCls} required />
                                    </div>
                                    <div>
                                        <label className={labelCls}>Monto <span className="text-red-500">*</span></label>
                                        <input type="number" step="0.01" value={formData.monto} onChange={(e) => setFormData({ ...formData, monto: e.target.value })} className={`${inputCls} font-mono`} required />
                                    </div>
                                </div>
                                <div className="grid grid-cols-2 gap-4">
                                    <div>
                                        <label className={labelCls}>Método de Pago <span className="text-red-500">*</span></label>
                                        <input type="text" value={formData.metodoPago || ''} onChange={(e) => setFormData({ ...formData, metodoPago: e.target.value })} className={inputCls} placeholder="Ej. Tarjeta" required />
                                    </div>
                                    <div>
                                        <label className={labelCls}>Estado <span className="text-red-500">*</span></label>
                                        <select value={formData.estado} onChange={(e) => setFormData({ ...formData, estado: Number(e.target.value) })} className={inputCls} required>
                                            <option value={1}>Pendiente</option>
                                            <option value={2}>Completado</option>
                                            <option value={3}>Rechazado</option>
                                            <option value={4}>Cancelado</option>
                                        </select>
                                    </div>
                                </div>
                                <div>
                                    <label className={labelCls}>Referencia Transacción</label>
                                    <input type="text" value={formData.referenciaTransaccion || ''} onChange={(e) => setFormData({ ...formData, referenciaTransaccion: e.target.value })} className={`${inputCls} font-mono text-sm`} placeholder="Ej. TX-123456789" />
                                </div>
                            </form>
                        </div>
                        <div className="p-6 border-t border-gray-100 dark:border-gray-700 bg-gray-50 dark:bg-gray-700 rounded-b-2xl flex gap-3">
                            <button type="button" onClick={handleCloseModal} className="flex-1 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-600 text-gray-700 dark:text-gray-200 py-2.5 rounded-xl font-semibold shadow-sm hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors">Cancelar</button>
                            <button type="submit" form="pagoForm" className="flex-1 bg-primary-600 text-white py-2.5 rounded-xl font-semibold shadow-sm hover:bg-primary-700 transition-all">Guardar</button>
                        </div>
                    </div>
                </div>
            )}
            </div>

            {qrModal && (
                <div className="fixed inset-0 bg-black/50 backdrop-blur-md flex items-center justify-center z-50 p-4 animate-fade-in">
                    <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-2xl w-full max-w-sm overflow-hidden border border-gray-100 dark:border-gray-700">
                        <div className="p-5 border-b border-gray-100 dark:border-gray-700 flex justify-between items-center">
                            <div>
                                <h2 className="text-lg text-gray-900 dark:text-gray-100 font-bold tracking-tight">QR de pago</h2>
                                <p className="text-sm text-gray-500 dark:text-gray-400 mt-0.5">Pedido #{qrModal.pedidoId}</p>
                            </div>
                            <button onClick={() => setQrModal(null)} className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 bg-gray-50 dark:bg-gray-700 hover:bg-gray-100 dark:hover:bg-gray-600 rounded-lg p-1.5 transition-colors">
                                <CloseIcon className="w-4 h-4" />
                            </button>
                        </div>
                        <div className="p-6 flex flex-col items-center gap-4">
                            <div className="bg-white p-3 rounded-xl border border-gray-200 shadow-sm">
                                <img src={qrModal.qrDataUrl} alt={`QR de pago para pedido ${qrModal.pedidoId}`} className="w-64 h-64" />
                            </div>
                            {qrModal.referencia && (
                                <div className="w-full text-center">
                                    <span className="text-xs font-semibold uppercase text-gray-400 dark:text-gray-500">Referencia</span>
                                    <p className="font-mono text-sm text-gray-700 dark:text-gray-200 break-all">{qrModal.referencia}</p>
                                </div>
                            )}
                            <a href={qrModal.url} target="_blank" rel="noreferrer" className="w-full bg-primary-600 text-white py-2.5 rounded-xl font-semibold shadow-sm hover:bg-primary-700 transition-all text-center">
                                Abrir enlace
                            </a>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default Pagos;
