// 可以直接从组件库导入的纯展示与布局组件，其余交互组件都要经 platform，新增要由框架负责人决定
const displayComponents = [
  'Row',
  'Col',
  'Flex',
  'Space',
  'Divider',
  'Card',
  'Descriptions',
  'Tag',
  'Badge',
  'Typography',
  'Statistic',
  'Empty',
  'Result',
  'Spin',
  'Skeleton',
  'Alert',
  'Tooltip',
  'Button',
];

/**
 * 业务模块里的限制
 *
 * 业务模块只写业务：不直接发请求、不碰浏览器存储与定时器、不直接用组件库的交互组件。
 * 这些能力由 platform 封装，封装里统一处理令牌、幂等、页面失活、业务时区等问题
 *
 * @param {string} modulesGlob 业务模块所在的文件匹配，如 src/modules/**
 */
export function moduleRestrictions(modulesGlob) {
  return [
    {
      files: [`${modulesGlob}/**/*.{ts,vue}`],
      rules: {
        'no-restricted-globals': [
          'error',
          ...[
            'fetch',
            'XMLHttpRequest',
            'localStorage',
            'sessionStorage',
            'indexedDB',
            'setInterval',
            'setTimeout',
            'Intl',
          ].map((name) => ({
            name,
            message: '业务模块不直接使用它，请经 platform 提供的封装',
          })),
        ],
        'no-restricted-imports': [
          'error',
          {
            paths: [
              { name: 'openapi-fetch', message: '业务模块不直接发请求：读经模块的 api.ts，写经 useCommand' },
              { name: 'vxe-table', message: '表格经 @/platform/table 使用，vxe-table 只在适配层内部出现' },
              { name: 'dayjs', message: '日期与时刻经 @/platform/format、@/platform/time' },
              { name: 'big.js', message: '金额与数量运算经 @/platform/format/decimal' },
              { name: 'pinia', importNames: ['defineStore'], message: '页面状态不放全局 store，用页面实例或查询缓存' },
              {
                name: '@tanstack/vue-query',
                importNames: ['useMutation'],
                message: '写入经 useCommand，它负责幂等、版本和结果未知',
              },
            ],
            patterns: [
              {
                group: ['antdv-next', 'antdv-next/*'],
                allowImportNames: displayComponents,
                message:
                  '交互组件经 @/platform/ui 使用；只有名单里的展示组件才能直接导入（名单在 eslint/restrictions.js）',
              },
            ],
          },
        ],
        'no-restricted-syntax': [
          'error',
          { selector: "NewExpression[callee.name='Date']", message: '时刻与日期经 @/platform/time，按业务时区处理' },
          {
            selector: "CallExpression[callee.object.name='Date'][callee.property.name='now']",
            message: '时刻与日期经 @/platform/time，按业务时区处理',
          },
        ],
      },
    },
  ];
}
