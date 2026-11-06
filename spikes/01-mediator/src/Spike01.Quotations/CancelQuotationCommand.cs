using Spike01.BuildingBlocks;

namespace Spike01.Quotations;

/// <summary>
/// 不返回值的命令（示例）
/// </summary>
public sealed record CancelQuotationCommand(Guid QuotationId) : ICommand;
