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
    /// Npgsql 默认会尝试加载 Kerberos 的库（libgssapi_krb5）协商 GSS 加密，镜像里没有这个库，
    /// 每次连接都在输出里打一行加载失败。本系统不用 Kerberos，传输加密靠 TLS
    /// </remarks>
    /// <param name="connectionString">配置里的连接串，可以为空</param>
    public static string Normalize(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        if (!builder.ShouldSerialize(GssEncryptionKey))
        {
            builder.GssEncryptionMode = GssEncryptionMode.Disable;
        }

        return builder.ConnectionString;
    }
}
