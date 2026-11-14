# 模型相关中文修订方案

- 核对时间：2026年11月15日 00:04:56（UTC+8）
- 状态：已按用户确认实施 M001～M037，C2259～C2310；模型、注释迁移和正反向 SQL 核对完成。
- 来源：用户要求继续上一条建议，先把模型相关待办的目标文案和影响列清楚；沿用已确认的中文表达标准。
- 当前分支：`codex-cn-copy-cleanup-2026-11-14`；HEAD：`bca15d72fff863db1b1e2f76d864c3ffce59b6e8`。
- 候选范围：37 项、17 个源码文件，涉及 Configuration、Files、Identity、Notifications、Sample 五个模块。M001～M037 已实施；方案记录 C2258，实际实施记录 C2259～C2310。
- 修改记录和双方路径清单只用于追溯来源；实际修改的 32 个源码/迁移/快照文件均已同步登记。

## 实施授权与当前进度

用户已明确批准上一条建议：修改 17 个源码文件、为五模块新增注释迁移并由 EF 更新当前快照，进行必要编译、离线模型与 SQL 核对。不执行数据库更新或全量测试，不暂存提交推送；BuildingBlocks 仅读取。本轮开始时 964 个已跟踪修改、10 个未跟踪文件，原有改动逐文件留档。

## 1. 建议与取舍

建议保留现有的注释生成方式：属性 `summary` 写简短、准确的含义，只有容易误用的约定才保留在合适位置。确需调整模型说明时，在后续实施阶段同步新增迁移。没有必要为这轮中文整理另造一套数据库注释机制。

| 方案 | 做法 | 收益与代价 | 判断 |
|---|---|---|---|
| 沿用现有来源并新增注释迁移 | 属性摘要继续生成列说明；表和影子列继续使用已有 HasComment | 中文与模型来源一致，不改框架；代价是需要审查新的模型差异和迁移 | 已采用并实施 |
| 暂只补充 remarks | 摘要、模型和数据库说明保持原样 | 当前可避免模型差异，但已核实的不准确摘要和数据库说明仍然存在 | 可继续暂留，不能当作问题已解决 |
| 分离源码与数据库注释来源 | 广泛使用独立 HasComment 覆盖，或新增专用特性/生成机制 | 允许两处措辞不同，但增加重复维护；若改生成机制还会涉及 BuildingBlocks | 本任务不采用；已有机制已支持必要的个别覆盖 |

本轮只调整已确认的模型中文说明，不改变数据含义、业务约束、权限、状态机或保留期。短信能力仍按已确认决定搁置；不在注释中把预留表写成已实现流程。

## 2. 已核实的影响链路

1. `EntityCommentGenerator.Collect` 收集 Entity 派生类及 record 的公开、非静态、非索引属性；`Summary` 只读取 XML 的 summary，合并空白，并将 cref 换成成员名。类型摘要、方法摘要和 remarks 不会在这条链路中生成属性列注释。
2. `GeneratedComments.Of` 按属性声明类型和成员名取生成文本。`ModelConventions.AddColumnComments` 只为实际映射的业务实体属性补充尚未显式配置的列说明；record 被收集不代表所有 DTO 都会入库。复杂属性也可能组合父、子摘要作为列说明。
3. 显式配置的 `HasComment` 优先；表注释和影子列注释由配置直接决定。修改这些文字同样会改变 EF 注释元数据。
4. 本方案 37 项当前文字与对应模块现有 `*DbContextModelSnapshot.cs` 中的注释逐项一致。方案阶段核对了源码和快照文本；本轮进一步编译并比较运行模型、生成正反向 SQL。没有查询实际数据库，不能据此声称部署库已验证。

依据（BuildingBlocks 仅读取）：

- [生成器](../../src/BuildingBlocks/Kmic.BuildingBlocks.Analyzers/Generators/EntityCommentGenerator.cs)：Collect、IsEntityOrValueObject、Summary。
- [生成文本读取](../../src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Persistence/GeneratedComments.cs)：Of。
- [模型约定](../../src/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure/Persistence/ModelConventions.cs)：Apply、AddColumnComments、CommentValueObjectColumns。
- [数据访问约定](../framework/05-infrastructure/05-03-数据访问.md)：3.9 节，注释随迁移进入数据库，显式 HasComment 可按需要覆盖。

## 3. 逐项文案（原文与已实施目标）

以下各项于方案阶段核对原文与快照，已于本轮逐项实施并记录 C 编号。行号为方案阶段位置，按成员名定位；实际改动与验证时间见修改记录。

**表中的核对依据和放置建议不应整段复制到源码。** 摘要只取“建议文案”；补充信息仅在调用者确实需要、且已有契约没有清楚说明时保留。相同的清理或权限规则集中说明，普通字段不再重复一遍。

### Files

| 编号 | 源码位置 | 当前摘要或模型说明 | 建议文案 | 核对结论与必要信息的放置 |
|---|---|---|---|---|
| M001 | `src/Modules/Files/Kmic.Files.Domain/Files/StoredFile.cs:46`，`OriginalName`（属性 summary） | 显示用的文件名：去掉路径与控制字符，只作显示 | 显示文件名 | 原摘要两次解释“显示”；字段只需名称。 必要信息：清理路径与控制字符的规则保留在 FileNames.Sanitize。依据 E02；快照原文一致；已实施（C2259），迁移/SQL 核对通过。 |
| M002 | `src/Modules/Files/Kmic.Files.Domain/Files/StoredFile.cs:76`，`Profile`（属性 summary） | 上传档案名；生成的文件是 Generated:来源 | 上传配置名称 | Name 是代码中的配置名称，不是 DisplayName；“上传档案名”未与当前术语一致。 必要信息：保存 UploadProfile.Name；生成文件使用 Generated:来源。依据 E02；快照原文一致；已实施（C2260），迁移/SQL 核对通过。 |
| M003 | `src/Modules/Files/Kmic.Files.Domain/Files/StoredFile.cs:81`，`UploadedBy`（属性 summary） | 上传的人；系统生成的文件没有 | 上传人或生成请求人 | RegisterGenerated 把 RequestedBy 写入同一字段，不能断言生成文件没有上传人。 必要信息：不必再解释可空类型。依据 E02；快照原文一致；已实施（C2261），迁移/SQL 核对通过。 |
| M004 | `src/Modules/Files/Kmic.Files.Domain/Files/StoredFile.cs:86`，`UploadedAt`（属性 summary） | 上传开始的时间 | 文件登记时间 | 此值既不是普遍的内容写入完成时间，也不是生成文件开始写入时间。 必要信息：上传文件在开始上传时记录；生成文件在处理登记消息时记录。依据 E02；快照原文一致；已实施（C2262），迁移/SQL 核对通过。 |
| M005 | `src/Modules/Files/Kmic.Files.Domain/Files/StoredFile.cs:96`，`ExpiresAt`（属性 summary） | 成为清理候选的时间：临时、已解除关联、被拒绝的文件才有，已关联的文件为空 | 清理到期时间 | Purge 和 Expire 不清空原期限，原摘要“只有某几种状态才有”过度概括。 必要信息：到期仅成为清理候选，实际清理由状态、互斥检查和后台任务决定。依据 E03；快照原文一致；已实施（C2263），迁移/SQL 核对通过。 |
| M006 | `src/Modules/Files/Kmic.Files.Domain/Files/StoredFile.cs:101`，`RejectReason`（属性 summary） | 被拒绝的原因：类型不符、超过大小、扫描没通过、写入失败 | 拒绝原因 | MarkLost 也会写入，原列举漏掉对象丢失；短名称更准确。 必要信息：具体失败类型由操作和状态历史说明，不在字段逐个列举。依据 E03；快照原文一致；已实施（C2264），迁移/SQL 核对通过。 |
| M007 | `src/Modules/Files/Kmic.Files.Domain/Files/StoredFile.cs:106`，`PurgedAt`（属性 summary） | 彻底删除的时间 | 彻底删除标记时间 | Purge 先写标记，工作器随后处理存储对象。 必要信息：不等于存储对象已删除。依据 E03；快照原文一致；已实施（C2265），迁移/SQL 核对通过。 |
| M008 | `src/Modules/Files/Kmic.Files.Domain/Files/StoredFile.cs:111`，`PurgedBy`（属性 summary） | 彻底删除的人 | 彻底删除操作人 | Purge 记录 by；后台删除不改此字段。 必要信息：指执行彻底删除标记的操作人，不是后台工作器。依据 E03；快照原文一致；已实施（C2266），迁移/SQL 核对通过。 |
| M009 | `src/Modules/Files/Kmic.Files.Domain/Files/StoredFile.cs:121`，`HistoryPurgedAt`（属性 summary） | 运维彻底删除历史版本之后登记的时间；为空表示历史版本可能仍在 | 历史版本清除登记时间 | RecordHistoryPurge 只登记已完成操作，不删除或检查存储中的历史版本。 必要信息：为空仅表示尚未登记，不据此断定历史版本存在或不存在。依据 E03；快照原文一致；已实施（C2267），迁移/SQL 核对通过。 |
| M010 | `src/Modules/Files/Kmic.Files.Domain/Files/StoredFile.cs:126`，`HistoryPurgeNote`（属性 summary） | 彻底删除历史版本的执行记录，如工单号 | 历史版本清除记录 | 字段保存运维传入的执行说明，短名称足够。 必要信息：工单号等示例已有接口说明，不重复放进摘要。依据 E03；快照原文一致；已实施（C2268），迁移/SQL 核对通过。 |
| M011 | `src/Modules/Files/Kmic.Files.Domain/Files/StoredFile.cs:131`，`ObjectDeletedAt`（属性 summary） | 对象在存储里被删除的时间：只有彻底删除的文件需要，空表示对象还在、等删除工作器处理 | 对象删除登记时间 | SQL 完成登记可能晚于存储删除；删除缺失对象也可成功，不能从空值推断对象还在。 必要信息：删除工作器成功后登记，包含对象已不存在的情况；空值不证明对象存在。到期清理删除整条记录，历史版本另行处理。依据 E03；快照原文一致；已实施（C2269），迁移/SQL 核对通过。 |
| M027 | `src/Modules/Files/Kmic.Files.Infrastructure/Persistence/Configurations/StoredFileConfiguration.cs:18`，`ToTable.HasComment`（表注释） | 文件元数据；Files 模块；一行一个文件，内容在对象存储里；状态是删除队列：删除中的行由删除工作器领取，先删对象再删行 | 文件元数据（Files）：一条记录对应一个文件，内容存于对象存储；到期清理先删对象再删记录，彻底删除保留记录 | 删除队列还包含 Purged 且尚未登记对象删除完成的记录，不只 Deleting；区别两种完成行为。 必要信息：保留模块、记录含义和必要生命周期约定。依据 E03；快照原文一致；已实施（C2285），迁移/SQL 核对通过。 |
| M028 | `src/Modules/Files/Kmic.Files.Infrastructure/Persistence/Configurations/StorageIssueConfiguration.cs:12`，`ToTable.HasComment`（表注释） | 存储问题；Files 模块；对账发现库里有记录而存储里找不到对象的文件，待运维还原后标为已解决 | 存储问题（Files）：一条记录对应一次对象缺失问题；登记处理结果，不执行还原，文件记录清理后仍保留 | Resolve 只登记说明、操作人和时间，未要求对象必须已还原；FileId 无外键。 必要信息：保留模块、记录含义和必要生命周期约定。依据 E03；快照原文一致；已实施（C2286），迁移/SQL 核对通过。 |
| M029 | `src/Modules/Files/Kmic.Files.Infrastructure/Persistence/Configurations/StoredFileConfiguration.cs:33`，`DeleteAttempts`（显式列注释） | 删除队列已经试了几次 | 删除队列领取次数，人工重试时清零 | ClaimAsync 每次领取递增；ResetDeletionAsync 清零次数并清除退避和领取信息。 必要信息：不改影子属性、领取或重试逻辑。依据 E03；快照原文一致；已实施（C2287），迁移/SQL 核对通过。 |

### Identity

| 编号 | 源码位置 | 当前摘要或模型说明 | 建议文案 | 核对结论与必要信息的放置 |
|---|---|---|---|---|
| M012 | `src/Modules/Identity/Kmic.Identity.Domain/Users/User.cs:36`，`Account`（属性 summary） | 登录名，唯一且不区分大小写；迁移来的用户沿用旧系统的手机号 | 登录名 | 当前领域字段没有“必须沿用旧手机号”的限制，迁移约定不应冒充当前字段含义。 必要信息：唯一且不区分大小写的约束保留在相邻说明或实体配置。依据 E04；快照原文一致；已实施（C2270），迁移/SQL 核对通过。 |
| M013 | `src/Modules/Identity/Kmic.Identity.Domain/Users/User.cs:46`，`EnglishName`（属性 summary） | 英文名，合同、报告上要印 | 英文名 | 字段含义简单，具体业务输出由使用模块决定。 必要信息：不补合同、报告等尚未在当前模块实现的用途。依据 E04；快照原文一致；已实施（C2271），迁移/SQL 核对通过。 |
| M014 | `src/Modules/Identity/Kmic.Identity.Domain/Users/User.cs:56`，`Mobile`（属性 summary） | 手机号，规范化为国际格式，填写时唯一；短信登录需要它 | 手机号 | 短信登录已搁置，原摘要将未接入用途写成当前功能；保留格式与唯一性约束。 必要信息：保存规范化的国际格式，填写时唯一。依据 E04；快照原文一致；已实施（C2272），迁移/SQL 核对通过。 |
| M015 | `src/Modules/Identity/Kmic.Identity.Domain/Users/User.cs:66`，`OrgUnitId`（属性 summary） | 所属组织单元，数据范围的“本组织”从它算起；为空时只剩“本人”的数据 | 所属组织单元 | 角色等级为 All 或有显式组织锚点时，不会因该值为空一律缩为本人数据。 必要信息：未指定角色授予锚点时，作为“本组织”范围的起点。依据 E04；快照原文一致；已实施（C2273），迁移/SQL 核对通过。 |
| M016 | `src/Modules/Identity/Kmic.Identity.Domain/Users/User.cs:81`，`PasswordHash`（属性 summary） | 密码哈希；为空表示还没有设置过密码（迁移来的员工靠短信登录） | 密码哈希 | 没有密码不会自动切换到短信登录；当前密码验证返回失败。 必要信息：未设置密码时为空。依据 E04；快照原文一致；已实施（C2274），迁移/SQL 核对通过。 |
| M017 | `src/Modules/Identity/Kmic.Identity.Domain/Sessions/Session.cs:41`，`DeviceName`（属性 summary） | 设备名称，App 上报或由 User-Agent 解析 | 登录时的 User-Agent | 登录与模拟端点传入原始 User-Agent，处理器仅截断，未解析设备型号。 必要信息：最多保留 200 个字符；不改变属性名或响应字段。依据 E05；快照原文一致；已实施（C2275），迁移/SQL 核对通过。 |
| M018 | `src/Modules/Identity/Kmic.Identity.Domain/Sessions/Session.cs:71`，`RefreshExpiresAt`（属性 summary） | 滑动过期时间 | 刷新令牌或模拟会话到期时间 | StartImpersonation 将该字段与 AbsoluteExpiresAt 同设为 expires，不能只描述滑动过期。 必要信息：普通会话随刷新更新；模拟会话不签发刷新令牌，此值等于会话到期时间。依据 E05；快照原文一致；已实施（C2276），迁移/SQL 核对通过。 |
| M035 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Configurations/SessionConfiguration.cs:12`，`ToTable.HasComment`（表注释） | 会话；Identity 模块；一行代表一次登录（含模拟身份）；作废或过期 30 天后由清理任务删除 | 会话（Identity）：一条记录对应一次普通或模拟登录；按绝对到期或作废时间及配置保留期清理 | 30 天是 EndedRetention 的默认值，不是固定规则；清理按 AbsoluteExpiresAt 或 RevokedAt 比较截止时间。 必要信息：保留模块、记录含义和必要生命周期约定。依据 E09；快照原文一致；已实施（C2293），迁移/SQL 核对通过。 |
| M036 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Configurations/LoginThrottleRowConfiguration.cs:11`，`ToTable.HasComment`（表注释） | 登录失败计数；Identity 模块；一行代表一个登录名（哈希）的失败状态；24 小时没有新失败由每日清理任务删除 | 登录失败计数（Identity）：每个登录名哈希一条失败状态记录；按配置的重置间隔清理，等待或锁定未结束时保留 | ResetAfter 可配置，清理同时检查 NextAllowedAt 和 LockedUntil；不能只写固定 24 小时。 必要信息：保留模块、记录含义和必要生命周期约定。依据 E09；快照原文一致；已实施（C2294），迁移/SQL 核对通过。 |
| M037 | `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Configurations/VerificationCodeRowConfiguration.cs:11`，`ToTable.HasComment`（表注释） | 短信验证码；Identity 模块；一行代表一次发送请求（哈希），过期超过 1 天由每日清理任务删除 | 预留的短信验证码记录（Identity）：保存验证码哈希及校验状态；发送、校验和清理流程尚未接入 | 产品源码仅见记录定义、映射和模型注册，未见读写/发送/校验或每日清理处理器；相关能力已明确搁置。 必要信息：保留模块、记录含义和必要生命周期约定。依据 E09；快照原文一致；已实施（C2295），迁移/SQL 核对通过。 |

### Configuration

| 编号 | 源码位置 | 当前摘要或模型说明 | 建议文案 | 核对结论与必要信息的放置 |
|---|---|---|---|---|
| M019 | `src/Modules/Configuration/Kmic.Configuration.Domain/Settings/SystemSettingChange.cs:29`，`OldValue`（属性 summary） | 改之前的值；此前用的是默认值时为空 | 修改前保存的值 | 显式保存的内容即使等于默认值，仍有非空旧值；不能用是否默认值判断。 必要信息：原无保存记录时为空。依据 E06；快照原文一致；已实施（C2277），迁移/SQL 核对通过。 |
| M020 | `src/Modules/Configuration/Kmic.Configuration.Domain/Settings/SystemSettingChange.cs:34`，`NewValue`（属性 summary） | 改之后的值；恢复默认时为空 | 修改后保存的值 | 重置和清除命令都将 NewValue 记为 null。 必要信息：恢复默认或清除已移除声明的设置时为空。依据 E06；快照原文一致；已实施（C2278），迁移/SQL 核对通过。 |
| M021 | `src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/Configurations/PreferenceValueConfiguration.cs:15`，`ToTable.HasComment`（表注释） | 偏好在某一层上的值；Configuration 模块；系统层、角色层、用户在角色下的层，各层同一个键只有一行 | 分层偏好值（Configuration）：按键、层级、角色和用户组合唯一；系统层无归属，角色层按角色保存，个人层按用户和角色保存 | 唯一索引包含 RoleId 和 UserId；“各层同一个键只有一行”遗漏了归属维度。 必要信息：保留模块、记录含义和必要生命周期约定。依据 E06；快照原文一致；已实施（C2279），迁移/SQL 核对通过。 |

### Notifications

| 编号 | 源码位置 | 当前摘要或模型说明 | 建议文案 | 核对结论与必要信息的放置 |
|---|---|---|---|---|
| M022 | `src/Modules/Notifications/Kmic.Notifications.Domain/Deliveries/Delivery.cs:98`，`Body`（属性 summary） | 渲染好的正文；含密模板投递完成后清空 | 渲染后的正文 | 原“投递完成后清空”未区分捕获状态；短摘要不对含糊的完成状态作承诺。 必要信息：清理规则集中保留在 DeliveryStore 和含密通知契约：已发送、最终失败或跳过时清空含密正文；不在每个字段重复。依据 E07；快照原文一致；已实施（C2280），迁移/SQL 核对通过。 |
| M023 | `src/Modules/Notifications/Kmic.Notifications.Domain/Deliveries/Delivery.cs:103`，`SmsParameters`（属性 summary） | 短信模板的参数，JSON；只有短信有。短信服务只收模板编码和参数，实际发送用它，含密模板投递完成后清空 | 短信模板参数（JSON） | 发送使用已审核模板编码与参数的限制已由短信渠道契约说明，无需把整个发送过程写进字段摘要。 必要信息：仅短信使用；与正文共用含密清理说明。依据 E07；快照原文一致；已实施（C2281），迁移/SQL 核对通过。 |
| M024 | `src/Modules/Notifications/Kmic.Notifications.Domain/Deliveries/Delivery.cs:128`，`CodeHash`（属性 summary） | 渲染时代码里默认内容的哈希，事后能知道发出的是哪一版默认内容 | 模板声明及默认内容的哈希 | TemplateHash 还包含键、显示名、模块、渠道、发件配置和含密标记；发布内容被使用时，该值也不是发送正文的独立内容哈希。 必要信息：不含模型结构与示例数据。依据 E07；快照原文一致；已实施（C2282），迁移/SQL 核对通过。 |
| M025 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Configurations/DeliveryConfiguration.cs:36`，`Attempts`（显式列注释） | 已经尝试发送几次 | 领取次数，人工重试时清零 | 领取 SQL 在模板、过期和地址检查之前递增，不能称实际调用渠道的次数。 必要信息：不改影子属性、领取或重试逻辑。依据 E07；快照原文一致；已实施（C2283），迁移/SQL 核对通过。 |
| M026 | `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Configurations/CapturedMessageConfiguration.cs:12`，`ToTable.HasComment`（表注释） | 非生产环境捕获的邮件与短信；Notifications 模块；本该发出去的内容留在这里供查看，只在非生产环境有数据，超过保留期由清理任务删除 | 捕获内容（Notifications）：一条记录对应一次邮件或短信投递；开启捕获时保存内容而不实际发送，按保留期清理 | 写入分支判断 capture.Enabled，不按 Production/Development 决定是否建表或写入。 必要信息：保留模块、记录含义和必要生命周期约定。依据 E07；快照原文一致；已实施（C2284），迁移/SQL 核对通过。 |

### Sample

| 编号 | 源码位置 | 当前摘要或模型说明 | 建议文案 | 核对结论与必要信息的放置 |
|---|---|---|---|---|
| M030 | `src/Modules/Sample/Kmic.Sample.Domain/Specimens/Specimen.cs:50`，`OwnerId`（属性 summary） | 负责人，创建者 | 负责人 | 字段应直接表达负责人，避免在字段摘要再次解释创建流程。 必要信息：创建时取当前用户的约定已在创建命令说明。依据 E08；快照原文一致；已实施（C2288），迁移/SQL 核对通过。 |
| M031 | `src/Modules/Sample/Kmic.Sample.Domain/Specimens/Specimen.cs:55`，`OrgUnitId`（属性 summary） | 所属组织，数据范围按它判断 | 所属组织 | 字段短名称足够，数据范围计算规则由接口/契约说明。 必要信息：业务归属创建后保留，不随用户所属组织变化。依据 E08；快照原文一致；已实施（C2289），迁移/SQL 核对通过。 |
| M032 | `src/Modules/Sample/Kmic.Sample.Infrastructure/Persistence/Configurations/SpecimenConfiguration.cs:14`，`ToTable.HasComment`（表注释） | 示例标本；Sample 模块；一行代表一个标本；可软删除，有状态流转 | 业务模块示例（Sample）：一条记录对应一个示例；草稿可软删除，提交后可关闭 | 统一 Specimen 的示例称呼；删除命令仅接受草稿，状态机允许 Submitted 到 Closed。 必要信息：保留模块、记录含义和必要生命周期约定。依据 E08；快照原文一致；已实施（C2290），迁移/SQL 核对通过。 |
| M033 | `src/Modules/Sample/Kmic.Sample.Infrastructure/Persistence/Configurations/SpecimenItemConfiguration.cs:11`，`ToTable.HasComment`（表注释） | 示例标本的明细；Sample 模块；一行代表一项明细，随标本一起保存 | 示例明细（Sample）：一条记录对应一项明细，随示例一起保存 | 去掉旧“标本”称呼，保留归属与保存生命周期。 必要信息：保留模块、记录含义和必要生命周期约定。依据 E08；快照原文一致；已实施（C2291），迁移/SQL 核对通过。 |
| M034 | `src/Modules/Sample/Kmic.Sample.Infrastructure/Persistence/Configurations/SpecimenAttachmentConfiguration.cs:13`，`ToTable.HasComment`（表注释） | 示例标本的附件；Sample 模块；一行代表一个附件，保存文件 ID 和关联时的快照，随标本一起保存 | 示例附件（Sample）：一条记录对应一个附件，保存文件 ID 及关联时的文件信息，随示例一起保存 | 保存的是文件名、大小、类型；内容由 Files 管理，不在此表保存。 必要信息：保留模块、记录含义和必要生命周期约定。依据 E08；快照原文一致；已实施（C2292），迁移/SQL 核对通过。 |

## 4. 关键表达的选择理由

- **UploadedAt 使用“文件登记时间”。** 普通上传在开始时先建记录，生成文件则等登记消息处理时建记录；“上传开始时间”不能覆盖两种来源，“上传完成时间”也不符合实现。生成文件的保留期从写入开始计算，不能用 UploadedAt 加保留时长倒推其期限。
- **UploadedBy 使用“上传人或生成请求人”。** 这比一律称上传人多几个字，但涵盖同一字段的两种实际来源，不再承诺生成文件一定为空。
- **Profile 使用“上传配置名称”。** 实际存的是 UploadProfile.Name，作为程序中的配置名称使用，不是 DisplayName。沿用当前模块已统一的叫法，在必要的相邻说明中给出 Generated:来源，不再混用“档案”。BuildingBlocks 中尚有的旧术语本阶段不改。
- **PurgedAt 与 ObjectDeletedAt 都保留“标记/登记”的区别。** 前者是用户发起彻底删除时的标记；后者由工作器在存储删除操作成功后写入。对象已经缺失也可能删除成功，数据库登记失败或尚未执行时仍可能为空；历史版本清除另行登记。
- **User.OrgUnitId 不再承诺“为空时只剩本人”。** 实际范围由角色等级、显式锚点和用户所属组织共同决定；字段只解释默认组织起点。
- **Attempts 使用“领取次数”。** 领取在检查模板、有效期、地址和调用渠道之前计数；失败、重试及过期接管不能按“已发送几次”理解。
- **不把所有 NULL、单位和状态枚举都写进字段。** 简单含义可从类型、名称和既有契约理解的就保留短名称；确会导致误用的来源、标记时间、清理条件才补充。

推荐写法示例（仅方案，未写入源码）：

```csharp
/// <summary>
/// 上传人或生成请求人
/// </summary>
public UserId? UploadedBy { get; private set; }

/// <summary>
/// 文件登记时间
/// </summary>
/// <remarks>
/// 上传文件在开始上传时记录；生成文件在处理登记消息时记录
/// </remarks>
public DateTimeOffset UploadedAt { get; private set; }
```

## 5. 实现依据

| 编号 | 核对位置 | 已核实的行为 |
|---|---|---|
| E02 | `Files.Application/Uploads/Start/StartUploadCommandHandler.Handle`；`Attachments/GeneratedFiles.WriteAsync`；`RegisterGeneratedFileCommandHandler.Handle`；`Files.Domain/Files/StoredFile.Start/RegisterGenerated` | 普通上传保存 profile.Name、当前用户和开始时刻；生成文件保存 RequestedBy，登记时刻由处理器取得；清理到期时间预先按写入开始时刻加保留期计算。 |
| E03 | `StoredFile.Attach/Expire/Purge/MarkLost/RecordHistoryPurge`；`Files.Infrastructure/Deletion/FilesDeletionStore.ClaimAsync/CompleteAsync`；`StoredFileDeletionWork.ProcessAsync`；`StoredFileRepository.ResetDeletionAsync`；`StorageIssue.Resolve` 和其配置 | Purge 仅先标记且不清除旧期限；队列处理 Deleting 和对象未删除登记的 Purged，前者删除记录，后者保留并登记时间。存储删除成功与数据库登记分开；问题解决仅登记结果。 |
| E04 | `Identity.Domain/Users/User`、`UserFields`；`Identity.Infrastructure/Persistence/Configurations/UserConfiguration`；`BuildingBlocks.Application/Authorization/DataScope.Resolve/Anchors`（只读）；`docs/framework/00-README.md` 延后表和实施 T-08 | 登录名和手机号有各自格式/唯一性约束；无密码不会执行短信登录。数据范围可为 All，且优先使用授予锚点。短信能力仍搁置。 |
| E05 | `Identity.Presentation/Authentication/LoginEndpoint`；`LoginCommandHandler.Handle`；`StartImpersonationCommandHandler.ExecuteAsync`；`Session.Start/StartImpersonation/Rotate` | User-Agent 从请求头传入并截到 200 字符，没有设备解析；模拟会话无刷新令牌，但 RefreshExpiresAt 与绝对到期时间相同。 |
| E06 | `Configuration.Application/Settings/Set/SetSettingCommandHandler`、`Reset/ResetSettingCommandHandler`、`Purge/PurgeObsoleteSettingCommandHandler`；`PreferenceValueConfiguration` | 旧值取 row?.Value；重置/清除的新值均为 null。偏好唯一键含键、层级、角色和用户，且关闭 null 区分。 |
| E07 | `Notifications.Infrastructure/Sending/DeliveryStore`、`DeliveryWork.RecordResultAsync`；`DeliveryRepository.RetryAsync/ClearSecretBodiesAsync`；`NotificationProcessor.AddDelivery`；`TemplateHash.Of` | Attempts 在领取时增加、人工重试清零；暂时失败时才按上限决定是否继续重试。含密清理覆盖 Sent/Failed/Skipped，Captured 不在此范围。捕获分支由 Enabled 控制；哈希含声明及默认内容。 |
| E08 | `Sample.Application/Specimens/Create/CreateSpecimenCommandHandler`、`Delete/DeleteSpecimenCommandHandler`；`Sample.Domain/Specimens/Specimen`；三个实体配置 | 创建时取当前用户和所属组织；删除命令仅接受草稿，采用软删除；明细与附件随示例保存。附件只保存文件引用和关联时信息。 |
| E09 | `SessionRepository.PurgeEndedAsync`；`PurgeSessionsCommandHandler`；`SessionLifetimeOptions.EndedRetention`；`PurgeLoginThrottleCommandHandler`、`LoginThrottleStore.PurgeAsync`；`VerificationCodeRow` 及配置/模型注册 | 会话保留期和登录失败重置间隔可配置；登录失败清理还保留等待或锁定未结束的记录。VerificationCodeRow 当前仅见预留定义与映射，发送、校验、清理流程未接入。 |

以上简写路径均位于对应模块的 `src/Modules/<模块>/Kmic.<模块>.<层>/` 下；逐项文件位置以上表为准。E01 为第 2 节的生成与建模链路。

## 6. 已实施范围与验证边界

| 模块 | 受影响的写入 DbContext | 源码文件数 | 项数 |
|---|---|---|---|
| Configuration | `ConfigurationDbContext` | 2 | 3 |
| Files | `FilesDbContext` | 3 | 14 |
| Identity | `IdentityDbContext` | 5 | 10 |
| Notifications | `NotificationsDbContext` | 3 | 5 |
| Sample | `SampleDbContext` | 4 | 5 |

同模块查询上下文使用相同模型配置，也会读取新的注释元数据；不为读写上下文分别维护两份文字。五个写入 DbContext 各已新增一份注释迁移，并由 EF 更新各自当前快照，具体名称与 SQL 结果见下表。

以下为本轮已批准并执行的实施边界：

1. 再核对分支、HEAD、暂存区、目标文件完整内容、Git 差异及来源。保留已有本地改动；先确认当前模型与当前快照的完整基线，再改候选文字，避免把其他未提交模型变更混入本次迁移。
2. 只调整已核实的 summary/remarks、表和影子列 HasComment；不改属性名、类型、可空性、默认值、索引、外键、数据范围、审计策略、状态编码、SQL 条件或业务数据。没有必要改 BuildingBlocks 的生成方式。
3. 用户已批准必要编译与每模块新增迁移；旧迁移保留，让 EF 更新对应当前 ModelSnapshot，并逐项审查新迁移和 Down 原注释。该例外仅覆盖本轮 37 项，不扩展到 BuildingBlocks、全套测试或数据库操作。
4. 检查实际生成的迁移 SQL：预期业务表变化仅为 COMMENT 元数据更新。迁移代码可能用 AlterColumn 表示 comment 变化，不能只看 API 名称就认定改了列结构；须核对类型、可空性和默认值未变，以及 SQL 不含结构变更、数据更新或索引/约束变更。
5. 若出现计划外模型差异，先定位来源并保留，不随注释任务带入。数据库实际注释、迁移历史与部署执行需在后续目标环境单独核对；不运行 database update、重建数据库或全套 check.cs。
6. 每次实际实施都从修改记录的真实最新编号继续，逐处登记原文、目标文案、源码位置、依据和模型验证结果。M001～M037 是候选编号，不替代实际 C 编号，也不把候选文件预先标为已完成。

## 7. 保留与另列事项

- 原有准确清楚的模型说明继续保留，例如字节数、SHA-256、状态名称、引用归属和需要的版本约束。没有因为同一文件出现候选项就重写全部属性。
- 其他表说明中必要的模块、行含义、唯一性、只追加权限和聚合归属不删；不为统一字数把它们全部缩成表名。
- `scripts/check.cs` 机械用词规则、快照相关普通源码、CN-01/02/03、框架旧术语均保留原待办；本方案未修改这些规则或行为。涉及 BuildingBlocks 的后续写入仍需按用户约定先说明具体范围与影响。
- 已对照框架延后事项和实施待办：本工作不触发短信网关、短信登录、病毒扫描、预签名大文件下载、缓存扩容或数据迁移分析，不把它们顺带实现。
- C2258 方案阶段仅修改方案及交接文档；本轮已实施 37 项，完成必要编译与离线模型、迁移和 SQL 核对；没有执行迁移或同步数据库，也未运行测试、OpenAPI 或项目脚本，未暂存提交推送。

## 实施结果（2026年11月15日 00:04:56，UTC+8）

已完成 M001～M037：17 个源码文件、37 处文案；新增 5 份注释迁移及 5 份设计文件，EF 更新 5 份当前快照，共 32 个实际文件、52 项记录 C2259～C2310；旧迁移保留。

17 个源码文件的精确替换回放通过，133 个 XML 注释块有效；五份当前快照与开始版本相比，恰为方案 37 处注释替换。Up/Down 逐项对应，正向和回退 SQL 各 37 条 COMMENT，其余仅事务与迁移历史记录操作；无结构、约束、索引或业务数据变化。五个运行模型均无待迁移差异。

五模块 Domain 与 Infrastructure（含新增迁移）独立编译均为 0 警告、0 错误，设计入口加载的 DLL 与本次产物一致。API 原始基线编译失败：GetReadableFileQuery.cs:4 的已有未使用 using 触发 IDE0005；Files.Application 验证时仅在临时编译输入副本移除该 using，仓库文件未改，分析器规则未降低。因此不把本轮记为原始工作区整站编译通过。

| 模块 | 原最后迁移 | 新增迁移 | Up/Down 注释数 | 运行模型与快照 |
|---|---|---|---|---|
| Configuration | `20261112192842_InitializeModuleSchema` | `20261114155829_UpdateChineseComments` | 3 / 3 | 无待迁移差异 |
| Files | `20261113142601_StorageIssues` | `20261114155834_UpdateChineseComments` | 14 / 14 | 无待迁移差异 |
| Identity | `20261112231549_CatalogTables` | `20261114155838_UpdateChineseComments` | 10 / 10 | 无待迁移差异 |
| Notifications | `20261113154558_DeliveryAttachments` | `20261114155843_UpdateChineseComments` | 5 / 5 | 无待迁移差异 |
| Sample | `20261113051815_SpecimenAttachments` | `20261114155848_UpdateChineseComments` | 5 / 5 | 无待迁移差异 |

SQL 为离线生成，存于本轮临时验证目录；没有提交临时入口或编译覆盖文件。可从各模块新的 Up/Down 审阅全部 37 处变更。

范围外 2661 个既有文件字节保持不变（含原有测试、web/、BuildingBlocks、旧迁移及五份对话交接文件）；分支/HEAD 不变，暂存区为空。开始 964 个已跟踪修改、10 个未跟踪文件，结束 976 个已跟踪修改、20 个未跟踪文件（新增 10 个迁移/设计文件）；Codex 路径 982 条且无重复，ClaudeCode 清单仍为 0 字节。

未执行数据库连接、database update 或 SQL；测试、OpenAPI 生成、全套项目脚本未运行；未暂存、提交或推送。实际部署库注释和迁移历史未核对，数据库应用需后续另行安排。CN-01/02/03、检查规则及 BuildingBlocks 待办不在本次修复范围。

## 修订记录

| 时间（UTC+8） | 内容 | 实际状态 |
|---|---|---|
| 2026年11月14日 23:29:05 | C2258：汇总 37 项、17 文件的模型中文修订建议，核实实现来源、现有快照和迁移影响 | 仅方案；源码未改，模型验证未运行 |
| 2026年11月15日 00:04:56 | C2259～C2310：落实 37 项，生成五模块注释迁移及当前快照，完成必要编译、模型与正反向 SQL 核对 | 已实施；数据库未执行，测试未运行；API 基线既有 IDE0005 单列 |

## 方案阶段完成核对（C2258，历史）

2026年11月14日 23:32:27（UTC+8）：37 项候选原文与对应快照均一致；本阶段源码零修改，除本阶段方案/跟踪文档外的 2684 个既有文件字节保持不变；git diff --check 通过。分支/HEAD 不变，暂存区为空；964 个已跟踪修改、10 个未跟踪文件（本阶段新增方案 1 个）。编译、测试、项目脚本、模型/迁移生成和数据库操作均未运行，未暂存提交推送。
