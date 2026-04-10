using SalesSystem.Domain.Contracts;

namespace SalesSystem.Domain.Entities;

public sealed class Product
{
    // Identity and core properties
    public Guid Id { get; }
    public int ProductNumber { get; private set; }
    public string Name { get; private set; }
    public decimal NetPrice { get; private set; }
    public decimal GrossPrice => decimal.Round(NetPrice * (1m + VatRate), 2, MidpointRounding.AwayFromZero);
    public decimal VatRate { get; private set; }
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; }

    private Product(Guid id, int productNumber, string name, decimal netPrice, decimal vatRate, int stockQuantity, bool isActive)
    {
        Id = id;
        ProductNumber = productNumber;
        Name = name;
        NetPrice = decimal.Round(netPrice, 2, MidpointRounding.AwayFromZero);
        VatRate = vatRate;
        StockQuantity = stockQuantity;
        IsActive = isActive;
    }

    // Public operations
    public static Result<Product> Create(int productNumber, string name, decimal netPrice, decimal vatRate, int stockQuantity, bool isActive = true)
    {
        Result result;

        result = ValidateProductNumber(productNumber);
        if (result.IsFailure)
            return Result<Product>.Failure(result.Error);

        result = ValidateName(name);
        if (result.IsFailure)
            return Result<Product>.Failure(result.Error);

        result = ValidateNetPrice(netPrice);
        if (result.IsFailure)
            return Result<Product>.Failure(result.Error);

        result = ValidateVatRate(vatRate);
        if (result.IsFailure)
            return Result<Product>.Failure(result.Error);

        result = ValidateStockQuantity(stockQuantity);
        if (result.IsFailure)
            return Result<Product>.Failure(result.Error);

        var product = new Product(
            Guid.NewGuid(),
            productNumber,
            name,
            netPrice,
            vatRate,
            stockQuantity,
            isActive);

        return Result<Product>.Success(product);
    }

    public Result Update(int productNumber, string name, decimal netPrice, decimal vatRate)
    {
        Result result;

        result = ValidateProductNumberChange(productNumber);
        if (result.IsFailure)
            return result;

        result = ValidateNameChange(name);
        if (result.IsFailure)
            return result;

        var newNetPrice = decimal.Round(netPrice, 2, MidpointRounding.AwayFromZero);
        result = ValidateNetPriceChange(newNetPrice);
        if (result.IsFailure)
            return result;

        result = ValidateVatRateChange(vatRate);
        if (result.IsFailure)
            return result;

        if (productNumber != ProductNumber)
            ApplyProductNumberChange(productNumber);

        if (name != Name)
            ApplyNameChange(name);

        if (newNetPrice != NetPrice)
            ApplyNetPriceChange(newNetPrice);

        if (vatRate != VatRate)
            ApplyVatRateChange(vatRate);

        return Result.Success();
    }

    public Result IncreaseStock(int quantity)
    {
        var result = ValidateStockIncreaseQuantity(quantity);
        if (result.IsFailure)
            return result;

        ApplyStockIncrease(quantity);
        return Result.Success();
    }

    public Result DecreaseStock(int quantity)
    {
        var result = ValidateStockDecreaseQuantity(quantity);
        if (result.IsFailure)
            return result;

        ApplyStockDecrease(quantity);
        return Result.Success();
    }

    public Result Activate()
    {
        var result = ValidateActivate();
        if (result.IsFailure)
            return result;

        ApplyActivate();
        return Result.Success();
    }

    public Result Deactivate()
    {
        var result = ValidateDeactivate();
        if (result.IsFailure)
            return result;

        ApplyDeactivate();
        return Result.Success();
    }

    // Product number operations
    private Result ValidateProductNumberChange(int productNumber)
    {
        if (productNumber == ProductNumber)
            return Result.Success();

        return ValidateProductNumber(productNumber);
    }
    private static Result ValidateProductNumber(int productNumber)
    {
        if (productNumber <= 0)
        {
            return Result.Failure(
                new Error("Product.InvalidProductNumber", "Product number must be greater than zero."));
        }

        return Result.Success();
    }
    private void ApplyProductNumberChange(int productNumber)
    {
        ProductNumber = productNumber;
    }

    // Name operations
    private Result ValidateNameChange(string name)
    {
        if (name == Name)
            return Result.Success();

        return ValidateName(name);
    }
    private static Result ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(
                new Error("Product.InvalidName", "Product name is required."));
        }

        return Result.Success();
    }
    private void ApplyNameChange(string name)
    {
        Name = name;
    }

    // Net price operations
    private Result ValidateNetPriceChange(decimal netPrice)
    {
        if (netPrice == NetPrice)
            return Result.Success();

        return ValidateNetPrice(netPrice);
    }
    private static Result ValidateNetPrice(decimal netPrice)
    {
        if (netPrice < 0)
        {
            return Result.Failure(
                new Error("Product.InvalidNetPrice", "Net price cannot be negative."));
        }

        return Result.Success();
    }
    private void ApplyNetPriceChange(decimal netPrice)
    {
        NetPrice = decimal.Round(netPrice, 2, MidpointRounding.AwayFromZero);
    }

    // VAT rate operations
    private Result ValidateVatRateChange(decimal vatRate)
    {
        if (vatRate == VatRate)
            return Result.Success();

        return ValidateVatRate(vatRate);
    }
    private static Result ValidateVatRate(decimal vatRate)
    {
        if (vatRate < 0 || vatRate > 1)
        {
            return Result.Failure(
                new Error("Product.InvalidVatRate", "VAT rate must be between 0 and 1."));
        }

        return Result.Success();
    }
    private void ApplyVatRateChange(decimal vatRate)
    {
        VatRate = vatRate;
    }

    // Stock quantity operations
    private Result ValidateStockIncreaseQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            return Result.Failure(
                new Error("Product.InvalidStockIncreaseQuantity", "Stock increase quantity must be greater than zero."));
        }

        return Result.Success();
    }

    private Result ValidateStockDecreaseQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            return Result.Failure(
                new Error("Product.InvalidStockDecreaseQuantity", "Stock decrease quantity must be greater than zero."));
        }

        if (quantity > StockQuantity)
        {
            return Result.Failure(
                new Error("Product.InsufficientStock", "Not enough stock available."));
        }

        return Result.Success();
    }

    private static Result ValidateStockQuantity(int stockQuantity)
    {
        if (stockQuantity < 0)
        {
            return Result.Failure(
                new Error("Product.InvalidStockQuantity", "Stock quantity cannot be negative."));
        }

        return Result.Success();
    }

    private void ApplyStockIncrease(int quantity)
    {
        StockQuantity += quantity;
    }

    private void ApplyStockDecrease(int quantity)
    {
        StockQuantity -= quantity;
    }

    // Activation operations
    private Result ValidateActivate()
    {
        if (IsActive)
        {
            return Result.Failure(
                new Error("Product.AlreadyActive", "Product is already active."));
        }

        return Result.Success();
    }

    private void ApplyActivate()
    {
        IsActive = true;
    }

    // Deactivation operations
    private Result ValidateDeactivate()
    {
        if (!IsActive)
        {
            return Result.Failure(
                new Error("Product.AlreadyInactive", "Product is already inactive."));
        }

        return Result.Success();
    }

    private void ApplyDeactivate()
    {
        IsActive = false;
    }
}