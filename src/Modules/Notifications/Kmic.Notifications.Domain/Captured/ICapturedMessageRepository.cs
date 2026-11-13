using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Notifications.Domain.Captured;

/// <summary>
/// 捕获内容的仓储
/// </summary>
public interface ICapturedMessageRepository : IRepository<CapturedMessage, CapturedMessageId>;
