# 中文整理交接摘要

更新时间：2026年11月15日 12:34:55（UTC+8）。本轮规则同步 C2641～C2651，收尾 C2652；下一编号 C2653。未新增源码改动或提交。

## 本轮规则同步结果（2026年11月15日 12:34:55，UTC+8）

- 结果：已同步根 CLAUDE.md、06 编码规范和框架入口修订记录，11 项 C2641～C2651；C2652 收尾，下一编号 C2653。此前已有准确要求保留，只补执行缺口。
- [根规则](../../CLAUDE.md)：覆盖所有中文，包括注释、文案、提示、异常、校验信息、字段说明、接口文档、特性理由、提交与交接说明及回复。
- [编码规范 10.3 与附录 A](../framework/06-coding-standards.md#103-注释写法)：先核实含义并判断必要性；多余的删、简单的写短、必要约束说清楚；按用途决定是否写，不机械补摘要、参数或 inheritdoc；已准确顺口的保持稳定，修改后整篇复读及回查同类说明。
- 增加具体删减示例：名称与代码已明确的 New/取值/转发说明可省；配置字段保留含义，简单 TimeSpan 换算不重复说明；普通 Handle 的无价值模板可省；复杂查询、权限、锁、事务和失败行为须保留。现有 CS1591、OpenAPI 与列说明要求保持。
- [框架入口](../framework/00-README.md)追加修订记录；没有修改业务设计、检查脚本或工程约束。
- 核对：3 份规则文件精确替换回放一致；本轮只改 3 份规则文档和 7 份跟踪文档，2695 个范围外文件字节不变；源码、检查脚本和原有本地成果保持原样。未编译、未运行测试、项目脚本或数据库操作；未暂存、提交或推送；当前 66 个已跟踪修改，无未跟踪，暂存区为空，HEAD 仍为 2d1a81f。
- 边界：此前讨论的 New/Handle/换算属性等源码删减示例本轮未执行；本轮完成的是规则落文，不能写成源码已删减。已有 3 条持久化旧措辞及行为待办仍按前轮边界保留。

| 文件 | 逐项记录 |
|---|---|
| [CLAUDE.md](../../CLAUDE.md) | C2641 |
| [06 编码规范](../framework/06-coding-standards.md) | C2642～C2650 |
| [框架入口](../framework/00-README.md) | C2651 |

原文、改后内容、UTC+8 时间和核对依据见[修改记录](修改记录.md)；C2652 为本轮交接收尾，下一实际编号 C2653。下方按日期保留前轮源码结果。

## 本轮中文表达复盘（2026年11月15日 12:14:12，UTC+8）

- 结果：按用途复盘 1267 个源码及构建部署文件的中文，修订 34 文件；C2568～C2639 共 72 次逐处操作，含整篇复读后的修订，C2640 登记收尾。本轮重点是读起来是否顺口、说明是否必要；没有为统一措辞重写已有清楚内容。
- 当前状态：2664 个本轮范围外既有文件 SHA-256 不变，包括前轮规范成果、原有 BuildingBlocks/测试修改及全部迁移快照；当前 65 个已跟踪修改，无未跟踪，暂存区为空。本轮实际改动 41 文件（34 源码/脚本/模板加 7 份跟踪文档），原有 32 个本地修改均保留；check.cs 在前轮规则改动基础上只改文字。
- 核对：34 文件精确修改回放一致；Roslyn 只读解析无语法错误；除中文文字及 check.cs 汇总中删除的两处 files.Length 展示外，其余可执行标记一致；1173 个范围内 C# 文件只读扫描未发现旧引用标记。check.cs 删除候选文件总数的展示，避免把跳过的二进制文件、锁文件等算作已检查；条件、调用、检查顺序、计数与退出行为不变。
- 限制：未编译、未运行测试、项目检查脚本或数据库操作；未暂存、提交或推送，HEAD 仍为 2d1a81f；静态核对不代表运行验收。未改持久化属性摘要、HasComment、存储值或迁移；没有降低检查标准。

### 本轮改动与逐项记录

| 文件 | 修改位置 | 记录 |
|---|---|---|
| [check.cs](../../scripts/check.cs) | 文件头：用途、失败行为与修复选项；前端检查前的依赖说明；接口类型检查步骤标题；依赖边界检查步骤标题；密钥扫描前说明；RunWhenProjectsExist 前重复注释；空解决方案提示；RunWeb 前重复注释；缺少前端目录提示；Run Windows 启动原因；CheckEncoding 前重复范围说明；NuGet 锁文件例外原因；修复单文件提示；BOM 提示；换行提示；编码检查结果汇总；ListFiles 集中说明；CheckTemporaryReferences 前规则说明；脚本自身排除原因；旧引用标记错误提示；旧引用扫描汇总；TemporaryReference 正则说明 | C2568、C2569、C2570、C2571、C2572、C2573、C2574、C2575、C2576、C2577、C2578、C2579、C2580、C2581、C2582、C2583、C2584、C2585、C2586、C2587、C2588、C2589 |
| [DeclaredValues.cs](../../src/Modules/Configuration/Kmic.Configuration.Application/DeclaredValues.cs) | 类型用途；Parse.invalidWith 参数 | C2590、C2591 |
| [PreferenceSource.cs](../../src/Modules/Configuration/Kmic.Configuration.Application/Preferences/PreferenceSource.cs) | 类型用途 | C2592 |
| [SettingActions.cs](../../src/Modules/Configuration/Kmic.Configuration.Application/Settings/SettingActions.cs) | 操作目录用途 | C2593 |
| [RoleActions.cs](../../src/Modules/Identity/Kmic.Identity.Application/Roles/RoleActions.cs) | 操作目录用途 | C2594 |
| [UserActions.cs](../../src/Modules/Identity/Kmic.Identity.Application/Users/UserActions.cs) | 操作目录用途 | C2595 |
| [FileAttachments.cs](../../src/Modules/Files/Kmic.Files.Application/Attachments/FileAttachments.cs) | CheckAsSystemAsync 重复注释；系统作用域豁免理由 | C2596、C2597 |
| [ChangePasswordCommand.cs](../../src/Modules/Identity/Kmic.Identity.Application/Authentication/ChangePassword/ChangePasswordCommand.cs) | NotIdempotent 理由 | C2598 |
| [LoginCommand.cs](../../src/Modules/Identity/Kmic.Identity.Application/Authentication/Login/LoginCommand.cs) | NotIdempotent 理由；NotIdempotent 理由整篇复读 | C2599、C2633 |
| [RefreshTokenCommand.cs](../../src/Modules/Identity/Kmic.Identity.Application/Authentication/Refresh/RefreshTokenCommand.cs) | NotAudited 理由；NotIdempotent 理由；NotIdempotent 理由整篇复读 | C2600、C2601、C2634 |
| [CapturedDetailResponse.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Captured/Get/CapturedDetailResponse.cs) | 类型 summary；Address 字段说明；Role 字段说明 | C2602、C2610、C2612 |
| [CapturedListItem.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Captured/List/CapturedListItem.cs) | 类型 summary；Address 字段说明；Role 字段说明 | C2603、C2611、C2613 |
| [DeliveryDetailResponse.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Get/DeliveryDetailResponse.cs) | 类型 summary；Role 字段说明 | C2604、C2614 |
| [DeliveryListItem.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/List/DeliveryListItem.cs) | 类型 summary | C2605 |
| [TemplateDetailResponse.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Templates/Get/TemplateDetailResponse.cs) | 类型 summary；Example 字段说明 | C2606、C2620 |
| [TemplateListItem.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Templates/List/TemplateListItem.cs) | 类型 summary | C2607 |
| [TemplatePreviewResponse.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Templates/Preview/TemplatePreviewResponse.cs) | 类型 summary | C2608 |
| [InventoryObject.cs](../../src/Modules/Files/Kmic.Files.Application/InventoryObject.cs) | 类型 summary | C2609 |
| [RecipientRole.cs](../../src/Modules/Notifications/Kmic.Notifications.Contracts/RecipientRole.cs) | 枚举类型 summary | C2615 |
| [DeliveryRole.cs](../../src/Modules/Notifications/Kmic.Notifications.Domain/Deliveries/DeliveryRole.cs) | 枚举类型 summary | C2616 |
| [TemplateChannelInfo.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Templates/Get/TemplateChannelInfo.cs) | 类型用途；DefaultSubject 字段说明；DefaultBody 字段说明 | C2617、C2618、C2619 |
| [TemplateVersionInfo.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Templates/Get/TemplateVersionInfo.cs) | 类型用途 | C2621 |
| [ClearSecretBodiesCommand.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Purge/ClearSecretBodiesCommand.cs) | remarks 清理条件 | C2622 |
| [FluidTemplateRenderer.cs](../../src/Modules/Notifications/Kmic.Notifications.Infrastructure/Rendering/FluidTemplateRenderer.cs) | 未声明变量错误 | C2623 |
| [ListDeliveriesQuery.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/List/ListDeliveriesQuery.cs) | From 时间筛选说明；To 时间筛选说明；remarks 列表和审计说明整篇复读 | C2624、C2625、C2639 |
| [ListDeliveriesRequest.cs](../../src/Modules/Notifications/Kmic.Notifications.Presentation/Deliveries/ListDeliveriesRequest.cs) | From 时间筛选说明；To 时间筛选说明 | C2626、C2627 |
| [ListMySiteMessagesQuery.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/SiteMessages/List/ListMySiteMessagesQuery.cs) | Unread 字段说明 | C2628 |
| [ListMySiteMessagesRequest.cs](../../src/Modules/Notifications/Kmic.Notifications.Presentation/SiteMessages/ListMySiteMessagesRequest.cs) | Unread 字段说明 | C2629 |
| [SetSettingEndpoint.cs](../../src/Modules/Configuration/Kmic.Configuration.Presentation/Settings/SetSettingEndpoint.cs) | WithDescription 常规流程复述 | C2630 |
| [new-module.cs](../../scripts/new-module.cs) | 文件头使用说明；Normalize 步骤标题 | C2631、C2637 |
| [CustomerModule.cs](../../templates/kmic-module/src/Modules/Customer/Kmic.Customer.Infrastructure/CustomerModule.cs) | 服务注册类 remarks | C2632 |
| [Recipient.cs](../../src/Modules/Notifications/Kmic.Notifications.Contracts/Recipient.cs) | Role 参数说明 | C2635 |
| [DeliveryItem.cs](../../src/Modules/Notifications/Kmic.Notifications.Infrastructure/Sending/DeliveryItem.cs) | Role 参数说明 | C2636 |
| [GetAuditQuery.cs](../../src/Modules/Operations/Kmic.Operations.Application/Audit/Get/GetAuditQuery.cs) | 查询用途 | C2638 |

逐处原文、改后文字、UTC+8 时间及实现依据见[修改记录](修改记录.md) C2568～C2639；C2640 为本轮交接收尾。[复核清单](Codex-中文源码全面复核清单.md)保存 1267 文件当前结论；历史轮结果按日期理解。

### 本轮已采用的表达尺度

| 原表达 | 处理后 | 判断依据 |
|---|---|---|
| check.cs 文件头重复列出全部步骤 | 保留用法、失败退出、Docker 和 --fix；执行步骤看下方数组 | 不维护重复目录，调用前提和写文件行为仍清楚 |
| 已规整 | 已修复 | 使用常用动词 |
| 检查候选文件总数，问题若干处 | 发现若干处编码或换行问题 | 不把跳过项计作实际检查量 |
| 一条捕获内容的详情 | 捕获内容详情 | 标题写名称即可 |
| 带逐字段错误的同一个错误 | 生成包含字段错误的业务错误 | 参数实际是错误生成函数 |
| 响应含令牌，所以不记审计 | 仅轮换会话凭据，令牌重用通过安全事件记录 | 实际审计不自动记录响应，不能凭假设写理由 |
| 查询记录读取审计 | 查询成功后记录读审计 | 补清谓语和实际记录条件 |

### 稳定维护建议与结束条件

建议这次集中审读完成后结束全仓改写，按具体问题维护。此建议不改变业务设计或质量检查配置。

| 维护方式 | 效果与取舍 | 建议 |
|---|---|---|
| 反复全仓扫描并换词 | 容易发现新候选，也会重复改写已清楚的句子，难形成稳定版本 | 不作为日常方式 |
| 固定当前尺度，随业务变更或具体问题修改 | 保持稳定；需要明确指出误导、病句、重复或遗漏，不能只因个人偏爱换词 | 推荐 |

本轮结束条件已经逐项完成：全量中文归类审读；疑点结合实现核实；同类问题回查；34 个改动文件整篇复读；差异与保护核对；保留项和边界写明。自然度不靠禁词表、固定字数或注释标签数量判定。以后只有含义变化、实际误导、明显不顺或无价值重复才改；运行验收和范围外行为待办不冒充已完成。

### 保留的持久化说明

以下 3 条说明会进入数据库列注释，本轮没有改动，也没有把它们当成已经精修合格。改善这些表述须另行确定模型变更范围，不能只改摘要却留下旧模型，亦不直接改旧迁移。

| 文件 | 属性 | 当前保留措辞 |
|---|---|---|
| [CapturedMessage.cs](../../src/Modules/Notifications/Kmic.Notifications.Domain/Captured/CapturedMessage.cs) | Address、Role | 本该发往的完整地址；邮件里的角色 |
| [Delivery.cs](../../src/Modules/Notifications/Kmic.Notifications.Domain/Deliveries/Delivery.cs) | Role | 邮件里的角色 |

CN-01/02/03、既有 API IDE0005、Sample 设置未接入业务及 BuildingBlocks 旧术语继续另列，未作为文案问题顺带改行为。接手前重核 Git 来源；最新记录 C2640，下一编号 C2641。

## 上一轮按新规范复查结果（2026年11月15日 11:43:25，UTC+8）

- 本轮进度：已完成；按调整后规范筛查 1267 个源码及构建部署文件，其中 1179 个含中文，共 5504 行；核实候选实现后只修订 14 文件/14 处（C2553～C2566），其余保持原样；C2567 登记复查收尾。
- 结论：主要修正登记时间、失败状态、异步请求与实际完成、事务内标记与已持久化、严格渲染和预览校验边界，并删除两处低价值重复。Configuration 现有中文保留；未为统一格式重写简单成员。
- 当前 Git：2684 个本轮范围外既有文件逐文件 SHA-256 不变，包含上一轮规范/脚本成果和原有 BuildingBlocks、测试改动；当前 32 个已跟踪修改，无未跟踪，暂存区为空。新增源码文字成果为本节 14 文件，前轮 3 规范文档/1 检查脚本仍保留未提交；7 份跟踪文档继续同步。按用户要求未编译、未运行测试、整套检查脚本或数据库操作；本轮未暂存、提交或推送，HEAD 保持 2d1a81f。
- 静态核对：14 处精确替换回放一致；Roslyn 只读解析 14 文件无语法错误，除注释和两处 WithDescription 中文字符串外，可执行标记完全一致；未改持久化属性摘要或 HasComment，ChannelList 已确认未映射列。只读静态解析不代表产品编译或测试验收。

| 文件 | 修改位置 | 记录 |
|---|---|---|
| [FileSort.cs](../../src/Modules/Files/Kmic.Files.Application/Administration/FileSort.cs) | Map 默认排序说明 | C2553 |
| [RejectUploadCommandHandler.cs](../../src/Modules/Files/Kmic.Files.Application/Uploads/Reject/RejectUploadCommandHandler.cs) | RejectUploadCommandHandler summary | C2554 |
| [RecordHistoryPurgeRequest.cs](../../src/Modules/Files/Kmic.Files.Presentation/Administration/RecordHistoryPurgeRequest.cs) | RecordHistoryPurgeRequest summary | C2555 |
| [PublishTemplateCommand.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Templates/Publish/PublishTemplateCommand.cs) | PublishTemplateCommand remarks 发布校验 | C2556 |
| [PublishTemplateEndpoint.cs](../../src/Modules/Notifications/Kmic.Notifications.Presentation/Templates/PublishTemplateEndpoint.cs) | 发布接口 WithDescription | C2557 |
| [PreviewTemplateEndpoint.cs](../../src/Modules/Notifications/Kmic.Notifications.Presentation/Templates/PreviewTemplateEndpoint.cs) | 预览接口 WithDescription | C2558 |
| [CancelMyJobCommandHandler.cs](../../src/Modules/Operations/Kmic.Operations.Application/BackgroundJobs/Mine/CancelMyJobCommandHandler.cs) | CancelMyJobCommandHandler summary | C2559 |
| [SetJobOverrideRequest.cs](../../src/Modules/Operations/Kmic.Operations.Presentation/ScheduledJobs/SetJobOverrideRequest.cs) | Cron 参数的间隔约束 | C2560 |
| [RetryMessagesCommandHandler.cs](../../src/Modules/Operations/Kmic.Operations.Application/Messages/Retry/RetryMessagesCommandHandler.cs) | RetryMessagesCommandHandler remarks | C2561 |
| [RotationOutcome.cs](../../src/Modules/Identity/Kmic.Identity.Domain/Sessions/RotationOutcome.cs) | Reused 结果说明 | C2562 |
| [UpdateUserProfileRequest.cs](../../src/Modules/Identity/Kmic.Identity.Presentation/Users/UpdateUserProfileRequest.cs) | Account 字段说明 | C2563 |
| [SampleUploadProfiles.cs](../../src/Modules/Sample/Kmic.Sample.Application/SampleUploadProfiles.cs) | 上传权限 remarks | C2564 |
| [RemoveSpecimenAttachmentCommandHandler.cs](../../src/Modules/Sample/Kmic.Sample.Application/Specimens/Attachments/Remove/RemoveSpecimenAttachmentCommandHandler.cs) | 解除关联调用前的事务说明 | C2565 |
| [NotificationTemplate.cs](../../src/Modules/Notifications/Kmic.Notifications.Domain/Templates/NotificationTemplate.cs) | ChannelList 计算属性 summary | C2566 |

每处原文、改后文字、UTC+8 时间及实现证据见[修改记录](修改记录.md) C2553～C2566；C2567 为范围及交接收尾。1267 文件逐项结论见[复核清单](Codex-中文源码全面复核清单.md)。

## 此前本地提交与规范调整（本轮开始前）

- 已按用户要求先本地提交一版：`2d1a81fe9bf90a64cb7ef9dc3ff30abe01e60ffe`，132 文件（119 源码、6 个此前授权生成并已验证的注释迁移相关文件、7 份跟踪文档）。未推送、未合并 main。C2518 记录这次一次性授权。
- 随后完成规范与脚本调整：3 份规范文档、1 个检查脚本，33 项 C2519～C2551；C2552 为收尾记录。后续成果仅留工作区，不再次暂存提交，不推送。
- 分支 `codex-cn-copy-cleanup-2026-11-14`，HEAD `2d1a81fe9bf90a64cb7ef9dc3ff30abe01e60ffe`。当时 18 个已跟踪修改：规范轮 11 文件（4 个目标和 7 份跟踪文档），加原有 7 个 BuildingBlocks/测试改动；当前状态见上方本轮结果。
- 既有业务源码文案保持提交版本。本轮没有批量转换 XML 排版、增删现有业务注释，也未改业务逻辑、测试、web/、BuildingBlocks、迁移或快照。
- 前轮 1267 文件复查及 119 源码/199 项文字修订已随提交保存，下面的原文示例与完整路径表用于追溯；不把此前“未提交”或旧规范描述当作当前状态。

## 前轮保留的规范与检查脚本调整（未提交）

| 文件 | 调整内容 | 逐项记录 |
|---|---|---|
| [06 编码规范](../framework/06-coding-standards.md) | 按信息价值决定是否说明；public/private 不决定注释数量；允许自然标点与单行/多行 XML；标签按需、参数组完整；保留有价值术语与引用，准确文字保持稳定 | C2519～C2539、C2545～C2546 |
| [框架入口](../framework/00-README.md) | 同步用户已确认的规范尺度及修订记录，功能验收状态不变 | C2540～C2541 |
| [检查脚本](../../scripts/check.cs) | 移除禁词、章节形状、句号及标签行数硬检查；仅拦截明确的旧临时引用标记；保留全部其他质量步骤 | C2542～C2544 |
| [数据访问说明](../framework/05-infrastructure/05-03-数据访问.md) | 与新规范对齐，按实际机制说明编译收集属性摘要、建模写入列注释、模型检查缺失；没有改动模型或生成器 | C2547～C2551 |

现有 Contracts/BuildingBlocks 的 CS1591、XML 语法及参数引用检查、OpenAPI 完整性和数据库列说明要求继续保留。当前脚本不新增跳过编译或测试的选项，完整执行时仍按原顺序运行这些步骤；本次按用户要求未执行。

## 接手时遵守的表达标准

先核对实现和调用位置，再判断是否需要说明。几个字能说清的就写短；已经准确自然的保留。名称已清楚的常规操作不逐步复述；不机械要求所有 public 成员配齐 summary、remarks、param、returns。

用途放简短摘要，调用规则集中到接口或契约，特殊实现原因写在代码旁，特性理由直接解释原因。权限、事务、并发、时间边界、失败行为和调用前提必须保留。see cref 只用于有导航价值的关联，不为处理器机械加上对应请求类型。

保留的例子：IStoredFileRepository 的状态范围、时间边界、游标、锁与重读、ResetDeletionAsync 返回值；IGeneratedFiles 的先写存储再登记消息；RevokeMySessionEndpoint 的权限范围、404、204 和重复撤销行为；Session.Rotate 的独立事务补偿要求。接口文档的参数标签保留完整组，避免只删一部分后产生 CS1573；信息很少且不需参数说明的组可以整体删除。

## 这次具体改了什么

| 位置 | 原说明或问题 | 当前说明或处理 | 记录 |
|---|---|---|---|
| StoredFile.GeneratedProfilePrefix | 常量说明夹带下载和到期清理流程 | 摘要仅为“生成文件的 Profile 前缀”；流程约定留在实际调用契约 | C2312 |
| FilesErrors.TypeNotAllowed | 文件类型不允许，或文件内容与扩展名不符 | 不支持该文件类型，或文件内容与扩展名不一致 | C2324 |
| SetMyPreferenceCommand.NotAudited | 个人体验配置，不是业务数据 | 仅调整当前用户在当前角色下的偏好 | C2331 |
| 定时任务 Enabled/Cron | “空”混淆 null 和空字符串 | null 使用代码默认值；Cron 空字符串无效，命令、请求、列表、实体保持一致 | C2340～C2341、C2352～C2357、C2385～C2386 |
| FileDetail.ObjectDeletedAt | 存储对象删除时间，仅已彻底删除的文件记录 | 对象删除登记时间；与实际登记流程一致，不承诺精确物理删除时刻 | C2369 |
| MobileNumbers | 声称手机号已是登录凭据 | 说明格式统一服务于唯一性校验；不将搁置的短信登录写成现有能力 | C2377～C2378 |
| Session.Rotate | “超出宽限期则作废整个会话”未说明失败回滚 | 补清返回重用结果及事务回滚后的独立事务复查要求，链接 RevokeIfReused | C2379 |
| 存储问题“可以还原” | 容易理解为实时保证版本存在 | 明确为“发现问题时仍能读取记录中的对象版本” | C2389～C2391 |
| PreviewTemplateEndpoint | 各种失败都像是返回 200 | 仅渲染器返回错误时包装为 200；前置模板、渠道、数据校验返回对应错误 | C2388、C2430 |
| SiteMessage.MarkRead | 参数 now 注释仅为“现在” | 删除这组低信息参数说明，保留不覆盖首次已读时间的行为约定 | C2413 |
| AssignUserOrgUnitRequest.OrgUnitId | “不给表示不属于任何单元”含糊 | 组织单元，须启用；未指定时清空所属组织 | C2426 |

FilesPermissions.FileManage 当前为“管理文件”，具体范围是“允许重试文件删除、将存储问题标为已解决”。这是登记运维处理结果的权限，不代表它自动恢复存储对象。ListDictionaryTypesQueryHandler 的简短用途保留，无需重复添加 see cref。以上两处在本地两轮中没有重复改写，不计入下方 63 文件。

## 前轮跨层复盘示例（C2432～C2470）

| 位置 | 原问题 | 当前说明 | 记录 |
|---|---|---|---|
| 字典移动父项 | “不给”未区分参数缺省和空白；同父项是否重排不明确 | null 或空白移至顶层；父项未变时保留原顺序 | C2432～C2434 |
| 偏好层 | 系统层、角色层“对所有人生效”混淆保存范围与生效优先级 | 写明全局共用、角色共用、用户与角色分别保存 | C2435～C2437 |
| 角色组织锚点 | 请求可空、命令非空、空列表清空与组织回退混用“不给” | 按各层真实参数写清未指定、空列表及整体替换的含义 | C2438～C2442 |
| 文件关联 | “系统用户”容易与普通系统内用户混淆 | 明确仅关联人为系统账号时享有无上传人文件的例外 | C2443～C2445 |
| 通知收件人 | “优先使用用户 ID”像是建议，暗示地址可回退 | 指定 UserId 时查询 Identity 并忽略 Email/Phone；外部收件人直接提供地址 | C2446～C2452、C2469～C2470 |
| Cron | “执行时间”“执行间隔”易误解成实际运行保证 | Cron 表达式、计划触发间隔及预览结果的限制 | C2454～C2459、C2466～C2468 |
| 模板预览 | 含密模板“只能用示例”未讲清是否能传 Model；错误称为模板有误 | 含密模板不得指定 Model；结果字段按渲染成功/失败说明 | C2461～C2465 |

## 本轮接口与契约复盘示例（C2471～C2517）

| 位置 | 原问题 | 当前说明或处理 | 记录 |
|---|---|---|---|
| 七个查询上下文 | 摘要重复类声明已写明的查询接口 | 保留简短用途及不跟踪约定，删除继承关系复述 | C2471～C2477 |
| 角色、查询映射、文件关联实现 | 反复解释版本要跟随内容、SQL CASE、接口已有的锁和事务 | 保留授权版本/缓存约定、框架表边界和独立系统查询作用域，删去重复 | C2478～C2480、C2510 |
| 开发与维护命令 | 容器未运行被说成无库、用户数为空被说成未迁移、表缺失被推断为模块未建立 | 只说明当前检查结果和可执行的处理方向；Status 不输出被捕获的标准错误，C2490 已补正 C2484 | C2482～C2485、C2490 |
| 文件关联、文件名清理 | 常规参数填满注释；用途、输入来源和返回条件混写 | 整组删去低信息参数标签，保留数量/事务/错误、扩展名截断条件及 null 含义 | C2486～C2487、C2491 |
| 会话查询及密码事件 | “有效”易误解为未过期；密码事件用途混入整段流程 | 写明未作废及用户/模拟操作人匹配，不按绝对到期过滤；密码变更保留当前会话的区别继续解释 | C2488～C2489 |
| 渲染和偏好回退 | 用时限制像硬超时；有保存值被当作必定可用 | 渲染用时在每次写入时检查；偏好无可用值时回退默认 | C2492～C2493 |
| 手动执行入口 | 标题容易被读成任务立即完成 | 统一为“手动触发”，明确只提交触发请求；运行检查、409、幂等与审计约定保留 | C2494～C2501 |
| 模板版本与退役详情 | 从未发布与恢复默认混淆；空版本被当作必有默认内容 | 按是否选用发布版本说明默认内容，并写明已移除声明的模板不提供默认、变量和示例 | C2502～C2505 |
| 排序列表 | “全部、不能多不能少”重复，却没点明停用项 | 简短说明完整无重复列表，明确停用单元/字典项也须包含 | C2506～C2509 |
| 用户资料授权、缓存和限流 | “登录凭据”抽象，基础编辑与额外检查不明确；架构备注重复；等待时长像保证 | 直接指明账号或手机号的额外授权；删去分层位置复述；限流返回建议等待时长 | C2511～C2517 |

## 已提交文案版 2d1a81f 的 119 个源码文件

下表汇总已随 2d1a81f 提交的四轮源码文字变化。精确原文、现文、UTC+8 时间和实现核对依据见 C 编号；更早已提交的内容保留在修改记录历史中。

### Configuration：16 个文件

| 文件（相对仓库根目录） | 修改位置 | 逐项记录 |
|---|---|---|
| [src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Items/AddDictionaryItemCommand.cs](../../src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Items/AddDictionaryItemCommand.cs) | SortOrder 参数 | C2421 |
| [src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Items/MoveDictionaryItemCommand.cs](../../src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Items/MoveDictionaryItemCommand.cs) | 命令用途与同父项行为；NewParentCode 参数 | C2432～C2433 |
| [src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Items/ReorderDictionaryItemsCommand.cs](../../src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Items/ReorderDictionaryItemsCommand.cs) | ParentCode 参数 | C2424 |
| [src/Modules/Configuration/Kmic.Configuration.Application/Preferences/IPreferenceLayers.cs](../../src/Modules/Configuration/Kmic.Configuration.Application/Preferences/IPreferenceLayers.cs) | 无额外信息的分层说明 | C2515 |
| [src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Layers/ResetPreferenceLayerCommand.cs](../../src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Layers/ResetPreferenceLayerCommand.cs) | 命令用途与回退约定；无生效值的回退条件 | C2362、C2493 |
| [src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Layers/SetPreferenceLayerCommand.cs](../../src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Layers/SetPreferenceLayerCommand.cs) | Key 参数 | C2361 |
| [src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Mine/MyPreferenceItem.cs](../../src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Mine/MyPreferenceItem.cs) | IsFamily 参数 | C2420 |
| [src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Mine/ResetMyPreferenceCommand.cs](../../src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Mine/ResetMyPreferenceCommand.cs) | NotAudited 理由 | C2333 |
| [src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Mine/SetMyPreferenceCommand.cs](../../src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Mine/SetMyPreferenceCommand.cs) | 命令 summary；命令 remarks；Key param；NotAudited 理由；NotIdempotent 理由 | C2328～C2332 |
| [src/Modules/Configuration/Kmic.Configuration.Domain/Dictionaries/DictionaryType.cs](../../src/Modules/Configuration/Kmic.Configuration.Domain/Dictionaries/DictionaryType.cs) | SortOrder 参数；ReorderItems 用途与排序规则 | C2423、C2509 |
| [src/Modules/Configuration/Kmic.Configuration.Domain/Preferences/PreferenceLayer.cs](../../src/Modules/Configuration/Kmic.Configuration.Domain/Preferences/PreferenceLayer.cs) | System 成员用途；Role 成员用途；UserRole 成员用途 | C2435～C2437 |
| [src/Modules/Configuration/Kmic.Configuration.Domain/Settings/SystemSettingChange.cs](../../src/Modules/Configuration/Kmic.Configuration.Domain/Settings/SystemSettingChange.cs) | 类型 summary；TraceId summary；Record summary | C2318～C2320 |
| [src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/ConfigurationQueryDbContext.cs](../../src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/ConfigurationQueryDbContext.cs) | 查询上下文用途 | C2471 |
| [src/Modules/Configuration/Kmic.Configuration.Presentation/Dictionaries/AddDictionaryItemRequest.cs](../../src/Modules/Configuration/Kmic.Configuration.Presentation/Dictionaries/AddDictionaryItemRequest.cs) | SortOrder 参数 | C2422 |
| [src/Modules/Configuration/Kmic.Configuration.Presentation/Dictionaries/MoveDictionaryItemRequest.cs](../../src/Modules/Configuration/Kmic.Configuration.Presentation/Dictionaries/MoveDictionaryItemRequest.cs) | NewParentCode 参数 | C2434 |
| [src/Modules/Configuration/Kmic.Configuration.Presentation/Dictionaries/ReorderDictionaryItemsRequest.cs](../../src/Modules/Configuration/Kmic.Configuration.Presentation/Dictionaries/ReorderDictionaryItemsRequest.cs) | ParentCode 参数；OrderedCodes 参数 | C2425、C2507 |

### Files：24 个文件

| 文件（相对仓库根目录） | 修改位置 | 逐项记录 |
|---|---|---|
| [src/Modules/Files/Kmic.Files.Application/Administration/FileDetail.cs](../../src/Modules/Files/Kmic.Files.Application/Administration/FileDetail.cs) | ObjectDeletedAt 参数；HistoryPurgedAt 参数；UploadedAt 参数；详情用途 | C2369～C2370、C2394、C2396 |
| [src/Modules/Files/Kmic.Files.Application/Administration/FileListItem.cs](../../src/Modules/Files/Kmic.Files.Application/Administration/FileListItem.cs) | UploadedAt 参数；FileType 参数；Size 参数 | C2395、C2397～C2398 |
| [src/Modules/Files/Kmic.Files.Application/Administration/ListFilesQuery.cs](../../src/Modules/Files/Kmic.Files.Application/Administration/ListFilesQuery.cs) | From 参数；To 参数 | C2399～C2400 |
| [src/Modules/Files/Kmic.Files.Application/Attachments/FileAttachments.cs](../../src/Modules/Files/Kmic.Files.Application/Attachments/FileAttachments.cs) | AttachAsync RS0030 豁免理由；DetachAsync RS0030 豁免理由；实现说明去重 | C2411～C2412、C2510 |
| [src/Modules/Files/Kmic.Files.Application/Attachments/GeneratedFiles.cs](../../src/Modules/Files/Kmic.Files.Application/Attachments/GeneratedFiles.cs) | Register RS0030 豁免理由 | C2410 |
| [src/Modules/Files/Kmic.Files.Application/Cleanup/PurgeExpiredFilesCommand.cs](../../src/Modules/Files/Kmic.Files.Application/Cleanup/PurgeExpiredFilesCommand.cs) | NotAudited 理由 | C2374 |
| [src/Modules/Files/Kmic.Files.Application/Cleanup/ReconcileStorageCommand.cs](../../src/Modules/Files/Kmic.Files.Application/Cleanup/ReconcileStorageCommand.cs) | NotAudited 理由 | C2376 |
| [src/Modules/Files/Kmic.Files.Application/Cleanup/ResetStaleUploadsCommand.cs](../../src/Modules/Files/Kmic.Files.Application/Cleanup/ResetStaleUploadsCommand.cs) | NotAudited 理由 | C2375 |
| [src/Modules/Files/Kmic.Files.Application/ContractQueries/CheckAttachabilityQuery.cs](../../src/Modules/Files/Kmic.Files.Application/ContractQueries/CheckAttachabilityQuery.cs) | 系统账号的关联例外；UploaderId 参数 | C2444～C2445 |
| [src/Modules/Files/Kmic.Files.Application/Uploads/FileNames.cs](../../src/Modules/Files/Kmic.Files.Application/Uploads/FileNames.cs) | Sanitize 用途、截断与返回约定 | C2491 |
| [src/Modules/Files/Kmic.Files.Contracts/IFileAttachability.cs](../../src/Modules/Files/Kmic.Files.Contracts/IFileAttachability.cs) | 系统账号的关联例外 | C2443 |
| [src/Modules/Files/Kmic.Files.Contracts/IFileAttachments.cs](../../src/Modules/Files/Kmic.Files.Contracts/IFileAttachments.cs) | AttachAsync 参数组与数量约定；DetachAsync 常规参数组 | C2486～C2487 |
| [src/Modules/Files/Kmic.Files.Contracts/IGeneratedFiles.cs](../../src/Modules/Files/Kmic.Files.Contracts/IGeneratedFiles.cs) | WriteAsync summary；WriteAsync returns；Register summary | C2407～C2409 |
| [src/Modules/Files/Kmic.Files.Domain/Errors/FilesErrors.cs](../../src/Modules/Files/Kmic.Files.Domain/Errors/FilesErrors.cs) | TypeNotAllowed 用户提示；TooLarge 用户提示 | C2324～C2325 |
| [src/Modules/Files/Kmic.Files.Domain/Files/IStoredFileRepository.cs](../../src/Modules/Files/Kmic.Files.Domain/Files/IStoredFileRepository.cs) | ResetDeletionAsync summary | C2326 |
| [src/Modules/Files/Kmic.Files.Domain/Files/StorageIssueKind.cs](../../src/Modules/Files/Kmic.Files.Domain/Files/StorageIssueKind.cs) | RestorableMissing summary；LostMissing summary | C2390～C2391 |
| [src/Modules/Files/Kmic.Files.Domain/Files/StoredFile.cs](../../src/Modules/Files/Kmic.Files.Domain/Files/StoredFile.cs) | GeneratedProfilePrefix summary；Start summary；RegisterGenerated summary/remarks；Attach summary；RecordHistoryPurge summary；ExpiresAt remarks | C2312～C2317 |
| [src/Modules/Files/Kmic.Files.Infrastructure/Deletion/FilesDeletionStore.cs](../../src/Modules/Files/Kmic.Files.Infrastructure/Deletion/FilesDeletionStore.cs) | 类型用途与 SQL 实现约定；队列范围说明；完成处理的两种结果 | C2372、C2404～C2405 |
| [src/Modules/Files/Kmic.Files.Infrastructure/Deletion/StoredFileDeletionWork.cs](../../src/Modules/Files/Kmic.Files.Infrastructure/Deletion/StoredFileDeletionWork.cs) | 失败重试约定 | C2371 |
| [src/Modules/Files/Kmic.Files.Infrastructure/Persistence/FilesQueryDbContext.cs](../../src/Modules/Files/Kmic.Files.Infrastructure/Persistence/FilesQueryDbContext.cs) | 查询上下文用途 | C2472 |
| [src/Modules/Files/Kmic.Files.Infrastructure/Persistence/Repositories/StoredFileRepository.cs](../../src/Modules/Files/Kmic.Files.Infrastructure/Persistence/Repositories/StoredFileRepository.cs) | ResetDeletionAsync 状态条件 | C2406 |
| [src/Modules/Files/Kmic.Files.Presentation/Administration/ListFilesEndpoint.cs](../../src/Modules/Files/Kmic.Files.Presentation/Administration/ListFilesEndpoint.cs) | 列表筛选说明 | C2403 |
| [src/Modules/Files/Kmic.Files.Presentation/Administration/ListFilesRequest.cs](../../src/Modules/Files/Kmic.Files.Presentation/Administration/ListFilesRequest.cs) | From 参数；To 参数 | C2401～C2402 |
| [src/Modules/Files/Kmic.Files.Presentation/Administration/ListStorageIssuesEndpoint.cs](../../src/Modules/Files/Kmic.Files.Presentation/Administration/ListStorageIssuesEndpoint.cs) | 可还原分类的时间边界 | C2389 |

### Identity：26 个文件

| 文件（相对仓库根目录） | 修改位置 | 逐项记录 |
|---|---|---|
| [src/Modules/Identity/Kmic.Identity.Application/Authentication/MySessions/RevokeMySessionCommandHandler.cs](../../src/Modules/Identity/Kmic.Identity.Application/Authentication/MySessions/RevokeMySessionCommandHandler.cs) | 处理器类型 summary | C2327 |
| [src/Modules/Identity/Kmic.Identity.Application/Catalog/SyncPermissionCatalogCommand.cs](../../src/Modules/Identity/Kmic.Identity.Application/Catalog/SyncPermissionCatalogCommand.cs) | 同步用途；同步变化及角色版本约定 | C2383～C2384 |
| [src/Modules/Identity/Kmic.Identity.Application/Users/EventHandlers/RevokeSessionsWhenPasswordChanged.cs](../../src/Modules/Identity/Kmic.Identity.Application/Users/EventHandlers/RevokeSessionsWhenPasswordChanged.cs) | 密码变化的会话处理说明 | C2489 |
| [src/Modules/Identity/Kmic.Identity.Application/Users/ForceLogout/ForceLogoutCommand.cs](../../src/Modules/Identity/Kmic.Identity.Application/Users/ForceLogout/ForceLogoutCommand.cs) | 会话范围说明；SessionId 参数 | C2380～C2381 |
| [src/Modules/Identity/Kmic.Identity.Application/Users/Get/UserGrantItem.cs](../../src/Modules/Identity/Kmic.Identity.Application/Users/Get/UserGrantItem.cs) | Anchors 参数 | C2442 |
| [src/Modules/Identity/Kmic.Identity.Application/Users/Grants/GrantRole/GrantRoleCommand.cs](../../src/Modules/Identity/Kmic.Identity.Application/Users/Grants/GrantRole/GrantRoleCommand.cs) | Anchors 参数 | C2438 |
| [src/Modules/Identity/Kmic.Identity.Application/Users/UserAdminAction.cs](../../src/Modules/Identity/Kmic.Identity.Application/Users/UserAdminAction.cs) | Edit 用途与额外检查；ChangeCredentials 动作用途；ChangeCredentials 显示名 | C2512～C2514 |
| [src/Modules/Identity/Kmic.Identity.Application/Users/UserPolicy.cs](../../src/Modules/Identity/Kmic.Identity.Application/Users/UserPolicy.cs) | 受保护用户资料的额外限制 | C2511 |
| [src/Modules/Identity/Kmic.Identity.Domain/Organization/OrgTree.cs](../../src/Modules/Identity/Kmic.Identity.Domain/Organization/OrgTree.cs) | Reorder 用途与完整列表约定 | C2508 |
| [src/Modules/Identity/Kmic.Identity.Domain/Roles/Role.cs](../../src/Modules/Identity/Kmic.Identity.Domain/Roles/Role.cs) | 授权版本与全权限角色说明 | C2478 |
| [src/Modules/Identity/Kmic.Identity.Domain/Sessions/ISessionRepository.cs](../../src/Modules/Identity/Kmic.Identity.Domain/Sessions/ISessionRepository.cs) | ListActiveInvolvingAsync 范围 | C2488 |
| [src/Modules/Identity/Kmic.Identity.Domain/Sessions/Session.cs](../../src/Modules/Identity/Kmic.Identity.Domain/Sessions/Session.cs) | Rotate 事务回滚与重用处理 | C2379 |
| [src/Modules/Identity/Kmic.Identity.Domain/Users/MobileNumbers.cs](../../src/Modules/Identity/Kmic.Identity.Domain/Users/MobileNumbers.cs) | 规范化用途说明；Normalize 用途与输入格式 | C2377～C2378 |
| [src/Modules/Identity/Kmic.Identity.Domain/Users/User.cs](../../src/Modules/Identity/Kmic.Identity.Domain/Users/User.cs) | 类型职责与历史引用 | C2365 |
| [src/Modules/Identity/Kmic.Identity.Infrastructure/Access/AccessContextLoader.cs](../../src/Modules/Identity/Kmic.Identity.Infrastructure/Access/AccessContextLoader.cs) | 类型 summary；角色不可用时的原因注释 | C2364、C2382 |
| [src/Modules/Identity/Kmic.Identity.Infrastructure/Access/PermissionCatalogStore.cs](../../src/Modules/Identity/Kmic.Identity.Infrastructure/Access/PermissionCatalogStore.cs) | 改名冲突处理；Upsert 用途与返回值 | C2418～C2419 |
| [src/Modules/Identity/Kmic.Identity.Infrastructure/Access/RoleSnapshot.cs](../../src/Modules/Identity/Kmic.Identity.Infrastructure/Access/RoleSnapshot.cs) | 缓存与序列化约定 | C2363 |
| [src/Modules/Identity/Kmic.Identity.Infrastructure/Operations/IdentityBootstrapper.cs](../../src/Modules/Identity/Kmic.Identity.Infrastructure/Operations/IdentityBootstrapper.cs) | BootstrapAsync rootUnitName 参数 | C2427 |
| [src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/IdentityQueryDbContext.cs](../../src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/IdentityQueryDbContext.cs) | 查询上下文用途 | C2473 |
| [src/Modules/Identity/Kmic.Identity.Presentation/Organization/AssignUserOrgUnitRequest.cs](../../src/Modules/Identity/Kmic.Identity.Presentation/Organization/AssignUserOrgUnitRequest.cs) | OrgUnitId 参数 | C2426 |
| [src/Modules/Identity/Kmic.Identity.Presentation/Organization/ReorderOrgUnitsRequest.cs](../../src/Modules/Identity/Kmic.Identity.Presentation/Organization/ReorderOrgUnitsRequest.cs) | OrderedChildren 参数 | C2506 |
| [src/Modules/Identity/Kmic.Identity.Presentation/Users/ForceLogoutEndpoint.cs](../../src/Modules/Identity/Kmic.Identity.Presentation/Users/ForceLogoutEndpoint.cs) | 会话处理范围 | C2428 |
| [src/Modules/Identity/Kmic.Identity.Presentation/Users/ForceLogoutRequest.cs](../../src/Modules/Identity/Kmic.Identity.Presentation/Users/ForceLogoutRequest.cs) | SessionId 参数 | C2429 |
| [src/Modules/Identity/Kmic.Identity.Presentation/Users/GrantRoleRequest.cs](../../src/Modules/Identity/Kmic.Identity.Presentation/Users/GrantRoleRequest.cs) | Anchors 参数 | C2439 |
| [src/Modules/Identity/Kmic.Identity.Presentation/Users/SetGrantAnchorsRequest.cs](../../src/Modules/Identity/Kmic.Identity.Presentation/Users/SetGrantAnchorsRequest.cs) | 请求用途；Anchors 参数 | C2440～C2441 |
| [src/Modules/Identity/Kmic.Identity.Presentation/Users/UpdateUserProfileEndpoint.cs](../../src/Modules/Identity/Kmic.Identity.Presentation/Users/UpdateUserProfileEndpoint.cs) | 修改账号手机号的权限说明 | C2393 |

### Notifications：25 个文件

| 文件（相对仓库根目录） | 修改位置 | 逐项记录 |
|---|---|---|
| [src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Get/DeliveryDetailResponse.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Get/DeliveryDetailResponse.cs) | RecipientUserId 参数 | C2469 |
| [src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/List/DeliveryListItem.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/List/DeliveryListItem.cs) | RecipientUserId 参数 | C2470 |
| [src/Modules/Notifications/Kmic.Notifications.Application/Processing/NotificationProcessor.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Processing/NotificationProcessor.cs) | 渲染复用范围；Resolve 地址来源注释 | C2416、C2447 |
| [src/Modules/Notifications/Kmic.Notifications.Application/Rendering/ITemplateRenderer.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Rendering/ITemplateRenderer.cs) | 渲染限制的检查时机 | C2492 |
| [src/Modules/Notifications/Kmic.Notifications.Application/Templates/ChannelRendering.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Templates/ChannelRendering.cs) | 按渠道渲染的用途与格式约定 | C2481 |
| [src/Modules/Notifications/Kmic.Notifications.Application/Templates/Get/TemplateChannelInfo.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Templates/Get/TemplateChannelInfo.cs) | ActiveVersion 参数 | C2504 |
| [src/Modules/Notifications/Kmic.Notifications.Application/Templates/List/TemplateListItem.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Templates/List/TemplateListItem.cs) | ActiveVersions 参数 | C2503 |
| [src/Modules/Notifications/Kmic.Notifications.Application/Templates/Preview/PreviewTemplateQuery.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Templates/Preview/PreviewTemplateQuery.cs) | Model 参数 | C2464 |
| [src/Modules/Notifications/Kmic.Notifications.Application/Templates/Preview/TemplatePreviewResponse.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Templates/Preview/TemplatePreviewResponse.cs) | Subject 参数；Body 参数；Error 参数 | C2461～C2463 |
| [src/Modules/Notifications/Kmic.Notifications.Application/Templates/Publish/PublishTemplateCommand.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Templates/Publish/PublishTemplateCommand.cs) | 发布前渲染的验证范围 | C2387 |
| [src/Modules/Notifications/Kmic.Notifications.Application/Templates/Sync/SyncTemplateCatalogCommand.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Templates/Sync/SyncTemplateCatalogCommand.cs) | 命令 summary；同步时机及变化处理 | C2367～C2368 |
| [src/Modules/Notifications/Kmic.Notifications.Application/Templates/Sync/SyncTemplateCatalogCommandHandler.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Templates/Sync/SyncTemplateCatalogCommandHandler.cs) | 未匹配声明的处理 | C2417 |
| [src/Modules/Notifications/Kmic.Notifications.Contracts/Recipient.cs](../../src/Modules/Notifications/Kmic.Notifications.Contracts/Recipient.cs) | 收件地址选择约定 | C2446 |
| [src/Modules/Notifications/Kmic.Notifications.Contracts/SendSecretNotification.cs](../../src/Modules/Notifications/Kmic.Notifications.Contracts/SendSecretNotification.cs) | DedupKey param | C2343 |
| [src/Modules/Notifications/Kmic.Notifications.Domain/Errors/NotificationsErrors.cs](../../src/Modules/Notifications/Kmic.Notifications.Domain/Errors/NotificationsErrors.cs) | TemplateSecretMismatch 用户提示；ModelInvalid 用户提示；SiteMessageLinkInvalid 用户提示；ModelInvalid 参数 | C2334～C2336、C2415 |
| [src/Modules/Notifications/Kmic.Notifications.Domain/SiteMessages/SiteMessage.cs](../../src/Modules/Notifications/Kmic.Notifications.Domain/SiteMessages/SiteMessage.cs) | MarkRead 参数；站内路径限制 | C2413～C2414 |
| [src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/NotificationsQueryDbContext.cs](../../src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/NotificationsQueryDbContext.cs) | 查询上下文用途 | C2474 |
| [src/Modules/Notifications/Kmic.Notifications.Infrastructure/Sending/ContactLookup.cs](../../src/Modules/Notifications/Kmic.Notifications.Infrastructure/Sending/ContactLookup.cs) | 类型用途与地址保存约定 | C2450 |
| [src/Modules/Notifications/Kmic.Notifications.Infrastructure/Sending/DeliveryItem.cs](../../src/Modules/Notifications/Kmic.Notifications.Infrastructure/Sending/DeliveryItem.cs) | RecipientUserId 参数；Address 参数 | C2448～C2449 |
| [src/Modules/Notifications/Kmic.Notifications.Infrastructure/Sending/DeliveryStore.cs](../../src/Modules/Notifications/Kmic.Notifications.Infrastructure/Sending/DeliveryStore.cs) | 类型用途与 SQL 实现约定 | C2373 |
| [src/Modules/Notifications/Kmic.Notifications.Infrastructure/Sending/DeliveryWork.cs](../../src/Modules/Notifications/Kmic.Notifications.Infrastructure/Sending/DeliveryWork.cs) | 联系方式读取范围；ResolveAddressAsync 地址来源注释；含密投递过期失败说明 | C2451～C2453 |
| [src/Modules/Notifications/Kmic.Notifications.Presentation/Templates/GetTemplateEndpoint.cs](../../src/Modules/Notifications/Kmic.Notifications.Presentation/Templates/GetTemplateEndpoint.cs) | 已移除代码声明模板的返回范围 | C2505 |
| [src/Modules/Notifications/Kmic.Notifications.Presentation/Templates/ListTemplatesEndpoint.cs](../../src/Modules/Notifications/Kmic.Notifications.Presentation/Templates/ListTemplatesEndpoint.cs) | 默认内容的使用条件 | C2502 |
| [src/Modules/Notifications/Kmic.Notifications.Presentation/Templates/PreviewTemplateEndpoint.cs](../../src/Modules/Notifications/Kmic.Notifications.Presentation/Templates/PreviewTemplateEndpoint.cs) | 预览响应与校验失败说明；预览失败的包装边界（补正 C2388） | C2388、C2430 |
| [src/Modules/Notifications/Kmic.Notifications.Presentation/Templates/PreviewTemplateRequest.cs](../../src/Modules/Notifications/Kmic.Notifications.Presentation/Templates/PreviewTemplateRequest.cs) | Model 参数 | C2465 |

### Operations：22 个文件

| 文件（相对仓库根目录） | 修改位置 | 逐项记录 |
|---|---|---|
| [src/Modules/Operations/Kmic.Operations.Application/ClientErrors/IClientErrorLimiter.cs](../../src/Modules/Operations/Kmic.Operations.Application/ClientErrors/IClientErrorLimiter.cs) | TryAcquire 返回等待时长的含义 | C2517 |
| [src/Modules/Operations/Kmic.Operations.Application/ClientErrors/ReportClientErrorsCommand.cs](../../src/Modules/Operations/Kmic.Operations.Application/ClientErrors/ReportClientErrorsCommand.cs) | NotAudited 理由 | C2366 |
| [src/Modules/Operations/Kmic.Operations.Application/IOperationsQueryDb.cs](../../src/Modules/Operations/Kmic.Operations.Application/IOperationsQueryDb.cs) | ScheduledJobSettings summary；WithTarget summary；WithTarget targetsJson param | C2337～C2339 |
| [src/Modules/Operations/Kmic.Operations.Application/MergedCursorPaging.cs](../../src/Modules/Operations/Kmic.Operations.Application/MergedCursorPaging.cs) | PageAsync 调用约定 | C2342 |
| [src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/ListRoutesQuery.cs](../../src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/ListRoutesQuery.cs) | Status 参数 | C2460 |
| [src/Modules/Operations/Kmic.Operations.Application/Messages/Summary/GetMessageSummaryQuery.cs](../../src/Modules/Operations/Kmic.Operations.Application/Messages/Summary/GetMessageSummaryQuery.cs) | 待终结订阅积压说明 | C2392 |
| [src/Modules/Operations/Kmic.Operations.Application/OperationsPermissions.cs](../../src/Modules/Operations/Kmic.Operations.Application/OperationsPermissions.cs) | JobsManage 权限说明 | C2501 |
| [src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/CronRules.cs](../../src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/CronRules.cs) | MinimumInterval 用途；Check 用途；间隔不足的字段错误 | C2454～C2456 |
| [src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/List/ScheduledJobItem.cs](../../src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/List/ScheduledJobItem.cs) | OverrideEnabled 参数；OverrideCron 参数；DefaultCron 参数 | C2385～C2386、C2459 |
| [src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/Preview/PreviewCronQuery.cs](../../src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/Preview/PreviewCronQuery.cs) | 预览校验范围 | C2458 |
| [src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/RunNow/RunJobNowCommand.cs](../../src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/RunNow/RunJobNowCommand.cs) | 命令用途；手动触发的返回与记录约定 | C2494～C2495 |
| [src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/ScheduledJobAction.cs](../../src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/ScheduledJobAction.cs) | RunNow 动作用途；RunNow 动作显示名 | C2498～C2499 |
| [src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/ScheduledJobRunItem.cs](../../src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/ScheduledJobRunItem.cs) | ScheduledFireTime param；StartedAt param；FinishedAt param；InstanceId param；TraceId param | C2344～C2348 |
| [src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/Set/SetJobOverrideCommand.cs](../../src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/Set/SetJobOverrideCommand.cs) | Enabled param；Cron param | C2340～C2341 |
| [src/Modules/Operations/Kmic.Operations.Domain/Errors/OperationsErrors.cs](../../src/Modules/Operations/Kmic.Operations.Domain/Errors/OperationsErrors.cs) | CronInvalid 用途；CronInvalid 用户提示；CronInvalidWith 用途 | C2466～C2468 |
| [src/Modules/Operations/Kmic.Operations.Domain/Jobs/JobRunTrigger.cs](../../src/Modules/Operations/Kmic.Operations.Domain/Jobs/JobRunTrigger.cs) | Manual 触发方式说明 | C2500 |
| [src/Modules/Operations/Kmic.Operations.Domain/Jobs/ScheduledJobSetting.cs](../../src/Modules/Operations/Kmic.Operations.Domain/Jobs/ScheduledJobSetting.cs) | 类型 remarks；Enabled summary；Cron summary；IsDefault summary；Apply summary | C2353～C2357 |
| [src/Modules/Operations/Kmic.Operations.Infrastructure/ClientErrors/ClientErrorLimiter.cs](../../src/Modules/Operations/Kmic.Operations.Infrastructure/ClientErrors/ClientErrorLimiter.cs) | 限流器的回收与存储说明 | C2516 |
| [src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/OperationsQueryDbContext.cs](../../src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/OperationsQueryDbContext.cs) | 查询上下文用途；执行记录 SQL 查询原因；路由与心跳 SQL 查询原因 | C2475、C2479～C2480 |
| [src/Modules/Operations/Kmic.Operations.Presentation/ScheduledJobs/PreviewCronEndpoint.cs](../../src/Modules/Operations/Kmic.Operations.Presentation/ScheduledJobs/PreviewCronEndpoint.cs) | 预览校验范围 | C2457 |
| [src/Modules/Operations/Kmic.Operations.Presentation/ScheduledJobs/RunJobNowEndpoint.cs](../../src/Modules/Operations/Kmic.Operations.Presentation/ScheduledJobs/RunJobNowEndpoint.cs) | 接口标题；触发成功与执行完成的区别 | C2496～C2497 |
| [src/Modules/Operations/Kmic.Operations.Presentation/ScheduledJobs/SetJobOverrideRequest.cs](../../src/Modules/Operations/Kmic.Operations.Presentation/ScheduledJobs/SetJobOverrideRequest.cs) | Cron param | C2352 |

### Sample：2 个文件

| 文件（相对仓库根目录） | 修改位置 | 逐项记录 |
|---|---|---|
| [src/Modules/Sample/Kmic.Sample.Domain/Specimens/Specimen.cs](../../src/Modules/Sample/Kmic.Sample.Domain/Specimens/Specimen.cs) | AddItem summary/remarks；AddAttachment 参数约定；RemoveAttachment param | C2321～C2323 |
| [src/Modules/Sample/Kmic.Sample.Infrastructure/Persistence/SampleQueryDbContext.cs](../../src/Modules/Sample/Kmic.Sample.Infrastructure/Persistence/SampleQueryDbContext.cs) | 查询上下文用途 | C2476 |

### 宿主：2 个文件

| 文件（相对仓库根目录） | 修改位置 | 逐项记录 |
|---|---|---|
| [src/Hosts/Kmic.MigrationRunner/RunnerApp.cs](../../src/Hosts/Kmic.MigrationRunner/RunnerApp.cs) | 清理表不存在的提示 | C2482 |
| [src/Hosts/Kmic.Operator/IdentityCommands.cs](../../src/Hosts/Kmic.Operator/IdentityCommands.cs) | root-unit-name 选项说明 | C2485 |

### 模板：1 个文件

| 文件（相对仓库根目录） | 修改位置 | 逐项记录 |
|---|---|---|
| [templates/kmic-module/src/Modules/Customer/Kmic.Customer.Infrastructure/Persistence/CustomerQueryDbContext.cs](../../templates/kmic-module/src/Modules/Customer/Kmic.Customer.Infrastructure/Persistence/CustomerQueryDbContext.cs) | 查询上下文用途 | C2477 |

### 脚本：1 个文件

| 文件（相对仓库根目录） | 修改位置 | 逐项记录 |
|---|---|---|
| [scripts/dev.cs](../../scripts/dev.cs) | reset 时容器未运行的错误；status 用户数查询失败的提示；用户数为空时的检查方向（补正 C2484） | C2483～C2484、C2490 |

## 随 2d1a81f 提交的 6 个既有模型相关文件

这些是前轮已授权生成的结果，本交接轮未修改。两份迁移仅调整三条列注释：Configuration.SystemSettingChange.TraceId；Operations.ScheduledJobSetting.Enabled、Cron。旧迁移保留，不手改旧迁移、设计文件或快照。

| 文件（相对仓库根目录） | 状态 | 记录 |
|---|---|---|
| [src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/Migrations/20261114165501_SimplifySettingChangeTraceComment.Designer.cs](../../src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/Migrations/20261114165501_SimplifySettingChangeTraceComment.Designer.cs) | 已随 2d1a81f 提交，本轮未改写 | C2349 |
| [src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/Migrations/20261114165501_SimplifySettingChangeTraceComment.cs](../../src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/Migrations/20261114165501_SimplifySettingChangeTraceComment.cs) | 已随 2d1a81f 提交，本轮未改写 | C2350 |
| [src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/Migrations/ConfigurationDbContextModelSnapshot.cs](../../src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/Migrations/ConfigurationDbContextModelSnapshot.cs) | 已随 2d1a81f 提交，本轮未改写 | C2351 |
| [src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/Migrations/20261114170053_ClarifyJobSettingNullComments.Designer.cs](../../src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/Migrations/20261114170053_ClarifyJobSettingNullComments.Designer.cs) | 已随 2d1a81f 提交，本轮未改写 | C2358 |
| [src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/Migrations/20261114170053_ClarifyJobSettingNullComments.cs](../../src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/Migrations/20261114170053_ClarifyJobSettingNullComments.cs) | 已随 2d1a81f 提交，本轮未改写 | C2359 |
| [src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/Migrations/OperationsDbContextModelSnapshot.cs](../../src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/Migrations/OperationsDbContextModelSnapshot.cs) | 已随 2d1a81f 提交，本轮未改写 | C2360 |

## 验证结果和未运行项目

- 首次提交前，119 个源码相对 e2f5ee9 的静态可执行标记核对通过，差异仅为注释和中文字符串；提交准确包含 132 文件，原有 7 文件排除。
- 本轮规范/脚本：33 项精确替换回放一致；scripts/check.cs 经已有 PowerShell Roslyn 解析，语法错误 0；同一正则对 2404 个 C#/TS/Vue 文件只读扫描，残留临时引用标记 0；其余 14 个检查步骤及 6 个基础辅助方法逐段保持一致，未引入编译/测试跳过开关；编译配置、分析器、模型和 OpenAPI 完整性检查实现均未改变。
- 内容保护：提交后范围外 2694 个既有文件逐文件 SHA-256 不变，119 个业务源码成果和 6 个既有模型文件保持提交版本；原有 3 个 BuildingBlocks、4 个测试文件的本地改动字节保持；当前 18 个已跟踪修改（本轮 11 文件加原有 7 文件），无未跟踪项，暂存区为空，HEAD 为 2d1a81fe9bf90a64cb7ef9dc3ff30abe01e60ffe。
- 产品编译、测试、整套 scripts/check.cs、OpenAPI 生成及数据库连接/SQL/database update 均未运行；静态语法与只读扫描不代表编译或运行验收通过；未推送；首次版本提交后没有再次暂存或提交。
- 既有三条模型注释（Configuration.TraceId、Operations.Enabled/Cron）的离线编译/模型/SQL核对来自先前授权轮，已记录且本轮未重跑；本次只原样纳入首次提交，没有新模型改动。
- 原始 API 的已有 IDE0005（GetReadableFileQuery.cs:4 未使用 using）未处理；不宣称整站构建通过。最终 diff、链接和文件映射结果见最新协作记录。

## 保留的原有本地改动

以下 7 个文件不是本轮文字成果，必须保留，不随文案工作清理、覆盖或纳入提交：

- `src/BuildingBlocks/Kmic.BuildingBlocks.Application/Files/IFileUploadProcessor.cs`
- `src/BuildingBlocks/Kmic.BuildingBlocks.Domain/Results/Error.cs`
- `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Files/FileUploadProcessor.cs`
- `tests/Kmic.Api.Tests/ApiTestBase.cs`
- `tests/Kmic.Api.Tests/Configuration/DictionaryAdministrationTests.cs`
- `tests/Kmic.Api.Tests/Configuration/SettingAdministrationTests.cs`
- `tests/Kmic.Api.Tests/Files/FileLifecycleTests.cs`

## 后续任务边界

本轮中文审查没有剩余待改项，不继续为统一措辞反复改写准确说明。下列事项是独立行为或框架任务，未因改文字而解决：

- CN-01：含密死信管理接口的载荷保护尚未补齐；若需改 BuildingBlocks，先说明具体文件和影响，取得用户确认。
- CN-02：Serilog RequestPath 与路由模板的记录方式；CN-03：手工触发任务时，运行检查与触发不是原子操作。
- Sample 设置示例尚未接入创建/提交校验；已有说明明确其示例边界，不当作已生效的业务规则。
- 检查脚本的机械词表和格式硬要求已在本轮获准调整；BuildingBlocks 旧术语、既有 API IDE0005 继续另列，未修复。
- 短信登录、短信网关、登录人机验证、病毒扫描等延后事项，继续按框架及实施待办处理；不因出现接口或占位实现就宣称已完成。
- 数据库尚未应用任何本轮迁移；实际部署库注释与迁移历史未经核对，不能把离线验证记为数据库更新完成。

## Claude Code 如何接手

1. 按顺序读根目录 [CLAUDE.md](../../CLAUDE.md)、[框架入口](../framework/00-README.md)、[S3 进度](S3-平台模块.md)、本摘要、[开发交接](开发交接.md)、[协作记录](协作交接记录.md)、[修改记录](修改记录.md)。完整覆盖和保留结论查[全面复核清单](Codex-中文源码全面复核清单.md)。
2. 重新核对实际分支、HEAD、git status、暂存区和 diff；文档是交接索引，实际差异优先。旧段落的“待改”“整份排除已处理路径”属于历史阶段，当前标准以本文及开发交接顶部为准。双方已处理与未提交文件允许复审，但修改前仍需核对来源，不能覆盖他人改动。
3. [Codex 路径清单](Codex-已按中文规范处理的文件.md)仅记录来源；[ClaudeCode 清单](ClaudeCode-已按中文规范处理的文件.md)按用户要求保持空文件，不据此认定合格。当前均无需再机械清空或重复整理。
4. 如有新发现，先核对调用链、事务、状态与失败分支，再做最小文字修改；逐处记录并同步实际文件表、路径清单、开发交接、协作记录和 S3。不改业务逻辑、测试、web/、BuildingBlocks、旧迁移及快照；必要模型调整另核实授权。
5. 用户授权的一次性本地提交已完成（2d1a81f），未推送；之后规范/脚本成果不再暂存提交。接手时按最新用户授权决定后续操作，不能沿用此前一次性提交或推送授权。

最新实际记录为 C2567，下一编号 C2568；始终以修改记录中的真实编号为准，不按历史正文提及的编号推断。
