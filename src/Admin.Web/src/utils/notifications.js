import { AlertIcon, CheckCircleIcon, MessagesIcon } from '../components/Icons';

/**
 * Mapea el string 'icono' de cada notificación → componente SVG real
 */
export const NOTIFICATION_ICONS = {
    success:      CheckCircleIcon,
    warning:      AlertIcon,
    error:        AlertIcon,
    info:         MessagesIcon,
    notification: MessagesIcon,
};

/**
 * Mapea el string 'color' (que viene del backend/WS) → clases con variante dark
 * para garantizar legibilidad en ambos modos de visualización.
 */
export const NOTIFICATION_COLORS = {
    'bg-blue-50': {
        container: 'bg-blue-50 dark:bg-blue-900/20 border-blue-100 dark:border-blue-800/30',
        text: 'text-blue-700 dark:text-blue-400'
    },
    'bg-green-50': {
        container: 'bg-green-50 dark:bg-green-900/20 border-green-100 dark:border-green-800/30',
        text: 'text-green-700 dark:text-green-400'
    },
    'bg-red-50': {
        container: 'bg-red-50 dark:bg-red-900/20 border-red-100 dark:border-red-800/30',
        text: 'text-red-700 dark:text-red-400'
    },
    'bg-yellow-50': {
        container: 'bg-yellow-50 dark:bg-yellow-900/20 border-yellow-100 dark:border-yellow-800/30',
        text: 'text-yellow-700 dark:text-yellow-400'
    },
    'bg-purple-50': {
        container: 'bg-purple-50 dark:bg-purple-900/20 border-purple-100 dark:border-purple-800/30',
        text: 'text-purple-700 dark:text-purple-400'
    },
    'bg-gray-50': {
        container: 'bg-gray-50 dark:bg-gray-700/30 border-gray-100 dark:border-gray-700/50',
        text: 'text-gray-700 dark:text-gray-400'
    },
};

/**
 * Helper para obtener las clases de color de una notificación.
 * @param {string} color - El nombre de la clase de color base.
 * @returns {string} Clases de Tailwind combinadas.
 */
export const getNotifColor = (color) =>
    NOTIFICATION_COLORS[color]?.container ?? 'bg-gray-50 dark:bg-gray-700/30 border-gray-100 dark:border-gray-700/50';

/**
 * Helper para obtener las clases de color de texto e icono de una notificación.
 * @param {string} color - El nombre de la clase de color base.
 * @returns {string} Clases de Tailwind de texto.
 */
export const getNotifTextColor = (color) =>
    NOTIFICATION_COLORS[color]?.text ?? 'text-gray-700 dark:text-gray-400';

/**
 * Helper para obtener el componente de icono de una notificación.
 * @param {string} icono - El nombre del icono.
 * @returns {React.Component} El componente SVG.
 */
export const getNotifIcon = (icono) => NOTIFICATION_ICONS[icono] ?? MessagesIcon;
