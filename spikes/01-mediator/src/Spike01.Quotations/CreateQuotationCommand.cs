using Spike01.BuildingBlocks;

namespace Spike01.Quotations;

/// <summary>
/// 新建报价（示例）
/// </summary>
public sealed record CreateQuotationCommand(string Title, Guid CustomerId) : ICommand<Guid>, IValidatable
{
    public Error? Validate() =>
        string.IsNullOrWhiteSpace(Title) ? new Error("Quotations.TitleRequired", "Title is required.") : null;
}
