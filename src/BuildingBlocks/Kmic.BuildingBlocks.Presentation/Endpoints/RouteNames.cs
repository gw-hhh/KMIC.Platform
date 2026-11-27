using System.Text;

namespace Kmic.BuildingBlocks.Presentation.Endpoints;

internal static class RouteNames
{
    // 将 PascalCase 模块名转换为全小写连字符路由段（如 AfterSales → after-sales）
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
