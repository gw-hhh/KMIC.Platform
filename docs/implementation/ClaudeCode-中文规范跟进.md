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
