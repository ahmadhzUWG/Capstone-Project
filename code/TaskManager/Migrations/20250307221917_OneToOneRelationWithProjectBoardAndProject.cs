using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagerWebsite.Migrations
{
    /// <inheritdoc />
    public partial class OneToOneRelationWithProjectBoardAndProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectBoards_ProjectId",
                table: "ProjectBoards");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectBoards_ProjectId",
                table: "ProjectBoards",
                column: "ProjectId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectBoards_ProjectId",
                table: "ProjectBoards");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectBoards_ProjectId",
                table: "ProjectBoards",
                column: "ProjectId");
        }
    }
}
