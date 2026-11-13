using System.Reflection;
using Kmic.BuildingBlocks.Application.Files;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Infrastructure.Background;
using Kmic.BuildingBlocks.Infrastructure.Files;
using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Kmic.BuildingBlocks.Infrastructure.Modules;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Files.Application;
using Kmic.Files.Application.Attachments;
using Kmic.Files.Application.Cleanup;
using Kmic.Files.Application.ContractQueries;
using Kmic.Files.Contracts;
using Kmic.Files.Domain.Files;
using Kmic.Files.Infrastructure.Deletion;
using Kmic.Files.Infrastructure.Persistence;
using Kmic.Files.Infrastructure.Persistence.Repositories;
using Kmic.Files.Infrastructure.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.Files.Infrastructure;

/// <summary>
/// Files 模块的服务端注册类
/// </summary>
/// <remarks>
/// 处理器、校验器、EF 配置自动发现；这里只写自动发现做不到的注册
/// </remarks>
public sealed class FilesModule : IModule
{
    /// <inheritdoc />
    public string Name => FilesModuleInfo.Name;

    /// <inheritdoc />
    public Assembly ApplicationAssembly => typeof(IFilesQueryDb).Assembly;

    /// <inheritdoc />
    public void AddServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddModuleDbContexts<FilesDbContext, FilesQueryDbContext, IFilesQueryDb>();
        services.AddScoped<IStoredFileRepository, StoredFileRepository>();
        services.AddKmicOptions<FilesOptions>(FilesOptions.Section);
        services.AddKmicOptions<FilesStorageOptions>(FilesStorageOptions.Section);
        services.AddSingleton<IStorageKeyFactory, StorageKeyFactory>();
        services.AddSingleton<IFileStorage, ConfiguredFileStorage>();
        services.AddSingleton<IFileUploadProcessor, FileUploadProcessor>();
        services.AddSingleton<IFileContentReader, FileContentReader>();
        services.AddScoped<IFileDownloads, FileDownloads>();
        services.AddScoped<IFileAttachability, FileAttachability>();
        services.AddScoped<IFileAttachments, FileAttachments>();
    }

    /// <inheritdoc />
    public void AddWorkerServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddKmicOptions<FilesDeletionOptions>(FilesDeletionOptions.Section);
        services.AddKmicLeasedWork<StoredFileDeletionWork, DeletionItem>();
    }

    /// <inheritdoc />
    public void ConfigureMessaging(IMessagingBuilder messaging)
    {
        ArgumentNullException.ThrowIfNull(messaging);

        // 订阅名存进收件箱，发布后不能改
        messaging.Subscribe<AttachFiles, AttachFilesCommand>("Files.Attach", (message, _) => new AttachFilesCommand(message));
        messaging.Subscribe<DetachFiles, DetachFilesCommand>("Files.Detach", (message, _) => new DetachFilesCommand(message));
        messaging.Subscribe<RegisterGeneratedFile, RegisterGeneratedFileCommand>("Files.RegisterGenerated", (message, _) => new RegisterGeneratedFileCommand(message));
    }

    /// <inheritdoc />
    public void ConfigureScheduling(ISchedulingBuilder scheduling)
    {
        ArgumentNullException.ThrowIfNull(scheduling);

        scheduling.Schedule("Files.PurgeExpired", "0 15 * * * ?", run => new PurgeExpiredFilesCommand(run.ScheduledFireTime), o => o.DisplayName("清理到期的文件"));
        scheduling.Schedule("Files.ResetStaleUploads", "0 45 * * * ?", run => new ResetStaleUploadsCommand(run.ScheduledFireTime), o => o.DisplayName("重置上传中断的文件"));
    }
}
