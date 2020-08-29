using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenGeneratorSVC.DTO
{
    public class clsError
    {
        public int? Codigo { get; set; }
        public int? Linea { get; set; }
        public string Mensaje { get; set; }
        public string Origen { get; set; }
    }
}
