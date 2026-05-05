# Assignment Mapping

Version: v0.1.0

## Original Workshop Requirement

The workshop describes a small online store that sells products and needs a shopping cart backend.

Required parts:

- a product class with the properties and behavior needed by the store
- a shopping cart class
- the cart tracks different product items
- each cart item has a quantity
- the cart has a user id
- endpoints for reading, creating, editing, and deleting a shopping cart

The workshop also says that quantity increase/decrease can be handled by a future frontend. This project therefore exposes one practical cart item endpoint where the frontend can set the desired quantity.

## Product Requirement

Implemented through the `Product` domain entity.

Product data includes:

- SKU
- name
- description
- brand id
- category id
- unit price
- inventory tracking flag
- stock quantity
- status

Product behavior includes:

- validation during creation
- detail updates
- brand/category changes
- price changes
- status changes
- stock increase/decrease
- availability checks
- supply checks before adding to a cart

## Shopping Cart Requirement

Implemented through the `ShoppingCart` and `ShoppingCartItem` domain entities.

A shopping cart contains:

- cart id
- user id
- cart items
- total quantity
- total price

Each cart item contains:

- product id
- product name
- unit price
- quantity
- line total

The cart supports setting an item quantity. Quantity `0` removes the item from the cart.

## Required Cart Endpoints

The required cart API is implemented through:

- `GET /api/shopping-carts`
- `GET /api/shopping-carts/{id}`
- `GET /api/shopping-carts/user/{userId}`
- `POST /api/shopping-carts`
- `PUT /api/shopping-carts/{id}/items`
- `DELETE /api/shopping-carts/{id}`

These cover reading, creating, editing, and deleting shopping carts.

## Extra Scope

The project also includes:

- product API endpoints
- product brands
- product categories
- SQLite persistence
- Swagger
- Docker
- seed data
- unit tests

These support a more realistic API but are not extra requirements from the workshop PDF.

## User Scope

The workshop says a `User` class is optional.

This project includes `User` and `UserRole` as placeholder/foundation code only. The domain, application, repository, persistence, tests, and seed data exist for future account work.

There are no user API endpoints, no real login, no frontend account flow, no password hashing service, and no role-based controller authorization in this version.

For this assignment, the important requirement is that shopping carts store a `UserId`.
