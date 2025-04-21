using System;

namespace ProyectoBD2.Models;

public class Reservation
{
    public int? EstadiaId { get; set; }
    public int? HabitacionId { get; set; }
    public string? DescripcionHabitacion { get; set; }
    public string? NombreMascota { get; set; }
    public string? NombreCliente { get; set; }
    public string? Telefono { get; set; }
    public DateTime? FechaIngreso { get; set; }
    public DateTime? FechaSalida { get; set; }
    public string? EstadoActual { get; set; }
    public string? Observaciones { get; set; }
    public int? ServicioAlimentacionEspecial { get; set; }
    public int? ServicioPaseoDiario { get; set; }
    public int? ServicioBanoCepillado { get; set; }
    public int? ServicioMedicamento { get; set; }
    public DateTime? FechaReserva { get; set; }
}