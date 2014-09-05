using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRC.Connect;
using System.Diagnostics;
using System.Data.Entity;
using NRC.Common;
using LinqKit;
using System.Linq.Expressions;
using Faust;

namespace Faust.ResourceAccess
{
    public abstract class TableBaseAccessor<TDbContext, TEntity> : ConnectServiceBase, ITableBaseAccessor<TEntity>
        where TEntity : class, new()
        where TDbContext : DbContext, new()
    {
        public virtual TEntity Create(TEntity entity, UserContext userContext)
        {
            Logger.Log(string.Format(Resources.Accessors_Create, typeof(TEntity).Name), TraceEventType.Verbose);
            UserContextHelper.CheckUserContext(userContext);
            if (entity == null) throw new ArgumentException(Resources.Error_CreateEntityParameterNull, "entity");

            using (var db = DatabaseFactory.Create<TDbContext>(userContext))
            {
                db.Set<TEntity>().Add(entity);
                db.SaveChanges();
            }

            Logger.Log(string.Format(Resources.Accessors_Created, typeof(TEntity).Name), TraceEventType.Verbose);

            return entity;
        }

        public virtual TEntity Update(TEntity entity, UserContext userContext)
        {
            using (var db = DatabaseFactory.Create<TDbContext>(userContext))
            {
                db.Entry(entity).State = System.Data.EntityState.Modified;
                db.SaveChanges();
            }

            Logger.Log(string.Format(Resources.Accessors_Update, typeof(TEntity).Name), TraceEventType.Verbose);

            return entity;
        }

        public virtual TEntity[] FindMany(TEntity searchCriteria, UserContext userContext)
        {
            UserContextHelper.CheckUserContext(userContext);

            Logger.Log(string.Format(Resources.Accessors_FindMany, typeof(TEntity).ToString()), TraceEventType.Verbose);
            var predicate = SetupFindMany(searchCriteria, userContext);
            predicate = SearchCriteriaExpression(searchCriteria, predicate);

            TEntity[] result;
            using (var db = DatabaseFactory.Create<TDbContext>(userContext))
            {
                result = db.Set<TEntity>().AsExpandable().Where(predicate).ToArray();
            }

            Logger.Log(string.Format(Resources.Accessors_FoundMany, result.Length, typeof(TEntity).ToString()), TraceEventType.Verbose);
            return result;
        }

        public virtual TEntity FindFirst(TEntity searchCriteria, UserContext userContext)
        {
            UserContextHelper.CheckUserContext(userContext);

            var predicate = SetupFindMany(searchCriteria, userContext);
            predicate = SearchCriteriaExpression(searchCriteria, predicate);

            TEntity result;
            using (var db = DatabaseFactory.Create<TDbContext>(userContext))
            {
                result = db.Set<TEntity>().AsExpandable().Where(predicate).FirstOrDefault();
            }

            return result;
        }

        public abstract Expression<Func<TEntity, bool>> SearchCriteriaExpression(TEntity searchCriteria, Expression<Func<TEntity, bool>> predicate);

        public virtual TEntity Delete(TEntity entity, UserContext userContext)
        {
            UserContextHelper.CheckUserContext(userContext);
            if (entity == null) throw new ArgumentException(Resources.Error_DeleteEntityParameterNull, "entity");

            using (var db = DatabaseFactory.Create<TDbContext>(userContext))
            {
                db.Entry(entity).State = System.Data.EntityState.Deleted;
                db.SaveChanges();
            }

            Logger.Log(string.Format(Resources.Accessors_Deleted, typeof(TEntity).Name), TraceEventType.Verbose);

            return entity;
        }

        public virtual TEntity[] DeleteMany(TEntity searchCriteria, UserContext userContext)
        {
            UserContextHelper.CheckUserContext(userContext);
            if (searchCriteria == null) throw new ArgumentException(Resources.Error_SearchParameterNull, "searchCriteria");

            TEntity[] foundArray = FindMany(searchCriteria, userContext);

            using (var db = DatabaseFactory.Create<TDbContext>(userContext))
            {
                foreach (TEntity entity in foundArray)
                {
                    //db.Set<TEntity>().Remove(entity);
                    db.Entry(entity).State = System.Data.EntityState.Deleted;
                }
                db.SaveChanges();
            }

            Logger.Log(string.Format(Resources.Accessors_DeletedMany, foundArray.Length, typeof(TEntity).Name), TraceEventType.Verbose);

            return foundArray;
        }

        public virtual TEntity[] FindAll(UserContext userContext)
        {
            UserContextHelper.CheckUserContext(userContext);

            Logger.Log(string.Format(Resources.Accessors_FindAll, typeof(TEntity).Name), TraceEventType.Verbose);

            using (var db = DatabaseFactory.Create<TDbContext>(userContext))
            {
                return db.Set<TEntity>().ToArray();
            }
        }

        public virtual void LogFindManyResults(TEntity[] entities)
        {
            string message = (entities != null) ? string.Format(Resources.Accessors_FoundMany, entities.Length, typeof(TEntity).Name) :
                    string.Format(Resources.Accessors_NoFound, typeof(TEntity).Name);
            Logger.Log(message, TraceEventType.Verbose, null);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Expression<Func<TEntity, bool>> SetupFindMany(TEntity searchCriteria, UserContext userContext)
        {
            if (searchCriteria == null) throw new ArgumentException(Resources.Error_SearchParameterNull, "searchCriteria");

            var predicate = PredicateBuilder.True<TEntity>();

            return predicate;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Expression<Func<TEntity, bool>> SetupFindManyOr(TEntity searchCriteria, UserContext userContext)
        {
            if (searchCriteria == null) throw new ArgumentException(Resources.Error_SearchParameterNull, "searchCriteria");

            var predicate = PredicateBuilder.False<TEntity>();

            return predicate;
        }
    }
}
