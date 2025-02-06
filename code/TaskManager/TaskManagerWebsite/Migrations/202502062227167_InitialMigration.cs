namespace TaskManagerWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialMigration : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Users", newName: "User");
            DropColumn("dbo.User", "PasswordHash");
        }
        
        public override void Down()
        {
            AddColumn("dbo.User", "PasswordHash", c => c.String(nullable: false));
            RenameTable(name: "dbo.User", newName: "Users");
        }
    }
}
