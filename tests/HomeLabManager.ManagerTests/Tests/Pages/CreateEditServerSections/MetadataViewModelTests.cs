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
    public void Constructor_ConfirmExpectedConstructorBehavior()
    {
        var server = new ServerHostDto
        {
            Metadata = new ServerMetadataDto
            {
                DisplayName = "Test",
                Name = "TEST-SERVER-NAME"
            }
        };
        using (var editor = new MetadataEditViewModel(server, Array.Empty<string>(), Array.Empty<string>()))
        {
            Assert.That(editor.HasChanges, Is.False);
            Assert.That(editor.HasErrors, Is.False);
            Assert.That(editor.Validator, Is.Not.Null);
            Assert.That(editor.DisplayName, Is.EqualTo(server.Metadata.DisplayName));
            Assert.That(editor.Name, Is.EqualTo(server.Metadata.Name));
        }
    }

    [Test]
    public async Task DisplayName_Validation_ConfirmDisplayNameValidationWorksAsExpected()
    {
        var server = new ServerHostDto
        {
            Metadata = new ServerMetadataDto
            {
                DisplayName = "Test",
                Name = "TEST-SERVER-NAME"
            }
        };
        // Test not-null/empty rule.
        using (var editor = new MetadataEditViewModel(server, Array.Empty<string>(), Array.Empty<string>()))
        {
            Assert.That(editor.HasErrors, Is.False);

            editor.DisplayName = null;

            await editor.Validator.WaitValidatingCompletedAsync().ConfigureAwait(false);

            Assert.That(editor.HasErrors, Is.True);
            Assert.That(editor.GetErrors(nameof(editor.DisplayName)), Is.Not.Null);

            editor.DisplayName = "";

            await editor.Validator.WaitValidatingCompletedAsync().ConfigureAwait(false);

            Assert.That(editor.HasErrors, Is.True);
            Assert.That(editor.GetErrors(nameof(editor.DisplayName)), Is.Not.Null);

            editor.DisplayName = "Hi!";

            await editor.Validator.WaitValidatingCompletedAsync().ConfigureAwait(false);

            Assert.That(editor.HasErrors, Is.False);
        }

        var notUniqueDisplayName = "failed";
        // Test uniqueness of display name.
        using (var editor = new MetadataEditViewModel(server, new[] { notUniqueDisplayName }, Array.Empty<string>()))
        {
            editor.DisplayName = notUniqueDisplayName;

            await editor.Validator.WaitValidatingCompletedAsync().ConfigureAwait(false);

            Assert.That(editor.HasErrors, Is.True);
            Assert.That(editor.GetErrors(nameof(editor.DisplayName)), Is.Not.Null);

            editor.DisplayName = "Hi!";

            await editor.Validator.WaitValidatingCompletedAsync().ConfigureAwait(false);

            Assert.That(editor.HasErrors, Is.False);
        }
    }

    [Test]
    public async Task Name_Validation_ConfirmNameValidationWorksAsExpected()
    {
        var server = new ServerHostDto
        {
            Metadata = new ServerMetadataDto
            {
                DisplayName = "Test",
                Name = "TEST-SERVER-NAME"
            }
        };
        // Test not-null/empty rule.
        using (var editor = new MetadataEditViewModel(server, Array.Empty<string>(), Array.Empty<string>()))
        {
            Assert.That(editor.HasErrors, Is.False);

            editor.Name = null;

            await editor.Validator.WaitValidatingCompletedAsync().ConfigureAwait(false);

            Assert.That(editor.HasErrors, Is.True);
            Assert.That(editor.GetErrors(nameof(editor.Name)), Is.Not.Null);

            editor.Name = "";

            await editor.Validator.WaitValidatingCompletedAsync().ConfigureAwait(false);

            Assert.That(editor.HasErrors, Is.True);
            Assert.That(editor.GetErrors(nameof(editor.Name)), Is.Not.Null);

            editor.Name = "Hi";

            await editor.Validator.WaitValidatingCompletedAsync().ConfigureAwait(false);

            Assert.That(editor.HasErrors, Is.False);
        }

        // Test name meets expected format.
        using (var editor = new MetadataEditViewModel(server, Array.Empty<string>(), Array.Empty<string>()))
        {
            await editor.Validator.WaitValidatingCompletedAsync().ConfigureAwait(false);

            Assert.That(editor.HasErrors, Is.False);

            editor.Name = "-invalid";

            await editor.Validator.WaitValidatingCompletedAsync().ConfigureAwait(false);

            Assert.That(editor.HasErrors, Is.True);
            Assert.That(editor.GetErrors(nameof(editor.Name)), Is.Not.Null);

            editor.Name = "valid.google.com";

            await editor.Validator.WaitValidatingCompletedAsync().ConfigureAwait(false);

            Assert.That(editor.HasErrors, Is.False);

            editor.Name = "!-invalid";

            await editor.Validator.WaitValidatingCompletedAsync().ConfigureAwait(false);

            Assert.That(editor.HasErrors, Is.True);
            Assert.That(editor.GetErrors(nameof(editor.Name)), Is.Not.Null);
        }

        var notUniqueName = "failed";
        // Test uniqueness of name.
        using (var editor = new MetadataEditViewModel(server, Array.Empty<string>(), new[] { notUniqueName }))
        {
            editor.Name = notUniqueName;

            await editor.Validator.WaitValidatingCompletedAsync().ConfigureAwait(false);

            Assert.That(editor.HasErrors, Is.True);
            Assert.That(editor.GetErrors(nameof(editor.Name)), Is.Not.Null);

            editor.Name = "Hi";

            await editor.Validator.WaitValidatingCompletedAsync().ConfigureAwait(false);

            Assert.That(editor.HasErrors, Is.False);
        }
    }
}
