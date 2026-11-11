using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Kmic.BuildingBlocks.Infrastructure.Jobs;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Scheduling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Hosting;

public sealed class BackgroundProcessingTests
{
    [Theory]
    [InlineData("Api,Worker", HostRoles.Api | HostRoles.Worker)]
    [InlineData(" Worker ", HostRoles.Worker)]
    [InlineData("Api", HostRoles.Api)]
    public void ParseRoles_KnownNames(string configured, HostRoles expected) =>
        BackgroundProcessingServiceCollectionExtensions.ParseRoles(configured).ShouldBe(expected);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" , ")]
    [InlineData("Api,Web")]
    public void ParseRoles_EmptyOrUnknown_RefusesToStart(string? configured) =>
        Should.Throw<InvalidOperationException>(() => BackgroundProcessingServiceCollectionExtensions.ParseRoles(configured));

    [Fact]
    public void ApiOnly_NoBackgroundLoops_NoStreamPool()
    {
        var services = new ServiceCollection().AddKmicBackgroundProcessing(HostRoles.Api);

        var hosted = HostedServices(services);
        hosted.ShouldContain(typeof(StartupTaskRunner));
        hosted.ShouldNotContain(typeof(MessagingWorker));
        hosted.ShouldNotContain(typeof(JobWorker));
        hosted.ShouldNotContain(typeof(SchedulerService));
        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IOptions<DatabaseOptions>>().Value.Worker.StreamMaxPoolSize.ShouldBe(0);
    }

    [Fact]
    public void Worker_StartupTasksThenLoops()
    {
        var hosted = HostedServices(new ServiceCollection().AddKmicBackgroundProcessing(HostRoles.Worker));

        // 宿主按注册顺序启动，启动任务必须排在后台循环前面
        hosted[0].ShouldBe(typeof(StartupTaskRunner));
        hosted.ShouldContain(typeof(MessagingListenerService));
        hosted.ShouldContain(typeof(MessagingWorker));
        hosted.ShouldContain(typeof(JobWorker));
        hosted.ShouldContain(typeof(SchedulerService));
        hosted.Count(t => t == typeof(MessagingListenerService)).ShouldBe(1);
    }

    private static List<Type?> HostedServices(IServiceCollection services) =>
        [.. services.Where(d => d.ServiceType == typeof(IHostedService)).Select(d => d.ImplementationType)];
}
