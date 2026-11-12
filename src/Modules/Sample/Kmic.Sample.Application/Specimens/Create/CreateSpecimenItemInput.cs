namespace Kmic.Sample.Application.Specimens.Create;

/// <summary>
/// 创建时给出的一项明细
/// </summary>
/// <param name="Name">名称</param>
/// <param name="Quantity">数量</param>
public sealed record CreateSpecimenItemInput(string Name, int Quantity);
