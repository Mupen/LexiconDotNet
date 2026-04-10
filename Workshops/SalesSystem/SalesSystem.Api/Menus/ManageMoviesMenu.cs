using SalesSystem.Api.Interfaces;
using SalesSystem.Api.UI;

namespace SalesSystem.Api.Menus;

public class ManageMoviesMenu : MenuBase
{
    public ManageMoviesMenu(IUserIO ui) : base(ui)
    {
    }

    public override Task RunAsync()
    {
        ShowHeader("Manage Movies Menu");
        ShowMessage("Not implemented yet.");
        ShowPause();
        return Task.CompletedTask;
    }
}