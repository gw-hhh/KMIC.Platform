namespace Kmic.Files.Application;

/// <summary>
/// 生成文件在对象存储里的键
/// </summary>
/// <remarks>
/// 键由环境、年月和文件 ID 组成，与原始文件名无关：防路径穿越、防覆盖、防通过文件名猜测
/// </remarks>
public interface IStorageKeyFactory
{
    /// <summary>
    /// 给一个文件算存储键
    /// </summary>
    /// <param name="fileId">文件 ID</param>
    /// <param name="at">上传开始的时间</param>
    string KeyFor(Guid fileId, DateTimeOffset at);
}
