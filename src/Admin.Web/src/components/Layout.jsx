import { Link, Outlet, useLocation } from 'react-router-dom';
import { useState } from 'react';
import {
    DashboardIcon, InventoryIcon, OrdersIcon, CategoriesIcon,
    ClientsIcon, UsersIcon, PaymentsIcon, ConversationsIcon,
    MessagesIcon, LogoutIcon, MenuIcon, CloseIcon,
    ChatlyIcon, SunIcon, MoonIcon, CheckCircleIcon, AlertIcon
} from './Icons';
import { getNotifColor, getNotifIcon, getNotifTextColor } from '../utils/notifications';

function Layout({ onLogout, notifications = [], unreadCount = 0, onOpenNotifications, isDark, toggleDark }) {
    const location = useLocation();
    const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
    const [isNotificationPanelOpen, setIsNotificationPanelOpen] = useState(false);
    const toggleNotificationPanel = () => {
        const nextValue = !isNotificationPanelOpen;
        setIsNotificationPanelOpen(nextValue);
        if (nextValue) onOpenNotifications?.();
    };

    const navItems = [
        { path: '/',              label: 'Dashboard',      Icon: DashboardIcon },
        { path: '/inventario',    label: 'Inventario',     Icon: InventoryIcon },
        { path: '/categorias',    label: 'Categorías',     Icon: CategoriesIcon },
        { path: '/clientes',      label: 'Clientes',       Icon: ClientsIcon },
        { path: '/usuarios',      label: 'Usuarios',       Icon: UsersIcon },
        { path: '/pedidos',       label: 'Pedidos',        Icon: OrdersIcon },
        { path: '/pagos',         label: 'Pagos',          Icon: PaymentsIcon },
        { path: '/conversaciones',label: 'Conversaciones', Icon: ConversationsIcon },
        { path: '/mensajes',      label: 'Mensajes',       Icon: MessagesIcon },
    ];

    const isActive = (path) => location.pathname === path;
    const recentNotifications = notifications.slice(0, 6);

    return (
        <div className="flex flex-col md:flex-row h-screen bg-gray-100 dark:bg-gray-900 overflow-hidden relative transition-colors duration-300">

            {/* ── Mobile Header ──────────────────────────────────── */}
            <div className="md:hidden bg-white dark:bg-gray-800 shadow-sm border-b border-gray-200 dark:border-gray-700 px-4 py-3 flex items-center justify-between z-10 shrink-0 transition-colors">
                {/* LEFT: hamburger */}
                <button
                    onClick={() => setIsMobileMenuOpen(true)}
                    className="p-2 text-gray-500 dark:text-gray-400 hover:text-primary-600 dark:hover:text-primary-400 bg-gray-100 dark:bg-gray-700 hover:bg-primary-50 dark:hover:bg-gray-600 rounded-xl transition-colors"
                    aria-label="Abrir menú"
                >
                    <MenuIcon className="w-6 h-6" />
                </button>

                {/* CENTER: brand */}
                <div className="flex items-center gap-2">
                    <ChatlyIcon className="w-7 h-7 text-primary-600 dark:text-primary-400" />
                    <span className="font-black text-lg text-gray-900 dark:text-gray-100 tracking-tighter">CHATLY</span>
                </div>

                {/* RIGHT: dark-mode toggle + notifications */}
                <div className="flex items-center gap-2">
                    <button
                        onClick={toggleDark}
                        className="p-2 rounded-xl text-gray-500 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
                        title={isDark ? 'Modo claro' : 'Modo oscuro'}
                    >
                        {isDark
                            ? <SunIcon className="w-5 h-5 text-amber-400" />
                            : <MoonIcon className="w-5 h-5 text-indigo-500" />
                        }
                    </button>
                    <button
                        onClick={toggleNotificationPanel}
                        className="relative p-2 text-gray-500 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-xl transition-colors"
                        aria-label="Notificaciones"
                    >
                        <MessagesIcon className="w-5 h-5" />
                        {unreadCount > 0 && (
                            <span className="absolute -top-1 -right-1 min-w-[18px] h-[18px] px-1 rounded-full bg-red-500 text-white text-[10px] font-bold flex items-center justify-center">
                                {unreadCount > 9 ? '9+' : unreadCount}
                            </span>
                        )}
                    </button>
                </div>
            </div>

            {/* ── Mobile Overlay ─────────────────────────────────── */}
            {isMobileMenuOpen && (
                <div
                    className="md:hidden fixed inset-0 bg-black/50 backdrop-blur-sm z-20 transition-opacity"
                    onClick={() => setIsMobileMenuOpen(false)}
                />
            )}

            {/* ── Sidebar ────────────────────────────────────────── */}
            <aside className={`
                fixed md:static inset-y-0 left-0
                transform ${isMobileMenuOpen ? 'translate-x-0' : '-translate-x-full'} md:translate-x-0
                w-72 bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700
                transition-all duration-300 ease-out flex flex-col z-30
            `}>
                {/* Logo */}
                <div className="p-6 flex items-center justify-between md:justify-start shrink-0 border-b border-gray-100 dark:border-gray-700">
                    <div className="flex flex-col">
                        <div className="flex items-center gap-3">
                            <ChatlyIcon className="w-9 h-9 text-primary-600 dark:text-primary-400" />
                            <span className="text-2xl font-black text-gray-900 dark:text-gray-100 tracking-tighter">CHATLY</span>
                        </div>
                        <p className="text-xs font-medium text-gray-400 dark:text-gray-500 mt-1 uppercase tracking-wider ml-12">Admin Panel</p>
                    </div>
                    <button
                        className="md:hidden text-gray-400 hover:text-gray-600 dark:text-gray-500 dark:hover:text-gray-300 p-2 bg-gray-50 dark:bg-gray-700 rounded-lg transition-colors"
                        onClick={() => setIsMobileMenuOpen(false)}
                        aria-label="Cerrar menú"
                    >
                        <CloseIcon className="w-4 h-4" />
                    </button>
                </div>

                {/* Nav */}
                <nav className="mt-4 flex-1 overflow-y-auto overflow-x-hidden w-full px-4 space-y-0.5">
                    {navItems.map(({ path, label, Icon }) => {
                        const active = isActive(path);
                        return (
                            <Link
                                key={path}
                                to={path}
                                onClick={() => setIsMobileMenuOpen(false)}
                                className={`flex items-center px-4 py-3 rounded-xl transition-all duration-200 group gap-3 ${
                                    active
                                        ? 'bg-primary-600 text-white shadow-md shadow-primary-500/20'
                                        : 'text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-700 hover:text-gray-900 dark:hover:text-gray-100'
                                }`}
                            >
                                <Icon className={`w-5 h-5 shrink-0 transition-transform duration-200 ${active ? 'scale-110' : 'group-hover:scale-110'}`} />
                                <span className="font-medium text-sm">{label}</span>
                            </Link>
                        );
                    })}
                </nav>

                {/* Footer */}
                <div className="p-4 shrink-0 border-t border-gray-100 dark:border-gray-700 space-y-2">
                    {/* Dark mode toggle (desktop) */}
                    <button
                        onClick={toggleDark}
                        className="hidden md:flex w-full items-center gap-3 px-4 py-2.5 rounded-xl text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors text-sm font-medium"
                        title={isDark ? 'Modo claro' : 'Modo oscuro'}
                    >
                        {isDark
                            ? <><SunIcon className="w-5 h-5 text-amber-400" /><span>Modo Claro</span></>
                            : <><MoonIcon className="w-5 h-5 text-indigo-500" /><span>Modo Oscuro</span></>
                        }
                    </button>

                    <button
                        onClick={onLogout}
                        className="w-full flex items-center gap-3 px-4 py-3 text-sm font-medium text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-900/20 hover:bg-red-100 dark:hover:bg-red-900/30 rounded-xl transition-colors"
                    >
                        <LogoutIcon className="w-5 h-5" />
                        <span>Cerrar Sesión</span>
                    </button>
                </div>
            </aside>

            {/* ── Main Content ──────────────────────────────────── */}
            <main className="flex-1 overflow-y-auto bg-gray-50 dark:bg-gray-900 transition-colors">
                <div className="p-4 md:p-8">
                    {/* Desktop: top bar with notifications button */}
                    <div className="hidden md:flex items-center justify-end mb-4">
                        <button
                            onClick={toggleNotificationPanel}
                            className="relative flex items-center gap-2 px-4 py-2 bg-white dark:bg-gray-800 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
                        >
                            <MessagesIcon className="w-5 h-5" />
                            <span>Notificaciones</span>
                            {unreadCount > 0 && (
                                <span className="min-w-[22px] h-[22px] px-1 rounded-full bg-red-500 text-white text-[11px] font-bold flex items-center justify-center">
                                    {unreadCount > 99 ? '99+' : unreadCount}
                                </span>
                            )}
                        </button>
                    </div>

                    {/* Notification Panel */}
                    {isNotificationPanelOpen && (
                        <div className="mb-6 bg-white dark:bg-gray-800 rounded-2xl shadow-md border border-gray-100 dark:border-gray-700 overflow-hidden transition-colors">
                            <div className="px-5 py-4 border-b border-gray-100 dark:border-gray-700 flex items-center justify-between">
                                <div>
                                    <h2 className="text-lg font-semibold text-gray-800 dark:text-gray-100">Centro de notificaciones</h2>
                                    <p className="text-sm text-gray-500 dark:text-gray-400">Siguen llegando aunque no estés dentro del dashboard.</p>
                                </div>
                                <button
                                    onClick={() => setIsNotificationPanelOpen(false)}
                                    className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 text-xl"
                                >
                                    <CloseIcon className="w-5 h-5" />
                                </button>
                            </div>
                            <div className="max-h-80 overflow-y-auto p-4 space-y-3">
                                {recentNotifications.length === 0 && (
                                    <p className="text-sm text-gray-400 dark:text-gray-500">Todavía no hay notificaciones.</p>
                                )}
                                {recentNotifications.map((notification) => {
                                    const NotifIcon = getNotifIcon(notification.icono);
                                    return (
                                        <div
                                            key={notification.id}
                                            className={`flex items-start gap-3 p-3 rounded-xl border ${getNotifColor(notification.color)}`}
                                        >
                                            <span className={`mt-0.5 shrink-0 ${getNotifTextColor(notification.color)}`}>
                                                <NotifIcon className="w-5 h-5" />
                                            </span>
                                            <div className="flex-1">
                                                <p className="text-sm font-semibold text-gray-800 dark:text-gray-100">{notification.titulo}</p>
                                                <p className="text-sm text-gray-600 dark:text-gray-400">{notification.mensaje}</p>
                                                <p className="text-xs text-gray-400 dark:text-gray-500 mt-1">{notification.fecha}</p>
                                            </div>
                                        </div>
                                    );
                                })}
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
