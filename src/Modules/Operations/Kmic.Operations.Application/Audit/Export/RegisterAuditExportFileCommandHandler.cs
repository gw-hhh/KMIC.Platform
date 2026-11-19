using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Contracts;

namespace Kmic.Operations.Application.Audit.Export;

/// <summary>
/// 请求登记审计导出文件
/// </summary>
public sealed class RegisterAuditExportFileCommandHandler(IGeneratedFiles generatedFiles) : ICommandHandler<RegisterAuditExportFileCommand>
{
    /// <inheritdoc />
    public ValueTask<Result> Handle(RegisterAuditExportFileCommand command, CancellationToken cancellationToken)
    {
        generatedFiles.Register(command.File);
        return ValueTask.FromResult(Result.Success());
    }
}
