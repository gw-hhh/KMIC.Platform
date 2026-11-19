using Kmic.Configuration.Contracts;

namespace Kmic.Configuration.Application.Dictionaries.Get;

/// <summary>
/// 批量字典查询结果
/// </summary>
/// <param name="Dictionaries">字典快照</param>
/// <param name="ETag">字典版本标识</param>
public sealed record DictionariesResponse(IReadOnlyList<DictionarySnapshot> Dictionaries, string ETag);
