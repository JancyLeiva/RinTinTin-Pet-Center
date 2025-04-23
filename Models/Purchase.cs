using System;

namespace ProyectoBD2.Models;

public class Purchase
{
    public int? CompraId { get; set; }
    public string? CodigoCompra { get; set; }
    public string? Proveedor { get; set; }
    public DateTime? Fecha { get; set; }
    public decimal? Total { get; set; }
    public string? Estado { get; set; }
}