using SalesSystem.Api.Interfaces;
using SalesSystem.Api.UI;

namespace SalesSystem.Api.Menus;

public class ManageTicketsMenu : MenuBase
{
    public ManageTicketsMenu(IUserIO ui) : base(ui)
    {
    }

    public override Task RunAsync()
    {
        ShowHeader("Manage Tickets Menu");
        ShowMessage("Not implemented yet.");
        ShowPause();
        return Task.CompletedTask;
    }
}