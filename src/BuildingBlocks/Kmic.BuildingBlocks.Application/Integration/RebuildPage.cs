using Kmic.BuildingBlocks.Application.ReadModels;

namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 源端的一页快照，已经转成读模型行
/// </summary>
/// <param name="Rows">本页的行，按 ID 升序</param>
/// <param name="Next">下一页的起点，没有下一页时为空</param>
internal sealed record RebuildPage(IReadOnlyList<IReadModel> Rows, Guid? Next);
