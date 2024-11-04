using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HabitTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationsDataNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HabitsNotificationSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HabitId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsSending = table.Column<bool>(type: "boolean", nullable: false),
                    CountOfNotifications = table.Column<int>(type: "integer", nullable: false),
                    NotificationTime = table.Column<List<TimeOnly>>(type: "time without time zone[]", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TimeStart = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    TimeEnd = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HabitsNotificationSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HabitsNotificationSettings_Habits_HabitId",
                        column: x => x.HabitId,
                        principalTable: "Habits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HabitsNotificationSettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HabitsNotificationSettings_HabitId",
                table: "HabitsNotificationSettings",
                column: "HabitId");

            migrationBuilder.CreateIndex(
                name: "IX_HabitsNotificationSettings_UserId",
                table: "HabitsNotificationSettings",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HabitsNotificationSettings");
        }
    }
}
