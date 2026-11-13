# 模块登记表

全部模块的登记表（07 第 2.4 节）。新增模块时先在此登记，再按 07 第 6.2 节建立项目。

- 编号：10～19 平台模块，20～89 业务模块，99 参考模块；编号 `NN` 的模块，`[LoggerMessage]` 的 `EventId` 使用 `NN000`～`NN999`（06 D6）。
- 架构测试检查：模块名、编号唯一；`EventId` 落在本模块的编号段内。
- 设计文档：`docs/modules/<模块名小写>/README.md`，格式见 07 第 8 节；评审通过后才开始编码。

| 模块 | schema | 编号 | 类型 | 依赖 | 负责人 | 设计文档 | 状态 |
|---|---|---|---|---|---|---|---|
| Identity | `Identity` | 10 | 平台 | — | @gw-hhh | [identity/README.md](identity/README.md) | 设计已确认（2026-11-12，D-A～D-G 按建议；业务数据类待确认项见设计 12.3）；未开始实现 |
| Operations | `Operations` | 11 | 平台 | Identity | @gw-hhh | [operations/README.md](operations/README.md) | 设计已确认（2026-11-12，D-O1～D-O3 按建议）；已实现审计汇总与查询、定时任务管理、后台作业汇总、前端错误接收、消息积压与死信管理（2026-11-13），其余用例（路由与实例、补投、订阅终结、重建收尾）开工前先补框架公开服务（设计第 13.1 节） |
| Configuration | `Configuration` | 12 | 平台 | Identity | @gw-hhh | [configuration/README.md](configuration/README.md) | 设计已确认（2026-11-12，D-C1～D-C3 按建议）；已实现字典、系统设置、偏好（2026-11-13），功能开关查询未做 |
| Files | `Files` | 13 | 平台 | Identity | @gw-hhh | [files/README.md](files/README.md) | 设计已确认（2026-11-12，D-F1～D-F4 按建议）；已实现上传、关联与解除关联（互斥协议）、下载信息查询，`Sample` 已接入附件；管理、清理与对账、删除工作器未做（设计 15 节） |
| Notifications | `Notifications` | 14 | 平台 | Identity、Files | @gw-hhh | [notifications/README.md](notifications/README.md) | 设计已确认（2026-11-12，D-N1～D-N3 按建议）；已建模块，实现了站内消息（本人访问）、模板声明目录与渲染、发送请求的处理（投递、站内消息、跳过、去重、非生产捕获）；投递工作器与邮件、短信渠道、模板版本与管理、投递管理未做（设计 15 节） |
| Sample | `Sample` | 99 | 参考（07 D3） | Identity、Files | @gw-hhh | 07 业务模块标准写法 | 未开始 |

平台模块的编号按 08 阶段 3 的实施顺序分配（2026-11-05，S0-07）。业务模块的划分与试点模块的选定见 08 并行线 B 与 D4。
