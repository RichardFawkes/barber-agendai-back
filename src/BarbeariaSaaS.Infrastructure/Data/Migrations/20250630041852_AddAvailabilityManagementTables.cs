using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarbeariaSaaS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAvailabilityManagementTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BusinessBreaks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "TIME", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "TIME", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AppliesToAllDays = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessBreaks", x => x.Id);
                    table.CheckConstraint("CK_BusinessBreak_Time", "StartTime < EndTime");
                    table.ForeignKey(
                        name: "FK_BusinessBreaks_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ManualBlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "DATE", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "TIME", nullable: true),
                    EndTime = table.Column<TimeSpan>(type: "TIME", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManualBlocks", x => x.Id);
                    table.CheckConstraint("CK_ManualBlock_Type", "(Type = 2) OR (Type = 1 AND StartTime IS NOT NULL AND EndTime IS NOT NULL AND StartTime < EndTime)");
                    table.ForeignKey(
                        name: "FK_ManualBlocks_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ManualBlocks_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SpecialDays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "DATE", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsOpen = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CustomStartTime = table.Column<TimeSpan>(type: "TIME", nullable: true),
                    CustomEndTime = table.Column<TimeSpan>(type: "TIME", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialDays", x => x.Id);
                    table.CheckConstraint("CK_SpecialDay_Hours", "(IsOpen = 0) OR (IsOpen = 1 AND CustomStartTime IS NOT NULL AND CustomEndTime IS NOT NULL AND CustomStartTime < CustomEndTime)");
                    table.ForeignKey(
                        name: "FK_SpecialDays_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SlotDurationMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    AdvanceBookingDays = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    MaxBookingsPerDay = table.Column<int>(type: "int", nullable: false, defaultValue: 50),
                    BookingBufferMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Timezone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "America/Sao_Paulo"),
                    AutoConfirmBookings = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantSettings", x => x.Id);
                    table.CheckConstraint("CK_TenantSetting_AdvanceDays", "AdvanceBookingDays > 0");
                    table.CheckConstraint("CK_TenantSetting_Buffer", "BookingBufferMinutes >= 0");
                    table.CheckConstraint("CK_TenantSetting_MaxBookings", "MaxBookingsPerDay > 0");
                    table.CheckConstraint("CK_TenantSetting_SlotDuration", "SlotDurationMinutes > 0");
                    table.ForeignKey(
                        name: "FK_TenantSettings_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessBreaks_TenantId",
                table: "BusinessBreaks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ManualBlocks_CreatedBy",
                table: "ManualBlocks",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ManualBlocks_TenantId",
                table: "ManualBlocks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ManualBlocks_TenantId_Date",
                table: "ManualBlocks",
                columns: new[] { "TenantId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_SpecialDays_Date",
                table: "SpecialDays",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialDays_TenantId_Date",
                table: "SpecialDays",
                columns: new[] { "TenantId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantSettings_TenantId",
                table: "TenantSettings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessBreaks");

            migrationBuilder.DropTable(
                name: "ManualBlocks");

            migrationBuilder.DropTable(
                name: "SpecialDays");

            migrationBuilder.DropTable(
                name: "TenantSettings");
        }
    }
}
