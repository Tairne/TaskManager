using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Migrations
{
    /// <inheritdoc />
    public partial class AddPositionToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "role",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "User",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValueSql: "User");

            migrationBuilder.AddColumn<string>(
                name: "position",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "priority",
                table: "tasks",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "position",
                table: "users");

            migrationBuilder.AlterColumn<string>(
                name: "role",
                table: "users",
                type: "text",
                nullable: false,
                defaultValueSql: "User",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "User");

            migrationBuilder.AlterColumn<int>(
                name: "priority",
                table: "tasks",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);
        }
    }
}
