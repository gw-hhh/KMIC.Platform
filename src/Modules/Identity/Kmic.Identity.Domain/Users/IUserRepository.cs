using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Identity.Domain.Users;

/// <summary>
/// 用户的仓储
/// </summary>
public interface IUserRepository : IRepository<User, UserId>;
