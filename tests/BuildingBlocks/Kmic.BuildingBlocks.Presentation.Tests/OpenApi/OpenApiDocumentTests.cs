using System.Text.Json;
using Shouldly;

namespace Kmic.BuildingBlocks.Presentation.Tests.OpenApi;

/// <summary>
/// OpenAPI 文档里的框架约定：类型、错误响应、用例声明和给前端生成代码用的目录
/// </summary>
public sealed class OpenApiDocumentTests(OpenApiFixture fixture) : IClassFixture<OpenApiFixture>
{
    private static readonly string[] _problemStatuses = ["400", "401", "403", "404", "409", "422", "429", "500"];

    private JsonElement Doc => fixture.Document;

    [Fact]
    public void StronglyTypedId_IsAUuidString()
    {
        var parameter = Operation("/api/v1/samples/things/{id}", "get").GetProperty("parameters")[0];

        parameter.GetProperty("name").GetString().ShouldBe("id");
        Types(parameter.GetProperty("schema")).ShouldBe(["string"]);
        parameter.GetProperty("schema").GetProperty("format").GetString().ShouldBe("uuid");

        var id = Schema("ThingDto").GetProperty("properties").GetProperty("id");
        Types(id).ShouldBe(["string"]);
        id.GetProperty("format").GetString().ShouldBe("uuid");
    }

    [Fact]
    public void Timestamp_IsAnIsoStringWithTheDateTimeFormat()
    {
        var at = Schema("EchoPayload").GetProperty("properties").GetProperty("at");

        Types(at).ShouldBe(["string"]);
        at.GetProperty("format").GetString().ShouldBe("date-time");
    }

    [Fact]
    public void Query_RespondsWith200AndItsResponseType()
    {
        var responses = Operation("/api/v1/samples/things/{id}", "get").GetProperty("responses");

        responses.GetProperty("200").GetProperty("content").GetProperty("application/json").GetProperty("schema").GetProperty("$ref").GetString()
            .ShouldBe("#/components/schemas/ThingDto");
        responses.EnumerateObject().Select(r => r.Name).Where(n => n.StartsWith('2')).ShouldBe(["200"]);
    }

    [Fact]
    public void CreateCommand_RespondsWith201AndALocationHeader()
    {
        var responses = Operation("/api/v1/samples/things", "post").GetProperty("responses");

        responses.EnumerateObject().Select(r => r.Name).Where(n => n.StartsWith('2')).ShouldBe(["201"]);
        responses.GetProperty("201").GetProperty("headers").TryGetProperty("Location", out _).ShouldBeTrue();

        var created = responses.GetProperty("201").GetProperty("content").GetProperty("application/json").GetProperty("schema");
        var schema = Schema(created.GetProperty("$ref").GetString()!.Split('/')[^1]);
        schema.GetProperty("properties").EnumerateObject().Select(p => p.Name).ShouldBe(["id", "version"]);
    }

    [Fact]
    public void CommandWithoutAValue_RespondsWith204()
    {
        var responses = Operation("/api/v1/samples/things/{id}", "delete").GetProperty("responses");

        responses.EnumerateObject().Select(r => r.Name).Where(n => n.StartsWith('2')).ShouldBe(["204"]);
        responses.GetProperty("204").TryGetProperty("content", out _).ShouldBeFalse();
    }

    [Fact]
    public void JobCommand_RespondsWith202AndTheJobId()
    {
        var responses = Operation("/api/v1/samples/things/export", "post").GetProperty("responses");

        responses.EnumerateObject().Select(r => r.Name).Where(n => n.StartsWith('2')).ShouldBe(["202"]);
        var accepted = responses.GetProperty("202").GetProperty("content").GetProperty("application/json").GetProperty("schema");
        Schema(accepted.GetProperty("$ref").GetString()!.Split('/')[^1]).GetProperty("properties").EnumerateObject().Select(p => p.Name).ShouldBe(["jobId"]);
    }

    [Fact]
    public void PagedList_DescribesTheItemsAndTheCounts()
    {
        var list = Operation("/api/v1/samples/things", "get").GetProperty("responses").GetProperty("200").GetProperty("content").GetProperty("application/json").GetProperty("schema");
        var page = Schema(list.GetProperty("$ref").GetString()!.Split('/')[^1]);

        page.GetProperty("properties").EnumerateObject().Select(p => p.Name).ShouldBe(["items", "page", "pageSize", "totalCount", "totalCountExact"]);
        page.GetProperty("properties").GetProperty("items").GetProperty("items").GetProperty("$ref").GetString().ShouldBe("#/components/schemas/ThingDto");
    }

    [Fact]
    public void SmartEnum_IsAStringWithItsCodes()
    {
        var status = Schema("ThingStatus");

        Types(status).ShouldBe(["string"]);
        status.GetProperty("enum").EnumerateArray().Select(e => e.GetString()).ShouldBe(["Draft", "Done"]);
        status.TryGetProperty("properties", out _).ShouldBeFalse();
    }

    [Fact]
    public void SmartEnumDisplayNames_AreCollectedAtDocumentLevel_NotLeftOnTheSchema()
    {
        var labels = Doc.GetProperty("x-kmic-enums").GetProperty("ThingStatus").EnumerateArray().ToList();

        labels.Select(l => l.GetProperty("code").GetString()).ShouldBe(["Draft", "Done"]);
        labels.Select(l => l.GetProperty("name").GetString()).ShouldBe(["草稿", "已完成"]);
        Schema("ThingStatus").TryGetProperty("x-kmic-enum", out _).ShouldBeFalse();
    }

    [Fact]
    public void EveryOperation_HasTheProblemResponses()
    {
        var operations = Doc.GetProperty("paths").EnumerateObject()
            .SelectMany(p => p.Value.EnumerateObject().Select(o => (Path: p.Name, Method: o.Name, Operation: o.Value)))
            .ToList();

        operations.ShouldNotBeEmpty();
        foreach (var (path, method, operation) in operations)
        {
            var responses = operation.GetProperty("responses");
            foreach (var status in _problemStatuses)
            {
                var content = responses.GetProperty(status).GetProperty("content").GetProperty("application/problem+json");
                content.GetProperty("schema").GetProperty("$ref").GetString().ShouldBe("#/components/schemas/KmicProblem", $"{method} {path} {status}");
            }
        }
    }

    [Fact]
    public void ProblemSchema_ListsTheWhitelistedMembers()
    {
        var problem = Schema("KmicProblem");

        problem.GetProperty("properties").EnumerateObject().Select(p => p.Name)
            .ShouldBe(["type", "title", "status", "detail", "instance", "code", "traceId", "retryable", "currentVersion", "errors"], ignoreOrder: true);
        problem.GetProperty("required").EnumerateArray().Select(r => r.GetString())
            .ShouldBe(["type", "title", "status", "code", "traceId", "retryable"], ignoreOrder: true);
    }

    [Fact]
    public void Operation_CarriesThePermissionOfItsMessage()
    {
        Operation("/api/v1/samples/things/{id}", "get").GetProperty("x-permission").GetString().ShouldBe("Test.Thing.Read");
        Operation("/api/v1/samples/things", "post").GetProperty("x-permission").GetString().ShouldBe("Test.Thing.Write");
    }

    [Fact]
    public void IdempotentCommand_RequiresTheKeyHeader()
    {
        var create = Operation("/api/v1/samples/things", "post");

        create.GetProperty("x-idempotent").GetBoolean().ShouldBeTrue();
        var header = create.GetProperty("parameters").EnumerateArray().Single(p => string.Equals(p.GetProperty("name").GetString(), "Idempotency-Key", StringComparison.Ordinal));
        header.GetProperty("in").GetString().ShouldBe("header");
        header.GetProperty("required").GetBoolean().ShouldBeTrue();
        header.GetProperty("description").GetString()!.ShouldContain("幂等键");
    }

    [Fact]
    public void NonIdempotentOperation_HasNoKeyHeader()
    {
        var delete = Operation("/api/v1/samples/things/{id}", "delete");

        delete.TryGetProperty("x-idempotent", out _).ShouldBeFalse();
        delete.GetProperty("parameters").EnumerateArray().Select(p => p.GetProperty("name").GetString()).ShouldNotContain("Idempotency-Key", StringComparer.Ordinal);
    }

    [Fact]
    public void BearerIsTheDefaultSecurity_AnonymousOperationsOptOut()
    {
        var scheme = Doc.GetProperty("components").GetProperty("securitySchemes").GetProperty("Bearer");

        scheme.GetProperty("type").GetString().ShouldBe("http");
        scheme.GetProperty("scheme").GetString().ShouldBe("bearer");
        scheme.GetProperty("bearerFormat").GetString().ShouldBe("JWT");
        Doc.GetProperty("security").EnumerateArray().Single().TryGetProperty("Bearer", out _).ShouldBeTrue();
        Operation("/api/v1/identity/public-c", "get").GetProperty("security").GetArrayLength().ShouldBe(0);
    }

    [Fact]
    public void AnonymousEndpoints_AreListedAtDocumentLevel() =>
        Doc.GetProperty("x-anonymous-endpoints").EnumerateArray().Select(e => e.GetString()).ShouldBe(["GET /api/v1/identity/public-c"]);

    [Fact]
    public void DeprecatedEndpoint_IsMarkedWithItsSunsetDate()
    {
        var old = Operation("/api/v1/legacy/old-me", "get");

        old.GetProperty("deprecated").GetBoolean().ShouldBeTrue();
        old.GetProperty("x-sunset").GetString().ShouldBe("2027-05-04");
    }

    [Fact]
    public void SortableEndpoint_ListsItsSortableFields() =>
        Operation("/api/v1/samples/things", "get").GetProperty("x-sortable-fields").EnumerateArray().Select(f => f.GetString()).ShouldBe(["name"]);

    [Fact]
    public void Permissions_AreExportedWithNamesAndGroups()
    {
        var permissions = Doc.GetProperty("x-kmic-permissions").EnumerateArray().ToList();

        permissions.Select(p => p.GetProperty("code").GetString()).ShouldBe(["Test.Thing.Read", "Test.Thing.Write"]);
        var write = permissions[1];
        write.GetProperty("name").GetString().ShouldBe("修改测试数据");
        write.GetProperty("group").GetString().ShouldBe("测试");
        write.GetProperty("sensitive").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    public void ErrorCodes_IncludeBusinessAndFrameworkErrors()
    {
        var codes = Doc.GetProperty("x-kmic-error-codes").EnumerateArray().ToDictionary(e => e.GetProperty("code").GetString()!, e => e, StringComparer.Ordinal);

        codes["Test.Missing"].GetProperty("type").GetString().ShouldBe("NotFound");
        codes["Test.Missing"].GetProperty("message").GetString().ShouldBe("没有这条数据");
        codes["Common.TooBusy"].GetProperty("type").GetString().ShouldBe("Framework");
        codes.Keys.ShouldBe(codes.Keys.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void DictionaryTypes_AreExported()
    {
        var industry = Doc.GetProperty("x-kmic-dictionary-types").EnumerateArray().Single();

        industry.GetProperty("code").GetString().ShouldBe("Test.Industry");
        industry.GetProperty("name").GetString().ShouldBe("行业");
        industry.GetProperty("hierarchical").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    public void DictionaryField_IsMarkedWithItsDictionaryType()
    {
        var industry = Schema("CreateThingRequest").GetProperty("properties").GetProperty("industry");

        industry.GetProperty("x-dictionary").GetString().ShouldBe("Test.Industry");
        Schema("CreateThingRequest").GetProperty("properties").GetProperty("name").TryGetProperty("x-dictionary", out _).ShouldBeFalse();
    }

    [Fact]
    public void UploadProfiles_AreAnEmptyObjectUntilTheFilesModuleExists() =>
        Doc.GetProperty("x-kmic-upload-profiles").EnumerateObject().ShouldBeEmpty();

    private static IEnumerable<string?> Types(JsonElement schema) =>
        schema.GetProperty("type").ValueKind == JsonValueKind.Array
            ? schema.GetProperty("type").EnumerateArray().Select(t => t.GetString()).Where(t => !string.Equals(t, "null", StringComparison.Ordinal))
            : [schema.GetProperty("type").GetString()];

    private JsonElement Operation(string path, string method) => Doc.GetProperty("paths").GetProperty(path).GetProperty(method);

    private JsonElement Schema(string name) => Doc.GetProperty("components").GetProperty("schemas").GetProperty(name);
}
