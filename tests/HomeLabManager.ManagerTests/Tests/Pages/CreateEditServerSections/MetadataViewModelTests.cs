using HomeLabManager.Common.Data.Git.Server;
using HomeLabManager.Manager.Pages.CreateEditServer;
using HomeLabManager.Manager.Pages.CreateEditServer.Sections;
using HomeLabManager.Manager.Pages.ServerListing;
using HomeLabManager.Manager.Services.Navigation.Requests;

namespace HomeLabManager.ManagerTests.Tests.Pages.CreateEditServerSections;

public sealed class MetadataViewModelTests
{
    [SetUp]
    public void SetUp() => Utils.RegisterTestServices();

    [Test]
    public async Task Constructor_ConfirmExpectedConstructorBehavior()
    {
        var server = new ServerHostDto
        {
            Metadata = new ServerMetadataDto
            {
                DisplayName = "Test",
                Name = "TEST-SERVER-NAME"
            }
        };
        using (var editor = new MetadataViewModel(server, Array.Empty<string>(), Array.Empty<string>()))
        {
            Assert.That(editor.HasChanges, Is.False);
            Assert.That(editor.HasErrors, Is.False);
            Assert.That(editor.Validator, Is.Not.Null);
            Assert.That(editor.DisplayName, Is.EqualTo(server.Metadata.DisplayName));
            Assert.That(editor.Name, Is.EqualTo(server.Metadata.Name));
        }
    }
}
