import { Link, useLocation } from 'react-router-dom';
import { useState } from 'react';
import {
    DashboardIcon, InventoryIcon, OrdersIcon, CategoriesIcon,
    ClientsIcon, UsersIcon, PaymentsIcon, ConversationsIcon,
    MessagesIcon, LogoutIcon, SunIcon, MoonIcon, MenuIcon, CloseIcon,
    ChatlyIcon
} from './Icons';

function Layout({ children, onLogout, isDark, toggleDark }) {
    const location = useLocation();
    const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

    const navItems = [
        { path: '/',              label: 'Dashboard',      Icon: DashboardIcon },
        { path: '/inventario',    label: 'Inventario',     Icon: InventoryIcon },
        { path: '/pedidos',       label: 'Pedidos',        Icon: OrdersIcon },
        { path: '/categorias',    label: 'Categorías',     Icon: CategoriesIcon },
        { path: '/clientes',      label: 'Clientes',       Icon: ClientsIcon },
        { path: '/usuarios',      label: 'Usuarios',       Icon: UsersIcon },
        { path: '/pagos',         label: 'Pagos',          Icon: PaymentsIcon },
        { path: '/conversaciones',label: 'Conversaciones', Icon: ConversationsIcon },
        { path: '/mensajes',      label: 'Mensajes',       Icon: MessagesIcon },
    ];

    const isActive = (path) => location.pathname === path;

    return (
        <div className="flex flex-col md:flex-row h-screen bg-neutral-50 dark:bg-dark-base overflow-hidden relative font-sans text-neutral-800 dark:text-neutral-100 transition-colors duration-300">

            {/* ── Mobile Header ──────────────────────────────────── */}
            <div className="md:hidden bg-white dark:bg-dark-elevated border-b border-neutral-200 dark:border-dark-border px-6 py-4 flex items-center justify-between z-10 shrink-0 transition-colors">
                <div className="flex items-center gap-2">
                    <button
                        onClick={() => setIsMobileMenuOpen(true)}
                        className="p-2 text-neutral-500 dark:text-neutral-400 hover:text-primary-600 dark:hover:text-cyan-500 bg-neutral-100 dark:bg-dark-input hover:bg-primary-50 dark:hover:bg-dark-input rounded-xl transition-colors"
                    >
                        <MenuIcon className="w-6 h-6" />
                    </button>
                    {/* Dark mode toggle */}
                    <button
                        onClick={toggleDark}
                        className="p-2 rounded-xl text-neutral-500 dark:text-neutral-400 hover:bg-neutral-100 dark:hover:bg-dark-input transition-colors"
                        title={isDark ? 'Modo claro' : 'Modo oscuro'}
                    >
                        {isDark ? <SunIcon className="w-5 h-5" /> : <MoonIcon className="w-5 h-5" />}
                    </button>
                </div>
                <div className="flex items-center gap-3">
                    <ChatlyIcon className="w-8 h-8 text-primary-600 dark:text-cyan-500" />
                    <span className="font-black text-lg text-neutral-900 dark:text-neutral-100 tracking-tighter">CHATLY</span>
                </div>
            </div>

            {/* ── Mobile Overlay ─────────────────────────────────── */}
            {isMobileMenuOpen && (
                <div
                    className="md:hidden fixed inset-0 bg-neutral-900/50 dark:bg-black/70 backdrop-blur-sm z-20 transition-opacity"
                    onClick={() => setIsMobileMenuOpen(false)}
                />
            )}

            {/* ── Sidebar ────────────────────────────────────────── */}
            <aside className={`
                fixed md:static inset-y-0 left-0
                transform ${isMobileMenuOpen ? 'translate-x-0' : '-translate-x-full'} md:translate-x-0
                w-72 bg-white dark:bg-dark-elevated border-r border-neutral-200 dark:border-dark-border
                transition-all duration-300 ease-out flex flex-col z-30
            `}>
                {/* Logo */}
                <div className="p-6 flex items-center justify-between md:justify-start shrink-0 border-b border-neutral-100 dark:border-dark-border">
                    <div className="flex flex-col">
                        <div className="flex items-center gap-3">
                            <ChatlyIcon className="w-9 h-9 text-primary-600 dark:text-cyan-500" />
                            <span className="text-2xl font-black text-neutral-900 dark:text-neutral-100 tracking-tighter">CHATLY</span>
                        </div>
                        <p className="text-xs font-medium text-neutral-400 dark:text-neutral-600 mt-1 uppercase tracking-wider ml-12">Admin Panel</p>
                    </div>
                    <button
                        className="md:hidden text-neutral-400 dark:text-neutral-600 hover:text-neutral-600 dark:hover:text-neutral-400 p-2 bg-neutral-50 dark:bg-dark-input rounded-lg transition-colors"
                        onClick={() => setIsMobileMenuOpen(false)}
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
                                        ? 'bg-primary-500 dark:bg-cyan-500/20 text-white dark:text-cyan-400 shadow-md shadow-primary-500/20 dark:shadow-cyan-500/10'
                                        : 'text-neutral-600 dark:text-neutral-400 hover:bg-neutral-100 dark:hover:bg-dark-input hover:text-neutral-900 dark:hover:text-neutral-100'
                                }`}
                            >
                                <Icon className={`w-5 h-5 shrink-0 transition-transform duration-200 ${active ? 'scale-110' : 'group-hover:scale-110'}`} />
                                <span className="font-medium text-sm">{label}</span>
                            </Link>
                        );
                    })}
                </nav>

                {/* Footer */}
                <div className="p-4 shrink-0 border-t border-neutral-100 dark:border-dark-border space-y-2">
                    {/* Dark mode toggle (desktop) */}
                    <button
                        onClick={toggleDark}
                        className="hidden md:flex w-full items-center gap-3 px-4 py-2.5 rounded-xl text-neutral-600 dark:text-neutral-400 hover:bg-neutral-100 dark:hover:bg-dark-input transition-colors text-sm font-medium"
                    >
                        {isDark
                            ? <><SunIcon className="w-5 h-5 text-amber-400" /><span>Modo Claro</span></>
                            : <><MoonIcon className="w-5 h-5 text-indigo-400" /><span>Modo Oscuro</span></>
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
            <main className="flex-1 overflow-y-auto bg-neutral-50 dark:bg-dark-base transition-colors">
                <div className="p-6 md:p-10 max-w-7xl mx-auto">
                    {children}
                </div>
            </main>
        </div>
    );
}

export default Layout;
