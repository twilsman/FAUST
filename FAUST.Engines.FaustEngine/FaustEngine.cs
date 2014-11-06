using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;

namespace Faust.Engines
{
    public class FaustEngine : IFaustEngine
    {
        public FaustMigrationScript[] GetReleaseMigrationScripts(DirectoryInfo release, UserContext userContext)
        {
            Logger.Log(string.Format("FaustEngine.GetReleaseMigrationScripts:  Getting migrationscripts for release {0}.", release.Name), TraceEventType.Verbose);

            IEnumerable<FileInfo> files = release.EnumerateFiles().Where(file => file.Extension == ".sql");

            return files.Select(file => new FaustMigrationScript
                                {
                                    ReleaseNumber = int.Parse(file.Directory.Name),
                                    ScriptName = file.Name,
                                    FilePath = file.FullName,
                                    Successful = null,
                                    Commands = GetSqlCommands(file),
                                })
                        .Where(script => UnityCache.Resolve<IFaustMigrationsHistoryAccessor>()
                                                .FindMany(new FaustMigrationHistory
                                                {
                                                    ReleaseNumber = script.ReleaseNumber,
                                                    ScriptName = script.ScriptName,
                                                    Committed = true,
                                                }, userContext).Length == 0)
                        .OrderBy(s => GetScriptOrdinal(s.ScriptName))
                        .ToArray();
        }

        public FaustMigrationHistory RunMigrationScript(FaustMigrationScript script, UserContext userContext)
        {
            Logger.Log(string.Format("FaustEngine.RunMigrationScript: Running commands for script {0}.", script.ScriptName), TraceEventType.Verbose);

            FaustMigrationHistory migrationHistory = new FaustMigrationHistory();

            try
            {
                migrationHistory.ReleaseNumber = script.ReleaseNumber;
                migrationHistory.ScriptName = script.ScriptName;
                migrationHistory.Committed = true;
                migrationHistory.Successful = true;
                migrationHistory.LastRun = DateTime.Now;

                foreach (string sqlCommand in script.Commands)
                {
                    migrationHistory.Log = sqlCommand;

                    UnityCache.Resolve<IFaustAccessor>().ExecuteSqlCommand(sqlCommand, userContext);
                }

                return migrationHistory;
            }
            catch (Exception ex)
            {
                migrationHistory.Successful = false;
                migrationHistory.Log = string.Format("Error: {0}\r\nCommand: {1}", ex.Message, migrationHistory.Log);

                Logger.Log(string.Format("FaustEngine.RunMigrationScript: {0}", migrationHistory.Log), TraceEventType.Error);

                return migrationHistory;
            }
        }

        public void SaveMigrationsHistory(FaustMigrationHistory[] migrationHistories, UserContext userContext)
        {
            Logger.Log(string.Format("FaustEngine.SaveMigrationsHistory: Saving History for {0} scripts.", migrationHistories.Length), TraceEventType.Verbose);

            foreach (FaustMigrationHistory migration in migrationHistories)
            {
                FaustMigrationHistory searchCriteria = new FaustMigrationHistory
                {
                    ReleaseNumber = migration.ReleaseNumber,
                    ScriptName = migration.ScriptName
                };

                using (TransactionScope loggingScope = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    FaustMigrationHistory existingHistory = UnityCache.Resolve<IFaustMigrationsHistoryAccessor>()
                                                               .FindMany(searchCriteria, userContext)
                                                               .FirstOrDefault();
                    if (existingHistory == null)
                    {
                        UnityCache.Resolve<IFaustMigrationsHistoryAccessor>().Create(migration, userContext);
                    }
                    else
                    {
                        existingHistory.LastRun = migration.LastRun;
                        existingHistory.Committed = migration.Committed;
                        existingHistory.Successful = migration.Successful;
                        existingHistory.Log = migration.Log;

                        UnityCache.Resolve<IFaustMigrationsHistoryAccessor>().Update(existingHistory, userContext);
                    }

                    loggingScope.Complete();
                }
            }
        }

        [ExcludeFromCodeCoverage]
        private int GetScriptOrdinal(string scriptName)
        {
            string[] parts = scriptName.Split(new char[] { '.' });
            return int.Parse(parts[0]);
        }

        [ExcludeFromCodeCoverage]
        private string[] GetSqlCommands(FileInfo scriptFile)
        {
            StreamReader streamReader = new StreamReader(scriptFile.FullName);
            string scriptFileContents = streamReader.ReadToEnd();
            streamReader.Close();

            Regex regex = new Regex(@"\bgo\b", RegexOptions.IgnoreCase);

            string[] matches = Regex.Split(scriptFileContents, @"\bgo\b", RegexOptions.IgnoreCase);

            return matches.Select(s => s.Trim()).Where(m => !string.IsNullOrWhiteSpace(m)).ToArray();
        }
    }
}
