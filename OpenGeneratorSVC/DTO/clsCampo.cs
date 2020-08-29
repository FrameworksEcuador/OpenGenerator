using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenGeneratorSVC.DTO
{
    public class clsCampo
    {
        public String column_name { get; set; }
        public uint? ordinal_position { get; set; }
        public Boolean? is_nullable { get; set; }
        public String data_type { get; set; }
        public long? character_maximum_length { get; set; }
        public long? numeric_precision { get; set; }
        public long? numeric_scale { get; set; }
        public long? datetime_precision { get; set; }
        public String constraint_type { get; set; }

    }
}
