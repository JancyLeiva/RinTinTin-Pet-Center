using System;

namespace ProyectoBD2.Models
{
    public class Appointment
    {
        public int CitaID { get; set; }
        public string? TipoServicio { get; set; }
        public string? Servicio { get; set; }
        public string? Mascota { get; set; }
        public string? Cliente { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string? Estado { get; set; }
        public bool EsEmergencia { get; set; }
    }
}