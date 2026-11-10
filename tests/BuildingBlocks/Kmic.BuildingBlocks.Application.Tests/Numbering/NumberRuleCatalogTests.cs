using Kmic.BuildingBlocks.Application.Numbering;
using Shouldly;

namespace Kmic.BuildingBlocks.Application.Tests.Numbering;

public sealed class NumberRuleCatalogTests
{
    private static readonly DateOnly _date = new(2026, 11, 4);

    [Fact]
    public void Render_FillsDatePartsAndPadsSequence()
    {
        var rule = Catalog().Get(GoodRules.Monthly);

        rule.Render(_date, 7, partition: null).ShouldBe("SP-202611-0007");
        rule.Render(_date, 12_345, partition: null).ShouldBe("SP-202611-12345");
        rule.PeriodKey(_date).ShouldBe("202611");
        rule.PartitionKey(null).ShouldBe("-");
        rule.SequenceDigits.ShouldBe(4);
        rule.WarningThreshold.ShouldBe(9_000);
        rule.Module.ShouldBe("BuildingBlocks");
    }

    [Theory]
    [InlineData(GoodRules.Never, "-")]
    [InlineData(GoodRules.Yearly, "2026")]
    [InlineData(GoodRules.Monthly, "202611")]
    [InlineData(GoodRules.Daily, "20261104")]
    public void PeriodKey_FollowsReset(string name, string expected) => Catalog().Get(name).PeriodKey(_date).ShouldBe(expected);

    [Fact]
    public void Partitioned_RequiresWellFormedPartition()
    {
        var rule = Catalog().Get(GoodRules.Partitioned);

        rule.Render(_date, 3, "PL01").ShouldBe("RV-26-PL01-003");
        rule.PartitionKey("PL01").ShouldBe("PL01");
        Should.Throw<ArgumentException>(() => rule.Render(_date, 3, null));
        Should.Throw<ArgumentException>(() => rule.Render(_date, 3, "pl01"));
        Should.Throw<ArgumentException>(() => rule.Render(_date, 3, "ABCDEFGHIJK"));
        Should.Throw<ArgumentException>(() => Catalog().Get(GoodRules.Never).Render(_date, 1, "PL01"));
    }

    [Fact]
    public void Get_UnknownRule_Throws() =>
        Should.Throw<InvalidOperationException>(() => Catalog().Get("BuildingBlocks.Missing"));

    [Theory]
    [InlineData(BadRules.UnknownPlaceholder, "{hh} 不是可用的占位符")]
    [InlineData(BadRules.TwoSequences, "现在有 2 个")]
    [InlineData(BadRules.NoSequence, "现在有 0 个")]
    [InlineData(BadRules.LongSequence, "{seq:19} 不是可用的占位符")]
    [InlineData(BadRules.UnbalancedBrace, "不成对的花括号")]
    [InlineData(BadRules.MonthlyWithoutYear, "缺少区分周期的日期段")]
    [InlineData(BadRules.DailyWithoutDay, "缺少区分周期的日期段")]
    [InlineData(BadRules.PartitionNotDeclared, "{partition}")]
    [InlineData(BadRules.PartitionMissing, "{partition}")]
    [InlineData(BadRules.OtherModule, "本模块名")]
    [InlineData(BadRules.Lowercase, "大驼峰")]
    public void Build_WrongDeclaration_RefusesWithReason(string name, string reason)
    {
        var thrown = Should.Throw<DeclarationException>(() => NumberRuleCatalog.Build([typeof(BadRules)]));

        thrown.Problems.ShouldContain(p => p.Contains(name, StringComparison.Ordinal) && p.Contains(reason, StringComparison.Ordinal));
    }

    [Fact]
    public void Build_NotConstantOrDuplicated_Refuses()
    {
        var thrown = Should.Throw<DeclarationException>(() => NumberRuleCatalog.Build([typeof(GoodRules), typeof(DuplicateRules)]));

        thrown.Problems.ShouldContain(p => p.Contains("BuildingBlocks.Monthly 重复声明", StringComparison.Ordinal));
        thrown.Problems.ShouldContain(p => p.Contains(nameof(DuplicateRules.NotConstant), StringComparison.Ordinal) && p.Contains("public string 常量", StringComparison.Ordinal));
    }

    private static NumberRuleCatalog Catalog() => NumberRuleCatalog.Build([typeof(GoodRules)]);

    public static class GoodRules
    {
        [NumberRule("SN-{seq:6}")]
        public const string Never = "BuildingBlocks.Never";

        [NumberRule("SY-{yyyy}-{seq:5}", Reset = NumberReset.Yearly)]
        public const string Yearly = "BuildingBlocks.Yearly";

        [NumberRule("SP-{yyyy}{MM}-{seq:4}", Reset = NumberReset.Monthly)]
        public const string Monthly = "BuildingBlocks.Monthly";

        [NumberRule("SD{yy}{MM}{dd}{seq:3}", Reset = NumberReset.Daily)]
        public const string Daily = "BuildingBlocks.Daily";

        [NumberRule("RV-{yy}-{partition}-{seq:3}", Reset = NumberReset.Yearly, Partitioned = true)]
        public const string Partitioned = "BuildingBlocks.Partitioned";
    }

    public static class BadRules
    {
        [NumberRule("X-{hh}-{seq:3}")]
        public const string UnknownPlaceholder = "BuildingBlocks.UnknownPlaceholder";

        [NumberRule("X-{seq:3}-{seq:3}")]
        public const string TwoSequences = "BuildingBlocks.TwoSequences";

        [NumberRule("X-{yyyy}")]
        public const string NoSequence = "BuildingBlocks.NoSequence";

        [NumberRule("X-{seq:19}")]
        public const string LongSequence = "BuildingBlocks.LongSequence";

        [NumberRule("X-{seq:3")]
        public const string UnbalancedBrace = "BuildingBlocks.UnbalancedBrace";

        [NumberRule("X-{MM}-{seq:3}", Reset = NumberReset.Monthly)]
        public const string MonthlyWithoutYear = "BuildingBlocks.MonthlyWithoutYear";

        [NumberRule("X-{yyyy}{MM}-{seq:3}", Reset = NumberReset.Daily)]
        public const string DailyWithoutDay = "BuildingBlocks.DailyWithoutDay";

        [NumberRule("X-{partition}-{seq:3}")]
        public const string PartitionNotDeclared = "BuildingBlocks.PartitionNotDeclared";

        [NumberRule("X-{seq:3}", Partitioned = true)]
        public const string PartitionMissing = "BuildingBlocks.PartitionMissing";

        [NumberRule("X-{seq:3}")]
        public const string OtherModule = "Other.Rule";

        [NumberRule("X-{seq:3}")]
        public const string Lowercase = "BuildingBlocks.lowercase";
    }

    public static class DuplicateRules
    {
        [NumberRule("Y-{seq:3}")]
        public const string Monthly = "BuildingBlocks.Monthly";

        [NumberRule("Y-{seq:3}")]
        public static readonly string NotConstant = "BuildingBlocks.NotConstant";
    }
}
