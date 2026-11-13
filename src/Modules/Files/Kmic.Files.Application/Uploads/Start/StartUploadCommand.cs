using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Files.Application.Uploads.Start;

/// <summary>
/// 开始上传：登记一行“上传中”，返回文件 ID 和存储键，内容由上传端点随后流式写入存储
/// </summary>
/// <remarks>
/// 文件流不进入消息，也不在数据库事务里：先用这个命令登记，再写存储，最后用 CompleteUpload 或 RejectUpload 登记结果。
/// 权限看上传档案声明的权限点：操作者当前的角色要有它
/// </remarks>
/// <param name="Profile">上传档案名，如 Sample.ContractScan</param>
/// <param name="FileName">客户端给的文件名，整理后只作显示</param>
/// <param name="DeclaredSize">客户端声明的大小；超过档案上限直接拒绝，实际大小以读到的字节为准</param>
[AllowAuthenticated]
[NotAudited("上传结果由文件状态历史记录")]
[NotIdempotent("每次上传都是一个新文件")]
public sealed record StartUploadCommand(string Profile, string FileName, long DeclaredSize) : ICommand<UploadStarted>;
