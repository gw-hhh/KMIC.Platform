using Kmic.Files.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Files.Infrastructure.Persistence;

internal static class FilesModel
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new StoredFileConfiguration());
        modelBuilder.ApplyConfiguration(new FileReferenceConfiguration());
        modelBuilder.ApplyConfiguration(new StorageIssueConfiguration());
    }
}
