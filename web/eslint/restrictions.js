// 允许直接从组件库导入的纯展示与布局组件；其余交互组件须通过 platform 封装引入，新增项由框架负责人审定
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
 * 业务模块代码约束
 *
 * 业务模块仅聚焦业务逻辑，禁止直接发起网络请求、操作浏览器存储与定时器，或直接使用组件库的交互组件。
 * 这些能力统一由 platform 封装，并在其中处理令牌、幂等、页面失活及业务时区等通用问题。
 *
 * @param {string} modulesGlob 业务模块的文件匹配模式，如 src/modules/**
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
            message: '业务模块禁止直接使用该全局 API，请通过 platform 提供的封装访问',
          })),
        ],
        'no-restricted-imports': [
          'error',
          {
            paths: [
              { name: 'openapi-fetch', message: '业务模块禁止直接发请求：查询请经模块 api.ts，写入请经 useCommand' },
              { name: 'vxe-table', message: '表格须经 @/platform/table 使用，vxe-table 仅限适配层内部引用' },
              { name: 'dayjs', message: '日期与时刻须经 @/platform/format、@/platform/time 处理' },
              { name: 'big.js', message: '金额与数量运算须经 @/platform/format/decimal 处理' },
              {
                name: 'pinia',
                importNames: ['defineStore'],
                message: '页面状态不应放入全局 store，请使用页面实例或查询缓存',
              },
              {
                name: '@tanstack/vue-query',
                importNames: ['useMutation'],
                message: '写入请使用 useCommand，由其统一处理幂等、版本并发与结果未知状态',
              },
              {
                name: '@tanstack/vue-query',
                importNames: ['useQuery'],
                message: '读取请使用 page.useQuery，非当前标签页时自动暂停并在切回后刷新',
              },
            ],
            patterns: [
              {
                group: ['antdv-next', 'antdv-next/*'],
                allowImportNames: displayComponents,
                message:
                  '交互组件须通过 @/platform/ui 使用；仅名单内的纯展示组件可直接导入（详见 eslint/restrictions.js）',
              },
            ],
          },
        ],
        'no-restricted-syntax': [
          'error',
          { selector: "NewExpression[callee.name='Date']", message: '时刻与日期须经 @/platform/time 按业务时区处理' },
          {
            selector: "CallExpression[callee.object.name='Date'][callee.property.name='now']",
            message: '时刻与日期须经 @/platform/time 按业务时区处理',
          },
        ],
      },
    },
  ];
}
