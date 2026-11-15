using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Files.Application.Uploads.Start;

/// <summary>
/// 登记上传并返回文件 ID 和存储键，供端点流式写入文件
/// </summary>
/// <remarks>
/// 文件流不进入消息，写存储时不持有数据库事务：先登记上传，再写存储，最后用 CompleteUpload 或 RejectUpload 登记结果。
/// 操作者当前角色须具备上传配置要求的权限
/// </remarks>
/// <param name="Profile">上传配置标识，如 Sample.SpecimenAttachment</param>
/// <param name="FileName">客户端文件名，清理后仅用于显示</param>
/// <param name="DeclaredSize">客户端声明的大小；超过配置上限时拒绝，实际大小以读到的字节为准</param>
[AllowAuthenticated]
[NotAudited("上传状态变化记入文件历史")]
[NotIdempotent("每次上传都是一个新文件")]
public sealed record StartUploadCommand(string Profile, string FileName, long DeclaredSize) : ICommand<UploadStarted>;
