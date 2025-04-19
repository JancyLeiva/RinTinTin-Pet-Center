using System;
using System.Collections.Generic;
using System.Data;
using ProyectoBD2.DataAccess;

namespace ProyectoBD2.Services
{
    public static class AuthService
    {
        private static readonly Dictionary<string, (object valor, ParameterDirection? direccion)> DefaultParameters = new()
        {
            { "@Usuario", (string.Empty, null) },
            { "@Contrasena", (string.Empty, null) },
            { "@EsValido", (0, ParameterDirection.Output) },
            { "@Roles", ("", ParameterDirection.Output) }
        };

        public static (bool EsValido, string? Roles) ValidarCredenciales(string? usuario, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contrasena))
                return (false, null);

            try
            {
                var parameters = new Dictionary<string, (object valor, ParameterDirection? direccion)>(DefaultParameters)
                {
                    ["@Usuario"] = (usuario, null),
                    ["@Contrasena"] = (contrasena, null)
                };

                var salida = new Dictionary<string, object?>();
                DbAccess.ExecuteStoredProcedureNonQuery("dbPrj.spUsuarioValidacion", parameters, ref salida);

                var esValido = Convert.ToBoolean(salida["@EsValido"]);
                var roles = salida["@Roles"]?.ToString();

                return (esValido, roles);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ValidarCredenciales: {ex.Message}");
                throw;
            }
        }

        public static bool AutenticarUsuario(string? usuario, string contrasena)
        {
            var (esValido, roles) = ValidarCredenciales(usuario, contrasena);

            if (!esValido) return esValido;
            SessionService.Usuario = usuario;
            SessionService.Roles = roles;

            return esValido;
        }

        public static void CerrarSesion()
        {
            SessionService.LogOut();
        }
    }
}