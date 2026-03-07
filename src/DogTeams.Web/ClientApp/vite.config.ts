import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: process.env.services__api__https__0 || process.env.services__api__http__0 || 'https://localhost:7001',
        changeOrigin: true,
        secure: false,
      }
    }
  }
})
