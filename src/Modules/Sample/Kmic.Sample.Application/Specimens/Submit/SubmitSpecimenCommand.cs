using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens.Submit;

/// <summary>
/// 提交示例
/// </summary>
/// <param name="Id">示例 ID</param>
/// <param name="ExpectedVersion">读取时的版本号</param>
[RequirePermission(SamplePermissions.Submit)]
[Audited]
[Idempotent]
public sealed record SubmitSpecimenCommand(SpecimenId Id, long ExpectedVersion) : ICommand<VersionedResponse<SpecimenId>>;
