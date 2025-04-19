using System;
using System.Collections.Generic;
using System.Data;
using ProyectoBD2.DataAccess;

namespace ProyectoBD2.Services
{
    public static class DataServices
    {
        private static readonly Dictionary<string, (object valor, ParameterDirection? direccion)> EmptyParams = new();

        #region Citas (Appointments)

        public static DataTable FindAppointmentsByDate(string? fecha)
        {
            var parameters = new Dictionary<string, (object valor, ParameterDirection? direccion)>
            {
                { "@Fecha", (fecha ?? DateTime.Now.ToString("yyyy-MM-dd"), null) }
            };

            return DbAccess.ExecuteStoredProcedure("dbPrj.spConsultarCitasPorFecha", parameters);
        }

        public static DataTable CreateAppointment(string identidadCliente, int mascotaId, string estado, int servicioId,
            DateTime fechaInicio, int esEmergencia = 0)
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

        public static DataTable UpdateAppointment(int citaId, int mascotaId, string estado, int servicioId,
            DateTime fechaInicio, int esEmergencia = 0)
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

        public static DataTable DeleteAppointment(int citaId)
        {
            var parameters = new Dictionary<string, (object valor, ParameterDirection? direccion)>
            {
                { "@CitaID", (citaId, null) }
            };

            return DbAccess.ExecuteStoredProcedure("dbPrj.spAnularCita", parameters);
        }

        #endregion

        #region Clientes y Mascotas (Clients and Pets)

        public static DataTable FindClients(string? busqueda)
        {
            var parameters = new Dictionary<string, (object valor, ParameterDirection? direccion)>
            {
                { "@Busqueda", (busqueda != null ? busqueda : DBNull.Value, null) }
            };

            return DbAccess.ExecuteStoredProcedure("dbPrj.spAutocompletarCliente", parameters);
        }

        public static DataTable FindPets(string? identidadCliente)
        {
            var parameters = new Dictionary<string, (object valor, ParameterDirection? direccion)>
            {
                { "@IdentidadCliente", (identidadCliente != null ? identidadCliente : DBNull.Value, null) }
            };

            return DbAccess.ExecuteStoredProcedure("dbPrj.spListaMascotasPorCliente", parameters);
        }

        #endregion

        #region Áreas y Servicios (Areas and Services)

        public static DataTable FindAreas()
        {
            return DbAccess.ExecuteSqlRawQuery("SELECT * FROM dbPrj.vArea");
        }

        public static DataTable FindServices()
        {
            return DbAccess.ExecuteStoredProcedure("dbPrj.spObtenerServiciosConTipo", EmptyParams);
        }

        #endregion
    }
}