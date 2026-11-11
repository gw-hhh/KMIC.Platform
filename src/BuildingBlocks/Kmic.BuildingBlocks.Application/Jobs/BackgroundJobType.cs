namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>
/// 一种后台作业的声明
/// </summary>
/// <param name="Name">作业名称</param>
/// <param name="Version">参数版本</param>
/// <param name="ClrType">参数类型</param>
/// <param name="Module">所属模块</param>
/// <param name="HandlerType">处理器类型</param>
/// <param name="Attribute">声明</param>
internal sealed record BackgroundJobType(string Name, int Version, Type ClrType, string Module, Type HandlerType, BackgroundJobAttribute Attribute)
{
    /// <summary>
    /// 处理器接口，如 IJobHandler&lt;ImportSamples&gt;
    /// </summary>
    public Type HandlerInterface => typeof(IJobHandler<>).MakeGenericType(ClrType);
}
