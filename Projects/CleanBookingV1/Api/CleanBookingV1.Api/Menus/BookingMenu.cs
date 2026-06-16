using CleanBookingV1.Application.UseCases.Bookings;

namespace CleanBookingV1.Api.Menus;

public class BookingMenu(
    CreateBooking createBooking,
    GetAllBookings getAllBookings,
    GetBookingById getBookingById,
    UpdateBooking updateBooking,
    DeleteBooking deleteBooking)
{
    private readonly CreateBooking _createBooking = createBooking;
    private readonly GetAllBookings _getAllBookings = getAllBookings;
    private readonly GetBookingById _getBookingById = getBookingById;
    private readonly UpdateBooking _updateBooking = updateBooking;
    private readonly DeleteBooking _deleteBooking = deleteBooking;

    public void Run()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== BOOKING MENU ===");
            Console.WriteLine("1. List all bookings");
            Console.WriteLine("2. Get booking by id");
            Console.WriteLine("3. Create booking");
            Console.WriteLine("4. Delete booking");
            Console.WriteLine("0. Back");

            string? input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    ListAllBookings();
                    break;
                case "2":
                    GetBookingById();
                    break;
                case "3":
                    CreateBooking();
                    break;
                case "4":
                    DeleteBooking();
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Invalid choice.");
                    Pause();
                    break;
            }
        }
    }

    private void ListAllBookings()
    {
        var result = _getAllBookings.Execute();

        Console.Clear();
        Console.WriteLine("=== ALL BOOKINGS ===");

        if (result.IsFailure)
        {
            Console.WriteLine(result.Error.Message);
            Pause();
            return;
        }

        foreach (var booking in result.Value!)
        {
            Console.WriteLine(
                $"Id: {booking.Id} | Guest: {booking.GuestName} | Room: {booking.RoomId} | Guests: {booking.NumberOfGuests} | Price: {booking.TotalPrice}");
        }

        Pause();
    }

    private void GetBookingById()
    {
        Console.Clear();
        Console.Write("Enter booking id: ");

        if (!Guid.TryParse(Console.ReadLine(), out Guid bookingId))
        {
            Console.WriteLine("Invalid id.");
            Pause();
            return;
        }

        var request = new GetBookingByIdRequest(bookingId);
        var result = _getBookingById.Execute(request);

        if (result.IsFailure)
        {
            Console.WriteLine(result.Error.Message);
            Pause();
            return;
        }

        var booking = result.Value!;
        Console.WriteLine($"Id: {booking.Id}");
        Console.WriteLine($"Guest: {booking.GuestName}");
        Console.WriteLine($"Room: {booking.RoomId}");
        Console.WriteLine($"Guests: {booking.NumberOfGuests}");
        Console.WriteLine($"Price: {booking.TotalPrice}");
        Console.WriteLine($"Check-in: {booking.Stay.Start}");
        Console.WriteLine($"Check-out: {booking.Stay.End}");
        Console.WriteLine($"Parking: {booking.ParkingSpaceId}");

        Pause();
    }

    private void CreateBooking()
    {
        Console.Clear();

        Console.Write("Guest name: ");
        string guestName = Console.ReadLine() ?? string.Empty;

        Console.Write("Room id: ");
        if (!int.TryParse(Console.ReadLine(), out int roomId))
        {
            Console.WriteLine("Invalid room id.");
            Pause();
            return;
        }

        Console.Write("Number of guests: ");
        if (!int.TryParse(Console.ReadLine(), out int numberOfGuests))
        {
            Console.WriteLine("Invalid number.");
            Pause();
            return;
        }

        Console.Write("Check-in (yyyy-mm-dd): ");
        if (!DateTime.TryParse(Console.ReadLine(), out DateTime checkIn))
        {
            Console.WriteLine("Invalid date.");
            Pause();
            return;
        }

        Console.Write("Check-out (yyyy-mm-dd): ");
        if (!DateTime.TryParse(Console.ReadLine(), out DateTime checkOut))
        {
            Console.WriteLine("Invalid date.");
            Pause();
            return;
        }

        Console.Write("Parking required? (y/n): ");
        bool parkingRequested = Console.ReadLine()?.ToLower() == "y";

        var request = new CreateBookingRequest(
            GuestName: guestName,
            RoomId: roomId,
            NumberOfGuests: numberOfGuests,
            CheckIn: checkIn,
            CheckOut: checkOut,
            ParkingRequested: parkingRequested,
            EstimatedArrivalTime: null);

        var result = _createBooking.Execute(request);

        Console.WriteLine(result.IsSuccess
            ? $"Booking created with id {result.Value!.Id}"
            : result.Error.Message);

        Pause();
    }

    private void DeleteBooking()
    {
        Console.Clear();
        Console.Write("Enter booking id to delete: ");

        if (!Guid.TryParse(Console.ReadLine(), out Guid bookingId))
        {
            Console.WriteLine("Invalid id.");
            Pause();
            return;
        }

        var request = new DeleteBookingRequest(bookingId);
        var result = _deleteBooking.Execute(request);

        Console.WriteLine(result.IsSuccess
            ? "Booking deleted successfully."
            : result.Error.Message);

        Pause();
    }

    private static void Pause()
    {
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }
}