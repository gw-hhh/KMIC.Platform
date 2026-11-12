namespace Kmic.Configuration.Contracts;

/// <summary>
/// 其他模块读取字典：后端导出、通知内容里需要名称，命令里要校验用户输入的字典编码
/// </summary>
/// <remarks>
/// 不检查权限点，也不套数据范围：字典对所有登录用户一致。读缓存，管理员改动之后跨实例失效
/// </remarks>
public interface IDictionaries
{
    /// <summary>
    /// 取字典快照，含停用的项与状态；类型不存在返回空
    /// </summary>
    /// <param name="typeCode">类型编码，如 Sample.Industry</param>
    /// <param name="ct">取消令牌</param>
    Task<DictionarySnapshot?> GetAsync(string typeCode, CancellationToken ct);

    /// <summary>
    /// 这个字典项现在能不能选用：类型存在、编码存在而且启用。命令校验用户输入的字典编码时用，
    /// 返回 false 时由调用方按自己的业务返回 422
    /// </summary>
    /// <param name="typeCode">类型编码</param>
    /// <param name="itemCode">字典项编码</param>
    /// <param name="ct">取消令牌</param>
    Task<bool> IsSelectableAsync(string typeCode, string itemCode, CancellationToken ct);
}
