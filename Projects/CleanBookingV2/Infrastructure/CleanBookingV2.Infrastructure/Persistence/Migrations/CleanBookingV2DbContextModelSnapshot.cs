using System;
using CleanBookingV2.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace CleanBookingV2.Infrastructure.Persistence.Migrations;

[DbContext(typeof(CleanBookingV2DbContext))]
partial class CleanBookingV2DbContextModelSnapshot : ModelSnapshot
{
    /// <summary>
    /// EF Core-generated snapshot of the current model.
    /// Migrations use this method to detect what changed since the last migration.
    /// It is kept in source control so database schema evolution is reproducible.
    /// </summary>
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.9");

        modelBuilder.Entity("CleanBookingV2.Domain.Entities.Booking", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("TEXT");
            b.Property<string>("GuestName").IsRequired().HasMaxLength(150).HasColumnType("TEXT");
            b.Property<TimeOnly?>("EstimatedArrivalTime").HasColumnType("TEXT");
            b.Property<int>("NumberOfGuests").HasColumnType("INTEGER");
            b.Property<int?>("ParkingSpaceId").HasColumnType("INTEGER");
            b.Property<int>("RoomId").HasColumnType("INTEGER");
            b.Property<string>("Status").IsRequired().HasMaxLength(30).HasColumnType("TEXT");
            b.Property<decimal>("TotalPrice").HasColumnType("decimal(18,2)");
            b.Property<Guid>("Version").IsConcurrencyToken().HasColumnType("TEXT");
            b.HasKey("Id");
            b.HasIndex("ParkingSpaceId");
            b.HasIndex("RoomId");
            b.ToTable("Bookings");
        });

        modelBuilder.Entity("CleanBookingV2.Domain.Entities.ParkingSpace", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<bool>("IsActive").HasColumnType("INTEGER");
            b.Property<string>("Name").IsRequired().HasMaxLength(100).HasColumnType("TEXT");
            b.Property<string>("ParkingSpaceType").IsRequired().HasMaxLength(30).HasColumnType("TEXT");
            b.HasKey("Id");
            b.ToTable("ParkingSpaces");
            b.HasData(
                new { Id = 1, IsActive = true, Name = "Parking Space 1", ParkingSpaceType = "Standard" },
                new { Id = 2, IsActive = true, Name = "Parking Space 2", ParkingSpaceType = "Standard" });
        });

        modelBuilder.Entity("CleanBookingV2.Domain.Entities.Room", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            b.Property<int>("Capacity").HasColumnType("INTEGER");
            b.Property<bool>("IsActive").HasColumnType("INTEGER");
            b.Property<string>("Name").IsRequired().HasMaxLength(100).HasColumnType("TEXT");
            b.Property<decimal>("PricePerNight").HasColumnType("decimal(18,2)");
            b.Property<string>("RoomType").IsRequired().HasMaxLength(30).HasColumnType("TEXT");
            b.Property<int>("SizeInSquareMeters").HasColumnType("INTEGER");
            b.HasKey("Id");
            b.ToTable("Rooms");
            b.HasData(
                new { Id = 1, Capacity = 1, IsActive = true, Name = "Room 1", PricePerNight = 550m, RoomType = "Single", SizeInSquareMeters = 11 },
                new { Id = 2, Capacity = 2, IsActive = true, Name = "Room 2", PricePerNight = 700m, RoomType = "Double", SizeInSquareMeters = 14 },
                new { Id = 3, Capacity = 2, IsActive = true, Name = "Room 3", PricePerNight = 765m, RoomType = "Double", SizeInSquareMeters = 16 },
                new { Id = 4, Capacity = 3, IsActive = true, Name = "Room 4", PricePerNight = 850m, RoomType = "Family", SizeInSquareMeters = 24 });
        });

        modelBuilder.Entity("CleanBookingV2.Domain.Entities.Booking", b =>
        {
            b.HasOne("CleanBookingV2.Domain.Entities.ParkingSpace", null)
                .WithMany()
                .HasForeignKey("ParkingSpaceId")
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne("CleanBookingV2.Domain.Entities.Room", null)
                .WithMany()
                .HasForeignKey("RoomId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.OwnsOne("CleanBookingV2.Domain.ValueObjects.DateRange", "Stay", b1 =>
            {
                b1.Property<Guid>("BookingId").HasColumnType("TEXT");
                b1.Property<DateTime>("End").HasColumnType("TEXT").HasColumnName("CheckOut");
                b1.Property<DateTime>("Start").HasColumnType("TEXT").HasColumnName("CheckIn");
                b1.HasKey("BookingId");
                b1.ToTable("Bookings");
                b1.WithOwner().HasForeignKey("BookingId");
            });

            b.Navigation("Stay").IsRequired();
        });
    }
}
