namespace ReactNews.UnitTests.ProjectGaps;

/// <summary>
/// What: Documents the remaining operational test gap around Docker startup.
/// How: Keeps one visible xUnit note while auth, admin, saved article, preference, and editorial behavior are covered by real tests elsewhere.
/// Why: Docker smoke tests need running containers and ports, so they belong in a separate operational verification step instead of normal unit tests.
/// </summary>
public sealed class DockerOperationalGapTests
{
    /// <summary>
    /// What: Keeps Docker smoke-test coverage visible in the test report.
    /// How: Uses a simple passing assertion and explains that Verify.ps1 currently builds backend tests and frontend assets, but does not start Docker.
    /// Why: Unit tests should not require Docker Desktop, fixed ports, or real container lifecycle work on every local test run.
    /// </summary>
    [Fact]
    public void DockerSmokeTest_IsDocumentedUntilOperationalVerificationIsAdded()
    {
        Assert.True(true);
    }
}
