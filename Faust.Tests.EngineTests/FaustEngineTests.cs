using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Faust.Engines;

namespace Faust.Tests.EngineTests
{
    [TestClass]
    public class FaustEngineTests
    {
        [TestMethod]
        public void GetReleaseMigrationScripts_TestFindOne()
        {
            bool foundMany = false;

            UnityCache.RegisterInstance<IFaustMigrationsHistoryAccessor>(new Fakes.StubIFaustMigrationsHistoryAccessor
            {
                FindManyFaustMigrationHistoryUserContext = (history, userContext) => {
                    foundMany = true;
                    return new FaustMigrationHistory[] {};
                }
            });

            DirectoryInfo releaseDir = new DirectoryInfo(@"..\..\migrations\UnitTest\001");

            FaustMigrationScript[] results = new FaustEngine().GetReleaseMigrationScripts(releaseDir, new UserContext());

            Assert.IsTrue(foundMany);
            Assert.AreEqual(1, results.Length);
        }

        [TestMethod]
        public void GetReleaseMigrationScripts_TestFindZero()
        {
            bool foundMany = false;

            UnityCache.RegisterInstance<IFaustMigrationsHistoryAccessor>(new Fakes.StubIFaustMigrationsHistoryAccessor
            {
                FindManyFaustMigrationHistoryUserContext = (history, userContext) =>
                {
                    foundMany = true;
                    return new FaustMigrationHistory[] { new FaustMigrationHistory() };
                }
            });

            DirectoryInfo releaseDir = new DirectoryInfo(@"..\..\migrations\UnitTest\001");

            FaustMigrationScript[] results = new FaustEngine().GetReleaseMigrationScripts(releaseDir, new UserContext());

            Assert.IsTrue(foundMany);
            Assert.AreEqual(0, results.Length);
        }

        [TestMethod]
        public void RunMigrationScript_Test()
        {
            bool executed = false;

            UnityCache.RegisterInstance<IFaustAccessor>(new Fakes.StubIFaustAccessor
            {
                ExecuteSqlCommandStringUserContext = (command, userContext) => 
                {
                    executed = true;
                    return 1;
                }
            });

            FaustMigrationScript script = new FaustMigrationScript
            {
                ReleaseNumber = 1,
                ScriptName = "UnitTestScript",
                Commands = new string[] { "Command 1"}
            };

            FaustMigrationHistory result = new FaustEngine().RunMigrationScript(script, new UserContext());
            Assert.IsTrue(executed);
            Assert.IsTrue(result.Successful.Value);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void RunMigrationScriptError_Test()
        {
            bool executed = false;

            UnityCache.RegisterInstance<IFaustAccessor>(new Fakes.StubIFaustAccessor
            {
                ExecuteSqlCommandStringUserContext = (command, userContext) =>
                {
                    executed = true;
                    throw new Exception();
                }
            });

            FaustMigrationScript script = new FaustMigrationScript
            {
                ReleaseNumber = 1,
                ScriptName = "UnitTestScript",
                Commands = new string[] { "Command 1" }
            };

            FaustMigrationHistory result = new FaustEngine().RunMigrationScript(script, new UserContext());
            Assert.IsTrue(executed);
            Assert.IsFalse(result.Successful.Value);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void SaveMigrationsHistoryCreate_Test()
        {
            bool created = false;
            bool updated = false;

            UnityCache.RegisterInstance<IFaustMigrationsHistoryAccessor>(new Fakes.StubIFaustMigrationsHistoryAccessor
            {
                FindManyFaustMigrationHistoryUserContext = (history, userContext) => {
                    return new FaustMigrationHistory[]{};
                },
                CreateFaustMigrationHistoryUserContext = (history, userContext) =>
                {
                    created = true;
                    return new FaustMigrationHistory();
                },
                UpdateFaustMigrationHistoryUserContext = (history, userContext) =>
                {
                    updated = true;
                    return new FaustMigrationHistory();
                }
            });

            new FaustEngine().SaveMigrationsHistory(new FaustMigrationHistory[] { new FaustMigrationHistory() }, new UserContext());

            Assert.IsTrue(created);
            Assert.IsFalse(updated);
        }

        [TestMethod]
        public void SaveMigrationsHistoryUpdate_Test()
        {
            bool created = false;
            bool updated = false;

            UnityCache.RegisterInstance<IFaustMigrationsHistoryAccessor>(new Fakes.StubIFaustMigrationsHistoryAccessor
            {
                FindManyFaustMigrationHistoryUserContext = (history, userContext) =>
                {
                    return new FaustMigrationHistory[] {new FaustMigrationHistory()};
                },
                CreateFaustMigrationHistoryUserContext = (history, userContext) =>
                {
                    created = true;
                    return new FaustMigrationHistory();
                },
                UpdateFaustMigrationHistoryUserContext = (history, userContext) =>
                {
                    updated = true;
                    return new FaustMigrationHistory();
                }
            });

            new FaustEngine().SaveMigrationsHistory(new FaustMigrationHistory[] { new FaustMigrationHistory() }, new UserContext());

            Assert.IsFalse(created);
            Assert.IsTrue(updated);
        }
    }
}
