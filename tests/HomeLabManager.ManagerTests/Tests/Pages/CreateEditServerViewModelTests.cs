using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Manager.Pages.CreateEditServer;
using HomeLabManager.Manager.Pages.ServerListing;
using HomeLabManager.Manager.Services.Navigation.Requests;

namespace HomeLabManager.ManagerTests.Tests.Pages;

public sealed class MetadataViewModelTests
{
    [SetUp]
    public void SetUp() => Utils.RegisterTestServices();

    [Test]
    public async Task Constructor_PageTitleVariations()
    {
        var newServer = new CreateEditServerViewModel();
        var request = new CreateEditServerNavigationRequest(true, new ServerHostDto() { Metadata = new ServerMetadataDto { DisplayName = "server" } }, Array.Empty<string>(), Array.Empty<string>());
        await newServer.NavigateTo(request).ConfigureAwait(false);
        Assert.That(newServer.Title, Is.EqualTo("Create New Server Host"));
        
        var newVM = new CreateEditServerViewModel();
        request = new CreateEditServerNavigationRequest(true, new ServerVmDto() { Metadata = new ServerMetadataDto { DisplayName = "vm" } }, Array.Empty<string>(), Array.Empty<string>());
        await newVM.NavigateTo(request).ConfigureAwait(false);
        Assert.That(newVM.Title, Is.EqualTo("Create New Virtual Machine"));

        var editingServer = new CreateEditServerViewModel();
        request = new CreateEditServerNavigationRequest(false, new ServerHostDto { Metadata = new ServerMetadataDto { DisplayName = "server" } }, Array.Empty<string>(), Array.Empty<string>());
        await editingServer.NavigateTo(request).ConfigureAwait(false);
        Assert.That(editingServer.Title, Is.EqualTo("Editing server"));

        var editingVM = new CreateEditServerViewModel();
        request = new CreateEditServerNavigationRequest(false, new ServerVmDto { Metadata = new ServerMetadataDto { DisplayName = "vm" } }, Array.Empty<string>(), Array.Empty<string>());
        await editingVM.NavigateTo(request).ConfigureAwait(false);
        Assert.That(editingVM.Title, Is.EqualTo("Editing vm"));
    }
}
