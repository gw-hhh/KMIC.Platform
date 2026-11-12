# 术语表

一个概念一个名字（06 D3）。标识符只用英文；业务概念的英文名统一在这里登记，**先登记、再写代码**，评审时对照。同一个中文词在不同模块含义不同时分两行登记。

- 格式：`中文术语 | 英文标识符 | 所属模块 | 含义 | 备注`。“所属模块”为“框架”的是 BuildingBlocks 与前端框架中的概念。
- 英文标识符里通用词典没有的词，同时加入拼写检查的项目词典 `.cspell/kmic-terms.txt`。
- 本表初版（2026-11-05）只收框架概念；业务术语由各业务模块设计文档的“2 术语”一节登记（07 第 8 节）。

| 中文术语 | 英文标识符 | 所属模块 | 含义 | 备注 |
|---|---|---|---|---|
| 聚合 / 聚合根 | `Aggregate` / `AggregateRoot` | 框架 | 一组必须一起保持一致的对象，外部只经聚合根修改 | 05-01 |
| 实体 | `Entity` | 框架 | 有标识的领域对象 | 05-01 |
| 值对象 | `ValueObject` | 框架 | 没有标识、按值比较的不可变对象 | 05-01 |
| 强类型 ID | `StronglyTypedId`（各类型名为 `XxxId`） | 框架 | 包装 UUID v7 的标识类型，不同实体的 ID 不能混用 | ADR-0009 |
| 智能枚举 | `Enumeration` | 框架 | 带编码与显示名的封闭取值集合 | 05-01 第 6 节 |
| 状态机 | `StateMachine` | 框架 | 声明状态与允许的流转 | 05-01 第 7 节 |
| 领域事件 | `DomainEvent` | 框架 | 聚合内发生的、本模块关心的事实 | 05-01 第 8 节 |
| 集成事件 | `IntegrationEvent` | 框架 | 通过 Outbox 发给其他模块的事实 | 05-07 |
| 结果 / 错误 | `Result` / `Error` | 框架 | 预期内的失败用返回值表达，不抛异常 | ADR-0010 |
| 命令 / 查询 | `Command` / `Query` | 框架 | 修改数据的请求 / 只读请求 | ADR-0003、05-02 |
| 处理器 | `Handler` | 框架 | 执行命令或查询的类 | 05-02 |
| 收据 | `Receipt` | 框架 | 命令的响应，只含标识、版本、状态等最小信息 | 05-02 第 2.2 节 |
| 逐条收据 | `ItemReceipt` | 框架 | 批量命令中每一项的结果 | 05-02 第 2.2 节 |
| 幂等键 | `IdempotencyKey` | 框架 | 同一个写请求重试时携带的相同键，保证只执行一次 | 05-02 第 5 节 |
| 结果未知 | `ResultUnknown` | 框架 | 写请求已发出、但无法确认是否已提交 | 05-02 第 6.1 节 |
| 版本号（并发） | `Version` | 框架 | 乐观并发控制的版本，修改时必须携带 | 05-03 |
| 发件箱 / 收件箱 | `Outbox` / `Inbox` | 框架 | 与业务数据同事务写入的待发消息 / 已处理消息记录 | ADR-0019、05-07 |
| 读模型 | `ReadModel` | 框架 | 从其他模块的事件同步来的只读副本 | 05-07 第 8 节 |
| 后台作业 | `BackgroundJob` | 框架 | 用户发起、在后台执行的长任务（如导出） | 05-08 |
| 定时任务 | `ScheduledJob` | 框架 | 按时间表执行的任务 | 05-08 |
| 权限点 | `Permission` | 框架 | 一个可授予的操作权限，编码全系统唯一 | 05-05 |
| 角色 | `Role` | Identity | 权限点的集合 | 05-05 |
| 数据范围 | `DataScope` | 框架 | 用户能看到哪些数据（按组织等维度） | 05-05 第 5 节 |
| 允许的动作 | `AllowedActions` | 框架 | 详情接口返回的、当前用户对这条数据能做的动作 | 05-05 |
| 组织单元 | `OrgUnit` | Identity | 组织树上的节点 | ADR-0031 |
| 会话 | `Session` | Identity | 一次登录的服务端记录 | 05-04 |
| 访问上下文 | `AccessContext` | 框架 | 一次请求中当前用户、角色、权限与数据范围的快照 | ADR-0026 |
| 模拟身份 | `Impersonation` | Identity | 管理员以他人身份查看（默认只读） | 05-04 第 9 节 |
| 偏好 | `Preference` | Configuration | 按“内置 → 系统 → 角色 → 用户”四层覆盖的个人设置 | 05-10 第 7 节 |
| 系统设置 | `Setting` | Configuration | 管理员维护的全局设置 | 05-10 |
| 字典 | `Dictionary` / `DictionaryItem` | Configuration | 运行时可维护的选项集合 | 05-10 |
| 功能开关 | `FeatureFlag` | 框架 | 按配置启用或停用某个功能 | 05-10 D6 |
| 上传档案 | `UploadProfile` | Files | 一类附件的大小、类型与权限规定 | 05-11 第 3.1 节 |
| 编号（单号） | `Number` | 框架 | 按规则生成的业务单号 | 05-13 |
| 业务时区 | `BusinessTimeZone` | 框架 | 业务日期、显示时间所用的时区 | 05-13 |
| 审计日志 | `AuditLog` | Operations | 谁在何时做了什么的记录 | 05-09 |
| 业务历史 | `BusinessHistory` | 框架 | 单据状态流转的记录 | 05-09 |
| 用户 | `User` | Identity | 能登录系统的人或系统账号 | Identity 设计 2 |
| 账号 | `Account` | Identity | 登录名，唯一，不区分大小写；迁移用户沿用旧系统的手机号 | Identity 设计 2 |
| 授予 | `UserRoleGrant` | Identity | 用户拥有某个角色，可带锚点 | Identity 设计 2 |
| 锚点 | `GrantAnchor` | Identity | 授予上指定的组织单元，作为“本组织及下级”的计算起点 | 05-05 D5 |
| 受保护角色 | `ProtectedRole`（`Role.IsProtected`） | Identity | 只能由受保护角色的持有人管理的角色，如系统管理员 | 05-05 D11 |
| 权限点目录 | `PermissionCatalog` | Identity | 代码中声明、启动时同步到数据库的权限点集合 | 05-05 第 3.2 节 |
| 刷新令牌 | `RefreshToken` | Identity | 换取新访问令牌的不透明令牌，只存哈希 | 05-04 |
| 验证码 | `VerificationCode` | Identity | 短信验证码，只存哈希，一次性、次数有限 | 05-04 第 7.3 节 |
| 登录限制 | `LoginThrottle` | Identity | 按登录名累计的失败次数与等待、锁定状态；不是用户状态 | 05-04 D4 |
| 安全事件 | `SecurityEvent` | Identity | 登录、授权拒绝、授权配置变更等记录，独立通道写入 | 05-09 第 6 节 |
| 模拟会话 | `ImpersonationSession` | Identity | 管理员以他人身份查看系统的会话 | 05-04 第 9 节 |
| 系统账号 | `SystemUser` | Identity | 代表“系统”的固定账号，不可登录 | 05-03 第 4 节 |
| 审计记录 | `AuditEntry` | Operations | `AuditLog` 表中的一行：一次命令执行的汇总 | Operations 设计 2 |
| 定时任务覆盖值 | `JobOverride` | Operations | 对代码里默认开关与 Cron 的修改，只存被改过的项 | Operations 设计 2 |
| 订阅路由 | `MessageRoute` | Operations | 消息类型到订阅的登记，决定投递到哪些 Inbox | 05-07 第 6.3 节 |
| 终结 / 重新激活 | `Finalize` / `Reactivate` | Operations | 订阅路由的运维操作 | 05-07 第 6.3 节 |
| 补投 | `Redeliver` | Operations | 按消息类型与序号区间向指定订阅重新投递 | 05-07 第 9 节 |
| 死信 | `DeadLetter` | 框架 | 重试用尽或不可重试的消息 | 05-07 |
| 字典类型 | `DictionaryType` | Configuration | 一类候选值，由代码声明；类型是聚合根，字典项是子实体 | Configuration 设计 2 |
| 层级字典 | `IsHierarchical` | Configuration | 字典项可以有父项的字典类型 | Configuration 设计 3.2 |
| 设置值 / 设置变更 | `SystemSettingValue` / `SystemSettingChange` | Configuration | 被修改过的设置值（没有行表示用代码默认值）/ 一次修改的新旧值与原因，只追加 | Configuration 设计 3.3 |
| 偏好层 / 偏好族 | `PreferenceLayer` / `PreferenceFamily` | Configuration | `System` / `Role` / `UserRole` 三层；键带后缀的偏好集合（`Ui.Shell`、`Ui.Table.*`） | 05-10 第 7 节 |
| 文件 | `StoredFile` | Files | 一个已上传或生成的文件的元数据，内容在对象存储 | Files 设计 2 |
| 引用 | `FileReference` | Files | 文件被某个业务对象关联的登记，只由 `AttachFiles` / `DetachFiles` 维护 | 05-11 第 3.3 节 |
| 临时文件 | `Available`（未关联） | Files | 已上传、尚未被业务关联的文件，到期成为清理候选 | 05-11 第 4.2 节 |
| 生成文件 | `GeneratedFile` | Files | 作业或系统生成的文件（导出结果、PDF），不经上传 | 05-11 第 4.4 节 |
| 文件描述 | `FileDescriptor` | Files | 文件的元数据（不含内容），业务下载查询据它组装 | 05-11 第 5.1 节 |
| 存储问题 | `StorageIssue` | Files | 对账发现的“库中有记录、存储中没有对象”，等待还原 | Files 设计 3.3 |
| 彻底删除 | `Purge`（`Purged`） | Files | 误传敏感文件时应用层删除对象、元数据标为已删除；历史版本由运维另行清除 | 05-11 第 6 节 |
| 通知 / 投递 | `Notification` / `Delivery` | Notifications | 一次发送请求的记录 / 通知在一个渠道上给一个收件人的一次发送 | Notifications 设计 2 |
| 渠道 | `Channel` | Notifications | `Email` / `Sms` / `InApp` | 05-11 第 8 节 |
| 通知模板 / 模板版本 | `NotificationTemplate` / `TemplateVersion` | Notifications | 代码声明的一种通知 / 管理员发布的修改后内容，只增不改 | 05-11 第 7.4 节 |
| 含密模板 | `CarriesSecret` | Notifications | 内容含一次性短时凭据（验证码）的模板，投递后清空正文 | Identity 设计 D-F |
| 发件账号 | `SenderProfile` | Notifications | 部署配置里的一组 SMTP 账号，模板按场景选择 | 05-11 第 7.4 节 |
| 去重键 | `DedupKey` | Notifications | 业务给出的“同一件事只通知一次”的键 | 05-11 第 7.2 节 |
| 站内消息 | `SiteMessage` | Notifications | 只有收件人本人可见的系统内消息，带已读状态 | 05-11 第 8.3 节 |
| 捕获消息 | `CapturedMessage` | Notifications | 非生产环境本应发出的内容，供测试人员查看 | 05-11 第 8.4 节 |
| 页面实例 | `PageInstance` | 前端框架 | 一个标签对应的页面及其未保存、保存中、结果未知等状态 | F03 第 6 节 |
| 工作区 | `Workspace` | 前端框架 | 标签的唯一可写来源 | F03 第 5.2 节 |
| 业务表格 | `KTable` | 前端框架 | 基于 VXE Table 的列表表格 | F07 |
| 表格键 | `tableKey` | 前端框架 | `模块.页面.表格`，用于保存列设置 | F03 D5、F07 |
