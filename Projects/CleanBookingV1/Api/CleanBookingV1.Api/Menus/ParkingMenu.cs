using CleanBookingV1.Application.Requests;
using CleanBookingV1.Application.Requests.Parking;
using CleanBookingV1.Application.UseCases.Parking;
using CleanBookingV1.Domain.Enums;

namespace CleanBookingV1.Api.Menus;

public class ParkingMenu(
    GetAllParkingSpaces getAllParkingSpaces,
    CreateParkingSpace createParkingSpace,
    UpdateParkingSpace updateParkingSpace,
    DeleteParkingSpace deleteParkingSpace,
    ActivateParkingSpace activateParkingSpace,
    DeactivateParkingSpace deactivateParkingSpace)
{
    private readonly GetAllParkingSpaces _getAllParkingSpaces = getAllParkingSpaces;
    private readonly CreateParkingSpace _createParkingSpace = createParkingSpace;
    private readonly UpdateParkingSpace _updateParkingSpace = updateParkingSpace;
    private readonly DeleteParkingSpace _deleteParkingSpace = deleteParkingSpace;
    private readonly ActivateParkingSpace _activateParkingSpace = activateParkingSpace;
    private readonly DeactivateParkingSpace _deactivateParkingSpace = deactivateParkingSpace;

    public async Task RunAsync()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== PARKING MENU ===");
            Console.WriteLine("1. List all parking spaces");
            Console.WriteLine("2. Create parking space");
            Console.WriteLine("3. Update parking space");
            Console.WriteLine("4. Delete parking space");
            Console.WriteLine("5. Activate parking space");
            Console.WriteLine("6. Deactivate parking space");
            Console.WriteLine("0. Back");

            string? input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    await ListAllParkingSpacesAsync();
                    break;
                case "2":
                    await CreateParkingSpaceAsync();
                    break;
                case "3":
                    await UpdateParkingSpaceAsync();
                    break;
                case "4":
                    await DeleteParkingSpaceAsync();
                    break;
                case "5":
                    await ActivateParkingSpaceAsync();
                    break;
                case "6":
                    await DeactivateParkingSpaceAsync();
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

    private async Task ListAllParkingSpacesAsync()
    {
        var result = await _getAllParkingSpaces.ExecuteAsync();

        Console.Clear();
        Console.WriteLine("=== ALL PARKING SPACES ===");

        if (result.IsFailure)
        {
            Console.WriteLine(result.Error.Message);
            Pause();
            return;
        }

        foreach (var parkingSpace in result.Value!)
        {
            Console.WriteLine(
                $"Id: {parkingSpace.Id} | Name: {parkingSpace.Name} | Type: {parkingSpace.Type} | Active: {parkingSpace.IsActive}");
        }

        Pause();
    }

    private async Task CreateParkingSpaceAsync()
    {
        Console.Clear();

        Console.Write("Enter parking space name: ");
        string name = Console.ReadLine() ?? string.Empty;

        Console.WriteLine("Parking space type:");
        Console.WriteLine("1. Standard");
        Console.WriteLine("2. Disabled");
        Console.WriteLine("3. Employee");
        Console.WriteLine("4. Service");
        Console.Write("Choose parking space type: ");

        if (!int.TryParse(Console.ReadLine(), out int parkingTypeValue) ||
            !Enum.IsDefined(typeof(ParkingSpaceType), parkingTypeValue))
        {
            Console.WriteLine("Invalid parking space type.");
            Pause();
            return;
        }

        var request = new CreateParkingSpaceRequest(
            Name: name,
            Type: (ParkingSpaceType)parkingTypeValue);

        var result = await _createParkingSpace.ExecuteAsync(request);

        Console.WriteLine(result.IsSuccess
            ? $"Parking space created successfully with id {result.Value!.Id}."
            : result.Error.Message);

        Pause();
    }

    private async Task UpdateParkingSpaceAsync()
    {
        Console.Clear();

        Console.Write("Enter parking space id to update: ");
        if (!int.TryParse(Console.ReadLine(), out int parkingSpaceId))
        {
            Console.WriteLine("Invalid id.");
            Pause();
            return;
        }

        Console.Write("Enter new parking space name: ");
        string name = Console.ReadLine() ?? string.Empty;

        Console.WriteLine("Parking space type:");
        Console.WriteLine("1. Standard");
        Console.WriteLine("2. Disabled");
        Console.WriteLine("3. Employee");
        Console.WriteLine("4. Service");
        Console.Write("Choose parking space type: ");

        if (!int.TryParse(Console.ReadLine(), out int parkingTypeValue) ||
            !Enum.IsDefined(typeof(ParkingSpaceType), parkingTypeValue))
        {
            Console.WriteLine("Invalid parking space type.");
            Pause();
            return;
        }

        var request = new UpdateParkingSpaceRequest(
            ParkingSpaceId: parkingSpaceId,
            Name: name,
            Type: (ParkingSpaceType)parkingTypeValue);

        var result = await _updateParkingSpace.ExecuteAsync(request);

        Console.WriteLine(result.IsSuccess
            ? "Parking space updated successfully."
            : result.Error.Message);

        Pause();
    }

    private async Task DeleteParkingSpaceAsync()
    {
        Console.Clear();
        Console.Write("Enter parking space id to delete: ");

        if (!int.TryParse(Console.ReadLine(), out int parkingSpaceId))
        {
            Console.WriteLine("Invalid id.");
            Pause();
            return;
        }

        var request = new DeleteParkingSpaceRequest(parkingSpaceId);

        var result = await _deleteParkingSpace.ExecuteAsync(request);

        Console.WriteLine(result.IsSuccess
            ? "Parking space deleted successfully."
            : result.Error.Message);

        Pause();
    }

    private async Task ActivateParkingSpaceAsync()
    {
        Console.Clear();
        Console.Write("Enter parking space id to activate: ");

        if (!int.TryParse(Console.ReadLine(), out int parkingSpaceId))
        {
            Console.WriteLine("Invalid id.");
            Pause();
            return;
        }

        var request = new ActivateParkingSpaceRequest(parkingSpaceId);

        var result = await _activateParkingSpace.ExecuteAsync(request);

        Console.WriteLine(result.IsSuccess
            ? "Parking space activated successfully."
            : result.Error.Message);

        Pause();
    }

    private async Task DeactivateParkingSpaceAsync()
    {
        Console.Clear();
        Console.Write("Enter parking space id to deactivate: ");

        if (!int.TryParse(Console.ReadLine(), out int parkingSpaceId))
        {
            Console.WriteLine("Invalid id.");
            Pause();
            return;
        }

        var request = new DeactivateParkingSpaceRequest(parkingSpaceId);

        var result = await _deactivateParkingSpace.ExecuteAsync(request);

        Console.WriteLine(result.IsSuccess
            ? "Parking space deactivated successfully."
            : result.Error.Message);

        Pause();
    }

    private static void Pause()
    {
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }
}