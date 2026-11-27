using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Testing.Database;

namespace Kmic.Api.Tests.Files;

/// <summary>文件模块 API 测试基类：配置临时本地存储目录（启用 Worker 时才实际处理关联/解绑消息）</summary>
public abstract class FilesApiTestBase(
    ClassDatabase<ApiTemplate> database,
    ApiTemplate template,
    bool withWorker = false,
    string? storageRoot = null,
    IReadOnlyDictionary<string, string>? moreSettings = null,
    Action<IServiceCollection>? configureServices = null)
    : ApiTestBase(database, template, SettingsFor(withWorker, storageRoot ?? _storageRoot, moreSettings), configureServices)
{
    protected const string Profile = "Sample.SpecimenAttachment";

    private static readonly string _storageRoot = Path.Combine(Path.GetTempPath(), "kmic-files-api-tests");

    protected static byte[] Pdf(string content = "合同内容") => [.. "%PDF-1.7\n"u8, .. Encoding.UTF8.GetBytes(content)];

    private protected IdentitySeed NewSeed() => new(ConnectionString);

    protected static string StoragePathOf(string storageKey) => Path.Combine(_storageRoot, storageKey.Replace('/', Path.DirectorySeparatorChar));

    private protected static async Task<T?> ColumnAsync<T>(IdentitySeed seed, Guid fileId, string column) =>
        await seed.ScalarAsync<T>($"""SELECT "{column}" FROM "Files"."StoredFile" WHERE "Id" = @id""", ("id", fileId));

    private protected static async Task<bool> IsNullAsync(IdentitySeed seed, Guid fileId, string column) =>
        await seed.ScalarAsync<bool>($"""SELECT "{column}" IS NULL FROM "Files"."StoredFile" WHERE "Id" = @id""", ("id", fileId));

    private static Dictionary<string, string> SettingsFor(bool withWorker, string storageRoot, IReadOnlyDictionary<string, string>? moreSettings)
    {
        var settings = new Dictionary<string, string>(StringComparer.Ordinal) { ["Files:Storage:LocalRoot"] = storageRoot };
        foreach (var (key, value) in moreSettings ?? new Dictionary<string, string>(StringComparer.Ordinal))
        {
            settings[key] = value;
        }

        if (withWorker)
        {
            settings["Kmic:Roles"] = "Api,Worker";
        }

        return settings;
    }

    protected static async Task<HttpResponseMessage> UploadAsync(HttpClient client, string fileName, byte[] content, string profile = Profile, long? declaredSize = null)
    {
        using var form = new MultipartFormDataContent();
        var part = new ByteArrayContent(content);
        part.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(part, "file", fileName);
        var url = $"/api/v1/files?profile={Uri.EscapeDataString(profile)}" + (declaredSize is null ? string.Empty : $"&size={declaredSize}");
        return await client.PostAsync(url, form, Ct);
    }

    protected static async Task<Guid> UploadOkAsync(HttpClient client, string fileName, byte[] content)
    {
        var response = await UploadAsync(client, fileName, content);
        response.EnsureSuccessStatusCode();
        return (await JsonAsync(response)).GetProperty("fileId").GetGuid();
    }

    protected static async Task<HttpResponseMessage> AddAttachmentAsync(HttpClient client, Guid specimenId, Guid fileId, long version) =>
        await client.PostAsJsonAsync($"/api/v1/sample/specimens/{specimenId}/attachments", new { fileId, expectedVersion = version }, Ct);
}
