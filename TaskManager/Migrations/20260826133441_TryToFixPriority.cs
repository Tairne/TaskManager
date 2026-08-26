using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Migrations
{
    /// <inheritdoc />
    public partial class TryToFixPriority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "priority",
                table: "tasks",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "priority",
                table: "tasks",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
