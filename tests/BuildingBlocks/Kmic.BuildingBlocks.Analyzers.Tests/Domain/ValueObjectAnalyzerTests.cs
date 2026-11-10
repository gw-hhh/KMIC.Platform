using Kmic.BuildingBlocks.Analyzers.Domain;
using Verify = Kmic.BuildingBlocks.Analyzers.Tests.AnalyzerVerifier<Kmic.BuildingBlocks.Analyzers.Domain.ValueObjectAnalyzer>;

namespace Kmic.BuildingBlocks.Analyzers.Tests.Domain;

public sealed class ValueObjectAnalyzerTests
{
    [Fact]
    public async Task StandardValueObject_NoDiagnostic()
    {
        const string Source = """
            using System;
            using System.Collections.Generic;
            using System.Linq;
            using Kmic.BuildingBlocks.Domain.Events;
            using Kmic.BuildingBlocks.Domain.Results;

            public sealed record EmailAddress
            {
                private EmailAddress(string value) => Value = value;

                public string Value { get; private init; }

                public string Domain => Value[(Value.IndexOf('@') + 1)..];

                public static Result<EmailAddress> Create(string input) => new EmailAddress(input);
            }

            public sealed record Tags
            {
                private Tags(IReadOnlyList<string> values) => Values = values;

                public IReadOnlyList<string> Values { get; private init; }

                public static Result<Tags> Create(IReadOnlyList<string> values) => new Tags(values);

                public bool Equals(Tags? other) => other is not null && Values.SequenceEqual(other.Values);

                public override int GetHashCode() => Values.Count;
            }

            public sealed record SampleSubmitted(Guid SampleId) : IDomainEvent;
            """;

        await Verify.VerifyAsync(Source, Verify.ModuleDomain);
    }

    [Fact]
    public async Task WrongShape_Reports()
    {
        const string Source = """
            using System.Collections.Generic;
            using Kmic.BuildingBlocks.Domain.Results;

            public record {|#0:Money|}
            {
                public {|#1:Money|}(decimal amount) => Amount = amount;

                public decimal {|#2:Amount|} { get; }

                public static Result<Money> Create(decimal amount) => new Money(amount);
            }

            public sealed record {|#3:Point|}(int X, int Y);

            public sealed record {|#4:Labels|}
            {
                private Labels() { }

                public string {|#5:Text|} { get; init; } = "";

                public List<string> {|#6:Items|} { get; private init; } = [];
            }
            """;

        await Verify.VerifyAsync(
            Source,
            Verify.ModuleDomain,
            Verify.Diagnostic(ValueObjectAnalyzer.Shape, 0, "Money"),
            Verify.Diagnostic(ValueObjectAnalyzer.NonPrivateConstructor, 0, "Money"),
            Verify.Diagnostic(ValueObjectAnalyzer.PropertyAccessor, 2, "Amount"),
            Verify.Diagnostic(ValueObjectAnalyzer.Shape, 3, "Point"),
            Verify.Diagnostic(ValueObjectAnalyzer.NonPrivateConstructor, 3, "Point"),
            Verify.Diagnostic(ValueObjectAnalyzer.MissingFactory, 3, "Point"),
            Verify.Diagnostic(ValueObjectAnalyzer.MissingFactory, 4, "Labels"),
            Verify.Diagnostic(ValueObjectAnalyzer.PropertyAccessor, 5, "Text"),
            Verify.Diagnostic(ValueObjectAnalyzer.CollectionProperty, 6, "Items"));
    }
}
