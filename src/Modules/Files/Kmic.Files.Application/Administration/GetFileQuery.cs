using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Files.Application.Administration;

/// <summary>
/// 查询文件详情，不返回存储键或下载地址；管理员下载也须通过业务对象授权
/// </summary>
[RequirePermission(FilesPermissions.FileView)]
public sealed record GetFileQuery(StoredFileId Id) : IQuery<FileDetail>;
