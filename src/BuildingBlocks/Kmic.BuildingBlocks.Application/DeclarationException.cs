namespace Kmic.BuildingBlocks.Application;

/// <summary>
/// 声明错误
/// </summary>
public sealed class DeclarationException : InvalidOperationException
{
    /// <summary>
    /// 用全部问题创建
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
    /// 问题列表
    /// </summary>
    public IReadOnlyList<string> Problems { get; }
}
