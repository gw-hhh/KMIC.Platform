using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Files.Application.Uploads.Reject;

/// <summary>
/// 登记上传失败，包含类型不符、超出大小、写入失败或客户端中断
/// </summary>
/// <remarks>
/// 只有上传人能登记
/// </remarks>
/// <param name="FileId">文件 ID</param>
/// <param name="Reason">原因</param>
[AllowAuthenticated]
[NotAudited("上传状态变化记入文件历史")]
[NotIdempotent("拒绝后重复提交会因状态不符而失败")]
public sealed record RejectUploadCommand(Guid FileId, string Reason) : ICommand;
