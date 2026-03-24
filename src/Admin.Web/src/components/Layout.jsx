import { Link, useLocation } from 'react-router-dom';
import { useState } from 'react';

function Layout({ children, onLogout }) {
    const location = useLocation();
    const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

    const navItems = [
        { path: '/', label: 'Dashboard', icon: '📊' },
        { path: '/inventario', label: 'Inventario', icon: '📦' },
        { path: '/pedidos', label: 'Pedidos', icon: '🛒' },
        { path: '/categorias', label: 'Categorías', icon: '🏷️' },
        { path: '/clientes', label: 'Clientes', icon: '👥' },
        { path: '/usuarios', label: 'Usuarios', icon: '👤' },
        { path: '/pagos', label: 'Pagos', icon: '💲' },
        { path: '/conversaciones', label: 'Conversaciones', icon: '💬' },
        { path: '/mensajes', label: 'Mensajes', icon: '📨' }
    ];

    const isActive = (path) => location.pathname === path;

    return (
        <div className="flex flex-col md:flex-row h-screen bg-neutral-50 overflow-hidden relative font-sans text-neutral-800">
            {/* Mobile Header with Hamburger */}
            <div className="md:hidden bg-white border-b border-neutral-200 px-6 py-4 flex items-center justify-between z-10 shrink-0">
                <div className="flex items-center gap-3">
                    <img src="/src/resources/ChatlyIcon.svg" alt="Chatly" className="w-8 h-8" />
                    <span className="font-black text-lg text-neutral-900 tracking-tighter">CHATLY</span>
                </div>
                <button
                    onClick={() => setIsMobileMenuOpen(true)}
                    className="p-2 text-neutral-500 hover:text-primary-600 bg-neutral-100 hover:bg-primary-50 rounded-xl transition-colors"
                >
                    <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
                    </svg>
                </button>
            </div>

            {/* Overlay for mobile when sidebar is open */}
            {isMobileMenuOpen && (
                <div
                    className="md:hidden fixed inset-0 bg-neutral-900/40 backdrop-blur-sm z-20 transition-opacity"
                    onClick={() => setIsMobileMenuOpen(false)}
                />
            )}

            {/* Sidebar */}
            <aside className={`
                fixed md:static inset-y-0 left-0
                transform ${isMobileMenuOpen ? 'translate-x-0' : '-translate-x-full'} md:translate-x-0
                w-72 bg-white border-r border-neutral-200 transition-transform duration-300 ease-out flex flex-col z-30
            `}>
                <div className="p-6 flex items-center justify-between md:justify-start shrink-0 border-b border-neutral-100">
                    <div className="flex flex-col">
                        <h1 className="text-2xl font-black flex items-center gap-3 tracking-tighter">
                            <img src="/src/resources/ChatlyIcon.svg" alt="Chatly" className="w-9 h-9" />
                            <span className="text-neutral-900">CHATLY</span>
                        </h1>
                        <p className="text-xs font-medium text-neutral-400 mt-1 uppercase tracking-wider ml-12">Admin Panel</p>
                    </div>
                    {/* Close button for mobile inside sidebar */}
                    <button
                        className="md:hidden text-neutral-400 hover:text-neutral-600 p-2 bg-neutral-50 rounded-lg"
                        onClick={() => setIsMobileMenuOpen(false)}
                    >
                        ✕
                    </button>
                </div>

                <nav className="mt-6 flex-1 overflow-y-auto overflow-x-hidden w-full px-4 space-y-1">
                    {navItems.map((item) => {
                        const active = isActive(item.path);
                        return (
                            <Link
                                key={item.path}
                                to={item.path}
                                onClick={() => setIsMobileMenuOpen(false)}
                                className={`flex items-center px-4 py-3 rounded-xl transition-all duration-200 group ${active
                                    ? 'bg-primary-500 text-white shadow-md shadow-primary-500/20'
                                    : 'text-neutral-600 hover:bg-neutral-100 hover:text-neutral-900'
                                    }`}
                            >
                                <span className={`text-xl mr-3 transition-transform duration-200 ${active ? 'scale-110' : 'group-hover:scale-110'}`}>
                                    {item.icon}
                                </span>
                                <span className="font-medium">{item.label}</span>
                            </Link>
                        );
                    })}
                </nav>

                <div className="p-6 shrink-0 border-t border-neutral-100">
                    <button
                        onClick={onLogout}
                        title="Cerrar Sesión"
                        className="w-full flex items-center justify-center px-4 py-3 text-sm font-medium text-red-600 bg-red-50 hover:bg-red-100 rounded-xl transition-colors"
                    >
                        <span className="text-xl mr-2">🚪</span>
                        <span>Cerrar Sesión</span>
                    </button>
                </div>
            </aside>

            {/* Main Content */}
            <main className="flex-1 overflow-y-auto">
                <div className="p-6 md:p-10 max-w-7xl mx-auto">
                    {children}
                </div>
            </main>
        </div>
    );
}

export default Layout;
