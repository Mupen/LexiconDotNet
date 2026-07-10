using ReactNews.Application.Contracts.Common;

namespace ReactNews.UnitTests.Application;

/// <summary>
/// What: Verifies the small Result wrapper used by use cases to return either a value or an expected error.
/// How: Creates success and failure results, then asserts the flags and stored payloads.
/// Why: A simple Result contract lets controllers handle expected failures without turning normal validation problems into exceptions.
/// </summary>
public sealed class ResultTests
{
    /// <summary>
    /// What: Checks that a successful result exposes its value and no error.
    /// How: Creates a success result with a string value and verifies the success/failure flags.
    /// Why: Callers depend on these flags to decide whether to map the response to HTTP 200 or to an error response.
    /// </summary>
    [Fact]
    public void Success_ReturnsValueWithoutError()
    {
        var result = Result<string>.Success("ok");

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal("ok", result.Value);
        Assert.Null(result.Error);
    }

    /// <summary>
    /// What: Checks that a failed result exposes an error and no value.
    /// How: Creates a validation error, wraps it in a failure result, and asserts the error code/message.
    /// Why: Keeping failure results value-free avoids accidental use of partial or invalid data after validation fails.
    /// </summary>
    [Fact]
    public void Failure_ReturnsErrorWithoutValue()
    {
        var error = Error.Validation("Invalid input.");

        var result = Result<string>.Failure(error);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Null(result.Value);
        Assert.Equal("validation_error", result.Error?.Code);
        Assert.Equal("Invalid input.", result.Error?.Message);
    }
}
