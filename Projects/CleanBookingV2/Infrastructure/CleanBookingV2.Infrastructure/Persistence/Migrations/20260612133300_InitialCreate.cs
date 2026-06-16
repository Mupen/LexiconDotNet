using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanBookingV2.Infrastructure.Persistence.Migrations;

/// <summary>
/// Initial EF Core migration that creates the SQLite schema and seed data.
/// Migration files are generated from DbContext configuration, so comments here
/// explain the purpose of the generated operations rather than changing the logic.
/// </summary>
public partial class InitialCreate : Migration
{
    /// <summary>
    /// Applies the first database schema.
    /// EF Core calls this when migrating an empty database forward. It creates the
    /// catalog tables first, then bookings, because bookings depend on room and
    /// parking foreign keys.
    /// </summary>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ParkingSpaces",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                ParkingSpaceType = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ParkingSpaces", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Rooms",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                RoomType = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                SizeInSquareMeters = table.Column<int>(type: "INTEGER", nullable: false),
                Capacity = table.Column<int>(type: "INTEGER", nullable: false),
                PricePerNight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Rooms", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Bookings",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                GuestName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                CheckIn = table.Column<DateTime>(type: "TEXT", nullable: false),
                CheckOut = table.Column<DateTime>(type: "TEXT", nullable: false),
                NumberOfGuests = table.Column<int>(type: "INTEGER", nullable: false),
                RoomId = table.Column<int>(type: "INTEGER", nullable: false),
                ParkingSpaceId = table.Column<int>(type: "INTEGER", nullable: true),
                TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                EstimatedArrivalTime = table.Column<string>(type: "TEXT", nullable: true),
                Status = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                Version = table.Column<Guid>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Bookings", x => x.Id);
                table.ForeignKey(
                    name: "FK_Bookings_ParkingSpaces_ParkingSpaceId",
                    column: x => x.ParkingSpaceId,
                    principalTable: "ParkingSpaces",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Bookings_Rooms_RoomId",
                    column: x => x.RoomId,
                    principalTable: "Rooms",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.InsertData(
            table: "ParkingSpaces",
            columns: new[] { "Id", "IsActive", "Name", "ParkingSpaceType" },
            values: new object[,]
            {
                { 1, true, "Parking Space 1", "Standard" },
                { 2, true, "Parking Space 2", "Standard" }
            });

        migrationBuilder.InsertData(
            table: "Rooms",
            columns: new[] { "Id", "Capacity", "IsActive", "Name", "PricePerNight", "RoomType", "SizeInSquareMeters" },
            values: new object[,]
            {
                { 1, 1, true, "Room 1", 550m, "Single", 11 },
                { 2, 2, true, "Room 2", 700m, "Double", 14 },
                { 3, 2, true, "Room 3", 765m, "Double", 16 },
                { 4, 3, true, "Room 4", 850m, "Family", 24 }
            });

        migrationBuilder.CreateIndex(
            name: "IX_Bookings_ParkingSpaceId",
            table: "Bookings",
            column: "ParkingSpaceId");

        migrationBuilder.CreateIndex(
            name: "IX_Bookings_RoomId",
            table: "Bookings",
            column: "RoomId");
    }

    /// <summary>
    /// Reverts the initial schema.
    /// Tables are dropped in dependency order: bookings first, then referenced
    /// catalog tables. This mirrors the foreign-key relationships created in Up.
    /// </summary>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Bookings");

        migrationBuilder.DropTable(
            name: "ParkingSpaces");

        migrationBuilder.DropTable(
            name: "Rooms");
    }
}
