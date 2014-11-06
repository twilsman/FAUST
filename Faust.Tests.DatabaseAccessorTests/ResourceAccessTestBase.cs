using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Faust.Tests.DatabaseAccessorTests
{
    public abstract class ResourceAccessTestBase
    {
        private TransactionScope _testTransactionScope;

        #region Properties

        private UserContext _defaultUserContext;
        public UserContext DefaultUserContext
        {
            get
            {
                if (_defaultUserContext == null)
                    _defaultUserContext = UserContext.GetUserContext(ConfigurationManager.AppSettings["DefaultDatabase"]);

                return _defaultUserContext;
            }
            set
            {
                // You can override the connection string 
                // for testing if you need to
                _defaultUserContext = value;
            }
        }

        #endregion


        public virtual void TestInitialize()
        {
            _testTransactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = TransactionIsolationLevel });
        }

        public virtual void TestCleanup()
        {
            _testTransactionScope.Dispose();
        }

        public virtual IsolationLevel TransactionIsolationLevel
        {
            get
            {
                return IsolationLevel.ReadCommitted;
            }
        }

    }
}
