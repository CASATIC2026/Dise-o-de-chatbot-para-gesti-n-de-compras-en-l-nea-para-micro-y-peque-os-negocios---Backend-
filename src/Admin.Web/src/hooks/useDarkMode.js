import { useState, useEffect } from 'react';

/**
 * useDarkMode
 * Manages dark/light mode by toggling the `dark` class on <html>.
 * Persists user preference to localStorage.
 * Falls back to the OS preference on first visit.
 */
export function useDarkMode() {
    const [isDark, setIsDark] = useState(() => {
        const saved = localStorage.getItem('chatly-theme');
        if (saved !== null) return saved === 'dark';
        return window.matchMedia('(prefers-color-scheme: dark)').matches;
    });

    useEffect(() => {
        const root = document.documentElement;
        if (isDark) {
            root.classList.add('dark');
        } else {
            root.classList.remove('dark');
        }
        localStorage.setItem('chatly-theme', isDark ? 'dark' : 'light');
    }, [isDark]);

    const toggleDark = () => setIsDark(prev => !prev);

    return { isDark, toggleDark };
}
