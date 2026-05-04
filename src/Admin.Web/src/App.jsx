import { createBrowserRouter, Navigate, RouterProvider } from 'react-router-dom';
import { useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import Layout from './components/Layout';
import Dashboard from './pages/Dashboard';
import Inventario from './pages/Inventario';
import Pedidos from './pages/Pedidos';
import Login from './pages/Login';
import Clientes from './pages/Clientes';
import Usuarios from './pages/Usuarios';
import Pagos from './pages/Pagos';
import Conversaciones from './pages/Conversaciones';
import Mensajes from './pages/Mensajes';
import Categoria from './pages/Categoria';
import { useDarkMode } from './hooks/useDarkMode';

const SIGNALR_HUB_URL = import.meta.env.VITE_SIGNALR_URL || '/notificationHub';

const colorMap = {
    success: 'bg-green-50',
    warning: 'bg-yellow-50',
    error: 'bg-red-50',
    info: 'bg-blue-50',
};

const iconoMap = {
    success: 'success',
    warning: 'warning',
    error: 'error',
    info: 'info',
};

const ROLES = {
    ADMINISTRADOR: 'Administrador',
    VENDEDOR: 'Vendedor',
};

const rutasPorRol = {
    [ROLES.ADMINISTRADOR]: ['/', '/inventario', '/pedidos', '/clientes', '/usuarios', '/pagos', '/conversaciones', '/mensajes', '/categorias'],
    [ROLES.VENDEDOR]: ['/', '/inventario', '/pedidos', '/clientes', '/pagos'],
};

const parseJwt = (token) => {
    try {
        const base64Url = token.split('.')[1];
        if (!base64Url) {
            return null;
        }

        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(
            atob(base64)
                .split('')
                .map((char) => `%${(`00${char.charCodeAt(0).toString(16)}`).slice(-2)}`)
                .join('')
        );

        return JSON.parse(jsonPayload);
    } catch (error) {
        console.error('Error parsing JWT:', error);
        return null;
    }
};

const getUserRoleFromToken = (token) => {
    const payload = parseJwt(token);
    return payload?.role || payload?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || null;
};

const createProtectedElement = (element, userRole) => {
    if (!userRole) {
        return <Navigate to="/" replace />;
    }

    return element;
};

function App() {
    const [isAuthenticated, setIsAuthenticated] = useState(!!localStorage.getItem('token'));
    const [userRole, setUserRole] = useState(() => localStorage.getItem('userRole'));
    const { isDark, toggleDark } = useDarkMode();
    const [notifications, setNotifications] = useState([
        {
            id: 1,
            titulo: 'Sistema en linea',
            mensaje: 'Escuchando nuevas notificaciones...',
            tipo: 'info',
            icono: 'info',
            color: 'bg-blue-50',
            fecha: 'Ahora',
        }
    ]);
    const [unreadCount, setUnreadCount] = useState(0);

    const allowedRoutes = rutasPorRol[userRole] || [];

    const normalizeNotification = (notificacion) => {
        const tipo = `${notificacion?.tipo || 'info'}`.toLowerCase();

        return {
            id: notificacion?.id || Date.now(),
            titulo: notificacion?.titulo || 'Notificacion',
            mensaje: notificacion?.mensaje || 'Sin mensaje',
            tipo,
            icono: iconoMap[tipo] || 'info',
            color: colorMap[tipo] || 'bg-gray-50',
            fecha: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
        };
    };

    const showDesktopNotification = (notification) => {
        if (typeof window === 'undefined' || !('Notification' in window)) {
            return;
        }

        if (Notification.permission !== 'granted') {
            return;
        }

        if (document.visibilityState === 'visible' && document.hasFocus()) {
            return;
        }

        const desktopNotification = new Notification(notification.titulo, {
            body: notification.mensaje,
            tag: `chatbot-${notification.id}`,
        });

        desktopNotification.onclick = () => {
            window.focus();
            desktopNotification.close();
        };
    };

    const handleLogin = (token) => {
        const role = getUserRoleFromToken(token);
        localStorage.setItem('token', token);
        localStorage.setItem('userRole', role || '');
        setUserRole(role);
        setIsAuthenticated(true);
    };

    const handleLogout = () => {
        localStorage.removeItem('token');
        localStorage.removeItem('userRole');
        setIsAuthenticated(false);
        setUserRole(null);
        setUnreadCount(0);
    };

    const markNotificationsAsRead = () => {
        setUnreadCount(0);
    };

    useEffect(() => {
        const token = localStorage.getItem('token');
        if (!token) {
            setUserRole(null);
            return;
        }

        setUserRole(getUserRoleFromToken(token));
    }, []);

    useEffect(() => {
        if (!isAuthenticated || typeof window === 'undefined' || !('Notification' in window)) {
            return;
        }

        if (Notification.permission === 'default') {
            Notification.requestPermission().catch(() => {
                console.warn('No se pudo solicitar permiso para notificaciones de escritorio.');
            });
        }
    }, [isAuthenticated]);

    useEffect(() => {
        if (!isAuthenticated) {
            return undefined;
        }

        let isMounted = true;

        const connection = new signalR.HubConnectionBuilder()
            .withUrl(SIGNALR_HUB_URL, {
                accessTokenFactory: () => localStorage.getItem('token') || '',
            })
            .withAutomaticReconnect()
            .build();

        const startConnection = async () => {
            if (connection.state !== signalR.HubConnectionState.Disconnected) {
                return;
            }

            try {
                await connection.start();
                console.log('SignalR conectado');
            } catch (err) {
                console.error('Error al conectar SignalR:', err);
                setTimeout(startConnection, 5000);
            }
        };

        connection.on('ReceiveNotification', (notificacion) => {
            if (!isMounted) {
                return;
            }

            const normalizedNotification = normalizeNotification(notificacion);

            setNotifications((prev) => [normalizedNotification, ...prev].slice(0, 30));
            setUnreadCount((prev) => prev + 1);
            showDesktopNotification(normalizedNotification);
        });

        startConnection();

        return () => {
            isMounted = false;
            connection.stop();
        };
    }, [isAuthenticated]);

    const router = createBrowserRouter([
        {
            path: '/',
            element: (
                <Layout
                    onLogout={handleLogout}
                    notifications={notifications}
                    unreadCount={unreadCount}
                    onOpenNotifications={markNotificationsAsRead}
                    isDark={isDark}
                    toggleDark={toggleDark}
                />
            ),
            children: [
                { index: true, element: createProtectedElement(<Dashboard notifications={notifications} />, userRole) },
                { path: 'inventario', element: allowedRoutes.includes('/inventario') ? <Inventario /> : <Navigate to="/" replace /> },
                { path: 'clientes', element: allowedRoutes.includes('/clientes') ? <Clientes /> : <Navigate to="/" replace /> },
                { path: 'usuarios', element: allowedRoutes.includes('/usuarios') ? <Usuarios /> : <Navigate to="/" replace /> },
                { path: 'pedidos', element: allowedRoutes.includes('/pedidos') ? <Pedidos /> : <Navigate to="/" replace /> },
                { path: 'pagos', element: allowedRoutes.includes('/pagos') ? <Pagos /> : <Navigate to="/" replace /> },
                { path: 'conversaciones', element: allowedRoutes.includes('/conversaciones') ? <Conversaciones /> : <Navigate to="/" replace /> },
                { path: 'mensajes', element: allowedRoutes.includes('/mensajes') ? <Mensajes /> : <Navigate to="/" replace /> },
                { path: 'categorias', element: allowedRoutes.includes('/categorias') ? <Categoria /> : <Navigate to="/" replace /> },
                { path: '*', element: <Navigate to="/" replace /> },
            ]
        }
    ], {
        future: {
            v7_startTransition: true,
            v7_relativeSplatPath: true,
            v7_fetcherPersist: true,
            v7_normalizeFormMethod: true,
            v7_partialHydration: true,
            v7_skipActionErrorRevalidation: true,
        }
    });

    if (!isAuthenticated) {
        return <Login onLogin={handleLogin} isDark={isDark} toggleDark={toggleDark} />;
    }

    return <RouterProvider router={router} future={{ v7_startTransition: true }} />;
}

export default App;
