using System.Net;
using System.Net.Http.Json;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Presentation.Tests.Cases;
using Kmic.BuildingBlocks.Presentation.Tests.Support;
using Shouldly;
using static Kmic.BuildingBlocks.Presentation.Tests.Support.Messages;

namespace Kmic.BuildingBlocks.Presentation.Tests.Http;

/// <summary>
/// Result 转 HTTP：成功的几种写法、六种 ErrorType 的状态码、ProblemDetails 的字段
/// </summary>
public sealed class ResultHttpTests : IAsyncLifetime
{
    private ApiHost _api = null!;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync() => _api = await ApiHost.StartAsync([ThingEndpoints.Module()]);

    public async ValueTask DisposeAsync() => await _api.DisposeAsync();

    [Fact]
    public async Task ToOk_Returns200WithTheValue()
    {
        var id = ThingId.New();
        _api.Script.On<GetThingQuery, ThingDto>((q, _) => new ThingDto(q.Id, "华东报价"));

        var response = await _api.Client.GetAsync($"/api/v1/samples/things/{id.Value}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.JsonAsync();
        body.GetProperty("id").GetString().ShouldBe(id.Value.ToString());
        body.GetProperty("name").GetString().ShouldBe("华东报价");
    }

    [Fact]
    public async Task ToNoContent_Returns204WithoutABody()
    {
        _api.Script.On<DeleteThingCommand>((_, _) => Result.Success());

        var response = await _api.Client.DeleteAsync($"/api/v1/samples/things/{ThingId.New().Value}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await response.Content.ReadAsStringAsync(Ct)).ShouldBeEmpty();
    }

    [Fact]
    public async Task ToCreated_Returns201WithLocationIdAndVersion()
    {
        var id = ThingId.New();
        _api.Script.On<CreateThingCommand, CreatedResponse<ThingId>>((_, _) => new CreatedResponse<ThingId>(id, 1));

        var response = await _api.Client.PostAsJsonAsync("/api/v1/samples/things", new { name = "a" }, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location!.OriginalString.ShouldBe($"/api/v1/samples/things/{id.Value}");
        var body = await response.JsonAsync();

        // 响应里只有 id 和 version：AggregateId 是框架回填版本号用的，不对外
        body.EnumerateObject().Select(p => p.Name).ShouldBe(["id", "version"]);
        body.GetProperty("id").GetString().ShouldBe(id.Value.ToString());
        body.GetProperty("version").GetInt64().ShouldBe(1);
    }

    [Fact]
    public async Task ToAccepted_Returns202WithLocationAndJobId()
    {
        var jobId = Guid.CreateVersion7();
        _api.Script.On<StartExportCommand, JobAccepted>((_, _) => new JobAccepted(jobId));

        var response = await _api.Client.PostAsync("/api/v1/samples/things/export", content: null, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        response.Headers.Location!.OriginalString.ShouldBe($"/api/v1/operations/jobs/{jobId}");
        (await response.JsonAsync()).GetProperty("jobId").GetGuid().ShouldBe(jobId);
    }

    [Fact]
    public async Task ModifyResponse_CarriesTheNewVersion()
    {
        var id = ThingId.New();
        _api.Script.On<RenameThingCommand, VersionedResponse<ThingId>>((_, _) => new VersionedResponse<ThingId>(id, 43));

        var response = await _api.Client.PostAsJsonAsync($"/api/v1/samples/things/{id.Value}/rename", new { expectedVersion = 42, name = "b" }, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.JsonAsync()).GetProperty("version").GetInt64().ShouldBe(43);
    }

    [Theory]
    [InlineData(ErrorType.Validation, 400, false)]
    [InlineData(ErrorType.Forbidden, 403, false)]
    [InlineData(ErrorType.NotFound, 404, false)]
    [InlineData(ErrorType.Conflict, 409, false)]
    [InlineData(ErrorType.BusinessRule, 422, false)]
    [InlineData(ErrorType.Unavailable, 503, true)]
    public async Task EveryErrorType_MapsToItsStatusAndRetryable(ErrorType type, int status, bool retryable)
    {
        var error = ErrorOf(type, "Sample.Failed", "出错了");
        _api.Script.On<GetThingQuery, ThingDto>((_, _) => error);

        var response = await _api.Client.GetAsync($"/api/v1/samples/things/{ThingId.New().Value}", Ct);

        var body = await response.ShouldBeProblemAsync(status, "Sample.Failed");
        body.GetProperty("retryable").GetBoolean().ShouldBe(retryable);
        body.GetProperty("title").GetString().ShouldBe("出错了");
    }

    [Theory]
    [InlineData(ErrorType.Validation, false)]
    [InlineData(ErrorType.Conflict, false)]
    [InlineData(ErrorType.Unavailable, true)]
    public async Task RetryAfter_IsSentOnlyWhenUnavailable(ErrorType type, bool expected)
    {
        _api.Script.On<GetThingQuery, ThingDto>((_, _) => ErrorOf(type, "Sample.Failed", "出错了"));

        var response = await _api.Client.GetAsync($"/api/v1/samples/things/{ThingId.New().Value}", Ct);

        response.Headers.Contains("Retry-After").ShouldBe(expected);
    }

    [Fact]
    public async Task ErrorCanDeclareRetryable_OverridingTheTypeDefault()
    {
        // 相同请求正在处理：是冲突，但稍后重试就会成功
        _api.Script.On<GetThingQuery, ThingDto>((_, _) => CommonErrors.RequestInProgress.WithMetadata(ErrorMetadataKeys.Retryable, true));

        var response = await _api.Client.GetAsync($"/api/v1/samples/things/{ThingId.New().Value}", Ct);

        var body = await response.ShouldBeProblemAsync(409, "Common.RequestInProgress");
        body.GetProperty("retryable").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    public async Task Detail_IsOmittedWhenTheErrorHasNone_AndSentWhenItHas()
    {
        var withoutDetail = ErrorOf(ErrorType.NotFound, "Sample.Missing", "没有这条数据");
        _api.Script.On<GetThingQuery, ThingDto>((_, _) => withoutDetail);
        var plain = await (await _api.Client.GetAsync($"/api/v1/samples/things/{ThingId.New().Value}", Ct)).ShouldBeProblemAsync(404, "Sample.Missing");

        _api.Script.On<GetThingQuery, ThingDto>((_, _) => withoutDetail.WithDetail("单据 SP-2026-000123 不存在"));
        var detailed = await (await _api.Client.GetAsync($"/api/v1/samples/things/{ThingId.New().Value}", Ct)).ShouldBeProblemAsync(404, "Sample.Missing");

        plain.TryGetProperty("detail", out _).ShouldBeFalse();
        detailed.GetProperty("detail").GetString().ShouldBe("单据 SP-2026-000123 不存在");
    }

    [Fact]
    public async Task Instance_IsTheRequestPath()
    {
        var id = ThingId.New();
        _api.Script.On<GetThingQuery, ThingDto>((_, _) => ErrorOf(ErrorType.NotFound, "Sample.Missing", "没有"));

        var response = await _api.Client.GetAsync($"/api/v1/samples/things/{id.Value}?secret=1", Ct);

        // 不带查询串：筛选条件里可能有不该出现在日志和报障截图里的内容
        (await response.JsonAsync()).GetProperty("instance").GetString().ShouldBe($"/api/v1/samples/things/{id.Value}");
    }

    [Fact]
    public async Task FieldErrors_AreAnArrayWithPathCodeAndMessage()
    {
        var error = CommonErrors.ValidationFailedWith(
        [
            new FieldError("items[2].quantity", "Range", "数量必须大于 0"),
            new FieldError("contactEmail", "Format", "邮箱格式不正确"),
        ]);
        _api.Script.On<GetThingQuery, ThingDto>((_, _) => error);

        var response = await _api.Client.GetAsync($"/api/v1/samples/things/{ThingId.New().Value}", Ct);

        var body = await response.ShouldBeProblemAsync(400, "Common.ValidationFailed");
        var errors = body.GetProperty("errors").EnumerateArray().ToList();
        errors.Count.ShouldBe(2);
        errors[0].GetProperty("path").GetString().ShouldBe("items[2].quantity");
        errors[0].GetProperty("code").GetString().ShouldBe("Range");
        errors[0].GetProperty("message").GetString().ShouldBe("数量必须大于 0");
        errors[1].GetProperty("path").GetString().ShouldBe("contactEmail");
    }

    [Fact]
    public async Task ErrorsMember_IsAbsentWithoutFieldErrors()
    {
        _api.Script.On<GetThingQuery, ThingDto>((_, _) => ErrorOf(ErrorType.NotFound, "Sample.Missing", "没有"));

        var response = await _api.Client.GetAsync($"/api/v1/samples/things/{ThingId.New().Value}", Ct);

        (await response.JsonAsync()).TryGetProperty("errors", out _).ShouldBeFalse();
    }

    [Fact]
    public async Task CurrentVersion_IsSentForAConcurrencyConflictOnly()
    {
        _api.Script.On<GetThingQuery, ThingDto>((_, _) => CommonErrors.VersionConflict.WithMetadata(ErrorMetadataKeys.CurrentVersion, 43L));
        var conflict = await (await _api.Client.GetAsync($"/api/v1/samples/things/{ThingId.New().Value}", Ct)).ShouldBeProblemAsync(409, "Common.VersionConflict");

        _api.Script.On<GetThingQuery, ThingDto>((_, _) => CommonErrors.DuplicateValue);
        var duplicate = await (await _api.Client.GetAsync($"/api/v1/samples/things/{ThingId.New().Value}", Ct)).ShouldBeProblemAsync(409, "Common.DuplicateValue");

        conflict.GetProperty("currentVersion").GetInt64().ShouldBe(43);
        duplicate.TryGetProperty("currentVersion", out _).ShouldBeFalse();
    }

    [Fact]
    public async Task OtherMetadata_IsNeverSent()
    {
        // 附加数据只有白名单里的键会输出，业务错误不能借它把内部信息带给调用方
        _api.Script.On<GetThingQuery, ThingDto>((_, _) => ErrorOf(ErrorType.Conflict, "Sample.Clash", "冲突").WithMetadata("internalOwner", "alice@example.com"));

        var response = await _api.Client.GetAsync($"/api/v1/samples/things/{ThingId.New().Value}", Ct);

        await response.ShouldBeProblemAsync(409, "Sample.Clash");
        (await response.Content.ReadAsStringAsync(Ct)).ShouldNotContain("alice@example.com");
    }

    [Fact]
    public async Task ChineseTextIsNotEscaped()
    {
        _api.Script.On<GetThingQuery, ThingDto>((_, _) => ErrorOf(ErrorType.NotFound, "Sample.Missing", "没有这条数据"));

        var response = await _api.Client.GetAsync($"/api/v1/samples/things/{ThingId.New().Value}", Ct);

        (await response.Content.ReadAsStringAsync(Ct)).ShouldContain("没有这条数据");
    }

    [Fact]
    public async Task IdempotencyKeyHeader_ReachesTheRequestMetadata()
    {
        string? seen = null;
        _api.Script.On<CreateThingCommand, CreatedResponse<ThingId>>((_, metadata) =>
        {
            seen = metadata.IdempotencyKey;
            return new CreatedResponse<ThingId>(ThingId.New(), 1);
        });
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/samples/things") { Content = JsonContent.Create(new { name = "a" }) };
        request.Headers.Add("Idempotency-Key", "key-1");

        var response = await _api.Client.SendAsync(request, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        seen.ShouldBe("key-1");
        response.Headers.Contains("Idempotent-Replayed").ShouldBeFalse();
    }

    [Fact]
    public async Task IdempotencyKeyGivenTwice_IsTreatedAsNoValidKey()
    {
        string? seen = null;
        _api.Script.On<CreateThingCommand, CreatedResponse<ThingId>>((_, metadata) =>
        {
            seen = metadata.IdempotencyKey;
            return new CreatedResponse<ThingId>(ThingId.New(), 1);
        });
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/samples/things") { Content = JsonContent.Create(new { name = "a" }) };
        request.Headers.Add("Idempotency-Key", ["key-1", "key-2"]);

        await _api.Client.SendAsync(request, Ct);

        // 空字符串：管道里的幂等行为把它当作缺少有效的键，返回 Common.IdempotencyKeyRequired
        seen.ShouldBe(string.Empty);
    }

    [Fact]
    public async Task WithoutTheHeader_NoIdempotencyKeyIsSet()
    {
        string? seen = "unset";
        _api.Script.On<CreateThingCommand, CreatedResponse<ThingId>>((_, metadata) =>
        {
            seen = metadata.IdempotencyKey;
            return new CreatedResponse<ThingId>(ThingId.New(), 1);
        });

        await _api.Client.PostAsJsonAsync("/api/v1/samples/things", new { name = "a" }, Ct);

        seen.ShouldBeNull();
    }

    [Fact]
    public async Task ReplayedResult_IsMarkedWithAHeaderAndKeepsTheStatus()
    {
        _api.Script.On<CreateThingCommand, CreatedResponse<ThingId>>((_, metadata) =>
        {
            metadata.Replayed = true;
            return new CreatedResponse<ThingId>(ThingId.New(), 1);
        });

        var response = await _api.Client.PostAsJsonAsync("/api/v1/samples/things", new { name = "a" }, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.GetValues("Idempotent-Replayed").ShouldHaveSingleItem().ShouldBe("true");
    }

    [Fact]
    public async Task ReplayedFailure_IsAlsoMarked()
    {
        _api.Script.On<CreateThingCommand, CreatedResponse<ThingId>>((_, metadata) =>
        {
            metadata.Replayed = true;
            return CommonErrors.DuplicateValue;
        });

        var response = await _api.Client.PostAsJsonAsync("/api/v1/samples/things", new { name = "a" }, Ct);

        await response.ShouldBeProblemAsync(409, "Common.DuplicateValue");
        response.Headers.GetValues("Idempotent-Replayed").ShouldHaveSingleItem().ShouldBe("true");
    }

    private static Error ErrorOf(ErrorType type, string code, string message) => type switch
    {
        ErrorType.Validation => Error.Validation(code, message),
        ErrorType.Forbidden => Error.Forbidden(code, message),
        ErrorType.NotFound => Error.NotFound(code, message),
        ErrorType.Conflict => Error.Conflict(code, message),
        ErrorType.BusinessRule => Error.BusinessRule(code, message),
        ErrorType.Unavailable => Error.Unavailable(code, message),
        _ => throw new ArgumentOutOfRangeException(nameof(type)),
    };
}
