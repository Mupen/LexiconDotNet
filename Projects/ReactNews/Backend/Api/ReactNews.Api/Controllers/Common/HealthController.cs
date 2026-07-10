using Microsoft.AspNetCore.Mvc;

namespace ReactNews.Api.Controllers.Common;

/// <summary>
/// Lightweight operational endpoints.
/// </summary>
/// <remarks>
/// What: confirms that the backend process is running.
/// Why: this verifies the API separately from NewsAPI and separately from the
/// React frontend.
/// How: /api/health returns static process information and does not call any
/// external dependency.
/// </remarks>
[ApiController]
public sealed class HealthController : ControllerBase
{
    /// <summary>
    /// Returns a friendly root response.
    /// </summary>
    /// <remarks>
    /// What: handles GET / so opening the API root does not look like a broken
    /// app.
    /// How: returns a small JSON object with useful local URLs.
    /// Why: development tools often open the backend root. A helpful response is
    /// clearer than an unexplained 404.
    /// </remarks>
    [HttpGet("/")]
    public IActionResult Root()
    {
        return Ok(new
        {
            application = "ReactNews.Api",
            message = "ReactNews API is running. Start the Vite frontend separately and open http://localhost:5173.",
            health = "/api/health",
            articles = "/api/articles"
        });
    }

    /// <summary>
    /// Returns basic API health information.
    /// </summary>
    /// <remarks>
    /// What: confirms the API process can answer requests.
    /// How: returns static application information and the current UTC time.
    /// Why: this endpoint should not depend on NewsAPI or the database because it
    /// is meant to isolate "is the API running?" from deeper dependency checks.
    /// </remarks>
    [HttpGet("/api/health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "ok",
            application = "ReactNews.Api",
            checkedAt = DateTimeOffset.UtcNow
        });
    }
}
