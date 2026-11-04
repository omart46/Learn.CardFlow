using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Learn.CardFlow.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class Added_RequestResubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NeedsResubmission",
                table: "AppEmployeeCardRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NeedsResubmission",
                table: "AppEmployeeCardRequests");
        }
    }
}
