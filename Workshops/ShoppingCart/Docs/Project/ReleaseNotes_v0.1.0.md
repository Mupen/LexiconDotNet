# Release Notes v0.1.0

This is a learning/workshop checkpoint, not a production release.

## Added

- Product, brand, category, and shopping cart API foundation.
- SQLite persistence with EF Core migrations.
- Swagger launch setup.
- Development seed data.
- Database files under `ShoppingCart.Api/Data`.
- Log files under `ShoppingCart.Api/Logs`.
- Product read model with brand/category names.
- Experimental user account foundation with roles:
  - Customer
  - Clerk
  - Admin
- User repository, queries, and use cases for future work.
- Development seed users with placeholder password hashes.
- Unit tests for domain, application, query, and persistence behavior.
- Dockerfile, Docker Compose, and Visual Studio Docker profile.

## Not Added Yet

- Frontend.
- User API endpoints.
- Real login.
- Password hashing service.
- Cookie/JWT authentication.
- Role-based authorization policies.
- Checkout/payment flow.
- Production deployment setup.

## Scope Note

The user/account code is not a completed feature in this version. It is included only as a foundation for later work and to support cart ownership by user id.
