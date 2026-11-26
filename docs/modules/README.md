# 模块登记表

全部模块的登记表（07 第 2.4 节）。新增模块时先在此登记，再按 07 第 6.2 节建立项目。

- 编号：10～19 平台模块，20～89 业务模块，99 参考模块；编号 `NN` 的模块，`[LoggerMessage]` 的 `EventId` 使用 `NN000`～`NN999`（06 D6）。
- 架构测试检查：模块名、编号唯一；`EventId` 落在本模块的编号段内。
- 设计文档：`docs/modules/<模块名小写>/README.md`，格式见 07 第 8 节；评审通过后才开始编码。
- 中文名是权限分组等界面上的显示名，新建模块的脚本从这里读取。
- 本表只记设计状态；实现进度见 [implementation/00-README.md](../implementation/00-README.md)。

| 模块 | 中文名 | schema | 编号 | 类型 | 依赖 | 负责人 | 设计文档 | 设计状态 |
|---|---|---|---|---|---|---|---|---|
| Identity | 身份与授权 | `Identity` | 10 | 平台 | — | @gw-hhh | [identity/README.md](identity/README.md) | 已确认（2026-11-12，D-A～D-G 按建议；业务数据类待确认项见设计 12.3） |
| Operations | 运维 | `Operations` | 11 | 平台 | Identity | @gw-hhh | [operations/README.md](operations/README.md) | 已确认（2026-11-12，D-O1～D-O3 按建议） |
| Configuration | 配置 | `Configuration` | 12 | 平台 | Identity | @gw-hhh | [configuration/README.md](configuration/README.md) | 已确认（2026-11-12，D-C1～D-C3 按建议） |
| Files | 文件 | `Files` | 13 | 平台 | Identity | @gw-hhh | [files/README.md](files/README.md) | 已确认（2026-11-12，D-F1～D-F4 按建议） |
| Notifications | 通知 | `Notifications` | 14 | 平台 | Identity、Files | @gw-hhh | [notifications/README.md](notifications/README.md) | 已确认（2026-11-12，D-N1～D-N3 按建议） |
| Sample | 示例 | `Sample` | 99 | 参考（07 D3） | Identity、Files | @gw-hhh | 07 业务模块标准写法 | — |

平台模块的编号按 08 阶段 3 的实施顺序分配（2026-11-05，S0-07）。业务模块的划分与试点模块的选定见 08 并行线 B 与 D4。
