using System.Globalization;
using HomeLabManager.Common.Converters;
using HomeLabManager.Common.Utils;

namespace HomeLabManager.CommonTests;

/// <summary>
/// Test DisposableOperations.
/// </summary>
[TestFixture]
public sealed class DisposableOperationTests
{
    [Test]
    public void Construction_BasicFunctionality_ConfirmActionsRanAsExpected()
    {
        var testValue = 0;

        using (var operation = new DisposableOperation(() => testValue = 5, () => testValue = -5))
        {
            Assert.That(testValue, Is.EqualTo(5));
        }
        Assert.That(testValue, Is.EqualTo(-5));
    }
}
