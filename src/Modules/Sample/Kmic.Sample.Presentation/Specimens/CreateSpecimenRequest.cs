namespace Kmic.Sample.Presentation.Specimens;

// 属性名须与对应命令保持一致，以确保校验错误路径准确映射
/// <param name="Title">标题</param>
/// <param name="Items">明细</param>
public sealed record CreateSpecimenRequest(string Title, IReadOnlyList<CreateSpecimenItemRequest> Items);
