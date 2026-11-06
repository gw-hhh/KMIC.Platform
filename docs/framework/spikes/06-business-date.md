# 原型 #6：按业务日期分组的 EF 函数 `ToBusinessDate`

- 状态：结论已得出，成立；没有待决事项（2026-11-06）
- 来源：08 第 4 节原型 #6（非阻断）；05-13 第 3.5 节
- 原型代码：分支 `spike/06-07-10-ef-and-testing` 的 `spikes/06-07-10-ef-and-testing/`（与原型 #7、#10 共用；一次性代码，不合入主干）
- 环境：EF Core 10.0.14；Npgsql.EntityFrameworkCore.PostgreSQL 10.0.3；PostgreSQL 18

## 1. 结论

**成立。** `KmicFunctions.ToBusinessDate(DateTimeOffset)` 用 EF 的方法翻译器插件实现（与原型 #5 的 `ContainsText` 同一个插件机制），生成：

```sql
SELECT CAST(e."OccurredAt" AT TIME ZONE 'Asia/Shanghai' AS date) AS "Day", count(*)::int AS "Count"
FROM app."Event" AS e
WHERE e."Id" = ANY (@ids)
GROUP BY CAST(e."OccurredAt" AT TIME ZONE 'Asia/Shanghai' AS date)
```

| 场景 | 结果 |
|---|---|
| 上海时区，跨越当地午夜的四个时刻（`15:59:59Z`、`16:00:00Z` 等） | 分组结果与 .NET `TimeZoneInfo` 的换算一致 |
| 纽约时区，2026-11-01 夏令时结束（当天 25 小时） | 一致 |

2 项测试通过，连续运行 3 次结果相同。

## 2. 写法补充

05-13 第 3.5 节写的是“时区作为参数传入（不写死字符串）”。实现中时区**作为常量写入 SQL**：业务时区是部署配置，进程内不变；翻译器在 EF 的内部服务中运行，拿不到查询之外的参数值，只能从选项扩展中取得配置的时区。原文“不写死字符串”的意思（不在代码中写死）不变：

- 时区来自配置，启动时已校验可解析（05-13 第 3.3 节），常量由 EF 按字符串字面量转义；
- 同一进程内 SQL 文本相同，不影响查询缓存与执行计划；
- 选项扩展把时区计入内部服务提供者的缓存键，时区不同的上下文不会共用翻译结果（测试中两个时区交替使用验证了这一点）。

已修订 05-13 第 3.5 节（2026-11-06，补充说明）。

## 3. 留到阶段 2 的事项

`KmicFunctions` 的声明放在 Application，翻译器与选项扩展放在 Infrastructure，由 `ModuleDbContext` 统一注册；“`ToBusinessDate` 出现在 `Where` 中”的分析器提示（05-13 第 3.4 节）一并实现。
