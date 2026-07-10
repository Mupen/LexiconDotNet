using ReactNews.Domain.Entities.Users;

namespace ReactNews.Application.Interfaces;

/// <summary>
/// What: Stores and retrieves ReactNews users.
/// How: Application use cases depend on this interface while Infrastructure owns EF Core persistence.
/// Why: Authentication rules should not depend directly on SQLite.
/// </summary>
public interface IUserStore
{
    User? FindByEmail(string email);

    User? FindById(string id);

    User Save(User user);

    bool Delete(string id);
}
