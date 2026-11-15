namespace Kmic.Sample.Application.Specimens.Create;

/// <summary>
/// 新建示例时提交的明细项
/// </summary>
public sealed record CreateSpecimenItemInput(string Name, int Quantity);
