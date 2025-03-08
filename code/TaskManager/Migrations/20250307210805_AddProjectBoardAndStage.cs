using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagerWebsite.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectBoardAndStage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Remove leftover references to "UserId1" in GroupManagers
            migrationBuilder.DropForeignKey(
                name: "FK_GroupManagers_AspNetUsers_UserId1",
                table: "GroupManagers"
            );

            migrationBuilder.DropIndex(
                name: "IX_GroupManagers_UserId1",
                table: "GroupManagers"
            );

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "GroupManagers"
            );

            // 2) Make ProjectCreatorId in Projects nullable
            migrationBuilder.AlterColumn<int>(
                name: "ProjectCreatorId",
                table: "Projects",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int"
            );

            // 3) Create the ProjectBoards table
            migrationBuilder.CreateTable(
                name: "ProjectBoards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectBoards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectBoards_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            // 4) Create the Stages table
            migrationBuilder.CreateTable(
                name: "Stages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    ProjectBoardId = table.Column<int>(type: "int", nullable: false),
                    CreatorGroupId = table.Column<int>(type: "int", nullable: true),
                    CreatorUserId = table.Column<int>(type: "int", nullable: false),
                    AssignedGroupId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stages_AspNetUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_Stages_Groups_AssignedGroupId",
                        column: x => x.AssignedGroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_Stages_Groups_CreatorGroupId",
                        column: x => x.CreatorGroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_Stages_ProjectBoards_ProjectBoardId",
                        column: x => x.ProjectBoardId,
                        principalTable: "ProjectBoards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            // 5) Create needed indexes
            migrationBuilder.CreateIndex(
                name: "IX_GroupRequests_GroupId",
                table: "GroupRequests",
                column: "GroupId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_GroupRequests_ProjectId",
                table: "GroupRequests",
                column: "ProjectId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ProjectBoards_ProjectId",
                table: "ProjectBoards",
                column: "ProjectId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Stages_AssignedGroupId",
                table: "Stages",
                column: "AssignedGroupId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Stages_CreatorGroupId",
                table: "Stages",
                column: "CreatorGroupId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Stages_CreatorUserId",
                table: "Stages",
                column: "CreatorUserId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Stages_ProjectBoardId",
                table: "Stages",
                column: "ProjectBoardId"
            );

            // 6) Re-link foreign keys for GroupRequests -> Groups/Projects
            migrationBuilder.AddForeignKey(
                name: "FK_GroupRequests_Groups_GroupId",
                table: "GroupRequests",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_GroupRequests_Projects_ProjectId",
                table: "GroupRequests",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            // 7) If your DB does NOT already have the FK for PrimaryManagerId,
            //    uncomment this block. Otherwise, skip it to avoid duplicates.

            /*
            migrationBuilder.AddForeignKey(
                name: "FK_Groups_AspNetUsers_PrimaryManagerId",
                table: "Groups",
                column: "PrimaryManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
            */
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse the above operations

            // 1) Drop the new tables
            migrationBuilder.DropTable(name: "Stages");
            migrationBuilder.DropTable(name: "ProjectBoards");

            // 2) Remove the newly added indexes (if EF hasn't already done so)
            // GroupRequests indexes, etc. — EF usually handles these with DropTable calls, 
            // but we can remove them if needed.

            // 3) Revert ProjectCreatorId to non-null
            migrationBuilder.AlterColumn<int>(
                name: "ProjectCreatorId",
                table: "Projects",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true
            );

            // 4) If you re-linked the foreign key for Groups -> AspNetUsers in Up, drop it here
            /*
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_AspNetUsers_PrimaryManagerId",
                table: "Groups"
            );
            */
            migrationBuilder.AddColumn<bool>(
                name: "IsPrimaryManager",
                table: "GroupManagers",
                type: "bit",
                nullable: false,
                defaultValue: false
            );
        }
    }
}
