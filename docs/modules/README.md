# 模块登记表

全部模块的登记表（07 第 2.4 节）。新增模块时先在此登记，再按 07 第 6.2 节建立项目。

- 编号：10～19 平台模块，20～89 业务模块，99 参考模块；编号 `NN` 的模块，`[LoggerMessage]` 的 `EventId` 使用 `NN000`～`NN999`（06 D6）。
- 架构测试检查：模块名、编号唯一；`EventId` 落在本模块的编号段内。
- 设计文档：`docs/modules/<模块名小写>/README.md`，格式见 07 第 8 节；评审通过后才开始编码。

| 模块 | schema | 编号 | 类型 | 依赖 | 负责人 | 设计文档 | 状态 |
|---|---|---|---|---|---|---|---|
| Identity | `Identity` | 10 | 平台 | — | @gw-hhh | [identity/README.md](identity/README.md) | 设计待审（2026-11-12 起草，待决 D-A～D-G）；评审通过前不编码 |
| Operations | `Operations` | 11 | 平台 | 阶段 3 设计时确定 | @gw-hhh | `modules/operations/` | 未开始 |
| Configuration | `Configuration` | 12 | 平台 | 阶段 3 设计时确定 | @gw-hhh | `modules/configuration/` | 未开始 |
| Files | `Files` | 13 | 平台 | 阶段 3 设计时确定 | @gw-hhh | `modules/files/` | 未开始 |
| Notifications | `Notifications` | 14 | 平台 | 阶段 3 设计时确定 | @gw-hhh | `modules/notifications/` | 未开始 |
| Sample | `Sample` | 99 | 参考（07 D3） | Identity | @gw-hhh | 07 业务模块标准写法 | 未开始 |

平台模块的编号按 08 阶段 3 的实施顺序分配（2026-11-05，S0-07）。业务模块的划分与试点模块的选定见 08 并行线 B 与 D4。
