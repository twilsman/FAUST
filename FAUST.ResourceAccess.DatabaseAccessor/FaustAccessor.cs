using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faust.ResourceAccess
{
    public class FaustAccessor : IFaustAccessor
    {
        public int ExecuteSqlCommand(string sqlCommand, UserContext userContext)
        {
            using (var db = new DbContext(userContext.ConnectionString))
            {
                var commandTimeout = ConfigurationManager.AppSettings["DefaultCommandTimeoutSecondsOverride"] as string;
                if (!string.IsNullOrWhiteSpace(commandTimeout))
                {
                    ((IObjectContextAdapter)db).ObjectContext.CommandTimeout = Convert.ToInt32(commandTimeout);
                }

                return db.Database.ExecuteSqlCommand(sqlCommand);
            }
        }
    }
}
