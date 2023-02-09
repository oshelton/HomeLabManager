using System.Globalization;
using HomeLabManager.Common.Converters;

namespace HomeLabManager.CommonTests;

/// <summary>
/// Test CommonConverters.
/// </summary>
public class ConverterTests
{
    [Test]
    public void ObjectsAreEqual()
    {
        Assert.That(CommonConverters.ObjectsAreEqual.Convert(null, typeof(bool), null, CultureInfo.InvariantCulture) as bool?, Is.False);
        Assert.That(CommonConverters.ObjectsAreEqual.Convert(null, typeof(bool), 5, CultureInfo.InvariantCulture) as bool?, Is.False);
        Assert.That(CommonConverters.ObjectsAreEqual.Convert(5, typeof(bool), null, CultureInfo.InvariantCulture) as bool?, Is.False);
        Assert.That(CommonConverters.ObjectsAreEqual.Convert(5, typeof(bool), 3, CultureInfo.InvariantCulture) as bool?, Is.False);
        Assert.That(CommonConverters.ObjectsAreEqual.Convert(TestEnum.ONE, typeof(bool), TestEnum.TWO, CultureInfo.InvariantCulture) as bool?, Is.False);

        Assert.That(CommonConverters.ObjectsAreEqual.Convert(5, typeof(bool), 5, CultureInfo.InvariantCulture) as bool?, Is.True);
        Assert.That(CommonConverters.ObjectsAreEqual.Convert("Hi!", typeof(bool), "Hi!", CultureInfo.InvariantCulture) as bool?, Is.True);
        Assert.That(CommonConverters.ObjectsAreEqual.Convert(TestEnum.ONE, typeof(bool), TestEnum.ONE, CultureInfo.InvariantCulture) as bool?, Is.True);
    }

    private enum TestEnum
    {
        ONE,
        TWO,
        THREE,
    }
}
