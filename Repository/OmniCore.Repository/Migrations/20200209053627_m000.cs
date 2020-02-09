using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OmniCore.Repository.Migrations
{
    public partial class m000 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Medications",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SyncId = table.Column<Guid>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Updated = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Hormone = table.Column<int>(nullable: false),
                    UnitName = table.Column<string>(nullable: true),
                    UnitNameShort = table.Column<string>(nullable: true),
                    UnitsPerMilliliter = table.Column<decimal>(nullable: false),
                    ProfileCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SyncId = table.Column<Guid>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Updated = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ManagedRemotely = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Gender = table.Column<int>(nullable: true),
                    DateOfBirth = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pods",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SyncId = table.Column<Guid>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Updated = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    UserId = table.Column<long>(nullable: true),
                    MedicationId = table.Column<long>(nullable: true),
                    Options = table.Column<string>(nullable: true),
                    ExpiresSoonReminder = table.Column<string>(nullable: true),
                    ReservoirLowReminder = table.Column<string>(nullable: true),
                    ExpiredReminder = table.Column<string>(nullable: true),
                    RadioAddress = table.Column<uint>(nullable: false),
                    Lot = table.Column<uint>(nullable: false),
                    Serial = table.Column<uint>(nullable: false),
                    HwRevision = table.Column<string>(nullable: true),
                    SwRevision = table.Column<string>(nullable: true),
                    PodUtcOffset = table.Column<TimeSpan>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pods_Medications_MedicationId",
                        column: x => x.MedicationId,
                        principalTable: "Medications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pods_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MedicationDeliveries",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SyncId = table.Column<Guid>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Updated = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    PodId = table.Column<long>(nullable: true),
                    DeliveryStart = table.Column<DateTime>(nullable: true),
                    IntendedDuration = table.Column<TimeSpan>(nullable: true),
                    IntendedAmount = table.Column<decimal>(nullable: true),
                    DeliveredAmount = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicationDeliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicationDeliveries_Pods_PodId",
                        column: x => x.PodId,
                        principalTable: "Pods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PodRequests",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SyncId = table.Column<Guid>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Updated = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    PodId = table.Column<long>(nullable: true),
                    RequestStatus = table.Column<int>(nullable: false),
                    RequestType = table.Column<int>(nullable: false),
                    Parameters = table.Column<string>(nullable: true),
                    StartEarliest = table.Column<DateTime>(nullable: true),
                    StartLatest = table.Column<DateTime>(nullable: true),
                    Started = table.Column<DateTime>(nullable: true),
                    ResultReceived = table.Column<DateTime>(nullable: true),
                    FailureType = table.Column<int>(nullable: true),
                    ErrorText = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PodRequests_Pods_PodId",
                        column: x => x.PodId,
                        principalTable: "Pods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Radios",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SyncId = table.Column<Guid>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Updated = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DeviceUuid = table.Column<Guid>(nullable: false),
                    ServiceUuid = table.Column<Guid>(nullable: false),
                    DeviceName = table.Column<string>(nullable: true),
                    UserDescription = table.Column<string>(nullable: true),
                    Options = table.Column<string>(nullable: true),
                    PodEntityId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Radios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Radios_Pods_PodEntityId",
                        column: x => x.PodEntityId,
                        principalTable: "Pods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PodResponses",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SyncId = table.Column<Guid>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Updated = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    PodRequestId = table.Column<long>(nullable: true),
                    Progress = table.Column<int>(nullable: true),
                    Faulted = table.Column<bool>(nullable: true),
                    FaultResponse = table.Column<string>(nullable: true),
                    RadioResponse = table.Column<string>(nullable: true),
                    StatusResponse = table.Column<string>(nullable: true),
                    VersionResponse = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PodResponses_PodRequests_PodRequestId",
                        column: x => x.PodRequestId,
                        principalTable: "PodRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RadioEvents",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SyncId = table.Column<Guid>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Updated = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    RadioId = table.Column<long>(nullable: true),
                    EventType = table.Column<int>(nullable: false),
                    Data = table.Column<byte[]>(nullable: true),
                    Text = table.Column<string>(nullable: true),
                    Rssi = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RadioEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RadioEvents_Radios_RadioId",
                        column: x => x.RadioId,
                        principalTable: "Radios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicationDeliveries_PodId",
                table: "MedicationDeliveries",
                column: "PodId");

            migrationBuilder.CreateIndex(
                name: "IX_PodRequests_PodId",
                table: "PodRequests",
                column: "PodId");

            migrationBuilder.CreateIndex(
                name: "IX_PodResponses_PodRequestId",
                table: "PodResponses",
                column: "PodRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Pods_MedicationId",
                table: "Pods",
                column: "MedicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Pods_UserId",
                table: "Pods",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RadioEvents_RadioId",
                table: "RadioEvents",
                column: "RadioId");

            migrationBuilder.CreateIndex(
                name: "IX_Radios_PodEntityId",
                table: "Radios",
                column: "PodEntityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicationDeliveries");

            migrationBuilder.DropTable(
                name: "PodResponses");

            migrationBuilder.DropTable(
                name: "RadioEvents");

            migrationBuilder.DropTable(
                name: "PodRequests");

            migrationBuilder.DropTable(
                name: "Radios");

            migrationBuilder.DropTable(
                name: "Pods");

            migrationBuilder.DropTable(
                name: "Medications");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
