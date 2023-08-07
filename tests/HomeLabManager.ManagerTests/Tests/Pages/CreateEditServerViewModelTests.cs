using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Manager.Pages.CreateEditServer;
using HomeLabManager.Manager.Services.Navigation;
using HomeLabManager.Manager.Services.Navigation.Requests;
using HomeLabManager.ManagerTests.MockServiceExtensions;
using Moq;

namespace HomeLabManager.ManagerTests.Tests.Pages;

public sealed class MetadataViewModelTests
{
    [SetUp]
    public void SetUp() => _services = Utils.RegisterTestServices();

    [Test]
    public async Task Constructor_PageTitleVariations_ConfirmExpectedPageTitleVariations()
    {
        var servers = _services.MockServerDataManager.SetupSimpleServers(1, _ => new[]
            {
                new ServerVmDto
                {
                    Metadata = new ServerMetadataDto
                    {
                        DisplayName = "VM 1",
                        Name = "VM-1"
                    }
                },
            });

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
            var request = new CreateEditServerNavigationRequest(false, servers[0]);
            await editingServer.NavigateTo(request).ConfigureAwait(false);
            Assert.That(editingServer.Title, Is.EqualTo($"Editing {servers[0].Metadata.DisplayName}"));
        }

        using (var editingVM = new CreateEditServerViewModel())
        {
            var vm = new ServerVmDto { Metadata = new ServerMetadataDto { DisplayName = "vm" } };
            var request = new CreateEditServerNavigationRequest(false, servers[0].VMs[0], 0);
            await editingVM.NavigateTo(request).ConfigureAwait(false);
            Assert.That(editingVM.Title, Is.EqualTo($"Editing {servers[0].VMs[0].Metadata.DisplayName}"));
        }
    }

    private (
        Mock<ICoreConfigurationManager> MockCoreConfigManager,
        Mock<IServerDataManager> MockServerDataManager,
        Mock<INavigationService> MockNavigationService
    ) _services;
}
