using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

public static class KmicConnectionString
{
    private const string GssEncryptionKey = "Gss Encryption Mode";

    /// <summary>规范化连接串（默认禁用 GSS 加密协商；若启用 Include Error Detail 则抛异常防泄漏数据）</summary>
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
                "连接串中的 Include Error Detail 必须关闭：数据库错误详情会携带出错行数据（如重复的邮箱、手机号），可能随异常写入日志。");
        }

        if (!builder.ShouldSerialize(GssEncryptionKey))
        {
            builder.GssEncryptionMode = GssEncryptionMode.Disable;
        }

        return builder.ConnectionString;
    }
}
