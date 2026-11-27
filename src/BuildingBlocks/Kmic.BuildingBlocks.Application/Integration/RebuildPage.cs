using Kmic.BuildingBlocks.Application.ReadModels;

namespace Kmic.BuildingBlocks.Application.Integration;

internal sealed record RebuildPage(IReadOnlyList<IReadModel> Rows, Guid? Next);
