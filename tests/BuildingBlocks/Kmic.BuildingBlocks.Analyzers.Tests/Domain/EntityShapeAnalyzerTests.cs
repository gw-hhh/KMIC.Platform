using Kmic.BuildingBlocks.Analyzers.Domain;
using Verify = Kmic.BuildingBlocks.Analyzers.Tests.AnalyzerVerifier<Kmic.BuildingBlocks.Analyzers.Domain.EntityShapeAnalyzer>;

namespace Kmic.BuildingBlocks.Analyzers.Tests.Domain;

public sealed class EntityShapeAnalyzerTests
{
    [Fact]
    public async Task StandardEntity_NoDiagnostic()
    {
        const string Source = """
            using System;
            using System.Collections.Generic;
            using Kmic.BuildingBlocks.Domain.Entities;

            public sealed class Sample : AggregateRoot<Guid>
            {
                private readonly List<string> _items = [];

                private Sample(Guid id) : base(id) { }

                private Sample() { }

                public string Title { get; private set; } = "";

                public string Display => Title;

                public IReadOnlyList<string> Items => _items;

                public static Sample Create() => new(Guid.CreateVersion7());
            }

            public abstract class Document : Entity<Guid>
            {
                protected Document(Guid id) : base(id) { }
            }
            """;

        await Verify.VerifyAsync(Source, Verify.ModuleDomain);
    }

    [Fact]
    public async Task NonPrivateConstructor_Reports()
    {
        const string Source = """
            using System;
            using Kmic.BuildingBlocks.Domain.Entities;

            public sealed class Sample : AggregateRoot<Guid>
            {
                public {|#0:Sample|}(Guid id) : base(id) { }
            }

            public sealed class {|#1:Line|} : Entity<Guid>
            {
            }
            """;

        await Verify.VerifyAsync(
            Source,
            Verify.ModuleDomain,
            Verify.Diagnostic(EntityShapeAnalyzer.NonPrivateConstructor, 0, "Sample"),
            Verify.Diagnostic(EntityShapeAnalyzer.NonPrivateConstructor, 1, "Line"));
    }

    [Fact]
    public async Task WritablePropertyOrMutableCollection_Reports()
    {
        const string Source = """
            using System;
            using System.Collections.Generic;
            using Kmic.BuildingBlocks.Domain.Entities;

            public sealed class Sample : AggregateRoot<Guid>
            {
                private Sample() { }

                public string {|#0:Title|} { get; set; } = "";

                public string {|#1:Code|} { get; private init; } = "";

                public List<string> {|#2:Items|} { get; } = [];

                public string[] {|#3:Tags|} => [];
            }
            """;

        await Verify.VerifyAsync(
            Source,
            Verify.ModuleDomain,
            Verify.Diagnostic(EntityShapeAnalyzer.WritableProperty, 0, "Title"),
            Verify.Diagnostic(EntityShapeAnalyzer.WritableProperty, 1, "Code"),
            Verify.Diagnostic(EntityShapeAnalyzer.MutableCollection, 2, "Items"),
            Verify.Diagnostic(EntityShapeAnalyzer.MutableCollection, 3, "Tags"));
    }
}
