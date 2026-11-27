namespace Kmic.BuildingBlocks.Application.Files;

/// <summary>文件内容读取器；本接口不校验权限，传入的存储键须来自已授权的下载查询。</summary>
public interface IFileContentReader
{
    /// <summary>打开存储对象读取流，由调用方负责释放；存储对象不存在时抛出 <see cref="FileNotFoundException" />。</summary>
    Task<Stream> OpenReadAsync(string storageKey, CancellationToken ct);
}
