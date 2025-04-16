using System;

namespace ProyectoBD2.Models
{
    public class Appointment
    {
        public int CitaID { get; set; }
        public int ClienteID { get; set; }
        public string ClienteNombre { get; set; } // Display name for UI
        public int MascotaID { get; set; }
        public string MascotaNombre { get; set; } // Display name for UI
        public string Estado { get; set; }
        public int ServicioID { get; set; }
        public string ServicioNombre { get; set; } // Display name for UI
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public bool EsEmergencia { get; set; }
    }
}