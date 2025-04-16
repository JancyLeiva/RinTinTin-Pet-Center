using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoBD2.Services {

    /*Hice esta clase para almacenar los datos del usuario autenticado 
     * (para así poder usar los roles que retorna el SP para habilitar 
     * o restingir funcionalidades del sistema, es decir, controlar los accesos). */

    public static class SessionService { 
        public static string Usuario {  get; set; } = ""; 
        public static string Roles { get; set; } = ""; 
        public static bool EstaAutenticado => !string.IsNullOrWhiteSpace(Usuario); 
    } 
}
