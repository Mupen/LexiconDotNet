using SalesSystem.Api.Helpers;
using SalesSystem.Api.Interfaces;
using SalesSystem.Api.UI;
using SalesSystem.Application.Queries.Products;
using SalesSystem.Application.Requests.Products;
using SalesSystem.Application.UseCases.Products;
using SalesSystem.Domain.Entities;

namespace SalesSystem.Api.Menus;

public class SellProductsMenu : MenuBase
{
    private readonly GetAvailableProducts _availableProducts;
    private readonly DecreaseProductStock _sellProduct;

    public SellProductsMenu(IUserIO ui, GetAvailableProducts availableProducts, DecreaseProductStock sellProduct) : base(ui)
    {
        _availableProducts = availableProducts;
        _sellProduct = sellProduct;
    }

    public override async Task RunAsync()
    {
        var cart = new Dictionary<int, int>();
        string? message = null;

        while (true)
        {

            var products = (await _availableProducts.ExecuteAsync()).ToList();
            

            var input = ShowMenu(products, cart, message);
            var action = ParseMenuAction(input);
            if (action is not null)
            {
                switch (action)
                {
                    case MenuAction.ConfirmTransaction:
                        if (await ConfirmAndCompleteTransactionAsync(cart, products))
                            return;
                        message = null;
                        break;

                    case MenuAction.HelpTransaction:
                        ShowHelpScreen();
                        message = null;
                        break;

                    case MenuAction.CancelTransaction:
                        ShowCancelledScreen();
                        return;

                    default:
                        message = ProcessCartInput(input, cart, products);
                        break;
                }
            }
            else
            {
                message = ProcessCartInput(input, cart, products);
            }
        }
    }

    private enum MenuAction
    {
        ConfirmTransaction,
        HelpTransaction,
        CancelTransaction
    }

    private static MenuAction? ParseMenuAction(string? input)
    {
        return UserInput.Normalize(input) switch
        {
            "C" => MenuAction.ConfirmTransaction,
            "H" => MenuAction.HelpTransaction,
            "X" => MenuAction.CancelTransaction,
            _ => null
        };
    }

    private string ShowMenu(List<Product> products, Dictionary<int, int> cart, string? message)
    {
        ShowHeader("Products Menu",
            "C. Confirm Transaction",
            "H. Help Transaction",
            "X. Cancel Transaction");

        ShowProducts(products, cart);
        ShowCart(cart, products);
        if (!string.IsNullOrWhiteSpace(message))
        {
            ShowMessage(message);
        }
        return ShowPrompt(prompt: "> ");
    }

    private void ShowProducts(List<Product> products, Dictionary<int, int> cart)
    {
        ShowSubHeader("Products List");

        const int totalWidth = 100;

        var sortedProducts = products
            .OrderBy(p => p.ProductNumber)
            .ToList();

        bool hasVisibleProducts = false;

        foreach (var product in sortedProducts)
        {
            cart.TryGetValue(product.ProductNumber, out int quantityInCart);
            int displayStock = product.StockQuantity - quantityInCart;

            if (displayStock <= 0)
                continue;

            hasVisibleProducts = true;

            string left =
                $"Product: {product.ProductNumber,4} | " +
                $"Qty: {displayStock,4} | " +
                $"Name: ";

            string right =
                $" | Price: {product.GrossPrice,4} SEK";

            int nameWidth = totalWidth - left.Length - right.Length;
            if (nameWidth < 0)
                nameWidth = 0;

            string name = product.Name.Length > nameWidth
                ? product.Name[..Math.Max(0, nameWidth)]
                : product.Name.PadRight(nameWidth);

            _ui.WriteLine(left + name + right);
        }

        if (!hasVisibleProducts)
        {
            _ui.WriteLine("No products available.");
        }
    }

    private void ShowCart(Dictionary<int, int> cart, List<Product> products)
    {
        ShowSubHeader("Transaction Actions");

        const int totalWidth = 100;

        int totalItems = 0;
        decimal total = 0;

        foreach (var item in cart)
        {
            totalItems += item.Value;

            var product = products.First(p => p.ProductNumber == item.Key);
            total += product.GrossPrice * item.Value;
        }

        // Summary FIRST
        _ui.WriteLine($"Current Cart: Products: {cart.Count}, Items: {totalItems}, Total: {total} SEK");

        if (cart.Count == 0)
        {
            return;
        }

        _ui.WriteLine(new string('-', totalWidth));

        foreach (var item in cart)
        {
            var product = products.First(p => p.ProductNumber == item.Key);
            var lineTotal = product.GrossPrice * item.Value;

            string left =
                $"Product: {product.ProductNumber,4} | " +
                $"Qty: {item.Value,4} | " +
                $"Name: {product.Name}";

            string right =
                $" | Price: {product.GrossPrice,4} SEK | Total: {lineTotal,4} SEK";

            int spaces = totalWidth - left.Length - right.Length;
            if (spaces < 1) spaces = 1;

            _ui.WriteLine(left + new string(' ', spaces) + right);
        }

        _ui.WriteLine(new string('-', totalWidth));
    }


    private bool? ShowConfirmationScreen(Dictionary<int, int> cart, List<Product> products)
    {
        while (true)
        {
            _ui.Clear();
            _ui.WriteLine("=== Confirm Transaction Screen ===");
            _ui.WriteLine();

            if (cart.Count == 0)
            {
                _ui.WriteLine("Cart is empty.");
                _ui.WriteLine();
                _ui.WriteLine("Press any key to return...");
                _ui.WaitForKey();
                return false;
            }

            decimal total = 0;

            foreach (var item in cart)
            {
                var product = products.First(p => p.ProductNumber == item.Key);
                var lineTotal = product.GrossPrice * item.Value;

                _ui.WriteLine(
                    $"{product.ProductNumber} | {product.Name} | {item.Value} x {product.GrossPrice} SEK = {lineTotal} SEK");

                total += lineTotal;
            }

            _ui.WriteLine();
            _ui.WriteLine($"Total: {total} SEK");
            _ui.WriteLine();
            _ui.WriteLine("C = Confirm the transaction");
            _ui.WriteLine("B = Back to transaction");
            _ui.WriteLine("X = Cancel the transaction");
            _ui.WriteLine();
            _ui.Write("> ");

            var input = _ui.ReadLine()?.Trim().ToUpper();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            if (input == "C")
                return true;

            if (input == "B")
                return false;

            if (input == "X")
                return null;
        }
    }

    private void ShowHelpScreen()
    {
        _ui.Clear();
        _ui.WriteLine("========== Help Transaction Screen ==========");
        _ui.WriteLine();
        _ui.WriteLine("  number:quantity          Add item");
        _ui.WriteLine("  R number:quantity        Remove item");
        _ui.WriteLine("  C                        Complete transaction");
        _ui.WriteLine("  X                        Cancel / Exit");
        _ui.WriteLine("  H                        Show help");
        _ui.WriteLine();
        _ui.WriteLine("Examples:");
        _ui.WriteLine("  2:1");
        _ui.WriteLine("  2:1, 3:2");
        _ui.WriteLine("  2:1, R 3:1");
        _ui.WriteLine();
        _ui.WriteLine("Press any key to continue...");
        _ui.WriteLine();
        _ui.Write("> ");
        _ui.WaitForKey();
    }


    private void ShowCancelledScreen()
    {
        _ui.Clear();
        _ui.WriteLine("========== Cancelled Transaction Screen ==========");
        _ui.WriteLine();
        _ui.WriteLine("Transaction cancelled.");
        _ui.WriteLine();
        _ui.WriteLine("Press any key to return to main menu...");
        _ui.WriteLine();
        _ui.Write("> ");
        _ui.WaitForKey();
    }

    private string? ProcessCartInput(string input, Dictionary<int, int> cart, List<Product> products)
    {
        var entries = input.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var entry in entries)
        {
            var trimmed = entry.Trim();

            bool isRemove = trimmed.StartsWith("R ");

            if (isRemove)
                trimmed = trimmed.Substring(2).Trim();

            var parts = trimmed.Split(':');

            if (parts.Length != 2)
                return $"Invalid format: {entry}";

            if (!int.TryParse(parts[0].Trim(), out int productNumber) ||
                !int.TryParse(parts[1].Trim(), out int quantity))
            {
                return $"Invalid numbers: {entry}";
            }

            var product = products.FirstOrDefault(p => p.ProductNumber == productNumber);

            if (product is null)
                return $"Product {productNumber} not found.";

            string? resultMessage = isRemove
                ? RemoveFromCart(cart, productNumber, quantity)
                : AddToCart(cart, product, quantity);

            if (resultMessage is not null)
                return resultMessage;
        }

        return "Cart updated.";
    }

    private async Task<bool> ConfirmAndCompleteTransactionAsync(Dictionary<int, int> cart, List<Product> products)
    {
        var confirmed = ShowConfirmationScreen(cart, products);

        switch (confirmed)
        {
            case true:
                await CompleteTransaction(cart, products);
                _ui.WriteLine();
                _ui.WriteLine("Press any key to return to main menu...");
                _ui.WriteLine();
                _ui.Write("> ");
                _ui.WaitForKey();
                return true;

            case null:
                ShowCancelledScreen();
                return true;

            default:
                return false;
        }
    }

    private string? AddToCart(Dictionary<int, int> cart, Product product, int quantity)
    {
        if (quantity <= 0)
            return "Quantity must be greater than zero.";

        cart.TryGetValue(product.ProductNumber, out int existing);

        if (existing + quantity > product.StockQuantity)
            return $"Not enough stock for {product.Name}.";

        cart[product.ProductNumber] = existing + quantity;
        return $"Added {quantity} x {product.Name}.";
    }

    private string? RemoveFromCart(Dictionary<int, int> cart, int productNumber, int quantity)
    {
        if (quantity <= 0)
            return "Quantity must be greater than zero.";

        if (!cart.TryGetValue(productNumber, out int existing))
            return $"Product {productNumber} not in cart.";

        if (quantity > existing)
            return "Cannot remove more than in cart.";

        if (quantity == existing)
            cart.Remove(productNumber);
        else
            cart[productNumber] = existing - quantity;

        return $"Removed {quantity} from product {productNumber}.";
    }

    private async Task CompleteTransaction(Dictionary<int, int> cart, List<Product> products)
    {
        if (cart.Count == 0)
        {
            _ui.WriteLine("Cart is empty.");
            return;
        }

        foreach (var item in cart)
        {
            var product = products.First(p => p.ProductNumber == item.Key);

            var result = await _sellProduct.ExecuteAsync(
                new DecreaseProductStockRequest(product.Id, item.Value));

            if (result.IsFailure)
            {
                _ui.WriteLine($"Error: {result.Error.Message}");
                return;
            }
        }

        cart.Clear();
        _ui.WriteLine("Transaction completed.");
    }
}