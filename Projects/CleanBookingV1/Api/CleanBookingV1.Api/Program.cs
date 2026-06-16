using CleanBookingV1.Api.Menus;
using CleanBookingV1.Application.Interfaces;
using CleanBookingV1.Application.UseCases.Bookings;
using CleanBookingV1.Application.UseCases.Parking;
using CleanBookingV1.Application.UseCases.Rooms;
using CleanBookingV1.Infrastructure.Repositories;
using CleanBookingV1.Infrastructure.Services;

namespace CleanBookingV1.Api;

public class Program
{
    public static async Task Main()
    {
        // Repositories
        // IRoomRepository roomRepository = new InMemoryRoomRepository();
        // IBookingRepository bookingRepository = new InMemoryBookingRepository();
        IParkingSpaceRepository parkingSpaceRepository = new InMemoryParkingSpaceRepository();

        // Services
        // IGuidGenerator guidGenerator = new SystemGuidGenerator();
        IIntIdGenerator parkingIdGenerator = new InMemoryIntIdGenerator(2);
        // IIntIdGenerator roomIdGenerator = new InMemoryIntIdGenerator(4);

        // Booking use cases
        // var createBooking = new CreateBooking(bookingRepository, roomRepository, parkingSpaceRepository, guidGenerator);
        // var getAllBookings = new GetAllBookings(bookingRepository);
        // var getBookingById = new GetBookingById(bookingRepository);
        // var updateBooking = new UpdateBooking(bookingRepository, roomRepository, parkingSpaceRepository);
        // var deleteBooking = new DeleteBooking(bookingRepository);

        // Room use cases
        // var getAllRooms = new GetAllRooms(roomRepository);
        // var getRoomById = new GetRoomById(roomRepository);
        // var createRoom = new CreateRoom(roomRepository, roomIdGenerator);
        // var updateRoom = new UpdateRoom(roomRepository);
        // var deleteRoom = new DeleteRoom(roomRepository);
        // var activateRoom = new ActivateRoom(roomRepository);
        // var deactivateRoom = new DeactivateRoom(roomRepository);

        // Parking use cases
        var getAllParkingSpaces = new GetAllParkingSpaces(parkingSpaceRepository);
        var createParkingSpace = new CreateParkingSpace(parkingSpaceRepository, parkingIdGenerator);
        var updateParkingSpace = new UpdateParkingSpace(parkingSpaceRepository);
        var deleteParkingSpace = new DeleteParkingSpace(parkingSpaceRepository);
        var activateParkingSpace = new ActivateParkingSpace(parkingSpaceRepository);
        var deactivateParkingSpace = new DeactivateParkingSpace(parkingSpaceRepository);

        // Menus
        // var bookingMenu = new BookingMenu(createBooking, getAllBookings, getBookingById, updateBooking, deleteBooking);
        // var roomMenu = new RoomMenu(getAllRooms, getRoomById, createRoom, updateRoom, deleteRoom, activateRoom, deactivateRoom);
        var parkingMenu = new ParkingMenu(getAllParkingSpaces, createParkingSpace, updateParkingSpace, deleteParkingSpace, activateParkingSpace, deactivateParkingSpace);
        var mainMenu = new MainMenu(parkingMenu);

        // Run
        await mainMenu.RunAsync();
    }
}