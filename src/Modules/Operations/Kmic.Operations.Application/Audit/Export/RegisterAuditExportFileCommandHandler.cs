using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Contracts;

namespace Kmic.Operations.Application.Audit.Export;

/// <summary>
/// 处理 <see cref="RegisterAuditExportFileCommand"/>
/// </summary>
public sealed class RegisterAuditExportFileCommandHandler(IGeneratedFiles generatedFiles) : ICommandHandler<RegisterAuditExportFileCommand>
{
    /// <inheritdoc />
    public ValueTask<Result> Handle(RegisterAuditExportFileCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        generatedFiles.Register(command.File);
        return ValueTask.FromResult(Result.Success());
    }
}
