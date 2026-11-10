using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Application.Tests.Fixtures;

[ContractQuery(ContractAccess.Reference)]
public sealed record LookupThingQuery : IQuery<string>;
