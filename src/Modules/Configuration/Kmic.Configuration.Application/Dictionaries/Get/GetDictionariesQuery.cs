using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Configuration.Application.Dictionaries.Get;

/// <summary>
/// 批量查询字典，供前端启动及字典变更后加载
/// </summary>
/// <remarks>
/// 登录即可，返回字典项及启用状态，包含历史数据显示所需的已停用项。一次最多查询 20 个类型，不存在的类型不返回
/// </remarks>
[AllowAuthenticated]
public sealed record GetDictionariesQuery(IReadOnlyList<string> Types) : IQuery<DictionariesResponse>;
