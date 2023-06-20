using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Manager.Pages.CreateEditServer;
using HomeLabManager.Manager.Pages.ServerListing;

namespace HomeLabManager.ManagerTests.Tests.Pages;

public sealed class CreateEditServerViewModelTests
{
    [SetUp]
    public void SetUp() => Utils.RegisterTestServices();

    [Test]
    public void TestCreation()
    {
        var newServer = new CreateEditServerViewModel(true, new ServerHostDto());
        Assert.That(newServer.Title, Is.EqualTo("Create New Server Host"));
        
        var newVM = new CreateEditServerViewModel(true, new ServerVmDto());
        Assert.That(newVM.Title, Is.EqualTo("Create New Virtual Machine"));

        var editingServer = new CreateEditServerViewModel(false, new ServerHostDto { Metadata = new ServerMetadataDto { DisplayName = "server" } });
        Assert.That(editingServer.Title, Is.EqualTo("Editing server"));

        var editingVM = new CreateEditServerViewModel(false, new ServerVmDto { Metadata = new ServerMetadataDto { DisplayName = "vm" } });
        Assert.That(editingVM.Title, Is.EqualTo("Editing vm"));
    }
}
