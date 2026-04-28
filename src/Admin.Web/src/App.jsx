import { createBrowserRouter, RouterProvider, Navigate } from 'react-router-dom';
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
    success: '✅',
    warning: '⚠️',
    error: '❌',
    info: 'ℹ️',
};

function App() {
    const [isAuthenticated, setIsAuthenticated] = useState(!!localStorage.getItem('token'));
    const { isDark, toggleDark } = useDarkMode();
    const [notifications, setNotifications] = useState([
        {
            id: 1,
            titulo: 'Sistema en línea',
            mensaje: 'Escuchando nuevas notificaciones...',
            tipo: 'info',
            icono: '🚀',
            color: 'bg-blue-50',
            fecha: 'Ahora',
        }
    ]);
    const [unreadCount, setUnreadCount] = useState(0);

    const normalizeNotification = (notificacion) => {
        const tipo = `${notificacion?.tipo || 'info'}`.toLowerCase();

        return {
            id: notificacion?.id || Date.now(),
            titulo: notificacion?.titulo || 'Notificación',
            mensaje: notificacion?.mensaje || 'Sin mensaje',
            tipo,
            icono: iconoMap[tipo] || '🔔',
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
        localStorage.setItem('token', token);
        setIsAuthenticated(true);
    };

    const handleLogout = () => {
        localStorage.removeItem('token');
        setIsAuthenticated(false);
        setUnreadCount(0);
    };

    const markNotificationsAsRead = () => {
        setUnreadCount(0);
    };

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
                console.log('🚀 SignalR Conectado con éxito');
            } catch (err) {
                console.error('❌ Error al conectar SignalR:', err);
                setTimeout(startConnection, 5000);
            }
        };

        connection.on('ReceiveNotification', (notificacion) => {
            if (!isMounted) {
                return;
            }

            const normalizedNotification = normalizeNotification(notificacion);
            console.log('🔔 NOTIFICACIÓN RECIBIDA:', normalizedNotification);

            setNotifications((prev) => [normalizedNotification, ...prev].slice(0, 30));
            setUnreadCount((prev) => prev + 1);
            showDesktopNotification(normalizedNotification);
        });

        startConnection();

        return () => {
            isMounted = false;
            connection.stop();
            console.log('📡 SignalR Desconectado');
        };
    }, [isAuthenticated]);

    const router = createBrowserRouter([
        {
            path: "/",
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
                { index: true, element: <Dashboard notifications={notifications} /> },
                { path: "inventario", element: <Inventario /> },
                { path: "pedidos", element: <Pedidos /> },
                { path: "clientes", element: <Clientes /> },
                { path: "usuarios", element: <Usuarios /> },
                { path: "pagos", element: <Pagos /> },
                { path: "conversaciones", element: <Conversaciones /> },
                { path: "mensajes", element: <Mensajes /> },
                { path: "categorias", element: <Categoria /> },
                { path: "*", element: <Navigate to="/" replace /> },
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
