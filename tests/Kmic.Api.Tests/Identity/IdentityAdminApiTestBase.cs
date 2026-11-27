using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Waiting;
using Shouldly;

namespace Kmic.Api.Tests.Identity;

public abstract class IdentityAdminApiTestBase(ClassDatabase<ApiTemplate> database, ApiTemplate template, IReadOnlyDictionary<string, string>? settings = null)
    : IdentityApiTestBase(database, template, settings)
{
    protected const string Password = "correct horse battery";

    protected static string[] ActionsOf(JsonElement detail) =>
        [.. detail.GetProperty("allowedActions").EnumerateArray().Select(a => a.GetString()!).Order(StringComparer.Ordinal)];

    /// <summary>创建仅含指定权限点的角色与用户，登录并返回访问令牌</summary>
    protected async Task<string> LoginAsOperatorAsync(string account, IReadOnlyCollection<string> permissions, bool isProtected = false)
    {
        var role = await Seed.RoleAsync("操作者-" + account, permissions, isProtected: isProtected);
        var user = await Seed.UserAsync(account, Password, displayName: "操作者 " + account);
        await Seed.GrantAsync(user, role);
        return (await LoginTokensAsync(account)).GetProperty("access_token").GetString()!;
    }

    protected async Task<JsonElement> LoginTokensAsync(string account, string password = Password)
    {
        var login = await PostAsync(NewClient(), "/api/v1/identity/auth/login", new { account, password, clientType = "app" });
        login.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await JsonAsync(login);
    }

    /// <summary>调用 Identity 管理接口，写操作自动附带新幂等键</summary>
    protected async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, string token, object? body = null)
    {
        using var request = new HttpRequestMessage(method, url.StartsWith("/api/", StringComparison.Ordinal) ? url : "/api/v1/identity" + url) { Content = body is null ? null : JsonContent.Create(body) };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (method != HttpMethod.Get)
        {
            request.Headers.Add("Idempotency-Key", Guid.CreateVersion7().ToString());
        }

        return await NewClient().SendAsync(request, Ct);
    }

    /// <summary>轮询等待后台任务将匹配的安全事件落库并返回条数</summary>
    protected async Task<long> WaitForSecurityEventsAsync(string where, long atLeast = 1) =>
        await Eventually.UntilAsync(
            _ => Seed.CountAsync($"""SELECT count(*) FROM "Identity"."SecurityEvent" WHERE {where}"""),
            count => count >= atLeast,
            "安全事件写入数据库",
            ct: Ct);
}
