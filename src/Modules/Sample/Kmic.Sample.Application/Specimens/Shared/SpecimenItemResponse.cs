namespace Kmic.Sample.Application.Specimens.Shared;

/// <summary>
/// 标本明细
/// </summary>
/// <param name="Name">名称</param>
/// <param name="Quantity">数量</param>
public sealed record SpecimenItemResponse(string Name, int Quantity);
