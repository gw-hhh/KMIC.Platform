using Kmic.Configuration.Contracts;

namespace Kmic.Configuration.Application.Dictionaries.Get;

/// <summary>批量字典查询结果</summary>
/// <param name="ETag">基于各字典编码与版本号生成的缓存标识</param>
public sealed record DictionariesResponse(IReadOnlyList<DictionarySnapshot> Dictionaries, string ETag);
