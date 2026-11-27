namespace Kmic.BuildingBlocks.Domain.Results;

/// <summary>字段级校验错误。</summary>
/// <param name="Path">字段路径</param>
/// <param name="Code">错误码</param>
/// <param name="Message">错误提示</param>
public sealed record FieldError(string Path, string Code, string Message);
