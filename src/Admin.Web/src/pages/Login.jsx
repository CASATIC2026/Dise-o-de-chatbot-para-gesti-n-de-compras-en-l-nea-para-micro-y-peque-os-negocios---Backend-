import { useState } from 'react';
import axios from 'axios';

// Mini bar chart purely with CSS/SVG
const MiniBarChart = () => {
    const bars = [
        { h: 40, active: false }, { h: 65, active: false }, { h: 85, active: true },
        { h: 55, active: false }, { h: 70, active: false }, { h: 90, active: true },
        { h: 60, active: false },
    ];
    return (
        <div className="flex items-end gap-2 h-20 mt-4">
            {bars.map((bar, i) => (
                <div
                    key={i}
                    className={`flex-1 rounded-t-md transition-all duration-700 ${bar.active ? 'bg-primary-600' : 'bg-primary-200'}`}
                    style={{ height: `${bar.h}%`, animationDelay: `${i * 0.1}s` }}
                />
            ))}
        </div>
    );
};

function Login({ onLogin }) {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);
    const [showPassword, setShowPassword] = useState(false);

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        setLoading(true);

        try {
            const response = await axios.post('/api/auth/login', {
                email,
                password,
            });

            if (response.data.token) {
                onLogin(response.data.token);
            }
        } catch (err) {
            setError(err.response?.data?.message || 'Error al iniciar sesión');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="min-h-screen flex bg-white font-sans overflow-hidden">
            {/* ─── Left Panel: Form ─────────────────────────────── */}
            <div className="w-full lg:w-[45%] flex flex-col p-8 md:p-16 lg:p-24 relative z-10 animate-fade-in">
                {/* Logo */}
                <div className="mb-12 flex items-center gap-3">
                    <img src="/src/resources/ChatlyIcon.svg" alt="Chatly" className="w-10 h-10" />
                    <span className="text-2xl font-black text-neutral-900 tracking-tighter">CHATLY</span>
                </div>

                <div className="flex-1 flex flex-col justify-center max-w-md mx-auto w-full">
                    <div className="mb-10">
                        <h1 className="text-3xl font-bold text-neutral-900 mb-3 tracking-tight">
                            Inicia sesión en tu cuenta
                        </h1>
                        <p className="text-neutral-500 font-medium">Por favor ingresa tus datos para continuar.</p>
                    </div>

                    <form onSubmit={handleSubmit} className="space-y-5">
                        {/* Email */}
                        <div className="space-y-2">
                            <label className="text-sm font-bold text-neutral-700 block ml-1">Email / Usuario</label>
                            <div className="relative group">
                                <span className="absolute left-4 top-1/2 -translate-y-1/2 text-neutral-400 group-focus-within:text-primary-500 transition-colors text-sm">✉</span>
                                <input
                                    type="text"
                                    value={email}
                                    onChange={(e) => setEmail(e.target.value)}
                                    className="w-full pl-11 pr-4 py-3.5 bg-neutral-50 border border-neutral-200 rounded-2xl focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 focus:bg-white transition-all text-neutral-900 placeholder:text-neutral-400"
                                    placeholder="admin@example.com"
                                    required
                                />
                            </div>
                        </div>

                        {/* Password */}
                        <div className="space-y-2">
                            <div className="flex justify-between items-center ml-1">
                                <label className="text-sm font-bold text-neutral-700">Contraseña</label>
                                <button type="button" className="text-xs font-bold text-primary-600 hover:text-primary-700 transition-colors">
                                    ¿Olvidaste tu contraseña?
                                </button>
                            </div>
                            <div className="relative group">
                                <span className="absolute left-4 top-1/2 -translate-y-1/2 text-neutral-400 group-focus-within:text-primary-500 transition-colors text-sm">🔒</span>
                                <input
                                    type={showPassword ? 'text' : 'password'}
                                    value={password}
                                    onChange={(e) => setPassword(e.target.value)}
                                    className="w-full pl-11 pr-12 py-3.5 bg-neutral-50 border border-neutral-200 rounded-2xl focus:outline-none focus:border-primary-500 focus:ring-4 focus:ring-primary-500/10 focus:bg-white transition-all text-neutral-900 placeholder:text-neutral-400"
                                    placeholder="••••••••"
                                    required
                                />
                                <button
                                    type="button"
                                    onClick={() => setShowPassword(!showPassword)}
                                    className="absolute right-4 top-1/2 -translate-y-1/2 text-neutral-400 hover:text-neutral-600 transition-colors text-sm"
                                >
                                    {showPassword ? '👁' : '🙈'}
                                </button>
                            </div>
                        </div>

                        {/* Remember me */}
                        <div className="flex items-center gap-2 px-1">
                            <input type="checkbox" id="remember" className="w-4 h-4 rounded border-neutral-300 text-primary-600 focus:ring-primary-500 accent-primary-600" />
                            <label htmlFor="remember" className="text-sm font-semibold text-neutral-600 cursor-pointer">Recordarme por 30 días</label>
                        </div>

                        {/* Error */}
                        {error && (
                            <div className="bg-red-50 border border-red-100 text-red-600 px-4 py-3 rounded-2xl text-sm font-semibold flex items-center gap-2">
                                ⚠️ {error}
                            </div>
                        )}

                        {/* CTA */}
                        <button
                            type="submit"
                            disabled={loading}
                            className="w-full bg-gradient-to-r from-primary-600 to-primary-700 text-white py-4 rounded-2xl font-bold shadow-lg shadow-primary-500/25 hover:shadow-primary-500/40 hover:-translate-y-0.5 active:translate-y-0 transition-all disabled:opacity-50 disabled:cursor-not-allowed text-base"
                        >
                            {loading ? (
                                <span className="flex items-center justify-center gap-2">
                                    <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                                    Iniciando...
                                </span>
                            ) : 'Entrar al Panel'}
                        </button>
                    </form>

                    <p className="mt-10 text-center text-xs text-neutral-400 font-medium">
                        CHATLY © {new Date().getFullYear()} — Gestión de E-commerce Inteligente
                    </p>
                </div>
            </div>

            {/* ─── Right Panel: Live Dashboard Showcase ─────────── */}
            <div className="hidden lg:flex lg:w-[55%] relative overflow-hidden bg-[#f0f4ff]">

                {/* Soft blobs */}
                <div className="absolute top-[-120px] right-[-120px] w-[500px] h-[500px] rounded-full bg-primary-200/40 blur-[100px]" />
                <div className="absolute bottom-[-80px] left-[-80px] w-[400px] h-[400px] rounded-full bg-secondary-200/30 blur-[80px]" />

                <div className="relative z-10 flex flex-col items-center justify-center w-full p-12 gap-6">

                    {/* ── "Bot Deployed" floating toast (top right) */}
                    <div className="absolute top-8 right-8 flex items-center gap-2 bg-white px-4 py-2.5 rounded-2xl shadow-lg border border-neutral-100 animate-float text-sm">
                        <span className="w-6 h-6 rounded-full bg-emerald-100 flex items-center justify-center">
                            <span className="animate-pulse-dot w-2.5 h-2.5 rounded-full bg-emerald-500 block" />
                        </span>
                        <div>
                            <div className="text-xs font-black text-neutral-800">Bot Deployed</div>
                            <div className="text-[10px] text-neutral-400">en Línea</div>
                        </div>
                    </div>

                    {/* ── Language Support pill (bottom left) */}
                    <div className="absolute bottom-8 left-8 bg-white px-4 py-3 rounded-2xl shadow-lg border border-neutral-100 animate-float-delayed">
                        <div className="text-xs font-black text-neutral-800 mb-2">Soporte Multilenguaje</div>
                        <div className="flex gap-1.5">
                            {['ES', 'EN', 'FR', '+42'].map((lang, i) => (
                                <span key={i} className={`text-[10px] font-bold px-2 py-0.5 rounded-md ${i < 3 ? 'bg-neutral-100 text-neutral-700' : 'bg-primary-600 text-white'}`}>
                                    {lang}
                                </span>
                            ))}
                        </div>
                    </div>

                    {/* ── Main Dashboard Mockup card */}
                    <div className="bg-white rounded-3xl shadow-2xl shadow-neutral-300/50 w-full max-w-lg border border-neutral-100 overflow-hidden animate-fade-in-up">
                        {/* Window chrome */}
                        <div className="flex items-center gap-1.5 px-5 pt-4 pb-3 border-b border-neutral-100">
                            <span className="w-3 h-3 rounded-full bg-red-400" />
                            <span className="w-3 h-3 rounded-full bg-amber-400" />
                            <span className="w-3 h-3 rounded-full bg-emerald-400" />
                            <span className="ml-auto text-[10px] font-black text-neutral-400 tracking-widest uppercase">Live Dashboard</span>
                        </div>

                        <div className="p-5 space-y-4">
                            {/* Header row */}
                            <div className="flex items-center gap-3">
                                <div className="w-10 h-10 rounded-xl bg-primary-600 flex items-center justify-center shadow-md shadow-primary-500/30">
                                    <img src="/src/resources/ChatlyIcon.svg" alt="Chatly" className="w-6 h-6" />
                                </div>
                                <div className="flex-1 space-y-1.5">
                                    <div className="h-2.5 w-3/4 bg-neutral-100 rounded-full" />
                                    <div className="h-2 w-1/2 bg-neutral-100 rounded-full" />
                                </div>
                            </div>

                            {/* Stat cards */}
                            <div className="grid grid-cols-2 gap-3">
                                <div className="bg-primary-50 rounded-2xl p-4">
                                    <div className="text-2xl font-black text-primary-700">98.4%</div>
                                    <div className="text-xs font-bold text-primary-500 mt-1">Precisión de Respuesta</div>
                                </div>
                                <div className="bg-purple-50 rounded-2xl p-4">
                                    <div className="text-2xl font-black text-purple-700">0.4s</div>
                                    <div className="text-xs font-bold text-purple-500 mt-1">Tiempo Promedio</div>
                                </div>
                            </div>

                            {/* Bar chart section */}
                            <div className="bg-neutral-50 rounded-2xl p-4">
                                <div className="flex justify-between items-center">
                                    <span className="text-xs font-black text-neutral-700">Sesiones Activas</span>
                                    <span className="text-[10px] font-bold text-primary-600 bg-primary-50 px-2 py-0.5 rounded-full">Tiempo real</span>
                                </div>
                                <MiniBarChart />
                                <div className="flex justify-between mt-2">
                                    {['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom'].map(d => (
                                        <span key={d} className="text-[9px] text-neutral-400 font-medium flex-1 text-center">{d}</span>
                                    ))}
                                </div>
                            </div>
                        </div>
                    </div>

                    {/* Label below */}
                    <p className="text-neutral-500 text-sm font-semibold text-center tracking-tight">
                        Monitorea tu chatbot de e-commerce en tiempo real
                    </p>
                </div>
            </div>
        </div>
    );
}

export default Login;
