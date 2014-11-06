using Faust.ResourceAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faust.Tests.DatabaseAccessorTests
{
    [TestClass]
    public class FaustMigrationsHistoryAccessorTest : ResourceAccessTestBase
    {
        private FaustMigrationsHistoryAccessor _faustMigrationsHistoryAccessor;
        private FaustMigrationHistory _migrationHistory;


        [TestInitialize]
        public override void TestInitialize()
        {
            base.TestInitialize();

            _faustMigrationsHistoryAccessor = new FaustMigrationsHistoryAccessor();
            _faustMigrationsHistoryAccessor.Initialize(base.DefaultUserContext);
            using (var db = new FaustDB(base.DefaultUserContext))
            {
                _migrationHistory = new FaustMigrationHistory();
                _migrationHistory.FaustMigrationHistoryId = 1;
                _migrationHistory.ReleaseNumber = 1;
                _migrationHistory.ScriptName = "1.tony.sql";
                _migrationHistory.Committed = false;
                _migrationHistory.Successful = false;
                _migrationHistory.LastRun = DateTime.Now;

                db.FaustMigrationsHistory.Add(_migrationHistory);
                db.SaveChanges();
            }
        }

        [TestCleanup]
        public override void TestCleanup()
        {
            base.TestCleanup();
        }

        [TestMethod]
        public void FaustMigrationsHistoryAccessor_FindMany()
        {
            FaustMigrationHistory[] result = _faustMigrationsHistoryAccessor.FindMany(new FaustMigrationHistory { ScriptName = "1.tony.sql" }, base.DefaultUserContext);
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod]
        public void FaustMigrationsHistoryAccessor_Create()
        {
            FaustMigrationHistory migrationHistory = new FaustMigrationHistory
            {
                ReleaseNumber = 2,
                ScriptName = "2.tony.sql",
                Committed = false,
                Successful = false,
                LastRun = DateTime.Now,
            };

            _faustMigrationsHistoryAccessor.Create(migrationHistory, base.DefaultUserContext);

            int id = migrationHistory.FaustMigrationHistoryId;

            FaustMigrationHistory[] result = _faustMigrationsHistoryAccessor.FindMany(new FaustMigrationHistory { ScriptName = "2.tony.sql" }, base.DefaultUserContext);

            Assert.IsTrue(Array.Find(result, item => migrationHistory.FaustMigrationHistoryId == id) != null);
        }

        [TestMethod]
        public void FaustMigrationsHistoryAccessor_Update()
        {
            FaustMigrationHistory[] result = _faustMigrationsHistoryAccessor.FindMany(new FaustMigrationHistory { ScriptName = "1.tony.sql" }, base.DefaultUserContext);
            result[0].Log = "Test Client Update";

            _faustMigrationsHistoryAccessor.Update(result[0], base.DefaultUserContext);

            int id = result[0].FaustMigrationHistoryId;

            FaustMigrationHistory testing;
            using(FaustDB db = new FaustDB(base.DefaultUserContext))
            {
                testing = db.FaustMigrationsHistory.Where(f => f.FaustMigrationHistoryId == id).FirstOrDefault();
            }

            Assert.IsTrue(testing.Log == "Test Client Update");
        }

        [TestMethod]
        public void FaustMigrationsHistoryAccessor_Delete()
        {
            FaustMigrationHistory migrationHistory = new FaustMigrationHistory
            {
                ReleaseNumber = 2,
                ScriptName = "2.tony.sql",
                Committed = false,
                Successful = false,
                LastRun = DateTime.Now,
            };
            _faustMigrationsHistoryAccessor.Create(migrationHistory, base.DefaultUserContext);

            int id = migrationHistory.FaustMigrationHistoryId;

            _faustMigrationsHistoryAccessor.Delete(migrationHistory, base.DefaultUserContext);

            FaustMigrationHistory testing;
            using (FaustDB db = new FaustDB(base.DefaultUserContext))
            {
                testing = db.FaustMigrationsHistory.Where(f => f.FaustMigrationHistoryId == id).FirstOrDefault();
            }
            Assert.IsNull(testing);
        }

        [TestMethod]
        public void FaustMigrationsHistoryAccessor_DeleteMany()
        {
            FaustMigrationHistory migrationHistory1 = new FaustMigrationHistory
            {
                FaustMigrationHistoryId = 1,
                ReleaseNumber = 1,
                ScriptName = "1.xyzUnitTest.sql",
                Committed = false,
                Successful = false,
                LastRun = DateTime.Now,
                Log = "This needs to be deleted for the purpose of unit testing"
            };


            FaustMigrationHistory migrationHistory2 = _faustMigrationsHistoryAccessor.Create(new FaustMigrationHistory
            {
                FaustMigrationHistoryId = 2,
                ReleaseNumber = 2,
                ScriptName = "2.xyzUnitTest.sql",
                Committed = false,
                Successful = false,
                LastRun = DateTime.Now,
                Log = "This needs to be deleted for the purpose of unit testing"

            }, base.DefaultUserContext);

            int id = migrationHistory1.FaustMigrationHistoryId;
            int id2 = migrationHistory2.FaustMigrationHistoryId;

            _faustMigrationsHistoryAccessor.DeleteMany(new FaustMigrationHistory { Log = "This needs to be deleted for the purpose of unit testing" }, base.DefaultUserContext);

            FaustMigrationHistory testing = _faustMigrationsHistoryAccessor.FindMany(new FaustMigrationHistory { ScriptName = "1.xyzUnitTest.sql" }, base.DefaultUserContext)
                                                                           .FirstOrDefault();
            Assert.IsNull(testing);

            testing = _faustMigrationsHistoryAccessor.FindMany(new FaustMigrationHistory { ScriptName = "2.xyzUnitTest.sql" }, base.DefaultUserContext).FirstOrDefault();
            Assert.IsNull(testing);
        }

        [TestMethod]
        public void FaustMigrationsHistoryAccessor_DeleteDebugEntries()
        {
            _faustMigrationsHistoryAccessor.Create(new FaustMigrationHistory
            {
                FaustMigrationHistoryId = 1,
                ReleaseNumber = -999,
                ScriptName = "1.abc.sql",
                Log = "test",
                LastRun = DateTime.Now
            }, base.DefaultUserContext);

            _faustMigrationsHistoryAccessor.Create(new FaustMigrationHistory
            {
                FaustMigrationHistoryId = 2,
                ReleaseNumber = -999,
                ScriptName = "2.abc.sql",
                Log = "test",
                LastRun = DateTime.Now
            }, base.DefaultUserContext);

            _faustMigrationsHistoryAccessor.Create(new FaustMigrationHistory
            {
                FaustMigrationHistoryId = 3,
                ReleaseNumber = -999,
                ScriptName = "3.abc.sql",
                Log = "test",
                LastRun = DateTime.Now
            }, base.DefaultUserContext);

            int count = _faustMigrationsHistoryAccessor.FindMany(new FaustMigrationHistory { ReleaseNumber = -999 }, base.DefaultUserContext).Length;

            Assert.IsTrue(count == 3);

            _faustMigrationsHistoryAccessor.DeleteDebugEntries(base.DefaultUserContext);

            count = _faustMigrationsHistoryAccessor.FindMany(new FaustMigrationHistory { ReleaseNumber = -999 }, base.DefaultUserContext).Length;

            Assert.IsTrue(count == 0);

        }
    }
}
