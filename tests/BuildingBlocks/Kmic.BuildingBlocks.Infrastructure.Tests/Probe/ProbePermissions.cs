using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

[PermissionGroup("BuildingBlocks", "单据")]
internal static class ProbePermissions
{
    [Permission("重新统计单据")]
    public const string Recount = "BuildingBlocks.Ticket.Recount";
}
