using Kmic.Files.Domain.Files;
using Vogen;

namespace Kmic.Files.Infrastructure.Persistence;

[EfCoreConverter<FileReferenceId>]
[EfCoreConverter<StorageIssueId>]
internal sealed partial class FilesIdConverters;
