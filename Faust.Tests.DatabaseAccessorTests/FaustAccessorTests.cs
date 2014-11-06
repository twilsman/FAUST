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
    public class FaustAccessorTest : ResourceAccessTestBase
    {
        private FaustAccessor _faustAccessor;

        [TestInitialize]
        public override void TestInitialize()
        {
            base.TestInitialize();

            _faustAccessor = new FaustAccessor();
        }

        [TestCleanup]
        public override void TestCleanup()
        {
            base.TestCleanup();
        }

        [TestMethod]
        public void FaustAccessor_ExecuteSqlCommand()
        {
            int result = _faustAccessor.ExecuteSqlCommand("select 1 as [result]", base.DefaultUserContext);
            Assert.IsTrue(result == -1);
        }
    }
}
