namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 游标的编码与校验：游标是不透明字符串，带签名，客户端改不了里面的值
/// </summary>
public interface ICursorProtector
{
    /// <summary>
    /// 把一行的排序键和 ID 编成游标
    /// </summary>
    /// <param name="key">排序键的文本形式</param>
    /// <param name="id">行的 ID，排序键相同时用它定先后</param>
    string Protect(string key, Guid id);

    /// <summary>
    /// 读出游标里的值；格式不对或签名对不上返回否
    /// </summary>
    bool TryUnprotect(string cursor, out string key, out Guid id);
}
