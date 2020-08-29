/********************************************************************************************
	Archivo         :	dalTransaccion.cs    									
	Diseñado por	:	FRAMEWORKS CIA LTDA										
	Módulo			:	OpenGenerator
	Descripción	    :	Administración de Transacciones
																				
********************************************************************************************
	Este programa es parte del paquete de OpenGenerator propiedad de FRAMEWORKS CIA LTDA
	Su uso no autorizado queda expresamente prohibido asi como					
	cualquier alteracion o agregado hecho por alguno de sus						
	usuarios sin el debido consentimiento por escrito de FRAMEWORKS CIA LTDA
********************************************************************************************
	Fecha de Escritura:	Sep 7 2019  6:40PM											
	Autor		  :	Patricio Martínez										
********************************************************************************************
	MODIFICACIONES																
	Fecha		Autor		        Razón										    
    2020-06-25  Patricio Martínez   Adaptación para MediaCollector                                                                       
********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Configuration;
using System.Diagnostics;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace OpenGeneratorSVC.DAL
{
    public class dalTransaccion
    {
        private static string pattern = @"INSERT\b|UPDATE\b|DELETE\b|MERGE\b";
        private static Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
        private const int MAXINTENTOS = 3;
        private const int SLEEPMILIS = 500;
        private const int MAXCHARLOG = 4000;    // Número máximo de caracteres que van al log
        private char[] SEPARADOR = { ',' };

        private DateTime dtInicio;              // Fecha y hora de inicio de la transacción
        private String strEntrada;
        private StringBuilder sbComando;
        protected string strConexion = ConfigurationBinder.GetValue<String>(Startup.Configuration, "ConnectionStrings:BDD_GENERATOR");

        public bool validarTransaccion(string usuario, object entrada, MySqlCommand comando, out DTO.clsResultado resultado)
        {
            if (usuario == null || usuario.Length <= 0)
            {
                resultado = new DTO.clsResultado();
                resultado.Resultado = -11;
                resultado.Mensaje = comando.CommandText + " - API.20002: Usuario no autorizado";
                return false;
            }
            iniciarTransacion(usuario, entrada, comando, out resultado);

            return validar(usuario, comando);
        }

        /// <summary>
        /// Valida el comando mediante búsqueda de INSERT, UPDATE, DELETE en su nombre o parámetros
        /// </summary>
        /// <param name="cmd">Comando a ejecutarse</param>
        /// <returns>true .- Si el comando ha sido validado correctamente, de otro modo genera una excepción</returns>
        public bool validar(string usuario, MySqlCommand cmd)
        {
            MatchCollection listaMatchComando = rgx.Matches(cmd.CommandText);
            if (listaMatchComando.Count > 0) throw (new Exception("Posible Inyección de SQL en el comando : " + cmd.CommandText));
            foreach (MySqlParameter par in cmd.Parameters)
            {
                if (par.MySqlDbType.Equals(SqlDbType.Char) ||
                    par.MySqlDbType.Equals(SqlDbType.NChar) ||
                    par.MySqlDbType.Equals(SqlDbType.NVarChar) ||
                    par.MySqlDbType.Equals(SqlDbType.Text) ||
                    par.MySqlDbType.Equals(SqlDbType.NText) ||
                    par.MySqlDbType.Equals(SqlDbType.VarChar))
                {
                    MatchCollection listaMatchParam = rgx.Matches(par.Value.ToString());
                    if (listaMatchParam.Count > 0)
                        throw (new Exception("Posible Inyección de SQL en el comando : " + cmd.CommandText +
                            " Parámetro: " + par.ParameterName + " Valor: " + par.Value.ToString()));
                }
            }

            return true;
        }

        public void iniciarTransacion(string usuario, object entrada, MySqlCommand comando, out DTO.clsResultado resultado)
        {
            resultado = new DTO.clsResultado();
            dtInicio = DateTime.Now;                        // Toma la fecha y hora inicial de la transacción
            return;
/*
            // No registrar en la BDD las transacciones de la lista
            List<string> ListaTranNoRegistrar = ConfigurationBinder.GetValue<String>(Startup.Configuration, "TransaccionNoRegistrar").Split(SEPARADOR).ToList();
            if (ListaTranNoRegistrar.Contains(comando.CommandText))
            {
                resultado.TrId = 0;
                return;
            }

            strEntrada = JsonConvert.SerializeObject(entrada);
            if (strEntrada.Length > MAXCHARLOG) strEntrada = strEntrada.Substring(0, MAXCHARLOG);

            // Construir el comando
            sbComando = new StringBuilder();
            sbComando.Append("{");
            foreach (MySqlParameter par in comando.Parameters)
            {
                if (par.Value != DBNull.Value && par.Value != null)
                {
                    if (par.ParameterName == "@I_Password")     // No registrar la password
                        continue;
                    sbComando.Append("\"" + par.ParameterName + "\":" + JsonConvert.SerializeObject(par.Value));
                }
                else
                {
                    sbComando.Append("\"" + par.ParameterName + "\":null");
                }
                if (comando.Parameters.IndexOf(par) != comando.Parameters.Count - 1)
                    sbComando.Append(",");
            }
            sbComando.Append("}");

            try
            {
                using (MySqlConnection con = new MySqlConnection(strConexion))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand("Transaccion_ADD", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(new MySqlParameter("I_Transaccion", SqlDbType.VarChar, comando.CommandText.Length, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, comando.CommandText));
                        cmd.Parameters.Add(new SqlParameter("I_Email", SqlDbType.VarChar, usuario.Length, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, usuario));
                        cmd.Parameters.Add(new SqlParameter("I_FechaIni", SqlDbType.DateTime, -1, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, dtInicio));
                        cmd.Parameters.Add(new SqlParameter("I_Parametros", SqlDbType.VarChar, sbComando.Length, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, sbComando.ToString()));

                        cmd.Parameters.Add(new SqlParameter("@O_RETVAL", SqlDbType.Int, 0, ParameterDirection.Output, false, 0, 0, "", DataRowVersion.Default, 0));
                        cmd.Parameters.Add(new SqlParameter("@O_RETMSG", SqlDbType.NVarChar, 128, ParameterDirection.Output, false, 0, 0, "", DataRowVersion.Default, ""));

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                resultado.TrId = dr["TR_ID"] as Int64?;
                            }
                        }
                        resultado.Resultado = Convert.ToInt32(cmd.Parameters["@O_RETVAL"].Value);
                        resultado.Mensaje = cmd.Parameters["@O_RETMSG"].Value.ToString();
                    }
                    con.Close();
                }
            }
            catch (Exception error)
            {
                Startup.Logger.LogError("Error en IniciarTransaccion ... " + " Descripción=  " + error.Message + " Stack: " + error.StackTrace, null);
            }

*/
        }

        public void registrarTransaccion(object salida, DTO.clsResultado resultado)
        {
            DateTime dtFinal = DateTime.Now;                        // Toma la fecha y hora inicial de la transacción
            int cmdResult = 0;
            string cmdMensaje = "";
            return;
/*
            // No registrar en la BDD las transacciones de la lista
            if (resultado.TrId <= 0)
                return;

            try
            {
                using (SqlConnection con = new SqlConnection(strConexion))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("Transaccion_UPD", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(new SqlParameter("I_Id", SqlDbType.BigInt, -1, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, resultado.TrId));
                        cmd.Parameters.Add(new SqlParameter("I_FechaFin", SqlDbType.DateTime, -1, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, dtFinal));
                        cmd.Parameters.Add(new SqlParameter("I_Error", SqlDbType.Int, -1, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, resultado.Resultado));
                        cmd.Parameters.Add(new SqlParameter("I_Mensaje", SqlDbType.VarChar, resultado.Mensaje.Length, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, resultado.Mensaje));

                        cmd.Parameters.Add(new SqlParameter("@O_RETVAL", SqlDbType.Int, 0, ParameterDirection.Output, false, 0, 0, "", DataRowVersion.Default, 0));
                        cmd.Parameters.Add(new SqlParameter("@O_RETMSG", SqlDbType.NVarChar, 128, ParameterDirection.Output, false, 0, 0, "", DataRowVersion.Default, ""));

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                            }
                        }
                        cmdResult = Convert.ToInt32(cmd.Parameters["@O_RETVAL"].Value);
                        cmdMensaje = cmd.Parameters["@O_RETMSG"].Value.ToString();
                    }
                    con.Close();
                }
            }
            catch (Exception error)
            {
                Startup.Logger.LogError("Error en RegistrarTransaccion... " + " Descripción=  " + error.Message + " Stack: " + error.StackTrace, null);
            }
*/
        }

        /// <summary>
        /// Consulta de Transaccion, puede ser individual (por ID) o con filtros y órdenes.
        /// </summary>
        /// <param name="filtro">El filtro indica el modo, página, condiciones y orden de la consulta. Si viene el ID y modo=0, se consulta el registro específico</param>
        /// <param name="resultado">Contiene el Código, Mensaje y Número de páginas obtenidos como resultados de la consulta</param>
        /// <returns>Retorna la lista de transacciones</returns>
        //public List<Models.clsTransaccion> TransaccionConsultar(Models.clsFiltro filtro, out Models.clsResultado resultado)
        //{
        //    string strConexion = ConfigurationManager.ConnectionStrings["BDD_YES"].ConnectionString;
        //    List<Models.clsTransaccion> datos = new List<Models.clsTransaccion>();
        //    resultado = new Models.clsResultado();
        //    try
        //    {
        //        using (SqlConnection con = new SqlConnection(strConexion))
        //        {
        //            con.Open();
        //            using (SqlCommand cmd = new SqlCommand("Transaccion_QRY", con))
        //            {
        //                cmd.CommandType = CommandType.StoredProcedure;

        //                if (filtro.Id != null && filtro.Id > 0)
        //                    cmd.Parameters.Add(new SqlParameter("@I_ID", SqlDbType.Int, -1, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, filtro.Id));

        //                cmd.Parameters.Add(new SqlParameter("@I_MODO", SqlDbType.Int, -1, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, filtro.Modo));
        //                cmd.Parameters.Add(new SqlParameter("@I_FILAS", SqlDbType.Int, -1, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, filtro.Filas));

        //                if (filtro.Filtro != null && filtro.Filtro.Length > 0)
        //                    cmd.Parameters.Add(new SqlParameter("@I_FILTRO", SqlDbType.VarChar, filtro.Filtro.Length, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, filtro.Filtro));
        //                if (filtro.Orden != null && filtro.Orden.Length > 0)
        //                    cmd.Parameters.Add(new SqlParameter("@I_ORDEN", SqlDbType.VarChar, filtro.Orden.Length, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, filtro.Orden));

        //                cmd.Parameters.Add(new SqlParameter("@I_IR_A_PAGINA", SqlDbType.Int, -1, ParameterDirection.Input, false, 0, 0, "", DataRowVersion.Default, filtro.Pagina));

        //                cmd.Parameters.Add(new SqlParameter("@O_ROWS", SqlDbType.Int, 0, ParameterDirection.Output, false, 0, 0, "", DataRowVersion.Default, 0));
        //                cmd.Parameters.Add(new SqlParameter("@O_PAGES", SqlDbType.Int, 0, ParameterDirection.Output, false, 0, 0, "", DataRowVersion.Default, 0));
        //                cmd.Parameters.Add(new SqlParameter("@O_RETVAL", SqlDbType.Int, 0, ParameterDirection.Output, false, 0, 0, "", DataRowVersion.Default, 0));
        //                cmd.Parameters.Add(new SqlParameter("@O_RETMSG", SqlDbType.NVarChar, 256, ParameterDirection.Output, false, 0, 0, "", DataRowVersion.Default, ""));

        //                string strFuncionarioTran = BLL.bllTokens.GetLogin(filtro.Token);
        //                if (validarTransaccion(strFuncionarioTran, filtro, cmd, out resultado))
        //                {
        //                    using (SqlDataReader dr = cmd.ExecuteReader())
        //                    {
        //                        while (dr.Read())
        //                        {
        //                            DTO.clsTransaccion tran = new DTO.clsTransaccion();
        //                            tran.Id = dr["TR_ID"] as Int64?;
        //                            tran.Fecha = dr["TR_FECHA"] as DateTime?;
        //                            tran.Tiempo = dr["TR_TIEMPO"] as Int32?;
        //                            tran.Login = dr["TR_LOGIN"] as String;
        //                            tran.Transaccion = dr["TR_TRANSACCION"] as String;
        //                            tran.Entrada = dr["TR_ENTRADA"] as String;
        //                            tran.Comando = dr["TR_COMANDO"] as String;
        //                            tran.Salida = dr["TR_SALIDA"] as String;
        //                            tran.Resultado = dr["TR_RESULTADO"] as Int32?;
        //                            tran.Mensaje = dr["TR_MENSAJE"] as String;
        //                            datos.Add(tran);
        //                        }
        //                    }
        //                    resultado.Resultado = Convert.ToInt32(cmd.Parameters["@O_RETVAL"].Value);
        //                    resultado.Mensaje = cmd.Parameters["@O_RETMSG"].Value.ToString();
        //                    resultado.TotalPaginas = (cmd.Parameters["@O_PAGES"].Value != DBNull.Value) ? Convert.ToInt32(cmd.Parameters["@O_PAGES"].Value) : 0;
        //                    resultado.TotalRegistros = (cmd.Parameters["@O_ROWS"].Value != DBNull.Value) ? Convert.ToInt32(cmd.Parameters["@O_ROWS"].Value) : 0;
        //                }
        //            }

        //            con.Close();
        //        }
        //    }
        //    catch (Exception error)
        //    {
        //        string strSource = ConfigurationManager.AppSettings["NombreLog"];
        //        using (EventLog eventLog = new System.Diagnostics.EventLog("Application", Environment.MachineName, strSource))
        //        {
        //            eventLog.WriteEntry("Error en DAL TransaccionConsultar... " + " Descripción=  " + error.Message + " Stack: " + error.StackTrace, EventLogEntryType.Error, 0);
        //        }
        //        resultado.Resultado = -10;
        //        resultado.Mensaje = ConfigurationManager.AppSettings["ErrorInternoMensaje"];
        //    }
        //    return datos;
        //}

    }
}