namespace Kmic.Sample.Presentation.Specimens;

/// <param name="ExpectedVersion">读取时的版本号</param>
public sealed record DeleteSpecimenRequest(long ExpectedVersion);
