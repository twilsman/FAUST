using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Faust.Managers;

namespace Faust.Tests.ManagerTests
{
    [TestClass]
    public class FaustManagerTests
    {
        [TestMethod]
        public void BasicTest()
        {
            bool initialized = false;
            bool debugDeleted = false;
            bool gotScripts = false;
            bool ranScript = false;
            bool savedHistory = false;

            UnityCache.RegisterInstance<IFaustMigrationsHistoryAccessor>(new Fakes.StubIFaustMigrationsHistoryAccessor
            {
                InitializeUserContext = (userContext) =>
                {
                    initialized = true;
                },
                DeleteDebugEntriesUserContext = (userContext) =>
                {
                    debugDeleted = true;
                    return new FaustMigrationHistory[] { };
                }

            });

            UnityCache.RegisterInstance<IFaustEngine>(new Fakes.StubIFaustEngine
            {
                GetReleaseMigrationScriptsDirectoryInfoUserContext = (releaseDir, userContext) =>
                {
                    gotScripts = true;
                    return new FaustMigrationScript[] {new FaustMigrationScript()};
                },
                RunMigrationScriptFaustMigrationScriptUserContext = (script, userContext) =>
                {
                    ranScript = true;
                    return new FaustMigrationHistory();
                },
                SaveMigrationsHistoryFaustMigrationHistoryArrayUserContext = (histories, userContext) =>
                {
                    savedHistory = true;
                }
            });

            new FaustManager().RunMigration("unitTest");

            Assert.IsTrue(initialized);
            Assert.IsTrue(debugDeleted);
            Assert.IsTrue(gotScripts);
            Assert.IsTrue(ranScript);
            Assert.IsTrue(savedHistory);
        }
    }
}
