using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace ProyectoBD2.DataAccess
{
    public static class DbAccess
    {
        private const string ConnectionString = "Server=3.128.144.165; Database=DB20202000577; UID=jancy.leiva; PWD=JL20202000577; TrustServerCertificate=True;Pooling=true;Min Pool Size=5;Max Pool Size=100;";
        private const int DefaultCommandTimeout = 30;

        public static void ExecuteStoredProcedureNonQuery(
            string procedureName,
            Dictionary<string, (object valor, ParameterDirection? direccion)> parameters,
            ref Dictionary<string, object?> outputValues)
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = CreateStoredProcCommand(procedureName, parameters, connection);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();
                CollectOutputParameters(command, outputValues);
            }
            catch (Exception ex)
            {
                LogError(ex, procedureName);
                throw;
            }
        }

        public static DataTable ExecuteStoredProcedure(
            string procedureName,
            Dictionary<string, (object valor, ParameterDirection? direccion)>? parameters)
        {
            var dataTable = new DataTable();

            using var connection = new SqlConnection(ConnectionString);
            using var command = CreateStoredProcCommand(procedureName, parameters, connection);

            try
            {
                using var adapter = new SqlDataAdapter(command);
                connection.Open();
                adapter.Fill(dataTable);
                return dataTable;
            }
            catch (Exception ex)
            {
                LogError(ex, procedureName);
                throw;
            }
        }

        public static DataTable ExecuteSqlRawQuery(
            string sqlQuery,
            Dictionary<string, object>? parameters = null)
        {
            var dataTable = new DataTable();

            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(sqlQuery, connection);
            command.CommandType = CommandType.Text;
            command.CommandTimeout = DefaultCommandTimeout;

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }
            }

            try
            {
                using var adapter = new SqlDataAdapter(command);
                connection.Open();
                adapter.Fill(dataTable);
                return dataTable;
            }
            catch (Exception ex)
            {
                LogError(ex, sqlQuery);
                throw;
            }
        }

        private static SqlCommand CreateStoredProcCommand(
            string procedureName,
            Dictionary<string, (object valor, ParameterDirection? direccion)>? parameters,
            SqlConnection connection)
        {
            var command = new SqlCommand(procedureName, connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = DefaultCommandTimeout
            };

            if (parameters == null) return command;

            foreach (var param in parameters)
            {
                var sqlParam = new SqlParameter(param.Key, param.Value.valor);
                
                if (param.Value.direccion.HasValue)
                {
                    sqlParam.Direction = param.Value.direccion.Value;
                    
                    if (sqlParam.SqlDbType is SqlDbType.NVarChar or SqlDbType.VarChar)
                        sqlParam.Size = 1000;
                }
                
                command.Parameters.Add(sqlParam);
            }

            return command;
        }

        private static void CollectOutputParameters(SqlCommand command, Dictionary<string, object?> outputValues)
        {
            foreach (SqlParameter param in command.Parameters)
            {
                if (param.Direction is ParameterDirection.Output or ParameterDirection.InputOutput)
                {
                    outputValues[param.ParameterName] = param.Value;
                }
            }
        }

        private static void LogError(Exception ex, string operation)
        {
            #if DEBUG
            if (ex is SqlException sqlEx)
            {
                Console.WriteLine($"SQL error in {operation}: {sqlEx.Number}, Message: {sqlEx.Message}");
            }
            else
            {
                Console.WriteLine($"Error in {operation}: {ex.Message}");
            }
            #endif
        }
    }
}