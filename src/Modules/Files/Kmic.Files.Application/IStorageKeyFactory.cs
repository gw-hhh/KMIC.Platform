namespace Kmic.Files.Application;

/// <summary>生成文件对象存储键（不采用原始文件名，以防路径穿越）</summary>
public interface IStorageKeyFactory
{
    string KeyFor(Guid fileId, DateTimeOffset at);
}
