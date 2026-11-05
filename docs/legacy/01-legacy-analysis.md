# 科隆网页版销售工作平台 现状分析

> 用途：为后续 .NET 10 + Vue 3 重构做现状摸底。本文只记录现状，不含任何代码改动。
> 分析日期：2026-11-03；代码基线：`main` @ `1d500fe`；数据库统计来自测试库 `47.102.147.170`（只读查询）。
> 标注说明：`假设` = 根据代码推断、未实际验证；`待确认` = 需要业务或运维确认。

---

## 0. 结论速览

- **体量**：一个 WebForms 单体，约 750 个 aspx 页面、166 个 ashx 接口，`Web/Manage` 下 C# 约 24 万行，DAL 12 万行，aspx 标记 43 万行；4 个 SQL Server 库共约 340 张表。
- **架构**：名义上是 Model / DAL / BLL / Web 三层（动软代码生成器风格），实际上大量业务逻辑和 SQL 直接写在 ashx 和 aspx.cs 里，页面层直接操作数据库超过 1100 处。
- **前端**：layui 2.4.5 + jQuery 1.7.2 为主，约 290 个页面用 layui table + ashx 返回 JSON；另有约 50 个页面局部用了 Vue 2.6。基本是“服务端渲染页面壳 + Ajax 拉 JSON”，这对拆前后端是有利的。
- **最大风险**：
  1. 认证靠 DES 加密 Cookie 存用户 ID/角色，无服务端会话，密码明文比对；
  2. 大量字符串拼接 SQL（`strWhere` 模式贯穿 DAL）；
  3. 业务规则散落在 Web.config、数据库触发器、跨库视图、页面代码和硬编码用户 ID 里；
  4. 业务数据跨 4 个库读写，其中 `Kelong_Log` 不只是日志，还存业务快照。
- **重构建议方向**：按菜单模块分批迁移（绞杀者模式），先抽 API 和统一认证，数据库结构先保持不动。

---

## 1. 技术栈

### 1.1 后端

| 项 | 现状 |
|---|---|
| 运行时 | .NET Framework 4.8，IIS 托管，Web 项目是 Web Application（`Web.csproj`） |
| 语言 | C#（老式 csproj + `packages.config`） |
| Web 框架 | ASP.NET WebForms（aspx/ascx）+ 通用处理程序（ashx）；引用了 Web API 2 程序集但未见实际使用 |
| OWIN | `Microsoft.Owin` + `Startup.cs`，仅用来挂载 Hangfire（`[assembly: OwinStartup]` 已注释，依赖约定发现 `Web.Startup`，`待确认` 线上是否实际启动） |
| 构建/部署 | MSBuild 2019 + NuGet，`deploy-pre.bat` / `pre-to-produ.bat` 打包后 robocopy 到 IIS 目录，`脚本/*.template` 为 dev/pre/prod 构建模板 |
| 测试 | `Web.Tests` 不是标准测试项目，用 PowerShell 调 `csc.exe` 编译 + 自定义断言，覆盖 AR 账龄、特殊评审、服务收费等少数模块 |

### 1.2 主要第三方库

| 库 | 用途 | 所在项目 |
|---|---|---|
| Dapper / Dapper.Contrib | 新代码的数据访问（`DapperHelper.Kelong/AfterSale/Log/Order`） | DBUtility |
| FreeSql (+SqlServer Provider) | AR 账龄、ERP BP 两个新模块的 ORM | Web |
| EntityFramework 6 | 只有引用，未见业务使用（`假设` 可删除） | Web |
| Hangfire (Core/SqlServer/MemoryStorage) | 定时任务，存储在主库 | Web |
| Mapster | DTO 映射 | DTO |
| Newtonsoft.Json | 全局 JSON 序列化 | 多处 |
| NPOI / EPPlus / MiniExcel / MyXls | Excel 导入导出（四套并存） | Model / Web / OfficeHelper |
| Aspose.Words | Word 合同/报价单生成 | Web |
| iTextSharp | PDF 生成 | PCommon |
| ICSharpCode.SharpZipLib | 打包下载 | Web |
| MathNet.Numerics / ExtendedNumerics.BigDecimal | 统计/金额计算 | Web |
| aliyun-net-sdk-dysmsapi | 阿里云短信验证码 | Web |
| URLRewriter | 伪静态 URL 重写（规则多为早期遗留） | Web |
| AspNetPager | 服务端分页控件（旧页面） | Common / Web |
| ChnCharInfo | 汉字转拼音 | Web |
| JWT | 引用存在，未见核心使用 | Common |
| Microsoft.Office.Interop.Outlook | 引用存在，实际发信走 SMTP | PCommon |
| MySql.Data / System.Data.OracleClient | DBUtility 里有 MySQL/Oracle 帮助类，`假设` 未在业务中使用 | DBUtility |

### 1.3 前端

| 库 | 版本 | 使用量（按 aspx 引用次数） |
|---|---|---|
| layui（layuiadmin 后台框架） | 2.4.5 | 约 657 处 |
| jQuery | 1.7.2 为主，另有 1.3.2 / 1.4.2 / 1.8.3 / 2.1.1 / 2.2.4 / 3.7.1 混用 | 约 600+ 处 |
| formSelects-v4 / xm-select | 多选下拉 | 约 155 处 |
| remodal、SuperSlide | 弹层、轮播（旧页面） | 约 200+ 处 |
| Vue | 2.6.12（另有一份 3.5 prod 文件） | 约 50 个页面，集中在 `Weekly_New`、`Project_Up` |
| ECharts | 多版本 | 看板与统计页 |
| uploadify、Lodop、city-picker | 上传、打印、省市选择 | 少量 |

---

## 2. 项目结构与依赖

### 2.1 解决方案项目

| 项目 | 程序集/命名空间 | 职责 | 规模 |
|---|---|---|---|
| Model | `Jia.Model` | 表实体（动软生成，一表一类） | 247 文件 / 6 万行 |
| DTO | `DTO` | 新模块的查询参数与返回对象（Ar、ErpBp、Quotation、Common） | 27 文件 |
| DBUtility | `Maticsoft.DBUtility` | `DbHelperSQL`、`DbHelperSQL_AfterSale/Log/Order`、`DapperHelper`、连接串读取 | 20 文件 / 1.1 万行 |
| DAL | `Jia.DAL` | 每表 CRUD + `GetList(strWhere)` 等查询，SQL 拼接 | 234 文件 / 12.4 万行 |
| BLL | `Jia.BLL` | 大部分是 DAL 的透传，少量缓存与组合逻辑 | 233 文件 / 4.1 万行 |
| PCommon | `PCommon` | 发信（`OutlookDotComMail`）、节假日/工作日、PDF、字典读取 | 18 文件 |
| Common | `Jia.Common` | `BasePage` 页面基类、Cookie/DES、权限判断、上传、日志、错误邮件、全局快捷搜索等 | 56 文件 / 1.6 万行 |
| OfficeHelper | `OfficeHelper` | EPPlus 导入导出封装 | 3 文件 |
| Web | `Web` | 全部页面、ashx 接口、Hangfire 作业、部分新模块的 Service | 1613 cs / 752 aspx / 166 ashx |
| WeiXinLoginApi | — | 微信扫码登录封装；**Web 未引用**，`假设` 已废弃 | 8 文件 |

### 2.2 依赖关系

```
Web ──► Common ──► BLL ──► DAL ──► DBUtility
 │        │         │       └────► Model / DTO
 │        └► PCommon ┘
 ├──► BLL / DBUtility / Model / DTO / PCommon（Web 直接越层访问 DBUtility）
 └──► FreeSql、Hangfire、Aspose 等第三方
```

要点：
- **Common 依赖 BLL**，而 PCommon 也依赖 BLL，工具层和业务层互相缠绕。
- **Web 直接引用 DBUtility**，页面里 `DbHelperSQL.*` 调用约 1180 处、`DapperHelper.*` 约 166 处，三层架构基本被绕过。
- 新模块（AR 账龄、ERP BP）采用 “ashx + Service + FreeSql 实体” 结构，代码在 `Web/Manage/<模块>/` 下，是比较接近重构目标的形态。

### 2.3 仓库内杂项

- `Web` 根目录有大量测试/副本页：`Charts - 副本.aspx`、`WebForm1/2.aspx`、`ceshi*.aspx`、`TestOwin*.aspx`、`*.xlsx` 等。
- `Web/Manage/TeshuhetongShenqin2/GetJson.ashx - 副本.cs`（4779 行）等副本文件被编译进项目，`待确认`。
- `TestResults/FATLive/site` 是一份站点副本，搜索代码时容易误命中。
- `SQL_Scripts/`、`脚本/` 存放发版 SQL，没有迁移工具，库结构以“手工脚本 + 线上库”为准。

---

## 3. 业务模块清单

以 `Base_SysMenu` 菜单（444 项：25 个一级、142 个页面、277 个按钮权限）为准，对应 `Web/Manage` 下的目录：

| # | 菜单模块 | 主要目录 | 页面数 (aspx/ashx) | 复杂度 |
|---|---|---|---|---|
| 1 | 系统管理 | SysMenu, Role, UserInfo, UserCode, Industry, ZhiCheng, CustomerCode, ProjectCode, SystemSet, LoginLog, SICCode, AttachmentMaintenance, Authorization | ~45 / ~12 | 中 |
| 2 | BP 管理（客户/代理商/Leads） | Customer_Up, Customer, AgentManagement, Leads, Email(BP解锁) | ~68 / ~13 | 高 |
| 3 | 商机管理 | Project_Up, Project, Project_Up/Pinggu, Charts(业绩) | ~110 / ~20 | 很高 |
| 4 | 报价 & 合同 | Quotation, Quotation_Temp, Quotation_Temp2, Quotation_RFQ, Quotation_Limit, Permit_ZSH, TeshuPingshenNum | ~120 / ~27 | 很高 |
| 5 | 任务平台（RFQ / 文控 / Order 排产 / BP 项目审核） | QuotationShenhe, QuotationPaichan, Task | ~90 / ~10 | 很高 |
| 6 | 特殊合同 / 规格 / 交货期评审 | TeshuhetongShenqin2, Quotation(Pingshen_*) | ~60 / ~3 | 很高 |
| 7 | 技术服务（售后） | AfterSale（Service、TechnicalExchange、Repair、Order、Set） | 77 / 12 | 高 |
| 8 | 每日活动 / 周报 | Weekly_New, Weekly, WeeklyActivity | ~44 / ~10 | 中 |
| 9 | Open Order（订单跟踪） | OpenOrder | 7 / 1 | 中 |
| 10 | 收款信息 + 承兑汇票 | Bank_Order | 20 / 4 | 中 |
| 11 | Forecast 预测与业绩目标 | Target, Charts(Achievement_*), Order, ErpBP, Address | ~30 / ~4 | 中高 |
| 12 | AR 账龄管理 | ARAging | 3 / 1 | 中（新） |
| 13 | Sales Worktop / 人事行政 | WorkTop, HR | 23 / 2 | 低 |
| 14 | 模板与消息 | TempManage, AfterSale/Set(EmailTemp), Message, leaveMessage, fankuixinxi | ~10 / ~7 | 低 |
| 15 | 回收箱 | DeleteList | 4 / 4 | 低 |
| 16 | 产品配置器对接 | Ajax/GetConfiguratorToken, ConfigReport, Jobs/PriceConfigSCPJob | 3 / 2 | 中（外部系统） |
| 17 | 首页 / 看板 / 全局搜索 | Index.aspx, WebForm_* 各角色首页, Charts, Ajax/GlobalQuickSearch | ~40 / ~3 | 中 |

### 3.1 各模块核心实体与主要流程

> 表名后的数字为测试库大致行数，用来判断数据量级。

#### 1）系统管理
- **实体**：`Base_UserInfo`（用户，含上级 `ZhuyaoShangjiID/2/3`、大区 `DayuId`、多角色 `RoleId_List`）、`Base_Role`、`Base_SysMenu`、`Base_Role_Class`（角色-菜单）、`Base_User_Class`（用户-菜单，直接给人加权限）、`Base_UserCode`（大区）、`Base_User_Parent`/`Base_UserSee`/`Base_UserDaquFenzu`（数据可见范围）、`Base_Dictionary`、`PreferenceConfig`（JSON 配置）、`J_LoginLog`（39 万）。
- **流程**：菜单树维护 → 角色/用户授权到菜单或按钮 → 登录后按“当前角色 + 用户个人授权”合并计算权限；管理员可“切换身份”模拟其他用户。
- **附件运维中心**：按配置的 SQL 模板查/改业务附件，配置存在库里（`SQL_Scripts/20260708_*`）。

#### 2）BP 管理
- **实体**：`Base_Customer`（6.6 万，87 列）、`Base_Agent`（代理商/中间商）、`Base_Customer_Baifang`（拜访）、`Base_CustomerDaoRuData`（Leads 导入）、`ERP_BP_Data/ERP_BP_ALL`（ERP 同步 BP）、`Base_Customer_AnnualStatistics`、联系人、`Email_BPUnLock`（在售后库）。
- **流程**：Leads 导入 → 销售认领 → 审核（进入 BP & 项目审核任务）→ 成为客户；客户有负责人、锁定（`IsLock`）与解锁审批；OEM 申请；SIC/国标行业/客户价值等字段带单独按钮权限（`PermissionNames`）。
- **隐性规则**：敏感客户名关键词（配置 `MinganBP`）、特殊 BP 名单（`TeshuKehuBP`）、BP 锁定解锁的最后审批人写死在配置 `BPUnLockLastShenpi`。

#### 3）商机管理
- **实体**：`Base_ProjectTrack_Up`（商机主表，144 列，6 万）、`Base_ProjectTrack_pruduct`（商机产品，24 万）、MRO/OEM 分支表（`Base_ProjectTrack_MRO*`、`Base_ProjectTrack_OEM*`）、`Base_ProjectTrack_KuaquyuQueren`（跨区域确认）、`Base_ProjectTrack_Up_Lingqu`（认领）、大项目评估（Pinggu）、年度订单目标。
- **流程**：新建/导入商机 → 认领 → 跟进更新（字段校验规则可配置，`YanzhengZiduan`）→ 跨区域项目需对方确认 → 中标/结案 → 与报价单同步（`20261103_优化_商机报价同步.sql`）。
- **隐性规则**：商机更新有效期（`ProjectUpdateUseDate`）、预计结案日期规则；`Project_UP_Fuji/Ziji` 存储过程做上下级层级查询；`Project_Up/GetJson.ashx.cs` 单文件 1.37 万行，是最大的单点。

#### 4）报价 & 合同
- **实体**：`Base_QUOTATION`（报价单，156 列，10 万）、`Base_QUOTATION_Shenhe`（报价审核）、`Base_QUOTATION_HeTong`（合同，112 列，8 万）、`Base_QUOTATION_HeTong_Fendan_*`（分单申请/审批）、`Base_QUOTATION_HeTong_QianyuefangInfo`（签约方）、`Base_QUOTATION_Hetong_SOAFile/SignFile`、`Base_QUOTATION_HeTong_ERP_SONum`（主库与订单库各一份）、`Base_QUOTATION_HeTong_DomesticLogisticsProcess`（物流）、模板表 `QUOTATION_Temp/Temp2/Temp_t`、`Rule_BaojiaHetong*`（规则）。
- **流程**：报价单（可委托报价给 RFQ 部门）→ 报价入库审核 → 盖章任务（RFQ/文控）→ 生成合同（Aspose 套模板生成 Word/PDF）→ 合同审核 → 排产任务 → ERP SO 号回填 → 物流/签收单。
- **隐性规则**：报价单号由页面计算后拼接“报价属性”后缀；税率写在配置 `Shuilv`；价格类型“不调整”名单 `PriceType_butiaozheng`；中石化框架客户识别依赖配置 `zhongshihua` 里的【ZSE】等标签；国外工厂列表 `GuowaiGongchang`；修改报价需要密码（配置 `BaojiaPwd`，明文）。

#### 5）任务平台
- **实体**：`Base_QUOTATION_GaizhangShenhe`（RFQ/文控任务，20 万，60 列，多个状态字段）、`Base_HeTongPaichan`（排产任务，9 万）、`RFQ_Assign_Tasks/Gaizhang/Shouquan`（分配与授权）、`Rule_BaojiaHetongType`/`Rule_PaichanType`（任务分类，`Quanxian` 字段用 `like '%用户ID%'` 存人员）、`Base_ShenHeMessage`（审批消息，25 万）、`Base_RenwuRead`、`Base_QUOTATION_GaizhangShenhe_Pingfen`（委托报价打分）。
- **流程**：业务单据提交 → 按规则生成任务并归类 → 平台列表 / 组认领 / 个人认领 → 处理（通过/驳回/转交）→ 回写源单据状态 → 发邮件通知。
- **隐性规则**：任务可见范围由 `BasePage.GetShenheType` 按“是否经理权限按钮 10224/10267”拼接 where；多种“全部任务”页面（大区经理/文控/文控专项）是同一逻辑的不同过滤。

#### 6）特殊合同 / 规格 / 交货期评审
- **实体**：主库 `Base_QUOTATION_Teshu_pingshen`（规格/交货期评审，55 列）、`Base_QUOTATION_PingshenNum`、`Base_QUOTATION_SQ_PingshenNum`、`TeshuPingshen_*`（年度评审号）；**售后库** `TeshuPingshen_Main`（109 列）、`TeshuPingshen_Shenqing/Product/Result/Daoru/Price_zhixing/FrameworkContract/Hetong_Fapiao`、`Teshupingshen_ShenpiList`（审批人员）；**日志库** 同名 `Base_QUOTATION_Teshu_pingshen` 存任务级快照。
- **流程（规格评审）**：销售选商机或报价单 → 保存草稿（`IsShenhe=-1`）→ 提交生成 RFQ 任务（`GaizhangShenhe`）并在日志库写快照 → RFQ 认领 → 生成新 SQ / 调用已有 SQ / 技术答复 → 完成评审回写三处；修改评审时新建记录并以 `SourcePingshenID` 指向原评审。详见 `docs/特殊规格评审流程图.html`。
- **流程（特殊合同评审）**：多级审批（区域、集团管理、工厂价格），审批人配置在表和 Web.config（`Teshupingshen_IsKOGJingliID`、`Teshupingshen_Jituanguanli` 等硬编码用户 ID）；有超时自动通过的定时任务（当前注释停用）。
- **隐性规则**：合同版本由触发器 `Update_hetongbanben` 跨库读取 `Kelong_Shouhuo.TeshuPingshen_Main` 计算。

#### 7）技术服务（售后）— 主要在 `Kelong_Shouhuo` 库
- **实体**：`Table_Service`（服务申请，93 列）、`Table_ServiceList`、`Table_ServiceRiji`、`Table_Service_Xianshang`（线上服务）、`Table_Gongshi*`（工时）、`Repair_*`（维修登记→预检→跟进→结案→开票→发货）、`Table_Exchange*`（技术交流）、`Pingfen`（评分）、`Table_Email`（邮件模板，Forecast 等其他模块也从这里读模板）。
- **流程**：服务/技术交流申请 → 一级审批 → 派工 → 服务日记/工时 → 结案 → 评分（时效 `ShohouPinfenShixiao`）；维修有每 10 个工作日自动提醒（`RepairReminderJob`）。
- **规模**：`AfterSale/SendEmail.cs` 单文件 6300 行，邮件内容大量写在代码里。

#### 8）每日活动 / 周报
- **实体**：`WeeklyGoOut`（31 万）、`WeeklyActivity`（23 万）、`Base_WeeklyPlan`、`WeeklyPizhu`（批阅）、`Base_WeeklyUser`、`Base_User_Parent_Weekly`（周报上下级）。
- **流程**：销售按日历填写活动 → 上级批阅 → 统计提交情况。旧版周报（改版前）菜单仍保留只读查询。
- **隐性规则**：节假日/调休影响应填天数（`SystemSet/JieJiaDate.aspx`、`xmlconfig/holidayConfig.json`、`PCommon/HolidayHelper`）。

#### 9）Open Order
- **实体**：`OpenOrder_Status`、`OpenOrder_Status_History`（226 万，全库最大）、`OpenOrder_Product`、`OpenOrder_DataLog`、`OpenOrder_VisitorLog`、`OpenOrder_JinkouBaoguan*`（进口报关）、`OpenOrder_DeliveryInvoiceDate`。
- **流程**：导入 ERP 订单状态数据 → 按合同/客户查询订单进度 → 记录访客日志。导入会写全量历史，数据增长最快。

#### 10）收款信息 + 承兑汇票 — `kelong_order` 库
- **实体**：`Collection`（收款）、`BankAcceptanceBill`（承兑汇票）、`SubLedger/SubLedgerInfo`（明细账）、`DB_Documents`、`Base_QUOTATION_HeTong_ERP_SONum`，每张表都有对应 `*_Log` 修改记录表。
- **说明**：该库有 `__EFMigrationsHistory` 与 `Users` 表，`待确认` 是否有另一个 .NET Core / EF 系统也在写这个库。

#### 11）Forecast 预测与业绩
- **实体**：`Achievement_*`（目标、审批 `Achievement_Shenpi`）、`SalesTargets`、`Order_Info_Daoru_All`（40 万，Incoming 导入）、`Order_Info_Shangji*`、`ERP_BP_*`、`Base_ReceivingAddress`（收货地址库）。
- **流程**：开启填写 → 销售填写 → 上级查看批阅 → 导入 Actual/Incoming → 统计报表（`sp_get_*_monthly_summary_*` 系列存储过程）；配置年份 `CurrentYear` 会被页面写回 Web.config。
- **对外**：`PowerBI_ForecastTarget` 视图供 Power BI 读取（`待确认` 报表方）。

#### 12）AR 账龄
- **实体**（FreeSql）：`ArTransaction`、`ArTransactionInstallment`、`ArPaymentTerm`、`ArImportStaging`、`ArManualAdjustment`、`ArSalesReply`、`ArTermOperationLog/Detail`。
- **流程**：财务导入 → 预检 → 入库 → 按账期计算账龄（`AgingCalculator`）→ 销售回复 → 看板。代码结构最新、有单元测试，可作为重构样板。

#### 13）其他
- **Sales Worktop / 人事行政**：类别 + 内容 + 权限的简单 CMS。
- **模板与消息**：消息模板、两套邮件模板（主库 + 售后库 `Table_Email`）、邮件发送失败记录。
- **回收箱**：客户、代理商、商机、报价单的软删除恢复（`DeleteMark`/`IsDel`）。
- **产品配置器**：外部系统 `configurator.krohnechina.com`（及测试版 `price.krohnechina.com`），菜单 URL 里用 `{{AuthorizeToken}}` 占位，由 `GetConfiguratorToken.ashx` 生成令牌跳转；每天 2 点同步特殊规格价格联动。

---

## 4. 数据层

### 4.1 数据库

SQL Server 2016，4 个库都在同一实例，靠连接串和**跨库三段式名称**互访：

| 库 | 表 | 视图 | 存储过程 | 触发器 | 函数 | 外键 | 用途 |
|---|---|---|---|---|---|---|---|
| KelongV2 | 192 | 74 | 26 | 10 | 2 | **2** | 主业务库，含 Hangfire 表 |
| Kelong_Shouhuo | 54 | 23 | 4 | 4 | 0 | 0 | 售后、特殊合同评审、部分邮件模板 |
| Kelong_Log | 75 | 3 | 2 | 1 | 0 | 0 | 操作日志、邮件发送记录，**以及业务快照** |
| kelong_order | 16 | 4 | 2 | 1 | 0 | 0 | 收款、承兑汇票、明细账 |

- 主库只有 2 个外键、25 张表没有主键，表间关系几乎全靠代码约定（如 `QID`、`HTID`、`ProjectTrack_ID`、`PingshenID`、`QSID`）。
- 4 个库里都有 `dts_*` 表和 DDL 触发器 `dts_cdc_sync_ddl`，说明接入了阿里云 DTS 数据同步（`待确认` 同步到哪里、谁在用）。
- 存在 `Table_1`、`Table_2`、`aaaa`、`KelongTempBackup*` 等临时/备份表，其中 `Table_2` 有 1 万行，`待确认` 是否有代码引用。
- 主库大宽表：`Base_QUOTATION` 156 列、`Base_ProjectTrack_Up` 144 列、`Base_QUOTATION_HeTong` 112 列、`TeshuPingshen_Main` 109 列。

### 4.2 数据访问方式（四套并存）

| 方式 | 使用位置 | 特点 |
|---|---|---|
| 动软 DAL + `DbHelperSQL` | DAL/BLL 及大部分旧页面 | `GetList(string strWhere)`、`GetModelList(strWhere)`，where 条件由调用方拼字符串；DAL 中 `strWhere` 出现约 1675 处，只有约 95 个文件用了 `SqlParameter` |
| 页面直写 SQL | ashx / aspx.cs | `DbHelperSQL.Query/ExecuteSql` 约 1180 处，相当一部分直接拼接请求参数 |
| `DapperHelper`（Kelong/AfterSale/Log/Order） | 新增代码、定时任务 | 参数化，项目规则要求新代码优先用它 |
| FreeSql | AR 账龄、ERP BP | 实体映射，单例 `IFreeSql` |

事务使用约 130 处（`ExecuteSqlTran`、`BeginTransaction`），多数跨表写入没有事务包裹，跨库写入无法保证一致性。

### 4.3 存储过程 / 触发器 / 视图

- **存储过程（主库 26 个）**：
  - 统计报表：`sp_get_One/More_user/daqu_monthly_summary_CRK/Order(_New)`、`sp_OEM报表`、`sp_OEM报表产品`、`sp_OEM_商机中标月份透视`；
  - 层级查询：`Project_UP_Fuji`、`Project_UP_Fuji_zubie`、`Project_UP_Ziji`、`QUOTATION_Temp2_Fuji/Ziji`；
  - Leads 提醒：`Leads_message_1/2/3`、`Leads_message_daqu_1/2/3`；
  - 批处理：`sp_AutoMarkPerformanceIndustry`（跨库，客户行业标识，每天 0 点由 Hangfire 调用）、`BackupData`；
  - DTS 相关：`capture_clean_proc`、`dts_source_instance_identify`。
- **触发器（主库 9 个业务触发器）**：`Update_UserName`、`Update_Name_New1`、`Update_A_Name`、`Update_IndustryName`、`Update_ProjectTrackName`、`Update_ProjectTrack_MROName/OEMName`、`Update_info`、`Update_hetongbanben`。
  - 主要作用是**同步冗余名称字段**：如改用户名/大区时，批量更新客户、导入数据、商机等表里的 `FzrNames`、`Xiaoshou` 等；
  - 写法为 `SELECT @x = col FROM INSERTED`，**只处理单行更新**，批量更新时只同步其中一行；
  - 触发器内部会临时 `disable/enable trigger all`，并发时有风险；
  - `Update_hetongbanben` 跨库读 `Kelong_Shouhuo` 计算合同版本。
- **视图（主库 74 个）**：大量 `Daochu_*`（导出用）、`View_*`（列表用）、`Select_Jiansuo_*`（检索用），部分跨库（`Daochu_Gaizhang_Renwu*`、`View_ProjectTrack_UpList`、`PowerBI_ForecastTarget`）。导出口径基本固化在视图里。
- **函数**：`fn_split`（字符串拆分，配合 `in` 查询）、`f_str`。

### 4.4 核心关系（约定，非外键）

```
Base_UserInfo ─┬─< Base_Customer (FzrIDs)            Base_UserCode(大区) ─< Base_UserInfo.DayuId
               ├─< Base_ProjectTrack_Up (FzrIDs)
               └─< 各类任务的认领人/申请人
Base_Customer ─< Base_ProjectTrack_Up ─< Base_ProjectTrack_pruduct
Base_ProjectTrack_Up ─< Base_QUOTATION (ProjectTrack_ID) ─< Base_QUOTATION_HeTong (QID)
Base_QUOTATION_HeTong ─< Base_HeTongPaichan / Fendan_* / ERP_SONum / 物流 / 签收单
Base_QUOTATION ─< Base_QUOTATION_GaizhangShenhe (QID, QSID) ─< Kelong_Log.Base_QUOTATION_Teshu_pingshen (QSID)
Base_QUOTATION_Teshu_pingshen (PingshenID) ─< Base_QUOTATION_GaizhangShenhe.Yejibiao_PingshenID
Base_QUOTATION_PingshenNum (HTID, shenqingid) ── Kelong_Shouhuo.TeshuPingshen_Main (TY_ShenqingID)
```

---

## 5. 横切关注点

### 5.1 认证
- 登录入口：`Manage/Login.aspx` + `Manage/Ajax/LoginOn.ashx`（账号密码）、`PhoneLoginOn.ashx`（手机验证码，可选）、`LoginYanzheng.ashx`；售后另有一套 `AfterSale/Ajax/PhoneLoginOn.ashx`。
- **密码明文存储、明文比对**（`modeAdmin.UserPwd != pwd`，MD5 代码已注释）。
- 登录成功后把 `ab`(用户ID)、`role`(当前角色)、`abName`、`gonghao`、`roleList`、`hyid`、`daquid` 等用 DES 加密写入 Cookie，**没有服务端 Session**，每次请求从 Cookie 解密取身份。DES 密钥在配置中。
- 另生成 `DengluBiaoshi_PC/PHONE` 登录标识写到用户表和 Cookie `Token`，用于单点登录挤下线（`假设`，未完整核实）。
- 短信验证码存在 Cookie（`CheckCode`、`CheckPhones`），可被客户端读取篡改，`待确认` 线上是否开启（`DuanxianYanzhengma`）。
- 每个 ashx 自己判断 `userid` 是否为空；只有 1 个文件用了统一的 `ApiAuthenticationHelper`。漏写判断即未授权访问。

### 5.2 授权
- 菜单/按钮统一在 `Base_SysMenu`（`MenuType` 1=目录 2=页面 3=按钮），权限 = 当前角色的 `Base_Role_Class` ∪ 用户个人 `Base_User_Class`。
- 页面通过 `BasePage.PermissionID` + `GetQuanxian(url)` 校验，按钮通过 `BasePage.SetQX(按钮ID)` 判断，**按钮 ID 以数字硬编码在代码里**（如 10224、10267、10545）。
- 数据范围（看谁的数据）散落在各模块：上级链 `ZhuyaoShangjiID/2/3`、`Base_User_Parent`、`Base_UserSee`、大区、行业、`Rule_*.Quanxian like '%ID%'`，约 170 处相关代码，没有统一的数据权限服务。
- 不少审批节点直接用 Web.config 里的用户 ID（`Teshupingshen_IsKOGJingliID`、`Teshupingshen_Jituanguanli`、`BPUnLockLastShenpi`、`ZurenlingRenwuType` 等）。
- 管理员可“切换身份”，本质是改写 Cookie 中的用户/角色。

### 5.3 日志
- 文本日志：`Jia.Common.WriteInLog` 写 `Web/Log` 目录（开关 `LogInFile`）。
- 业务操作日志：分散写到 `Kelong_Log` 库（`Base_ProjectTrackUpdate_Log`、`RFQ_Order_CaozuoLog`、`Repair_Log`、`WeeklyGoOut_log` 等），以及主库 `*_Log` 表、`kelong_order` 的 `*_Log` 表。
- 导出日志：`isSaveToExcelLog` 控制，导出文件留存在 `userfiles/ToExcelLog*`。
- 登录日志：`J_LoginLog`。

### 5.4 异常处理
- `BasePage.PageBase_Error`：写文本日志后用 `alert` 把异常消息直接弹给用户。
- `Global.Application_Error`：写日志 + 发错误邮件；同类错误 1 小时内去重，白名单规则存在字典表（`GLOBAL_ERROR_EMAIL_WHITELIST_*`），邮件模板也在字典表。
- ashx 多为 `try/catch` 后返回 `{"code":1,"msg":...}`，返回结构不统一（`code`/`msg`/`count`/`data` 是 layui table 约定，但各接口细节不同）。

### 5.5 缓存
- `HttpRuntime.Cache`（`DataCache`）：菜单权限 `GetTableByCache`、错误邮件去重等。
- 配置项 `ModelCache`、`DataTableCache` 当前为 0（关闭）。
- **`BasePage.btnds` 是 `public static DataSet`**，所有用户共享，存在并发串数据风险。
- 无分布式缓存，多实例部署时缓存不一致。

### 5.6 配置
- 全部在 `Web.config` `appSettings`，约 130 项，混杂：连接串、SMTP 账号密码、短信密钥、业务开关、审批人 ID、税率、年份、产品名单、邮件抄送人。
- **运行时写回 Web.config**：`ConfigHelper.WriteConfig` 约 79 处（`CurrentYear`、`SystemSwitch`、`PageSize`、邮件收件人等）。每次写入都会触发应用池重启，且多服务器不同步。
- `Index.aspx.cs` 里有按日期 `2023-03-01` 触发的一次性改配置 + 改数据代码。
- 另有 `PreferenceConfig`（JSON）、`Base_Dictionary`、`Rule_Tongyong` 三处存配置，与 Web.config 并存。
- `ConStringEncrypt=false`，连接串明文；Web.config、`CommonToken.SecretKey`、Hangfire 面板账号都有明文密钥。

### 5.7 定时任务（Hangfire，存储在主库）

| 任务 | 时间 | 状态 | 说明 |
|---|---|---|---|
| 客户行业标识任务 | 每天 0:00 | 启用 | 调 `sp_AutoMarkPerformanceIndustry` 等 SQL |
| 维修提醒任务 | 每天 7:00 | 启用 | 售后库，按工作日每 10 天提醒 |
| 配置器-自动刷新特殊规格联动 | 每天 2:00 | 启用 | 调配置器 HTTP 接口 |
| Forecast 任务 | — | 注释 | 开启填写通知、未提交提醒 |
| 特殊合同评审审批超时检查 | 每天 10:00 | 注释 | 超时提醒与自动通过，也可通过 ashx 手动触发 |

- 由 `EnableRecurringJobs` 控制，Hangfire Worker 跑在 Web 进程里，`WorkerCount = CPU × 5`；多台 Web 服务器时会同时抢任务（Hangfire 本身有分布式锁，但 `DisableGlobalLocks=true`）。
- `/hangfire` 面板用 Basic 认证，账号密码写死在代码里。
- 另有不走 Hangfire 的“定时”：页面访问时顺带执行的逻辑（如首页按日期执行的代码）、SQL Server Agent 作业（`待确认`）。

### 5.8 文件存储
- 全部存本地磁盘 `Web/userfiles/...`（报价附件、合同、签收单、导出日志等），路径以相对 URL 存在业务表字段里。
- 上传大小 200MB（`FileMaxSize`），IIS 允许 1GB；有扩展名黑名单 `UploadBlockedExtensions` 和 `UploadSecurityHelper`，旧上传组件（uploadify、`UpLoadFile`、`UploadFile_1`、`Upload`）有多套，`待确认` 是否都走了安全校验。
- `VirtualPath` 配置为内网地址，部分文件链接拼接该地址。
- 静态 MIME 配置 `.*` → `application/force-download`。

### 5.9 第三方集成

| 对象 | 方式 | 位置 |
|---|---|---|
| 邮件 | SMTP，4 组发件账号（默认、盖章、售后、Outlook 365），按场景选择 | `PCommon/OutlookDotComMail`，`AfterSale/SendEmail.cs` |
| 阿里云短信 | SDK，登录验证码 | 登录相关 ashx |
| 产品配置器 | 令牌跳转 + HTTP 同步 | `GetConfiguratorToken.ashx`、`PriceConfigSCPJob` |
| ERP（`假设` SAP） | Excel/数据导入：BP、Incoming、Actual、Open Order、SO 号 | ErpBP、Order、OpenOrder、Bank_Order |
| Power BI | 直接读视图 `PowerBI_ForecastTarget` | 数据库 |
| 阿里云 DTS | 数据库级同步 | 4 个库的 `dts_*` |
| 中国银行汇率 | 页面抓取（`boc.cn`） | 报价相关，`待确认` 是否仍在用 |
| 微信登录 | `WeiXinLoginApi` 项目 | Web 未引用，`假设` 废弃 |
| 传真/电信 ISMP/支付宝 | `FaxService`、`IsmpSpEngineService`、`AliPay` | Common 中有类，Web 未调用，`假设` 废弃 |

---

## 6. 前端现状

- **形态**：服务端页面（aspx）输出 HTML 壳 + 内联 `<script>`，数据通过 jQuery `$.ajax` / layui `table.render` 请求同目录下的 `GetJson.ashx?Ctype=xxx`，返回 layui 约定 JSON。约 290 个页面是这种模式；约 74 个页面仍用 `asp:GridView/Repeater/Button` 等服务端控件，31 个页面依赖 PostBack。
- **页面数量**：`Web/Manage` 下 aspx 约 700 个（含弹窗/编辑/查看子页面），`Web/view`（layuiadmin 示例）约 160 个 html，大部分为框架自带示例，`假设` 未使用。
- **布局**：`Manage/Index.aspx` 是 layuiadmin 风格的主框架，左侧菜单 + iframe 多标签页，各业务页在 iframe 中打开，页面之间靠 `parent.layui`、`window.parent` 互相调用（刷新列表、关闭标签、打开新标签）。
- **接口风格**：每个模块一个或几个“大 ashx”，以 `Ctype`/`action` 参数分发，例如：

  | 文件 | 行数 | 分支数 |
  |---|---|---|
  | `Project_Up/GetJson.ashx.cs` | 13,763 | — |
  | `QuotationShenhe/GetJson.ashx.cs` | 8,268 | — |
  | `TeshuhetongShenqin2/GetJson.ashx.cs` | 7,116 | — |
  | `Quotation/GetJson.ashx.cs` | 6,939 | 74 |
  | `QuotationPaichan/GetJson.ashx.cs` | 4,987 | — |
  | `AfterSale/Service/GetJson.ashx.cs` | 4,266 | — |

- **前端业务逻辑**：大量计算和校验写在页面内联脚本或 `Jisuan.js`、`AmountInput.js`、`LockTishi.js`、`IsFeikehu.js`、`QuerenShidian.js` 等公共脚本里（金额计算、锁定提示、非客户判断），重构时必须逐一迁到后端或 Vue 组件。
- **多版本混用**：同一页面可能同时引两个 jQuery；layui 2.4.5 较旧；部分页面已引 Vue 2，可作为 Vue 化的试点参考，但不能直接复用到 Vue 3。
- **导出**：大部分导出在 `Daochu*.ashx` 后端生成 Excel（EPPlus/NPOI/MiniExcel），少量前端 layui 导出。

---

## 7. 问题与风险

### 7.1 安全（重构时必须修正，不建议平移）
1. 密码明文存储与比对。
2. 身份完全依赖 DES 加密 Cookie，无服务端会话、无过期校验、无防篡改签名；固定密钥一旦泄露即可伪造任意用户。
3. SQL 注入面大：`strWhere` 拼接贯穿 DAL，页面直接拼接请求参数，`like '%' + userid + '%'` 用于权限判断。
4. `validateRequest="false"`、`enableEventValidation="false"`，配合 `PageBase_Error` 把异常信息原样 `alert` 到页面，存在 XSS 和信息泄露。
5. Web.config 和代码中有明文的数据库、SMTP、短信、Hangfire、报价密码等凭据，且已在 Git 历史中。
6. ashx 鉴权需每个接口自己写，存在遗漏风险。
7. `compilation debug="true"`、`/hangfire` 面板弱口令。

### 7.2 技术债
1. **巨型 ashx**：单文件数千到上万行，`switch(Ctype)` 分发，业务、SQL、邮件、导出混在一起。
2. **全局锁**：`Quotation/GetJson.ashx` 等 3 个 handler 用 `static lock` 包住整个请求处理，所有用户的报价操作串行执行，是明显的性能瓶颈。
3. **静态共享状态**：`BasePage.btnds` 静态字段跨请求共享。
4. **数据访问四套并存**（DbHelperSQL / 动软 DAL / Dapper / FreeSql）+ **Excel 库四套并存**（NPOI / EPPlus / MiniExcel / MyXls）。
5. BLL 基本是 DAL 透传，真正的业务逻辑在页面层。
6. 副本文件、测试页面、废弃项目（WeiXinLoginApi、Fax、ISMP、AliPay）混在主工程里。
7. 运行时改 Web.config，每次都会重启应用池。
8. 拼音命名（`Shenhe`、`Paichan`、`Gaizhang`、`Teshu_pingshen`）与英文混用，同一概念多种拼写（`Pingshen` / `pingshen`、`HeTong` / `Hetong`、`pruduct`）。
9. 数据库几乎没有外键与约束，部分表无主键，冗余名称字段靠触发器同步。
10. 没有自动化迁移工具，表结构靠手工脚本，环境间可能有结构差异（`待确认`）。

### 7.3 耦合严重的地方
1. **跨库耦合**：主库触发器/视图/存储过程直接引用 `Kelong_Shouhuo`、`Kelong_Log`；特殊评审一条业务分散写到 3 个库；邮件模板 `Table_Email` 在售后库却被 Forecast 等主库模块使用。拆服务或换库前必须先梳理。
2. **商机 ↔ 报价 ↔ 合同 ↔ 任务 ↔ 评审** 是一条长链，状态字段互相回写（`IsShenhe`、`Zhuangtai`、`HTZhuangtai`、`GuigePingshenRenwuZhuangtai` 等），一处改动常要同步多张表。
3. **Common ↔ BLL** 双向缠绕，`BasePage` 里混有业务方法（`GetShangjiLingdao`、`IsZSH`、`GetProductType`、`GetShenheType`）。
4. **前端 iframe 框架耦合**：页面之间通过 `parent` 调用，迁 Vue SPA 时需要统一替换为路由/事件。

### 7.4 重构时容易遗漏的隐性逻辑
1. **数据库触发器**：改用户名、大区、客户名、代理商名、行业名、商机名时自动刷新各业务表的冗余字段；改合同附件/审核状态时自动重算合同版本。新系统如不保留触发器，需在应用层补齐。
2. **Kelong_Log 里的业务快照**：特殊评审任务记录、商机历史（`Base_ProjectTrack_Up/Up2`）等不是纯日志，有页面在读。
3. **Web.config 里的业务规则**：税率、审批人用户 ID、产品名单（`Teshupingshen_ProductStr`）、中石化标签、国外工厂、敏感客户关键词、BP 解锁审批人、价格类型、报价有效期 `BaojianYouxiaoqi`、`CurrentYear` 等。
4. **硬编码的权限按钮 ID 和用户 ID**：代码中 `SetQX(10224)` 这类数字、`jean.zhang@krohne.com` 这类失败兜底收件人。
5. **工作日计算**：审批时效、超时提醒、周报应填天数都依赖节假日配置（库表 + `holidayConfig.json` + `审批所用工作日计算.xlsx` 口径）。
6. **软删除**：`DeleteMark`、`IsDel` 并存，回收箱可恢复；字典项删除/禁用后历史数据仍显示原值、当前值显示 `[已删除]`/`[已禁用]` 的规则（见项目规则）。
7. **编号规则**：报价单号、合同号、SQ 号、年度特殊评审号（`Table_TeshuPingshenNum`）在代码中生成，部分依赖全局锁防重。
8. **邮件**：内容大多在代码里拼接，部分在模板表；测试环境标题会加 `EmailTest` 前缀；`IsSendEmail*` 开关按发件账号区分；错误邮件有去重和白名单。
9. **导出口径**：大量 `Daochu_*` 视图决定导出字段和过滤，业务方对导出格式通常很敏感。
10. **存储过程报表**：月度汇总、OEM 报表的计算口径在 SQL 里。
11. **前端脚本中的计算**：金额、税率换算、锁定提示、非客户判断等。
12. **外部依赖**：Power BI 视图、DTS 同步、配置器令牌、`kelong_order` 的另一个 EF 系统（`待确认`）——改表结构前要确认下游。
13. **页面访问即执行的逻辑**：首页加载时按日期修改配置和数据等。
14. **切换身份**：管理员模拟登录功能是测试和运维的常用手段，新系统需要保留并加审计。

---

## 8. 重构建议（供下一步规划参考）

1. **先定边界，后迁代码**：以菜单 17 个模块为迁移单元，优先选依赖少、结构新的模块试点（AR 账龄、ERP BP、Worktop/HR、回收箱），最后迁商机—报价—合同—任务—评审主链。
2. **新旧并行**：新系统（.NET 10 Web API + Vue 3）与旧 WebForms 共用同一数据库，通过统一登录（新签发 JWT/Cookie，旧系统兼容读取）实现菜单级切换，避免一次性切换。
3. **数据库先不动结构**：第一阶段只读写现有表，用 EF Core/Dapper 映射现有表名列名；触发器、视图、存储过程保持，待应用层补齐后再逐个下线。
4. **先补“规则清单”**：把 Web.config 业务项、硬编码 ID、触发器、`Daochu_*` 视图、前端计算脚本整理成规则表，迁移时逐条对照验收。
5. **安全项随迁移一并修正**：密码改哈希（需迁移方案）、参数化查询、统一鉴权与数据权限中间件、凭据迁到安全存储。
6. **补测试基线**：对主链路关键接口（报价保存、合同生成、任务流转、评审）先录制旧系统的输入输出作为对比基线。

### 待确认清单
- OWIN/Hangfire 在生产是否实际启动，`EnableRecurringJobs` 生产值。
- `kelong_order` 库是否有其他系统（EF）在写入。
- DTS 同步的目标和使用方；Power BI 报表负责人。
- `WeiXinLoginApi`、Fax、ISMP、AliPay、中国银行汇率抓取是否完全废弃。
- 短信验证码登录生产是否开启。
- SQL Server Agent 是否有作业。
- 各环境表结构是否一致。
- `Table_1/2/4`、`aaaa` 等临时表是否有引用。
