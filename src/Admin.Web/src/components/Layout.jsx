import { Link, Outlet, useLocation } from 'react-router-dom';
import { useState } from 'react';

function Icon({ name, className = 'w-5 h-5' }) {
    const commonProps = {
        className,
        viewBox: '0 0 24 24',
        fill: 'none',
        stroke: 'currentColor',
        strokeWidth: 1.8,
        strokeLinecap: 'round',
        strokeLinejoin: 'round',
        'aria-hidden': true,
    };

    switch (name) {
        case 'dashboard':
            return (
                <svg {...commonProps}>
                    <path d="M3 13h8V3H3zM13 21h8v-6h-8zM13 10h8V3h-8zM3 21h8v-4H3z" />
                </svg>
            );
        case 'inventario':
            return (
                <svg {...commonProps}>
                    <path d="M4 7 12 3l8 4-8 4-8-4Z" />
                    <path d="M4 7v10l8 4 8-4V7" />
                    <path d="M12 11v10" />
                </svg>
            );
        case 'pedidos':
            return (
                <svg {...commonProps}>
                    <circle cx="9" cy="20" r="1.5" />
                    <circle cx="17" cy="20" r="1.5" />
                    <path d="M5 5h2l2.2 9.5a1 1 0 0 0 1 .8h7.9a1 1 0 0 0 1-.8L21 8H8" />
                </svg>
            );
        case 'categorias':
            return (
                <svg {...commonProps}>
                    <path d="M20 10 10 20l-6-6L14 4h4l2 2v4Z" />
                    <circle cx="16.5" cy="7.5" r="1" />
                </svg>
            );
        case 'clientes':
            return (
                <svg {...commonProps}>
                    <path d="M16 21v-2a4 4 0 0 0-4-4H7a4 4 0 0 0-4 4v2" />
                    <circle cx="9.5" cy="7" r="4" />
                    <path d="M22 21v-2a4 4 0 0 0-3-3.87" />
                    <path d="M16 3.13a4 4 0 0 1 0 7.75" />
                </svg>
            );
        case 'usuarios':
            return (
                <svg {...commonProps}>
                    <path d="M15 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" />
                    <circle cx="8" cy="7" r="4" />
                    <path d="M20 8v6" />
                    <path d="M23 11h-6" />
                </svg>
            );
        case 'pagos':
            return (
                <svg {...commonProps}>
                    <path d="M12 2v20" />
                    <path d="M17 5H9.5a3.5 3.5 0 0 0 0 7H14.5a3.5 3.5 0 0 1 0 7H6" />
                </svg>
            );
        case 'conversaciones':
            return (
                <svg {...commonProps}>
                    <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z" />
                </svg>
            );
        case 'mensajes':
            return (
                <svg {...commonProps}>
                    <rect x="3" y="5" width="18" height="14" rx="2" />
                    <path d="m3 7 9 6 9-6" />
                </svg>
            );
        case 'notification':
            return (
                <svg {...commonProps}>
                    <path d="M15 17h5l-1.4-1.4A2 2 0 0 1 18 14.2V11a6 6 0 1 0-12 0v3.2a2 2 0 0 1-.6 1.4L4 17h5" />
                    <path d="M10 17a2 2 0 0 0 4 0" />
                </svg>
            );
        case 'logout':
            return (
                <svg {...commonProps}>
                    <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" />
                    <path d="M16 17l5-5-5-5" />
                    <path d="M21 12H9" />
                </svg>
            );
        case 'panel':
            return (
                <svg {...commonProps}>
                    <path d="M4 19h16" />
                    <path d="M5 19V9l7-5 7 5v10" />
                    <path d="M9 19v-6h6v6" />
                </svg>
            );
        case 'close':
            return (
                <svg {...commonProps}>
                    <path d="M18 6 6 18" />
                    <path d="m6 6 12 12" />
                </svg>
            );
        case 'menu':
            return (
                <svg {...commonProps}>
                    <path d="M4 6h16M4 12h16M4 18h16" />
                </svg>
            );
        case 'success':
            return (
                <svg {...commonProps}>
                    <circle cx="12" cy="12" r="9" />
                    <path d="m8.5 12 2.5 2.5 4.5-5" />
                </svg>
            );
        case 'warning':
            return (
                <svg {...commonProps}>
                    <path d="M12 3 2.8 19h18.4L12 3Z" />
                    <path d="M12 9v4" />
                    <path d="M12 17h.01" />
                </svg>
            );
        case 'error':
            return (
                <svg {...commonProps}>
                    <circle cx="12" cy="12" r="9" />
                    <path d="m15 9-6 6" />
                    <path d="m9 9 6 6" />
                </svg>
            );
        case 'info':
        default:
            return (
                <svg {...commonProps}>
                    <circle cx="12" cy="12" r="9" />
                    <path d="M12 10v5" />
                    <path d="M12 7h.01" />
                </svg>
            );
    }
}

function Layout({ onLogout, notifications = [], unreadCount = 0, onOpenNotifications, userRole, allowedRoutes = [] }) {
    const location = useLocation();
    const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
    const [isNotificationPanelOpen, setIsNotificationPanelOpen] = useState(false);

    const navItems = [
        { path: '/', label: 'Dashboard', icon: 'dashboard' },
        { path: '/inventario', label: 'Inventario', icon: 'inventario' },
        { path: '/pedidos', label: 'Pedidos', icon: 'pedidos' },
        { path: '/categorias', label: 'Categorias', icon: 'categorias' },
        { path: '/clientes', label: 'Clientes', icon: 'clientes' },
        { path: '/usuarios', label: 'Usuarios', icon: 'usuarios' },
        { path: '/pagos', label: 'Pagos', icon: 'pagos' },
        { path: '/conversaciones', label: 'Conversaciones', icon: 'conversaciones' },
        { path: '/mensajes', label: 'Mensajes', icon: 'mensajes' }
    ].filter((item) => allowedRoutes.includes(item.path));

    const isActive = (path) => location.pathname === path;
    const recentNotifications = notifications.slice(0, 6);

    const toggleNotificationPanel = () => {
        const nextValue = !isNotificationPanelOpen;
        setIsNotificationPanelOpen(nextValue);

        if (nextValue) {
            onOpenNotifications?.();
        }
    };

    return (
        <div className="flex flex-col md:flex-row h-screen bg-gray-100 overflow-hidden relative">
            <div className="md:hidden bg-white shadow-sm p-4 flex items-center justify-between z-10 shrink-0">
                <div className="flex items-center">
                    <span className="text-primary-600"><Icon name="panel" className="w-6 h-6" /></span>
                    <span className="ml-2 font-bold text-primary-600">Panel</span>
                </div>
                <div className="flex items-center gap-2">
                    <button
                        onClick={toggleNotificationPanel}
                        className="relative p-2 text-gray-500 hover:text-gray-700 bg-gray-100 rounded-lg"
                    >
                        <Icon name="notification" className="w-5 h-5" />
                        {unreadCount > 0 && (
                            <span className="absolute -top-1 -right-1 min-w-[20px] h-5 px-1 rounded-full bg-red-500 text-white text-[10px] font-bold flex items-center justify-center">
                                {unreadCount > 9 ? '9+' : unreadCount}
                            </span>
                        )}
                    </button>
                    <button
                        onClick={() => setIsMobileMenuOpen(true)}
                        className="p-2 text-gray-500 hover:text-gray-700 bg-gray-100 rounded-lg"
                    >
                        <Icon name="menu" className="w-6 h-6" />
                    </button>
                </div>
            </div>

            {isMobileMenuOpen && (
                <div
                    className="md:hidden fixed inset-0 bg-black/50 z-20"
                    onClick={() => setIsMobileMenuOpen(false)}
                />
            )}

            <aside className={`
                fixed md:static inset-y-0 left-0
                transform ${isMobileMenuOpen ? 'translate-x-0' : '-translate-x-full'} md:translate-x-0
                w-64 bg-white shadow-lg transition-transform duration-300 ease-in-out flex flex-col z-30
            `}>
                <div className="p-4 md:p-6 flex items-center justify-between md:justify-start shrink-0">
                    <div className="flex flex-col">
                        <h1 className="text-xl md:text-2xl font-bold text-primary-600 flex items-center">
                            <Icon name="panel" className="w-6 h-6" />
                            <span className="ml-2 whitespace-nowrap">Admin Panel</span>
                        </h1>
                        <p className="text-sm text-gray-500 mt-1 whitespace-nowrap">{userRole || 'Sin rol'}</p>
                    </div>
                    <button
                        className="md:hidden text-gray-500 hover:text-gray-700 p-2"
                        onClick={() => setIsMobileMenuOpen(false)}
                    >
                        <Icon name="close" className="w-5 h-5" />
                    </button>
                </div>

                <nav className="mt-2 md:mt-6 flex-1 overflow-y-auto overflow-x-hidden w-full">
                    {navItems.map((item) => (
                        <Link
                            key={item.path}
                            to={item.path}
                            onClick={() => setIsMobileMenuOpen(false)}
                            className={`flex items-center justify-start px-6 py-3 text-gray-700 hover:bg-primary-50 hover:text-primary-600 transition-colors ${isActive(item.path) ? 'bg-primary-100 text-primary-600 border-r-4 border-primary-600' : ''}`}
                        >
                            <span className="mr-3 text-gray-500">
                                <Icon name={item.icon} className="w-5 h-5" />
                            </span>
                            <span className="font-medium whitespace-nowrap">{item.label}</span>
                        </Link>
                    ))}
                </nav>

                <div className="p-4 md:p-6 shrink-0">
                    <button
                        onClick={onLogout}
                        title="Cerrar Sesion"
                        className="w-full flex items-center justify-center px-4 py-2 text-sm font-medium text-white bg-red-500 rounded-lg hover:bg-red-600 transition-colors"
                    >
                        <Icon name="logout" className="w-5 h-5 mr-2" />
                        <span className="whitespace-nowrap">Cerrar Sesion</span>
                    </button>
                </div>
            </aside>

            <main className="flex-1 overflow-y-auto">
                <div className="p-4 md:p-8">
                    <div className="flex items-center justify-between mb-4">
                        <div>
                            <p className="text-sm text-gray-500">Acceso actual</p>
                            <p className="font-semibold text-gray-800">{userRole || 'Sin rol'}</p>
                        </div>
                        <button
                            onClick={toggleNotificationPanel}
                            className="relative hidden md:flex items-center gap-2 px-4 py-2 bg-white rounded-xl shadow-sm border border-gray-200 text-gray-700 hover:bg-gray-50"
                        >
                            <Icon name="notification" className="w-5 h-5" />
                            <span>Notificaciones</span>
                            {unreadCount > 0 && (
                                <span className="min-w-[22px] h-[22px] px-1 rounded-full bg-red-500 text-white text-[11px] font-bold flex items-center justify-center">
                                    {unreadCount > 99 ? '99+' : unreadCount}
                                </span>
                            )}
                        </button>
                    </div>

                    {isNotificationPanelOpen && (
                        <div className="mb-6 bg-white rounded-2xl shadow-md border border-gray-100 overflow-hidden">
                            <div className="px-5 py-4 border-b border-gray-100 flex items-center justify-between">
                                <div>
                                    <h2 className="text-lg font-semibold text-gray-800">Centro de notificaciones</h2>
                                    <p className="text-sm text-gray-500">Siguen llegando aunque no estes dentro del dashboard.</p>
                                </div>
                                <button
                                    onClick={() => setIsNotificationPanelOpen(false)}
                                    className="text-gray-400 hover:text-gray-600"
                                >
                                    <Icon name="close" className="w-5 h-5" />
                                </button>
                            </div>

                            <div className="max-h-80 overflow-y-auto p-4 space-y-3">
                                {recentNotifications.length === 0 && (
                                    <p className="text-sm text-gray-400">Todavia no hay notificaciones.</p>
                                )}
                                {recentNotifications.map((notification) => (
                                    <div
                                        key={notification.id}
                                        className={`flex items-start gap-3 p-3 rounded-xl border border-gray-100 ${notification.color}`}
                                    >
                                        <span className="mt-0.5 text-gray-700">
                                            <Icon name={notification.icono} className="w-5 h-5" />
                                        </span>
                                        <div className="flex-1">
                                            <p className="text-sm font-semibold text-gray-800">{notification.titulo}</p>
                                            <p className="text-sm text-gray-600">{notification.mensaje}</p>
                                            <p className="text-xs text-gray-400 mt-1">{notification.fecha}</p>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>
                    )}

                    <Outlet />
                </div>
            </main>
        </div>
    );
}

export default Layout;
