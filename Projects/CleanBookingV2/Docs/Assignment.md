# Assignment Mapping

Purpose: map project files and implemented behavior back to the assignment requirements.

| Requirement | Implementation |
| --- | --- |
| Four Bed and Breakfast rooms | Seeded in `CleanBookingV2DbContext` |
| Room prices | Stored on `Room.PricePerNight` |
| Check room availability | `GetAvailableRooms` and booking use cases |
| Prevent overlapping bookings | `BookingAvailabilityService` |
| Calculate total cost | `CreateBooking` and `UpdateBooking` |
| Check-in rules | `Booking` domain entity |
| Check-out rules | `Booking` domain entity |
| Late arrival estimate | `Booking` domain entity |
| Two parking spaces | Seeded in `CleanBookingV2DbContext` |
| Parking availability | `BookingAvailabilityService` |
| Web API | `CleanBookingV2.Api` |
| SQLite database | `CleanBookingV2.Infrastructure` |
| Swagger/OpenAPI | API development startup |
| React frontend | `Frontend/Public` |
| Docker support | `docker-compose.yml` |
