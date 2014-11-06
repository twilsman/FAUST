using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faust
{
    public interface IFaustMigrationsHistoryAccessor
    {
        void Initialize(UserContext userContext);
        FaustMigrationHistory[] DeleteDebugEntries(UserContext userContext);
        FaustMigrationHistory Create(FaustMigrationHistory entity, UserContext userContext);
        FaustMigrationHistory Update(FaustMigrationHistory entity, UserContext userContext);
        FaustMigrationHistory[] FindMany(FaustMigrationHistory searchCriteria, UserContext userContext);
    }
}
