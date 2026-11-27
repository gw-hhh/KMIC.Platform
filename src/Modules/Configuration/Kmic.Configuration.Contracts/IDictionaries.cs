namespace Kmic.Configuration.Contracts;

/// <summary>供其他模块读取字典快照或校验字典项是否可选（不检查权限，查询失败时抛异常）</summary>
public interface IDictionaries
{
    /// <summary>读取指定类型的字典快照</summary>
    Task<DictionarySnapshot?> GetAsync(string typeCode, CancellationToken ct);

    /// <summary>校验字典项是否存在且已启用</summary>
    Task<bool> IsSelectableAsync(string typeCode, string itemCode, CancellationToken ct);
}
