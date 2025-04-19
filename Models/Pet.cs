namespace ProyectoBD2.Models;

public class Pet
{
    public int? ClienteId { get; set; }
    public int? MascotaId { get; set; }
    public string? Nombre { get; set; }
    public string? Especie { get; set; }
    public string? Raza { get; set; }
    public decimal? Peso { get; set; }
    public int? Edad { get; set; }
    public string? Color { get; set; }
    public string? Descripcion { get; set; }
    public bool? Activo { get; set; }
    public string? Dueño { get; set; }
}