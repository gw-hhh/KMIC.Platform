using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Files.Application.Uploads.Complete;

/// <summary>
/// 登记上传完成：内容已经写入存储，记下实际的类型、大小、SHA-256，文件转为可用
/// </summary>
/// <remarks>
/// 只有上传它的人能登记，状态要是“上传中”。可用的文件是临时文件，过了临时有效期还没有被业务关联就清理
/// </remarks>
/// <param name="FileId">文件 ID</param>
/// <param name="FileType">识别出的文件类型编码</param>
/// <param name="ContentType">识别出的内容类型</param>
/// <param name="Size">实际字节数</param>
/// <param name="Sha256">内容的 SHA-256，小写十六进制</param>
/// <param name="ObjectVersionId">存储里对象的版本 ID，没有开启版本控制时为空</param>
[AllowAuthenticated]
[NotAudited("上传结果由文件状态历史记录")]
[NotIdempotent("只作用于上传中的文件，重复提交时状态已经不对")]
public sealed record CompleteUploadCommand(Guid FileId, string FileType, string ContentType, long Size, string Sha256, string? ObjectVersionId) : ICommand<UploadCompleted>;
