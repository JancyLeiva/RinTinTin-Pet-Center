using System;
using System.Collections.Generic;
using System.Data;
using ProyectoBD2.DataAccess;

namespace ProyectoBD2.Services
{
    public static class AuthService
    {
        public static (bool EsValido, string? Roles) ValidarCredenciales(string? usuario, string contrasena)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contrasena))
                {
                    return (false, null);
                }

                var parametros = new Dictionary<string, (object valor, ParameterDirection? direccion)> 
                { 
                    { "@Usuario", (usuario, null) }, 
                    { "@Contrasena", (contrasena, null) }, 
                    { "@EsValido", (0, ParameterDirection.Output) }, 
                    { "@Roles", ("", ParameterDirection.Output) } 
                };

                var salida = new Dictionary<string, object>();

                DbAccess.ExecuteStoredProcedureNonQuery("dbPrj.spUsuarioValidacion", parametros, ref salida);

                var esValido = Convert.ToBoolean(salida["@EsValido"]);
                var roles = salida["@Roles"]?.ToString() ?? "";

                return (esValido, roles);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ValidarCredenciales: {ex.Message}");
                throw new Exception("Error al validar credenciales. Contacte al administrador.", ex);
            }
        }

        public static bool AutenticarUsuario(string? usuario, string contrasena)
        {
            try
            {
                var (esValido, roles) = ValidarCredenciales(usuario, contrasena);
                
                if (!esValido)
                    return false;
                
                SessionService.Usuario = usuario;
                SessionService.Roles = roles;
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void CerrarSesion()
        {
            SessionService.LogOut();
        }
    }
}