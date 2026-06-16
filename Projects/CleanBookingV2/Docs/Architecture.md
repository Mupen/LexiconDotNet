# Architecture

Purpose: explain the code structure, layer responsibilities, and request flow.

CleanBookingV2 uses a layered backend with a React frontend. The backend is the source of truth for bookings, prices, availability, and business rules.

The frontend holds working state. The backend owns authoritative state.

## Backend Layers

### Domain

Path: `Domain\CleanBookingV2.Domain`

The domain layer contains business objects and rules that should not depend on HTTP, EF Core, React, or SQLite.

Important files:

- `Entities\Booking.cs`
- `Entities\Room.cs`
- `Entities\ParkingSpace.cs`
- `ValueObjects\DateRange.cs`
- `Policies\BookingPolicy.cs`
- `Enums\BookingStatus.cs`

Examples of rules enforced here:

- Booking id must not be empty.
- Guest name is required.
- Guest count must be greater than zero.
- Check-in must be between `14:00` and `22:30`.
- Check-out must be no later than `12:00`.
- Late arrivals after `20:00` require an estimated arrival time.
- Cancelled bookings cannot be updated.
- Cancelled bookings do not block availability.

### Application

Path: `Application\CleanBookingV2.Application`

The application layer coordinates use cases. It knows what workflow must happen, but it depends on interfaces instead of EF Core directly.

Important files:

- `UseCases\Bookings\CreateBooking.cs`
- `UseCases\Bookings\UpdateBooking.cs`
- `UseCases\Bookings\CancelBooking.cs`
- `Services\BookingPreparationService.cs`
- `Services\BookingAvailabilityService.cs`
- `Queries\Rooms\GetAvailableRooms.cs`
- `Queries\Parking\GetAvailableParkingSpaces.cs`

Create and update follow the same practical flow:

1. Load the selected room.
2. Validate that the room exists, is active, and has enough capacity.
3. Create a valid `DateRange`.
4. Check room availability.
5. Check parking availability when parking is selected.
6. Calculate total price from room price and number of nights.
7. Create or update the `Booking`.
8. Save changes.

### Infrastructure

Path: `Infrastructure\CleanBookingV2.Infrastructure`

Infrastructure implements application interfaces with EF Core and SQLite.

Important files:

- `Persistence\CleanBookingV2DbContext.cs`
- `Persistence\EfBookingTransaction.cs`
- `Repositories\EfBookingRepository.cs`
- `ReadRepositories\EfBookingReadRepository.cs`
- `ReadRepositories\EfRoomAvailabilityQuery.cs`
- `ReadRepositories\EfParkingSpaceAvailabilityQuery.cs`

The project separates write repositories from read queries:

- Write repositories return domain entities when the use case needs to change state.
- Read repositories project directly into read models for API responses.

This keeps booking mutations close to the domain model while still allowing efficient list and availability queries.

### API

Path: `Api\CleanBookingV2.Api`

The API layer maps HTTP requests to application use cases and maps application results back to HTTP responses.

Important files:

- `Controllers\BookingsController.cs`
- `Controllers\RoomsController.cs`
- `Controllers\ParkingSpacesController.cs`
- `Mapping\ProblemDetailsMapping.cs`
- `Mapping\ResponseMapping.cs`
- `Program.cs`

The API returns normal HTTP semantics:

- `200 OK` for successful reads.
- `201 Created` for created bookings.
- `204 No Content` for successful update/cancel operations.
- `400 Bad Request` for validation and business-rule failures.
- `404 Not Found` for missing bookings.
- `409 Conflict` for concurrency conflicts.

## Frontend

Path: `Frontend\Public`

The frontend is a React/Vite app. It displays data, captures user intent, and calls the API. It does not own booking truth.

Important files:

- `src\App.jsx`
- `src\hooks\useDashboardData.js`
- `src\api\apiClient.js`
- `src\api\bookingsApi.js`
- `src\components\BookingForm.jsx`
- `src\components\BookingList.jsx`
- `src\components\RoomPanel.jsx`

Frontend state is used for:

- current page
- booking form values
- selected room or booking
- latest availability search result
- UI status messages
- display preferences

Browser local storage is used only by `src\storage\preferencesStorage.js` for display preferences. It is not used for bookings, prices, identity, or permissions.

The frontend also loads `GET /api/booking-policy` and uses that response to render booking time options, guest limits, and validation hints. This keeps the web UI aligned with backend-owned rules while still treating the frontend values as working state.

## Data Flow Example

Creating a booking:

1. User searches availability in React.
2. React calls `GET /api/rooms/available` and `GET /api/parking-spaces/available`.
3. User chooses room and optional parking.
4. React sends `POST /api/bookings`.
5. `BookingsController` calls `CreateBooking`.
6. `CreateBooking` uses `BookingPreparationService`.
7. Availability is re-checked on the backend.
8. Domain `Booking` validates invariant rules.
9. EF Core saves the booking in SQLite.
10. API returns the created booking response.
11. React refreshes dashboard data.

The important design choice is that availability shown in the frontend is only a snapshot. The backend re-checks availability when saving because another booking could have been created after the user searched.

## State Ownership

Backend authoritative state:

- bookings
- room and parking availability
- booking price
- room capacity
- booking policy rules
- validation results

Frontend working state:

- form inputs
- selected room or booking
- latest availability search results
- loading and status messages
- display preferences

The frontend can guide the user with policy data and availability snapshots, but create and update requests still go back through the backend so authoritative rules are applied at save time.

## Good Design Choices

- Business rules are primarily backend/domain rules.
- Use cases are small and named by action.
- API contracts are separate from domain entities.
- EF Core is hidden behind application interfaces.
- Read models avoid returning EF entities directly from controllers.
- Cancel is a soft delete, so history remains available.

## Known Tradeoffs

- SQLite is good for local/demo use, but it is not ideal for high-concurrency booking systems.
- Migrations run at API startup, which is convenient locally but not preferred for production deployments.
- Overlap protection is implemented in application queries and transactions; a production database should also enforce this with stronger locking or constraints where possible.
- The frontend duplicates some time-window knowledge for user experience. The backend remains authoritative, but duplicated UI rules must be kept in sync.
