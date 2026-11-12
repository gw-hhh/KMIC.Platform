using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens.Delete;

/// <summary>
/// 删除示例标本，只能删除草稿，软删除
/// </summary>
/// <param name="Id">标本</param>
/// <param name="ExpectedVersion">调用方看到的版本号</param>
[RequirePermission(SamplePermissions.Delete)]
[Audited]
public sealed record DeleteSpecimenCommand(SpecimenId Id, long ExpectedVersion) : ICommand;
