# 原型 #5：不区分大小写的列与模糊匹配

- 状态：结论已得出；D1 已确认（2026-11-10，按建议，附下方补充）；已修订 05-03 第 3.7、9.3、11 节与 ADR-0030
- 来源：08 第 4 节原型 #5（非阻断）；ADR-0030 待确认；05-03 第 3.7 节待确认；06 第 11.2 节待确认
- 原型代码：分支 `spike/05-13-collation-rds` 的 `spikes/05-13-collation-rds/`（与原型 #13 共用；一次性代码，不合入主干）
- 环境：EF Core 10.0.14；Npgsql.EntityFrameworkCore.PostgreSQL 10.0.3；PostgreSQL 18.6（ICU 153）

## 1. 结论

**06 第 11.2 节的写法成立，三处 `待确认` 关闭；但排序规则不能按 05-03 原定的“建在模块 schema 中”来引用（D1）。**

| 待确认项 | 结论 |
|---|---|
| ICU 非确定性排序规则 `und-u-ks-level2` | 等值比较与唯一索引都不区分大小写（`ABC-1` 与 `abc-1` 冲突），**区分重音**（`Cafe` ≠ `Café`） |
| PG 18 在非确定性排序规则下的 `LIKE` | **支持**，并且不区分大小写（`LIKE '%abc%'` 命中 `ABC-001`、`abc-002`）；`ILIKE` 仍不支持（报错 “nondeterministic collations are not supported for ILIKE”） |
| `ContainsText` 的翻译 | 按 06 的写法实现为 EF 的方法翻译器插件，生成 `t."Name" COLLATE "C" ILIKE '%' \|\| replace(replace(replace(@term, '\', '\\'), '%', '\%'), '_', '\_') \|\| '%' ESCAPE '\'`；默认列与不区分大小写的列写法、结果一致；中文、`%`、`_`、`\` 都按普通字符匹配；`term` 始终是参数 |
| `StartsWithText` | 同样的翻译，只在末尾加 `%` |
| `pg_trgm` 索引 | 按 06 的表达式建 GIN 索引 `(("Name" COLLATE "C") gin_trgm_ops)` 后，上面的条件走 Bitmap Index Scan（2 万行） |

共 18 项测试（与原型 #13 合计），本机 PG 18 上全部通过，连续运行 3 次结果相同。

与 06 写法的一处差别：06 写的是“`@pattern = '%' + 转义后的 term + '%'`”，即在程序中转义后作为参数传入。翻译器拿到的是查询中的参数表达式，不能在程序中改写它的值，所以转义改为在数据库端用 `replace` 完成。效果相同；按实际参数值做计划时（原型 #2 D2：不开启自动预备语句）`replace(…)` 在计划时求值，trigram 索引照样可用（已验证）。

## 2. 待决事项

### D1 不区分大小写的排序规则建在 `public`，由迁移程序创建一次（已确认 2026-11-10，修订 05-03 第 3.7 节）

05-03 第 3.7 节定的是“排序规则由框架在每个模块的初始迁移中创建于模块 schema 内”。验证发现：

- **Npgsql 10 的 `UseCollation` 不支持带 schema 的名称**：`UseCollation("sample.CaseInsensitive")` 与 `UseCollation("\"sample\".\"CaseInsensitive\"")` 都生成 `COLLATE "sample.CaseInsensitive"`（被当作一个标识符），建表失败。只写名称时生成 `COLLATE "CaseInsensitive"`，按建表会话的 `search_path` 解析，而模块 schema 不在其中。
- 列在**建表时**按内部 ID 绑定排序规则，之后的查询与 `search_path` 无关。
- 如果模块 schema 和 `public` 中都有同名排序规则，不设 `search_path` 建表时会**悄悄绑到 `public` 的那个**。

| 选项 | 做法 | 优点 | 缺点 |
|---|---|---|---|
| **A 建在 `public`，迁移程序创建一次** | `MigrationRunner` 在执行任何模块迁移之前运行框架步骤 `CREATE COLLATION IF NOT EXISTS public."CaseInsensitive" (…)`；模块模型中不声明 `HasCollation`（否则每个模块的迁移都会尝试创建它）；`.IsCaseInsensitive()` 映射为 `UseCollation("CaseInsensitive")`。测试基础设施建库时执行同一个框架步骤 | 用 EF / Npgsql 的标准行为，迁移脚本中的列定义就是普通的 `COLLATE "CaseInsensitive"`；排序规则只有一份，不会出现同名绑错 | 模块的表依赖一个模块之外的对象（与 `pg_trgm` 扩展同类的数据库级定义，无状态）；以后拆出模块时要在新库中先建它 |
| B 建在模块 schema，迁移时设置 `search_path` | `MigrationRunner` 执行某个模块的迁移前 `SET search_path = "模块", public` | 每个模块自带排序规则 | 迁移脚本单独拿出来执行就会失败或绑错；正确性依赖会话设置，审查脚本时看不出来 |
| C 建在模块 schema，框架替换 Npgsql 的迁移 SQL 生成器 | 生成器输出 `COLLATE "模块"."CaseInsensitive"` | 脚本自包含、每个模块自带 | 框架要继承并改写 Npgsql 的迁移 SQL 生成器，跟随 Npgsql 升级维护 |

- **建议**：A。
- **理由**：排序规则是无状态的定义，和扩展一样适合数据库级只建一份；A 不需要任何非标准的写法。
- **取舍**：模块的表结构依赖 `public` 中的一个对象；迁移账号需要在 `public` 建对象的权限（PG 15 起默认收回了 `PUBLIC` 在 `public` 上的 `CREATE`，迁移账号是库的属主即可）。

## 3. 文档修订

| 文档 | 修订 | 时间 |
|---|---|---|
| ADR-0030、05-03 第 3.7 节、06 第 11.2 节 | `待确认` 关闭；06 补充“转义在数据库端完成” | 已修订（2026-11-06，补充说明） |
| 05-03 第 3.7、9.3 节 | 排序规则的位置与创建方式按 D1 | D1 确认后 |

## 4. 留到阶段 2 的事项

| 事项 | 说明 |
|---|---|
| 框架实现 | `KmicFunctions`（Application 中声明）+ 翻译器插件（Infrastructure，经 `IDbContextOptionsExtension` 注册，由 `ModuleDbContext` 统一添加）；`.IsCaseInsensitive()` 映射扩展 |
| 测试 | 本页第 1 节的场景进入框架测试（06 第 11.2 节测试要点） |

## 确认记录（2026-11-10）

- D1：按建议，排序规则建在 `public`，由迁移程序统一创建。补充：框架步骤是“核对”而不只是“创建”——`public` 中已有同名排序规则但定义不符（例如区分大小写）时迁移失败，因为单靠 `IF NOT EXISTS` 会继续沿用它，唯一约束的行为就会与设计不同；`search_path` 中排在 `public` 之前的 schema 不得有同名对象；迁移后核对每个不区分大小写的列实际绑定的排序规则（05-03 第 3.7 节）。迁移账号在 RDS 上的权限与行为随原型 #13 的云端验证确认。
