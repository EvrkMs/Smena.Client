import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// WebView2 отдаёт файлы через SetVirtualHostNameToFolderMapping("app.local", "webui", ...)
// и грузит https://app.local/index.html — относительные пути обязательны (base: './').
export default defineConfig({
  plugins: [react()],
  base: './',
  build: {
    outDir: 'dist',
    emptyOutDir: true,
    sourcemap: false,
  },
})
