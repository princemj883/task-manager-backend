using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace task_manager_backend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUpdatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "tasks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "tasks",
                type: "timestamp without time zone",
                nullable: true);
        }
    }
}
