namespace Kmic.BuildingBlocks.Domain.Results;

/// <summary>
/// 字段错误
/// </summary>
/// <param name="Path">字段路径，如 items[2].quantity</param>
/// <param name="Code">错误码，如 Required、MaxLength</param>
/// <param name="Message">提示文字</param>
public sealed record FieldError(string Path, string Code, string Message);
