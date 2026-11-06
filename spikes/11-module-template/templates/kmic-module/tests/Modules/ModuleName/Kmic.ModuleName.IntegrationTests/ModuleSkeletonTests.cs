using Kmic.ModuleName.Contracts;
using Kmic.ModuleName.Infrastructure.Persistence;
using Kmic.ModuleName.Presentation;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Kmic.ModuleName.IntegrationTests;

public sealed class ModuleSkeletonTests
{
    [Fact]
    public void ModuleInfo_Generated_NamesAndNumbersAreConsistent()
    {
        ModuleNameModuleInfo.Slug.ShouldBe(ModuleNameModuleInfo.Name.ToLowerInvariant());
        ModuleNameModuleInfo.EventIdBase.ShouldBe(ModuleNameModuleInfo.Number * 1000);
        ModuleNameEndpoints.RoutePrefix.ShouldBe($"/api/v1/{ModuleNameModuleInfo.Slug}");
    }

    [Fact]
    public void Model_Generated_MatchesInitialMigration()
    {
        using var context = new ModuleNameDbContextFactory().CreateDbContext([]);

        context.Database.GetMigrations().ShouldHaveSingleItem().ShouldEndWith("_InitializeModuleSchema");
        context.Database.HasPendingModelChanges().ShouldBeFalse();
    }
}
