using CleanBookingV1.Application.UseCases.Rooms;
using CleanBookingV1.Domain.Enums;

namespace CleanBookingV1.Api.Menus;

public class RoomMenu(
    GetAllRooms getAllRooms,
    GetRoomById getRoomById,
    CreateRoom createRoom,
    UpdateRoom updateRoom,
    DeleteRoom deleteRoom,
    ActivateRoom activateRoom,
    DeactivateRoom deactivateRoom)
{
    private readonly GetAllRooms _getAllRooms = getAllRooms;
    private readonly GetRoomById _getRoomById = getRoomById;
    private readonly CreateRoom _createRoom = createRoom;
    private readonly UpdateRoom _updateRoom = updateRoom;
    private readonly DeleteRoom _deleteRoom = deleteRoom;
    private readonly ActivateRoom _activateRoom = activateRoom;
    private readonly DeactivateRoom _deactivateRoom = deactivateRoom;

    public void Run()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== ROOM MENU ===");
            Console.WriteLine("1. List all rooms");
            Console.WriteLine("2. Get room by id");
            Console.WriteLine("3. Create room");
            Console.WriteLine("4. Update room");
            Console.WriteLine("5. Delete room");
            Console.WriteLine("6. Activate room");
            Console.WriteLine("7. Deactivate room");
            Console.WriteLine("0. Back");

            string? input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    ListAllRooms();
                    break;
                case "2":
                    GetRoomById();
                    break;
                case "3":
                    CreateRoom();
                    break;
                case "4":
                    UpdateRoom();
                    break;
                case "5":
                    DeleteRoom();
                    break;
                case "6":
                    ActivateRoom();
                    break;
                case "7":
                    DeactivateRoom();
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

    private void ListAllRooms()
    {
        var result = _getAllRooms.Execute();

        Console.Clear();
        Console.WriteLine("=== ALL ROOMS ===");

        if (result.IsFailure)
        {
            Console.WriteLine(result.Error.Message);
            Pause();
            return;
        }

        foreach (var room in result.Value!)
        {
            Console.WriteLine(
                $"Id: {room.Id} | Name: {room.Name} | Type: {room.RoomType} | Capacity: {room.Capacity} | Price: {room.PricePerNight} | Active: {room.IsActive}");
        }

        Pause();
    }

    private void GetRoomById()
    {
        Console.Clear();
        Console.Write("Enter room id: ");

        if (!int.TryParse(Console.ReadLine(), out int roomId))
        {
            Console.WriteLine("Invalid room id.");
            Pause();
            return;
        }

        var request = new GetRoomByIdRequest(roomId);
        var result = _getRoomById.Execute(request);

        if (result.IsFailure)
        {
            Console.WriteLine(result.Error.Message);
            Pause();
            return;
        }

        var room = result.Value!;
        Console.WriteLine($"Id: {room.Id}");
        Console.WriteLine($"Name: {room.Name}");
        Console.WriteLine($"Type: {room.RoomType}");
        Console.WriteLine($"Size: {room.SizeInSquareMeters}");
        Console.WriteLine($"Capacity: {room.Capacity}");
        Console.WriteLine($"Price: {room.PricePerNight}");
        Console.WriteLine($"Active: {room.IsActive}");

        Pause();
    }

    private void CreateRoom()
    {
        Console.Clear();

        Console.Write("Enter room name: ");
        string name = Console.ReadLine() ?? string.Empty;

        Console.WriteLine("Room type:");
        Console.WriteLine("1. Single");
        Console.WriteLine("2. DoubleTwin");
        Console.WriteLine("3. DoubleBed");
        Console.WriteLine("4. Family");
        Console.Write("Choose room type: ");

        if (!int.TryParse(Console.ReadLine(), out int roomTypeValue) ||
            !Enum.IsDefined(typeof(RoomType), roomTypeValue))
        {
            Console.WriteLine("Invalid room type.");
            Pause();
            return;
        }

        Console.Write("Enter size in square meters: ");
        if (!int.TryParse(Console.ReadLine(), out int sizeInSquareMeters))
        {
            Console.WriteLine("Invalid size.");
            Pause();
            return;
        }

        Console.Write("Enter capacity: ");
        if (!int.TryParse(Console.ReadLine(), out int capacity))
        {
            Console.WriteLine("Invalid capacity.");
            Pause();
            return;
        }

        Console.Write("Enter price per night: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal pricePerNight))
        {
            Console.WriteLine("Invalid price.");
            Pause();
            return;
        }

        var request = new CreateRoomRequest(
            Name: name,
            RoomType: (RoomType)roomTypeValue,
            SizeInSquareMeters: sizeInSquareMeters,
            Capacity: capacity,
            PricePerNight: pricePerNight);

        var result = _createRoom.Execute(request);

        Console.WriteLine(result.IsSuccess
            ? $"Room created successfully with id {result.Value!.Id}."
            : result.Error.Message);

        Pause();
    }

    private void UpdateRoom()
    {
        Console.Clear();

        Console.Write("Enter room id to update: ");
        if (!int.TryParse(Console.ReadLine(), out int roomId))
        {
            Console.WriteLine("Invalid id.");
            Pause();
            return;
        }

        Console.Write("Enter new room name: ");
        string name = Console.ReadLine() ?? string.Empty;

        Console.WriteLine("Room type:");
        Console.WriteLine("1. Single");
        Console.WriteLine("2. DoubleTwin");
        Console.WriteLine("3. DoubleBed");
        Console.WriteLine("4. Family");
        Console.Write("Choose room type: ");

        if (!int.TryParse(Console.ReadLine(), out int roomTypeValue) ||
            !Enum.IsDefined(typeof(RoomType), roomTypeValue))
        {
            Console.WriteLine("Invalid room type.");
            Pause();
            return;
        }

        Console.Write("Enter new size in square meters: ");
        if (!int.TryParse(Console.ReadLine(), out int sizeInSquareMeters))
        {
            Console.WriteLine("Invalid size.");
            Pause();
            return;
        }

        Console.Write("Enter new capacity: ");
        if (!int.TryParse(Console.ReadLine(), out int capacity))
        {
            Console.WriteLine("Invalid capacity.");
            Pause();
            return;
        }

        Console.Write("Enter new price per night: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal pricePerNight))
        {
            Console.WriteLine("Invalid price.");
            Pause();
            return;
        }

        var request = new UpdateRoomRequest(
            RoomId: roomId,
            Name: name,
            RoomType: (RoomType)roomTypeValue,
            SizeInSquareMeters: sizeInSquareMeters,
            Capacity: capacity,
            PricePerNight: pricePerNight);

        var result = _updateRoom.Execute(request);

        Console.WriteLine(result.IsSuccess
            ? "Room updated successfully."
            : result.Error.Message);

        Pause();
    }

    private void DeleteRoom()
    {
        Console.Clear();
        Console.Write("Enter room id to delete: ");

        if (!int.TryParse(Console.ReadLine(), out int roomId))
        {
            Console.WriteLine("Invalid id.");
            Pause();
            return;
        }

        var request = new DeleteRoomRequest(roomId);
        var result = _deleteRoom.Execute(request);

        Console.WriteLine(result.IsSuccess
            ? "Room deleted successfully."
            : result.Error.Message);

        Pause();
    }

    private void ActivateRoom()
    {
        Console.Clear();
        Console.Write("Enter room id to activate: ");

        if (!int.TryParse(Console.ReadLine(), out int roomId))
        {
            Console.WriteLine("Invalid id.");
            Pause();
            return;
        }

        var request = new ActivateRoomRequest(roomId);
        var result = _activateRoom.Execute(request);

        Console.WriteLine(result.IsSuccess
            ? "Room activated successfully."
            : result.Error.Message);

        Pause();
    }

    private void DeactivateRoom()
    {
        Console.Clear();
        Console.Write("Enter room id to deactivate: ");

        if (!int.TryParse(Console.ReadLine(), out int roomId))
        {
            Console.WriteLine("Invalid id.");
            Pause();
            return;
        }

        var request = new DeactivateRoomRequest(roomId);
        var result = _deactivateRoom.Execute(request);

        Console.WriteLine(result.IsSuccess
            ? "Room deactivated successfully."
            : result.Error.Message);

        Pause();
    }

    private static void Pause()
    {
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }
}