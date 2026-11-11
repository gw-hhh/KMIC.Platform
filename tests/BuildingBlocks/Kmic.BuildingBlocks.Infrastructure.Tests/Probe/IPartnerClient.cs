using System.Net;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

/// <summary>
/// 外部合作方的客户端，测试用
/// </summary>
internal interface IPartnerClient
{
    Task<HttpStatusCode> GetStatusAsync(CancellationToken ct);

    Task<HttpStatusCode> PostAsync(CancellationToken ct);
}
