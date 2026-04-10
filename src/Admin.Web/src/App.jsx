import { createBrowserRouter, RouterProvider, Navigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
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

function App() {
    const [isAuthenticated, setIsAuthenticated] = useState(!!localStorage.getItem('token'));

    const handleLogin = (token) => {
        localStorage.setItem('token', token);
        setIsAuthenticated(true);
    };

    const handleLogout = () => {
        localStorage.removeItem('token');
        setIsAuthenticated(false);
    };

    // Configuración del Router con las Future Flags activas
    const router = createBrowserRouter([
        {
            path: "/",
            element: <Layout onLogout={handleLogout} />,
            children: [
                { index: true, element: <Dashboard /> },
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
        return <Login onLogin={handleLogin} />;
    }

    return <RouterProvider router={router} future={{ v7_startTransition: true }} />;
}

export default App;