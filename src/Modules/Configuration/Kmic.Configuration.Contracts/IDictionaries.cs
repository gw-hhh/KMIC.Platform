namespace Kmic.Configuration.Contracts;

/// <summary>
/// 供其他模块读取字典名称及校验字典项是否可选
/// </summary>
/// <remarks>
/// 不检查权限，查询失败时抛异常
/// </remarks>
public interface IDictionaries
{
    /// <summary>
    /// 读取字典快照，包含停用项及状态；类型不存在时返回 null
    /// </summary>
    Task<DictionarySnapshot?> GetAsync(string typeCode, CancellationToken ct);

    /// <summary>
    /// 字典项是否存在且启用，供命令校验输入编码；返回 false 时由调用方给出业务错误
    /// </summary>
    Task<bool> IsSelectableAsync(string typeCode, string itemCode, CancellationToken ct);
}
