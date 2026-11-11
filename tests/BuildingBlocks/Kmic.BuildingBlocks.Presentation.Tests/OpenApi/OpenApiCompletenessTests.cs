using System.Text.Json;
using Kmic.BuildingBlocks.Presentation.OpenApi;
using Shouldly;

namespace Kmic.BuildingBlocks.Presentation.Tests.OpenApi;

public sealed class OpenApiCompletenessTests
{
    private const string Complete = """
        {
          "paths": {
            "/api/v1/samples/things": {
              "get": {
                "summary": "列表",
                "parameters": [ { "name": "q", "in": "query", "description": "关键字" } ]
              }
            }
          },
          "components": {
            "schemas": {
              "ThingDto": { "properties": { "name": { "type": "string", "description": "名称" } } },
              "ThingStatus": { "type": "string", "enum": [ "Draft", "Done" ] }
            }
          },
          "x-kmic-enums": {
            "ThingStatus": [ { "code": "Draft", "name": "草稿" }, { "code": "Done", "name": "已完成" } ]
          }
        }
        """;

    [Fact]
    public void CompleteDocument_HasNoProblems() => Check(Complete).ShouldBeEmpty();

    [Fact]
    public void OperationWithoutSummary_IsReported() =>
        Check(Complete.Replace("\"summary\": \"列表\",", "", StringComparison.Ordinal)).ShouldHaveSingleItem().ShouldBe("操作 GET /api/v1/samples/things 没有摘要（summary）。");

    [Fact]
    public void ParameterWithoutDescription_IsReported() =>
        Check(Complete.Replace(", \"description\": \"关键字\"", "", StringComparison.Ordinal)).ShouldHaveSingleItem().ShouldBe("操作 GET /api/v1/samples/things 的参数 q 没有说明。");

    [Fact]
    public void PropertyWithoutDescription_IsReported() =>
        Check(Complete.Replace(", \"description\": \"名称\"", "", StringComparison.Ordinal)).ShouldHaveSingleItem().ShouldBe("架构 ThingDto 的字段 name 没有说明。");

    [Fact]
    public void EnumValueWithoutADisplayName_IsReported() =>
        Check(Complete.Replace("\"name\": \"已完成\"", "\"name\": \"\"", StringComparison.Ordinal)).ShouldHaveSingleItem().ShouldBe("智能枚举 ThingStatus 的取值 Done 没有显示名。");

    [Fact]
    public void DisplayNamesThatDoNotMatchTheSchema_AreReported() =>
        Check(Complete.Replace("\"code\": \"Done\"", "\"code\": \"Finished\"", StringComparison.Ordinal)).ShouldHaveSingleItem().ShouldBe("智能枚举 ThingStatus 的显示名与架构里的取值不一致。");

    [Fact]
    public void MissingEnumLabels_AreReported() =>
        Check("""{ "paths": {} }""").ShouldHaveSingleItem().ShouldContain("x-kmic-enums");

    [Fact]
    public void AllProblemsAreReportedTogether()
    {
        var broken = Complete
            .Replace("\"summary\": \"列表\",", "", StringComparison.Ordinal)
            .Replace(", \"description\": \"名称\"", "", StringComparison.Ordinal);

        Check(broken).Count.ShouldBe(2);
    }

    private static IReadOnlyList<string> Check(string json)
    {
        using var document = JsonDocument.Parse(json);
        return OpenApiCompleteness.Check(document.RootElement);
    }
}
