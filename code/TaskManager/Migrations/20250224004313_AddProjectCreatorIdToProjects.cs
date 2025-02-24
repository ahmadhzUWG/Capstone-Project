using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagerWebsite.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectCreatorIdToProjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectCreatorId",
                table: "Projects",
                type: "int",
                nullable: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_AspNetUsers_ProjectCreatorId",  
                table: "Projects",                              
                column: "ProjectCreatorId",                    
                principalTable: "AspNetUsers",                  
                principalColumn: "Id",                          
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectCreatorId",
                table: "Projects");
        }
    }
}
