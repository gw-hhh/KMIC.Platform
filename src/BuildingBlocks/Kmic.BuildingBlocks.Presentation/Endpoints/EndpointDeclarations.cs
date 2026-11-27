using System.Text.RegularExpressions;
using Kmic.BuildingBlocks.Application.UseCases;
using Microsoft.AspNetCore.Authorization;

namespace Kmic.BuildingBlocks.Presentation.Endpoints;

// 启动期校验端点声明，避免路由歧义或权限配置遗漏到运行时才暴露
internal static partial class EndpointDeclarations
{
    // 允许匿名访问的消息与端点仅限于认证模块（Identity）
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
                problems.Add($"{name}：没有声明 .Sends<T>()。每个端点须声明所发送的消息。");
                return;
            case > 1:
                problems.Add($"{name}：声明了 {sends.Count} 个 .Sends<T>()，一个端点只能发送一个消息。");
                return;
        }

        var messageType = sends[0].MessageType;
        if (catalog.Find(messageType) is not { } useCase)
        {
            problems.Add($"{name}：发送的 {messageType.FullName} 不在用例目录中，请检查宿主是否已注册其所在模块。");
            return;
        }

        CheckAccess(endpoint, name, useCase, problems);
    }

    private static void CheckAccess(RouteEndpoint endpoint, string name, UseCase useCase, List<string> problems)
    {
        switch (useCase.Requirement)
        {
            case AccessRequirement.SystemOnly:
                problems.Add($"{name}：{useCase.MessageType.Name} 已标注 [SystemOnly]，仅限后台作业与定时任务使用，不可通过端点发送。");
                return;
            case AccessRequirement.ContractQuery:
                problems.Add($"{name}：{useCase.MessageType.Name} 已标注 [ContractQuery]，仅限其他模块通过 Contracts 调用，不可通过端点发送。");
                return;
        }

        var messageAnonymous = useCase.Requirement == AccessRequirement.Anonymous;
        var endpointAnonymous = endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null;
        if (messageAnonymous != endpointAnonymous)
        {
            problems.Add(messageAnonymous
                ? $"{name}：{useCase.MessageType.Name} 标注了 [AllowAnonymous]，端点也须调用 .AllowAnonymous()。"
                : $"{name}：端点调用了 .AllowAnonymous()，但 {useCase.MessageType.Name} 未标注 [AllowAnonymous]。");
        }
        else if (messageAnonymous && !string.Equals(useCase.Module, AnonymousModule, StringComparison.Ordinal))
        {
            problems.Add($"{name}：匿名访问只允许在 {AnonymousModule} 模块中，{useCase.MessageType.Name} 属于 {useCase.Module} 模块。");
        }
    }

    // 若两个端点的方法与路径相同，默认直到首次请求匹配时才触发歧义异常，需在启动期提前拦截
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

    // 路由匹配忽略参数名与约束（如 {id}、{sampleId}、{id:guid} 视为同一路由），需先归一化再判重
    [GeneratedRegex(@"\{[^}]+\}", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex RouteParameter();
}
