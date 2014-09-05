using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Faust
{
    [DataContract]
    public class UserContext
    {

        [DataMember]
        public string ConnectionString { get; set; }

        [DataMember]
        public string ConnectionProvider { get; set; }

        /// <summary>
        /// String representing the user's time zone (i.e. "Central Standard Time")
        /// </summary>
        [DataMember]
        public string TimeZone { get; set; }

        [DataMember]
        public int UserID { get; set; }
    }
}
