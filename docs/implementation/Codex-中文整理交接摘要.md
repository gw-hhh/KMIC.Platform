# 中文整理交接摘要

更新时间：2026年11月15日 10:45:16（UTC+8）。初建 C2431；本轮源码复盘 C2471～C2517。精确原文、位置及依据见[修改记录](修改记录.md)。

## 当前提交授权（2026年11月15日 10:56:06，UTC+8）

C2518：先将本页列出的 119 源码、6 个既有模型文件及 7 份跟踪文档本地提交一次，不推送；原有 7 个 BuildingBlocks/测试改动不提交。提交实际哈希随后在开发交接与协作记录顶部登记。之后规范和检查脚本调整不再提交；下方此前“本地未提交”段落为本次提交前状态，历史范围保留。

## 当前结论与 Git 状态

- 本轮允许范围 1267 文件的中文提取、分区筛查及候选调用链复盘已完成：37 个源码文件新增 47 项文字修订 C2471～C2517，1142 个含中文文件保留、88 个无中文；上版推送后累计 119 个源码文件、199 项文字修订，另有 6 个既有模型相关文件。
- 覆盖六模块、宿主、Aspire、开发脚本及模块模板，共 1174 个 C# 文件、93 个工程配套文件。完整提取 2017 组含中文 XML 注释，结合接口说明、局部注释、错误、特性及实际调用链复盘；其他准确文字结合当前同版本既有核对保留，路径清单不作为质量豁免。
- 分支 `codex-cn-copy-cleanup-2026-11-14`，HEAD `e2f5ee9c3cadcdaf5820446f6fafdee7e40e6dd1`。上一版 e2f5ee9 已推送，之后四轮文字成果仍在本地，未合并 main。
- 当前 134 个已跟踪修改、5 个原有未跟踪文件，暂存区为空。本轮无新增文件；四个未跟踪迁移/设计文件及本摘要完整保留。
- 上版之后记录 C2312～C2517 共 206 项：199 项源码文字操作、6 项既有 EF 文件记录、1 项交接文档 C2431。C2430 补正 C2388；C2490 补正 C2484 的检查方向，因此操作数不等于最终差异条数。
- 本轮“完成”指允许范围的中文复盘完成，不表示整站功能验收、测试通过或数据库更新。

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

## 当前未提交的 119 个源码文件

下表汇总上版推送后四轮的全部源码文字变化。精确原文、现文、UTC+8 时间和实现核对依据见 C 编号；更早已提交的内容保留在修改记录历史中。

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

## 已生成但未提交的 6 个模型相关文件

这些是前轮已授权生成的结果，本交接轮未修改。两份迁移仅调整三条列注释：Configuration.SystemSettingChange.TraceId；Operations.ScheduledJobSetting.Enabled、Cron。旧迁移保留，不手改旧迁移、设计文件或快照。

| 文件（相对仓库根目录） | 状态 | 记录 |
|---|---|---|
| [src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/Migrations/20261114165501_SimplifySettingChangeTraceComment.Designer.cs](../../src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/Migrations/20261114165501_SimplifySettingChangeTraceComment.Designer.cs) | 已生成，未跟踪 | C2349 |
| [src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/Migrations/20261114165501_SimplifySettingChangeTraceComment.cs](../../src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/Migrations/20261114165501_SimplifySettingChangeTraceComment.cs) | 已生成，未跟踪 | C2350 |
| [src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/Migrations/ConfigurationDbContextModelSnapshot.cs](../../src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/Migrations/ConfigurationDbContextModelSnapshot.cs) | 当前快照，已跟踪修改 | C2351 |
| [src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/Migrations/20261114170053_ClarifyJobSettingNullComments.Designer.cs](../../src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/Migrations/20261114170053_ClarifyJobSettingNullComments.Designer.cs) | 已生成，未跟踪 | C2358 |
| [src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/Migrations/20261114170053_ClarifyJobSettingNullComments.cs](../../src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/Migrations/20261114170053_ClarifyJobSettingNullComments.cs) | 已生成，未跟踪 | C2359 |
| [src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/Migrations/OperationsDbContextModelSnapshot.cs](../../src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/Migrations/OperationsDbContextModelSnapshot.cs) | 当前快照，已跟踪修改 | C2360 |

## 验证结果和未运行项目

- 本轮：47 条精确替换回放一致；37 个修改文件除 10 处中文字符串外，可执行代码标记完全一致，49 个属性摘要和 HasComment 调用未变；全范围 2336 组 XML 注释语法有效，1174 个 C# 文件中的 336 组参数标签完整；业务逻辑、实体属性摘要、列说明、迁移和快照未修改。
- 覆盖与保护：1267 文件内容 SHA 与逐文件结论一致；本轮范围外 2661 个既有文件逐文件 SHA-256 不变，包含原有 BuildingBlocks/测试改动、前轮模型成果及全部旧迁移；结束 134 个已跟踪修改、5 个原有未跟踪文件，分支/HEAD 不变、暂存区为空；Codex 路径 999 条无重复，ClaudeCode 清单保持 0 字节。
- 先前几轮文字修订的精确回放与静态核对已完成。三条模型注释仅在此前授权轮调整：Configuration.TraceId、Operations.Enabled/Cron；当时两模块独立编译 0 警告/错误，模型与快照一致，正反向 SQL 各 3 条 COMMENT，无结构、索引、约束或业务数据变化。本轮未重跑或覆盖该结果。
- 未编译产品项目、未运行测试、OpenAPI 生成或全套项目脚本，未执行数据库连接、SQL 或 database update；仅使用既有临时工具做静态核对；未暂存、提交或推送。临时 Roslyn 工具采用 --no-build，不编译产品项目；静态核对不等于测试通过。
- 原始 API 既有 IDE0005（Files.Application/ContractQueries/GetReadableFileQuery.cs:4 未使用 using）未修复，不宣称整站构建通过；早期临时编译输入的说明仍在开发交接历史中。
- 最终 git diff --check、交接链接及文件映射结果见最新协作记录。

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
- 检查脚本的机械词表/格式要求、BuildingBlocks 旧术语、既有 API IDE0005 均另列；不在本轮擅自修复。
- 短信登录、短信网关、登录人机验证、病毒扫描等延后事项，继续按框架及实施待办处理；不因出现接口或占位实现就宣称已完成。
- 数据库尚未应用任何本轮迁移；实际部署库注释与迁移历史未经核对，不能把离线验证记为数据库更新完成。

## Claude Code 如何接手

1. 按顺序读根目录 [CLAUDE.md](../../CLAUDE.md)、[框架入口](../framework/00-README.md)、[S3 进度](S3-平台模块.md)、本摘要、[开发交接](开发交接.md)、[协作记录](协作交接记录.md)、[修改记录](修改记录.md)。完整覆盖和保留结论查[全面复核清单](Codex-中文源码全面复核清单.md)。
2. 重新核对实际分支、HEAD、git status、暂存区和 diff；文档是交接索引，实际差异优先。旧段落的“待改”“整份排除已处理路径”属于历史阶段，当前标准以本文及开发交接顶部为准。双方已处理与未提交文件允许复审，但修改前仍需核对来源，不能覆盖他人改动。
3. [Codex 路径清单](Codex-已按中文规范处理的文件.md)仅记录来源；[ClaudeCode 清单](ClaudeCode-已按中文规范处理的文件.md)按用户要求保持空文件，不据此认定合格。当前均无需再机械清空或重复整理。
4. 如有新发现，先核对调用链、事务、状态与失败分支，再做最小文字修改；逐处记录并同步实际文件表、路径清单、开发交接、协作记录和 S3。不改业务逻辑、测试、web/、BuildingBlocks、旧迁移及快照；必要模型调整另核实授权。
5. 本轮不暂存、不提交、不推送。上一版推送授权已履行，不自动延续到当前本地成果。接手时若有新的明确用户授权，以最新指令为准。

最新实际记录为 C2517，下一编号 C2518；始终以修改记录中的真实编号为准，不按历史正文提及的编号推断。
