using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Errors;

/// <summary>
/// 错误目录中的一项
/// </summary>
/// <param name="Code">错误码，如 Sample.NoItems</param>
/// <param name="Type">错误类型，决定 HTTP 状态码</param>
/// <param name="Message">提示文字</param>
public sealed record ErrorDefinition(string Code, ErrorType Type, string Message);
