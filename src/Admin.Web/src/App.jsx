import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import Layout from './components/Layout';
import Dashboard from './pages/Dashboard';
import Inventario from './pages/Inventario';
import Pedidos from './pages/Pedidos';
import Login from './pages/Login';
import { useState, useEffect } from 'react';
import Clientes from './pages/Clientes';
import Usuarios from './pages/Usuarios';
import Pagos from './pages/Pagos';
import Conversaciones from './pages/Conversaciones';
import Mensajes from './pages/Mensajes';
import Categoria from './pages/Categoria';
import { useDarkMode } from './hooks/useDarkMode';

function App() {
    const [isAuthenticated, setIsAuthenticated] = useState(false);
    const { isDark, toggleDark } = useDarkMode();

    useEffect(() => {
        const token = localStorage.getItem('token');
        if (token) setIsAuthenticated(true);
    }, []);

    const handleLogin = (token) => {
        localStorage.setItem('token', token);
        setIsAuthenticated(true);
    };

    const handleLogout = () => {
        localStorage.removeItem('token');
        setIsAuthenticated(false);
    };

    if (!isAuthenticated) {
        return <Login onLogin={handleLogin} isDark={isDark} toggleDark={toggleDark} />;
    }

    return (
        <Router>
            <Layout onLogout={handleLogout} isDark={isDark} toggleDark={toggleDark}>
                <Routes>
                    <Route path="/" element={<Dashboard />} />
                    <Route path="/inventario" element={<Inventario />} />
                    <Route path="/pedidos" element={<Pedidos />} />
                    <Route path="/clientes" element={<Clientes />} />
                    <Route path="/usuarios" element={<Usuarios />} />
                    <Route path="/pagos" element={<Pagos />} />
                    <Route path="/conversaciones" element={<Conversaciones />} />
                    <Route path="/mensajes" element={<Mensajes />} />
                    <Route path="/categorias" element={<Categoria />} />
                    <Route path="*" element={<Navigate to="/" replace />} />
                </Routes>
            </Layout>
        </Router>
    );
}

export default App;
