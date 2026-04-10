using SalesSystem.UnitTests.Api.Helpers;
using Xunit;

namespace SalesSystem.UnitTests.Api.Menus;

public class MainMenuTests
{
    [Fact]
    public async Task RunAsync_SelectSellTickets_ShouldCallSellTicketsMenu()
    {
        var ctx = new MainMenuTestContext();
        ctx.UI.AddInputs("1", "X");

        var menu = ctx.CreateMenu();

        await menu.RunAsync();

        ctx.AssertOnlyCalled(ctx.SellTicketsMenu);
    }

    [Fact]
    public async Task RunAsync_SelectSellProducts_ShouldCallSellProductsMenu()
    {
        var ctx = new MainMenuTestContext();
        ctx.UI.AddInputs("2", "X");

        var menu = ctx.CreateMenu();

        await menu.RunAsync();

        ctx.AssertOnlyCalled(ctx.SellProductsMenu);
    }

    [Fact]
    public async Task RunAsync_SelectManageMovies_ShouldCallManageMoviesMenu()
    {
        var ctx = new MainMenuTestContext();
        ctx.UI.AddInputs("3", "X");

        var menu = ctx.CreateMenu();

        await menu.RunAsync();

        ctx.AssertOnlyCalled(ctx.ManageMoviesMenu);
    }

    [Fact]
    public async Task RunAsync_SelectManageTickets_ShouldCallManageTicketsMenu()
    {
        var ctx = new MainMenuTestContext();
        ctx.UI.AddInputs("4", "X");

        var menu = ctx.CreateMenu();

        await menu.RunAsync();

        ctx.AssertOnlyCalled(ctx.ManageTicketsMenu);
    }

    [Fact]
    public async Task RunAsync_SelectManageProducts_ShouldCallManageProductsMenu()
    {
        var ctx = new MainMenuTestContext();
        ctx.UI.AddInputs("5", "X");

        var menu = ctx.CreateMenu();

        await menu.RunAsync();

        ctx.AssertOnlyCalled(ctx.ManageProductsMenu);
    }

    [Fact]
    public async Task RunAsync_SelectManageAccounting_ShouldCallManageAccountingMenu()
    {
        var ctx = new MainMenuTestContext();
        ctx.UI.AddInputs("6", "X");

        var menu = ctx.CreateMenu();

        await menu.RunAsync();

        ctx.AssertOnlyCalled(ctx.ManageAccountingMenu);
    }

    [Fact]
    public async Task RunAsync_SelectExitProgram_ShouldNotCallAnySubMenu()
    {
        var ctx = new MainMenuTestContext();
        ctx.UI.AddInput("X");

        var menu = ctx.CreateMenu();

        await menu.RunAsync();

        ctx.AssertNoneCalled();
        Assert.Contains("Exiting program...", ctx.UI.GetAllOutput());
    }

    [Fact]
    public async Task RunAsync_InvalidInput_ShouldShowInvalidOptionMessage()
    {
        var ctx = new MainMenuTestContext();
        ctx.UI.AddInputs("INVALID", "X");

        var menu = ctx.CreateMenu();

        await menu.RunAsync();

        ctx.AssertNoneCalled();
        Assert.Contains("Invalid Option.", ctx.UI.GetAllOutput());
    }
}