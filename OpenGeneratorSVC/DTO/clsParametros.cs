using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenGeneratorSVC.DTO
{
    public class clsParametros
    {
        public string Autor { get; set; }                   // Nombre del Autor
        public string Aplicacion { get; set; }              // Nombre de la aplicación
        public string Namespace { get; set; }               // Nombre del Namespace
        public string BaseDeDatos { get; set; }             // Base de Datos sobre la que se va a trabajar
        public string Tabla { get; set; }                   // Nombre de la Tabla
        public string Plantilla { get; set; }               // Código de la Plantilla
        public bool bolTienePk { get; set; }                // Tiene clave primaria
        public bool bolTieneConcurrencia { get; set; }      // Tiene clave el campo de concurrencia VERSION
        public bool bolUsarTransactor { get; set; }         // Usar el Transactor
    }
}
