using Kmic.BuildingBlocks.Application.Files;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Contracts;

namespace Kmic.Files.Application.ContractQueries;

public sealed class FileContents(IContractQuerySender sender, IFileContentReader reader) : IFileContents
{
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
