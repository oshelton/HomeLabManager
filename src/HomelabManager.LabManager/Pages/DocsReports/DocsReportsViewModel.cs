using HomeLabManager.Manager.Services.Navigation.Requests;

namespace HomeLabManager.Manager.Pages.DocsReports;

/// <summary>
/// Home Page View Model.
/// </summary>
public sealed class DocsReportsViewModel : PageBaseViewModel<DocsReportsViewModel>
{
    public DocsReportsViewModel(): base() {}

    public override string Title => "Documents and Reports";

    public override Task NavigateTo(INavigationRequest request) => Task.CompletedTask;

    public override Task<bool> TryNavigateAway() => Task.FromResult(true);

    protected override void Dispose(bool isDisposing) { }
}
