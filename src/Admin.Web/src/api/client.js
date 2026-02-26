import axios from 'axios';

const API_BASE_URL = '/api'; 

const api = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Add token to requests
api.interceptors.request.use((config) => {
    const token = localStorage.getItem('token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

// Handle 401 errors (Sesión expirada o token inválido)
api.interceptors.response.use(
    (response) => response,
    (error) => {
        // Si el Gateway o los Microservicios rechazan el token (401)
        if (error.response?.status === 401) {
            localStorage.removeItem('token');
            // Solo redirecciona si no estamos ya en el login para evitar bucles
            if (window.location.pathname !== '/') {
                window.location.href = '/';
            }
        }
        return Promise.reject(error);
    }
);

export default api;