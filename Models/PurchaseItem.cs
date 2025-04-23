namespace ProyectoBD2.Models;

public class PurchaseItem
{
    public int? ArticuloId { get; set; }
    public string? NombreArticulo { get; set; }
    public int? Cantidad { get; set; }
    public decimal? Precio { get; set; }
    public decimal? Descuento { get; set; }
    public decimal? Impuesto { get; set; }
}