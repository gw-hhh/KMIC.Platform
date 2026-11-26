using System.Reflection;
using Kmic.BuildingBlocks.Infrastructure.Modules;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Sample.Application;
using Kmic.Sample.Application.ContractQueries;
using Kmic.Sample.Contracts;
using Kmic.Sample.Domain.Errors;
using Kmic.Sample.Domain.SpecimenCategories;
using Kmic.Sample.Domain.Specimens;
using Kmic.Sample.Infrastructure.Persistence;
using Kmic.Sample.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.Sample.Infrastructure;

/// <summary>
/// Sample 模块的服务端注册类
/// </summary>
public sealed class SampleModule : IModule
{
    /// <inheritdoc />
    public string Name => SampleModuleInfo.Name;

    /// <inheritdoc />
    public Assembly ApplicationAssembly => typeof(ISampleQueryDb).Assembly;

    /// <inheritdoc />
    public void AddServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddModuleDbContexts<SampleDbContext, SampleQueryDbContext, ISampleQueryDb>();
        services.AddScoped<ISpecimenRepository, SpecimenRepository>();
        services.AddRepository<SampleDbContext, SpecimenCategory, SpecimenCategoryId>(SampleErrors.CategoryNotFound);
        services.AddScoped<ISpecimenReferences, SpecimenReferences>();
    }
}
