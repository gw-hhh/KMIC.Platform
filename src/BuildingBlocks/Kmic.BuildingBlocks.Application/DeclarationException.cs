namespace Kmic.BuildingBlocks.Application;

/// <summary>
/// 启动时读取的声明（用例、权限点等）有误，宿主拒绝启动；消息中列出全部问题
/// </summary>
public sealed class DeclarationException : InvalidOperationException
{
    /// <summary>
    /// 汇总全部问题
    /// </summary>
    public DeclarationException(string subject, IReadOnlyList<string> problems)
        : base($"{subject}有误，拒绝启动：" + Environment.NewLine + string.Join(Environment.NewLine, problems ?? [])) =>
        Problems = problems ?? [];

    /// <summary>
    /// 无说明的实例
    /// </summary>
    public DeclarationException()
        : this("声明", [])
    {
    }

    /// <summary>
    /// 带说明的实例
    /// </summary>
    public DeclarationException(string message)
        : base(message) => Problems = [message];

    /// <summary>
    /// 带说明与内部异常的实例
    /// </summary>
    public DeclarationException(string message, Exception innerException)
        : base(message, innerException) => Problems = [message];

    /// <summary>
    /// 每条一个问题，指明出问题的类型或成员与原因
    /// </summary>
    public IReadOnlyList<string> Problems { get; }
}
