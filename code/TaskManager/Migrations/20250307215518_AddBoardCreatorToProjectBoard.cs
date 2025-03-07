using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagerWebsite.Migrations
{
    /// <inheritdoc />
    public partial class AddBoardCreatorToProjectBoard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BoardCreatorId",
                table: "ProjectBoards",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectBoards_BoardCreatorId",
                table: "ProjectBoards",
                column: "BoardCreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectBoards_AspNetUsers_BoardCreatorId",
                table: "ProjectBoards",
                column: "BoardCreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectBoards_AspNetUsers_BoardCreatorId",
                table: "ProjectBoards");

            migrationBuilder.DropIndex(
                name: "IX_ProjectBoards_BoardCreatorId",
                table: "ProjectBoards");

            migrationBuilder.DropColumn(
                name: "BoardCreatorId",
                table: "ProjectBoards");
        }
    }
}
