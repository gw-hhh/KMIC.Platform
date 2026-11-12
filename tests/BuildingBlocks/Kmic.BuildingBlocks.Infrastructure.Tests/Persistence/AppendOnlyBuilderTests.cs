using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence;

public sealed class AppendOnlyBuilderTests
{
    private sealed class Journal
    {
        public Guid Id { get; set; }
    }

    private sealed class Ordinary
    {
        public Guid Id { get; set; }
    }

    [Fact]
    public void IsAppendOnly_MarksTheEntityTypeForTheMigrationGenerator()
    {
        var builder = new ModelBuilder();
        builder.Entity<Journal>().IsAppendOnly();
        builder.Entity<Ordinary>();

        builder.Entity<Journal>().Metadata.FindAnnotation("Kmic:AppendOnly")?.Value.ShouldBe(true);
        builder.Entity<Ordinary>().Metadata.FindAnnotation("Kmic:AppendOnly").ShouldBeNull();
    }
}
