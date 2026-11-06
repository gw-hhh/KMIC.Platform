# 原型 #9：.NET 10 System.Text.Json 选项的名称、默认值与实际行为

- 状态：已确认（2026-11-06）：D1、D2 均按建议
- 来源：08 第 4 节原型 #9（非阻断）；05-06 第 5.1 节“`待确认`：.NET 10 中 `AllowDuplicateProperties`、`RespectNullableAnnotations` 的确切名称和默认值”
- 原型代码：分支 `spike/09-json-options` 的 `spikes/09-json-options/`（一次性代码，不合入主干）
- 环境：.NET SDK 10.0.403（System.Text.Json 10.0）

## 1. 结论

**成立。** 05-06 第 5.1 节用到的选项名称都正确，`待确认` 关闭。另有三个发现：.NET 10 新增的 `Strict` 预设正好覆盖 05-06 要的严格行为（D1）；属性名是否区分大小写，05-06 没有规定（D2）；“缺失 → 400”的说法需要更正（第 3 节）。

### 1.1 各预设的实际取值

| 选项 | `Default` | `Web` | **`Strict`（.NET 10 新增）** | ASP.NET Core 接口层默认 | 05-06 要求 |
|---|---|---|---|---|---|
| `PropertyNamingPolicy` | 无 | 小驼峰 | 无 | 小驼峰 | 小驼峰 |
| `PropertyNameCaseInsensitive` | false | true | false | **true** | 未规定（D2） |
| `NumberHandling` | `Strict` | `AllowReadingFromString` | `Strict` | **`AllowReadingFromString`** | `Strict` |
| `UnmappedMemberHandling` | `Skip` | `Skip` | `Disallow` | `Skip` | `Disallow`（05-06 D7） |
| `AllowDuplicateProperties` | true | true | false | true | false |
| `RespectNullableAnnotations` | false | false | true | false | true |
| `RespectRequiredConstructorParameters` | false | false | true | false | true |
| `DefaultIgnoreCondition` | `Never` | `Never` | `Never` | `Never` | `Never` |
| `MaxDepth` | 0（即 64） | 0 | 0 | 0 | 32 |

ASP.NET Core 接口层默认基于 `Web` 预设：数字可以用字符串传，属性名不区分大小写。框架配置如果只“补几项”，数字的严格检查会悄悄失效，必须逐项显式设置。

### 1.2 实际行为（在 `Strict` 预设上加小驼峰、字符串枚举、`MaxDepth = 32`）

| 输入 | 结果 | 错误位置 |
|---|---|---|
| 同名属性出现两次 | 拒绝 | `$.title` |
| 未知字段 | 拒绝 | `$.extra` |
| 不可空的构造参数或属性收到 `null` | 拒绝 | `$.title`、`$.remark` |
| 缺少构造参数、缺少 `required` 属性 | 拒绝 | `$`（字段名只在消息文字中） |
| 缺少不可空、但既非构造参数也非 `required` 的属性 | **接受**，保留初始值 | — |
| 数字写成字符串（`"1"`） | 拒绝 | `$.quantity` |
| 枚举写成整数 | 拒绝（`allowIntegerValues: false`） | `$.kind` |
| 枚举名大小写不同（`"urgent"`） | 接受（内置转换器读取时不区分大小写，没有选项可改） | — |
| 属性名大小写不同（`"Title"`） | 拒绝（当作未知字段） | `$.Title` |
| 嵌套超过 32 层 | 拒绝 | 到第 32 层的路径 |
| 不带时区偏移的时刻（`2026-11-04T08:30:00`） | 内置行为**接受**，当作服务器本地时间 | — |
| 序列化值为 `null` 的属性 | 输出 `"note":null`，不省略 | — |

共 17 项测试，全部通过。

## 2. 决定

### D1 以 `Strict` 预设为基准（已确认，2026-11-06）

| 选项 | 做法 | 优点 | 缺点 |
|---|---|---|---|
| **A 以 .NET 10 的 `Strict` 预设为基准** | `KmicJsonOptions.Create()` 从 `JsonSerializerOptions.Strict` 复制，再加小驼峰、转换器、`MaxDepth = 32`；接口层不能替换 ASP.NET Core 的选项实例，由 `KmicJsonOptions.Configure(target)` 逐项写入同样的取值；用一个测试逐项比较两者的全部公开设置，保证 HTTP、幂等记录、Outbox 的序列化结果一致（05-06 第 5.1 节要求） | 用的是平台为“严格输入”提供的标准组合，以后 .NET 在 `Strict` 中加入新的严格项，非 HTTP 路径直接得到，比较测试会提示接口层同步 | 依赖 `Strict` 预设的内容（.NET 文档有列出，且有测试守住） |
| B 逐项手写 | 按 05-06 的表逐项设置 | 每一项都显式可见 | 新增的严格项要自己发现；两处配置靠人工保持一致 |

- **决定（已确认，2026-11-06）**：A。
- **理由**：两种做法最终取值相同，A 多了“平台标准组合”这个基准和自动比较，长期维护更可靠。
- **取舍**：要理解 `Strict` 预设包含哪些设置（上表已列出）。

### D2 属性名区分大小写（已确认，2026-11-06；05-06 原未规定）

| 选项 | 做法 | 优点 | 缺点 |
|---|---|---|---|
| **A 区分大小写** | `PropertyNameCaseInsensitive = false`（`Strict` 预设的取值） | 与“未知字段即拒绝”（05-06 D7）一致：`"Title"` 写错大小写会得到 400 和字段路径，而不是碰巧被接受；匹配更快 | 手写调用（调试工具、外部系统）必须用准确的小驼峰 |
| B 不区分大小写 | ASP.NET Core 默认 | 手写调用更宽容 | 同一个字段有多种写法都能通过，接口契约变得含糊 |

- **决定（已确认，2026-11-06）**：A。
- **理由**：前端用生成的类型调用，总是准确的小驼峰；接口契约应该只有一种写法。
- **取舍**：外部系统对接时要按文档写准字段名。

## 3. 需要更正与补充的写法

已修订 05-06 第 5.1 节（2026-11-06，补充说明）：

| 原文 | 更正 | 原因 |
|---|---|---|
| “可空性：不可空的属性收到 `null` 或缺失 → 400” | 收到 `null` → 400；**缺失**只有构造参数和 `required` 属性才 → 400，其余属性缺失时保留初始值（表示可选，带默认值） | 实测行为；与 06 第 5.2 节“DTO 必填属性用 `required`”配合，必填就必须标出来 |
| `待确认`：选项名称与默认值 | 关闭，取值见第 1.1 节 | 已核实 |
| （新增）属性名大小写 | 按 D2 | 原未规定 |
| （新增）接口层必须逐项覆盖 ASP.NET Core 默认值 | 尤其 `NumberHandling`、`PropertyNameCaseInsensitive` | 默认基于 `Web` 预设，比我们要的宽松 |

## 4. 留到阶段 2 的事项

| 事项 | 说明 |
|---|---|
| 缺少必填字段时的 400 响应 | 错误位置只到对象（`$`），字段名只在消息文字中，.NET 不提供结构化的缺失字段清单。前端用生成的类型时几乎不会发生；框架把这类错误映射为 `Common.ValidationFailed`，字段位置写 `$`，不解析消息文字 |
| 不带时区偏移的时刻 | 按 05-06 实现框架转换器拒绝；本原型已确认内置行为会接受，转换器是必要的 |
| 枚举名大小写 | 普通枚举的读取不区分大小写，无法关闭。普通枚举只用于不进数据库的技术性取值，影响很小；业务状态用智能枚举，由框架转换器严格匹配 `Code` |
