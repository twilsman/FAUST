using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faust.ResourceAccess
{
    public class FaustMigrationsHistoryAccessor
    {
        public void Initialize(UserContext userContext)
        {
            Logger.Log(Resources.FaustMigrationsHistoryAccessor_Initialize, TraceEventType.Verbose);

            using (var db = new FaustDB(userContext))
            {
                try
                {
                    db.Database.ExecuteSqlCommand(@"
                        if (NOT EXISTS (SELECT * 
                                         FROM INFORMATION_SCHEMA.TABLES 
                                         WHERE TABLE_SCHEMA = 'dbo' 
                                         AND  TABLE_NAME = 'FaustMigrationsHistory'))
                        begin
                            create table dbo.FaustMigrationsHistory
	                        (
		                        FaustMigrationHistoryId int identity(1,1),
		                        ReleaseNumber int NOT NULL,
                                ScriptName varchar(max) NOT NULL,
		                        LastRun datetime NOT NULL DEFAULT(getdate()),
                                Committed bit default(null),
                                Successful bit default(null),
                                Log text,

                                constraint PK_FaustMigrationsHistory_FaustMigrationHistoryId primary key (FaustMigrationHistoryId)
	                        )
                        end");
                }
                catch (Exception ex)
                {
                    Logger.Log(string.Format(Resources.FaustMigrationsHistoryAccessor_InitializeError, ex.Message, ex.StackTrace), TraceEventType.Error);
                    throw;
                }
            }
        }

        public FaustMigrationHistory Create(FaustMigrationHistory entity, UserContext userContext)
        {
            Logger.Log(string.Format(Resources.Accessors_Create, typeof(FaustMigrationHistory).Name), TraceEventType.Verbose);

            if (entity == null) throw new ArgumentException(Resources.Error_CreateEntityParameterNull, "entity");

            using (var db = new FaustDB(userContext))
            {
                db.Set<FaustMigrationHistory>().Add(entity);
                db.SaveChanges();
            }

            Logger.Log(string.Format(Resources.Accessors_Created, typeof(FaustDB).Name), TraceEventType.Verbose);

            return entity;
        }
    }    
}
