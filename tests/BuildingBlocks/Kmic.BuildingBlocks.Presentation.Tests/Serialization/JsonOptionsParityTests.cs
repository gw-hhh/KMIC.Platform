using System.Reflection;
using System.Text.Json;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Presentation.Tests.Cases;
using Kmic.BuildingBlocks.Presentation.Tests.Support;
using Microsoft.Extensions.Options;
using Shouldly;
using HttpJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace Kmic.BuildingBlocks.Presentation.Tests.Serialization;

/// <summary>
/// 写进 ASP.NET Core 的选项和全系统共用的选项逐项相同：
/// 否则 HTTP 与幂等记录、Outbox 的序列化结果会不一致
/// </summary>
public sealed class JsonOptionsParityTests
{
    // 解析器链和只读状态不是“设置”，转换器单独比较
    private static readonly HashSet<string> _skipped = new(StringComparer.Ordinal)
    {
        nameof(JsonSerializerOptions.TypeInfoResolver),
        nameof(JsonSerializerOptions.TypeInfoResolverChain),
        nameof(JsonSerializerOptions.IsReadOnly),
        nameof(JsonSerializerOptions.Converters),
    };

    [Fact]
    public async Task HttpOptions_AreIdenticalToKmicJsonOptions_InEverySetting()
    {
        await using var api = await ApiHost.StartAsync([ThingEndpoints.Module()]);
        var http = api.App.Services.GetRequiredService<IOptions<HttpJsonOptions>>().Value.SerializerOptions;
        var expected = KmicJsonOptions.Create();

        var differences = typeof(JsonSerializerOptions)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0 && !_skipped.Contains(p.Name))
            .Select(p => (p.Name, Expected: p.GetValue(expected), Actual: p.GetValue(http)))
            .Where(x => !Equals(x.Expected, x.Actual))
            .Select(x => $"{x.Name}: 期望 {x.Expected ?? "null"}，实际 {x.Actual ?? "null"}")
            .ToList();

        differences.ShouldBeEmpty();
        http.Converters.Select(c => c.GetType()).ShouldBe(expected.Converters.Select(c => c.GetType()));
    }
}
