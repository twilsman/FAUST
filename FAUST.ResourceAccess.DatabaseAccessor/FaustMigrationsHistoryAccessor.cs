using LinqKit;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Faust.ResourceAccess
{
    public class FaustMigrationsHistoryAccessor : IFaustMigrationsHistoryAccessor
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

        public FaustMigrationHistory Update(FaustMigrationHistory entity, UserContext userContext)
        {
            Logger.Log(string.Format(Resources.Accessors_Updating, typeof(FaustMigrationHistory).Name, entity.FaustMigrationHistoryId), TraceEventType.Verbose);

            using (var db = new FaustDB(userContext))
            {
                db.Entry(entity).State = EntityState.Modified;
                db.SaveChanges();
            }

            Logger.Log(string.Format(Resources.Accessors_Updated, typeof(FaustMigrationHistory).Name, entity.FaustMigrationHistoryId), TraceEventType.Verbose);

            return entity;
        }

        public FaustMigrationHistory[] FindMany(FaustMigrationHistory searchCriteria, UserContext userContext)
        {

            Logger.Log(string.Format(Resources.Accessors_FindMany, typeof(FaustMigrationHistory).ToString()), TraceEventType.Verbose);
            var predicate = SetupFindMany(searchCriteria, userContext);
            predicate = SearchCriteriaExpression(searchCriteria, predicate);

            FaustMigrationHistory[] result;
            using (var db = new FaustDB(userContext))
            {
                result = db.Set<FaustMigrationHistory>().AsExpandable().Where(predicate).ToArray();
            }

            Logger.Log(string.Format(Resources.Accessors_FoundMany, result.Length, typeof(FaustMigrationHistory).ToString()), TraceEventType.Verbose);
            return result;
        }

        public FaustMigrationHistory[] DeleteDebugEntries(UserContext userContext)
        {
            Logger.Log("FaustMigrationsHistoryAccessor - Deleting all debug Migration Histories", TraceEventType.Verbose);

            FaustMigrationHistory[] deletedHistories = new FaustMigrationHistory[0];

            using (var db = new FaustDB(userContext))
            {
                IEnumerable<FaustMigrationHistory> histories = db.Set<FaustMigrationHistory>().Where(fmh => fmh.ReleaseNumber < 0);

                deletedHistories = histories.ToArray();

                histories.ForEach(h => db.Entry(h).State = EntityState.Deleted);
                db.SaveChanges();
            }

            Logger.Log(string.Format("FaustMigrationsHistoryAccessor - Deleted {0} Migration Histories", deletedHistories.Length), TraceEventType.Verbose);

            return deletedHistories;

        }
        public Expression<Func<FaustMigrationHistory, bool>> SearchCriteriaExpression(FaustMigrationHistory searchCriteria, Expression<Func<FaustMigrationHistory, bool>> predicate)
        {
            if (searchCriteria.FaustMigrationHistoryId > 0)
            {
                predicate = predicate.And(migration => migration.FaustMigrationHistoryId == searchCriteria.FaustMigrationHistoryId);
            }

            if (searchCriteria.ReleaseNumber != default(int))
            {
                predicate = predicate.And(migration => migration.ReleaseNumber == searchCriteria.ReleaseNumber);
            }

            if (!string.IsNullOrEmpty(searchCriteria.ScriptName))
            {
                predicate = predicate.And(migration => migration.ScriptName == searchCriteria.ScriptName);
            }

            if (searchCriteria.LastRun != null && searchCriteria.LastRun != DateTime.MinValue)
            {
                predicate = predicate.And(migration => migration.LastRun == searchCriteria.LastRun);
            }

            if (searchCriteria.Successful != null)
            {
                predicate = predicate.And(migration => migration.Successful == searchCriteria.Successful);
            }

            if (searchCriteria.Committed != null)
            {
                predicate = predicate.And(migration => migration.Committed == searchCriteria.Committed);
            }
            return predicate;
        }

        public Expression<Func<FaustMigrationHistory, bool>> SetupFindMany(FaustMigrationHistory searchCriteria, UserContext userContext)
        {
            if (searchCriteria == null) throw new ArgumentException(Resources.Error_SearchParameterNull, "searchCriteria");

            var predicate = PredicateBuilder.True<FaustMigrationHistory>();

            return predicate;
        }
    }    
}
