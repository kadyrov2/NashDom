using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NashDom.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Apartments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApartmentNumber = table.Column<int>(type: "integer", nullable: false),
                    Entrance = table.Column<int>(type: "integer", nullable: false),
                    Floor = table.Column<int>(type: "integer", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    Car = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Apartments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TemplateName = table.Column<string>(type: "text", nullable: false),
                    OrganizationName = table.Column<string>(type: "text", nullable: false),
                    Inn = table.Column<string>(type: "text", nullable: false),
                    Kpp = table.Column<string>(type: "text", nullable: false),
                    Bik = table.Column<string>(type: "text", nullable: false),
                    CorrespondentAccount = table.Column<string>(type: "text", nullable: false),
                    BankName = table.Column<string>(type: "text", nullable: false),
                    SettlementAccount = table.Column<string>(type: "text", nullable: false),
                    ServiceCode = table.Column<string>(type: "text", nullable: false),
                    HeaderText = table.Column<string>(type: "text", nullable: true),
                    FooterText = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MeterReadings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ColdWater = table.Column<double>(type: "double precision", nullable: false),
                    HotWater = table.Column<double>(type: "double precision", nullable: false),
                    ApartmentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeterReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeterReadings_Apartments_ApartmentId",
                        column: x => x.ApartmentId,
                        principalTable: "Apartments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MeterReplacements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HotWaterMeterNumber = table.Column<string>(type: "text", nullable: false),
                    HotWaterMeterDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    HotWaterVerificationYear = table.Column<int>(type: "integer", nullable: true),
                    HotWaterNote = table.Column<string>(type: "text", nullable: false),
                    ColdWaterMeterNumber = table.Column<string>(type: "text", nullable: false),
                    ColdWaterMeterDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ColdWaterVerificationYear = table.Column<int>(type: "integer", nullable: true),
                    ColdWaterNote = table.Column<string>(type: "text", nullable: false),
                    ApartmentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeterReplacements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeterReplacements_Apartments_ApartmentId",
                        column: x => x.ApartmentId,
                        principalTable: "Apartments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonalAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountNumber = table.Column<string>(type: "text", nullable: false),
                    OwnerName = table.Column<string>(type: "text", nullable: false),
                    ApartmentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalAccounts_Apartments_ApartmentId",
                        column: x => x.ApartmentId,
                        principalTable: "Apartments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistrationCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    Patronymic = table.Column<string>(type: "text", nullable: false),
                    BirthDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    BirthPlaceRegion = table.Column<string>(type: "text", nullable: false),
                    BirthPlaceDistrict = table.Column<string>(type: "text", nullable: false),
                    BirthPlaceCity = table.Column<string>(type: "text", nullable: false),
                    BirthPlaceVillage = table.Column<string>(type: "text", nullable: false),
                    ArrivedFromRegion = table.Column<string>(type: "text", nullable: false),
                    ArrivedFromDistrict = table.Column<string>(type: "text", nullable: false),
                    ArrivedFromCity = table.Column<string>(type: "text", nullable: false),
                    ArrivedFromStreet = table.Column<string>(type: "text", nullable: false),
                    ArrivedHouse = table.Column<string>(type: "text", nullable: false),
                    ArrivedBuilding = table.Column<string>(type: "text", nullable: false),
                    ArrivedApartment = table.Column<string>(type: "text", nullable: false),
                    ArrivedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DocumentType = table.Column<string>(type: "text", nullable: false),
                    DocumentSeries = table.Column<string>(type: "text", nullable: false),
                    DocumentNumber = table.Column<string>(type: "text", nullable: false),
                    DocumentIssuedBy = table.Column<string>(type: "text", nullable: false),
                    DocumentDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResidenceLocality = table.Column<string>(type: "text", nullable: false),
                    ResidenceStreet = table.Column<string>(type: "text", nullable: false),
                    ResidenceHouse = table.Column<string>(type: "text", nullable: false),
                    ResidenceBuilding = table.Column<string>(type: "text", nullable: false),
                    ResidenceApartment = table.Column<string>(type: "text", nullable: false),
                    MilitaryNotes = table.Column<string>(type: "text", nullable: false),
                    RegistrationNote = table.Column<string>(type: "text", nullable: false),
                    DeregistrationNote = table.Column<string>(type: "text", nullable: false),
                    ApartmentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrationCards_Apartments_ApartmentId",
                        column: x => x.ApartmentId,
                        principalTable: "Apartments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Accruals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonalAccountId = table.Column<int>(type: "integer", nullable: false),
                    AccrualDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ServiceCode = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accruals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accruals_PersonalAccounts_PersonalAccountId",
                        column: x => x.PersonalAccountId,
                        principalTable: "PersonalAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accruals_PersonalAccountId",
                table: "Accruals",
                column: "PersonalAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Apartments_ApartmentNumber",
                table: "Apartments",
                column: "ApartmentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MeterReadings_ApartmentId",
                table: "MeterReadings",
                column: "ApartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_MeterReplacements_ApartmentId",
                table: "MeterReplacements",
                column: "ApartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalAccounts_AccountNumber",
                table: "PersonalAccounts",
                column: "AccountNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonalAccounts_ApartmentId",
                table: "PersonalAccounts",
                column: "ApartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationCards_ApartmentId",
                table: "RegistrationCards",
                column: "ApartmentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accruals");

            migrationBuilder.DropTable(
                name: "MeterReadings");

            migrationBuilder.DropTable(
                name: "MeterReplacements");

            migrationBuilder.DropTable(
                name: "OrganizationSettings");

            migrationBuilder.DropTable(
                name: "RegistrationCards");

            migrationBuilder.DropTable(
                name: "PersonalAccounts");

            migrationBuilder.DropTable(
                name: "Apartments");
        }
    }
}
