using Spike01.BuildingBlocks;

namespace Spike01.Quotations;

/// <summary>
/// 查询报价标题（示例）
/// </summary>
public sealed record GetQuotationTitleQuery(Guid QuotationId) : IQuery<string>;
