using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faust
{
    [NotMapped]
    public class FaustMigrationScript : FaustMigrationHistory
    {
        public string[] Commands { get; set; }

        public string FilePath { get; set; }
    }
}
