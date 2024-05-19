using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace vgt_saga_flight.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Airports",
                columns: table => new
                {
                    AirportDbId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AirportCode = table.Column<string>(type: "text", nullable: false),
                    AirportCity = table.Column<string>(type: "text", nullable: false),
                    IsDeparture = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airports", x => x.AirportDbId);
                });

            migrationBuilder.CreateTable(
                name: "Flights",
                columns: table => new
                {
                    FlightDbId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Price = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    FlightTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ArrivalAirportAirportDbId = table.Column<int>(type: "integer", nullable: false),
                    DepartureAirportAirportDbId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flights", x => x.FlightDbId);
                    table.ForeignKey(
                        name: "FK_Flights_Airports_ArrivalAirportAirportDbId",
                        column: x => x.ArrivalAirportAirportDbId,
                        principalTable: "Airports",
                        principalColumn: "AirportDbId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Flights_Airports_DepartureAirportAirportDbId",
                        column: x => x.DepartureAirportAirportDbId,
                        principalTable: "Airports",
                        principalColumn: "AirportDbId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    BookingId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FlightDbId = table.Column<int>(type: "integer", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Temporary = table.Column<int>(type: "integer", nullable: false),
                    TemporaryDt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.BookingId);
                    table.ForeignKey(
                        name: "FK_Bookings_Flights_FlightDbId",
                        column: x => x.FlightDbId,
                        principalTable: "Flights",
                        principalColumn: "FlightDbId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_FlightDbId",
                table: "Bookings",
                column: "FlightDbId");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_ArrivalAirportAirportDbId",
                table: "Flights",
                column: "ArrivalAirportAirportDbId");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_DepartureAirportAirportDbId",
                table: "Flights",
                column: "DepartureAirportAirportDbId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "Flights");

            migrationBuilder.DropTable(
                name: "Airports");
        }
    }
}
