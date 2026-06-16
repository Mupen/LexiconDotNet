# Features And Business Rules

Purpose: explain what the application does and which business rules it enforces.

This document explains what CleanBookingV2 does from a product and business-rule point of view.

## Core Features

### Rooms

The system starts with four seeded rooms:

- Room 1: single room, capacity 1, 550 DKK per night.
- Room 2: double room, capacity 2, 700 DKK per night.
- Room 3: double room, capacity 2, 765 DKK per night.
- Room 4: family room, capacity 3, 850 DKK per night.

Users can:

- list all rooms
- search rooms available for a stay
- inspect room capacity and price
- choose an available room for a booking

### Parking Spaces

The system starts with two seeded parking spaces.

Users can:

- list all parking spaces
- search available parking spaces for a stay
- attach one optional parking space to a booking

Parking is optional. A booking can be created without parking.

### Bookings

Users can:

- list bookings
- create a booking
- update a booking
- cancel a booking
- hide cancelled bookings in the frontend

Cancellation is a soft delete. The booking stays in the database with status `Cancelled`, but it no longer blocks room or parking availability.

## Booking Rules

### Date And Time Rules

The backend enforces these rules:

- Check-in must be after or equal to `14:00`.
- Check-in must be before or equal to `22:30`.
- Check-out must be no later than `12:00`.
- Check-out must be after check-in.
- Arrivals after `20:00` require `estimatedArrivalTime`.

The time policy is defined in `Domain\CleanBookingV2.Domain\Policies\BookingPolicy.cs`.

These rules are enforced in `Domain\CleanBookingV2.Domain\Entities\Booking.cs` and `Domain\CleanBookingV2.Domain\ValueObjects\DateRange.cs`.

### Capacity Rules

The selected room must support the requested number of guests.

Example:

- Room 1 has capacity 1.
- A booking for 2 guests in Room 1 must fail.

This is checked in `BookingPreparationService`.

### Room Availability Rules

Only active bookings block availability.

Two active bookings cannot use the same room if their stays overlap.

Back-to-back stays are allowed when the previous stay ends before the next stay starts.

Example:

- Booking A: June 12, 14:00 to June 14, 12:00.
- Booking B: June 14, 14:00 to June 16, 12:00.
- These do not overlap.

### Parking Availability Rules

Parking follows the same overlap rule as rooms.

Only one active overlapping booking can use the same parking space.

### Price Rules

The frontend does not calculate the final booking price.

The backend calculates:

```text
room price per night * number of nights
```

`DateRange.GetNumberOfNights()` calculates nights from the date difference.

## Frontend Features

The React app has three main views:

- Home: entry page and room image.
- Stays: availability search and room browsing.
- Booking: create, edit, cancel, and view activity.

The frontend stores these preferences in local storage:

- density: comfortable or compact
- hide cancelled bookings: true or false

It does not store authoritative booking data in local storage.

The frontend loads `GET /api/booking-policy` and uses the backend policy to render time dropdowns, guest limits, and hints. These values are still working-state helpers; the backend remains authoritative when saving.

## API Features

Main endpoints:

- `GET /api/rooms`
- `GET /api/booking-policy`
- `GET /api/rooms/available`
- `GET /api/parking-spaces`
- `GET /api/parking-spaces/available`
- `GET /api/bookings`
- `GET /api/bookings/{id}`
- `POST /api/bookings`
- `PUT /api/bookings/{id}`
- `DELETE /api/bookings/{id}`

See `ApiReference.md` for request examples.

## Expected Failure Cases

The API should reject:

- missing guest name
- missing check-in or check-out
- check-out before check-in
- check-in before `14:00`
- check-in after `22:30`
- check-out after `12:00`
- late arrival after `20:00` without estimated arrival time
- guest count higher than room capacity
- unknown room id
- inactive room
- unknown parking id
- inactive parking space
- overlapping room booking
- overlapping parking assignment

These failures should appear as problem details responses with useful `title` and `detail` values.
