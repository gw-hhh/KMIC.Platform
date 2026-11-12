using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens.Close;

/// <summary>
/// 关闭示例标本
/// </summary>
/// <param name="Id">标本</param>
/// <param name="ExpectedVersion">调用方看到的版本号，别人改过就返回版本冲突</param>
[RequirePermission(SamplePermissions.Close)]
[Audited]
public sealed record CloseSpecimenCommand(SpecimenId Id, long ExpectedVersion) : ICommand<VersionedResponse<SpecimenId>>;
