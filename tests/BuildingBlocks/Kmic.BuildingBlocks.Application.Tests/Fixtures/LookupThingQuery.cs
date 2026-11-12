using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.References;

namespace Kmic.BuildingBlocks.Application.Tests.Fixtures;

[ContractQuery(ContractAccess.Reference)]
public sealed record LookupThingQuery : IQuery<ReferenceInfo>;
