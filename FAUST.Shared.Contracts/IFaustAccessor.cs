using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faust
{
    public interface IFaustAccessor
    {
        int ExecuteSqlCommand(string sqlCommand, UserContext userContext);
    }
}
