using System.Text;

namespace Kmic.BuildingBlocks.Presentation.Endpoints;

/// <summary>
/// 路由段的写法
/// </summary>
internal static class RouteNames
{
    /// <summary>
    /// 模块名转路由段：全小写，多个单词用连字符（AfterSales → after-sales）
    /// </summary>
    public static string Segment(string module)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(module);

        var builder = new StringBuilder(module.Length + 4);
        for (var i = 0; i < module.Length; i++)
        {
            var c = module[i];
            if (char.IsUpper(c) && i > 0)
            {
                builder.Append('-');
            }

            builder.Append(char.ToLowerInvariant(c));
        }

        return builder.ToString();
    }
}
