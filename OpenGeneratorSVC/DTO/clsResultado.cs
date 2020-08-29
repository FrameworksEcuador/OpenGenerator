using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenGeneratorSVC.DTO
{
    public class clsResultado
    {
        public int Resultado { get; set; }              // Código de resultado de la ejecución del proceso. Si retorna 0 es OK, positivos son mensajes para el usuario, negativos solo internos
        public string Mensaje { get; set; }             // Mensaje a ser presentado al usuario
        public List<clsError> Errores { get; set; }     // Lista de errores internos del sistema
        public int? TotalPaginas { get; set; }          // Número de páginas, de acuerdo a la consulta
        public string Autoriza { get; set; }
        public Int64? IdUsuario { get; set; }
        public Int64? TrId { get; set; }                // Id de la transacción almacenada en la BDD
        public int? CodTran { get; set; }
        public int? TotalRegistros { get; set; }
        public string Informacion { get; set; }
        public string Token { get; set; }
    }
}
