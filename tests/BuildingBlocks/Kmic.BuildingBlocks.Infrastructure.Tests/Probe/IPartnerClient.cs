using System.Net;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

internal interface IPartnerClient
{
    Task<HttpStatusCode> GetStatusAsync(CancellationToken ct);

    Task<HttpStatusCode> PostAsync(CancellationToken ct);
}
