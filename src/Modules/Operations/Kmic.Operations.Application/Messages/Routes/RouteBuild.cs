namespace Kmic.Operations.Application.Messages.Routes;

/// <summary>
/// 声明某个订阅的一个构建，和运行着它的实例数
/// </summary>
/// <param name="Build">构建号</param>
/// <param name="Instances">运行着这个构建的实例数</param>
public sealed record RouteBuild(string Build, int Instances);
