namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 用例声明有误，宿主拒绝启动；消息中列出全部问题
/// </summary>
public sealed class UseCaseCatalogException : InvalidOperationException
{
    /// <summary>
    /// 汇总全部问题
    /// </summary>
    public UseCaseCatalogException(IReadOnlyList<string> problems)
        : base("用例声明有误，拒绝启动：" + Environment.NewLine + string.Join(Environment.NewLine, problems ?? [])) =>
        Problems = problems ?? [];

    /// <summary>
    /// 无说明的实例
    /// </summary>
    public UseCaseCatalogException()
        : this([])
    {
    }

    /// <summary>
    /// 带说明的实例
    /// </summary>
    public UseCaseCatalogException(string message)
        : base(message) => Problems = [message];

    /// <summary>
    /// 带说明与内部异常的实例
    /// </summary>
    public UseCaseCatalogException(string message, Exception innerException)
        : base(message, innerException) => Problems = [message];

    /// <summary>
    /// 每条一个问题，指明消息类型与原因
    /// </summary>
    public IReadOnlyList<string> Problems { get; }
}
