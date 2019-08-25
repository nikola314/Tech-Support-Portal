namespace TechSupportPortal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Accounts",
                c => new
                    {
                        AccountId = c.Int(nullable: false, identity: true),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Mail = c.String(),
                        Username = c.String(),
                        Password = c.String(),
                        Tokens = c.Int(nullable: false),
                        Role = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.AccountId);
            
            CreateTable(
                "dbo.Channels",
                c => new
                    {
                        ChannelId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        AccountId = c.Int(nullable: false),
                        IsOpen = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ChannelId)
                .ForeignKey("dbo.Accounts", t => t.AccountId, cascadeDelete: false)
                .Index(t => t.AccountId);
            
            CreateTable(
                "dbo.Actions",
                c => new
                    {
                        ActionId = c.Int(nullable: false, identity: true),
                        Text = c.String(),
                        AccountId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(),
                        Title = c.String(),
                        Image = c.Binary(),
                        CategoryId = c.Int(),
                        IsLocked = c.Boolean(),
                        LastLockedAt = c.DateTime(),
                        ActionRespToId = c.Int(),
                        Upvotes = c.Int(),
                        Downvotes = c.Int(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.ActionId)
                .ForeignKey("dbo.Categories", t => t.CategoryId, cascadeDelete: false)
                .ForeignKey("dbo.Accounts", t => t.AccountId, cascadeDelete: false)
                .ForeignKey("dbo.Actions", t => t.ActionRespToId, cascadeDelete: true)
                .Index(t => t.AccountId)
                .Index(t => t.CategoryId)
                .Index(t => t.ActionRespToId);
            
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        CategoryId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.CategoryId);
            
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        OrderId = c.Int(nullable: false, identity: true),
                        AccountId = c.Int(nullable: false),
                        TokenPack = c.Int(nullable: false),
                        Quantity = c.Int(nullable: false),
                        Price = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.OrderId)
                .ForeignKey("dbo.Accounts", t => t.AccountId, cascadeDelete: true)
                .Index(t => t.AccountId);
            
            CreateTable(
                "dbo.QuestionChannels",
                c => new
                    {
                        QuestionId = c.Int(nullable: false),
                        ChannelId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.QuestionId, t.ChannelId })
                .ForeignKey("dbo.Actions", t => t.QuestionId, cascadeDelete: false)
                .ForeignKey("dbo.Channels", t => t.ChannelId, cascadeDelete: false)
                .Index(t => t.QuestionId)
                .Index(t => t.ChannelId);
            
            CreateTable(
                "dbo.AgentChannels",
                c => new
                    {
                        AgentId = c.Int(nullable: false),
                        ChannelId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.AgentId, t.ChannelId })
                .ForeignKey("dbo.Accounts", t => t.AgentId, cascadeDelete: true)
                .ForeignKey("dbo.Channels", t => t.ChannelId, cascadeDelete: true)
                .Index(t => t.AgentId)
                .Index(t => t.ChannelId);
            
            CreateTable(
                "dbo.Votes",
                c => new
                    {
                        AccountId = c.Int(nullable: false),
                        ResponseId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.AccountId, t.ResponseId })
                .ForeignKey("dbo.Accounts", t => t.AccountId, cascadeDelete: false)
                .ForeignKey("dbo.Actions", t => t.ResponseId, cascadeDelete: false)
                .Index(t => t.AccountId)
                .Index(t => t.ResponseId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Votes", "ResponseId", "dbo.Actions");
            DropForeignKey("dbo.Votes", "AccountId", "dbo.Accounts");
            DropForeignKey("dbo.Orders", "AccountId", "dbo.Accounts");
            DropForeignKey("dbo.AgentChannels", "ChannelId", "dbo.Channels");
            DropForeignKey("dbo.AgentChannels", "AgentId", "dbo.Accounts");
            DropForeignKey("dbo.Actions", "ActionRespToId", "dbo.Actions");
            DropForeignKey("dbo.Actions", "AccountId", "dbo.Accounts");
            DropForeignKey("dbo.QuestionChannels", "ChannelId", "dbo.Channels");
            DropForeignKey("dbo.QuestionChannels", "QuestionId", "dbo.Actions");
            DropForeignKey("dbo.Actions", "CategoryId", "dbo.Categories");
            DropForeignKey("dbo.Channels", "AccountId", "dbo.Accounts");
            DropIndex("dbo.Votes", new[] { "ResponseId" });
            DropIndex("dbo.Votes", new[] { "AccountId" });
            DropIndex("dbo.AgentChannels", new[] { "ChannelId" });
            DropIndex("dbo.AgentChannels", new[] { "AgentId" });
            DropIndex("dbo.QuestionChannels", new[] { "ChannelId" });
            DropIndex("dbo.QuestionChannels", new[] { "QuestionId" });
            DropIndex("dbo.Orders", new[] { "AccountId" });
            DropIndex("dbo.Actions", new[] { "ActionRespToId" });
            DropIndex("dbo.Actions", new[] { "CategoryId" });
            DropIndex("dbo.Actions", new[] { "AccountId" });
            DropIndex("dbo.Channels", new[] { "AccountId" });
            DropTable("dbo.Votes");
            DropTable("dbo.AgentChannels");
            DropTable("dbo.QuestionChannels");
            DropTable("dbo.Orders");
            DropTable("dbo.Categories");
            DropTable("dbo.Actions");
            DropTable("dbo.Channels");
            DropTable("dbo.Accounts");
        }
    }
}
