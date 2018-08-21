namespace RequestAutoV1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SuccessProbabilityAdd : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Results",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        URL = c.String(unicode: false),
                        StatusCode = c.Int(nullable: false),
                        SuccessProbability = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.Searches",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        URL = c.String(unicode: false),
                        StatusCode = c.Int(nullable: false),
                        CorrectionWord = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.SearchResults",
                c => new
                    {
                        Search_id = c.Int(nullable: false),
                        Result_id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Search_id, t.Result_id })
                .ForeignKey("dbo.Searches", t => t.Search_id, cascadeDelete: true)
                .ForeignKey("dbo.Results", t => t.Result_id, cascadeDelete: true)
                .Index(t => t.Search_id)
                .Index(t => t.Result_id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SearchResults", "Result_id", "dbo.Results");
            DropForeignKey("dbo.SearchResults", "Search_id", "dbo.Searches");
            DropIndex("dbo.SearchResults", new[] { "Result_id" });
            DropIndex("dbo.SearchResults", new[] { "Search_id" });
            DropTable("dbo.SearchResults");
            DropTable("dbo.Searches");
            DropTable("dbo.Results");
        }
    }
}
