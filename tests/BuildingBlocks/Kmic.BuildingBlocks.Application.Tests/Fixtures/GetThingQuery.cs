using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Application.Tests.Fixtures;

[AllowAuthenticated]
public sealed record GetThingQuery : IQuery<string>;
