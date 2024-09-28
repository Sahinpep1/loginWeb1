using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace loginWeb1.Data
{
    using loginWeb1.Models;
    using System.Data.Entity;

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("DefaultConnection")
        {
        }

        // DbSets representing the tables in the database
        public DbSet<User> Users { get; set; }
        public DbSet<UserPassword> UserPasswords { get; set; }
        public DbSet<LoginAttempt> LoginAttempts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Configuration of model properties
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
            modelBuilder.Entity<LoginAttempt>().ToTable("LoginAttempts");
        }
    }

}