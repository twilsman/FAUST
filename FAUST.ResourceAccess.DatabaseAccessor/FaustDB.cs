using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faust.ResourceAccess
{
    public class FaustDB : DbContext
    {
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FaustMigrationHistory>().ToTable("FaustMigrationsHistory");
            modelBuilder.Entity<FaustMigrationHistory>().HasKey(k => k.FaustMigrationHistoryId);
        }

        public FaustDB(UserContext userContext)
        {
            InitDb(userContext);

            // Allow app config to override the default EF command timeout
            var commandTimeout = ConfigurationManager.AppSettings["DefaultCommandTimeoutSecondsOverride"] as string;
            if (!string.IsNullOrWhiteSpace(commandTimeout))
            {
                ((IObjectContextAdapter)this).ObjectContext.CommandTimeout = Convert.ToInt32(commandTimeout);
            }

        }

        public DbSet<FaustMigrationHistory> FaustMigrationsHistory { get; set; }

        protected void InitDb(UserContext userContext)
        {
            EntityConnectionStringBuilder connectionString = new EntityConnectionStringBuilder();
            connectionString.Provider = userContext.ConnectionProvider;
            connectionString.ProviderConnectionString = userContext.ConnectionString;

            Database.Connection.ConnectionString = connectionString.ProviderConnectionString;

            Database.SetInitializer<FaustDB>(null);
        }
    }
}
