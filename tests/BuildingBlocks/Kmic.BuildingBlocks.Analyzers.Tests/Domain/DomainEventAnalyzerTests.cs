using Kmic.BuildingBlocks.Analyzers.Domain;
using Verify = Kmic.BuildingBlocks.Analyzers.Tests.AnalyzerVerifier<Kmic.BuildingBlocks.Analyzers.Domain.DomainEventAnalyzer>;

namespace Kmic.BuildingBlocks.Analyzers.Tests.Domain;

public sealed class DomainEventAnalyzerTests
{
    [Fact]
    public async Task EventCarryingIdsAndValues_NoDiagnostic()
    {
        const string Source = """
            using System;
            using System.Collections.Generic;
            using Kmic.BuildingBlocks.Domain.Events;

            public sealed record SampleSubmitted(Guid SampleId, IReadOnlyList<Guid> ItemIds, decimal Amount) : IDomainEvent;
            """;

        await Verify.VerifyAsync(Source, Verify.ModuleDomain);
    }

    [Fact]
    public async Task EventCarryingEntity_Reports()
    {
        const string Source = """
            using System;
            using System.Collections.Generic;
            using Kmic.BuildingBlocks.Domain.Entities;
            using Kmic.BuildingBlocks.Domain.Events;

            public sealed class Line : Entity<Guid>
            {
                private Line() { }
            }

            public sealed record LinesChanged(IReadOnlyList<Line> {|#0:Lines|}) : IDomainEvent;

            public sealed record LineAdded : IDomainEvent
            {
                public required Line {|#1:Line|} { get; init; }
            }
            """;

        await Verify.VerifyAsync(
            Source,
            Verify.ModuleDomain,
            Verify.Diagnostic(DomainEventAnalyzer.Rule, 0, "LinesChanged", "Lines", "Line"),
            Verify.Diagnostic(DomainEventAnalyzer.Rule, 1, "LineAdded", "Line", "Line"));
    }
}
