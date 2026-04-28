/** @type {import('tailwindcss').Config} */
export default {
    content: [
        "./index.html",
        "./src/**/*.{js,ts,jsx,tsx}",
    ],
    theme: {
        extend: {
            colors: {
                primary: {
                    DEFAULT: '#407BFF',
                    50: '#eef3ff',
                    100: '#e0ebff',
                    200: '#c7d9ff',
                    300: '#a3beff',
                    400: '#7e9aff',
                    500: '#407BFF', // Base Primary
                    600: '#2b5adc',
                    700: '#2246b7',
                    800: '#1e3c94',
                    900: '#1c3576',
                },
                secondary: {
                    DEFAULT: '#616DD5',
                    50: '#f1f2fc',
                    100: '#e5e7fa',
                    200: '#d0d3f6',
                    300: '#b1b7ef',
                    400: '#8e96e5',
                    500: '#616DD5', // Base Secondary
                    600: '#4d56c0',
                    700: '#4045a1',
                    800: '#363b84',
                    900: '#2f3469',
                },
                tertiary: {
                    DEFAULT: '#AC56AA',
                    50: '#fdf2fb',
                    100: '#fae6f8',
                    200: '#f4ccef',
                    300: '#eca5e3',
                    400: '#df71d2',
                    500: '#AC56AA', // Base Tertiary
                    600: '#8f418b',
                    700: '#72316e',
                    800: '#5c2a59',
                    900: '#4e264c',
                },
                neutral: {
                    DEFAULT: '#F8F9FA',
                    50: '#F8F9FA', // Base Neutral
                    100: '#f1f3f5',
                    200: '#e9ecef',
                    300: '#dee2e6',
                    400: '#ced4da',
                    500: '#adb5bd',
                    600: '#868e96',
                    700: '#495057',
                    800: '#343a40',
                    900: '#212529',
                },
                // ── Dark Mode: Obsidian Cyan Varied ──────────────────────
                cyan: {
                    DEFAULT: '#00C2CB',
                    50:  '#e0fafa',
                    100: '#b3f2f4',
                    200: '#80eaec',
                    300: '#4de1e4',
                    400: '#26dadd',
                    500: '#00C2CB',
                    600: '#009fa8',
                    700: '#007b83',
                    800: '#00585e',
                    900: '#003539',
                },
                indigo: {
                    DEFAULT: '#9492ff',
                    50:  '#f0efff',
                    100: '#d9d8ff',
                    200: '#b8b5ff',
                    300: '#9492ff',
                    400: '#7b77ff',
                    500: '#5651e5',
                    600: '#4340c2',
                    700: '#332f9e',
                    800: '#25217a',
                    900: '#181456',
                },
                dark: {
                    base:     '#0e0e0e',
                    elevated: '#141414',
                    surface:  '#1a1a1a',
                    input:    '#242424',
                    border:   '#2a2a2a',
                },
            },
            fontFamily: {
                sans: ['Inter', 'system-ui', 'sans-serif'],
            }
        },
    },
    darkMode: 'class',
    plugins: [],
}
