using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoBD2.Services {
    public static class SessionService 
    {
        public static string? Usuario { get; set; }
    
        private static string? RolesString { get; set; }
        public static string? Roles 
        { 
            get => RolesString;
            set 
            {
                RolesString = value;
                RolesList = value?.Split(',').Select(r => r.Trim()).ToList() ?? [];
            } 
        }

        private static List<string> RolesList { get; set; } = [];
    
        public static bool EstaAutenticado => !string.IsNullOrWhiteSpace(Usuario);

        private static bool TieneRol(string rol) => RolesList.Contains(rol);

        public static bool EsAdministrador => TieneRol("Administrador");
        public static bool EsVeterinario => TieneRol("Veterinario");
        public static bool EsRecepcionista => TieneRol("Recepcionista");
    
        public static void LogOut()
        {
            Usuario = "";
            Roles = "";
            RolesList.Clear();
        }
    }
}
