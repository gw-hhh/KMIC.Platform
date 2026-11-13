# Operations 模块设计

- 实现：审计汇总与查询已实现并通过端到端测试（2026-11-13）；定时任务管理、后台作业汇总、消息管理、前端错误接收、审计导出未做（需要 13.1 里的框架服务）。实现时的差异已就地订正，汇总见第 13 节末。
- 状态：**已确认**（2026-11-12）。起草当天，用户授权“遇到需要选择的地方，默认采用最推荐的方案”，D-O1～D-O3 与小项均按建议决定；第 12 节的待确认事项（保留期、量级、前端错误限额）依赖合规或试运行数据，不阻塞编码。开工前先补框架的公开服务（第 13.1 节）。
- 模块名 / schema / 编号：`Operations` / `"Operations"` / 11（平台模块，登记见 `docs/modules/README.md`）；`[LoggerMessage]` 的 `EventId` 使用 11000～11999
- 项目：`Kmic.Operations.Domain`、`Application`、`Infrastructure`、`Presentation`、`Contracts`
- 依据：ADR-0018、0019、0025、0027；05-07 第 6.3、7.5、9 节（消息路由、重建、管理视图）；05-08 第 5、6.5、7 节（定时任务、作业、管理端点）；05-09 第 3、7 节（审计）；05-12 第 2.5、6 节（前端错误、健康检查）；Identity 模块设计（`docs/modules/identity/README.md`）

## 0. 概述

### 0.1 职责与边界

Operations 的特点：**它自己的业务数据很少，大部分是管理其他模块的框架表**（各模块的 Outbox / Inbox / 作业表）。这些表属于各模块，由框架为每个模块登记存储接口，Operations 经接口汇总与操作（05-07 第 9.1 节、05-08 D6）；这与投递程序一样是框架基础设施的例外，不是 Operations 读取其他模块的业务数据。

| 做 | 不做 |
|---|---|
| 审计长期存储 `AuditLog`：订阅 `Framework.AuditRecorded`，提供统一审计查询与导出（05-09 D3） | 业务历史：各模块自己的 `{聚合}StateHistory`，业务页面展示它（05-09 第 5 节） |
| 定时任务管理：开关、Cron 覆盖、立即执行、执行记录；Quartz 的表（05-08 第 5 节） | 具体的定时任务：由各模块声明，命令在各模块里 |
| 后台作业汇总：全部作业列表、管理员取消、“我的任务”汇总（05-08 D6） | 作业本身的存储与执行：各模块的 `BackgroundJob` 表与框架执行器 |
| 消息管理：积压与死信、重试、放弃、订阅路由、补投、终结与重新激活、重建作业的收尾确认、实例与心跳（05-07 第 9.1 节） | 消息的投递与处理：框架的投递程序与 Inbox 处理器 |
| 前端错误接收端点（05-12 第 2.5 节，F03 D9） | 日志、指标、追踪：观测平台（05-12），Operations 不存日志 |
| 运行状态（依赖状态）页面：可选，见 D-O1，建议不做 | 安全事件：Identity 模块（05-09 第 6.3 节），登录、授权拒绝等不在这里 |
| | 系统设置、字典、功能开关：Configuration 模块 |
| | 附件的运维（查错、彻底删除）：Files 模块（05-11 第 6 节） |

### 0.2 与框架文档的关系

05-07、05-08、05-09 已确认的决定全部沿用，本文不重新讨论。本文补的是：模块自己的表与聚合、用例与权限点清单、前端页面、框架还要补哪些公开服务（第 13 节）、旧系统对照，以及框架文档留给 Operations 实现时再定的几件事（D-O1～D-O3）。

### 0.3 模块划分检查表（07 第 6.3 节）

| # | 检查项 | 结论 |
|---|---|---|
| 1 | 强不变量是否都归本模块 | 是。清单见下 |
| 2 | 每个命令是否只改本模块的数据 | **不完全**。审计、定时任务配置、路由状态在本模块；重试、放弃、取消、补投改的是**其他模块的框架表**（Outbox / Inbox / 作业行）。这是 05-07 第 9.1 节已确认的框架例外，经各模块的存储接口执行，每个模块一个独立事务；批量操作按模块分批、返回逐条收据，不追求跨模块原子（失败的条目可以再次执行） |
| 3 | 跨模块“先检查、后写入” | 放弃消息前调用接收方的 `IAbandonPolicy`（例如附件互斥，05-11 第 3.3 节）；检查与放弃之间的变化由该协议在接收方一侧用状态条件和锁处理，Operations 不自行加跨模块锁 |
| 4 | 订阅事件失败时的处理 | Operations 唯一的订阅是审计汇总。失败进入死信，在本模块的消息管理页可见；积压与死信由 05-07 的指标告警。**审计汇总失败不影响业务**（业务事务只写 Outbox），但会造成审计查询暂时缺记录，所以这条订阅的死信要按最高优先级处理（运维手册，阶段 5） |
| 5 | 其他模块数据怎么取 | 操作人、角色的显示名经 Identity 的 `IUserReferences`、`IRoleReferences`（`Reference` 查询，05-07 D3）；其余管理视图经框架存储接口 |
| 6 | 依赖是否单向无环 | 是。Operations `DependsOn(Identity)`；Identity 不依赖 Operations。其他模块不引用 Operations 的 Contracts（Operations 不对外提供 Contracts） |
| 7 | 拆成两个模块的理由 | 不拆。审计、任务、消息管理都是“运维”一类能力，共用一套权限点分组与前端区域（ADR-0027） |

本模块的强不变量：

| 编号 | 不变量 | 由谁保证 |
|---|---|---|
| O1 | 审计记录只追加；同一 `AuditId` 只写一次 | 运行账号无修改与删除权限（框架的只追加迁移约定）；`ON CONFLICT DO NOTHING`（05-09 第 3.7 节） |
| O2 | 定时任务的生效配置 = 代码默认值 ⊕ 覆盖值，修改后整个集群的触发器随之同步 | `JobOverride` 聚合 + 框架的调度同步 |
| O3 | 订阅路由的状态只能 生效 → 待终结 → 已终结 单向变化；重新激活是显式操作并留痕；终结时对剩余未完成行的处置与终结在同一事务 | 路由的状态条件 + 命令事务（05-07 第 6.3 节） |
| O4 | 人工重试、放弃与消息处理完成互不覆盖 | 状态条件 `UPDATE`（05-07 第 7.3 节，框架已有） |
| O5 | 定时任务执行记录在命令事务之外写入，命令回滚时失败记录仍在 | 框架的 `CommandJob`（已实现） |

---

## 1. 旧系统对照

| 旧系统 | 做什么 | 新系统 | 说明 |
|---|---|---|---|
| Hangfire 面板 `/hangfire` | 查看与触发任务；Basic 认证，账号密码写死在代码里 | 定时任务页 | 复用登录和权限点；改时间、停用、立即执行都有审计 |
| `EnableRecurringJobs` 总开关 | 所有定时任务同时开或关 | 每个任务独立开关 | FR-JOB-02 |
| 任务清单：客户行业标识（每天 0:00）、维修提醒（每天 7:00）、配置器特殊规格联动（每天 2:00）；Forecast、特殊合同评审超时检查已注释停用 | | 各业务模块声明 | 见第 9 节，这些任务属于业务，不在 Operations |
| 文本日志 `Web/Log`、`Kelong_Log` 库里分散的业务操作日志 | 排查与追溯 | 审计 `AuditLog`（谁在何时做了什么、改了哪些字段）+ 各模块业务历史 + 观测平台的日志 | 旧业务日志是各业务自己的流转记录，归新系统的业务历史，不迁入 `AuditLog` |
| 导出日志（`isSaveToExcelLog`、`ToExcelLog*`） | 记录导出了什么 | 导出走后台作业，作业记录与审计共同留痕 | 05-08、05-09 |
| `Global.Application_Error`：写日志并发错误邮件，同类错误 1 小时内去重，白名单存字典表 | 异常通知 | 日志进观测平台，告警规则在观测平台（05-12）；前端错误经 `client-errors` 进入同一平台 | 不再由应用发邮件；邮件去重与白名单是观测平台告警的职责 |
| 附件运维中心（`AttachmentMaintenance`，按库里配置的 SQL 模板查改附件） | 修正附件数据 | 不在 Operations | 属于 Files 模块的管理（05-11 第 6 节）；这个工具的使用场景要在 Files 设计时核对，不原样复制“可执行任意 SQL 模板”的做法 |
| 无 | 消息积压与死信 | 消息管理页 | 新增，旧系统没有消息总线 |

---

## 2. 术语

确认本文后一并登记到 `docs/glossary.md`。

| 中文 | 英文标识符 | 含义 |
|---|---|---|
| 审计记录 | `AuditEntry`（表 `AuditLog`） | 一次命令执行的汇总：谁、何时、做了什么、改了哪些字段 |
| 定时任务覆盖值 | `JobOverride`（表 `ScheduledJobSetting`） | 对代码里默认开关与 Cron 的修改，只存被改过的项 |
| 定时任务执行记录 | `ScheduledJobRun` | 一次触发的开始、结束与结果 |
| 订阅路由 | `MessageRoute` | 消息类型到订阅的登记，决定投递到哪些 Inbox |
| 终结 / 重新激活 | `Finalize` / `Reactivate` | 订阅路由的运维操作（05-07 第 6.3 节） |
| 补投 | `Redeliver` | 按消息类型与序号区间向指定订阅重新投递 |
| 死信 | `DeadLetter` | 重试用尽或不可重试的消息 |
| 实例心跳 | `MessagingInstance` | 实例定期登记构建号与订阅清单，只用于诊断 |

---

## 3. 聚合与表

### 3.1 总览

| 表 | 内容 | 谁写 | 在本模块的形态 |
|---|---|---|---|
| `AuditLog` | 审计记录（05-09 第 3.7 节） | Operations 的订阅处理 | 聚合 `AuditEntry`（只插入） |
| `ScheduledJobSetting` | 覆盖值（05-08 第 5.3 节） | Operations 的管理命令；框架只读 | 聚合 `JobOverride` |
| `ScheduledJobRun` | 执行记录（05-08 第 5.4 节） | 框架的 `CommandJob` | 只读查询（框架表） |
| `qrtz_*` | Quartz 集群表 | Quartz | 不映射，只在迁移里建（`SchedulingTables.QuartzCreateSql`） |
| `MessageRoute`、`MessagingInstance` | 订阅路由与心跳（05-07 第 6.3 节） | 框架的启动任务与心跳；Operations 的管理命令改状态 | 经框架服务操作 |

建表语句：路由表、调度表、Quartz 表在框架里已有（`MessageRouteTable`、`SchedulingTables`，框架测试暂借 `FrameworkSchema` 代建），Operations 的初始迁移执行它们；`AuditLog`、`MessagingInstance` 的建表语句由本模块（或框架，按第 13 节）补上。强类型 ID：`AuditEntryId`（即 `AuditId`，来自消息）。

所有表都不声明数据范围：`HasNoDataScope("运维数据按权限点管理")`。审计查询不套数据范围，审计人员需要看全部（05-09 第 3.8 节）。

### 3.2 `AuditEntry`

字段见 05-09 第 3.7 节（`Id`、`OccurredAt`、`Module`、`Action`、`Origin`、`ActorUserId`、`ActualUserId`、`RoleId`、`SessionId`、`OnBehalfOfUserId`、`ClientIp`、`TraceId`、`Targets`、`Changes`、`Details`、`ReceivedAt`）。补充：

- 没有行为方法，只有一个工厂 `Record(message)`；不是聚合行为，是“把消息落成一行”。迁移带框架的只追加约定（`Kmic:AppendOnly` 注解，运行账号撤销修改与删除）。
- 索引沿用 05-09：`(ActorUserId, OccurredAt DESC)`、`(Action, OccurredAt DESC)`、`Targets` 上的 GIN；另加 `(OccurredAt DESC)`，时间范围是必填条件，按它扫描要能走索引。
- 保留期到期的数据只能由迁移程序的维护命令删除：`Kmic.MigrationRunner purge --table audit-log --older-than-days N`（已实现，保留期必填、没有默认值）。保留期本身仍是 `待确认`（00-README“待决事项”，阶段 5）。
- 分区：不分区（05-09 D8 的触发条件见 `docs/framework/00-README.md` 延后事项）。

### 3.3 `JobOverride`

| 字段 | 说明 |
|---|---|
| `JobName` | 稳定名称，主键（如 `Identity.PurgeSessions`） |
| `Enabled` | `bool?`，为空表示跟随代码默认 |
| `Cron` | `varchar(100)?`，为空表示跟随代码默认 |
| `UpdatedBy`、`UpdatedAt`、`ConcurrencyVersion` | |

- 方法：`Apply(enabled?, cron?)`、`RestoreDefault()`（两项都置空，行保留还是删除：保留行、两项置空，便于审计追溯；框架读取时空值等于没有覆盖）。
- 代码里已删除的任务：覆盖行保留，回滚后配置仍在（05-08 第 5.3 节）。
- Cron 的合法性与“最短间隔”在 `Apply` 之外由命令处理器校验（需要调度库的解析器）：合法的 Quartz Cron 表达式；最短间隔 1 分钟（秒级触发对这套系统没有业务意义，且可能压垮数据库），不合规返回 `Operations.CronInvalid`。

---

## 4. 用例

命名遵循 05-02。**授权**为权限点或声明；管理类命令都 `[Idempotent]`（HTTP 来源须带幂等键）、`[Audited]`、携带 `ExpectedVersion`（有版本的对象）。Operations 自己的命令不继承 Identity 那个授权管理基类：它们不改授权。

### 4.1 审计

| 用例 | 授权 | 说明 |
|---|---|---|
| `RecordAuditCommand` | `[SystemOnly]`，订阅 `Framework.AuditRecorded` | 按 `AuditId` 幂等插入；`[NotAudited("审计汇总本身")]`；失败的重试与死信由框架处理 |
| `ListAuditQuery` | `Operations.Audit.View`（敏感，`[AuditedRead]`） | 时间范围必填且不超过 93 天；操作人、实际操作人、模块、动作、资源类型 + ID 筛选；**游标分页**（05-06 第 6.3 节，第 10 节） |
| `GetAuditQuery` | 同上 | 含字段级变更；操作人、角色名经引用查询解析，已停用显示标记 |
| `ExportAuditJob` | 提交命令 `Operations.Audit.Export`（敏感），作业类型 `Operations.ExportAudit` | 05-08 第 6 节后台作业，导出文件按 05-11 存放；作业审计 |

### 4.2 定时任务

| 用例 | 授权 | 说明 |
|---|---|---|
| `ListScheduledJobsQuery` | `Operations.Jobs.View` | 名称、默认与覆盖的开关与 Cron、实际生效值、下次触发、最近一次结果 |
| `ListScheduledJobRunsQuery` | `Operations.Jobs.View` | 某任务的执行记录，按时间倒序分页 |
| `PreviewCronQuery` | `Operations.Jobs.View` | 校验 Cron 并返回接下来 5 次触发时间（05-08 第 5.3 节） |
| `SetJobOverrideCommand` | `Operations.Jobs.Manage` | 停用 / 启用、改 Cron；保存后同步到 Quartz 触发器，整个集群即时生效 |
| `RestoreJobDefaultCommand` | `Operations.Jobs.Manage` | 恢复默认 |
| `RunJobNowCommand` | `Operations.Jobs.Manage` | 立即执行一次，执行记录的 `Trigger = Manual`、`TriggeredBy` 为操作人；该任务正在运行时拒绝（`Operations.JobAlreadyRunning`，409） |

### 4.3 后台作业

| 用例 | 授权 | 说明 |
|---|---|---|
| `ListBackgroundJobsQuery` | `Operations.Jobs.View` | 遍历各模块的作业存储，按模块、类型、状态、提交人筛选；按模块并行取、归并分页 |
| `CancelBackgroundJobCommand` | `Operations.Jobs.Manage` | 请求取消（05-08 第 6.6 节）；提交人本人取消走各模块路由组下的框架端点，不经这里 |
| `ListMyJobsQuery` | `[AllowAuthenticated]` | `GET /api/v1/operations/my-jobs`：汇总当前用户在各模块提交的作业（外壳的“作业入口”使用，F03 7.9） |

### 4.4 消息管理

| 用例 | 授权 | 说明 |
|---|---|---|
| `GetMessageSummaryQuery` | `Operations.Messages.View` | 各模块 Outbox / Inbox 积压、最老消息时长、死信数；待终结订阅的积压单独列出（预期内，不计入通用积压） |
| `ListDeadMessagesQuery`、`GetDeadMessageQuery` | `Operations.Messages.View` | 死信列表；查看载荷 `[AuditedRead]` |
| `ListBlockedKeysQuery` | `Operations.Messages.View` | 被有序订阅阻塞的键 |
| `RetryMessagesCommand` | `Operations.Messages.Manage`（敏感） | 单条或批量；状态回到待处理、次数清零；返回逐条收据 |
| `AbandonMessagesCommand` | 同上 | 原因必填；先调接收方的 `IAbandonPolicy`，拒绝则不改状态并返回原因 |
| `ListRoutesQuery`、`ListInstancesQuery` | `Operations.Messages.View` | 路由、重建轮次、实例与心跳（诊断） |
| `RedeliverCommand` | `Operations.Messages.Manage` | 按消息类型与 `Seq` 区间向指定订阅补投，只补投载荷完整的 Outbox 行（05-07 第 9.2 节） |
| `FinalizeRouteCommand`、`ReactivateRouteCommand` | 同上 | 终结与重新激活（第 5 节的约束） |
| `TriggerRebuildCommand`、`ConfirmRebuildCommand`、`AbandonRebuildCommand` | 同上 | 读模型重建的手动触发与收尾确认（05-07 第 8 节）；放弃本轮对路由标记的处理见 D-O2 |

### 4.5 其他

| 用例 | 授权 | 说明 |
|---|---|---|
| `ReportClientErrorsCommand` | `[AllowAuthenticated]`，`[NotAudited("前端错误日志")]` | 批量；字段白名单与长度上限见 8 节；只写日志（`Kmic.ClientErrors`），不落库；限流 429 |

### 4.6 系统任务

框架已有：清理 Outbox / Inbox、幂等记录、作业、执行记录的每日任务（`PurgeMessagesTask` 等，05-08 第 5.7 节），随调度器一起运行，不属于本模块的命令。Operations 自己没有周期性任务。

---

## 4A. 前端

外壳的“作业入口”和前端错误上报属于外壳（F03）；本模块是运维管理页面。路由名前缀 `operations.`，菜单归属“系统管理 / 运维”。

### 页面清单

| 路由名 | 类型 | 权限点 | 打开方式与理由 |
|---|---|---|---|
| `operations.audit` | 列表 | `Operations.Audit.View` | 时间范围必填（默认最近 24 小时，最长 93 天）；游标分页，不显示总数（05-06 第 6.3 节） |
| `operations.audit.detail` | 详情 | 同上 | 标签页：字段级变更表（新值 / 旧值，遮盖字段显示“已修改”或遮盖格式）；可从安全事件的追踪 ID 直达 |
| `operations.scheduledJobs` | 列表 | `Operations.Jobs.View` | 行内显示“默认 / 覆盖”两列与下次触发 |
| `operations.scheduledJobs.detail` | 详情 | 同上 | 标签页：配置与执行记录；修改计划用抽屉（3 个字段：开关、Cron、预览下次 5 次触发） |
| `operations.jobs` | 列表 | `Operations.Jobs.View` | 全部后台作业；取消是行操作（有权限才显示）；详情用抽屉（进度、结果、错误） |
| `operations.messages` | 概览 + 死信 + 被阻塞的键 | `Operations.Messages.View` | 同一页面的三个区块；重试与放弃是勾选后的批量操作（F07 批量模式） |
| `operations.messageRoutes` | 路由、重建、实例 | `Operations.Messages.View`（操作需 `Manage`） | 终结、重新激活、补投各用确认对话框，对话框里写明后果（“终结后旧版本无法自动恢复该订阅”），确认文字是完整的一句话（F04 第 9 节） |

### 动作

详情页动作与后端动作目录一致：定时任务 `edit`、`restoreDefault`、`runNow`。其余页面的操作是列表行操作或批量操作，按权限点显示（F07 D7）。

### 旧系统页面对照

旧系统没有对应页面（Hangfire 面板除外，见第 1 节）。不保留的交互：Hangfire 的面板风格与共享账号。

---

## 5. 授权

### 5.1 权限点

`Operations` 分组。敏感列为 `Sensitive = true`（用到它的命令须 `[Audited]`，查询须 `[AuditedRead]`）。

| 权限点 | 敏感 | 说明 |
|---|---|---|
| `Operations.Audit.View` | ✓ | 查看审计 |
| `Operations.Audit.Export` | ✓ | 导出审计 |
| `Operations.Jobs.View` | | 查看定时任务、执行记录、全部后台作业 |
| `Operations.Jobs.Manage` | | 改计划、立即执行、取消作业（05-08 第 7.1 节只要求 `[Audited]`，没有标敏感，沿用） |
| `Operations.Messages.View` | | 查看积压、死信、路由、实例 |
| `Operations.Messages.Manage` | ✓ | 重试、放弃、补投、终结、重新激活、重建收尾 |
| `Operations.Health.View` | | 仅当 D-O1 选择提供运行状态页时才声明 |

### 5.2 资源策略

没有按资源的策略：这些权限点就是授权的全部。危险操作的保护靠确认对话框、`IAbandonPolicy`、状态条件和审计，不靠“谁拥有这个资源”。

### 5.3 敏感权限点的管理

审计、消息管理会暴露业务数据（审计里的字段值、死信载荷）。系统管理员角色（`GrantsAllPermissions`）自动拥有它们；是否需要分权，见 D-O3。

---

## 6. 跨模块

| 方向 | 内容 |
|---|---|
| 依赖 | Identity：`IUserReferences`、`IRoleReferences`（显示名）。`DependsOn("Identity")` |
| 订阅 | `Framework.AuditRecorded`（来自全部模块的 Outbox），订阅名 `Operations.RecordAudit` |
| 发布 | 无 |
| 对外 Contracts | 无。业务页面不展示审计（05-09 第 3.8 节）；需要“某条数据的操作记录”时，那是业务历史 |
| 框架存储接口 | `IModuleMessageStore`、`IModuleJobStore`、`IAbandonPolicy<T>`：Operations 遍历调用，见第 13 节 |

容量保护的软线不拦 Operations 的运维命令（05-07 第 9.5 节）：这些命令写的审计消息进入 Outbox，软线触发时仍然要能重试、放弃、扩容后恢复，否则无法自救。

---

## 7. 后台处理

| 项 | 类型 | 说明 |
|---|---|---|
| 审计汇总 | Inbox 订阅 | Worker 角色处理（ADR-0025） |
| 审计导出 | 后台作业 | 05-08 第 6 节 |
| 定时任务计划同步 | 框架启动任务 | 已实现（`SyncSchedulesTask`）；Operations 的修改命令提交后另触发一次同步，使修改立即生效（第 13 节） |

---

## 8. 其他构件

| 构件 | 内容 |
|---|---|
| 配置（05-10） | `Operations:Audit`（页面默认时间范围、导出行数上限）、`Operations:ClientErrors`（见下） |
| 前端错误端点的限额（05-12 第 2.5 节写“阶段 1 定”，这里给出，`待确认`压测与前端联调） | 每次请求最多 20 条；错误类型 ≤ 100 字、消息 ≤ 1000 字、堆栈 ≤ 8000 字、路由名 ≤ 200 字、前端版本 ≤ 50 字；每用户每分钟 30 次，超限 429；未知字段拒绝 |
| 通知模板 | 无 |
| 编号、字典、附件 | 无 |

---

## 9. 数据迁移

没有需要迁入 Operations 的旧数据：

- 旧业务操作日志（`Kelong_Log` 库的 `*_Log` 表、主库与订单库的 `*_Log`）是各业务自己的流转与修改记录，字段含义各不相同，归各业务模块迁移时决定怎么进入业务历史；不写入 `AuditLog`（它的字段是新框架的统一格式，旧记录没有 `Action`、`Targets` 的对应）。旧库留作归档。
- Hangfire 的任务配置不迁移：新系统的任务在代码里声明。旧系统的任务清单（第 1 节）交给相应业务模块的设计文档，由它们决定哪些保留、怎么用命令重写；旧任务里调用存储过程的（如客户行业标识）按业务模块的规则重写，不在 Operations。

---

## 10. 延后事项对照

对照 `docs/framework/00-README.md` 的“延后事项”表：

| 事项 | 结论 |
|---|---|
| 游标分页（FR-API-04，S2-12 余项“业务第一次需要时”） | **命中**。审计查询是第一个需要它的接口（05-09 第 3.8 节、05-06 第 6.3 节）：游标分页随本模块实现，并在 Operations 的审计列表上作为第一个使用者验证。实现后在 S2 清单中划掉这一项 |
| `AuditLog`、`SecurityEvent` 按月分区 | 不命中，但 `AuditLog` 是本模块的：上线后观察表大小与查询耗时，触发条件见 05-09 D8；查询必须带时间范围（已写入页面与接口） |
| 业务事实类订阅的替代切换 | 不命中；终结与补投的界面已按 05-07 第 6.3 节的约束设计，第一次需要替代切换时再设计 |
| 其余（关系可见、扩展维度、授予有效期、API 版本、功能开关、Redis、创建键、trigram） | 不命中 |

**新登记的延后事项**：无。

实施待办（`docs/implementation/00-README.md`）：T-20（RDS 验证）与本模块无直接关系；`Quartz` 集群在 RDS 上的复测随 T-20（T-15 已注明）。

---

## 11. 框架负责 / 业务负责

| 框架负责 | Operations 负责 |
|---|---|
| 消息投递与处理、重试退避、死信标记、租约与状态条件（已实现） | 管理视图与人工操作的命令、页面、权限点 |
| 定时任务的声明、Quartz 装配、`CommandJob`、执行记录的写入、计划同步（已实现） | 覆盖值的维护与展示、立即执行、执行记录查询 |
| 作业的存储、执行、续期、取消机制（已实现） | 跨模块汇总列表、管理员取消入口、“我的任务”汇总 |
| 审计事件的采集与写入 Outbox（已实现）、只追加表约定与清理命令（已实现） | 订阅、落库、查询与导出 |
| 前端错误的日志分类、脱敏兜底（05-12） | 端点、白名单、限额 |
| 各模块的 `IModuleMessageStore`、`IModuleJobStore` 注册（第 13 节，待补） | 调用它们 |

---

## 12. 待决（均已按建议决定，2026-11-12）

下面的“建议”即决定。

### D-O1 是否提供“运行状态”页（依赖状态）

`/health/dependencies` 已实现，只在内网直连可访问（05-12 第 6.1 节）。

| 选项 | 做法 | 优点 | 缺点 |
|---|---|---|---|
| **A 不提供页面** | 运维通过内网访问端点，或由监控平台抓取 | 少一个页面与一个权限点；暴露面最小 | 没有观测平台权限的人看不到依赖状态 |
| B 在 Operations 提供页面，权限点 `Operations.Health.View` | 页面经 Operations 端点读同样的检查 | 管理员在系统里就能看到 | 依赖状态（数据库、对象存储、短信网关）经过了公网可达的 API；需要为它单独做访问控制与限速；与观测平台重复 |

- **建议**：A。**理由**：依赖状态本来就是运维与监控的数据，观测平台（05-12）更合适；首期不提供页面不影响任何业务。**取舍**：管理员无法在系统内查看依赖状态。

### D-O2 重建收尾“放弃本轮”要不要清除路由的“需要初始化”标记

05-07 第 8.3 节（实现补充）留了这件事（“到时一起定”）：启动任务看路由的“需要初始化”标记排队重建；一轮因“找不到的行数”超过阈值而暂停，运维核实后可以确认收尾（置终态）或放弃本轮。

| 选项 | 做法 | 优点 | 缺点 |
|---|---|---|---|
| **A 放弃即清除标记** | 放弃本轮后不再自动排队；需要时运维手动 `TriggerRebuild` | 不会在每次重启后重复同一轮必然暂停的重建；放弃是一个明确的“我已处理” | 放弃后如果问题其实没解决，系统不会自动再试，靠运维记得手动触发 |
| B 放弃保留标记 | 下次启动自动再排一轮 | 不会遗忘 | 若暂停原因是数据本身缺失（行就是找不到），每次发版重启都会重新扫一遍并再次暂停，也会反复告警 |

- **建议**：A。**理由**：放弃是人的明确决定，应当终结这件事；“重新触发”也是人的明确动作。放弃时写审计，界面提示“此后不会自动重建”。**取舍**：需要运维手动再触发。

### D-O3 审计权限是否与系统管理员分权

系统管理员角色 `GrantsAllPermissions`，自动拥有 `Operations.Audit.View`、`Audit.Export` 等敏感权限点；也就是说，管理员可以查看审计，包括自己的操作记录。

| 选项 | 做法 | 优点 | 缺点 |
|---|---|---|---|
| **A 保持现状，靠留痕** | 查看审计本身是 `[AuditedRead]`；授权配置变更写安全事件；对“给自己或他人授予审计权限”设告警 | 不新增框架机制；管理员的角色管理能力本来就能给自己授予任何东西，硬性分权在应用内无法真正做到 | 没有硬性的职责分离 |
| B 审计类权限点不自动授予系统管理员 | 在 `[Permission]` 上增加“不随全权限角色自动授予”的标记；审计员是单独的角色 | 默认配置下管理员看不到审计 | 要改 05-05 的同步规则；管理员仍可通过角色管理给自己或同伙授予，只是多一步，并留下安全事件；分权是否有效取决于是否同时限制了谁能管理角色 |
| C B + 要求审计角色的授予由第二个人批准 | | 最严格 | 需要审批流，超出平台模块范围 |

- **建议**：A，并把“是否以 OWASP ASVS 第 2 级为安全验收基线”（00-README 待决，阶段 3 结束）的结论作为是否升级到 B 的依据。**理由**：应用内的分权无法抵御有授权管理能力的内部人，真正有效的是留痕与告警；B 的收益在于“默认不暴露”，可以在验收标准明确后补。**取舍**：首期没有硬性分权，需要在运维手册里写明“审计角色的授予与审计查看都会被记录和告警”。

### 小项（按建议执行，已确认，2026-11-12）

| 事项 | 默认 |
|---|---|
| Cron 最短间隔 | 1 分钟 |
| 立即执行时任务正在运行 | 拒绝，409 |
| 死信载荷、审计字段值的显示 | 原样显示给有权限点的人，每次查看写读审计；不做二次脱敏（遮盖已在审计采集时完成，死信载荷按 05-07 第 5.3 节不含敏感字段） |
| 审计导出的行数上限 | 配置项，默认 100 万行；超过时要求缩小时间范围 |
| `RestoreJobDefault` | 保留行、两项置空（便于追溯） |

### 待确认

| 事项 | 需要什么 | 影响 |
|---|---|---|
| 审计保留期 | 业务与合规确认（00-README 待决，阶段 5） | `purge` 的参数、索引与分区的规划 |
| 审计量级估算 | 阶段 3 试运行观察：每日审计行数与平均行大小 | 是否需要分区、`Targets` GIN 索引的代价 |
| 前端错误端点的限额 | 与前端联调 | 8 节 |

---

## 13. 与其他文档的接口，以及框架需要补的公开服务

### 13.1 框架需要补的公开服务（阶段 3 开始实现 Operations 时，先在框架里完成）

05-07 第 9.1 节、05-08 第 7.1 节已规定接口形态，实现尚未做（对应记录里的“管理端点、心跳表→阶段 3”）：

| 服务 | 作用 | 出处 |
|---|---|---|
| `IScheduledJobControl` | 列出调度库里的任务与下次触发时间、同步覆盖值到触发器、立即触发一次（已实现，2026-11-13；另有 `Preview`，见 05-08 修订记录） | 05-08 第 5.3、7.1 节；现有 `ScheduleSynchronizer`、`SyncSchedulesTask` 是内部类型 |
| `IModuleMessageStore` | 每个模块一个，读写本模块的 Outbox / Inbox：积压汇总、死信、重试、放弃、补投 | 05-07 第 9.1 节 |
| `IModuleJobStore` | 每个模块一个，汇总与取消本模块的作业 | 05-08 D6 |
| `IAbandonPolicy<TRequest>` | 接收方为请求型消息注册的放弃前检查 | 05-07 第 9.1 节；Files 的 `AttachFiles` 是第一个使用者 |
| 路由与重建的管理服务 | 终结、重新激活、手动触发与收尾确认、放弃（D-O2） | 05-07 第 6.3、8 节；现有 `MessageRoutes`、`RebuildRunStore` 是内部类型 |
| `MessagingInstance` 心跳写入与建表语句 | 实例定期登记构建号与订阅清单 | 05-07 第 6.3 节 |
| 游标分页 | 审计列表使用 | 第 10 节；已实现（2026-11-13） |

这些都是“框架基础设施写入 / 读取 Operations 表与各模块框架表”的例外（05-08 第 5.4 节、05-07 第 9.1 节已确认），不新增决定。

### 13.2 需要同步修订的文档

按 R01 L2 修订并登记修订记录（只落实决定）。**2026-11-12 已完成**，其中 `S2-框架骨架.md` 一项见该清单的进展记录：

| 文档 | 修订 | 来源 |
|---|---|---|
| 05-07 第 8.3 节（实现补充） | “放弃时是否清除路由的‘需要初始化’标记”按 D-O2 定稿，并把“待用户确认”的标注去掉（连同其余已确认的实现补充） | D-O2 |
| 05-12 第 2.5 节 | “各字段有长度上限（阶段 1 定）”改为引用本文第 8 节的数值 | 本文 8 节 |
| 05-12 第 6.1 节 | D-O1 选 A 时，`/health/dependencies` 的“或要求权限点 `Operations.Health.View`”一句改为只保留内网访问 | D-O1 |
| `docs/implementation/S2-框架骨架.md` | 游标分页（S2-12 余项）标为随 Operations 实现；第 13.1 节的公开服务列为阶段 3 的开工项 | 本文 10、13.1 |
| `docs/glossary.md`、`docs/modules/README.md` | 登记第 2 节术语与模块状态 | 本文 |

---

### 实现时的其他差异（2026-11-13）

- 聚合与表都叫 `AuditLog`（设计写的聚合名 `AuditEntry`）：模型规则要求业务实体的表名等于类名，而框架的清理命令（`purge --table audit-log`）认的表就是 `"Operations"."AuditLog"`。强类型 ID 因此叫 `AuditLogId`。
- 写入时间用审计的创建时间列（`CreatedAt`），不另设 `ReceivedAt`；`Targets`、`Changes`、`Details` 存 `jsonb`，领域里是 JSON 文本；`ClientIp` 是 `inet`。
- 按操作对象筛选用 JSON 包含运算（`@>`）并走 GIN 索引。JSON 函数是 Npgsql 专有的，Application 不引用驱动，所以放在查询入口 `IOperationsQueryDb.WithTarget` 里，由 Infrastructure 实现。
- 订阅 `Operations.RecordAudit` 把 `Framework.AuditRecorded` 转成 `RecordAuditCommand`，按审计 ID 幂等（先查再写，重复投递只写一次）。消息从产生端的发件箱经路由到这里的收件箱的整条链路，由跑 Api 加 Worker 两个角色的宿主测试（`WorkerRoleTests`，2026-11-13）走通：一个带审计的命令执行后，审计行出现在审计表里，列表接口能查到。
- `GET audit`、`GET audit/{id}` 已实现；`Operations.Audit.View` 是目前唯一的权限点，其余随用例加入。列表缺少 `from`/`to` 时由请求绑定直接返回 400（不带逐字段错误），超过 93 天或结束早于开始是逐字段错误。
- Operations 的初始迁移已建消息路由表、定时任务设置与执行记录表、Quartz 表（框架提供建表语句，`MessageRouteTable`、`SchedulingTables` 改为公开）；测试用的 `FrameworkSchema` 增加 `operationsHasMigrations`，API 测试不再代建。`MessagingInstance` 心跳表仍未做。

### 实现时的其他差异：定时任务（2026-11-13）

- 设置聚合叫 `ScheduledJobSetting`（设计写的 `JobOverride`）：模型规则要求实体类名等于表名，而这张表框架已经按这个名字建好并读取。表在初始迁移里由框架的建表语句建成（任务名做主键），新增迁移 `ScheduledJobSettingAggregate` 把它改成聚合的形状：`Id` 做主键、任务名唯一、补上创建与并发版本列；框架读它时只用任务名、开关、Cron 三列，不受影响。
- 保存覆盖用整体替换：给空的项恢复成跟着默认值（`PUT …/override`）；“恢复默认”是单独的命令，设置行保留、两项清空。已经从代码里删除的任务只在列表里标为“已删除”，不能再改（没有触发器可同步）。
- 版本号规则与系统设置一致：从来没改过的任务没有设置行，调用方不带版本；已经有行（含恢复默认后留着的）就要带列表里的 `overrideVersion`，否则 409。
- 保存后的同步放在提交之后执行，失败只记警告、不影响保存；列表每项带 `inSync`（调度器里的状态与按设置应有的状态是否一致），改了设置却没同步上时界面能看出来。
- Cron 的最短间隔用预览出的相邻触发时间判断（秒位只有一个取值才不会一分钟内触发多次，头几次触发里就能看出来）；预览、保存都走同一条规则，错误都是 `Operations.CronInvalid` 加 `cron` 字段错误。
- 立即执行：任务正在任何实例上运行时返回 409 `Operations.JobAlreadyRunning`（查 Quartz 集群共享的“正在触发”表，不看本实例）；触发写进调度库，由运行着调度器的实例执行，执行记录里触发方式是手动、触发人是操作人（模拟登录时是真正动手的人）。触发不在命令的事务里：事务之后才失败时任务已经交给调度器，任务本来就要求可以重复执行，这一点接受。
- 执行记录用原生 SQL 映射到只读行类型（表由框架的调度器写），状态与触发方式在查询里换成编码文字，响应里是智能枚举 `JobRunStatus`、`JobRunTrigger`；翻页用游标（开始时间加 ID，行 ID 是 Guid 的重载）。
- 动作目录 `ScheduledJobActions`：`Edit`、`RestoreDefault`（有覆盖才有）、`RunNow`，都要 `Operations.Jobs.Manage`；资源策略是一条全部允许的空策略（动作目录要求每类资源有一个策略，权限点就是授权的全部）。
- 权限点 `Operations.Jobs.View`、`Operations.Jobs.Manage` 随用例加入（非敏感，沿用 05-08 第 7.1 节）。

---

## 14. 决策汇总

| 编号 | 事项 | 决定（已确认，2026-11-12） |
|---|---|---|
| D-O1 | 是否提供“运行状态”页 | A：不提供，依赖状态留给内网端点与观测平台 |
| D-O2 | 重建“放弃本轮”是否清除“需要初始化”标记 | A：清除，之后只靠手动触发 |
| D-O3 | 审计权限是否与系统管理员分权 | A：保持现状，靠留痕与告警；ASVS 基线确定后再看是否升级 |

| 待确认 | 处理方式 |
|---|---|
| 审计保留期、量级、前端错误限额 | 见 12 节“待确认” |
