# API Reference

Version: v0.1.0

Base URL when running locally:

```text
http://localhost:5088
```

Swagger UI:

```text
http://localhost:5088/swagger
```

The API returns JSON. Enums are serialized as strings, for example `Draft`, `Active`, `Archived`, or `Discontinued`.

## Error Handling

Expected business and validation errors are returned as ProblemDetails JSON.

Typical status codes:

| Status | Meaning |
| --- | --- |
| `400 Bad Request` | Invalid request data or failed business rule. |
| `404 Not Found` | Requested product, brand, category, cart, or related resource was not found. |
| `409 Conflict` | Duplicate or conflicting data, such as duplicate SKU or duplicate name. |

ProblemDetails responses include an additional `code` extension with the application/domain error code.

## Products

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/api/products` | List all products. |
| `GET` | `/api/products/available` | List products that are currently available for purchase. |
| `GET` | `/api/products/{id}` | Get one product by id. |
| `POST` | `/api/products` | Create a product. |
| `PUT` | `/api/products/{id}` | Update SKU, name, and description. |
| `PATCH` | `/api/products/{id}/brand` | Change product brand. |
| `PATCH` | `/api/products/{id}/category` | Change product category. |
| `PATCH` | `/api/products/{id}/price` | Change product price. |
| `PATCH` | `/api/products/{id}/status` | Change product status. |
| `PATCH` | `/api/products/{id}/stock/increase` | Increase tracked inventory stock. |
| `PATCH` | `/api/products/{id}/stock/decrease` | Decrease tracked inventory stock. |

### Product Response

```json
{
  "id": "guid",
  "sku": "SKU-001",
  "name": "Product name",
  "description": "Product description",
  "brandId": "guid",
  "brandName": "Brand name",
  "categoryId": "guid",
  "categoryName": "Category name",
  "unitPrice": 199.00,
  "trackInventory": true,
  "stockQuantity": 10,
  "status": "Active",
  "isAvailable": true
}
```

### Create Product Request

```json
{
  "sku": "SKU-001",
  "name": "Product name",
  "description": "Product description",
  "brandId": "guid",
  "categoryId": "guid",
  "unitPrice": 199.00,
  "trackInventory": true,
  "stockQuantity": 10,
  "status": "Active"
}
```

Validation summary:

- `sku` is required and has a maximum length of 64 characters.
- `name` is required and has a maximum length of 200 characters.
- `description` has a maximum length of 1000 characters.
- `unitPrice` cannot be negative.
- `stockQuantity` cannot be negative when provided.
- `brandId` and `categoryId` must reference existing records.

### Product Mutation Requests

Update details:

```json
{
  "sku": "SKU-001",
  "name": "Updated product name",
  "description": "Updated description"
}
```

Change brand:

```json
{
  "brandId": "guid"
}
```

Change category:

```json
{
  "categoryId": "guid"
}
```

Change price:

```json
{
  "unitPrice": 249.00
}
```

Change status:

```json
{
  "status": "Archived"
}
```

Adjust stock:

```json
{
  "quantity": 5
}
```

## Product Brands

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/api/product-brands` | List all product brands. |
| `GET` | `/api/product-brands/{id}` | Get one product brand by id. |
| `POST` | `/api/product-brands` | Create a product brand. |
| `PUT` | `/api/product-brands/{id}` | Rename a product brand. |

Brand response:

```json
{
  "id": "guid",
  "name": "Brand name"
}
```

Create or rename request:

```json
{
  "name": "Brand name"
}
```

Validation summary:

- `name` is required.
- `name` has a maximum length of 100 characters.
- Duplicate names return a conflict response.

## Product Categories

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/api/product-categories` | List all product categories. |
| `GET` | `/api/product-categories/{id}` | Get one product category by id. |
| `POST` | `/api/product-categories` | Create a root or child category. |
| `PUT` | `/api/product-categories/{id}` | Rename a product category. |
| `PATCH` | `/api/product-categories/{id}/parent` | Move a category under another parent or make it a root category. |

Category response:

```json
{
  "id": "guid",
  "name": "Category name",
  "parentCategoryId": null,
  "isRootCategory": true
}
```

Create request:

```json
{
  "name": "Category name",
  "parentCategoryId": null
}
```

Move request:

```json
{
  "parentCategoryId": "guid"
}
```

Use `null` for `parentCategoryId` to make a category a root category.

Validation summary:

- `name` is required.
- `name` has a maximum length of 100 characters.
- Parent category ids must reference existing categories.
- Duplicate/conflicting category names return a conflict response where applicable.

## Shopping Carts

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/api/shopping-carts` | List all shopping carts. |
| `GET` | `/api/shopping-carts/{id}` | Get one shopping cart by id. |
| `GET` | `/api/shopping-carts/user/{userId}` | List carts for one user id. |
| `POST` | `/api/shopping-carts` | Create an empty shopping cart for a user id. |
| `PUT` | `/api/shopping-carts/{id}/items` | Add, update, or remove a cart item. |
| `DELETE` | `/api/shopping-carts/{id}` | Delete a shopping cart. |

### Shopping Cart Response

```json
{
  "id": "guid",
  "userId": "guid",
  "totalQuantity": 2,
  "totalPrice": 398.00,
  "items": [
    {
      "id": "guid",
      "productId": "guid",
      "productName": "Product name",
      "unitPrice": 199.00,
      "quantity": 2,
      "lineTotal": 398.00
    }
  ]
}
```

### Create Shopping Cart Request

```json
{
  "userId": "guid"
}
```

### Set Shopping Cart Item Request

```json
{
  "productId": "guid",
  "quantity": 2
}
```

Behavior:

- Quantity greater than `0` adds the product or updates its quantity.
- Quantity `0` removes the product from the cart.
- Product must exist.
- Product must be active and available.
- Quantity must not exceed available stock for tracked products.

## User Endpoints

There are no user API endpoints in v0.1.0.

The project contains user domain, application, repository, persistence, seed data, and tests as foundation work for future authentication and authorization. That code is not exposed through controllers in this version.
