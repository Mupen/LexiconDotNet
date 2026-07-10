using ReactNews.Application.Contracts.Auth;
using ReactNews.Application.Interfaces;
using ReactNews.Application.Mapping;

namespace ReactNews.Application.Queries.Auth;

/// <summary>
/// What: Loads the currently authenticated account by id.
/// How: Reads from IUserStore and maps the user to AuthResponse when found.
/// Why: The API can get the id from cookie claims while Application owns user lookup.
/// </summary>
public sealed class GetCurrentUser
{
    private readonly IUserStore _userStore;

    public GetCurrentUser(IUserStore userStore)
    {
        _userStore = userStore;
    }

    public AuthResponse? Execute(string userId)
    {
        return _userStore.FindById(userId) is { } user
            ? new AuthResponse(user.ToDto())
            : null;
    }
}
