using System.Data;
using System;
using System.Collections.Generic;
using ProyectoBD2.DataAccess;
using ProyectoBD2.Models;

namespace ProyectoBD2.Services;
using ProyectoBD2.DataAccess;


public static class AppointmentsService
{
    public static DataTable FindAll(int? tipoServicioId, string? modoConsulta, string? fecha)
    {
        try
        {
            var parametros = new Dictionary<string, (object valor, ParameterDirection? direccion)>
            {
                { "@TipoServicioID", (tipoServicioId.HasValue ? (object)tipoServicioId.Value : DBNull.Value, null) },
                { "@ModoConsulta", (modoConsulta ?? "Citas", null) },
                { "@Fecha", (fecha ?? DateTime.Now.ToString("yyyy-MM-dd"), null) }
            };

            return DBAccess.ExecuteStoredProcedureToDataTable("dbPrj.spConsultarHorariosOCitasPorArea", parametros);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in FindAll method: {ex.Message}");
            throw;
        }
    }

    public static DataTable FindAreas()
    {
        try
        {
            return DBAccess.ExecuteSqlQueryToDataTable("SELECT * FROM dbPrj.vArea");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
}