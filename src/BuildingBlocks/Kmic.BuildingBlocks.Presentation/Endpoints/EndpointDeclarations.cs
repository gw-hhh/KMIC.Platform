using System.Text.RegularExpressions;
using Kmic.BuildingBlocks.Application.UseCases;
using Microsoft.AspNetCore.Authorization;

namespace Kmic.BuildingBlocks.Presentation.Endpoints;

/// <summary>
/// 端点声明的启动校验
/// </summary>
/// <remarks>
/// 问题汇总后一次报出，任何一条都拒绝启动：漏写的声明不会变成运行时才发现的“接口不存在”或“授权不一致”
/// </remarks>
internal static partial class EndpointDeclarations
{
    // 允许匿名的消息和端点只能在认证模块里
    private const string AnonymousModule = "Identity";

    public static IReadOnlyList<string> Check(IEnumerable<RouteEndpoint> endpoints, UseCaseCatalog catalog)
    {
        var problems = new List<string>();
        var list = endpoints.ToList();
        foreach (var endpoint in list)
        {
            CheckSends(endpoint, catalog, problems);
        }

        CheckDuplicateRoutes(list, problems);
        return problems;
    }

    private static void CheckSends(RouteEndpoint endpoint, UseCaseCatalog catalog, List<string> problems)
    {
        var name = Describe(endpoint);
        var sends = endpoint.Metadata.GetOrderedMetadata<SendsMetadata>();
        switch (sends.Count)
        {
            case 0:
                problems.Add($"{name}：没有声明 .Sends<T>()。每个端点要声明它发送的消息。");
                return;
            case > 1:
                problems.Add($"{name}：声明了 {sends.Count} 个 .Sends<T>()，一个端点只能发送一个消息。");
                return;
        }

        var messageType = sends[0].MessageType;
        if (catalog.Find(messageType) is not { } useCase)
        {
            problems.Add($"{name}：发送的 {messageType.FullName} 不在用例目录中，检查宿主是否注册了它所在的模块。");
            return;
        }

        CheckAccess(endpoint, name, useCase, problems);
    }

    private static void CheckAccess(RouteEndpoint endpoint, string name, UseCase useCase, List<string> problems)
    {
        switch (useCase.Requirement)
        {
            case AccessRequirement.SystemOnly:
                problems.Add($"{name}：{useCase.MessageType.Name} 标了 [SystemOnly]，只给后台作业和定时任务使用，不能由端点发送。");
                return;
            case AccessRequirement.ContractQuery:
                problems.Add($"{name}：{useCase.MessageType.Name} 标了 [ContractQuery]，只给其他模块通过 Contracts 调用，不能由端点发送。");
                return;
        }

        var messageAnonymous = useCase.Requirement == AccessRequirement.Anonymous;
        var endpointAnonymous = endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null;
        if (messageAnonymous != endpointAnonymous)
        {
            problems.Add(messageAnonymous
                ? $"{name}：{useCase.MessageType.Name} 标了 [AllowAnonymous]，端点也要 .AllowAnonymous()。"
                : $"{name}：端点 .AllowAnonymous()，但 {useCase.MessageType.Name} 没有标 [AllowAnonymous]。");
        }
        else if (messageAnonymous && !string.Equals(useCase.Module, AnonymousModule, StringComparison.Ordinal))
        {
            problems.Add($"{name}：匿名访问只允许在 {AnonymousModule} 模块中，{useCase.MessageType.Name} 属于 {useCase.Module} 模块。");
        }
    }

    // 两个端点的方法与路径相同，路由要到第一次请求匹配时才报“多个端点匹配”
    private static void CheckDuplicateRoutes(List<RouteEndpoint> endpoints, List<string> problems)
    {
        var duplicates = endpoints
            .SelectMany(e => MethodsOf(e).Select(method => $"{method} {RouteParameter().Replace(e.RoutePattern.RawText ?? string.Empty, "{}")}"))
            .GroupBy(key => key, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1);
        foreach (var group in duplicates)
        {
            problems.Add($"路由 {group.Key} 被 {group.Count()} 个端点同时声明。");
        }
    }

    private static IEnumerable<string> MethodsOf(RouteEndpoint endpoint) =>
        endpoint.Metadata.GetMetadata<IHttpMethodMetadata>() is { HttpMethods.Count: > 0 } methods ? methods.HttpMethods : ["*"];

    private static string Describe(RouteEndpoint endpoint) =>
        $"端点 {string.Join('/', MethodsOf(endpoint))} {endpoint.RoutePattern.RawText}";

    // 路径里的参数名不参与匹配，{id} 和 {sampleId} 是同一个路由；约束写法（{id:guid}）同样归一
    [GeneratedRegex(@"\{[^}]+\}", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex RouteParameter();
}
