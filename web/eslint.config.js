import boundaries from 'eslint-plugin-boundaries';
import vue from 'eslint-plugin-vue';
import tseslint from 'typescript-eslint';

const resolver = process.env.KMIC_RESOLVER ?? 'node';

export default tseslint.config(
  { ignores: ['dist/**', 'node_modules/**'] },
  ...tseslint.configs.recommended,
  ...vue.configs['flat/recommended'],
  {
    files: ['**/*.vue'],
    languageOptions: { parserOptions: { parser: tseslint.parser, extraFileExtensions: ['.vue'] } },
  },
  {
    files: ['src/**/*.{ts,vue}'],
    plugins: { boundaries },
    settings: {
      'boundaries/elements': [
        { type: 'app', pattern: 'src/app' },
        { type: 'shell', pattern: 'src/shell' },
        { type: 'platform', pattern: 'src/platform' },
        { type: 'module', pattern: 'src/modules/*', capture: ['name'] },
        { type: 'generated', pattern: 'src/generated' },
      ],
      'import/resolver':
        resolver === 'typescript'
          ? { typescript: { project: './tsconfig.json' } }
          : { node: { extensions: ['.ts', '.vue', '.js'] } },
    },
    rules: {
      'boundaries/no-unknown-dependencies': 'error',
      'boundaries/dependencies': [
        'error',
        {
          default: 'disallow',
          policies: [
            { from: { element: { type: 'app' } }, allow: { to: { element: { types: ['shell', 'module', 'platform', 'generated'] } } } },
            { from: { element: { type: 'shell' } }, allow: { to: { element: { types: ['platform', 'generated', 'module'] } } } },
            { from: { element: { type: 'platform' } }, allow: { to: { element: { types: ['platform', 'generated'] } } } },
            {
              from: { element: { type: 'module' } },
              allow: { to: { element: { types: ['platform', 'generated'] } } },
            },
            {
              from: { element: { type: 'module' } },
              allow: { to: { element: { type: 'module', captured: { name: '{{ from.element.captured.name }}' } } } },
            },
          ],
        },
      ],
    },
  },
  {
    files: ['src/modules/**/*.{ts,vue}'],
    rules: {
      'no-restricted-imports': [
        'error',
        { paths: [{ name: 'antdv-next', importNames: ['Select', 'Input', 'InputNumber', 'Form'], message: '经 platform/ui 的 K 组件使用' }] },
      ],
    },
  },
);
