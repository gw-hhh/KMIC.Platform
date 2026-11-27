using System.Reflection;
using Kmic.BuildingBlocks.Infrastructure.Modules;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Sample.Application;
using Kmic.Sample.Application.ContractQueries;
using Kmic.Sample.Contracts;
using Kmic.Sample.Domain.Specimens;
using Kmic.Sample.Infrastructure.Persistence;
using Kmic.Sample.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.Sample.Infrastructure;

public sealed class SampleModule : IModule
{
    public string Name => SampleModuleInfo.Name;

    public Assembly ApplicationAssembly => typeof(ISampleQueryDb).Assembly;

    public void AddServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddModuleDbContexts<SampleDbContext, SampleQueryDbContext, ISampleQueryDb>();
        services.AddScoped<ISpecimenRepository, SpecimenRepository>();
        services.AddScoped<ISpecimenReferences, SpecimenReferences>();
    }
}
