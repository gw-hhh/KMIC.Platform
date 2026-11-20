using System.Net;
using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Waiting;
using Kmic.Configuration.Application;
using Kmic.Operations.Application;
using Shouldly;

namespace Kmic.Api.Tests.Operations;

/// <summary>
/// 消息容量保护：容量设置的修改与读取，宿主启动后写出状态
/// </summary>
public sealed class MessagingCapacityTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : IdentityAdminApiTestBase(database, template),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private const string Path = "/api/v1/configuration/settings/Operations.MessagingCapacity";

    private static readonly string[] _manage = [ConfigurationPermissions.SettingView, OperationsPermissions.MessagingCapacityManage];

    // 同一个类的测试共用一个库，设置已经保存过时要带上当前版本
    private async Task<HttpResponseMessage> SetAsync(string token, long bytes, string reason)
    {
        var current = await JsonAsync(await SendAsync(HttpMethod.Get, Path, token));
        long? version = current.GetProperty("isModified").GetBoolean() ? current.GetProperty("version").GetInt64() : null;
        return await SendAsync(HttpMethod.Put, Path, token, new { value = new { capacityBytes = bytes }, reason, expectedVersion = version });
    }

    [Fact]
    public async Task TheCapacityAnAdministratorSets_IsReadBackUncachedWithItsUpdateTime()
    {
        var manager = await LoginAsOperatorAsync("capacity-manager", _manage);
        var before = TimeProvider.System.GetUtcNow();

        var set = await SetAsync(manager, 21_474_836_480L, "同步 RDS 已分配容量");

        set.StatusCode.ShouldBe(HttpStatusCode.OK, await set.Content.ReadAsStringAsync(Ct));
        var capacity = (await Factory.Services.GetRequiredService<IMessagingCapacitySource>().ReadAsync(Ct)).ShouldNotBeNull();
        capacity.Bytes.ShouldBe(21_474_836_480L);
        capacity.UpdatedAt.ShouldBeGreaterThanOrEqualTo(before.AddSeconds(-5));
    }

    [Fact]
    public async Task NotSetOrZero_MeansNoCapacity()
    {
        var manager = await LoginAsOperatorAsync("capacity-zero-manager", _manage);

        (await SetAsync(manager, 0L, "暂不启用容量保护")).StatusCode.ShouldBe(HttpStatusCode.OK);

        (await Factory.Services.GetRequiredService<IMessagingCapacitySource>().ReadAsync(Ct)).ShouldBeNull();
    }

    [Theory]
    [InlineData(1000L)]
    [InlineData(-1L)]
    public async Task ACapacityBelowOneGiBThatIsNotZero_IsRejected(long bytes)
    {
        var manager = await LoginAsOperatorAsync($"capacity-small-manager{bytes}", _manage);

        var set = await SetAsync(manager, bytes, "单位写错了");

        set.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task WithoutThePermission_TheCapacityCannotBeChanged()
    {
        var viewer = await LoginAsOperatorAsync("capacity-viewer", [ConfigurationPermissions.SettingView]);

        var set = await SendAsync(HttpMethod.Put, Path, viewer, new { value = new { capacityBytes = 21_474_836_480L }, reason = "越权", expectedVersion = (long?)null });

        set.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AfterStartup_TheHostWritesTheAdmissionState()
    {
        await Eventually.UntilAsync(
            _ => Seed.CountAsync("""SELECT count(*) FROM "Operations"."MessagingAdmission" """),
            count => count == 1,
            "宿主启动后第一个周期就写出容量保护状态",
            ct: Ct);

        (await Seed.CountAsync("""SELECT count(*) FROM "Operations"."MessagingAdmission" WHERE "State" = 0""")).ShouldBe(1);
    }
}
