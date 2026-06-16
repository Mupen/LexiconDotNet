# Manual Test Checklist

Purpose: provide a focused checklist for manually verifying behavior after changes.

Use this checklist after backend or frontend changes that affect booking behavior.

## Setup

1. Start the app from `Projects/CleanBookingV2`:

   ```powershell
   .\Start.ps1
   ```

2. Open Swagger:

   ```text
   http://localhost:5217/swagger
   ```

3. Open the frontend:

   ```text
   http://localhost:5173
   ```

## API Checks

- `GET /api/rooms` returns four seeded rooms.
- `GET /api/booking-policy` returns check-in, checkout, late-arrival, slot, and max-guest policy values.
- `GET /api/parking-spaces` returns two seeded parking spaces.
- `GET /api/rooms/available` returns only active rooms with enough capacity and no active overlapping booking.
- `GET /api/parking-spaces/available` returns only parking spaces with no active overlapping booking.
- `POST /api/bookings` creates a booking with valid dates, room, and guest count.
- `POST /api/bookings` accepts `parkingSpaceId: null`.
- `POST /api/bookings` rejects overlapping active room bookings.
- `POST /api/bookings` rejects overlapping active parking assignments.
- `POST /api/bookings` rejects room capacity overflow.
- `POST /api/bookings` rejects check-in before `14:00`.
- `POST /api/bookings` rejects check-in after `22:30`.
- `POST /api/bookings` rejects check-out after `12:00`.
- `POST /api/bookings` rejects check-out before check-in.
- `POST /api/bookings` rejects late arrival after `20:00` without `estimatedArrivalTime`.
- `PUT /api/bookings/{id}` updates guest, stay, room, parking, and recalculated price.
- `PUT /api/bookings/{id}` ignores the booking's own current room and parking when checking availability.
- `DELETE /api/bookings/{id}` marks a booking as `Cancelled`.
- Cancelled bookings no longer block room availability.
- Cancelled bookings no longer block parking availability.

## Frontend Checks

- App loads at `http://localhost:5173`.
- Home, Stays, and Booking navigation works.
- Refresh button reloads bookings, rooms, and parking spaces.
- Stays search displays available rooms for selected dates and guest count.
- Booking form requires availability search before creating a new booking.
- Creating a booking refreshes the booking list.
- Selecting a booking fills the edit form.
- Updating a booking refreshes the booking list.
- Cancelling a booking refreshes the booking list.
- Hide cancelled preference filters cancelled bookings.
- Density preference switches between comfortable and compact layouts.
- Backend validation errors are shown in the status message.

## Build Checks

Full verification:

```powershell
cd Projects\CleanBookingV2
.\Verify.ps1
```

Backend:

```powershell
dotnet restore Projects\CleanBookingV2\UnitTests\CleanBookingV2.UnitTests\CleanBookingV2.UnitTests.csproj --verbosity normal
dotnet build Projects\CleanBookingV2\UnitTests\CleanBookingV2.UnitTests\CleanBookingV2.UnitTests.csproj --no-restore --verbosity normal
dotnet test Projects\CleanBookingV2\UnitTests\CleanBookingV2.UnitTests\CleanBookingV2.UnitTests.csproj --no-restore --no-build --verbosity normal
```

Frontend:

```powershell
cd Projects\CleanBookingV2\Frontend\Public
npm run build
```

Expected current automated results:

- Backend: 24 tests passed, 0 failed.
- Frontend: Vite build passed from `Projects\CleanBookingV2\Frontend\Public`.

## Docker Checks

From `Projects/CleanBookingV2`:

```powershell
docker compose up --build
```

Verify:

- API responds on `http://localhost:5217`.
- Frontend responds on `http://localhost:5173`.
- Frontend can create a booking through the API.
- Database survives container restart because of the `cleanbooking_data` volume.
