using System;
using System.Collections.Generic;
using System.Data;
using ProyectoBD2.DataAccess;

namespace ProyectoBD2.Services
{
    public static class DataServices
    {
        private static readonly Dictionary<string, (object valor, ParameterDirection? direccion)> EmptyParams = new();

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


        public static DataTable FindClientsOnAppointments(string? busqueda)
        {
            var parameters = new Dictionary<string, (object valor, ParameterDirection? direccion)>
            {
                { "@Busqueda", (busqueda != null ? busqueda : DBNull.Value, null) }
            };

            return DbAccess.ExecuteStoredProcedure("dbPrj.spAutocompletarCliente", parameters);
        }
        
        public static DataTable FindAllClients()
        {
            return DbAccess.ExecuteStoredProcedure("dbPrj.spListaClientes", EmptyParams);
        }

        public static DataTable FindPetsOnAppointments(string? identidadCliente)
        {
            var parameters = new Dictionary<string, (object valor, ParameterDirection? direccion)>
            {
                { "@IdentidadCliente", (identidadCliente != null ? identidadCliente : DBNull.Value, null) }
            };

            return DbAccess.ExecuteStoredProcedure("dbPrj.spListaMascotasPorCliente", parameters);
        }

        public static DataTable FindAreas()
        {
            return DbAccess.ExecuteSqlRawQuery("SELECT * FROM dbPrj.vArea");
        }

        public static DataTable FindServices()
        {
            return DbAccess.ExecuteStoredProcedure("dbPrj.spObtenerServiciosConTipo", EmptyParams);
        }

        public static DataTable FindPetByClientId(int? clienteId)
        {
            var parameters = new Dictionary<string, (object valor, ParameterDirection? direccion)>
            {
                { "@ClienteID", (clienteId != null ? clienteId : DBNull.Value, null) }
            };
            
            return DbAccess.ExecuteStoredProcedure("dbPrj.spListaDeMascotasDeUnCliente", parameters);
        }

        public static DataTable CreateClient(string? nombre, string? identidad, string? telefono, string? correo, string? direccion, string? telefonoAdicional)
        {
            var parameters = new Dictionary<string, (object valor, ParameterDirection? direccion)>
            {
                { "@Nombre", (nombre != null ? nombre : DBNull.Value, null) },
                { "Identidad", (identidad != null ? identidad : DBNull.Value, null) },
                { "@Telefono", (telefono != null ? telefono : DBNull.Value, null) },
                { "@Correo", (correo != null ? correo : DBNull.Value, null) },
                { "@Direccion", (direccion != null ? direccion : DBNull.Value, null) },
                { "@TelefonoAdicional", (telefonoAdicional != null ? telefonoAdicional : DBNull.Value, null) }
            };
            return DbAccess.ExecuteStoredProcedure("dbPrj.spClienteInsert", parameters);
        }
        
        public static DataTable UpdateClient(int? clienteId, string? nombre, string? identidad, string? telefono, string? correo, string? direccion, string? telefonoAdicional)
        {
            var parameters = new Dictionary<string, (object valor, ParameterDirection? direccion)>
            {
                { "@ClienteID", (clienteId != null ? clienteId : DBNull.Value, null) },
                { "@Nombre", (nombre != null ? nombre : DBNull.Value, null) },
                { "Identidad", (identidad != null ? identidad : DBNull.Value, null) },
                { "@Telefono", (telefono != null ? telefono : DBNull.Value, null) },
                { "@Correo", (correo != null ? correo : DBNull.Value, null) },
                { "@Direccion", (direccion != null ? direccion : DBNull.Value, null) },
                { "@TelefonoAdicional", (telefonoAdicional != null ? telefonoAdicional : DBNull.Value, null) }
            };
            return DbAccess.ExecuteStoredProcedure("dbPrj.spClienteUpdate", parameters);
        }
        
        public static DataTable DeleteClient(int? clienteId)
        {
            var parameters = new Dictionary<string, (object valor, ParameterDirection? direccion)>
            {
                { "@ClienteID", (clienteId != null ? clienteId : DBNull.Value, null) }
            };
            return DbAccess.ExecuteStoredProcedure("dbPrj.spClienteDesactivar", parameters);
        }
        
        public static DataTable FindPetsByClientId(int? clienteId)
        {
            var parameters = new Dictionary<string, (object valor, ParameterDirection? direccion)>
            {
                { "@ClienteID", (clienteId != null ? clienteId : DBNull.Value, null) }
            };
            return DbAccess.ExecuteStoredProcedure("dbPrj.spListaDeMascotasDeUnCliente", parameters);
        }

        public static DataTable CreatePet(string? nombre, string? especie, string? raza, decimal? peso, int? edad, string? color, string? descripcion, int? clienteId)
        {
            var parameters = new Dictionary<string, (object valor, ParameterDirection? direccion)>
            {
                { "@Nombre", (nombre != null ? nombre : DBNull.Value, null) },
                { "@Especie", (especie != null ? especie : DBNull.Value, null) },
                { "@Raza", (raza != null ? raza : DBNull.Value, null) },
                { "@ClienteID", (clienteId != null ? clienteId : DBNull.Value, null) },
                { "@Peso", (peso != null ? peso : DBNull.Value, null) },
                { "@Edad", (edad != null ? edad : DBNull.Value, null) },
                { "@Color", (color != null ? color : DBNull.Value, null) },
                { "@Descripcion", (descripcion != null ? descripcion : DBNull.Value, null) }
            };
            return DbAccess.ExecuteStoredProcedure("dbPrj.spMascotaInsert", parameters);
        }
        
        public static DataTable UpdatePet(int? mascotaId, string? nombre, string? especie, string? raza, decimal? peso, int? edad, string? color, string? descripcion)
        {
            var parameters = new Dictionary<string, (object valor, ParameterDirection? direccion)>
            {
                { "@MascotaID", (mascotaId != null ? mascotaId : DBNull.Value, null) },
                { "@Nombre", (nombre != null ? nombre : DBNull.Value, null) },
                { "@Especie", (especie != null ? especie : DBNull.Value, null) },
                { "@Raza", (raza != null ? raza : DBNull.Value, null) },
                { "@Peso", (peso != null ? peso : DBNull.Value, null) },
                { "@Edad", (edad != null ? edad : DBNull.Value, null) },
                { "@Color", (color != null ? color : DBNull.Value, null) },
                { "@Descripcion", (descripcion != null ? descripcion : DBNull.Value, null) }
            };
            return DbAccess.ExecuteStoredProcedure("dbPrj.spMascotaUpdate", parameters);
        }
        
        public static DataTable DeletePet(int? mascotaId)
        {
            var parameters = new Dictionary<string, (object valor, ParameterDirection? direccion)>
            {
                { "@MascotaID", (mascotaId != null ? mascotaId : DBNull.Value, null) }
            };
            return DbAccess.ExecuteStoredProcedure("dbPrj.spMascotaDesactivar", parameters);
        }
    }
}