using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Learn.CardFlow.Migrations.SqlServer
{
    public partial class Added_RejectionReason : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "AppEmployeeCardRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "AppEmployeeCardRequests");
        }
    }
}