using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Learn.CardFlow.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class Added_ProductionBatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProductionBatchId",
                table: "AppEmployeeCardRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppProductionBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_AppProductionBatches", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppEmployeeCardRequests_ProductionBatchId",
                table: "AppEmployeeCardRequests",
                column: "ProductionBatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppEmployeeCardRequests_AppProductionBatches_ProductionBatchId",
                table: "AppEmployeeCardRequests",
                column: "ProductionBatchId",
                principalTable: "AppProductionBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppEmployeeCardRequests_AppProductionBatches_ProductionBatchId",
                table: "AppEmployeeCardRequests");

            migrationBuilder.DropTable(
                name: "AppProductionBatches");

            migrationBuilder.DropIndex(
                name: "IX_AppEmployeeCardRequests_ProductionBatchId",
                table: "AppEmployeeCardRequests");

            migrationBuilder.DropColumn(
                name: "ProductionBatchId",
                table: "AppEmployeeCardRequests");
        }
    }
}
