namespace Kmic.BuildingBlocks.Application.Jobs;

internal sealed record BackgroundJobType(string Name, int Version, Type ClrType, string Module, Type? HandlerType, BackgroundJobAttribute Attribute)
{
    public Type HandlerInterface => typeof(IJobHandler<>).MakeGenericType(ClrType);
}
