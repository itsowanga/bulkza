import path from 'node:path';
import react from '@vitejs/plugin-react';
import { loadEnv } from 'vite';
import { defineConfig } from 'vitest/config';

// In development the browser talks to the Vite server and Vite forwards /api to the backend.
// That keeps everything on one origin, so the API needs no CORS policy for local work.
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');
  const proxyTarget = env.VITE_PROXY_TARGET || 'http://localhost:8080';

  return {
    plugins: [react()],
    resolve: { alias: { '@': path.resolve(import.meta.dirname, 'src') } },
    server: {
      port: 5173,
      proxy: { '/api': { target: proxyTarget, changeOrigin: true } },
    },
    test: { environment: 'node', include: ['src/**/*.test.ts'] },
  };
});
