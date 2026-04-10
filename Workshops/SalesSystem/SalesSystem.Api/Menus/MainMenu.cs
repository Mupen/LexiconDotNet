using SalesSystem.Api.Helpers;
using SalesSystem.Api.Interfaces;
using SalesSystem.Api.UI;

namespace SalesSystem.Api.Menus;

public class MainMenu : MenuBase
{
    private readonly IMenu _sellTicketsMenu;
    private readonly IMenu _sellProductsMenu;
    private readonly IMenu _manageMoviesMenu;
    private readonly IMenu _manageTicketsMenu;
    private readonly IMenu _manageProductsMenu;
    private readonly IMenu _manageAccountingMenu;


    public MainMenu(
        IUserIO ui,
        IMenu sellTicketsMenu,
        IMenu sellProductsMenu,
        IMenu manageMoviesMenu,
        IMenu manageTicketsMenu,
        IMenu manageProductsMenu,
        IMenu manageAccountingMenu) : base(ui)
    {
        _sellTicketsMenu = sellTicketsMenu;
        _sellProductsMenu = sellProductsMenu;
        _manageMoviesMenu = manageMoviesMenu;
        _manageTicketsMenu = manageTicketsMenu;
        _manageProductsMenu = manageProductsMenu;
        _manageAccountingMenu = manageAccountingMenu;
    }

    public override async Task RunAsync()
    {
        var isRunning = true;

        while (isRunning)
        {
            var input = ShowMenu();
            var action = ParseMenuAction(input);

            switch (action)
            {
                case MenuAction.SellTickets:
                    await _sellTicketsMenu.RunAsync();
                    break;

                case MenuAction.SellProducts:
                    await _sellProductsMenu.RunAsync();
                    break;

                case MenuAction.ManageMovies:
                    await _manageMoviesMenu.RunAsync();
                    break;

                case MenuAction.ManageTickets:
                    await _manageTicketsMenu.RunAsync();
                    break;

                case MenuAction.ManageProducts:
                    await _manageProductsMenu.RunAsync();
                    break;

                case MenuAction.ManageAccounting:
                    await _manageAccountingMenu.RunAsync();
                    break;

                case MenuAction.ExitProgram:
                    _ui.Clear();
                    ShowMessage("Exiting program...");
                    ShowPause();
                    isRunning = false;
                    break;

                case MenuAction.InvalidInput:
                    _ui.Clear();
                    ShowMessage("Invalid Option.");
                    ShowPause();
                    break;

            }
        }
    }

    private enum MenuAction
    {
        SellTickets,
        SellProducts,
        ManageMovies,
        ManageTickets,
        ManageProducts,
        ManageAccounting,
        ExitProgram,
        InvalidInput
    }


    private static MenuAction ParseMenuAction(string? input)
    {
        return UserInput.Normalize(input) switch
        {
            "1" => MenuAction.SellTickets,
            "2" => MenuAction.SellProducts,
            "3" => MenuAction.ManageMovies,
            "4" => MenuAction.ManageTickets,
            "5" => MenuAction.ManageProducts,
            "6" => MenuAction.ManageAccounting,
            "X" => MenuAction.ExitProgram,
            _ => MenuAction.InvalidInput
        };
    }

    private string ShowMenu()
    {
        ShowHeader("Main Menu",
            "1. Sell Tickets",
            "2. Sell Products",
            "3. Manage Movies",
            "4. Manage Tickets",
            "5. Manage Products",
            "6. Manage Accounting",
            "X. Exit Program");

        return ShowPrompt(prompt: "Select Option: ");
    }
}
