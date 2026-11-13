using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Files.Application.Uploads.Reject;

/// <summary>
/// 登记上传被拒绝：类型不符、超过大小、写入失败或客户端中断
/// </summary>
/// <remarks>
/// 只有上传它的人能登记，状态要是“上传中”。被拒绝的文件不能关联，保留一小段时间就清理；已经写进存储的内容由清理删除
/// </remarks>
/// <param name="FileId">文件 ID</param>
/// <param name="Reason">原因，最多 500 个字符</param>
[AllowAuthenticated]
[NotAudited("上传结果由文件状态历史记录")]
[NotIdempotent("只作用于上传中的文件，重复提交时状态已经不对")]
public sealed record RejectUploadCommand(Guid FileId, string Reason) : ICommand;
