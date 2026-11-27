using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Repositories;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Sample.Domain.SpecimenCategories;

namespace Kmic.Sample.Application.Maintenance.SpecimenCategories;

/// <summary>删除示例分类</summary>
/// <param name="Id">示例分类 ID</param>
/// <param name="ExpectedVersion">读取时的版本号</param>
[RequirePermission(SamplePermissions.CategoryManage)]
[Audited]
public sealed record DeleteSpecimenCategoryCommand(SpecimenCategoryId Id, long ExpectedVersion) : ICommand;

public sealed class DeleteSpecimenCategoryCommandHandler(IRepository<SpecimenCategory, SpecimenCategoryId> categories)
    : ICommandHandler<DeleteSpecimenCategoryCommand>
{
    public async ValueTask<Result> Handle(DeleteSpecimenCategoryCommand command, CancellationToken cancellationToken)
    {
        var loaded = await categories.GetForUpdateAsync(command.Id, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        categories.Remove(loaded.Value);
        return Result.Success();
    }
}
