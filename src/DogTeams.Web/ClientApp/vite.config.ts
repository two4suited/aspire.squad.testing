import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

const getProxyTarget = (): string => {
  const httpsTarget = process.env.services__api__https__0;
  const httpTarget = process.env.services__api__http__0;
  const target = httpsTarget || httpTarget || 'https://localhost:7001';
  console.log(`[Vite Config] API Proxy Target: ${target}`);
  console.log(`[Vite Config]   https: ${httpsTarget}`);
  console.log(`[Vite Config]   http: ${httpTarget}`);
  return target;
};

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: getProxyTarget(),
        changeOrigin: true,
        secure: false,
      }
    }
  }
})
