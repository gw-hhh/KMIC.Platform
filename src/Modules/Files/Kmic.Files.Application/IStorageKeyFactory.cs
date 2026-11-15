namespace Kmic.Files.Application;

/// <summary>
/// 生成文件的对象存储键
/// </summary>
/// <remarks>
/// 键由环境、年月和文件 ID 组成，不使用原始文件名，避免路径穿越、覆盖及按文件名猜测存储位置
/// </remarks>
public interface IStorageKeyFactory
{
    /// <summary>
    /// 生成指定文件的存储键
    /// </summary>
    /// <param name="fileId">文件 ID</param>
    /// <param name="at">用于划分目录的时间，取 UTC 年月</param>
    string KeyFor(Guid fileId, DateTimeOffset at);
}
