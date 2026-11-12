# 07 业务模块标准写法

- 状态：已确认（2026-11-04，D1～D3 均按建议确定）；之后的修订见文末“修订记录”
- 日期：2026-11-04
- 适用范围：每个业务模块与平台模块的项目结构、注册方式、文件组织、各构件的标准写法、测试写法、模块设计文档格式
- 依据：ADR-0003、ADR-0004、ADR-0006、ADR-0021；05 系列全部；06 编码规范

## 0. 范围

| 本文内容 | 不在本文 |
|---|---|
| 一个模块由哪些项目组成、怎么注册到宿主 | 各构件的设计与规则（05 系列）；本文只给写法并注明出处 |
| 每个项目的目录结构、一个用例由哪些文件组成 | 通用编码规则（06） |
| 各构件的标准示例：聚合、仓储、映射、命令、查询、授权、端点、跨模块通信、后台处理 | 任何具体业务模块的划分与设计 |
| 新增用例、新增模块的步骤；`dotnet new` 模块模板的内容 | 宿主、BuildingBlocks 本身的实现 |
| 模块测试的写法；业务模块设计文档的格式 | 实施顺序（08） |

### 0.1 本文决定的分类

| 类别 | 本文中的内容 |
|---|---|
| 第一类：现在确定 | D1～D3：模块注册类的位置、Application 的文件粒度、模块模板与参考模块的形态 |
| 第二类：框架定写法，业务定用不用 | 第 4 节中标为“按需”的构件（Contracts 查询、事件、订阅、定时任务、后台作业、编号、附件、通知）——模块需要时按本文写，不需要就不写 |
| 第三类：延后 | 无 |

对照 00-README“延后事项”表：本文是写法模板，`Sample` 不是业务模块，**不触发**任何延后事项。表中“关系可见”“授予有效期”等事项在第一个真实业务模块设计时，按第 8 节模块设计文档模板的“延后事项对照”一节处理。

### 0.2 框架负责 / 业务负责

| 框架负责（本文） | 业务负责（各模块设计时决定） |
|---|---|
| 项目结构、注册方式、目录与文件组织 | 有哪些模块、各模块的边界 |
| 每种构件“怎么写” | 模块有哪些聚合、用例、权限点、范围对象、事件 |
| 模块设计文档的格式与必填项 | 设计文档的内容 |

文中的 `Sample` 模块、`Sample` 聚合（单据：标题、明细、草稿 → 已提交 → 已通过 / 已驳回）及其全部用例都是**示例**，只用于演示写法，不代表任何业务规则。

---

## 1. 旧项目怎么做 → 新框架怎么做

| 问题 | 旧项目 | 新框架 | 章节 |
|---|---|---|---|
| 一个功能的代码在哪 | 页面 `.aspx.cs` + ashx 中的 `switch(Ctype)` 分支 + BLL + 手写 SQL，分散且无固定位置 | 一个用例一个文件夹：命令（含声明）、校验器、处理器；端点一个文件 | 3 |
| 巨型文件 | ashx 单文件 3000～1.4 万行 | 一个类型一个文件；一个端点一个文件 | 3、D2 |
| 公共逻辑 | `BasePage` 基类取用户、判登录、判权限 | 管道统一处理，处理器只写业务编排 | 4.3 |
| 新增功能的步骤 | 无约定，按个人习惯 | 新增用例、新增模块各有固定步骤清单 | 5、6 |
| 新模块的起点 | 复制已有页面改 | `dotnet new` 生成骨架 + 可运行的参考模块 | D3 |
| 设计文档 | 无 | 每个模块一份设计文档，格式固定，含延后事项对照 | 8 |

---

## 2. 模块的组成与注册

### 2.1 项目

每个模块 5 个项目（ADR-0004），另有测试项目：

```mermaid
graph TD
    Host[Kmic.Api 宿主] --> Pre[Kmic.Sample.Presentation]
    Host --> Inf[Kmic.Sample.Infrastructure]
    Pre --> App[Kmic.Sample.Application]
    Inf --> App
    App --> Dom[Kmic.Sample.Domain]
    App --> Con[Kmic.Sample.Contracts]
    App --> OC[其他模块.Contracts]
    Inf --> OC
```

| 项 | 约定 |
|---|---|
| 模块名 | 单个英文名词，大驼峰（`Sample`、`Identity`）；即 schema 名、权限点 / 错误码 / 消息名前缀、路由前缀（小写）。模块名是稳定契约，发布后不改 |
| 目录 | `src/Modules/Sample/Kmic.Sample.{层}/`；测试 `tests/Modules/Sample/` |
| 模块名常量 | `Kmic.Sample.Contracts` 中的 `SampleModuleInfo.Name`；模块内各层通过引用链都能看到（Application → Contracts），其他模块通过 Contracts 引用 |
| 模块编号 | 两位数字，登记在模块登记表（2.4），用于日志 `EventId` 分段（06 D6）：编号 `NN` 的模块使用 `NN000`～`NN999` |

### 2.2 D1 模块注册类放在哪里

ADR-0021 规定每个模块提供一个 `IModule`，负责注册服务、端点、权限点、定时任务和迁移；05-06 D1 写的是“`IModule.MapEndpoints` 扫描本模块 Presentation 程序集”。05-07、05-08 的示例把 `IModule` 放在 Infrastructure。但 **Infrastructure 不引用 Presentation**（ADR-0004），放在 Infrastructure 的注册类拿不到端点所在的程序集；而 Presentation 只能引用 Application，也拿不到 DbContext。需要定下注册类的位置。

| 选项 | 做法 | 优点 | 缺点 |
|---|---|---|---|
| **A 两个注册类** | Infrastructure 中 `SampleModule : IModule`（服务、数据、消息、调度）；Presentation 中 `SampleEndpoints : ModuleEndpoints`（端点）；宿主 `AddModule<SampleModule, SampleEndpoints>()` | 不改变 ADR-0004 的引用规则；Worker 角色天然只用第一个类（05-08：Worker 不映射端点）；两个类在编译期由宿主配对 | 每个模块两个注册类；需要启动校验两者的模块名一致 |
| B 每个模块增加第 6 个项目 `Kmic.Sample.Module` | 组合根项目引用 Infrastructure 与 Presentation，`IModule` 放在这里 | 一个注册类 | 违背 ADR-0004 的“5 个项目”，15 个模块多 15 个项目；这个项目能同时看到所有层，成为新的“什么都能引用”的位置 |
| C 宿主按程序集名称约定加载 Presentation | `IModule` 在 Infrastructure，按字符串 `Kmic.Sample.Presentation` 找端点程序集 | 一个注册类，不加项目 | 依赖字符串约定，改名后要到运行时才发现；隐式 |

- **决定（已确认，2026-11-04）**：A。
- **理由**：A 是唯一既不改引用规则、又完全显式的做法；“两个注册类”恰好对应“服务端组装”与“HTTP 暴露”两件事，与 Api / Worker 两种角色的分工一致。
- **取舍**：每个模块多一个很小的类（通常只有一行）。相应地，05-06 D1 中端点扫描由 `ModuleEndpoints` 承担，ADR-0021 第 2 节的注册写法为 `AddModule<QuotationModule, QuotationEndpoints>()`；05-07 的模块依赖按模块名声明（见下），因为模块之间只能引用 Contracts，拿不到对方的 `CustomerModule` 类型。

### 2.3 注册类的形态

```csharp
// BuildingBlocks.Infrastructure
public interface IModule
{
    string Name { get; }                                   // = XxxModuleInfo.Name
    Assembly ApplicationAssembly { get; }                  // 框架从这里扫描用例、权限点、范围对象、编号规则、作业、上传档案、通知模板、设置
    void AddServices(IServiceCollection services);         // DbContext、仓储、Contracts 实现、类型化客户端、Options
    void DeclareDependencies(IModuleDependencies d) { }    // d.DependsOn(CustomerModuleInfo.Name)
    void ConfigureMessaging(IMessagingBuilder m) { }       // 订阅（05-07 第 7.2 节）
    void ConfigureScheduling(ISchedulingBuilder s) { }     // 定时任务（05-08 第 5 节）
}

// BuildingBlocks.Presentation
public abstract class ModuleEndpoints
{
    public abstract string Module { get; }                 // = XxxModuleInfo.Name
    // 扫描本类所在程序集的 IEndpoint，映射到 /api/v{n}/{module 小写}（05-06 D1、D3）
    public virtual void ConfigureGroup(RouteGroupBuilder group) { }   // 模块级的路由组设置（极少使用）
}
```

```csharp
// Kmic.Sample.Contracts/SampleModuleInfo.cs
public static class SampleModuleInfo
{
    public const string Name = "Sample";
}

// Kmic.Sample.Infrastructure/SampleModule.cs
public sealed class SampleModule : IModule
{
    public string Name => SampleModuleInfo.Name;
    public Assembly ApplicationAssembly => typeof(ISampleQueryDb).Assembly;

    public void AddServices(IServiceCollection services)
    {
        services.AddModuleDbContexts<SampleDbContext, SampleQueryDbContext, ISampleQueryDb>();
        services.AddScoped<ISampleRepository, SampleRepository>();
        services.AddKmicOptions<SampleImportOptions>(SampleImportOptions.Section);
    }

    public void ConfigureScheduling(ISchedulingBuilder s) =>
        s.Schedule<CloseExpiredSamplesCommand>("Sample.CloseExpiredSamples", cron: "0 0 2 * * ?",
            run => new CloseExpiredSamplesCommand(run.ScheduledFireTime));
}

// Kmic.Sample.Presentation/SampleEndpoints.cs
public sealed class SampleEndpoints : ModuleEndpoints
{
    public override string Module => SampleModuleInfo.Name;
}

// Kmic.Api/Program.cs
builder.AddKmicHost()
       .AddModule<IdentityModule, IdentityEndpoints>()
       .AddModule<SampleModule, SampleEndpoints>();
```

| 启动校验 | 规则 |
|---|---|
| 配对 | `IModule.Name` 与 `ModuleEndpoints.Module` 相同；同一模块名只注册一次 |
| 归属 | `ApplicationAssembly` 的程序集名为 `Kmic.{Name}.Application`；端点程序集为 `Kmic.{Name}.Presentation` |
| 依赖 | `DependsOn` 的模块已注册且已启用（05-07 第 4.1 节） |
| 停用 | 配置 `Modules:{模块}:Enabled = false` 时该模块不注册服务、端点、订阅、调度；依赖它的模块已启用 → 拒绝启动 |

- 处理器、校验器、端点、EF 配置一律自动发现（ADR-0021 第 2 节），`AddServices` 中只写自动发现做不到的注册。

### 2.4 模块登记表

`KMIC.Platform/docs/modules/README.md` 维护全部模块的登记表：

| 模块 | schema | 编号 | 类型 | 依赖 | 负责人 | 设计文档 |
|---|---|---|---|---|---|---|
| Identity | `Identity` | 10 | 平台 | — | | `modules/identity/` |
| Sample | `Sample` | 99 | 参考（D3） | Identity | | 本文 |

- 编号 10～19 留给平台模块，20～89 给业务模块，99 为参考模块。架构测试检查：模块名、编号唯一；`[LoggerMessage]` 的 `EventId` 落在本模块的编号段内。

---

## 3. 目录结构

### 3.1 各项目

```text
src/Modules/Sample/
├─ Kmic.Sample.Domain/
│  ├─ Specimens/                      一个聚合一个文件夹
│  │  ├─ Specimen.cs                  聚合根（含状态机声明）
│  │  ├─ SpecimenItem.cs              子实体
│  │  ├─ SpecimenId.cs  SpecimenItemId.cs
│  │  ├─ SpecimenStatus.cs  SpecimenAction.cs
│  │  ├─ Events/SpecimenSubmitted.cs  领域事件
│  │  └─ ISpecimenRepository.cs
│  ├─ Shared/                         多个聚合共用的值对象、领域服务、计算策略（按需）
│  └─ Errors/SampleErrors.cs          错误目录：唯一的文件夹，按聚合分文件（05-01 第 2.4 节）
│
├─ Kmic.Sample.Application/
│  ├─ ISampleQueryDb.cs               查询接口（05-03 D1）
│  ├─ SamplePermissions.cs            权限点（05-05 第 3 节）
│  ├─ SampleScopes.cs                 范围对象（05-05 第 5.1 节）
│  ├─ SampleNumberRules.cs            编号规则（按需，05-13）
│  ├─ Specimens/                      按聚合分组
│  │  ├─ SpecimenPolicy.cs            资源策略
│  │  ├─ SpecimenActions.cs           资源动作目录
│  │  ├─ Create/                      一个用例一个文件夹
│  │  │  ├─ CreateSpecimenCommand.cs
│  │  │  ├─ CreateSpecimenItemInput.cs
│  │  │  ├─ CreateSpecimenCommandValidator.cs
│  │  │  └─ CreateSpecimenCommandHandler.cs
│  │  ├─ Submit/  Close/  Delete/ …
│  │  ├─ GetDetail/
│  │  │  ├─ GetSpecimenDetailQuery.cs
│  │  │  ├─ SpecimenDetailResponse.cs
│  │  │  └─ GetSpecimenDetailQueryHandler.cs
│  │  ├─ List/
│  │  │  ├─ ListSpecimensQuery.cs  SpecimenListItem.cs  SpecimenSort.cs
│  │  │  └─ ListSpecimensQueryHandler.cs
│  │  ├─ EventHandlers/               领域事件处理器
│  │  └─ Shared/                      本聚合多个用例共用的 DTO
│  ├─ (CloseExpired/ …)               定时任务的命令也是用例（按需）
│  └─ (Contracts/)                    本模块 Contracts 接口的实现（薄适配器，按需，05-07 D2）
│
├─ Kmic.Sample.Infrastructure/
│  ├─ SampleModule.cs
│  └─ Persistence/
│     ├─ SampleDbContext.cs  SampleQueryDbContext.cs  SampleDbContextFactory.cs  SampleIdConverters.cs
│     ├─ Configurations/SpecimenConfiguration.cs …
│     ├─ Repositories/SpecimenRepository.cs
│     └─ Migrations/
│     （外部系统的类型化客户端 Clients/、部署配置 Options/，按需）
│
├─ Kmic.Sample.Presentation/
│  ├─ SampleEndpoints.cs
│  └─ Specimens/                      按资源分组，一个端点一个文件，请求模型各自一个文件（05-06）
│     ├─ CreateSpecimenEndpoint.cs  CreateSpecimenRequest.cs  CreateSpecimenItemRequest.cs
│     └─ GetSpecimenEndpoint.cs  ListSpecimensEndpoint.cs  ListSpecimensRequest.cs …
│
└─ Kmic.Sample.Contracts/
   ├─ SampleModuleInfo.cs
   ├─ Events/SpecimenSubmitted.cs     集成事件（按需）
   └─ （ISampleReferences.cs 对外查询接口、Snapshots/ 快照 DTO，按需）

tests/Modules/Sample/
└─ Kmic.Sample.Domain.Tests/          Specimens/SpecimenTests.cs
tests/Kmic.Api.Tests/                 宿主级测试，所有模块共用一个项目（ADR-0021），按模块分目录：
   ├─ Sample/                         端点与授权（SpecimenApiTests）、授权矩阵、动作一致性；处理器的集成测试同在这里
   ├─ Contracts/Snapshots/            每个集成消息一份 JSON Schema 快照
   └─ ModuleConformanceTests.cs       对宿主里的每个模块：启动、模型与最新迁移一致
```

| 规则 | 说明 |
|---|---|
| 命名空间 = 目录 | `Kmic.Sample.Application.Specimens.Submit`（06 第 3.2 节） |
| 订阅、定时任务的命令 | 也是用例，放在**所修改对象**的用例文件夹中；注册（订阅名、Cron）集中在 `SampleModule` |
| 跨聚合的用例 | 放在“主要修改对象”的聚合文件夹中；一个命令仍然只修改一个聚合（ADR-0003） |
| 共用 DTO | 同一聚合内共用放 `Specimens/Shared/`；不跨聚合共用响应 DTO（各自演进） |
| 不允许的目录 | `Helpers/`、`Utils/`、`Common/`、`Services/` 这类按“技术杂项”命名的目录（评审）；确有共用逻辑时，按它在领域中的含义命名 |

### 3.2 D2 一个用例的文件粒度

06 第 3.2 节规定“一个文件一个顶层类型”，并把“一个用例的命令、校验器、处理器是否同文件”留给本文。

| 选项 | 做法 | 优点 | 缺点 |
|---|---|---|---|
| **A 一个类型一个文件，一个用例一个文件夹** | 如 3.1：`Submit/` 下三个文件 | 与 06 的通用规则一致，没有例外；文件名即类型名，按名称跳转、搜索、评审差异都最直接；05-02 第 2.2 节已按此写 | 小用例也是三四个文件 |
| B 一个用例一个文件 | `SubmitSample.cs` 中放命令、校验器、处理器、响应 | 一个用例一屏看完（“垂直切片”常见写法） | 文件名与类型名不一致；用例变大后文件很长，又回到“大文件”；与 06 规则冲突需要开例外 |

- **决定（已确认，2026-11-04）**：A。原定有两种例外：Presentation 中请求模型与端点同一文件；只被一个命令 / 查询使用的内嵌 DTO 与该消息同一文件。**实现时没有采用这两种例外**（见修订记录 2026-11-12）：文件名与类型名一致由 MA0048 检查，是错误级，例外要逐处豁免；改为一个类型一个文件，请求模型、内嵌 DTO 各自一个文件，放在同一文件夹里，被第二个消息使用时移到 `Shared/`。
- **理由**：用例文件夹已经把相关文件放在一起，B 的“看得全”优势很小；A 让每个类型都有固定、可预测的位置。
- **取舍**：文件数量多一些，模块模板与 IDE 模板负责生成。

---

## 4. 标准写法

各小节列出标准写法在参考模块 `Sample` 里对应的真实文件，以及这些写法的要点。`Sample` 随解决方案编译、测试，框架改动破坏了写法会立刻暴露（D3），所以这里不再抄代码；规则的来源写在小节标题后。

- 路径省略 `src/Modules/Sample/`，各项目目录名为 `Kmic.Sample.Domain` 这样的形式。
- `Sample` 的聚合叫 `Specimen`（标本），原因见本文修订记录；下文的“聚合”“聚合根”泛指它。

### 4.1 聚合（05-01）

| 构件 | 文件 |
|---|---|
| 聚合根（状态机声明、工厂方法、业务方法、`Raise`） | `Kmic.Sample.Domain/Specimens/Specimen.cs` |
| 子实体与它的 ID | `SpecimenItem.cs`、`SpecimenItemId.cs` |
| 强类型 ID | `SpecimenId.cs` |
| 状态与动作（智能枚举） | `SpecimenStatus.cs`、`SpecimenAction.cs` |
| 领域事件 | `Events/SpecimenSubmitted.cs` |
| 仓储接口 | `ISpecimenRepository.cs` |
| 错误目录 | `Kmic.Sample.Domain/Errors/SampleErrors.cs` |
| 领域测试 | `tests/Modules/Sample/Kmic.Sample.Domain.Tests/Specimens/SpecimenTests.cs` |

| 要点 | 出处 |
|---|---|
| 工厂方法创建，构造函数私有；属性 `private set`；集合只读公开 | 05-01 第 3.2 节，KM0103～KM0105 |
| 不取时间、不取当前用户：操作人、业务日期由处理器传入 | 05-01 第 9 节，05-13 D3 |
| 状态只经状态机改；需要通知时显式 `Raise` | 05-01 第 7、8 节 |
| 跨模块引用用 `XxxRef` 强类型 ID，不持有其他聚合 | 05-01 第 3.3 节 |
| 业务失败返回错误目录中的 `Error` | 05-01 第 2 节 |

### 4.2 持久化（05-03、05-05 第 5.4 节）

| 构件 | 文件 |
|---|---|
| 实体配置（字符串长度、不区分大小写、唯一约束映射错误码、数据范围） | `Kmic.Sample.Infrastructure/Persistence/Configurations/SpecimenConfiguration.cs`、`SpecimenItemConfiguration.cs` |
| 仓储（只写 `Aggregate` 里要带出的子实体） | `Persistence/Repositories/SpecimenRepository.cs` |
| 写入、查询两个上下文与设计时工厂、ID 转换器 | `Persistence/SampleDbContext.cs`、`SampleQueryDbContext.cs`、`SampleDbContextFactory.cs`、`SampleIdConverters.cs` |
| 查询接口（只暴露聚合根） | `Kmic.Sample.Application/ISampleQueryDb.cs` |
| 模块注册（`AddModuleDbContexts`、仓储） | `Kmic.Sample.Infrastructure/SampleModule.cs` |
| 迁移 | `Persistence/Migrations/` |

| 要点 | 出处 |
|---|---|
| 两个 DbContext：写入（跟踪）与查询（不跟踪），由 `AddModuleDbContexts` 注册 | 05-03 第 2 节 |
| 审计字段、软删除、版本、schema、强类型 ID、枚举 CHECK 由约定处理，配置中不写 | 05-03 第 3～6 节 |
| 字符串列声明长度；`decimal` 声明精度；需要时声明不区分大小写 | 05-03 第 3.5～3.7 节 |
| 唯一约束映射错误码，缺失时启动失败 | 05-03 第 7.3 节 |
| 数据范围在配置中声明 | 05-05 第 5.4 节 |
| 敏感字段在配置中标注数据分类 | 05-09 第 3.3 节、05-12 D2 |
| 表、列、人工索引写中文数据库注释：表在配置里写 `HasComment`，列取实体属性的 XML 注释，缺失时启动失败 | 05-03 第 3.9 节（R01 M1） |
| 迁移：`dotnet ef migrations add <名称> -p Kmic.Sample.Infrastructure -c SampleDbContext`；初始迁移由新建模块的脚本生成（第 6.1 节），含 `InitializeModuleSchema` | 05-03 第 9 节 |
| 模型改了就要生成迁移：`Kmic.Api.Tests` 的 `ModuleConformanceTests` 对宿主里的每个模块检查模型与最新迁移一致 | 05-03 第 9 节 |

### 4.3 命令（05-02）

| 用例 | 文件夹（`Kmic.Sample.Application/Specimens/`） | 演示什么 |
|---|---|---|
| 创建 | `Create/`：命令、校验器、处理器、`CreateSpecimenItemInput` | `[Idempotent]`、`[Audited]`；输入形状校验；用当前用户填负责人和所属组织；取号放在最后一个异步步骤 |
| 提交 | `Submit/`：命令、处理器 | 修改类命令：`ExpectedVersion`；加载 → 资源授权 → 业务方法 |
| 关闭 | `Close/` | 同上，另一个动作 |
| 删除 | `Delete/` | 软删除；只有草稿可以删 |

修改类命令的处理顺序固定：`GetForUpdateAsync`（第 2 层数据范围、第 5 层并发版本）→ `IResourceAuthorizer.Check`（第 3 层资源策略）→ 聚合的业务方法（第 4 层状态）。授权检查和状态变化是两步，不能写成同时求值。

| 要点 | 出处 |
|---|---|
| 每个命令：授权声明五选一、`[Audited]` / `[NotAudited]` 二选一；创建、提交、审批类 `[Idempotent]` | 05-02 第 3 节 |
| 修改类命令带 `ExpectedVersion` | 05-06 D6、05-03 第 6 节 |
| 处理器只编排：不 `SaveChanges`、不开事务、不写 Outbox；一个命令只改一个聚合 | 05-02 第 2.2、6 节 |
| 处理器是 `public sealed`（Mediator 生成的代码要引用它），参数名沿用 `command` / `cancellationToken` | 05-02 第 2.2 节、06 第 4.2 节 |
| 一个命令只取一次当前时间 | 05-13 第 3.1 节 |
| 校验器是 `internal sealed`，只查形状，不注入数据访问 | 05-02 第 4.4 节 |
| 只服务于一个命令的内嵌 DTO 也单独一个文件（`CreateSpecimenItemInput.cs`），不与命令同文件 | 本文 D2 修订 |
| 用户传入的其他模块 ID 用 `CallerScoped` 查询校验 | 05-07 第 4 节 |

### 4.4 查询（05-03 D1、05-06 第 6 节、05-05 第 7 节）

| 用例 | 文件夹（`Kmic.Sample.Application/Specimens/`） | 演示什么 |
|---|---|---|
| 详情 | `GetDetail/`：查询、响应、处理器 | 经 `IEntityFinder` 取单个对象：范围外与不存在同样返回 `NotFound`，范围外另记安全事件；响应带 `allowedActions` |
| 列表 | `List/`：查询、列表项、`SpecimenSort`、处理器 | 投影到 DTO；筛选（关键字、状态）；排序白名单；分页 |
| 共用 DTO | `Shared/SpecimenItemResponse.cs` | 同一聚合内共用放 `Shared/` |

| 要点 | 出处 |
|---|---|
| 从 `ISampleQueryDb` 出发（已带数据范围与软删除过滤器）；列表、详情、导出、下拉共用同一个起点 | 05-05 第 5.4 节 |
| 列表投影到 DTO，不加载聚合；详情需要 `allowedActions` 时才加载聚合 | 05-05 D10 |
| 筛选模型的字段名与查询参数一致；日期筛选参数为 `DateOnly`，用 `IBusinessCalendar` 换算成半开区间（`Sample` 的列表没有日期筛选，规则见出处） | 05-06 第 6.4 节、05-13 第 3.4 节 |
| 文本模糊匹配用 `KmicFunctions.ContainsText` | 06 第 11.2 节 |
| 引用其他模块的显示名：先收集本页 ID，再一次 `ResolveAsync` | 05-07 第 4 节 |
| 导出复用同一个查询的筛选与排序 | 05-08 第 6.8 节 |
| 数据量可能很大的列表声明上限计数（`CountMode.Capped`），响应带 `totalCountExact` | 05-06 第 6.1 节（R01 M4） |
| 查询处理器同样是 `public sealed` | 05-02 第 2.2 节 |

### 4.5 授权声明（05-05）

| 构件 | 文件 |
|---|---|
| 权限点（`模块.资源.动作`，敏感点标 `Sensitive`） | `Kmic.Sample.Application/SamplePermissions.cs` |
| 范围对象 | `SampleScopes.cs` |
| 资源策略（纯函数；只实现一个 `IResourcePolicy<,>`） | `Specimens/SpecimenPolicy.cs` |
| 资源动作目录（权限点与命令的 `[RequirePermission]` 同源；状态检查取自聚合的 `CanFire`） | `Specimens/SpecimenActions.cs` |

动作目录里的每个动作都要有对应的命令。`Kmic.Api.Tests/Sample/SpecimenAllowedActionsTests.cs` 对每个状态、每种身份验证“`allowedActions` 包含某动作 ⇔ 执行对应命令不会被拒绝”；`SpecimenAuthorizationMatrixTests.cs` 验证范围等级 × 数据归属 × 出口的授权矩阵。

### 4.6 端点（05-06）

| 构件 | 文件（`Kmic.Sample.Presentation/`） |
|---|---|
| 模块端点注册类 | `SampleEndpoints.cs` |
| 创建、详情、列表、提交、关闭、删除 | `Specimens/CreateSpecimenEndpoint.cs`、`GetSpecimenEndpoint.cs`、`ListSpecimensEndpoint.cs`、`SubmitSpecimenEndpoint.cs`、`CloseSpecimenEndpoint.cs`、`DeleteSpecimenEndpoint.cs` |
| 请求模型 | `CreateSpecimenRequest.cs`、`SubmitSpecimenRequest.cs` 等，一个类型一个文件 |

| 动作 | 方法与路径（模块前缀由 `SampleEndpoints` 加上：`/api/v1/sample`） | 结果转换 |
|---|---|---|
| 新建 | `POST /specimens` | `ToCreated(id => …)` |
| 详情 | `GET /specimens/{id}` | `ToOk()` |
| 列表 | `GET /specimens`（查询参数 `[AsParameters]`） | `ToOk()` |
| 修改 | `PUT /specimens/{id}` | `ToOk()`（返回新版本） |
| 动作 | `POST /specimens/{id}/submit` | `ToOk()` |
| 删除 | `DELETE /specimens/{id}`，版本号在请求体 `expectedVersion` | `ToNoContent()` |
| 后台作业 | `POST /specimens/imports` | `ToAccepted(…)` |

- 并发版本在请求体里，不用 `If-Match`，也不放在查询参数里（05-06 D6）。
- 路径参数的说明写在处理方法的参数上：`[Description("标本 ID")] SpecimenId id`；缺少时文档完整性检查失败。
- 统计、大范围查询等重型端点声明并发策略：`.RequireConcurrency(ConcurrencyPolicies.Heavy, ConcurrencyPolicies.HeavyPerUser)`（05-06 第 8.3 节，R01 M5）。
- 端点的 `.WithSummary(...)`、请求模型与响应 DTO 的 XML 注释就是前端看到的接口文档，缺少时 CI 失败（05-06 第 7.1 节，R01 M2）。
- 端点是 `internal sealed`。

### 4.7 跨模块（按需，05-07）

| 需要 | 写法 | 出处 |
|---|---|---|
| 让其他模块查本模块数据 | Contracts 中定义 `ISampleReferences` / `ISampleXxxQueries`；Application 的 `Contracts/` 文件夹中写薄适配器，发送带 `[ContractQuery(...)]` 的查询；在 `AddServices` 中注册 | 05-07 第 4.1 节 |
| 查其他模块数据 | 在 `DeclareDependencies` 中 `d.DependsOn(CustomerModuleInfo.Name)`；处理器注入对方的 Contracts 接口 | 05-07 第 4.1 节 |
| 通知其他模块“发生了什么” | Contracts 中定义 `[IntegrationMessage("Sample.SampleApproved")]` 事件；在**领域事件处理器**或命令处理器中 `publisher.Add(...)` | 05-07 第 5 节 |
| 被其他模块引用、需要同步副本 | 发布 `SampleChanged` 快照事件，实现 `ISampleSnapshotSource` | 05-07 第 5.2、8.3 节 |
| 收到其他模块的事件后做事 | 本模块定义 `[SystemOnly]` 命令；在 `ConfigureMessaging` 中 `Subscribe<事件, 命令>("Sample.Xxx", 映射)` | 05-07 第 7.2 节 |
| 请平台模块做事（发通知、登记文件引用） | `publisher.Send(new SendNotification(...))` | 05-07 D6、05-11 |

`Sample` 演示“通知其他模块发生了什么”这一种，其余几种框架测试里的 Probe、Echo 两个模块有完整用法（`tests/BuildingBlocks/Kmic.BuildingBlocks.Infrastructure.Tests/Probe`、`tests/BuildingBlocks/Kmic.Echo.Probe`）：

| 构件 | 文件 |
|---|---|
| 集成事件（载荷只用 `Guid` 与基础类型） | `Kmic.Sample.Contracts/Events/SpecimenSubmitted.cs` |
| 领域事件 → 集成事件的处理器 | `Kmic.Sample.Application/Specimens/EventHandlers/PublishSpecimenSubmitted.cs` |
| 提交后事件随业务数据写进 Outbox 的测试 | `tests/Kmic.Api.Tests/Sample/SpecimenApiTests.cs` 的 `Submit_PublishesTheIntegrationEventWithTheBusinessData` |
| 契约快照 | `tests/Kmic.Api.Tests/Contracts/Snapshots/Sample.SpecimenSubmitted.v1.json`，由 `MessageContractTests` 对宿主里的全部模块检查 |

- 领域事件处理器不用在模块注册类里登记：`AddKmicApplication` 扫描各模块的 Application 程序集，实现了 `IDomainEventHandler<T>` 的类自动注册，可以是 `internal`。
- 集成事件的契约改动要过快照检查（05-07 第 5.5 节）：兼容的改动（新增可空字段）更新快照；破坏性的改动新建 `Version = 2` 的消息类。更新快照的方法：设环境变量 `KMIC_UPDATE_SNAPSHOTS=1` 重跑 `MessageContractTests`。
- 领域事件与集成事件同名、以命名空间区分（05-07 第 3.3 节）；集成事件的载荷只用 `Guid` 与基础类型（Contracts 不含强类型 ID）。
- 需要加载本模块其他数据时，领域事件处理器可以注入本模块仓储（05-02 第 6.2 节）。

### 4.8 后台处理（按需，05-08）

| 需要 | 写法 | 出处 |
|---|---|---|
| 定时执行 | `[SystemOnly]` 命令（可重复执行）+ `ConfigureScheduling` 中 `Schedule<命令>(名称, Cron, 映射)` | 05-08 第 5 节 |
| 用户发起的长任务（导入、导出、批量） | `[BackgroundJob]` 作业参数 + `IJobHandler<T>`（编排者，只发命令和查询）+ 一个提交作业的 `[Idempotent]` 命令 + 返回 202 的端点 | 05-08 第 6 节 |
| 启动时执行 | 一般不需要；确需时实现 `IStartupTask` | 05-08 第 3.3 节 |

### 4.9 其他按需构件

| 需要 | 写法 | 出处 |
|---|---|---|
| 部署配置 | Infrastructure `Options/` 中的 `sealed class`，`AddKmicOptions<T>` | 05-10 第 2 节 |
| 管理员可改的系统设置、用户偏好 | Application 中声明设置 / `[Preference]` | 05-10 第 6、7 节 |
| 字典取值校验 | 注入 `IDictionaries`，`EnsureSelectableAsync` | 05-10 D3 |
| 业务编号 | `SampleNumberRules` 中 `[NumberRule]` 常量；处理器最后一步 `NextAsync` | 05-13 第 2 节 |
| 附件 | `[UploadProfile]` 声明；命令引用临时文件 ID，经 `IFileAttachments.AttachAsync(owner, fileIds)` 关联（直接发送 `AttachFiles` 由分析器拦截） | 05-11 第 3 节 |
| 通知 | `[NotificationTemplate]` 模型；`publisher.Send(SendNotification.For(...))` | 05-11 第 7、8 节 |
| 外部系统 | Application 中定义接口，Infrastructure `Clients/` 中 `AddKmicHttpClient` | 05-11 第 9 节 |
| 日志 | 每个类旁边的 `{类名}Log` 静态 partial 类，`[LoggerMessage]`，`EventId` 在本模块编号段 | 06 D6 |
| 缓存 | 只在确有需要时，按 05-10 第 4.1 节分类说明 | 05-10 第 4 节 |

---

## 5. 新增一个用例的步骤

以“给 `Sample` 增加‘驳回’动作”为例（示例）：

| 步骤 | 位置 | 内容 |
|---|---|---|
| 1 | Domain | 聚合方法 `Reject(now, reason)`；状态机增加流转；需要通知时 `Raise`；错误目录增加错误 |
| 2 | Domain 测试 | 新方法的成功、失败、非法流转 |
| 3 | Application | `SamplePermissions` 增加权限点；`SampleActions` 增加动作；`SamplePolicy` 中写明规则或显式 `Allow` |
| 4 | Application | `Samples/Reject/`：命令（声明）、校验器（如需要）、处理器 |
| 5 | Presentation | `Samples/RejectSample.cs`：请求模型 + 端点 |
| 6 | 集成测试 | 处理器：成功、业务失败、版本冲突、幂等重放 |
| 7 | API 测试 | 授权：无权限点 403、范围外 404、资源策略拒绝 403；`allowedActions` 一致性测试自动覆盖新动作 |
| 8 | 文档 | 模块设计文档的“用例”“权限点”两张表；新术语登记术语表（06 D3） |
| 9 | 检查 | 本地 `dotnet build`（分析器）、`dotnet test`；OpenAPI 与契约快照的变化在合并请求中可见 |

需要表结构变化时，在步骤 1 之后增加迁移（`dotnet ef migrations add`），破坏性变化按 06 第 11.3 节分步。

---

## 6. 新增一个模块

### 6.1 D3 模块模板与参考模块的形态

ADR-0004 已定用 `dotnet new` 模板生成 5 个项目和标准目录。还需要决定：模板生成什么内容，以及“完整写法”的样板放在哪里。

| 选项 | 做法 | 优点 | 缺点 |
|---|---|---|---|
| A 模板生成完整示例代码 | 新模块生成后就带一个完整的示例聚合和全部用例，开发者改名或删除 | 新模块马上能运行 | 每个新模块都要删一遍示例代码，漏删就留在生产代码中；模板中的示例不参与编译和测试，框架改动后会悄悄过时 |
| B 模板只生成空骨架，写法只在本文中 | 模板生成项目、引用、注册类、空的权限点 / 错误目录 | 生成物干净 | 文档中的示例代码同样不经编译，会与框架实现逐渐脱节 |
| **C 空骨架模板 + 仓库中常驻一个可运行的参考模块 `Sample`** | 模板同 B；`src/Modules/Sample` 是一个真实、完整、随 CI 编译和测试的模块，覆盖本文第 4 节全部构件；只在开发与测试环境注册，生产不注册 | 写法样板始终能编译、测试通过，框架改动若破坏写法会在 CI 中立刻暴露；同时是框架的端到端集成测试；新模块不带任何多余代码 | 需要维护一个不上生产的模块；需要一条规则保证它不会被生产环境注册 |

- **决定（已确认，2026-11-04）**：C。
  - 模板：`kmic-module`，生成 5 个项目、测试项目、`CustomerModuleInfo`、`CustomerModule`、`CustomerEndpoints`、两个 DbContext、`ICustomerQueryDb`、空的 `CustomerPermissions` / `CustomerScopes` / `CustomerErrors`、解决方案过滤文件 `Customer.slnf`。模板经下文的脚本调用，不直接运行 `dotnet new`。
  - 参考模块：`Sample` 编号 99；宿主中以 `AddModule<SampleModule, SampleEndpoints>(onlyIn: KmicEnvironments.NonProduction)` 注册，生产环境启动校验发现 `Sample` 被注册即拒绝启动。
  - 本文第 4 节的示例在实施后改为引用 `Sample` 模块中的真实文件，文档只保留要点，避免两份代码。
- **理由**：“标准写法”的价值在于始终正确；只有参与编译和测试的样板才能长期保持正确。
- **取舍**：多维护一个模块；它也承担了框架端到端测试的作用，这部分维护本来就要做。

新建模块的方式（原型 #11，`docs/framework/spikes/11-module-template.md`）：

- **用脚本新建模块**：`dotnet run scripts/new-module.cs -- <模块名> <编号>`。脚本依次：对照模块登记表校验名称与编号 → `dotnet new kmic-module` → `dotnet sln Kmic.slnx add`（显式指定解决方案）→ 构建 → 生成初始迁移 → `check.cs --fix` → 格式检查 → 运行模块测试，任一步失败即停止。模板本身不做“加入解决方案”：模板引擎的这个功能只认 `.sln`，并且会向上层目录查找，原型中误改了上层目录的解决方案。
- **初始迁移由脚本调用 EF 工具生成**，不是模板中的静态文件：脚本运行 `dotnet ef migrations add InitializeModuleSchema`，迁移内容总是与当时的模型一致，框架表结构变化后不需要同步修改模板。EF 工具作为仓库本地工具锁定版本（根目录 `dotnet-tools.json`，与 EF Core 同为 10.0.14），脚本自动执行 `dotnet tool restore`。
- 迁移文件按生成代码处理，编码由 `check.cs --fix` 规整（06 第 3.2 节）。

### 6.2 步骤

| 步骤 | 内容 |
|---|---|
| 1 | 写模块设计文档 `docs/modules/<模块>/README.md`（第 8 节格式），评审通过 |
| 2 | 模块登记表（2.4）登记模块名、schema、编号、依赖、负责人 |
| 3 | `dotnet run scripts/new-module.cs -- <模块> <编号>`（6.1） |
| 4 | 宿主 `Program.cs` 中 `AddModule<XxxModule, XxxEndpoints>()`；测试宿主同步注册 |
| 5 | `CODEOWNERS` 增加模块目录的负责人 |
| 6 | 按设计文档逐个用例实现（第 5 节） |
| 7 | 迁移脚本随 CI 生成，人工审查（05-03 第 9.2 节） |

### 6.3 模块划分检查表

“一个事务只改一个模块”（ADR-0006）不变，模块之间只有最终一致（事件、请求型消息）。因此，**必须在同一次提交中成立的规则（强不变量）决定模块边界**：它涉及的数据必须归同一个模块。发现一条强不变量跨了两个模块时，合并模块或调整数据归属，不在框架中增加跨模块事务。

| 框架负责 | 业务负责（每个模块设计时） |
|---|---|
| 强制“一个事务只改一个模块”（事务行为、架构测试）；跨模块通信机制（05-07）；少数横切场景的专门互斥协议（如附件，05-11 第 3.3 节） | 模块划分；列出强不变量并判断归属；逐项填写下表 |

| # | 检查项 | 不通过时 |
|---|---|---|
| 1 | 列出本模块的强不变量（**示例**：明细金额合计等于单据总额；同一时段同一资源不能被重复占用），每一条涉及的数据是否都归本模块所有 | 把数据移到本模块，或与另一个模块合并 |
| 2 | 每个命令是否只修改本模块的数据；需要其他模块改数据的地方，业务上能否接受最终一致（秒级延迟，失败后重试或人工处理） | 不能接受 → 它其实是第 1 项的强不变量 |
| 3 | 跨模块的“先检查、后写入”：命令用 Contracts 查询的结果做判断，查询之后、提交之前对方数据改变，后果能否接受 | 不能接受 → 调整数据归属；横切场景由框架提供专门协议，业务模块不自行加跨模块锁 |
| 4 | 本模块订阅的事件处理失败、进入死信时，业务上是否有可见的待处理状态和人工处理入口 | 补充状态与处理入口 |
| 5 | 需要的其他模块数据，用 Contracts 查询（实时）还是读模型（本地副本、最终一致），理由是否写明 | 按 05-07 第 4、8 节补充 |
| 6 | 模块之间的依赖是否单向、无环（`DependsOn`，05-07 第 4.1 节） | 调整职责，或改用事件反转依赖 |
| 7 | 拆成两个模块的理由（团队分工、变更频率、数据量、权限边界）是否大于因此失去的事务一致性 | 不拆 |

- 检查结论写进模块设计文档第 0 节（第 8 节），评审时逐项确认。

---

## 7. 测试写法

测试规则见 06 第 12 节，这里给出模块里各类测试的标准形态和 `Sample` 中对应的真实文件。

### 7.1 领域测试

不依赖任何基础设施，直接构造聚合、调用业务方法、断言结果与领域事件：`tests/Modules/Sample/Kmic.Sample.Domain.Tests/Specimens/SpecimenTests.cs`。每个模块一个领域测试项目，由新建模块的脚本生成。

### 7.2 处理器的集成测试（处理器 + 真实数据库）

`tests/Kmic.Api.Tests/Sample/SpecimenCommandTests.cs`：以某个身份建立执行作用域，消息走完整管道（校验、授权、幂等、事务、审计）落到真实 PostgreSQL，断言 `Result` 与错误码，不经过 HTTP。

- 身份用 `TestIdentity`（`Kmic.BuildingBlocks.Testing`），在 `ApiTestBase` 里登记到测试用的访问上下文加载器；`IExecutionScopeFactory.CreateScope(identity)` 建立作用域，`ICommandSender` / `IQuerySender` 发送消息；带 `[Idempotent]` 的命令先设置 `RequestMetadata.IdempotencyKey`。
- 每个测试类一个克隆库（06 D8），同一个类里的测试共用一个库，所以断言不要依赖“库里只有我的数据”：用唯一的标题或编号挑出自己的数据。
- 与 API 测试放在同一个项目：处理器测试和端点测试都需要完整的宿主与迁移过的数据库，集中在 `Kmic.Api.Tests` 里按模块分目录（ADR-0021），不再为每个模块另建集成测试项目。

### 7.3 每个模块都要满足的测试

原设计是由模板为每个模块生成六类测试。实现时改为：能对宿主里全部模块通用的，写成遍历式测试，新增模块自动覆盖；必须按模块写的，在 `Sample` 里给出样板，新模块照写。

| 测试 | 内容 | 怎么实现 | 出处 |
|---|---|---|---|
| 启动 | 模块注册、用例目录、权限点目录、EF 模型校验全部通过 | 宿主启动时检查，`ModuleConformanceTests.Host_StartsWithEveryRegisteredModule` 启动宿主并检查就绪 | 05-02、05-05、05-03 |
| 唯一约束映射 | 模块的每个唯一索引都登记了错误码 | EF 模型定稿时的校验，缺失即启动失败，不需要另写测试 | 05-02 第 4.3 节 |
| `allowedActions` 一致性 | 动作目录中每个动作 ⇔ 对应命令不因第 1、3、4 层被拒 | 每个资源一份，样板：`Sample/SpecimenAllowedActionsTests.cs`。动作目录里的每个动作都要有对应的命令；身份要覆盖“负责人、非负责人、没有权限的人、负责人但缺其中一个动作的权限” | 05-05 第 7.1 节 |
| 授权矩阵 | 每个范围对象：范围等级 × 数据归属 × 出口 | 每个范围对象一份，样板：`Sample/SpecimenAuthorizationMatrixTests.cs`（四个等级 × 四种归属 × 列表与详情）。模块有导出、下拉、统计出口时加进同一张矩阵；身份状态（角色切换、用户禁用、角色收回）由框架测试覆盖 | 05-05 第 12.2 节 |
| 消息契约快照 | 每个集成消息的 JSON Schema | 全部模块通用：`Contracts/MessageContractTests` 从宿主出发找到各模块的 Contracts 程序集，每个消息一个文件放在 `Contracts/Snapshots/`；兼容变化更新快照，破坏性变化失败（`MessageContractSnapshots`） | 05-07 第 5.5 节 |
| 迁移 | 模型与最新迁移一致（无未生成的变更） | 全部模块通用：`ModuleConformanceTests.EveryModuleModel_MatchesItsLatestMigration` | 05-03 第 9 节 |

---

## 8. 业务模块设计文档模板

每个业务模块在实现前写一份设计文档 `KMIC.Platform/docs/modules/<模块小写>/README.md`，评审通过后才开始编码。格式：

| 节 | 内容 | 必填 |
|---|---|---|
| 0 概述 | 模块职责、边界（做什么、不做什么）、模块名 / schema / 编号；第 6.3 节模块划分检查表的逐项结论（含本模块的强不变量清单） | ✓ |
| 1 旧系统对照 | 旧系统中对应的页面、表、规则；哪些保留、哪些改变、哪些不做 | ✓ |
| 2 术语 | 本模块新增的术语（同时登记 `docs/glossary.md`） | ✓ |
| 3 聚合 | 每个聚合：字段、不变量、状态机（Mermaid 状态图）、所属人 / 所属组织的业务含义 | ✓ |
| 4 用例 | 命令与查询清单：名称、权限点、幂等、审计、资源策略规则；修改金额、额度、库存这类数值的操作，写明对应的业务单据（05-02 第 5.4 节） | ✓ |
| 4A 前端 | 页面清单（路由名、页面类型、菜单归属、权限点、新建与编辑的打开方式及理由）、列表、表单、动作、旧系统页面对照，格式见前端 F05 第 17 节。编号用 4A，是为了不改动其后各节的编号 | ✓ |
| 5 授权 | 权限点、范围对象、资源策略、敏感权限点 | ✓ |
| 6 跨模块 | 依赖的模块与 Contracts；对外提供的 Contracts；发布与订阅的事件 | 按需 |
| 7 后台处理 | 定时任务、后台作业 | 按需 |
| 8 其他构件 | 编号规则、附件、通知模板、设置、字典 | 按需 |
| 9 数据迁移 | 旧数据如何导入、编号如何衔接 | ✓ |
| 10 延后事项对照 | 逐条对照 00-README“延后事项”表：命中的在本模块一并设计，并从表中删除；新产生的延后事项登记进去 | ✓ |
| 11 框架负责 / 业务负责 | 本模块用到的框架能力与本模块自己的规则分界 | ✓ |
| 12 待决 | 需要业务负责人拍板的事项，附选项与建议 | ✓ |

---

## 9. 与其他文档的接口

| 本文提供 / 需要 | 对方 |
|---|---|
| `IModule` + `ModuleEndpoints` 两个注册类、`AddModule<TModule, TEndpoints>()` | 05-06 D1、ADR-0021 |
| `DependsOn(模块名)`，不用 `DependsOn<TModule>()` | 05-07 第 4.1 节 |
| `XxxModuleInfo.Name` 放在 Contracts | 05-07 第 3 节 |
| 模块编号与 `EventId` 分段 | 06 D6 |
| 参考模块 `Sample`、`dotnet new kmic-module` 模板 | 08 实施计划（框架阶段的交付物） |
| 模块登记表 `docs/modules/README.md`、模块设计文档模板 | 各业务模块设计 |
| 测试基座 `TestIdentity` / `TestOrgTree`、`TestAccessContextLoader`、契约快照 `MessageContractSnapshots` | `Kmic.BuildingBlocks.Testing`（06 D8、05-05 第 12 节、05-07 第 5.5 节） |

本文编写过程中顺带修正了 05 系列示例中的不一致（不改变任何决定）：

| 文档 | 修正 |
|---|---|
| 05-02 第 2.2 节 | 处理器示例改用 05-05 定义的 `IResourceAuthorizer<Sample, SampleAction>` 与 `SampleAction.Submit` |
| 05-07、05-08、05-10 | 示例中的命令 / 查询补上 `Command` / `Query` 后缀（05-02 命名规则）；`ICommand<Result<T>>` 改为 `ICommand<T>`、`IQuery<Result<T>>` 改为 `IQuery<T>`（05-02 第 2.1 节的接口定义）；`new CustomerId(...)` 改为 `CustomerId.From(...)`（Vogen 禁止 `new`）；`SendBatch` 改为 `SendBatchAsync` |
| 06 | 集成事件命名改为与 05-07 第 3.3 节一致（无后缀）；配置节、注册方法名与 05-10 一致；测试基础设施项目名统一为 `Kmic.BuildingBlocks.Testing` |

---

## 10. 决策汇总

| 编号 | 事项 | 决定（均已确认，2026-11-04） |
|---|---|---|
| D1 | 模块注册类放在哪里 | 两个注册类：Infrastructure 的 `IModule` + Presentation 的 `ModuleEndpoints`，宿主 `AddModule<TModule, TEndpoints>()` 配对 |
| D2 | 一个用例的文件粒度 | 一个类型一个文件、一个用例一个文件夹；原定的两种“同文件”例外实现时没有采用，请求模型和内嵌 DTO 同样单独一个文件 |
| D3 | 模块模板与参考模块 | 空骨架模板（经 `scripts/new-module.cs` 调用，初始迁移由脚本生成）+ 仓库中常驻、随 CI 编译测试、生产不注册的参考模块 `Sample` |

---

## 11. 修订记录

| 日期 | 内容 | 来源 |
|---|---|---|
| 2026-11-05 | 新增第 6.3 节模块划分检查表；第 8 节设计文档第 0 节附检查结论、第 4 节写明数值操作对应的业务单据 | R02-14 |
| 2026-11-05 | 修正：第 4.9 节“附件”一行的 `publisher.Send(AttachFiles…)` 与 05-11 第 3.3 节不一致，改为经 `IFileAttachments.AttachAsync` 关联 | R02 第 4 批 |
| 2026-11-05 | 第 8 节设计文档模板增加“4A 前端” | 前端 F05 |
| 2026-11-06 | 第 4.3、4.4 节示例中的命令、查询处理器改为 `public sealed` | 原型 #1 D1 |
| 2026-11-06 | 第 6.1、6.2 节：新建模块用 `scripts/new-module.cs`；初始迁移由脚本调用 EF 工具生成（D3 中“模板生成初始迁移”相应修订）；模板生成能力的待确认关闭 | 原型 #11 D1～D3 |
| 2026-11-10 | 文档整合：以上补充说明并入正文，决定不变 | R01 L2（S0-08） |
| 2026-11-11 | 示例中处理器的参数名改为 `command` / `query` / `cancellationToken`：Mediator 接口规定了参数名，写成 `cmd`、`q`、`ct` 会触发 CA1725 编译不过（决定不变，06 第 4.2 节已补充这条例外） | 注释与风格复查 |
| 2026-11-12 | 第 2.3 节实现时补全（决定不变）：`IModule` 在 `Kmic.BuildingBlocks.Infrastructure.Modules`，`DeclareDependencies`、`ConfigureMessaging`、`ConfigureScheduling` 是默认实现的接口成员；宿主组装放在新项目 `Kmic.BuildingBlocks.Hosting`（引用 Infrastructure 与 Presentation，不属于任何一层，只有宿主和宿主测试引用它）。注册写法改为 `builder.AddKmicHost(host => host.AddModule<IdentityModule, IdentityEndpoints>().AddModule<SampleModule, SampleEndpoints>(onlyIn: KmicEnvironments.NonProduction))`——原写作 `builder.AddKmicHost().AddModule<…>()` 的链式调用，但框架服务（用例目录、权限点目录等）要拿到全部模块的程序集才能一次构建，链式写法没有收尾的时机，所以改成一个回调里登记完。`onlyIn: NonProduction` 的模块在生产环境不注册（原“生产环境启动校验发现 Sample 被注册即拒绝启动”：同一个 Program 在各环境运行，注册了就拒绝等于生产无法启动，所以改为跳过；漏写 `onlyIn` 而在生产环境注册 `Sample` 时才拒绝启动）。启动校验一次报全：配对、重复、Application 与 Presentation 程序集名、`Kmic:Roles`、依赖、配置里关闭的模块被依赖、数据库连接串、Api 角色必须有 `IAccessContextLoader`。`Modules:{模块}:Enabled = false` 时该模块不注册，被依赖则拒绝启动 | S2-14 实现 |
| 2026-11-12 | 参考模块 Sample 实现时补全（决定不变）：①聚合名不能和模块名相同——命名空间 `Kmic.Sample` 会遮住同名的类型 `Sample`，模块里其他命名空间的代码无法用 using 引用它，所以参考模块的聚合叫 `Specimen`（标本）；业务模块的聚合名与模块名相同时同样会遇到，划分模块时避开（第 6.3 节检查表）。②路径参数的说明写在处理方法的参数上：`[Description("标本 ID")] SpecimenId id`（.NET 10 读取它；处理方法上的 XML 注释 `param` 读不到），文档完整性检查要求每个参数有说明。③初始迁移用宿主 `Kmic.Api` 做 `--startup-project`（类库不能做启动项目，宿主引用了 EF 设计时工具）：`dotnet ef migrations add InitializeModuleSchema --project src/Modules/<模块>/Kmic.<模块>.Infrastructure --startup-project src/Hosts/Kmic.Api --context <模块>DbContext --output-dir Persistence/Migrations`，生成后在 `Up` 开头补 `migrationBuilder.InitializeModuleSchema("<模块>");` 与它的 using（模块的 schema 授权给运行账号），新建模块的脚本（S2-16）自动完成。④迁移程序与测试模板库不认识每个模块的 DbContext 类型：`ModuleMigrator.MigrateAllAsync` 按模块注册服务时登记的 `ModuleWriteContext` 逐个迁移；Operations、Identity 两个模块建好之前，测试用 `FrameworkSchema.CreateAsync` 建它们的框架表并授权。⑤`AddMediator` 在宿主里调用，管道行为的顺序与 05-02 第 4.1 节一致 | S2-17 实现 |
| 2026-11-12 | 第 4 节、第 7 节改为引用 Sample 中的真实文件（退出标准 4），不再抄代码；第 3.1 节目录树改为 Sample 的实际结构（决定不变）。对照真实实现发现并更正的差异：①D2 原定的两种“同文件”例外没有采用——文件名与类型名一致由 MA0048 检查，是错误级，例外要逐处豁免；请求模型与端点、内嵌 DTO 与命令各自单独一个文件（D2 选 A 不变，例外取消，规则更严）。②删除端点的并发版本在请求体 `expectedVersion`，不在查询参数里（与 05-06 D6 一致，原表写错）。③查询侧取单个对象的写法由示例中的 `FindByIdOrNotFoundAsync` 扩展方法改为注入 `IEntityFinder`（`Kmic.BuildingBlocks.Application.Querying`）：扩展方法拿不到安全事件服务和当前用户，范围外访问要记安全事件（05-05 第 5.10 节）需要这两样；找不到时补查是否被数据范围挡住，正常路径没有额外开销。④领域事件处理器由 `AddKmicApplication` 按程序集扫描自动注册，可以是 `internal`，不必在模块注册类里逐个登记。⑤第 7 节：处理器的集成测试与 API 测试同在 `Kmic.Api.Tests`、按模块分目录，不另建 `Kmic.<模块>.IntegrationTests` 项目（两者都需要完整宿主与迁移过的库，另建项目要重复一套宿主组装；ADR-0021 已定 API 测试集中在一个项目）；原定“模板为每个模块生成六类测试”改为：模型与迁移一致、契约快照、启动、唯一约束映射对宿主里全部模块遍历检查，授权矩阵与 `allowedActions` 一致性按资源写、Sample 给样板。⑥Sample 新增 `Close` 命令与端点（动作目录里有这个动作，一致性测试需要对应的命令）、`Sample.SpecimenSubmitted` 集成事件（演示领域事件 → 集成事件，并给契约快照一个真实对象）。第 4 节示例中框架 API 的确切签名的“待确认”关闭 | S2-18 |
