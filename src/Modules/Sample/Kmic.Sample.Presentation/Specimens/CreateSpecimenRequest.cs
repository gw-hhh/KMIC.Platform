namespace Kmic.Sample.Presentation.Specimens;

/// <summary>
/// 创建示例的请求
/// </summary>
/// <remarks>
/// 字段名要和命令属性一致，校验错误路径才对得上
/// </remarks>
/// <param name="Title">标题</param>
/// <param name="Items">明细</param>
public sealed record CreateSpecimenRequest(string Title, IReadOnlyList<CreateSpecimenItemRequest> Items);
