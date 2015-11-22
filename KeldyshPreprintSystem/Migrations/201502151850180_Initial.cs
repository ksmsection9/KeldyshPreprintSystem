namespace KeldyshPreprintSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Authors",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PaperSubmissionModelId = c.Int(nullable: false),
                        FirstnameRussian = c.String(nullable: false),
                        LastnameRussian = c.String(nullable: false),
                        PatronymRussian = c.String(),
                        FirstnameEnglish = c.String(nullable: false),
                        LastnameEnglish = c.String(nullable: false),
                        PatronymEnglish = c.String(),
                        Email = c.String(nullable: false),
                        PersonalWeb = c.String(),
                        PlaceOfWork = c.String(),
                        SPIN = c.String(),
                        ResearcherID = c.String(),
                        ORCID = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PaperSubmissionModels", t => t.PaperSubmissionModelId, cascadeDelete: true)
                .Index(t => t.PaperSubmissionModelId);
            
            CreateTable(
                "dbo.PaperSubmissionModels",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        authorsIndex = c.String(),
                        TitleRussian = c.String(nullable: false, maxLength: 200),
                        TitleEnglish = c.String(nullable: false, maxLength: 200),
                        AbstractRussian = c.String(nullable: false),
                        AbstractEnglish = c.String(nullable: false),
                        KeywordsRussian = c.String(nullable: false),
                        KeywordsEnglish = c.String(nullable: false),
                        Languages = c.String(nullable: false),
                        NumberOfPages = c.Int(nullable: false),
                        NumberOfAuthorsCopies = c.Int(nullable: false),
                        FieldOfResearch = c.Int(nullable: false),
                        UDK = c.String(maxLength: 100),
                        Bibliography = c.String(nullable: false),
                        FinancialSupport = c.String(),
                        ContactName = c.String(nullable: false),
                        ContactPhone = c.String(nullable: false),
                        Review = c.String(nullable: false, maxLength: 3000),
                        submissionState = c.Int(nullable: false),
                        submissionDate = c.String(),
                        lastStatusChangeDate = c.String(),
                        Owner = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Reviewers", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Reviewers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PaperSubmissionModelId = c.Int(nullable: false),
                        Firstname = c.String(nullable: false),
                        Lastname = c.String(nullable: false),
                        Patronym = c.String(),
                        JobTitle = c.String(nullable: false),
                        Degree = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PaperSubmissionModels", "Id", "dbo.Reviewers");
            DropForeignKey("dbo.Authors", "PaperSubmissionModelId", "dbo.PaperSubmissionModels");
            DropIndex("dbo.PaperSubmissionModels", new[] { "Id" });
            DropIndex("dbo.Authors", new[] { "PaperSubmissionModelId" });
            DropTable("dbo.Reviewers");
            DropTable("dbo.PaperSubmissionModels");
            DropTable("dbo.Authors");
        }
    }
}
