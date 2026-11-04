using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Learn.CardFlow.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class Added_CardDeliveries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppCardDeliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeCardRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CardQualityCheckId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecipientName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveredById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppCardDeliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppCardDeliveries_AppCardQualityChecks_CardQualityCheckId",
                        column: x => x.CardQualityCheckId,
                        principalTable: "AppCardQualityChecks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppCardDeliveries_AppEmployeeCardRequests_EmployeeCardRequestId",
                        column: x => x.EmployeeCardRequestId,
                        principalTable: "AppEmployeeCardRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppCardDeliveries_CardQualityCheckId",
                table: "AppCardDeliveries",
                column: "CardQualityCheckId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppCardDeliveries_EmployeeCardRequestId",
                table: "AppCardDeliveries",
                column: "EmployeeCardRequestId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppCardDeliveries");
        }
    }
}
