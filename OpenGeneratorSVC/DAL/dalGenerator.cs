using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace OpenGeneratorSVC.DAL
{
    public class dalGenerator: dalTransaccion
    {
        public List<String> fnConsultarBases(out DTO.clsResultado resultado)
        {
            List<String> ListaBases = new List<string>();
            resultado = new DTO.clsResultado();
            try
            {
                using (MySqlConnection con = new MySqlConnection(strConexion))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand("show DATABASES;", con))
                    {
                        cmd.CommandType = System.Data.CommandType.Text;

                        using (MySqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                String dato = dr[0] as string;
                                ListaBases.Add(dato);
                            }
                        }
                    }
                    con.Close();

                }
            }
            catch (Exception error)
            {
                resultado.Resultado = -1;
                resultado.Mensaje = "Se ha producido un error al consultar las Bases de Datos: " + error.Message + "Stack: " + error.StackTrace;
            }
            return ListaBases;
        }

        public List<String> fnConsultarTablas(string esquema, out DTO.clsResultado resultado)
        {
            List<String> ListaTablas = new List<string>();
            resultado = new DTO.clsResultado();
            try
            {
                using (MySqlConnection con = new MySqlConnection(strConexion))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand("select TABLE_NAME from INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='" + esquema + "';", con))
                    {
                        cmd.CommandType = System.Data.CommandType.Text;

                        using (MySqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                String dato = dr[0] as string;
                                ListaTablas.Add(dato);
                            }
                        }
                    }
                    con.Close();

                }
            }
            catch (Exception error)
            {
                resultado.Resultado = -1;
                resultado.Mensaje = "Se ha producido un error al consultar las tablas de la Bdd: " + esquema + 
                    ": " + error.Message + "Stack: " + error.StackTrace;
            }
            return ListaTablas;
        }

        public List<String> fnConsultarPlantillas(out DTO.clsResultado resultado)
        {
            List<String> ListaPlantillas = new List<string>();
            resultado = new DTO.clsResultado();
            try
            {
                using (MySqlConnection con = new MySqlConnection(strConexion))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT te_id FROM generator.template;", con))
                    {
                        cmd.CommandType = System.Data.CommandType.Text;

                        using (MySqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                String dato = dr[0] as string;
                                ListaPlantillas.Add(dato);
                            }
                        }
                    }
                    con.Close();

                }
            }
            catch (Exception error)
            {
                resultado.Resultado = -1;
                resultado.Mensaje = "Se ha producido un error al consultar las Plantillas: " + error.Message + "Stack: " + error.StackTrace;
            }
            return ListaPlantillas;
        }

        public String fnConsultarTextoPlantilla(string CodigoPlantilla, out DTO.clsResultado resultado)
        {
            resultado = new DTO.clsResultado();
            try
            {
                using (MySqlConnection con = new MySqlConnection(strConexion))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand("select te_code from generator.template WHERE te_id='" + CodigoPlantilla + "';", con))
                    {
                        cmd.CommandType = System.Data.CommandType.Text;

                        using (MySqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                String dato = dr[0] as string;
                                return dato;
                            }
                        }
                    }
                    con.Close();

                }
            }
            catch (Exception error)
            {
                resultado.Resultado = -1;
                resultado.Mensaje = "Se ha producido un error al consultar el texto de la plantilla: " + CodigoPlantilla+
                    ": " + error.Message + "Stack: " + error.StackTrace;
            }
            return "";
        }

        public List<DTO.clsCampo> fnConsultarCampos(DTO.clsParametros parametros, out DTO.clsResultado resultado)
        {
            List<DTO.clsCampo> ListaCampos = new List<DTO.clsCampo>();
            resultado = new DTO.clsResultado();
            try
            {
                using (MySqlConnection con = new MySqlConnection(strConexion))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand(
                        "SELECT COLUMN_NAME, ORDINAL_POSITION, IS_NULLABLE, " +
                        "DATA_TYPE, " +
                        "CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE, " +
                        "DATETIME_PRECISION, COLUMN_KEY " +
                        "FROM INFORMATION_SCHEMA.COLUMNS " +
                        "WHERE table_name = '" + parametros.Tabla + "' " +
                        "AND table_schema = '" + parametros.BaseDeDatos + "' " +
                        "ORDER BY ORDINAL_POSITION", con))
                    {
                        cmd.CommandType = System.Data.CommandType.Text;

                        using (MySqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                DTO.clsCampo campo = new DTO.clsCampo();
                                campo.column_name = dr[0] as string;
                                campo.ordinal_position = dr[1] as uint?;
                                campo.is_nullable = (dr[2] as string).ToUpper().Equals("YES") ? true: false;
                                campo.data_type = dr[3] as string;
                                campo.character_maximum_length = dr[4] as long?;
                                campo.numeric_precision = dr[5] as long?;
                                campo.numeric_scale = dr[6] as long?;
                                campo.datetime_precision = dr[7] as long?;
                                campo.constraint_type = dr[8] as string;
                                ListaCampos.Add(campo);
                            }
                        }
                    }
                    con.Close();

                }
            }
            catch (Exception error)
            {
                resultado.Resultado = -1;
                resultado.Mensaje = "Se ha producido un error al consultar los campos de : " + parametros.Tabla +
                    ": " + error.Message + "Stack: " + error.StackTrace;
            }
            return ListaCampos;
        }

    }
}
