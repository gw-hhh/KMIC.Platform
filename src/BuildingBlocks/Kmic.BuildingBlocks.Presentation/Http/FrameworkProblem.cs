namespace Kmic.BuildingBlocks.Presentation.Http;

/// <summary>不经过 Result 通道、由框架或中间件直接返回的 HTTP 错误定义</summary>
public sealed record FrameworkProblem(string Code, int Status, string Title, bool Retryable = false);
