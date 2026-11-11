namespace Kmic.BuildingBlocks.Presentation.Http;

/// <summary>
/// 不经过 Result、由中间件或框架产生的错误
/// </summary>
/// <param name="Code">错误码</param>
/// <param name="Status">HTTP 状态码</param>
/// <param name="Title">提示文字</param>
/// <param name="Retryable">原样重试是否可能成功</param>
public sealed record FrameworkProblem(string Code, int Status, string Title, bool Retryable = false);
