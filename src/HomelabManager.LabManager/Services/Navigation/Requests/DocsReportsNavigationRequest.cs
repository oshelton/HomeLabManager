using HomeLabManager.Manager.Pages;
using HomeLabManager.Manager.Pages.DocsReports;

namespace HomeLabManager.Manager.Services.Navigation.Requests;

/// <summary>
/// Navigation Request class for the Docs/Reports Page.
/// </summary>
public sealed class DocsReportsNavigationRequest : INavigationRequest
{
    /// <summary>
    /// Create instance of the Docs/Reports Page.
    /// </summary>
    public PageBaseViewModel CreatePage() => new DocsReportsViewModel();
}
