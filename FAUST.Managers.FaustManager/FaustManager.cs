using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Faust.Managers
{
    public class FaustManager : IFaustManager
    {
        public void RunMigration(string databaseName)
        {
            Logger.Log(string.Format("FaustManager.RunMigration:  Running Migrations for database {0}.", databaseName), TraceEventType.Verbose);
            Console.WriteLine("##teamcity[message text='FAUSTing Migrations']");

            UserContext userContext = UserContext.GetUserContext(databaseName);

            UnityCache.Resolve<IFaustMigrationsHistoryAccessor>().Initialize(userContext);

            List<DirectoryInfo> releaseDirs = Directory.GetDirectories(ConfigurationManager.AppSettings["MigrationsDirectory"] + databaseName)
                                                   .Select(dir => new DirectoryInfo(dir))
                                                   .Where(dir => int.Parse(dir.Name) > 0)
                                                   .OrderBy(dir => int.Parse(dir.Name))
                                                   .ToList();

            if (ConfigurationManager.AppSettings["RunDebugMigrations"] == "true")
            {
                List<DirectoryInfo> holdingDirs = Directory.GetDirectories(ConfigurationManager.AppSettings["MigrationsDirectory"] + databaseName)
                                                   .Select(dir => new DirectoryInfo(dir))
                                                   .Where(dir => int.Parse(dir.Name) < 0)
                                                   .OrderByDescending(dir => int.Parse(dir.Name))
                                                   .ToList();
                holdingDirs.ForEach(d => releaseDirs.Add(d));

                UnityCache.Resolve<IFaustMigrationsHistoryAccessor>().DeleteDebugEntries(userContext);
            }

            foreach (DirectoryInfo release in releaseDirs)
            {
                FaustMigrationScript[] scripts = UnityCache.Resolve<IFaustEngine>().GetReleaseMigrationScripts(release, userContext);

                List<FaustMigrationHistory> migrationHistories = new List<FaustMigrationHistory>();

                TransactionScopeOption scopeOption = TransactionScopeOption.Required;

                if (release.EnumerateFiles().Any(file => file.Name == "no.transaction"))
                {
                    scopeOption = TransactionScopeOption.Suppress;
                }
                TimeSpan desiredTimeout = TimeSpan.FromSeconds(int.Parse(ConfigurationManager.AppSettings["DefaultCommandTimeoutSecondsOverride"]));

                Settings.OverrideTransactionManagerMaximumTimeout(desiredTimeout);
                using (TransactionScope releaseScope = new TransactionScope(scopeOption, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
                {
                    foreach (FaustMigrationScript script in scripts)
                    {
                        FaustMigrationHistory migrationHistory = UnityCache.Resolve<IFaustEngine>().RunMigrationScript(script, userContext);

                        migrationHistories.Add(migrationHistory);

                        if (!migrationHistory.Successful ?? false)
                        {
                            releaseScope.Dispose();
                            migrationHistories.ForEach(m => m.Committed = false);
                            Console.WriteLine(string.Format("##teamcity[message text='{0}' errorDetails='' status='ERROR']", migrationHistory.Log));
                            Console.WriteLine("##teamcity[buildProblem description='FAUST Failed.  See build log for details.']");
                            Logger.Log(string.Format("FaustManager.RunMigration: {0}", migrationHistory.Log), TraceEventType.Error);
                            break;
                        }
                    }

                    if (Transaction.Current != null)
                    {
                        releaseScope.Complete();
                        Console.WriteLine("##teamcity[message text='FAUSTing completed without errors.']");
                    }

                    UnityCache.Resolve<IFaustEngine>().SaveMigrationsHistory(migrationHistories.ToArray(), userContext);
                }
            }
        }
    }
}
