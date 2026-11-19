namespace Kmic.Files.Application;

/// <summary>
/// 生成文件的对象存储键
/// </summary>
/// <remarks>
/// 不用原始文件名，避免路径穿越
/// </remarks>
public interface IStorageKeyFactory
{
    /// <summary>
    /// 生成指定文件的存储键
    /// </summary>
    string KeyFor(Guid fileId, DateTimeOffset at);
}
