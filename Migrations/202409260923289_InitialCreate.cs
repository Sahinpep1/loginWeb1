namespace loginWeb1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LoginAttempts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Email = c.String(nullable: false),
                        AttemptedAt = c.DateTime(nullable: false),
                        IsSuccessful = c.Boolean(nullable: false),
                        FailureReason = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserPasswords",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PasswordHash = c.String(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Username = c.String(nullable: false, maxLength: 50),
                        Email = c.String(nullable: false, maxLength: 50),
                        PasswordHash = c.String(nullable: false),
                        IsVerified = c.Boolean(nullable: false),
                        VerificationToken = c.String(),
                        PasswordLastChanged = c.DateTime(nullable: false),
                        VerificationTokenExpiry = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Email, unique: true);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserPasswords", "UserId", "dbo.Users");
            DropIndex("dbo.Users", new[] { "Email" });
            DropIndex("dbo.UserPasswords", new[] { "UserId" });
            DropTable("dbo.Users");
            DropTable("dbo.UserPasswords");
            DropTable("dbo.LoginAttempts");
        }
    }
}
