using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Faust
{
    [DataContract]
    public class UserContext
    {
        [DataMember]
        public string ConnectionString { get; set; }
        [DataMember]
        public string ConnectionProvider { get; set; }
        [DataMember]
        public int UserID { get; set; }

        public static UserContext GetUserContext(string appSettingConnectionStringName)
        {
            UserContext userContext = new UserContext();

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
                        userContext.ConnectionProvider = connection.ProviderName;
                        userContext.ConnectionString = connection.ConnectionString;
                    }
                }
            }

            return userContext;
        }
    }
}
