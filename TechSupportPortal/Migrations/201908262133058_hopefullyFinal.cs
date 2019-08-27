namespace TechSupportPortal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class hopefullyFinal : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.AgentChannels", name: "AgentId", newName: "AccountId");
            RenameIndex(table: "dbo.AgentChannels", name: "IX_AgentId", newName: "IX_AccountId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.AgentChannels", name: "IX_AccountId", newName: "IX_AgentId");
            RenameColumn(table: "dbo.AgentChannels", name: "AccountId", newName: "AgentId");
        }
    }
}
