using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessBookingWindow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxAdvanceBookingDays",
                table: "Businesses",
                type: "int",
                nullable: false,
                defaultValue: 60);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Businesses_MaxAdvanceBookingDays_Range",
                table: "Businesses",
                sql: "[MaxAdvanceBookingDays] BETWEEN 1 AND 365");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Businesses_MaxAdvanceBookingDays_Range",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "MaxAdvanceBookingDays",
                table: "Businesses");
        }
    }
}
