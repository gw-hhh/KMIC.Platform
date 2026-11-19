namespace Kmic.Sample.Presentation.Specimens;

/// <summary>
/// 提交示例的请求
/// </summary>
/// <param name="ExpectedVersion">读取时的版本号</param>
public sealed record SubmitSpecimenRequest(long ExpectedVersion);
