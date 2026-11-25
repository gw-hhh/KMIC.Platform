// 页面样式只能用 --kmic-* 变量，颜色、字号、圆角、层级、动效时长不写死值（F04 第 8.3 节）
export default {
  extends: ['stylelint-config-standard-scss', 'stylelint-config-recommended-vue/scss'],
  plugins: ['stylelint-declaration-strict-value'],
  reportDescriptionlessDisables: true,
  reportNeedlessDisables: true,
  rules: {
    'declaration-no-important': true,
    'color-named': 'never',
    'color-no-hex': true,
    'scale-unlimited/declaration-strict-value': [
      [
        '/color$/',
        'background-color',
        'fill',
        'stroke',
        'border-radius',
        'z-index',
        'transition-duration',
        'animation-duration',
      ],
      {
        // 圆形用 50%；组件内部的叠放顺序（-1～3）不算全局层级
        ignoreValues: ['transparent', 'inherit', 'currentColor', 'none', '0', '50%', '/^-?[0-3]$/', 'unset', 'initial'],
        ignoreFunctions: false,
        severity: 'error',
      },
    ],
    'selector-class-pattern': ['^[a-z][a-z0-9-]*(?:__[a-z0-9-]+)?(?:--[a-z0-9-]+)?$', { message: '类名用小写短横线' }],
    'scss/at-mixin-pattern': null,
    'custom-property-pattern': null,
    'selector-pseudo-class-no-unknown': [true, { ignorePseudoClasses: ['deep', 'global'] }],
  },
  overrides: [
    {
      // 变量的来源本身要写具体的值
      files: ['src/app/styles/**'],
      rules: { 'scale-unlimited/declaration-strict-value': null },
    },
  ],
};
