import { Link, useLocation } from 'react-router-dom';
import { useState } from 'react';

function Layout({ children, onLogout }) {
    const location = useLocation();
    const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

    const navItems = [
        { path: '/', label: 'Dashboard', icon: '📊' },
        { path: '/inventario', label: 'Inventario', icon: '📦' },
        { path: '/pedidos', label: 'Pedidos', icon: '🛒' },
        { path: '/categorias', label: 'Categorias', icon: '🏷️' },
        { path: '/clientes', label: 'Clientes', icon: '👥' },
        { path: '/usuarios', label: 'Usuarios', icon: '👤' },
        { path: '/pagos', label: 'Pagos', icon: '💲' },
        { path: '/conversaciones', label: 'Conversaciones', icon: '💬' },
        { path: '/mensajes', label: 'Mensajes', icon: '📨' }
    ];

    const isActive = (path) => location.pathname === path;

    return (
        <div className="flex flex-col md:flex-row h-screen bg-gray-100 overflow-hidden relative">
            {/* Mobile Header with Hamburger */}
            <div className="md:hidden bg-white shadow-sm p-4 flex items-center justify-between z-10 shrink-0">
                <div className="flex items-center">
                    <span className="text-xl">🤖</span>
                    <span className="ml-2 font-bold text-primary-600">Admin Panel</span>
                </div>
                <button
                    onClick={() => setIsMobileMenuOpen(true)}
                    className="p-2 text-gray-500 hover:text-gray-700 bg-gray-100 rounded-lg"
                >
                    <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
                    </svg>
                </button>
            </div>

            {/* Overlay for mobile when sidebar is open */}
            {isMobileMenuOpen && (
                <div
                    className="md:hidden fixed inset-0 bg-black/50 z-20"
                    onClick={() => setIsMobileMenuOpen(false)}
                />
            )}

            {/* Sidebar */}
            <aside className={`
                fixed md:static inset-y-0 left-0
                transform ${isMobileMenuOpen ? 'translate-x-0' : '-translate-x-full'} md:translate-x-0
                w-64 bg-white shadow-lg transition-transform duration-300 ease-in-out flex flex-col z-30
            `}>
                <div className="p-4 md:p-6 flex items-center justify-between md:justify-start shrink-0">
                    <div className="flex flex-col">
                        <h1 className="text-xl md:text-2xl font-bold text-primary-600 flex items-center">
                            <span>🤖</span>
                            <span className="ml-2 whitespace-nowrap">Admin Panel</span>
                        </h1>
                        <p className="text-sm text-gray-500 mt-1 whitespace-nowrap">Chatbot E-commerce</p>
                    </div>
                    {/* Close button for mobile inside sidebar */}
                    <button
                        className="md:hidden text-gray-500 hover:text-gray-700 p-2"
                        onClick={() => setIsMobileMenuOpen(false)}
                    >
                        ✕
                    </button>
                </div>

                <nav className="mt-2 md:mt-6 flex-1 overflow-y-auto overflow-x-hidden w-full">
                    {navItems.map((item) => (
                        <Link
                            key={item.path}
                            to={item.path}
                            onClick={() => setIsMobileMenuOpen(false)}
                            className={`flex items-center justify-start px-6 py-3 text-gray-700 hover:bg-primary-50 hover:text-primary-600 transition-colors ${isActive(item.path) ? 'bg-primary-100 text-primary-600 border-r-4 border-primary-600' : ''
                                }`}
                        >
                            <span className="text-xl mr-3">{item.icon}</span>
                            <span className="font-medium whitespace-nowrap">{item.label}</span>
                        </Link>
                    ))}
                </nav>

                <div className="p-4 md:p-6 shrink-0">
                    <button
                        onClick={onLogout}
                        title="Cerrar Sesión"
                        className="w-full flex items-center justify-center px-4 py-2 text-sm font-medium text-white bg-red-500 rounded-lg hover:bg-red-600 transition-colors"
                    >
                        <span className="text-xl mr-2">🚪</span>
                        <span className="whitespace-nowrap">Cerrar Sesión</span>
                    </button>
                </div>
            </aside>

            {/* Main Content */}
            <main className="flex-1 overflow-y-auto">
                <div className="p-4 md:p-8">
                    {children}
                </div>
            </main>
        </div>
    );
}

export default Layout;
