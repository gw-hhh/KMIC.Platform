using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 连接串的统一处理
/// </summary>
public static class KmicConnectionString
{
    private const string GssEncryptionKey = "Gss Encryption Mode";

    /// <summary>
    /// 默认关闭 GSS 加密协商，连接串里明确写了的不动
    /// </summary>
    /// <remarks>
    /// 打开了 Include Error Detail 的连接串直接拒绝：错误详情里有业务数据
    /// </remarks>
    /// <param name="connectionString">配置里的连接串，可以为空</param>
    /// <exception cref="InvalidOperationException">连接串打开了 Include Error Detail</exception>
    public static string Normalize(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        if (builder.IncludeErrorDetail)
        {
            throw new InvalidOperationException(
                "连接串里的 Include Error Detail 必须关闭：数据库的错误详情里带着出错那一行的数据（如重复的邮箱、手机号），会跟着异常写进日志。");
        }

        if (!builder.ShouldSerialize(GssEncryptionKey))
        {
            builder.GssEncryptionMode = GssEncryptionMode.Disable;
        }

        return builder.ConnectionString;
    }
}
