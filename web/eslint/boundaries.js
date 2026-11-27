import boundaries from 'eslint-plugin-boundaries';

/**
 * 目录分层依赖规则
 *
 * app 可依赖 shell、modules、platform；shell 与 modules 仅可依赖 platform；各层均可使用 generated。
 * 禁止 modules 间互访、modules 依赖 shell 及 platform 依赖 shell；业务模块仅允许内部引用自身。
 *
 * root 为源码根目录：正式检查传入 web/src，边界测试传入 tests/boundaries/legal|violations/src，两者共用同一套规则
 *
 * @param {{ root: string, tsconfig: string }} options
 */
export function boundariesConfig({ root, tsconfig }) {
  return [
    {
      files: [`${root}/**/*.{ts,vue}`],
      plugins: { boundaries },
      settings: {
        // 默认的 Node 解析器无法识别 @/ 别名，会导致越界导入被误判为外部包并放行，须使用 TypeScript 解析器
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
              // 业务模块仅允许访问自身模块、platform 与 generated，禁止引用其他模块和 shell
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
