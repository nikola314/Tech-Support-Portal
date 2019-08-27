namespace TechSupportPortal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class cmon : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Packs",
                c => new
                    {
                        PackId = c.Int(nullable: false, identity: true),
                        Price = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.PackId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Packs");
        }
    }
}
