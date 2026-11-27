namespace Kmic.Sample.Presentation.Specimens;

/// <param name="Name">名称</param>
/// <param name="Quantity">数量</param>
public sealed record CreateSpecimenItemRequest(string Name, int Quantity);
