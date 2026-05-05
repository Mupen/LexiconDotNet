# Domain Model

Version: v0.1.0

The Domain layer contains the business concepts and rules for the shopping cart system. It does not depend on ASP.NET Core, EF Core, SQLite, Swagger, Docker, or test frameworks.

## Main Entities

| Entity | Responsibility |
| --- | --- |
| `Product` | Represents a sellable catalog item and owns product-level rules. |
| `ProductBrand` | Represents a product brand. |
| `ProductCategory` | Represents a root or child product category. |
| `ShoppingCart` | Represents a cart owned by one user id and calculates cart totals. |
| `ShoppingCartItem` | Represents one product line inside a cart. |
| `User` | Placeholder/foundation account entity for future authentication work. |

## Product

A product contains:

- `Id`
- `Sku`
- `Name`
- `Description`
- `BrandId`
- `CategoryId`
- `UnitPrice`
- `TrackInventory`
- `StockQuantity`
- `Status`

Product behavior includes:

- creating a product with validated values
- updating SKU, name, and description
- changing brand
- changing category
- changing price
- changing status
- increasing stock
- decreasing stock
- checking whether the product is available
- checking whether requested quantity can be supplied

Product status is represented by `ProductStatus`.

Current statuses:

- `Draft`
- `Active`
- `Archived`
- `Discontinued`

Availability is calculated from the product status and inventory state. A product must be active and available before it can be added to a cart.

## Product Brand

A product brand contains:

- `Id`
- `Name`

Brand behavior includes:

- creating a brand
- renaming a brand

The application layer checks persistence-related rules such as duplicates before saving.

## Product Category

A product category contains:

- `Id`
- `Name`
- `ParentCategoryId`

Category behavior includes:

- creating a root category
- creating a child category
- renaming a category
- moving a category under another parent
- moving a category back to root by clearing the parent id

The `IsRootCategory` response value is derived from whether `ParentCategoryId` is `null`.

## Shopping Cart

A shopping cart contains:

- `Id`
- `UserId`
- `Items`
- `TotalQuantity`
- `TotalPrice`

Cart behavior includes:

- setting product item quantity
- adding a new item when the product is not already in the cart
- updating quantity when the product already exists in the cart
- removing an item when quantity is set to `0`
- calculating total quantity
- calculating total price

The cart stores `UserId` as required by the assignment. It does not validate login state because authentication is outside the current version scope.

## Shopping Cart Item

A shopping cart item contains:

- `Id`
- `ProductId`
- `ProductName`
- `UnitPrice`
- `Quantity`
- `LineTotal`

The item stores product name and unit price at the time it is added to the cart. This makes the cart response stable and lets the cart calculate totals without exposing the full product entity.

## User Foundation

The project includes a `User` entity and `UserRole` enum as foundation work.

A user contains:

- `Id`
- `Email`
- `DisplayName`
- `PasswordHash`
- `Role`
- `IsActive`
- `CreatedAtUtc`

Current roles:

- `Customer`
- `Clerk`
- `Admin`

This is not a completed authentication system. The current version does not include login, password hashing, cookie or JWT authentication, or role-based controller authorization.

There are no user API controllers in v0.1.0. The user foundation exists in domain, application, infrastructure, seed data, and tests only.

## Result Contracts

Expected business failures use `Result` and `Result<T>`.

Examples:

- duplicate SKU
- duplicate brand name
- missing product
- missing category
- missing cart
- invalid quantity
- insufficient stock

This keeps normal business failures explicit without using exceptions for expected validation paths.
