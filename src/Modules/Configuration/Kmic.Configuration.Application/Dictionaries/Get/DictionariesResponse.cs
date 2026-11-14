using Kmic.Configuration.Contracts;

namespace Kmic.Configuration.Application.Dictionaries.Get;

/// <summary>
/// 批量字典查询结果
/// </summary>
/// <param name="Dictionaries">字典快照，按请求中的类型顺序排列</param>
/// <param name="ETag">字典版本标识，用于条件请求；与 If-None-Match 匹配时返回 304</param>
public sealed record DictionariesResponse(IReadOnlyList<DictionarySnapshot> Dictionaries, string ETag);
