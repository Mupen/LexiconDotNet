using CleanBookingV2.Domain.Entities;
using CleanBookingV2.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CleanBookingV2.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for CleanBookingV2.
/// Infrastructure owns persistence details so the domain and application layers can
/// stay independent of SQLite and EF Core. The DbContext maps domain entities to
/// tables, configures conversions, and seeds demo data for local learning.
/// </summary>
public sealed class CleanBookingV2DbContext : DbContext
{
    public CleanBookingV2DbContext(DbContextOptions<CleanBookingV2DbContext> options)
        : base(options)
    {
    }

    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<ParkingSpace> ParkingSpaces => Set<ParkingSpace>();
    public DbSet<Booking> Bookings => Set<Booking>();

    /// <summary>
    /// Configures entity mapping, seed data, owned value objects, and relationships.
    /// Fluent configuration is used instead of scattering persistence attributes
    /// across domain classes because persistence is an infrastructure concern.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Room seed data gives the demo application a usable catalog immediately
        // after migrations run. In a production system this might come from admin
        // screens or reviewed seed scripts instead.
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(room => room.Id);
            entity.Property(room => room.Name).HasMaxLength(100).IsRequired();
            entity.Property(room => room.RoomType).HasConversion<string>().HasMaxLength(30);
            entity.Property(room => room.PricePerNight).HasColumnType("decimal(18,2)");

            entity.HasData(
                new Room(1, "Room 1", RoomType.Single, 11, 1, 550m),
                new Room(2, "Room 2", RoomType.Double, 14, 2, 700m),
                new Room(3, "Room 3", RoomType.Double, 16, 2, 765m),
                new Room(4, "Room 4", RoomType.Family, 24, 3, 850m));
        });

        // Parking spaces are seeded separately because parking is optional and has
        // its own availability timeline independent from room availability.
        modelBuilder.Entity<ParkingSpace>(entity =>
        {
            entity.HasKey(space => space.Id);
            entity.Property(space => space.Name).HasMaxLength(100).IsRequired();
            entity.Property(space => space.ParkingSpaceType).HasConversion<string>().HasMaxLength(30);

            entity.HasData(
                new ParkingSpace(1, "Parking Space 1", ParkingSpaceType.Standard),
                new ParkingSpace(2, "Parking Space 2", ParkingSpaceType.Standard));
        });

        // Booking owns the DateRange value object and uses conversion for enum and
        // TimeOnly values so SQLite can store them predictably.
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(booking => booking.Id);
            entity.Property(booking => booking.GuestName).HasMaxLength(150).IsRequired();
            entity.Property(booking => booking.TotalPrice).HasColumnType("decimal(18,2)");
            entity.Property(booking => booking.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(booking => booking.Version).IsConcurrencyToken();
            // SQLite does not have a native TimeOnly column type. Storing HH:mm text
            // is simple, readable, and enough for the demo's half-hour time slots.
            entity.Property(booking => booking.EstimatedArrivalTime)
                .HasConversion(
                    time => time.HasValue ? time.Value.ToString("HH:mm") : null,
                    value => value == null ? null : TimeOnly.Parse(value));

            entity.OwnsOne(booking => booking.Stay, stay =>
            {
                // DateRange is part of the Booking row, not a separate aggregate.
                // OwnsOne maps the value object's properties into booking columns.
                stay.Property(dateRange => dateRange.Start).HasColumnName("CheckIn").IsRequired();
                stay.Property(dateRange => dateRange.End).HasColumnName("CheckOut").IsRequired();
            });

            // Restrict deletes so historical bookings cannot silently lose their
            // referenced room or parking data if catalog rows are removed later.
            entity.HasOne<Room>()
                .WithMany()
                .HasForeignKey(booking => booking.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<ParkingSpace>()
                .WithMany()
                .HasForeignKey(booking => booking.ParkingSpaceId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
