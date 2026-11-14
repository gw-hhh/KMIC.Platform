using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Kmic.Api.Tests.Identity;
using Kmic.BuildingBlocks.Testing.Database;

namespace Kmic.Api.Tests.Files;

/// <summary>
/// 文件相关 API 测试的公共部分：本地存储放在临时目录，上传、添加附件的小工具
/// </summary>
/// <remarks>
/// 带 Worker 角色的测试会让关联、解除关联的请求消息真的被处理，不带的只验证 Api 一侧
/// </remarks>
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

    /// <summary>
    /// 一份有效的 PDF：文件头对，内容随便
    /// </summary>
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

    /// <summary>
    /// 上传一个文件，multipart/form-data，第一个带文件名的部分
    /// </summary>
    protected static async Task<HttpResponseMessage> UploadAsync(HttpClient client, string fileName, byte[] content, string profile = Profile, long? declaredSize = null)
    {
        using var form = new MultipartFormDataContent();
        var part = new ByteArrayContent(content);
        part.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(part, "file", fileName);
        var url = $"/api/v1/files?profile={Uri.EscapeDataString(profile)}" + (declaredSize is null ? string.Empty : $"&size={declaredSize}");
        return await client.PostAsync(url, form, Ct);
    }

    /// <summary>
    /// 上传成功，返回文件 ID
    /// </summary>
    protected static async Task<Guid> UploadOkAsync(HttpClient client, string fileName, byte[] content)
    {
        var response = await UploadAsync(client, fileName, content);
        response.EnsureSuccessStatusCode();
        return (await JsonAsync(response)).GetProperty("fileId").GetGuid();
    }

    /// <summary>
    /// 给标本添加附件
    /// </summary>
    protected static async Task<HttpResponseMessage> AddAttachmentAsync(HttpClient client, Guid specimenId, Guid fileId, long version) =>
        await client.PostAsJsonAsync($"/api/v1/sample/specimens/{specimenId}/attachments", new { fileId, expectedVersion = version }, Ct);
}
