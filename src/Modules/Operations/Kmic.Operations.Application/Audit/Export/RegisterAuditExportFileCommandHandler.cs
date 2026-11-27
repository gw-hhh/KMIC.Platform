using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Contracts;

namespace Kmic.Operations.Application.Audit.Export;

public sealed class RegisterAuditExportFileCommandHandler(IGeneratedFiles generatedFiles) : ICommandHandler<RegisterAuditExportFileCommand>
{
    public ValueTask<Result> Handle(RegisterAuditExportFileCommand command, CancellationToken cancellationToken)
    {
        generatedFiles.Register(command.File);
        return ValueTask.FromResult(Result.Success());
    }
}
