namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>带签名的游标编解码器。</summary>
public interface ICursorProtector
{
    /// <summary>将记录的排序键与 ID 编码为游标字符串。</summary>
    string Protect(string key, Guid id);

    /// <summary>尝试解析并校验游标。</summary>
    bool TryUnprotect(string cursor, out string key, out Guid id);
}
