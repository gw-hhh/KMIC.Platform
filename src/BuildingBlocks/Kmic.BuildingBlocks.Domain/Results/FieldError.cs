namespace Kmic.BuildingBlocks.Domain.Results;

/// <summary>
/// 校验失败中的一个字段错误
/// </summary>
/// <param name="Path">字段路径，小驼峰并带数组下标，与请求 JSON 一致，如 items[2].quantity</param>
/// <param name="Code">校验规则的错误码，如 Required、MaxLength</param>
/// <param name="Message">给用户看的提示</param>
public sealed record FieldError(string Path, string Code, string Message);
