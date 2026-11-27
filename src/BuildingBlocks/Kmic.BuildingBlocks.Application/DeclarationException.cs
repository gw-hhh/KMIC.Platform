namespace Kmic.BuildingBlocks.Application;

/// <summary>模块声明校验失败时抛出的异常，用于阻止宿主启动。</summary>
public sealed class DeclarationException : InvalidOperationException
{
    public DeclarationException(string subject, IReadOnlyList<string> problems)
        : base($"{subject}有误，拒绝启动：" + Environment.NewLine + string.Join(Environment.NewLine, problems ?? [])) =>
        Problems = problems ?? [];

    public DeclarationException()
        : this("声明", [])
    {
    }

    public DeclarationException(string message)
        : base(message) => Problems = [message];

    public DeclarationException(string message, Exception innerException)
        : base(message, innerException) => Problems = [message];

    public IReadOnlyList<string> Problems { get; }
}
