namespace ProyectoBD2.Models;

public class Empleado
{
    public int? EmpleadoID { get; set; }
    public string? CodigoEmpleado { get; set; }
    public string? Nombre { get; set; }
    public string? Identificacion { get; set; }
    public string? Puesto { get; set; }
    public int? DepartamentoID { get; set; }
    public string? Telefono { get; set; }
}