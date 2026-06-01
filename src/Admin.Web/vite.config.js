import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig(({ mode }) => {
  // Load env file based on `mode` in the current working directory.
  // Set the third parameter to '' to load all envs regardless of the `VITE_` prefix.
  const env = loadEnv(mode, process.cwd(), '');

  return {
    plugins: [react()],
    server: {
      port: 5173,
      allowedHosts: env.VITE_ALLOWED_HOST ? [env.VITE_ALLOWED_HOST] : true,
      proxy: {
        '/api': {
          target: 'http://gateway-service:3000', 
          changeOrigin: true,
          secure: false,
        },
        '/notificationHub': {
          target: 'http://inventario-service:8080',
          changeOrigin: true,
          secure: false,
          ws: true,
        },
      },
    },
  }
})
