using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Npgsql;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Persistence;

public sealed class ConnectionStringTests
{
    [Fact]
    public void GssEncryption_DisabledByDefault()
    {
        var normalized = KmicConnectionString.Normalize("Host=db;Database=kmic;Username=u;Password=p");

        var builder = new NpgsqlConnectionStringBuilder(normalized);
        builder.GssEncryptionMode.ShouldBe(GssEncryptionMode.Disable);
        builder.Host.ShouldBe("db");
        builder.Database.ShouldBe("kmic");
    }

    [Fact]
    public void ExplicitSetting_Kept()
    {
        var normalized = KmicConnectionString.Normalize("Host=db;Gss Encryption Mode=Require");

        new NpgsqlConnectionStringBuilder(normalized).GssEncryptionMode.ShouldBe(GssEncryptionMode.Require);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Empty_ReturnedAsIs(string value) => KmicConnectionString.Normalize(value).ShouldBe(value);

    [Fact]
    public void Options_NormalizeWhateverIsBound()
    {
        var options = new DatabaseOptions { ConnectionString = "Host=db", MigrationConnectionString = "Host=db-admin" };

        new NpgsqlConnectionStringBuilder(options.ConnectionString).GssEncryptionMode.ShouldBe(GssEncryptionMode.Disable);
        new NpgsqlConnectionStringBuilder(options.MigrationConnectionString).GssEncryptionMode.ShouldBe(GssEncryptionMode.Disable);
    }
}
