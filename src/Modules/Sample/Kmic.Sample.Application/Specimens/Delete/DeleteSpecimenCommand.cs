using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens.Delete;

/// <summary>软删除草稿状态的示例</summary>
/// <param name="Id">示例 ID</param>
/// <param name="ExpectedVersion">读取时的版本号</param>
[RequirePermission(SamplePermissions.Delete)]
[Audited]
public sealed record DeleteSpecimenCommand(SpecimenId Id, long ExpectedVersion) : ICommand;
