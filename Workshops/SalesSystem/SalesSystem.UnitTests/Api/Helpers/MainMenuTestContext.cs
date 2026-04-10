using SalesSystem.Api.Menus;
using SalesSystem.UnitTests.Api.Fakes;
using Xunit;

namespace SalesSystem.UnitTests.Api.Helpers;

internal sealed class MainMenuTestContext
{
    public FakeUserIO UI { get; } = new();

    public FakeMenu SellTicketsMenu { get; } = new();
    public FakeMenu SellProductsMenu { get; } = new();
    public FakeMenu ManageMoviesMenu { get; } = new();
    public FakeMenu ManageTicketsMenu { get; } = new();
    public FakeMenu ManageProductsMenu { get; } = new();
    public FakeMenu ManageAccountingMenu { get; } = new();

    public MainMenu CreateMenu()
    {
        return new MainMenu(
            UI,
            SellTicketsMenu,
            SellProductsMenu,
            ManageMoviesMenu,
            ManageTicketsMenu,
            ManageProductsMenu,
            ManageAccountingMenu);
    }


    public void AssertOnlyCalled(FakeMenu expected)
    {
        var all = new[]
        {
        SellTicketsMenu,
        SellProductsMenu,
        ManageMoviesMenu,
        ManageTicketsMenu,
        ManageProductsMenu,
        ManageAccountingMenu};

        foreach (var menu in all)
        {
            if (ReferenceEquals(menu, expected))
                Assert.Equal(1, menu.RunCallCount);
            else
                Assert.Equal(0, menu.RunCallCount);
        }
    }

    public void AssertNoneCalled()
    {
        var all = new[]
        {
        SellTicketsMenu,
        SellProductsMenu,
        ManageMoviesMenu,
        ManageTicketsMenu,
        ManageProductsMenu,
        ManageAccountingMenu
    };

        foreach (var menu in all)
        {
            Assert.Equal(0, menu.RunCallCount);
        }
    }
}