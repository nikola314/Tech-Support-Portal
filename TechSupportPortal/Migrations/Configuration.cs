namespace TechSupportPortal.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<TechSupportPortal.MyDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(TechSupportPortal.MyDbContext db)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
            db.Categories.Add(new Models.Category { Name = "Java" });
            db.Categories.Add(new Models.Category { Name = "C++" });
            db.Categories.Add(new Models.Category { Name = "C#" });
            db.Categories.Add(new Models.Category { Name = "Javascript" });
            db.Categories.Add(new Models.Category { Name = "Python" });
            db.Categories.Add(new Models.Category { Name = "Ruby" });
            db.Accounts.Add(new Models.Account
            {
                FirstName = "Nikola",
                LastName = "Kesic",
                Role = Models.AccountRole.Admin,
                Password = "0ROGFrVnBi9mD8+C24ZbIBSjekyInDY6MkmMBzgJsRVHXDln",
                ConfirmPassword = "0ROGFrVnBi9mD8+C24ZbIBSjekyInDY6MkmMBzgJsRVHXDln",
                Mail = "n@k.c",
                Status = Models.AccountStatus.Active,
                Tokens = 0,
                Username = "nikola314"
            });
            db.Accounts.Add(new Models.Account
            {
                FirstName = "Milorad",
                LastName = "Vulic",
                Role = Models.AccountRole.Agent,
                Password = "jgYIDyqNrWGKfajq4DDSfzAGVN7CVb+v8MoxDFOq7+WePMvd",
                ConfirmPassword = "jgYIDyqNrWGKfajq4DDSfzAGVN7CVb+v8MoxDFOq7+WePMvd",
                Mail = "m@k.c",
                Status = Models.AccountStatus.Active,
                Tokens = 0,
                Username = "mickoni17"
            });
            db.Accounts.Add(new Models.Account
            {
                FirstName = "Dimitrije",
                LastName = "Milenkovic",
                Role = Models.AccountRole.Client,
                Password = "tQXWCho8fXDhGcsaTj+yXbxzCwmoU48UYNNImTBom+1vBKs7",
                ConfirmPassword = "tQXWCho8fXDhGcsaTj+yXbxzCwmoU48UYNNImTBom+1vBKs7",
                Mail = "d@k.c",
                Status = Models.AccountStatus.Active,
                Tokens = 0,
                Username = "dimec"
            });
            db.Packs.Add(new Models.Pack { packName = Models.TokenPack.S, Price = 10, Amount = 10 });
            db.Packs.Add(new Models.Pack { packName = Models.TokenPack.G, Price = 30 , Amount = 30});
            db.Packs.Add(new Models.Pack { packName = Models.TokenPack.P, Price = 100, Amount = 100 });
        }
    }
}
