using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Files.Application.Uploads.Complete;

/// <summary>登记已写入存储的文件信息并将上传标记为完成（仅限上传人登记）</summary>
/// <param name="FileId">文件 ID</param>
/// <param name="FileType">识别出的文件类型编码</param>
/// <param name="ContentType">识别出的内容类型</param>
/// <param name="Size">实际字节数</param>
/// <param name="Sha256">内容 SHA-256</param>
/// <param name="ObjectVersionId">对象版本 ID，存储未开启版本控制时为空</param>
[AllowAuthenticated]
[NotAudited("上传状态变化记入文件历史")]
[NotIdempotent("仅接受上传中的文件，完成后重复提交将因状态不符而失败")]
public sealed record CompleteUploadCommand(Guid FileId, string FileType, string ContentType, long Size, string Sha256, string? ObjectVersionId) : ICommand<UploadCompleted>;
