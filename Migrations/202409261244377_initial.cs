namespace loginWeb1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "confirmPassword", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "confirmPassword");
        }
    }
}
