using System.Net;
using System.Text.Json;
using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.Operations.Application;
using Shouldly;

namespace Kmic.Api.Tests.Operations;

/// <summary>
/// 读模型重建的管理接口：列表、触发、确认、放弃的授权与错误。现在没有模块声明重建方式，所以只测这些入口；
/// 重建本身（暂停、确认收尾、放弃、手动触发）由框架测试在带重建订阅的测试模块上覆盖
/// </summary>
public sealed class RebuildManagementTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Root = "/api/v1/operations/messages/rebuilds";

    private static readonly string[] _view = [OperationsPermissions.MessagesView];
    private static readonly string[] _manage = [OperationsPermissions.MessagesView, OperationsPermissions.MessagesManage];

    [Fact]
    public async Task TheList_IsEmptyWhenNoSubscriptionDeclaresARebuild()
    {
        var viewer = await LoginAsOperatorAsync("rebuild-list-viewer", _view);

        var response = await SendAsync(HttpMethod.Get, Root, viewer);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await JsonAsync(response)).ValueKind.ShouldBe(JsonValueKind.Array);
        (await JsonAsync(response)).GetArrayLength().ShouldBe(0);
    }

    [Theory]
    [InlineData("trigger")]
    [InlineData("confirm")]
    [InlineData("abandon")]
    public async Task AnUnknownSubscription_IsNotFound(string action)
    {
        var manager = await LoginAsOperatorAsync("rebuild-unknown-" + action, _manage);

        var response = await SendAsync(HttpMethod.Post, $"{Root}/{action}", manager, new { subscription = "Test.NoSuchSubscription" });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe("Operations.RebuildSubscriptionUnknown");
    }

    [Fact]
    public async Task ManagingARebuild_NeedsTheManagePermission_AndASubscriptionName()
    {
        var viewer = await LoginAsOperatorAsync("rebuild-manage-viewer", _view);
        var manager = await LoginAsOperatorAsync("rebuild-manage-empty", _manage);
        var plain = await LoginAsOperatorAsync("rebuild-plain", []);

        (await SendAsync(HttpMethod.Post, $"{Root}/trigger", viewer, new { subscription = "Any" })).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await SendAsync(HttpMethod.Get, Root, plain)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await SendAsync(HttpMethod.Post, $"{Root}/trigger", manager, new { subscription = " " })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
