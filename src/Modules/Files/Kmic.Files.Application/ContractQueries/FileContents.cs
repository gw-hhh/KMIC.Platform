using Kmic.BuildingBlocks.Application.Files;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Contracts;

namespace Kmic.Files.Application.ContractQueries;

/// <summary>
/// <see cref="IFileContents"/> 的实现：经契约查询确认文件能读，再从存储打开内容
/// </summary>
public sealed class FileContents(IContractQuerySender sender, IFileContentReader reader) : IFileContents
{
    /// <inheritdoc />
    public async Task<Result<FileContent>> OpenReadAsync(Guid fileId, CancellationToken ct)
    {
        var located = await sender.SendAsync(new GetReadableFileQuery(fileId), ct);
        if (located.IsFailure)
        {
            return located.Error;
        }

        var file = located.Value;
        try
        {
            return new FileContent(file.Name, file.ContentType, file.Size, await reader.OpenReadAsync(file.StorageKey, ct));
        }
        catch (FileNotFoundException)
        {
            return CommonErrors.FileContentMissing;
        }
    }
}
