namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 声明命令或查询受功能开关控制
/// </summary>
/// <remarks>
/// 开关关闭时用例不执行：HTTP 请求返回 404（和功能不存在一样），后台入口（集成事件、定时任务、后台作业）也不执行并记一条 Information。
/// 开关只决定功能是否开放，不授予权限：开启后仍按权限点与数据范围授权。开关的值来自部署配置 <c>Kmic:Features:{名称}</c>
/// </remarks>
/// <param name="name">开关名，格式 <c>{模块}.{功能}</c>，如 <c>Sample.NewImport</c></param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class FeatureGateAttribute(string name) : Attribute
{
    /// <summary>
    /// 开关名
    /// </summary>
    public string Name { get; } = name;
}
