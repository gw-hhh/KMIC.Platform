using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

internal static class TicketErrors
{
    public static readonly Error NumberDuplicated = Error.Conflict("Probe.NumberDuplicated", "编号已存在");

    public static readonly Error NotFound = Error.NotFound("Probe.TicketNotFound", "单据不存在");

    public static readonly Error Rejected = Error.BusinessRule("Probe.Rejected", "测试要求拒绝");
}
