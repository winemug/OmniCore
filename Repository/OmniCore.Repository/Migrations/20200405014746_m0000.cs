using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OmniCore.Repository.Migrations
{
    public partial class m0000 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "Medications",
                table => new
                {
                    Id = table.Column<long>()
                        .Annotation("Sqlite:Autoincrement", true),
                    SyncId = table.Column<Guid>(nullable: true),
                    Created = table.Column<DateTime>(),
                    Updated = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(),
                    Name = table.Column<string>(nullable: true),
                    Hormone = table.Column<int>(),
                    UnitName = table.Column<string>(nullable: true),
                    UnitNameShort = table.Column<string>(nullable: true),
                    UnitsPerMilliliter = table.Column<decimal>(),
                    ProfileCode = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Medications", x => x.Id); });

            migrationBuilder.CreateTable(
                "Radios",
                table => new
                {
                    Id = table.Column<long>()
                        .Annotation("Sqlite:Autoincrement", true),
                    SyncId = table.Column<Guid>(nullable: true),
                    Created = table.Column<DateTime>(),
                    Updated = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(),
                    DeviceUuid = table.Column<Guid>(),
                    ServiceUuid = table.Column<Guid>(),
                    DeviceName = table.Column<string>(nullable: true),
                    UserDescription = table.Column<string>(nullable: true),
                    Options = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Radios", x => x.Id); });

            migrationBuilder.CreateTable(
                "Users",
                table => new
                {
                    Id = table.Column<long>()
                        .Annotation("Sqlite:Autoincrement", true),
                    SyncId = table.Column<Guid>(nullable: true),
                    Created = table.Column<DateTime>(),
                    Updated = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(),
                    ManagedRemotely = table.Column<bool>(),
                    Name = table.Column<string>(nullable: true),
                    Gender = table.Column<int>(nullable: true),
                    DateOfBirth = table.Column<DateTime>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Users", x => x.Id); });

            migrationBuilder.CreateTable(
                "RadioEvents",
                table => new
                {
                    Id = table.Column<long>()
                        .Annotation("Sqlite:Autoincrement", true),
                    SyncId = table.Column<Guid>(nullable: true),
                    Created = table.Column<DateTime>(),
                    Updated = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(),
                    RadioId = table.Column<long>(nullable: true),
                    EventType = table.Column<int>(),
                    Data = table.Column<byte[]>(nullable: true),
                    Text = table.Column<string>(nullable: true),
                    Rssi = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RadioEvents", x => x.Id);
                    table.ForeignKey(
                        "FK_RadioEvents_Radios_RadioId",
                        x => x.RadioId,
                        "Radios",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "Pods",
                table => new
                {
                    Id = table.Column<long>()
                        .Annotation("Sqlite:Autoincrement", true),
                    SyncId = table.Column<Guid>(nullable: true),
                    Created = table.Column<DateTime>(),
                    Updated = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(),
                    Type = table.Column<int>(),
                    UserId = table.Column<long>(nullable: true),
                    MedicationId = table.Column<long>(nullable: true),
                    Options = table.Column<string>(nullable: true),
                    ExpiresSoonReminder = table.Column<string>(nullable: true),
                    ReservoirLowReminder = table.Column<string>(nullable: true),
                    ExpiredReminder = table.Column<string>(nullable: true),
                    RadioAddress = table.Column<uint>(),
                    Lot = table.Column<uint>(),
                    Serial = table.Column<uint>(),
                    HwRevision = table.Column<string>(nullable: true),
                    SwRevision = table.Column<string>(nullable: true),
                    PodUtcOffset = table.Column<TimeSpan>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pods", x => x.Id);
                    table.ForeignKey(
                        "FK_Pods_Medications_MedicationId",
                        x => x.MedicationId,
                        "Medications",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        "FK_Pods_Users_UserId",
                        x => x.UserId,
                        "Users",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "MedicationDeliveries",
                table => new
                {
                    Id = table.Column<long>()
                        .Annotation("Sqlite:Autoincrement", true),
                    SyncId = table.Column<Guid>(nullable: true),
                    Created = table.Column<DateTime>(),
                    Updated = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(),
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
                        "FK_MedicationDeliveries_Pods_PodId",
                        x => x.PodId,
                        "Pods",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "PodRadioEntity",
                table => new
                {
                    Id = table.Column<long>()
                        .Annotation("Sqlite:Autoincrement", true),
                    SyncId = table.Column<Guid>(nullable: true),
                    Created = table.Column<DateTime>(),
                    Updated = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(),
                    PodId = table.Column<long>(nullable: true),
                    RadioId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PodRadioEntity", x => x.Id);
                    table.ForeignKey(
                        "FK_PodRadioEntity_Pods_PodId",
                        x => x.PodId,
                        "Pods",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        "FK_PodRadioEntity_Radios_RadioId",
                        x => x.RadioId,
                        "Radios",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "PodRequests",
                table => new
                {
                    Id = table.Column<long>()
                        .Annotation("Sqlite:Autoincrement", true),
                    SyncId = table.Column<Guid>(nullable: true),
                    Created = table.Column<DateTime>(),
                    Updated = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(),
                    PodId = table.Column<long>(nullable: true),
                    RequestStatus = table.Column<int>(),
                    RequestType = table.Column<int>(),
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
                        "FK_PodRequests_Pods_PodId",
                        x => x.PodId,
                        "Pods",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "PodResponses",
                table => new
                {
                    Id = table.Column<long>()
                        .Annotation("Sqlite:Autoincrement", true),
                    SyncId = table.Column<Guid>(nullable: true),
                    Created = table.Column<DateTime>(),
                    Updated = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(),
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
                        "FK_PodResponses_PodRequests_PodRequestId",
                        x => x.PodRequestId,
                        "PodRequests",
                        "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                "IX_MedicationDeliveries_PodId",
                "MedicationDeliveries",
                "PodId");

            migrationBuilder.CreateIndex(
                "IX_PodRadioEntity_PodId",
                "PodRadioEntity",
                "PodId");

            migrationBuilder.CreateIndex(
                "IX_PodRadioEntity_RadioId",
                "PodRadioEntity",
                "RadioId");

            migrationBuilder.CreateIndex(
                "IX_PodRequests_PodId",
                "PodRequests",
                "PodId");

            migrationBuilder.CreateIndex(
                "IX_PodResponses_PodRequestId",
                "PodResponses",
                "PodRequestId");

            migrationBuilder.CreateIndex(
                "IX_Pods_MedicationId",
                "Pods",
                "MedicationId");

            migrationBuilder.CreateIndex(
                "IX_Pods_UserId",
                "Pods",
                "UserId");

            migrationBuilder.CreateIndex(
                "IX_RadioEvents_RadioId",
                "RadioEvents",
                "RadioId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "MedicationDeliveries");

            migrationBuilder.DropTable(
                "PodRadioEntity");

            migrationBuilder.DropTable(
                "PodResponses");

            migrationBuilder.DropTable(
                "RadioEvents");

            migrationBuilder.DropTable(
                "PodRequests");

            migrationBuilder.DropTable(
                "Radios");

            migrationBuilder.DropTable(
                "Pods");

            migrationBuilder.DropTable(
                "Medications");

            migrationBuilder.DropTable(
                "Users");
        }
    }
}