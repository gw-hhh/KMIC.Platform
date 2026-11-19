using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Files.Application.Uploads.Start;

/// <summary>
/// 登记上传并返回文件 ID 和存储键，供端点流式写入文件
/// </summary>
/// <remarks>
/// 写存储时不持有事务
/// </remarks>
/// <param name="Profile">上传配置标识</param>
/// <param name="FileName">客户端文件名</param>
/// <param name="DeclaredSize">客户端声明的大小</param>
[AllowAuthenticated]
[NotAudited("上传状态变化记入文件历史")]
[NotIdempotent("每次上传都是一个新文件")]
public sealed record StartUploadCommand(string Profile, string FileName, long DeclaredSize) : ICommand<UploadStarted>;
