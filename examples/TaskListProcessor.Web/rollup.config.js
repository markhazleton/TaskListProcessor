import { nodeResolve } from '@rollup/plugin-node-resolve';
import commonjs from '@rollup/plugin-commonjs';
import terser from '@rollup/plugin-terser';

export default {
  input: 'src/js/main.js',
  output: {
    file: 'wwwroot/js/site.js',
    format: 'iife',
    name: 'TaskListProcessor',
    sourcemap: true
  },
  plugins: [
    nodeResolve({
      browser: true,
      preferBuiltins: false
    }),
    commonjs(),
    // Only minify in production
    process.env.NODE_ENV === 'production' && terser()
  ].filter(Boolean)
};
