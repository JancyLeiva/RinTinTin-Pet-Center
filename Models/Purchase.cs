using System;

namespace ProyectoBD2.Models;

public class Purchase
{
    public int? ProveedorId { get; set; }
    public DateTime? Fecha { get; set; }
    public string? Estado { get; set; }
}