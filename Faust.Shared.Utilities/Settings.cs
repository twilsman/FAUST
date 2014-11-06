using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Faust
{
    public class Settings
    {
        public static void OverrideTransactionManagerMaximumTimeout(TimeSpan timeout)
        {
            //TransactionScope inherits a *maximum* timeout from Machine.config.  There's no way to override it from
            //code unless you use reflection.  Hence this code!
            var type = typeof(TransactionManager);
            var cachedMaxTimeout = type.GetField("_cachedMaxTimeout", BindingFlags.NonPublic | BindingFlags.Static);
            cachedMaxTimeout.SetValue(null, true);

            var maximumTimeout = type.GetField("_maximumTimeout", BindingFlags.NonPublic | BindingFlags.Static);
            maximumTimeout.SetValue(null, timeout);

            var defaultTimeout = type.GetField("_defaultTimeout", BindingFlags.NonPublic | BindingFlags.Static);
            defaultTimeout.SetValue(null, timeout);
        }
    }
}
