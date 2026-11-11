using System.Net;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

internal sealed class PartnerClient(HttpClient http) : IPartnerClient
{
    public async Task<HttpStatusCode> GetStatusAsync(CancellationToken ct)
    {
        using var response = await http.GetAsync(new Uri("status", UriKind.Relative), ct);
        return response.StatusCode;
    }

    public async Task<HttpStatusCode> PostAsync(CancellationToken ct)
    {
        using var content = new StringContent("{}");
        using var response = await http.PostAsync(new Uri("orders", UriKind.Relative), content, ct);
        return response.StatusCode;
    }
}
