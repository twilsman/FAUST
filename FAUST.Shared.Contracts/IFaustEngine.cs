using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faust
{
    public interface IFaustEngine
    {
        FaustMigrationScript[] GetReleaseMigrationScripts(DirectoryInfo release, UserContext userContext);

        FaustMigrationHistory RunMigrationScript(FaustMigrationScript script, UserContext userContext);

        void SaveMigrationsHistory(FaustMigrationHistory[] migrationHistories, UserContext userContext);
    }
}
