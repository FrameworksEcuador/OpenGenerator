using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenGeneratorSVC.BLL
{
    public class bllGenerator
    {
        private string strNombreColumnaVersion = "";
        private List<DTO.clsCampo> ListaCampos = null;
        public List<String> fnConsultarBases(out DTO.clsResultado resultado)
        {
            DAL.dalGenerator dal = new DAL.dalGenerator();
            return dal.fnConsultarBases(out resultado);
        }

        public List<String> fnConsultarTablas(string esquema, out DTO.clsResultado resultado)
        {
            DAL.dalGenerator dal = new DAL.dalGenerator();
            return dal.fnConsultarTablas(esquema, out resultado);
        }

        public List<String> fnConsultarPlantillas(out DTO.clsResultado resultado)
        {
            DAL.dalGenerator dal = new DAL.dalGenerator();
            return dal.fnConsultarPlantillas(out resultado);
        }

        public String fnGenerar(DTO.clsParametros parametros, out DTO.clsResultado resultado)
        {
            StringBuilder strTexto = new StringBuilder();
            resultado = new DTO.clsResultado();
            DAL.dalGenerator dal = new DAL.dalGenerator();
            try
            {
                String strTextoPlantilla = dal.fnConsultarTextoPlantilla(parametros.Plantilla, out resultado);
                ListaCampos = dal.fnConsultarCampos(parametros, out resultado);
                parametros.bolTienePk = false;
                parametros.bolTieneConcurrencia = false;
                strNombreColumnaVersion = "";
                foreach(DTO.clsCampo campo in ListaCampos)
                {
                    if (campo.constraint_type != null && campo.constraint_type.Equals("PRI"))
                        parametros.bolTienePk = true;
                    if (campo.column_name != null && campo.column_name.ToUpper().Contains("VERSION"))
                    {
                        parametros.bolTieneConcurrencia = true;
                        strNombreColumnaVersion = campo.column_name;
                    }
                }

                strTexto = new StringBuilder();
                if (!parametros.bolTienePk)
                {
                    strTexto.Append("******** TABLA NO TIENE PK DEFINIDA - ALGUNAS PLANTILLAS SE GENERARAN CON ERROR **************");
                }
                strTexto.Append(strTextoPlantilla);
                if (parametros.bolTieneConcurrencia)
                {
                    if (strTextoPlantilla.Contains("<<<INCLUYE-CONCURRENCIA>>>"))                        // 1
                        strTexto.Replace("<<<INCLUYE-CONCURRENCIA>>>", fnIncluyeConcurrencia(parametros));
                    if (strTextoPlantilla.Contains("<<<INCLUYE-UPD-CONCURRENCIA>>>"))                    // 2
                        strTexto.Replace("<<<INCLUYE-UPD-CONCURRENCIA>>>", "," + strNombreColumnaVersion.ToUpper() + " = getdate()");
                    if (strTextoPlantilla.Contains("<<<INCLUYE-ADD-CONCURRENCIA-CAMPO>>>"))              // 3
                        strTexto.Replace("<<<INCLUYE-ADD-CONCURRENCIA-CAMPO>>>", "," + strNombreColumnaVersion.ToUpper());
                    if (strTextoPlantilla.Contains("<<<INCLUYE-UPD-CONCURRENCIA-CAMPO>>>"))              // 4
                        strTexto.Replace("<<<INCLUYE-UPD-CONCURRENCIA-CAMPO>>>", strNombreColumnaVersion.ToUpper());
                    if (strTextoPlantilla.Contains("<<<INCLUYE-ADD-CONCURRENCIA-CAMPO-VALOR>>>"))        // 5
                        strTexto.Replace("<<<INCLUYE-ADD-CONCURRENCIA-CAMPO-VALOR>>>", ", getdate()");
                }
                else
                {
                    if (strTextoPlantilla.Contains("<<<INCLUYE-CONCURRENCIA>>>"))                        // 1
                        strTexto.Replace("<<<INCLUYE-CONCURRENCIA>>>", "");
                    if (strTextoPlantilla.Contains("<<<INCLUYE-UPD-CONCURRENCIA>>>"))                    // 2
                        strTexto.Replace("<<<INCLUYE-UPD-CONCURRENCIA>>>", "");
                    if (strTextoPlantilla.Contains("<<<INCLUYE-ADD-CONCURRENCIA-CAMPO>>>"))              // 3
                        strTexto.Replace("<<<INCLUYE-ADD-CONCURRENCIA-CAMPO>>>", "");
                    if (strTextoPlantilla.Contains("<<<INCLUYE-UPD-CONCURRENCIA-CAMPO>>>"))              // 4
                        strTexto.Replace("<<<INCLUYE-UPD-CONCURRENCIA-CAMPO>>>", "");
                    if (strTextoPlantilla.Contains("<<<INCLUYE-ADD-CONCURRENCIA-CAMPO-VALOR>>>"))        // 5
                        strTexto.Replace("<<<INCLUYE-ADD-CONCURRENCIA-CAMPO-VALOR>>>", "");
                }

                if (strTextoPlantilla.Contains("<<<CAMPOS-INPUT-TIPO>>>"))                           // 6
                    strTexto.Replace("<<<CAMPOS-INPUT-TIPO>>>", fnCamposInputTipo(false, false));
                if (strTextoPlantilla.Contains("<<<CAMPOS-INPUT-TIPO-MAY>>>"))                       // 7
                    strTexto.Replace("<<<CAMPOS-INPUT-TIPO>>>", fnCamposInputTipo(false, false).ToUpper());

                if (parametros.bolUsarTransactor)
                {
                    if (strTextoPlantilla.Contains("<<<INCLUYE-TRANSACTOR>>>"))                     // 8
                        strTexto.Replace("<<<INCLUYE-TRANSACTOR>>>", fnCamposTransactor());
                    if (strTextoPlantilla.Contains("<<<INCLUYE_TRANSACTOR>>>"))                     // 8
                        strTexto.Replace("<<<INCLUYE_TRANSACTOR>>>", fnCamposTransactor());
                    if (strTextoPlantilla.Contains("<<<LOG-INTERNO-CON-TRANSACTOR>>>"))             // 9
                        strTexto.Replace("<<<LOG-INTERNO-CON-TRANSACTOR>>>", fnLogInternoTransactor(parametros));
                    if (strTextoPlantilla.Contains("<<<LOG_INTERNO_CON_TRANSACTOR>>>"))             // 9
                        strTexto.Replace("<<<LOG_INTERNO_CON_TRANSACTOR>>>", fnLogInternoTransactor(parametros));
                }
                else
                {
                    if (strTextoPlantilla.Contains("<<<INCLUYE-TRANSACTOR>>>"))                     // 8
                        strTexto.Replace("<<<INCLUYE-TRANSACTOR>>>", "");
                    if (strTextoPlantilla.Contains("<<<INCLUYE_TRANSACTOR>>>"))                     // 8
                        strTexto.Replace("<<<INCLUYE_TRANSACTOR>>>", "");
                    if (strTextoPlantilla.Contains("<<<LOG-INTERNO-CON-TRANSACTOR>>>"))             // 9
                        strTexto.Replace("<<<LOG-INTERNO-CON-TRANSACTOR>>>", "");
                    if (strTextoPlantilla.Contains("<<<LOG_INTERNO_CON_TRANSACTOR>>>"))             // 9
                        strTexto.Replace("<<<LOG_INTERNO_CON_TRANSACTOR>>>", "");
                }

                if (strTextoPlantilla.Contains("<<<AUTOR>>>"))                                       // 10
                    strTexto.Replace("<<<AUTOR>>>", parametros.Autor);
                if (strTextoPlantilla.Contains("<<<NOMBRE-ESQUEMA>>>"))                              // 11
                    strTexto.Replace("<<<NOMBRE-ESQUEMA>>>", parametros.BaseDeDatos);
                if (strTextoPlantilla.Contains("<<<NOMBRE-NAMESPACE>>>"))                            // 12
                    strTexto.Replace("<<<NOMBRE-NAMESPACE>>>", parametros.Namespace);
                if (strTextoPlantilla.Contains("<<<NOMBRE-APLICACION>>>"))                            // 12
                    strTexto.Replace("<<<NOMBRE-APLICACION>>>", parametros.Aplicacion);

                if (strTextoPlantilla.Contains("<<<NOMBRE-TABLA>>>"))                                // 13
                    strTexto.Replace("<<<NOMBRE-TABLA>>>", fnNombreTabla(parametros));
                if (strTextoPlantilla.Contains("<<<NOMBRE-TABLA-MAY>>>"))                            // 14
                    strTexto.Replace("<<<NOMBRE-TABLA-MAY>>>", fnNombreTabla(parametros).ToUpper());
                if (strTextoPlantilla.Contains("<<<NOMBRE-TABLA-MIN>>>"))                            // 15
                    strTexto.Replace("<<<NOMBRE-TABLA-MIN>>>", fnNombreTabla(parametros).ToLower());
                if (strTextoPlantilla.Contains("<<<NOMBRE-TABLA-CAMEL>>>"))                          // 16
                    strTexto.Replace("<<<NOMBRE-TABLA-CAMEL>>>", fnNombreTablaCamel(parametros));
                if (strTextoPlantilla.Contains("<<<NOMBRE-TABLA-FULL>>>"))                           // 17
                    strTexto.Replace("<<<NOMBRE-TABLA-FULL>>>", parametros.Tabla);

                if (strTextoPlantilla.Contains("<<<COLUMNA_CONTROL_VERSION>>>"))                           // 18
                    strTexto.Replace("<<<COLUMNA_CONTROL_VERSION>>>", strNombreColumnaVersion.ToUpper());
                if (strTextoPlantilla.Contains("<<<COLUMNA-CONTROL-VERSION>>>"))                           // 18
                    strTexto.Replace("<<<COLUMNA-CONTROL-VERSION>>>", strNombreColumnaVersion.ToUpper());
                if (strTextoPlantilla.Contains("<<<FECHA-CREACION>>>"))                                    // 19
                    strTexto.Replace("<<<FECHA-CREACION>>>", DateTime.Now.ToString("yyyy/MM/dd hh:mm tt"));
                if (strTextoPlantilla.Contains("<<<CAMPOS-NOMBRE>>>"))                                    // 20
                    strTexto.Replace("<<<CAMPOS-NOMBRE>>>", fnCamposNombre(false, false));
                if (strTextoPlantilla.Contains("<<<CAMPOS-NOMBRE-TODOS>>>"))                              // 21
                    strTexto.Replace("<<<CAMPOS-NOMBRE-TODOS>>>", fnCamposNombre(true, false));
                if (strTextoPlantilla.Contains("<<<CAMPOS-INPUT-NOMBRE>>>"))                              // 22
                    strTexto.Replace("<<<CAMPOS-INPUT-NOMBRE>>>", fnCamposInputNombre(false, false));
                if (strTextoPlantilla.Contains("<<<CAMPOS-INPUT-NOMBRE-MAY>>>"))                          // 23
                    strTexto.Replace("<<<CAMPOS-INPUT-NOMBRE-MAY>>>", fnCamposInputNombre(false, false).ToUpper());
                if (strTextoPlantilla.Contains("<<<CAMPOS-PK-NOMBRE>>>"))                                 // 24
                    strTexto.Replace("<<<CAMPOS-PK-NOMBRE>>>", fnCamposNombre(false, true));
                if (strTextoPlantilla.Contains("<<<CAMPOS-PK-INPUT-TIPO>>>"))                             // 25
                    strTexto.Replace("<<<CAMPOS-PK-INPUT-TIPO>>>", fnCamposInputTipo(false, true));
                if (strTextoPlantilla.Contains("<<<CAMPOS-PK-INPUT-TIPO-MAY>>>"))                         // 26
                    strTexto.Replace("<<<CAMPOS-PK-INPUT-TIPO-MAY>>>", fnCamposInputTipo(false, true).ToUpper());
                if (strTextoPlantilla.Contains("<<<CAMPOS-PK-INPUT-NOMBRE>>>"))                           // 27
                    strTexto.Replace("<<<CAMPOS-PK-INPUT-NOMBRE>>>", fnCamposInputNombre(false, true));
                if (strTextoPlantilla.Contains("<<<CAMPOS-NOMBRE-INPUT>>>"))                              // 28
                    strTexto.Replace("<<<CAMPOS-NOMBRE-INPUT>>>", fnCamposNombreInputWhere(false, false, ","));
                if (strTextoPlantilla.Contains("<<<CAMPOS-NOMBRE-INPUT-MAY>>>"))                          // 29
                    strTexto.Replace("<<<CAMPOS-NOMBRE-INPUT-MAY>>>", fnCamposNombreInputWhere(false, false, ",").ToUpper());
                if (strTextoPlantilla.Contains("<<<CAMPOS_NOMBRE_INPUT>>>"))                              // 28
                    strTexto.Replace("<<<CAMPOS_NOMBRE_INPUT>>>", fnCamposNombreInputWhere(false, false, ","));
                if (strTextoPlantilla.Contains("<<<CAMPOS_NOMBRE_INPUT_MAY>>>"))                          // 29
                    strTexto.Replace("<<<CAMPOS_NOMBRE_INPUT_MAY>>>", fnCamposNombreInputWhere(false, false, ",").ToUpper());
                if (strTextoPlantilla.Contains("<<<CAMPOS-PK-NOMBRE-INPUT>>>"))                           // 30
                    strTexto.Replace("<<<CAMPOS-PK-NOMBRE-INPUT>>>", fnCamposNombreInputWhere(false, true, ","));

                if (strTextoPlantilla.Contains("<<<CAMPOS-PK-NOMBRE-INPUT-WHERE>>>"))                // 31
                    strTexto.Replace("<<<CAMPOS-PK-NOMBRE-INPUT-WHERE>>>", fnCamposNombreInputWhere(false, true));
                if (strTextoPlantilla.Contains("<<<CAMPOS_PK_NOMBRE_INPUT_WHERE>>>"))                // 31
                    strTexto.Replace("<<<CAMPOS_PK_NOMBRE_INPUT_WHERE>>>", fnCamposNombreInputWhere(false, true));
                if (strTextoPlantilla.Contains("<<<CAMPOS-PK-NOMBRE-INPUT-WHERE-MAY>>>"))            // 32
                    strTexto.Replace("<<<CAMPOS-PK-NOMBRE-INPUT-WHERE-MAY>>>", fnCamposNombreInputWhere(false, true).ToUpper());
                if (strTextoPlantilla.Contains("<<<CAMPOS_PK_NOMBRE_INPUT_WHERE_MAY>>>"))            // 32
                    strTexto.Replace("<<<CAMPOS_PK_NOMBRE_INPUT_WHERE_MAY>>>", fnCamposNombreInputWhere(false, true).ToUpper());

                if (strTextoPlantilla.Contains("<<<CAMPOS-PK-NOMBRE-INPUT-WHERE-CONCAT>>>"))         // 33
                    strTexto.Replace("<<<CAMPOS-PK-NOMBRE-INPUT-WHERE-CONCAT>>>", fnCamposNombreInputWhereConvert(false, true));
                if (strTextoPlantilla.Contains("<<<CAMPOS_PK_NOMBRE_INPUT_WHERE_CONCAT>>>"))         // 33
                    strTexto.Replace("<<<CAMPOS_PK_NOMBRE_INPUT_WHERE_CONCAT>>>", fnCamposNombreInputWhereConvert(false, true));

                if (strTextoPlantilla.Contains("<<<CAMPOS-DTO>>>"))                                  // 34
                    strTexto.Replace("<<<CAMPOS-DTO>>>", fnCamposDTO(true, false));
                if (strTextoPlantilla.Contains("<<<CAMPOS-DTO-JSON>>>"))                             // 34
                    strTexto.Replace("<<<CAMPOS-DTO-JSON>>>", fnCamposDTOJSON(true, false));

                if (strTextoPlantilla.Contains("<<<CAMPOS-CLS>>>"))                                  // 35
                    strTexto.Replace("<<<CAMPOS-CLS>>>", fnCamposCLS(true, false));
                if (strTextoPlantilla.Contains("<<<CAMPOS-LIMPIAR>>>"))                              // 36
                    strTexto.Replace("<<<CAMPOS-LIMPIAR>>>", fnCamposLimpiar(parametros, true, false));
                if (strTextoPlantilla.Contains("<<<CAMPOS-FILTRO>>>"))                               // 37
                    strTexto.Replace("<<<CAMPOS-FILTRO>>>", fnCamposFiltro(parametros));
                if (strTextoPlantilla.Contains("<<<CAMPOS-DAL-ADD>>>"))                              // 38
                    strTexto.Replace("<<<CAMPOS-DAL-ADD>>>", fnCamposDAL(parametros, true, false));
                if (strTextoPlantilla.Contains("<<<CAMPOS-DAL-RESULTADO-ADD>>>"))                    // 39
                    strTexto.Replace("<<<CAMPOS-DAL-RESULTADO-ADD>>>", fnCamposDALResultado(parametros));
                if (strTextoPlantilla.Contains("<<<CAMPOS-DAL-DEL>>>"))                              // 40
                    strTexto.Replace("<<<CAMPOS-DAL-DEL>>>", fnCamposDAL(parametros, false, true));

                if (strTextoPlantilla.Contains("<<<CAMPOS-WEB-HTML>>>"))                              // 48
                    strTexto.Replace("<<<CAMPOS-WEB-HTML>>>", fnCamposWebHTML(parametros));
                if (strTextoPlantilla.Contains("<<<CAMPOS-WEB-FILTRO>>>"))                            // 49
                    strTexto.Replace("<<<CAMPOS-WEB-FILTRO>>>", fnCamposWebFiltro(parametros));
                if (strTextoPlantilla.Contains("<<<CAMPOS-WEB-TITULO>>>"))                            // 50
                    strTexto.Replace("<<<CAMPOS-WEB-TITULO>>>", fnCamposWebTitulo());
                if (strTextoPlantilla.Contains("<<<CAMPOS-WEB-DETALLE>>>"))                           // 51
                    strTexto.Replace("<<<CAMPOS-WEB-DETALLE>>>", fnCamposWebDetalle());


                if (strTextoPlantilla.Contains("<<<AÑO>>>"))                                         // 52
                    strTexto.Replace("<<<AÑO>>>", DateTime.Now.ToString("yyyy"));
                if (strTextoPlantilla.Contains("<<<MES>>>"))                                         // 53
                    strTexto.Replace("<<<MES>>>", DateTime.Now.ToString("MM"));

            }
            catch (Exception ex)
            {
                resultado.Resultado = -2;
                resultado.Mensaje = ex.Message;
            }

            return strTexto.ToString();
        }

        private String fnIncluyeConcurrencia(DTO.clsParametros parametros)
        {
            return "SELECT @W_INSTANTE = ' + @COLUMNA_CONTROL_VERSION + '" + Environment.NewLine +
                   "FROM " + fnNombreTabla(parametros) + Environment.NewLine +
                   "WHERE " + fnCamposNombreInputWhere(false, false) + ";" + Environment.NewLine +
                   Environment.NewLine +
                   "IF(CAST(@W_INSTANTE AS DATETIME2(0)) <> CAST(@I_INSTANTE AS DATETIME2(0)))" + Environment.NewLine +
                   "BEGIN" + Environment.NewLine +
                   "SET O_RETVAL = -3; " + Environment.NewLine +
                   "SET O_RETMSG = 'VERSION INCORRECTA'; " + Environment.NewLine +
                   "RETURN O_RETVAL; " + Environment.NewLine +
                   "--EXEC LOGDB.DBO.LOG_ADD @I_TEXTO = 'ERROR EN <<<NOMBRE-TABLA>>>_UPD O <<<NOMBRE-TABLA>>>_DEL', @I_ERROR_INFO = @O_RETMSG " + Environment.NewLine +
                   "END;" + Environment.NewLine;
        }

        public String fnNombreTabla(DTO.clsParametros parametros)
        {
            return parametros.Tabla.Substring(3);
        }

        public String fnNombreTablaCamel(DTO.clsParametros parametros)
        {
            return fnInitCap(parametros.Tabla.Substring(3)).Replace("_", "");
        }

        private String fnCamposNombre(Boolean todosVersion = false, Boolean SoloClavePrimaria = false)
        {
            StringBuilder strResultado = new StringBuilder();
            foreach (DTO.clsCampo campo in ListaCampos)
            {
                if (SoloClavePrimaria)
                {
                    // Solo toma en cuenta los campos de la clave primaria
                    if (campo.constraint_type == null || !campo.constraint_type.Equals("PRI")) continue;
                }
                if (todosVersion == false)
                {
                    // No toma en cuenta el campo version
                    if (campo.column_name.ToLower().Equals(strNombreColumnaVersion)) continue;
                }

                if (strResultado.Length > 0)
                    strResultado.Append(",");
                strResultado.Append(campo.column_name);
                strResultado.Append(Environment.NewLine);
            }
            return strResultado.ToString();
        }

        private String fnCamposInputNombre(Boolean todosVersion = false, Boolean SoloClavePrimaria = false)
        {
            StringBuilder strResultado = new StringBuilder();
            foreach (DTO.clsCampo campo in ListaCampos)
            {
                if (SoloClavePrimaria)
                {
                    // Solo toma en cuenta los campos de la clave primaria
                    if (campo.constraint_type == null || !campo.constraint_type.Equals("PRI")) continue;
                }
                if (todosVersion == false)
                {
                    // No toma en cuenta el campo version
                    if (campo.column_name.ToLower().Equals(strNombreColumnaVersion)) continue;
                }

                if (strResultado.Length > 0)
                    strResultado.Append(",");
                strResultado.Append("I_");
                strResultado.Append(fnInitCap(campo.column_name.Substring(3)).Replace("_", ""));
                strResultado.Append(Environment.NewLine);
            }
            return strResultado.ToString();
        }

        private String fnCamposNombreInputWhere(Boolean todosVersion = false, Boolean SoloClavePrimaria = false, String separador = "AND")
        {
            StringBuilder strResultado = new StringBuilder();
            foreach (DTO.clsCampo campo in ListaCampos)
            {
                if (SoloClavePrimaria)
                {
                    // Solo toma en cuenta los campos de la clave primaria
                    if (campo.constraint_type == null || !campo.constraint_type.Equals("PRI")) continue;
                }
                if (todosVersion == false)
                {
                    // No toma en cuenta el campo version
                    if (campo.column_name.ToLower().Equals(strNombreColumnaVersion)) continue;
                }

                if (strResultado.Length > 0)
                    strResultado.Append(separador + " ");
                strResultado.Append(campo.column_name);
                strResultado.Append(" = I_");
                strResultado.Append(fnInitCap(campo.column_name.Substring(3).Replace("_", "")));
                strResultado.Append(Environment.NewLine);
            }
            return strResultado.ToString();
        }

        private String fnCamposNombreInputWhereConvert(Boolean todosVersion = false, Boolean SoloClavePrimaria = false)
        {
            StringBuilder strResultado = new StringBuilder();
            foreach (DTO.clsCampo campo in ListaCampos)
            {
                if (SoloClavePrimaria)
                {
                    // Solo toma en cuenta los campos de la clave primaria
                    if (campo.constraint_type == null || !campo.constraint_type.Equals("PRI")) continue;
                }
                if (todosVersion == false)
                {
                    // No toma en cuenta el campo version
                    if (campo.column_name.ToLower().Equals(strNombreColumnaVersion)) continue;
                }

                if (strResultado.Length > 0)
                    strResultado.Append(" || AND ");
                strResultado.Append(campo.column_name);
                switch (campo.data_type)
                {
                    case "char":
                    case "nchar":
                    case "ntext":
                    case "nvarchar":
                    case "text":
                    case "varchar":
                        strResultado.Append(" = ' || I_");
                        strResultado.Append(fnInitCap(campo.column_name.Substring(3)));
                        //strResultado.Append(" || '");
                        break;
                    default:
                        strResultado.Append(" = ' || CONVERT(VARCHAR,I_");
                        strResultado.Append(fnInitCap(campo.column_name.Substring(3)).Replace("_", ""));
                        strResultado.Append(")");
                        break;
                }
                strResultado.Append(";");
                strResultado.Append(Environment.NewLine);
            }
            return strResultado.ToString();
        }

        private String fnCamposInputTipo(Boolean todosVersion = false, Boolean SoloClavePrimaria = false)
        {
            StringBuilder strResultado = new StringBuilder();
            foreach (DTO.clsCampo campo in ListaCampos)
            {
                if (SoloClavePrimaria)
                {
                    // Solo toma en cuenta los campos de la clave primaria
                    if (campo.constraint_type == null || !campo.constraint_type.Equals("PRI")) continue;
                }
                if (todosVersion == false)
                {
                    // No toma en cuenta el campo version
                    if (campo.column_name.ToLower().Equals(strNombreColumnaVersion)) continue;
                }
                if (strResultado.Length > 0)
                    strResultado.Append(",");
                strResultado.Append("I_");
                strResultado.Append(fnInitCap(campo.column_name.Substring(3)).Replace("_", ""));
                strResultado.Append(" ");
                switch (campo.data_type)
                {
                    case "varchar":
                        strResultado.Append("varchar(" + campo.character_maximum_length.ToString() + ")");
                        break;
                    case "char":
                        strResultado.Append("char(" + campo.character_maximum_length.ToString() + ")");
                        break;
                    case "decimal":
                        strResultado.Append("decimal(" + campo.numeric_precision.ToString() + ", " + campo.numeric_scale.ToString() + ")");
                        break;
                    default:
                        strResultado.Append(campo.data_type);
                        break;
                }
                //if (campo.constraint_type == null || !campo.constraint_type.Equals("PRI"))
                //    strResultado.Append(" = NULL");
                strResultado.Append(Environment.NewLine);
            }
            return strResultado.ToString();
        }

        private String fnCamposDTO(Boolean todosVersion = false, Boolean SoloClavePrimaria = false)
        {
            StringBuilder strResultado = new StringBuilder();
            foreach (DTO.clsCampo campo in ListaCampos)
            {
                if (SoloClavePrimaria)
                {
                    // Solo toma en cuenta los campos de la clave primaria
                    if (campo.constraint_type == null || !campo.constraint_type.Equals("PRI")) continue;
                }
                if (todosVersion == false)
                {
                    // No toma en cuenta el campo version
                    if (campo.column_name.ToLower().Equals(strNombreColumnaVersion)) continue;
                }
                strResultado.Append("[DataMember]");
                strResultado.Append(Environment.NewLine);
                strResultado.Append("public ");
                switch (campo.data_type)
                {
                    case "binary": strResultado.Append("Byte[]?"); break;
                    case "bit": strResultado.Append("Boolean?"); break;
                    case "char": strResultado.Append("String"); break;
                    case "date": strResultado.Append("DateTime?"); break;
                    case "datetime": strResultado.Append("DateTime?"); break;
                    case "datetime2": strResultado.Append("DateTime?"); break;
                    case "datetimeoffset": strResultado.Append("DateTimeOffset?"); break;
                    case "decimal": strResultado.Append("Decimal?"); break;
                    case "float": strResultado.Append("Double?"); break;
                    case "image": strResultado.Append("Byte[]?"); break;
                    case "int": strResultado.Append("Int32?"); break;
                    case "money": strResultado.Append("Decimal?"); break;
                    case "nchar": strResultado.Append("String"); break;
                    case "ntext": strResultado.Append("String"); break;
                    case "numeric": strResultado.Append("Decimal?"); break;
                    case "nvarchar": strResultado.Append("String"); break;
                    case "real": strResultado.Append("Single?"); break;
                    case "rowversion": strResultado.Append("Byte[]?"); break;
                    case "smalldatetime": strResultado.Append("DateTime?"); break;
                    case "smallint": strResultado.Append("Int16?"); break;
                    case "smallmoney": strResultado.Append("Decimal?"); break;
                    case "sql_variant": strResultado.Append("Object *?"); break;
                    case "text": strResultado.Append("String"); break;
                    case "time": strResultado.Append("TimeSpan?"); break;
                    case "timestamp": strResultado.Append("Byte[]?"); break;
                    case "tinyint": strResultado.Append("Byte?"); break;
                    case "uniqueidentifier": strResultado.Append("Guid?"); break;
                    case "varbinary": strResultado.Append("Byte[]?"); break;
                    case "varchar": strResultado.Append("String"); break;
                    case "xml": strResultado.Append("Xml?"); break;
                    default:
                        strResultado.Append("// tipo de dato no registrado");
                        break;
                }
                strResultado.Append(" ");
                strResultado.Append(fnInitCap(campo.column_name.Substring(3)).Replace("_", ""));
                strResultado.Append(" {get; set;}");
                strResultado.Append(Environment.NewLine);
            }
            return strResultado.ToString();
        }

        private String fnCamposDTOJSON(Boolean todosVersion = false, Boolean SoloClavePrimaria = false)
        {
            StringBuilder strResultado = new StringBuilder();
            foreach (DTO.clsCampo campo in ListaCampos)
            {
                if (SoloClavePrimaria)
                {
                    // Solo toma en cuenta los campos de la clave primaria
                    if (campo.constraint_type == null || !campo.constraint_type.Equals("PRI")) continue;
                }
                if (todosVersion == false)
                {
                    // No toma en cuenta el campo version
                    if (campo.column_name.ToLower().Equals(strNombreColumnaVersion)) continue;
                }
                strResultado.Append("[JsonPropertyName(\"" + fnInitCap(campo.column_name.Substring(3)).Replace("_", "") + "\")]");
                strResultado.Append(Environment.NewLine);
                strResultado.Append("public ");
                switch (campo.data_type)
                {
                    case "binary": strResultado.Append("Byte[]?"); break;
                    case "bit": strResultado.Append("Boolean?"); break;
                    case "char": strResultado.Append("String"); break;
                    case "date": strResultado.Append("DateTime?"); break;
                    case "datetime": strResultado.Append("DateTime?"); break;
                    case "datetime2": strResultado.Append("DateTime?"); break;
                    case "datetimeoffset": strResultado.Append("DateTimeOffset?"); break;
                    case "decimal": strResultado.Append("Decimal?"); break;
                    case "float": strResultado.Append("Double?"); break;
                    case "image": strResultado.Append("Byte[]?"); break;
                    case "int": strResultado.Append("Int32?"); break;
                    case "money": strResultado.Append("Decimal?"); break;
                    case "nchar": strResultado.Append("String"); break;
                    case "ntext": strResultado.Append("String"); break;
                    case "numeric": strResultado.Append("Decimal?"); break;
                    case "nvarchar": strResultado.Append("String"); break;
                    case "real": strResultado.Append("Single?"); break;
                    case "rowversion": strResultado.Append("Byte[]?"); break;
                    case "smalldatetime": strResultado.Append("DateTime?"); break;
                    case "smallint": strResultado.Append("Int16?"); break;
                    case "smallmoney": strResultado.Append("Decimal?"); break;
                    case "sql_variant": strResultado.Append("Object *?"); break;
                    case "text": strResultado.Append("String"); break;
                    case "time": strResultado.Append("TimeSpan?"); break;
                    case "timestamp": strResultado.Append("Byte[]?"); break;
                    case "tinyint": strResultado.Append("Byte?"); break;
                    case "uniqueidentifier": strResultado.Append("Guid?"); break;
                    case "varbinary": strResultado.Append("Byte[]?"); break;
                    case "varchar": strResultado.Append("String"); break;
                    case "xml": strResultado.Append("Xml?"); break;
                    default:
                        strResultado.Append("// tipo de dato no registrado");
                        break;
                }
                strResultado.Append(" ");
                strResultado.Append(fnInitCap(campo.column_name.Substring(3)).Replace("_", ""));
                strResultado.Append(" {get; set;}");
                strResultado.Append(Environment.NewLine);
            }
            return strResultado.ToString();
        }

        private String fnCamposCLS(Boolean todosVersion = false, Boolean SoloClavePrimaria = false)
        {
            StringBuilder strResultado = new StringBuilder();
            foreach (DTO.clsCampo campo in ListaCampos)
            {
                if (SoloClavePrimaria)
                {
                    // Solo toma en cuenta los campos de la clave primaria
                    if (campo.constraint_type == null || !campo.constraint_type.Equals("PRI")) continue;
                }
                if (todosVersion == false)
                {
                    // No toma en cuenta el campo version
                    if (campo.column_name.ToLower().Equals(strNombreColumnaVersion)) continue;
                }
                strResultado.Append(fnInitCap(campo.column_name.Substring(3)).Replace("_", "") + ": ");
                switch (campo.data_type)
                {
                    case "binary": strResultado.Append("Number"); break;
                    case "bit": strResultado.Append("Boolean"); break;
                    case "char": strResultado.Append("String"); break;
                    case "date": strResultado.Append("Date"); break;
                    case "datetime": strResultado.Append("Date"); break;
                    case "datetime2": strResultado.Append("Date"); break;
                    case "datetimeoffset": strResultado.Append("Number?"); break;
                    case "decimal": strResultado.Append("Number"); break;
                    case "float": strResultado.Append("Number"); break;
                    case "image": strResultado.Append("String"); break;
                    case "int": strResultado.Append("Number"); break;
                    case "money": strResultado.Append("Number"); break;
                    case "nchar": strResultado.Append("String"); break;
                    case "ntext": strResultado.Append("String"); break;
                    case "numeric": strResultado.Append("Number"); break;
                    case "nvarchar": strResultado.Append("String"); break;
                    case "real": strResultado.Append("Number"); break;
                    case "rowversion": strResultado.Append("String"); break;
                    case "smalldatetime": strResultado.Append("Date"); break;
                    case "smallint": strResultado.Append("Number"); break;
                    case "smallmoney": strResultado.Append("Number"); break;
                    case "sql_variant": strResultado.Append("String"); break;
                    case "text": strResultado.Append("String"); break;
                    case "time": strResultado.Append("Date"); break;
                    case "timestamp": strResultado.Append("Date"); break;
                    case "tinyint": strResultado.Append("Number"); break;
                    case "uniqueidentifier": strResultado.Append("String"); break;
                    case "varbinary": strResultado.Append("String"); break;
                    case "varchar": strResultado.Append("String"); break;
                    case "xml": strResultado.Append("String"); break;
                    default:
                        strResultado.Append("// tipo de dato no registrado");
                        break;
                }
                strResultado.Append(";");
                strResultado.Append(Environment.NewLine);
            }
            return strResultado.ToString();
        }

        private String fnCamposLimpiar(DTO.clsParametros parametros, Boolean todosVersion = false, Boolean SoloClavePrimaria = false)
        {
            StringBuilder strResultado = new StringBuilder();
            foreach (DTO.clsCampo campo in ListaCampos)
            {
                if (SoloClavePrimaria)
                {
                    // Solo toma en cuenta los campos de la clave primaria
                    if (campo.constraint_type == null || !campo.constraint_type.Equals("PRI")) continue;
                }
                if (todosVersion == false)
                {
                    // No toma en cuenta el campo version
                    if (campo.column_name.ToLower().Equals(strNombreColumnaVersion)) continue;
                }
                strResultado.Append("this." + parametros.Tabla.Substring(3).ToLower().Replace("_", "") +
                     "." + fnInitCap(campo.column_name.Substring(3)).Replace("_", "") + " = ");
                switch (campo.data_type)
                {
                    case "binary": strResultado.Append("null"); break;
                    case "bit": strResultado.Append("null"); break;
                    case "char": strResultado.Append("''"); break;
                    case "date": strResultado.Append("null"); break;
                    case "datetime": strResultado.Append("null"); break;
                    case "datetime2": strResultado.Append("null"); break;
                    case "datetimeoffset": strResultado.Append("null"); break;
                    case "decimal": strResultado.Append("null"); break;
                    case "float": strResultado.Append("null"); break;
                    case "image": strResultado.Append("''"); break;
                    case "int": strResultado.Append("null"); break;
                    case "money": strResultado.Append("null"); break;
                    case "nchar": strResultado.Append("''"); break;
                    case "ntext": strResultado.Append("''"); break;
                    case "numeric": strResultado.Append("null"); break;
                    case "nvarchar": strResultado.Append("''"); break;
                    case "real": strResultado.Append("null"); break;
                    case "rowversion": strResultado.Append("''"); break;
                    case "smalldatetime": strResultado.Append("null"); break;
                    case "smallint": strResultado.Append("null"); break;
                    case "smallmoney": strResultado.Append("null"); break;
                    case "sql_variant": strResultado.Append("''"); break;
                    case "text": strResultado.Append("''"); break;
                    case "time": strResultado.Append("null"); break;
                    case "timestamp": strResultado.Append("null"); break;
                    case "tinyint": strResultado.Append("null"); break;
                    case "uniqueidentifier": strResultado.Append("''"); break;
                    case "varbinary": strResultado.Append("''"); break;
                    case "varchar": strResultado.Append("''"); break;
                    case "xml": strResultado.Append("''"); break;
                    default:
                        strResultado.Append("// tipo de dato no registrado");
                        break;
                }
                strResultado.Append(";");
                strResultado.Append(Environment.NewLine);
            }
            return strResultado.ToString();
        }

        private String fnCamposFiltro(DTO.clsParametros parametros)
        {
            StringBuilder strResultado = new StringBuilder();
            foreach (DTO.clsCampo campo in ListaCampos)
            {
                switch (campo.data_type)
                {
                    case "binary":
                    case "bit":
                    case "date":
                    case "datetime":
                    case "datetime2":
                    case "datetimeoffset":
                    case "decimal":
                    case "float":
                    case "image":
                    case "int":
                    case "money":
                    case "numeric":
                    case "real":
                    case "rowversion":
                    case "smalldatetime":
                    case "smallint":
                    case "smallmoney":
                    case "sql_variant":
                    case "time":
                    case "timestamp":
                    case "tinyint":
                        // this.filtro.Filtro += this.filtro.Filtro.length > 0 && this.mensaje.IdMensaje != null ? " AND " : "";
                        // this.filtro.Filtro += this.mensaje.IdMensaje != null ? "ME_ID_MENSAJE = " + this.mensaje.IdMensaje + "" : "";

                        strResultado.Append("this.filtro.Filtro += this.filtro.Filtro.length > 0 && this." +
                             parametros.Tabla.Substring(3).ToLower().Replace("_", "") +
                             "." + fnInitCap(campo.column_name.Substring(3)).Replace("_", "") + " != null ? \" AND \" : \"\";");
                        strResultado.Append(Environment.NewLine);
                        strResultado.Append("this.filtro.Filtro += this." +
                             parametros.Tabla.Substring(3).ToLower().Replace("_", "") +
                             "." + fnInitCap(campo.column_name.Substring(3)).Replace("_", "") + " != null ? \"" +
                             campo.column_name + " = \" + " +
                             "this." + parametros.Tabla.Substring(3).ToLower().Replace("_", "") +
                             "." + fnInitCap(campo.column_name.Substring(3)).Replace("_", "") +
                             " + \"\" : \"\");");
                        break;

                    case "char":
                    case "nchar":
                    case "ntext":
                    case "nvarchar":
                    case "text":
                    case "uniqueidentifier":
                    case "varbinary":
                    case "varchar":
                    case "xml":
                        //this.filtro.Filtro += this.filtro.Filtro.length > 0 && this.mensaje.Login.length > 0 ? " AND " : "";
                        //this.filtro.Filtro += this.mensaje.Login.length > 0 ? "ME_LOGIN LIKE '%" + this.mensaje.Login + "%'" + "" : "";

                        strResultado.Append("this.filtro.Filtro += this.filtro.Filtro.length > 0 && this." +
                             parametros.Tabla.Substring(3).ToLower().Replace("_", "") +
                             "." + fnInitCap(campo.column_name.Substring(3)).Replace("_", "") + ".length > 0 ? \" AND \" : \"\";");
                        strResultado.Append(Environment.NewLine);
                        strResultado.Append("this.filtro.Filtro += this." +
                             parametros.Tabla.Substring(3).ToLower().Replace("_", "") +
                             "." + fnInitCap(campo.column_name.Substring(3)).Replace("_", "") + ".length > 0 ? \"" +
                             campo.column_name + " LIKE '%\" + " +
                             "this." + parametros.Tabla.Substring(3).ToLower().Replace("_", "") +
                             "." + fnInitCap(campo.column_name.Substring(3)).Replace("_", "") +
                             " + %'\" + \"\" : \"\");");
                        break;

                    default:
                        strResultado.Append("// tipo de dato no registrado");
                        break;
                }
                strResultado.Append(Environment.NewLine);
            }
            return strResultado.ToString();
        }

        private String fnCamposDAL(DTO.clsParametros parametros, Boolean todosVersion = false, Boolean SoloClavePrimaria = false)
        {
            StringBuilder strResultado = new StringBuilder();
            foreach (DTO.clsCampo campo in ListaCampos)
            {
                if (SoloClavePrimaria)
                {
                    // Solo toma en cuenta los campos de la clave primaria
                    if (campo.constraint_type == null || !campo.constraint_type.Equals("PRI")) continue;
                }
                if (todosVersion == false)
                {
                    // No toma en cuenta el campo version
                    if (campo.column_name.ToLower().Equals(strNombreColumnaVersion)) continue;
                }

                // if (Mensaje.IdMensaje != null)
                //cmd.Parameters.Add(new SqlParameter("I_IdMensaje", SqlDbType.Int, Mensaje.IdMensaje.Length, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, Mensaje.IdMensaje));

                strResultado.Append("if (" + fnInitCap(parametros.Tabla.Substring(3)).Replace("_", "") +
                     "." + fnInitCap(campo.column_name.Substring(3)).Replace("_", ""));
                strResultado.Append(" != null)");
                strResultado.Append(Environment.NewLine);
                strResultado.Append("cmd.Parameters.Add(new SqlParameter(\"I_" + fnInitCap(campo.column_name.Substring(3)).Replace("_", "") + "\", SqlDbType.");
                switch (campo.data_type)
                {
                    case "binary": strResultado.Append("Binary"); break;
                    case "bit": strResultado.Append("Bit"); break;
                    case "char": strResultado.Append("Char"); break;
                    case "date": strResultado.Append("DateTime"); break;
                    case "datetime": strResultado.Append("DateTime"); break;
                    case "datetime2": strResultado.Append("DateTime"); break;
                    case "datetimeoffset": strResultado.Append("DateTimeOffset"); break;
                    case "decimal": strResultado.Append("Decimal"); break;
                    case "float": strResultado.Append("Float"); break;
                    case "image": strResultado.Append("Image"); break;
                    case "int": strResultado.Append("Int"); break;
                    case "money": strResultado.Append("Money"); break;
                    case "nchar": strResultado.Append("NChar"); break;
                    case "ntext": strResultado.Append("NText"); break;
                    case "numeric": strResultado.Append("Decimal"); break;
                    case "nvarchar": strResultado.Append("NVarChar"); break;
                    case "real": strResultado.Append("Real"); break;
                    case "rowversion": strResultado.Append("Binary"); break;
                    case "smalldatetime": strResultado.Append("SmallDateTime"); break;
                    case "smallint": strResultado.Append("SmallInt"); break;
                    case "smallmoney": strResultado.Append("SmallMoney"); break;
                    case "sql_variant": strResultado.Append("Binary"); break;
                    case "text": strResultado.Append("VarChar"); break;
                    case "time": strResultado.Append("Time"); break;
                    case "timestamp": strResultado.Append("TimeStamp"); break;
                    case "tinyint": strResultado.Append("TinyInt"); break;
                    case "uniqueidentifier": strResultado.Append("UniqueIdentifier"); break;
                    case "varbinary": strResultado.Append("VarBinary"); break;
                    case "varchar": strResultado.Append("VarChar"); break;
                    case "xml": strResultado.Append("Xml"); break;
                    default:
                        strResultado.Append("// tipo de dato no registrado");
                        break;
                }
                strResultado.Append(", ");
                switch (campo.data_type)
                {
                    case "binary": strResultado.Append("-1"); break;
                    case "bit": strResultado.Append("-1"); break;
                    case "date": strResultado.Append("-1"); break;
                    case "datetime": strResultado.Append("-1"); break;
                    case "datetime2": strResultado.Append("-1"); break;
                    case "datetimeoffset": strResultado.Append("-1"); break;
                    case "decimal": strResultado.Append("-1"); break;
                    case "float": strResultado.Append("-1"); break;
                    case "int": strResultado.Append("-1"); break;
                    case "money": strResultado.Append("-1"); break;
                    case "numeric": strResultado.Append("-1"); break;
                    case "real": strResultado.Append("-1"); break;
                    case "rowversion": strResultado.Append("-1"); break;
                    case "smalldatetime": strResultado.Append("-1"); break;
                    case "smallint": strResultado.Append("-1"); break;
                    case "smallmoney": strResultado.Append("-1"); break;
                    case "sql_variant": strResultado.Append("-1"); break;
                    case "time": strResultado.Append("-1"); break;
                    case "timestamp": strResultado.Append("-1"); break;
                    case "tinyint": strResultado.Append("-1"); break;
                    case "uniqueidentifier": strResultado.Append("-1"); break;
                    case "char":
                    case "image":
                    case "nchar":
                    case "ntext":
                    case "nvarchar":
                    case "text":
                    case "varbinary":
                    case "varchar":
                    case "xml":
                        strResultado.Append(fnInitCap(parametros.Tabla.Substring(3)).Replace("_", "") + "." +
                        fnInitCap(campo.column_name.Substring(3)).Replace("_", "") + ".Length"); break;
                    default:
                        strResultado.Append("// tipo de dato no registrado");
                        break;
                }
                strResultado.Append(", ParameterDirection.Input, false, 0, 0, \"\", DataRowVersion.Default, ");
                strResultado.Append(fnInitCap(parametros.Tabla.Substring(3)).Replace("_", "") +
                     "." + fnInitCap(campo.column_name.Substring(3)).Replace("_", "") + "))");
                strResultado.Append(";");
                strResultado.Append(Environment.NewLine);
            }
            return strResultado.ToString();
        }

        private String fnCamposDALResultado(DTO.clsParametros parametros)
        {
            StringBuilder strResultado = new StringBuilder();
            foreach (DTO.clsCampo campo in ListaCampos)
            {
                // Mensaje.IdMensaje = dr["ME_ID_MENSAJE"] as Int32? ;
                strResultado.Append(fnInitCap(parametros.Tabla.Substring(3)).Replace("_", "") +
                     "." + fnInitCap(campo.column_name.Substring(3)).Replace("_", ""));
                strResultado.Append(" = dr[\"" + campo.column_name + "\"] as ");
                switch (campo.data_type)
                {
                    case "binary": strResultado.Append("Byte[]?"); break;
                    case "bit": strResultado.Append("Boolean?"); break;
                    case "char": strResultado.Append("String"); break;
                    case "date": strResultado.Append("DateTime?"); break;
                    case "datetime": strResultado.Append("DateTime?"); break;
                    case "datetime2": strResultado.Append("DateTime?"); break;
                    case "datetimeoffset": strResultado.Append("DateTimeOffset?"); break;
                    case "decimal": strResultado.Append("Decimal?"); break;
                    case "float": strResultado.Append("Double?"); break;
                    case "image": strResultado.Append("Byte[]?"); break;
                    case "int": strResultado.Append("Int32?"); break;
                    case "money": strResultado.Append("Decimal?"); break;
                    case "nchar": strResultado.Append("String"); break;
                    case "ntext": strResultado.Append("String"); break;
                    case "numeric": strResultado.Append("Decimal?"); break;
                    case "nvarchar": strResultado.Append("String"); break;
                    case "real": strResultado.Append("Single?"); break;
                    case "rowversion": strResultado.Append("Byte[]?"); break;
                    case "smalldatetime": strResultado.Append("DateTime?"); break;
                    case "smallint": strResultado.Append("Int16?"); break;
                    case "smallmoney": strResultado.Append("Decimal?"); break;
                    case "sql_variant": strResultado.Append("Object *?"); break;
                    case "text": strResultado.Append("String"); break;
                    case "time": strResultado.Append("TimeSpan?"); break;
                    case "timestamp": strResultado.Append("Byte[]?"); break;
                    case "tinyint": strResultado.Append("Byte?"); break;
                    case "uniqueidentifier": strResultado.Append("Guid?"); break;
                    case "varbinary": strResultado.Append("Byte[]?"); break;
                    case "varchar": strResultado.Append("String"); break;
                    case "xml": strResultado.Append("Xml?"); break;
                    default:
                        strResultado.Append("// tipo de dato no registrado");
                        break;
                }
                strResultado.Append(";");
                strResultado.Append(Environment.NewLine);
            }
            return strResultado.ToString();
        }

        private String fnCamposWebHTML(DTO.clsParametros parametros)
        {
            StringBuilder strResultado = new StringBuilder();
            foreach (DTO.clsCampo campo in ListaCampos)
            {
                //<div class="ui-g ui-g-12 ui-md-3">
                //<label class="fg-label"><strong>IdMensaje</strong></label>
                //<input type="text" class="form-control material fg-input" [(ngModel)]="mensaje.IdMensaje">
                //</div>
                strResultado.Append("<div class=\"ui-g ui-g-12 ui-md-3\">" + Environment.NewLine);
                strResultado.Append("<label class=\"fg-label\"><strong>" + fnInitCap(campo.column_name.Substring(3)).Replace("_", "") + "</strong></label>" + Environment.NewLine);
                strResultado.Append("<input type=\"text\" class=\"form-control material fg-input\" [(ngModel)]=\"");
                strResultado.Append(parametros.Tabla.Substring(3).ToLower().Replace("_", "") +
                     "." + fnInitCap(campo.column_name.Substring(3)).Replace("_", "") + "\">" + Environment.NewLine);
                strResultado.Append("</div>" + Environment.NewLine);
            }
            return strResultado.ToString();
        }

        private String fnCamposWebFiltro(DTO.clsParametros parametros)
        {
            StringBuilder strResultado = new StringBuilder();
            foreach (DTO.clsCampo campo in ListaCampos)
            {
                //<td width="10%">
                //<input type="text" class="forma-input-sin-izquierda col-xs-12" maxlength="5" [(ngModel)]="mensaje.IdMensaje" placeholder="IdMensaje" />
                //</td>
                strResultado.Append("<td width=\"10%\">" + Environment.NewLine);
                strResultado.Append("<input type=\"text\" class=\"forma-input-sin-izquierda col-xs-12\" maxlength=\"" +
                     campo.character_maximum_length + "\" [(ngModel)]=\"");
                strResultado.Append(parametros.Tabla.Substring(3).ToLower().Replace("_", "") +
                     "." + fnInitCap(campo.column_name.Substring(3)).Replace("_", "") + "\" placeholder=\"" +
                     fnInitCap(campo.column_name.Substring(3)).Replace("_", "") + "\" />" + Environment.NewLine);
                strResultado.Append("</td>" + Environment.NewLine);
            }
            return strResultado.ToString();
        }

        private String fnCamposWebTitulo()
        {
            StringBuilder strResultado = new StringBuilder();
            foreach (DTO.clsCampo campo in ListaCampos)
            {
                //<th width="10%">
                //<a title="Ordenar por IdMensaje" (click)="fnOrdenar('ME_ID_MENSAJE')">IdMensaje</a>
                //<div class="pi {{filtro.Icono}}" style="float: right" *ngIf="filtro.Campo=='ME_ID_MENSAJE'"></div>
                //</th>
                strResultado.Append("<th width=\"10%\">" + Environment.NewLine);
                strResultado.Append("<a title=\"Ordenar por " + fnInitCap(campo.column_name.Substring(3)).Replace("_", "") + "\"" +
                     "(click)=\"fnOrdenar('" + campo.column_name + "')\">" + fnInitCap(campo.column_name.Substring(3)).Replace("_", "") + "</a>" +
                     Environment.NewLine);
                strResultado.Append("<div class=\"pi {{filtro.Icono}}\" style=\"float: right\" *ngIf=\"filtro." +
                     fnInitCap(campo.column_name.Substring(3)).Replace("_", "") + "=='" +
                     campo.column_name + "'\">" + "</div>" +
                     Environment.NewLine);
                strResultado.Append("</th>" + Environment.NewLine);
            }
            return strResultado.ToString();
        }

        private String fnCamposWebDetalle()
        {
            StringBuilder strResultado = new StringBuilder();
            foreach (DTO.clsCampo campo in ListaCampos)
            {
                //<td>{{reg.IdMensaje}}</td>
                strResultado.Append("<td>{{reg." + fnInitCap(campo.column_name.Substring(3)).Replace("_", "") + "}}</td>" + Environment.NewLine);
            }
            return strResultado.ToString();
        }

        private String fnCamposTransactor()
        {
            return "t_ssn           INT,            -- Número secuencial del sistema" + Environment.NewLine +
                   "t_phase         INT,            --Número de fase de la transacción" + Environment.NewLine +
                   "t_ichannel      VARCHAR(64),    --Canal de entrada de la transacción" + Environment.NewLine +
                   "t_idispatcher   INT,            --Número del despachador asignado" + Environment.NewLine +
                   "t_idusr         VARCHAR(128),   --Identificador del usuario en la sesión" + Environment.NewLine +
                   "t_ussn          INT,            --Número secuencial del usuario - recibo" + Environment.NewLine +
                   "t_org           VARCHAR(64),    --Aplicación de origen de la transacción" + Environment.NewLine +
                   "t_filial        VARCHAR(64),    --Filial de origen" + Environment.NewLine +
                   "t_office        VARCHAR(64),    --Oficina de origen" + Environment.NewLine +
                   "t_term          VARCHAR(32),    --Terminal de origen" + Environment.NewLine;
        }

        private String fnLogInternoTransactor(DTO.clsParametros parametros)
        {
            return "EXEC fw_security.DBO.Log_Interno_ADD t_filial, t_idusr, t_org, ERRORINFO, " + fnNombreTabla(parametros);
        }

        private String fnInitCap(String strInput)
        {
            int intIndex = 0;
            char chrActual;
            char chrAnterior;
            StringBuilder strOutput = new StringBuilder();
            while (intIndex < strInput.Length)
            {
                chrActual = strInput[intIndex];
                chrAnterior = (intIndex <= 0) ? ' ' : strInput[intIndex - 1];
                if (chrAnterior.Equals(' ') ||
                     chrAnterior.Equals(';') ||
                     chrAnterior.Equals(':') ||
                     chrAnterior.Equals('!') ||
                     chrAnterior.Equals('?') ||
                     chrAnterior.Equals(',') ||
                     chrAnterior.Equals('.') ||
                     chrAnterior.Equals('_') ||
                     chrAnterior.Equals('-') ||
                     chrAnterior.Equals('/') ||
                     chrAnterior.Equals('&') ||
                     chrAnterior.Equals('(') ||
                     chrAnterior.Equals(')')
                     )
                {
                    strOutput.Append(chrActual.ToString().ToUpper());
                }
                else
                    strOutput.Append(chrActual.ToString().ToLower());
                intIndex += 1;
            }
            return strOutput.ToString();
        }


    }
}
