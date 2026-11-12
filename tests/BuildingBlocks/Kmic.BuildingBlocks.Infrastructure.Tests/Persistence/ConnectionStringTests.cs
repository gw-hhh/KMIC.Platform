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

    [Theory]
    [InlineData("Host=db;Include Error Detail=true")]
    [InlineData("Host=db;IncludeErrorDetail=True;Username=u")]
    public void IncludeErrorDetail_Refused(string value) =>
        Should.Throw<InvalidOperationException>(() => KmicConnectionString.Normalize(value)).Message.ShouldContain("Include Error Detail");

    [Fact]
    public void IncludeErrorDetailOff_Accepted() =>
        new NpgsqlConnectionStringBuilder(KmicConnectionString.Normalize("Host=db;Include Error Detail=false")).IncludeErrorDetail.ShouldBeFalse();

    [Fact]
    public void Options_RefuseTheSettingWhenBound() =>
        Should.Throw<InvalidOperationException>(() => new DatabaseOptions { ConnectionString = "Host=db;Include Error Detail=true" });

    [Fact]
    public void Options_NormalizeWhateverIsBound()
    {
        var options = new DatabaseOptions { ConnectionString = "Host=db", MigrationConnectionString = "Host=db-admin" };

        new NpgsqlConnectionStringBuilder(options.ConnectionString).GssEncryptionMode.ShouldBe(GssEncryptionMode.Disable);
        new NpgsqlConnectionStringBuilder(options.MigrationConnectionString).GssEncryptionMode.ShouldBe(GssEncryptionMode.Disable);
    }
}
