using HomeLabManager.Common.Data.Git;
using Moq;

namespace HomeLabManager.ManagerTests.MockServiceExtensions;

/// <summary>
/// Class for commonly used mocking helpers for the Git Data Manager Service.
/// </summary>
internal static class MockGitDataManagerExtensions
{
    /// <summary>
    /// Initiallize an IGitDataManager mock with the bare required functionality to get a simple test to run.
    /// </summary>
    public static void InitializeMinimumRequiredConfiguration(this Mock<IGitDataManager> mockService)
    {
        if (mockService is null)
            throw new ArgumentNullException(nameof(mockService));

        mockService.Reset();
    }
}
