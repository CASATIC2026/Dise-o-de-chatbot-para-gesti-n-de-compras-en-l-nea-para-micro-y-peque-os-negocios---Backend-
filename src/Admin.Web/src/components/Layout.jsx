import { Link, useLocation } from 'react-router-dom';

function Layout({ children, onLogout }) {
    const location = useLocation();

    const navItems = [
        { path: '/', label: 'Dashboard', icon: '📊' },
        { path: '/inventario', label: 'Inventario', icon: '📦' },
        { path: '/pedidos', label: 'Pedidos', icon: '🛒' },
        { path: '/categorias', label: 'Categorias', icon: '🛒' },
    ];

    const isActive = (path) => location.pathname === path;

    return (
        <div className="flex h-screen bg-gray-100">
            {/* Sidebar */}
            <aside className="w-64 bg-white shadow-lg">
                <div className="p-6">
                    <h1 className="text-2xl font-bold text-primary-600">🤖 Admin Panel</h1>
                    <p className="text-sm text-gray-500">Chatbot E-commerce</p>
                </div>

                <nav className="mt-6">
                    {navItems.map((item) => (
                        <Link
                            key={item.path}
                            to={item.path}
                            className={`flex items-center px-6 py-3 text-gray-700 hover:bg-primary-50 hover:text-primary-600 transition-colors ${isActive(item.path) ? 'bg-primary-100 text-primary-600 border-r-4 border-primary-600' : ''
                                }`}
                        >
                            <span className="mr-3 text-xl">{item.icon}</span>
                            <span className="font-medium">{item.label}</span>
                        </Link>
                    ))}
                </nav>

                <div className="absolute bottom-0 w-64 p-6 border-t">
                    <button
                        onClick={onLogout}
                        className="w-full px-4 py-2 text-sm font-medium text-white bg-red-500 rounded-lg hover:bg-red-600 transition-colors"
                    >
                        🚪 Cerrar Sesión
                    </button>
                </div>
            </aside>

            {/* Main Content */}
            <main className="flex-1 overflow-y-auto">
                <div className="p-8">
                    {children}
                </div>
            </main>
        </div>
    );
}

export default Layout;
