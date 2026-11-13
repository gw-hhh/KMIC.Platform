using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Files.Application.Administration;

/// <summary>
/// 查询一个文件的详情：不返回存储键，也没有下载地址，管理员也不经这里下载内容
/// </summary>
/// <param name="Id">文件</param>
[RequirePermission(FilesPermissions.FileView)]
public sealed record GetFileQuery(StoredFileId Id) : IQuery<FileDetail>;
