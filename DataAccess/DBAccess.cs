using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System;

namespace ProyectoBD2.DataAccess
{
    /*Esta clase la cree para evitar exponer la cadena de conexion por todas partes del 
     programa y tambien para facilitar muchas cosas y reutilizar codigo*/
    public static class DBAccess
    {
        private static readonly string connectionString = "Server=3.128.144.165; Database=DB20202000577; UID=jancy.leiva; PWD=JL20202000577; TrustServerCertificate=True;";

    public static void ExecuteStoredProcedureNonQuery(string nombreSP, Dictionary<string, (object valor, ParameterDirection? direccion)> parametros, ref Dictionary<string, object> valoresSalida)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(nombreSP, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                foreach (var param in parametros)
                {
                    SqlParameter sqlParam = new SqlParameter(param.Key, param.Value.valor ?? DBNull.Value);
                    if (param.Value.direccion.HasValue)
                    {
                        sqlParam.Direction = param.Value.direccion.Value;

                        if (sqlParam.SqlDbType == SqlDbType.NVarChar || sqlParam.SqlDbType == SqlDbType.VarChar)
                            sqlParam.Size = 1000;
                    }

                    cmd.Parameters.Add(sqlParam);
                }

                conn.Open();
                cmd.ExecuteNonQuery();

                foreach (SqlParameter p in cmd.Parameters)
                {
                    if (p.Direction == ParameterDirection.Output || p.Direction == ParameterDirection.InputOutput)
                    {
                        valoresSalida[p.ParameterName] = p.Value;
                    }
                }
            }
        }
    }
}