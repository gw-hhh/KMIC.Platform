using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Presentation.OpenApi;
using Kmic.BuildingBlocks.Presentation.Tests.Cases;
using Kmic.BuildingBlocks.Presentation.Tests.Support;
using Microsoft.AspNetCore.OpenApi;
using Shouldly;

namespace Kmic.BuildingBlocks.Presentation.Tests.OpenApi;

/// <summary>
/// 架构名就是前端生成的类型名，全系统唯一
/// </summary>
public sealed class OpenApiSchemaNameTests
{
    [Fact]
    public async Task TwoTypesWithTheSameName_FailDocumentGeneration()
    {
        await using var api = await ApiHost.StartAsync(
            [new ListedModule("Samples", DuplicateNameEndpoints.All)],
            services: services => services.AddOpenApi("v1", options => options.AddKmicConventions()));
        var provider = api.App.Services.GetRequiredKeyedService<IOpenApiDocumentProvider>("v1");

        var thrown = await Should.ThrowAsync<DeclarationException>(provider.GetOpenApiDocumentAsync(TestContext.Current.CancellationToken));

        thrown.Message.ShouldContain("SubmitRequest");
        thrown.Message.ShouldContain("First");
        thrown.Message.ShouldContain("Second");
    }
}
