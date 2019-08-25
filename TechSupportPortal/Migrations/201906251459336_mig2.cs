namespace TechSupportPortal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class mig2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Accounts", "FirstName", c => c.String(nullable: false));
            AlterColumn("dbo.Accounts", "LastName", c => c.String(nullable: false));
            AlterColumn("dbo.Accounts", "Mail", c => c.String(nullable: false));
            AlterColumn("dbo.Accounts", "Username", c => c.String(nullable: false));
            AlterColumn("dbo.Accounts", "Password", c => c.String(nullable: false));
            AlterColumn("dbo.Channels", "CreatedAt", c => c.DateTime());
            AlterColumn("dbo.Categories", "Name", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Categories", "Name", c => c.String());
            AlterColumn("dbo.Channels", "CreatedAt", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Accounts", "Password", c => c.String());
            AlterColumn("dbo.Accounts", "Username", c => c.String());
            AlterColumn("dbo.Accounts", "Mail", c => c.String());
            AlterColumn("dbo.Accounts", "LastName", c => c.String());
            AlterColumn("dbo.Accounts", "FirstName", c => c.String());
        }
    }
}
