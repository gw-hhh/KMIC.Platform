# 模块登记表

全部模块的登记表（07 第 2.4 节）。新增模块时先在此登记，再按 07 第 6.2 节建立项目。

- 编号：10～19 平台模块，20～89 业务模块，99 参考模块；编号 `NN` 的模块，`[LoggerMessage]` 的 `EventId` 使用 `NN000`～`NN999`（06 D6）。
- 架构测试检查：模块名、编号唯一；`EventId` 落在本模块的编号段内。
- 设计文档：`docs/modules/<模块名小写>/README.md`，格式见 07 第 8 节；评审通过后才开始编码。

| 模块 | schema | 编号 | 类型 | 依赖 | 负责人 | 设计文档 | 状态 |
|---|---|---|---|---|---|---|---|
| Identity | `Identity` | 10 | 平台 | — | @gw-hhh | [identity/README.md](identity/README.md) | 设计已确认（2026-11-12，D-A～D-G 按建议；业务数据类待确认项见设计 12.3）；未开始实现 |
| Operations | `Operations` | 11 | 平台 | Identity | @gw-hhh | [operations/README.md](operations/README.md) | 设计已确认（2026-11-12，D-O1～D-O3 按建议）；未开始实现，开工前先补框架公开服务（设计第 13.1 节） |
| Configuration | `Configuration` | 12 | 平台 | Identity | @gw-hhh | [configuration/README.md](configuration/README.md) | 设计已确认（2026-11-12，D-C1～D-C3 按建议）；已实现字典、系统设置、偏好（2026-11-13），功能开关查询未做 |
| Files | `Files` | 13 | 平台 | Identity | @gw-hhh | [files/README.md](files/README.md) | 设计已确认（2026-11-12，D-F1～D-F4 按建议）；未开始实现，开工前先补框架的上传与关联构件（设计 11.1） |
| Notifications | `Notifications` | 14 | 平台 | Identity、Files | @gw-hhh | [notifications/README.md](notifications/README.md) | 设计已确认（2026-11-12，D-N1～D-N3 按建议）；未开始实现，开工前先补框架的租约队列与模板声明（设计 11.1） |
| Sample | `Sample` | 99 | 参考（07 D3） | Identity | @gw-hhh | 07 业务模块标准写法 | 未开始 |

平台模块的编号按 08 阶段 3 的实施顺序分配（2026-11-05，S0-07）。业务模块的划分与试点模块的选定见 08 并行线 B 与 D4。
