namespace ProyectoBD2.Models;

public class Stock
{
    public string? NombreProveedor { get; set; }
    public string? Clasificacion { get; set; }
    public decimal? ExistenciaActual { get; set; }
    public decimal? ExistenciaMinima { get; set; }
    public string? Estado { get; set; }
}