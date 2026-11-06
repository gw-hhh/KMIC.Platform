using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Xunit;

namespace Spike09.Tests;

// 探查：各预设与 ASP.NET Core 接口层默认选项的实际取值
public sealed class PresetTests
{
    [Fact]
    public void Presets_PrintRelevantSettings()
    {
        Print("Default", JsonSerializerOptions.Default);
        Print("Web", JsonSerializerOptions.Web);
        Print("Strict", JsonSerializerOptions.Strict);
        Print("AspNetCore JsonOptions", new JsonOptions().SerializerOptions);
    }

    private static void Print(string name, JsonSerializerOptions options) =>
        TestContext.Current.TestOutputHelper?.WriteLine($"""
            {name}
              PropertyNamingPolicy = {options.PropertyNamingPolicy?.GetType().Name ?? "null"}
              PropertyNameCaseInsensitive = {options.PropertyNameCaseInsensitive}
              NumberHandling = {options.NumberHandling}
              UnmappedMemberHandling = {options.UnmappedMemberHandling}
              AllowDuplicateProperties = {options.AllowDuplicateProperties}
              RespectNullableAnnotations = {options.RespectNullableAnnotations}
              RespectRequiredConstructorParameters = {options.RespectRequiredConstructorParameters}
              DefaultIgnoreCondition = {options.DefaultIgnoreCondition}
              MaxDepth = {options.MaxDepth}
              AllowTrailingCommas = {options.AllowTrailingCommas}
              ReadCommentHandling = {options.ReadCommentHandling}
            """);
}
