using HomeLabManager.Common.Data.CoreConfiguration;
using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Manager.Pages.CreateEditServer;
using HomeLabManager.Manager.Services.Navigation;
using HomeLabManager.Manager.Services.Navigation.Requests;
using Moq;

namespace HomeLabManager.ManagerTests.Tests.Pages;

public sealed class CreateEditServerViewModelTests
{
    [SetUp]
    public void SetUp() => _services = Utils.RegisterTestServices();

    [Test]
    public async Task Constructor_PageTitleVariations_ConfirmExpectedPageTitleVariations()
    {
        // ARRANGE
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
            
            // ACT
            await newServer.NavigateTo(request).ConfigureAwait(false);

            // ASSERT
            Assert.That(newServer.Title, Is.EqualTo("Create New Server Host"));
        }

        // ARRANGE
        using (var newVM = new CreateEditServerViewModel())
        {
            var vm = new ServerVmDto { Metadata = new ServerMetadataDto { DisplayName = "vm" } };
            var server = new ServerHostDto { Metadata = new ServerMetadataDto { DisplayName = "server" }, VMs = new[] { vm } };
            var request = new CreateEditServerNavigationRequest(true, vm);
            
            // ACT
            await newVM.NavigateTo(request).ConfigureAwait(false);

            // ASSERT
            Assert.That(newVM.Title, Is.EqualTo("Create New Virtual Machine"));
        }

        // ARRANGE
        using (var editingServer = new CreateEditServerViewModel())
        {
            var request = new CreateEditServerNavigationRequest(false, servers[0]);

            // ACT
            await editingServer.NavigateTo(request).ConfigureAwait(false);

            // ASSERT
            Assert.That(editingServer.Title, Is.EqualTo($"Editing {servers[0].Metadata.DisplayName}"));
        }

        // ARRANGE
        using (var editingVM = new CreateEditServerViewModel())
        {
            var vm = new ServerVmDto { Metadata = new ServerMetadataDto { DisplayName = "vm" } };
            var request = new CreateEditServerNavigationRequest(false, servers[0].VMs[0], 0);

            // ACT
            await editingVM.NavigateTo(request).ConfigureAwait(false);

            // ASSERT
            Assert.That(editingVM.Title, Is.EqualTo($"Editing {servers[0].VMs[0].Metadata.DisplayName}"));
        }
    }

    [Test]
    public async Task Constructor_InitializeEditorViewModels_ConfirmEditingViewModelsConstructedAsExpected()
    {
        // ARRANGE
        var servers = _services.MockServerDataManager.SetupSimpleServers(2, generateIds: true);

        using (var newServer = new CreateEditServerViewModel())
        {
            var request = new CreateEditServerNavigationRequest(true, servers[1]);

            // ACT
            await newServer.NavigateTo(request).ConfigureAwait(false);

            // ASSERT
            // Metadata
            Assert.That(newServer.Metadata, Is.Not.Null);
            Assert.That(newServer.Metadata.DisplayName, Is.EqualTo(servers[1].Metadata.DisplayName));
            Assert.That(newServer.Metadata.Name, Is.EqualTo(servers[1].Metadata.Name));

            //TODO: Add support for other editing sections.
        }
    }

    [Test]
    public async Task HasChanges_MetadataChangesDetected_ConfirmMetadataChangesAreDetected()
    {
        // ARRANGE
        var servers = _services.MockServerDataManager.SetupSimpleServers(1, generateIds: true);
        var toEdit = servers[0];
        var initialDisplayName = toEdit.Metadata.DisplayName;
        var initialName = toEdit.Metadata.Name;

        using (var newServer = new CreateEditServerViewModel())
        {
            var request = new CreateEditServerNavigationRequest(true, toEdit);

            await newServer.NavigateTo(request).ConfigureAwait(false);

            // ASSERT
            Assert.That(newServer.CanSave, Is.False);

            // Display Name testing.
            {
                // ACT
                newServer.Metadata.DisplayName = "new name";

                await Task.Delay(600).ConfigureAwait(false);

                // ASSERT
                Assert.That(newServer.HasChanges, Is.True);

                // ACT
                newServer.Metadata.DisplayName = initialDisplayName;

                await Task.Delay(600).ConfigureAwait(false);

                // ASSERT
                Assert.That(newServer.HasChanges, Is.False);
            }

            // Name testing.
            {
                // ACT
                newServer.Metadata.Name = "NEW-HOST";

                await Task.Delay(600).ConfigureAwait(false);

                // ASSERT
                Assert.That(newServer.HasChanges, Is.True);

                // ACT
                newServer.Metadata.Name = initialName;

                await Task.Delay(600).ConfigureAwait(false);

                // ASSERT
                Assert.That(newServer.HasChanges, Is.False);
            }
        }
    }

    [Test]
    public async Task HasErrors_MetadataErrorsDetected_ConfirmMetadataErrorsAreDetected()
    {
        // ARRANGE
        var servers = _services.MockServerDataManager.SetupSimpleServers(1, generateIds: true);
        var toEdit = servers[0];
        var initialDisplayName = toEdit.Metadata.DisplayName;
        var initialName = toEdit.Metadata.Name;

        using (var newServer = new CreateEditServerViewModel())
        {
            var request = new CreateEditServerNavigationRequest(true, toEdit);

            await newServer.NavigateTo(request).ConfigureAwait(false);

            // ASSERT
            Assert.That(newServer.CanSave, Is.False);

            // Display Name testing.
            {
                // ACT
                newServer.Metadata.DisplayName = null;

                // ASSERT
                Assert.That(newServer.HasErrors, Is.True);

                // ACT
                newServer.Metadata.DisplayName = initialDisplayName;

                // ASSERT
                Assert.That(newServer.HasErrors, Is.False);
            }

            // Name testing.
            {
                // ACT
                newServer.Metadata.Name = null;

                // ASSERT
                Assert.That(newServer.HasErrors, Is.True);

                // ACT
                newServer.Metadata.Name = initialName;

                // ASSERT
                Assert.That(newServer.HasErrors, Is.False);
            }
        }
    }

    [Test]
    public async Task CanSave_MetadataChanges_ConfirmMetadataChangesResultInCanSaveBeingTrue()
    {
        // ARRANGE
        var servers = _services.MockServerDataManager.SetupSimpleServers(1, generateIds: true);
        var toEdit = servers[0];
        var initialDisplayName = toEdit.Metadata.DisplayName;
        var initialName = toEdit.Metadata.Name;

        using (var newServer = new CreateEditServerViewModel())
        {
            var request = new CreateEditServerNavigationRequest(true, toEdit);

            await newServer.NavigateTo(request).ConfigureAwait(false);

            // ASSERT
            Assert.That(newServer.CanSave, Is.False);

            // Display Name testing.
            {
                // ACT
                newServer.Metadata.DisplayName = "new name";

                await Task.Delay(600).ConfigureAwait(false);

                // ASSERT
                Assert.That(newServer.CanSave, Is.True);

                // ACT
                newServer.Metadata.DisplayName = initialDisplayName;

                await Task.Delay(600).ConfigureAwait(false);

                // ASSERT
                Assert.That(newServer.CanSave, Is.False);
            }

            // Name testing.
            {
                // ACT
                newServer.Metadata.Name = "NEW-HOST";

                await Task.Delay(600).ConfigureAwait(false);

                // ASSERT
                Assert.That(newServer.CanSave, Is.True);

                // ACT
                newServer.Metadata.Name = initialName;

                await Task.Delay(600).ConfigureAwait(false);

                // ASSERT
                Assert.That(newServer.CanSave, Is.False);
            }
        }
    }

    private (
        Mock<ICoreConfigurationManager> MockCoreConfigManager,
        Mock<IServerDataManager> MockServerDataManager,
        Mock<INavigationService> MockNavigationService
    ) _services;
}
