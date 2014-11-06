using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faust
{
    class Program
    {
        static void Main(string[] args)
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
