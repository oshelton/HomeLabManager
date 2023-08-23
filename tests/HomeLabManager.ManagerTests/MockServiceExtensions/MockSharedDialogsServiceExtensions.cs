using HomeLabManager.Manager.Services.SharedDialogs;
using Moq;

namespace HomeLabManager.ManagerTests.MockServiceExtensions;

/// <summary>
/// Class for commonly used mocking helpers for the Navigation Service.
/// </summary>
internal static class MockSharedDialogsServiceExtensions
{
    /// <summary>
    /// Initiallize an INavigationService mock with the bare required functionality to get a simple test to run.
    /// </summary>
    public static void InitializeMinimumRequiredConfiguration(this Mock<ISharedDialogsService> mockService)
    {
        if (mockService is null)
            throw new ArgumentNullException(nameof(mockService));

        mockService.Reset();
    }
}
