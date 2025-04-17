using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System;

namespace ProyectoBD2.DataAccess
{
    public static class DBAccess
    {
        private static readonly string connectionString =
            "Server=3.128.144.165; Database=DB20202000577; UID=jancy.leiva; PWD=JL20202000577; TrustServerCertificate=True;";

        public static void ExecuteStoredProcedureNonQuery(string nombreSP,
            Dictionary<string, (object valor, ParameterDirection? direccion)> parametros,
            ref Dictionary<string, object> valoresSalida)
        {
            using var conn = new SqlConnection(connectionString);
            using var cmd = new SqlCommand(nombreSP, conn);
            cmd.CommandType = CommandType.StoredProcedure;

            foreach (var param in parametros)
            {
                var sqlParam = new SqlParameter(param.Key, param.Value.valor ?? DBNull.Value);
                if (param.Value.direccion.HasValue)
                {
                    sqlParam.Direction = param.Value.direccion.Value;

                    if (sqlParam.SqlDbType is SqlDbType.NVarChar or SqlDbType.VarChar)
                        sqlParam.Size = 1000;
                }

                cmd.Parameters.Add(sqlParam);
            }

            conn.Open();
            cmd.ExecuteNonQuery();

            foreach (SqlParameter p in cmd.Parameters)
            {
                if (p.Direction is ParameterDirection.Output or ParameterDirection.InputOutput)
                {
                    valoresSalida[p.ParameterName] = p.Value;
                }
            }
        }

        public static DataTable ExecuteStoredProcedureToDataTable(string procedureName,
            Dictionary<string, (object valor, ParameterDirection? direccion)>? parameters)
        {
            var dataTable = new DataTable();

            using var conn = new SqlConnection(connectionString);
            using var cmd = new SqlCommand(procedureName, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 120;

            Console.WriteLine($"Executing stored procedure: {procedureName}");

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    var sqlParam = new SqlParameter(param.Key, param.Value.valor ?? DBNull.Value);

                    if (param.Value.direccion.HasValue)
                    {
                        sqlParam.Direction = param.Value.direccion.Value;

                        if (sqlParam.SqlDbType is SqlDbType.NVarChar or SqlDbType.VarChar)
                            sqlParam.Size = 1000;
                    }

                    cmd.Parameters.Add(sqlParam);
                    Console.WriteLine($"Parameter: {param.Key} = {param.Value.valor ?? "NULL"}");
                }
            }

            try
            {
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    conn.Open();
                    adapter.Fill(dataTable);
                    Console.WriteLine(
                        $"Results returned: {dataTable.Rows.Count} rows, {dataTable.Columns.Count} columns");
                }

                var outputValues = new Dictionary<string, object>();
                foreach (SqlParameter p in cmd.Parameters)
                {
                    if (p.Direction is not (ParameterDirection.Output or ParameterDirection.InputOutput)) continue;
                    outputValues[p.ParameterName] = p.Value;
                    Console.WriteLine($"Output parameter: {p.ParameterName} = {p.Value}");
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL error: {ex.Number}, Message: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }

            return dataTable;
        }

        public static DataTable ExecuteSqlQueryToDataTable(string sqlQuery,
            Dictionary<string, object>? parameters = null)
        {
            var dataTable = new DataTable();

            using var conn = new SqlConnection(connectionString);
            using var cmd = new SqlCommand(sqlQuery, conn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 120;

            Console.WriteLine($"Executing SQL query: {sqlQuery}");

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    var sqlParam = new SqlParameter(param.Key, param.Value ?? DBNull.Value);
                    cmd.Parameters.Add(sqlParam);
                    Console.WriteLine($"Parameter: {param.Key} = {param.Value ?? "NULL"}");
                }
            }

            try
            {
                using var adapter = new SqlDataAdapter(cmd);
                conn.Open();
                adapter.Fill(dataTable);
                Console.WriteLine($"Results returned: {dataTable.Rows.Count} rows, {dataTable.Columns.Count} columns");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL error: {ex.Number}, Message: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }

            return dataTable;
        }
    }
}