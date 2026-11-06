# 原型 #4：Vogen 在 Infrastructure 中生成 EF 值转换器

- 状态：结论已得出，成立；没有待决事项（2026-11-06）
- 来源：08 第 4 节原型 #4（非阻断）；05-01 第 4.2 节“`待确认`：Vogen 8.0.x 中‘在其他项目生成 EF 转换器’的标记特性的确切名称与用法”
- 原型代码：与原型 #2 共用，分支 `spike/02-ef-datascope` 的 `spikes/02-ef-datascope/`（一次性代码，不合入主干）
- 环境：Vogen 8.0.7；EF Core 10.0.14；Npgsql.EntityFrameworkCore.PostgreSQL 10.0.3；PostgreSQL 18

## 1. 结论

**成立。** 05-01 第 4.2 节的写法可行：Domain 只声明 ID，Infrastructure 用 `[EfCoreConverter<T>]` 标记，Vogen 生成转换器和注册方法。`待确认` 关闭，不需要启用备选方案（按 `IStronglyTypedId` 扫描、泛型转换器统一注册）。

```csharp
// 模块 Infrastructure：每个 ID 一行标记
[EfCoreConverter<SampleId>]
[EfCoreConverter<UserId>]
[EfCoreConverter<OrgUnitId>]
internal sealed partial class VogenEfCoreConverters;

// 模块上下文
protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
    configurationBuilder.RegisterAllInVogenEfCoreConverters();
```

| 验证项 | 结果 |
|---|---|
| 生成的内容 | 每个 ID 一个 `ValueConverter<TId, Guid>` 和一个 `ValueComparer<TId>`；外加一个按标记类命名的注册方法 `RegisterAllIn{类名}()`，内容是逐个 `Properties<TId>().HaveConversion<转换器, 比较器>()`。注册方法是 `internal`，只能在本程序集中调用 |
| 读库时的校验 | 转换器经 `UnsafeAccessor` 调用 Vogen 生成的私有方法 `__Deserialize`，会执行 ID 的 `Validate`（读到 `Guid.Empty` 会抛异常）；全程不用反射 |
| 映射范围 | 主键、普通列、拥有集合的外键与复合主键、可空比较、作为参数的 `IReadOnlyList<OrgUnitId>`（数组元素同样转换，翻译为 `= ANY`），在 PG 18 上全部正确（原型 #2 的 13 项测试） |
| 漏写一个标记 | 第一次构建模型时抛出 `InvalidOperationException`：“The 'OrgUnitId' property 'Sample.OrgUnitId' could not be mapped because the database provider does not support this type”。任何使用上下文的测试都会失败，不会留到运行时 |
| Domain 的依赖 | Domain 项目只引用 Vogen（生成 JSON 转换器、`TypeConverter`），不引用 EF Core |

## 2. 文档写法的更正

05-03 第 3.2 节写的是“约定按 `IStronglyTypedId` 扫描模块程序集，统一注册”，与 05-01 第 4.2 节不一致：扫描是 05-01 的备选方案。按 05-01 更正为“由 Vogen 生成的注册方法统一注册”，已修订（2026-11-06，补充说明），只改写法，不改决定。

不改用扫描的理由：生成的转换器和比较器不用反射、读库时带校验，和选用源码生成式中介者（ADR-0005）的理由相同；代价是每个 ID 在标记类上多写一行，漏写会在模型构建时报错（第 1 节）。

## 3. 留到阶段 2 的事项

| 事项 | 说明 |
|---|---|
| 框架 ID 的转换器 | `UserId`、`RoleId`、`SessionId` 等由 BuildingBlocks 定义的 ID（05-02 第 7.1 节）在 `BuildingBlocks.Infrastructure` 中声明一次，由 `ModuleDbContext` 统一注册；模块只声明本模块的 ID 和 `XxxRef`。注册方法是 `internal`，基类需要给模块留一个调用本模块注册方法的钩子 |
| 模块模板 | 生成标记类，带上模块第一个聚合的 ID（与原型 #11 的模板正式内容一起做） |
| 架构测试 | 不需要单独检查“每个 ID 都有转换器”，漏写在模型构建时就失败 |
