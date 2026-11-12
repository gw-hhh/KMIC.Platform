using Kmic.Configuration.Contracts;

namespace Kmic.Configuration.Application.Dictionaries.Get;

/// <summary>
/// 一批字典
/// </summary>
/// <param name="Dictionaries">取到的字典快照，按请求里类型的次序</param>
/// <param name="ETag">各类型版本号的组合：什么都没变时前端带着它再问，得到 304</param>
public sealed record DictionariesResponse(IReadOnlyList<DictionarySnapshot> Dictionaries, string ETag);
