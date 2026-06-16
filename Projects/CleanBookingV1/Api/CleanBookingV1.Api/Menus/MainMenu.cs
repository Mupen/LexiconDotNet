namespace CleanBookingV1.Api.Menus;

public class MainMenu(ParkingMenu parkingMenu)
{
    // private readonly BookingMenu _bookingMenu = bookingMenu;
    // private readonly RoomMenu _roomMenu = roomMenu;
    private readonly ParkingMenu _parkingMenu = parkingMenu;

    public async Task RunAsync()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== MAIN MENU ===");
            Console.WriteLine("1. Bookings");
            Console.WriteLine("2. Rooms");
            Console.WriteLine("3. Parking");
            Console.WriteLine("0. Exit");
            Console.WriteLine("=== MAIN MENU ===");

            Console.Write("> ");
            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    // _bookingMenu.Run();
                    break;
                case "2":
                    // _roomMenu.Run();
                    break;
                case "3":
                    await _parkingMenu.RunAsync();
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Invalid choice");
                    Console.ReadKey();
                    break;
            }
        }
    }
}