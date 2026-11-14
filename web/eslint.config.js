import js from '@eslint/js';
import globals from 'globals';
import tseslint from 'typescript-eslint';
import pluginVue from 'eslint-plugin-vue';
import vueParser from 'vue-eslint-parser';
import { boundariesConfig } from './eslint/boundaries.js';
import { moduleRestrictions } from './eslint/restrictions.js';

// 配置、边界规则和组件库名单的修改要经框架负责人审核
export default tseslint.config(
  {
    // generated/ 是生成代码不手改；边界样例是故意写错的，由 scripts/check-boundaries.mjs 单独检查
    ignores: ['dist', 'node_modules', 'src/generated', 'tests/boundaries', 'coverage'],
  },
  {
    // 所有豁免只能指定规则、写在下一行；多余的豁免也报错
    linterOptions: { reportUnusedDisableDirectives: 'error' },
  },
  js.configs.recommended,
  ...tseslint.configs.strictTypeChecked,
  ...pluginVue.configs['flat/recommended'],
  {
    files: ['**/*.{ts,vue}'],
    languageOptions: {
      globals: { ...globals.browser },
      parser: vueParser,
      parserOptions: {
        parser: tseslint.parser,
        projectService: true,
        extraFileExtensions: ['.vue'],
        tsconfigRootDir: import.meta.dirname,
      },
    },
  },
  {
    files: ['**/*.{js,mjs}'],
    ...tseslint.configs.disableTypeChecked,
    languageOptions: { globals: { ...globals.node } },
  },
  {
    files: ['**/*.{ts,vue}'],
    rules: {
      '@typescript-eslint/consistent-type-imports': 'error',
      '@typescript-eslint/consistent-type-assertions': [
        'error',
        { assertionStyle: 'as', objectLiteralTypeAssertions: 'never' },
      ],
      '@typescript-eslint/no-non-null-assertion': 'error',
      '@typescript-eslint/ban-ts-comment': [
        'error',
        { 'ts-expect-error': 'allow-with-description', minimumDescriptionLength: 5 },
      ],
      '@typescript-eslint/no-floating-promises': 'error',
      '@typescript-eslint/no-misused-promises': 'error',
      'no-console': 'error',
      'no-empty': 'error',
      'vue/block-order': ['error', { order: ['script', 'template', 'style'] }],
      'vue/multi-word-component-names': 'error',
      'vue/no-v-html': 'error',
      'vue/prefer-use-template-ref': 'error',
      'vue/no-undef-components': 'error',
      'vue/component-name-in-template-casing': ['error', 'PascalCase'],
      'vue/define-macros-order': 'error',
    },
  },
  {
    // 测试里构造数据、模拟响应时需要 as，对象字面量断言也常见
    files: ['**/*.test.ts', 'tests/**/*.ts'],
    rules: {
      '@typescript-eslint/consistent-type-assertions': 'off',
    },
  },
  ...boundariesConfig({ root: 'src', tsconfig: 'tsconfig.json' }),
  ...moduleRestrictions('src/modules'),
);
