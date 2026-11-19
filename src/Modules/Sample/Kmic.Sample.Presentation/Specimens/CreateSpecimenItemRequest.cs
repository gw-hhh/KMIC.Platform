namespace Kmic.Sample.Presentation.Specimens;

/// <summary>
/// 创建示例时的一项明细
/// </summary>
/// <param name="Name">名称</param>
/// <param name="Quantity">数量</param>
public sealed record CreateSpecimenItemRequest(string Name, int Quantity);
