import boundaries from 'eslint-plugin-boundaries';

/**
 * 目录分层的依赖规则
 *
 * app 可以用 shell、modules、platform；shell 和 modules 只能用 platform；所有层都可以用 generated。
 * modules 之间、modules 到 shell、platform 到 shell 都不允许；modules 只能导入自己所在的模块
 *
 * root 是 src 所在的目录：正式检查用 web/src，边界样例用 tests/boundaries/legal|violations/src，两边共用同一套规则
 *
 * @param {{ root: string, tsconfig: string }} options
 */
export function boundariesConfig({ root, tsconfig }) {
  return [
    {
      files: [`${root}/**/*.{ts,vue}`],
      plugins: { boundaries },
      settings: {
        // 默认的 node 解析器认不出 @/ 别名，越界导入会被当作外部包悄悄放过，必须用 TypeScript 解析器
        'import/resolver': { typescript: { project: tsconfig } },
        'boundaries/elements': [
          { type: 'app', pattern: `${root}/app/**` },
          { type: 'shell', pattern: `${root}/shell/**` },
          { type: 'platform', pattern: `${root}/platform/**` },
          { type: 'module', pattern: `${root}/modules/*/**`, capture: ['name'] },
          { type: 'generated', pattern: `${root}/generated/**` },
        ],
      },
      rules: {
        'boundaries/dependencies': [
          'error',
          {
            default: 'disallow',
            policies: [
              {
                from: { element: { type: 'app' } },
                allow: { to: { element: { types: ['app', 'shell', 'module', 'platform', 'generated'] } } },
              },
              {
                from: { element: { type: 'shell' } },
                allow: { to: { element: { types: ['shell', 'platform', 'generated'] } } },
              },
              {
                from: { element: { type: 'platform' } },
                allow: { to: { element: { types: ['platform', 'generated'] } } },
              },
              // 业务模块只能用自己（同名）、platform 和 generated，不能碰别的模块和 shell
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
  ];
}
