using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Configuration.Application.Dictionaries.Get;

/// <summary>
/// 取一批字典：前端启动时和字典变化后拉
/// </summary>
/// <remarks>
/// 登录即可，不需要权限点。含停用的项并带状态，前端据此把历史数据显示成“已停用”。一次最多 20 个类型；
/// 不存在的类型不出现在结果里
/// </remarks>
/// <param name="Types">字典类型编码</param>
[AllowAuthenticated]
public sealed record GetDictionariesQuery(IReadOnlyList<string> Types) : IQuery<DictionariesResponse>;
