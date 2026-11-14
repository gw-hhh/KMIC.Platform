namespace Kmic.Sample.Application.Specimens.Create;

/// <summary>
/// 新建示例时提交的明细项
/// </summary>
/// <param name="Name">名称</param>
/// <param name="Quantity">数量</param>
public sealed record CreateSpecimenItemInput(string Name, int Quantity);
