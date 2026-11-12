# KMIC.Platform

科隆销售平台新系统：.NET 10 后端 + Vue 3 前端，PostgreSQL，Linux + Docker 部署。旧系统（`kelong.web`，WebForms + SQL Server）只作参考，不要求兼容。

- 当前阶段：阶段 2（框架骨架 + 参考模块）已完成，阶段 3（平台模块）设计中，Identity、Operations 两份设计待评审。阶段与待办见 [`docs/implementation/00-README.md`](docs/implementation/00-README.md)；在另一台机器上继续开发见 [`docs/implementation/换机器继续开发.md`](docs/implementation/换机器继续开发.md)。
- 本文件夹是独立项目，目前临时放在旧仓库 `kelong.web` 中只为方便读取旧代码，之后单独成为仓库；新系统的所有设计、开发文档和资料都放在这里，且不依赖旧仓库的任何文件。
- 项目规则与工作约定：[`CLAUDE.md`](CLAUDE.md)。

## 目录

| 路径 | 内容 | 状态 |
|---|---|---|
| `CLAUDE.md` | 项目规则、命名、工作约定、参考资料位置 | 生效 |
| `docs/framework/` | 底层框架设计：需求、选型、架构、各项基础能力、ADR。**从 [`docs/framework/00-README.md`](docs/framework/00-README.md) 开始阅读**，进度表与延后事项表都在那里 | 设计已确认 |
| `docs/frontend/` | 前端框架设计：旧前端盘点与需求、技术选型、架构、规范。**从 [`docs/frontend/00-README.md`](docs/frontend/00-README.md) 开始阅读** | 设计已确认 |
| `docs/legacy/` | 旧系统完整分析（含业务模块细节），供设计参考 | 参考资料 |
| `docs/implementation/` | 实施各阶段的任务清单、进度、操作说明与按触发条件的待办。**从 [`docs/implementation/00-README.md`](docs/implementation/00-README.md) 开始阅读** | 阶段 3 设计中 |

以下目录已经建立（`docs/modules/` 目前有 Identity、Operations 两份待审的设计文档）或在规划中：

| 路径 | 内容 | 状态 |
|---|---|---|
| `docs/modules/` | 各模块的设计文档与模块登记表 | 已建立 |
| `src/` | 后端源码：`Kmic.BuildingBlocks.*`、各模块 5 个项目（ADR-0004）、`Kmic.Api`、`Kmic.MigrationRunner`、`Kmic.Operator`、Aspire 编排 | 已建立 |
| `tests/` | 测试项目与架构测试 | 已建立 |
| `scripts/`、`templates/` | 本地与 CI 共用的检查脚本；`dotnet new` 模块模板与新建模块脚本 | 已建立 |
| `docs/ops/` | 部署、运维手册 | 规划，阶段 5 |
| `web/` | Vue 3 前端（最新技术栈）；前端工具链原型在分支 `spike/fe-01-toolchain` | 规划，尚未建立 |

## 命名

- 仓库 / 文件夹名：`KMIC.Platform`。
- 代码根命名空间与项目名为 `Kmic`（如 `Kmic.BuildingBlocks.Application`、`Kmic.Identity.Domain`），不加 `Platform` 段；三个字母以上的缩写只首字母大写（06 第 4.2 节）。
- 2026-11-04 由 `Kelong.Platform` / `Kelong.*` 更名；旧系统名称（`Kelong_Log` 等）保持原样。
