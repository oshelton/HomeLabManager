using System.Reactive.Subjects;
using HomeLabManager.Common.Data.CoreConfiguration;
using Moq;

namespace HomeLabManager.ManagerTests.MockServiceExtensions;

/// <summary>
/// Class for commonly used mocking helpers for the Core Configuration manager service.
/// </summary>
internal static class MockCoreConfigurationManagerExtensions
{
    /// <summary>
    /// Initiallize an ICoreConfigurationManager mock with the bare required functionality to get a simple test to run.
    /// </summary>
    public static void InitializeMinimumRequiredConfiguration(this Mock<ICoreConfigurationManager> mockService)
    {
        if (mockService is null)
            throw new ArgumentNullException(nameof(mockService));

        mockService.Reset();

        mockService.SetupGet(x => x.CoreConfigurationUpdated).Returns(() => new Subject<CoreConfigurationDto>());
        mockService.Setup(x => x.GetCoreConfiguration()).Returns(() => new CoreConfigurationDto());
    }
}
