using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

[DataScopeObjectGroup("Probe")]
public static class ProbeScopes
{
    [DataScopeObject("测试单据")]
    public const string Ticket = "Probe.Ticket";
}
