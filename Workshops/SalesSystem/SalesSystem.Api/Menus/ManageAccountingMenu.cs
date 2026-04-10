using SalesSystem.Api.Interfaces;
using SalesSystem.Api.UI;

namespace SalesSystem.Api.Menus;

public class ManageAccountingMenu : MenuBase
{
    public ManageAccountingMenu(IUserIO ui) : base(ui)
    {
    }

    public override Task RunAsync()
    {
        ShowHeader("Manage Accounting Menu");
        ShowMessage("Not implemented yet.");
        ShowPause();
        return Task.CompletedTask;
    }
}