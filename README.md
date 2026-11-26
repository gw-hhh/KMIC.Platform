# KMIC.Platform

科隆销售平台新系统：.NET 10 后端 + Vue 3 前端，PostgreSQL，Linux + Docker 部署。旧系统（`kelong.web`，WebForms + SQL Server）只作参考，不要求兼容。

- 进度、待决事项与待办：[`docs/implementation/00-README.md`](docs/implementation/00-README.md)（唯一来源）；在另一台机器上继续开发见 [`docs/implementation/换机器继续开发.md`](docs/implementation/换机器继续开发.md)。
- 本文件夹是独立项目，目前临时放在旧仓库 `kelong.web` 中只为方便读取旧代码，之后单独成为仓库；新系统的所有设计、开发文档和资料都放在这里，且不依赖旧仓库的任何文件。
- 项目规则与工作约定：[`CLAUDE.md`](CLAUDE.md)。
- 本机运行：`dotnet run scripts/dev.cs -- up`，说明见 [`docs/implementation/本地运行.md`](docs/implementation/本地运行.md)。

## 目录

| 路径 | 内容 | 状态 |
|---|---|---|
| `CLAUDE.md` | 项目规则、命名、工作约定、参考资料位置 | 生效 |
| `docs/framework/` | 底层框架设计：需求、选型、架构、各项基础能力、ADR。**从 [`docs/framework/00-README.md`](docs/framework/00-README.md) 开始阅读** | 设计已确认 |
| `docs/frontend/` | 前端框架设计：旧前端盘点与需求、技术选型、架构、规范。**从 [`docs/frontend/00-README.md`](docs/frontend/00-README.md) 开始阅读** | 设计已确认 |
| `docs/legacy/` | 旧系统完整分析（业务模块、底层能力、隐性约定），设计与数据迁移参考 | 参考资料 |
| `docs/implementation/` | 进度与待办（唯一来源）、本地运行、换机器、Gitea 准备等操作说明 | 生效 |

其余目录：

| 路径 | 内容 | 状态 |
|---|---|---|
| `docs/modules/` | 各模块的设计文档与模块登记表（五个平台模块已确认） | 已建立 |
| `src/` | 后端源码：`Kmic.BuildingBlocks.*`、各模块 5 个项目（ADR-0004）、`Kmic.Api`、`Kmic.MigrationRunner`、`Kmic.Operator`、`Kmic.ServiceDefaults`（日志、追踪、指标的统一配置） | 已建立 |
| `tests/` | 测试项目与架构测试 | 已建立 |
| `scripts/`、`templates/` | 本地与 CI 共用的检查脚本；`dotnet new` 模块模板与新建模块脚本 | 已建立 |
| `docs/ops/` | 部署、运维手册 | 规划，阶段 5 |
| `web/` | Vue 3 前端：外壳、请求层已有，表单与业务表格在做 | 已建立 |

## 命名

- 仓库 / 文件夹名：`KMIC.Platform`。
- 代码根命名空间与项目名为 `Kmic`（如 `Kmic.BuildingBlocks.Application`、`Kmic.Identity.Domain`），不加 `Platform` 段；三个字母以上的缩写只首字母大写（06 第 4.2 节）。
- 2026-11-04 由 `Kelong.Platform` / `Kelong.*` 更名；旧系统名称（`Kelong_Log` 等）保持原样。
