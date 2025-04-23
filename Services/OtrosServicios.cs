using System;
using System.Collections.Generic;
using System.Data;
using ProyectoBD2.DataAccess;

namespace ProyectoBD2.Services;

public static class OtrosServicios
{
    private static readonly Dictionary<string, (object valor, ParameterDirection? direccion)> EmptyParams = 
        new Dictionary<string, (object valor, ParameterDirection? direccion)>();

    public static DataTable AllEmpleados()
    {
        return DbAccess.ExecuteStoredProcedure("dbPrj.sp_VerTodosEmpleados", EmptyParams);
    }
    
    public static DataTable CrearEmpleado(string? nombre, string? identificacion, string? puesto, int? departamentoID, string? telefono)
    {
        var parameters = new Dictionary<string, (object valor, ParameterDirection? direccion)>
        {
            { "Nombre", (nombre != null ? nombre : DBNull.Value, null) },
            { "Identificacion", (identificacion != null ? identificacion : DBNull.Value, null) },
            { "Puesto", (puesto != null ? puesto : DBNull.Value, null) },
            { "DepartamentoID", (departamentoID != null ? departamentoID : DBNull.Value, null) },
            { "Telefono", (telefono != null ? telefono : DBNull.Value, null) }
        };
        
        return DbAccess.ExecuteStoredProcedure("dbPrj.sp_CrearEmpleado", parameters);
    }
    
    public static DataTable EliminarEmpleado(int? EmpleadoID)
    {
        var parameters = new Dictionary<string, (object valor, ParameterDirection? direccion)>
        {
            { "EmpleadoID", (EmpleadoID != null ? EmpleadoID : DBNull.Value, null) }
        };
        
        return DbAccess.ExecuteStoredProcedure("dbPrj.sp_EliminarEmpleado", parameters);
    }
    
    public static DataTable EditarEmpleado(int? empleadoID, string? nombre, string? identificacion, string? puesto, int? departamentoID, string? telefono)
    {
        var parameters = new Dictionary<string, (object valor, ParameterDirection? direccion)>
        {
            { "EmpleadoID", (empleadoID != null ? empleadoID : DBNull.Value, null) },
            { "Nombre", (nombre != null ? nombre : DBNull.Value, null) },
            { "Identificacion", (identificacion != null ? identificacion : DBNull.Value, null) },
            { "Puesto", (puesto != null ? puesto : DBNull.Value, null) },
            { "DepartamentoID", (departamentoID != null ? departamentoID : DBNull.Value, null) },
            { "Telefono", (telefono != null ? telefono : DBNull.Value, null) }
        };
    
        return DbAccess.ExecuteStoredProcedure("dbPrj.sp_EditarEmpleado", parameters);
    }
}