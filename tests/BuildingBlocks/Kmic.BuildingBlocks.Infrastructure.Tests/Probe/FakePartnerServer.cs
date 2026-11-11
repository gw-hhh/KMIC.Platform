using System.Net;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

/// <summary>
/// 假的合作方：按顺序返回给定的状态码，用完后一直返回最后一个
/// </summary>
internal sealed class FakePartnerServer(params HttpStatusCode[] responses) : HttpMessageHandler
{
    private int _calls;

    public int Calls => Volatile.Read(ref _calls);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var index = Interlocked.Increment(ref _calls) - 1;
        var status = responses[Math.Min(index, responses.Length - 1)];
        return Task.FromResult(new HttpResponseMessage(status) { RequestMessage = request });
    }
}
