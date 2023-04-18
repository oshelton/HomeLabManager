using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeLabManager.Manager.Pages;

namespace HomeLabManager.Manager.Services.Navigation.Requests;

/// <summary>
/// Interface for navigation requests between pages.
/// </summary>
public interface INavigationRequest
{
    // Create an instance of the associated Page.
    PageBaseViewModel CreatePage();
}
