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
