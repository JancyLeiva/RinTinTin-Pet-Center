using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ProyectoBD2.DataAccess;

namespace ProyectoBD2.Services
{

    /*Esta es una clase creada para validar las credenciales del usuario mediante
     * el llamado de un SP, haciendo uso de la clase de acceso a la BD del proyecto*/
    public static class AuthService
    {
        public static (bool EsValido, string Roles) ValidarCredenciales(string usuario, string contrasena)
        {
            var parametros = new Dictionary<string, (object valor, ParameterDirection? direccion)> 
            { 
                { "@Usuario", (usuario, null) }, 
                { "@Contrasena", (contrasena, null) }, 
                { "@EsValido", (0, ParameterDirection.Output) }, 
                { "@Roles", ("", ParameterDirection.Output) } 
            };

            var salida = new Dictionary<string, object>();

            DBAccess.ExecuteStoredProcedureNonQuery("dbPrj.spUsuarioValidacion", parametros, ref salida);

            bool esValido = Convert.ToBoolean(salida["@EsValido"]);
            string roles = salida["@Roles"]?.ToString() ?? "";

            return (esValido, roles);
        }
    }
}