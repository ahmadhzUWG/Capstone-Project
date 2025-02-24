using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagerWebsite.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectGroupRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupProject");

            migrationBuilder.CreateTable(
                name: "GroupProjects",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupProjects", x => new { x.ProjectId, x.GroupId });
                    table.ForeignKey(
                        name: "FK_GroupProjects_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupProjects_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupProjects_GroupId",
                table: "GroupProjects",
                column: "GroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupProjects");

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
        }
    }
}
