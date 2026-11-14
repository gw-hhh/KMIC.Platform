namespace Kmic.Configuration.Contracts;

/// <summary>
/// 供其他模块读取字典名称及校验字典项是否可选
/// </summary>
/// <remarks>
/// 不检查权限点，也不套数据范围：字典对所有登录用户一致。读缓存，管理员改动之后跨实例失效
/// </remarks>
public interface IDictionaries
{
    /// <summary>
    /// 取字典快照，含停用的项与状态；类型不存在返回空
    /// </summary>
    Task<DictionarySnapshot?> GetAsync(string typeCode, CancellationToken ct);

    /// <summary>
    /// 判断字典项是否存在且启用，供命令校验输入编码；返回 false 时由调用方生成相应业务错误
    /// </summary>
    Task<bool> IsSelectableAsync(string typeCode, string itemCode, CancellationToken ct);
}
