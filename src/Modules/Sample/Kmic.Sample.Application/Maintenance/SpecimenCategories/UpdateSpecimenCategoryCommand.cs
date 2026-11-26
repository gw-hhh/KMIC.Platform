using FluentValidation;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Repositories;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Sample.Domain.SpecimenCategories;

namespace Kmic.Sample.Application.Maintenance.SpecimenCategories;

/// <summary>
/// 修改示例分类
/// </summary>
/// <param name="Id">示例分类 ID</param>
/// <param name="Name">名称</param>
/// <param name="IsEnabled">是否启用</param>
/// <param name="ExpectedVersion">读取时的版本号</param>
[RequirePermission(SamplePermissions.CategoryManage)]
[Audited]
public sealed record UpdateSpecimenCategoryCommand(SpecimenCategoryId Id, string Name, bool IsEnabled, long ExpectedVersion)
    : ICommand<VersionedResponse<SpecimenCategoryId>>;

internal sealed class UpdateSpecimenCategoryCommandValidator : AbstractValidator<UpdateSpecimenCategoryCommand>
{
    public UpdateSpecimenCategoryCommandValidator() => RuleFor(c => c.Name).NotEmpty().MaximumLength(50);
}

/// <summary>
/// 修改示例分类
/// </summary>
public sealed class UpdateSpecimenCategoryCommandHandler(IRepository<SpecimenCategory, SpecimenCategoryId> categories)
    : ICommandHandler<UpdateSpecimenCategoryCommand, VersionedResponse<SpecimenCategoryId>>
{
    /// <inheritdoc />
    public async ValueTask<Result<VersionedResponse<SpecimenCategoryId>>> Handle(UpdateSpecimenCategoryCommand command, CancellationToken cancellationToken)
    {
        var loaded = await categories.GetForUpdateAsync(command.Id, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        loaded.Value.Update(command.Name, command.IsEnabled);
        return new VersionedResponse<SpecimenCategoryId>(command.Id);
    }
}
