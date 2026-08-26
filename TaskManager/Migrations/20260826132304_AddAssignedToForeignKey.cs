using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedToForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_tasks_assigned_to",
                table: "tasks",
                column: "assigned_to");

            migrationBuilder.AddForeignKey(
                name: "fk_tasks_users_assigned_to",
                table: "tasks",
                column: "assigned_to",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_tasks_users_assigned_to",
                table: "tasks");

            migrationBuilder.DropIndex(
                name: "ix_tasks_assigned_to",
                table: "tasks");
        }
    }
}
