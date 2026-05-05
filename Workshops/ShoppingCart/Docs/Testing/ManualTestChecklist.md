# Manual Test Checklist

Version: v0.1.0

Use this checklist to verify the API manually through Swagger before hand-in.

Swagger URL:

```text
http://localhost:5088/swagger
```

## Startup

- [ ] API starts without errors.
- [ ] Swagger opens.
- [ ] Seed data is available.
- [ ] API responses are JSON.
- [ ] Enum values appear as strings, for example `Active`, `Draft`, `Archived`, or `Discontinued`.

## Products

- [ ] `GET /api/products` returns seeded products.
- [ ] `GET /api/products/available` returns only available products.
- [ ] `GET /api/products/{id}` returns one existing product.
- [ ] `GET /api/products/{missingId}` returns 404.
- [ ] Product response includes brand and category names.
- [ ] Tracked products have stock quantity.
- [ ] Untracked products have `stockQuantity: null`.
- [ ] `POST /api/products` creates a product when brand and category ids are valid.
- [ ] Creating a product with a duplicate SKU returns 409.
- [ ] `PUT /api/products/{id}` updates SKU, name, and description.
- [ ] `PATCH /api/products/{id}/price` updates price.
- [ ] `PATCH /api/products/{id}/status` updates product status.
- [ ] `PATCH /api/products/{id}/stock/increase` increases tracked stock.
- [ ] `PATCH /api/products/{id}/stock/decrease` decreases tracked stock.

## Shopping Carts

- [ ] `GET /api/shopping-carts` returns carts.
- [ ] `POST /api/shopping-carts` creates a cart for a user id.
- [ ] `GET /api/shopping-carts/{id}` returns the created cart.
- [ ] `PUT /api/shopping-carts/{id}/items` adds a product to the cart.
- [ ] Fetching the cart shows correct item quantity.
- [ ] Fetching the cart shows correct line total.
- [ ] Fetching the cart shows correct total quantity.
- [ ] Fetching the cart shows correct total price.
- [ ] Updating the same product changes the quantity.
- [ ] Setting quantity to zero removes the item.
- [ ] Adding more than available stock returns an error.
- [ ] `DELETE /api/shopping-carts/{id}` deletes the cart.
- [ ] Fetching the deleted cart returns 404.

## Brands And Categories

- [ ] Product brands can be listed.
- [ ] Product brands can be created.
- [ ] Product brands can be renamed.
- [ ] Product categories can be listed.
- [ ] Product categories can be created as root categories.
- [ ] Product categories can be created as child categories.
- [ ] Product categories can be renamed.
- [ ] Product categories can be moved under a different parent.
- [ ] Products reference existing brand ids and category ids.
- [ ] Products include display names for brand and category.

## Error Responses

- [ ] Missing resource returns 404.
- [ ] Invalid request returns 400.
- [ ] Duplicate or conflicting data returns 409 where applicable.
- [ ] Error responses use ProblemDetails JSON.

## Persistence

- [ ] Created data remains after API restart.
- [ ] Resetting the SQLite database recreates schema and seed data.
