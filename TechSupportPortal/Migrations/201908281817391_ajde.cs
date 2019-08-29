namespace TechSupportPortal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ajde : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Packs", "Amount", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Packs", "Amount");
        }
    }
}
