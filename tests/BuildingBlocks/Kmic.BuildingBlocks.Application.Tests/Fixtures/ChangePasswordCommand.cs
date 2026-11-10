using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Application.Tests.Fixtures;

[AllowAuthenticated]
[AllowWhenPasswordChangeRequired]
[NotAudited("测试")]
public sealed record ChangePasswordCommand : ICommand;
