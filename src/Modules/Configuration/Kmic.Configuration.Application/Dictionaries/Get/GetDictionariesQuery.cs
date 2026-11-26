using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Configuration.Application.Dictionaries.Get;

/// <summary>
/// 批量查询字典，供前端启动和字典变更后加载
/// </summary>
/// <param name="Types">字典类型</param>
[AllowAuthenticated]
public sealed record GetDictionariesQuery(IReadOnlyList<string> Types) : IQuery<DictionariesResponse>;
