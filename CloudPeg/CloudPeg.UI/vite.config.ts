import { fileURLToPath, URL } from 'node:url'
import {resolve} from 'path'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'

// https://vite.dev/config/
export default defineConfig({
  base: "./",
  plugins: [
    vue(),
    vueDevTools(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
  // build: {
  //   lib: {
  //     entry: resolve(__dirname, 'vuefinder/dist/vuefinder.js'),
  //     formats: ['es', 'cjs'],
  //     name: 'VueFinder',
  //     // the proper extensions will be added
  //     fileName: 'vuefinder',
  //   },
  //   rollupOptions: {
  //     // make sure to externalize deps that shouldn't be bundled
  //     // into your library
  //     external: [
  //       'vue',
  //       'mitt',
  //       'vanilla-lazyload',
  //       'dragselect',
  //       'cropperjs/dist/cropper.css',
  //       'cropperjs',
  //       '@uppy/core',
  //       '@uppy/xhr-upload',
  //     ],
  //     output: {
  //
  //       // Provide global variables to use in the UMD build
  //       // for externalized deps
  //       globals: {
  //         vue: 'Vue'
  //       }
  //     }
  //   }
  // },
})
