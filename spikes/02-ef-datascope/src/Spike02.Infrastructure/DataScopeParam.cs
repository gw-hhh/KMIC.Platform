using Spike02.Domain;

namespace Spike02.Infrastructure;

/// <summary>
/// 只在数据范围表达式中使用的占位符，框架注册过滤器时替换为上下文成员
/// </summary>
public static class DataScopeParam
{
    public static UserId? CurrentUser =>
        throw new InvalidOperationException("DataScopeParam can only be used inside data scope expressions.");
}
