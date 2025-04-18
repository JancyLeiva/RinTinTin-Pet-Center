using System.Data;
using System;
using System.Collections.Generic;
using ProyectoBD2.DataAccess;
using ProyectoBD2.Models;

namespace ProyectoBD2.Services;

using ProyectoBD2.DataAccess;

public static class AppointmentsService
{
    public static DataTable FindAll(string? fecha)
    {
        try
        {
            var parameters = new Dictionary<string, (object valor, ParameterDirection? direccion)>
            {
                { "@Fecha", (fecha ?? DateTime.Now.ToString("yyyy-MM-dd"), null) }
            };

            return DBAccess.ExecuteStoredProcedureToDataTable("dbPrj.spConsultarCitasPorFecha", parameters);
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

    public static DataTable FindClients(string? busqueda)
    {
        try
        {
            var parameters = new Dictionary<string, (object valor, ParameterDirection? direccion)>
            {
                { "@Busqueda", (busqueda ?? null, null)! },
            };
            return DBAccess.ExecuteStoredProcedureToDataTable("dbPrj.spAutocompletarCliente", parameters);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public static DataTable FindPets(string? identidadCliente)
    {
        try
        {
            var parameters = new Dictionary<string, (object valor, ParameterDirection? direccion)>
            {
                { "@IdentidadCliente", (identidadCliente ?? null, null)! },
            };
            return DBAccess.ExecuteStoredProcedureToDataTable("dbPrj.spListaMascotasPorCliente", parameters);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public static DataTable FindServices()
    {
        try
        {
            return DBAccess.ExecuteStoredProcedureToDataTable("dbPrj.spObtenerServiciosConTipo", null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public static DataTable CreateAppointment(string identidadCliente, int mascotaId, string estado, int servicioId,
        DateTime fechaInicio, int esEmergencia = 0)
    {
        try
        {
            var parameters = new Dictionary<string, (object valor, ParameterDirection? direccion)>
            {
                { "@IdentidadCliente", (identidadCliente, null) },
                { "@MascotaID", (mascotaId, null) },
                { "@Estado", (estado, null) },
                { "@ServicioID", (servicioId, null) },
                { "@FechaInicio", (fechaInicio, null) },
                { "@FechaFin", (fechaInicio.AddHours(1), null) },
                { "@Emergencia", (esEmergencia, null) }
            };

            return DBAccess.ExecuteStoredProcedureToDataTable("dbPrj.spCitaInsert", parameters);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}