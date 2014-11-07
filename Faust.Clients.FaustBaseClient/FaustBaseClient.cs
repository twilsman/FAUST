using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faust.Clients
{
    public class FaustBaseClient
    {
        public static void RunMigrations(string[] args)
        {
            if (args.Length > 0)
            {

                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings[args[0]]))
                {
                    UnityCache.Resolve<IFaustManager>().RunMigration(args[0]);
                }
                else
                {
                    Console.WriteLine("##teamcity[message text='Specified database does not exist in App.config' errorDetails='' status='ERROR']");
                    Console.WriteLine("##teamcity[buildProblem description='FAUST Failed.  See build log for details.']");
                }
            }
            else
            {
                UnityCache.Resolve<IFaustManager>().RunMigration(ConfigurationManager.AppSettings["DefaultDatabase"]);
            }
        }
    }
}
