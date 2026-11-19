namespace Kmic.BuildingBlocks.Application.Files;

/// <summary>
/// 读取文件内容，给下载响应用
/// </summary>
/// <remarks>
/// 不检查权限，存储键要来自已授权的下载查询
/// </remarks>
public interface IFileContentReader
{
    /// <summary>
    /// 打开对象读取，由调用方释放
    /// </summary>
    /// <exception cref="FileNotFoundException">对象不在存储里（被删除标记覆盖或丢失）</exception>
    Task<Stream> OpenReadAsync(string storageKey, CancellationToken ct);
}
