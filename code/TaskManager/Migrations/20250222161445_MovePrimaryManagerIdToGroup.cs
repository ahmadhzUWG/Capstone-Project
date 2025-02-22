using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagerWebsite.Migrations
{
    /// <inheritdoc />
    public partial class MovePrimaryManagerIdToGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPrimaryManager",
                table: "GroupManagers");

            migrationBuilder.AddColumn<int>(
                name: "PrimaryManagerId",
                table: "Groups",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "GroupManagers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_PrimaryManagerId",
                table: "Groups",
                column: "PrimaryManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupManagers_UserId1",
                table: "GroupManagers",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupManagers_AspNetUsers_UserId1",
                table: "GroupManagers",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_AspNetUsers_PrimaryManagerId",
                table: "Groups",
                column: "PrimaryManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupManagers_AspNetUsers_UserId1",
                table: "GroupManagers");

            migrationBuilder.DropForeignKey(
                name: "FK_Groups_AspNetUsers_PrimaryManagerId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_PrimaryManagerId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_GroupManagers_UserId1",
                table: "GroupManagers");

            migrationBuilder.DropColumn(
                name: "PrimaryManagerId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "GroupManagers");

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimaryManager",
                table: "GroupManagers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
