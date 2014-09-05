using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Faust
{
    [DataContract]
    public class FaustMigrationHistory
    {
        [DataMember]
        public int FaustMigrationHistoryId { get; set; }

        [DataMember]
        public int ReleaseNumber { get; set; }

        [DataMember]
        public string ScriptName { get; set; }

        [DataMember]
        public DateTime? LastRun { get; set; }

        [DataMember]
        public bool? Committed { get; set; }

        [DataMember]
        public bool? Successful { get; set; }

        [DataMember]
        public string Log { get; set; }
    }
}
