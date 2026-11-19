using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens.Close;

/// <summary>
/// 关闭示例
/// </summary>
/// <param name="Id">示例 ID</param>
/// <param name="ExpectedVersion">读取时的版本号</param>
[RequirePermission(SamplePermissions.Close)]
[Audited]
public sealed record CloseSpecimenCommand(SpecimenId Id, long ExpectedVersion) : ICommand<VersionedResponse<SpecimenId>>;
