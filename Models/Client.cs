namespace ProyectoBD2.Models;

public class Client
{
    public int? ClienteId { get; set; }
    public string? Nombre { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public string? Direccion { get; set; }
    public string? TelefonoAdicional { get; set; }
    public string? NumIdentidad { get; set; }
    public bool? Activo { get; set; }
}