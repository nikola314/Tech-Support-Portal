
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;
using TechSupportPortal.Models;

namespace TechSupportPortal
{
    public class MyDbContext: DbContext
    {

        public DbSet<Action> Actions { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Pack> Packs { get; set; }

        public MyDbContext() : base("myContext")
        {
            Database.SetInitializer<MyDbContext>(new DropCreateDatabaseIfModelChanges<MyDbContext>());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Entity<Account>()
               .HasMany<Channel>(s => s.AgentChannels)
               .WithMany(c => c.Agents)
               .Map(cs =>
               {
                   cs.MapLeftKey("AccountId");
                   cs.MapRightKey("ChannelId");
                   cs.ToTable("AgentChannels");
               });
            modelBuilder.Entity<Account>()
               .HasMany<Response>(s => s.Votes)
               .WithMany(c => c.Voters)
               .Map(cs =>
               {
                   cs.MapLeftKey("AccountId");
                   cs.MapRightKey("ResponseId");
                   cs.ToTable("Votes");
               });
            modelBuilder.Entity<Question>()
               .HasMany<Channel>(s => s.Channels)
               .WithMany(c => c.Questions)
               .Map(cs =>
               {
                   cs.MapLeftKey("QuestionId");
                   cs.MapRightKey("ChannelId");
                   cs.ToTable("QuestionChannels");
               });

        }
    }
}