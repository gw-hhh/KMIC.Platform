using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Identity.Domain.Sessions;

/// <summary>
/// 会话的仓储
/// </summary>
public interface ISessionRepository : IRepository<Session, SessionId>;
