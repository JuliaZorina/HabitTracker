using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HabitTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHabitIsSuspendedField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSuspended",
                table: "Habits",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSuspended",
                table: "Habits");
        }
    }
}
