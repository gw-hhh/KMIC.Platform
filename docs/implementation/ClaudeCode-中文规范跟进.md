# Claude Code 中文规范跟进

记录 Claude Code 在后续开发中，按新中文表达要求新增或修改过的代码，供后续中文整理时排除已处理的范围，避免重复阅读和修改。

## 记录规则

- 只记录实际处理过的内容，不做全局检查，也不改写已有代码的中文。
- 每次修改后立即追加一行：时间、文件相对路径、具体类 / 方法 / 成员、处理范围、简短说明。
- 处理范围写明：
  - **整份完成**：整份文件的中文都已符合要求。只有确实整份核对过才这样标。
  - **部分**：只处理了列出的成员，其余未检查。
- 整份完成的文件，同时把相对路径追加到 `ClaudeCode-已按中文规范处理的文件.md`（一行一个路径，去重，只放路径），供后续整理时直接跳过。只处理了部分成员的文件不放进去，只记在本文件的表里。
- 中文范围包括注释、XML 文档注释、特性中的名称和描述、权限名、校验和错误提示、界面文案、硬编码字符串、说明文档。
- 路径相对 `KMIC.Platform/`，时间为本机时间。

## 中文表达要求（2026-11-13）

- 准确、直观、顺口、自然、简洁；不生硬直译，不编造业务含义。
- 注释保留简短用途和必要约束（权限、调用条件、事务、并发、失败行为、特殊顺序、容易误用的原因），常规步骤不写；已有表达准确的保留。
- 同一概念在注释、特性、权限名、提示、文档中称呼统一，例如 Specimen 统一叫“示例”。
- 只改表达，不夹带功能变化；涉及字符串比较、协议、持久化数据、数据库列说明、生成行为时先核实影响。

## 对比起点

- 提交 `3ec8914`（2026-11-13 18:33，“Notifications：处理发送请求，Sample 接入通知”）。
- 起点时工作区已有未提交的 Notifications 改动（Channels、Sending、`Delivery` 等，按旧写法写的）。它们没有原版可对比，所以另存了本地快照 `refs/baselines/comment-req-01`：
  - 对比单个文件：`git diff refs/baselines/comment-req-01 -- <文件路径>`
  - 不需要时删除：`git update-ref -d refs/baselines/comment-req-01`
- 这些文件在下面记录之前，都视为未处理。

## 记录

| 时间 | 文件 | 类 / 方法 / 成员 | 范围 | 说明 |
|---|---|---|---|---|
| 2026-11-13 18:49 | （暂无） | | | 建立记录，尚无代码按新要求改动 |
| 2026-11-13 18:58 | `tests/Kmic.Api.Tests/Notifications/ReceivedMail.cs` | `ReceivedMail` 及其参数注释 | 整份完成 | 新增，假 SMTP 服务器收到的一封邮件 |
| 2026-11-13 18:58 | `tests/Kmic.Api.Tests/Notifications/StubSmtpServer.cs` | `StubSmtpServer` 的全部成员，内部类 `Transaction` | 整份完成 | 新增，邮件渠道测试用的最小 SMTP 服务器 |
| 2026-11-13 18:58 | `tests/Kmic.Api.Tests/Notifications/MailKitEmailSenderTests.cs` | `MailKitEmailSenderTests` 的全部成员 | 整份完成 | 新增，邮件渠道的发送与失败分类测试 |
| 2026-11-13 18:58 | `docs/modules/notifications/README.md` | 第 15 节新增的“投递队列与邮件、短信渠道接口”段落、小调整、“未做”一行 | 部分 | 其余章节未检查 |
| 2026-11-13 18:58 | `docs/implementation/S3-平台模块.md` | S3-05 行的状态文字、2026-11-13 新增的日志行 | 部分 | 其余内容未检查 |
| 2026-11-13 18:59 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Channels/UnconfiguredSmsGateway.cs` | `UnconfiguredSmsGateway` 的 remarks | 部分 | 去掉“实施待办”的说法，直接写原因；类的 summary 和失败提示文字未改 |
| 2026-11-13 18:58 | `docs/framework/05-infrastructure/05-11-文件与通知.md` | 修订记录里新增的 2026-11-13“第 8.1～8.3 节实现时补全”一行 | 部分 | 其余内容未检查 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Domain/Templates/NotificationTemplateId.cs` | NotificationTemplateId | 整份完成 | 新增，通知模板 ID |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Domain/Templates/TemplateVersionId.cs` | TemplateVersionId | 整份完成 | 新增，模板版本 ID |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Domain/Templates/TemplateChannel.cs` | TemplateChannel 及邮件、短信、站内消息三个取值 | 整份完成 | 新增，取值的显示名和说明 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Domain/Templates/TemplateStatus.cs` | TemplateStatus 及启用、退役两个取值 | 整份完成 | 新增，取值的显示名和说明 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Domain/Templates/TemplateVersion.cs` | TemplateVersion 的常量、属性、Create | 整份完成 | 新增，模板的发布版本 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Domain/Templates/NotificationTemplate.cs` | NotificationTemplate 的常量、属性、Create、Synchronize、Retire、Publish、RevertToDefault、CanChange 等 | 整份完成 | 新增，通知模板聚合 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Domain/Templates/ITemplateRepository.cs` | ITemplateRepository 的三个方法 | 整份完成 | 新增，模板仓储 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Domain/Errors/NotificationsErrors.cs` | TemplateRetired、TemplateChannelNotDeclared、TemplateKeyDuplicated、TemplateVersionDuplicated、PreviewModelNotAllowed | 部分 | 新增的五个错误的注释和提示，其余错误未检查 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/NotificationsPermissions.cs` | NotificationsPermissions 及两个权限点 | 整份完成 | 新增 TemplateView、TemplateManage 的显示名和说明，重写类注释 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/INotificationsQueryDb.cs` | INotificationsQueryDb | 整份完成 | 新增 Templates、TemplateVersions 两个入口的说明 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/Processing/ChannelContent.cs` | ChannelContent | 整份完成 | 新增，一个渠道渲染出的内容 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/Processing/RenderedContent.cs` | RenderedContent 及 For | 整份完成 | 按渠道重写，注释同步更新 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/Processing/NotificationProcessor.cs` | 类的 summary 与 remarks、RenderAsync、VersionsOf 的注释 | 部分 | 改为按渠道取生效内容；其余成员的注释未检查 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/TemplateChannels.cs` | TemplateChannels 的两个方法 | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/TemplateHash.cs` | TemplateHash | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/ChannelRendering.cs` | ChannelRendering | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/TemplateModels.cs` | TemplateModels | 整份完成 | 新增，从处理器里挪出 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Sync/SyncTemplateCatalogCommand.cs` | SyncTemplateCatalogCommand | 整份完成 | 新增，同步模板的启动任务命令 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Sync/TemplateSyncResult.cs` | TemplateSyncResult | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Sync/SyncTemplateCatalogCommandHandler.cs` | SyncTemplateCatalogCommandHandler | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/List/ListTemplatesQuery.cs` | ListTemplatesQuery | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/List/TemplateListResponse.cs` | TemplateListResponse | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/List/TemplateListItem.cs` | TemplateListItem | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/List/ListTemplatesQueryHandler.cs` | ListTemplatesQueryHandler | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Get/GetTemplateQuery.cs` | GetTemplateQuery | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Get/TemplateDetailResponse.cs` | TemplateDetailResponse | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Get/TemplateChannelInfo.cs` | TemplateChannelInfo | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Get/TemplateVersionInfo.cs` | TemplateVersionInfo | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Get/GetTemplateQueryHandler.cs` | GetTemplateQueryHandler | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Preview/PreviewTemplateQuery.cs` | PreviewTemplateQuery | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Preview/TemplatePreviewResponse.cs` | TemplatePreviewResponse | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Preview/PreviewTemplateQueryHandler.cs` | PreviewTemplateQueryHandler | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Publish/PublishTemplateCommand.cs` | PublishTemplateCommand | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Publish/PublishTemplateCommandValidator.cs` | PublishTemplateCommandValidator | 整份完成 | 新增，无中文 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Publish/PublishTemplateCommandHandler.cs` | PublishTemplateCommandHandler | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Revert/RevertTemplateCommand.cs` | RevertTemplateCommand | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Revert/RevertTemplateCommandValidator.cs` | RevertTemplateCommandValidator | 整份完成 | 新增，无中文 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Revert/RevertTemplateCommandHandler.cs` | RevertTemplateCommandHandler | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Configurations/NotificationTemplateConfiguration.cs` | NotificationTemplateConfiguration，含数据库表注释 | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Configurations/TemplateVersionConfiguration.cs` | TemplateVersionConfiguration，含数据库表注释 | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Repositories/TemplateRepository.cs` | TemplateRepository | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Templates/SyncTemplateCatalogTask.cs` | SyncTemplateCatalogTask | 整份完成 | 新增，启动任务 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Templates/ListTemplatesEndpoint.cs` | ListTemplatesEndpoint 的摘要与说明 | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Templates/GetTemplateEndpoint.cs` | GetTemplateEndpoint 的摘要与说明 | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Templates/PreviewTemplateRequest.cs` | PreviewTemplateRequest | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Templates/PreviewTemplateEndpoint.cs` | PreviewTemplateEndpoint 的摘要与说明 | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Templates/PublishTemplateRequest.cs` | PublishTemplateRequest | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Templates/PublishTemplateEndpoint.cs` | PublishTemplateEndpoint 的摘要与说明 | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Templates/RevertTemplateRequest.cs` | RevertTemplateRequest | 整份完成 | 新增 |
| 2026-11-13 19:55 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Templates/RevertTemplateEndpoint.cs` | RevertTemplateEndpoint 的摘要与说明 | 整份完成 | 新增 |
| 2026-11-13 19:55 | `tests/Kmic.Api.Tests/Notifications/NotificationTemplateTests.cs` | NotificationTemplateTests 的全部成员 | 整份完成 | 新增，模板同步、发布、恢复、预览的测试 |
| 2026-11-13 19:55 | `docs/modules/notifications/README.md` | 第 15 节新增的“模板同步、版本与管理”段落、小调整、“未做”一行 | 部分 | 其余章节未检查 |
| 2026-11-13 19:55 | `docs/implementation/S3-平台模块.md` | S3-05 行的状态文字、新增的模板同步日志行 | 部分 | 其余内容未检查 |
| 2026-11-13 19:55 | `docs/framework/05-infrastructure/05-11-文件与通知.md` | 修订记录里新增的“第 7.4、7.5 节实现时补全”一行 | 部分 | 其余内容未检查 |
| 2026-11-13 21:17 | `src/BuildingBlocks/Kmic.BuildingBlocks.Testing/TestContainerNames.cs` | TestContainerNames | 整份完成 | 新增，测试容器的命名约定 |
| 2026-11-13 21:17 | `src/BuildingBlocks/Kmic.BuildingBlocks.Testing/TestPorts.cs` | TestPorts | 整份完成 | 新增，测试容器的映射端口选在系统临时端口范围之外 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Domain/Deliveries/DeliveryColumns.cs` | DeliveryColumns 及九个列名常量 | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Domain/Deliveries/IDeliveryRepository.cs` | RetryAsync、AbandonAsync、PurgeAsync、ClearSecretBodiesAsync | 部分 | 新增四个方法的注释，原有的 FindExistingAsync 未检查 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Domain/Notifications/INotificationRepository.cs` | PurgeAsync | 部分 | 新增方法的注释，原有的 ExistsForAsync 未检查 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Domain/Captured/ICapturedMessageRepository.cs` | ICapturedMessageRepository 及 PurgeAsync | 整份完成 | 新增 PurgeAsync，类注释未改 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Domain/Errors/NotificationsErrors.cs` | DeliveryNotRetryable、DeliverySecretExpired、DeliveryNotAbandonable | 部分 | 新增三个错误的注释和提示 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/NotificationsPermissions.cs` | DeliveryView、DeliveryManage、CapturedView | 整份完成 | 新增三个权限点的显示名和说明 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/INotificationsQueryDb.cs` | Deliveries、CapturedMessages | 整份完成 | 新增两个入口的说明 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/List/ListDeliveriesQuery.cs` | ListDeliveriesQuery | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/List/DeliveryListItem.cs` | DeliveryListItem | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/List/DeliverySort.cs` | DeliverySort | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/List/ListDeliveriesQueryValidator.cs` | ListDeliveriesQueryValidator 的校验提示 | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/List/ListDeliveriesQueryHandler.cs` | ListDeliveriesQueryHandler | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Get/GetDeliveryQuery.cs` | GetDeliveryQuery | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Get/DeliveryDetailResponse.cs` | DeliveryDetailResponse | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Get/GetDeliveryQueryHandler.cs` | GetDeliveryQueryHandler | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Retry/RetryDeliveryCommand.cs` | RetryDeliveryCommand | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Retry/RetryDeliveryCommandHandler.cs` | RetryDeliveryCommandHandler | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Abandon/AbandonDeliveryCommand.cs` | AbandonDeliveryCommand | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Abandon/AbandonDeliveryCommandValidator.cs` | AbandonDeliveryCommandValidator | 整份完成 | 新增，无中文 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Abandon/AbandonDeliveryCommandHandler.cs` | AbandonDeliveryCommandHandler | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Purge/PurgeDeliveriesCommand.cs` | PurgeDeliveriesCommand | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Purge/PurgeDeliveriesCommandHandler.cs` | PurgeDeliveriesCommandHandler | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Purge/ClearSecretBodiesCommand.cs` | ClearSecretBodiesCommand | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Purge/ClearSecretBodiesCommandHandler.cs` | ClearSecretBodiesCommandHandler | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/Captured/List/ListCapturedQuery.cs` | ListCapturedQuery | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/Captured/List/CapturedListItem.cs` | CapturedListItem | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/Captured/List/CapturedSort.cs` | CapturedSort | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/Captured/List/ListCapturedQueryValidator.cs` | ListCapturedQueryValidator 的校验提示 | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/Captured/List/ListCapturedQueryHandler.cs` | ListCapturedQueryHandler | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/Captured/Get/GetCapturedQuery.cs` | GetCapturedQuery | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/Captured/Get/CapturedDetailResponse.cs` | CapturedDetailResponse | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/Captured/Get/GetCapturedQueryHandler.cs` | GetCapturedQueryHandler | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/Captured/Purge/PurgeCapturedCommand.cs` | PurgeCapturedCommand | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Application/Captured/Purge/PurgeCapturedCommandHandler.cs` | PurgeCapturedCommandHandler | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/NotificationsModule.cs` | ConfigureScheduling 里新增的三个定时任务及显示名 | 部分 | 其余成员未检查 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Configurations/DeliveryConfiguration.cs` | AbandonedAt、AbandonReason 的列注释 | 部分 | 其余成员未检查 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Repositories/DeliveryRepository.cs` | DeliveryRepository | 整份完成 | 新增四个方法，文件里没有中文 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Repositories/CapturedMessageRepository.cs` | CapturedMessageRepository | 整份完成 | 新增 PurgeAsync，文件里没有中文 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Deliveries/ListDeliveriesRequest.cs` | ListDeliveriesRequest | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Deliveries/ListDeliveriesEndpoint.cs` | ListDeliveriesEndpoint 的摘要与说明 | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Deliveries/GetDeliveryEndpoint.cs` | GetDeliveryEndpoint 的摘要与说明 | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Deliveries/RetryDeliveryEndpoint.cs` | RetryDeliveryEndpoint 的摘要与说明 | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Deliveries/AbandonDeliveryRequest.cs` | AbandonDeliveryRequest | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Deliveries/AbandonDeliveryEndpoint.cs` | AbandonDeliveryEndpoint 的摘要与说明 | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Captured/ListCapturedRequest.cs` | ListCapturedRequest | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Captured/ListCapturedEndpoint.cs` | ListCapturedEndpoint 的摘要与说明 | 整份完成 | 新增 |
| 2026-11-13 21:17 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Captured/GetCapturedEndpoint.cs` | GetCapturedEndpoint 的摘要与说明 | 整份完成 | 新增 |
| 2026-11-13 21:17 | `tests/Kmic.Api.Tests/Notifications/DeliveryManagementTests.cs` | DeliveryManagementTests 的全部成员 | 整份完成 | 新增，投递管理与清理的测试 |
| 2026-11-13 21:17 | `tests/Kmic.Api.Tests/Notifications/CapturedMessageTests.cs` | CapturedMessageTests 的全部成员 | 整份完成 | 新增，捕获内容的测试 |
| 2026-11-13 21:17 | `docs/modules/notifications/README.md` | 第 15 节新增的“投递管理与清理”段落、小调整、“未做”一行 | 部分 | 其余章节未检查 |
| 2026-11-13 21:17 | `docs/implementation/S3-平台模块.md` | S3-05 行的状态文字、新增的投递管理与清理日志行 | 部分 | 其余内容未检查 |
| 2026-11-13 21:17 | `docs/framework/05-infrastructure/05-11-文件与通知.md` | 修订记录里新增的“第 8.1、8.2 节实现时补全”一行 | 部分 | 其余内容未检查 |
| 2026-11-13 21:17 | `docs/implementation/00-README.md` | T-08 行 | 部分 | 标为搁置，其余未检查 |
| 2026-11-13 21:17 | `docs/framework/00-README.md` | 延后事项表里新增的“短信登录与重置密码…”一行 | 部分 | 其余未检查 |
| 2026-11-13 21:17 | `docs/modules/identity/README.md` | “实施待办”一段中关于 T-08 的描述 | 部分 | 改为搁置，其余未检查 |
| 2026-11-13 21:17 | `docs/implementation/换机器继续开发.md` | 第 4 节新增的“Docker 命名约定”一条 | 部分 | 其余未检查 |
| 2026-11-13 21:59 | `scripts/dev.cs` | 脚本里的全部注释和提示文字 | 整份完成 | 新增，本机一键运行（数据库容器、迁移、首个管理员、签名密钥、启动 Api） |
| 2026-11-13 21:59 | `docs/implementation/本地运行.md` | 全文 | 整份完成 | 新增 |
| 2026-11-13 21:59 | `README.md` | 目录前新增的“本机运行”一条 | 部分 | 其余未检查 |
| 2026-11-13 21:59 | `.gitignore` | 新增的 .local 一行及其注释 | 部分 | 其余未检查 |
| 2026-11-13 21:59 | `tests/Kmic.Api.Tests/Notifications/DeliveryManagementTests.cs` | 变量 dedups 改名为 keys | 部分 | 只改名字，通过拼写检查；其余按之前的记录 |
| 2026-11-13 23:10 | `src/BuildingBlocks/Kmic.BuildingBlocks.Application/Auditing/IActorReferences.cs` | 接口及两个方法的说明 | 整份完成 | 新增，业务历史里解析操作人与角色名 |
| 2026-11-13 23:10 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/History/StateHistoryQuery.cs` | 类说明、方法注释、“已删除”显示文字 | 整份完成 | 新增，业务历史查询的实现 |
| 2026-11-13 23:10 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Pipeline/ModuleQueryContext.cs` | 类说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Pipeline/ModuleQueryContexts.cs` | 类说明、异常消息 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Persistence/PersistenceServiceCollectionExtensions.cs` | AddModuleContextRegistries 上的注释 | 部分 | 其余未检查 |
| 2026-11-13 23:10 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Files/IFileStorage.cs` | ExistsAsync 的说明 | 部分 | 其余未检查 |
| 2026-11-13 23:10 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Files/FileStorageKeys.cs` | TryGetFileId、CreatedAt 的说明 | 部分 | 其余未检查 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Domain/Files/StorageIssueId.cs` | 类说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Domain/Files/StorageIssueKind.cs` | 类说明、两个种类的说明与显示名 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Domain/Files/StorageIssueStatus.cs` | 类说明、两个状态的说明与显示名 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Domain/Files/StorageIssue.cs` | 类、属性、方法说明 | 整份完成 | 新增，对账发现的存储问题 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Domain/Files/IStorageIssueRepository.cs` | 接口说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Domain/Files/StoredObjectRef.cs` | 类说明与属性说明 | 整份完成 | 新增，对账用的查询结果 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Domain/Files/StoredFile.cs` | GeneratedProfilePrefix、MarkLost、对象丢失流转的注释 | 部分 | 其余未检查 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Domain/Files/StoredFileAction.cs` | Lose 的说明与显示名 | 部分 | 其余未检查 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Domain/Files/IStoredFileRepository.cs` | FindExistingAsync、FindExpectedObjectsAsync 的说明 | 部分 | 其余未检查 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Domain/Errors/FilesErrors.cs` | StorageIssueNotFound、ReconcileSuspicious 的说明与提示 | 部分 | 其余未检查 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Application/IStorageInventory.cs` | 接口与方法说明 | 整份完成 | 新增，对账读存储的窄接口 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Application/InventoryObject.cs` | 类说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Application/FilesOptions.cs` | ReconcileMinAgeDays、ReconcileBatchSize、ReconcileMaxChanges、GeneratedFileMaxMb、ReconcileMinAge 的说明 | 部分 | 其余未检查 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Application/Cleanup/ReconcileStorageCommand.cs` | 类说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Application/Cleanup/ReconcileStorageCommandHandler.cs` | 方法注释、结果说明文字 | 整份完成 | 新增，对账 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Application/Administration/ResolveStorageIssueCommand.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Application/Administration/StorageIssueItem.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Application/Administration/StorageIssueSort.cs` | 类与字段说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Application/Administration/ListStorageIssuesQuery.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Application/Administration/ListStorageIssuesQueryHandler.cs` | 方法注释 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Application/Administration/FileDetail.cs` | History 参数的说明 | 部分 | 其余未检查 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Application/ContractQueries/GetReadableFileQuery.cs` | 类说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Application/ContractQueries/GetReadableFileQueryHandler.cs` | 方法注释 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Application/ContractQueries/FileContents.cs` | 类说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Application/ContractQueries/GetFileDownloadQueryHandler.cs` | 整份注释 | 整份完成 | 改写：生成的文件登记完成就能下载 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Application/Attachments/GeneratedFiles.cs` | 类说明、方法注释、异常消息、登记豁免的理由 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Contracts/GeneratedFile.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Contracts/IGeneratedFiles.cs` | 接口与方法说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Contracts/IFileContents.cs` | 接口与方法说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Contracts/FileContent.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Contracts/RegisterGeneratedFile.cs` | MessageName 的说明 | 部分 | 其余未检查 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Infrastructure/Persistence/Configurations/StorageIssueConfiguration.cs` | 表说明与注释 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Infrastructure/FilesModule.cs` | 对账任务的显示名 | 部分 | 其余未检查 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Presentation/Administration/ListStorageIssuesRequest.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Presentation/Administration/ListStorageIssuesEndpoint.cs` | 接口摘要与说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Presentation/Administration/ResolveStorageIssueRequest.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Files/Kmic.Files.Presentation/Administration/ResolveStorageIssueEndpoint.cs` | 接口摘要与说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Identity/Kmic.Identity.Application/ContractQueries/ActorReferences.cs` | 类说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Identity/Kmic.Identity.Infrastructure/IdentityModule.cs` | IActorReferences 注册上的注释 | 部分 | 其余未检查 |
| 2026-11-13 23:10 | `src/Modules/Operations/Kmic.Operations.Application/Audit/AuditFilters.cs` | 类与方法说明、注释 | 整份完成 | 新增，列表和导出共用的筛选 |
| 2026-11-13 23:10 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/AuditExportOptions.cs` | 类与属性说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/ExportAuditCommand.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/ExportAuditCommandValidator.cs` | 校验提示 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/ExportAuditJob.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/ExportAuditJobHandler.cs` | 类说明、方法注释、文件名和进度文字 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/AuditCsv.cs` | 类说明、表头、方法说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/AuditExportSummary.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/AuditExportCheckpoint.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/RegisterAuditExportFileCommand.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/RegisterAuditExportFileCommandHandler.cs` | 类说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/GetAuditExportFileQuery.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/GetAuditExportFileQueryHandler.cs` | 类说明、注释 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Operations/Kmic.Operations.Application/OperationsPermissions.cs` | AuditExport 的说明与显示名 | 部分 | 其余未检查 |
| 2026-11-13 23:10 | `src/Modules/Operations/Kmic.Operations.Domain/Errors/OperationsErrors.cs` | AuditExportNotFound、AuditExportNotReady、AuditExportFailed 的说明与提示 | 部分 | 其余未检查 |
| 2026-11-13 23:10 | `src/Modules/Operations/Kmic.Operations.Presentation/Audit/ExportAuditRequest.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Operations/Kmic.Operations.Presentation/Audit/ExportAuditEndpoint.cs` | 接口摘要与说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `src/Modules/Operations/Kmic.Operations.Presentation/Audit/DownloadAuditExportEndpoint.cs` | 接口摘要与说明 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `tests/Kmic.Api.Tests/Files/FileReconciliationTests.cs` | 类说明、测试里的断言说明、测试数据 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `tests/Kmic.Api.Tests/Files/FileReconciliationGuardTests.cs` | 类说明、测试数据 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `tests/Kmic.Api.Tests/Files/GeneratedFileTests.cs` | 类说明、测试里的断言说明、测试数据 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `tests/Kmic.Api.Tests/Files/FilesApiTestBase.cs` | 新增参数的处理 | 部分 | 其余按之前的记录 |
| 2026-11-13 23:10 | `tests/Kmic.Api.Tests/Files/FileAdministrationTests.cs` | Detail_ShowsTheStateHistoryInOrder_WithTheNamesOfThePeopleInvolved 里的说明与测试数据 | 部分 | 其余未检查 |
| 2026-11-13 23:10 | `tests/Kmic.Api.Tests/Operations/AuditExportTests.cs` | 类说明、等待方法的注释与超时信息、测试数据 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `tests/Kmic.Api.Tests/Identity/JobAccessLoadingTests.cs` | 类说明、断言说明、测试数据 | 整份完成 | 新增 |
| 2026-11-13 23:10 | `tests/Kmic.Api.Tests/Identity/AccessContextLoadingBenchmark.cs` | 类说明、报告文字、测试数据 | 整份完成 | 新增，默认不跑的参考测量 |
| 2026-11-13 23:10 | `tests/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure.Tests/Files/FileStorageContractTests.cs` | Exists_TrueAfterWrite_FalseAfterDeleteOrWhenNeverWritten | 部分 | 其余未检查 |
| 2026-11-13 23:10 | `tests/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure.Tests/Files/S3FileStorageTests.cs` | Exists_WithVersioning_DeleteMarkerHidesTheObjectButNotItsVersion、Admin 的注释与断言说明 | 部分 | 其余未检查 |
| 2026-11-13 23:10 | `BannedSymbols.Modules.txt` | 文件关联相关的三组新增条目的注释与替代做法 | 部分 | 其余未检查 |
| 2026-11-13 23:10 | `docs/modules/files/README.md` | 13.1 新增的行、第 15 节新增的两段 | 部分 | 其余未检查 |
| 2026-11-13 23:10 | `docs/modules/operations/README.md` | 状态行、审计导出一节、第 13 节被吞掉的行内代码的补回 | 部分 | 其余未检查 |
| 2026-11-13 23:10 | `docs/framework/05-infrastructure/05-09-审计与业务历史.md` | 修订记录新增一行 | 部分 | 其余未检查 |
| 2026-11-13 23:10 | `docs/framework/05-infrastructure/05-11-文件与通知.md` | 修订记录新增一行 | 部分 | 其余未检查 |
| 2026-11-13 23:10 | `docs/framework/05-infrastructure/05-04-认证与会话.md` | 修订记录新增一行（压测参考数据） | 部分 | 其余未检查 |
| 2026-11-13 23:10 | `docs/implementation/S3-平台模块.md` | S3-02、S3-04、S3-06 及新增的进展记录 | 部分 | 其余未检查 |
| 2026-11-14 01:10 | `src/Modules/Notifications/Kmic.Notifications.Application/Channels/EmailAttachment.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Notifications/Kmic.Notifications.Application/Channels/EmailMessage.cs` | Attachments 参数的说明 | 部分 | 其余按之前的记录 |
| 2026-11-14 01:10 | `src/Modules/Notifications/Kmic.Notifications.Application/Channels/NotificationsDeliveryOptions.cs` | MaxAttachmentMb 的说明 | 部分 | 其余未检查 |
| 2026-11-14 01:10 | `src/Modules/Notifications/Kmic.Notifications.Application/Processing/NotificationRequest.cs` | AttachmentFileIds 参数的说明 | 部分 | 其余未检查 |
| 2026-11-14 01:10 | `src/Modules/Notifications/Kmic.Notifications.Application/Processing/NotificationProcessor.cs` | AttachmentsOf 的注释 | 部分 | 其余未检查 |
| 2026-11-14 01:10 | `src/Modules/Notifications/Kmic.Notifications.Domain/Deliveries/Delivery.cs` | AttachmentFileIds 属性的说明 | 部分 | 其余未检查 |
| 2026-11-14 01:10 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Sending/DeliveryItem.cs` | AttachmentFileIds 参数的说明 | 部分 | 其余未检查 |
| 2026-11-14 01:10 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Sending/DeliveryWork.cs` | SendEmailAsync 的注释 | 部分 | 其余未检查 |
| 2026-11-14 01:10 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Sending/AttachmentLoader.cs` | 类说明、注释、失败原因文字 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Sending/AttachmentLoad.cs` | 类与属性说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `tests/Kmic.Api.Tests/Notifications/DeliveryAttachmentTests.cs` | 类说明、断言说明、测试数据 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `tests/Kmic.Api.Tests/Notifications/RecordingEmailSender.cs` | 类说明与注释 | 整份完成 | 改写：记录附件内容 |
| 2026-11-14 01:10 | `tests/Kmic.Api.Tests/Notifications/MailKitEmailSenderTests.cs` | AnAttachment_IsSentAsABase64PartWithItsFileName | 部分 | 其余未检查 |
| 2026-11-14 01:10 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Messaging/MessagingInstanceTable.cs` | 类说明、表注释、字段说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Messaging/InstanceHeartbeat.cs` | 类说明、注释 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Messaging/InstanceIdentity.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Messaging/MessageRouteAdmin.cs` | 类说明、方法注释、放弃时写进错误说明的文字 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Messaging/RebuildAdmin.cs` | 类说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Messaging/RebuildRunStore.cs` | ConfirmAsync、AbandonAsync、ListAsync、NotifyJobsAsync 的说明与注释 | 部分 | 其余未检查 |
| 2026-11-14 01:10 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Messaging/MessagingOptions.cs` | HeartbeatInterval 的说明 | 部分 | 其余未检查 |
| 2026-11-14 01:10 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Messaging/ReadModelRebuildHandler.cs` | 确认收尾相关的注释 | 部分 | 其余未检查 |
| 2026-11-14 01:10 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Persistence/PersistenceServiceCollectionExtensions.cs` | AddOperationsAdminServices 的注释 | 部分 | 其余未检查 |
| 2026-11-14 01:10 | `src/BuildingBlocks/Kmic.BuildingBlocks.Application/Integration/IMessageRouteAdmin.cs` | 接口、方法与常量说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/BuildingBlocks/Kmic.BuildingBlocks.Application/Integration/RouteAdminOutcome.cs` | 枚举与成员说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/BuildingBlocks/Kmic.BuildingBlocks.Application/Integration/RouteFinalizeResult.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/BuildingBlocks/Kmic.BuildingBlocks.Application/Integration/RedeliverResult.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/BuildingBlocks/Kmic.BuildingBlocks.Application/Integration/IReadModelRebuildAdmin.cs` | 接口与方法说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/BuildingBlocks/Kmic.BuildingBlocks.Application/Integration/RebuildRunState.cs` | 枚举与成员说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/BuildingBlocks/Kmic.BuildingBlocks.Application/Integration/RebuildRunInfo.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/BuildingBlocks/Kmic.BuildingBlocks.Application/Integration/RebuildAdminOutcome.cs` | 枚举与成员说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/BuildingBlocks/Kmic.BuildingBlocks.Application/Jobs/ReadModelRebuildJob.cs` | ConfirmMissing、Scanned 参数的说明 | 部分 | 其余未检查 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Domain/Messages/MessageRouteStatus.cs` | 类说明、状态说明与显示名 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Domain/Messages/RebuildRunStatus.cs` | 类说明、状态说明与显示名 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Domain/Errors/OperationsErrors.cs` | 路由、补投、读模型重建相关错误的说明与提示 | 部分 | 其余按之前的记录 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/MessageRouteRow.cs` | 类与属性说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/RouteItem.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/RouteBuild.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/ListRoutesQuery.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/ListRoutesQueryHandler.cs` | 类说明与注释 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/FinalizeRouteCommand.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/FinalizeRouteResponse.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/FinalizeRouteCommandHandler.cs` | 类说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/ReactivateRouteCommand.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/ReactivateRouteCommandHandler.cs` | 类说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/RedeliverCommand.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/RedeliverResponse.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/RedeliverCommandHandler.cs` | 类说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/RedeliverCommandValidator.cs` | 校验提示 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Instances/MessagingInstanceRow.cs` | 类与属性说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Instances/InstanceSubscriptions.cs` | 类与方法说明、注释 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Instances/InstanceSubscription.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Instances/InstanceItem.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Instances/ListInstancesQuery.cs` | 类说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Instances/ListInstancesQueryHandler.cs` | 类说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Rebuilds/RebuildItem.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Rebuilds/ListRebuildsQuery.cs` | 类说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Rebuilds/ListRebuildsQueryHandler.cs` | 类说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Rebuilds/TriggerRebuildCommand.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Rebuilds/ConfirmRebuildCommand.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Rebuilds/AbandonRebuildCommand.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Rebuilds/TriggerRebuildCommandHandler.cs` | 类说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Rebuilds/ConfirmRebuildCommandHandler.cs` | 类说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Rebuilds/AbandonRebuildCommandHandler.cs` | 类说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Rebuilds/RebuildOutcomes.cs` | 类说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Application/IOperationsQueryDb.cs` | MessageRoutes、MessagingInstances 的说明 | 部分 | 其余未检查 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/OperationsQueryDbContext.cs` | 路由表与心跳表查询上的注释 | 部分 | 其余未检查 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/Migrations/20261113155025_MessagingInstances.cs` | 迁移里的注释 | 部分 | 其余是工具生成的 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/ListRoutesEndpoint.cs` | 接口摘要、说明、参数说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/ListInstancesEndpoint.cs` | 接口摘要与说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/FinalizeRouteRequest.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/FinalizeRouteEndpoint.cs` | 接口摘要与说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/ReactivateRouteRequest.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/ReactivateRouteEndpoint.cs` | 接口摘要与说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/RedeliverRequest.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/RedeliverEndpoint.cs` | 接口摘要与说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/ListRebuildsEndpoint.cs` | 接口摘要与说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/RebuildRequest.cs` | 类与参数说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/TriggerRebuildEndpoint.cs` | 接口摘要与说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/ConfirmRebuildEndpoint.cs` | 接口摘要与说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/AbandonRebuildEndpoint.cs` | 接口摘要与说明 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `tests/Kmic.Api.Tests/Operations/RouteAndInstanceTests.cs` | 类说明、断言说明、测试数据 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `tests/Kmic.Api.Tests/Operations/RouteFinalizeTests.cs` | 类说明、断言说明、测试数据 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `tests/Kmic.Api.Tests/Operations/RedeliverTests.cs` | 类说明、断言说明、测试数据 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `tests/Kmic.Api.Tests/Operations/RebuildManagementTests.cs` | 类说明、断言说明、测试数据 | 整份完成 | 新增 |
| 2026-11-14 01:10 | `tests/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure.Tests/Messaging/ReadModelRebuildTests.cs` | Admin_ 开头的三个测试及它们的辅助方法 | 部分 | 其余未检查 |
| 2026-11-14 01:10 | `docs/modules/operations/README.md` | 状态行、第 13 节末新增的“路由、实例、终结、补投、读模型重建”一节 | 部分 | 其余未检查 |
| 2026-11-14 01:10 | `docs/modules/notifications/README.md` | 邮件附件的实现说明 | 部分 | 其余未检查 |
| 2026-11-14 01:10 | `docs/framework/05-infrastructure/05-07-模块间通信.md` | 修订记录新增一行 | 部分 | 其余未检查 |
| 2026-11-14 01:10 | `docs/framework/00-README.md` | 延后事项新增两行 | 部分 | 其余未检查 |
| 2026-11-14 01:10 | `docs/implementation/S3-平台模块.md` | S3-02、S3-05 的状态与新增的进展记录 | 部分 | 其余未检查 |
| 2026-11-14 03:02 | `web/src/platform/api/kmicError.ts` | 全部 | 整份完成 | 前端骨架新写，注释、提示、测试名都按要求写 |
| 2026-11-14 03:02 | `web/src/platform/api/kmicFetch.ts` | 全部 | 整份完成 | 前端骨架新写，注释、提示、测试名都按要求写 |
| 2026-11-14 03:02 | `web/src/platform/api/client.ts` | 全部 | 整份完成 | 前端骨架新写，注释、提示、测试名都按要求写 |
| 2026-11-14 03:02 | `web/src/platform/api/index.ts` | 全部 | 整份完成 | 前端骨架新写，注释、提示、测试名都按要求写 |
| 2026-11-14 03:02 | `web/src/platform/api/kmicFetch.test.ts` | 全部 | 整份完成 | 前端骨架新写，注释、提示、测试名都按要求写 |
| 2026-11-14 03:02 | `web/src/platform/api/kmicError.test.ts` | 全部 | 整份完成 | 前端骨架新写，注释、提示、测试名都按要求写 |
| 2026-11-14 03:02 | `web/src/platform/session/session.ts` | 全部 | 整份完成 | 前端骨架新写，注释、提示、测试名都按要求写 |
| 2026-11-14 03:02 | `web/src/platform/session/session.test.ts` | 全部 | 整份完成 | 前端骨架新写，注释、提示、测试名都按要求写 |
| 2026-11-14 03:02 | `web/src/app/main.ts` | 全部 | 整份完成 | 前端骨架新写，注释、提示、测试名都按要求写 |
| 2026-11-14 03:02 | `web/src/app/router.ts` | 全部 | 整份完成 | 前端骨架新写，注释、提示、测试名都按要求写 |
| 2026-11-14 03:02 | `web/src/app/App.vue` | 全部 | 整份完成 | 前端骨架新写，注释、提示、测试名都按要求写 |
| 2026-11-14 03:02 | `web/src/shell/pages/HomePage.vue` | 全部 | 整份完成 | 前端骨架新写，注释、提示、测试名都按要求写 |
| 2026-11-14 03:02 | `web/env.d.ts` | 全部 | 整份完成 | 前端骨架新写，注释、提示、测试名都按要求写 |
| 2026-11-14 03:02 | `web/eslint.config.js` | 全部 | 整份完成 | 前端骨架新写，注释、提示、测试名都按要求写 |
| 2026-11-14 03:02 | `web/eslint/boundaries.js` | 全部 | 整份完成 | 前端骨架新写，注释、提示、测试名都按要求写 |
| 2026-11-14 03:02 | `web/eslint/restrictions.js` | 全部 | 整份完成 | 前端骨架新写，注释、提示、测试名都按要求写 |
| 2026-11-14 03:02 | `web/scripts/check-boundaries.mjs` | 全部 | 整份完成 | 前端骨架新写，注释、提示、测试名都按要求写 |
| 2026-11-14 03:02 | `web/scripts/generate-api.mjs` | 全部 | 整份完成 | 前端骨架新写，注释、提示、测试名都按要求写 |
| 2026-11-14 03:02 | `web/pnpm-workspace.yaml` | 全部 | 整份完成 | 前端骨架新写，注释、提示、测试名都按要求写 |
| 2026-11-14 03:02 | `web/tests/boundaries/eslint.config.js` | 全部 | 整份完成 | 前端骨架新写，注释、提示、测试名都按要求写 |
| 2026-11-14 03:02 | `web/tests/boundaries/legal/**`、`web/tests/boundaries/violations/**` | 全部样例文件 | 整份完成 | 边界样例，只有页面里的几个中文词 |
| 2026-11-14 03:02 | `scripts/check.cs` | 顶部说明、前端七步的名称、`RunWeb` 的注释 | 部分 | 其余未检查 |
| 2026-11-14 03:02 | `docs/implementation/S0-04-版本与许可.md` | 第 4 节前端版本行与第 7 节修订记录 | 部分 | 其余未检查 |
| 2026-11-14 03:02 | `docs/frontend/F03-前端架构.md`、`docs/frontend/F04-前端编码规范.md`、`docs/frontend/00-README.md` | 修订记录新增一行；F04 第 3.1 节 ESLint 配置行；README 进度表实现列 | 部分 | 其余未检查 |
| 2026-11-14 03:02 | `docs/implementation/S3-平台模块.md` | S3-07 状态、进展记录里的前端骨架一行 | 部分 | 其余未检查 |
| 2026-11-14 09:48 | `src/Modules/Operations/Kmic.Operations.Application/BackgroundJobs/Mine/` | GetMyJobQuery、GetMyJobQueryHandler、MyJobs、CancelMyJobCommand、CancelMyJobCommandHandler 及两个校验器 | 整份完成 | 新文件 |
| 2026-11-14 09:48 | `src/Modules/Operations/Kmic.Operations.Presentation/BackgroundJobs/` | GetMyJobEndpoint、CancelMyJobEndpoint | 整份完成 | 新文件，接口摘要与说明 |
| 2026-11-14 09:48 | `src/BuildingBlocks/Kmic.BuildingBlocks.Application/` | FeatureGateAttribute、IFeatureFlags、FeatureGateBehavior；UseCase.FeatureGate；CommonErrors.FeatureDisabled | 部分 | 只处理新增的成员 |
| 2026-11-14 09:48 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Features/` | FeatureFlags、FeatureServiceCollectionExtensions | 整份完成 | 新文件 |
| 2026-11-14 09:48 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Persistence/` | IDataScopeSql、DataScopeSql、SqlFragments | 整份完成 | 新文件 |
| 2026-11-14 09:48 | `src/Modules/Configuration/（Application/Features、Presentation/Features）` | GetFeaturesQuery、FeaturesResponse、GetFeaturesQueryHandler、GetFeaturesEndpoint | 整份完成 | 新文件 |
| 2026-11-14 09:48 | `src/Modules/Notifications/` | ITemplateRenderer.RenderStrictAsync、FluidTemplateRenderer 严格渲染、ChannelRendering 的 strict 参数、模板错误提示 | 部分 | 只处理新增的成员 |
| 2026-11-14 09:48 | `tests/（Operations/BackgroundJobTests、Notifications/TemplateRendererTests、NotificationTemplateTests、Configuration/FeatureTests、BuildingBlocks 下 DataScopeSqlTests、FeatureFlagsTests、FeatureGateTests 与相关夹具）` | 本批新增的测试 | 部分 | 只处理新增的测试，其余未检查 |
| 2026-11-14 09:48 | `docs/framework/05-infrastructure/（05-02、05-05、05-08、05-10）、docs/framework/00-README.md、docs/modules/（operations、notifications、configuration）、docs/implementation/（S0-04、S2-18-规则核对、S3-平台模块）` | 本批新增或改写的段落与修订记录 | 部分 | 其余未检查 |
| 2026-11-14 10:56 | `docs/framework/00-README.md` | 待决事项表新增三行（作业端点位置、功能开关关闭时后台入口的处理、功能开关包名） | 部分 | 其余未检查 |
| 2026-11-14 11:03 | `docs/implementation/协作交接记录.md` | 新文件，全部 | 整份完成 | 协作交接用 |
| 2026-11-18 | `src/BuildingBlocks/Kmic.BuildingBlocks.Application/Integration/IntegrationMessageCatalog.cs` | CanShowPayload | 部分 | 新增方法的说明 |
| 2026-11-18 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Dead/GetDeadMessageQuery.cs`、`Messages/DeadMessageDetailItem.cs` | 类说明的 remarks、Payload 参数 | 部分 | 删去“保护仍待补齐”，改写为实际行为 |
| 2026-11-18 | `src/Modules/Files/Kmic.Files.Application/ContractQueries/GetReadableFileQuery.cs` | 类说明的 remarks | 部分 | 补回可读状态范围 |
| 2026-11-18 | `src/Modules/Notifications/Kmic.Notifications.Domain/Errors/NotificationsErrors.cs` | DeliverySecretNotRetryable（取代 DeliverySecretExpired） | 部分 | 新错误码与提示 |
| 2026-11-18 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Retry/RetryDeliveryCommandHandler.cs` | 整份 | 整份完成 | 重写 |
| 2026-11-18 | `src/Modules/Notifications/Kmic.Notifications.Domain/Deliveries/IDeliveryRepository.cs` | RetryAsync | 部分 | 去掉创建时间条件的说明 |
| 2026-11-18 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Sending/DeliveryWork.cs` | ProcessAsync 新增的参数检查 | 部分 | 新注释与失败原因 |
| 2026-11-18 | `docs/modules/notifications/README.md`、`docs/modules/operations/README.md`、`docs/framework/05-infrastructure/05-07-模块间通信.md`、`docs/implementation/S3-平台模块.md`、`docs/implementation/开发交接.md` | CN-01、CN-05 相关的行与修订记录 | 部分 | 其余未检查 |
| 2026-11-18 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Scheduling/SchedulerService.cs` | 整份 | 整份完成 | 停机注释重写 |
| 2026-11-18 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/GetAuditExportFileQueryHandler.cs` | 整份 | 整份完成 | 新增登记等待时间的注释 |
| 2026-11-18 | `src/Modules/Operations/Kmic.Operations.Presentation/Audit/DownloadAuditExportEndpoint.cs` | WithDescription | 部分 | 409 与 404 的说明 |
| 2026-11-18 | `tests/Kmic.Api.Tests/Operations/AuditExportTests.cs`、`tests/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure.Tests/Jobs/JobExecutionTests.cs` | 新增测试与注释 | 部分 | 其余未检查 |
| 2026-11-18 | `docs/modules/operations/README.md`、`docs/implementation/S3-平台模块.md` | 审计导出下载、偶发失败的进展记录 | 部分 | 其余未检查 |
| 2026-11-18 | `src/BuildingBlocks/Kmic.BuildingBlocks.Application/Authorization/DataScopeRules.cs` | 新文件，全部 | 整份完成 | 数据范围规则 |
| 2026-11-18 | `src/Modules/Identity/Kmic.Identity.Application/Authorization/Explain/`（ExplainAuthorizationQuery、Handler、AuthorizationExplanation、DataScopeExplanation、ExplainedOrgUnit） | 新文件，全部 | 整份完成 | 授权诊断 |
| 2026-11-18 | `src/Modules/Identity/Kmic.Identity.Presentation/Authorization/`（ExplainAuthorizationEndpoint、ExplainAuthorizationRequest） | 新文件，全部 | 整份完成 | 授权诊断端点 |
| 2026-11-18 | `src/Modules/Identity/Kmic.Identity.Application/IdentityPermissions.cs` | AuthorizationExplain | 部分 | 新权限点 |
| 2026-11-18 | `tests/Kmic.Api.Tests/Identity/AuthorizationExplainTests.cs` | 新文件，全部 | 整份完成 | 授权诊断测试 |
| 2026-11-18 | `docs/modules/identity/README.md`、`docs/framework/05-infrastructure/05-05-授权.md`、`docs/framework/00-README.md` | 授权诊断的实现记录、修订记录、延后事项一行 | 部分 | 其余未检查 |
| 2026-11-18 | `src/BuildingBlocks/Kmic.BuildingBlocks.Testing/Identity/TestIdentityExtensions.cs` | As、AsSystem | 部分 | 新增方法的说明 |
| 2026-11-18 | `docs/implementation/S2-框架骨架.md`、`docs/framework/05-infrastructure/05-05-授权.md` | S2-05 一行、第 12.1 节 app.As 一句 | 部分 | 其余未检查 |
| 2026-11-18 | `docs/implementation/00-README.md` | T-26 一行 | 部分 | 其余未检查 |
| 2026-11-18 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Caching/MeteredHybridCache.cs` | 新文件 | 整份完成 | 缓存命中统计的包装 |
| 2026-11-18 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Observability/IBacklogGauges.cs` | 新文件 | 整份完成 | 模块量表接口 |
| 2026-11-18 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Observability/BacklogGauge.cs` | 新文件 | 整份完成 | 量表定义 |
| 2026-11-18 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Observability/BacklogReading.cs` | 新文件 | 整份完成 | 量表读数 |
| 2026-11-18 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Sending/NotificationsMetrics.cs` | 新文件 | 整份完成 | 投递结果指标 |
| 2026-11-18 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Sending/DeliveryBacklogGauges.cs` | 新文件 | 整份完成 | 投递积压量表 |
| 2026-11-18 | `src/Modules/Files/Kmic.Files.Application/FilesMetrics.cs` | 新文件 | 整份完成 | 清理跳过指标 |
| 2026-11-18 | `src/Modules/Files/Kmic.Files.Infrastructure/Storage/StorageIssueGauges.cs` | 新文件 | 整份完成 | 待还原文件数量表 |
| 2026-11-18 | `src/BuildingBlocks/Kmic.BuildingBlocks.Application/Observability/FrameworkMetrics.cs` | 类说明的维度一句；CacheRequest、FileUploaded、FileUploadFailed、ReasonOf；三个上传指标与缓存读取指标的说明 | 部分 | 其余未检查 |
| 2026-11-18 | `src/BuildingBlocks/Kmic.BuildingBlocks.Application/Caching/CacheKey.cs` | CategoryOf | 部分 | 新增方法 |
| 2026-11-18 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Caching/CacheServiceCollectionExtensions.cs` | AddKmicCache 的说明、DecorateWithMetrics | 部分 | 其余未检查 |
| 2026-11-18 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Observability/BacklogSampler.cs` | 类说明、SampleModulesAsync、ReadingsOf、ModuleGauges | 部分 | 其余未检查 |
| 2026-11-18 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Observability/ObservabilityServiceCollectionExtensions.cs` | AddKmicBacklogGauges | 部分 | 新增方法 |
| 2026-11-18 | `src/Modules/Files/Kmic.Files.Presentation/Uploads/UploadFileEndpoint.cs` | HandleAsync 中声明大小超限的一处注释 | 部分 | 其余未检查 |
| 2026-11-18 | `tests/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure.Tests/Observability/BacklogSamplerTests.cs` | ModuleGauges_ 两个测试、QueueGauges | 部分 | 其余未检查 |
| 2026-11-18 | `tests/Kmic.Api.Tests/Notifications/DeliveryTests.cs` | 两个测试中新增的指标断言与注释 | 部分 | 其余未检查 |
| 2026-11-18 | `tests/Kmic.ArchitectureTests/Rules/StructureRules.cs` | AuthorizationAdminHandlers、CacheKeys、KeysOf、HandledCommands | 部分 | 其余未检查 |
| 2026-11-18 | `tests/Kmic.ArchitectureTests/Violations/BadSamples.cs` | OrdersCacheKeys、KeyBuildingService、GrantEverythingCommand、RenameRoleSampleCommand 及其处理器 | 部分 | 新增样例 |
| 2026-11-18 | `docs/framework/05-infrastructure/05-12-可观测性.md` | 第 4.1 节维度一格、2026-11-18 修订记录 | 部分 | 其余未检查 |
| 2026-11-18 | `docs/framework/00-README.md` | 延后事项“外部调用待办”一行 | 部分 | 其余未检查 |
| 2026-11-18 | `docs/implementation/00-README.md` | T-28 一行 | 部分 | 其余未检查 |
| 2026-11-18 | `docs/implementation/S2-框架骨架.md` | S2-10、S2-11、S2-12 三行的新增内容 | 部分 | 其余未检查 |
| 2026-11-18 | `docs/implementation/S2-18-规则核对.md` | 15.4 汇总一格、两条规则的机制、第 5 节说明与小结 | 部分 | 其余未检查 |
| 2026-11-18 | `docs/implementation/S3-平台模块.md` | S3-00、S3-02、第 3 节 B 与 C 两行的状态、2026-11-18 进展一行 | 部分 | 其余未检查 |
| 2026-11-18 | `src/Aspire/Kmic.ServiceDefaults/RequestPathRemover.cs` | 新文件，全部 | 整份完成 | 移除日志中的请求实际路径 |
| 2026-11-18 | `src/Aspire/Kmic.ServiceDefaults/ServiceDefaultsExtensions.cs` | `UseKmicRequestLogging` 的 remarks | 部分 | 其余未检查 |
| 2026-11-18 | `tests/Kmic.ServiceDefaults.Tests/RequestLogTests.cs` | 新增与改动的三个测试 | 部分 | 其余未检查 |
| 2026-11-18 | `tests/Kmic.ServiceDefaults.Tests/TestApp.cs` | 新增的日志定义与 `/orders/{id}/note` 端点 | 部分 | 其余未检查 |
| 2026-11-18 | `src/Modules/Files/Kmic.Files.Application/Attachments/FileAttachments.cs` | `AttachAsync` 判断上传人的一行注释 | 部分 | 其余未检查 |
| 2026-11-18 | `tests/Kmic.Api.Tests/Files/SpecimenAttachmentTests.cs` | 新增测试 `AddAttachment_UnderWritableImpersonation_AcceptsAFileUploadedInTheSameSession` | 部分 | 其余未检查 |
| 2026-11-18 | `src/BuildingBlocks/Kmic.BuildingBlocks.Contracts/Jobs/BackgroundJobFinished.cs` | 新文件，全部 | 整份完成 | 后台作业结束事件 |
| 2026-11-18 | `src/BuildingBlocks/Kmic.BuildingBlocks.Contracts/Jobs/BackgroundJobResult.cs` | 新文件，全部 | 整份完成 | 后台作业结果 |
| 2026-11-18 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Jobs/UpdatedJob.cs` | 新文件，全部 | 整份完成 | 修改后的作业行 |
| 2026-11-18 | `src/BuildingBlocks/Kmic.BuildingBlocks.Application/Jobs/BackgroundJobAttribute.cs` | `displayName` 参数与 `DisplayName` | 部分 | 其余未检查 |
| 2026-11-18 | `src/BuildingBlocks/Kmic.BuildingBlocks.Application/Jobs/BackgroundJobCatalog.cs` | `DisplayNameOf`、显示名称校验 | 部分 | 其余未检查 |
| 2026-11-18 | `src/BuildingBlocks/Kmic.BuildingBlocks.Application/Jobs/JobView.cs` | `JobName` 参数说明 | 部分 | 其余未检查 |
| 2026-11-18 | `src/BuildingBlocks/Kmic.BuildingBlocks.Application/Jobs/ReadModelRebuildJob.cs` | 显示名称“重建读模型” | 部分 | 其余未检查 |
| 2026-11-18 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Jobs/JobStore.cs` | `FinishAsync`、`ReleaseAsync`、`RequestCancelAsync`、`UpdateAsync` | 部分 | 其余未检查 |
| 2026-11-18 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Jobs/JobExecutor.cs` | `RequestCancelAsync`、`FinishAsync`、`ReleaseAsync`、`UpdateAsync`、`ResultOf` | 部分 | 其余未检查 |
| 2026-11-18 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Jobs/JobsLog.cs` | 日志 2305 | 部分 | 英文日志消息 |
| 2026-11-18 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Maintenance/PurgeJobsTask.cs` | 类的 remarks | 部分 | 其余未检查 |
| 2026-11-18 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/ExportAuditJob.cs` | 显示名称“审计导出” | 部分 | 其余未检查 |
| 2026-11-18 | `src/Modules/Operations/Kmic.Operations.Application/BackgroundJobs/BackgroundJobItem.cs` | `JobName` 参数说明 | 部分 | 其余未检查 |
| 2026-11-18 | `src/Modules/Notifications/Kmic.Notifications.Application/BackgroundJobs/BackgroundJobFinishedNotice.cs` | 新文件，全部 | 整份完成 | 作业结束通知模板 |
| 2026-11-18 | `src/Modules/Notifications/Kmic.Notifications.Application/BackgroundJobs/NotifyJobFinishedCommand.cs` | 新文件，全部 | 整份完成 | 作业结束通知命令 |
| 2026-11-18 | `src/Modules/Notifications/Kmic.Notifications.Application/BackgroundJobs/NotifyJobFinishedCommandHandler.cs` | 新文件，全部 | 整份完成 | 作业结束通知处理器 |
| 2026-11-18 | `tests/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure.Tests/Jobs/JobExecutionTests.cs` | 结束事件的断言、`Shutdown_CancelRequested_CancelledWithExpiryAndEvent`、`FinishedEventsAsync` | 部分 | 其余未检查 |
| 2026-11-18 | `tests/BuildingBlocks/Kmic.BuildingBlocks.Application.Tests/Jobs/BackgroundJobCatalogTests.cs` | 样例的显示名称、`Build_BlankDisplayName_Rejected` | 部分 | 其余未检查 |
| 2026-11-18 | `tests/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure.Tests/Maintenance/PurgeTests.cs` | `Jobs_ExpiredDeleted_EvenWithResultFile` | 部分 | 其余未检查 |
| 2026-11-18 | `tests/Kmic.Api.Tests/Operations/AuditExportTests.cs` | `SiteMessageWhenSentAsync` 与两处站内消息断言 | 部分 | 其余未检查 |
| 2026-11-18 | `docs/framework/05-infrastructure/05-08-后台处理.md` | 第 6.4、6.8 节相关句、5.7 节清理表一行、示例声明、接口表一行、2026-11-18 修订记录 | 部分 | 其余未检查 |
| 2026-11-18 | `docs/framework/00-README.md` | 待决事项“后台作业结束通知”一行 | 部分 | 其余未检查 |
| 2026-11-18 | `docs/modules/notifications/README.md` | 4.1 用例一行、6.3、8.6、第 15 节 2026-11-18 一段 | 部分 | 其余未检查 |
| 2026-11-18 | `docs/modules/operations/README.md` | 后台作业汇总一段的新增句 | 部分 | 其余未检查 |
| 2026-11-18 | `docs/implementation/开发交接.md` | CN-02、CN-04 的状态行与处理结果 | 部分 | 其余未检查 |
| 2026-11-18 | `docs/implementation/S3-平台模块.md` | 进展记录“后台作业结束通知与 Codex 复核的两项 P2”一行 | 部分 | 其余未检查 |
| 2026-11-18 | `docs/implementation/S2-框架骨架.md` | S2-08 第三部分“未做”里的完成通知一句 | 部分 | 其余未检查 |
| 2026-11-18 | `tests/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure.Tests/Background/FakeWork.cs` | `DelayAfterFailure` 与两处时间记录 | 部分 | 其余未检查 |
| 2026-11-18 | `tests/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure.Tests/Background/LeasedQueueRunnerTests.cs` | `WhenTheDatabaseIsUnavailable_ItBacksOffAndPicksUpAgain` 的注释与断言 | 部分 | 其余未检查 |
| 2026-11-19 | `src/BuildingBlocks/Kmic.BuildingBlocks.Testing/Database/ClassDatabase.cs` | 类说明、`Shared<T>`、`DisposeAsync` 的释放顺序注释 | 部分 | 其余未检查 |
| 2026-11-19 | `tests/Kmic.Api.Tests/ApiFactory.cs` | 类说明（去掉“Identity 模块还没有”的过时说法）、`Tokens`、匿名限流放宽的注释 | 部分 | 其余未检查 |
| 2026-11-19 | `tests/Kmic.Api.Tests/ApiTestBase.cs` | `InitializeAsync` 的注释 | 部分 | 其余未检查 |
| 2026-11-19 | `tests/Kmic.Api.Tests/Identity/IdentityApiTestBase.cs` | `InitializeAsync` 的注释 | 部分 | 其余未检查 |
| 2026-11-19 | `docs/implementation/S3-平台模块.md` | 进展记录“Api 测试改为每个测试类一个宿主”一行 | 部分 | 其余未检查 |
| 2026-11-19 | `tests/BuildingBlocks/Kmic.BuildingBlocks.Testing.Tests/Database/ClassDatabaseTests.cs` | `Shared_IsCreatedOncePerType_AndDisposedBeforeTheCloneIsDropped`、`SharedProbe` | 部分 | 其余未检查 |
| 2026-11-19 | `docs/implementation/换机器继续开发.md` | 第 3 节完整检查的耗时 | 部分 | 其余未检查 |
| 2026-11-19 | `scripts/check.cs` | 文件头注释、格式与依赖漏洞两个步骤、步骤用时与总用时的输出、`WhenProjectsExist`、`CheckVulnerabilities`、`LastPassedScan`、`LockFilesFingerprint`、`Items`、`Capture` | 部分 | 其余未检查 |
| 2026-11-19 | `docs/framework/adr/0021-工程结构与质量门禁.md` | 第 4 节“格式”“依赖漏洞”两行、修订记录 2026-11-19 一行 | 部分 | 其余未检查 |
| 2026-11-19 | `docs/framework/06-coding-standards.md` | 第 16 节“格式与编码”一行、修订记录 2026-11-19 一行 | 部分 | 其余未检查 |
| 2026-11-19 | `docs/implementation/S3-平台模块.md` | 进展记录“检查脚本提速”一行 | 部分 | 其余未检查 |
| 2026-11-19 | `docs/implementation/00-README.md` | 待办 T-29 一行 | 部分 | 其余未检查 |
| 2026-11-19 | `docs/framework/06-coding-standards.md` | 编码速查第 26 条、A.3 的 `summary` 与 `remarks` 写法和“其他”两条、修订记录 2026-11-19 第二行 | 部分 | 其余未检查 |
| 2026-11-19 | `CLAUDE.md` | 开发规则第 4 条新增的注释写法一句 | 部分 | 其余未检查 |
| 2026-11-19 | `docs/implementation/00-README.md` | 已完成待办 T-05 的说明 | 部分 | 其余未检查 |
| 2026-11-19 | `.dockerignore` | 新增 `.local/` 及其注释 | 整份完成 | 只有一行注释 |
| 2026-11-19 | `src/Aspire/Kmic.ServiceDefaults/ServiceDefaultsExtensions.cs` | `UseKmicRequestLogging` 的 summary 与 remarks | 部分 | 合并后复核：实际路径已统一移除，删掉 User-Agent 长度、健康检查级别等细节，开关已在 `ObservabilityOptions` 说明 |
| 2026-11-19 | `src/Aspire/Kmic.ServiceDefaults/RequestPathRemover.cs` | 类说明 | 整份完成 | summary 一句说清，删掉作用域继承的实现细节 |
| 2026-11-19 | `src/BuildingBlocks/Kmic.BuildingBlocks.Application/Caching/CacheKey.cs` | `CategoryOf` 的 remarks | 部分 | 改成调用方要遵守的约束 |
| 2026-11-19 | `src/BuildingBlocks/Kmic.BuildingBlocks.Application/Integration/IntegrationMessageCatalog.cs` | `CanShowPayload` | 部分 | remarks 合成一句，删掉无信息的参数说明 |
| 2026-11-19 | `src/BuildingBlocks/Kmic.BuildingBlocks.Application/Jobs/BackgroundJobCatalog.cs` | `DisplayNameOf` | 部分 | 并进 summary，删掉多版本取名的细节 |
| 2026-11-19 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Jobs/JobExecutor.cs` | `UpdateAsync` 上方注释 | 部分 | 合成一句 |
| 2026-11-19 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Jobs/JobRunResult.cs` | `OutcomeUnknown` | 整份完成 | 合并后过时：写最终结果和放回队列时提交结果未知也返回它 |
| 2026-11-19 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Jobs/JobStore.cs` | `ReleaseAsync` 的 summary、`UpdateAsync` 上方注释 | 部分 | 理顺语句 |
| 2026-11-19 | `src/BuildingBlocks/Kmic.BuildingBlocks.Application/Observability/FrameworkMetrics.cs` | `CacheRequest` 的 `hit` 参数 | 部分 | 去掉双重否定 |
| 2026-11-19 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Caching/CacheServiceCollectionExtensions.cs` | `DecorateWithMetrics` 上方注释 | 部分 | 理顺语句 |
| 2026-11-19 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Observability/BacklogSampler.cs` | 类说明 | 部分 | summary 理顺，remarks 去掉与 `SampleModulesAsync` 重复的一句 |
| 2026-11-19 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Scheduling/SchedulerService.cs` | `StopAsync` 注释 | 部分 | 两行合成一句，先说做法再说原因 |
| 2026-11-19 | `src/BuildingBlocks/Kmic.BuildingBlocks.Testing/Database/ClassDatabase.cs` | `Shared` | 部分 | 按新原则：remarks 只留容易用错的一条 |
| 2026-11-19 | `src/Modules/Identity/Kmic.Identity.Application/IdentityPermissions.cs` | `AuthorizationExplain` 的 summary | 部分 | 改成短语 |
| 2026-11-19 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Sending/DeliveryWork.cs` | `ProcessAsync` 中含密短信的注释 | 部分 | 改成“做法 + 目的” |
| 2026-11-19 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Deliveries/RetryDeliveryEndpoint.cs` | 接口说明 | 整份完成 | 合并后过时：含密通知现在一律不能重试，不再按有效期判断 |
| 2026-11-19 | `src/BuildingBlocks/Kmic.BuildingBlocks.Application/Authorization/DataScopeRules.cs` | 类说明、`AnchorsOf`、`OrgUnitsOf` | 整份完成 | remarks 并进 summary |
| 2026-11-19 | `src/BuildingBlocks/Kmic.BuildingBlocks.Contracts/Jobs/BackgroundJobFinished.cs` | 类说明 | 整份完成 | remarks 只留订阅方要知道的一句 |
| 2026-11-19 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Caching/MeteredHybridCache.cs` | 类说明 | 整份完成 | 规则并进 summary，remarks 只留容易误解的一条 |
| 2026-11-19 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Observability/IBacklogGauges.cs` | 接口说明、`ReadAsync` | 整份完成 | remarks 只留实现方要遵守的约束，删掉“取消令牌”参数说明 |
| 2026-11-19 | `src/Modules/Files/Kmic.Files.Application/FilesMetrics.cs` | 构造函数 | 整份完成 | 删掉“创建指标”套话 |
| 2026-11-19 | `src/Modules/Files/Kmic.Files.Infrastructure/Storage/StorageIssueGauges.cs` | 量表说明 | 整份完成 | 理顺语句 |
| 2026-11-19 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Sending/DeliveryBacklogGauges.cs` | 类说明 | 整份完成 | 统计口径并进 summary，remarks 只留容易误读的一条 |
| 2026-11-19 | `src/Modules/Identity/Kmic.Identity.Application/Authorization/Explain/DataScopeExplanation.cs` | 字段说明 | 整份完成 | 布尔字段写成“是否……”，去掉“授予上设置的”这类绕口说法 |
| 2026-11-19 | `src/Modules/Identity/Kmic.Identity.Application/Authorization/Explain/ExplainAuthorizationQuery.cs` | 类说明 | 整份完成 | summary 改短 |
| 2026-11-19 | `src/Modules/Notifications/Kmic.Notifications.Application/BackgroundJobs/NotifyJobFinishedCommand.cs` | 类说明 | 整份完成 | 业务规则并进 summary，删掉 `[SystemOnly]` 和收件箱已经表达的内容 |
| 2026-11-19 | `BacklogGauge.cs`、`BacklogReading.cs`、`UpdatedJob.cs`、`BackgroundJobResult.cs`、`AuthorizationExplanation.cs`、`ExplainAuthorizationQueryHandler.cs`、`ExplainedOrgUnit.cs`、`ExplainAuthorizationEndpoint.cs`、`ExplainAuthorizationRequest.cs`、`BackgroundJobFinishedNotice.cs`、`NotifyJobFinishedCommandHandler.cs`、`NotificationsMetrics.cs` | 全部 | 整份完成 | 合并后新增的文件，按新原则核对过，无需修改 |
| 2026-11-19 | `tests/Kmic.Api.Tests/ApiFactory.cs` | 类说明、限流设置的注释 | 部分 | 理顺语句，去掉句末句号 |
| 2026-11-19 | `tests/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure.Tests/Jobs/JobExecutionTests.cs` | 超时上限的注释 | 部分 | 改成“做法 + 目的” |
| 2026-11-19 | `tests/Kmic.Api.Tests/Identity/AuthorizationExplainTests.cs` | 类说明 | 部分 | 测试类只写前提，删掉“授权诊断，端到端” |
| 2026-11-19 | `scripts/check.cs` | 顶部说明、步骤名、`CheckWording`、`CheckCommentStyle`、`HasInlineSummaryTag` 及其注释，`Run`、`CheckEncoding`、`ListFiles` 里的注释 | 部分 | 恢复两道检查；句号不再检查；去掉一句话注释末尾的句号 |
| 2026-11-19 | `scripts/new-module.cs` | 顶部说明最后一行 | 部分 | 去掉句末句号 |
| 2026-11-19 | `src/Modules/Files/Kmic.Files.Infrastructure/Deletion/FilesDeletionStore.cs` | 类的 remarks | 部分 | “租约到期”改为“领取过期” |
| 2026-11-19 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Sending/DeliveryStore.cs` | 类的 remarks | 部分 | “租约已过期”改为“领取已过期” |
| 2026-11-19 | `src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Scheduling/ScheduledJobControl.cs` | `TriggerAsync` 注释、`IsBusyAsync` | 部分 | CN-03 新写 |
| 2026-11-19 | `src/BuildingBlocks/Kmic.BuildingBlocks.Application/Scheduling/ScheduledJobTriggerOutcome.cs`、`IScheduledJobControl.cs` | `AlreadyRunning`、`TriggerAsync` 的说明 | 部分 | 拒绝条件补上“已在排队” |
| 2026-11-19 | `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/RunNow/RunJobNowCommand.cs`、`Kmic.Operations.Domain/Errors/OperationsErrors.cs`、`Kmic.Operations.Presentation/ScheduledJobs/RunJobNowEndpoint.cs` | remarks、`JobAlreadyRunning` 的说明与提示、接口说明 | 部分 | 同上 |
| 2026-11-19 | `tests/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure.Tests/Scheduling/ScheduledJobControlTests.cs` | 新增两个测试及辅助方法的注释 | 部分 | 新写 |
| 2026-11-19 | `docs/framework/06-coding-standards.md` | 第 10.3 节规则表与“引用”“工具边界”、A.3 格式、A.5、编码速查第 26 条、修订记录 | 部分 | 恢复检查后的写法；句号改为“一句话时尽量不加，不强制” |
| 2026-11-19 | `docs/framework/00-README.md` | 待决事项、延后事项新增一行、修订记录 | 部分 | 其余未检查 |
| 2026-11-19 | `docs/framework/05-infrastructure/05-05-授权.md`、`05-07-模块间通信.md`、`05-08-后台处理.md`、`05-10-配置与缓存.md` | 修订记录新增行；05-10 第 8.1 节“后台入口的后果” | 部分 | 其余未检查 |
| 2026-11-19 | `docs/modules/operations/README.md` | `RunJobNowCommand` 行、错误表一行、“立即执行”一段 | 部分 | 其余未检查 |
| 2026-11-19 | `docs/modules/identity/README.md`、`docs/modules/notifications/README.md` | “已实现，待用户确认”改为已确认 | 部分 | 其余未检查 |
| 2026-11-19 | `docs/implementation/S3-平台模块.md` | 进展记录新增一行、Codex 的 36 行并成一行 | 部分 | 其余未检查 |
| 2026-11-19 | `docs/implementation/00-README.md` | 清单表“协作交接”一行 | 部分 | 其余未检查 |
| 2026-11-19 | `docs/implementation/协作交接记录.md` | 末尾说明一句 | 部分 | Codex 的记录已删除 |
| 2026-11-19 | `src/Modules/Identity/Kmic.Identity.Application/`：Authentication/AuthTokens、Authentication/GetMe/GetMeQuery、Authentication/ILoginThrottleStore、Authentication/Login/LoginCommandHandler、Authentication/LoginThrottleOptions、Authentication/LoginThrottlePolicy、Authentication/MySessions/ListMySessionsQuery、Authentication/PasswordOptions、Authentication/Refresh/RefreshTokenCommand、Authentication/Refresh/RevokeReusedSessionCommand、Authentication/SessionLifetimeOptions、Authentication/TemporaryPasswordGenerator、Authentication/TrustedOriginsOptions、Authorization/AuthorizationAdminCommandHandler、Authorization/AuthorizationChangeEvents、Authorization/IOperatorRecheck、Catalog/IPermissionCatalogStore、Catalog/PurgeRetiredPermissionsCommand、Catalog/SyncPermissionCatalogCommand、ContractQueries/GetOrgUnitReferencesQueryHandler、ContractQueries/GetRoleReferencesQueryHandler、ContractQueries/GetUserReferencesQueryHandler、IIdentityCommandReads、IIdentityQueryDb、IIdentityReads、IdentityPermissions、Impersonation/End/EndImpersonationCommand、Organization/AssignUser/AssignUserOrgUnitCommand、Organization/Disable/DisableOrgUnitCommand、Organization/Get/GetOrgTreeQuery、Organization/Move/MoveOrgUnitCommand、Roles/RoleTarget、Roles/SetPermissions/SetRolePermissionsCommand、SecurityEvents/ListSecurityEventsQuery、Users/Disable/DisableUserCommand、Users/EventHandlers/RevokeSessionsWhenPasswordChanged、Users/EventHandlers/RevokeSessionsWhenUserDisabled、Users/ForceLogout/ForceLogoutCommand、Users/Grants/GrantPolicy、Users/Grants/SetAnchors/SetGrantAnchorsCommand、Users/Options/SearchUserOptionsQuery、Users/ResetPassword/ResetUserPasswordCommand、Users/UpdateProfile/UpdateUserProfileCommand、Users/UserPolicy、Users/UserTarget | 类型与成员的 summary、remarks | 部分 | 注释清理第一批：remarks 并进 summary、精简成一句或删除；只看了 remarks 和过长的 summary，其余注释未检查 |
| 2026-11-19 | `src/Modules/Identity/Kmic.Identity.Contracts/`：IOrgUnitReferences、IRoleReferences、IUserContacts、IUserReferences、IdentityModuleInfo | 类型与成员的 summary、remarks | 部分 | 注释清理第一批：remarks 并进 summary、精简成一句或删除；只看了 remarks 和过长的 summary，其余注释未检查 |
| 2026-11-19 | `src/Modules/Identity/Kmic.Identity.Domain/`：Organization/OrgTree、Organization/OrgTreeId、Organization/OrgUnit、Roles/Role、Roles/RoleScopeLevel、Sessions/ISessionRepository、Sessions/RefreshToken、Sessions/Session、Users/IUserRepository、Users/MobileNumbers、Users/ProtectedHolders、Users/User、Users/UserRoleGrant | 类型与成员的 summary、remarks | 部分 | 注释清理第一批：remarks 并进 summary、精简成一句或删除；只看了 remarks 和过长的 summary，其余注释未检查 |
| 2026-11-19 | `src/Modules/Identity/Kmic.Identity.Infrastructure/`：Access/AccessContextLoader、Access/RoleSnapshot、IdentityModule、Operations/IdentityBootstrapper | 类型与成员的 summary、remarks | 部分 | 注释清理第一批：remarks 并进 summary、精简成一句或删除；只看了 remarks 和过长的 summary，其余注释未检查 |
| 2026-11-19 | `src/Modules/Identity/Kmic.Identity.Presentation/`：Authentication/CookieEndpointGuard、Authentication/RefreshCookie | 类型与成员的 summary、remarks | 部分 | 注释清理第一批：remarks 并进 summary、精简成一句或删除；只看了 remarks 和过长的 summary，其余注释未检查 |
| 2026-11-19 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Migrations/20261119064723_IdentityCommentCleanup.cs` 及 Designer、快照 | 生成的迁移 | 部分 | `User.Account`、`User.Mobile`、`Role.CatalogFingerprint` 的列注释随 summary 更新，SQL 只有 COMMENT ON |
| 2026-11-19 | `src/Modules/Operations/Kmic.Operations.Application/`：Audit/Export/ExportAuditCommand、Audit/Export/ExportAuditJob、Audit/Export/ExportAuditJobHandler、Audit/List/ListAuditQuery、Audit/Record/RecordAuditCommand、BackgroundJobs/Cancel/CancelBackgroundJobCommand、BackgroundJobs/Cancel/CancelBackgroundJobCommandHandler、BackgroundJobs/List/ListBackgroundJobsQuery、BackgroundJobs/Mine/CancelMyJobCommand、BackgroundJobs/Mine/CancelMyJobCommandHandler、BackgroundJobs/Mine/ListMyJobsQuery、BackgroundJobs/Mine/MyJobs、ClientErrors/ClientErrorOptions、ClientErrors/ClientErrorReport、ClientErrors/ReportClientErrorsCommand、IOperationsQueryDb、MergedCursorPaging、Messages/Abandon/AbandonMessagesCommand、Messages/Abandon/AbandonMessagesCommandHandler、Messages/Blocked/ListBlockedKeysQuery、Messages/Dead/ListDeadMessagesQuery、Messages/Instances/ListInstancesQuery、Messages/Rebuilds/TriggerRebuildCommandHandler、Messages/Retry/RetryMessagesCommand、Messages/Retry/RetryMessagesCommandHandler、Messages/Routes/FinalizeRouteCommandHandler、Messages/Routes/ListRoutesQuery、Messages/Routes/MessageRouteRow、Messages/Routes/ReactivateRouteCommand、Messages/Routes/RedeliverCommand、Messages/Routes/RedeliverCommandHandler、Messages/Summary/GetMessageSummaryQuery、OperationsPermissions、ScheduledJobs/List/ListScheduledJobsQuery、ScheduledJobs/Preview/PreviewCronQuery、ScheduledJobs/Restore/RestoreJobDefaultCommand、ScheduledJobs/RunNow/RunJobNowCommand、ScheduledJobs/RunNow/RunJobNowCommandHandler、ScheduledJobs/Runs/ListScheduledJobRunsQuery、ScheduledJobs/ScheduledJobPolicy、ScheduledJobs/ScheduledJobRunRow | 类型与成员的 summary、remarks | 部分 | 注释清理第二批：remarks 并进 summary、精简成一句或删除；只看了 remarks 和过长的 summary，其余注释未检查 |
| 2026-11-19 | `src/Modules/Operations/Kmic.Operations.Contracts/`：OperationsModuleInfo | 类型与成员的 summary、remarks | 部分 | 注释清理第二批：remarks 并进 summary、精简成一句或删除；只看了 remarks 和过长的 summary，其余注释未检查 |
| 2026-11-19 | `src/Modules/Operations/Kmic.Operations.Domain/`：Audit/AuditLog、Jobs/BackgroundJobStatus、Jobs/ScheduledJobSetting、Messages/MessageBoxKind、Messages/MessageRouteStatus、Messages/RebuildRunStatus | 类型与成员的 summary、remarks | 部分 | 注释清理第二批：remarks 并进 summary、精简成一句或删除；只看了 remarks 和过长的 summary，其余注释未检查 |
| 2026-11-19 | `src/Modules/Operations/Kmic.Operations.Infrastructure/`：ClientErrors/ClientErrorLimiter、ClientErrors/ClientErrorLog、OperationsModule | 类型与成员的 summary、remarks | 部分 | 注释清理第二批：remarks 并进 summary、精简成一句或删除；只看了 remarks 和过长的 summary，其余注释未检查 |
