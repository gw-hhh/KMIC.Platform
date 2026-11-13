namespace Kmic.BuildingBlocks.Application.Files;

/// <summary>
/// 读取文件内容：下载响应用它打开存储里的对象
/// </summary>
/// <remarks>
/// 存储本身（IFileStorage）只有 Files 模块和框架基础设施能用，这是给下载响应的窄接口，只读、只认存储键。
/// 业务代码拿到存储键的唯一来源是业务的下载查询，那里已经完成了授权
/// </remarks>
public interface IFileContentReader
{
    /// <summary>
    /// 打开对象读取，由调用方释放
    /// </summary>
    /// <param name="storageKey">存储键</param>
    /// <param name="ct">取消令牌</param>
    /// <exception cref="FileNotFoundException">对象不在存储里（被删除标记覆盖或丢失）</exception>
    Task<Stream> OpenReadAsync(string storageKey, CancellationToken ct);
}
