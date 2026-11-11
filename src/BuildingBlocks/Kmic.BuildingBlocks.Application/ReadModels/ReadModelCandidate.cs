namespace Kmic.BuildingBlocks.Application.ReadModels;

/// <summary>
/// 重建收尾时要向源端确认的一行
/// </summary>
/// <param name="Id">读模型行的 ID</param>
/// <param name="SourceVersion">选为候选时的来源版本</param>
internal sealed record ReadModelCandidate(Guid Id, long SourceVersion);
