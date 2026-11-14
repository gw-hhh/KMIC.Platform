# 中文源码全面复核清单

- 更新时间：2026年11月15日 00:04:56（UTC+8）
- 当前分支：`codex-cn-copy-cleanup-2026-11-14`；基线：`bca15d72fff863db1b1e2f76d864c3ffce59b6e8`
- 范围：1267 个源码及工程配套文件；已记录 1267 个文件的结论。
- 清单用于证明原 1267 文件的中文复核覆盖范围，不代表业务功能验收。本轮另按用户确认实施五模块模型说明、注释迁移与当前快照；新增的 10 个迁移/设计文件在修改记录登记，不计入原 1267 文件基数。测试、web/、BuildingBlocks、旧迁移和业务逻辑未改。
- 原全面复核阶段未编译测试；本轮模型中文例外已执行必要独立编译、模型及离线 SQL 核对。API 基线既有 IDE0005 另列；测试、OpenAPI、全套脚本与数据库操作未运行；未暂存提交推送。

- [模型中文修订方案](Codex-模型中文修订方案.md)的 M001～M037 已实施：C2259～C2310；17 源码文件、五份新迁移及设计文件、五份当前快照，模型及 SQL 仅注释变化。以下结论已同步本轮实际结果。

## 范围限制内保留的待办

- 全部原范围文件均有复核结论；这不代表所有原有中文均已验收。已确认的 37 项模型说明已落实，其他范围外和检查规则事项继续保留；业务行为未改。
- `StoredFile.Profile/UploadedBy/UploadedAt` 等已按实际赋值改为上传配置名称、上传人或生成请求人、文件登记时间，并以新迁移同步列说明（C2259～C2269）。
- `Session.DeviceName/RefreshExpiresAt` 已据原始 User-Agent 与模拟会话到期赋值修订，并以新迁移同步列说明（C2275～C2276）。
- `scripts/check.cs`：用词正则仍机械禁止“租约”等技术词，与按上下文判断的标准可能冲突；更改正则会改变检查行为，本轮只记录，未运行或修改检查规则。
- 37 项之外的既有模型/框架事项及 CN-01/02/03 等行为待办继续保留，见[开发交接](开发交接.md)。本轮完成不代表这些行为问题已修复。

| 序号 | 文件 | 结论 | 核对说明 | 时间（UTC+8） |
|---|---|---|---|---|
| 1 | `.dockerignore` | 保留 | 已对照配置项或调用实现核对中文。工程规则、依赖及发布设置未改变；无需改写的说明保留。 | 2026年11月14日 22:17:41 |
| 2 | `.editorconfig` | 保留 | 已对照配置项或调用实现核对中文。工程规则、依赖及发布设置未改变；无需改写的说明保留。 | 2026年11月14日 22:17:41 |
| 3 | `.gitattributes` | 保留 | 已对照配置项或调用实现核对中文。工程规则、依赖及发布设置未改变；无需改写的说明保留。 | 2026年11月14日 22:17:41 |
| 4 | `.gitea/CODEOWNERS` | 保留 | 已对照配置项或调用实现核对中文。工程规则、依赖及发布设置未改变；无需改写的说明保留。 | 2026年11月14日 22:17:41 |
| 5 | `.github/workflows/ci.yml` | 保留 | 已对照配置项或调用实现核对中文。工程规则、依赖及发布设置未改变；无需改写的说明保留。 | 2026年11月14日 22:17:41 |
| 6 | `.gitignore` | 保留 | 已对照配置项或调用实现核对中文。工程规则、依赖及发布设置未改变；无需改写的说明保留。 | 2026年11月14日 22:17:41 |
| 7 | `.gitleaks.toml` | 保留 | 已对照配置项或调用实现核对中文。工程规则、依赖及发布设置未改变；无需改写的说明保留。 | 2026年11月14日 22:17:41 |
| 8 | `BannedSymbols.Modules.txt` | 已修订 | 已对照配置项或调用实现核对中文。工程规则、依赖及发布设置未改变；无需改写的说明保留。；本阶段 C2201 | 2026年11月14日 22:17:42 |
| 9 | `BannedSymbols.txt` | 保留 | 已对照配置项或调用实现核对中文。工程规则、依赖及发布设置未改变；无需改写的说明保留。 | 2026年11月14日 22:17:42 |
| 10 | `Directory.Build.props` | 保留 | 已对照配置项或调用实现核对中文。工程规则、依赖及发布设置未改变；无需改写的说明保留。 | 2026年11月14日 22:17:42 |
| 11 | `Directory.Build.targets` | 已修订 | 已对照配置项或调用实现核对中文。工程规则、依赖及发布设置未改变；无需改写的说明保留。；本阶段 C2202 | 2026年11月14日 22:17:42 |
| 12 | `Directory.Packages.props` | 保留 | 已对照配置项或调用实现核对中文。工程规则、依赖及发布设置未改变；无需改写的说明保留。 | 2026年11月14日 22:17:42 |
| 13 | `Dockerfile` | 保留 | 已对照配置项或调用实现核对中文。工程规则、依赖及发布设置未改变；无需改写的说明保留。 | 2026年11月14日 22:17:42 |
| 14 | `nuget.config` | 保留 | 已对照配置项或调用实现核对中文。工程规则、依赖及发布设置未改变；无需改写的说明保留。 | 2026年11月14日 22:17:43 |
| 15 | `scripts/check.cs` | 保留（规则另列） | 核对配套脚本的命令顺序、失败退出、凭据保存位置及模板查询契约：中文用途和提示清楚，保留；仅静态阅读，没有执行脚本、编译、测试、生成迁移或访问数据库。；另列待办：用词正则仍机械禁止“租约”等技术词，与按上下文判断的标准可能冲突；更改正则会改变检查行为，本轮只记录，未运行或修改检查规则。 | 2026年11月14日 22:40:49 |
| 16 | `scripts/dev.cs` | 保留 | 核对配套脚本的命令顺序、失败退出、凭据保存位置及模板查询契约：中文用途和提示清楚，保留；仅静态阅读，没有执行脚本、编译、测试、生成迁移或访问数据库。 | 2026年11月14日 22:40:49 |
| 17 | `scripts/new-module.cs` | 保留 | 核对配套脚本的命令顺序、失败退出、凭据保存位置及模板查询契约：中文用途和提示清楚，保留；仅静态阅读，没有执行脚本、编译、测试、生成迁移或访问数据库。 | 2026年11月14日 22:40:49 |
| 18 | `src/Aspire/Kmic.AppHost/Kmic.AppHost.csproj` | 保留 | 核对入口装配、属性脱敏实现和命令定义：保留采样、500 日志、敏感输入来源、退出码及维护权限等调用约定；常规标题与短字段说明保留。 | 2026年11月14日 22:22:54 |
| 19 | `src/Aspire/Kmic.AppHost/Program.cs` | 保留 | 核对入口装配、属性脱敏实现和命令定义：保留采样、500 日志、敏感输入来源、退出码及维护权限等调用约定；常规标题与短字段说明保留。 | 2026年11月14日 22:22:55 |
| 20 | `src/Aspire/Kmic.AppHost/init/01-roles.sql` | 保留 | 已对照配置项或调用实现核对中文。工程规则、依赖及发布设置未改变；无需改写的说明保留。 | 2026年11月14日 22:17:43 |
| 21 | `src/Aspire/Kmic.AppHost/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 22 | `src/Aspire/Kmic.ServiceDefaults/ClassifiedDestructuringPolicy.cs` | 已修订 | 核对入口装配、属性脱敏实现和命令定义：保留采样、500 日志、敏感输入来源、退出码及维护权限等调用约定；常规标题与短字段说明保留。；本阶段 C2203 | 2026年11月14日 22:22:55 |
| 23 | `src/Aspire/Kmic.ServiceDefaults/Kmic.ServiceDefaults.csproj` | 保留 | 核对入口装配、属性脱敏实现和命令定义：保留采样、500 日志、敏感输入来源、退出码及维护权限等调用约定；常规标题与短字段说明保留。 | 2026年11月14日 22:22:55 |
| 24 | `src/Aspire/Kmic.ServiceDefaults/ObservabilityOptions.cs` | 保留 | 核对入口装配、属性脱敏实现和命令定义：保留采样、500 日志、敏感输入来源、退出码及维护权限等调用约定；常规标题与短字段说明保留。 | 2026年11月14日 22:22:55 |
| 25 | `src/Aspire/Kmic.ServiceDefaults/RedactionExtensions.cs` | 保留 | 核对入口装配、属性脱敏实现和命令定义：保留采样、500 日志、敏感输入来源、退出码及维护权限等调用约定；常规标题与短字段说明保留。 | 2026年11月14日 22:22:55 |
| 26 | `src/Aspire/Kmic.ServiceDefaults/SensitivePropertyMasker.cs` | 保留 | 核对入口装配、属性脱敏实现和命令定义：保留采样、500 日志、敏感输入来源、退出码及维护权限等调用约定；常规标题与短字段说明保留。 | 2026年11月14日 22:22:55 |
| 27 | `src/Aspire/Kmic.ServiceDefaults/ServiceDefaultsExtensions.cs` | 保留 | 核对入口装配、属性脱敏实现和命令定义：保留采样、500 日志、敏感输入来源、退出码及维护权限等调用约定；常规标题与短字段说明保留。 | 2026年11月14日 22:22:55 |
| 28 | `src/Aspire/Kmic.ServiceDefaults/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 29 | `src/Hosts/Kmic.Api/Kmic.Api.csproj` | 保留 | 核对入口装配、属性脱敏实现和命令定义：保留采样、500 日志、敏感输入来源、退出码及维护权限等调用约定；常规标题与短字段说明保留。 | 2026年11月14日 22:22:55 |
| 30 | `src/Hosts/Kmic.Api/Program.cs` | 保留 | 核对入口装配、属性脱敏实现和命令定义：保留采样、500 日志、敏感输入来源、退出码及维护权限等调用约定；常规标题与短字段说明保留。 | 2026年11月14日 22:22:56 |
| 31 | `src/Hosts/Kmic.Api/Properties/launchSettings.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 32 | `src/Hosts/Kmic.Api/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 33 | `src/Hosts/Kmic.MigrationRunner/Kmic.MigrationRunner.csproj` | 保留 | 核对入口装配、属性脱敏实现和命令定义：保留采样、500 日志、敏感输入来源、退出码及维护权限等调用约定；常规标题与短字段说明保留。 | 2026年11月14日 22:22:56 |
| 34 | `src/Hosts/Kmic.MigrationRunner/NumbersCommands.cs` | 保留 | 核对入口装配、属性脱敏实现和命令定义：保留采样、500 日志、敏感输入来源、退出码及维护权限等调用约定；常规标题与短字段说明保留。 | 2026年11月14日 22:22:56 |
| 35 | `src/Hosts/Kmic.MigrationRunner/Program.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 36 | `src/Hosts/Kmic.MigrationRunner/RunnerApp.cs` | 保留 | 核对入口装配、属性脱敏实现和命令定义：保留采样、500 日志、敏感输入来源、退出码及维护权限等调用约定；常规标题与短字段说明保留。 | 2026年11月14日 22:22:56 |
| 37 | `src/Hosts/Kmic.MigrationRunner/RunnerContext.cs` | 保留 | 核对入口装配、属性脱敏实现和命令定义：保留采样、500 日志、敏感输入来源、退出码及维护权限等调用约定；常规标题与短字段说明保留。 | 2026年11月14日 22:22:56 |
| 38 | `src/Hosts/Kmic.MigrationRunner/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 39 | `src/Hosts/Kmic.Operator/IdentityCommands.cs` | 保留 | 核对入口装配、属性脱敏实现和命令定义：保留采样、500 日志、敏感输入来源、退出码及维护权限等调用约定；常规标题与短字段说明保留。 | 2026年11月14日 22:22:56 |
| 40 | `src/Hosts/Kmic.Operator/Kmic.Operator.csproj` | 保留 | 核对入口装配、属性脱敏实现和命令定义：保留采样、500 日志、敏感输入来源、退出码及维护权限等调用约定；常规标题与短字段说明保留。 | 2026年11月14日 22:22:56 |
| 41 | `src/Hosts/Kmic.Operator/OperatorApp.cs` | 保留 | 核对入口装配、属性脱敏实现和命令定义：保留采样、500 日志、敏感输入来源、退出码及维护权限等调用约定；常规标题与短字段说明保留。 | 2026年11月14日 22:22:56 |
| 42 | `src/Hosts/Kmic.Operator/OperatorContext.cs` | 保留 | 核对入口装配、属性脱敏实现和命令定义：保留采样、500 日志、敏感输入来源、退出码及维护权限等调用约定；常规标题与短字段说明保留。 | 2026年11月14日 22:22:56 |
| 43 | `src/Hosts/Kmic.Operator/Program.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 44 | `src/Hosts/Kmic.Operator/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 45 | `src/Modules/Configuration/Kmic.Configuration.Application/ConfigurationCacheKeys.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:02 |
| 46 | `src/Modules/Configuration/Kmic.Configuration.Application/ConfigurationPermissions.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 47 | `src/Modules/Configuration/Kmic.Configuration.Application/ConfigurationScopes.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:03 |
| 48 | `src/Modules/Configuration/Kmic.Configuration.Application/ContractQueries/DictionaryLookup.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:03 |
| 49 | `src/Modules/Configuration/Kmic.Configuration.Application/ContractQueries/GetDictionarySnapshotQuery.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:03 |
| 50 | `src/Modules/Configuration/Kmic.Configuration.Application/ContractQueries/GetDictionarySnapshotQueryHandler.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:03 |
| 51 | `src/Modules/Configuration/Kmic.Configuration.Application/ContractQueries/GetDictionarySnapshotQueryValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 52 | `src/Modules/Configuration/Kmic.Configuration.Application/DeclaredValues.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:03 |
| 53 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Admin/DictionaryTypeDetail.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:03 |
| 54 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Admin/DictionaryTypeListItem.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:03 |
| 55 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Admin/GetDictionaryTypeQuery.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:03 |
| 56 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Admin/GetDictionaryTypeQueryHandler.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:03 |
| 57 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Admin/ListDictionaryTypesQuery.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:03 |
| 58 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Admin/ListDictionaryTypesQueryHandler.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:03 |
| 59 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/DictionaryChanges.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:04 |
| 60 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/DictionaryOptions.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:04 |
| 61 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Get/DictionariesResponse.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:04 |
| 62 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Get/GetDictionariesQuery.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:04 |
| 63 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Get/GetDictionariesQueryHandler.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:04 |
| 64 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Get/GetDictionariesQueryValidator.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:04 |
| 65 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/IDictionarySnapshots.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:04 |
| 66 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Items/AddDictionaryItemCommand.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:04 |
| 67 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Items/AddDictionaryItemCommandHandler.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:04 |
| 68 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Items/AddDictionaryItemCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 69 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Items/DisableDictionaryItemCommand.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:05 |
| 70 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Items/DisableDictionaryItemCommandHandler.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:05 |
| 71 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Items/EnableDictionaryItemCommand.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:05 |
| 72 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Items/EnableDictionaryItemCommandHandler.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:05 |
| 73 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Items/MoveDictionaryItemCommand.cs` | 已修订 | 完成跨文件一致性复查后重新核对本轮最终内容：字典/组织移动仅在父项变化时重排；取消写所属模块；投递上限仅在暂时失败时判断；事件账号与模拟会话时间说明均与实现一致。；本阶段 C2216 | 2026年11月14日 22:41:41 |
| 74 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Items/MoveDictionaryItemCommandHandler.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:05 |
| 75 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Items/RenameDictionaryItemCommand.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:05 |
| 76 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Items/RenameDictionaryItemCommandHandler.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:05 |
| 77 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Items/RenameDictionaryItemCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 78 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Items/ReorderDictionaryItemsCommand.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:06 |
| 79 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Items/ReorderDictionaryItemsCommandHandler.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:06 |
| 80 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Sync/DictionarySyncResult.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 81 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Sync/SyncDictionaryTypesCommand.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:06 |
| 82 | `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Sync/SyncDictionaryTypesCommandHandler.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:06 |
| 83 | `src/Modules/Configuration/Kmic.Configuration.Application/Features/FeaturesResponse.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 84 | `src/Modules/Configuration/Kmic.Configuration.Application/Features/GetFeaturesQuery.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:06 |
| 85 | `src/Modules/Configuration/Kmic.Configuration.Application/Features/GetFeaturesQueryHandler.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:07 |
| 86 | `src/Modules/Configuration/Kmic.Configuration.Application/IConfigurationQueryDb.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:07 |
| 87 | `src/Modules/Configuration/Kmic.Configuration.Application/Kmic.Configuration.Application.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 88 | `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/IPreferenceLayers.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:07 |
| 89 | `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/LayeredPreferences.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:07 |
| 90 | `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Layers/ListPreferenceDefinitionsQuery.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:07 |
| 91 | `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Layers/ListPreferenceDefinitionsQueryHandler.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:08 |
| 92 | `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Layers/PreferenceDefinitionItem.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:08 |
| 93 | `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Layers/PreferenceDefinitionsResponse.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:08 |
| 94 | `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Layers/PreferenceLayerTarget.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:08 |
| 95 | `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Layers/PreferenceLayerValue.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:08 |
| 96 | `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Layers/ResetPreferenceLayerCommand.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:09 |
| 97 | `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Layers/ResetPreferenceLayerCommandHandler.cs` | 保留 | 核对字典查询排序/计数、变更处理器及分层偏好投影：保留启停状态、并发版本、父子/重排约定、缓存提交后失效及缺省层级规则；现有用途和属性说明清楚，不机械添加 see/参数标签。 | 2026年11月14日 22:26:09 |
| 98 | `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Layers/SetPreferenceLayerCommand.cs` | 保留 | 核对个人/管理偏好写入、覆盖与回退，以及设置查询和历史投影：保留模拟身份、版本不匹配、允许层级和历史值空值语义；精简生硬措辞，未改值解析、校验或查询。 | 2026年11月14日 22:26:22 |
| 99 | `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Layers/SetPreferenceLayerCommandHandler.cs` | 保留 | 核对个人/管理偏好写入、覆盖与回退，以及设置查询和历史投影：保留模拟身份、版本不匹配、允许层级和历史值空值语义；精简生硬措辞，未改值解析、校验或查询。 | 2026年11月14日 22:26:22 |
| 100 | `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Layers/SetPreferenceLayerCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 101 | `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Mine/GetMyPreferencesQuery.cs` | 保留 | 核对个人/管理偏好写入、覆盖与回退，以及设置查询和历史投影：保留模拟身份、版本不匹配、允许层级和历史值空值语义；精简生硬措辞，未改值解析、校验或查询。 | 2026年11月14日 22:26:22 |
| 102 | `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Mine/GetMyPreferencesQueryHandler.cs` | 保留 | 核对个人/管理偏好写入、覆盖与回退，以及设置查询和历史投影：保留模拟身份、版本不匹配、允许层级和历史值空值语义；精简生硬措辞，未改值解析、校验或查询。 | 2026年11月14日 22:26:22 |
| 103 | `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Mine/MyPreferenceItem.cs` | 保留 | 核对个人/管理偏好写入、覆盖与回退，以及设置查询和历史投影：保留模拟身份、版本不匹配、允许层级和历史值空值语义；精简生硬措辞，未改值解析、校验或查询。 | 2026年11月14日 22:26:23 |
| 104 | `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Mine/MyPreferencesResponse.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 105 | `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Mine/ResetMyPreferenceCommand.cs` | 保留 | 核对个人/管理偏好写入、覆盖与回退，以及设置查询和历史投影：保留模拟身份、版本不匹配、允许层级和历史值空值语义；精简生硬措辞，未改值解析、校验或查询。 | 2026年11月14日 22:26:23 |
| 106 | `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Mine/ResetMyPreferenceCommandHandler.cs` | 保留 | 核对个人/管理偏好写入、覆盖与回退，以及设置查询和历史投影：保留模拟身份、版本不匹配、允许层级和历史值空值语义；精简生硬措辞，未改值解析、校验或查询。 | 2026年11月14日 22:26:23 |
| 107 | `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Mine/SetMyPreferenceCommand.cs` | 保留 | 核对个人/管理偏好写入、覆盖与回退，以及设置查询和历史投影：保留模拟身份、版本不匹配、允许层级和历史值空值语义；精简生硬措辞，未改值解析、校验或查询。 | 2026年11月14日 22:26:23 |
| 108 | `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Mine/SetMyPreferenceCommandHandler.cs` | 保留 | 核对个人/管理偏好写入、覆盖与回退，以及设置查询和历史投影：保留模拟身份、版本不匹配、允许层级和历史值空值语义；精简生硬措辞，未改值解析、校验或查询。 | 2026年11月14日 22:26:23 |
| 109 | `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Mine/SetMyPreferenceCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 110 | `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/PreferenceOptions.cs` | 保留 | 核对个人/管理偏好写入、覆盖与回退，以及设置查询和历史投影：保留模拟身份、版本不匹配、允许层级和历史值空值语义；精简生硬措辞，未改值解析、校验或查询。 | 2026年11月14日 22:26:23 |
| 111 | `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/PreferenceSource.cs` | 保留 | 核对个人/管理偏好写入、覆盖与回退，以及设置查询和历史投影：保留模拟身份、版本不匹配、允许层级和历史值空值语义；精简生硬措辞，未改值解析、校验或查询。 | 2026年11月14日 22:26:23 |
| 112 | `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/PreferenceSubject.cs` | 保留 | 核对个人/管理偏好写入、覆盖与回退，以及设置查询和历史投影：保留模拟身份、版本不匹配、允许层级和历史值空值语义；精简生硬措辞，未改值解析、校验或查询。 | 2026年11月14日 22:26:23 |
| 113 | `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/PreferencesReader.cs` | 保留 | 核对个人/管理偏好写入、覆盖与回退，以及设置查询和历史投影：保留模拟身份、版本不匹配、允许层级和历史值空值语义；精简生硬措辞，未改值解析、校验或查询。 | 2026年11月14日 22:26:23 |
| 114 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/Changes/ListSettingChangesQuery.cs` | 保留 | 核对个人/管理偏好写入、覆盖与回退，以及设置查询和历史投影：保留模拟身份、版本不匹配、允许层级和历史值空值语义；精简生硬措辞，未改值解析、校验或查询。 | 2026年11月14日 22:26:24 |
| 115 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/Changes/ListSettingChangesQueryHandler.cs` | 保留 | 核对个人/管理偏好写入、覆盖与回退，以及设置查询和历史投影：保留模拟身份、版本不匹配、允许层级和历史值空值语义；精简生硬措辞，未改值解析、校验或查询。 | 2026年11月14日 22:26:24 |
| 116 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/Changes/SettingChangeItem.cs` | 已修订 | 核对个人/管理偏好写入、覆盖与回退，以及设置查询和历史投影：保留模拟身份、版本不匹配、允许层级和历史值空值语义；精简生硬措辞，未改值解析、校验或查询。；本阶段 C2209 | 2026年11月14日 22:26:24 |
| 117 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/Changes/SettingChangeRow.cs` | 保留 | 核对个人/管理偏好写入、覆盖与回退，以及设置查询和历史投影：保留模拟身份、版本不匹配、允许层级和历史值空值语义；精简生硬措辞，未改值解析、校验或查询。 | 2026年11月14日 22:26:24 |
| 118 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/Changes/SettingChangeSort.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 119 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/Get/GetSettingQuery.cs` | 保留 | 核对个人/管理偏好写入、覆盖与回退，以及设置查询和历史投影：保留模拟身份、版本不匹配、允许层级和历史值空值语义；精简生硬措辞，未改值解析、校验或查询。 | 2026年11月14日 22:26:24 |
| 120 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/Get/GetSettingQueryHandler.cs` | 保留 | 核对个人/管理偏好写入、覆盖与回退，以及设置查询和历史投影：保留模拟身份、版本不匹配、允许层级和历史值空值语义；精简生硬措辞，未改值解析、校验或查询。 | 2026年11月14日 22:26:24 |
| 121 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/Get/ListSettingsQuery.cs` | 保留 | 核对个人/管理偏好写入、覆盖与回退，以及设置查询和历史投影：保留模拟身份、版本不匹配、允许层级和历史值空值语义；精简生硬措辞，未改值解析、校验或查询。 | 2026年11月14日 22:26:24 |
| 122 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/Get/ListSettingsQueryHandler.cs` | 保留 | 核对个人/管理偏好写入、覆盖与回退，以及设置查询和历史投影：保留模拟身份、版本不匹配、允许层级和历史值空值语义；精简生硬措辞，未改值解析、校验或查询。 | 2026年11月14日 22:26:24 |
| 123 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/Get/ObsoleteSettingItem.cs` | 保留 | 核对个人/管理偏好写入、覆盖与回退，以及设置查询和历史投影：保留模拟身份、版本不匹配、允许层级和历史值空值语义；精简生硬措辞，未改值解析、校验或查询。 | 2026年11月14日 22:26:25 |
| 124 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/Get/SettingItem.cs` | 保留 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。 | 2026年11月14日 22:27:01 |
| 125 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/Get/SettingItems.cs` | 保留 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。 | 2026年11月14日 22:27:01 |
| 126 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/Get/SettingsResponse.cs` | 保留 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。 | 2026年11月14日 22:27:01 |
| 127 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/Purge/PurgeObsoleteSettingCommand.cs` | 保留 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。 | 2026年11月14日 22:27:01 |
| 128 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/Purge/PurgeObsoleteSettingCommandHandler.cs` | 保留 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。 | 2026年11月14日 22:27:01 |
| 129 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/Purge/PurgeObsoleteSettingCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 130 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/Reset/ResetSettingCommand.cs` | 保留 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。 | 2026年11月14日 22:27:01 |
| 131 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/Reset/ResetSettingCommandHandler.cs` | 保留 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。 | 2026年11月14日 22:27:02 |
| 132 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/Reset/ResetSettingCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 133 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/Set/SetSettingCommand.cs` | 保留 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。 | 2026年11月14日 22:27:02 |
| 134 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/Set/SetSettingCommandHandler.cs` | 保留 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。 | 2026年11月14日 22:27:02 |
| 135 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/Set/SetSettingCommandValidator.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 136 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/SettingAction.cs` | 保留 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。 | 2026年11月14日 22:27:02 |
| 137 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/SettingActions.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 138 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/SettingFieldInfo.cs` | 保留 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。 | 2026年11月14日 22:27:02 |
| 139 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/SettingFields.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 140 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/SettingOptions.cs` | 保留 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。 | 2026年11月14日 22:27:03 |
| 141 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/SettingPolicy.cs` | 保留 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。 | 2026年11月14日 22:27:03 |
| 142 | `src/Modules/Configuration/Kmic.Configuration.Application/Settings/SettingTarget.cs` | 保留 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。 | 2026年11月14日 22:27:03 |
| 143 | `src/Modules/Configuration/Kmic.Configuration.Application/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 144 | `src/Modules/Configuration/Kmic.Configuration.Contracts/ConfigurationModuleInfo.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 145 | `src/Modules/Configuration/Kmic.Configuration.Contracts/DictionaryEntry.cs` | 保留 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。 | 2026年11月14日 22:27:03 |
| 146 | `src/Modules/Configuration/Kmic.Configuration.Contracts/DictionarySnapshot.cs` | 保留 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。 | 2026年11月14日 22:27:03 |
| 147 | `src/Modules/Configuration/Kmic.Configuration.Contracts/IDictionaries.cs` | 保留 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。 | 2026年11月14日 22:27:03 |
| 148 | `src/Modules/Configuration/Kmic.Configuration.Contracts/Kmic.Configuration.Contracts.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 149 | `src/Modules/Configuration/Kmic.Configuration.Contracts/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 150 | `src/Modules/Configuration/Kmic.Configuration.Domain/Dictionaries/DictionaryItem.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 151 | `src/Modules/Configuration/Kmic.Configuration.Domain/Dictionaries/DictionaryItemId.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 152 | `src/Modules/Configuration/Kmic.Configuration.Domain/Dictionaries/DictionaryItemStatus.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 153 | `src/Modules/Configuration/Kmic.Configuration.Domain/Dictionaries/DictionaryType.cs` | 已修订 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。；本阶段 C2210,C2211 | 2026年11月14日 22:27:04 |
| 154 | `src/Modules/Configuration/Kmic.Configuration.Domain/Dictionaries/DictionaryTypeId.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 155 | `src/Modules/Configuration/Kmic.Configuration.Domain/Dictionaries/DictionaryTypeStatus.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 156 | `src/Modules/Configuration/Kmic.Configuration.Domain/Dictionaries/IDictionaryTypeRepository.cs` | 保留 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。 | 2026年11月14日 22:27:04 |
| 157 | `src/Modules/Configuration/Kmic.Configuration.Domain/Errors/ConfigurationErrors.cs` | 保留 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。 | 2026年11月14日 22:27:04 |
| 158 | `src/Modules/Configuration/Kmic.Configuration.Domain/Kmic.Configuration.Domain.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 159 | `src/Modules/Configuration/Kmic.Configuration.Domain/Preferences/IPreferenceRepository.cs` | 保留 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。 | 2026年11月14日 22:27:04 |
| 160 | `src/Modules/Configuration/Kmic.Configuration.Domain/Preferences/PreferenceLayer.cs` | 保留 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。 | 2026年11月14日 22:27:04 |
| 161 | `src/Modules/Configuration/Kmic.Configuration.Domain/Preferences/PreferenceValue.cs` | 已修订 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。；本阶段 C2212 | 2026年11月14日 22:27:05 |
| 162 | `src/Modules/Configuration/Kmic.Configuration.Domain/Preferences/PreferenceValueId.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 163 | `src/Modules/Configuration/Kmic.Configuration.Domain/Settings/ISystemSettingRepository.cs` | 保留 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。 | 2026年11月14日 22:27:05 |
| 164 | `src/Modules/Configuration/Kmic.Configuration.Domain/Settings/SettingChangeId.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 165 | `src/Modules/Configuration/Kmic.Configuration.Domain/Settings/SystemSettingChange.cs` | 已修订（模型中文） | 本轮 C2277～C2278；先核对实际行为，再精简或纠正摘要/模型说明。已同步注释迁移与快照，运行模型及 SQL 仅注释变化；测试未运行 | 2026年11月15日 00:04:56 |
| 166 | `src/Modules/Configuration/Kmic.Configuration.Domain/Settings/SystemSettingValue.cs` | 已修订 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。；本阶段 C2213 | 2026年11月14日 22:27:05 |
| 167 | `src/Modules/Configuration/Kmic.Configuration.Domain/Settings/SystemSettingValueId.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 168 | `src/Modules/Configuration/Kmic.Configuration.Domain/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 169 | `src/Modules/Configuration/Kmic.Configuration.Infrastructure/ConfigurationModule.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 170 | `src/Modules/Configuration/Kmic.Configuration.Infrastructure/Dictionaries/DictionarySnapshotReader.cs` | 保留 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。 | 2026年11月14日 22:27:05 |
| 171 | `src/Modules/Configuration/Kmic.Configuration.Infrastructure/Dictionaries/SyncDictionaryTypesTask.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 172 | `src/Modules/Configuration/Kmic.Configuration.Infrastructure/Kmic.Configuration.Infrastructure.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 173 | `src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/ConfigurationDbContext.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 174 | `src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/ConfigurationDbContextFactory.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 175 | `src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/ConfigurationIdConverters.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 176 | `src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/ConfigurationModel.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 177 | `src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/ConfigurationQueryDbContext.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 178 | `src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/Configurations/DictionaryItemConfiguration.cs` | 沿用已复核 | 沿用上轮逐文件结论（模型说明保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 179 | `src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/Configurations/DictionaryTypeConfiguration.cs` | 沿用已复核 | 沿用上轮逐文件结论（模型说明保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 180 | `src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/Configurations/PreferenceValueConfiguration.cs` | 已修订（模型中文） | 本轮 C2279；先核对实际行为，再精简或纠正摘要/模型说明。已同步注释迁移与快照，运行模型及 SQL 仅注释变化；测试未运行 | 2026年11月15日 00:04:56 |
| 181 | `src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/Configurations/SystemSettingChangeConfiguration.cs` | 沿用已复核 | 沿用上轮逐文件结论（模型说明保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 182 | `src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/Configurations/SystemSettingValueConfiguration.cs` | 沿用已复核 | 沿用上轮逐文件结论（模型说明保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 183 | `src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/Repositories/DictionaryTypeRepository.cs` | 沿用已复核 | 上轮已完整读取关联文件并核实修改，本轮内容未变；原编号 C2150。 | 2026年11月14日 22:12:28 |
| 184 | `src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/Repositories/PreferenceRepository.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 185 | `src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/Repositories/SystemSettingRepository.cs` | 保留 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。 | 2026年11月14日 22:27:05 |
| 186 | `src/Modules/Configuration/Kmic.Configuration.Infrastructure/Preferences/PreferenceLayerReader.cs` | 已修订 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。；本阶段 C2214 | 2026年11月14日 22:27:06 |
| 187 | `src/Modules/Configuration/Kmic.Configuration.Infrastructure/Settings/SettingsReader.cs` | 已修订 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。；本阶段 C2215 | 2026年11月14日 22:27:06 |
| 188 | `src/Modules/Configuration/Kmic.Configuration.Infrastructure/Settings/SettingsSnapshot.cs` | 保留 | 逐文件核对系统设置写入/重置/清除、字典聚合及偏好缓存：保留调用方校验、空版本要求、追加历史和无效缓存回退条件；精简普通用途及缓存原因。快照和模型注释只读核对，未修改持久化文案。 | 2026年11月14日 22:27:06 |
| 189 | `src/Modules/Configuration/Kmic.Configuration.Infrastructure/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 190 | `src/Modules/Configuration/Kmic.Configuration.Presentation/ConfigurationEndpoints.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 191 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Dictionaries/AddDictionaryItemEndpoint.cs` | 保留 | 核对字典和偏好端点的命令映射、筛选和响应：保留版本、幂等键、历史项可见性与分层回退说明；修正移动字典项对非层级与未变父项的过度概括。 | 2026年11月14日 22:27:27 |
| 192 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Dictionaries/AddDictionaryItemRequest.cs` | 保留 | 核对字典和偏好端点的命令映射、筛选和响应：保留版本、幂等键、历史项可见性与分层回退说明；修正移动字典项对非层级与未变父项的过度概括。 | 2026年11月14日 22:27:27 |
| 193 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Dictionaries/DictionaryItemVersionRequest.cs` | 保留 | 核对字典和偏好端点的命令映射、筛选和响应：保留版本、幂等键、历史项可见性与分层回退说明；修正移动字典项对非层级与未变父项的过度概括。 | 2026年11月14日 22:27:27 |
| 194 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Dictionaries/DisableDictionaryItemEndpoint.cs` | 保留 | 核对字典和偏好端点的命令映射、筛选和响应：保留版本、幂等键、历史项可见性与分层回退说明；修正移动字典项对非层级与未变父项的过度概括。 | 2026年11月14日 22:27:28 |
| 195 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Dictionaries/EnableDictionaryItemEndpoint.cs` | 保留 | 核对字典和偏好端点的命令映射、筛选和响应：保留版本、幂等键、历史项可见性与分层回退说明；修正移动字典项对非层级与未变父项的过度概括。 | 2026年11月14日 22:27:28 |
| 196 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Dictionaries/GetDictionariesEndpoint.cs` | 保留 | 核对字典和偏好端点的命令映射、筛选和响应：保留版本、幂等键、历史项可见性与分层回退说明；修正移动字典项对非层级与未变父项的过度概括。 | 2026年11月14日 22:27:28 |
| 197 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Dictionaries/GetDictionaryTypeEndpoint.cs` | 保留 | 核对字典和偏好端点的命令映射、筛选和响应：保留版本、幂等键、历史项可见性与分层回退说明；修正移动字典项对非层级与未变父项的过度概括。 | 2026年11月14日 22:27:28 |
| 198 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Dictionaries/ListDictionaryTypesEndpoint.cs` | 保留 | 核对字典和偏好端点的命令映射、筛选和响应：保留版本、幂等键、历史项可见性与分层回退说明；修正移动字典项对非层级与未变父项的过度概括。 | 2026年11月14日 22:27:28 |
| 199 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Dictionaries/MoveDictionaryItemEndpoint.cs` | 已修订 | 核对字典和偏好端点的命令映射、筛选和响应：保留版本、幂等键、历史项可见性与分层回退说明；修正移动字典项对非层级与未变父项的过度概括。；本阶段 C2217 | 2026年11月14日 22:27:28 |
| 200 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Dictionaries/MoveDictionaryItemRequest.cs` | 保留 | 核对字典和偏好端点的命令映射、筛选和响应：保留版本、幂等键、历史项可见性与分层回退说明；修正移动字典项对非层级与未变父项的过度概括。 | 2026年11月14日 22:27:28 |
| 201 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Dictionaries/RenameDictionaryItemEndpoint.cs` | 保留 | 核对字典和偏好端点的命令映射、筛选和响应：保留版本、幂等键、历史项可见性与分层回退说明；修正移动字典项对非层级与未变父项的过度概括。 | 2026年11月14日 22:27:28 |
| 202 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Dictionaries/RenameDictionaryItemRequest.cs` | 保留 | 核对字典和偏好端点的命令映射、筛选和响应：保留版本、幂等键、历史项可见性与分层回退说明；修正移动字典项对非层级与未变父项的过度概括。 | 2026年11月14日 22:27:28 |
| 203 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Dictionaries/ReorderDictionaryItemsEndpoint.cs` | 保留 | 核对字典和偏好端点的命令映射、筛选和响应：保留版本、幂等键、历史项可见性与分层回退说明；修正移动字典项对非层级与未变父项的过度概括。 | 2026年11月14日 22:27:28 |
| 204 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Dictionaries/ReorderDictionaryItemsRequest.cs` | 保留 | 核对字典和偏好端点的命令映射、筛选和响应：保留版本、幂等键、历史项可见性与分层回退说明；修正移动字典项对非层级与未变父项的过度概括。 | 2026年11月14日 22:27:29 |
| 205 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Features/GetFeaturesEndpoint.cs` | 保留 | 核对字典和偏好端点的命令映射、筛选和响应：保留版本、幂等键、历史项可见性与分层回退说明；修正移动字典项对非层级与未变父项的过度概括。 | 2026年11月14日 22:27:29 |
| 206 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Kmic.Configuration.Presentation.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 207 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Preferences/GetMyPreferencesEndpoint.cs` | 保留 | 核对字典和偏好端点的命令映射、筛选和响应：保留版本、幂等键、历史项可见性与分层回退说明；修正移动字典项对非层级与未变父项的过度概括。 | 2026年11月14日 22:27:29 |
| 208 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Preferences/ListPreferenceDefinitionsEndpoint.cs` | 保留 | 核对字典和偏好端点的命令映射、筛选和响应：保留版本、幂等键、历史项可见性与分层回退说明；修正移动字典项对非层级与未变父项的过度概括。 | 2026年11月14日 22:27:29 |
| 209 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Preferences/ResetMyPreferenceEndpoint.cs` | 保留 | 核对字典和偏好端点的命令映射、筛选和响应：保留版本、幂等键、历史项可见性与分层回退说明；修正移动字典项对非层级与未变父项的过度概括。 | 2026年11月14日 22:27:29 |
| 210 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Preferences/ResetPreferenceLayerRequest.cs` | 保留 | 核对字典和偏好端点的命令映射、筛选和响应：保留版本、幂等键、历史项可见性与分层回退说明；修正移动字典项对非层级与未变父项的过度概括。 | 2026年11月14日 22:27:29 |
| 211 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Preferences/ResetRolePreferenceEndpoint.cs` | 保留 | 核对字典和偏好端点的命令映射、筛选和响应：保留版本、幂等键、历史项可见性与分层回退说明；修正移动字典项对非层级与未变父项的过度概括。 | 2026年11月14日 22:27:29 |
| 212 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Preferences/ResetSystemPreferenceEndpoint.cs` | 保留 | 核对各偏好和系统设置端点实际命令及请求校验：说明用途、权限与值的回退/删除行为清楚，保留幂等键、版本、秘密不可存储等约定。 | 2026年11月14日 22:27:40 |
| 213 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Preferences/SetMyPreferenceEndpoint.cs` | 保留 | 核对各偏好和系统设置端点实际命令及请求校验：说明用途、权限与值的回退/删除行为清楚，保留幂等键、版本、秘密不可存储等约定。 | 2026年11月14日 22:27:41 |
| 214 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Preferences/SetMyPreferenceRequest.cs` | 保留 | 核对各偏好和系统设置端点实际命令及请求校验：说明用途、权限与值的回退/删除行为清楚，保留幂等键、版本、秘密不可存储等约定。 | 2026年11月14日 22:27:41 |
| 215 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Preferences/SetPreferenceLayerRequest.cs` | 保留 | 核对各偏好和系统设置端点实际命令及请求校验：说明用途、权限与值的回退/删除行为清楚，保留幂等键、版本、秘密不可存储等约定。 | 2026年11月14日 22:27:41 |
| 216 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Preferences/SetRolePreferenceEndpoint.cs` | 保留 | 核对各偏好和系统设置端点实际命令及请求校验：说明用途、权限与值的回退/删除行为清楚，保留幂等键、版本、秘密不可存储等约定。 | 2026年11月14日 22:27:41 |
| 217 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Preferences/SetSystemPreferenceEndpoint.cs` | 保留 | 核对各偏好和系统设置端点实际命令及请求校验：说明用途、权限与值的回退/删除行为清楚，保留幂等键、版本、秘密不可存储等约定。 | 2026年11月14日 22:27:41 |
| 218 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Settings/GetSettingEndpoint.cs` | 保留 | 核对各偏好和系统设置端点实际命令及请求校验：说明用途、权限与值的回退/删除行为清楚，保留幂等键、版本、秘密不可存储等约定。 | 2026年11月14日 22:27:41 |
| 219 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Settings/ListSettingChangesEndpoint.cs` | 保留 | 核对各偏好和系统设置端点实际命令及请求校验：说明用途、权限与值的回退/删除行为清楚，保留幂等键、版本、秘密不可存储等约定。 | 2026年11月14日 22:27:41 |
| 220 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Settings/ListSettingChangesRequest.cs` | 保留 | 核对各偏好和系统设置端点实际命令及请求校验：说明用途、权限与值的回退/删除行为清楚，保留幂等键、版本、秘密不可存储等约定。 | 2026年11月14日 22:27:41 |
| 221 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Settings/ListSettingsEndpoint.cs` | 保留 | 核对各偏好和系统设置端点实际命令及请求校验：说明用途、权限与值的回退/删除行为清楚，保留幂等键、版本、秘密不可存储等约定。 | 2026年11月14日 22:27:41 |
| 222 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Settings/PurgeObsoleteSettingEndpoint.cs` | 保留 | 核对各偏好和系统设置端点实际命令及请求校验：说明用途、权限与值的回退/删除行为清楚，保留幂等键、版本、秘密不可存储等约定。 | 2026年11月14日 22:27:42 |
| 223 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Settings/ResetSettingEndpoint.cs` | 保留 | 核对各偏好和系统设置端点实际命令及请求校验：说明用途、权限与值的回退/删除行为清楚，保留幂等键、版本、秘密不可存储等约定。 | 2026年11月14日 22:27:42 |
| 224 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Settings/SetSettingEndpoint.cs` | 保留 | 核对各偏好和系统设置端点实际命令及请求校验：说明用途、权限与值的回退/删除行为清楚，保留幂等键、版本、秘密不可存储等约定。 | 2026年11月14日 22:27:42 |
| 225 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Settings/SetSettingRequest.cs` | 保留 | 核对各偏好和系统设置端点实际命令及请求校验：说明用途、权限与值的回退/删除行为清楚，保留幂等键、版本、秘密不可存储等约定。 | 2026年11月14日 22:27:42 |
| 226 | `src/Modules/Configuration/Kmic.Configuration.Presentation/Settings/SettingReasonRequest.cs` | 保留 | 核对各偏好和系统设置端点实际命令及请求校验：说明用途、权限与值的回退/删除行为清楚，保留幂等键、版本、秘密不可存储等约定。 | 2026年11月14日 22:27:42 |
| 227 | `src/Modules/Configuration/Kmic.Configuration.Presentation/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 228 | `src/Modules/Files/Kmic.Files.Application/Administration/FileDetail.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:09 |
| 229 | `src/Modules/Files/Kmic.Files.Application/Administration/FileListItem.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:09 |
| 230 | `src/Modules/Files/Kmic.Files.Application/Administration/FileReferenceItem.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:09 |
| 231 | `src/Modules/Files/Kmic.Files.Application/Administration/FileSort.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 232 | `src/Modules/Files/Kmic.Files.Application/Administration/GetFileQuery.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:09 |
| 233 | `src/Modules/Files/Kmic.Files.Application/Administration/GetFileQueryHandler.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:09 |
| 234 | `src/Modules/Files/Kmic.Files.Application/Administration/ListFilesQuery.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:09 |
| 235 | `src/Modules/Files/Kmic.Files.Application/Administration/ListFilesQueryHandler.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:09 |
| 236 | `src/Modules/Files/Kmic.Files.Application/Administration/ListStorageIssuesQuery.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:09 |
| 237 | `src/Modules/Files/Kmic.Files.Application/Administration/ListStorageIssuesQueryHandler.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:10 |
| 238 | `src/Modules/Files/Kmic.Files.Application/Administration/PurgeFileCommand.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:10 |
| 239 | `src/Modules/Files/Kmic.Files.Application/Administration/PurgeFileCommandHandler.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:10 |
| 240 | `src/Modules/Files/Kmic.Files.Application/Administration/PurgeFileCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 241 | `src/Modules/Files/Kmic.Files.Application/Administration/RecordHistoryPurgeCommand.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:10 |
| 242 | `src/Modules/Files/Kmic.Files.Application/Administration/RecordHistoryPurgeCommandHandler.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:10 |
| 243 | `src/Modules/Files/Kmic.Files.Application/Administration/RecordHistoryPurgeCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 244 | `src/Modules/Files/Kmic.Files.Application/Administration/ResolveStorageIssueCommand.cs` | 沿用已复核 | 上轮已完整读取关联文件并核实修改，本轮内容未变；原编号 C2122。 | 2026年11月14日 22:12:28 |
| 245 | `src/Modules/Files/Kmic.Files.Application/Administration/ResolveStorageIssueCommandHandler.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:10 |
| 246 | `src/Modules/Files/Kmic.Files.Application/Administration/ResolveStorageIssueCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 247 | `src/Modules/Files/Kmic.Files.Application/Administration/RetryDeletionCommand.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:10 |
| 248 | `src/Modules/Files/Kmic.Files.Application/Administration/RetryDeletionCommandHandler.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:10 |
| 249 | `src/Modules/Files/Kmic.Files.Application/Administration/StorageIssueItem.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:11 |
| 250 | `src/Modules/Files/Kmic.Files.Application/Administration/StorageIssueSort.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 251 | `src/Modules/Files/Kmic.Files.Application/Attachments/AttachFilesCommand.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:11 |
| 252 | `src/Modules/Files/Kmic.Files.Application/Attachments/AttachFilesCommandHandler.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:11 |
| 253 | `src/Modules/Files/Kmic.Files.Application/Attachments/DetachFilesCommand.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:11 |
| 254 | `src/Modules/Files/Kmic.Files.Application/Attachments/DetachFilesCommandHandler.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:11 |
| 255 | `src/Modules/Files/Kmic.Files.Application/Attachments/FileAttachments.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:11 |
| 256 | `src/Modules/Files/Kmic.Files.Application/Attachments/GeneratedFiles.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:11 |
| 257 | `src/Modules/Files/Kmic.Files.Application/Attachments/RegisterGeneratedFileCommand.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:12 |
| 258 | `src/Modules/Files/Kmic.Files.Application/Attachments/RegisterGeneratedFileCommandHandler.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:12 |
| 259 | `src/Modules/Files/Kmic.Files.Application/Cleanup/PurgeExpiredFilesCommand.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:12 |
| 260 | `src/Modules/Files/Kmic.Files.Application/Cleanup/PurgeExpiredFilesCommandHandler.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:12 |
| 261 | `src/Modules/Files/Kmic.Files.Application/Cleanup/ReconcileStorageCommand.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:13 |
| 262 | `src/Modules/Files/Kmic.Files.Application/Cleanup/ReconcileStorageCommandHandler.cs` | 保留 | 核对对账扫描/二次确认、上传三阶段、文件名清理和上传权限实现：保留异常空清单保护、最短存续时间、上传登记与完成区别、无权时 404 及关联事务约定；文件信息快照只读核对。 | 2026年11月14日 22:24:31 |
| 263 | `src/Modules/Files/Kmic.Files.Application/Cleanup/ResetStaleUploadsCommand.cs` | 保留 | 核对对账扫描/二次确认、上传三阶段、文件名清理和上传权限实现：保留异常空清单保护、最短存续时间、上传登记与完成区别、无权时 404 及关联事务约定；文件信息快照只读核对。 | 2026年11月14日 22:24:31 |
| 264 | `src/Modules/Files/Kmic.Files.Application/Cleanup/ResetStaleUploadsCommandHandler.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:13 |
| 265 | `src/Modules/Files/Kmic.Files.Application/ContractQueries/CheckAttachabilityQuery.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:13 |
| 266 | `src/Modules/Files/Kmic.Files.Application/ContractQueries/CheckAttachabilityQueryHandler.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:13 |
| 267 | `src/Modules/Files/Kmic.Files.Application/ContractQueries/FileAttachability.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:13 |
| 268 | `src/Modules/Files/Kmic.Files.Application/ContractQueries/FileContents.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 269 | `src/Modules/Files/Kmic.Files.Application/ContractQueries/FileDownloads.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:13 |
| 270 | `src/Modules/Files/Kmic.Files.Application/ContractQueries/GetFileDownloadQuery.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:14 |
| 271 | `src/Modules/Files/Kmic.Files.Application/ContractQueries/GetFileDownloadQueryHandler.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:14 |
| 272 | `src/Modules/Files/Kmic.Files.Application/ContractQueries/GetReadableFileQuery.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:14 |
| 273 | `src/Modules/Files/Kmic.Files.Application/ContractQueries/GetReadableFileQueryHandler.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:14 |
| 274 | `src/Modules/Files/Kmic.Files.Application/FilesOptions.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:15 |
| 275 | `src/Modules/Files/Kmic.Files.Application/FilesPermissions.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:15 |
| 276 | `src/Modules/Files/Kmic.Files.Application/FilesScopes.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:15 |
| 277 | `src/Modules/Files/Kmic.Files.Application/IFilesQueryDb.cs` | 保留 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。 | 2026年11月14日 22:24:15 |
| 278 | `src/Modules/Files/Kmic.Files.Application/IStorageInventory.cs` | 已修订 | 核对管理查询投影、清理/关联处理器和契约查询：保留权限、版本、互斥、未完成消息探查、下载就绪状态及时间边界；常规标题和字段说明准确，删除无额外信息的参数标签。；本阶段 C2204 | 2026年11月14日 22:24:15 |
| 279 | `src/Modules/Files/Kmic.Files.Application/IStorageKeyFactory.cs` | 保留 | 核对对账扫描/二次确认、上传三阶段、文件名清理和上传权限实现：保留异常空清单保护、最短存续时间、上传登记与完成区别、无权时 404 及关联事务约定；文件信息快照只读核对。 | 2026年11月14日 22:24:31 |
| 280 | `src/Modules/Files/Kmic.Files.Application/InventoryObject.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 281 | `src/Modules/Files/Kmic.Files.Application/Kmic.Files.Application.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 282 | `src/Modules/Files/Kmic.Files.Application/Uploads/Complete/CompleteUploadCommand.cs` | 保留 | 核对对账扫描/二次确认、上传三阶段、文件名清理和上传权限实现：保留异常空清单保护、最短存续时间、上传登记与完成区别、无权时 404 及关联事务约定；文件信息快照只读核对。 | 2026年11月14日 22:24:31 |
| 283 | `src/Modules/Files/Kmic.Files.Application/Uploads/Complete/CompleteUploadCommandHandler.cs` | 保留 | 核对对账扫描/二次确认、上传三阶段、文件名清理和上传权限实现：保留异常空清单保护、最短存续时间、上传登记与完成区别、无权时 404 及关联事务约定；文件信息快照只读核对。 | 2026年11月14日 22:24:31 |
| 284 | `src/Modules/Files/Kmic.Files.Application/Uploads/Complete/CompleteUploadCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 285 | `src/Modules/Files/Kmic.Files.Application/Uploads/Complete/UploadCompleted.cs` | 保留 | 核对对账扫描/二次确认、上传三阶段、文件名清理和上传权限实现：保留异常空清单保护、最短存续时间、上传登记与完成区别、无权时 404 及关联事务约定；文件信息快照只读核对。 | 2026年11月14日 22:24:31 |
| 286 | `src/Modules/Files/Kmic.Files.Application/Uploads/FileNames.cs` | 保留 | 核对对账扫描/二次确认、上传三阶段、文件名清理和上传权限实现：保留异常空清单保护、最短存续时间、上传登记与完成区别、无权时 404 及关联事务约定；文件信息快照只读核对。 | 2026年11月14日 22:24:32 |
| 287 | `src/Modules/Files/Kmic.Files.Application/Uploads/Reject/RejectUploadCommand.cs` | 保留 | 核对对账扫描/二次确认、上传三阶段、文件名清理和上传权限实现：保留异常空清单保护、最短存续时间、上传登记与完成区别、无权时 404 及关联事务约定；文件信息快照只读核对。 | 2026年11月14日 22:24:32 |
| 288 | `src/Modules/Files/Kmic.Files.Application/Uploads/Reject/RejectUploadCommandHandler.cs` | 保留 | 核对对账扫描/二次确认、上传三阶段、文件名清理和上传权限实现：保留异常空清单保护、最短存续时间、上传登记与完成区别、无权时 404 及关联事务约定；文件信息快照只读核对。 | 2026年11月14日 22:24:32 |
| 289 | `src/Modules/Files/Kmic.Files.Application/Uploads/Reject/RejectUploadCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 290 | `src/Modules/Files/Kmic.Files.Application/Uploads/Start/StartUploadCommand.cs` | 保留 | 核对对账扫描/二次确认、上传三阶段、文件名清理和上传权限实现：保留异常空清单保护、最短存续时间、上传登记与完成区别、无权时 404 及关联事务约定；文件信息快照只读核对。 | 2026年11月14日 22:24:32 |
| 291 | `src/Modules/Files/Kmic.Files.Application/Uploads/Start/StartUploadCommandHandler.cs` | 保留 | 核对对账扫描/二次确认、上传三阶段、文件名清理和上传权限实现：保留异常空清单保护、最短存续时间、上传登记与完成区别、无权时 404 及关联事务约定；文件信息快照只读核对。 | 2026年11月14日 22:24:32 |
| 292 | `src/Modules/Files/Kmic.Files.Application/Uploads/Start/StartUploadCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 293 | `src/Modules/Files/Kmic.Files.Application/Uploads/Start/UploadStarted.cs` | 保留 | 核对对账扫描/二次确认、上传三阶段、文件名清理和上传权限实现：保留异常空清单保护、最短存续时间、上传登记与完成区别、无权时 404 及关联事务约定；文件信息快照只读核对。 | 2026年11月14日 22:24:32 |
| 294 | `src/Modules/Files/Kmic.Files.Application/Uploads/UploadAction.cs` | 保留 | 核对对账扫描/二次确认、上传三阶段、文件名清理和上传权限实现：保留异常空清单保护、最短存续时间、上传登记与完成区别、无权时 404 及关联事务约定；文件信息快照只读核对。 | 2026年11月14日 22:24:32 |
| 295 | `src/Modules/Files/Kmic.Files.Application/Uploads/UploadProfilePolicy.cs` | 保留 | 核对对账扫描/二次确认、上传三阶段、文件名清理和上传权限实现：保留异常空清单保护、最短存续时间、上传登记与完成区别、无权时 404 及关联事务约定；文件信息快照只读核对。 | 2026年11月14日 22:24:32 |
| 296 | `src/Modules/Files/Kmic.Files.Application/Uploads/UploadProfileTarget.cs` | 保留 | 核对对账扫描/二次确认、上传三阶段、文件名清理和上传权限实现：保留异常空清单保护、最短存续时间、上传登记与完成区别、无权时 404 及关联事务约定；文件信息快照只读核对。 | 2026年11月14日 22:24:32 |
| 297 | `src/Modules/Files/Kmic.Files.Application/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 298 | `src/Modules/Files/Kmic.Files.Contracts/AttachFiles.cs` | 保留 | 核对对账扫描/二次确认、上传三阶段、文件名清理和上传权限实现：保留异常空清单保护、最短存续时间、上传登记与完成区别、无权时 404 及关联事务约定；文件信息快照只读核对。 | 2026年11月14日 22:24:33 |
| 299 | `src/Modules/Files/Kmic.Files.Contracts/DetachFiles.cs` | 保留 | 核对对账扫描/二次确认、上传三阶段、文件名清理和上传权限实现：保留异常空清单保护、最短存续时间、上传登记与完成区别、无权时 404 及关联事务约定；文件信息快照只读核对。 | 2026年11月14日 22:24:33 |
| 300 | `src/Modules/Files/Kmic.Files.Contracts/FileContent.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 301 | `src/Modules/Files/Kmic.Files.Contracts/FileSnapshot.cs` | 保留 | 核对对账扫描/二次确认、上传三阶段、文件名清理和上传权限实现：保留异常空清单保护、最短存续时间、上传登记与完成区别、无权时 404 及关联事务约定；文件信息快照只读核对。 | 2026年11月14日 22:24:33 |
| 302 | `src/Modules/Files/Kmic.Files.Contracts/FilesModuleInfo.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 303 | `src/Modules/Files/Kmic.Files.Contracts/GeneratedFile.cs` | 保留 | 核对文件契约、仓储约定及 StoredFile 状态机：保留锁/事务、异步登记、流释放、分页和重置删除规则；修正状态说明中的操作者及参数含义。持久化属性注释问题另列，不改列注释。 | 2026年11月14日 22:25:15 |
| 304 | `src/Modules/Files/Kmic.Files.Contracts/IFileAttachability.cs` | 保留 | 核对文件契约、仓储约定及 StoredFile 状态机：保留锁/事务、异步登记、流释放、分页和重置删除规则；修正状态说明中的操作者及参数含义。持久化属性注释问题另列，不改列注释。 | 2026年11月14日 22:25:15 |
| 305 | `src/Modules/Files/Kmic.Files.Contracts/IFileAttachments.cs` | 保留 | 核对文件契约、仓储约定及 StoredFile 状态机：保留锁/事务、异步登记、流释放、分页和重置删除规则；修正状态说明中的操作者及参数含义。持久化属性注释问题另列，不改列注释。 | 2026年11月14日 22:25:15 |
| 306 | `src/Modules/Files/Kmic.Files.Contracts/IFileContents.cs` | 保留 | 核对文件契约、仓储约定及 StoredFile 状态机：保留锁/事务、异步登记、流释放、分页和重置删除规则；修正状态说明中的操作者及参数含义。持久化属性注释问题另列，不改列注释。 | 2026年11月14日 22:25:15 |
| 307 | `src/Modules/Files/Kmic.Files.Contracts/IFileDownloads.cs` | 保留 | 核对文件契约、仓储约定及 StoredFile 状态机：保留锁/事务、异步登记、流释放、分页和重置删除规则；修正状态说明中的操作者及参数含义。持久化属性注释问题另列，不改列注释。 | 2026年11月14日 22:25:16 |
| 308 | `src/Modules/Files/Kmic.Files.Contracts/IGeneratedFiles.cs` | 保留 | 核对文件契约、仓储约定及 StoredFile 状态机：保留锁/事务、异步登记、流释放、分页和重置删除规则；修正状态说明中的操作者及参数含义。持久化属性注释问题另列，不改列注释。 | 2026年11月14日 22:25:16 |
| 309 | `src/Modules/Files/Kmic.Files.Contracts/Kmic.Files.Contracts.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 310 | `src/Modules/Files/Kmic.Files.Contracts/RegisterGeneratedFile.cs` | 保留 | 核对文件契约、仓储约定及 StoredFile 状态机：保留锁/事务、异步登记、流释放、分页和重置删除规则；修正状态说明中的操作者及参数含义。持久化属性注释问题另列，不改列注释。 | 2026年11月14日 22:25:16 |
| 311 | `src/Modules/Files/Kmic.Files.Contracts/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 312 | `src/Modules/Files/Kmic.Files.Domain/Errors/FilesErrors.cs` | 保留 | 核对文件契约、仓储约定及 StoredFile 状态机：保留锁/事务、异步登记、流释放、分页和重置删除规则；修正状态说明中的操作者及参数含义。持久化属性注释问题另列，不改列注释。 | 2026年11月14日 22:25:16 |
| 313 | `src/Modules/Files/Kmic.Files.Domain/Files/FileReference.cs` | 保留 | 核对文件契约、仓储约定及 StoredFile 状态机：保留锁/事务、异步登记、流释放、分页和重置删除规则；修正状态说明中的操作者及参数含义。持久化属性注释问题另列，不改列注释。 | 2026年11月14日 22:25:16 |
| 314 | `src/Modules/Files/Kmic.Files.Domain/Files/FileReferenceId.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 315 | `src/Modules/Files/Kmic.Files.Domain/Files/IStorageIssueRepository.cs` | 保留 | 核对文件契约、仓储约定及 StoredFile 状态机：保留锁/事务、异步登记、流释放、分页和重置删除规则；修正状态说明中的操作者及参数含义。持久化属性注释问题另列，不改列注释。 | 2026年11月14日 22:25:16 |
| 316 | `src/Modules/Files/Kmic.Files.Domain/Files/IStoredFileRepository.cs` | 保留 | 核对文件契约、仓储约定及 StoredFile 状态机：保留锁/事务、异步登记、流释放、分页和重置删除规则；修正状态说明中的操作者及参数含义。持久化属性注释问题另列，不改列注释。 | 2026年11月14日 22:25:16 |
| 317 | `src/Modules/Files/Kmic.Files.Domain/Files/StorageIssue.cs` | 保留 | 核对文件契约、仓储约定及 StoredFile 状态机：保留锁/事务、异步登记、流释放、分页和重置删除规则；修正状态说明中的操作者及参数含义。持久化属性注释问题另列，不改列注释。 | 2026年11月14日 22:25:16 |
| 318 | `src/Modules/Files/Kmic.Files.Domain/Files/StorageIssueId.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 319 | `src/Modules/Files/Kmic.Files.Domain/Files/StorageIssueKind.cs` | 保留 | 核对文件契约、仓储约定及 StoredFile 状态机：保留锁/事务、异步登记、流释放、分页和重置删除规则；修正状态说明中的操作者及参数含义。持久化属性注释问题另列，不改列注释。 | 2026年11月14日 22:25:16 |
| 320 | `src/Modules/Files/Kmic.Files.Domain/Files/StorageIssueStatus.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 321 | `src/Modules/Files/Kmic.Files.Domain/Files/StoredFile.cs` | 已修订（模型中文） | 本轮 C2259～C2269；先核对实际行为，再精简或纠正摘要/模型说明。已同步注释迁移与快照，运行模型及 SQL 仅注释变化；测试未运行 | 2026年11月15日 00:04:56 |
| 322 | `src/Modules/Files/Kmic.Files.Domain/Files/StoredFileAction.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 323 | `src/Modules/Files/Kmic.Files.Domain/Files/StoredFileStatus.cs` | 已修订 | 核对文件契约、仓储约定及 StoredFile 状态机：保留锁/事务、异步登记、流释放、分页和重置删除规则；修正状态说明中的操作者及参数含义。持久化属性注释问题另列，不改列注释。；本阶段 C2208 | 2026年11月14日 22:25:17 |
| 324 | `src/Modules/Files/Kmic.Files.Domain/Files/StoredObjectRef.cs` | 保留 | 核对文件契约、仓储约定及 StoredFile 状态机：保留锁/事务、异步登记、流释放、分页和重置删除规则；修正状态说明中的操作者及参数含义。持久化属性注释问题另列，不改列注释。 | 2026年11月14日 22:25:17 |
| 325 | `src/Modules/Files/Kmic.Files.Domain/Kmic.Files.Domain.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 326 | `src/Modules/Files/Kmic.Files.Domain/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 327 | `src/Modules/Files/Kmic.Files.Infrastructure/Deletion/DeletionItem.cs` | 保留 | 核对删除队列 SQL 的领取次数、租约令牌校验与两类完成行为，以及仓储查询的包含边界和重置列；用途说明清楚，保留失败重试和并发保护。HasComment/影子列注释只读保留，模型文案问题不直接改。 | 2026年11月14日 22:25:31 |
| 328 | `src/Modules/Files/Kmic.Files.Infrastructure/Deletion/FilesDeletionOptions.cs` | 保留 | 核对删除队列 SQL 的领取次数、租约令牌校验与两类完成行为，以及仓储查询的包含边界和重置列；用途说明清楚，保留失败重试和并发保护。HasComment/影子列注释只读保留，模型文案问题不直接改。 | 2026年11月14日 22:25:31 |
| 329 | `src/Modules/Files/Kmic.Files.Infrastructure/Deletion/FilesDeletionStore.cs` | 保留 | 核对删除队列 SQL 的领取次数、租约令牌校验与两类完成行为，以及仓储查询的包含边界和重置列；用途说明清楚，保留失败重试和并发保护。HasComment/影子列注释只读保留，模型文案问题不直接改。 | 2026年11月14日 22:25:31 |
| 330 | `src/Modules/Files/Kmic.Files.Infrastructure/Deletion/StoredFileDeletionWork.cs` | 保留 | 核对删除队列 SQL 的领取次数、租约令牌校验与两类完成行为，以及仓储查询的包含边界和重置列；用途说明清楚，保留失败重试和并发保护。HasComment/影子列注释只读保留，模型文案问题不直接改。 | 2026年11月14日 22:25:31 |
| 331 | `src/Modules/Files/Kmic.Files.Infrastructure/FilesLog.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 332 | `src/Modules/Files/Kmic.Files.Infrastructure/FilesModule.cs` | 保留 | 核对删除队列 SQL 的领取次数、租约令牌校验与两类完成行为，以及仓储查询的包含边界和重置列；用途说明清楚，保留失败重试和并发保护。HasComment/影子列注释只读保留，模型文案问题不直接改。 | 2026年11月14日 22:25:31 |
| 333 | `src/Modules/Files/Kmic.Files.Infrastructure/Kmic.Files.Infrastructure.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 334 | `src/Modules/Files/Kmic.Files.Infrastructure/Persistence/Configurations/FileReferenceConfiguration.cs` | 保留 | 核对删除队列 SQL 的领取次数、租约令牌校验与两类完成行为，以及仓储查询的包含边界和重置列；用途说明清楚，保留失败重试和并发保护。HasComment/影子列注释只读保留，模型文案问题不直接改。 | 2026年11月14日 22:25:31 |
| 335 | `src/Modules/Files/Kmic.Files.Infrastructure/Persistence/Configurations/StorageIssueConfiguration.cs` | 已修订（模型中文） | 本轮 C2286；先核对实际行为，再精简或纠正摘要/模型说明。已同步注释迁移与快照，运行模型及 SQL 仅注释变化；测试未运行 | 2026年11月15日 00:04:56 |
| 336 | `src/Modules/Files/Kmic.Files.Infrastructure/Persistence/Configurations/StoredFileConfiguration.cs` | 已修订（模型中文） | 本轮 C2285～C2287；先核对实际行为，再精简或纠正摘要/模型说明。已同步注释迁移与快照，运行模型及 SQL 仅注释变化；测试未运行 | 2026年11月15日 00:04:56 |
| 337 | `src/Modules/Files/Kmic.Files.Infrastructure/Persistence/FilesDbContext.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 338 | `src/Modules/Files/Kmic.Files.Infrastructure/Persistence/FilesDbContextFactory.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 339 | `src/Modules/Files/Kmic.Files.Infrastructure/Persistence/FilesIdConverters.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 340 | `src/Modules/Files/Kmic.Files.Infrastructure/Persistence/FilesModel.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 341 | `src/Modules/Files/Kmic.Files.Infrastructure/Persistence/FilesQueryDbContext.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 342 | `src/Modules/Files/Kmic.Files.Infrastructure/Persistence/Repositories/StorageIssueRepository.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 343 | `src/Modules/Files/Kmic.Files.Infrastructure/Persistence/Repositories/StoredFileRepository.cs` | 保留 | 核对删除队列 SQL 的领取次数、租约令牌校验与两类完成行为，以及仓储查询的包含边界和重置列；用途说明清楚，保留失败重试和并发保护。HasComment/影子列注释只读保留，模型文案问题不直接改。 | 2026年11月14日 22:25:32 |
| 344 | `src/Modules/Files/Kmic.Files.Infrastructure/Storage/ConfiguredFileStorage.cs` | 保留 | 核对删除队列 SQL 的领取次数、租约令牌校验与两类完成行为，以及仓储查询的包含边界和重置列；用途说明清楚，保留失败重试和并发保护。HasComment/影子列注释只读保留，模型文案问题不直接改。 | 2026年11月14日 22:25:32 |
| 345 | `src/Modules/Files/Kmic.Files.Infrastructure/Storage/FileContentReader.cs` | 保留 | 核对存储代理、环境键、管理端点与流式上传实现：字段单位、权限、返回码、两次登记短事务和第一个文件段规则表达清楚；保留错误与清理约束。 | 2026年11月14日 22:25:43 |
| 346 | `src/Modules/Files/Kmic.Files.Infrastructure/Storage/FilesStorageOptions.cs` | 保留 | 核对存储代理、环境键、管理端点与流式上传实现：字段单位、权限、返回码、两次登记短事务和第一个文件段规则表达清楚；保留错误与清理约束。 | 2026年11月14日 22:25:43 |
| 347 | `src/Modules/Files/Kmic.Files.Infrastructure/Storage/StorageInventory.cs` | 保留 | 核对存储代理、环境键、管理端点与流式上传实现：字段单位、权限、返回码、两次登记短事务和第一个文件段规则表达清楚；保留错误与清理约束。 | 2026年11月14日 22:25:44 |
| 348 | `src/Modules/Files/Kmic.Files.Infrastructure/Storage/StorageKeyFactory.cs` | 保留 | 核对存储代理、环境键、管理端点与流式上传实现：字段单位、权限、返回码、两次登记短事务和第一个文件段规则表达清楚；保留错误与清理约束。 | 2026年11月14日 22:25:44 |
| 349 | `src/Modules/Files/Kmic.Files.Infrastructure/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 350 | `src/Modules/Files/Kmic.Files.Presentation/Administration/GetFileEndpoint.cs` | 保留 | 核对存储代理、环境键、管理端点与流式上传实现：字段单位、权限、返回码、两次登记短事务和第一个文件段规则表达清楚；保留错误与清理约束。 | 2026年11月14日 22:25:44 |
| 351 | `src/Modules/Files/Kmic.Files.Presentation/Administration/ListFilesEndpoint.cs` | 保留 | 核对存储代理、环境键、管理端点与流式上传实现：字段单位、权限、返回码、两次登记短事务和第一个文件段规则表达清楚；保留错误与清理约束。 | 2026年11月14日 22:25:44 |
| 352 | `src/Modules/Files/Kmic.Files.Presentation/Administration/ListFilesRequest.cs` | 保留 | 核对存储代理、环境键、管理端点与流式上传实现：字段单位、权限、返回码、两次登记短事务和第一个文件段规则表达清楚；保留错误与清理约束。 | 2026年11月14日 22:25:44 |
| 353 | `src/Modules/Files/Kmic.Files.Presentation/Administration/ListStorageIssuesEndpoint.cs` | 保留 | 核对存储代理、环境键、管理端点与流式上传实现：字段单位、权限、返回码、两次登记短事务和第一个文件段规则表达清楚；保留错误与清理约束。 | 2026年11月14日 22:25:44 |
| 354 | `src/Modules/Files/Kmic.Files.Presentation/Administration/ListStorageIssuesRequest.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 355 | `src/Modules/Files/Kmic.Files.Presentation/Administration/PurgeFileEndpoint.cs` | 保留 | 核对存储代理、环境键、管理端点与流式上传实现：字段单位、权限、返回码、两次登记短事务和第一个文件段规则表达清楚；保留错误与清理约束。 | 2026年11月14日 22:25:44 |
| 356 | `src/Modules/Files/Kmic.Files.Presentation/Administration/PurgeFileRequest.cs` | 保留 | 核对存储代理、环境键、管理端点与流式上传实现：字段单位、权限、返回码、两次登记短事务和第一个文件段规则表达清楚；保留错误与清理约束。 | 2026年11月14日 22:25:44 |
| 357 | `src/Modules/Files/Kmic.Files.Presentation/Administration/RecordHistoryPurgeEndpoint.cs` | 保留 | 核对存储代理、环境键、管理端点与流式上传实现：字段单位、权限、返回码、两次登记短事务和第一个文件段规则表达清楚；保留错误与清理约束。 | 2026年11月14日 22:25:44 |
| 358 | `src/Modules/Files/Kmic.Files.Presentation/Administration/RecordHistoryPurgeRequest.cs` | 保留 | 核对存储代理、环境键、管理端点与流式上传实现：字段单位、权限、返回码、两次登记短事务和第一个文件段规则表达清楚；保留错误与清理约束。 | 2026年11月14日 22:25:45 |
| 359 | `src/Modules/Files/Kmic.Files.Presentation/Administration/ResolveStorageIssueEndpoint.cs` | 保留 | 核对存储代理、环境键、管理端点与流式上传实现：字段单位、权限、返回码、两次登记短事务和第一个文件段规则表达清楚；保留错误与清理约束。 | 2026年11月14日 22:25:45 |
| 360 | `src/Modules/Files/Kmic.Files.Presentation/Administration/ResolveStorageIssueRequest.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 361 | `src/Modules/Files/Kmic.Files.Presentation/Administration/RetryDeletionEndpoint.cs` | 保留 | 核对存储代理、环境键、管理端点与流式上传实现：字段单位、权限、返回码、两次登记短事务和第一个文件段规则表达清楚；保留错误与清理约束。 | 2026年11月14日 22:25:45 |
| 362 | `src/Modules/Files/Kmic.Files.Presentation/FilesEndpoints.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 363 | `src/Modules/Files/Kmic.Files.Presentation/Kmic.Files.Presentation.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 364 | `src/Modules/Files/Kmic.Files.Presentation/Uploads/UploadFileEndpoint.cs` | 保留 | 核对存储代理、环境键、管理端点与流式上传实现：字段单位、权限、返回码、两次登记短事务和第一个文件段规则表达清楚；保留错误与清理约束。 | 2026年11月14日 22:25:45 |
| 365 | `src/Modules/Files/Kmic.Files.Presentation/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 366 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/AuthTokens.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:18 |
| 367 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/ChangePassword/ChangePasswordCommand.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:18 |
| 368 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/ChangePassword/ChangePasswordCommandHandler.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:18 |
| 369 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/ChangePassword/ChangePasswordCommandValidator.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 370 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/ClientKinds.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:18 |
| 371 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/GetMe/GetMeQuery.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:18 |
| 372 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/GetMe/GetMeQueryHandler.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:18 |
| 373 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/GetMe/MeResponse.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:18 |
| 374 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/GetMe/MeRole.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:18 |
| 375 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/ILoginThrottleStore.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:19 |
| 376 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/IPasswordService.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:19 |
| 377 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/IWeakPasswordList.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:19 |
| 378 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/Login/LoginCommand.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:19 |
| 379 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/Login/LoginCommandHandler.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:19 |
| 380 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/Login/LoginCommandValidator.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 381 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/LoginNames.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:19 |
| 382 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/LoginThrottleOptions.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:19 |
| 383 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/LoginThrottlePolicy.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:20 |
| 384 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/LoginThrottleState.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:20 |
| 385 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/Logout/LogoutCommand.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:20 |
| 386 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/Logout/LogoutCommandHandler.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:20 |
| 387 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/MySessions/ListMySessionsQuery.cs` | 已修订 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。；本阶段 C2239,C2240 | 2026年11月14日 22:34:20 |
| 388 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/MySessions/ListMySessionsQueryHandler.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:20 |
| 389 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/MySessions/MySessionItem.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:21 |
| 390 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/MySessions/RevokeMySessionCommand.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:21 |
| 391 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/MySessions/RevokeMySessionCommandHandler.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:21 |
| 392 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/PasswordCheck.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:21 |
| 393 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/PasswordOptions.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:21 |
| 394 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/PasswordPolicy.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:22 |
| 395 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/Refresh/RefreshTokenCommand.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 396 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/Refresh/RefreshTokenCommandHandler.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:22 |
| 397 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/Refresh/RevokeReusedSessionCommand.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:22 |
| 398 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/Refresh/RevokeReusedSessionCommandHandler.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:22 |
| 399 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/RefreshTokens.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 400 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/SessionLifetimeOptions.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:22 |
| 401 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/SwitchRole/SwitchRoleCommand.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:23 |
| 402 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/SwitchRole/SwitchRoleCommandHandler.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:23 |
| 403 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/TemporaryPasswordGenerator.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:23 |
| 404 | `src/Modules/Identity/Kmic.Identity.Application/Authentication/TrustedOriginsOptions.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:23 |
| 405 | `src/Modules/Identity/Kmic.Identity.Application/Authorization/AuthorizationAdminCommandHandler.cs` | 保留 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。 | 2026年11月14日 22:34:23 |
| 406 | `src/Modules/Identity/Kmic.Identity.Application/Authorization/AuthorizationChangeEvents.cs` | 已修订 | 核对认证处理器、登录限制算法、会话列表/撤销与授权基类：保留独立失败计数、旧令牌重用后另行提交、宽限期、模拟限制和角色选择规则；精简会话范围与重复参数说明。；本阶段 C2241,C2242 | 2026年11月14日 22:34:24 |
| 407 | `src/Modules/Identity/Kmic.Identity.Application/Authorization/IOperatorRecheck.cs` | 保留 | 核对操作者复查契约、权限目录同步/清理和跨模块引用查询：保留固定加锁顺序、退役保留回滚、角色编码改名、目录指纹、停用项仍可显示和系统身份联系方式约定；现有表达准确。 | 2026年11月14日 22:34:36 |
| 408 | `src/Modules/Identity/Kmic.Identity.Application/Authorization/OperatorFacts.cs` | 保留 | 核对操作者复查契约、权限目录同步/清理和跨模块引用查询：保留固定加锁顺序、退役保留回滚、角色编码改名、目录指纹、停用项仍可显示和系统身份联系方式约定；现有表达准确。 | 2026年11月14日 22:34:37 |
| 409 | `src/Modules/Identity/Kmic.Identity.Application/Catalog/CatalogChanges.cs` | 保留 | 核对操作者复查契约、权限目录同步/清理和跨模块引用查询：保留固定加锁顺序、退役保留回滚、角色编码改名、目录指纹、停用项仍可显示和系统身份联系方式约定；现有表达准确。 | 2026年11月14日 22:34:37 |
| 410 | `src/Modules/Identity/Kmic.Identity.Application/Catalog/CatalogContent.cs` | 保留 | 核对操作者复查契约、权限目录同步/清理和跨模块引用查询：保留固定加锁顺序、退役保留回滚、角色编码改名、目录指纹、停用项仍可显示和系统身份联系方式约定；现有表达准确。 | 2026年11月14日 22:34:37 |
| 411 | `src/Modules/Identity/Kmic.Identity.Application/Catalog/CatalogOptions.cs` | 保留 | 核对操作者复查契约、权限目录同步/清理和跨模块引用查询：保留固定加锁顺序、退役保留回滚、角色编码改名、目录指纹、停用项仍可显示和系统身份联系方式约定；现有表达准确。 | 2026年11月14日 22:34:37 |
| 412 | `src/Modules/Identity/Kmic.Identity.Application/Catalog/CatalogSyncResult.cs` | 保留 | 核对操作者复查契约、权限目录同步/清理和跨模块引用查询：保留固定加锁顺序、退役保留回滚、角色编码改名、目录指纹、停用项仍可显示和系统身份联系方式约定；现有表达准确。 | 2026年11月14日 22:34:37 |
| 413 | `src/Modules/Identity/Kmic.Identity.Application/Catalog/IPermissionCatalogStore.cs` | 保留 | 核对操作者复查契约、权限目录同步/清理和跨模块引用查询：保留固定加锁顺序、退役保留回滚、角色编码改名、目录指纹、停用项仍可显示和系统身份联系方式约定；现有表达准确。 | 2026年11月14日 22:34:37 |
| 414 | `src/Modules/Identity/Kmic.Identity.Application/Catalog/PurgeRetiredPermissionsCommand.cs` | 保留 | 核对操作者复查契约、权限目录同步/清理和跨模块引用查询：保留固定加锁顺序、退役保留回滚、角色编码改名、目录指纹、停用项仍可显示和系统身份联系方式约定；现有表达准确。 | 2026年11月14日 22:34:37 |
| 415 | `src/Modules/Identity/Kmic.Identity.Application/Catalog/PurgeRetiredPermissionsCommandHandler.cs` | 保留 | 核对操作者复查契约、权限目录同步/清理和跨模块引用查询：保留固定加锁顺序、退役保留回滚、角色编码改名、目录指纹、停用项仍可显示和系统身份联系方式约定；现有表达准确。 | 2026年11月14日 22:34:37 |
| 416 | `src/Modules/Identity/Kmic.Identity.Application/Catalog/SyncPermissionCatalogCommand.cs` | 保留 | 核对操作者复查契约、权限目录同步/清理和跨模块引用查询：保留固定加锁顺序、退役保留回滚、角色编码改名、目录指纹、停用项仍可显示和系统身份联系方式约定；现有表达准确。 | 2026年11月14日 22:34:37 |
| 417 | `src/Modules/Identity/Kmic.Identity.Application/Catalog/SyncPermissionCatalogCommandHandler.cs` | 保留 | 核对操作者复查契约、权限目录同步/清理和跨模块引用查询：保留固定加锁顺序、退役保留回滚、角色编码改名、目录指纹、停用项仍可显示和系统身份联系方式约定；现有表达准确。 | 2026年11月14日 22:34:38 |
| 418 | `src/Modules/Identity/Kmic.Identity.Application/ContractQueries/ActorReferences.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 419 | `src/Modules/Identity/Kmic.Identity.Application/ContractQueries/GetOrgUnitReferencesQuery.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 420 | `src/Modules/Identity/Kmic.Identity.Application/ContractQueries/GetOrgUnitReferencesQueryHandler.cs` | 保留 | 核对操作者复查契约、权限目录同步/清理和跨模块引用查询：保留固定加锁顺序、退役保留回滚、角色编码改名、目录指纹、停用项仍可显示和系统身份联系方式约定；现有表达准确。 | 2026年11月14日 22:34:38 |
| 421 | `src/Modules/Identity/Kmic.Identity.Application/ContractQueries/GetOrgUnitReferencesQueryValidator.cs` | 保留 | 核对操作者复查契约、权限目录同步/清理和跨模块引用查询：保留固定加锁顺序、退役保留回滚、角色编码改名、目录指纹、停用项仍可显示和系统身份联系方式约定；现有表达准确。 | 2026年11月14日 22:34:38 |
| 422 | `src/Modules/Identity/Kmic.Identity.Application/ContractQueries/GetRoleReferencesQuery.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 423 | `src/Modules/Identity/Kmic.Identity.Application/ContractQueries/GetRoleReferencesQueryHandler.cs` | 保留 | 核对操作者复查契约、权限目录同步/清理和跨模块引用查询：保留固定加锁顺序、退役保留回滚、角色编码改名、目录指纹、停用项仍可显示和系统身份联系方式约定；现有表达准确。 | 2026年11月14日 22:34:38 |
| 424 | `src/Modules/Identity/Kmic.Identity.Application/ContractQueries/GetRoleReferencesQueryValidator.cs` | 保留 | 核对操作者复查契约、权限目录同步/清理和跨模块引用查询：保留固定加锁顺序、退役保留回滚、角色编码改名、目录指纹、停用项仍可显示和系统身份联系方式约定；现有表达准确。 | 2026年11月14日 22:34:38 |
| 425 | `src/Modules/Identity/Kmic.Identity.Application/ContractQueries/GetUserContactsQuery.cs` | 保留 | 核对操作者复查契约、权限目录同步/清理和跨模块引用查询：保留固定加锁顺序、退役保留回滚、角色编码改名、目录指纹、停用项仍可显示和系统身份联系方式约定；现有表达准确。 | 2026年11月14日 22:34:38 |
| 426 | `src/Modules/Identity/Kmic.Identity.Application/ContractQueries/GetUserContactsQueryHandler.cs` | 保留 | 核对操作者复查契约、权限目录同步/清理和跨模块引用查询：保留固定加锁顺序、退役保留回滚、角色编码改名、目录指纹、停用项仍可显示和系统身份联系方式约定；现有表达准确。 | 2026年11月14日 22:34:38 |
| 427 | `src/Modules/Identity/Kmic.Identity.Application/ContractQueries/GetUserContactsQueryValidator.cs` | 保留 | 核对操作者复查契约、权限目录同步/清理和跨模块引用查询：保留固定加锁顺序、退役保留回滚、角色编码改名、目录指纹、停用项仍可显示和系统身份联系方式约定；现有表达准确。 | 2026年11月14日 22:34:38 |
| 428 | `src/Modules/Identity/Kmic.Identity.Application/ContractQueries/GetUserReferencesQuery.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 429 | `src/Modules/Identity/Kmic.Identity.Application/ContractQueries/GetUserReferencesQueryHandler.cs` | 保留 | 核对引用解析、身份读取接口、模拟会话和组织归属命令：保留禁用引用可读、查询/命令上下文差异、模拟写入权限固定、无刷新令牌、组织锁及既有业务数据归属不变等约定；明确刷新记录仅存哈希。 | 2026年11月14日 22:35:01 |
| 430 | `src/Modules/Identity/Kmic.Identity.Application/ContractQueries/GetUserReferencesQueryValidator.cs` | 保留 | 核对引用解析、身份读取接口、模拟会话和组织归属命令：保留禁用引用可读、查询/命令上下文差异、模拟写入权限固定、无刷新令牌、组织锁及既有业务数据归属不变等约定；明确刷新记录仅存哈希。 | 2026年11月14日 22:35:01 |
| 431 | `src/Modules/Identity/Kmic.Identity.Application/ContractQueries/OrgUnitReferences.cs` | 保留 | 核对引用解析、身份读取接口、模拟会话和组织归属命令：保留禁用引用可读、查询/命令上下文差异、模拟写入权限固定、无刷新令牌、组织锁及既有业务数据归属不变等约定；明确刷新记录仅存哈希。 | 2026年11月14日 22:35:01 |
| 432 | `src/Modules/Identity/Kmic.Identity.Application/ContractQueries/RoleReferences.cs` | 保留 | 核对引用解析、身份读取接口、模拟会话和组织归属命令：保留禁用引用可读、查询/命令上下文差异、模拟写入权限固定、无刷新令牌、组织锁及既有业务数据归属不变等约定；明确刷新记录仅存哈希。 | 2026年11月14日 22:35:01 |
| 433 | `src/Modules/Identity/Kmic.Identity.Application/ContractQueries/UserContacts.cs` | 保留 | 核对引用解析、身份读取接口、模拟会话和组织归属命令：保留禁用引用可读、查询/命令上下文差异、模拟写入权限固定、无刷新令牌、组织锁及既有业务数据归属不变等约定；明确刷新记录仅存哈希。 | 2026年11月14日 22:35:01 |
| 434 | `src/Modules/Identity/Kmic.Identity.Application/ContractQueries/UserReferences.cs` | 保留 | 核对引用解析、身份读取接口、模拟会话和组织归属命令：保留禁用引用可读、查询/命令上下文差异、模拟写入权限固定、无刷新令牌、组织锁及既有业务数据归属不变等约定；明确刷新记录仅存哈希。 | 2026年11月14日 22:35:01 |
| 435 | `src/Modules/Identity/Kmic.Identity.Application/IIdentityCommandReads.cs` | 保留 | 核对引用解析、身份读取接口、模拟会话和组织归属命令：保留禁用引用可读、查询/命令上下文差异、模拟写入权限固定、无刷新令牌、组织锁及既有业务数据归属不变等约定；明确刷新记录仅存哈希。 | 2026年11月14日 22:35:01 |
| 436 | `src/Modules/Identity/Kmic.Identity.Application/IIdentityQueryDb.cs` | 已修订 | 核对引用解析、身份读取接口、模拟会话和组织归属命令：保留禁用引用可读、查询/命令上下文差异、模拟写入权限固定、无刷新令牌、组织锁及既有业务数据归属不变等约定；明确刷新记录仅存哈希。；本阶段 C2243 | 2026年11月14日 22:35:02 |
| 437 | `src/Modules/Identity/Kmic.Identity.Application/IIdentityReads.cs` | 已修订 | 核对引用解析、身份读取接口、模拟会话和组织归属命令：保留禁用引用可读、查询/命令上下文差异、模拟写入权限固定、无刷新令牌、组织锁及既有业务数据归属不变等约定；明确刷新记录仅存哈希。；本阶段 C2244 | 2026年11月14日 22:35:02 |
| 438 | `src/Modules/Identity/Kmic.Identity.Application/IdentityPermissions.cs` | 保留 | 核对引用解析、身份读取接口、模拟会话和组织归属命令：保留禁用引用可读、查询/命令上下文差异、模拟写入权限固定、无刷新令牌、组织锁及既有业务数据归属不变等约定；明确刷新记录仅存哈希。 | 2026年11月14日 22:35:02 |
| 439 | `src/Modules/Identity/Kmic.Identity.Application/Impersonation/End/EndImpersonationCommand.cs` | 保留 | 核对引用解析、身份读取接口、模拟会话和组织归属命令：保留禁用引用可读、查询/命令上下文差异、模拟写入权限固定、无刷新令牌、组织锁及既有业务数据归属不变等约定；明确刷新记录仅存哈希。 | 2026年11月14日 22:35:02 |
| 440 | `src/Modules/Identity/Kmic.Identity.Application/Impersonation/End/EndImpersonationCommandHandler.cs` | 保留 | 核对引用解析、身份读取接口、模拟会话和组织归属命令：保留禁用引用可读、查询/命令上下文差异、模拟写入权限固定、无刷新令牌、组织锁及既有业务数据归属不变等约定；明确刷新记录仅存哈希。 | 2026年11月14日 22:35:02 |
| 441 | `src/Modules/Identity/Kmic.Identity.Application/Impersonation/Start/StartImpersonationCommand.cs` | 保留 | 核对引用解析、身份读取接口、模拟会话和组织归属命令：保留禁用引用可读、查询/命令上下文差异、模拟写入权限固定、无刷新令牌、组织锁及既有业务数据归属不变等约定；明确刷新记录仅存哈希。 | 2026年11月14日 22:35:02 |
| 442 | `src/Modules/Identity/Kmic.Identity.Application/Impersonation/Start/StartImpersonationCommandHandler.cs` | 保留 | 核对引用解析、身份读取接口、模拟会话和组织归属命令：保留禁用引用可读、查询/命令上下文差异、模拟写入权限固定、无刷新令牌、组织锁及既有业务数据归属不变等约定；明确刷新记录仅存哈希。 | 2026年11月14日 22:35:02 |
| 443 | `src/Modules/Identity/Kmic.Identity.Application/Impersonation/Start/StartImpersonationCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 444 | `src/Modules/Identity/Kmic.Identity.Application/Kmic.Identity.Application.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 445 | `src/Modules/Identity/Kmic.Identity.Application/Maintenance/PurgeLoginThrottleCommand.cs` | 保留 | 核对引用解析、身份读取接口、模拟会话和组织归属命令：保留禁用引用可读、查询/命令上下文差异、模拟写入权限固定、无刷新令牌、组织锁及既有业务数据归属不变等约定；明确刷新记录仅存哈希。 | 2026年11月14日 22:35:02 |
| 446 | `src/Modules/Identity/Kmic.Identity.Application/Maintenance/PurgeLoginThrottleCommandHandler.cs` | 保留 | 核对引用解析、身份读取接口、模拟会话和组织归属命令：保留禁用引用可读、查询/命令上下文差异、模拟写入权限固定、无刷新令牌、组织锁及既有业务数据归属不变等约定；明确刷新记录仅存哈希。 | 2026年11月14日 22:35:03 |
| 447 | `src/Modules/Identity/Kmic.Identity.Application/Maintenance/PurgeSessionsCommand.cs` | 保留 | 核对引用解析、身份读取接口、模拟会话和组织归属命令：保留禁用引用可读、查询/命令上下文差异、模拟写入权限固定、无刷新令牌、组织锁及既有业务数据归属不变等约定；明确刷新记录仅存哈希。 | 2026年11月14日 22:35:03 |
| 448 | `src/Modules/Identity/Kmic.Identity.Application/Maintenance/PurgeSessionsCommandHandler.cs` | 保留 | 核对引用解析、身份读取接口、模拟会话和组织归属命令：保留禁用引用可读、查询/命令上下文差异、模拟写入权限固定、无刷新令牌、组织锁及既有业务数据归属不变等约定；明确刷新记录仅存哈希。 | 2026年11月14日 22:35:03 |
| 449 | `src/Modules/Identity/Kmic.Identity.Application/Organization/AssignUser/AssignUserOrgUnitCommand.cs` | 保留 | 核对引用解析、身份读取接口、模拟会话和组织归属命令：保留禁用引用可读、查询/命令上下文差异、模拟写入权限固定、无刷新令牌、组织锁及既有业务数据归属不变等约定；明确刷新记录仅存哈希。 | 2026年11月14日 22:35:03 |
| 450 | `src/Modules/Identity/Kmic.Identity.Application/Organization/AssignUser/AssignUserOrgUnitCommandHandler.cs` | 保留 | 核对引用解析、身份读取接口、模拟会话和组织归属命令：保留禁用引用可读、查询/命令上下文差异、模拟写入权限固定、无刷新令牌、组织锁及既有业务数据归属不变等约定；明确刷新记录仅存哈希。 | 2026年11月14日 22:35:03 |
| 451 | `src/Modules/Identity/Kmic.Identity.Application/Organization/Create/CreateOrgUnitCommand.cs` | 保留 | 核对引用解析、身份读取接口、模拟会话和组织归属命令：保留禁用引用可读、查询/命令上下文差异、模拟写入权限固定、无刷新令牌、组织锁及既有业务数据归属不变等约定；明确刷新记录仅存哈希。 | 2026年11月14日 22:35:04 |
| 452 | `src/Modules/Identity/Kmic.Identity.Application/Organization/Create/CreateOrgUnitCommandHandler.cs` | 保留 | 核对引用解析、身份读取接口、模拟会话和组织归属命令：保留禁用引用可读、查询/命令上下文差异、模拟写入权限固定、无刷新令牌、组织锁及既有业务数据归属不变等约定；明确刷新记录仅存哈希。 | 2026年11月14日 22:35:04 |
| 453 | `src/Modules/Identity/Kmic.Identity.Application/Organization/Create/CreateOrgUnitCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 454 | `src/Modules/Identity/Kmic.Identity.Application/Organization/Disable/DisableOrgUnitCommand.cs` | 保留 | 核对引用解析、身份读取接口、模拟会话和组织归属命令：保留禁用引用可读、查询/命令上下文差异、模拟写入权限固定、无刷新令牌、组织锁及既有业务数据归属不变等约定；明确刷新记录仅存哈希。 | 2026年11月14日 22:35:04 |
| 455 | `src/Modules/Identity/Kmic.Identity.Application/Organization/Disable/DisableOrgUnitCommandHandler.cs` | 保留 | 核对组织停启、移动、排序及角色目录/创建处理器：保留组织树统一版本、并发归属互斥、历史单元可见、目录排序和初始无权限点等说明；不扩大业务保证。 | 2026年11月14日 22:35:13 |
| 456 | `src/Modules/Identity/Kmic.Identity.Application/Organization/Enable/EnableOrgUnitCommand.cs` | 保留 | 核对组织停启、移动、排序及角色目录/创建处理器：保留组织树统一版本、并发归属互斥、历史单元可见、目录排序和初始无权限点等说明；不扩大业务保证。 | 2026年11月14日 22:35:13 |
| 457 | `src/Modules/Identity/Kmic.Identity.Application/Organization/Enable/EnableOrgUnitCommandHandler.cs` | 保留 | 核对组织停启、移动、排序及角色目录/创建处理器：保留组织树统一版本、并发归属互斥、历史单元可见、目录排序和初始无权限点等说明；不扩大业务保证。 | 2026年11月14日 22:35:13 |
| 458 | `src/Modules/Identity/Kmic.Identity.Application/Organization/Get/GetOrgTreeQuery.cs` | 保留 | 核对组织停启、移动、排序及角色目录/创建处理器：保留组织树统一版本、并发归属互斥、历史单元可见、目录排序和初始无权限点等说明；不扩大业务保证。 | 2026年11月14日 22:35:14 |
| 459 | `src/Modules/Identity/Kmic.Identity.Application/Organization/Get/GetOrgTreeQueryHandler.cs` | 保留 | 核对组织停启、移动、排序及角色目录/创建处理器：保留组织树统一版本、并发归属互斥、历史单元可见、目录排序和初始无权限点等说明；不扩大业务保证。 | 2026年11月14日 22:35:14 |
| 460 | `src/Modules/Identity/Kmic.Identity.Application/Organization/Get/OrgTreeResponse.cs` | 保留 | 核对组织停启、移动、排序及角色目录/创建处理器：保留组织树统一版本、并发归属互斥、历史单元可见、目录排序和初始无权限点等说明；不扩大业务保证。 | 2026年11月14日 22:35:14 |
| 461 | `src/Modules/Identity/Kmic.Identity.Application/Organization/Get/OrgUnitItem.cs` | 保留 | 核对组织停启、移动、排序及角色目录/创建处理器：保留组织树统一版本、并发归属互斥、历史单元可见、目录排序和初始无权限点等说明；不扩大业务保证。 | 2026年11月14日 22:35:14 |
| 462 | `src/Modules/Identity/Kmic.Identity.Application/Organization/Move/MoveOrgUnitCommand.cs` | 保留 | 核对组织停启、移动、排序及角色目录/创建处理器：保留组织树统一版本、并发归属互斥、历史单元可见、目录排序和初始无权限点等说明；不扩大业务保证。 | 2026年11月14日 22:35:14 |
| 463 | `src/Modules/Identity/Kmic.Identity.Application/Organization/Move/MoveOrgUnitCommandHandler.cs` | 保留 | 核对组织停启、移动、排序及角色目录/创建处理器：保留组织树统一版本、并发归属互斥、历史单元可见、目录排序和初始无权限点等说明；不扩大业务保证。 | 2026年11月14日 22:35:14 |
| 464 | `src/Modules/Identity/Kmic.Identity.Application/Organization/Reorder/ReorderOrgUnitsCommand.cs` | 保留 | 核对组织停启、移动、排序及角色目录/创建处理器：保留组织树统一版本、并发归属互斥、历史单元可见、目录排序和初始无权限点等说明；不扩大业务保证。 | 2026年11月14日 22:35:14 |
| 465 | `src/Modules/Identity/Kmic.Identity.Application/Organization/Reorder/ReorderOrgUnitsCommandHandler.cs` | 保留 | 核对组织停启、移动、排序及角色目录/创建处理器：保留组织树统一版本、并发归属互斥、历史单元可见、目录排序和初始无权限点等说明；不扩大业务保证。 | 2026年11月14日 22:35:14 |
| 466 | `src/Modules/Identity/Kmic.Identity.Application/Organization/Update/UpdateOrgUnitCommand.cs` | 保留 | 核对组织停启、移动、排序及角色目录/创建处理器：保留组织树统一版本、并发归属互斥、历史单元可见、目录排序和初始无权限点等说明；不扩大业务保证。 | 2026年11月14日 22:35:15 |
| 467 | `src/Modules/Identity/Kmic.Identity.Application/Organization/Update/UpdateOrgUnitCommandHandler.cs` | 保留 | 核对组织停启、移动、排序及角色目录/创建处理器：保留组织树统一版本、并发归属互斥、历史单元可见、目录排序和初始无权限点等说明；不扩大业务保证。 | 2026年11月14日 22:35:15 |
| 468 | `src/Modules/Identity/Kmic.Identity.Application/Organization/Update/UpdateOrgUnitCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 469 | `src/Modules/Identity/Kmic.Identity.Application/Roles/Catalogs/DataScopeObjectItem.cs` | 保留 | 核对组织停启、移动、排序及角色目录/创建处理器：保留组织树统一版本、并发归属互斥、历史单元可见、目录排序和初始无权限点等说明；不扩大业务保证。 | 2026年11月14日 22:35:15 |
| 470 | `src/Modules/Identity/Kmic.Identity.Application/Roles/Catalogs/ListDataScopeObjectsQuery.cs` | 保留 | 核对组织停启、移动、排序及角色目录/创建处理器：保留组织树统一版本、并发归属互斥、历史单元可见、目录排序和初始无权限点等说明；不扩大业务保证。 | 2026年11月14日 22:35:15 |
| 471 | `src/Modules/Identity/Kmic.Identity.Application/Roles/Catalogs/ListDataScopeObjectsQueryHandler.cs` | 保留 | 核对组织停启、移动、排序及角色目录/创建处理器：保留组织树统一版本、并发归属互斥、历史单元可见、目录排序和初始无权限点等说明；不扩大业务保证。 | 2026年11月14日 22:35:15 |
| 472 | `src/Modules/Identity/Kmic.Identity.Application/Roles/Catalogs/ListPermissionsQuery.cs` | 保留 | 核对组织停启、移动、排序及角色目录/创建处理器：保留组织树统一版本、并发归属互斥、历史单元可见、目录排序和初始无权限点等说明；不扩大业务保证。 | 2026年11月14日 22:35:15 |
| 473 | `src/Modules/Identity/Kmic.Identity.Application/Roles/Catalogs/ListPermissionsQueryHandler.cs` | 保留 | 核对组织停启、移动、排序及角色目录/创建处理器：保留组织树统一版本、并发归属互斥、历史单元可见、目录排序和初始无权限点等说明；不扩大业务保证。 | 2026年11月14日 22:35:15 |
| 474 | `src/Modules/Identity/Kmic.Identity.Application/Roles/Catalogs/PermissionItem.cs` | 保留 | 核对组织停启、移动、排序及角色目录/创建处理器：保留组织树统一版本、并发归属互斥、历史单元可见、目录排序和初始无权限点等说明；不扩大业务保证。 | 2026年11月14日 22:35:15 |
| 475 | `src/Modules/Identity/Kmic.Identity.Application/Roles/Create/CreateRoleCommand.cs` | 保留 | 核对组织停启、移动、排序及角色目录/创建处理器：保留组织树统一版本、并发归属互斥、历史单元可见、目录排序和初始无权限点等说明；不扩大业务保证。 | 2026年11月14日 22:35:15 |
| 476 | `src/Modules/Identity/Kmic.Identity.Application/Roles/Create/CreateRoleCommandHandler.cs` | 保留 | 核对组织停启、移动、排序及角色目录/创建处理器：保留组织树统一版本、并发归属互斥、历史单元可见、目录排序和初始无权限点等说明；不扩大业务保证。 | 2026年11月14日 22:35:16 |
| 477 | `src/Modules/Identity/Kmic.Identity.Application/Roles/Create/CreateRoleCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 478 | `src/Modules/Identity/Kmic.Identity.Application/Roles/Disable/DisableRoleCommand.cs` | 保留 | 核对组织停启、移动、排序及角色目录/创建处理器：保留组织树统一版本、并发归属互斥、历史单元可见、目录排序和初始无权限点等说明；不扩大业务保证。 | 2026年11月14日 22:35:16 |
| 479 | `src/Modules/Identity/Kmic.Identity.Application/Roles/Disable/DisableRoleCommandHandler.cs` | 保留 | 核对角色查询、策略和权限/数据范围替换：保留受保护角色操作者要求、内置角色不可手工配置、版本校验与未知编码整体拒绝；修正退役编码可能被自动过滤的歧义。 | 2026年11月14日 22:35:32 |
| 480 | `src/Modules/Identity/Kmic.Identity.Application/Roles/Enable/EnableRoleCommand.cs` | 保留 | 核对角色查询、策略和权限/数据范围替换：保留受保护角色操作者要求、内置角色不可手工配置、版本校验与未知编码整体拒绝；修正退役编码可能被自动过滤的歧义。 | 2026年11月14日 22:35:32 |
| 481 | `src/Modules/Identity/Kmic.Identity.Application/Roles/Enable/EnableRoleCommandHandler.cs` | 保留 | 核对角色查询、策略和权限/数据范围替换：保留受保护角色操作者要求、内置角色不可手工配置、版本校验与未知编码整体拒绝；修正退役编码可能被自动过滤的歧义。 | 2026年11月14日 22:35:32 |
| 482 | `src/Modules/Identity/Kmic.Identity.Application/Roles/Get/GetRoleQuery.cs` | 保留 | 核对角色查询、策略和权限/数据范围替换：保留受保护角色操作者要求、内置角色不可手工配置、版本校验与未知编码整体拒绝；修正退役编码可能被自动过滤的歧义。 | 2026年11月14日 22:35:32 |
| 483 | `src/Modules/Identity/Kmic.Identity.Application/Roles/Get/GetRoleQueryHandler.cs` | 保留 | 核对角色查询、策略和权限/数据范围替换：保留受保护角色操作者要求、内置角色不可手工配置、版本校验与未知编码整体拒绝；修正退役编码可能被自动过滤的歧义。 | 2026年11月14日 22:35:33 |
| 484 | `src/Modules/Identity/Kmic.Identity.Application/Roles/Get/RoleDataScopeItem.cs` | 保留 | 核对角色查询、策略和权限/数据范围替换：保留受保护角色操作者要求、内置角色不可手工配置、版本校验与未知编码整体拒绝；修正退役编码可能被自动过滤的歧义。 | 2026年11月14日 22:35:33 |
| 485 | `src/Modules/Identity/Kmic.Identity.Application/Roles/Get/RoleDetailResponse.cs` | 保留 | 核对角色查询、策略和权限/数据范围替换：保留受保护角色操作者要求、内置角色不可手工配置、版本校验与未知编码整体拒绝；修正退役编码可能被自动过滤的歧义。 | 2026年11月14日 22:35:33 |
| 486 | `src/Modules/Identity/Kmic.Identity.Application/Roles/Get/RolePermissionItem.cs` | 已修订 | 核对角色查询、策略和权限/数据范围替换：保留受保护角色操作者要求、内置角色不可手工配置、版本校验与未知编码整体拒绝；修正退役编码可能被自动过滤的歧义。；本阶段 C2245 | 2026年11月14日 22:35:33 |
| 487 | `src/Modules/Identity/Kmic.Identity.Application/Roles/List/ListRolesQuery.cs` | 保留 | 核对角色查询、策略和权限/数据范围替换：保留受保护角色操作者要求、内置角色不可手工配置、版本校验与未知编码整体拒绝；修正退役编码可能被自动过滤的歧义。 | 2026年11月14日 22:35:33 |
| 488 | `src/Modules/Identity/Kmic.Identity.Application/Roles/List/ListRolesQueryHandler.cs` | 保留 | 核对角色查询、策略和权限/数据范围替换：保留受保护角色操作者要求、内置角色不可手工配置、版本校验与未知编码整体拒绝；修正退役编码可能被自动过滤的歧义。 | 2026年11月14日 22:35:33 |
| 489 | `src/Modules/Identity/Kmic.Identity.Application/Roles/List/RoleListItem.cs` | 保留 | 核对角色查询、策略和权限/数据范围替换：保留受保护角色操作者要求、内置角色不可手工配置、版本校验与未知编码整体拒绝；修正退役编码可能被自动过滤的歧义。 | 2026年11月14日 22:35:33 |
| 490 | `src/Modules/Identity/Kmic.Identity.Application/Roles/List/RoleSort.cs` | 保留 | 核对角色查询、策略和权限/数据范围替换：保留受保护角色操作者要求、内置角色不可手工配置、版本校验与未知编码整体拒绝；修正退役编码可能被自动过滤的歧义。 | 2026年11月14日 22:35:33 |
| 491 | `src/Modules/Identity/Kmic.Identity.Application/Roles/RoleActions.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 492 | `src/Modules/Identity/Kmic.Identity.Application/Roles/RoleAdminAction.cs` | 保留 | 核对角色查询、策略和权限/数据范围替换：保留受保护角色操作者要求、内置角色不可手工配置、版本校验与未知编码整体拒绝；修正退役编码可能被自动过滤的歧义。 | 2026年11月14日 22:35:33 |
| 493 | `src/Modules/Identity/Kmic.Identity.Application/Roles/RoleAdminLoading.cs` | 保留 | 核对角色查询、策略和权限/数据范围替换：保留受保护角色操作者要求、内置角色不可手工配置、版本校验与未知编码整体拒绝；修正退役编码可能被自动过滤的歧义。 | 2026年11月14日 22:35:33 |
| 494 | `src/Modules/Identity/Kmic.Identity.Application/Roles/RolePolicy.cs` | 保留 | 核对角色查询、策略和权限/数据范围替换：保留受保护角色操作者要求、内置角色不可手工配置、版本校验与未知编码整体拒绝；修正退役编码可能被自动过滤的歧义。 | 2026年11月14日 22:35:34 |
| 495 | `src/Modules/Identity/Kmic.Identity.Application/Roles/RoleTarget.cs` | 保留 | 核对角色查询、策略和权限/数据范围替换：保留受保护角色操作者要求、内置角色不可手工配置、版本校验与未知编码整体拒绝；修正退役编码可能被自动过滤的歧义。 | 2026年11月14日 22:35:34 |
| 496 | `src/Modules/Identity/Kmic.Identity.Application/Roles/SetDataScopes/SetRoleDataScopesCommand.cs` | 保留 | 核对角色查询、策略和权限/数据范围替换：保留受保护角色操作者要求、内置角色不可手工配置、版本校验与未知编码整体拒绝；修正退役编码可能被自动过滤的歧义。 | 2026年11月14日 22:35:34 |
| 497 | `src/Modules/Identity/Kmic.Identity.Application/Roles/SetDataScopes/SetRoleDataScopesCommandHandler.cs` | 保留 | 核对角色查询、策略和权限/数据范围替换：保留受保护角色操作者要求、内置角色不可手工配置、版本校验与未知编码整体拒绝；修正退役编码可能被自动过滤的歧义。 | 2026年11月14日 22:35:34 |
| 498 | `src/Modules/Identity/Kmic.Identity.Application/Roles/SetDataScopes/SetRoleDataScopesCommandValidator.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 499 | `src/Modules/Identity/Kmic.Identity.Application/Roles/SetPermissions/SetRolePermissionsCommand.cs` | 保留 | 核对角色查询、策略和权限/数据范围替换：保留受保护角色操作者要求、内置角色不可手工配置、版本校验与未知编码整体拒绝；修正退役编码可能被自动过滤的歧义。 | 2026年11月14日 22:35:34 |
| 500 | `src/Modules/Identity/Kmic.Identity.Application/Roles/SetPermissions/SetRolePermissionsCommandHandler.cs` | 保留 | 核对角色查询、策略和权限/数据范围替换：保留受保护角色操作者要求、内置角色不可手工配置、版本校验与未知编码整体拒绝；修正退役编码可能被自动过滤的歧义。 | 2026年11月14日 22:35:34 |
| 501 | `src/Modules/Identity/Kmic.Identity.Application/Roles/SetPermissions/SetRolePermissionsCommandValidator.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 502 | `src/Modules/Identity/Kmic.Identity.Application/Roles/Update/UpdateRoleCommand.cs` | 保留 | 核对角色查询、策略和权限/数据范围替换：保留受保护角色操作者要求、内置角色不可手工配置、版本校验与未知编码整体拒绝；修正退役编码可能被自动过滤的歧义。 | 2026年11月14日 22:35:34 |
| 503 | `src/Modules/Identity/Kmic.Identity.Application/Roles/Update/UpdateRoleCommandHandler.cs` | 保留 | 核对角色查询、策略和权限/数据范围替换：保留受保护角色操作者要求、内置角色不可手工配置、版本校验与未知编码整体拒绝；修正退役编码可能被自动过滤的歧义。 | 2026年11月14日 22:35:35 |
| 504 | `src/Modules/Identity/Kmic.Identity.Application/Roles/Update/UpdateRoleCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 505 | `src/Modules/Identity/Kmic.Identity.Application/SecurityEvents/ListSecurityEventsQuery.cs` | 已修订 | 完成跨文件一致性复查后重新核对本轮最终内容：字典/组织移动仅在父项变化时重排；取消写所属模块；投递上限仅在暂时失败时判断；事件账号与模拟会话时间说明均与实现一致。；本阶段 C2251 | 2026年11月14日 22:41:41 |
| 506 | `src/Modules/Identity/Kmic.Identity.Application/SecurityEvents/ListSecurityEventsQueryHandler.cs` | 保留 | 核对安全事件来源、用户详情投影及授予限制：更正等待/锁定截止和事件账号范围；保留当前角色保护、最后可登录持有人锁、模拟会话关联与归属检查。 | 2026年11月14日 22:36:35 |
| 507 | `src/Modules/Identity/Kmic.Identity.Application/SecurityEvents/ListSecurityEventsQueryValidator.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 508 | `src/Modules/Identity/Kmic.Identity.Application/SecurityEvents/SecurityEventItem.cs` | 已修订 | 核对安全事件来源、用户详情投影及授予限制：更正等待/锁定截止和事件账号范围；保留当前角色保护、最后可登录持有人锁、模拟会话关联与归属检查。；本阶段 C2247,C2248 | 2026年11月14日 22:36:35 |
| 509 | `src/Modules/Identity/Kmic.Identity.Application/SecurityEvents/SecurityEventRow.cs` | 已修订 | 核对安全事件来源、用户详情投影及授予限制：更正等待/锁定截止和事件账号范围；保留当前角色保护、最后可登录持有人锁、模拟会话关联与归属检查。；本阶段 C2249,C2250 | 2026年11月14日 22:36:35 |
| 510 | `src/Modules/Identity/Kmic.Identity.Application/SecurityEvents/SecurityEventSort.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 511 | `src/Modules/Identity/Kmic.Identity.Application/Users/Create/CreateUserCommand.cs` | 保留 | 核对安全事件来源、用户详情投影及授予限制：更正等待/锁定截止和事件账号范围；保留当前角色保护、最后可登录持有人锁、模拟会话关联与归属检查。 | 2026年11月14日 22:36:35 |
| 512 | `src/Modules/Identity/Kmic.Identity.Application/Users/Create/CreateUserCommandHandler.cs` | 保留 | 核对安全事件来源、用户详情投影及授予限制：更正等待/锁定截止和事件账号范围；保留当前角色保护、最后可登录持有人锁、模拟会话关联与归属检查。 | 2026年11月14日 22:36:36 |
| 513 | `src/Modules/Identity/Kmic.Identity.Application/Users/Create/CreateUserCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 514 | `src/Modules/Identity/Kmic.Identity.Application/Users/Disable/DisableUserCommand.cs` | 保留 | 核对安全事件来源、用户详情投影及授予限制：更正等待/锁定截止和事件账号范围；保留当前角色保护、最后可登录持有人锁、模拟会话关联与归属检查。 | 2026年11月14日 22:36:36 |
| 515 | `src/Modules/Identity/Kmic.Identity.Application/Users/Disable/DisableUserCommandHandler.cs` | 保留 | 核对安全事件来源、用户详情投影及授予限制：更正等待/锁定截止和事件账号范围；保留当前角色保护、最后可登录持有人锁、模拟会话关联与归属检查。 | 2026年11月14日 22:36:36 |
| 516 | `src/Modules/Identity/Kmic.Identity.Application/Users/Enable/EnableUserCommand.cs` | 保留 | 核对安全事件来源、用户详情投影及授予限制：更正等待/锁定截止和事件账号范围；保留当前角色保护、最后可登录持有人锁、模拟会话关联与归属检查。 | 2026年11月14日 22:36:36 |
| 517 | `src/Modules/Identity/Kmic.Identity.Application/Users/Enable/EnableUserCommandHandler.cs` | 保留 | 核对安全事件来源、用户详情投影及授予限制：更正等待/锁定截止和事件账号范围；保留当前角色保护、最后可登录持有人锁、模拟会话关联与归属检查。 | 2026年11月14日 22:36:36 |
| 518 | `src/Modules/Identity/Kmic.Identity.Application/Users/EventHandlers/RevokeSessionsWhenPasswordChanged.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 519 | `src/Modules/Identity/Kmic.Identity.Application/Users/EventHandlers/RevokeSessionsWhenUserDisabled.cs` | 保留 | 核对安全事件来源、用户详情投影及授予限制：更正等待/锁定截止和事件账号范围；保留当前角色保护、最后可登录持有人锁、模拟会话关联与归属检查。 | 2026年11月14日 22:36:37 |
| 520 | `src/Modules/Identity/Kmic.Identity.Application/Users/ForceLogout/ForceLogoutCommand.cs` | 保留 | 核对安全事件来源、用户详情投影及授予限制：更正等待/锁定截止和事件账号范围；保留当前角色保护、最后可登录持有人锁、模拟会话关联与归属检查。 | 2026年11月14日 22:36:37 |
| 521 | `src/Modules/Identity/Kmic.Identity.Application/Users/ForceLogout/ForceLogoutCommandHandler.cs` | 保留 | 核对安全事件来源、用户详情投影及授予限制：更正等待/锁定截止和事件账号范围；保留当前角色保护、最后可登录持有人锁、模拟会话关联与归属检查。 | 2026年11月14日 22:36:37 |
| 522 | `src/Modules/Identity/Kmic.Identity.Application/Users/Get/GetUserQuery.cs` | 保留 | 核对安全事件来源、用户详情投影及授予限制：更正等待/锁定截止和事件账号范围；保留当前角色保护、最后可登录持有人锁、模拟会话关联与归属检查。 | 2026年11月14日 22:36:37 |
| 523 | `src/Modules/Identity/Kmic.Identity.Application/Users/Get/GetUserQueryHandler.cs` | 保留 | 核对安全事件来源、用户详情投影及授予限制：更正等待/锁定截止和事件账号范围；保留当前角色保护、最后可登录持有人锁、模拟会话关联与归属检查。 | 2026年11月14日 22:36:37 |
| 524 | `src/Modules/Identity/Kmic.Identity.Application/Users/Get/UserAnchorItem.cs` | 保留 | 核对安全事件来源、用户详情投影及授予限制：更正等待/锁定截止和事件账号范围；保留当前角色保护、最后可登录持有人锁、模拟会话关联与归属检查。 | 2026年11月14日 22:36:37 |
| 525 | `src/Modules/Identity/Kmic.Identity.Application/Users/Get/UserDetailResponse.cs` | 已修订 | 核对安全事件来源、用户详情投影及授予限制：更正等待/锁定截止和事件账号范围；保留当前角色保护、最后可登录持有人锁、模拟会话关联与归属检查。；本阶段 C2246 | 2026年11月14日 22:36:38 |
| 526 | `src/Modules/Identity/Kmic.Identity.Application/Users/Get/UserGrantItem.cs` | 保留 | 核对安全事件来源、用户详情投影及授予限制：更正等待/锁定截止和事件账号范围；保留当前角色保护、最后可登录持有人锁、模拟会话关联与归属检查。 | 2026年11月14日 22:36:38 |
| 527 | `src/Modules/Identity/Kmic.Identity.Application/Users/Grants/GrantAction.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 528 | `src/Modules/Identity/Kmic.Identity.Application/Users/Grants/GrantContext.cs` | 保留 | 核对安全事件来源、用户详情投影及授予限制：更正等待/锁定截止和事件账号范围；保留当前角色保护、最后可登录持有人锁、模拟会话关联与归属检查。 | 2026年11月14日 22:36:38 |
| 529 | `src/Modules/Identity/Kmic.Identity.Application/Users/Grants/GrantLoading.cs` | 保留 | 核对安全事件来源、用户详情投影及授予限制：更正等待/锁定截止和事件账号范围；保留当前角色保护、最后可登录持有人锁、模拟会话关联与归属检查。 | 2026年11月14日 22:36:38 |
| 530 | `src/Modules/Identity/Kmic.Identity.Application/Users/Grants/GrantPolicy.cs` | 保留 | 核对安全事件来源、用户详情投影及授予限制：更正等待/锁定截止和事件账号范围；保留当前角色保护、最后可登录持有人锁、模拟会话关联与归属检查。 | 2026年11月14日 22:36:38 |
| 531 | `src/Modules/Identity/Kmic.Identity.Application/Users/Grants/GrantRole/GrantRoleCommand.cs` | 保留 | 核对安全事件来源、用户详情投影及授予限制：更正等待/锁定截止和事件账号范围；保留当前角色保护、最后可登录持有人锁、模拟会话关联与归属检查。 | 2026年11月14日 22:36:39 |
| 532 | `src/Modules/Identity/Kmic.Identity.Application/Users/Grants/GrantRole/GrantRoleCommandHandler.cs` | 保留 | 核对安全事件来源、用户详情投影及授予限制：更正等待/锁定截止和事件账号范围；保留当前角色保护、最后可登录持有人锁、模拟会话关联与归属检查。 | 2026年11月14日 22:36:39 |
| 533 | `src/Modules/Identity/Kmic.Identity.Application/Users/Grants/GrantRole/GrantRoleCommandValidator.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 534 | `src/Modules/Identity/Kmic.Identity.Application/Users/Grants/GrantTarget.cs` | 保留 | 核对角色/锚点变更、用户查询分页、密码重置和会话查询：保留末位持有人保护、锚点启用要求、临时密码不入日志/幂等、会话包含模拟关系和资料改凭据需另授权；普通文案清楚。 | 2026年11月14日 22:36:53 |
| 535 | `src/Modules/Identity/Kmic.Identity.Application/Users/Grants/OrgAnchors.cs` | 保留 | 核对角色/锚点变更、用户查询分页、密码重置和会话查询：保留末位持有人保护、锚点启用要求、临时密码不入日志/幂等、会话包含模拟关系和资料改凭据需另授权；普通文案清楚。 | 2026年11月14日 22:36:53 |
| 536 | `src/Modules/Identity/Kmic.Identity.Application/Users/Grants/RevokeRole/RevokeRoleCommand.cs` | 保留 | 核对角色/锚点变更、用户查询分页、密码重置和会话查询：保留末位持有人保护、锚点启用要求、临时密码不入日志/幂等、会话包含模拟关系和资料改凭据需另授权；普通文案清楚。 | 2026年11月14日 22:36:53 |
| 537 | `src/Modules/Identity/Kmic.Identity.Application/Users/Grants/RevokeRole/RevokeRoleCommandHandler.cs` | 保留 | 核对角色/锚点变更、用户查询分页、密码重置和会话查询：保留末位持有人保护、锚点启用要求、临时密码不入日志/幂等、会话包含模拟关系和资料改凭据需另授权；普通文案清楚。 | 2026年11月14日 22:36:53 |
| 538 | `src/Modules/Identity/Kmic.Identity.Application/Users/Grants/SetAnchors/SetGrantAnchorsCommand.cs` | 保留 | 核对角色/锚点变更、用户查询分页、密码重置和会话查询：保留末位持有人保护、锚点启用要求、临时密码不入日志/幂等、会话包含模拟关系和资料改凭据需另授权；普通文案清楚。 | 2026年11月14日 22:36:54 |
| 539 | `src/Modules/Identity/Kmic.Identity.Application/Users/Grants/SetAnchors/SetGrantAnchorsCommandHandler.cs` | 保留 | 核对角色/锚点变更、用户查询分页、密码重置和会话查询：保留末位持有人保护、锚点启用要求、临时密码不入日志/幂等、会话包含模拟关系和资料改凭据需另授权；普通文案清楚。 | 2026年11月14日 22:36:54 |
| 540 | `src/Modules/Identity/Kmic.Identity.Application/Users/Grants/SetAnchors/SetGrantAnchorsCommandValidator.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 541 | `src/Modules/Identity/Kmic.Identity.Application/Users/List/ListUsersQuery.cs` | 保留 | 核对角色/锚点变更、用户查询分页、密码重置和会话查询：保留末位持有人保护、锚点启用要求、临时密码不入日志/幂等、会话包含模拟关系和资料改凭据需另授权；普通文案清楚。 | 2026年11月14日 22:36:54 |
| 542 | `src/Modules/Identity/Kmic.Identity.Application/Users/List/ListUsersQueryHandler.cs` | 保留 | 核对角色/锚点变更、用户查询分页、密码重置和会话查询：保留末位持有人保护、锚点启用要求、临时密码不入日志/幂等、会话包含模拟关系和资料改凭据需另授权；普通文案清楚。 | 2026年11月14日 22:36:54 |
| 543 | `src/Modules/Identity/Kmic.Identity.Application/Users/List/UserListItem.cs` | 保留 | 核对角色/锚点变更、用户查询分页、密码重置和会话查询：保留末位持有人保护、锚点启用要求、临时密码不入日志/幂等、会话包含模拟关系和资料改凭据需另授权；普通文案清楚。 | 2026年11月14日 22:36:54 |
| 544 | `src/Modules/Identity/Kmic.Identity.Application/Users/List/UserRoleBrief.cs` | 保留 | 核对角色/锚点变更、用户查询分页、密码重置和会话查询：保留末位持有人保护、锚点启用要求、临时密码不入日志/幂等、会话包含模拟关系和资料改凭据需另授权；普通文案清楚。 | 2026年11月14日 22:36:54 |
| 545 | `src/Modules/Identity/Kmic.Identity.Application/Users/List/UserSort.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 546 | `src/Modules/Identity/Kmic.Identity.Application/Users/Options/SearchUserOptionsQuery.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 547 | `src/Modules/Identity/Kmic.Identity.Application/Users/Options/SearchUserOptionsQueryHandler.cs` | 保留 | 核对角色/锚点变更、用户查询分页、密码重置和会话查询：保留末位持有人保护、锚点启用要求、临时密码不入日志/幂等、会话包含模拟关系和资料改凭据需另授权；普通文案清楚。 | 2026年11月14日 22:36:54 |
| 548 | `src/Modules/Identity/Kmic.Identity.Application/Users/Options/UserOption.cs` | 保留 | 核对角色/锚点变更、用户查询分页、密码重置和会话查询：保留末位持有人保护、锚点启用要求、临时密码不入日志/幂等、会话包含模拟关系和资料改凭据需另授权；普通文案清楚。 | 2026年11月14日 22:36:54 |
| 549 | `src/Modules/Identity/Kmic.Identity.Application/Users/Options/UserOptionSort.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 550 | `src/Modules/Identity/Kmic.Identity.Application/Users/ResetPassword/ResetUserPasswordCommand.cs` | 保留 | 核对角色/锚点变更、用户查询分页、密码重置和会话查询：保留末位持有人保护、锚点启用要求、临时密码不入日志/幂等、会话包含模拟关系和资料改凭据需另授权；普通文案清楚。 | 2026年11月14日 22:36:55 |
| 551 | `src/Modules/Identity/Kmic.Identity.Application/Users/ResetPassword/ResetUserPasswordCommandHandler.cs` | 保留 | 核对角色/锚点变更、用户查询分页、密码重置和会话查询：保留末位持有人保护、锚点启用要求、临时密码不入日志/幂等、会话包含模拟关系和资料改凭据需另授权；普通文案清楚。 | 2026年11月14日 22:36:55 |
| 552 | `src/Modules/Identity/Kmic.Identity.Application/Users/ResetPassword/ResetUserPasswordResponse.cs` | 保留 | 核对角色/锚点变更、用户查询分页、密码重置和会话查询：保留末位持有人保护、锚点启用要求、临时密码不入日志/幂等、会话包含模拟关系和资料改凭据需另授权；普通文案清楚。 | 2026年11月14日 22:36:55 |
| 553 | `src/Modules/Identity/Kmic.Identity.Application/Users/RoleName.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 554 | `src/Modules/Identity/Kmic.Identity.Application/Users/Sessions/ListUserSessionsQuery.cs` | 保留 | 核对角色/锚点变更、用户查询分页、密码重置和会话查询：保留末位持有人保护、锚点启用要求、临时密码不入日志/幂等、会话包含模拟关系和资料改凭据需另授权；普通文案清楚。 | 2026年11月14日 22:36:55 |
| 555 | `src/Modules/Identity/Kmic.Identity.Application/Users/Sessions/ListUserSessionsQueryHandler.cs` | 保留 | 核对角色/锚点变更、用户查询分页、密码重置和会话查询：保留末位持有人保护、锚点启用要求、临时密码不入日志/幂等、会话包含模拟关系和资料改凭据需另授权；普通文案清楚。 | 2026年11月14日 22:36:55 |
| 556 | `src/Modules/Identity/Kmic.Identity.Application/Users/Sessions/UserSessionItem.cs` | 已修订 | 完成跨文件一致性复查后重新核对本轮最终内容：字典/组织移动仅在父项变化时重排；取消写所属模块；投递上限仅在暂时失败时判断；事件账号与模拟会话时间说明均与实现一致。；本阶段 C2253 | 2026年11月14日 22:41:41 |
| 557 | `src/Modules/Identity/Kmic.Identity.Application/Users/UnlockLogin/UnlockLoginCommand.cs` | 保留 | 核对角色/锚点变更、用户查询分页、密码重置和会话查询：保留末位持有人保护、锚点启用要求、临时密码不入日志/幂等、会话包含模拟关系和资料改凭据需另授权；普通文案清楚。 | 2026年11月14日 22:36:55 |
| 558 | `src/Modules/Identity/Kmic.Identity.Application/Users/UnlockLogin/UnlockLoginCommandHandler.cs` | 保留 | 核对角色/锚点变更、用户查询分页、密码重置和会话查询：保留末位持有人保护、锚点启用要求、临时密码不入日志/幂等、会话包含模拟关系和资料改凭据需另授权；普通文案清楚。 | 2026年11月14日 22:36:55 |
| 559 | `src/Modules/Identity/Kmic.Identity.Application/Users/UpdateProfile/UpdateUserProfileCommand.cs` | 沿用已复核 | 上轮已完整读取关联文件并核实修改，本轮内容未变；原编号 C2133。 | 2026年11月14日 22:12:28 |
| 560 | `src/Modules/Identity/Kmic.Identity.Application/Users/UpdateProfile/UpdateUserProfileCommandHandler.cs` | 保留 | 核对角色/锚点变更、用户查询分页、密码重置和会话查询：保留末位持有人保护、锚点启用要求、临时密码不入日志/幂等、会话包含模拟关系和资料改凭据需另授权；普通文案清楚。 | 2026年11月14日 22:36:55 |
| 561 | `src/Modules/Identity/Kmic.Identity.Application/Users/UpdateProfile/UpdateUserProfileCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 562 | `src/Modules/Identity/Kmic.Identity.Application/Users/UserActions.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 563 | `src/Modules/Identity/Kmic.Identity.Application/Users/UserAdminAction.cs` | 保留 | 完整核对用户管理策略、引用契约、错误目录和组织/角色/会话聚合：保留授权版本、内置角色、树约束、刷新宽限和独立作废事务；明确模拟会话到期字段。实体属性相关疑点另列。 | 2026年11月14日 22:37:43 |
| 564 | `src/Modules/Identity/Kmic.Identity.Application/Users/UserAdminLoading.cs` | 保留 | 完整核对用户管理策略、引用契约、错误目录和组织/角色/会话聚合：保留授权版本、内置角色、树约束、刷新宽限和独立作废事务；明确模拟会话到期字段。实体属性相关疑点另列。 | 2026年11月14日 22:37:43 |
| 565 | `src/Modules/Identity/Kmic.Identity.Application/Users/UserNames.cs` | 保留 | 完整核对用户管理策略、引用契约、错误目录和组织/角色/会话聚合：保留授权版本、内置角色、树约束、刷新宽限和独立作废事务；明确模拟会话到期字段。实体属性相关疑点另列。 | 2026年11月14日 22:37:43 |
| 566 | `src/Modules/Identity/Kmic.Identity.Application/Users/UserPolicy.cs` | 保留 | 完整核对用户管理策略、引用契约、错误目录和组织/角色/会话聚合：保留授权版本、内置角色、树约束、刷新宽限和独立作废事务；明确模拟会话到期字段。实体属性相关疑点另列。 | 2026年11月14日 22:37:44 |
| 567 | `src/Modules/Identity/Kmic.Identity.Application/Users/UserTarget.cs` | 保留 | 完整核对用户管理策略、引用契约、错误目录和组织/角色/会话聚合：保留授权版本、内置角色、树约束、刷新宽限和独立作废事务；明确模拟会话到期字段。实体属性相关疑点另列。 | 2026年11月14日 22:37:44 |
| 568 | `src/Modules/Identity/Kmic.Identity.Application/Users/UserTargets.cs` | 保留 | 完整核对用户管理策略、引用契约、错误目录和组织/角色/会话聚合：保留授权版本、内置角色、树约束、刷新宽限和独立作废事务；明确模拟会话到期字段。实体属性相关疑点另列。 | 2026年11月14日 22:37:44 |
| 569 | `src/Modules/Identity/Kmic.Identity.Application/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 570 | `src/Modules/Identity/Kmic.Identity.Contracts/IOrgUnitReferences.cs` | 保留 | 完整核对用户管理策略、引用契约、错误目录和组织/角色/会话聚合：保留授权版本、内置角色、树约束、刷新宽限和独立作废事务；明确模拟会话到期字段。实体属性相关疑点另列。 | 2026年11月14日 22:37:44 |
| 571 | `src/Modules/Identity/Kmic.Identity.Contracts/IRoleReferences.cs` | 保留 | 完整核对用户管理策略、引用契约、错误目录和组织/角色/会话聚合：保留授权版本、内置角色、树约束、刷新宽限和独立作废事务；明确模拟会话到期字段。实体属性相关疑点另列。 | 2026年11月14日 22:37:44 |
| 572 | `src/Modules/Identity/Kmic.Identity.Contracts/IUserContacts.cs` | 保留 | 完整核对用户管理策略、引用契约、错误目录和组织/角色/会话聚合：保留授权版本、内置角色、树约束、刷新宽限和独立作废事务；明确模拟会话到期字段。实体属性相关疑点另列。 | 2026年11月14日 22:37:44 |
| 573 | `src/Modules/Identity/Kmic.Identity.Contracts/IUserReferences.cs` | 保留 | 完整核对用户管理策略、引用契约、错误目录和组织/角色/会话聚合：保留授权版本、内置角色、树约束、刷新宽限和独立作废事务；明确模拟会话到期字段。实体属性相关疑点另列。 | 2026年11月14日 22:37:44 |
| 574 | `src/Modules/Identity/Kmic.Identity.Contracts/IdentityModuleInfo.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 575 | `src/Modules/Identity/Kmic.Identity.Contracts/Kmic.Identity.Contracts.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 576 | `src/Modules/Identity/Kmic.Identity.Contracts/UserContact.cs` | 保留 | 完整核对用户管理策略、引用契约、错误目录和组织/角色/会话聚合：保留授权版本、内置角色、树约束、刷新宽限和独立作废事务；明确模拟会话到期字段。实体属性相关疑点另列。 | 2026年11月14日 22:37:44 |
| 577 | `src/Modules/Identity/Kmic.Identity.Contracts/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 578 | `src/Modules/Identity/Kmic.Identity.Domain/Errors/IdentityErrors.cs` | 保留 | 完整核对用户管理策略、引用契约、错误目录和组织/角色/会话聚合：保留授权版本、内置角色、树约束、刷新宽限和独立作废事务；明确模拟会话到期字段。实体属性相关疑点另列。 | 2026年11月14日 22:37:45 |
| 579 | `src/Modules/Identity/Kmic.Identity.Domain/Kmic.Identity.Domain.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 580 | `src/Modules/Identity/Kmic.Identity.Domain/Organization/IOrgTreeRepository.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 581 | `src/Modules/Identity/Kmic.Identity.Domain/Organization/OrgTree.cs` | 已修订 | 完整核对用户管理策略、引用契约、错误目录和组织/角色/会话聚合：保留授权版本、内置角色、树约束、刷新宽限和独立作废事务；明确模拟会话到期字段。实体属性相关疑点另列。；本阶段 C2254 | 2026年11月14日 22:37:45 |
| 582 | `src/Modules/Identity/Kmic.Identity.Domain/Organization/OrgTreeId.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 583 | `src/Modules/Identity/Kmic.Identity.Domain/Organization/OrgUnit.cs` | 保留 | 完整核对用户管理策略、引用契约、错误目录和组织/角色/会话聚合：保留授权版本、内置角色、树约束、刷新宽限和独立作废事务；明确模拟会话到期字段。实体属性相关疑点另列。 | 2026年11月14日 22:37:45 |
| 584 | `src/Modules/Identity/Kmic.Identity.Domain/Organization/OrgUnitStatus.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 585 | `src/Modules/Identity/Kmic.Identity.Domain/Roles/IRoleRepository.cs` | 保留 | 完整核对用户管理策略、引用契约、错误目录和组织/角色/会话聚合：保留授权版本、内置角色、树约束、刷新宽限和独立作废事务；明确模拟会话到期字段。实体属性相关疑点另列。 | 2026年11月14日 22:37:45 |
| 586 | `src/Modules/Identity/Kmic.Identity.Domain/Roles/Role.cs` | 保留 | 完整核对用户管理策略、引用契约、错误目录和组织/角色/会话聚合：保留授权版本、内置角色、树约束、刷新宽限和独立作废事务；明确模拟会话到期字段。实体属性相关疑点另列。 | 2026年11月14日 22:37:46 |
| 587 | `src/Modules/Identity/Kmic.Identity.Domain/Roles/RoleAction.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 588 | `src/Modules/Identity/Kmic.Identity.Domain/Roles/RoleDataScope.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 589 | `src/Modules/Identity/Kmic.Identity.Domain/Roles/RoleDataScopeId.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 590 | `src/Modules/Identity/Kmic.Identity.Domain/Roles/RolePermissionEntry.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 591 | `src/Modules/Identity/Kmic.Identity.Domain/Roles/RolePermissionEntryId.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 592 | `src/Modules/Identity/Kmic.Identity.Domain/Roles/RoleScopeLevel.cs` | 保留 | 完整核对用户管理策略、引用契约、错误目录和组织/角色/会话聚合：保留授权版本、内置角色、树约束、刷新宽限和独立作废事务；明确模拟会话到期字段。实体属性相关疑点另列。 | 2026年11月14日 22:37:46 |
| 593 | `src/Modules/Identity/Kmic.Identity.Domain/Roles/RoleStatus.cs` | 保留 | 完整核对用户管理策略、引用契约、错误目录和组织/角色/会话聚合：保留授权版本、内置角色、树约束、刷新宽限和独立作废事务；明确模拟会话到期字段。实体属性相关疑点另列。 | 2026年11月14日 22:37:46 |
| 594 | `src/Modules/Identity/Kmic.Identity.Domain/Sessions/ClientType.cs` | 保留 | 完整核对用户管理策略、引用契约、错误目录和组织/角色/会话聚合：保留授权版本、内置角色、树约束、刷新宽限和独立作废事务；明确模拟会话到期字段。实体属性相关疑点另列。 | 2026年11月14日 22:37:46 |
| 595 | `src/Modules/Identity/Kmic.Identity.Domain/Sessions/ISessionRepository.cs` | 保留 | 完整核对用户管理策略、引用契约、错误目录和组织/角色/会话聚合：保留授权版本、内置角色、树约束、刷新宽限和独立作废事务；明确模拟会话到期字段。实体属性相关疑点另列。 | 2026年11月14日 22:37:46 |
| 596 | `src/Modules/Identity/Kmic.Identity.Domain/Sessions/RefreshToken.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 597 | `src/Modules/Identity/Kmic.Identity.Domain/Sessions/RefreshTokenId.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 598 | `src/Modules/Identity/Kmic.Identity.Domain/Sessions/RevokeReason.cs` | 保留 | 完整核对用户管理策略、引用契约、错误目录和组织/角色/会话聚合：保留授权版本、内置角色、树约束、刷新宽限和独立作废事务；明确模拟会话到期字段。实体属性相关疑点另列。 | 2026年11月14日 22:37:47 |
| 599 | `src/Modules/Identity/Kmic.Identity.Domain/Sessions/Rotation.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 600 | `src/Modules/Identity/Kmic.Identity.Domain/Sessions/RotationOutcome.cs` | 保留 | 完整核对用户管理策略、引用契约、错误目录和组织/角色/会话聚合：保留授权版本、内置角色、树约束、刷新宽限和独立作废事务；明确模拟会话到期字段。实体属性相关疑点另列。 | 2026年11月14日 22:37:47 |
| 601 | `src/Modules/Identity/Kmic.Identity.Domain/Sessions/Session.cs` | 已修订（模型中文） | 本轮 C2275～C2276；先核对实际行为，再精简或纠正摘要/模型说明。已同步注释迁移与快照，运行模型及 SQL 仅注释变化；测试未运行 | 2026年11月15日 00:04:56 |
| 602 | `src/Modules/Identity/Kmic.Identity.Domain/Sessions/SessionStatus.cs` | 保留 | 完整核对用户管理策略、引用契约、错误目录和组织/角色/会话聚合：保留授权版本、内置角色、树约束、刷新宽限和独立作废事务；明确模拟会话到期字段。实体属性相关疑点另列。 | 2026年11月14日 22:37:47 |
| 603 | `src/Modules/Identity/Kmic.Identity.Domain/Users/Events/PasswordChanged.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 604 | `src/Modules/Identity/Kmic.Identity.Domain/Users/Events/UserDisabled.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 605 | `src/Modules/Identity/Kmic.Identity.Domain/Users/GrantAnchor.cs` | 保留 | 完整核对用户聚合、访问上下文和目录/登录限制存储：保留事件与改密差异、逐请求状态、缓存版本校正、全权限角色、独立限流事务和首次失败并发保护；快照文件只读保留。 | 2026年11月14日 22:38:40 |
| 606 | `src/Modules/Identity/Kmic.Identity.Domain/Users/GrantAnchorId.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 607 | `src/Modules/Identity/Kmic.Identity.Domain/Users/GrantId.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 608 | `src/Modules/Identity/Kmic.Identity.Domain/Users/IUserRepository.cs` | 保留 | 完整核对用户聚合、访问上下文和目录/登录限制存储：保留事件与改密差异、逐请求状态、缓存版本校正、全权限角色、独立限流事务和首次失败并发保护；快照文件只读保留。 | 2026年11月14日 22:38:40 |
| 609 | `src/Modules/Identity/Kmic.Identity.Domain/Users/MobileNumbers.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 610 | `src/Modules/Identity/Kmic.Identity.Domain/Users/ProtectedHolders.cs` | 保留 | 完整核对用户聚合、访问上下文和目录/登录限制存储：保留事件与改密差异、逐请求状态、缓存版本校正、全权限角色、独立限流事务和首次失败并发保护；快照文件只读保留。 | 2026年11月14日 22:38:40 |
| 611 | `src/Modules/Identity/Kmic.Identity.Domain/Users/User.cs` | 已修订（模型中文） | 本轮 C2270～C2274；先核对实际行为，再精简或纠正摘要/模型说明。已同步注释迁移与快照，运行模型及 SQL 仅注释变化；测试未运行 | 2026年11月15日 00:04:56 |
| 612 | `src/Modules/Identity/Kmic.Identity.Domain/Users/UserAction.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 613 | `src/Modules/Identity/Kmic.Identity.Domain/Users/UserFields.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 614 | `src/Modules/Identity/Kmic.Identity.Domain/Users/UserKind.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 615 | `src/Modules/Identity/Kmic.Identity.Domain/Users/UserRoleGrant.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 616 | `src/Modules/Identity/Kmic.Identity.Domain/Users/UserStatus.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 617 | `src/Modules/Identity/Kmic.Identity.Domain/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 618 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Access/AccessContext.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 619 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Access/AccessContextLoader.cs` | 已修订 | 完整核对用户聚合、访问上下文和目录/登录限制存储：保留事件与改密差异、逐请求状态、缓存版本校正、全权限角色、独立限流事务和首次失败并发保护；快照文件只读保留。；本阶段 C2256 | 2026年11月14日 22:38:40 |
| 620 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Access/AccessStateRow.cs` | 保留 | 完整核对用户聚合、访问上下文和目录/登录限制存储：保留事件与改密差异、逐请求状态、缓存版本校正、全权限角色、独立限流事务和首次失败并发保护；快照文件只读保留。 | 2026年11月14日 22:38:40 |
| 621 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Access/IdentityCacheKeys.cs` | 保留 | 完整核对用户聚合、访问上下文和目录/登录限制存储：保留事件与改密差异、逐请求状态、缓存版本校正、全权限角色、独立限流事务和首次失败并发保护；快照文件只读保留。 | 2026年11月14日 22:38:41 |
| 622 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Access/OrgNode.cs` | 保留 | 完整核对用户聚合、访问上下文和目录/登录限制存储：保留事件与改密差异、逐请求状态、缓存版本校正、全权限角色、独立限流事务和首次失败并发保护；快照文件只读保留。 | 2026年11月14日 22:38:41 |
| 623 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Access/OrgTreeSnapshot.cs` | 保留 | 完整核对用户聚合、访问上下文和目录/登录限制存储：保留事件与改密差异、逐请求状态、缓存版本校正、全权限角色、独立限流事务和首次失败并发保护；快照文件只读保留。 | 2026年11月14日 22:38:41 |
| 624 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Access/PermissionCatalogStore.cs` | 保留 | 完整核对用户聚合、访问上下文和目录/登录限制存储：保留事件与改密差异、逐请求状态、缓存版本校正、全权限角色、独立限流事务和首次失败并发保护；快照文件只读保留。 | 2026年11月14日 22:38:41 |
| 625 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Access/RoleScopeLevelExtensions.cs` | 保留 | 完整核对用户聚合、访问上下文和目录/登录限制存储：保留事件与改密差异、逐请求状态、缓存版本校正、全权限角色、独立限流事务和首次失败并发保护；快照文件只读保留。 | 2026年11月14日 22:38:41 |
| 626 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Access/RoleSnapshot.cs` | 保留 | 完整核对用户聚合、访问上下文和目录/登录限制存储：保留事件与改密差异、逐请求状态、缓存版本校正、全权限角色、独立限流事务和首次失败并发保护；快照文件只读保留。 | 2026年11月14日 22:38:41 |
| 627 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Access/SyncPermissionCatalogTask.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 628 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Authentication/EmbeddedWeakPasswordList.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 629 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Authentication/LoginThrottleStore.cs` | 保留 | 完整核对用户聚合、访问上下文和目录/登录限制存储：保留事件与改密差异、逐请求状态、缓存版本校正、全权限角色、独立限流事务和首次失败并发保护；快照文件只读保留。 | 2026年11月14日 22:38:41 |
| 630 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Authentication/PasswordService.cs` | 保留 | 完整核对用户聚合、访问上下文和目录/登录限制存储：保留事件与改密差异、逐请求状态、缓存版本校正、全权限角色、独立限流事务和首次失败并发保护；快照文件只读保留。 | 2026年11月14日 22:38:41 |
| 631 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Authentication/weak-passwords.txt` | 保留 | 核对身份模块注册、登录限制记录和认证端点：保留服务生命周期原因、敏感属性审计处理、查询侧不跟踪、Cookie 校验及当前角色不可用仍可查询自身的约定；弱密码清单仅检查说明，不改数据。 | 2026年11月14日 22:39:00 |
| 632 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Authorization/OperatorRecheck.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 633 | `src/Modules/Identity/Kmic.Identity.Infrastructure/IdentityModule.cs` | 保留 | 核对身份模块注册、登录限制记录和认证端点：保留服务生命周期原因、敏感属性审计处理、查询侧不跟踪、Cookie 校验及当前角色不可用仍可查询自身的约定；弱密码清单仅检查说明，不改数据。 | 2026年11月14日 22:39:00 |
| 634 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Kmic.Identity.Infrastructure.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 635 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Operations/IdentityBootstrapper.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:04 |
| 636 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Configurations/CatalogStateRowConfiguration.cs` | 沿用已复核 | 沿用上轮逐文件结论（模型说明保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 637 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Configurations/DataScopeObjectRowConfiguration.cs` | 沿用已复核 | 沿用上轮逐文件结论（模型说明保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 638 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Configurations/GrantAnchorConfiguration.cs` | 沿用已复核 | 沿用上轮逐文件结论（模型说明保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 639 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Configurations/LoginThrottleRowConfiguration.cs` | 已修订（模型中文） | 本轮 C2294；先核对实际行为，再精简或纠正摘要/模型说明。已同步注释迁移与快照，运行模型及 SQL 仅注释变化；测试未运行 | 2026年11月15日 00:04:56 |
| 640 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Configurations/OrgTreeConfiguration.cs` | 沿用已复核 | 沿用上轮逐文件结论（模型说明保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 641 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Configurations/OrgUnitConfiguration.cs` | 沿用已复核 | 沿用上轮逐文件结论（模型说明保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 642 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Configurations/PermissionRowConfiguration.cs` | 沿用已复核 | 沿用上轮逐文件结论（模型说明保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 643 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs` | 沿用已复核 | 沿用上轮逐文件结论（模型说明保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 644 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Configurations/RoleConfiguration.cs` | 沿用已复核 | 沿用上轮逐文件结论（模型说明保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 645 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Configurations/RoleDataScopeConfiguration.cs` | 沿用已复核 | 沿用上轮逐文件结论（模型说明保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 646 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Configurations/RolePermissionEntryConfiguration.cs` | 沿用已复核 | 沿用上轮逐文件结论（模型说明保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 647 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Configurations/SessionConfiguration.cs` | 已修订（模型中文） | 本轮 C2293；先核对实际行为，再精简或纠正摘要/模型说明。已同步注释迁移与快照，运行模型及 SQL 仅注释变化；测试未运行 | 2026年11月15日 00:04:56 |
| 648 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Configurations/UserConfiguration.cs` | 保留 | 核对身份模块注册、登录限制记录和认证端点：保留服务生命周期原因、敏感属性审计处理、查询侧不跟踪、Cookie 校验及当前角色不可用仍可查询自身的约定；弱密码清单仅检查说明，不改数据。 | 2026年11月14日 22:39:00 |
| 649 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Configurations/UserRoleGrantConfiguration.cs` | 沿用已复核 | 沿用上轮逐文件结论（模型说明保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 650 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Configurations/VerificationCodeRowConfiguration.cs` | 已修订（模型中文） | 本轮 C2295；先核对实际行为，再精简或纠正摘要/模型说明。已同步注释迁移与快照，运行模型及 SQL 仅注释变化；测试未运行 | 2026年11月15日 00:04:56 |
| 651 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/IdentityCommandReads.cs` | 保留 | 核对身份模块注册、登录限制记录和认证端点：保留服务生命周期原因、敏感属性审计处理、查询侧不跟踪、Cookie 校验及当前角色不可用仍可查询自身的约定；弱密码清单仅检查说明，不改数据。 | 2026年11月14日 22:39:00 |
| 652 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/IdentityDbContext.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 653 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/IdentityDbContextFactory.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 654 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/IdentityIdConverters.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 655 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/IdentityModel.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 656 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/IdentityQueryDbContext.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 657 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Records/CatalogStateRow.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 658 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Records/DataScopeObjectRow.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 659 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Records/LoginThrottleRow.cs` | 保留 | 核对身份模块注册、登录限制记录和认证端点：保留服务生命周期原因、敏感属性审计处理、查询侧不跟踪、Cookie 校验及当前角色不可用仍可查询自身的约定；弱密码清单仅检查说明，不改数据。 | 2026年11月14日 22:39:00 |
| 660 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Records/PermissionRow.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 661 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Records/VerificationCodeRow.cs` | 保留 | 核对身份模块注册、登录限制记录和认证端点：保留服务生命周期原因、敏感属性审计处理、查询侧不跟踪、Cookie 校验及当前角色不可用仍可查询自身的约定；弱密码清单仅检查说明，不改数据。 | 2026年11月14日 22:39:01 |
| 662 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Repositories/OrgTreeRepository.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 663 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Repositories/RoleRepository.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 664 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Repositories/SessionRepository.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 665 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Repositories/UserRepository.cs` | 沿用已复核 | 上轮已完整读取关联文件并核实修改，本轮内容未变；原编号 C2148。 | 2026年11月14日 22:12:28 |
| 666 | `src/Modules/Identity/Kmic.Identity.Infrastructure/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 667 | `src/Modules/Identity/Kmic.Identity.Presentation/Authentication/ChangePasswordEndpoint.cs` | 保留 | 核对身份模块注册、登录限制记录和认证端点：保留服务生命周期原因、敏感属性审计处理、查询侧不跟踪、Cookie 校验及当前角色不可用仍可查询自身的约定；弱密码清单仅检查说明，不改数据。 | 2026年11月14日 22:39:01 |
| 668 | `src/Modules/Identity/Kmic.Identity.Presentation/Authentication/ChangePasswordRequest.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 669 | `src/Modules/Identity/Kmic.Identity.Presentation/Authentication/CookieEndpointGuard.cs` | 保留 | 核对身份模块注册、登录限制记录和认证端点：保留服务生命周期原因、敏感属性审计处理、查询侧不跟踪、Cookie 校验及当前角色不可用仍可查询自身的约定；弱密码清单仅检查说明，不改数据。 | 2026年11月14日 22:39:01 |
| 670 | `src/Modules/Identity/Kmic.Identity.Presentation/Authentication/GetMeEndpoint.cs` | 保留 | 核对身份模块注册、登录限制记录和认证端点：保留服务生命周期原因、敏感属性审计处理、查询侧不跟踪、Cookie 校验及当前角色不可用仍可查询自身的约定；弱密码清单仅检查说明，不改数据。 | 2026年11月14日 22:39:01 |
| 671 | `src/Modules/Identity/Kmic.Identity.Presentation/Authentication/ListMySessionsEndpoint.cs` | 保留 | 核对身份模块注册、登录限制记录和认证端点：保留服务生命周期原因、敏感属性审计处理、查询侧不跟踪、Cookie 校验及当前角色不可用仍可查询自身的约定；弱密码清单仅检查说明，不改数据。 | 2026年11月14日 22:39:01 |
| 672 | `src/Modules/Identity/Kmic.Identity.Presentation/Authentication/LoginEndpoint.cs` | 保留 | 核对认证、模拟身份与组织接口：保留 Cookie/响应体区别、可信来源、已撤销令牌不可再用、角色不可用的例外、模拟写入和根/循环限制；修正上级未变时不会重排。 | 2026年11月14日 22:39:29 |
| 673 | `src/Modules/Identity/Kmic.Identity.Presentation/Authentication/LoginRequest.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 674 | `src/Modules/Identity/Kmic.Identity.Presentation/Authentication/LogoutEndpoint.cs` | 保留 | 核对认证、模拟身份与组织接口：保留 Cookie/响应体区别、可信来源、已撤销令牌不可再用、角色不可用的例外、模拟写入和根/循环限制；修正上级未变时不会重排。 | 2026年11月14日 22:39:29 |
| 675 | `src/Modules/Identity/Kmic.Identity.Presentation/Authentication/RefreshCookie.cs` | 保留 | 核对认证、模拟身份与组织接口：保留 Cookie/响应体区别、可信来源、已撤销令牌不可再用、角色不可用的例外、模拟写入和根/循环限制；修正上级未变时不会重排。 | 2026年11月14日 22:39:29 |
| 676 | `src/Modules/Identity/Kmic.Identity.Presentation/Authentication/RefreshEndpoint.cs` | 保留 | 核对认证、模拟身份与组织接口：保留 Cookie/响应体区别、可信来源、已撤销令牌不可再用、角色不可用的例外、模拟写入和根/循环限制；修正上级未变时不会重排。 | 2026年11月14日 22:39:29 |
| 677 | `src/Modules/Identity/Kmic.Identity.Presentation/Authentication/RefreshRequest.cs` | 保留 | 核对认证、模拟身份与组织接口：保留 Cookie/响应体区别、可信来源、已撤销令牌不可再用、角色不可用的例外、模拟写入和根/循环限制；修正上级未变时不会重排。 | 2026年11月14日 22:39:29 |
| 678 | `src/Modules/Identity/Kmic.Identity.Presentation/Authentication/RevokeMySessionEndpoint.cs` | 保留 | 核对认证、模拟身份与组织接口：保留 Cookie/响应体区别、可信来源、已撤销令牌不可再用、角色不可用的例外、模拟写入和根/循环限制；修正上级未变时不会重排。 | 2026年11月14日 22:39:30 |
| 679 | `src/Modules/Identity/Kmic.Identity.Presentation/Authentication/SwitchRoleEndpoint.cs` | 保留 | 核对认证、模拟身份与组织接口：保留 Cookie/响应体区别、可信来源、已撤销令牌不可再用、角色不可用的例外、模拟写入和根/循环限制；修正上级未变时不会重排。 | 2026年11月14日 22:39:30 |
| 680 | `src/Modules/Identity/Kmic.Identity.Presentation/Authentication/SwitchRoleRequest.cs` | 保留 | 核对认证、模拟身份与组织接口：保留 Cookie/响应体区别、可信来源、已撤销令牌不可再用、角色不可用的例外、模拟写入和根/循环限制；修正上级未变时不会重排。 | 2026年11月14日 22:39:30 |
| 681 | `src/Modules/Identity/Kmic.Identity.Presentation/IdentityEndpoints.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 682 | `src/Modules/Identity/Kmic.Identity.Presentation/Impersonation/EndImpersonationEndpoint.cs` | 保留 | 核对认证、模拟身份与组织接口：保留 Cookie/响应体区别、可信来源、已撤销令牌不可再用、角色不可用的例外、模拟写入和根/循环限制；修正上级未变时不会重排。 | 2026年11月14日 22:39:30 |
| 683 | `src/Modules/Identity/Kmic.Identity.Presentation/Impersonation/StartImpersonationEndpoint.cs` | 保留 | 核对认证、模拟身份与组织接口：保留 Cookie/响应体区别、可信来源、已撤销令牌不可再用、角色不可用的例外、模拟写入和根/循环限制；修正上级未变时不会重排。 | 2026年11月14日 22:39:30 |
| 684 | `src/Modules/Identity/Kmic.Identity.Presentation/Impersonation/StartImpersonationRequest.cs` | 保留 | 核对认证、模拟身份与组织接口：保留 Cookie/响应体区别、可信来源、已撤销令牌不可再用、角色不可用的例外、模拟写入和根/循环限制；修正上级未变时不会重排。 | 2026年11月14日 22:39:30 |
| 685 | `src/Modules/Identity/Kmic.Identity.Presentation/Kmic.Identity.Presentation.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 686 | `src/Modules/Identity/Kmic.Identity.Presentation/Organization/AssignUserOrgUnitEndpoint.cs` | 保留 | 核对认证、模拟身份与组织接口：保留 Cookie/响应体区别、可信来源、已撤销令牌不可再用、角色不可用的例外、模拟写入和根/循环限制；修正上级未变时不会重排。 | 2026年11月14日 22:39:30 |
| 687 | `src/Modules/Identity/Kmic.Identity.Presentation/Organization/AssignUserOrgUnitRequest.cs` | 保留 | 核对认证、模拟身份与组织接口：保留 Cookie/响应体区别、可信来源、已撤销令牌不可再用、角色不可用的例外、模拟写入和根/循环限制；修正上级未变时不会重排。 | 2026年11月14日 22:39:30 |
| 688 | `src/Modules/Identity/Kmic.Identity.Presentation/Organization/CreateOrgUnitEndpoint.cs` | 保留 | 核对认证、模拟身份与组织接口：保留 Cookie/响应体区别、可信来源、已撤销令牌不可再用、角色不可用的例外、模拟写入和根/循环限制；修正上级未变时不会重排。 | 2026年11月14日 22:39:30 |
| 689 | `src/Modules/Identity/Kmic.Identity.Presentation/Organization/CreateOrgUnitRequest.cs` | 保留 | 核对认证、模拟身份与组织接口：保留 Cookie/响应体区别、可信来源、已撤销令牌不可再用、角色不可用的例外、模拟写入和根/循环限制；修正上级未变时不会重排。 | 2026年11月14日 22:39:31 |
| 690 | `src/Modules/Identity/Kmic.Identity.Presentation/Organization/DisableOrgUnitEndpoint.cs` | 保留 | 核对认证、模拟身份与组织接口：保留 Cookie/响应体区别、可信来源、已撤销令牌不可再用、角色不可用的例外、模拟写入和根/循环限制；修正上级未变时不会重排。 | 2026年11月14日 22:39:31 |
| 691 | `src/Modules/Identity/Kmic.Identity.Presentation/Organization/EnableOrgUnitEndpoint.cs` | 保留 | 核对认证、模拟身份与组织接口：保留 Cookie/响应体区别、可信来源、已撤销令牌不可再用、角色不可用的例外、模拟写入和根/循环限制；修正上级未变时不会重排。 | 2026年11月14日 22:39:31 |
| 692 | `src/Modules/Identity/Kmic.Identity.Presentation/Organization/GetOrgTreeEndpoint.cs` | 保留 | 核对认证、模拟身份与组织接口：保留 Cookie/响应体区别、可信来源、已撤销令牌不可再用、角色不可用的例外、模拟写入和根/循环限制；修正上级未变时不会重排。 | 2026年11月14日 22:39:31 |
| 693 | `src/Modules/Identity/Kmic.Identity.Presentation/Organization/MoveOrgUnitEndpoint.cs` | 已修订 | 核对认证、模拟身份与组织接口：保留 Cookie/响应体区别、可信来源、已撤销令牌不可再用、角色不可用的例外、模拟写入和根/循环限制；修正上级未变时不会重排。；本阶段 C2257 | 2026年11月14日 22:39:31 |
| 694 | `src/Modules/Identity/Kmic.Identity.Presentation/Organization/MoveOrgUnitRequest.cs` | 保留 | 核对组织/角色/权限与安全事件端点及请求定义：版本归属、完整重排列表、目录限制、受保护当前角色、时间范围等有调用价值，保留；短字段说明与实际类型一致。 | 2026年11月14日 22:39:46 |
| 695 | `src/Modules/Identity/Kmic.Identity.Presentation/Organization/OrgUnitVersionRequest.cs` | 保留 | 核对组织/角色/权限与安全事件端点及请求定义：版本归属、完整重排列表、目录限制、受保护当前角色、时间范围等有调用价值，保留；短字段说明与实际类型一致。 | 2026年11月14日 22:39:47 |
| 696 | `src/Modules/Identity/Kmic.Identity.Presentation/Organization/ReorderOrgUnitsEndpoint.cs` | 保留 | 核对组织/角色/权限与安全事件端点及请求定义：版本归属、完整重排列表、目录限制、受保护当前角色、时间范围等有调用价值，保留；短字段说明与实际类型一致。 | 2026年11月14日 22:39:47 |
| 697 | `src/Modules/Identity/Kmic.Identity.Presentation/Organization/ReorderOrgUnitsRequest.cs` | 保留 | 核对组织/角色/权限与安全事件端点及请求定义：版本归属、完整重排列表、目录限制、受保护当前角色、时间范围等有调用价值，保留；短字段说明与实际类型一致。 | 2026年11月14日 22:39:47 |
| 698 | `src/Modules/Identity/Kmic.Identity.Presentation/Organization/UpdateOrgUnitEndpoint.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 699 | `src/Modules/Identity/Kmic.Identity.Presentation/Organization/UpdateOrgUnitRequest.cs` | 保留 | 核对组织/角色/权限与安全事件端点及请求定义：版本归属、完整重排列表、目录限制、受保护当前角色、时间范围等有调用价值，保留；短字段说明与实际类型一致。 | 2026年11月14日 22:39:47 |
| 700 | `src/Modules/Identity/Kmic.Identity.Presentation/Roles/CreateRoleEndpoint.cs` | 保留 | 核对组织/角色/权限与安全事件端点及请求定义：版本归属、完整重排列表、目录限制、受保护当前角色、时间范围等有调用价值，保留；短字段说明与实际类型一致。 | 2026年11月14日 22:39:47 |
| 701 | `src/Modules/Identity/Kmic.Identity.Presentation/Roles/CreateRoleRequest.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 702 | `src/Modules/Identity/Kmic.Identity.Presentation/Roles/DisableRoleEndpoint.cs` | 保留 | 核对组织/角色/权限与安全事件端点及请求定义：版本归属、完整重排列表、目录限制、受保护当前角色、时间范围等有调用价值，保留；短字段说明与实际类型一致。 | 2026年11月14日 22:39:47 |
| 703 | `src/Modules/Identity/Kmic.Identity.Presentation/Roles/DisableRoleRequest.cs` | 保留 | 核对组织/角色/权限与安全事件端点及请求定义：版本归属、完整重排列表、目录限制、受保护当前角色、时间范围等有调用价值，保留；短字段说明与实际类型一致。 | 2026年11月14日 22:39:47 |
| 704 | `src/Modules/Identity/Kmic.Identity.Presentation/Roles/EnableRoleEndpoint.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 705 | `src/Modules/Identity/Kmic.Identity.Presentation/Roles/EnableRoleRequest.cs` | 保留 | 核对组织/角色/权限与安全事件端点及请求定义：版本归属、完整重排列表、目录限制、受保护当前角色、时间范围等有调用价值，保留；短字段说明与实际类型一致。 | 2026年11月14日 22:39:47 |
| 706 | `src/Modules/Identity/Kmic.Identity.Presentation/Roles/GetRoleEndpoint.cs` | 保留 | 核对组织/角色/权限与安全事件端点及请求定义：版本归属、完整重排列表、目录限制、受保护当前角色、时间范围等有调用价值，保留；短字段说明与实际类型一致。 | 2026年11月14日 22:39:48 |
| 707 | `src/Modules/Identity/Kmic.Identity.Presentation/Roles/ListDataScopeObjectsEndpoint.cs` | 保留 | 核对组织/角色/权限与安全事件端点及请求定义：版本归属、完整重排列表、目录限制、受保护当前角色、时间范围等有调用价值，保留；短字段说明与实际类型一致。 | 2026年11月14日 22:39:48 |
| 708 | `src/Modules/Identity/Kmic.Identity.Presentation/Roles/ListPermissionsEndpoint.cs` | 保留 | 核对组织/角色/权限与安全事件端点及请求定义：版本归属、完整重排列表、目录限制、受保护当前角色、时间范围等有调用价值，保留；短字段说明与实际类型一致。 | 2026年11月14日 22:39:48 |
| 709 | `src/Modules/Identity/Kmic.Identity.Presentation/Roles/ListRolesEndpoint.cs` | 保留 | 核对组织/角色/权限与安全事件端点及请求定义：版本归属、完整重排列表、目录限制、受保护当前角色、时间范围等有调用价值，保留；短字段说明与实际类型一致。 | 2026年11月14日 22:39:48 |
| 710 | `src/Modules/Identity/Kmic.Identity.Presentation/Roles/ListRolesRequest.cs` | 保留 | 核对组织/角色/权限与安全事件端点及请求定义：版本归属、完整重排列表、目录限制、受保护当前角色、时间范围等有调用价值，保留；短字段说明与实际类型一致。 | 2026年11月14日 22:39:48 |
| 711 | `src/Modules/Identity/Kmic.Identity.Presentation/Roles/SetRoleDataScopesEndpoint.cs` | 保留 | 核对组织/角色/权限与安全事件端点及请求定义：版本归属、完整重排列表、目录限制、受保护当前角色、时间范围等有调用价值，保留；短字段说明与实际类型一致。 | 2026年11月14日 22:39:48 |
| 712 | `src/Modules/Identity/Kmic.Identity.Presentation/Roles/SetRoleDataScopesRequest.cs` | 保留 | 核对组织/角色/权限与安全事件端点及请求定义：版本归属、完整重排列表、目录限制、受保护当前角色、时间范围等有调用价值，保留；短字段说明与实际类型一致。 | 2026年11月14日 22:39:48 |
| 713 | `src/Modules/Identity/Kmic.Identity.Presentation/Roles/SetRolePermissionsEndpoint.cs` | 保留 | 核对组织/角色/权限与安全事件端点及请求定义：版本归属、完整重排列表、目录限制、受保护当前角色、时间范围等有调用价值，保留；短字段说明与实际类型一致。 | 2026年11月14日 22:39:48 |
| 714 | `src/Modules/Identity/Kmic.Identity.Presentation/Roles/SetRolePermissionsRequest.cs` | 保留 | 核对组织/角色/权限与安全事件端点及请求定义：版本归属、完整重排列表、目录限制、受保护当前角色、时间范围等有调用价值，保留；短字段说明与实际类型一致。 | 2026年11月14日 22:39:48 |
| 715 | `src/Modules/Identity/Kmic.Identity.Presentation/Roles/UpdateRoleEndpoint.cs` | 保留 | 核对组织/角色/权限与安全事件端点及请求定义：版本归属、完整重排列表、目录限制、受保护当前角色、时间范围等有调用价值，保留；短字段说明与实际类型一致。 | 2026年11月14日 22:39:49 |
| 716 | `src/Modules/Identity/Kmic.Identity.Presentation/Roles/UpdateRoleRequest.cs` | 保留 | 核对组织/角色/权限与安全事件端点及请求定义：版本归属、完整重排列表、目录限制、受保护当前角色、时间范围等有调用价值，保留；短字段说明与实际类型一致。 | 2026年11月14日 22:39:49 |
| 717 | `src/Modules/Identity/Kmic.Identity.Presentation/SecurityEvents/ListSecurityEventsEndpoint.cs` | 保留 | 核对组织/角色/权限与安全事件端点及请求定义：版本归属、完整重排列表、目录限制、受保护当前角色、时间范围等有调用价值，保留；短字段说明与实际类型一致。 | 2026年11月14日 22:39:49 |
| 718 | `src/Modules/Identity/Kmic.Identity.Presentation/SecurityEvents/ListSecurityEventsRequest.cs` | 已修订 | 完成跨文件一致性复查后重新核对本轮最终内容：字典/组织移动仅在父项变化时重排；取消写所属模块；投递上限仅在暂时失败时判断；事件账号与模拟会话时间说明均与实现一致。；本阶段 C2252 | 2026年11月14日 22:41:41 |
| 719 | `src/Modules/Identity/Kmic.Identity.Presentation/Users/CreateUserEndpoint.cs` | 保留 | 核对用户端点及请求：保留无密码/角色初始状态、禁用和重置的会话效果、凭据修改额外权限、末位管理员保护和空锚点回退约定；中文准确清楚，保留。 | 2026年11月14日 22:40:10 |
| 720 | `src/Modules/Identity/Kmic.Identity.Presentation/Users/CreateUserRequest.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 721 | `src/Modules/Identity/Kmic.Identity.Presentation/Users/DisableUserEndpoint.cs` | 保留 | 核对用户端点及请求：保留无密码/角色初始状态、禁用和重置的会话效果、凭据修改额外权限、末位管理员保护和空锚点回退约定；中文准确清楚，保留。 | 2026年11月14日 22:40:10 |
| 722 | `src/Modules/Identity/Kmic.Identity.Presentation/Users/DisableUserRequest.cs` | 保留 | 核对用户端点及请求：保留无密码/角色初始状态、禁用和重置的会话效果、凭据修改额外权限、末位管理员保护和空锚点回退约定；中文准确清楚，保留。 | 2026年11月14日 22:40:10 |
| 723 | `src/Modules/Identity/Kmic.Identity.Presentation/Users/EnableUserEndpoint.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 724 | `src/Modules/Identity/Kmic.Identity.Presentation/Users/EnableUserRequest.cs` | 保留 | 核对用户端点及请求：保留无密码/角色初始状态、禁用和重置的会话效果、凭据修改额外权限、末位管理员保护和空锚点回退约定；中文准确清楚，保留。 | 2026年11月14日 22:40:10 |
| 725 | `src/Modules/Identity/Kmic.Identity.Presentation/Users/ForceLogoutEndpoint.cs` | 保留 | 核对用户端点及请求：保留无密码/角色初始状态、禁用和重置的会话效果、凭据修改额外权限、末位管理员保护和空锚点回退约定；中文准确清楚，保留。 | 2026年11月14日 22:40:10 |
| 726 | `src/Modules/Identity/Kmic.Identity.Presentation/Users/ForceLogoutRequest.cs` | 保留 | 核对用户端点及请求：保留无密码/角色初始状态、禁用和重置的会话效果、凭据修改额外权限、末位管理员保护和空锚点回退约定；中文准确清楚，保留。 | 2026年11月14日 22:40:10 |
| 727 | `src/Modules/Identity/Kmic.Identity.Presentation/Users/GetUserEndpoint.cs` | 保留 | 核对用户端点及请求：保留无密码/角色初始状态、禁用和重置的会话效果、凭据修改额外权限、末位管理员保护和空锚点回退约定；中文准确清楚，保留。 | 2026年11月14日 22:40:10 |
| 728 | `src/Modules/Identity/Kmic.Identity.Presentation/Users/GrantRoleEndpoint.cs` | 保留 | 核对用户端点及请求：保留无密码/角色初始状态、禁用和重置的会话效果、凭据修改额外权限、末位管理员保护和空锚点回退约定；中文准确清楚，保留。 | 2026年11月14日 22:40:11 |
| 729 | `src/Modules/Identity/Kmic.Identity.Presentation/Users/GrantRoleRequest.cs` | 保留 | 核对用户端点及请求：保留无密码/角色初始状态、禁用和重置的会话效果、凭据修改额外权限、末位管理员保护和空锚点回退约定；中文准确清楚，保留。 | 2026年11月14日 22:40:11 |
| 730 | `src/Modules/Identity/Kmic.Identity.Presentation/Users/ListUserSessionsEndpoint.cs` | 保留 | 核对用户端点及请求：保留无密码/角色初始状态、禁用和重置的会话效果、凭据修改额外权限、末位管理员保护和空锚点回退约定；中文准确清楚，保留。 | 2026年11月14日 22:40:11 |
| 731 | `src/Modules/Identity/Kmic.Identity.Presentation/Users/ListUsersEndpoint.cs` | 保留 | 核对用户端点及请求：保留无密码/角色初始状态、禁用和重置的会话效果、凭据修改额外权限、末位管理员保护和空锚点回退约定；中文准确清楚，保留。 | 2026年11月14日 22:40:11 |
| 732 | `src/Modules/Identity/Kmic.Identity.Presentation/Users/ListUsersRequest.cs` | 保留 | 核对用户端点及请求：保留无密码/角色初始状态、禁用和重置的会话效果、凭据修改额外权限、末位管理员保护和空锚点回退约定；中文准确清楚，保留。 | 2026年11月14日 22:40:11 |
| 733 | `src/Modules/Identity/Kmic.Identity.Presentation/Users/ResetUserPasswordEndpoint.cs` | 保留 | 核对用户端点及请求：保留无密码/角色初始状态、禁用和重置的会话效果、凭据修改额外权限、末位管理员保护和空锚点回退约定；中文准确清楚，保留。 | 2026年11月14日 22:40:11 |
| 734 | `src/Modules/Identity/Kmic.Identity.Presentation/Users/RevokeRoleEndpoint.cs` | 保留 | 核对用户端点及请求：保留无密码/角色初始状态、禁用和重置的会话效果、凭据修改额外权限、末位管理员保护和空锚点回退约定；中文准确清楚，保留。 | 2026年11月14日 22:40:11 |
| 735 | `src/Modules/Identity/Kmic.Identity.Presentation/Users/RevokeRoleRequest.cs` | 保留 | 核对用户端点及请求：保留无密码/角色初始状态、禁用和重置的会话效果、凭据修改额外权限、末位管理员保护和空锚点回退约定；中文准确清楚，保留。 | 2026年11月14日 22:40:11 |
| 736 | `src/Modules/Identity/Kmic.Identity.Presentation/Users/SearchUserOptionsEndpoint.cs` | 保留 | 核对用户端点及请求：保留无密码/角色初始状态、禁用和重置的会话效果、凭据修改额外权限、末位管理员保护和空锚点回退约定；中文准确清楚，保留。 | 2026年11月14日 22:40:11 |
| 737 | `src/Modules/Identity/Kmic.Identity.Presentation/Users/SearchUserOptionsRequest.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 738 | `src/Modules/Identity/Kmic.Identity.Presentation/Users/SetGrantAnchorsEndpoint.cs` | 保留 | 核对用户端点及请求：保留无密码/角色初始状态、禁用和重置的会话效果、凭据修改额外权限、末位管理员保护和空锚点回退约定；中文准确清楚，保留。 | 2026年11月14日 22:40:12 |
| 739 | `src/Modules/Identity/Kmic.Identity.Presentation/Users/SetGrantAnchorsRequest.cs` | 保留 | 核对用户端点及请求：保留无密码/角色初始状态、禁用和重置的会话效果、凭据修改额外权限、末位管理员保护和空锚点回退约定；中文准确清楚，保留。 | 2026年11月14日 22:40:12 |
| 740 | `src/Modules/Identity/Kmic.Identity.Presentation/Users/UnlockLoginEndpoint.cs` | 保留 | 核对用户端点及请求：保留无密码/角色初始状态、禁用和重置的会话效果、凭据修改额外权限、末位管理员保护和空锚点回退约定；中文准确清楚，保留。 | 2026年11月14日 22:40:12 |
| 741 | `src/Modules/Identity/Kmic.Identity.Presentation/Users/UpdateUserProfileEndpoint.cs` | 保留 | 核对用户端点及请求：保留无密码/角色初始状态、禁用和重置的会话效果、凭据修改额外权限、末位管理员保护和空锚点回退约定；中文准确清楚，保留。 | 2026年11月14日 22:40:12 |
| 742 | `src/Modules/Identity/Kmic.Identity.Presentation/Users/UpdateUserProfileRequest.cs` | 沿用已复核 | 上轮已完整读取关联文件并核实修改，本轮内容未变；原编号 C2132。 | 2026年11月14日 22:12:28 |
| 743 | `src/Modules/Identity/Kmic.Identity.Presentation/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 744 | `src/Modules/Notifications/Kmic.Notifications.Application/Captured/Get/CapturedDetailResponse.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:02 |
| 745 | `src/Modules/Notifications/Kmic.Notifications.Application/Captured/Get/GetCapturedQuery.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:02 |
| 746 | `src/Modules/Notifications/Kmic.Notifications.Application/Captured/Get/GetCapturedQueryHandler.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:02 |
| 747 | `src/Modules/Notifications/Kmic.Notifications.Application/Captured/List/CapturedListItem.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:02 |
| 748 | `src/Modules/Notifications/Kmic.Notifications.Application/Captured/List/CapturedSort.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 749 | `src/Modules/Notifications/Kmic.Notifications.Application/Captured/List/ListCapturedQuery.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:02 |
| 750 | `src/Modules/Notifications/Kmic.Notifications.Application/Captured/List/ListCapturedQueryHandler.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:02 |
| 751 | `src/Modules/Notifications/Kmic.Notifications.Application/Captured/List/ListCapturedQueryValidator.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 752 | `src/Modules/Notifications/Kmic.Notifications.Application/Captured/Purge/PurgeCapturedCommand.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:03 |
| 753 | `src/Modules/Notifications/Kmic.Notifications.Application/Captured/Purge/PurgeCapturedCommandHandler.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:03 |
| 754 | `src/Modules/Notifications/Kmic.Notifications.Application/Channels/EmailAttachment.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 755 | `src/Modules/Notifications/Kmic.Notifications.Application/Channels/EmailMessage.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:03 |
| 756 | `src/Modules/Notifications/Kmic.Notifications.Application/Channels/IEmailSender.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:03 |
| 757 | `src/Modules/Notifications/Kmic.Notifications.Application/Channels/ISmsGateway.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:03 |
| 758 | `src/Modules/Notifications/Kmic.Notifications.Application/Channels/NotificationsDeliveryOptions.cs` | 已修订 | 完成跨文件一致性复查后重新核对本轮最终内容：字典/组织移动仅在父项变化时重排；取消写所属模块；投递上限仅在暂时失败时判断；事件账号与模拟会话时间说明均与实现一致。；本阶段 C2226,C2227,C2234 | 2026年11月14日 22:41:41 |
| 759 | `src/Modules/Notifications/Kmic.Notifications.Application/Channels/SendOutcome.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:03 |
| 760 | `src/Modules/Notifications/Kmic.Notifications.Application/Channels/SendResult.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:03 |
| 761 | `src/Modules/Notifications/Kmic.Notifications.Application/Channels/SmsMessage.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:03 |
| 762 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Abandon/AbandonDeliveryCommand.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:04 |
| 763 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Abandon/AbandonDeliveryCommandHandler.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:04 |
| 764 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Abandon/AbandonDeliveryCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 765 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Get/DeliveryDetailResponse.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:04 |
| 766 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Get/GetDeliveryQuery.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:04 |
| 767 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Get/GetDeliveryQueryHandler.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:04 |
| 768 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/List/DeliveryListItem.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:05 |
| 769 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/List/DeliverySort.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 770 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/List/ListDeliveriesQuery.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:05 |
| 771 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/List/ListDeliveriesQueryHandler.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:05 |
| 772 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/List/ListDeliveriesQueryValidator.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:05 |
| 773 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Purge/ClearSecretBodiesCommand.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:05 |
| 774 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Purge/ClearSecretBodiesCommandHandler.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:05 |
| 775 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Purge/PurgeDeliveriesCommand.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:06 |
| 776 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Purge/PurgeDeliveriesCommandHandler.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:06 |
| 777 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Retry/RetryDeliveryCommand.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:06 |
| 778 | `src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Retry/RetryDeliveryCommandHandler.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:06 |
| 779 | `src/Modules/Notifications/Kmic.Notifications.Application/INotificationsQueryDb.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:06 |
| 780 | `src/Modules/Notifications/Kmic.Notifications.Application/Kmic.Notifications.Application.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 781 | `src/Modules/Notifications/Kmic.Notifications.Application/NotificationsCaptureOptions.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:07 |
| 782 | `src/Modules/Notifications/Kmic.Notifications.Application/NotificationsPermissions.cs` | 已修订 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。；本阶段 C2228 | 2026年11月14日 22:31:07 |
| 783 | `src/Modules/Notifications/Kmic.Notifications.Application/NotificationsRenderOptions.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:07 |
| 784 | `src/Modules/Notifications/Kmic.Notifications.Application/NotificationsRetentionOptions.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:07 |
| 785 | `src/Modules/Notifications/Kmic.Notifications.Application/NotificationsScopes.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:07 |
| 786 | `src/Modules/Notifications/Kmic.Notifications.Application/Processing/AddressMasking.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:08 |
| 787 | `src/Modules/Notifications/Kmic.Notifications.Application/Processing/ChannelContent.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 788 | `src/Modules/Notifications/Kmic.Notifications.Application/Processing/NotificationProcessor.cs` | 已修订 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。；本阶段 C2229 | 2026年11月14日 22:31:08 |
| 789 | `src/Modules/Notifications/Kmic.Notifications.Application/Processing/NotificationRequest.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:08 |
| 790 | `src/Modules/Notifications/Kmic.Notifications.Application/Processing/ProcessSendNotificationCommand.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:08 |
| 791 | `src/Modules/Notifications/Kmic.Notifications.Application/Processing/ProcessSendNotificationCommandHandler.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:08 |
| 792 | `src/Modules/Notifications/Kmic.Notifications.Application/Processing/ProcessSendSecretNotificationCommand.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:09 |
| 793 | `src/Modules/Notifications/Kmic.Notifications.Application/Processing/ProcessSendSecretNotificationCommandHandler.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:09 |
| 794 | `src/Modules/Notifications/Kmic.Notifications.Application/Processing/RenderedContent.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:09 |
| 795 | `src/Modules/Notifications/Kmic.Notifications.Application/Rendering/ITemplateRenderer.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:09 |
| 796 | `src/Modules/Notifications/Kmic.Notifications.Application/Rendering/RenderFormat.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 797 | `src/Modules/Notifications/Kmic.Notifications.Application/SiteMessages/List/ListMySiteMessagesQuery.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:09 |
| 798 | `src/Modules/Notifications/Kmic.Notifications.Application/SiteMessages/List/ListMySiteMessagesQueryHandler.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:10 |
| 799 | `src/Modules/Notifications/Kmic.Notifications.Application/SiteMessages/List/SiteMessageItem.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:10 |
| 800 | `src/Modules/Notifications/Kmic.Notifications.Application/SiteMessages/List/SiteMessageSort.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 801 | `src/Modules/Notifications/Kmic.Notifications.Application/SiteMessages/MarkRead/MarkAllSiteMessagesReadCommand.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:10 |
| 802 | `src/Modules/Notifications/Kmic.Notifications.Application/SiteMessages/MarkRead/MarkAllSiteMessagesReadCommandHandler.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:10 |
| 803 | `src/Modules/Notifications/Kmic.Notifications.Application/SiteMessages/MarkRead/MarkSiteMessageReadCommand.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:10 |
| 804 | `src/Modules/Notifications/Kmic.Notifications.Application/SiteMessages/MarkRead/MarkSiteMessageReadCommandHandler.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:11 |
| 805 | `src/Modules/Notifications/Kmic.Notifications.Application/SiteMessages/MarkRead/MarkedReadResponse.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:11 |
| 806 | `src/Modules/Notifications/Kmic.Notifications.Application/SiteMessages/Purge/PurgeSiteMessagesCommand.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:11 |
| 807 | `src/Modules/Notifications/Kmic.Notifications.Application/SiteMessages/Purge/PurgeSiteMessagesCommandHandler.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:11 |
| 808 | `src/Modules/Notifications/Kmic.Notifications.Application/SiteMessages/UnreadCount/GetMyUnreadCountQuery.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:11 |
| 809 | `src/Modules/Notifications/Kmic.Notifications.Application/SiteMessages/UnreadCount/GetMyUnreadCountQueryHandler.cs` | 保留 | 核对捕获、投递管理、通知分发与站内消息处理：保留正文清理范围、异步渠道发送、去重/事务、收件人归属、链接失效重建和秘密有效期约定；精简模板化说明，准确称领取次数。 | 2026年11月14日 22:31:12 |
| 810 | `src/Modules/Notifications/Kmic.Notifications.Application/SiteMessages/UnreadCount/UnreadCountResponse.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 811 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/ChannelRendering.cs` | 保留 | 核对模板渲染、发布/恢复/同步和通知契约及投递工厂：保留严格模式仅检查执行分支、同步锁不等待、含密请求限制、默认版本与捕获不发送的约定。实体属性注释保持不变，快照相关只读核对。 | 2026年11月14日 22:31:42 |
| 812 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Get/GetTemplateQuery.cs` | 保留 | 核对模板渲染、发布/恢复/同步和通知契约及投递工厂：保留严格模式仅检查执行分支、同步锁不等待、含密请求限制、默认版本与捕获不发送的约定。实体属性注释保持不变，快照相关只读核对。 | 2026年11月14日 22:31:42 |
| 813 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Get/GetTemplateQueryHandler.cs` | 保留 | 核对模板渲染、发布/恢复/同步和通知契约及投递工厂：保留严格模式仅检查执行分支、同步锁不等待、含密请求限制、默认版本与捕获不发送的约定。实体属性注释保持不变，快照相关只读核对。 | 2026年11月14日 22:31:42 |
| 814 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Get/TemplateChannelInfo.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 815 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Get/TemplateDetailResponse.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 816 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Get/TemplateVersionInfo.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 817 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/List/ListTemplatesQuery.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 818 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/List/ListTemplatesQueryHandler.cs` | 保留 | 核对模板渲染、发布/恢复/同步和通知契约及投递工厂：保留严格模式仅检查执行分支、同步锁不等待、含密请求限制、默认版本与捕获不发送的约定。实体属性注释保持不变，快照相关只读核对。 | 2026年11月14日 22:31:42 |
| 819 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/List/TemplateListItem.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 820 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/List/TemplateListResponse.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 821 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Preview/PreviewTemplateQuery.cs` | 保留 | 核对模板渲染、发布/恢复/同步和通知契约及投递工厂：保留严格模式仅检查执行分支、同步锁不等待、含密请求限制、默认版本与捕获不发送的约定。实体属性注释保持不变，快照相关只读核对。 | 2026年11月14日 22:31:42 |
| 822 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Preview/PreviewTemplateQueryHandler.cs` | 保留 | 核对模板渲染、发布/恢复/同步和通知契约及投递工厂：保留严格模式仅检查执行分支、同步锁不等待、含密请求限制、默认版本与捕获不发送的约定。实体属性注释保持不变，快照相关只读核对。 | 2026年11月14日 22:31:42 |
| 823 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Preview/TemplatePreviewResponse.cs` | 保留 | 核对模板渲染、发布/恢复/同步和通知契约及投递工厂：保留严格模式仅检查执行分支、同步锁不等待、含密请求限制、默认版本与捕获不发送的约定。实体属性注释保持不变，快照相关只读核对。 | 2026年11月14日 22:31:43 |
| 824 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Publish/PublishTemplateCommand.cs` | 保留 | 核对模板渲染、发布/恢复/同步和通知契约及投递工厂：保留严格模式仅检查执行分支、同步锁不等待、含密请求限制、默认版本与捕获不发送的约定。实体属性注释保持不变，快照相关只读核对。 | 2026年11月14日 22:31:43 |
| 825 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Publish/PublishTemplateCommandHandler.cs` | 保留 | 核对模板渲染、发布/恢复/同步和通知契约及投递工厂：保留严格模式仅检查执行分支、同步锁不等待、含密请求限制、默认版本与捕获不发送的约定。实体属性注释保持不变，快照相关只读核对。 | 2026年11月14日 22:31:43 |
| 826 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Publish/PublishTemplateCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 827 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Revert/RevertTemplateCommand.cs` | 保留 | 核对模板渲染、发布/恢复/同步和通知契约及投递工厂：保留严格模式仅检查执行分支、同步锁不等待、含密请求限制、默认版本与捕获不发送的约定。实体属性注释保持不变，快照相关只读核对。 | 2026年11月14日 22:31:43 |
| 828 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Revert/RevertTemplateCommandHandler.cs` | 保留 | 核对模板渲染、发布/恢复/同步和通知契约及投递工厂：保留严格模式仅检查执行分支、同步锁不等待、含密请求限制、默认版本与捕获不发送的约定。实体属性注释保持不变，快照相关只读核对。 | 2026年11月14日 22:31:43 |
| 829 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Revert/RevertTemplateCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 830 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Sync/SyncTemplateCatalogCommand.cs` | 保留 | 核对模板渲染、发布/恢复/同步和通知契约及投递工厂：保留严格模式仅检查执行分支、同步锁不等待、含密请求限制、默认版本与捕获不发送的约定。实体属性注释保持不变，快照相关只读核对。 | 2026年11月14日 22:31:43 |
| 831 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Sync/SyncTemplateCatalogCommandHandler.cs` | 保留 | 核对模板渲染、发布/恢复/同步和通知契约及投递工厂：保留严格模式仅检查执行分支、同步锁不等待、含密请求限制、默认版本与捕获不发送的约定。实体属性注释保持不变，快照相关只读核对。 | 2026年11月14日 22:31:43 |
| 832 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Sync/TemplateSyncResult.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 833 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/TemplateChannels.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 834 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/TemplateHash.cs` | 保留 | 核对模板渲染、发布/恢复/同步和通知契约及投递工厂：保留严格模式仅检查执行分支、同步锁不等待、含密请求限制、默认版本与捕获不发送的约定。实体属性注释保持不变，快照相关只读核对。 | 2026年11月14日 22:31:43 |
| 835 | `src/Modules/Notifications/Kmic.Notifications.Application/Templates/TemplateModels.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 836 | `src/Modules/Notifications/Kmic.Notifications.Application/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 837 | `src/Modules/Notifications/Kmic.Notifications.Contracts/Kmic.Notifications.Contracts.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 838 | `src/Modules/Notifications/Kmic.Notifications.Contracts/NotificationPriority.cs` | 保留 | 核对模板渲染、发布/恢复/同步和通知契约及投递工厂：保留严格模式仅检查执行分支、同步锁不等待、含密请求限制、默认版本与捕获不发送的约定。实体属性注释保持不变，快照相关只读核对。 | 2026年11月14日 22:31:43 |
| 839 | `src/Modules/Notifications/Kmic.Notifications.Contracts/NotificationsModuleInfo.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 840 | `src/Modules/Notifications/Kmic.Notifications.Contracts/Recipient.cs` | 保留 | 核对模板渲染、发布/恢复/同步和通知契约及投递工厂：保留严格模式仅检查执行分支、同步锁不等待、含密请求限制、默认版本与捕获不发送的约定。实体属性注释保持不变，快照相关只读核对。 | 2026年11月14日 22:31:44 |
| 841 | `src/Modules/Notifications/Kmic.Notifications.Contracts/RecipientRole.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 842 | `src/Modules/Notifications/Kmic.Notifications.Contracts/SendNotification.cs` | 保留 | 核对模板渲染、发布/恢复/同步和通知契约及投递工厂：保留严格模式仅检查执行分支、同步锁不等待、含密请求限制、默认版本与捕获不发送的约定。实体属性注释保持不变，快照相关只读核对。 | 2026年11月14日 22:31:44 |
| 843 | `src/Modules/Notifications/Kmic.Notifications.Contracts/SendSecretNotification.cs` | 保留 | 核对模板渲染、发布/恢复/同步和通知契约及投递工厂：保留严格模式仅检查执行分支、同步锁不等待、含密请求限制、默认版本与捕获不发送的约定。实体属性注释保持不变，快照相关只读核对。 | 2026年11月14日 22:31:44 |
| 844 | `src/Modules/Notifications/Kmic.Notifications.Contracts/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 845 | `src/Modules/Notifications/Kmic.Notifications.Domain/Captured/CapturedMessage.cs` | 已修订 | 完成跨文件一致性复查后重新核对本轮最终内容：字典/组织移动仅在父项变化时重排；取消写所属模块；投递上限仅在暂时失败时判断；事件账号与模拟会话时间说明均与实现一致。；本阶段 C2233 | 2026年11月14日 22:41:41 |
| 846 | `src/Modules/Notifications/Kmic.Notifications.Domain/Captured/CapturedMessageId.cs` | 保留 | 核对模板渲染、发布/恢复/同步和通知契约及投递工厂：保留严格模式仅检查执行分支、同步锁不等待、含密请求限制、默认版本与捕获不发送的约定。实体属性注释保持不变，快照相关只读核对。 | 2026年11月14日 22:31:44 |
| 847 | `src/Modules/Notifications/Kmic.Notifications.Domain/Captured/ICapturedMessageRepository.cs` | 保留 | 核对模板渲染、发布/恢复/同步和通知契约及投递工厂：保留严格模式仅检查执行分支、同步锁不等待、含密请求限制、默认版本与捕获不发送的约定。实体属性注释保持不变，快照相关只读核对。 | 2026年11月14日 22:31:44 |
| 848 | `src/Modules/Notifications/Kmic.Notifications.Domain/Deliveries/Delivery.cs` | 已修订（模型中文） | 本轮 C2280～C2282；先核对实际行为，再精简或纠正摘要/模型说明。已同步注释迁移与快照，运行模型及 SQL 仅注释变化；测试未运行 | 2026年11月15日 00:04:56 |
| 849 | `src/Modules/Notifications/Kmic.Notifications.Domain/Deliveries/DeliveryChannel.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 850 | `src/Modules/Notifications/Kmic.Notifications.Domain/Deliveries/DeliveryColumns.cs` | 保留 | 核对模板渲染、发布/恢复/同步和通知契约及投递工厂：保留严格模式仅检查执行分支、同步锁不等待、含密请求限制、默认版本与捕获不发送的约定。实体属性注释保持不变，快照相关只读核对。 | 2026年11月14日 22:31:44 |
| 851 | `src/Modules/Notifications/Kmic.Notifications.Domain/Deliveries/DeliveryId.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 852 | `src/Modules/Notifications/Kmic.Notifications.Domain/Deliveries/DeliveryPriority.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 853 | `src/Modules/Notifications/Kmic.Notifications.Domain/Deliveries/DeliveryRole.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 854 | `src/Modules/Notifications/Kmic.Notifications.Domain/Deliveries/DeliverySkipReason.cs` | 保留 | 核对模板渲染、发布/恢复/同步和通知契约及投递工厂：保留严格模式仅检查执行分支、同步锁不等待、含密请求限制、默认版本与捕获不发送的约定。实体属性注释保持不变，快照相关只读核对。 | 2026年11月14日 22:31:44 |
| 855 | `src/Modules/Notifications/Kmic.Notifications.Domain/Deliveries/DeliveryStatus.cs` | 保留 | 核对模板渲染、发布/恢复/同步和通知契约及投递工厂：保留严格模式仅检查执行分支、同步锁不等待、含密请求限制、默认版本与捕获不发送的约定。实体属性注释保持不变，快照相关只读核对。 | 2026年11月14日 22:31:45 |
| 856 | `src/Modules/Notifications/Kmic.Notifications.Domain/Deliveries/IDeliveryRepository.cs` | 保留 | 核对仓储保留期/重试条件、模板聚合和渲染器限额：保留渠道版本、历史保留、HTML 转义、步数与写入检查、SMTP 失败分类及唯一索引原因；精简口语重复。HasComment 和实体属性不改。 | 2026年11月14日 22:32:34 |
| 857 | `src/Modules/Notifications/Kmic.Notifications.Domain/Errors/NotificationsErrors.cs` | 保留 | 核对仓储保留期/重试条件、模板聚合和渲染器限额：保留渠道版本、历史保留、HTML 转义、步数与写入检查、SMTP 失败分类及唯一索引原因；精简口语重复。HasComment 和实体属性不改。 | 2026年11月14日 22:32:35 |
| 858 | `src/Modules/Notifications/Kmic.Notifications.Domain/Kmic.Notifications.Domain.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 859 | `src/Modules/Notifications/Kmic.Notifications.Domain/Notifications/INotificationRepository.cs` | 保留 | 核对仓储保留期/重试条件、模板聚合和渲染器限额：保留渠道版本、历史保留、HTML 转义、步数与写入检查、SMTP 失败分类及唯一索引原因；精简口语重复。HasComment 和实体属性不改。 | 2026年11月14日 22:32:35 |
| 860 | `src/Modules/Notifications/Kmic.Notifications.Domain/Notifications/Notification.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 861 | `src/Modules/Notifications/Kmic.Notifications.Domain/Notifications/NotificationId.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 862 | `src/Modules/Notifications/Kmic.Notifications.Domain/SiteMessages/ISiteMessageRepository.cs` | 保留 | 核对仓储保留期/重试条件、模板聚合和渲染器限额：保留渠道版本、历史保留、HTML 转义、步数与写入检查、SMTP 失败分类及唯一索引原因；精简口语重复。HasComment 和实体属性不改。 | 2026年11月14日 22:32:35 |
| 863 | `src/Modules/Notifications/Kmic.Notifications.Domain/SiteMessages/SiteMessage.cs` | 已修订 | 核对仓储保留期/重试条件、模板聚合和渲染器限额：保留渠道版本、历史保留、HTML 转义、步数与写入检查、SMTP 失败分类及唯一索引原因；精简口语重复。HasComment 和实体属性不改。；本阶段 C2231,C2232 | 2026年11月14日 22:32:35 |
| 864 | `src/Modules/Notifications/Kmic.Notifications.Domain/SiteMessages/SiteMessageId.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 865 | `src/Modules/Notifications/Kmic.Notifications.Domain/Templates/ITemplateRepository.cs` | 保留 | 核对仓储保留期/重试条件、模板聚合和渲染器限额：保留渠道版本、历史保留、HTML 转义、步数与写入检查、SMTP 失败分类及唯一索引原因；精简口语重复。HasComment 和实体属性不改。 | 2026年11月14日 22:32:35 |
| 866 | `src/Modules/Notifications/Kmic.Notifications.Domain/Templates/NotificationTemplate.cs` | 已修订 | 核对仓储保留期/重试条件、模板聚合和渲染器限额：保留渠道版本、历史保留、HTML 转义、步数与写入检查、SMTP 失败分类及唯一索引原因；精简口语重复。HasComment 和实体属性不改。；本阶段 C2230 | 2026年11月14日 22:32:35 |
| 867 | `src/Modules/Notifications/Kmic.Notifications.Domain/Templates/NotificationTemplateId.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 868 | `src/Modules/Notifications/Kmic.Notifications.Domain/Templates/TemplateChannel.cs` | 保留 | 核对仓储保留期/重试条件、模板聚合和渲染器限额：保留渠道版本、历史保留、HTML 转义、步数与写入检查、SMTP 失败分类及唯一索引原因；精简口语重复。HasComment 和实体属性不改。 | 2026年11月14日 22:32:35 |
| 869 | `src/Modules/Notifications/Kmic.Notifications.Domain/Templates/TemplateStatus.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 870 | `src/Modules/Notifications/Kmic.Notifications.Domain/Templates/TemplateVersion.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 871 | `src/Modules/Notifications/Kmic.Notifications.Domain/Templates/TemplateVersionId.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 872 | `src/Modules/Notifications/Kmic.Notifications.Domain/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 873 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Channels/EmailSenderOptions.cs` | 保留 | 核对仓储保留期/重试条件、模板聚合和渲染器限额：保留渠道版本、历史保留、HTML 转义、步数与写入检查、SMTP 失败分类及唯一索引原因；精简口语重复。HasComment 和实体属性不改。 | 2026年11月14日 22:32:35 |
| 874 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Channels/MailKitEmailSender.cs` | 保留 | 核对仓储保留期/重试条件、模板聚合和渲染器限额：保留渠道版本、历史保留、HTML 转义、步数与写入检查、SMTP 失败分类及唯一索引原因；精简口语重复。HasComment 和实体属性不改。 | 2026年11月14日 22:32:36 |
| 875 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Channels/NotificationsEmailOptions.cs` | 保留 | 核对仓储保留期/重试条件、模板聚合和渲染器限额：保留渠道版本、历史保留、HTML 转义、步数与写入检查、SMTP 失败分类及唯一索引原因；精简口语重复。HasComment 和实体属性不改。 | 2026年11月14日 22:32:36 |
| 876 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Channels/UnconfiguredSmsGateway.cs` | 保留 | 核对仓储保留期/重试条件、模板聚合和渲染器限额：保留渠道版本、历史保留、HTML 转义、步数与写入检查、SMTP 失败分类及唯一索引原因；精简口语重复。HasComment 和实体属性不改。 | 2026年11月14日 22:32:36 |
| 877 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Kmic.Notifications.Infrastructure.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 878 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/NotificationsLog.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 879 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/NotificationsModule.cs` | 保留 | 核对仓储保留期/重试条件、模板聚合和渲染器限额：保留渠道版本、历史保留、HTML 转义、步数与写入检查、SMTP 失败分类及唯一索引原因；精简口语重复。HasComment 和实体属性不改。 | 2026年11月14日 22:32:36 |
| 880 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Configurations/CapturedMessageConfiguration.cs` | 已修订（模型中文） | 本轮 C2284；先核对实际行为，再精简或纠正摘要/模型说明。已同步注释迁移与快照，运行模型及 SQL 仅注释变化；测试未运行 | 2026年11月15日 00:04:56 |
| 881 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Configurations/DeliveryConfiguration.cs` | 已修订（模型中文） | 本轮 C2283；先核对实际行为，再精简或纠正摘要/模型说明。已同步注释迁移与快照，运行模型及 SQL 仅注释变化；测试未运行 | 2026年11月15日 00:04:56 |
| 882 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Configurations/NotificationConfiguration.cs` | 保留 | 核对仓储保留期/重试条件、模板聚合和渲染器限额：保留渠道版本、历史保留、HTML 转义、步数与写入检查、SMTP 失败分类及唯一索引原因；精简口语重复。HasComment 和实体属性不改。 | 2026年11月14日 22:32:36 |
| 883 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Configurations/NotificationTemplateConfiguration.cs` | 保留 | 核对仓储保留期/重试条件、模板聚合和渲染器限额：保留渠道版本、历史保留、HTML 转义、步数与写入检查、SMTP 失败分类及唯一索引原因；精简口语重复。HasComment 和实体属性不改。 | 2026年11月14日 22:32:36 |
| 884 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Configurations/SiteMessageConfiguration.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 885 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Configurations/TemplateVersionConfiguration.cs` | 保留 | 核对仓储保留期/重试条件、模板聚合和渲染器限额：保留渠道版本、历史保留、HTML 转义、步数与写入检查、SMTP 失败分类及唯一索引原因；精简口语重复。HasComment 和实体属性不改。 | 2026年11月14日 22:32:37 |
| 886 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/NotificationsDbContext.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 887 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/NotificationsDbContextFactory.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 888 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/NotificationsIdConverters.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 889 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/NotificationsModel.cs` | 保留 | 核对仓储保留期/重试条件、模板聚合和渲染器限额：保留渠道版本、历史保留、HTML 转义、步数与写入检查、SMTP 失败分类及唯一索引原因；精简口语重复。HasComment 和实体属性不改。 | 2026年11月14日 22:32:37 |
| 890 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/NotificationsQueryDbContext.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 891 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Repositories/CapturedMessageRepository.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 892 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Repositories/DeliveryRepository.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 893 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Repositories/NotificationRepository.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 894 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Repositories/SiteMessageRepository.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 895 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Repositories/TemplateRepository.cs` | 保留 | 核对仓储保留期/重试条件、模板聚合和渲染器限额：保留渠道版本、历史保留、HTML 转义、步数与写入检查、SMTP 失败分类及唯一索引原因；精简口语重复。HasComment 和实体属性不改。 | 2026年11月14日 22:32:37 |
| 896 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Rendering/CappedWriter.cs` | 保留 | 核对仓储保留期/重试条件、模板聚合和渲染器限额：保留渠道版本、历史保留、HTML 转义、步数与写入检查、SMTP 失败分类及唯一索引原因；精简口语重复。HasComment 和实体属性不改。 | 2026年11月14日 22:32:37 |
| 897 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Rendering/FluidTemplateRenderer.cs` | 保留 | 核对仓储保留期/重试条件、模板聚合和渲染器限额：保留渠道版本、历史保留、HTML 转义、步数与写入检查、SMTP 失败分类及唯一索引原因；精简口语重复。HasComment 和实体属性不改。 | 2026年11月14日 22:32:37 |
| 898 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Sending/AttachmentLoad.cs` | 保留 | 完整核对附件释放、投递领取 SQL 和发送结果分支：补正重试上限仅在暂时失败时判断；保留租约令牌、秘密有效期、联系方式重查和渠道外事务。已放弃记录仍可查询，不再声称移出列表。 | 2026年11月14日 22:33:10 |
| 899 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Sending/AttachmentLoader.cs` | 保留 | 完整核对附件释放、投递领取 SQL 和发送结果分支：补正重试上限仅在暂时失败时判断；保留租约令牌、秘密有效期、联系方式重查和渠道外事务。已放弃记录仍可查询，不再声称移出列表。 | 2026年11月14日 22:33:10 |
| 900 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Sending/ContactLookup.cs` | 保留 | 完整核对附件释放、投递领取 SQL 和发送结果分支：补正重试上限仅在暂时失败时判断；保留租约令牌、秘密有效期、联系方式重查和渠道外事务。已放弃记录仍可查询，不再声称移出列表。 | 2026年11月14日 22:33:10 |
| 901 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Sending/DeliveryItem.cs` | 已修订 | 完整核对附件释放、投递领取 SQL 和发送结果分支：补正重试上限仅在暂时失败时判断；保留租约令牌、秘密有效期、联系方式重查和渠道外事务。已放弃记录仍可查询，不再声称移出列表。；本阶段 C2235 | 2026年11月14日 22:33:10 |
| 902 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Sending/DeliveryStore.cs` | 保留 | 完整核对附件释放、投递领取 SQL 和发送结果分支：补正重试上限仅在暂时失败时判断；保留租约令牌、秘密有效期、联系方式重查和渠道外事务。已放弃记录仍可查询，不再声称移出列表。 | 2026年11月14日 22:33:10 |
| 903 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Sending/DeliveryWork.cs` | 保留 | 完整核对附件释放、投递领取 SQL 和发送结果分支：补正重试上限仅在暂时失败时判断；保留租约令牌、秘密有效期、联系方式重查和渠道外事务。已放弃记录仍可查询，不再声称移出列表。 | 2026年11月14日 22:33:10 |
| 904 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Templates/SyncTemplateCatalogTask.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 905 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 906 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Captured/GetCapturedEndpoint.cs` | 保留 | 完整核对附件释放、投递领取 SQL 和发送结果分支：补正重试上限仅在暂时失败时判断；保留租约令牌、秘密有效期、联系方式重查和渠道外事务。已放弃记录仍可查询，不再声称移出列表。 | 2026年11月14日 22:33:10 |
| 907 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Captured/ListCapturedEndpoint.cs` | 保留 | 完整核对附件释放、投递领取 SQL 和发送结果分支：补正重试上限仅在暂时失败时判断；保留租约令牌、秘密有效期、联系方式重查和渠道外事务。已放弃记录仍可查询，不再声称移出列表。 | 2026年11月14日 22:33:11 |
| 908 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Captured/ListCapturedRequest.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 909 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Deliveries/AbandonDeliveryEndpoint.cs` | 已修订 | 完整核对附件释放、投递领取 SQL 和发送结果分支：补正重试上限仅在暂时失败时判断；保留租约令牌、秘密有效期、联系方式重查和渠道外事务。已放弃记录仍可查询，不再声称移出列表。；本阶段 C2236 | 2026年11月14日 22:33:11 |
| 910 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Deliveries/AbandonDeliveryRequest.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 911 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Deliveries/GetDeliveryEndpoint.cs` | 保留 | 核对通知管理、站内消息和模板端点：权限范围、含密正文清理、领取次数、重试限制及预览错误仍返回 200 的说明与实现一致；只精简绕口标题。 | 2026年11月14日 22:33:30 |
| 912 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Deliveries/ListDeliveriesEndpoint.cs` | 保留 | 核对通知管理、站内消息和模板端点：权限范围、含密正文清理、领取次数、重试限制及预览错误仍返回 200 的说明与实现一致；只精简绕口标题。 | 2026年11月14日 22:33:30 |
| 913 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Deliveries/ListDeliveriesRequest.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 914 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Deliveries/RetryDeliveryEndpoint.cs` | 保留 | 核对通知管理、站内消息和模板端点：权限范围、含密正文清理、领取次数、重试限制及预览错误仍返回 200 的说明与实现一致；只精简绕口标题。 | 2026年11月14日 22:33:30 |
| 915 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Kmic.Notifications.Presentation.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 916 | `src/Modules/Notifications/Kmic.Notifications.Presentation/NotificationsEndpoints.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 917 | `src/Modules/Notifications/Kmic.Notifications.Presentation/SiteMessages/GetMyUnreadCountEndpoint.cs` | 保留 | 核对通知管理、站内消息和模板端点：权限范围、含密正文清理、领取次数、重试限制及预览错误仍返回 200 的说明与实现一致；只精简绕口标题。 | 2026年11月14日 22:33:30 |
| 918 | `src/Modules/Notifications/Kmic.Notifications.Presentation/SiteMessages/ListMySiteMessagesEndpoint.cs` | 保留 | 核对通知管理、站内消息和模板端点：权限范围、含密正文清理、领取次数、重试限制及预览错误仍返回 200 的说明与实现一致；只精简绕口标题。 | 2026年11月14日 22:33:30 |
| 919 | `src/Modules/Notifications/Kmic.Notifications.Presentation/SiteMessages/ListMySiteMessagesRequest.cs` | 沿用已复核 | 上轮已完整读取关联文件并核实修改，本轮内容未变；原编号 C2175。 | 2026年11月14日 22:12:28 |
| 920 | `src/Modules/Notifications/Kmic.Notifications.Presentation/SiteMessages/MarkAllSiteMessagesReadEndpoint.cs` | 保留 | 核对通知管理、站内消息和模板端点：权限范围、含密正文清理、领取次数、重试限制及预览错误仍返回 200 的说明与实现一致；只精简绕口标题。 | 2026年11月14日 22:33:30 |
| 921 | `src/Modules/Notifications/Kmic.Notifications.Presentation/SiteMessages/MarkSiteMessageReadEndpoint.cs` | 保留 | 核对通知管理、站内消息和模板端点：权限范围、含密正文清理、领取次数、重试限制及预览错误仍返回 200 的说明与实现一致；只精简绕口标题。 | 2026年11月14日 22:33:30 |
| 922 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Templates/GetTemplateEndpoint.cs` | 已修订 | 核对通知管理、站内消息和模板端点：权限范围、含密正文清理、领取次数、重试限制及预览错误仍返回 200 的说明与实现一致；只精简绕口标题。；本阶段 C2237 | 2026年11月14日 22:33:30 |
| 923 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Templates/ListTemplatesEndpoint.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 924 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Templates/PreviewTemplateEndpoint.cs` | 保留 | 核对通知管理、站内消息和模板端点：权限范围、含密正文清理、领取次数、重试限制及预览错误仍返回 200 的说明与实现一致；只精简绕口标题。 | 2026年11月14日 22:33:31 |
| 925 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Templates/PreviewTemplateRequest.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 926 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Templates/PublishTemplateEndpoint.cs` | 已修订 | 核对通知管理、站内消息和模板端点：权限范围、含密正文清理、领取次数、重试限制及预览错误仍返回 200 的说明与实现一致；只精简绕口标题。；本阶段 C2238 | 2026年11月14日 22:33:31 |
| 927 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Templates/PublishTemplateRequest.cs` | 保留 | 核对通知管理、站内消息和模板端点：权限范围、含密正文清理、领取次数、重试限制及预览错误仍返回 200 的说明与实现一致；只精简绕口标题。 | 2026年11月14日 22:33:31 |
| 928 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Templates/RevertTemplateEndpoint.cs` | 保留 | 核对通知管理、站内消息和模板端点：权限范围、含密正文清理、领取次数、重试限制及预览错误仍返回 200 的说明与实现一致；只精简绕口标题。 | 2026年11月14日 22:33:31 |
| 929 | `src/Modules/Notifications/Kmic.Notifications.Presentation/Templates/RevertTemplateRequest.cs` | 保留 | 核对通知管理、站内消息和模板端点：权限范围、含密正文清理、领取次数、重试限制及预览错误仍返回 200 的说明与实现一致；只精简绕口标题。 | 2026年11月14日 22:33:31 |
| 930 | `src/Modules/Notifications/Kmic.Notifications.Presentation/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 931 | `src/Modules/Operations/Kmic.Operations.Application/Audit/AuditFilters.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:04 |
| 932 | `src/Modules/Operations/Kmic.Operations.Application/Audit/AuditTargetItem.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:04 |
| 933 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/AuditCsv.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:05 |
| 934 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/AuditExportCheckpoint.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 935 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/AuditExportOptions.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:05 |
| 936 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/AuditExportSummary.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 937 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/ExportAuditCommand.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:05 |
| 938 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/ExportAuditCommandHandler.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:05 |
| 939 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/ExportAuditCommandValidator.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:05 |
| 940 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/ExportAuditJob.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:05 |
| 941 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/ExportAuditJobHandler.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:05 |
| 942 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/GetAuditExportFileQuery.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:05 |
| 943 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/GetAuditExportFileQueryHandler.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:05 |
| 944 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/RegisterAuditExportFileCommand.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:06 |
| 945 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/RegisterAuditExportFileCommandHandler.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:06 |
| 946 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Get/AuditDetail.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:06 |
| 947 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Get/GetAuditQuery.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:06 |
| 948 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Get/GetAuditQueryHandler.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:06 |
| 949 | `src/Modules/Operations/Kmic.Operations.Application/Audit/List/AuditListItem.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:06 |
| 950 | `src/Modules/Operations/Kmic.Operations.Application/Audit/List/ListAuditQuery.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:06 |
| 951 | `src/Modules/Operations/Kmic.Operations.Application/Audit/List/ListAuditQueryHandler.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:06 |
| 952 | `src/Modules/Operations/Kmic.Operations.Application/Audit/List/ListAuditQueryValidator.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:06 |
| 953 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Record/RecordAuditCommand.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:07 |
| 954 | `src/Modules/Operations/Kmic.Operations.Application/Audit/Record/RecordAuditCommandHandler.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:07 |
| 955 | `src/Modules/Operations/Kmic.Operations.Application/BackgroundJobs/BackgroundJobItem.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:07 |
| 956 | `src/Modules/Operations/Kmic.Operations.Application/BackgroundJobs/BackgroundJobPaging.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:07 |
| 957 | `src/Modules/Operations/Kmic.Operations.Application/BackgroundJobs/Cancel/CancelBackgroundJobCommand.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:07 |
| 958 | `src/Modules/Operations/Kmic.Operations.Application/BackgroundJobs/Cancel/CancelBackgroundJobCommandHandler.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:07 |
| 959 | `src/Modules/Operations/Kmic.Operations.Application/BackgroundJobs/Cancel/CancelBackgroundJobCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 960 | `src/Modules/Operations/Kmic.Operations.Application/BackgroundJobs/List/ListBackgroundJobsQuery.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:08 |
| 961 | `src/Modules/Operations/Kmic.Operations.Application/BackgroundJobs/List/ListBackgroundJobsQueryHandler.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:08 |
| 962 | `src/Modules/Operations/Kmic.Operations.Application/BackgroundJobs/Mine/CancelMyJobCommand.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:08 |
| 963 | `src/Modules/Operations/Kmic.Operations.Application/BackgroundJobs/Mine/CancelMyJobCommandHandler.cs` | 已修订 | 完成跨文件一致性复查后重新核对本轮最终内容：字典/组织移动仅在父项变化时重排；取消写所属模块；投递上限仅在暂时失败时判断；事件账号与模拟会话时间说明均与实现一致。；本阶段 C2218 | 2026年11月14日 22:41:41 |
| 964 | `src/Modules/Operations/Kmic.Operations.Application/BackgroundJobs/Mine/CancelMyJobCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 965 | `src/Modules/Operations/Kmic.Operations.Application/BackgroundJobs/Mine/GetMyJobQuery.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:08 |
| 966 | `src/Modules/Operations/Kmic.Operations.Application/BackgroundJobs/Mine/GetMyJobQueryHandler.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:09 |
| 967 | `src/Modules/Operations/Kmic.Operations.Application/BackgroundJobs/Mine/GetMyJobQueryValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 968 | `src/Modules/Operations/Kmic.Operations.Application/BackgroundJobs/Mine/ListMyJobsQuery.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:09 |
| 969 | `src/Modules/Operations/Kmic.Operations.Application/BackgroundJobs/Mine/ListMyJobsQueryHandler.cs` | 保留 | 核对运维管理员操作、审计筛选/游标/导出和跨模块作业查询取消：保留系统操作人、会话撤销、敏感字段、公式转义、登记与检查点事务及取消不回滚已提交批次等约定；普通用途已清楚。 | 2026年11月14日 22:28:09 |
| 970 | `src/Modules/Operations/Kmic.Operations.Application/BackgroundJobs/Mine/MyJobs.cs` | 保留 | 核对本人作业归属、上报校验/限流、合并游标与死信处理：保留按实例计数、敏感字段禁止上报、UUID 排序差异、放弃检查、逐条结果和载荷含密保护尚未实现的真实说明；不修业务缺口。 | 2026年11月14日 22:28:19 |
| 971 | `src/Modules/Operations/Kmic.Operations.Application/ClientErrors/ClientErrorOptions.cs` | 保留 | 核对本人作业归属、上报校验/限流、合并游标与死信处理：保留按实例计数、敏感字段禁止上报、UUID 排序差异、放弃检查、逐条结果和载荷含密保护尚未实现的真实说明；不修业务缺口。 | 2026年11月14日 22:28:20 |
| 972 | `src/Modules/Operations/Kmic.Operations.Application/ClientErrors/ClientErrorReport.cs` | 保留 | 核对本人作业归属、上报校验/限流、合并游标与死信处理：保留按实例计数、敏感字段禁止上报、UUID 排序差异、放弃检查、逐条结果和载荷含密保护尚未实现的真实说明；不修业务缺口。 | 2026年11月14日 22:28:20 |
| 973 | `src/Modules/Operations/Kmic.Operations.Application/ClientErrors/IClientErrorLimiter.cs` | 保留 | 核对本人作业归属、上报校验/限流、合并游标与死信处理：保留按实例计数、敏感字段禁止上报、UUID 排序差异、放弃检查、逐条结果和载荷含密保护尚未实现的真实说明；不修业务缺口。 | 2026年11月14日 22:28:20 |
| 974 | `src/Modules/Operations/Kmic.Operations.Application/ClientErrors/IClientErrorLog.cs` | 保留 | 核对本人作业归属、上报校验/限流、合并游标与死信处理：保留按实例计数、敏感字段禁止上报、UUID 排序差异、放弃检查、逐条结果和载荷含密保护尚未实现的真实说明；不修业务缺口。 | 2026年11月14日 22:28:20 |
| 975 | `src/Modules/Operations/Kmic.Operations.Application/ClientErrors/ReportClientErrorsCommand.cs` | 保留 | 核对本人作业归属、上报校验/限流、合并游标与死信处理：保留按实例计数、敏感字段禁止上报、UUID 排序差异、放弃检查、逐条结果和载荷含密保护尚未实现的真实说明；不修业务缺口。 | 2026年11月14日 22:28:20 |
| 976 | `src/Modules/Operations/Kmic.Operations.Application/ClientErrors/ReportClientErrorsCommandHandler.cs` | 保留 | 核对本人作业归属、上报校验/限流、合并游标与死信处理：保留按实例计数、敏感字段禁止上报、UUID 排序差异、放弃检查、逐条结果和载荷含密保护尚未实现的真实说明；不修业务缺口。 | 2026年11月14日 22:28:20 |
| 977 | `src/Modules/Operations/Kmic.Operations.Application/ClientErrors/ReportClientErrorsCommandValidator.cs` | 保留 | 核对本人作业归属、上报校验/限流、合并游标与死信处理：保留按实例计数、敏感字段禁止上报、UUID 排序差异、放弃检查、逐条结果和载荷含密保护尚未实现的真实说明；不修业务缺口。 | 2026年11月14日 22:28:20 |
| 978 | `src/Modules/Operations/Kmic.Operations.Application/IOperationsQueryDb.cs` | 保留 | 核对本人作业归属、上报校验/限流、合并游标与死信处理：保留按实例计数、敏感字段禁止上报、UUID 排序差异、放弃检查、逐条结果和载荷含密保护尚未实现的真实说明；不修业务缺口。 | 2026年11月14日 22:28:20 |
| 979 | `src/Modules/Operations/Kmic.Operations.Application/Kmic.Operations.Application.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 980 | `src/Modules/Operations/Kmic.Operations.Application/MergedCursorPaging.cs` | 保留 | 核对本人作业归属、上报校验/限流、合并游标与死信处理：保留按实例计数、敏感字段禁止上报、UUID 排序差异、放弃检查、逐条结果和载荷含密保护尚未实现的真实说明；不修业务缺口。 | 2026年11月14日 22:28:20 |
| 981 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Abandon/AbandonMessagesCommand.cs` | 保留 | 核对本人作业归属、上报校验/限流、合并游标与死信处理：保留按实例计数、敏感字段禁止上报、UUID 排序差异、放弃检查、逐条结果和载荷含密保护尚未实现的真实说明；不修业务缺口。 | 2026年11月14日 22:28:21 |
| 982 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Abandon/AbandonMessagesCommandHandler.cs` | 保留 | 核对本人作业归属、上报校验/限流、合并游标与死信处理：保留按实例计数、敏感字段禁止上报、UUID 排序差异、放弃检查、逐条结果和载荷含密保护尚未实现的真实说明；不修业务缺口。 | 2026年11月14日 22:28:21 |
| 983 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Abandon/AbandonMessagesCommandValidator.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 984 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Blocked/BlockedKeyItem.cs` | 保留 | 核对本人作业归属、上报校验/限流、合并游标与死信处理：保留按实例计数、敏感字段禁止上报、UUID 排序差异、放弃检查、逐条结果和载荷含密保护尚未实现的真实说明；不修业务缺口。 | 2026年11月14日 22:28:21 |
| 985 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Blocked/ListBlockedKeysQuery.cs` | 保留 | 核对本人作业归属、上报校验/限流、合并游标与死信处理：保留按实例计数、敏感字段禁止上报、UUID 排序差异、放弃检查、逐条结果和载荷含密保护尚未实现的真实说明；不修业务缺口。 | 2026年11月14日 22:28:21 |
| 986 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Blocked/ListBlockedKeysQueryHandler.cs` | 保留 | 核对本人作业归属、上报校验/限流、合并游标与死信处理：保留按实例计数、敏感字段禁止上报、UUID 排序差异、放弃检查、逐条结果和载荷含密保护尚未实现的真实说明；不修业务缺口。 | 2026年11月14日 22:28:21 |
| 987 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Dead/GetDeadMessageQuery.cs` | 保留 | 核对本人作业归属、上报校验/限流、合并游标与死信处理：保留按实例计数、敏感字段禁止上报、UUID 排序差异、放弃检查、逐条结果和载荷含密保护尚未实现的真实说明；不修业务缺口。 | 2026年11月14日 22:28:21 |
| 988 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Dead/GetDeadMessageQueryHandler.cs` | 保留 | 核对本人作业归属、上报校验/限流、合并游标与死信处理：保留按实例计数、敏感字段禁止上报、UUID 排序差异、放弃检查、逐条结果和载荷含密保护尚未实现的真实说明；不修业务缺口。 | 2026年11月14日 22:28:21 |
| 989 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Dead/ListDeadMessagesQuery.cs` | 保留 | 核对本人作业归属、上报校验/限流、合并游标与死信处理：保留按实例计数、敏感字段禁止上报、UUID 排序差异、放弃检查、逐条结果和载荷含密保护尚未实现的真实说明；不修业务缺口。 | 2026年11月14日 22:28:21 |
| 990 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Dead/ListDeadMessagesQueryHandler.cs` | 保留 | 核对本人作业归属、上报校验/限流、合并游标与死信处理：保留按实例计数、敏感字段禁止上报、UUID 排序差异、放弃检查、逐条结果和载荷含密保护尚未实现的真实说明；不修业务缺口。 | 2026年11月14日 22:28:21 |
| 991 | `src/Modules/Operations/Kmic.Operations.Application/Messages/DeadMessageDetailItem.cs` | 保留 | 核对本人作业归属、上报校验/限流、合并游标与死信处理：保留按实例计数、敏感字段禁止上报、UUID 排序差异、放弃检查、逐条结果和载荷含密保护尚未实现的真实说明；不修业务缺口。 | 2026年11月14日 22:28:22 |
| 992 | `src/Modules/Operations/Kmic.Operations.Application/Messages/DeadMessageItem.cs` | 保留 | 核对消息 DTO、心跳解析、重建命令、重试和路由终结映射：保留收件箱行 ID 区别、心跳有效期、未完成消息/处理中限制与逐条结果；用具体模块说明事务边界。 | 2026年11月14日 22:28:40 |
| 993 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Instances/InstanceItem.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 994 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Instances/InstanceSubscription.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 995 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Instances/InstanceSubscriptions.cs` | 保留 | 核对消息 DTO、心跳解析、重建命令、重试和路由终结映射：保留收件箱行 ID 区别、心跳有效期、未完成消息/处理中限制与逐条结果；用具体模块说明事务边界。 | 2026年11月14日 22:28:40 |
| 996 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Instances/ListInstancesQuery.cs` | 保留 | 核对消息 DTO、心跳解析、重建命令、重试和路由终结映射：保留收件箱行 ID 区别、心跳有效期、未完成消息/处理中限制与逐条结果；用具体模块说明事务边界。 | 2026年11月14日 22:28:40 |
| 997 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Instances/ListInstancesQueryHandler.cs` | 保留 | 核对消息 DTO、心跳解析、重建命令、重试和路由终结映射：保留收件箱行 ID 区别、心跳有效期、未完成消息/处理中限制与逐条结果；用具体模块说明事务边界。 | 2026年11月14日 22:28:40 |
| 998 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Instances/MessagingInstanceRow.cs` | 保留 | 核对消息 DTO、心跳解析、重建命令、重试和路由终结映射：保留收件箱行 ID 区别、心跳有效期、未完成消息/处理中限制与逐条结果；用具体模块说明事务边界。 | 2026年11月14日 22:28:40 |
| 999 | `src/Modules/Operations/Kmic.Operations.Application/Messages/MessageItems.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1000 | `src/Modules/Operations/Kmic.Operations.Application/Messages/MessageLimits.cs` | 保留 | 核对消息 DTO、心跳解析、重建命令、重试和路由终结映射：保留收件箱行 ID 区别、心跳有效期、未完成消息/处理中限制与逐条结果；用具体模块说明事务边界。 | 2026年11月14日 22:28:41 |
| 1001 | `src/Modules/Operations/Kmic.Operations.Application/Messages/MessageReceipt.cs` | 保留 | 核对消息 DTO、心跳解析、重建命令、重试和路由终结映射：保留收件箱行 ID 区别、心跳有效期、未完成消息/处理中限制与逐条结果；用具体模块说明事务边界。 | 2026年11月14日 22:28:41 |
| 1002 | `src/Modules/Operations/Kmic.Operations.Application/Messages/MessageReceipts.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1003 | `src/Modules/Operations/Kmic.Operations.Application/Messages/MessageRef.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1004 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Rebuilds/AbandonRebuildCommand.cs` | 保留 | 核对消息 DTO、心跳解析、重建命令、重试和路由终结映射：保留收件箱行 ID 区别、心跳有效期、未完成消息/处理中限制与逐条结果；用具体模块说明事务边界。 | 2026年11月14日 22:28:41 |
| 1005 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Rebuilds/AbandonRebuildCommandHandler.cs` | 保留 | 核对消息 DTO、心跳解析、重建命令、重试和路由终结映射：保留收件箱行 ID 区别、心跳有效期、未完成消息/处理中限制与逐条结果；用具体模块说明事务边界。 | 2026年11月14日 22:28:41 |
| 1006 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Rebuilds/AbandonRebuildCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 1007 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Rebuilds/ConfirmRebuildCommand.cs` | 保留 | 核对消息 DTO、心跳解析、重建命令、重试和路由终结映射：保留收件箱行 ID 区别、心跳有效期、未完成消息/处理中限制与逐条结果；用具体模块说明事务边界。 | 2026年11月14日 22:28:41 |
| 1008 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Rebuilds/ConfirmRebuildCommandHandler.cs` | 保留 | 核对消息 DTO、心跳解析、重建命令、重试和路由终结映射：保留收件箱行 ID 区别、心跳有效期、未完成消息/处理中限制与逐条结果；用具体模块说明事务边界。 | 2026年11月14日 22:28:41 |
| 1009 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Rebuilds/ConfirmRebuildCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 1010 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Rebuilds/ListRebuildsQuery.cs` | 保留 | 核对消息 DTO、心跳解析、重建命令、重试和路由终结映射：保留收件箱行 ID 区别、心跳有效期、未完成消息/处理中限制与逐条结果；用具体模块说明事务边界。 | 2026年11月14日 22:28:41 |
| 1011 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Rebuilds/ListRebuildsQueryHandler.cs` | 保留 | 核对消息 DTO、心跳解析、重建命令、重试和路由终结映射：保留收件箱行 ID 区别、心跳有效期、未完成消息/处理中限制与逐条结果；用具体模块说明事务边界。 | 2026年11月14日 22:28:41 |
| 1012 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Rebuilds/RebuildItem.cs` | 保留 | 核对消息 DTO、心跳解析、重建命令、重试和路由终结映射：保留收件箱行 ID 区别、心跳有效期、未完成消息/处理中限制与逐条结果；用具体模块说明事务边界。 | 2026年11月14日 22:28:41 |
| 1013 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Rebuilds/RebuildOutcomes.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1014 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Rebuilds/TriggerRebuildCommand.cs` | 保留 | 核对消息 DTO、心跳解析、重建命令、重试和路由终结映射：保留收件箱行 ID 区别、心跳有效期、未完成消息/处理中限制与逐条结果；用具体模块说明事务边界。 | 2026年11月14日 22:28:42 |
| 1015 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Rebuilds/TriggerRebuildCommandHandler.cs` | 已修订 | 核对消息 DTO、心跳解析、重建命令、重试和路由终结映射：保留收件箱行 ID 区别、心跳有效期、未完成消息/处理中限制与逐条结果；用具体模块说明事务边界。；本阶段 C2219 | 2026年11月14日 22:28:42 |
| 1016 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Rebuilds/TriggerRebuildCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 1017 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Retry/RetryMessagesCommand.cs` | 保留 | 核对消息 DTO、心跳解析、重建命令、重试和路由终结映射：保留收件箱行 ID 区别、心跳有效期、未完成消息/处理中限制与逐条结果；用具体模块说明事务边界。 | 2026年11月14日 22:28:42 |
| 1018 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Retry/RetryMessagesCommandHandler.cs` | 保留 | 核对消息 DTO、心跳解析、重建命令、重试和路由终结映射：保留收件箱行 ID 区别、心跳有效期、未完成消息/处理中限制与逐条结果；用具体模块说明事务边界。 | 2026年11月14日 22:28:42 |
| 1019 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Retry/RetryMessagesCommandValidator.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1020 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/FinalizeRouteCommand.cs` | 保留 | 核对消息 DTO、心跳解析、重建命令、重试和路由终结映射：保留收件箱行 ID 区别、心跳有效期、未完成消息/处理中限制与逐条结果；用具体模块说明事务边界。 | 2026年11月14日 22:28:42 |
| 1021 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/FinalizeRouteCommandHandler.cs` | 保留 | 核对消息 DTO、心跳解析、重建命令、重试和路由终结映射：保留收件箱行 ID 区别、心跳有效期、未完成消息/处理中限制与逐条结果；用具体模块说明事务边界。 | 2026年11月14日 22:28:42 |
| 1022 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/FinalizeRouteCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 1023 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/FinalizeRouteResponse.cs` | 保留 | 核对消息 DTO、心跳解析、重建命令、重试和路由终结映射：保留收件箱行 ID 区别、心跳有效期、未完成消息/处理中限制与逐条结果；用具体模块说明事务边界。 | 2026年11月14日 22:28:43 |
| 1024 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/ListRoutesQuery.cs` | 保留 | 核对路由列表、补投结果、积压标识、权限目录及任务状态投影：保留心跳声明范围、重激活顺序、双端包含边界和实际/配置调度差异；删去复述特性语法的说明。 | 2026年11月14日 22:28:58 |
| 1025 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/ListRoutesQueryHandler.cs` | 保留 | 核对路由列表、补投结果、积压标识、权限目录及任务状态投影：保留心跳声明范围、重激活顺序、双端包含边界和实际/配置调度差异；删去复述特性语法的说明。 | 2026年11月14日 22:28:58 |
| 1026 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/MessageRouteRow.cs` | 保留 | 核对路由列表、补投结果、积压标识、权限目录及任务状态投影：保留心跳声明范围、重激活顺序、双端包含边界和实际/配置调度差异；删去复述特性语法的说明。 | 2026年11月14日 22:28:58 |
| 1027 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/ReactivateRouteCommand.cs` | 保留 | 核对路由列表、补投结果、积压标识、权限目录及任务状态投影：保留心跳声明范围、重激活顺序、双端包含边界和实际/配置调度差异；删去复述特性语法的说明。 | 2026年11月14日 22:28:58 |
| 1028 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/ReactivateRouteCommandHandler.cs` | 保留 | 核对路由列表、补投结果、积压标识、权限目录及任务状态投影：保留心跳声明范围、重激活顺序、双端包含边界和实际/配置调度差异；删去复述特性语法的说明。 | 2026年11月14日 22:28:59 |
| 1029 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/ReactivateRouteCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 1030 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/RedeliverCommand.cs` | 保留 | 核对路由列表、补投结果、积压标识、权限目录及任务状态投影：保留心跳声明范围、重激活顺序、双端包含边界和实际/配置调度差异；删去复述特性语法的说明。 | 2026年11月14日 22:28:59 |
| 1031 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/RedeliverCommandHandler.cs` | 保留 | 核对路由列表、补投结果、积压标识、权限目录及任务状态投影：保留心跳声明范围、重激活顺序、双端包含边界和实际/配置调度差异；删去复述特性语法的说明。 | 2026年11月14日 22:28:59 |
| 1032 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/RedeliverCommandValidator.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1033 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/RedeliverResponse.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1034 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/RouteBuild.cs` | 保留 | 核对路由列表、补投结果、积压标识、权限目录及任务状态投影：保留心跳声明范围、重激活顺序、双端包含边界和实际/配置调度差异；删去复述特性语法的说明。 | 2026年11月14日 22:28:59 |
| 1035 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/RouteItem.cs` | 保留 | 核对路由列表、补投结果、积压标识、权限目录及任务状态投影：保留心跳声明范围、重激活顺序、双端包含边界和实际/配置调度差异；删去复述特性语法的说明。 | 2026年11月14日 22:28:59 |
| 1036 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Summary/GetMessageSummaryQuery.cs` | 保留 | 核对路由列表、补投结果、积压标识、权限目录及任务状态投影：保留心跳声明范围、重激活顺序、双端包含边界和实际/配置调度差异；删去复述特性语法的说明。 | 2026年11月14日 22:28:59 |
| 1037 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Summary/GetMessageSummaryQueryHandler.cs` | 保留 | 核对路由列表、补投结果、积压标识、权限目录及任务状态投影：保留心跳声明范围、重激活顺序、双端包含边界和实际/配置调度差异；删去复述特性语法的说明。 | 2026年11月14日 22:28:59 |
| 1038 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Summary/MessageSummaryResponse.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1039 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Summary/ModuleBacklogItem.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1040 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Summary/OutboxBacklogItem.cs` | 保留 | 核对路由列表、补投结果、积压标识、权限目录及任务状态投影：保留心跳声明范围、重激活顺序、双端包含边界和实际/配置调度差异；删去复述特性语法的说明。 | 2026年11月14日 22:28:59 |
| 1041 | `src/Modules/Operations/Kmic.Operations.Application/Messages/Summary/SubscriptionBacklogItem.cs` | 保留 | 核对路由列表、补投结果、积压标识、权限目录及任务状态投影：保留心跳声明范围、重激活顺序、双端包含边界和实际/配置调度差异；删去复述特性语法的说明。 | 2026年11月14日 22:28:59 |
| 1042 | `src/Modules/Operations/Kmic.Operations.Application/OperationsPermissions.cs` | 已修订 | 核对路由列表、补投结果、积压标识、权限目录及任务状态投影：保留心跳声明范围、重激活顺序、双端包含边界和实际/配置调度差异；删去复述特性语法的说明。；本阶段 C2220 | 2026年11月14日 22:29:00 |
| 1043 | `src/Modules/Operations/Kmic.Operations.Application/OperationsScopes.cs` | 保留 | 核对路由列表、补投结果、积压标识、权限目录及任务状态投影：保留心跳声明范围、重激活顺序、双端包含边界和实际/配置调度差异；删去复述特性语法的说明。 | 2026年11月14日 22:29:00 |
| 1044 | `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/CronRules.cs` | 沿用已复核 | 上轮已完整读取关联文件并核实修改，本轮内容未变；原编号 C2192。 | 2026年11月14日 22:12:28 |
| 1045 | `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/List/ListScheduledJobsQuery.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1046 | `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/List/ListScheduledJobsQueryHandler.cs` | 保留 | 核对路由列表、补投结果、积压标识、权限目录及任务状态投影：保留心跳声明范围、重激活顺序、双端包含边界和实际/配置调度差异；删去复述特性语法的说明。 | 2026年11月14日 22:29:00 |
| 1047 | `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/List/ScheduledJobItem.cs` | 保留 | 核对路由列表、补投结果、积压标识、权限目录及任务状态投影：保留心跳声明范围、重激活顺序、双端包含边界和实际/配置调度差异；删去复述特性语法的说明。 | 2026年11月14日 22:29:00 |
| 1048 | `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/Preview/CronPreviewResponse.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1049 | `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/Preview/PreviewCronQuery.cs` | 沿用已复核 | 上轮已完整读取关联文件并核实修改，本轮内容未变；原编号 C2190。 | 2026年11月14日 22:12:28 |
| 1050 | `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/Preview/PreviewCronQueryHandler.cs` | 保留 | 核对路由列表、补投结果、积压标识、权限目录及任务状态投影：保留心跳声明范围、重激活顺序、双端包含边界和实际/配置调度差异；删去复述特性语法的说明。 | 2026年11月14日 22:29:00 |
| 1051 | `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/Preview/PreviewCronQueryValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 1052 | `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/Restore/RestoreJobDefaultCommand.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1053 | `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/Restore/RestoreJobDefaultCommandHandler.cs` | 保留 | 核对路由列表、补投结果、积压标识、权限目录及任务状态投影：保留心跳声明范围、重激活顺序、双端包含边界和实际/配置调度差异；删去复述特性语法的说明。 | 2026年11月14日 22:29:00 |
| 1054 | `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/Restore/RestoreJobDefaultCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 1055 | `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/RunNow/RunJobNowCommand.cs` | 保留 | 核对路由列表、补投结果、积压标识、权限目录及任务状态投影：保留心跳声明范围、重激活顺序、双端包含边界和实际/配置调度差异；删去复述特性语法的说明。 | 2026年11月14日 22:29:00 |
| 1056 | `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/RunNow/RunJobNowCommandHandler.cs` | 保留 | 核对调度触发、设置保存/恢复和状态转换：保留独立触发不可撤回、提交后同步失败不回滚、结果未知先核实、重建待确认等说明；实体属性和错误消息准确清楚，保留。 | 2026年11月14日 22:29:19 |
| 1057 | `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/RunNow/RunJobNowCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 1058 | `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/Runs/ListScheduledJobRunsQuery.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1059 | `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/Runs/ListScheduledJobRunsQueryHandler.cs` | 保留 | 核对调度触发、设置保存/恢复和状态转换：保留独立触发不可撤回、提交后同步失败不回滚、结果未知先核实、重建待确认等说明；实体属性和错误消息准确清楚，保留。 | 2026年11月14日 22:29:19 |
| 1060 | `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/Runs/ListScheduledJobRunsQueryValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 1061 | `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/ScheduledJobAction.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1062 | `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/ScheduledJobActions.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1063 | `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/ScheduledJobPolicy.cs` | 保留 | 核对调度触发、设置保存/恢复和状态转换：保留独立触发不可撤回、提交后同步失败不回滚、结果未知先核实、重建待确认等说明；实体属性和错误消息准确清楚，保留。 | 2026年11月14日 22:29:19 |
| 1064 | `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/ScheduledJobRunItem.cs` | 保留 | 核对调度触发、设置保存/恢复和状态转换：保留独立触发不可撤回、提交后同步失败不回滚、结果未知先核实、重建待确认等说明；实体属性和错误消息准确清楚，保留。 | 2026年11月14日 22:29:19 |
| 1065 | `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/ScheduledJobRunItems.cs` | 保留 | 核对调度触发、设置保存/恢复和状态转换：保留独立触发不可撤回、提交后同步失败不回滚、结果未知先核实、重建待确认等说明；实体属性和错误消息准确清楚，保留。 | 2026年11月14日 22:29:19 |
| 1066 | `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/ScheduledJobRunRow.cs` | 保留 | 核对调度触发、设置保存/恢复和状态转换：保留独立触发不可撤回、提交后同步失败不回滚、结果未知先核实、重建待确认等说明；实体属性和错误消息准确清楚，保留。 | 2026年11月14日 22:29:20 |
| 1067 | `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/ScheduledJobTarget.cs` | 保留 | 核对调度触发、设置保存/恢复和状态转换：保留独立触发不可撤回、提交后同步失败不回滚、结果未知先核实、重建待确认等说明；实体属性和错误消息准确清楚，保留。 | 2026年11月14日 22:29:20 |
| 1068 | `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/Set/SetJobOverrideCommand.cs` | 保留 | 核对调度触发、设置保存/恢复和状态转换：保留独立触发不可撤回、提交后同步失败不回滚、结果未知先核实、重建待确认等说明；实体属性和错误消息准确清楚，保留。 | 2026年11月14日 22:29:20 |
| 1069 | `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/Set/SetJobOverrideCommandHandler.cs` | 保留 | 核对调度触发、设置保存/恢复和状态转换：保留独立触发不可撤回、提交后同步失败不回滚、结果未知先核实、重建待确认等说明；实体属性和错误消息准确清楚，保留。 | 2026年11月14日 22:29:20 |
| 1070 | `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/Set/SetJobOverrideCommandValidator.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 1071 | `src/Modules/Operations/Kmic.Operations.Application/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 1072 | `src/Modules/Operations/Kmic.Operations.Contracts/Kmic.Operations.Contracts.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1073 | `src/Modules/Operations/Kmic.Operations.Contracts/OperationsModuleInfo.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1074 | `src/Modules/Operations/Kmic.Operations.Contracts/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 1075 | `src/Modules/Operations/Kmic.Operations.Domain/Audit/AuditLog.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1076 | `src/Modules/Operations/Kmic.Operations.Domain/Audit/AuditLogId.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1077 | `src/Modules/Operations/Kmic.Operations.Domain/Audit/IAuditRepository.cs` | 保留 | 核对调度触发、设置保存/恢复和状态转换：保留独立触发不可撤回、提交后同步失败不回滚、结果未知先核实、重建待确认等说明；实体属性和错误消息准确清楚，保留。 | 2026年11月14日 22:29:20 |
| 1078 | `src/Modules/Operations/Kmic.Operations.Domain/Errors/OperationsErrors.cs` | 保留 | 核对调度触发、设置保存/恢复和状态转换：保留独立触发不可撤回、提交后同步失败不回滚、结果未知先核实、重建待确认等说明；实体属性和错误消息准确清楚，保留。 | 2026年11月14日 22:29:20 |
| 1079 | `src/Modules/Operations/Kmic.Operations.Domain/Jobs/BackgroundJobStatus.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1080 | `src/Modules/Operations/Kmic.Operations.Domain/Jobs/IScheduledJobSettingRepository.cs` | 保留 | 核对调度触发、设置保存/恢复和状态转换：保留独立触发不可撤回、提交后同步失败不回滚、结果未知先核实、重建待确认等说明；实体属性和错误消息准确清楚，保留。 | 2026年11月14日 22:29:20 |
| 1081 | `src/Modules/Operations/Kmic.Operations.Domain/Jobs/JobRunStatus.cs` | 保留 | 核对调度触发、设置保存/恢复和状态转换：保留独立触发不可撤回、提交后同步失败不回滚、结果未知先核实、重建待确认等说明；实体属性和错误消息准确清楚，保留。 | 2026年11月14日 22:29:20 |
| 1082 | `src/Modules/Operations/Kmic.Operations.Domain/Jobs/JobRunTrigger.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1083 | `src/Modules/Operations/Kmic.Operations.Domain/Jobs/ScheduledJobSetting.cs` | 保留 | 核对调度触发、设置保存/恢复和状态转换：保留独立触发不可撤回、提交后同步失败不回滚、结果未知先核实、重建待确认等说明；实体属性和错误消息准确清楚，保留。 | 2026年11月14日 22:29:21 |
| 1084 | `src/Modules/Operations/Kmic.Operations.Domain/Jobs/ScheduledJobSettingId.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1085 | `src/Modules/Operations/Kmic.Operations.Domain/Kmic.Operations.Domain.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1086 | `src/Modules/Operations/Kmic.Operations.Domain/Messages/MessageBoxKind.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1087 | `src/Modules/Operations/Kmic.Operations.Domain/Messages/MessageReceiptStatus.cs` | 保留 | 核对调度触发、设置保存/恢复和状态转换：保留独立触发不可撤回、提交后同步失败不回滚、结果未知先核实、重建待确认等说明；实体属性和错误消息准确清楚，保留。 | 2026年11月14日 22:29:21 |
| 1088 | `src/Modules/Operations/Kmic.Operations.Domain/Messages/MessageRouteStatus.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1089 | `src/Modules/Operations/Kmic.Operations.Domain/Messages/MessageState.cs` | 保留 | 核对调度触发、设置保存/恢复和状态转换：保留独立触发不可撤回、提交后同步失败不回滚、结果未知先核实、重建待确认等说明；实体属性和错误消息准确清楚，保留。 | 2026年11月14日 22:29:21 |
| 1090 | `src/Modules/Operations/Kmic.Operations.Domain/Messages/RebuildRunStatus.cs` | 保留 | 核对调度触发、设置保存/恢复和状态转换：保留独立触发不可撤回、提交后同步失败不回滚、结果未知先核实、重建待确认等说明；实体属性和错误消息准确清楚，保留。 | 2026年11月14日 22:29:21 |
| 1091 | `src/Modules/Operations/Kmic.Operations.Domain/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 1092 | `src/Modules/Operations/Kmic.Operations.Infrastructure/ClientErrors/ClientErrorLimiter.cs` | 保留 | 核对调度触发、设置保存/恢复和状态转换：保留独立触发不可撤回、提交后同步失败不回滚、结果未知先核实、重建待确认等说明；实体属性和错误消息准确清楚，保留。 | 2026年11月14日 22:29:21 |
| 1093 | `src/Modules/Operations/Kmic.Operations.Infrastructure/ClientErrors/ClientErrorLog.cs` | 保留 | 核对运维注册、日志写入和审计/作业/错误上报端点：保留载荷不可进日志模板、模块事务、取消信号与下载权限；精简个人作业标题并补明限流按实例计数。 | 2026年11月14日 22:29:48 |
| 1094 | `src/Modules/Operations/Kmic.Operations.Infrastructure/Kmic.Operations.Infrastructure.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1095 | `src/Modules/Operations/Kmic.Operations.Infrastructure/OperationsModule.cs` | 保留 | 核对运维注册、日志写入和审计/作业/错误上报端点：保留载荷不可进日志模板、模块事务、取消信号与下载权限；精简个人作业标题并补明限流按实例计数。 | 2026年11月14日 22:29:48 |
| 1096 | `src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/Configurations/AuditLogConfiguration.cs` | 沿用已复核 | 沿用上轮逐文件结论（模型说明保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1097 | `src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/Configurations/ScheduledJobSettingConfiguration.cs` | 沿用已复核 | 沿用上轮逐文件结论（模型说明保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1098 | `src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/OperationsDbContext.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1099 | `src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/OperationsDbContextFactory.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1100 | `src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/OperationsIdConverters.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 1101 | `src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/OperationsModel.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 1102 | `src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/OperationsQueryDbContext.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1103 | `src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/Repositories/AuditRepository.cs` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 1104 | `src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/Repositories/ScheduledJobSettingRepository.cs` | 保留 | 核对运维注册、日志写入和审计/作业/错误上报端点：保留载荷不可进日志模板、模块事务、取消信号与下载权限；精简个人作业标题并补明限流按实例计数。 | 2026年11月14日 22:29:48 |
| 1105 | `src/Modules/Operations/Kmic.Operations.Infrastructure/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 1106 | `src/Modules/Operations/Kmic.Operations.Presentation/Audit/DownloadAuditExportEndpoint.cs` | 保留 | 核对运维注册、日志写入和审计/作业/错误上报端点：保留载荷不可进日志模板、模块事务、取消信号与下载权限；精简个人作业标题并补明限流按实例计数。 | 2026年11月14日 22:29:49 |
| 1107 | `src/Modules/Operations/Kmic.Operations.Presentation/Audit/ExportAuditEndpoint.cs` | 保留 | 核对运维注册、日志写入和审计/作业/错误上报端点：保留载荷不可进日志模板、模块事务、取消信号与下载权限；精简个人作业标题并补明限流按实例计数。 | 2026年11月14日 22:29:49 |
| 1108 | `src/Modules/Operations/Kmic.Operations.Presentation/Audit/ExportAuditRequest.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1109 | `src/Modules/Operations/Kmic.Operations.Presentation/Audit/GetAuditEndpoint.cs` | 保留 | 核对运维注册、日志写入和审计/作业/错误上报端点：保留载荷不可进日志模板、模块事务、取消信号与下载权限；精简个人作业标题并补明限流按实例计数。 | 2026年11月14日 22:29:49 |
| 1110 | `src/Modules/Operations/Kmic.Operations.Presentation/Audit/ListAuditEndpoint.cs` | 保留 | 核对运维注册、日志写入和审计/作业/错误上报端点：保留载荷不可进日志模板、模块事务、取消信号与下载权限；精简个人作业标题并补明限流按实例计数。 | 2026年11月14日 22:29:49 |
| 1111 | `src/Modules/Operations/Kmic.Operations.Presentation/Audit/ListAuditRequest.cs` | 保留 | 核对运维注册、日志写入和审计/作业/错误上报端点：保留载荷不可进日志模板、模块事务、取消信号与下载权限；精简个人作业标题并补明限流按实例计数。 | 2026年11月14日 22:29:49 |
| 1112 | `src/Modules/Operations/Kmic.Operations.Presentation/BackgroundJobs/CancelBackgroundJobEndpoint.cs` | 保留 | 核对运维注册、日志写入和审计/作业/错误上报端点：保留载荷不可进日志模板、模块事务、取消信号与下载权限；精简个人作业标题并补明限流按实例计数。 | 2026年11月14日 22:29:49 |
| 1113 | `src/Modules/Operations/Kmic.Operations.Presentation/BackgroundJobs/CancelMyJobEndpoint.cs` | 已修订 | 核对运维注册、日志写入和审计/作业/错误上报端点：保留载荷不可进日志模板、模块事务、取消信号与下载权限；精简个人作业标题并补明限流按实例计数。；本阶段 C2221 | 2026年11月14日 22:29:49 |
| 1114 | `src/Modules/Operations/Kmic.Operations.Presentation/BackgroundJobs/GetMyJobEndpoint.cs` | 已修订 | 核对运维注册、日志写入和审计/作业/错误上报端点：保留载荷不可进日志模板、模块事务、取消信号与下载权限；精简个人作业标题并补明限流按实例计数。；本阶段 C2222 | 2026年11月14日 22:29:50 |
| 1115 | `src/Modules/Operations/Kmic.Operations.Presentation/BackgroundJobs/ListBackgroundJobsEndpoint.cs` | 保留 | 核对运维注册、日志写入和审计/作业/错误上报端点：保留载荷不可进日志模板、模块事务、取消信号与下载权限；精简个人作业标题并补明限流按实例计数。 | 2026年11月14日 22:29:50 |
| 1116 | `src/Modules/Operations/Kmic.Operations.Presentation/BackgroundJobs/ListBackgroundJobsRequest.cs` | 保留 | 核对运维注册、日志写入和审计/作业/错误上报端点：保留载荷不可进日志模板、模块事务、取消信号与下载权限；精简个人作业标题并补明限流按实例计数。 | 2026年11月14日 22:29:50 |
| 1117 | `src/Modules/Operations/Kmic.Operations.Presentation/BackgroundJobs/ListMyJobsEndpoint.cs` | 保留 | 核对运维注册、日志写入和审计/作业/错误上报端点：保留载荷不可进日志模板、模块事务、取消信号与下载权限；精简个人作业标题并补明限流按实例计数。 | 2026年11月14日 22:29:50 |
| 1118 | `src/Modules/Operations/Kmic.Operations.Presentation/BackgroundJobs/ListMyJobsRequest.cs` | 已修订 | 核对运维注册、日志写入和审计/作业/错误上报端点：保留载荷不可进日志模板、模块事务、取消信号与下载权限；精简个人作业标题并补明限流按实例计数。；本阶段 C2223 | 2026年11月14日 22:29:50 |
| 1119 | `src/Modules/Operations/Kmic.Operations.Presentation/ClientErrors/ReportClientErrorsEndpoint.cs` | 已修订 | 核对运维注册、日志写入和审计/作业/错误上报端点：保留载荷不可进日志模板、模块事务、取消信号与下载权限；精简个人作业标题并补明限流按实例计数。；本阶段 C2224 | 2026年11月14日 22:29:50 |
| 1120 | `src/Modules/Operations/Kmic.Operations.Presentation/ClientErrors/ReportClientErrorsRequest.cs` | 保留 | 核对运维注册、日志写入和审计/作业/错误上报端点：保留载荷不可进日志模板、模块事务、取消信号与下载权限；精简个人作业标题并补明限流按实例计数。 | 2026年11月14日 22:29:51 |
| 1121 | `src/Modules/Operations/Kmic.Operations.Presentation/Kmic.Operations.Presentation.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1122 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/AbandonMessagesEndpoint.cs` | 保留 | 核对运维注册、日志写入和审计/作业/错误上报端点：保留载荷不可进日志模板、模块事务、取消信号与下载权限；精简个人作业标题并补明限流按实例计数。 | 2026年11月14日 22:29:51 |
| 1123 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/AbandonMessagesRequest.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1124 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/AbandonRebuildEndpoint.cs` | 保留 | 核对运维接口到命令、查询及状态投影：保留维护窗口、消息占用、人工确认、重试不保证成功和提交后同步边界；精简任务响应字段复述，准确说明 isDeclared。 | 2026年11月14日 22:30:13 |
| 1125 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/ConfirmRebuildEndpoint.cs` | 保留 | 核对运维接口到命令、查询及状态投影：保留维护窗口、消息占用、人工确认、重试不保证成功和提交后同步边界；精简任务响应字段复述，准确说明 isDeclared。 | 2026年11月14日 22:30:13 |
| 1126 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/FinalizeRouteEndpoint.cs` | 保留 | 核对运维接口到命令、查询及状态投影：保留维护窗口、消息占用、人工确认、重试不保证成功和提交后同步边界；精简任务响应字段复述，准确说明 isDeclared。 | 2026年11月14日 22:30:13 |
| 1127 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/FinalizeRouteRequest.cs` | 保留 | 核对运维接口到命令、查询及状态投影：保留维护窗口、消息占用、人工确认、重试不保证成功和提交后同步边界；精简任务响应字段复述，准确说明 isDeclared。 | 2026年11月14日 22:30:13 |
| 1128 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/GetDeadMessageEndpoint.cs` | 保留 | 核对运维接口到命令、查询及状态投影：保留维护窗口、消息占用、人工确认、重试不保证成功和提交后同步边界；精简任务响应字段复述，准确说明 isDeclared。 | 2026年11月14日 22:30:13 |
| 1129 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/GetMessageSummaryEndpoint.cs` | 保留 | 核对运维接口到命令、查询及状态投影：保留维护窗口、消息占用、人工确认、重试不保证成功和提交后同步边界；精简任务响应字段复述，准确说明 isDeclared。 | 2026年11月14日 22:30:13 |
| 1130 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/ListBlockedKeysEndpoint.cs` | 保留 | 核对运维接口到命令、查询及状态投影：保留维护窗口、消息占用、人工确认、重试不保证成功和提交后同步边界；精简任务响应字段复述，准确说明 isDeclared。 | 2026年11月14日 22:30:13 |
| 1131 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/ListDeadMessagesEndpoint.cs` | 保留 | 核对运维接口到命令、查询及状态投影：保留维护窗口、消息占用、人工确认、重试不保证成功和提交后同步边界；精简任务响应字段复述，准确说明 isDeclared。 | 2026年11月14日 22:30:14 |
| 1132 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/ListDeadMessagesRequest.cs` | 保留 | 核对运维接口到命令、查询及状态投影：保留维护窗口、消息占用、人工确认、重试不保证成功和提交后同步边界；精简任务响应字段复述，准确说明 isDeclared。 | 2026年11月14日 22:30:14 |
| 1133 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/ListInstancesEndpoint.cs` | 保留 | 核对运维接口到命令、查询及状态投影：保留维护窗口、消息占用、人工确认、重试不保证成功和提交后同步边界；精简任务响应字段复述，准确说明 isDeclared。 | 2026年11月14日 22:30:14 |
| 1134 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/ListRebuildsEndpoint.cs` | 保留 | 核对运维接口到命令、查询及状态投影：保留维护窗口、消息占用、人工确认、重试不保证成功和提交后同步边界；精简任务响应字段复述，准确说明 isDeclared。 | 2026年11月14日 22:30:14 |
| 1135 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/ListRoutesEndpoint.cs` | 保留 | 核对运维接口到命令、查询及状态投影：保留维护窗口、消息占用、人工确认、重试不保证成功和提交后同步边界；精简任务响应字段复述，准确说明 isDeclared。 | 2026年11月14日 22:30:14 |
| 1136 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/ReactivateRouteEndpoint.cs` | 保留 | 核对运维接口到命令、查询及状态投影：保留维护窗口、消息占用、人工确认、重试不保证成功和提交后同步边界；精简任务响应字段复述，准确说明 isDeclared。 | 2026年11月14日 22:30:14 |
| 1137 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/ReactivateRouteRequest.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1138 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/RebuildRequest.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1139 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/RedeliverEndpoint.cs` | 保留 | 核对运维接口到命令、查询及状态投影：保留维护窗口、消息占用、人工确认、重试不保证成功和提交后同步边界；精简任务响应字段复述，准确说明 isDeclared。 | 2026年11月14日 22:30:14 |
| 1140 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/RedeliverRequest.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1141 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/RetryMessagesEndpoint.cs` | 保留 | 核对运维接口到命令、查询及状态投影：保留维护窗口、消息占用、人工确认、重试不保证成功和提交后同步边界；精简任务响应字段复述，准确说明 isDeclared。 | 2026年11月14日 22:30:14 |
| 1142 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/RetryMessagesRequest.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1143 | `src/Modules/Operations/Kmic.Operations.Presentation/Messages/TriggerRebuildEndpoint.cs` | 保留 | 核对运维接口到命令、查询及状态投影：保留维护窗口、消息占用、人工确认、重试不保证成功和提交后同步边界；精简任务响应字段复述，准确说明 isDeclared。 | 2026年11月14日 22:30:14 |
| 1144 | `src/Modules/Operations/Kmic.Operations.Presentation/OperationsEndpoints.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1145 | `src/Modules/Operations/Kmic.Operations.Presentation/ScheduledJobs/ListScheduledJobRunsEndpoint.cs` | 保留 | 核对运维接口到命令、查询及状态投影：保留维护窗口、消息占用、人工确认、重试不保证成功和提交后同步边界；精简任务响应字段复述，准确说明 isDeclared。 | 2026年11月14日 22:30:15 |
| 1146 | `src/Modules/Operations/Kmic.Operations.Presentation/ScheduledJobs/ListScheduledJobRunsRequest.cs` | 保留 | 核对运维接口到命令、查询及状态投影：保留维护窗口、消息占用、人工确认、重试不保证成功和提交后同步边界；精简任务响应字段复述，准确说明 isDeclared。 | 2026年11月14日 22:30:15 |
| 1147 | `src/Modules/Operations/Kmic.Operations.Presentation/ScheduledJobs/ListScheduledJobsEndpoint.cs` | 已修订 | 核对运维接口到命令、查询及状态投影：保留维护窗口、消息占用、人工确认、重试不保证成功和提交后同步边界；精简任务响应字段复述，准确说明 isDeclared。；本阶段 C2225 | 2026年11月14日 22:30:15 |
| 1148 | `src/Modules/Operations/Kmic.Operations.Presentation/ScheduledJobs/PreviewCronEndpoint.cs` | 沿用已复核 | 上轮已完整读取关联文件并核实修改，本轮内容未变；原编号 C2191。 | 2026年11月14日 22:12:28 |
| 1149 | `src/Modules/Operations/Kmic.Operations.Presentation/ScheduledJobs/RestoreJobDefaultEndpoint.cs` | 保留 | 核对运维接口到命令、查询及状态投影：保留维护窗口、消息占用、人工确认、重试不保证成功和提交后同步边界；精简任务响应字段复述，准确说明 isDeclared。 | 2026年11月14日 22:30:15 |
| 1150 | `src/Modules/Operations/Kmic.Operations.Presentation/ScheduledJobs/RestoreJobDefaultRequest.cs` | 保留 | 核对运维接口到命令、查询及状态投影：保留维护窗口、消息占用、人工确认、重试不保证成功和提交后同步边界；精简任务响应字段复述，准确说明 isDeclared。 | 2026年11月14日 22:30:15 |
| 1151 | `src/Modules/Operations/Kmic.Operations.Presentation/ScheduledJobs/RunJobNowEndpoint.cs` | 保留 | 核对运维接口到命令、查询及状态投影：保留维护窗口、消息占用、人工确认、重试不保证成功和提交后同步边界；精简任务响应字段复述，准确说明 isDeclared。 | 2026年11月14日 22:30:15 |
| 1152 | `src/Modules/Operations/Kmic.Operations.Presentation/ScheduledJobs/SetJobOverrideEndpoint.cs` | 保留 | 核对运维接口到命令、查询及状态投影：保留维护窗口、消息占用、人工确认、重试不保证成功和提交后同步边界；精简任务响应字段复述，准确说明 isDeclared。 | 2026年11月14日 22:30:15 |
| 1153 | `src/Modules/Operations/Kmic.Operations.Presentation/ScheduledJobs/SetJobOverrideRequest.cs` | 保留 | 核对运维接口到命令、查询及状态投影：保留维护窗口、消息占用、人工确认、重试不保证成功和提交后同步边界；精简任务响应字段复述，准确说明 isDeclared。 | 2026年11月14日 22:30:15 |
| 1154 | `src/Modules/Operations/Kmic.Operations.Presentation/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 1155 | `src/Modules/Sample/Kmic.Sample.Application/ContractQueries/GetSpecimenReferencesQuery.cs` | 沿用已复核 | 上轮已完整读取关联文件并核实修改，本轮内容未变；原编号 C2130。 | 2026年11月14日 22:12:28 |
| 1156 | `src/Modules/Sample/Kmic.Sample.Application/ContractQueries/GetSpecimenReferencesQueryHandler.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:25 |
| 1157 | `src/Modules/Sample/Kmic.Sample.Application/ContractQueries/GetSpecimenReferencesQueryValidator.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:25 |
| 1158 | `src/Modules/Sample/Kmic.Sample.Application/ContractQueries/SpecimenReferences.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:25 |
| 1159 | `src/Modules/Sample/Kmic.Sample.Application/ISampleQueryDb.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:25 |
| 1160 | `src/Modules/Sample/Kmic.Sample.Application/Kmic.Sample.Application.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1161 | `src/Modules/Sample/Kmic.Sample.Application/Notifications/SpecimenAlertSms.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:25 |
| 1162 | `src/Modules/Sample/Kmic.Sample.Application/Notifications/SpecimenCode.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:26 |
| 1163 | `src/Modules/Sample/Kmic.Sample.Application/Notifications/SpecimenSubmittedNotice.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:26 |
| 1164 | `src/Modules/Sample/Kmic.Sample.Application/SampleDictionaries.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:26 |
| 1165 | `src/Modules/Sample/Kmic.Sample.Application/SampleListPageSize.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:26 |
| 1166 | `src/Modules/Sample/Kmic.Sample.Application/SampleNumberRules.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:26 |
| 1167 | `src/Modules/Sample/Kmic.Sample.Application/SamplePermissions.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:26 |
| 1168 | `src/Modules/Sample/Kmic.Sample.Application/SampleReportFooter.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:26 |
| 1169 | `src/Modules/Sample/Kmic.Sample.Application/SampleScopes.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:26 |
| 1170 | `src/Modules/Sample/Kmic.Sample.Application/SampleSubmissionSetting.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:27 |
| 1171 | `src/Modules/Sample/Kmic.Sample.Application/SampleUploadProfiles.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:27 |
| 1172 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/Attachments/Add/AddSpecimenAttachmentCommand.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:27 |
| 1173 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/Attachments/Add/AddSpecimenAttachmentCommandHandler.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:27 |
| 1174 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/Attachments/GetContent/GetSpecimenAttachmentContentQuery.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:27 |
| 1175 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/Attachments/GetContent/GetSpecimenAttachmentContentQueryHandler.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:27 |
| 1176 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/Attachments/Remove/RemoveSpecimenAttachmentCommand.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:27 |
| 1177 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/Attachments/Remove/RemoveSpecimenAttachmentCommandHandler.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:27 |
| 1178 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/Close/CloseSpecimenCommand.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:27 |
| 1179 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/Close/CloseSpecimenCommandHandler.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:28 |
| 1180 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/Create/CreateSpecimenCommand.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:28 |
| 1181 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/Create/CreateSpecimenCommandHandler.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:28 |
| 1182 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/Create/CreateSpecimenCommandValidator.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1183 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/Create/CreateSpecimenItemInput.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:28 |
| 1184 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/Delete/DeleteSpecimenCommand.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:28 |
| 1185 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/Delete/DeleteSpecimenCommandHandler.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:28 |
| 1186 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/EventHandlers/NotifyOwnerOfSubmission.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:29 |
| 1187 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/EventHandlers/PublishSpecimenSubmitted.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:29 |
| 1188 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/GetDetail/GetSpecimenDetailQuery.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:29 |
| 1189 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/GetDetail/GetSpecimenDetailQueryHandler.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:29 |
| 1190 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/GetDetail/SpecimenDetailResponse.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:30 |
| 1191 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/List/ListSpecimensQuery.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:30 |
| 1192 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/List/ListSpecimensQueryHandler.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:30 |
| 1193 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/List/SpecimenListItem.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:30 |
| 1194 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/List/SpecimenSort.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:30 |
| 1195 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/Shared/SpecimenAttachmentResponse.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:31 |
| 1196 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/Shared/SpecimenItemResponse.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:31 |
| 1197 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/SpecimenActions.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:31 |
| 1198 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/SpecimenPolicy.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:31 |
| 1199 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/Submit/SubmitSpecimenCommand.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:31 |
| 1200 | `src/Modules/Sample/Kmic.Sample.Application/Specimens/Submit/SubmitSpecimenCommandHandler.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:32 |
| 1201 | `src/Modules/Sample/Kmic.Sample.Application/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 1202 | `src/Modules/Sample/Kmic.Sample.Contracts/Events/SpecimenSubmitted.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:32 |
| 1203 | `src/Modules/Sample/Kmic.Sample.Contracts/ISpecimenReferences.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:32 |
| 1204 | `src/Modules/Sample/Kmic.Sample.Contracts/Kmic.Sample.Contracts.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1205 | `src/Modules/Sample/Kmic.Sample.Contracts/SampleModuleInfo.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1206 | `src/Modules/Sample/Kmic.Sample.Contracts/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 1207 | `src/Modules/Sample/Kmic.Sample.Domain/Errors/SampleErrors.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:32 |
| 1208 | `src/Modules/Sample/Kmic.Sample.Domain/Kmic.Sample.Domain.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1209 | `src/Modules/Sample/Kmic.Sample.Domain/Specimens/Events/SpecimenSubmitted.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:32 |
| 1210 | `src/Modules/Sample/Kmic.Sample.Domain/Specimens/ISpecimenRepository.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:32 |
| 1211 | `src/Modules/Sample/Kmic.Sample.Domain/Specimens/Specimen.cs` | 已修订（模型中文） | 本轮 C2288～C2289；先核对实际行为，再精简或纠正摘要/模型说明。已同步注释迁移与快照，运行模型及 SQL 仅注释变化；测试未运行 | 2026年11月15日 00:04:56 |
| 1212 | `src/Modules/Sample/Kmic.Sample.Domain/Specimens/SpecimenAction.cs` | 保留 | 逐文件核对示例命令、处理器、授权策略和聚合：名称与模板文案清楚；保留草稿/关闭限制、版本冲突、负责人约束、文件关联事务和引用解析约定；设置尚未接入创建/提交的说明与调用一致。 | 2026年11月14日 22:23:33 |
| 1213 | `src/Modules/Sample/Kmic.Sample.Domain/Specimens/SpecimenAttachment.cs` | 保留 | 核对附件快照字段赋值、模块注册和各接口到命令/查询的映射：说明与实现一致；保留权限、关闭限制、版本冲突、下载响应和分页参数约定。实体字段注释保持原样。 | 2026年11月14日 22:23:44 |
| 1214 | `src/Modules/Sample/Kmic.Sample.Domain/Specimens/SpecimenAttachmentId.cs` | 保留 | 核对附件快照字段赋值、模块注册和各接口到命令/查询的映射：说明与实现一致；保留权限、关闭限制、版本冲突、下载响应和分页参数约定。实体字段注释保持原样。 | 2026年11月14日 22:23:44 |
| 1215 | `src/Modules/Sample/Kmic.Sample.Domain/Specimens/SpecimenId.cs` | 保留 | 核对附件快照字段赋值、模块注册和各接口到命令/查询的映射：说明与实现一致；保留权限、关闭限制、版本冲突、下载响应和分页参数约定。实体字段注释保持原样。 | 2026年11月14日 22:23:44 |
| 1216 | `src/Modules/Sample/Kmic.Sample.Domain/Specimens/SpecimenItem.cs` | 保留 | 核对附件快照字段赋值、模块注册和各接口到命令/查询的映射：说明与实现一致；保留权限、关闭限制、版本冲突、下载响应和分页参数约定。实体字段注释保持原样。 | 2026年11月14日 22:23:44 |
| 1217 | `src/Modules/Sample/Kmic.Sample.Domain/Specimens/SpecimenItemId.cs` | 保留 | 核对附件快照字段赋值、模块注册和各接口到命令/查询的映射：说明与实现一致；保留权限、关闭限制、版本冲突、下载响应和分页参数约定。实体字段注释保持原样。 | 2026年11月14日 22:23:44 |
| 1218 | `src/Modules/Sample/Kmic.Sample.Domain/Specimens/SpecimenStatus.cs` | 保留 | 核对附件快照字段赋值、模块注册和各接口到命令/查询的映射：说明与实现一致；保留权限、关闭限制、版本冲突、下载响应和分页参数约定。实体字段注释保持原样。 | 2026年11月14日 22:23:44 |
| 1219 | `src/Modules/Sample/Kmic.Sample.Domain/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 1220 | `src/Modules/Sample/Kmic.Sample.Infrastructure/Kmic.Sample.Infrastructure.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1221 | `src/Modules/Sample/Kmic.Sample.Infrastructure/Persistence/Configurations/SpecimenAttachmentConfiguration.cs` | 已修订（模型中文） | 本轮 C2292；先核对实际行为，再精简或纠正摘要/模型说明。已同步注释迁移与快照，运行模型及 SQL 仅注释变化；测试未运行 | 2026年11月15日 00:04:56 |
| 1222 | `src/Modules/Sample/Kmic.Sample.Infrastructure/Persistence/Configurations/SpecimenConfiguration.cs` | 已修订（模型中文） | 本轮 C2290；先核对实际行为，再精简或纠正摘要/模型说明。已同步注释迁移与快照，运行模型及 SQL 仅注释变化；测试未运行 | 2026年11月15日 00:04:56 |
| 1223 | `src/Modules/Sample/Kmic.Sample.Infrastructure/Persistence/Configurations/SpecimenItemConfiguration.cs` | 已修订（模型中文） | 本轮 C2291；先核对实际行为，再精简或纠正摘要/模型说明。已同步注释迁移与快照，运行模型及 SQL 仅注释变化；测试未运行 | 2026年11月15日 00:04:56 |
| 1224 | `src/Modules/Sample/Kmic.Sample.Infrastructure/Persistence/Repositories/SpecimenRepository.cs` | 沿用已复核 | 沿用上轮逐文件结论（已修订），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1225 | `src/Modules/Sample/Kmic.Sample.Infrastructure/Persistence/SampleDbContext.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1226 | `src/Modules/Sample/Kmic.Sample.Infrastructure/Persistence/SampleDbContextFactory.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1227 | `src/Modules/Sample/Kmic.Sample.Infrastructure/Persistence/SampleIdConverters.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1228 | `src/Modules/Sample/Kmic.Sample.Infrastructure/Persistence/SampleModel.cs` | 保留 | 核对附件快照字段赋值、模块注册和各接口到命令/查询的映射：说明与实现一致；保留权限、关闭限制、版本冲突、下载响应和分页参数约定。实体字段注释保持原样。 | 2026年11月14日 22:23:44 |
| 1229 | `src/Modules/Sample/Kmic.Sample.Infrastructure/Persistence/SampleQueryDbContext.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1230 | `src/Modules/Sample/Kmic.Sample.Infrastructure/SampleModule.cs` | 保留 | 核对附件快照字段赋值、模块注册和各接口到命令/查询的映射：说明与实现一致；保留权限、关闭限制、版本冲突、下载响应和分页参数约定。实体字段注释保持原样。 | 2026年11月14日 22:23:45 |
| 1231 | `src/Modules/Sample/Kmic.Sample.Infrastructure/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 1232 | `src/Modules/Sample/Kmic.Sample.Presentation/Kmic.Sample.Presentation.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1233 | `src/Modules/Sample/Kmic.Sample.Presentation/SampleEndpoints.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1234 | `src/Modules/Sample/Kmic.Sample.Presentation/Specimens/AddSpecimenAttachmentEndpoint.cs` | 保留 | 核对附件快照字段赋值、模块注册和各接口到命令/查询的映射：说明与实现一致；保留权限、关闭限制、版本冲突、下载响应和分页参数约定。实体字段注释保持原样。 | 2026年11月14日 22:23:45 |
| 1235 | `src/Modules/Sample/Kmic.Sample.Presentation/Specimens/AddSpecimenAttachmentRequest.cs` | 保留 | 核对附件快照字段赋值、模块注册和各接口到命令/查询的映射：说明与实现一致；保留权限、关闭限制、版本冲突、下载响应和分页参数约定。实体字段注释保持原样。 | 2026年11月14日 22:23:45 |
| 1236 | `src/Modules/Sample/Kmic.Sample.Presentation/Specimens/CloseSpecimenEndpoint.cs` | 保留 | 核对附件快照字段赋值、模块注册和各接口到命令/查询的映射：说明与实现一致；保留权限、关闭限制、版本冲突、下载响应和分页参数约定。实体字段注释保持原样。 | 2026年11月14日 22:23:45 |
| 1237 | `src/Modules/Sample/Kmic.Sample.Presentation/Specimens/CloseSpecimenRequest.cs` | 保留 | 核对附件快照字段赋值、模块注册和各接口到命令/查询的映射：说明与实现一致；保留权限、关闭限制、版本冲突、下载响应和分页参数约定。实体字段注释保持原样。 | 2026年11月14日 22:23:45 |
| 1238 | `src/Modules/Sample/Kmic.Sample.Presentation/Specimens/CreateSpecimenEndpoint.cs` | 保留 | 核对附件快照字段赋值、模块注册和各接口到命令/查询的映射：说明与实现一致；保留权限、关闭限制、版本冲突、下载响应和分页参数约定。实体字段注释保持原样。 | 2026年11月14日 22:23:45 |
| 1239 | `src/Modules/Sample/Kmic.Sample.Presentation/Specimens/CreateSpecimenItemRequest.cs` | 保留 | 核对附件快照字段赋值、模块注册和各接口到命令/查询的映射：说明与实现一致；保留权限、关闭限制、版本冲突、下载响应和分页参数约定。实体字段注释保持原样。 | 2026年11月14日 22:23:45 |
| 1240 | `src/Modules/Sample/Kmic.Sample.Presentation/Specimens/CreateSpecimenRequest.cs` | 保留 | 核对附件快照字段赋值、模块注册和各接口到命令/查询的映射：说明与实现一致；保留权限、关闭限制、版本冲突、下载响应和分页参数约定。实体字段注释保持原样。 | 2026年11月14日 22:23:45 |
| 1241 | `src/Modules/Sample/Kmic.Sample.Presentation/Specimens/DeleteSpecimenEndpoint.cs` | 保留 | 核对附件快照字段赋值、模块注册和各接口到命令/查询的映射：说明与实现一致；保留权限、关闭限制、版本冲突、下载响应和分页参数约定。实体字段注释保持原样。 | 2026年11月14日 22:23:45 |
| 1242 | `src/Modules/Sample/Kmic.Sample.Presentation/Specimens/DeleteSpecimenRequest.cs` | 保留 | 核对附件快照字段赋值、模块注册和各接口到命令/查询的映射：说明与实现一致；保留权限、关闭限制、版本冲突、下载响应和分页参数约定。实体字段注释保持原样。 | 2026年11月14日 22:23:46 |
| 1243 | `src/Modules/Sample/Kmic.Sample.Presentation/Specimens/DownloadSpecimenAttachmentEndpoint.cs` | 保留 | 核对附件快照字段赋值、模块注册和各接口到命令/查询的映射：说明与实现一致；保留权限、关闭限制、版本冲突、下载响应和分页参数约定。实体字段注释保持原样。 | 2026年11月14日 22:23:46 |
| 1244 | `src/Modules/Sample/Kmic.Sample.Presentation/Specimens/GetSpecimenEndpoint.cs` | 保留 | 核对附件快照字段赋值、模块注册和各接口到命令/查询的映射：说明与实现一致；保留权限、关闭限制、版本冲突、下载响应和分页参数约定。实体字段注释保持原样。 | 2026年11月14日 22:23:46 |
| 1245 | `src/Modules/Sample/Kmic.Sample.Presentation/Specimens/ListSpecimensEndpoint.cs` | 保留 | 核对附件快照字段赋值、模块注册和各接口到命令/查询的映射：说明与实现一致；保留权限、关闭限制、版本冲突、下载响应和分页参数约定。实体字段注释保持原样。 | 2026年11月14日 22:23:46 |
| 1246 | `src/Modules/Sample/Kmic.Sample.Presentation/Specimens/ListSpecimensRequest.cs` | 保留 | 核对附件快照字段赋值、模块注册和各接口到命令/查询的映射：说明与实现一致；保留权限、关闭限制、版本冲突、下载响应和分页参数约定。实体字段注释保持原样。 | 2026年11月14日 22:23:46 |
| 1247 | `src/Modules/Sample/Kmic.Sample.Presentation/Specimens/RemoveSpecimenAttachmentEndpoint.cs` | 保留 | 核对附件快照字段赋值、模块注册和各接口到命令/查询的映射：说明与实现一致；保留权限、关闭限制、版本冲突、下载响应和分页参数约定。实体字段注释保持原样。 | 2026年11月14日 22:23:46 |
| 1248 | `src/Modules/Sample/Kmic.Sample.Presentation/Specimens/RemoveSpecimenAttachmentRequest.cs` | 保留 | 核对附件快照字段赋值、模块注册和各接口到命令/查询的映射：说明与实现一致；保留权限、关闭限制、版本冲突、下载响应和分页参数约定。实体字段注释保持原样。 | 2026年11月14日 22:23:46 |
| 1249 | `src/Modules/Sample/Kmic.Sample.Presentation/Specimens/SubmitSpecimenEndpoint.cs` | 保留 | 核对附件快照字段赋值、模块注册和各接口到命令/查询的映射：说明与实现一致；保留权限、关闭限制、版本冲突、下载响应和分页参数约定。实体字段注释保持原样。 | 2026年11月14日 22:23:46 |
| 1250 | `src/Modules/Sample/Kmic.Sample.Presentation/Specimens/SubmitSpecimenRequest.cs` | 保留 | 核对附件快照字段赋值、模块注册和各接口到命令/查询的映射：说明与实现一致；保留权限、关闭限制、版本冲突、下载响应和分页参数约定。实体字段注释保持原样。 | 2026年11月14日 22:23:47 |
| 1251 | `src/Modules/Sample/Kmic.Sample.Presentation/packages.lock.json` | 无中文 | 完整文件未发现中文，本次语言复核无须改写。 | 2026年11月14日 22:12:28 |
| 1252 | `templates/kmic-module/.template.config/template.json` | 保留 | 已对照配置项或调用实现核对中文。工程规则、依赖及发布设置未改变；无需改写的说明保留。 | 2026年11月14日 22:17:43 |
| 1253 | `templates/kmic-module/src/Modules/Customer/Kmic.Customer.Application/CustomerPermissions.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1254 | `templates/kmic-module/src/Modules/Customer/Kmic.Customer.Application/CustomerScopes.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1255 | `templates/kmic-module/src/Modules/Customer/Kmic.Customer.Application/ICustomerQueryDb.cs` | 保留 | 核对配套脚本的命令顺序、失败退出、凭据保存位置及模板查询契约：中文用途和提示清楚，保留；仅静态阅读，没有执行脚本、编译、测试、生成迁移或访问数据库。 | 2026年11月14日 22:40:49 |
| 1256 | `templates/kmic-module/src/Modules/Customer/Kmic.Customer.Application/Kmic.Customer.Application.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1257 | `templates/kmic-module/src/Modules/Customer/Kmic.Customer.Contracts/CustomerModuleInfo.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1258 | `templates/kmic-module/src/Modules/Customer/Kmic.Customer.Contracts/Kmic.Customer.Contracts.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1259 | `templates/kmic-module/src/Modules/Customer/Kmic.Customer.Domain/Errors/CustomerErrors.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1260 | `templates/kmic-module/src/Modules/Customer/Kmic.Customer.Domain/Kmic.Customer.Domain.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1261 | `templates/kmic-module/src/Modules/Customer/Kmic.Customer.Infrastructure/CustomerModule.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1262 | `templates/kmic-module/src/Modules/Customer/Kmic.Customer.Infrastructure/Kmic.Customer.Infrastructure.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1263 | `templates/kmic-module/src/Modules/Customer/Kmic.Customer.Infrastructure/Persistence/CustomerDbContext.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1264 | `templates/kmic-module/src/Modules/Customer/Kmic.Customer.Infrastructure/Persistence/CustomerDbContextFactory.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1265 | `templates/kmic-module/src/Modules/Customer/Kmic.Customer.Infrastructure/Persistence/CustomerQueryDbContext.cs` | 保留 | 核对配套脚本的命令顺序、失败退出、凭据保存位置及模板查询契约：中文用途和提示清楚，保留；仅静态阅读，没有执行脚本、编译、测试、生成迁移或访问数据库。 | 2026年11月14日 22:40:49 |
| 1266 | `templates/kmic-module/src/Modules/Customer/Kmic.Customer.Presentation/CustomerEndpoints.cs` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
| 1267 | `templates/kmic-module/src/Modules/Customer/Kmic.Customer.Presentation/Kmic.Customer.Presentation.csproj` | 沿用已复核 | 沿用上轮逐文件结论（保留），完整内容未变；模型与行为待办保留。 | 2026年11月14日 22:12:28 |
