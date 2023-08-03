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
    public async Task Constructor_PageTitleVariations_ConfirmExpectedPageTitleVariations()
    {
        using (var newServer = new CreateEditServerViewModel())
        {
            var request = new CreateEditServerNavigationRequest(true, new ServerHostDto() { Metadata = new ServerMetadataDto { DisplayName = "server" } });
            await newServer.NavigateTo(request).ConfigureAwait(false);
            Assert.That(newServer.Title, Is.EqualTo("Create New Server Host"));
        }

        using (var newVM = new CreateEditServerViewModel())
        {
            var vm = new ServerVmDto { Metadata = new ServerMetadataDto { DisplayName = "vm" } };
            var server = new ServerHostDto { Metadata = new ServerMetadataDto { DisplayName = "server" }, VMs = new[] { vm } };
            var request = new CreateEditServerNavigationRequest(true, vm);
            await newVM.NavigateTo(request).ConfigureAwait(false);
            Assert.That(newVM.Title, Is.EqualTo("Create New Virtual Machine"));
        }

        using (var editingServer = new CreateEditServerViewModel())
        {
            var request = new CreateEditServerNavigationRequest(false, new ServerHostDto { Metadata = new ServerMetadataDto { DisplayName = "server" } }, 5);
            await editingServer.NavigateTo(request).ConfigureAwait(false);
            Assert.That(editingServer.Title, Is.EqualTo("Editing server"));
        }

        using (var editingVM = new CreateEditServerViewModel())
        {
            var vm = new ServerVmDto { Metadata = new ServerMetadataDto { DisplayName = "vm" } };
            var server = new ServerHostDto { Metadata = new ServerMetadataDto { DisplayName = "server" }, VMs = new[] { vm } };
            var request = new CreateEditServerNavigationRequest(false, vm, 0);
            await editingVM.NavigateTo(request).ConfigureAwait(false);
            Assert.That(editingVM.Title, Is.EqualTo("Editing vm"));
        }
    }
}
