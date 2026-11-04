using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Learn.CardFlow.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class Added_CardQualityChecks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppCardQualityChecks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeCardRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductionBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_AppCardQualityChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppCardQualityChecks_AppEmployeeCardRequests_EmployeeCardRequestId",
                        column: x => x.EmployeeCardRequestId,
                        principalTable: "AppEmployeeCardRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppCardQualityChecks_AppProductionBatches_ProductionBatchId",
                        column: x => x.ProductionBatchId,
                        principalTable: "AppProductionBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppCardQualityChecks_EmployeeCardRequestId_ProductionBatchId",
                table: "AppCardQualityChecks",
                columns: new[] { "EmployeeCardRequestId", "ProductionBatchId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppCardQualityChecks_ProductionBatchId",
                table: "AppCardQualityChecks",
                column: "ProductionBatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppCardQualityChecks");
        }
    }
}
