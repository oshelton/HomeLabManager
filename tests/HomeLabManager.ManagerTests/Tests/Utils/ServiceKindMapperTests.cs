using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Common.Utils;

namespace HomeLabManager.ManagerTests.Tests.Utils;

public sealed class ServiceKindMapperTests
{
    [Test]
    public void GetIconKind_VerifyAllHandled_VerifyAllKindsReturnValidIcon()
    {
        foreach (var kind in Enum.GetValues<ServerKind>())
        {
            Assert.IsNotNull(ServerKindMapper.GetIconKind(kind));
        }

        Assert.IsNotNull(ServerKindMapper.GetIconKind(null));
    }

    [Test]
    public void GetLabel_VerifyAllHandled_VerifyAllKindsReturnValidLabels()
    {
        foreach (var kind in Enum.GetValues<ServerKind>())
        {
            Assert.IsNotNull(ServerKindMapper.GetLabel(kind));
        }

        Assert.IsNotNull(ServerKindMapper.GetIconKind(null));
    }
}
