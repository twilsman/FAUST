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

        public FaustDB()
            : this(DefaultUserContext("IlluminateDBConnectionStringName"))
        { }

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

        public static ConnectionStringSettings ConfigFileConnectionString(string appSettingConnectionStringName)
        {
            //just grab the connection string and provider from the config
            ConnectionStringSettingsCollection connections = ConfigurationManager.ConnectionStrings;

            if (connections.Count != 0)
            {
                string connectionStringName = ConfigurationManager.AppSettings[appSettingConnectionStringName];
                if (string.IsNullOrEmpty(connectionStringName))
                    throw new ArgumentNullException("userContext", "No connection string defined. Check your app / web config file");

                // look for the correct connection string
                foreach (ConnectionStringSettings connection in connections)
                {
                    if (string.Compare(connection.Name, connectionStringName, true) == 0)
                    {
                        return connection;
                    }
                }
                return null;
            }
            else
            {
                return null;
            }
        }
        
        public static UserContext DefaultUserContext(string appSettingConnectionStringName)
        {
            UserContext userContext = new UserContext();

            //just grab the connection string and provider from the config
            ConnectionStringSettings connectionString = ConfigFileConnectionString(appSettingConnectionStringName);

            if (connectionString == null)
                throw new ArgumentNullException("userContext", "No connection string found.");

            userContext.ConnectionString = connectionString.ConnectionString;
            userContext.ConnectionProvider = connectionString.ProviderName;

            return userContext;
        }

        protected void InitDb(UserContext userContext)
        {
            EntityConnectionStringBuilder connectionString = new EntityConnectionStringBuilder();
            connectionString.Provider = userContext.ConnectionProvider;
            connectionString.ProviderConnectionString = userContext.ConnectionString;

            Database.Connection.ConnectionString = connectionString.ProviderConnectionString;

            //dont want ef to try anything crazy like modifying the database. This will make it use the honor system and assume our data contracts are correct.
            Database.SetInitializer<FaustDB>(null);
        }
    }
}
