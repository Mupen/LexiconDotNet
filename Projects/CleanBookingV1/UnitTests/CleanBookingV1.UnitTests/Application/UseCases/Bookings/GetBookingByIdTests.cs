using CleanBookingV1.Application.UseCases.Bookings;
using CleanBookingV1.Domain.Entities;
using CleanBookingV1.Domain.ValueObjects;
using CleanBookingV1.Infrastructure.Repositories;
using FluentAssertions;

namespace CleanBookingV1.UnitTests.Application.UseCases.Bookings;

public sealed class GetBookingByIdTests
{
    [Fact]
    public void Execute_Should_ReturnBooking_When_IdExists()
    {
        var bookingRepository = new InMemoryBookingRepository();
        var useCase = new GetBookingById(bookingRepository);

        var booking = new Booking(
            id: Guid.NewGuid(),
            guestName: "Test",
            stay: new DateRange(DateTime.Today, DateTime.Today.AddDays(1)),
            numberOfGuests: 1,
            roomId: 1,
            parkingSpaceId: null,
            totalPrice: 100m,
            estimatedArrivalTime: null);

        bookingRepository.Add(booking);

        var request = new GetBookingByIdRequest(booking.Id);

        var result = useCase.Execute(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().Be(booking);
    }

    [Fact]
    public void Execute_Should_ReturnFailure_When_IdDoesNotExist()
    {
        var bookingRepository = new InMemoryBookingRepository();
        var useCase = new GetBookingById(bookingRepository);

        var request = new GetBookingByIdRequest(Guid.NewGuid());

        var result = useCase.Execute(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Booking.GetById.NotFound");
    }

    [Fact]
    public void Execute_Should_ReturnFailure_When_IdIsEmpty()
    {
        var bookingRepository = new InMemoryBookingRepository();
        var useCase = new GetBookingById(bookingRepository);

        var request = new GetBookingByIdRequest(Guid.Empty);

        var result = useCase.Execute(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Booking.GetById.InvalidId");
    }
}