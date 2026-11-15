# 中文整理交接摘要

## 本次提交推送授权（C3156，UTC+8）

- 2026年11月15日 19:58:59：用户明确要求推送代码，本次允许暂存、提交和推送当前中文整理及交接成果，取代此前对本次操作的禁止；不扩展为后续常驻授权。
- 范围：193 个已跟踪修改、8 个新注释迁移／Designer，共 201 文件；6 个模板锁文件保留本地、不纳入提交。CN-01～05 仍暂不修复，不新增业务逻辑改动。
- 编译、测试、项目脚本、EF 和数据库操作未运行；注释迁移未应用。此节为提交前记录，实际提交号及推送结果以 Git 和本次回复为准。
- 最新记录 C3156，下一编号 C3157；下方接手提示及历史 Git 状态是各自核对时的快照，接手时须重新核对。

<!-- c3152-copy-prompt:start -->
<a id="claude-code-handoff-prompt"></a>
## 可直接复制给 Claude Code 的接手提示（C3152 起，UTC+8）

- 保存时间：2026年11月15日 19:32:54；Git 状态核对时间：2026年11月15日 19:28:47（UTC+8）。以下是本次核对的快照，接手时必须重新检查；提示中的记录编号已同步到本次保存完成后。
- 本轮仅保存提示并同步交接索引，实际文件为六份文档：`docs/implementation/Codex-中文整理交接摘要.md`、`docs/implementation/修改记录.md`、`docs/implementation/协作交接记录.md`、`docs/implementation/开发交接.md`、`docs/implementation/Codex-中文源码全面复核清单.md`、`docs/implementation/S3-平台模块.md`。源码、业务逻辑、迁移、Designer 和快照未改；CN-01～05 继续暂不修复，此前注释迁移仍未执行。
- 本轮记录 C3152～C3155；下一编号 C3156。下方各历史章节中的“下一编号”按各自记录时间理解，以修改记录实际最大编号为准。

```text
请在 D:\KMIC-项目\KMIC.Platform 接手当前中文整理成果。先阅读最新交接并核对 Git 实际状态，不因历史“已处理清单”就默认文件合格，也不要重复推进已经完成的事项。

最近核对时间：2026年11月15日 19:28:47（UTC+8）。
当前分支：codex-cn-copy-cleanup-2026-11-14
HEAD：5319686960c5458c3c7a6aee5da2c5f66ff7a303
未提交状态：193 个已跟踪文件有修改，14 个未跟踪文件，暂存区为空。开始前重新检查；完整保留所有已有工作。
14 个未跟踪文件包括：
- Configuration、Identity、Notifications、Operations 四模块的 20261115093130_ClarifyModelComments.cs 及对应 Designer，共 8 个文件。
- templates/kmic-module 下原有 6 个 packages.lock.json，包括 5 个源码项目和 1 个测试项目；不要清理或重建它们。

必读文档：
1. D:\KMIC-项目\KMIC.Platform\CLAUDE.md
2. D:\KMIC-项目\KMIC.Platform\docs\framework\00-README.md
3. D:\KMIC-项目\KMIC.Platform\docs\framework\06-coding-standards.md，重点第 10.3 节及附录 A
4. D:\KMIC-项目\KMIC.Platform\docs\implementation\S3-平台模块.md
5. D:\KMIC-项目\KMIC.Platform\docs\implementation\协作交接记录.md
6. D:\KMIC-项目\KMIC.Platform\docs\implementation\开发交接.md
7. D:\KMIC-项目\KMIC.Platform\docs\implementation\修改记录.md
8. D:\KMIC-项目\KMIC.Platform\docs\implementation\Codex-中文整理交接摘要.md
9. D:\KMIC-项目\KMIC.Platform\docs\implementation\Codex-中文源码全面复核清单.md
同时查看双方处理路径清单：
- D:\KMIC-项目\KMIC.Platform\docs\implementation\Codex-已按中文规范处理的文件.md
- D:\KMIC-项目\KMIC.Platform\docs\implementation\ClaudeCode-已按中文规范处理的文件.md

当前交接结论：
- 8 处模型说明已按核实改稿落地，新增四模块的注释迁移并同步快照；仅完成静态核对，未执行数据库迁移，也未运行 EF 模型差异检查。此前专项授权不扩展到其他模型或迁移改动。
- CN-01～05 已完成静态核实，用户明确决定暂不修复。问题、触发条件、影响范围、优先级、建议方案及代码位置集中在《开发交接》顶部的“CN-01～05”章节。
- 建议顺序为 CN-01/P1、CN-05/P1、CN-04/P2、CN-02/P2、CN-03/P2；优先级不构成修复授权，建议方案尚未实施。
- 以 C3141 起的最新纠正结论为准：含密载荷允许按保留期存储；Quartz 已有执行互斥；含密邮件被目录禁止；当前短信网关是永久失败的占位实现。不要沿用历史记录中的不准确推断，也不要将这些问题标为已修复。

继续遵守：
- 不改业务逻辑，不修改测试项目、web/、BuildingBlocks；不擅自修改迁移或快照。
- 不编译、不运行测试、整套项目检查脚本、EF 命令或数据库操作。
- 不清理、重置、stash、暂存、提交或推送。
- 中文维护仍以准确、自然、简洁、有用为准；先核实实现，再判断删除或改写，不机械补模板或批量删改。
- 如有实际修改，逐处记录 UTC+8 时间、文件与位置、修改内容和依据，同步实际文件列表、相关路径清单、协作交接及 S3 进度；未运行的检查如实注明。

最新记录为 C3155，下一编号从 C3156 起，以《修改记录》实际最大编号为准。
```

<!-- c3152-copy-prompt:end -->

<!-- c3141-cn-handoff:start -->
## CN-01～05：已核实、暂不修复（C3141 起，UTC+8）

- 更新时间：2026年11月15日 18:58:48。用户决定五项暂不修复；本轮仅同步交接。静态核实不等于已运行复现，建议方案尚未实施，也不作为已确认设计或后续修复授权。
- P1 建议在含密消息／真实短信能力用于生产前处理；P2 建议在对应功能验收前处理。当前暂缓决定不因优先级自动改变。
- 完整问题、触发条件、影响范围、建议与代码位置集中在[开发交接](开发交接.md#cn-findings-20261115)，其余文档仅维护索引和纠正说明，避免多处复制正文。
- 此前 8 处模型说明已落地，注释迁移尚未执行；本轮不改变该状态，不修改任何源码、迁移、Designer 或快照。原 1267 文件复核阶段及 118 个改后复读记录保持；双方处理路径清单仍为 Codex 1032 条／Claude Code 0 条，本轮无新增源码处理路径。
- 本轮实际修改文件仅为：`docs/implementation/开发交接.md`、`docs/implementation/Codex-中文整理交接摘要.md`、`docs/implementation/Codex-中文源码全面复核清单.md`、`docs/implementation/协作交接记录.md`、`docs/implementation/S3-平台模块.md`、`docs/implementation/修改记录.md`。
- 分支 `codex-cn-copy-cleanup-2026-11-14`、HEAD `5319686960c5458c3c7a6aee5da2c5f66ff7a303`；起点及收尾均为 193 个已跟踪修改、14 个未跟踪文件，暂存区为空。所有既有工作保留。
- 编译、测试、整套项目脚本、EF 命令、运行复现、SQL 生成及数据库操作：未运行；未清理、重置、stash、暂存、提交或推送。
- 记录 C3141～C3151；下一编号 C3152。以下历史 CN 描述以本节及开发交接的纠正结论为准；历史修改记录和协作记录保留原文，不能继续沿用其中已被纠正的推断。

| 建议顺序 | 编号 | 优先级 | 当前问题 | 状态 |
|---|---|---|---|---|
| 第一项 | [CN-01](开发交接.md#cn-01) | P1 | 死信详情未隐藏含密载荷 | 已静态核实，用户决定暂不修复 |
| 第二项 | [CN-05](开发交接.md#cn-05) | P1 | 含密短信清空内容后仍允许人工重试 | 已静态核实，用户决定暂不修复 |
| 第三项 | [CN-04](开发交接.md#cn-04) | P2 | 模拟身份下上传与关联使用的用户不一致 | 已静态核实，用户决定暂不修复 |
| 第四项 | [CN-02](开发交接.md#cn-02) | P2 | 结构化请求日志保留实际路径 | 已静态核实，用户决定暂不修复 |
| 第五项 | [CN-03](开发交接.md#cn-03) | P2 | 手动触发的运行检查与受理存在竞争 | 已静态核实，用户决定暂不修复 |

### 必须沿用的纠正结论

| 事项 | 当前准确结论 |
|---|---|
| CN-01 的保护边界 | 设计允许 Outbox／Inbox 按保留期保存载荷；本条缺陷是管理详情未隐藏含密内容。不能把“不落库或立刻清空载荷”当作既定要求。 |
| CN-03 的并发边界 | Quartz 已有禁止同一任务并发执行的特性；本条确认的是重复受理、排队及检查与提交之间的竞争，不是已证实任务同时执行。 |
| CN-04 的影响范围 | 已确认可写模拟身份下 Available 文件的合法关联可能被拒；普通登录和 Detached 分支不能一概算入影响，未据此证明越权。 |
| CN-05／C3086 的旧推断 | 旧记录中将含密邮件列入影响的推断作废：目录禁止含密邮件，站内消息不走这条投递重试链。当前默认短信网关返回永久失败；接入真实网关后才需判断空参数请求的具体后果，不能宣称空短信一定发送成功。 |


<!-- c3141-cn-handoff:end -->

<!-- c3123-progress:start -->
## 八处模型说明已落地（C3123 起，UTC+8）

- 更新时间：2026年11月15日 17:39:13；状态：8 处已核实改稿已落地，4 组注释迁移及目标模型、4 份快照已同步；静态核对完成，编译、测试和迁移运行未验证。
- 用户已明确授权新增仅更新注释的迁移并同步模型快照；旧迁移不改，其余原约束保持。本轮只处理已核实的 8 处说明，不改业务逻辑。
- 实际文件：7 源文件／8 处中文，4 份现有快照，4 个新迁移及 4 个新 Designer，7 份交接／清单文件，共 26 文件。所有旧差异保留。
- 迁移：Notifications 4 项、Identity 1 项、Configuration 1 项、Operations 2 项，共 6 列／2 表；四模块均新增 `20261115093130_ClarifyModelComments`。Up 更新说明，Down 逆序恢复旧说明；迁移尚未应用到数据库。
- 静态核对：8 处源码精确替换、8 处快照 HasComment 替换；6 个列的类型、长度和可空性不变；4 个 Designer 的目标模型与各自快照一致；86 个既有迁移及 Designer 字节未变。已复读 7 源文件中的中文和新迁移，解析 98 组源码 XML 注释。
- 验证边界：未编译、未运行测试或整套项目脚本；未运行 EF 命令、模型差异 API、提供程序 SQL 生成或数据库操作。因此不声称运行时模型无差异、SQL 已生成验证或迁移已执行。
- 当前阶段：原清单 1267 文件不扩为迁移全面复查；1174 个 C# 文件及 2 个规则文件保留实现核对记录，55 个配置／清单中文已通读，36 个无中文支持文件仅扫描。改后复读记录由 112 增至 118（本轮 7 文件中 1 个已有该记录）；这 8 处模型说明待办已关闭，新增迁移／目标模型及快照仅作本轮专项核对。
- 路径清单：Codex 1032 条（本轮新增 10 条），Claude Code 0 条；历史路径不作为复查排除项或质量证明。实际文件列表见[中文整理交接摘要](Codex-中文整理交接摘要.md)，逐处改动及依据见[修改记录](修改记录.md)。
- Git 起点：183 个已跟踪修改、6 个原有未跟踪锁文件、空暂存区。收尾：193 个已跟踪修改、14 个未跟踪文件（原有 6 锁文件＋本轮 8 迁移文件），暂存区仍为空。分支 `codex-cn-copy-cleanup-2026-11-14` 与 HEAD `5319686960c5458c3c7a6aee5da2c5f66ff7a303` 不变。
- CN-01～05 行为疑点继续单列，未改业务逻辑；未清理、重置、stash、暂存、提交或推送。
- 本轮记录 C3123～C3140；下一编号 C3141。此次授权内的文件落地与交接已完成，运行验证留待允许执行时进行。

### 本轮八处改稿

| 记录 | 源文件与成员 | 已落地的完整改稿 |
|---|---|---|
| C3124 | `src/Modules/Notifications/Kmic.Notifications.Domain/Captured/CapturedMessage.cs:37`，Address | 收件地址（未脱敏） |
| C3125 | `src/Modules/Notifications/Kmic.Notifications.Domain/Captured/CapturedMessage.cs:42`，Role | 邮件收件方式 |
| C3126 | `src/Modules/Notifications/Kmic.Notifications.Domain/Deliveries/Delivery.cs:89`，Role | 邮件收件方式 |
| C3127 | `src/Modules/Identity/Kmic.Identity.Domain/Roles/Role.cs:54`，IsProtected | 是否受保护 |
| C3128 | `src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/Configurations/SystemSettingValueConfiguration.cs:13`，ToTable.HasComment | 系统设置的已保存值；Configuration 模块；每个设置一行，无记录时使用代码默认值 |
| C3129 | `src/Modules/Notifications/Kmic.Notifications.Domain/Templates/NotificationTemplate.cs:99`，ActiveVersions | 各渠道选用的发布版本号（JSON：渠道编码到版本号） |
| C3130 | `src/Modules/Operations/Kmic.Operations.Domain/Audit/AuditLog.cs:47`，ActualUserId | 模拟身份时的实际操作人 |
| C3131 | `src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/Configurations/ScheduledJobSettingConfiguration.cs:13`，ToTable.HasComment | 定时任务的自定义设置；Operations 模块；空值使用代码默认值，恢复默认时保留记录；框架据此同步触发器 |

### 本轮实际文件列表（26 个）

| 文件 | 本轮修改及核对范围 |
|---|---|
| `src/Modules/Notifications/Kmic.Notifications.Domain/Captured/CapturedMessage.cs` | 已核实中文改稿；修改后整份中文已复读 |
| `src/Modules/Notifications/Kmic.Notifications.Domain/Deliveries/Delivery.cs` | 已核实中文改稿；修改后整份中文已复读 |
| `src/Modules/Identity/Kmic.Identity.Domain/Roles/Role.cs` | 已核实中文改稿；修改后整份中文已复读 |
| `src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/Configurations/SystemSettingValueConfiguration.cs` | 已核实中文改稿；修改后整份中文已复读 |
| `src/Modules/Notifications/Kmic.Notifications.Domain/Templates/NotificationTemplate.cs` | 已核实中文改稿；修改后整份中文已复读 |
| `src/Modules/Operations/Kmic.Operations.Domain/Audit/AuditLog.cs` | 已核实中文改稿；修改后整份中文已复读 |
| `src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/Configurations/ScheduledJobSettingConfiguration.cs` | 已核实中文改稿；修改后整份中文已复读 |
| `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Migrations/NotificationsDbContextModelSnapshot.cs` | 仅同步本轮 HasComment；其他模型文本未变 |
| `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Migrations/IdentityDbContextModelSnapshot.cs` | 仅同步本轮 HasComment；其他模型文本未变 |
| `src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/Migrations/ConfigurationDbContextModelSnapshot.cs` | 仅同步本轮 HasComment；其他模型文本未变 |
| `src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/Migrations/OperationsDbContextModelSnapshot.cs` | 仅同步本轮 HasComment；其他模型文本未变 |
| `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Migrations/20261115093130_ClarifyModelComments.cs` | 新增注释迁移；Up／Down 及列元数据静态核对，未运行 |
| `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Migrations/20261115093130_ClarifyModelComments.Designer.cs` | 新增迁移目标模型；与本轮快照一致，非历史中文全面复查 |
| `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Migrations/20261115093130_ClarifyModelComments.cs` | 新增注释迁移；Up／Down 及列元数据静态核对，未运行 |
| `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Migrations/20261115093130_ClarifyModelComments.Designer.cs` | 新增迁移目标模型；与本轮快照一致，非历史中文全面复查 |
| `src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/Migrations/20261115093130_ClarifyModelComments.cs` | 新增注释迁移；Up／Down 及列元数据静态核对，未运行 |
| `src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/Migrations/20261115093130_ClarifyModelComments.Designer.cs` | 新增迁移目标模型；与本轮快照一致，非历史中文全面复查 |
| `src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/Migrations/20261115093130_ClarifyModelComments.cs` | 新增注释迁移；Up／Down 及列元数据静态核对，未运行 |
| `src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/Migrations/20261115093130_ClarifyModelComments.Designer.cs` | 新增迁移目标模型；与本轮快照一致，非历史中文全面复查 |
| `docs/implementation/修改记录.md` | 同步逐处记录、实际文件、阶段、路径或交接；不改历史记录 |
| `docs/implementation/Codex-中文整理交接摘要.md` | 同步逐处记录、实际文件、阶段、路径或交接；不改历史记录 |
| `docs/implementation/Codex-中文源码全面复核清单.md` | 同步逐处记录、实际文件、阶段、路径或交接；不改历史记录 |
| `docs/implementation/开发交接.md` | 同步逐处记录、实际文件、阶段、路径或交接；不改历史记录 |
| `docs/implementation/协作交接记录.md` | 同步逐处记录、实际文件、阶段、路径或交接；不改历史记录 |
| `docs/implementation/S3-平台模块.md` | 同步逐处记录、实际文件、阶段、路径或交接；不改历史记录 |
| `docs/implementation/Codex-已按中文规范处理的文件.md` | 同步逐处记录、实际文件、阶段、路径或交接；不改历史记录 |

<!-- c3123-progress:end -->

> 下方保留截至 C3122 及更早的历史记录；这 8 处模型说明的当前状态以上方落地结论为准。

<!-- c3112-model-audit:start -->
## 八处模型说明复核结论（C3112 起，UTC+8）

- 更新时间：2026年11月15日 17:25:44；状态：原约束内的中文整理和交接已完成；8 处数据库说明改稿已核实，待另行允许配套迁移后落地。
- 本轮已逐项核对 8 位置／7 源文件的语义、调用方、生成链、映射及现有快照。6 处是列说明，2 处是表说明；均会进入关系模型，未发现可按普通代码注释直接修改的项。
- 原约束并未全面禁止修改模型说明，而是要求先核实影响、单独记录，并禁止修改迁移和快照。现有 8 处快照注释均与源码旧说明一致；单改源码会留下这 8 处模型注释差异。项目的 ModuleMigrator 另有 HasPendingModelChanges 检查，并调用 MigrateAsync 执行迁移；本轮未运行它们，不声称已复现运行错误。
- 所有改稿已结合实现核实并复读；没有待猜测的业务含义。需要另行决定的是：是否允许为这 6 列、2 表新增仅更新注释的迁移、同步快照，并执行必要的模型差异校验。推荐随下一次正常模型变更一并处理；届时审查生成差异，不修改旧迁移。
- 本轮实际源码修改：0 文件／0 处。模型、持久化配置、旧迁移、快照、测试、web/ 和 BuildingBlocks 全部保留。未添加旧注释覆盖配置、关闭模型差异检查或增加重复 remarks 来绕开约束。

### 已核实的完整改稿（均未落地）

| 记录 | 源文件与位置 | 当前说明 | 完整改稿 | 核对依据及现有快照 |
|---|---|---|---|---|
| C3113 | `src/Modules/Notifications/Kmic.Notifications.Domain/Captured/CapturedMessage.cs:37`，Address（列说明） | 本该发往的完整地址 | 收件地址（未脱敏） | NotificationProcessor.AddDelivery 在捕获分支将 Resolve 返回的完整 address 传入 CapturedMessage.Create；脱敏值 masked 仅传给 Delivery，捕获记录保存邮箱或手机号，故不是假设原本会发送成功的地址；快照 `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Migrations/NotificationsDbContextModelSnapshot.cs:470` 含同一旧说明；原文保留 |
| C3114 | `src/Modules/Notifications/Kmic.Notifications.Domain/Captured/CapturedMessage.cs:42`，Role（列说明） | 邮件里的角色 | 邮件收件方式 | NotificationProcessor 将 Recipient.Role 转为 DeliveryRole 并传入捕获记录；DeliveryRole 的 To／Cc／Bcc 表示收件、抄送、密送，不是 Identity 权限角色；仅邮件渠道使用；快照 `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Migrations/NotificationsDbContextModelSnapshot.cs:508` 含同一旧说明；原文保留 |
| C3115 | `src/Modules/Notifications/Kmic.Notifications.Domain/Deliveries/Delivery.cs:89`，Role（列说明） | 邮件里的角色 | 邮件收件方式 | Delivery.Queue／Capture／Skip 保存同一 DeliveryRole；DeliveryWork 将 Role.Code 传入 EmailMessage，MailKitEmailSender.Build 分别写 mime.To／Cc／Bcc，含义与捕获记录一致；快照 `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Migrations/NotificationsDbContextModelSnapshot.cs:660` 含同一旧说明；原文保留 |
| C3116 | `src/Modules/Identity/Kmic.Identity.Domain/Roles/Role.cs:54`，IsProtected（列说明） | 是否受保护：只有持有受保护角色的人才能管理它，也不能借管理它的持有人绕开 | 是否受保护 | RolePolicy 和 UserPolicy 依赖操作者当前角色是否受保护，不是仅持有某个授予即可；具体凭据操作、模拟身份及最后一位受保护用户限制已集中在策略说明。属性只保留标志含义，避免重复或错误概括权限规则；快照 `src/Modules/Identity/Kmic.Identity.Infrastructure/Persistence/Migrations/IdentityDbContextModelSnapshot.cs:605` 含同一旧说明；原文保留 |
| C3117 | `src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/Configurations/SystemSettingValueConfiguration.cs:13`，ToTable.HasComment（表说明） | 被修改过的系统设置值；Configuration 模块；一个设置一行，没有这一行就用代码里的默认值 | 系统设置的已保存值；Configuration 模块；每个设置一行，无记录时使用代码默认值 | SetSettingCommandHandler 序列化并保存传入值，不要求其异于默认值；ResetSettingCommandHandler 删除当前记录，变更历史另存；SettingsSnapshot 未保存时返回 T.Default。表不是仅保存与默认值不同的项；快照 `src/Modules/Configuration/Kmic.Configuration.Infrastructure/Persistence/Migrations/ConfigurationDbContextModelSnapshot.cs:783` 含同一旧说明；原文保留 |
| C3118 | `src/Modules/Notifications/Kmic.Notifications.Domain/Templates/NotificationTemplate.cs:99`，ActiveVersions（列说明） | 各渠道当前生效的版本号，JSON：渠道编码到版本号；没有的渠道用代码里的默认内容 | 各渠道选用的发布版本号（JSON：渠道编码到版本号） | Retire 只改状态，保留选用映射；RevertToDefault 删除指定渠道映射，历史版本保留；TemplateRepository.GetActiveVersionsAsync 和详情查询可读取退役项。默认回退条件已放在类 remarks，列说明不把保留记录一概说成当前发送内容；快照 `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Migrations/NotificationsDbContextModelSnapshot.cs:893` 含同一旧说明；原文保留 |
| C3119 | `src/Modules/Operations/Kmic.Operations.Domain/Audit/AuditLog.cs:47`，ActualUserId（列说明） | 模拟登录时真正动手的人 | 模拟身份时的实际操作人 | CurrentUser.From 将 access.ImpersonatorUserId 赋给 ActualUserId，AuditRecorder.Build 和 RecordAuditCommandHandler 原样传递；对应发起模拟的用户，与 ActorUserId 中被模拟用户区分；快照 `src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/Migrations/OperationsDbContextModelSnapshot.cs:479` 含同一旧说明；原文保留 |
| C3120 | `src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/Configurations/ScheduledJobSettingConfiguration.cs:13`，ToTable.HasComment（表说明） | 定时任务的覆盖值；Operations 模块；运维改过的开关与执行时间，只存改过的项，没改的项按代码里的默认值；框架同步触发器时读这张表 | 定时任务的自定义设置；Operations 模块；空值使用代码默认值，恢复默认时保留记录；框架据此同步触发器 | SetJobOverrideCommandHandler 可保存与默认值相同的 Enabled／Cron；ScheduledJobSetting.RestoreDefault 只清空两项，不删除记录，后续继续使用该记录的并发版本。说明空值回退与记录保留，不再声称只存改过的项；快照 `src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/Migrations/OperationsDbContextModelSnapshot.cs:632` 含同一旧说明；原文保留 |

### 本轮实际文件与处理阶段

- 已改文件仅为 6 个交接文档：`修改记录.md`、`Codex-中文整理交接摘要.md`、`Codex-中文源码全面复核清单.md`、`开发交接.md`、`协作交接记录.md`、`S3-平台模块.md`。
- 8 处源码原文未变；其所在 7 文件的阶段为“已结合实现核对；完整改稿已核实、未落地”，保留模型待办，不记作源码修改或处理合格。
- 双方处理路径清单核对后未新增：Codex 1022 条、Claude Code 0 条；不把拟稿写入已处理清单。历史清单仍不作为复查排除项或质量证明。
- 全范围阶段不变：1174 个 C# 文件及 2 个规则文件有实现核对记录；55 个配置／清单文件中文已通读；36 个无中文支持文件仅扫描。既有 112 个改后复读记录保留；本轮没有新增“修改后已复读”源码记录。

### 另列的行为疑点（本轮未修改、未运行验证）

- CN-01：含密死信查询的载荷保护待补齐。
- CN-02：请求摘要使用路由模板，结构化属性仍含实际 RequestPath。
- CN-03：手动触发任务时，运行检查与提交触发请求不是原子操作。
- CN-04：模拟身份下，文件上传登记和关联检查使用的用户身份不一致。
- CN-05：含密失败投递的内容已清空，人工重试仍可能排队且不恢复内容。

- Git 起点：183 个已跟踪修改、5 个未跟踪模板锁文件；收尾：183 个已跟踪修改、6 个未跟踪锁文件，暂存区仍为空。分支 `codex-cn-copy-cleanup-2026-11-14` 与 HEAD `5319686960c5458c3c7a6aee5da2c5f66ff7a303` 未变。复核期间新增未跟踪文件 `templates/kmic-module/tests/Modules/Customer/Kmic.Customer.Domain.Tests/packages.lock.json`；本轮未创建或修改它，来源未确认，已原样保留。基线 2710 个文件中，仅 6 份交接文档变化，其他 2704 个文件字节未变。
- 编译、测试、整套项目检查脚本、OpenAPI／模型生成及数据库操作：未运行；未清理、重置、stash、暂存、提交或推送。
- 本轮记录 C3112～C3122；下一编号 C3123。原约束内工作完成，8 处模型说明仍以“改稿已核实、待配套迁移”交接，不能宣称已落地或行为问题已修复。

<!-- c3112-model-audit:end -->

> 以下保留此前阶段记录；最新模型说明状态以上方 C3112 起的结论为准。

<!-- c3052-progress:start -->
## 本次继续复核（C3052 起，UTC+8）

- 更新时间：2026年11月15日 17:09:29；状态：本次全范围中文复核完成；模型及行为待办保留。
- 起点：147 个已跟踪修改，5 个未跟踪模板锁文件，暂存区为空；分支 `codex-cn-copy-cleanup-2026-11-14`，HEAD `5319686960c5458c3c7a6aee5da2c5f66ff7a303`。既有工作全部保留。
- 本次 620 个文件有完整实现核对记录；实际修改 41 个源码及规则文件、55 处文字，最新记录 C3111。历史阶段不代替本次核对。
- 进展：本次 620 文件已结合实现核对（新增 553、重叠 67），55 个支持文件中文已通读；全清单 1174 个 C# 文件均有逐文件实现核对记录，36 个无中文支持文件仍仅扫描。39 个 C# 文件及 2 个规则文件共 55 处修改均已复读；静态回放／非注释文本、150 组 XML、37 组实体属性文档及既有工作指纹核对通过。当前 183 个已跟踪修改、5 个原有未跟踪锁文件、暂存区为空；Codex 路径 1022、Claude 0。模型说明 8 位置／7 文件及 CN-01～05 保留，功能验收状态不变；记录至 C3111，下一编号 C3112。
- 不改业务逻辑、测试、web/、BuildingBlocks、迁移、快照或生成列说明的属性摘要。编译、测试、整套项目检查脚本、OpenAPI 生成及数据库操作未运行；未清理、重置、stash、暂存、提交或推送。

### 本次行为疑点（仅记录）

- CN-05（原记 C3086；2026年11月15日 18:53:59 更正，C3146）：含密短信在终态清空 Body／SmsParameters 后，仍可能在投递 CreatedAt 后 300 秒内人工重新排队；再次领取未过期且地址可用时会以空参数调用网关，已过期则失败。目录禁止含密邮件，站内消息不走该重试链；当前短信网关返回永久失败，不能据此断言真实空短信发送成功。完整事实、建议及代码位置见[开发交接 CN-05](开发交接.md#cn-05)；未改逻辑，未运行复现。

### 本次模型说明待办（未改）

- M-06（C3085，2026年11月15日 16:52:03）：原文“各渠道当前生效的版本号，JSON：渠道编码到版本号；没有的渠道用代码里的默认内容”暂不修改；建议后续改为“各渠道选用的发布版本号（JSON：渠道编码到版本号）”，默认回退条件已放在类 remarks；核对依据：Retire 保留映射；GetTemplateQuery 和 TemplateRepository 可读取退役项，缺少映射不总表示正在使用默认内容；EntityCommentGenerator 会采集实体公开属性 summary 生成 HasComment，NotificationTemplateConfiguration 将该属性映射为 jsonb。直接改摘要会改变列注释来源，不为文案单独生成迁移
- M-07（C3100，2026年11月15日 16:58:33）：原文“模拟登录时真正动手的人”暂不修改；建议改为“模拟身份时的实际操作人”；核对依据：CurrentUser.From／AuditRecorder／RecordAuditCommandHandler 依次保留发起模拟者 ID；EntityCommentGenerator 采集该实体公开属性 summary，修改会改变生成列注释来源。参数说明已改，持久化摘要与旧迁移保持不变
- M-08（C3101，2026年11月15日 16:58:33）：原文包含“运维改过的开关与执行时间，只存改过的项，没改的项按代码里的默认值”，暂不修改；建议后续写明“定时任务的自定义设置；空值使用代码默认值，恢复默认时保留记录；框架据此同步触发器”；核对依据：ScheduledJobSetting.RestoreDefault 清空两值但保留记录，SetJobOverride 也可直接保存与默认值相同的值；HasComment 是关系表注释模型来源，改动可能生成模型差异，不为文案直接改配置或旧迁移

### 本次实际修改文件

| 文件 | 位置和记录 | 阶段 |
|---|---|---|
| `.github/workflows/ci.yml` | Gitea 迁移后的工作流说明（C3110） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `BannedSymbols.Modules.txt` | 清单的适用范围（C3108）；方法重载的签名规则（C3109） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `scripts/dev.cs` | 已有容器的密码来源（C3103）；Get 前重复的本机状态说明（C3104） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `scripts/new-module.cs` | 指定解决方案的原因（C3102） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Hosts/Kmic.MigrationRunner/NumbersCommands.cs` | 缺少日期时的提示（C3105） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Hosts/Kmic.MigrationRunner/RunnerContext.cs` | GuardedAsync 的异常输出约定（C3106） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Hosts/Kmic.Operator/OperatorContext.cs` | GuardedAsync 的异常输出约定（C3107） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Files/Kmic.Files.Application/Administration/FileDetail.cs` | Profile 参数说明（C3053） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Files/Kmic.Files.Application/Administration/FileListItem.cs` | Profile 参数说明（C3054） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Files/Kmic.Files.Application/Administration/ListFilesQuery.cs` | Profile 参数说明（C3055）；OwnerModule 参数说明（C3060）；OwnerType 参数说明（C3061）；OwnerId 参数说明（C3062） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Files/Kmic.Files.Application/Administration/ListStorageIssuesQueryHandler.cs` | 分页后补文件名的行内说明（C3057） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Files/Kmic.Files.Application/Administration/StorageIssueItem.cs` | FileName 参数说明（C3058） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Files/Kmic.Files.Application/ContractQueries/GetFileDownloadQuery.cs` | 类 remarks（C3052） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Files/Kmic.Files.Application/Uploads/Start/StartUploadCommand.cs` | Profile 参数说明（C3067） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Files/Kmic.Files.Application/Uploads/UploadProfileTarget.cs` | Name 参数说明（C3068） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Files/Kmic.Files.Domain/Files/StoredFile.cs` | Build 中枚举可彻底删除状态前的行内说明（C3059）；Expire 的清理锁约定（C3077） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Files/Kmic.Files.Domain/Files/StoredFileAction.cs` | Complete 动作 summary（C3066） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Files/Kmic.Files.Presentation/Administration/ListFilesRequest.cs` | Profile 参数说明（C3056）；OwnerModule 参数说明（C3063）；OwnerType 参数说明（C3064）；OwnerId 参数说明（C3065） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Notifications/Kmic.Notifications.Application/Channels/SendResult.cs` | Permanent 方法说明（C3078） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Get/TemplateChannelInfo.cs` | 类型用途（C3079） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Notifications/Kmic.Notifications.Domain/Notifications/Notification.cs` | Create 方法重复的 JSON 说明（C3081） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Notifications/Kmic.Notifications.Domain/Templates/ITemplateRepository.cs` | GetActiveVersionsAsync 用途（C3080） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Notifications/Kmic.Notifications.Domain/Templates/NotificationTemplate.cs` | 类型 remarks 的默认内容条件（C3082）；ActiveVersionOf 方法说明（C3083）；ParseActiveVersions 方法说明（C3084） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/ExportAuditCommand.cs` | ActualUserId 参数说明（C3087） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Operations/Kmic.Operations.Application/BackgroundJobs/Cancel/CancelBackgroundJobCommand.cs` | 类型用途（C3098）；本人取消所需权限（C3099） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Operations/Kmic.Operations.Application/Messages/Summary/MessageSummaryResponse.cs` | Modules 参数说明（C3089） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/Restore/RestoreJobDefaultCommandHandler.cs` | null 设置分支前的行内说明（C3092） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Operations/Kmic.Operations.Domain/Errors/OperationsErrors.cs` | BackgroundJobFinished 用途（C3094）；BackgroundJobFinished 提示（C3095） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Operations/Kmic.Operations.Infrastructure/Persistence/Configurations/AuditLogConfiguration.cs` | 索引配置前的重复说明（C3090） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Operations/Kmic.Operations.Presentation/Audit/DownloadAuditExportEndpoint.cs` | 下载权限与作业 404 条件（C3093） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Operations/Kmic.Operations.Presentation/Audit/ExportAuditRequest.cs` | ActualUserId 参数说明（C3088） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Operations/Kmic.Operations.Presentation/BackgroundJobs/CancelBackgroundJobEndpoint.cs` | 已处于取消中时的接口响应（C3096） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Operations/Kmic.Operations.Presentation/BackgroundJobs/CancelMyJobEndpoint.cs` | 已处于取消中时的接口响应（C3097） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Operations/Kmic.Operations.Presentation/ScheduledJobs/RestoreJobDefaultEndpoint.cs` | 设置记录不存在时的响应说明（C3091） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Sample/Kmic.Sample.Application/ContractQueries/SpecimenReferences.cs` | 重复的分批与失败说明（C3070） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Sample/Kmic.Sample.Application/Specimens/Attachments/GetContent/GetSpecimenAttachmentContentQuery.cs` | remarks 中的审计时点（C3075） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Sample/Kmic.Sample.Contracts/ISpecimenReferences.cs` | ResolveAsync 的失败行为（C3069） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Sample/Kmic.Sample.Domain/Errors/SampleErrors.cs` | NotFound 用途说明（C3072） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Sample/Kmic.Sample.Domain/Specimens/SpecimenStatus.cs` | Draft 状态说明（C3071） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Sample/Kmic.Sample.Presentation/Specimens/DownloadSpecimenAttachmentEndpoint.cs` | 接口说明中的 404 条件（C3074）；接口说明中的审计时点（C3076） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Sample/Kmic.Sample.Presentation/Specimens/GetSpecimenEndpoint.cs` | 接口说明中的 404 条件（C3073） | 已结合实现核对；修改后已复读；编译测试未运行 |

### 本轮静态核对与阶段

- 全范围 1267 文件：1174 个 C# 文件及 2 个规则文件已结合实现核对；55 个配置／清单文件中文已通读；36 个无中文支持文件仅扫描。共有 112 文件保留改后复读记录；本次 41 个修改文件均已复读。阶段逐文件列在全面复核清单中，不将历史路径清单当作质量证明。
- 55 次精确替换从本轮起点回放一致；9 处运行时提示／接口说明字符串均有编号，其余非注释文本不变；150 组 XML 注释可解析。StoredFile、Notification、NotificationTemplate 的 37 组属性文档及所有 HasComment 未改，模型说明累计 8 处／7 文件待办。
- 起点以外 2662 个既有文件字节未变，含 5 个未跟踪模板锁文件；保留此前未提交工作。当前 183 个已跟踪修改、暂存区为空，分支与 HEAD 不变，git diff --check 无输出。
- 本轮新增 CN-05：含密失败投递清空内容后仍可能在有效期内人工排队，重试不恢复正文和短信参数；仅静态记录，未运行复现。CN-01～04 也未修复。
- 记录至 C3111；后续从 C3112 起并重新核对 Git。下一阶段是按独立任务决定 8 处模型说明如何随正常模型变更处理，以及 CN-01～05 的业务方案；本轮不修改这些行为，不宣称运行验收。
- 编译、测试、整套项目检查脚本、OpenAPI 生成和数据库操作：未运行；未清理、重置、stash、暂存、提交或推送。

<!-- c3052-progress:end -->

> 以下为此前阶段记录；本次状态以上方 C3052 起的记录为准。

<!-- c3019-progress:start -->
## 本次继续复核（C3019 起，UTC+8）

- 更新时间：2026年11月15日 16:22:10；状态：本次复核已完成；模型与行为待办保留。
- 起点：129 个已跟踪修改，5 个未跟踪模板锁文件，暂存区为空；分支 `codex-cn-copy-cleanup-2026-11-14`，HEAD `5319686960c5458c3c7a6aee5da2c5f66ff7a303`。既有工作全部保留。
- 本次 241 个文件有完整实现核对记录；实际修改 23 个源码文件、31 处文字，最新记录 C3051。历史阶段不代替本次核对。
- 进展：241 文件实现核对、23 修改文件／31 处中文复读及静态差异核对完成；Configuration 与 Identity 清单内全部 C# 均有实现核对记录，模型待办保留，C3051 收尾。
- 不改业务逻辑、测试、web/、BuildingBlocks、迁移、快照或生成列说明的属性摘要。编译、测试、整套项目检查脚本、OpenAPI 生成及数据库操作未运行；未清理、重置、stash、暂存、提交或推送。

### 本次模型说明待办（未改）

- M-05（C3028，2026年11月15日 15:52:08）：SystemSettingValueConfiguration.ToTable 的 HasComment 仍写“被修改过的系统设置值”。读取与写入链路表明这里保存的是当前显式保存值，重置后记录会删除，历史另存在 SystemSettingChange；建议在后续模型变更时改为“已保存的系统设置值”。已对照本地 EF Core Relational 10.0.14 的 TableBuilder.HasComment、RelationalAnnotationNames.Comment 与迁移 TableOperation.Comment 文档，确认会进入关系模型及数据库表注释，不能当成普通代码注释修改。本次不改该调用、旧迁移或快照，不仅为措辞新建迁移；未运行模型生成、数据库或测试。加上原有 4 位置／3 文件，现有 5 个表／列说明位置、4 个文件待办，均未视为已解决。

### 本次实际修改文件

| 文件 | 位置和记录 | 阶段 |
|---|---|---|
| `src/Modules/Configuration/Kmic.Configuration.Application/ContractQueries/DictionaryLookup.cs` | 类 remarks（C3020） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/DictionaryChanges.cs` | Invalidate 的 summary（C3021） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Items/ReorderDictionaryItemsCommand.cs` | OrderedCodes 参数说明（C3022） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Configuration/Kmic.Configuration.Application/IConfigurationQueryDb.cs` | SettingValues 属性 summary（C3026） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Configuration/Kmic.Configuration.Contracts/IDictionaries.cs` | 接口 remarks 的失败行为（C3019） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Configuration/Kmic.Configuration.Domain/Preferences/PreferenceLayer.cs` | 类 remarks（C3024） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Configuration/Kmic.Configuration.Domain/Preferences/PreferenceValue.cs` | 类 remarks 的调用前提（C3025） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Configuration/Kmic.Configuration.Presentation/Dictionaries/ReorderDictionaryItemsEndpoint.cs` | WithDescription 的完整列表要求（C3023） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Configuration/Kmic.Configuration.Presentation/Preferences/GetMyPreferencesEndpoint.cs` | WithDescription 的偏好族返回条件（C3027） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Application/Authentication/AuthTokens.cs` | 类 summary（C3029）；类 remarks 的网页响应格式（C3030） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Application/Authentication/GetMe/MeResponse.cs` | Roles 参数说明（C3033） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Application/Authentication/MySessions/ListMySessionsQuery.cs` | 类用途与查询范围（C3036） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Application/Authentication/SessionLifetimeOptions.cs` | RefreshGrace 属性 summary（C3038） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Application/Authentication/SwitchRole/SwitchRoleCommandHandler.cs` | Execute 中重复条件的行内说明（C3035） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Application/ContractQueries/UserReferences.cs` | 类 remarks（C3040） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Application/IIdentityCommandReads.cs` | 接口 remarks 的锁类型与读取顺序（C3041） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Contracts/IUserReferences.cs` | 接口 remarks 的失败行为（C3039） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Domain/Errors/IdentityErrors.cs` | DisplayNameInvalid 的 summary（C3042）；DisplayNameInvalid 的提示（C3043）；RoleNameInvalid 的提示（C3044）；OrgUnitNameInvalid 的提示（C3045）；RefreshTokenReused 的 summary（C3046）；RoleNotSwitchable 的 summary（C3047） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Infrastructure/Operations/IdentityBootstrapper.cs` | BootstrapAsync 的密码校验 remarks（C3048）；ResetAdministratorPasswordAsync 的密码校验 remarks（C3049）；应急重置作废会话及清零限制的行内说明（C3050） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Presentation/Authentication/GetMeEndpoint.cs` | WithDescription 的角色列表含义（C3034） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Presentation/Authentication/ListMySessionsEndpoint.cs` | WithDescription 的范围（C3037） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Presentation/Authentication/LoginEndpoint.cs` | WithDescription 的网页令牌响应（C3031） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Presentation/Authentication/RefreshEndpoint.cs` | WithDescription 的重用判定前提（C3032） | 已结合实现核对；修改后已复读；编译测试未运行 |

### 本次核对结果与接手位置

- 本次完整阅读并结合实现核对 241 文件：Configuration 98、Identity 141、Notifications 相关调用方 2。新增实现核对记录 232，另 9 路径与此前重叠；实际修改 23 文件／31 处中文，均已复读，不把各轮范围直接相加。
- 清单内 Configuration 的 173 个 C#、Identity 的 367 个 C# 现均有实现核对记录。该记录不等同于逐成员行为验收，验证码等预留结构也不据此声称功能已实现，原有模型与行为待办保留。
- 当前 1267 文件清单：66 文件仅扫描；578 文件中文已通读；623 文件已结合实现核对，其中 72 文件有改后复读记录。模型待办已在对应文件阶段单列。
- 静态文本核对：31 次逐处替换能从本次起点回放；除 9 处已登记的提示／接口说明字符串外，非注释文本不变。103 组 XML 注释语法有效；PreferenceValue 的 5 组属性摘要、HasComment 与模型注解未改。该核对不调用编译器或项目检查脚本。
- 其余 2680 个既有文件字节未变，含原有 5 个未跟踪模板锁文件。当前 147 个已跟踪修改，暂存区为空；分支和 HEAD 未变，git diff --check 无输出。所有此前本地修改及旧交接历史保留。
- Codex 历史路径仍为 1018 条、Claude 0 条；本次 23 个修改路径此前均已登记，仍逐文件复查。新增 M-05 后共 5 个表／列说明位置、4 文件待办，CN-01～04 未修复；不为措辞直接修改模型或旧迁移。
- 编译、测试、整套项目检查脚本、OpenAPI 生成及数据库操作：未运行。C3051 收尾，下一编号 C3052，以修改记录实际最大编号为准。后续优先核对 Files、Notifications、Operations、Sample 及宿主、脚本和模板中仍只有中文通读记录的文件，历史路径仍不作排除项。

<!-- c3019-progress:end -->

> 以下为此前阶段记录；本次状态以上方 C3019 起的记录为准。

<!-- c2993-progress:start -->
## 本次继续复核（C2993 起，UTC+8）

- 更新时间：2026年11月15日 15:39:53；状态：本次复核已完成；待办保留。
- 起点：109 个已跟踪修改，5 个未跟踪模板锁文件，暂存区为空；分支 `codex-cn-copy-cleanup-2026-11-14`，HEAD `5319686960c5458c3c7a6aee5da2c5f66ff7a303`。既有工作全部保留。
- 本次 286 个文件有完整实现核对记录；实际修改 21 个源码文件、25 处文字，最新记录 C3018。历史阶段不代替本次核对。
- 进展：Configuration 69 文件、Identity 217 文件完成本次实现核对；21 修改文件／25 处中文已复读，静态差异与交接清单已核对，C3018 收尾。
- 不改业务逻辑、测试、web/、BuildingBlocks、迁移、快照或生成列说明的属性摘要。编译、测试、整套项目检查脚本、OpenAPI 生成及数据库操作未运行；未清理、重置、stash、暂存、提交或推送。

### 本次实际修改文件

| 文件 | 位置和记录 | 阶段 |
|---|---|---|
| `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Layers/PreferenceLayerValue.cs` | RoleName 参数说明（C2993） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Application/Organization/Create/CreateOrgUnitCommand.cs` | Name 参数说明（C2996） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Application/Organization/Reorder/ReorderOrgUnitsCommand.cs` | 类 summary（C3000）；OrderedChildren 参数说明（C3001） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Application/Organization/Update/UpdateOrgUnitCommand.cs` | Name 参数说明（C2997） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Application/Roles/Create/CreateRoleCommand.cs` | Name 参数说明（C3002） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Application/Roles/RoleActions.cs` | NotBuiltIn 前的行内说明（C3006） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Application/Roles/RoleTarget.cs` | 类 summary（C3005） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Application/Users/Create/CreateUserCommand.cs` | Account 参数说明（C3010） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Application/Users/Get/UserDetailResponse.cs` | ActiveSessionCount 参数说明（C3016） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Application/Users/Grants/GrantTarget.cs` | 类 summary（C3009） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Application/Users/Sessions/ListUserSessionsQuery.cs` | 类 summary（C3015） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Application/Users/UserNames.cs` | 类 summary（C3014） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Application/Users/UserTarget.cs` | 类 summary（C3007）；类 remarks（C3008） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Domain/Users/MobileNumbers.cs` | 类 remarks 中重复的用途理由（C3013） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Infrastructure/Access/AccessContextLoader.cs` | RoleSnapshotLoader 前的实现理由（C2995） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Infrastructure/Access/PermissionCatalogStore.cs` | RenameInPlace 前的行内说明（C2994） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Presentation/Organization/CreateOrgUnitRequest.cs` | Name 参数说明（C2998） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Presentation/Organization/UpdateOrgUnitRequest.cs` | Name 参数说明（C2999） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Presentation/Roles/CreateRoleRequest.cs` | Name 参数说明（C3003）；类 remarks（C3004） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Presentation/Users/CreateUserRequest.cs` | Account 参数说明（C3011）；类 remarks（C3012） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Presentation/Users/ListUserSessionsEndpoint.cs` | WithDescription 的状态和时间范围（C3017） | 已结合实现核对；修改后已复读；编译测试未运行 |

### 本次核对结果与接手位置

- 完整实现核对：Configuration 69 文件、Identity 217 文件，共 286 文件；实际修改 21 文件／25 处中文，均已复读。286 路径与先前阶段有 8 个重叠，新增 278 个实现核对记录；不把各轮文件数直接相加，不宣称所有成员行为已验收。
- 修改回放及非注释文本核对一致：只有 C3017 的 WithDescription 字符串改变，其余均为注释；24 组 XML 注释语法有效。唯一修改的领域文件是手机号静态辅助类，实体属性摘要、列说明及模型注解未改。
- 其余 2682 个既有文件字节未变，含原有 5 个未跟踪模板锁文件。当前 129 个已跟踪文件有修改，暂存区为空，分支和 HEAD 未变；git diff --check 无输出。
- 辅助文本核对首次把领域目录误写为 /Domain/Users/，导致白名单判断失败；确认实际路径为 Kmic.Identity.Domain 后仅修正辅助条件，再核对通过，源码未为此更改。该核对不调用编译器或运行项目检查脚本。
- Codex 历史路径清单 1018 条（新增 CreateRoleRequest.cs），Claude 清单 0 条；历史仅供了解范围。模型说明 4 位置／3 文件和 CN-01～04 仍待处理，Role.IsProtected 已核对实现但未改生成列说明的属性摘要。
- 编译、测试、整套项目检查脚本、OpenAPI 生成及数据库操作：未运行。交接 C3018，下一编号 C3019，以修改记录实际最大编号为准；接手先核对 Git 实际状态并保留所有已有工作。

- 当前 1267 文件清单：73 文件仅扫描；803 文件中文已通读（含 2 个映射说明待办文件）；391 文件已结合实现核对，其中 49 文件有改后复读记录、1 文件保留映射说明待办。各层按实际记录区分，不等同于全项目行为验收。
- 后续优先核对 Configuration 字典、Identity 验证码／模拟身份／清理，以及其余模块尚只记中文通读的文件；已处理路径仍参与复查。

<!-- c2993-progress:end -->

> 以下为此前阶段记录；本次状态以上方 C2993 起的记录为准。

<!-- c2978-progress:start -->
## 本次结合完整实现复核（C2978 起，UTC+8）

- 更新时间：2026年11月15日 15:10:38；状态：本次复核已完成；模型与行为待办保留。
- 本次起点：100 个已跟踪修改、5 个未跟踪模板锁文件、暂存区为空，分支及 HEAD 与 C2977 交接一致。全部既有工作保留。
- 本次已记录 95 个文件的完整实现核对；修改 10 个源码文件、13 处文字（C2978 起，最新记录 C2992）。上轮 1267 文件的中文通读不代替本次实现核对。
- 当前进展：95 个文件的完整实现核对依据已逐文件登记；10 个修改文件完成中文复读和静态差异核对，C2992 记录交接。
- 约束：只改中文说明和文案；不改业务逻辑、测试、web/、BuildingBlocks、迁移、快照或生成列注释的属性摘要。编译、测试、整套项目脚本和数据库操作未运行；未暂存提交推送。

### 本次行为疑点（未修改逻辑）

- CN-04（C2983，2026年11月15日 14:52:55）：静态链路：CurrentUser.From 将 UserId 设为被模拟用户、ActualUserId 设为发起模拟的用户；StartUploadCommandHandler 用 UserId 保存 UploadedBy，GeneratedFiles 用 UserId 保存 RequestedBy；FileAttachments.AttachAsync 却优先把 ActualUserId 传给 CheckAttachabilityQueryHandler。管理员 A 以可写模式模拟 B 时，新上传的可用文件登记在 B 名下，关联检查按 A 比较，因而不进入可关联结果；已解除关联的文件不受此上传人条件限制。Sample 的附件添加会返回 Files.NotAttachable。未运行复现、未改逻辑；后续须确认模拟身份场景的归属规则，再统一上传登记、生成请求人与关联校验。不能通过把“本人”换成泛称来视为已解决。

### 本次实际修改文件

| 文件 | 位置和记录 | 阶段 |
|---|---|---|
| `src/Modules/Files/Kmic.Files.Application/Cleanup/ReconcileStorageCommand.cs` | remarks 的等待期依据（C2978）；remarks 的空清单保护条件（C2979） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Files/Kmic.Files.Application/Cleanup/ResetStaleUploadsCommandHandler.cs` | 重读上传状态的行内说明（C2980） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Files/Kmic.Files.Application/Uploads/Reject/RejectUploadCommand.cs` | remarks 的允许状态（C2981）；NotIdempotent 理由（C2982） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Application/Authentication/Refresh/RefreshTokenCommand.cs` | remarks 的过期令牌与重用分支（C2988） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Identity/Kmic.Identity.Domain/Sessions/Session.cs` | Rotate 的令牌过期与宽限期说明（C2987） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Notifications/Kmic.Notifications.Application/Processing/NotificationRequest.cs` | remarks 的去重键区别（C2986） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Notifications/Kmic.Notifications.Contracts/SendNotification.cs` | remarks 的去重范围和记录清理边界（C2984）；DedupKey 参数说明（C2985） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Operations/Kmic.Operations.Application/MergedCursorPaging.cs` | DatabaseUuidOrder 的排序理由（C2989） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Sample/Kmic.Sample.Application/Specimens/EventHandlers/NotifyOwnerOfSubmission.cs` | remarks 中重复的业务键说明（C2990） | 已结合实现核对；修改后已复读；编译测试未运行 |
| `src/Modules/Sample/Kmic.Sample.Presentation/Specimens/DownloadSpecimenAttachmentEndpoint.cs` | WithDescription 的 Range 支持范围（C2991） | 已结合实现核对；修改后已复读；编译测试未运行 |

### 本次核对结果和接手位置

- 逐处文本核对：10 个文件、13 次替换均能从本次起点回放；除拒绝命令的特性理由和下载端点的接口说明外，非注释文本不变。35 组 XML 注释语法有效；Session 的 17 组属性说明和所有列说明映射未改。
- 其余 2693 个既有文件的 SHA-256 与本次起点一致，包含 5 个原有未跟踪模板锁文件。当前 109 个已跟踪文件有本地修改，暂存区为空；分支和 HEAD 未变，git diff --check 无输出。
- 本次 95 文件在全面复核清单中逐一记录实现依据；其余路径沿用上一阶段的实际阶段，不自动提升。该记录不代表所有成员的业务行为已经验收。
- 历史路径清单仍为 Codex 1017 条、Claude 0 条，只表示历史范围。模型说明仍有 4 位置／3 文件待办；CN-01～04 未修复。编译、测试、整套项目脚本、OpenAPI 生成及数据库操作均未运行。
- 交接收尾 C2992；下一编号 C2993，以修改记录实际最大编号为准。接手先核对实际 Git 状态，保留所有既有未提交工作。

<!-- c2978-progress:end -->

> 以下为此前阶段记录；本次状态以 C2978 起的记录为准。

<!-- c2948-progress:start -->
## 本次逐文件复核（C2948～C2977，UTC+8）

- 更新时间：2026年11月15日 14:45:43；本次通读、修改及交接记录已完成，模型列说明待办保留。
- 分支：`codex-cn-copy-cleanup-2026-11-14`；HEAD：`5319686960c5458c3c7a6aee5da2c5f66ff7a303`。
- 原 1267 文件范围逐文件复核：1174 个 C# 中，1122 个含中文、52 个未检出；93 个配套文件中，57 个含中文、36 个未检出。含中文内容已按文件通读，疑点沿实现、调用方及接口说明核对；其余准确文字保留。
- 本次修改 19 个源码文件，C2948～C2976 共 29 次逐处操作，含 4 次改后复读修正，净 25 个修改位置；C2977 记录核对和交接。19 个修改文件均完成整份中文复读。下表及逐文件阶段不宣称所有业务成员均已验收。
- 阶段口径：仅扫描未检出中文 88 文件；含中文 1179 文件中，19 文件的修改位置已结合实现核对并完成改后整文件复读，其他 1160 文件只记中文已通读。另有辅助实现阅读不据此扩大验收结论。
- 起点 83 个已跟踪修改、5 个未跟踪模板锁文件，暂存区为空；结束 100 个已跟踪修改、同样 5 个未跟踪，暂存区为空。额外 5 个锁文件仅扫描未检出中文，未修改，不计入原 1267 范围。
- 路径清单：本次 19 路径原已全部列入 Codex 清单，因此仍为 1017 条；Claude 清单仍为空。二者只表示处理历史，不作为本次复核排除项或质量证明。
- 限制：未改业务逻辑、测试、web/、BuildingBlocks、迁移和快照。编译、测试、整套项目检查脚本、OpenAPI 生成及数据库操作均未运行（用户要求）；未清理、重置、stash、暂存、提交或推送。
- 静态核对：19 个文件的 29 次逐处替换可从本次起点完整回放；去除独立注释行并反向还原 5 处已登记的说明字符串后，其余非注释文本与起点一致。64 组 XML 注释语法有效。HasComment 调用及生成列说明的实体属性摘要未改；2680 个其他既有跟踪文件的 SHA-256 与本次起点一致。git diff --check 无输出。此核对不调用编译器、不运行项目检查脚本，不代表编译、测试或业务验收。
- 核对辅助脚本曾漏处理文案的二次修订；查明是中间文本未存在于原始基线的判断错误，改为按记录逆序还原后核对一致，未为此修改项目源码。
- 接手从 C2978 起：先重核 Git 状态和实际差异；模型说明按本节单列待办处理，历史阶段不自动视为合格。

| 范围 | C# 中文已通读 | 其他文件中文已审读 | 仅扫描未检出中文 | 本次修改文件 |
|---|---:|---:|---:|---:|
| Configuration | 163 | 5 | 15 | 7 |
| Files | 119 | 5 | 14 | 2 |
| Identity | 358 | 6 | 14 | 1 |
| Notifications | 170 | 5 | 12 | 5 |
| Operations | 198 | 5 | 21 | 3 |
| Sample | 86 | 5 | 6 | 1 |
| 宿主、脚本及模板等 | 28 | 26 | 6 | 0 |
| 合计 | 1122 | 57 | 88 | 19 |

### 模型相关说明单独记录

- C2966：CapturedMessageConfiguration 的 HasNoDataScope 理由不再声称“只在非生产环境存在”。已对照捕获开关、查询权限和建模约定；捕获开关默认开启，生产环境要求显式关闭，权限理由不负责约束运行环境。
- C2972：DeliveryConfiguration 不再泛称联系方式已脱敏。NotificationProcessor 仍将外部收件人的完整地址写入 Address，管理查询返回的是 AddressMasked。
- 影响核对：两处仅更改 Kmic:NoDataScope 理由。ModelValidator 只检查声明是否存在，ModelValidationConvention 在模型定稿前移除理由注解；按当前代码静态判断，不改变数据范围过滤、列注释或最终持久化模型。未生成迁移、SQL 或运行模型验证，旧迁移和快照未改。
- 仍保留 4 个映射说明位置、3 个文件的待办：CapturedMessage.Address／Role、Delivery.Role、Role.IsProtected。属性摘要会经 EntityCommentGenerator 生成列说明，本次保持原样。其他历史数据库及 CN-01/02/03 等行为待办继续保留，不视为已解决。

<!-- c2948-progress:end -->

### 本次实际修改文件（C2948～C2976）

19 个源码文件、29 次逐处操作（含 4 次复读修正，净 25 个修改位置）。各处 UTC+8 时间、前后内容和依据见修改记录 C2948～C2976；下列文件均已结合实现核对修改位置，并复读改后整份中文。

| 文件 | 成员或位置 | 记录 |
|---|---|---|
| `src/Modules/Configuration/Kmic.Configuration.Application/Dictionaries/Items/AddDictionaryItemCommand.cs` | ParentCode 参数说明 | C2948 |
| `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Layers/PreferenceDefinitionItem.cs` | Module 参数说明；Module 复读消除空值歧义 | C2950、C2955 |
| `src/Modules/Configuration/Kmic.Configuration.Application/Preferences/Layers/ResetPreferenceLayerCommand.cs` | 重置后的读取行为 | C2951 |
| `src/Modules/Configuration/Kmic.Configuration.Presentation/Dictionaries/AddDictionaryItemRequest.cs` | ParentCode 参数说明 | C2949 |
| `src/Modules/Configuration/Kmic.Configuration.Presentation/Preferences/ResetMyPreferenceEndpoint.cs` | 重置个人偏好的接口说明；重置说明复读明确查询入口 | C2952、C2956 |
| `src/Modules/Configuration/Kmic.Configuration.Presentation/Preferences/ResetRolePreferenceEndpoint.cs` | 移除角色级设置的接口说明；重置说明复读明确查询入口 | C2953、C2957 |
| `src/Modules/Configuration/Kmic.Configuration.Presentation/Preferences/ResetSystemPreferenceEndpoint.cs` | 移除系统级设置的接口说明；重置说明复读明确查询入口 | C2954、C2958 |
| `src/Modules/Files/Kmic.Files.Application/FilesOptions.cs` | TempExpiry 换算属性说明；DetachedRetention 换算属性说明；RejectedRetention 换算属性说明；StaleUpload 换算属性说明；ReconcileMinAge 换算属性说明 | C2959、C2960、C2961、C2962、C2963 |
| `src/Modules/Files/Kmic.Files.Infrastructure/Deletion/FilesDeletionOptions.cs` | Lease 换算属性说明；Poll 换算属性说明 | C2964、C2965 |
| `src/Modules/Identity/Kmic.Identity.Domain/Organization/OrgTree.cs` | 类 remarks 的版本与规模说明 | C2967 |
| `src/Modules/Notifications/Kmic.Notifications.Application/Channels/NotificationsDeliveryOptions.cs` | Lease 换算属性说明；Poll 换算属性说明 | C2968、C2969 |
| `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Publish/PublishTemplateCommandHandler.cs` | Handle 中重复的严格渲染说明 | C2970 |
| `src/Modules/Notifications/Kmic.Notifications.Application/Templates/Sync/SyncTemplateCatalogCommandHandler.cs` | Handle 取锁前的重复说明 | C2971 |
| `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Configurations/CapturedMessageConfiguration.cs` | HasNoDataScope 理由（模型相关，单独核实） | C2966 |
| `src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Configurations/DeliveryConfiguration.cs` | HasNoDataScope 理由（模型相关，单独核实） | C2972 |
| `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/AuditExportOptions.cs` | ExportRetention 换算属性说明 | C2973 |
| `src/Modules/Operations/Kmic.Operations.Application/Audit/Export/ExportAuditJobHandler.cs` | SaveAsync 中的重复事务说明 | C2974 |
| `src/Modules/Operations/Kmic.Operations.Domain/Jobs/ScheduledJobSetting.cs` | 类 remarks 的调度器读写职责 | C2975 |
| `src/Modules/Sample/Kmic.Sample.Application/Specimens/Attachments/Remove/RemoveSpecimenAttachmentCommandHandler.cs` | Handle 调用 DetachAsync 前的重复约定 | C2976 |


> 以下为 C2854～C2947 及更早阶段的历史记录；本次状态以上方记录为准。历史中的跳过路径、运行检查或修改模型的授权不适用于本次。


更新时间：2026年11月15日 14:20:52（UTC+8）。当前轮 C2854～C2946，交接 C2947；下一编号 C2948。未暂存、提交或推送。

## 本轮中文表达复查（2026年11月15日 13:40:28，UTC+8）

- 本轮进度：2026年11月15日 14:20:52（UTC+8）：完成本轮范围内的中文通读及疑点修订，59 个源码文件、93 处文字调整，C2854～C2946；C2947 登记复查阶段及交接，下一编号 C2948。模型说明待办另列，不将其计为已改善。编译测试未运行，未暂存提交推送。
- 本轮方法：按 Configuration → Files → Identity → Notifications → Operations → Sample 及宿主／脚本／模板逐文件通读中文，长短说明均纳入；疑点再沿实现、调用方和接口说明核对。修改后复读整份文件的中文。没有把搜索命中、历史路径清单或改动数量当作质量证明，也没有逐个业务成员完成行为验收。
- 本轮取舍：简短有用的类／接口用途说明和已有 inheritdoc 保留；不按 XML 行数判断冗长。删除无额外信息的步骤复述及重复解释；参数说明按调用价值取舍，公开契约与接口文档需要的说明保留。权限、事务、锁、失败、边界与资源释放约定仍写清楚。
- 本轮限制：不改业务逻辑、测试、web/、BuildingBlocks、迁移和快照；不改会生成列说明的属性摘要或模型注解。不编译、不运行测试、项目检查脚本或数据库操作，不暂存、提交或推送。
- 核对结果：59 个文件的逐处替换回放一致；只读 Roslyn 解析无语法错误，反向还原已记录的中文字符串后，全部非注释标记与本轮开始一致；304 组 XML 语法有效。2639 个本轮范围外既有文件 SHA 不变，包含原有规范改动及排除范围；领域属性 XML、HasComment 未改。当前 83 个已跟踪修改、无未跟踪、暂存区为空，HEAD 保持 5319686；git diff --check 通过。静态核对不代表运行验收。

范围维持 1267 个文件：1174 个 C# 中有 1122 个含中文、52 个未检出中文；93 个其他源码／构建配置中有 57 个含中文、36 个未检出中文。1122 个 C# 的中文已通读，57 个其他文件按中文内容审读（重复配置说明只核对差异）。59 个实际修改文件有逐处实现核对和修改后复读记录；其余不据此宣称所有业务约定均已逐成员验证。

阶段按证据登记：①仅扫描＝提取或搜索；②中文已通读＝读过全部中文，判断必要性与表达，未承诺逐成员行为核验；③已结合实现核对＝追到实现／调用方，记录依据；④修改后已复读＝在③基础上，复读改后整份文件的中文。下表对未修改文件只记②；59 个修改文件记③④；没有中文的 88 个文件单列；4 个模型说明相关文件保留待办。

| 模块 | 含中文 C#，已通读 | 含中文其他文件，已审读 | 未检出中文 | 本轮修改文件 |
|---|---:|---:|---:|---:|
| 宿主、脚本与模板等 | 28 | 26 | 6 | 3 |
| Configuration | 163 | 5 | 15 | 8 |
| Files | 119 | 5 | 14 | 6 |
| Identity | 358 | 6 | 14 | 13 |
| Notifications | 170 | 5 | 12 | 17 |
| Operations | 198 | 5 | 21 | 9 |
| Sample | 86 | 5 | 6 | 3 |
| 合计 | 1122 | 57 | 88 | 59 |

本轮代表性修改：

| 位置 | 原表达的问题 | 当前表达／处理 |
|---|---|---|
| OperationsErrors.RouteHasUnfinished | 把“选择放弃”说成终结订阅的唯一方式 | 明确可先处理完成，也可选择放弃后终结 |
| ConfiguredFileStorage | 声称创建服务时会暴露配置校验错误，容易误解为已检查存储 | 明确构造时只选择并创建实现，不主动检查连通性或读写权限 |
| NotificationsErrors.PreviewModelNotAllowed | “只能用示例数据”没有说清请求限制 | 明确不接受自定义数据，提示移除 model 参数 |
| CookieEndpointGuard | 把未在当前实现核实的 CORS 配置写成既定前提 | 只说明实际校验的可信 Origin、自定义请求头及 Cookie 设置 |
| NotificationRequest／EmailAttachment | 每个显而易见参数都列说明，关键约定被淹没 | 保留去重键区别及内容流释放责任，省去低信息参数标签 |

本轮实际修改的 59 个文件如下，93 处均有 UTC+8 时间、前后内容及实现核对依据：

| 文件 | 成员或位置 | 编号 |
|---|---|---|
| [new-module.cs](../../scripts/new-module.cs) | Validate 用途 | C2884 |
| [RedactionExtensions.cs](../../src/Aspire/Kmic.ServiceDefaults/RedactionExtensions.cs) | 类用途；调用说明 | C2882、C2883 |
| [ServiceDefaultsExtensions.cs](../../src/Aspire/Kmic.ServiceDefaults/ServiceDefaultsExtensions.cs) | 类用途 | C2881 |
| [DictionaryLookup.cs](../../src/Modules/Configuration/Kmic.Configuration.Application/ContractQueries/DictionaryLookup.cs) | 失败约定 | C2878 |
| [GetDictionarySnapshotQuery.cs](../../src/Modules/Configuration/Kmic.Configuration.Application/ContractQueries/GetDictionarySnapshotQuery.cs) | 查询用途；ResultExemption 理由 | C2876、C2877 |
| [DeclaredValues.cs](../../src/Modules/Configuration/Kmic.Configuration.Application/DeclaredValues.cs) | invalidWith 回调用途 | C2932 |
| [FeaturesResponse.cs](../../src/Modules/Configuration/Kmic.Configuration.Application/Features/FeaturesResponse.cs) | 响应用途 | C2933 |
| [ObsoleteSettingItem.cs](../../src/Modules/Configuration/Kmic.Configuration.Application/Settings/Get/ObsoleteSettingItem.cs) | Value 字段说明 | C2935 |
| [IDictionaries.cs](../../src/Modules/Configuration/Kmic.Configuration.Contracts/IDictionaries.cs) | 授权与缓存约定；GetAsync 空结果约定 | C2879、C2880 |
| [ConfigurationErrors.cs](../../src/Modules/Configuration/Kmic.Configuration.Domain/Errors/ConfigurationErrors.cs) | PreferenceRoleNotFound 说明；PreferenceRoleNotFound：复读后缩短重复限定 | C2936、C2942 |
| [PreferenceLayerReader.cs](../../src/Modules/Configuration/Kmic.Configuration.Infrastructure/Preferences/PreferenceLayerReader.cs) | LayerSnapshot 用途 | C2934 |
| [FileAttachments.cs](../../src/Modules/Files/Kmic.Files.Application/Attachments/FileAttachments.cs) | LockScope 说明 | C2885 |
| [PurgeExpiredFilesCommandHandler.cs](../../src/Modules/Files/Kmic.Files.Application/Cleanup/PurgeExpiredFilesCommandHandler.cs) | 清理候选前的说明 | C2886 |
| [FilesOptions.cs](../../src/Modules/Files/Kmic.Files.Application/FilesOptions.cs) | StaleUploadMinutes 时间边界；ReconcileMinAgeDays 配置含义；ReconcileMinAge 换算属性说明 | C2887、C2888、C2889 |
| [IStorageKeyFactory.cs](../../src/Modules/Files/Kmic.Files.Application/IStorageKeyFactory.cs) | 键生成原因 | C2890 |
| [ConfiguredFileStorage.cs](../../src/Modules/Files/Kmic.Files.Infrastructure/Storage/ConfiguredFileStorage.cs) | 构造与配置错误说明 | C2891 |
| [UploadFileEndpoint.cs](../../src/Modules/Files/Kmic.Files.Presentation/Uploads/UploadFileEndpoint.cs) | NameOf 实现原因 | C2892 |
| [ILoginThrottleStore.cs](../../src/Modules/Identity/Kmic.Identity.Application/Authentication/ILoginThrottleStore.cs) | RecordFailureAsync 原子更新约定 | C2893 |
| [TemporaryPasswordGenerator.cs](../../src/Modules/Identity/Kmic.Identity.Application/Authentication/TemporaryPasswordGenerator.cs) | Generate 说明 | C2894 |
| [IPermissionCatalogStore.cs](../../src/Modules/Identity/Kmic.Identity.Application/Catalog/IPermissionCatalogStore.cs) | 目录存储用途；目录与聚合的区别 | C2898、C2899 |
| [UserReferences.cs](../../src/Modules/Identity/Kmic.Identity.Application/ContractQueries/UserReferences.cs) | 批量查询失败约定 | C2895 |
| [IIdentityQueryDb.cs](../../src/Modules/Identity/Kmic.Identity.Application/IIdentityQueryDb.cs) | SecurityEvents 只读职责 | C2896 |
| [IIdentityReads.cs](../../src/Modules/Identity/Kmic.Identity.Application/IIdentityReads.cs) | 读写侧约定 | C2897 |
| [IdentityPermissions.cs](../../src/Modules/Identity/Kmic.Identity.Application/IdentityPermissions.cs) | RoleManage 权限用途；UserGrant 权限用途；RoleManage：复读后展开生硬缩写 | C2937、C2938、C2943 |
| [StartImpersonationCommandHandler.cs](../../src/Modules/Identity/Kmic.Identity.Application/Impersonation/Start/StartImpersonationCommandHandler.cs) | ChooseRoleAsync 选择规则 | C2900 |
| [ListSecurityEventsQuery.cs](../../src/Modules/Identity/Kmic.Identity.Application/SecurityEvents/ListSecurityEventsQuery.cs) | 查询边界与审计说明 | C2901 |
| [IdentityErrors.cs](../../src/Modules/Identity/Kmic.Identity.Domain/Errors/IdentityErrors.cs) | ImpersonationDenied 提示；SystemUserImmutable 说明；GrantExists 说明；CannotChangeOwnGrants 说明 | C2904、C2939、C2940、C2941 |
| [IdentityModule.cs](../../src/Modules/Identity/Kmic.Identity.Infrastructure/IdentityModule.cs) | 访问上下文加载器生命周期 | C2902 |
| [IdentityBootstrapper.cs](../../src/Modules/Identity/Kmic.Identity.Infrastructure/Operations/IdentityBootstrapper.cs) | WriteEventAsync 前的重复用途说明 | C2946 |
| [CookieEndpointGuard.cs](../../src/Modules/Identity/Kmic.Identity.Presentation/Authentication/CookieEndpointGuard.cs) | CSRF 检查说明 | C2903 |
| [GetCapturedQuery.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Captured/Get/GetCapturedQuery.cs) | 未启用捕获的返回约定 | C2905 |
| [ListCapturedQuery.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Captured/List/ListCapturedQuery.cs) | 未启用捕获的返回约定 | C2906 |
| [EmailAttachment.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Channels/EmailAttachment.cs) | 附件类型：保留流所有权，省去常规参数标签 | C2930 |
| [RetryDeliveryCommandHandler.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Deliveries/Retry/RetryDeliveryCommandHandler.cs) | 含密投递重试约束 | C2911 |
| [NotificationsPermissions.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/NotificationsPermissions.cs) | DeliveryView 用途和审计要求 | C2910 |
| [NotificationsRenderOptions.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/NotificationsRenderOptions.cs) | MaxOutputKb 单位说明 | C2907 |
| [NotificationsRetentionOptions.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/NotificationsRetentionOptions.cs) | 保留期配置说明 | C2908 |
| [AddressMasking.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Processing/AddressMasking.cs) | Email 脱敏说明 | C2909 |
| [NotificationRequest.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Processing/NotificationRequest.cs) | 内部请求：删去逐项复述，保留两种去重键的区别 | C2929 |
| [RenderFormat.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Rendering/RenderFormat.cs) | HTML 转义用途 | C2912 |
| [ChannelRendering.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Templates/ChannelRendering.cs) | RenderAsync 前的重复说明 | C2944 |
| [DeliveryColumns.cs](../../src/Modules/Notifications/Kmic.Notifications.Domain/Deliveries/DeliveryColumns.cs) | 列名集合用途；列更新与查询方式 | C2917、C2918 |
| [NotificationsErrors.cs](../../src/Modules/Notifications/Kmic.Notifications.Domain/Errors/NotificationsErrors.cs) | SiteMessageLinkInvalid 说明；SiteMessageLinkInvalid 提示；PreviewModelNotAllowed 说明；PreviewModelNotAllowed 提示 | C2913、C2914、C2915、C2916 |
| [MailKitEmailSender.cs](../../src/Modules/Notifications/Kmic.Notifications.Infrastructure/Channels/MailKitEmailSender.cs) | 地址或附件内容类型解析错误；SMTP 永久失败提示 | C2923、C2924 |
| [UnconfiguredSmsGateway.cs](../../src/Modules/Notifications/Kmic.Notifications.Infrastructure/Channels/UnconfiguredSmsGateway.cs) | 占位实现说明 | C2925 |
| [CappedWriter.cs](../../src/Modules/Notifications/Kmic.Notifications.Infrastructure/Rendering/CappedWriter.cs) | 写入器用途；限制检查的边界；输出超限错误；渲染超时错误 | C2919、C2920、C2921、C2922 |
| [DeliveryStore.cs](../../src/Modules/Notifications/Kmic.Notifications.Infrastructure/Sending/DeliveryStore.cs) | SkippedAsync 前的流程复述 | C2945 |
| [ClientErrorOptions.cs](../../src/Modules/Operations/Kmic.Operations.Application/ClientErrors/ClientErrorOptions.cs) | PerMinute 配置说明 | C2871 |
| [BlockedKeyItem.cs](../../src/Modules/Operations/Kmic.Operations.Application/Messages/Blocked/BlockedKeyItem.cs) | LastError 字段说明 | C2868 |
| [MessageRouteRow.cs](../../src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/MessageRouteRow.cs) | 只读状态说明 | C2869 |
| [RedeliverResponse.cs](../../src/Modules/Operations/Kmic.Operations.Application/Messages/Routes/RedeliverResponse.cs) | Candidates 字段说明；Written 字段说明 | C2866、C2867 |
| [ScheduledJobRunRow.cs](../../src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/ScheduledJobRunRow.cs) | 只读状态说明 | C2870 |
| [OperationsErrors.cs](../../src/Modules/Operations/Kmic.Operations.Domain/Errors/OperationsErrors.cs) | CronInvalidWith 用途；BackgroundJobNotFound 适用场景；MessageNotFound 提示；RouteNotRetiring 说明；RouteInFlight 说明；RouteInFlight 提示；RouteHasUnfinished 说明；RouteHasUnfinished 提示；RedeliverTooMany 说明；RedeliverTooMany 提示；RebuildAlreadyRunning 提示；ClientErrorsThrottled 调用约定 | C2854、C2855、C2856、C2857、C2858、C2859、C2860、C2861、C2862、C2863、C2864、C2865 |
| [FinalizeRouteEndpoint.cs](../../src/Modules/Operations/Kmic.Operations.Presentation/Messages/FinalizeRouteEndpoint.cs) | 接口说明：终结前的状态要求；接口说明：占用条件；接口说明：未完成消息的处理选择 | C2872、C2873、C2874 |
| [ListBlockedKeysEndpoint.cs](../../src/Modules/Operations/Kmic.Operations.Presentation/Messages/ListBlockedKeysEndpoint.cs) | 接口说明：重试或放弃的选择 | C2875 |
| [TriggerRebuildEndpoint.cs](../../src/Modules/Operations/Kmic.Operations.Presentation/Messages/TriggerRebuildEndpoint.cs) | 与重建错误提示统一未结束条件 | C2931 |
| [CreateSpecimenItemInput.cs](../../src/Modules/Sample/Kmic.Sample.Application/Specimens/Create/CreateSpecimenItemInput.cs) | 明细输入：删除重复的参数标签 | C2926 |
| [ISpecimenReferences.cs](../../src/Modules/Sample/Kmic.Sample.Contracts/ISpecimenReferences.cs) | 接口用途 | C2927 |
| [SpecimenAttachment.cs](../../src/Modules/Sample/Kmic.Sample.Domain/Specimens/SpecimenAttachment.cs) | 类说明：保留快照用途，合并重复职责 | C2928 |

模型相关说明单独留待处理（5 个位置、4 个文件，前 3 条为既有待办；不计为本轮已优化）：

| 文件 | 待办、依据及建议 |
|---|---|
| [CapturedMessage.cs](../../src/Modules/Notifications/Kmic.Notifications.Domain/Captured/CapturedMessage.cs) | Address／Role 两条既有列说明仍待优化；原文分别为“本该发往的完整地址”“邮件里的角色”，未当作已经精修。 |
| [Delivery.cs](../../src/Modules/Notifications/Kmic.Notifications.Domain/Deliveries/Delivery.cs) | Role 的既有列说明“邮件里的角色”仍待优化，未改映射属性摘要。 |
| [Role.cs](../../src/Modules/Identity/Kmic.Identity.Domain/Roles/Role.cs) | IsProtected 仍写“只有持有受保护角色的人才能管理它……”；RolePolicy／UserPolicy 实际检查操作者当前角色，建议属性只留短名称，具体保护规则集中于策略。涉及列说明来源，本轮未改。 |
| [CapturedMessageConfiguration.cs](../../src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Configurations/CapturedMessageConfiguration.cs) | HasNoDataScope 理由写“只在非生产环境存在”；NotificationsCaptureOptions 是默认开启并要求生产显式关闭，并非此处按环境阻止保存。建议后续核实模型注解影响后改为权限范围理由，本轮未改注解。 |

已有 CN-01/02/03、Sample 设置未接入业务、BuildingBlocks 旧术语及 API IDE0005 等事项仍按下方历史记录理解；本轮没有改变其业务行为。未编译、未测试，不能以静态核对代替运行验收。

接手时从 C2948 起，先核对 Git 来源。当前清单用阶段而非“已处理”表示证据；只扫描过不能算通读，通读过不能算所有约定核实。准确清楚的文字保持稳定；发现同类疑点时，沿相关契约、错误目录、调用方和接口一起检查，不要求用户逐句找出。Codex 路径清单现 1017 条，新增 5 条；Claude 清单保持空，两者均不是合格证明。

## 本轮按信息价值精简（2026年11月15日 13:03:46，UTC+8）

- 本轮进度：2026年11月15日 13:27:27（UTC+8）：本轮复查完成。先恢复 42 条类用途说明及 43 处 inheritdoc，涉及 43 文件，均与 5319686 一致；随后在 20 个源码文件精简 26 处注释，规范 1 文件调整 4 处（含修订记录）。本轮范围 C2653～C2853，下一编号 C2854。编译测试未运行；未暂存提交推送。
- 本轮取舍：简短的类／接口中文用途说明和已有 inheritdoc 保留；三行 XML 标签不是冗长。重点精简逐步复述、成段重复及低信息局部说明；调用约定、权限、锁、事务、时间边界、失败与资源释放规则仍须保留。
- 本轮来源：外部已将此前成果提交为 5319686；本轮开始修改前重核分支、HEAD、索引及源码 SHA，保留所有既有工作。
- 本轮恢复：C2738～C2779 恢复类用途说明，C2780～C2822 恢复 inheritdoc；43 文件无净源码修改。原删除 C2653～C2737 仅为历史记录，不再代表当前结果。
- 本轮限制：不改业务逻辑、测试、web/、BuildingBlocks、迁移和快照，不改会生成列说明的属性摘要；未编译、未运行测试、项目检查脚本或数据库操作，未暂存、提交或推送。
- 核对结果：逐处替换回放一致；20 个源码文件的非注释行、全部属性 XML 及 HasComment 原样，227 组 XML 语法有效；43 个恢复文件与基线一致。2705 个既有文件 SHA 已比对，除本轮源码、规范及跟踪文档外均未变化。暂存区为空，无未跟踪；仅静态核对，不代表运行验收。

本轮从范围清单提取全部 1174 个 C# 文件的注释，重点复读较长说明、参数密集块和行内注释；筛查到 303 组较长 XML 候选、207 组中文行内说明。候选数量用于定位，不作为质量指标；接口字段、配置含义、复杂查询和并发失败说明按用途保留。没有将自动提取等同于逐个业务用例的运行验证。

本轮实际精简的 20 个源码文件如下（26 处逐项记录）：

| 文件 | 位置与内容 | 编号 |
|---|---|---|
| [ConfigurationPermissions.cs](../../src/Modules/Configuration/Kmic.Configuration.Application/ConfigurationPermissions.cs) | 类说明：删去通用命名要求 | C2823 |
| [FilesPermissions.cs](../../src/Modules/Files/Kmic.Files.Application/FilesPermissions.cs) | 类说明：删去重复的开发流程要求 | C2824 |
| [StoredFileDeletionWork.cs](../../src/Modules/Files/Kmic.Files.Infrastructure/Deletion/StoredFileDeletionWork.cs) | 类说明：删去框架循环的背景复述 | C2839 |
| [SyncPermissionCatalogCommandHandler.cs](../../src/Modules/Identity/Kmic.Identity.Application/Catalog/SyncPermissionCatalogCommandHandler.cs) | RenameOnRolesAsync：压缩执行过程复述 | C2847 |
| [IdentityPermissions.cs](../../src/Modules/Identity/Kmic.Identity.Application/IdentityPermissions.cs) | 类说明：合并通用要求并保留启动约束 | C2825 |
| [AccessContextLoader.cs](../../src/Modules/Identity/Kmic.Identity.Infrastructure/Access/AccessContextLoader.cs) | 角色快照加载：压缩权限过滤说明 | C2846 |
| [IdentityBootstrapper.cs](../../src/Modules/Identity/Kmic.Identity.Infrastructure/Operations/IdentityBootstrapper.cs) | BootstrapAsync：收拢必要约定，删除逐参数复述；ResetAdministratorPasswordAsync：删除整组低信息参数说明 | C2842、C2843 |
| [NotificationProcessor.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Processing/NotificationProcessor.cs) | ProcessAsync：删除重复的步骤说明；Resolve：删除逐分支预述；AttachmentsOf：保留延后读取原因，省去显而易见的细节；SmsParametersOf：压缩重复表述 | C2833、C2834、C2835、C2836 |
| [SyncTemplateCatalogCommandHandler.cs](../../src/Modules/Notifications/Kmic.Notifications.Application/Templates/Sync/SyncTemplateCatalogCommandHandler.cs) | 退役模板循环前的步骤复述 | C2831 |
| [NotificationsErrors.cs](../../src/Modules/Notifications/Kmic.Notifications.Domain/Errors/NotificationsErrors.cs) | 类说明：移除通用错误码命名指导 | C2827 |
| [SiteMessage.cs](../../src/Modules/Notifications/Kmic.Notifications.Domain/SiteMessages/SiteMessage.cs) | Create：省去常规参数标签，保留链接与时间含义 | C2844 |
| [NotificationTemplate.cs](../../src/Modules/Notifications/Kmic.Notifications.Domain/Templates/NotificationTemplate.cs) | 类说明：合并反复描述的默认与发布内容选择；类说明：复读后明确未发布时的默认内容 | C2841、C2848 |
| [NotificationTemplateConfiguration.cs](../../src/Modules/Notifications/Kmic.Notifications.Infrastructure/Persistence/Configurations/NotificationTemplateConfiguration.cs) | 唯一索引前的重复说明 | C2832 |
| [DeliveryWork.cs](../../src/Modules/Notifications/Kmic.Notifications.Infrastructure/Sending/DeliveryWork.cs) | 类说明：删去框架循环的背景复述；SendEmailAsync：压缩资源与失败处理说明 | C2837、C2838 |
| [RecordAuditCommand.cs](../../src/Modules/Operations/Kmic.Operations.Application/Audit/Record/RecordAuditCommand.cs) | 调用说明：只保留具体重复请求行为 | C2840 |
| [IOperationsQueryDb.cs](../../src/Modules/Operations/Kmic.Operations.Application/IOperationsQueryDb.cs) | WithTarget：压缩存储与分层原因 | C2845 |
| [OperationsPermissions.cs](../../src/Modules/Operations/Kmic.Operations.Application/OperationsPermissions.cs) | 类说明：直接说明启动失败条件 | C2826 |
| [ListScheduledJobsQueryHandler.cs](../../src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/List/ListScheduledJobsQueryHandler.cs) | DefaultEnabled 上方行内说明 | C2830 |
| [ScheduledJobPolicy.cs](../../src/Modules/Operations/Kmic.Operations.Application/ScheduledJobs/ScheduledJobPolicy.cs) | 类摘要：去掉与 remarks 重复的限制说明 | C2829 |
| [OperationsErrors.cs](../../src/Modules/Operations/Kmic.Operations.Domain/Errors/OperationsErrors.cs) | 类说明：移除通用错误码命名指导 | C2828 |

规范同步为 `docs/framework/06-coding-standards.md` 的 10.3、A.2、A.3 及修订记录（C2849～C2852），明确已有短用途说明和 inheritdoc 的保留尺度，未改脚本或工程检查。双方路径清单仅作历史参考：Claude 清单保持空，Codex 既有 1012 路径已覆盖本轮文件，无新增路径。

接手时从 C2854 起，先核对当前 Git 与实际差异。现有三条映射列说明和 CN-01/02/03 等行为事项仍按下方原记录处理，本轮未修改或宣称验收。

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
