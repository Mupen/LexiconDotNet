using SalesSystem.Api.Helpers;
using SalesSystem.Api.Interfaces;
using SalesSystem.Api.UI;
using SalesSystem.Application.Queries.Products;
using SalesSystem.Application.Requests.Products;
using SalesSystem.Application.UseCases.Products;
using SalesSystem.Domain.Entities;

namespace SalesSystem.Api.Menus;

public class ManageProductsMenu : MenuBase
{
    private readonly GetAllProducts _getAllProducts;
    private readonly GetProductByNumber _getProductByNumber;
    private readonly CreateProduct _createProduct;
    private readonly UpdateProduct _updateProduct;
    private readonly DeleteProduct _deleteProduct;
    private readonly IncreaseProductStock _increaseProductStock;
    private readonly DecreaseProductStock _decreaseProductStock;    
    private readonly ChangeProductStatus _changeProductStatus;

    public ManageProductsMenu(
        IUserIO ui,
        GetAllProducts getAllProducts,
        GetProductByNumber getProductByNumber,
        CreateProduct createProduct,
        UpdateProduct updateProduct,
        DeleteProduct deleteProduct,
        IncreaseProductStock increaseProductStock,
        DecreaseProductStock decreaseProductStock,
        ChangeProductStatus changeProductStatus) : base(ui)
    {
        _getAllProducts = getAllProducts;
        _getProductByNumber = getProductByNumber;
        _createProduct = createProduct;
        _updateProduct = updateProduct;
        _deleteProduct = deleteProduct;
        _increaseProductStock = increaseProductStock;
        _decreaseProductStock = decreaseProductStock;
        _changeProductStatus = changeProductStatus;
    }

    public override async Task RunAsync()
    {
        while (true)
        {
            var input = ShowMenu();
            var action = ParseMenuAction(input);

            switch (action)
            {
                case MenuAction.ShowAllProducts:
                    await ShowAllProductsAsync();
                    break;

                case MenuAction.CreateProduct:
                    await CreateProductAsync();
                    break;

                case MenuAction.UpdateProduct:
                    await UpdateProductAsync();
                    break;

                case MenuAction.RestockProduct:
                    await RestockProductAsync();
                    break;

                case MenuAction.ChangeProductStatus:
                    await ChangeProductStatusAsync();
                    break;

                case MenuAction.DeleteProduct:
                    await DeleteProductAsync();
                    break;

                case MenuAction.ExitMenu:
                    return;

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
        ShowAllProducts,
        CreateProduct,
        UpdateProduct,
        RestockProduct,
        ChangeProductStatus,
        DeleteProduct,
        ExitMenu,
        InvalidInput
    }

    private static MenuAction ParseMenuAction(string? input)
    {
        return UserInput.Normalize(input) switch
        {
            "1" => MenuAction.ShowAllProducts,
            "2" => MenuAction.CreateProduct,
            "3" => MenuAction.UpdateProduct,
            "4" => MenuAction.RestockProduct,
            "5" => MenuAction.ChangeProductStatus,
            "6" => MenuAction.DeleteProduct,
            "X" => MenuAction.ExitMenu,
            _ => MenuAction.InvalidInput
        };
    }

    private string ShowMenu()
    {
        ShowHeader("Manage Products",
            "1. Show all products",
            "2. Create product",
            "3. Update product",
            "4. Restock product",
            "5. Change product status",
            "6. Delete product",
            "X. Exit Menu");

        return ShowPrompt(prompt: "Select Option: ");
    }

    private async Task ShowAllProductsAsync()
    {
        ShowHeader("Product List");

        var products = (await _getAllProducts.ExecuteAsync())
        .OrderBy(p => p.ProductNumber)
        .ToList();

        if (products.Count == 0)
        {
            ShowMessage("No products found.");
        }
        else
        {
            foreach (var product in products)
            {
                var status = product.IsActive ? "Active" : "Inactive";

                _ui.WriteLine(
                    $"{product.ProductNumber} | {product.Name} | {product.GrossPrice} SEK | Stock: {product.StockQuantity} | {status}");
            }
        }
        ShowPause();
    }

    private async Task CreateProductAsync()
    {
        ShowHeader("Create Product");

        int productNumber = await ReadNewProductNumberAsync("Product number: ");
        string name = ReadRequiredText("Name: ");
        decimal price = ReadDecimal("Price: ");
        decimal vatRate = ReadVatRate("Vat: ");
        int stockQuantity = ReadInt("Stock quantity: ");
        bool isActive = ReadYesNo("Active (Y/N): ");

        var request = new CreateProductRequest(
            productNumber,
            name,
            price,
            vatRate,
            stockQuantity,
            isActive);

        var result = await _createProduct.ExecuteAsync(request);

        ShowMessage(
            result.IsSuccess
                ? "Product created successfully."
                : result.Error.Message);

        ShowPause();
    }

    private async Task UpdateProductAsync()
    {
        ShowHeader("Update Product");

        int existingProductNumber = ReadInt("Product number: ");
        var product = await FindProductByNumberAsync(existingProductNumber);

        if (product is null)
        {
            ShowMessage("Product not found.");
            ShowPause();
            return;
        }

        int newProductNumber = product.ProductNumber;
        string newName = product.Name;
        decimal newNetPrice = product.NetPrice;
        decimal newVatRate = product.VatRate;

        bool changeProductNumber = ReadYesNo($"Current product number: {product.ProductNumber}. Change? (Y/N): ");
        if (changeProductNumber)
        {
            newProductNumber = await ReadNewProductNumberAsync("New product number: ");
        }

        bool changeName = ReadYesNo($"Current name: {product.Name}. Change? (Y/N): ");
        if (changeName)
        {
            newName = ReadRequiredText("New name: ");
        }

        bool changeNetPrice = ReadYesNo(
            $"Current Net Price: {product.NetPrice}." +
            $"Current Gross Price: {product.GrossPrice}." +
            $"Current Vat Rate: {product.VatRate}." +
            $"Change Net Price? (Y/N): ");
        if (changeNetPrice)
        {
            newNetPrice = ReadDecimal("New Net Price: ");
        }

        bool changeVatRate = ReadYesNo(
            $"Current Net Price: {product.NetPrice}." +
            $"Current Gross Price: {product.GrossPrice}." +
            $"Current Vat Rate: {product.VatRate}." +
            $"Change Vat Rate? (Y/N): ");
        if (changeVatRate)
        {
            newVatRate = ReadDecimal("New Vat Rate: ");
        }


        var request = new UpdateProductRequest(
            product.Id,
            newProductNumber,
            newName,
            newNetPrice,
            newVatRate);

        var result = await _updateProduct.ExecuteAsync(request);

        ShowMessage(
            result.IsSuccess
                ? "Product updated successfully."
                : result.Error.Message);

        ShowPause();
    }

    private async Task RestockProductAsync()
    {
        ShowHeader("Restock Product");

        int productNumber = ReadInt("Product number: ");
        var product = await FindProductByNumberAsync(productNumber);

        if (product is null)
        {
            ShowMessage("Product not found.");
            ShowPause();
            return;
        }

        int quantity = ReadInt("Quantity to add: ");

        var request = new IncreaseProductStockRequest(product.Id, quantity);
        var result = await _increaseProductStock.ExecuteAsync(request);

        ShowMessage(
            result.IsSuccess
                ? "Product restocked successfully."
                : result.Error.Message);
        ShowPause();
    }

    private async Task ChangeProductStatusAsync()
    {
        ShowHeader("Change Product Status");

        int productNumber = ReadInt("Product number: ");
        var product = await FindProductByNumberAsync(productNumber);

        if (product is null)
        {
            ShowMessage("Product not found.");
            ShowPause();
            return;
        }

        bool isActive = ReadYesNo("Set product as active? (Y/N): ");

        var request = new ChangeProductStatusRequest(product.Id, isActive);
        var result = await _changeProductStatus.ExecuteAsync(request);

        ShowMessage(
            result.IsSuccess
                ? "Product status updated successfully."
                : result.Error.Message);
        ShowPause();
    }

    private async Task DeleteProductAsync()
    {
        ShowHeader("Delete Product");

        int productNumber = ReadInt("Product number: ");
        var product = await FindProductByNumberAsync(productNumber);

        if (product is null)
        {
            ShowMessage("Product not found.");
            ShowPause();
            return;
        }

        bool confirmed = ReadYesNo($"Delete '{product.Name}'? (Y/N): ");

        if (!confirmed)
        {
            ShowMessage("Delete cancelled.");
            ShowPause();
            return;
        }

        var result = await _deleteProduct.ExecuteAsync(product.Id);

        ShowMessage(
            result.IsSuccess
                ? "Product deleted successfully."
                : result.Error.Message);
        ShowPause();
    }

    private async Task<Product?> FindProductByNumberAsync(int productNumber)
    {
        var result = await _getProductByNumber.ExecuteAsync(productNumber);
        return result.IsSuccess ? result.Value : null;
    }


    private async Task<int> ReadNewProductNumberAsync(string label)
    {
        while (true)
        {
            int productNumber = ReadInt(label);

            var existingProduct = await FindProductByNumberAsync(productNumber);
            if (existingProduct is null)
                return productNumber;

            ShowMessage("Product number already exists. Try another number.");
        }
    }
}