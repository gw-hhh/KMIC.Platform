# 原型 #1：Mediator 3 的管道、跨模块分发与执行上下文切换

- 状态：结论已得出；D1～D3 待决（2026-11-06）
- 来源：08 第 4 节原型 #1（阻断）；05-02 第 2.2、4.1 节，05-07 第 4 节
- 原型代码：分支 `spike/01-mediator` 的 `spikes/01-mediator/`（一次性代码，不合入主干，08 阶段 1）
- 环境：.NET SDK 10.0.403；Mediator.SourceGenerator / Mediator.Abstractions 3.0.2；xUnit v3 4.0.2

## 1. 原型的结构

| 项目 | 相当于正式系统中的 | 内容 |
|---|---|---|
| `Spike01.BuildingBlocks` | `Kmic.BuildingBlocks.*` | 05-02 第 2.1 节的消息、处理器、发送接口；`Result` / `IResultFactory`；四个管道行为（日志、校验、授权、事务）；`IContractQuerySender` 与执行上下文 |
| `Spike01.Customers` | 一个模块（示例：客户） | 带 `[ContractQuery(CallerScoped)]` 的查询及其处理器；Contracts 接口与薄适配器（05-07 D2） |
| `Spike01.Quotations` | 另一个模块（示例：报价） | 两种命令（有、无返回值）和一个查询；命令处理器中经 Contracts 调用客户模块 |
| `Spike01.Tests` | 宿主 `Kmic.Api` | 唯一引用 `Mediator.SourceGenerator` 的项目，`AddMediator` 在这里一次性声明管道顺序；6 个测试 |

示例中的“客户”“报价”只用来构造两个模块，不代表任何业务设计。

## 2. 结论

| # | 验证内容 | 结论 | 依据 |
|---|---|---|---|
| 1 | 带泛型约束的开放泛型行为 | **成立**。`where TMessage : IBaseCommand` 的事务行为只用于命令；`where TResponse : Result, IResultFactory<TResponse>` 的校验、授权行为能泛型地构造失败结果（有、无返回值两种 `Result` 都可以） | 测试：命令经过事务、查询不经过；校验失败返回带错误码的 `Result<Guid>` 且处理器未执行 |
| 2 | 管道顺序一次性声明 | **成立**。`AddMediator` 的 `PipelineBehaviors` 数组即执行顺序 | 测试断言完整的执行轨迹；临时把事务行为挪到授权之前，2 个测试按预期失败，还原后通过 |
| 3 | 跨多个模块程序集生成分发代码 | **成立，但与“处理器 `internal sealed`”冲突**，见 D1 | 生成器只在宿主中运行，生成的代码直接引用处理器类型（为了不用反射）；处理器为 `internal` 时报 CS0122，Mediator 没有绕过的配置项 |
| 4 | 模块程序集的扫描范围 | **默认扫描全部引用的程序集更安全**，见 D2 | `Assemblies` 列表漏掉一个模块时，编译通过、运行时才抛 `MissingMessageHandlerException`；不写 `Assemblies` 时，消息没有处理器在宿主编译期即报 MSG0005（警告即错误） |
| 5 | `IContractQuerySender` 中切换执行上下文 | **成立**。用 `AsyncLocal` 在调用期间把来源改为 `ModuleCall`，返回后恢复 | 测试：被调模块的授权行为与处理器看到 `ModuleCall`，调用方返回后仍是 `Http`；50 个并发作用域互不串扰 |
| 6 | Contracts 查询只能经模块调用执行（05-07 第 4.3 节） | **成立**。授权行为按 `[ContractQuery]` 拒绝 `Http` 来源直接发送 | 测试：直接发送返回 `Common.ContractQueryOnlyFromModuleCall`，处理器未执行 |
| 7 | 处理器生命周期 | **成立**。`ServiceLifetime = Scoped` 时处理器可注入作用域服务（将来的 DbContext）；`ValidateScopes`、`ValidateOnBuild` 均通过 | 测试宿主开启两项校验 |
| 8 | 业务代码不引用 Mediator 的类型（03 第 4.1 节） | **成立**。模块项目只引用 BuildingBlocks 的接口，不引用 `Mediator` 命名空间 | 原型模块源码 |

**总体结论**：Mediator 3.0.2 满足 05-02 的管道设计，不需要重新评估中介者选型（ADR-0005）。需要修订的只有处理器的可见性约定（D1），以及宿主注册写法的两处细节（D2、D3）。

## 3. 待决事项

### D1 处理器的可见性（待决）

05-02 第 2.2 节、06 第 5.1 节规定处理器 `internal sealed`，`InternalsVisibleTo` 只能指向本模块的测试项目。Mediator 在宿主中生成的分发代码必须能访问处理器类型，两条规则不能同时满足。

| 选项 | 做法 | 优点 | 缺点 |
|---|---|---|---|
| **A 处理器改为 `public sealed`** | Application 项目中的命令、查询处理器公开；架构测试改为“除 Mediator 生成的代码外，任何类型不得引用处理器类型” | 只放开处理器这一类；其他内部类型（仓储、DbContext、配置）仍为 `internal`；宿主和测试项目不需要特殊配置 | 处理器在 IDE 中对宿主可见，靠架构测试而不是编译器阻止直接调用 |
| B 保持 `internal`，`InternalsVisibleTo` 指向宿主 | 每个模块的 Application 项目对 `Kmic.Api` 公开内部类型；06 第 5.1 节的规则增加宿主这一例外 | 处理器类型的写法不变 | 宿主能看到模块的**全部**内部类型（仓储、DbContext、实体配置），暴露面比 A 大得多；每个需要运行管道的测试项目也要加入名单 |
| C 每个模块各自运行生成器 | 每个模块生成自己的分发类，框架按消息所属模块路由 | 处理器保持 `internal`，不需要例外 | Mediator 的设计是一个应用一个分发类，多个 `AddMediator` 会互相覆盖注册；管道声明要在每个模块重复；框架要自建路由；偏离库的正常用法，升级风险高 |

- **建议**：A。
- **理由**：
  - “处理器只能经管道调用”真正要防的是绕过管道直接调用处理器。其他模块本来就只能引用对方的 Contracts 项目（03 第 6 节，架构测试守护），看不到 Application 项目；能看到处理器的只有本模块的 Infrastructure、Api 项目和宿主，而端点只能注入 `ICommandSender` / `IQuerySender` 已有架构测试（05-06）。A 在这个基础上补一条“任何类型不得引用处理器”的架构测试，防护是完整的。
  - B 为了处理器一类，把模块的全部内部类型对宿主公开，违背 `internal` 的本意；C 的维护成本和升级风险都高。
- **取舍**：放弃编译器层面的“处理器不可见”，改由架构测试守护；处理器出现在宿主的类型提示中。
- **确认后要修订**：05-02 第 2.2 节约定表与示例、第 4.1 节“待确认”、第 11 节；06 第 5.1 节可见性表与第 15.4 节架构测试表；07 第 4 节示例（3 处）；05-08 第 6 节示例（1 处）。领域事件处理器、作业处理器、校验器、端点不经 Mediator 生成代码，保持 `internal sealed`。

### D2 宿主注册时不限定扫描范围（待决）

| 选项 | 做法 | 优点 | 缺点 |
|---|---|---|---|
| **A 不写 `Assemblies`** | 生成器扫描宿主引用的全部程序集 | 新增模块只需宿主引用其项目（本来就要）；消息缺少处理器在编译期报错 | 生成器扫描范围较大（只影响编译时间，原型中无可感知差别） |
| B 显式列出各模块 | `options.Assemblies = [typeof(模块标记类型), …]` | 范围明确 | 新增模块多一处要改的地方；漏写时编译通过、运行时才报错 |

- **建议**：A。理由：B 的漏写只在运行时暴露，与“问题在构建时暴露”的原则（06）相反。
- **取舍**：放弃对扫描范围的显式控制。

### D3 关闭 CA1716（待决）

CA1716 要求标识符不与任何 .NET 语言的关键字同名，目的是方便其他语言（如 VB）的使用者。05-01 的 `Error` 类型及 `Fail(Error error)` 参数与 VB 关键字 `Error` 同名，构建失败。

| 选项 | 做法 | 优点 | 缺点 |
|---|---|---|---|
| **A 全局关闭 CA1716** | `.editorconfig` 中设为 `none`，写明理由 | 05-01 的命名不变 | 失去一条对本项目没有作用的检查 |
| B 改名 | `Error` 改为如 `ResultError`，参数 `error` 改名 | 规则保留 | 05 系列、07 中大量示例改名；`Error` 是 05-01 已确认的核心类型，改名只为满足一条跨语言规则 |
| C 逐处豁免 | 每处 `#pragma` 加理由 | 规则保留 | `Error` 在全部模块中被大量使用，逐处豁免不可行 |

- **建议**：A。理由：本系统只用 C#，所有程序集不提供给其他语言使用，这条规则保护的场景不存在。
- **取舍**：如将来有程序集要给其他 .NET 语言使用，需要在该项目中重新开启。
- **确认后要修订**：06 第 15.3 节规则表加一行；`.editorconfig`（原型分支已改，确认后改入主干）。

## 4. 已直接修正的工程问题（不涉及设计）

原型第一次构建与运行测试时发现，S0-05 的工程基础文件在有测试项目后才会暴露的两个问题。已在主干修正：

| 问题 | 原因 | 修正 |
|---|---|---|
| 测试项目构建失败：RS0030 报 `TaskAwaiter<T>.GetResult()` | xUnit v3 自动生成的程序入口用 `GetAwaiter().GetResult()`，禁用 API 检查也检查生成的代码 | `Directory.Build.props` 的测试项目节定义 `XUNIT_GENERATED_DISABLE_WARNINGS`：xUnit 只在它自己生成的入口文件中关闭警告，不影响测试代码 |
| `dotnet test` 报“.NET 10 SDK 不再支持 VSTest 目标” | xUnit v3 4.x 基于 Microsoft.Testing.Platform（MTP），.NET 10 SDK 要求显式切换 `dotnet test` 的模式 | `global.json` 加 `"test": { "runner": "Microsoft.Testing.Platform" }`；`scripts/check.cs` 的测试命令改为 `dotnet test --solution Kmic.slnx --no-build` |

S0-05 的验证（S0-开工准备 第 4.2 节）只构建了测试项目、没有经 `dotnet test` 运行，所以当时没有发现。

## 5. 留到阶段 2 的事项

| 事项 | 说明 |
|---|---|
| 模块测试项目中运行管道 | 需要完整管道的测试项目（集成测试）同样要引用 `Mediator.SourceGenerator` 并调用 `AddMediator`；宜由测试基础设施提供一个共用的注册方法，管道顺序与宿主共用同一处声明 |
| 异常转换、幂等行为 | 原型未包含（它们依赖数据库，属于原型 #15、#16 的范围）；放在 Infrastructure 项目中，宿主引用后同样可以列入 `PipelineBehaviors` |
| 执行上下文的身份部分 | 原型只验证了来源（`Origin`）的切换；05-07 规定 Contracts 调用期间身份沿用调用方，身份本身不切换，不需要额外验证 |
