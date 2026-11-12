namespace Kmic.Sample.Presentation.Specimens;

/// <summary>
/// 创建示例标本的请求
/// </summary>
/// <remarks>
/// 字段名与命令里的同名属性一致，校验错误的路径才能和请求体对上
/// </remarks>
/// <param name="Title">标题，最多 100 个字符</param>
/// <param name="Items">明细，至少一项，最多 50 项</param>
public sealed record CreateSpecimenRequest(string Title, IReadOnlyList<CreateSpecimenItemRequest> Items);
