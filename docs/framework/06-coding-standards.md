# 06 编码规范

- 状态：已确认（2026-11-04，D1～D10 均按建议确定）；之后的修订见文末“修订记录”
- 日期：2026-11-04
- 适用范围：后端全部 .NET 项目（`src/`、`tests/`）与仓库级约定（编码、换行、提交）。前端编码规范随前端框架设计另写。
- 依据：ADR-0004、ADR-0008、ADR-0010、ADR-0011、ADR-0012、ADR-0016、ADR-0021、ADR-0030；03 第 4.6～4.8、4.10 节；05 系列各文档的“分析器 / 架构测试 / 代码评审”条目

## 编码速查

写代码时最常用到的 40 条，每条一行；完整规则、理由与检查手段以所列章节为准。示例中的 `Sample` 等名称均为**示例**。

**类型与结构**

| # | 规则 | 示例 | 章节 |
|---|---|---|---|
| 1 | 命令、查询、DTO、事件用 `sealed record` | `public sealed record SubmitSampleCommand(SampleId SampleId) : ICommand;` | 5.2 |
| 2 | 类默认 `sealed`、默认 `internal`；命令与查询处理器是 `public sealed` | `public sealed class SubmitSampleCommandHandler(...)`；`internal sealed class SubmitSampleEndpoint` | 5.1 |
| 3 | 服务类用主构造函数注入，参数不重新赋值 | `class SamplePolicy(TimeProvider time)` | 5.3 |
| 4 | 文件范围命名空间；一个类型一个文件 | `namespace Kmic.Sample.Domain;` | 3.2 |
| 5 | 必填属性用 `required`；不用 `!`（固定例外见 5.2）；集合不返回 `null` | `public required string Title { get; init; }` | 5.2 |
| 6 | 成员顺序：常量与静态字段 → 实例字段 → 构造函数与工厂 → 属性 → 方法 → 嵌套类型 | — | 5.4 |
| 7 | 方法参数一般不超过 4 个（不含 `ct`），更多时用参数对象 | `Search(SampleSearchOptions options, CancellationToken ct)` | 5.4 |
| 8 | 现代写法是强制的 | `new()`、`[]`、`is not null`、`switch` 表达式、`?? throw` | 3.4 |
| 9 | 总是写大括号；禁止嵌套三元；提前返回，嵌套不超过三层 | `if (sample is null) { return; }` | 3.4、A.7 |

**命名**

| # | 规则 | 示例 | 章节 |
|---|---|---|---|
| 10 | 标识符只用英文；新业务概念先登记术语表 | `docs/glossary.md` | 4.1 |
| 11 | 私有字段 `_camelCase`，静态只读字段也一样 | `_items`、`_machine` | 4.2 |
| 12 | 缩写：两个字母全大写，三个及以上只首字母大写 | `IO`、`Http`、`SampleId` | 4.2 |
| 13 | 异步方法以 `Async` 结尾；Mediator 处理器的 `Handle` 例外 | `GetForUpdateAsync` | 4.4 |
| 14 | 取消令牌名为 `ct`，放最后；实现 Mediator 等外部接口、重写基类方法时沿用原参数名 | `CancellationToken ct = default`；`Handle(SubmitSampleCommand command, CancellationToken cancellationToken)` | 4.2、7 |
| 15 | 类型后缀固定 | `…Command`、`…Query`、`…Handler`、`…Validator`、`…Endpoint`、`…Errors` | 4.3 |
| 16 | `Get…` 必须存在（找不到返回失败）；`Find…` 可能为空；`List…` 返回集合 | `FindByCodeAsync` 返回 `T?` | A.7 |

**错误、异步、依赖**

| # | 规则 | 示例 | 章节 |
|---|---|---|---|
| 17 | 业务失败返回 `Result`，错误来自错误目录；不抛业务异常 | `return SampleErrors.NoItems;` | 6 |
| 18 | 不捕获 `Exception`；参数检查用 `ThrowIfNull` 系列 | `ArgumentNullException.ThrowIfNull(sample);` | 6 |
| 19 | 全部 I/O 异步；禁止 `.Result`、`.Wait()`、`Task.Run`；不写 `ConfigureAwait(false)` | `await repository.GetForUpdateAsync(id, version, ct);` | 7 |
| 20 | 模块代码不用 `lock`、`SemaphoreSlim`；业务并发靠数据库 | 版本号、行锁、唯一约束 | 7 |
| 21 | 当前时间注入 `TimeProvider`；ID 用强类型 ID 的 `New()` | `time.GetUtcNow()`、`SampleId.New()` | 8、15.2 |
| 22 | 只用构造函数注入；配置注入 `IOptions<T>`，不注入 `IConfiguration`、`IServiceProvider` | `IOptions<SampleImportOptions> options` | 8 |
| 23 | 字符串比较写明 `StringComparison`；格式化写明 `InvariantCulture`；正则用 `[GeneratedRegex]` | `string.Equals(a, b, StringComparison.Ordinal)` | 9 |

**日志与注释**

| # | 规则 | 示例 | 章节 |
|---|---|---|---|
| 24 | 日志一律 `[LoggerMessage]`，模板用英文 | `Message = "Sample {SampleId} submitted"` | 10.1、10.2 |
| 25 | 注释用中文，写原因，不复述代码；不写设计文档的节号与 D 编号 | `// 取号放在最后一步：行锁要持有到事务结束` | 10.3、A.2 |
| 26 | 类型与成员的说明用 `/// <summary>`，三行，末尾不加句号 | 见 A.3 的格式 | 10.3、A.3 |
| 27 | 实体与 `Options` 的每个公开属性必须有 XML 注释，简短写出含义 | `/// 删除时间` | 10.3、A.3 |
| 28 | 行内注释先说决定再说原因，一句一个意思；不用术语和设计文档的说法 | `// 连接池耗尽时不重试：重试只会让排队更长` | A.5、A.6 |
| 29 | `TODO` 必须带编号；不留注释掉的代码 | `// TODO(#123): …` | 10.3 |
| 30 | 豁免只用带理由的特性，禁止 `#pragma` | `[SuppressMessage("…", "CA1031", Justification = "…")]` | 2.3 |

**数据访问**

| # | 规则 | 示例 | 章节 |
|---|---|---|---|
| 31 | 查询直接投影到 DTO；列表必须分页；不在循环中查询 | `query.Select(x => new SampleListItem(...))` | 11.1 |
| 32 | 手写 SQL 只用插值形式，标识符加双引号 | `FromSql($"""SELECT … FROM "Sample"."Sample" WHERE "Id" = {id}""")` | 11.1 |
| 33 | 文本匹配用框架函数，不用 `Contains`、`ToLower` | `KmicFunctions.ContainsText(x.Name, filter.Q)` | 11.2 |
| 34 | 不直接用 `ExecuteUpdate`、`ExecuteDelete`、`IgnoreQueryFilters()` | `IAtomicUpdate`、`IPurge`、`WithoutDataScope()` | 15.2 |
| 35 | 破坏性表结构变更分步发布：扩展 → 迁移 → 停写 → 收缩 | 列改名示例 | 11.3 |

**测试与提交**

| # | 规则 | 示例 | 章节 |
|---|---|---|---|
| 36 | 测试名用英文 `行为_条件_结果` | `Submit_WhenDraft_RaisesSubmittedEvent` | 12.2 |
| 37 | 时间用 `FakeTimeProvider`，不 `Task.Delay`；不替换仓储与 DbContext | `SampleBuilder.Draft().WithItems(3).Build()` | 12.3 |
| 38 | 提交信息：英文类型与范围 + 中文描述；分支名 ASCII | `feat(sample): 新增提交接口`；`feat/sample-submit` | 13.1 |
| 39 | 文件 UTF-8 无 BOM、LF；用过 `dotnet new`、`dotnet ef` 后规整一次 | `dotnet run scripts/check.cs -- --fix` | 3.2 |
| 40 | 提交前本机检查必须通过 | `dotnet run scripts/check.cs` | 13.1 |

---

## 0. 范围

| 本文内容 | 不在本文 |
|---|---|
| 规则如何落地（分析器、架构测试、评审）与豁免写法 | 各构件的设计（05 系列） |
| 工程配置、格式、命名、类型设计、异常、异步、依赖注入、字符串、日志、注释；注释与排版细则（附录 A） | 模块的目录结构、一个用例由哪些文件组成、各构件的标准示例（07 业务模块标准写法） |
| 05 系列明确“留给编码规范”的内容：模糊查询写法（ADR-0030）、分步迁移示例（05-03 第 9.2 节） | CI 流水线的具体配置（ADR-0032） |
| 测试编写规范、版本控制约定、代码评审清单、规则总表 | 前端（Vue / TypeScript）编码规范 |

### 0.1 本文决定的分类

| 类别 | 本文中的内容 |
|---|---|
| 第一类：现在确定 | D1～D10 全部：都影响第一行代码的写法，事后改要改全部代码 |
| 第二类：框架定写法，业务定用不用 | 无 |
| 第三类：延后 | 关键字模糊匹配的 trigram 索引（第 11.2 节），已登记到 00-README“延后事项”表 |

对照 00-README“延后事项”表：无命中。

### 0.2 本文与业务的关系

本文只规定**怎么写代码**，不规定任何业务内容。文中的 `Sample`、`SubmitSample`、`DisplayName` 等都是**示例**；术语表（D3）由框架规定位置和格式，**内容**由各业务模块设计时填写。

---

## 1. 旧项目怎么做 → 新框架怎么做

| 问题 | 旧项目 | 新框架 | 章节 |
|---|---|---|---|
| 规则靠自觉 | 名义三层，无任何机制阻止越层；页面直接写 SQL 约 1180 处 | 编译期约束 → 分析器 → 架构测试 → 评审，能自动检查的不靠评审；全部规则登记在规则总表 | 2、15 |
| 格式 | 无格式约束；GBK / UTF-8 混用 | `.editorconfig` + `dotnet format` 校验；UTF-8、LF 由 `.gitattributes` 固定 | 3 |
| 命名 | 拼音与英文混用，同一概念多种拼写（`Pingshen` / `pingshen`、`HeTong` / `Hetong`、`pruduct`） | 标识符只用英文，术语表统一译名，CI 拼写检查 | 4 |
| 魔法数字 | 权限 ID、状态值以数字硬编码（10224、`IsShenhe = -1/0/1/2`） | 智能枚举、权限点常量（05-01、05-05）；分析器禁止 | 4、15 |
| 共享可变状态 | `public static DataSet btnds` 所有用户共享 | 禁止可变静态字段（架构测试）；类型默认 `sealed`、不可变 | 5 |
| 异常 | ashx 普遍 `try/catch` 后返回 `{"code":1}` | `Result` 表达业务失败；模块代码不捕获通用异常；异常只在最外层记录一次 | 6 |
| 并发 | `static lock` 包住整个请求 | 全部 I/O 异步；禁止阻塞调用；多实例下的并发由数据库保证（05 系列） | 7 |
| 日志 | 文本文件、无级别、无结构 | 源码生成的日志方法、消息模板、数据分类脱敏（05-12） | 10 |
| 数据访问 | 4 套 `DbHelperSQL`，字符串拼接 SQL | 只用 EF Core；手写 SQL 必须参数化、标识符加引号 | 11 |
| 表结构变更 | 手工 SQL 脚本 | EF 迁移，破坏性变更“扩展 → 迁移 → 收缩” | 11.3 |
| 测试 | PowerShell 调 `csc.exe` 的自定义脚本，覆盖很少 | xUnit v3 + Testcontainers，统一命名与数据隔离 | 12 |
| 版本控制 | `development` 合并到 `main`；中文分支名；提交信息无格式 | 短分支 + 合并请求 + 压缩合并；提交信息固定格式 | 13 |
| 死代码 | 大量副本文件、测试页面 | 未使用代码由分析器报错；不提交副本文件 | 3、14 |

---

## 2. 规则如何落地

### 2.1 强度阶梯

ADR-0008：**能用更强手段检查的，不靠更弱的手段**。

```mermaid
graph LR
    A[编译期<br/>项目引用、可空、可见性] --> B[分析器<br/>内置 / Meziantou / BannedApi / 自定义 KM]
    B --> C[架构测试<br/>ArchUnitNET + 反射 + EF 模型]
    C --> D[启动校验<br/>用例目录、权限点、映射]
    D --> E[代码评审<br/>第 14 节清单]
```

| 手段 | 适用 | 失败时 |
|---|---|---|
| 编译期 | 依赖方向、可空、可见性 | 编译不过 |
| 分析器 | 单个文件 / 单个语法结构就能判断的规则 | 编译不过（全部 Error 级别，ADR-0021 零警告） |
| 架构测试 | 需要看整个程序集或跨程序集的规则（依赖、命名、类型关系） | CI 失败，阻断合并 |
| 启动校验 | 需要运行时组装结果才能判断的规则（用例目录、EF 模型） | 拒绝启动；同一套校验也在架构测试中执行 |
| 代码评审 | 语义层面、工具无法判断的规则 | 评审不通过 |

- 每条规则**只登记一次**，登记在第 15 节规则总表中，写明手段与来源文档。新增规则时同时补充总表和对应的测试（ADR-0008 影响）。
- 评审清单（第 14 节）只放工具无法检查的条目；某条一旦能用工具检查，就从清单移到总表。

### 2.2 自定义分析器编号

`KM` + 4 位数字，前两位表示范围。05-01 已使用 `KM01xx`～`KM05xx`。

| 编号段 | 范围 |
|---|---|
| `KM01xx`～`KM09xx` | Domain（05-01 已用 01～05） |
| `KM10xx`～`KM19xx` | Application |
| `KM20xx`～`KM29xx` | Infrastructure、数据访问 |
| `KM30xx`～`KM39xx` | Presentation |
| `KM90xx`～`KM99xx` | 全部项目通用 |

- 分析器项目：`src/BuildingBlocks/Kmic.BuildingBlocks.Analyzers`（`netstandard2.0`，Roslyn 要求），通过 `Directory.Build.props` 以分析器方式引用到全部项目。
- 分析器项目只能编译为 `netstandard2.0`，但代码同样用最新语言特性（质量原则）：项目中写 `<LangVersion>latest</LangVersion>`（`netstandard2.0` 默认是 C# 7.3），并引用 PolySharp（编译期为 `netstandard2.0` 补上 `record`、`init`、`required`、列表模式、范围运算符等需要的类型，不带进运行时，MIT）。另需 `AnalyzerReleases.Shipped.md` / `Unshipped.md` 登记规则编号（RS2000 系），诊断描述在字段上直接创建；细节见 `docs/framework/spikes/08-state-analyzer.md` 第 3 节。
- 每条规则有对应的分析器测试（`Microsoft.CodeAnalysis.Testing`），至少覆盖“触发”与“不触发”各一例。

### 2.3 D1 规则豁免怎么写

总有个别位置需要豁免某条规则（如框架基础设施中记录日志时间戳要用 `DateTimeOffset.UtcNow`）。豁免写法决定了“例外”是否可见、可审查。

| 选项 | 做法 | 优点 | 缺点 |
|---|---|---|---|
| A 不限制 | `#pragma warning disable`、`[SuppressMessage]`、`.editorconfig` 降级都可以 | 灵活 | `#pragma` 忘记 `restore` 会扩大范围；豁免理由不留痕；评审时容易漏看 |
| **B 只允许带理由的特性，禁止 `#pragma`** | 只能用 `[SuppressMessage("类别", "规则", Justification = "理由")]`，作用于最小范围（成员）；`Justification` 必填（KM9001）；`#pragma warning disable` 在非生成代码中禁止（KM9002）；多余的豁免由 IDE0079 报错 | 每个例外都有理由、范围最小、可以全文搜索出清单；规则失效后豁免不会残留 | 写法比 `#pragma` 长；个别规则需要整文件豁免时要逐个成员标注 |
| C 只允许在 `.editorconfig` 中按路径降级 | 例外集中在配置文件 | 例外集中 | 粒度是文件或目录，范围过大；与代码分离，读代码时看不到 |

- **决定（已确认，2026-11-04）**：B。另外两条配套：
  - `.editorconfig` 中调整规则级别，只能在仓库根目录的那一份中做；子目录不得放 `.editorconfig`（架构测试检查）。
  - 根 `.editorconfig`、`Directory.Build.props`、`Directory.Packages.props`、`BannedSymbols.txt`、架构测试项目、分析器项目列入 `CODEOWNERS`，修改须框架负责人审核（第 13 节）。
- **理由**：豁免本身就是需要评审的设计决定；B 让每个例外都带着理由出现在它生效的位置，并且可以一次列出全部例外。
- **取舍**：豁免写法略长。

---

## 3. 工程配置与格式

### 3.1 `Directory.Build.props`（仓库根目录）

| 设置 | 值 | 说明 |
|---|---|---|
| `TargetFramework` | `net10.0` | 分析器项目除外（`netstandard2.0`） |
| `Nullable` | `enable` | |
| `ImplicitUsings` | `enable` | 项目共用的 `using` 放各项目的 `GlobalUsings.cs` |
| `TreatWarningsAsErrors` | `true` | ADR-0021 零警告 |
| `MSBuildTreatWarningsAsErrors` | `true` | `TreatWarningsAsErrors` 只管编译器警告，程序集版本冲突（MSB3277）等 MSBuild 警告由它让构建失败 |
| `AnalysisLevel` | `latest-recommended` | 03 第 4.10 节 |
| `EnforceCodeStyleInBuild` | `true` | `IDExxxx` 代码风格规则在构建时生效，不只在 IDE 中提示 |
| `GenerateDocumentationFile` | `true` | IDE0005（多余的 `using`）在构建时生效的前提；CS1591（缺少 XML 注释）只在 `BuildingBlocks.*` 与模块 `Contracts` 中为 Error，其余项目关闭（第 10.3 节） |
| `Deterministic` / `ContinuousIntegrationBuild` | `true` / CI 中为 `true` | 可重现构建 |
| `ManagePackageVersionsCentrally` | `true` | 版本在 `Directory.Packages.props`（ADR-0021） |
| `CentralPackageTransitivePinningEnabled` | `true` | 传递依赖的漏洞修复可在中心统一提升版本 |
| 分析器引用 | Meziantou.Analyzer、BannedApiAnalyzers、`Kmic.BuildingBlocks.Analyzers` | 全部项目；测试项目使用单独的 `BannedSymbols.Tests.txt` |

### 3.2 编码、换行与文件

| 项 | 约定 | 落地 |
|---|---|---|
| 字符编码 | UTF-8，**无 BOM** | `.editorconfig`：`charset = utf-8`；CI 检查（旧系统 GBK 混用的教训） |
| 换行 | LF | `.gitattributes`：`* text=auto eol=lf`；`*.sh` 强制 LF；Windows 开发机检出后同样是 LF，IDE 均支持 |
| 文件末尾 | 有且只有一个换行 | `.editorconfig`：`insert_final_newline = true` |
| 缩进 | 4 个空格；JSON、YAML、`.csproj`、`.props` 用 2 个空格 | `.editorconfig` |
| 行宽 | 不设上限：一屏宽能显示完就写一行，需要横向滚动时再换行（评审时参考） | — |
| 命名空间 | 文件范围命名空间（`namespace Kmic.Sample.Domain;`），与文件夹路径一致 | IDE0161、IDE0130 → Error |
| 一个文件 | 一个顶层类型，文件名与类型名一致；例外由 07 规定（一个用例的命令、校验器、处理器是否同文件） | 评审 + 07 |
| 不提交的内容 | 副本文件（`*_bak.cs`、`* - 副本.cs`）、注释掉的大段代码、个人调试代码 | `.gitignore`；评审 |

- **工具生成的文件**（原型 #11 D3）：Windows 上的 `dotnet sln`、`dotnet new`、`dotnet ef` 等工具按 CRLF 写文件，EF 迁移还带 BOM。本节规则不开例外，用 `dotnet run scripts/check.cs -- --fix` 一次规整（去掉 BOM、CRLF 改为 LF），用过这些工具后运行一次；忘了也会被检查拦下。EF 迁移（`src/**/Migrations/*.cs`）在 `.editorconfig` 中标为生成代码，不受代码规则约束，人工审查生成的 SQL（05-03 第 9 节）。
- **与前端共用的部分**（F04）：前端（`web/`）与本文共用 D3（标识符英文 + 术语表 + cspell）、D7（文字语言按读者区分）、D10（版本控制）、本节的编码与换行、附录 A 的注释原则，具体见 F04 第 0.3 节。相应地：根 `.editorconfig` 有 `[web/**.{ts,vue,scss,json}]` 配置节（2 个空格缩进），仍只有根目录这一份（D1）；cspell 的检查范围包含 `web/src`、`web/tests`，词典共用；`CODEOWNERS` 包含前端检查配置（`web/eslint.config.ts`、Stylelint 配置、`web/tsconfig*.json`、`web/tools/eslint-plugin-kmic/`、组件库允许名单），修改须框架负责人审核（F04 D1）。前端的规则总表在 F04 第 14 节，不并入本文第 15 节。

### 3.3 D2 格式化工具

ADR-0021 已定 `.editorconfig` + `dotnet format --verify-no-changes`。这里确认是否再引入独立的格式化工具。

| 选项 | 做法 | 优点 | 缺点 |
|---|---|---|---|
| **A 只用 `dotnet format`** | 空白、缩进、`using` 顺序、代码风格规则由 `.editorconfig` 定义，CI 校验 | 官方工具，无额外依赖；与 IDE（VS / Rider / VS Code）的格式化一致；规则可逐条配置 | 不负责**折行**：长表达式、链式调用、参数列表怎么换行由人决定，仍会有少量风格差异 |
| B 再引入 CSharpier | 类似前端的 Prettier：整份文件按固定算法重排，几乎不可配置；`dotnet format` 只保留分析器风格规则 | 格式完全确定，评审中不再讨论格式 | 多一个工具与 IDE 插件；它的折行风格与部分 `.editorconfig` 规则需要协调；版本升级会引起大范围格式变化 |

- **决定（已确认，2026-11-04）**：A。
- **理由**：`dotnet format` 已能保证绝大部分格式一致；折行差异在评审中影响很小，不值得多一个会重排整份文件的工具。
- **取舍**：折行风格不完全统一。若实施后评审中格式讨论明显增多，再评估 B（可以随时引入，不影响已有代码的语义）。

### 3.4 `.editorconfig` 中的主要风格选择

| 规则 | 选择 |
|---|---|
| `var` | 类型从右侧显而易见时用 `var`；内置类型（`int`、`string`）写明类型 |
| `this.` | 不使用 |
| 私有字段 | `_camelCase` |
| 大括号 | 总是使用，即使只有一行（`csharp_prefer_braces = true`） |
| 表达式主体 | 属性、索引器、单行方法可以用；构造函数、多行逻辑不用 |
| 模式匹配 | 优先 `is null` / `is not null`、`switch` 表达式 |
| 集合表达式 | 优先 `[]`（IDE0300 系） |
| `using` 顺序 | `System` 在前，之后按字母顺序；不分组空行 |
| 字符串 | 优先插值字符串（日志除外，第 10.1 节） |

上表是写法偏好（建议级别），对真实代码的效果在阶段 2 复核；Meziantou 规则逐条确认级别（03 第 4.10 节取舍）。

**现代写法在构建时检查**（质量原则）：新写法更清楚、更简洁或更可靠的，写成旧写法即构建失败；大多可用 `dotnet format` 自动改正。

| 类别 | 规则（均为 Error） |
|---|---|
| 创建与初始化 | `new()`（IDE0090）、对象初始化器（IDE0017）、集合表达式（IDE0028、IDE0300～IDE0304）、自动属性（IDE0032）、`field` 访问器简化（IDE0360）、主构造函数（IDE0290，见第 5.3 节） |
| 模式匹配 | `is 1 or 2`、`is > 0 and < 10`（IDE0078）、`is not`（IDE0083）、`as` / `is` 加转换改为模式（IDE0019、IDE0020、IDE0038、IDE0260）、`is not null` 代替类型判断（IDE0150）、扩展属性模式（IDE0170）、`switch` 表达式（IDE0066） |
| 空值 | `??`（IDE0029、IDE0030）、`?.`（IDE0031）、`??=`（IDE0074）、`?? throw`（IDE0270）、throw 表达式（IDE0016） |
| 其他 | `using` 声明（IDE0063）、索引与范围运算符（IDE0056、IDE0057）、复合赋值（IDE0054）、简化插值与条件（IDE0071、IDE0075）、`nameof`（IDE0082）、元组交换（IDE0180）、方法组（IDE0200）、`readonly` 结构与成员（IDE0250、IDE0251）、`nameof(List<>)`（IDE0340，C# 14）、隐式类型 lambda（IDE0350） |

- **仍只作建议**：把 `if` 改成三元（IDE0045、IDE0046，容易写出嵌套三元，附录 A.7）；链式调用末尾的 `ToList()` 改成 `[.. ]`（IDE0305，长查询反而难读）。
- **已验证**（SDK 10.0.403，临时项目逐条用旧写法触发）：上表规则均实际报出；IDE0038、IDE0260 为同类规则的变体，未单独构造例子。实体的 `private` 构造函数不会被要求改成主构造函数。
- 参数判空时 IDE0016（throw 表达式）与 CA1510（`ThrowIfNull`）会同时报出，按第 6 节用 `ArgumentNullException.ThrowIfNull`，两条都消除。

---

## 4. 命名

### 4.1 D3 标识符用什么语言

旧系统的命名问题不在于“用了拼音”，而在于**同一个概念有多个名字**：`Pingshen` / `pingshen`、`HeTong` / `Hetong`，以及 `pruduct` 这样的拼写错误。搜索、重构、跨模块对照都因此失效。

| 选项 | 做法 | 优点 | 缺点 |
|---|---|---|---|
| **A 只用英文 + 术语表 + CI 拼写检查** | 业务概念的英文名统一登记在术语表；CI 用拼写检查工具检查标识符和注释中的英文单词，项目词典由术语表生成 | 同一概念只有一个名字，且有出处可查；拼写错误、拼音在合并前被拦下；与 .NET 生态、第三方库命名一致 | 少数中国特有的业务概念（如某些审批类型）需要商定译名；拼写检查需要维护项目词典 |
| B 只用英文 + 术语表，不做工具检查 | 同上，靠评审 | 少一个工具 | 拼写错误与临时起的新译名只能靠人看出 |
| C 允许难译的概念用拼音 | 术语表中标注哪些概念用拼音 | 不必为难译的词想译名 | 拼音与英文的边界无法用工具判断；旧系统正是从“少量拼音”开始变成混用 |

- **决定（已确认，2026-11-04）**：A。具体做法：
  - 术语表放在 `KMIC.Platform/docs/glossary.md`，格式：`中文术语 | 英文标识符 | 所属模块 | 含义 | 备注`。同一个中文词在不同模块含义不同时，分两行登记（模块是限界上下文，允许同名不同义，但必须写明）。
  - 新增业务概念时先登记术语表，再写代码；评审时对照。
  - 拼写检查工具：cspell（MIT，Node 运行；前端工具链已有 Node）。检查 `src/`、`tests/` 中的标识符与注释里的英文部分；项目词典 = 术语表英文列 + 技术词汇表。版本与许可见 `docs/implementation/S0-04-版本与许可.md`（cspell 10.4.0，MIT）。
  - 标识符只允许 ASCII 字符（KM9004），测试项目同样适用（见 D9）。
- **理由**：一个概念一个名字是可维护性的基础，也是 DDD“统一语言”的落地方式；只有工具检查才能在第 1000 个文件时仍然成立。
- **取舍**：要为少数中国特有的业务概念商定译名；拼写检查有一定误报，需要补词典。

### 4.2 C# 命名

遵循 .NET 官方命名规范，以下为补充与强调。

| 对象 | 规则 | 示例 |
|---|---|---|
| 类型、方法、属性、事件、常量 | PascalCase | `SampleStatus`、`MaxItems` |
| 接口 | `I` + PascalCase | `INumberGenerator` |
| 参数、局部变量 | camelCase | `sampleId` |
| 私有字段 | `_camelCase`；`static readonly` 也用 `_camelCase` | `_items` |
| 泛型参数 | `T` 或 `T` + 描述 | `TId`、`TResponse` |
| 缩写 | 两个字母全大写（`IO`），三个及以上只首字母大写（`Http`、`Api`、`Sql`、`Id`） | `HttpClient`、`SampleId` |
| 布尔 | `Is` / `Has` / `Can` / `Should` 开头 | `IsDeleted`、`CanSubmit` |
| 取消令牌参数 | `ct`，放在最后（与 05 系列一致）。实现外部接口（Mediator 的处理器和管道行为、EF 的拦截器）或重写基类方法时，参数名沿用接口和基类的写法，否则 CA1725 报错 | `CancellationToken ct = default`；处理器 `Handle(SubmitSampleCommand command, CancellationToken cancellationToken)` |
| 命名空间 | `Kmic.<模块>.<层>[.<子目录>]` | `Kmic.Sample.Application.Submit` |
| 数据库对象 | 与 C# 同名，大驼峰，手写 SQL 加双引号（ADR-0012 第 1 条） | `"Sample"."SampleItem"` |
| 配置节 | 模块 `Modules:<模块>:<用途>`；框架 `Kmic:<能力>`（05-10 第 2.1 节） | `Modules:Sample:Import` |
| 权限点、错误码、消息类型名 | 以模块名开头（05-05、05-01、05-07 已定） | `Sample.Submit`、`Sample.NotFound` |

### 4.3 类型后缀汇总

05 系列已分别定下的后缀，这里集中列出（检查手段见第 15 节）：

| 类型 | 后缀 / 形式 | 来源 |
|---|---|---|
| 命令 | 动词开头 + `Command`（`SubmitSampleCommand`） | 05-02 |
| 查询 | `Get` / `List` / `Search` 开头 + `Query` | 05-02 |
| 命令 / 查询处理器 | 消息名 + `Handler`（`SubmitSampleCommandHandler`） | 05-02 |
| 校验器 | 消息名 + `Validator` | 05-02 |
| 端点 | `Endpoint` | 05-06 |
| 领域事件 | 过去时，无后缀（`SampleSubmitted`） | 05-01 |
| 集成事件 | `{聚合}{过去时}`，无后缀，定义在 Contracts（与同名领域事件以命名空间区分）；快照事件 `{聚合}Changed` | 05-07 第 3.3 节 |
| Contracts 接口 | `I{模块}References`、`I{模块}{用途}Queries`、`I{聚合}SnapshotSource`；快照 DTO `{聚合}Snapshot` | 05-07 第 3.3 节 |
| 错误目录 | `<聚合或模块>Errors`（静态类） | 05-01 |
| 强类型 ID | `Id`；跨模块引用 `Ref` | 05-01 |
| 资源策略 / 业务策略 / 计算策略 | `Policy` / `Policy` / `Calculator` | 05-01、05-05 |
| 领域服务 | `Service` 或 `Calculator` | 05-01 |
| 响应 DTO | `Response`（单个）、`Item`（列表行）、`Dto`（嵌套） | 07 统一示例 |
| 选项类 | `Options` | 05-10 |
| 后台作业处理器 | 作业参数类名 + `Handler`（`ImportSamplesHandler`） | 05-08 |
| 测试类 | 被测对象 + `Tests` | 第 12 节 |

### 4.4 D4 异步方法是否加 `Async` 后缀

05 系列的接口签名中，自有方法有的不带后缀（`INumberGenerator.Next`、`IFileStorage.OpenRead`），同时会调用大量带后缀的 BCL / EF 方法（`ToListAsync`）。需要统一。

| 选项 | 做法 | 优点 | 缺点 |
|---|---|---|---|
| **A 自有异步方法一律加 `Async`** | 返回 `Task` / `ValueTask` / `IAsyncEnumerable` 的方法以 `Async` 结尾；由第三方接口规定名称的实现（Mediator 的 `Handle`、`BackgroundService.ExecuteAsync`）按接口原名 | 符合 .NET 官方指南；与 BCL、EF Core、ASP.NET Core 一致，同一段代码中风格统一；分析器可强制（Meziantou MA0137 / MA0138） | 名字长一点；Mediator 处理器的 `Handle` 是例外 |
| B 都不加 | 所有 I/O 都是异步，后缀不提供信息 | 名字短 | 自有方法与 BCL / EF 方法在同一行中风格不一致；与官方指南相反，新成员与第三方代码示例都按 A 写 |

- **决定（已确认，2026-11-04）**：A，启用 MA0137（返回可等待类型的方法须以 `Async` 结尾）与 MA0138（不返回可等待类型的方法不得以 `Async` 结尾），接口实现与重写自动随接口名称。05 系列中的接口签名已统一修订（如 `Next` → `NextAsync`、`OpenRead` → `OpenReadAsync`）。
- **理由**：“流行规范”在这里就是官方指南，且整个生态都遵守；代码中最多的异步调用是 EF 和 BCL 的方法，自有方法与之一致最自然。
- **取舍**：名称稍长；Mediator 的 `Handle` 保留例外。
- 例外的边界：只有**继承自 Mediator 接口**的命令 / 查询处理器保留 `Handle`；框架自定义的处理器接口按规则加后缀——`IDomainEventHandler.HandleAsync`、`IJobHandler.ExecuteAsync`、`IStartupTask.ExecuteAsync`、端点的处理方法 `HandleAsync`、发送入口 `ICommandSender.SendAsync`。修订清单见 05-02、05-03、05-05～05-08、05-10、05-11、05-13 中的接口定义（2026-11-04）。

---

## 5. 类型与成员设计

### 5.1 可见性与继承

| 规则 | 落地 |
|---|---|
| 默认 `internal`；只有其他项目确实需要使用的类型才 `public` | 评审；IDE0051 / IDE0052 报告未使用的私有成员 |
| 类默认 `sealed`；需要被继承的基类（主要在 BuildingBlocks）显式写 `abstract` | CA1852（内部类型）、MA0053（公开类型）→ Error |
| 命令、查询处理器 `public sealed`：Mediator 在宿主中生成的分发代码要直接引用它们（05-02 第 2.2 节） | 架构测试：除 Mediator 生成的代码外，任何类型不得引用处理器类型 |
| 端点、校验器、EF 配置类、领域事件处理器、作业处理器 `internal sealed` | 架构测试（05-02、05-06 已定） |
| 模块中禁止 `protected` 成员（没有继承就不需要） | 评审 |
| `InternalsVisibleTo` 只用于本模块的测试项目 | 架构测试 |

### 5.2 不可变与可空

| 规则 | 说明 |
|---|---|
| 命令、查询、DTO、事件用 `sealed record` | 05-01、05-02、05-07 已定 |
| DTO 必填属性用 `required`，可选属性显式可空 | 编译期发现漏赋值（手写映射的保障，03 第 4.8 节） |
| 禁止 `!`（空包容运算符），除非紧接一行注释说明为什么不可能为空 | 评审。以下几类是固定例外，不必写注释：EF 导航属性和只给 EF 用的构造函数里的 `= null!` / `= default!`；反射取已知存在的成员或已经筛过的结果（如 `GetMethod(nameof(Bind))!`、确认是数组后的 `GetElementType()!`、`Activator.CreateInstance(...)!`）；建模时已经校验过的 EF 模型元数据；测试代码 |
| 方法返回集合时不返回 `null`，返回空集合 | 评审 |
| 公开集合类型用 `IReadOnlyList<T>` / `IReadOnlyCollection<T>` / `IReadOnlyDictionary<,>` | 实体由 KM0105 强制；其余评审 |
| 不使用 `dynamic`；`object` 只在框架序列化等少数位置使用 | BannedApi（`dynamic` 由 KM9006 检查） |
| 可变静态字段禁止 | 架构测试（05-10 第 4.5 节） |
| 魔法数字：除 0、1、-1 外，业务含义的数值必须是命名常量、智能枚举或配置 | 评审；状态与权限分别由智能枚举、权限点常量承载 |

### 5.3 D5 是否使用主构造函数注入依赖

C# 12 的主构造函数可以省去“字段 + 构造函数赋值”的样板代码，但它的参数**不是只读字段**，可以在类中被重新赋值。

| 选项 | 做法 | 优点 | 缺点 |
|---|---|---|---|
| **A 服务类允许，配合分析器禁止重新赋值** | 处理器、端点、校验器、基础设施服务可以用主构造函数注入；领域实体、值对象、智能枚举不适用（它们的构造函数必须私有，05-01） | 样板代码减少约三分之一；.NET 官方模板与新文档普遍采用 | 参数不是 `readonly`，需要分析器补上；参数在类中到处可见，读代码时不如字段显式 |
| B 禁止，统一 `private readonly` 字段 | 传统写法 | 只读由语言保证；风格与旧代码库一致 | 每个服务多一组字段和赋值，处理器中尤其明显 |

- **决定（已确认，2026-11-04）**：A。配套规则：主构造函数参数不得被重新赋值（KM9005）；CS9124（参数既被捕获又用于初始化字段）→ Error；IDE0290（使用主构造函数）→ Error。
- **理由**：注入依赖是这些类的唯一用途，主构造函数恰好表达这件事；唯一的风险（被重新赋值）可以用一条很简单的分析器规则消除。
- **取舍**：需要维护一条自定义分析器规则。
- IDE0290 为什么是错误而不是建议（质量原则）：建议级别是为了不强制改写旧代码，新项目没有旧代码，统一写法更重要。只有构造函数仅做参数赋值时才会报出，需要校验或私有构造函数（05-01 的实体）的不受影响（已验证，见第 3.4 节）。

### 5.4 其他约定

| 项 | 约定 |
|---|---|
| 扩展方法 | 只用于框架的流式配置（`IServiceCollection`、EF 配置）与 LINQ 辅助；业务逻辑不写成扩展方法 |
| 区域 | 不使用 `#region` |
| 参数数量 | 方法参数一般不超过 4 个（不含 `CancellationToken`），更多时改用参数对象（`record`）；主构造函数注入的依赖不计入。由评审判断，不强制 |
| 方法长度 | 不设硬性上限；处理器超过约 60 行时，评审中说明是否应把规则下沉到领域对象 |
| 反射 | 模块代码中不使用；框架的扫描注册在启动时一次完成 |

**类型内成员的顺序**，由评审检查（不引入 StyleCop.Analyzers：该库长期停在预览版，为一类规则多一个依赖不值得）：

| 顺序 | 成员 |
|---|---|
| 1 | 常量、静态只读字段 |
| 2 | 实例字段 |
| 3 | 构造函数；静态工厂方法（如 `New()`、`Create(...)`）紧随其后 |
| 4 | 属性 |
| 5 | 方法：公开 → 内部、受保护 → 私有；运算符与显式接口实现放在方法之后 |
| 6 | 嵌套类型 |

- 同一类成员中，相关的放在一起（如一个公开方法与它专用的私有方法相邻时，私有方法仍按第 5 条放到公开方法之后）。
- 主构造函数没有单独的位置，参数写在类型声明上。
- 空行：带注释的成员之间空一行；没有注释的同类字段之间不空行。

---

## 6. 异常与错误

依据 ADR-0010、05-01 第 2 节、05-12 第 2.4 节，汇总为写代码时的规则：

| 规则 | 落地 |
|---|---|
| 业务失败返回 `Result` 失败，错误来自错误目录；不用异常表达业务失败 | KM0101、KM0102 |
| 异常只表示程序缺陷或基础设施故障 | — |
| 模块代码不捕获 `Exception` / `SystemException`；需要处理特定异常时捕获具体类型 | CA1031 → Error（BuildingBlocks 的异常转换、未处理异常中间件、后台循环用 `[SuppressMessage]` 豁免并写理由） |
| 重新抛出用 `throw;` | CA2200 → Error |
| 参数检查用 `ArgumentNullException.ThrowIfNull`、`ArgumentException.ThrowIfNullOrWhiteSpace`、`ArgumentOutOfRangeException.ThrowIfNegative` 等 | CA1510～CA1513 → Error |
| 不 `catch` 后记录日志再抛出；一个错误只在最外层记录一次 | 05-12 第 2.4 节；评审 |
| 不用异常做流程控制（如 `try { Parse } catch` 判断格式），用 `TryParse` 或 `Result` | 评审 |
| 自定义异常类型只在 BuildingBlocks 中定义 | 架构测试：模块程序集中不得有继承 `Exception` 的类型 |

---

## 7. 异步与取消

| 规则 | 落地 |
|---|---|
| 所有 I/O（数据库、HTTP、文件、缓存）用异步 API | CA1849、MA0042 → Error |
| 禁止 `.Result`、`.Wait()`、`.GetAwaiter().GetResult()` | BannedApi（`Task.Wait`、`Task.Result`、`GetResult`） |
| 禁止 `async void`（事件处理器也不例外，本系统没有 UI 事件） | VSTHRD100 不引入；MA0155 → Error |
| 取消令牌一路传递到最底层调用 | CA2016、MA0040 → Error |
| 取消令牌参数放在最后，名为 `ct`，公开方法给默认值 `= default` | CA1068 → Error |
| 不调用 `ConfigureAwait(false)`：本系统只运行在 ASP.NET Core / Generic Host 中，没有同步上下文 | CA2007、MA0004 关闭 |
| 返回类型：Mediator 处理器按库要求 `ValueTask`；其他默认 `Task`，只有在高频且大多同步完成的路径（如缓存命中）才用 `ValueTask` | CA2012 → Error（`ValueTask` 只能等待一次） |
| 请求路径中不使用 `Task.Run`；需要后台执行的工作走后台作业（05-08） | BannedApi（模块代码中禁用 `Task.Run`、`new Thread`、`ThreadPool.QueueUserWorkItem`） |
| 大量数据流式处理用 `IAsyncEnumerable<T>`（导出等），不一次性加载到内存 | 评审 |
| 不在模块代码中使用 `lock`、`SemaphoreSlim` 处理业务并发；业务并发靠数据库（版本、行锁、唯一约束） | 架构测试：模块程序集中不得出现 `System.Threading.Lock`、`Monitor`、`SemaphoreSlim`、`Mutex` 的使用（旧系统 `static lock` 的教训） |

---

## 8. 依赖注入

| 规则 | 落地 |
|---|---|
| 只用构造函数注入（含主构造函数）；不用属性注入 | — |
| 模块代码不注入 `IServiceProvider`、不调用 `GetService` / `GetRequiredService`（服务定位） | BannedApi（BuildingBlocks 与宿主豁免） |
| 注册只在模块的 `IModule.AddServices` 中进行；处理器、校验器、端点自动发现（ADR-0021 第 2 节） | 架构测试：模块中其他位置不得调用 `IServiceCollection` 的注册方法 |
| 生命周期：无状态服务 `Scoped` 或 `Singleton`；依赖 `DbContext`、`ICurrentUser` 的一律 `Scoped` | 所有环境开启 `ValidateScopes` 与 `ValidateOnBuild`，捕获“单例持有作用域服务” |
| 选项通过 `IOptions<T>` / `IOptionsMonitor<T>` 注入，不注入 `IConfiguration` | BannedApi（05-10 已定） |
| 当前时间注入 `TimeProvider`，ID 生成用强类型 ID 的 `New()` | BannedApi（05-01、05-13 已定） |
| 一个类注入的依赖超过 6 个时，评审中说明是否职责过多 | 评审 |

---

## 9. 字符串、文化与格式

旧系统的日期以多种格式的字符串存储、解析依赖服务器区域设置。新系统中机器之间交换的字符串必须与区域设置无关。

| 规则 | 落地 |
|---|---|
| 字符串比较显式指定 `StringComparison`；标识、编码、键用 `Ordinal` / `OrdinalIgnoreCase` | CA1307、CA1309、MA0006 → Error |
| 数值、日期的格式化与解析显式指定 `CultureInfo.InvariantCulture`（日志、缓存键、文件名、外部接口） | CA1304、CA1305、MA0011 → Error |
| 面向用户的数字、日期格式由前端负责；后端接口只输出 05-06 规定的机器格式 | 05-06 第 5 节 |
| 大小写转换用 `ToUpperInvariant` / `ToLowerInvariant` | CA1308 关闭（不强制大写），MA0011 覆盖 |
| 正则表达式用 `[GeneratedRegex]`，并设置超时 | MA0009、SYSLIB1045 → Error |
| 拼接大量字符串用 `StringBuilder` 或 `string.Join`；SQL 不拼接（第 11.1 节） | 评审 |

---

## 10. 日志、注释与语言

### 10.1 D6 日志是否一律使用源码生成的日志方法

05-12 D2 已定：参数按数据分类脱敏依赖 `[LoggerMessage]`，“高频日志必须写成源码生成的方法”。这里确定“非高频”的日志是否也必须如此。

| 选项 | 做法 | 优点 | 缺点 |
|---|---|---|---|
| **A 一律使用 `[LoggerMessage]`** | 每个类的日志方法集中在该类的 `partial` 部分或同目录的 `Log` 静态类；CA1848 → Error | 规则只有一条，不需要判断“是否高频”；每条日志有固定的 `EventId` 与名称，可在观测平台中按事件查询和告警；参数个数、名称在编译期检查；脱敏标注（05-12）总能生效 | 每条日志多写几行声明 |
| B 只有高频与带敏感参数的日志使用 | 其他日志用 `logger.LogInformation("…", args)` | 写得快 | “是否高频”“是否敏感”需要每次判断，判断错就漏脱敏；两种写法并存 |

- **决定（已确认，2026-11-04）**：A，测试项目除外。`EventId` 编号按模块分段（每个模块 1000 个，段号表随模块注册登记，07 给出示例）。框架代码（BuildingBlocks 与宿主）使用 1000～9999，按项目分段：Application `1000`～`1999`、Infrastructure `2000`～`2999`、Presentation `3000`～`3999`、宿主 `9000`～`9999`（与分析器编号段的划分一致；阶段 2 实现时补充）。
- **理由**：05-12 的脱敏可靠性建立在“日志参数有类型声明”上；只要允许第二种写法，这个前提就有漏洞。
- **取舍**：日志声明代码多一些。

```csharp
// 示例
internal static partial class SampleLog
{
    [LoggerMessage(EventId = 12001, Level = LogLevel.Information, Message = "Sample {SampleId} submitted")]
    public static partial void SampleSubmitted(this ILogger logger, SampleId sampleId);
}
```

### 10.2 D7 注释、提交信息、日志与异常消息用什么语言

D3 已定标识符用英文。其余文字需要统一。

| 文字 | A 全部中文 | B 全部英文 | **C 按读者区分** |
|---|---|---|---|
| 代码注释、XML 文档注释 | 中文 | 英文 | 中文 |
| 提交信息、合并请求说明 | 中文 | 英文 | 中文描述；类型与范围用英文关键字（D10） |
| 日志消息模板 | 中文 | 英文 | 英文 |
| 程序缺陷类异常的消息（`InvalidOperationException` 等） | 中文 | 英文 | 中文（2026-11-11 修订，原为英文） |
| 面向用户的提示（错误目录、校验消息） | 中文 | — | 中文（05-01 D3 已定，将来按资源文件多语言） |
| 文档 | 中文 | 英文 | 中文（CLAUDE.md 已定） |

| 选项 | 优点 | 缺点 |
|---|---|---|
| A 全部中文 | 团队阅读最快；规则最简单 | 日志与 .NET、EF、Npgsql 等库的英文日志混排，同一个事件在查询中要用两种语言的关键字；日志模板作为事件标识（10.1）时，中文在部分终端、日志工具中的显示与检索不稳定 |
| B 全部英文 | 与生态完全一致 | 团队写注释与提交信息的成本高，写出来的注释质量下降；与中文文档、中文业务术语脱节 |
| **C 按读者区分** | 给人读、解释“为什么”的文字用中文，写得清楚；机器处理、与库输出混排的文字用英文，查询一致 | 规则多一条：要记住日志和异常用英文 |

- **决定（已确认，2026-11-04）**：C。
- **修订（用户确认，2026-11-11）**：程序缺陷类异常的消息改为中文。这些异常几乎都在开发时触发（声明写错、启动校验、用错框架），看的人是开发者，用中文说清哪里错、怎么改更直接；生产中出现时按异常类型和堆栈查找，不靠消息全文检索。日志模板仍用英文。
- **理由**：注释和提交信息的价值在于把意图说清楚，母语写得最准；日志模板与异常消息本质上是给工具检索的事件标识，和库的日志保持同一种语言才能统一查询与告警。
- **取舍**：开发者写日志和异常时需要切换语言（这两类文字都很短）。

### 10.3 注释写法

| 规则 | 说明 |
|---|---|
| 注释解释**为什么**，不复述代码做了什么；具体写法见附录 A《注释与排版细则》 | 评审 |
| 类型、成员要写说明时一律用 `/// <summary>`（不论公开还是内部，格式按附录 A.3），不用 `//` 写类型说明 | `scripts/check.cs`：`summary` 的开始、结束标签各占一行；注释末尾不加句号（多行注释看最后一行；只检查整行注释） |
| 必须写 XML 注释 ①：`BuildingBlocks.*` 与模块 `Contracts` 的公开类型和成员 | CS1591 → Error（只在这些项目中） |
| 必须写 XML 注释 ②：出现在 OpenAPI 中的请求模型、响应 DTO、枚举及其成员，作为接口文档的字段说明 | CI 检查生成的 OpenAPI：操作摘要、参数、字段、枚举值缺少说明即失败（05-06 第 7.1 节） |
| 必须写 XML 注释 ③：领域实体与配置类（`Options`）的类型本身和每个公开属性，简短写出含义（如“创建时间”“删除时间”“截止日期”），不补充空值含义、时区这类细节；只有名称和类型表达不了的单位才写（如整数“超时时间（秒）”，`TimeSpan` 不写）。数据库列注释默认由它生成（05-03 第 3.9 节） | 阶段 2 由自定义分析器在编译期检查，此前由评审检查（第 14 节） |
| 注释直接写原因，默认不引用文档；什么时候附 ADR 编号见下文“引用” | 评审 |
| `TODO` 必须带跟踪编号：`// TODO(#123): …`；没有编号的 `TODO` 报错 | KM9003 |
| 不保留注释掉的代码（版本控制里有历史） | 评审 |

**引用**

- 长期注释：只有写法反直觉、容易被人“优化”掉、而且原因一两句说不完时，才在原因后附 ADR 编号，如 `// 取号放在最后一步：行锁要持有到事务结束，提前取号会让并发提交互相等待（ADR-0024）`。这类 ADR 引用长期保留。
- 设计文档（01～09、05-xx、F0x）的节号与 `D` 编号不写进注释、异常消息和 `Justification`：读代码的人不需要知道文档结构，而且文档改版后节号会变。要对照设计文档时，写在提交说明或实施任务清单里。`scripts/check.cs` 检查节号、带文档号的 `D` 编号、括号中的文档号、原型编号与实施任务号，后端 `.cs` 与前端 `.ts`、`.vue` 中出现即失败。
- 阶段 2 前期允许过的行末临时引用【引用：…】已全部删除（2026-11-11，实施待办 T-17），不再使用，`scripts/check.cs` 同样拦截。

---

## 11. 数据访问写法补充

查询、映射、过滤器等设计见 05-03、05-05。本节只补充写代码时的规则，以及 05 系列明确交给编码规范的两项内容。

### 11.1 查询与手写 SQL

| 规则 | 落地 |
|---|---|
| 查询处理器用 LINQ 投影到 DTO（`Select`），不加载实体再映射 | 评审；05-03 已定查询上下文默认不跟踪 |
| 列表必须分页（05-06 第 6 节）；不写无上限的 `ToListAsync()` | 评审；慢请求日志（05-12） |
| 不在循环中查询数据库或调用 Contracts（N+1）；先收集 ID 再一次查询 | 评审（05-07 第 4 节） |
| 手写 SQL 只用 `FromSql` / `SqlQuery` / `ExecuteSql` 的插值形式（自动参数化）；禁止 `FromSqlRaw` / `SqlQueryRaw` / `ExecuteSqlRaw` | BannedApi（BuildingBlocks 豁免）；EF1002 → Error |
| 手写 SQL 中表名、列名、schema 名一律加双引号 | 评审（ADR-0012 第 1 条） |
| 手写 SQL 查询带归属的表必须使用 `IDataScopeSql`；查询软删除的表必须自行加条件 | 评审 + 架构测试提示（05-05 第 5.7 节、ADR-0012 第 3 条） |
| 不在 `WHERE` 中转换列（时区转换、函数包裹），以免无法使用索引 | 评审（05-13 第 3.4 节） |
| 查询表达式中不使用 `string.ToLower` / `ToUpper` / `Contains` / `StartsWith` / `EndsWith` 做文本匹配，改用 11.2 的框架函数 | KM2001 |

### 11.2 模糊匹配的标准写法（ADR-0030）

列表的关键字参数 `q`（05-06 第 6.4 节）需要在若干文本列上做“包含”匹配。两类列的情况不同：

| 列的类型 | 等值比较 | 包含匹配 |
|---|---|---|
| 默认列（区分大小写；绝大部分中文字段） | `x.Name == value` | 用框架函数 |
| `.IsCaseInsensitive()` 列（账号、邮箱、编码） | `x.Code == value`，自动不区分大小写，唯一索引同样生效 | 用框架函数 |

框架提供一个 EF 函数，所有模糊匹配都用它，不分列的类型：

```csharp
// BuildingBlocks.Infrastructure（EF 函数翻译）+ BuildingBlocks.Application（声明）
public static class KmicFunctions
{
    // 不区分大小写的“包含”匹配；term 中的 % _ \ 自动转义
    public static bool ContainsText(string? column, string term);

    // 不区分大小写的前缀匹配（如按编码开头查找），翻译方式相同
    public static bool StartsWithText(string? column, string term);
}

// 示例：查询处理器
query = query.Where(x =>
    KmicFunctions.ContainsText(x.Name, filter.Q) ||
    KmicFunctions.ContainsText(x.Code, filter.Q));
```

- 翻译为：`("Col" COLLATE "C") ILIKE @pattern ESCAPE '\'`，`@pattern` 为 `%` + 转义后的 term + `%`（原型 #5 已验证，`docs/framework/spikes/05-text-matching.md`）。
  - 显式指定确定性排序规则 `"C"`：非确定性排序规则的列不支持 `ILIKE`，统一转换后两类列写法相同。PG 18 在非确定性排序规则下支持 `LIKE`，但翻译仍统一用这一种。
  - `"C"` 下 `ILIKE` 只对 ASCII 字母做大小写折叠，中文不受影响；对编码、账号、邮箱这类 ASCII 字段正好合适。
- 实现为 EF 的方法翻译器插件。**转义在数据库端完成**（`'%' || replace(replace(replace(@term, '\', '\\'), '%', '\%'), '_', '\_') || '%'`）：翻译器拿到的是参数表达式，不能在程序中改写参数值。按实际参数值做计划时 trigram 索引照样可用。
- 翻译方式以后可以调整，**调用方写法不变**——这正是统一用框架函数的原因。
- 不使用 PG 的正则匹配与 `SIMILAR TO`。

**索引（延后事项）**：`'%词%'` 形式的匹配不能使用 B 树索引。小表全表扫描即可；某个列表的关键字搜索在数据量增长后变慢时，再为该列建 `pg_trgm` 的 GIN 表达式索引，索引表达式必须与上面的翻译完全一致（`("Col" COLLATE "C") gin_trgm_ops`）。由于查询写法已经固定，加索引时不改代码。本条已登记到 00-README“延后事项”表：

| 事项 | 触发条件 | 已定的约束 |
|---|---|---|
| 关键字模糊匹配的 trigram 索引（`pg_trgm`） | 某列表关键字搜索在慢请求日志中持续出现 | 查询只能用 `KmicFunctions.ContainsText` / `StartsWithText`；索引表达式与其翻译一致；扩展由迁移账号在迁移中创建 |

### 11.3 破坏性变更的分步示例（05-03 第 9.2 节）

前提：滚动更新时新旧两个版本的应用会同时运行几分钟（NFR-AVAIL），所以**任何一次发布后的表结构，都必须同时适用于这一版和上一版的代码**。

**示例一：列改名（`Sample.Name` → `Sample.DisplayName`，旧列原为 `NOT NULL`、无默认值）**

| 发布 | 迁移（表结构） | 代码 | 与上一版代码能否共存 |
|---|---|---|---|
| 1 扩展 | 新增可空列 `DisplayName`；按主键分批回填 `DisplayName = Name` | 属性仍映射旧列 `Name`（**读旧列**）；`.MirrorTo("DisplayName")` 保存时同步写新列 | 上一版只读写 `Name`，本版读的也是 `Name` → 可以。上一版在滚动期间新增、修改的行，新列为空或是旧值，由发布 2 的再次回填补齐 |
| 2 切换读取 | 再次回填；`DisplayName` 改为非空；**去掉 `Name` 上会拒绝“不写这一列”的约束**（非空、CHECK 等） | 属性改映射新列 `DisplayName`（**读新列**）；`.MirrorTo("Name")` 仍写旧列 | 两版都写两列：上一版读 `Name`，本版读 `DisplayName`，读到的都是最新值 → 可以。去掉 `Name` 的非空约束对两版都没有影响 |
| 3 停写 | 去掉映射后，迁移生成器会生成删除 `Name` 的操作，**手工删掉这一步**，列保留到发布 4 | 去掉 `.MirrorTo`，不再读写 `Name` | 本版新增的行 `Name` 为空：发布 2 已去掉非空约束 → 可以；上一版只读 `DisplayName`，写 `Name` 也无妨 |
| 4 收缩 | 手写删除 `Name` 列及其索引 | — | 上一版已完全不引用 `Name` → 可以 |

原则：**先有表结构，再让代码使用；先让代码不再使用，再删除表结构**；**读取只依赖所有运行中的版本都会写的列**。每一步都只在上一步的代码全部退出后进行。

| 规则 | 说明 |
|---|---|
| 停写旧列之前先放开它的约束 | 哪个发布的代码第一次不写旧列（本例为发布 3），在它开始之前——最迟在它自己的迁移中——去掉旧列上的非空（无默认值时）、CHECK、`NULLS NOT DISTINCT` 唯一约束、非空外键等会拒绝缺省值的约束；不给旧列补一个虚假的默认值。本例放在发布 2，与新列加非空在同一步完成。否则发布 2、3 共存期间，发布 3 的新增会因旧列为空而失败（2026-11-10 在本机 PG 18 上用 SQL 实测，见原型 #7 结论页第 2.1 节） |
| 旧列上的唯一约束与索引 | 在切换读取（发布 2）之前为新列建好对应的唯一约束与索引（大表用 `CREATE INDEX CONCURRENTLY`）；旧列上的随发布 4 一起删除 |
| 过渡期间的批量更新 | 带 `.MirrorTo` 的属性（发布 1～2）不得单独用 `ExecuteUpdate` 更新一列；确需批量更新时，在**同一条语句**中两列都写——批量更新不经过保存拦截器。由架构测试检查（扫描 `ExecuteUpdate` 中对带 `MirrorTo` 注解属性的 `SetProperty`） |
| 回退只退一个发布 | 例如发布 3 新增的行 `Name` 为空：退回发布 2（读新列）没有问题，再退回发布 1（读旧列）就会读到空值 |
| 发布 3 的迁移 | “去掉映射会生成删除列的操作”是按 EF 迁移的模型差异规则推断，阶段 2 实现 `.MirrorTo` 时用真实迁移确认；迁移评审（05-03 第 9.2 节）中逐项核对 |

- `.MirrorTo(...)` 的实现（原型 #7，`docs/framework/spikes/07-mirror-to.md`）：影子属性映射另一列 + 保存拦截器在保存前复制当前值。发布 1 读旧列（而不是新列）的原因也见该页：读新列会读到上一版在滚动期间写入的旧值，新建的行还会因新列为空而抛异常。

**示例二：拆表（把 `Sample` 上的联系信息列移到新表 `SampleContact`）**

| 发布 | 迁移 | 代码 |
|---|---|---|
| 1 扩展 | 新建 `SampleContact`；回填 | 写入时两处都写；读取仍从旧列读 |
| 2 迁移 | 再次回填补齐 | 读取改为从新表读；写入仍两处都写 |
| 3 停写 | — | 不再写旧列 |
| 4 收缩 | 删除旧列 | — |

| 规则 | 说明 |
|---|---|
| 回填大表时分批（每批按主键范围，如 5000 行），避免长事务锁表 | 迁移中手写 SQL；评审 |
| 新增非空列：先加可空列或带默认值，回填后再加非空约束 | PG 加带常量默认值的列不重写表 |
| 删除列、表、改类型、改名、加非空约束、加唯一约束，都属于破坏性变更 | 迁移评审（05-03 第 9.2 节的人工审查）中逐项确认分步方案 |
| 创建大表索引用 `CREATE INDEX CONCURRENTLY`（迁移中手写，不在事务中） | 评审 |

---

## 12. 测试编写

ADR-0021 已定测试层次、工具（xUnit v3、Testcontainers、`WebApplicationFactory`、ArchUnitNET、Shouldly、NSubstitute、Verify）与“每个命令和查询至少一个集成测试”。本节规定怎么写。

### 12.1 D8 集成测试之间如何隔离数据库

集成测试与 API 测试用真实 PostgreSQL。处理器会提交事务、写 Outbox、使用独立的查询连接（05-03），所以“每个测试一个事务、结束时回滚”行不通。

| 选项 | 做法 | 优点 | 缺点 |
|---|---|---|---|
| A 每个测试前清空数据 | 用 Respawn 等工具在每个测试前删除全部表数据 | 每个测试都从空库开始，断言最简单 | 测试必须全部串行；清空约几十毫秒 × 测试数；多一个第三方库（Respawn 的许可 `待确认`） |
| **B 每个测试类一个数据库，从模板克隆** | 测试运行开始时启动一个 PG 容器，执行全部迁移生成模板库；每个测试类用 `CREATE DATABASE … TEMPLATE` 克隆一份，类结束时删除；同一个类中的测试串行，各自创建自己的数据 | 测试类之间完全隔离且可并行；不需要第三方库；克隆比重新迁移快得多 | 同一个类中的测试共享数据库，断言不能依赖“表中总共有几行”；每个类有一次克隆开销（约一百毫秒级） |
| C 全部测试共享一个库，从不清空 | 每个测试只创建并只断言自己的数据 | 最快，可完全并行 | 列表、统计、唯一约束类测试很难写；测试之间的意外影响难以排查 |

- **决定（已确认，2026-11-04）**：B。框架提供测试基础设施项目 `Kmic.BuildingBlocks.Testing`：程序集级夹具（xUnit v3 的 assembly fixture）管理容器与模板库；测试类基类负责克隆、提供已配置的宿主与 `FakeTimeProvider`。需要“空库”语义的测试（如统计）放在单独的测试类中。
- **理由**：在“隔离可靠”与“速度”之间最平衡，且只依赖 PostgreSQL 自身能力，符合选型原则（03 第 1 节）。
- **取舍**：同一个类内的测试要自觉使用自己的数据。
- 实现要点（原型 #10，`docs/framework/spikes/10-test-infrastructure.md`）：PG 克隆模板库时要求模板库没有活动连接，连接池中空闲的物理连接同样算（克隆报 `55006`），所以迁移模板库的连接用 `Pooling=false`；程序集级夹具只创建一次，类级夹具可直接注入它；并发克隆可行，单次约 30 ms。

### 12.2 D9 测试方法名用什么语言

| 选项 | 做法 | 优点 | 缺点 |
|---|---|---|---|
| **A 英文：`行为_条件_结果`** | `Submit_WhenDraft_RaisesSubmittedEvent` | 与 D3 一致，测试名中的概念与代码、术语表中的英文名相同；KM9004 无例外 | 业务人员不能直接读懂测试报告 |
| B 中文方法名 | `草稿状态提交_成功并产生已提交事件`（C# 允许中文标识符） | 测试报告就是中文的业务规则清单 | 同一概念在测试中又多了一套中文叫法，正是旧系统“多个名字”的问题；输入时频繁切换输入法；KM9004 要为测试开例外 |
| C 英文方法名 + 中文 `DisplayName` | `[Fact(DisplayName = "草稿状态提交成功")]` | 报告是中文 | 每个测试写两遍名字，两者容易不一致 |

- **决定（已确认，2026-11-04）**：A。
- **修订（用户确认，2026-11-11）**：类名已经说明被测的行为时（如分析器测试、只测一个行为的测试类），可以只写 `条件_结果`，如 `EventCarryingEntity_Reports`；补上行为段只会让每个方法多一段相同的前缀。
- **理由**：测试是统一语言的一部分，用与代码相同的英文名，才能保证“一个概念一个名字”在测试中同样成立；需要中文说明的复杂规则，在测试方法上加一行中文注释。
- **取舍**：测试报告需要看懂英文名。

### 12.3 其他测试写法

| 规则 | 说明 |
|---|---|
| 结构 | Arrange / Act / Assert 三段，用空行分隔，不写这三个词的注释 |
| 一个测试只验证一个行为 | 可以有多个断言，但都服务于同一个行为 |
| 测试项目与命名空间 | `Kmic.<模块>.Domain.Tests`、`Kmic.<模块>.IntegrationTests`；测试类 = 被测对象 + `Tests` |
| 测试数据 | 用测试数据构建器（`SampleBuilder.Draft().WithItems(3).Build()`），每个聚合一个，放在模块测试项目中；不使用随机数据生成库（失败不可重现） |
| 时间 | 一律用 `FakeTimeProvider`；禁止 `Task.Delay` / `Thread.Sleep` 等待（BannedSymbols.Tests.txt）；等待异步结果用框架提供的有超时的轮询辅助方法 |
| 替身 | 只替换进程外依赖（外部 HTTP、邮件、对象存储）和时间；不替换仓储、DbContext、中介者（这些用真实实现 + 真实数据库） |
| 断言 | Shouldly；`Result` 的断言用框架提供的扩展（`result.ShouldBeFailure(SampleErrors.NotFound)`） |
| 快照 | Verify 只用于契约类输出（OpenAPI、消息 JSON Schema、匿名端点清单）；快照文件提交到仓库，变化必须在评审中可见 |
| 授权矩阵 | API 测试按“角色 × 动作 × 数据范围”用 `[Theory]` 数据驱动（ADR-0021） |
| 不允许跳过 | `Skip` 必须带跟踪编号（同 `TODO` 规则，KM9003） |

---

## 13. 版本控制

### 13.1 D10 分支与合并方式

旧仓库：`development` 分支开发，经合并请求合并到 `main`；分支名含中文（如 `fix/申请人显示账号`）；提交信息无固定格式。

| 选项 | 做法 | 优点 | 缺点 |
|---|---|---|---|
| **A 主干开发** | `main` 始终可发布；每项工作开短分支（建议不超过 2～3 天），经合并请求压缩合并（squash）回 `main`；发布从 `main` 打标签 | 合并冲突小；CI 只需守护一个分支；与“每次合并都经过全部质量门禁”配合最好；历史线性、每个提交对应一个合并请求 | 未完成的功能需要用功能开关（05-10）或不暴露端点的方式合入；对拆分工作的能力要求更高 |
| B GitFlow（`develop` / `release` / `main`） | 功能分支合入 `develop`，定期从 `release` 发布到 `main` | 发布节奏清晰，适合有固定版本周期的产品 | 长期分支之间反复合并；`develop` 与 `main` 的差异需要额外管理；对持续部署的 Web 系统收益小 |
| C 沿用旧仓库：`development` + `main` | 同旧仓库 | 团队熟悉 | 实际是 B 的简化版，`development` 成为长期集成分支，同样有反复合并的问题 |

- **决定（已确认，2026-11-04）**：A。配套约定：

| 项 | 约定 |
|---|---|
| 分支名 | 只用 ASCII：`<类型>/<简短英文描述>`，如 `feat/sample-submit`、`fix/outbox-retry` |
| 提交信息 | Conventional Commits 格式，类型与范围用英文、描述用中文：`feat(sample): 新增提交接口`。类型：`feat` `fix` `refactor` `perf` `test` `docs` `build` `ci` `chore`；范围为模块名或 `framework` |
| 破坏性接口变更 | 提交信息加 `!`（`feat(sample)!: …`），并在说明中写明影响（与 05-06 的 OpenAPI 破坏性检查对应） |
| 合并请求 | CI 全部通过 + 至少 1 人审核；修改 `CODEOWNERS` 中列出的文件须框架负责人审核（D1） |
| 合并方式 | 压缩合并，合并请求标题即最终提交信息（须符合上面的格式，CI 检查） |
| 保护 | `main` 禁止直接推送、禁止强制推送 |

- **理由**：本系统是持续部署的内部 Web 系统，没有需要并行维护的多个发布版本；主干开发让每次合并都经过全部门禁，问题暴露最早。
- **取舍**：未完成的功能合入主干时需要隐藏；具体的发布与标签规则在 08 实施计划与运维文档中定。CI 平台见 ADR-0032，上述规则不依赖具体平台。

**单人开发阶段的合并规则**（阶段 0 确认）：目前只有一位开发者，而代码平台不允许批准自己发起的合并请求。因此单人阶段的分支保护只强制“CI 全部通过”与“`main` 禁止直接推送、禁止强制推送”，不强制“至少 1 人审核”与 `CODEOWNERS` 负责人审核；人工审核由开发者审阅 AI 助手的改动完成。`CODEOWNERS` 照常维护（先写这位开发者）。第二位开发者加入时开启两项审核要求（`docs/implementation/00-README.md` 待办 T-04）。过渡期仓库在个人 GitHub、CI 暂时搁置时，以推送前本机 `dotnet run scripts/check.cs` 通过为准（`docs/implementation/S0-开工准备.md` 问题 7）。

---

## 14. 代码评审清单

只列工具无法检查的条目（第 2.1 节）。评审者对照本清单，合并请求模板中附上本清单的链接。

| 类别 | 检查项 | 来源 |
|---|---|---|
| 领域 | 状态变化是否都经过聚合的业务方法；子实体的修改方法是否为 `internal` | 05-01 第 3.2 节 |
| 领域 | 需要通知的状态变化是否 `Raise` 了领域事件 | 05-01 D7 |
| 领域 | 值对象的规范化（去空格、大小写）是否在工厂方法中完成 | 05-01 第 5 节 |
| 应用 | 处理器在事务内是否重新读取并校验状态、版本、指派关系（不依赖管道中的前置检查） | ADR-0011 第 11 条 |
| 应用 | 用户传入的引用 ID 是否用 `CallerScoped` 查询校验“能否选用” | 05-07 第 4 节 |
| 应用 | 取号之后是否还有 Contracts 查询或其他耗时操作 | 05-13 D1 |
| 应用 | `IAfterCommit` 是否只用于允许的用途（缓存失效） | 05-02 第 7 节 |
| 应用 | 查询处理器中使用缓存时，是否说明属于哪一类、是否“因人而异” | 05-10 第 4.1 节 |
| 授权 | 资源策略的 `switch` 中，无特殊规则的动作是否显式写出 `Allow` | 05-05 第 6 节 |
| 授权 | 调用 `Files.GetDescriptor` 之前是否已完成业务对象的授权 | 05-11 第 4 节 |
| 数据 | 手写 SQL：标识符加引号、参数化、带归属的表用 `IDataScopeSql`、软删除条件 | 11.1 |
| 数据 | 列表是否分页；是否有循环中查询（N+1） | 11.1 |
| 数据 | 迁移是否包含破坏性变更，分步方案是否正确 | 11.3 |
| 后台 | 定时任务命令是否可重复执行；大批量是否分批或转为后台作业 | 05-08 第 4 节 |
| 外部 | 查询中的外部调用是否有缓存或降级 | 05-11 第 6 节 |
| 接口 | 匿名端点清单、OpenAPI、消息契约快照的变化是否符合预期 | 05-06、05-07 |
| 通用 | 新增的业务概念是否已登记术语表；命名是否与术语表一致 | D3 |
| 通用 | 豁免（`[SuppressMessage]`）的理由是否成立 | D1 |
| 通用 | 注释是否解释“为什么”；是否有注释掉的代码 | 10.3 |
| 通用 | 注释、接口文档、用户可见文字是否符合附录 A（不翻译代码、不删调用约定、无空话） | 附录 A（R01 M3） |
| 应用 | 命令中用于判断与修改的数据是否经仓储读取；修改后是否又经 `IQuerySender` 读取本模块数据 | 05-03 第 2.1 节（R01 H2） |
| 接口 | 统计、大范围查询等重型端点是否声明并发策略；数据量大的列表是否声明上限计数 | 05-06 第 6.1、8.4 节（R01 M4、M5） |
| 数据 | 新增或修改的表、列、人工索引的数据库注释是否准确（缺失由模型校验拦截，内容质量靠评审） | 05-03 第 3.9 节（R01 M1） |
| 应用 | 修改金额、额度、库存这类数值的操作，是否建模为有 ID 的业务单据，而不是“没有记录的增量命令” | 05-02 第 5.4 节（R02） |
| 应用 | 高风险命令的收据字段是否只是本人可取回的最小信息 | 05-02 第 2.2、5.3 节（R02） |
| 通用 | 领域实体、配置类的类型和每个公开属性是否都有 XML 注释，是否简短写出含义；用整数表示的时长、大小是否写了单位 | 10.3 |
| 通用 | 类型内成员顺序与空行 | 5.4 |

---

## 15. 规则总表

### 15.1 自定义分析器（`KMxxxx`，全部 Error）

| 编号 | 规则 | 范围 | 来源 |
|---|---|---|---|
| KM0101～KM0105 | 错误目录、领域异常、实体构造与属性、可变集合 | Domain / 全部 | 05-01 第 11 节 |
| KM0201～KM0205 | 值对象写法 | Domain | 05-01 第 5.1 节 |
| KM0301 | 智能枚举写法 | Domain | 05-01 第 6 节 |
| KM0401 | 状态属性只能由状态机改写 | Domain | 05-01 第 7 节 |
| KM0501 | 领域事件不得携带实体对象 | Domain | 05-01 第 8 节 |
| KM2001 | 查询表达式中不得用 `ToLower` / `ToUpper` / `Contains` / `StartsWith` / `EndsWith` 做文本匹配 | Application、Infrastructure | 本文 11.1 |
| KM9001 | `[SuppressMessage]` 必须填写 `Justification` | 全部 | 本文 D1 |
| KM9002 | 非生成代码中禁止 `#pragma warning disable` | 全部 | 本文 D1 |
| KM9003 | `TODO` 与测试的 `Skip` 必须带跟踪编号 | 全部 | 本文 10.3、12.3 |
| KM9004 | 标识符只能使用 ASCII 字符 | 全部 | 本文 D3 |
| KM9005 | 主构造函数参数不得被重新赋值 | 全部 | 本文 D5 |
| KM9006 | 不得使用 `dynamic` | 全部 | 本文 5.2 |
| KM9007 | 手写代码禁止嵌套三元运算（加括号也不行） | 全部（生成代码除外） | 本文附录 A.7（R01 M3） |
| KM9008 | `[LoggerMessage]` 的模板里不得出现 `{@对象}`，整个对象不能写进日志 | 全部（生成代码除外） | 05-12 第 2.3 节、ADR-0016 |

### 15.2 禁用 API（BannedApiAnalyzers）

| 禁用 | 替代 | 豁免 | 来源 |
|---|---|---|---|
| `DateTime.Now` / `UtcNow` / `Today`、`DateTimeOffset.Now` / `UtcNow` | `TimeProvider` | 框架日志时间戳等少数位置 | ADR-0011、05-13 |
| `Guid.NewGuid()` | 强类型 ID 的 `New()`（UUID v7） | — | ADR-0009 |
| `new HttpClient()` | `IHttpClientFactory` 类型化客户端 | — | ADR-0021 |
| `IConfiguration` 注入与读取 | `IOptions<T>` | 宿主、BuildingBlocks | 05-10 |
| `IMemoryCache`、`IDistributedCache` | `HybridCache` | BuildingBlocks | 05-10 |
| Application 中的 `DbContext` 写入 API、`FromSql` | 命令走仓储；手写 SQL 在 Infrastructure | — | ADR-0004、05-03 |
| `IgnoreQueryFilters()` 等关闭过滤器的方法 | `WithoutDataScope` + `[BypassesDataScope]` | BuildingBlocks | 05-03、05-05 |
| 端点中的 `Results.Problem` / `Results.StatusCode` | 返回 `Result` | — | 05-06 |
| `FromSqlRaw` / `SqlQueryRaw` / `ExecuteSqlRaw` | 插值形式 | BuildingBlocks | 本文 11.1 |
| `Task.Wait`、`Task.Result`、`GetAwaiter().GetResult()` | `await` | — | 本文 7 |
| `Task.Run`、`new Thread`、`ThreadPool.QueueUserWorkItem` | 后台作业 | BuildingBlocks | 本文 7 |
| `IServiceProvider.GetService` / `GetRequiredService` | 构造函数注入 | BuildingBlocks、宿主 | 本文 8 |
| 测试中：`Task.Delay`、`Thread.Sleep` | `FakeTimeProvider`、轮询辅助方法 | — | 本文 12.3 |
| 已选定能力的替代库（MediatR、AutoMapper、Moq、FluentAssertions 等） | 03 第 5 节 | — | 03 选型原则 3 |

### 15.3 内置与 Meziantou 规则（提升为 Error 的主要条目）

| 规则 | 内容 | 本文章节 |
|---|---|---|
| IDE0005、IDE0161、IDE0130、IDE0079 | 多余 `using`、文件范围命名空间、命名空间与目录一致、多余的豁免 | 3、2.3 |
| CA1852、MA0053 | 类型密封 | 5.1 |
| CS9124、IDE0290 | 主构造函数参数重复捕获；使用主构造函数 | 5.3 |
| IDE0016～IDE0360 中的现代写法（共 39 条，含 IDE0290） | 新写法更清楚、更简洁或更可靠的，写成旧写法即失败；清单与仍只作建议的例外见第 3.4 节 | 3.4 |
| CA1031、CA2200、CA1510～CA1513 | 异常 | 6 |
| CA1849、MA0042、CA2016、MA0040、CA1068、CA2012 | 异步与取消 | 7 |
| MA0137、MA0138 | `Async` 后缀 | 4.4 |
| CA1307、CA1309、MA0006、CA1304、CA1305、MA0011、MA0009、SYSLIB1045 | 字符串与文化 | 9 |
| CA1848、CA2254、CA2017 | 日志 | 10.1 |
| EF1002 | SQL 注入风险 | 11.1 |
| 关闭：CA2007、MA0004 | `ConfigureAwait` | 7 |

关闭或限定范围的规则：

| 规则 | 处理 | 原因 |
|---|---|---|
| CA1716（标识符与语言关键字同名） | 全局关闭 | 它保护的是 VB 等其他语言的使用者；本系统只用 C#，而 05-01 的 `Error`、05-05 的 `IDataScope.For(...)` 都会被它拦下（原型 #1 D3） |
| RS1033（诊断说明要以标点结尾） | 只对分析器项目关闭：`.editorconfig` 的 `[src/BuildingBlocks/Kmic.BuildingBlocks.Analyzers/**.cs]` 节 | 它只认英文的 `.` `!` `?`，中文句号过不了；它要求的是写成完整的句子，中文说明以“。”结尾已经满足（2026-11-11 用户确认） |
| MA0097（实现 `IComparable` 时要求比较运算符） | 只对强类型 ID 文件关闭：`.editorconfig` 的 `[src/**/*{Id,Ref}.cs]` 节（依赖 05-01 第 4.1 节“以 `Id` / `Ref` 结尾”的命名约定，该约定由架构测试守护） | Vogen 默认生成 `IComparable` 但不生成运算符；键集分页（05-06 第 6.3 节）用 `CompareTo` 即可翻译为 SQL 的 `<`（原型 #2 验证），不需要运算符；其他手写类型仍检查 |

其余 Meziantou 规则在实施时逐条确认级别（03 第 4.10 节）。Meziantou.Analyzer 锁定 3.0.320，MA0137、MA0138、MA0155 存在且含义与本文一致（`docs/implementation/S0-04-版本与许可.md`）。

### 15.4 架构测试

按来源文档汇总；规则的完整说明在各文档中。

| 来源 | 架构测试 |
|---|---|
| ADR-0008、ADR-0021 | 依赖方向；模块之间只引用 Contracts；Contracts 之间不相互引用、不含领域类型；共享内核不含业务概念 |
| 05-01 | Domain 只引用 `BuildingBlocks.Domain`；Domain 不引用 `ICurrentUser` / EF / Mediator；错误码唯一且前缀为模块名；聚合根属性不得是其他聚合根；强类型 ID 命名；领域事件 `sealed record`、过去时、不在 Contracts 中；状态机 `Build()` 成功 |
| 05-02 | 命令 / 查询命名；消息为 `sealed record` 且不含领域类型；响应不得是领域类型；除 Mediator 生成的代码外任何类型不得引用命令、查询处理器；命令响应类型（`[SystemOnly]` 命令除外）的属性只能是标识、版本号、状态编码、错误码、计数、时刻与布尔值，不含嵌套对象，集合只允许元素同样只含这些属性的“逐条收据”（第 2.2 节，R02）；命令处理器不注入 `ISender` / `ICommandSender`；校验器不注入 DbContext、仓储、Contracts；`[AllowAnonymous]` 只在 Identity；幂等命名线索；领域事件处理器只依赖本模块 |
| 05-03 | 命令处理器不注入查询上下文；实体 schema = 模块名；`decimal` 声明精度；聚合之间无导航属性；只有聚合根实现 `ISoftDeletable`；业务表与业务列有数据库注释（第 3.9 节，与启动时模型校验同一套检查，R01 M1） |
| 05-05 | `[BypassesDataScope]` 标注；策略类只实现一个 `IResourcePolicy<,>`；敏感权限点标记；手写 SQL 查询范围对象的提示；Identity 模块中声明 `[RequirePermission]` 的命令，其处理器继承 `AuthorizationAdminCommandHandler<,>`（提交前重查操作者，第 4.6 节，R02） |
| 05-06 | 端点只注入 `ICommandSender` / `IQuerySender`；端点 `internal sealed` 且以 `Endpoint` 结尾；响应 DTO 不含领域类型；匿名端点清单快照 |
| 05-07 | 消息类型名与订阅名以模块名开头且唯一；请求型消息只定义在平台模块；读模型只经 `IReadModelWriter<T>` 写入 |
| 05-08 | 模块不注册 `IHostedService`；后台作业处理器不注入 DbContext、仓储 |
| 05-10 | 无可变静态字段；缓存键第一段为模块名；`HybridCache` 只在 Infrastructure 与查询处理器中使用；缓存值不可变；授权相关代码不依赖 `IPreferences` |
| 05-11 | 只有 Files 模块与框架使用 `IFileStorage` |
| 05-13 | 计数表只经 `INumberGenerator` 访问 |
| 本文 | 子目录不得有 `.editorconfig`（2.3）；`InternalsVisibleTo` 只指向本模块测试项目（5.1）；模块中不定义异常类型（6）；模块中不使用 `System.Threading.Lock` / `Monitor` / `SemaphoreSlim` / `Mutex`（7）；`IServiceCollection` 注册只在 `IModule` 中（8） |

---

## 16. 测试要点

| 对象 | 必测场景 |
|---|---|
| 自定义分析器 | 每条规则至少“触发”“不触发”各一例；`[SuppressMessage]` 带理由时不报错、不带理由时 KM9001 报错 |
| 架构测试 | 每条规则有一个故意违规的样例程序集或类型，验证测试确实会失败（防止规则写错后永远通过） |
| 格式与编码 | CI 中 `dotnet format --verify-no-changes`；非 UTF-8 或带 BOM 的文件、CRLF 文件被拦截 |
| 拼写检查 | 术语表中的词通过；拼音、常见拼写错误（如 `pruduct`）被拦截 |
| 模糊匹配 | `ContainsText` 在默认列与 `IsCaseInsensitive` 列上都不区分 ASCII 大小写；`%`、`_`、`\` 被转义；翻译出的 SQL 带参数 |
| 测试基础设施 | 两个测试类并行时数据互不可见；模板库在迁移变化后重建 |
| 提交信息 | 不符合格式的合并请求标题被 CI 拦截 |

---

## 17. 与其他文档的接口

| 本文提供 / 需要 | 对方 |
|---|---|
| 一个用例的文件组成、目录结构、各构件的标准示例、`EventId` 段号示例 | 07 业务模块标准写法 |
| `Kmic.BuildingBlocks.Analyzers`、`Kmic.BuildingBlocks.Testing` 两个框架项目 | ADR-0021 |
| `KmicFunctions.ContainsText` / `StartsWithText` | 05-03 第 3.7 节、05-06 第 6.4 节 |
| `.MirrorTo(...)` 迁移过渡映射辅助 | 05-03 第 9 节 |
| `Async` 后缀（D4） | 05 系列各接口签名 |
| 术语表 `docs/glossary.md` | 各业务模块设计 |
| 主干开发、提交格式、`CODEOWNERS` | 08 实施计划、ADR-0032 |
| trigram 索引延后事项 | 00-README“延后事项”表 |
| 与前端共用的规则（D3、D7、D10、第 3.2 节、附录 A）；`.editorconfig` 前端配置节、cspell 范围、`CODEOWNERS` 前端条目 | 前端 F04 第 0.3、2.3、3.1 节 |

---

## 18. 决策汇总

| 编号 | 事项 | 决定（均已确认，2026-11-04） |
|---|---|---|
| D1 | 规则豁免怎么写 | 只允许带理由的 `[SuppressMessage]`，禁止 `#pragma`；规则级别只在根 `.editorconfig` 调整；相关文件列入 `CODEOWNERS` |
| D2 | 格式化工具 | 只用 `dotnet format`，不引入 CSharpier |
| D3 | 标识符语言 | 只用英文；术语表 `docs/glossary.md`；CI 用 cspell 拼写检查；标识符只允许 ASCII |
| D4 | `Async` 后缀 | 自有异步方法一律加 `Async`（MA0137 / MA0138） |
| D5 | 主构造函数 | 服务类使用（IDE0290 为 Error），分析器禁止重新赋值 |
| D6 | 日志写法 | 一律 `[LoggerMessage]`（CA1848 → Error），测试项目除外 |
| D7 | 注释与消息的语言 | 注释、提交信息、程序缺陷异常消息中文；日志模板英文；用户提示中文 |
| D8 | 集成测试数据库隔离 | 每个测试类从模板库克隆一个数据库 |
| D9 | 测试方法名语言 | 英文 `行为_条件_结果`；类名已说明被测行为时可写 `条件_结果` |
| D10 | 分支与合并 | 主干开发 + 短分支 + 压缩合并；Conventional Commits（英文类型 + 中文描述）；分支名 ASCII；单人阶段只强制 CI |

本文的待决与待确认事项已全部关闭（见修订记录）。

---

## 19. 修订记录

| 日期 | 内容 | 来源 |
|---|---|---|
| 2026-11-04 | 新增附录 A《注释与排版细则》、KM9007（禁止嵌套三元）；模块代码禁用 `System.Threading.Lock` | R01 M3 |
| 2026-11-04 | 第 10.3 节：OpenAPI 中的模型必须有 XML 注释；第 14 节评审清单增加 R01 H2、M1、M4、M5 对应条目；第 15.4 节增加数据库注释的架构测试 | R01 M1、M2、M4、M5、H2 |
| 2026-11-05 | 第 14 节评审清单增加两条（增量命令、收据字段）；第 15.4 节增加命令响应只是收据、授权管理命令处理器基类两项架构测试 | R02 |
| 2026-11-05 | 第 3.2、17 节：与前端共用的规则 | 前端 F04 |
| 2026-11-05 | 第 13.1 节：单人开发阶段的合并规则；cspell、Meziantou 的版本与规则编号核实，待确认关闭 | 阶段 0（S0 问题 5、S0-04） |
| 2026-11-06 | 第 5.1 节：命令、查询处理器改为 `public sealed`；第 15.3 节：CA1716 全局关闭、MA0097 对强类型 ID 文件关闭 | 原型 #1 D1、D3；原型 #2 |
| 2026-11-06 | 第 2.2 节：分析器项目用 `LangVersion latest` + PolySharp；第 3.4 节：39 条现代写法规则提升为错误；第 5.3 节：IDE0290 由建议改为错误 | 质量原则；原型 #8 |
| 2026-11-06 | 第 5.4 节：类型内成员顺序、参数数量参考线由 5 改为 4；第 10.3 节：长期注释只写原因（必要时附 ADR）、临时【引用】、类型说明一律 `///`、实体与 `Options` 每个公开属性必须写 XML 注释、注释格式自动检查；新增附录 A.12 | 阶段 1 风格讨论（用户确认） |
| 2026-11-06 | 第 3.1 节 `MSBuildTreatWarningsAsErrors`；第 3.2 节工具生成文件用 `check.cs -- --fix` 规整、迁移标为生成代码 | 原型 #11 D3 |
| 2026-11-06 | 第 11.2 节：`ContainsText` 的翻译方式确定（转义在数据库端），待确认关闭；第 11.3 节：`.MirrorTo` 实现方式确定，发布 1 读哪一列转为待决；第 12.1 节：模板库克隆的实现要点，待确认关闭 | 原型 #5、#7、#10 |
| 2026-11-10 | 文档整合：以上补充说明并入正文，决定不变；新增“编码速查”；第 13.1 节 CI 平台的说法指向 ADR-0032；第 14 节评审清单中 05-06 的节号更正为 8.4 | R01 L2（S0-08） |
| 2026-11-10 | 第 11.3 节示例一：发布 1 读旧列；停写旧列之前先放开其约束；唯一约束、批量更新、回退的规则 | 原型 #7 D1；复核意见 |
| 2026-11-10 | D6：补充框架代码的 `EventId` 分段 | 阶段 2 S2-04 |
| 2026-11-11 | 第 10.3 节、A.3：实体与 `Options` 的属性注释只简短写含义，不补空值含义、时区；设计文档编号也不写进异常消息与 `Justification`，由 `scripts/check.cs` 检查；第 3.2 节、A.8、A.12：行宽不设上限，一屏宽能显示完就一行；A.12 新增“怎么写才不机械” | 阶段 2 注释复查（用户确认） |
| 2026-11-11 | 修正与上一条不一致的写法（决定不变）：A.12“内部类型只写是什么”去掉“时区、空值含义”，空值含义只在 `param` 中写；第 14 节评审清单同步；A.12“日志”的示例改为英文，与 D7 一致 | 注释复查 |
| 2026-11-11 | 附录 A 重排：原 A.12 的条目并回 A.1～A.11 各小节，A.12 只留与参考文档冲突的条目，两份自查清单合并（写法整理，决定不变）。新增：`remarks` 可写整个类型的设计原因、测试类默认不写 `summary`、分析器诊断的写法、`check.cs` 检查一组不用的说法（A.3、A.5、A.9）。第 10.3 节与编码速查删除临时【引用】（T-17 提前关闭）。第 4.2 节：实现外部接口、重写基类方法时沿用原参数名（CA1725） | 注释与风格复查（用户同意） |
| 2026-11-11 | D7：程序缺陷类异常的消息改为中文；D9：类名已说明行为时可写 `条件_结果`；第 5.2 节：列出 `!` 的固定例外；第 15.3 节：分析器项目关闭 RS1033 | 注释与风格复查（用户确认） |

---

## 附录 A 注释与排版细则

本附录是 06 第 10 节的细化：第 10 节定规则（语言、日志写法、哪些项目强制 XML 注释），本附录说明**怎么写**。可以用工具检查的条目已列入第 15 节规则总表；其余由代码评审对照本附录判断。示例中的 `Sample` 等名称均为**示例**。

部分条目来自用户提供的通用 .NET 代码风格参考（草案 v1，2026-11-06 阶段 1 确认），已并入各小节；与本项目已定设计冲突的参考条目按已定设计处理，见 A.12。

### A.1 总原则

1. 注释像同事当面解释：简单直接、通俗易懂，不废话，不做润色和修饰。
2. 只写代码表达不了、维护时必须知道的信息：用途、含义、原因、约定。
3. 不以注释数量衡量质量。已有注释清楚准确就保留，不为换一种说法而改写；没有信息量的注释直接删除。
4. 注释必须与实际实现一致，改代码时同步改注释；不编造历史原因、接口限制或业务规则。
5. 整理注释和排版时不夹带行为修改：不改条件求值顺序、空值处理、异常行为与数据处理顺序。

### A.2 什么时候写注释

| 写 | 不写 |
|---|---|
| 代码本身没有表达的关键业务约定 | 简单赋值、取值、常量、索引换算 |
| 特殊处理、历史兼容、必须遵守的执行顺序 | 明显的空值判断、条件判断、方法调用 |
| 看起来多余、实际必须保留的代码及其原因 | 普通的查询、遍历、映射、返回 |
| 并发控制、资源释放、算法中的关键约束或取舍 | 把代码逐句翻译成中文 |
| 授权责任、协议限制（精简时不能丢掉） | “查询数据”“遍历集合”“返回结果”这类流程标记 |

维护者需要知道的原因，写在相关代码行上方的 `//`，不堆在类型说明里（类型说明写什么见 A.3）。有价值的内部注释是这样的（示例）：

```csharp
// 取号放在最后一步：行锁持有到事务结束（ADR-0024）
// 只判断当前路径，共享依赖不算循环
// 缓冲区会归还到池中，结果必须先复制
```

### A.3 XML 注释写法

**格式**：`summary` 的开始标签、正文、结束标签各占一行，不写成单行；通常几个字或一句话即可。名称式注释不必凑成完整句子；一句话的注释末尾不加句号，需要两句时用句号隔开，最后一句不加。

```csharp
/// <summary>
/// 提交示例单据
/// </summary>
```

**`summary` 只写是什么**：类型写名词，如“命令事务”“数据库配置”；方法写做什么，如“按 ID 读取，用于修改”。原因、限制、做法不写进 `summary`。内部类型同样只写是什么，不写实现细节、计算方式。

| 对象 | 写什么 | 示例（示例） |
|---|---|---|
| 类、接口 | 一句话点明用途，不在类头堆设计说明 | “示例单据”“示例单据的资源策略” |
| 属性、字段 | 简短写出含义；只有名称表达不了的单位才在括号里写，`TimeSpan` 不写 | “截止日期”“文件大小（字节）”“超时时间（秒）” |
| 枚举与枚举成员 | 中文名称；属性使用枚举时只写属性含义，不重复列出枚举值 | “审批状态”；成员“草稿”“已提交” |
| 方法 | 公共业务方法与框架接口写用途或调用约定；简单取值、转换、转发、已有继承文档的方法不写 | “检查循环依赖” |
| `param` | 确实需要时才写，与说明同在一行，写业务含义而不是类型；为空有特殊含义时写一句 | `/// <param name="after">上一页最后一条的 ID，从头读时为空</param>` |
| `remarks` | 见下文 | — |
| `returns` / `exception` / `example` | 只在影响调用方理解时写，不为填模板而写 | — |

**`remarks` 少写**，大多数类型不需要：

- 写调用方不知道就会用错的约束，一两句，如“必须在事务里调用”“只能和 [AllowAuthenticated] 一起用”。
- 整个类型的设计原因也写在这里，一两句，如“用类而不是结构体，default 才不会被当成成功”。
- 只和某几行代码有关的原因，写在那几行上方的 `//`，不放进 `remarks`。

其他：

- 接口上写完整注释，实现类的对应成员用 `/// <inheritdoc />`，不重复写。
- 不写“取消令牌”“返回一个任务”这类类型已经表达的 `param`、`returns`；不机械补充“为空时返回 null”“异步返回 Task”。例外：同一方法里别的参数写了 `param` 时，编译器要求每个参数都写（CS1573），取消令牌就写“取消令牌”。
- 不扩写：写“检查循环依赖”，不写“对当前依赖关系进行完整性检查，确保不存在循环引用问题”。
- 进入接口文档的请求、响应字段仍按 A.4 写清调用约定，这是前端唯一能看到的说明。
- **测试类**默认不写 `summary`。只写读测试前必须知道的前提，如两个类并行运行、各用自己的克隆库，测试数据怎么准备，样例是故意写错的。不在类说明里列出覆盖了哪些情况：测试方法名已经说明了，新增测试时类说明也不会跟着改。

示例（示例）：

```csharp
/// <summary>
/// 按 ID 读取并锁住这一行
/// </summary>
/// <remarks>
/// 必须在事务里调用
/// </remarks>
Task<Result<TAggregate>> GetForUpdateLockedAsync(TId id, long? expectedVersion, CancellationToken ct);
```

### A.4 接口文档文字（OpenAPI）

请求模型、响应 DTO、枚举的 XML 注释和端点的 `WithSummary` 就是前端看到的接口文档（05-06 第 7.1 节，CI 检查完整性）。写给前端与测试看，先说：**这是什么、怎么传、失败时会怎样**。

| 不建议 | 更直接（示例） |
|---|---|
| 准确文件字节数 | 文件大小（字节），必须与实际文件一致 |
| 完整有界条目集合 | 字典项列表；已有条目保留，编码不能改，不再使用的请停用 |
| 整批原子处理 | 一项失败则整批不保存 |
| 上限由运行策略决定 | 不能超过服务端配置的上限（当前默认 200） |

- 格式要求、唯一性、整批失败、权限要求、配置上限、幂等与版本冲突这类调用约定，可以换成白话，**不能为了简短删掉**。
- 不把示例值写成接口限制；默认值、允许的范围、当前配置不是一回事。
- 名称相近的字段不能混写（如“幂等键”不是“单据 ID”）。

### A.5 用词

以下空话不出现在注释、接口文档和用户可见文字中，改为直接说明实际作用或删除：

| 空话 | 处理 |
|---|---|
| 对相关数据进行统一处理 | 写清楚处理了什么 |
| 实现业务数据的高效管理 | 删除 |
| 提供完整、灵活的数据处理能力 | 删除 |
| 确保整个流程的完整性与可靠性 | 写出具体保证了什么（如“失败时整体回滚”） |

| 规则 | 说明 |
|---|---|
| 用读者已有的词 | 少用术语，能用平常话就用（“按 ID 读取”而不是“加载聚合”）；不自造词（如“幂等租约”）；代码里已有的类型名照常用（如“聚合根”对应 `AggregateRoot`） |
| 不写设计文档的说法 | “管道第 6 位”“挂接点”“定稿阶段”“第 2 层检查”这类只在设计文档里有意义的说法不写，换成直接的描述 |

- `scripts/check.cs` 检查一组没有正当用法的说法（如“定稿”“挂接”“租约”和上表中的空话），出现在代码文件中即失败。评审时发现新的同类说法，加进 `check.cs` 的词表。
- 用户可见的标题、按钮、提示与错误文字同样适用：自然、直接、简洁，像正常产品写的，不用汇报式、营销式说法。

### A.6 方法内部注释的位置与形式

1. 放在对应代码或逻辑块的**上方**，用 `//`；行尾注释少用。
2. 一般一句短话；确需连续多行时可用 `/* ... */`。
3. 先说决定，再说原因：`// 连接池耗尽时不重试：重试只会让排队更长`，不写成 `// 重试只会排更长的队`，让读者自己推出“所以不重试”。
4. 一句一个意思：不用分号、冒号把三四层意思串成一句；分支各自的原因写在各自的分支里。
5. 不加装饰性的横线、星号框和分区标题；不用大量空格把行尾注释纵向对齐。
6. 长方法可以在关键阶段写简短步骤提示，如 `// 1. 校验登录凭据`；只在有助于看清主流程时使用，不给每次赋值编号。
7. `TODO` 与 `FIXME` 都必须带跟踪编号：`// TODO(#123): …`、`// FIXME(#130): …`（KM9003）；不使用 `HACK`，改写成说明原因的普通注释，需要以后清理的再加 `TODO` 编号。

示例（示例）：

```csharp
if (foreignKey.PrincipalToDependent is not null && !EntityKinds.IsAggregateRoot(foreignKey.DeclaringEntityType.ClrType))
{
    // 外键可空的话，子实体离开集合时 EF 只会把外键置空，行还留在表里
    foreignKey.IsRequired = true;
    foreignKey.DeleteBehavior = DeleteBehavior.Cascade;
}
else if (EntityKinds.IsAggregateRoot(foreignKey.PrincipalEntityType.ClrType))
{
    // 聚合根还被别的数据引用时，不允许物理删除
    foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
}
```

### A.7 条件、控制流与方法

| 规则 | 说明 | 检查 |
|---|---|---|
| 禁止嵌套三元（加括号也不行） | 多条件结果用 `switch` 表达式；涉及顺序或副作用用平铺的 `if / else`；简单二选一仍可用三元 | KM9007 |
| 先算后传 | 参数中的多行 `switch` 或复杂分支不易读时，先算出结果再传入；短表达式直接写 | 评审 |
| 复杂条件按含义整理 | 提取一个有名字的局部变量，比加注释更清楚 | 评审 |
| 保留大括号 | 控制语句都带大括号（第 3.4 节）；简单分支可以写在一行：`if (x is null) { return; }` | `.editorconfig` |

示例（示例）：

```csharp
var reason = (user, verification) switch
{
    (null, _) => LoginFailure.UnknownAccount,
    ({ IsEnabled: false }, _) => LoginFailure.Disabled,
    (_, PasswordVerification.Failed) => LoginFailure.InvalidPassword,
    _ => LoginFailure.None,
};
```

**控制流**

| 规则 | 说明 |
|---|---|
| 提前返回 | 校验与失败情况放在方法开头，先处理先返回，主流程保持在最左侧；嵌套不超过三层，超过就提前返回或提取方法 |
| `switch` 的选用 | 只是把一个值映射成另一个值，用 `switch` 表达式；每个分支有多条语句，用 `switch` 语句或 `if / else if`；判断有先后顺序、依赖短路求值时，保持平铺的 `if / else if`，不改写成表达式 |
| 循环 | 有副作用用 `foreach`；只做筛选和转换用 LINQ，`Select` 中不写副作用；用 `continue` 提前跳过，不嵌套 `if` |
| 用结构代替分支 | 分支超过三四个且还在增加时，用结构表达：按类型或编码选择做法用“策略 + 字典”；状态能否流转用状态机（05-01 第 7 节） |

**方法与命名**

| 规则 | 说明 |
|---|---|
| 布尔参数 | 不用多个布尔参数，改用选项对象、枚举或命名参数 |
| 空包装 | 不为减少行数拆出只转发参数的方法 |
| 动词约定 | `Get…` 必须存在，找不到返回失败的 `Result`（如仓储的 `GetForUpdateAsync`）；`Find…` 可能找不到，返回 `T?`；`List…` 返回集合，没有数据返回空集合；`Try…` 返回 `bool`，结果经 `out` 返回；`Is` / `Has` / `Can` 返回 `bool`。业务条件不满足返回 `Result` 失败，不抛异常（第 6 节） |
| 时间与单位 | 时刻以 `At` 结尾（`SubmittedAt`）；日期以 `Date` 结尾（`DueDate`）；时长用 `TimeSpan` 时不带单位，用整数时名称带单位（`TimeoutSeconds`、`SizeBytes`） |
| 字典 | 按“值 By 键”命名：`priceByProductId` |
| 含糊的名字 | 不用 `Manager`、`Helper`、`Util`、`Data`、`Info` 这类名字，按职责命名 |

### A.8 换行与排版

1. 不设固定行宽门槛（第 3.2 节）：一屏宽能显示完的条件、调用、表达式保持一行；需要横向滚动或包含复杂 Lambda 时再自然换行。
2. 不按参数个数、条件个数、点号个数机械换行；换行后正常缩进，不做人工纵向对齐。
3. 不同逻辑段落之间留一个空行，同一段的语句保持连续；不把每条语句都隔开，也不把复杂逻辑挤在一起。
4. 特性独立成行，放在目标声明上方；多个特性分行；注释写在特性之前。
5. 对象初始化：字段多时展开，每行一个；很小的对象可以一行。
6. LINQ 与链式调用：短链一行；较长时第一行放筛选或排序，之后接映射与收尾（`.ToListAsync(ct)`）；对象投影、复杂 Lambda 才展开；不把每个调用都拆成一行。

```csharp
/// <summary>
/// 页码，从 1 开始
/// </summary>
[Range(1, int.MaxValue)]
public int Page { get; init; } = 1;
```

确实要换行时：

| 规则 | 说明 |
|---|---|
| 参数换行 | 一行放不下时每个参数一行 |
| 条件换行 | `&&`、`\|\|` 等运算符放在新行行首 |
| 三元换行 | 一行放不下时 `?`、`:` 各占一行 |
| 单行大括号 | 只用于 `return`、`continue`、`break` 与整行约 100 列以内的 `throw`：`if (sample is null) { return; }`；多条语句、赋值、循环体一律多行 |

```csharp
if (sample.Status == SampleStatus.Submitted
    && sample.Items.Count > MaxItemsPerSample
    && !await quotas.HasExemptionAsync(sample.OwnerId, ct))
{
    return SampleErrors.TooManyItems;
}

var dueAt = useBusinessCalendar
    ? calendar.EndOfBusinessDay(sample.DueDate)
    : sample.DueDate.ToDateTime(TimeOnly.MaxValue);
```

### A.9 消息与字符串

| 规则 | 说明 |
|---|---|
| 异常消息 | 一两句，说清哪里错、怎么改，完整句子以“。”结尾：“不支持同步保存，请用 SaveChangesAsync。”，不讲设计理由 |
| 分析器的诊断 | 消息写哪里错、怎么改，末尾不加句号（与编译器自带的诊断一致）；规则的原因写在 `description` 里（IDE 悬停与错误列表中可见），一两句完整的话，分析器类上不再重复，以“。”结尾（分析器项目关闭了只认英文句点的 RS1033，见第 15.3 节） |
| 日志 | 模板用英文（D7），写成运维看日志能直接看懂的话：`Commit outcome of {MessageName} is unknown`，不写成 `CommitOutcomeUnknown: {MessageName}` 这类代码式的说法 |
| 多行字符串 | 多行文本，或文本里有双引号（PostgreSQL 的表名、列名）时用原始字符串 `"""`；`@` 只用于单行的路径和正则 |

- 多行固定说明（如端点的详细描述）不用一串 `+` 拼接，使用原始字符串 `"""..."""`；原始字符串保留换行、不解析转义字符。
- 整理既有文本时先确认最终字符串不变：协议内容、SQL、错误码、空白与换行不能因排版而改变。

```csharp
.WithDescription("""
    提交示例单据。
    已提交的单据不能再修改；版本不一致时返回 409，需刷新后重试。
    """)
```

### A.10 新语法的使用边界

| 语法 | 边界 |
|---|---|
| 集合表达式 `[]` | 目标类型明确、行为一致时使用；不把延迟枚举随手改成 `[.. source]`，不丢掉比较器、容量等约定 |
| `ArgumentNullException.ThrowIfNull` 等 | 只用于编程错误；业务与输入错误仍走 `Result` 与校验器（第 6 节） |
| 表达式体 | 简单方法使用；不为排版增删 `async / await` |
| 主构造函数 | 服务类使用（构造函数只做参数赋值时 IDE0290 会要求改用），参数不得重新赋值（D5、KM9005） |
| `field` 关键字 | 只放少量访问器逻辑，不代替校验，不悄悄加 `Trim()`、默认值或截断 |
| `System.Threading.Lock` | 与 `Monitor`、`SemaphoreSlim` 一样，模块代码中不得使用（第 7 节）；业务并发靠数据库 |
| `params ReadOnlySpan<T>` | 按需使用，不机械替换已有公开签名 |
| 扩展成员 `extension(...)`（C# 14） | 需要扩展属性时才用；只有扩展方法时仍用 `this` 参数写法；仍受第 5.4 节“扩展方法只用于框架流式配置与 LINQ 辅助”约束 |

### A.11 提交前自查

- 有没有只是在翻译代码的注释？有没有删掉了必要的调用约定？
- `summary` 是否只写了是什么？`remarks` 是否只有调用方必须知道的约束或整个类型的设计原因？
- 有没有用分号串起来的长句、设计文档的说法、自造的词？行内注释是否先说了决定？
- 一句话的注释是否没有句号？`summary` 是否多行？接口的实现是否用了 `<inheritdoc />`？
- 公共业务方法、框架接口、接口文档字段是否都有说明？实体、配置类的每个公开属性是否都有注释？
- 特性是否独立成行？简单的判断和调用是否被机械拆行？长查询是否又被硬挤成一两行？
- 不同业务阶段之间是否留了空行？步骤注释是否真的帮助理解？
- 有没有超过三层的嵌套、多个布尔参数、含糊的类名？
- 多行说明是否还在用 `+` 拼接？改写是否改变了最终字符串？
- 新语法是否改变了输入处理、求值顺序、公开签名或资源释放？
- 只改注释和排版的提交，是否夹带了行为修改？是否只整理了本次要改的文件，没有夹带无关重构？

### A.12 参考文档中按本项目已定设计处理的条目

参考文档（见本附录开头）中以下条目不采用参考写法：

| 参考写法 | 本项目 | 依据 |
|---|---|---|
| 日志模板用中文 | 英文 | D7 |
| 高频日志才用 `[LoggerMessage]` | 一律使用 | D6 |
| `Get` 找不到抛异常；业务异常带错误码 | 业务失败返回 `Result`；模块中不定义异常 | 第 6 节、ADR-0010 |
| Controller + 应用服务；简单模块直接用 DbContext | 端点 → 命令 / 查询处理器；写用仓储，读用 `IXxxQueryDb` | 05-02、05-03、05-06、07 |
| 关系紧密的小 `record`、枚举可同文件 | 一个类型一个文件 | 07 D2 |
| 可以用 `Lock` | 模块代码中禁用 | 第 7 节、A.10 |
| 测试中替换仓储 | 仓储、DbContext 用真实实现 + 真实数据库 | 第 12.3 节 |
| `async void` 可用于事件处理 | 一律禁止 | 第 7 节 |
| 行宽约 140 | 不设上限，一屏宽能显示完就一行 | 第 3.2 节 |
| 常量、静态只读字段都用 PascalCase | 常量 PascalCase；私有静态只读字段与其他私有字段一样用 `_camelCase` | 第 4.2 节 |
| 2026-11-12 | 第 15.2 节分层禁用清单落地（决定不变）：根目录 `BannedSymbols.Modules.txt` 只对 `src/Modules/**` 的项目生效（`Directory.Build.props` 按路径接入），覆盖 `IConfiguration`、`IMemoryCache`、`IDistributedCache`、`Task.Run`、`new Thread`、`ThreadPool`、服务定位（`GetService`、`GetRequiredService`、`GetServices`）、`IgnoreQueryFilters`、`FromSqlRaw` / `SqlQueryRaw` / `ExecuteSqlRaw`、`Results` 与 `TypedResults` 的 `Problem` / `StatusCode`；BuildingBlocks 与宿主不受限。实测发现：**方法的禁用 ID 必须逐个重载写全，不带参数列表的写法（如 `M:System.Threading.Tasks.Task.Run`）不会生效，也不报错**。因此架构测试 `BannedSymbolsTests` 逐条核对每个清单：每个符号必须解析成唯一的符号，清单必须接入构建；并在真实项目里用故意违规的代码确认过 `Task.Run`、`GetService` 会报 RS0030。Application 中的 DbContext 写入 API 不需要单独禁用：Application 不引用 EF（架构测试守护）。端点里不得出现 `Results.Problem` 的限制对模块的全部项目生效，BuildingBlocks 的 Presentation 里用到的不受限 | S2-15 / S2-18 |
| 2026-11-12 | 第 15.4 节架构测试补全（决定不变）：对照规则总表逐条核对，结果见 `docs/implementation/S2-18-规则核对.md`。新增：命令响应是收据、处理器不被引用（只检查成员签名）、领域事件处理器只依赖本模块、校验器不注入 Contracts、命令处理器不注入查询上下文、作业处理器不注入数据访问、授权代码不依赖偏好、`IServiceCollection` 只在 `IModule`、聚合类型初始化（状态机可构建）、敏感权限点按命名标记、读模型不是聚合根、创建 / 提交 / 审批 / 支付类命令声明幂等、`WithoutDataScope` 只在标了 `[BypassesDataScope]` 的处理器里、缓存值不可变；后两条读方法体里的调用（`Support/IlCalls`，含 async 状态机与 lambda 的嵌套类型）。没有机器检查的：共享内核不含业务概念、领域事件过去时（评审）；手写 SQL 查询范围对象的提示（等第一个手写 SQL）；Identity 授权管理命令的基类（阶段 3）；缓存键的模块名取值只能评审，键的形态由 `CacheKey.Of` 保证。规则暴露了一处实现与 05-02 的不一致：Sample 的提交命令没有 `[Idempotent]`，已补 | S2-18 |
| 2026-11-12 | 第 15.1 节新增 KM9008：`[LoggerMessage]` 的模板里出现 `{@对象}` 即编译失败（05-12 第 2.3 节落实 ADR-0016 的“禁止整对象输出”，实现见 05-12 修订记录）；自定义分析器共 22 个编号 | S2-18 |
