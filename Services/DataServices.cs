using System.Data;
using System;
using System.Collections.Generic;
using ProyectoBD2.DataAccess;
using ProyectoBD2.Models;

namespace ProyectoBD2.Services;

using ProyectoBD2.DataAccess;

public static class DataServices
{
    public static DataTable FindAppointmentsByDate(string? fecha)
    {
        try
        {
            var parameters = new Dictionary<string, (object valor, ParameterDirection? direccion)>
            {
                { "@Fecha", (fecha ?? DateTime.Now.ToString("yyyy-MM-dd"), null) }
            };

            return DbAccess.ExecuteStoredProcedure("dbPrj.spConsultarCitasPorFecha", parameters);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    public static DataTable FindAreas()
    {
        try
        {
            return DbAccess.ExecuteSqlRawQuery("SELECT * FROM dbPrj.vArea");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
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
            return DbAccess.ExecuteStoredProcedure("dbPrj.spAutocompletarCliente", parameters);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
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
            return DbAccess.ExecuteStoredProcedure("dbPrj.spListaMascotasPorCliente", parameters);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    public static DataTable FindServices()
    {
        try
        {
            return DbAccess.ExecuteStoredProcedure("dbPrj.spObtenerServiciosConTipo", null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
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

            return DbAccess.ExecuteStoredProcedure("dbPrj.spCitaInsert", parameters);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    public static DataTable UpdateAppointment(int citaId, int mascotaId, string estado, int servicioId,
        DateTime fechaInicio, int esEmergencia = 0)
    {
        try
        {
            var parameters = new Dictionary<string, (object valor, ParameterDirection? direccion)>
            {
                { "@CitaID", (citaId, null) },
                { "@MascotaID", (mascotaId, null) },
                { "@Estado", (estado, null) },
                { "@ServicioID", (servicioId, null) },
                { "@FechaInicio", (fechaInicio, null) },
                { "@FechaFin", (fechaInicio.AddHours(1), null) },
                { "@EsEmergencia", (esEmergencia, null) }
            };

            return DbAccess.ExecuteStoredProcedure("dbPrj.spCitaUpdate", parameters);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    public static DataTable DeleteAppointment(int citaId)
    {
        try
        {
            var parameters = new Dictionary<string, (object valor, ParameterDirection? direccion)>
            {
                { "@CitaID", (citaId, null) }
            };

            return DbAccess.ExecuteStoredProcedure("dbPrj.spAnularCita", parameters);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }
}