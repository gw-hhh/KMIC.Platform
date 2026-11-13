using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.Notifications.Application.Rendering;
using Shouldly;

namespace Kmic.Api.Tests.Notifications;

/// <summary>
/// 模板渲染（Fluid）：模板来自管理员，不信任。能访问什么、不能访问什么、上限在哪里，都在这里验证
/// </summary>
public sealed class TemplateRendererTests(ClassDatabase<ApiTemplate> database, ApiTemplate template)
    : ApiTestBase(
        database,
        template,
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // 把上限调小，测试才能在一瞬间触发
            ["Notifications:Template:MaxOutputKb"] = "1",
            ["Notifications:Template:MaxSteps"] = "5000",
            ["Notifications:Template:RenderTimeoutMs"] = "5000",
        }),
        IClassFixture<ClassDatabase<ApiTemplate>>
{
    private static readonly Order _order = new("S-2026-0001", "<b>张三</b>", new Address("上海"), [new Line("样品", 2), new Line("试剂", 5)]);

    private ITemplateRenderer Renderer => Factory.Services.GetRequiredService<ITemplateRenderer>();

    private async Task<string> RenderOkAsync(string text, RenderFormat format = RenderFormat.Text) =>
        (await Renderer.RenderAsync(text, _order, format, Ct)).ShouldBeSuccess();

    private async Task<string> RenderFailureAsync(string text)
    {
        var rendered = await Renderer.RenderAsync(text, _order, RenderFormat.Text, Ct);
        rendered.IsFailure.ShouldBeTrue();
        return rendered.Error.Message;
    }

    [Fact]
    public async Task Variables_AreTheModelsPublicProperties_IncludingNestedObjectsAndLists()
    {
        (await RenderOkAsync("单据 {{ Number }}，{{ Address.City }}")).ShouldBe("单据 S-2026-0001，上海");
        (await RenderOkAsync("{% for line in Lines %}{{ line.Name }}x{{ line.Quantity }};{% endfor %}")).ShouldBe("样品x2;试剂x5;");
    }

    [Fact]
    public async Task HtmlRendering_EscapesTheValuesButNotTheTemplateItself_AndTextRenderingDoesNot()
    {
        (await RenderOkAsync("<p>{{ Customer }}</p>", RenderFormat.Html)).ShouldBe("<p>&lt;b&gt;张三&lt;/b&gt;</p>");
        (await RenderOkAsync("{{ Customer }}")).ShouldBe("<b>张三</b>");
    }

    [Fact]
    public async Task WhatTheModelDoesNotExpose_CannotBeReached()
    {
        // 方法、没登记的类型、字符串自己的成员（只能用 Liquid 的过滤器）都读不到
        (await RenderOkAsync("[{{ Reveal }}]")).ShouldBe("[]");
        (await RenderOkAsync("[{{ Address.GetType }}]")).ShouldBe("[]");
        (await RenderOkAsync("[{{ Number.Length }}]")).ShouldBe("[]");
        (await RenderOkAsync("[{{ Number | size }}]")).ShouldBe("[11]");
    }

    [Theory]
    [InlineData("{% include 'secret' %}")]
    [InlineData("{% render 'secret' %}")]
    public async Task Include_AndRender_CannotReadAnyFile(string text)
    {
        var message = await RenderFailureAsync(text);

        message.ShouldStartWith("模板有误：");
    }

    [Fact]
    public async Task ASyntaxError_IsReported_AndCheckFindsItWithoutRendering()
    {
        var message = await RenderFailureAsync("单据 {{ Number ");
        var checkedOnly = Renderer.Check("{% if %}");

        message.ShouldStartWith("模板有误：");
        checkedOnly.IsFailure.ShouldBeTrue();
        Renderer.Check("单据 {{ Number }}").IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task AnEndlessLoop_IsStoppedByTheStepLimit()
    {
        var message = await RenderFailureAsync("{% for i in (1..10000000) %}{% assign x = i %}{% endfor %}");

        message.ShouldStartWith("模板有误：");
    }

    [Fact]
    public async Task TooMuchOutput_IsRefused()
    {
        var message = await RenderFailureAsync("{% for i in (1..4000) %}{{ Number }}{% endfor %}");

        message.ShouldContain("输出超过");
    }

    private sealed record Order(string Number, string Customer, Address Address, IReadOnlyList<Line> Lines)
    {
        public string Reveal() => Number;
    }

    private sealed record Address(string City);

    private sealed record Line(string Name, int Quantity);
}
