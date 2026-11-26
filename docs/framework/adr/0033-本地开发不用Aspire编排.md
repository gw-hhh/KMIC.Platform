# ADR-0033 本地开发不用 Aspire 编排，可观测面板单独起

- 状态：已确认
- 日期：2026-11-26
- 取代：ADR-0021 目录结构中的 `src/Aspire/Kmic.AppHost`（本地开发编排）；ADR-0016“开发环境使用 Aspire 仪表盘”的做法（仪表盘仍然用，改为独立镜像）

## 背景

本地开发原本由 `Kmic.AppHost`（Aspire）编排数据库、迁移和应用，同时提供仪表盘看日志、追踪和指标。实际情况：

- `Kmic.AppHost` 在开发机上一启动就崩溃（0xC0000005），新建的空 AppHost 同样崩溃，一直没有验证过。
- 本地启动早已由 `scripts/dev.cs` 承担：起数据库容器、建库和运行账号、迁移、建管理员、生成签名密钥、启动 Api。
- 两套启动方式已经出现分叉：AppHost 只配了 Worker 角色，联调所需的 Api 角色、签名密钥、可信来源等都只在 `dev.cs` 里。每加一项配置都要改两处，否则其中一套就是坏的。

## 备选方案

| 方案 | 优点 | 缺点 |
|---|---|---|
| 修好 AppHost，两套并存 | 保留 Aspire 的编排体验 | 长期维护两套本地启动方式；崩溃原因不在本项目代码里，修复时间不可控 |
| 只留 AppHost，去掉 `dev.cs` | 一套 | 现在跑不起来 |
| **只留 `dev.cs`，仪表盘用独立镜像** | 一套启动方式；仪表盘照样可用 | 没有 Aspire 的资源依赖图和一键起所有服务 |

## 决策

- 删除 `Kmic.AppHost`，本地启动只用 `scripts/dev.cs`。
- 可观测面板：`dotnet run scripts/dev.cs -- up --dashboard` 另起 `mcr.microsoft.com/dotnet/aspire-dashboard`（固定版本）容器，只绑定本机地址、免登录；`dev.cs` 给 Api 设置 `OTEL_EXPORTER_OTLP_ENDPOINT`。不加参数时不起面板，也不导出遥测。
- `Kmic.ServiceDefaults`（日志、追踪、指标、脱敏的统一配置）继续由 Api 使用，从 `src/Aspire/` 挪到 `src/Hosts/`。

## 影响

- `global.json` 不再固定 Aspire SDK，版本清单去掉 Aspire 的包。
- 实施待办 T-26（AppHost 崩溃）关闭。
- 05-12 第 7 节的开发环境观测改为本条的做法；02 的 FR-OPS-06（本地开发 Aspire 编排）由本条取代，02 正文不改。
