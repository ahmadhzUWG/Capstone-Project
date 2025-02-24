using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagerWebsite.Migrations
{
    /// <inheritdoc />
    public partial class Add_Projects_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ProjectLeadId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_AspNetUsers_ProjectLeadId",
                        column: x => x.ProjectLeadId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupProject",
                columns: table => new
                {
                    ProjectGroupsId = table.Column<int>(type: "int", nullable: false),
                    ProjectsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupProject", x => new { x.ProjectGroupsId, x.ProjectsId });
                    table.ForeignKey(
                        name: "FK_GroupProject_Groups_ProjectGroupsId",
                        column: x => x.ProjectGroupsId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupProject_Projects_ProjectsId",
                        column: x => x.ProjectsId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupProject_ProjectsId",
                table: "GroupProject",
                column: "ProjectsId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectLeadId",
                table: "Projects",
                column: "ProjectLeadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupProject");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
