# API Reference

Purpose: document the HTTP endpoints, request examples, response shapes, and error style.

Base URL for local development:

```text
http://localhost:5217
```

Swagger:

```text
http://localhost:5217/swagger
```

## Response Style

Successful reads return JSON.

Create returns `201 Created`.

Update and cancel return `204 No Content`.

Business-rule failures return problem details:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Booking.RoomUnavailable",
  "status": 400,
  "detail": "The selected room is not available for the requested stay."
}
```

## Booking Policy

### Get Booking Policy

```http
GET /api/booking-policy
```

Returns backend-owned booking policy values used by the frontend to render working-state controls.

Example response:

```json
{
  "earliestCheckIn": "14:00",
  "latestCheckIn": "22:30",
  "latestCheckOut": "12:00",
  "lateArrivalThreshold": "20:00",
  "timeSlotMinutes": 30,
  "maximumGuests": 3
}
```

The frontend uses this for dropdowns, guest limits, and hints. The backend still validates every create/update request.

## Rooms

### List Rooms

```http
GET /api/rooms
```

Returns all seeded rooms, including inactive rooms if any exist.

### Search Available Rooms

```http
GET /api/rooms/available?checkIn=2026-06-12T14:00:00&checkOut=2026-06-14T12:00:00&guests=2
```

Rules:

- `guests` must be greater than zero.
- `checkIn` and `checkOut` are required.
- Rooms must be active.
- Rooms must have enough capacity.
- Rooms with active overlapping bookings are excluded.

## Parking Spaces

### List Parking Spaces

```http
GET /api/parking-spaces
```

Returns all seeded parking spaces.

### Search Available Parking Spaces

```http
GET /api/parking-spaces/available?checkIn=2026-06-12T14:00:00&checkOut=2026-06-14T12:00:00
```

Rules:

- `checkIn` and `checkOut` are required.
- Parking spaces must be active.
- Parking spaces with active overlapping bookings are excluded.

## Bookings

### List Bookings

```http
GET /api/bookings
```

Returns bookings with room and parking names projected into the response.

### Get Booking

```http
GET /api/bookings/{id}
```

Returns `404 Not Found` when the booking does not exist.

### Create Booking

```http
POST /api/bookings
Content-Type: application/json
```

```json
{
  "guestName": "Ada Lovelace",
  "checkIn": "2026-06-12T14:00:00",
  "checkOut": "2026-06-14T12:00:00",
  "numberOfGuests": 2,
  "roomId": 2,
  "parkingSpaceId": 1,
  "estimatedArrivalTime": null
}
```

`parkingSpaceId` can be `null`.

### Update Booking

```http
PUT /api/bookings/{id}
Content-Type: application/json
```

Uses the same body shape as create.

The backend recalculates price and re-checks room and parking availability.

### Cancel Booking

```http
DELETE /api/bookings/{id}
```

Cancellation is a soft delete. The booking remains stored with status `Cancelled`.

## Booking Response

```json
{
  "id": "1f4df784-f418-4a20-98e1-ea144e4da001",
  "guestName": "Ada Lovelace",
  "checkIn": "2026-06-12T14:00:00",
  "checkOut": "2026-06-14T12:00:00",
  "numberOfGuests": 2,
  "roomId": 2,
  "roomName": "Room 2",
  "parkingSpaceId": 1,
  "parkingSpaceName": "Parking Space 1",
  "totalPrice": 1400,
  "estimatedArrivalTime": null,
  "status": "Active"
}
```

Enums are serialized as strings.
