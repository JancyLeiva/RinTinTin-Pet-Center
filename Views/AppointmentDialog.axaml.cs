using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ProyectoBD2.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using Avalonia.Data;
using ProyectoBD2.Services;

namespace ProyectoBD2.Windows
{
    public partial class AppointmentDialog : Window
    {
        private ObservableCollection<Client>? _clients;
        private ObservableCollection<Appointment>? _appointments;
        private ObservableCollection<Pet>? _pets;
        private ObservableCollection<string>? _horasDisponibles;
        private readonly ObservableCollection<string> _estados = ["Pendiente", "En curso", "Finalizada", "Cancelada"];
        private readonly ObservableCollection<string> _horasHabiles = ["08:00", "09:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00", "16:00", "17:00"];
        public AppointmentDialog()
        {
            InitializeComponent();
            LoadClients();
            ClienteAutoCompleteBox.ValueMemberBinding = new Binding("Nombre");
            MascotaAutoCompleteBox.ValueMemberBinding = new Binding("Mascota");
            ClienteAutoCompleteBox.FilterMode = AutoCompleteFilterMode.Contains;
            MascotaAutoCompleteBox.FilterMode = AutoCompleteFilterMode.Contains;
            EstadoComboBox.ItemsSource = _estados;
            EstadoComboBox.SelectedIndex = 0;
            EstadoStackPanel.IsVisible = false;
            HoraComboBox.IsEnabled = false;
            MascotaAutoCompleteBox.IsEnabled = false;
            FechaDatePicker.SelectedDateChanged += (s, e) => LoadHorarios();
            ClienteAutoCompleteBox.SelectionChanged += (s, e) => LoadMascotas();
        }

        private void LoadClients()
        {
            _clients = [];
            var busqueda = "";
            var data = AppointmentsService.FindClients(busqueda);
            Console.Write(data);
            
            foreach (DataRow row in data.Rows)
            {
                _clients.Add(new Client
                {
                    Nombre = (string)row["Nombre"],
                    NumIdentidad = (string)row["NumIdentidad"],
                });
            }
            
            ClienteAutoCompleteBox.ItemsSource = _clients;
        }

        private void LoadHorarios()
        {
            _horasDisponibles = [];
            _appointments = [];
            var fecha = FechaDatePicker.SelectedDate!.Value.ToString("yyyy-MM-dd");
            int? serviceTypeId = null;
            const string queryMode = "Citas";

            var data = AppointmentsService.FindAll(serviceTypeId, queryMode, fecha);

            foreach (DataRow row in data.Rows)
            {
                _appointments.Add(new Appointment
                {
                    CitaID = (int)row["CitaID"],
                    Cliente = (string)row["Cliente"],
                    Mascota = (string)row["Mascota"],
                    TipoServicio = (string)row["TipoServicio"],
                    Servicio = (string)row["Servicio"],
                    FechaInicio = (DateTime)row["FechaInicio"],
                    FechaFin = (DateTime)row["FechaFin"],
                    Estado = (string)row["Estado"],
                    EsEmergencia = (bool)row["EsEmergencia"]
                });
            }
            
            foreach (var hora in _horasHabiles)
            {
                var horaOcupada = _appointments.Any(cita => hora == cita.FechaInicio.ToString("HH:mm"));
                if (!horaOcupada)
                {
                    _horasDisponibles.Add(hora);
                }
            }
            
            HoraComboBox.ItemsSource = _horasDisponibles;
            HoraComboBox.IsEnabled = true;
        }

        private void LoadMascotas()
        {
            _pets = [];
            var identidadCliente = ClienteAutoCompleteBox.SelectedItem as Client;
            if (identidadCliente == null)
            {
                MascotaAutoCompleteBox.IsEnabled = false;
                MascotaAutoCompleteBox.Text = "";
                return;
            }

            var data = AppointmentsService.FindPets(identidadCliente!.NumIdentidad);

            foreach (DataRow row in data.Rows)
            {
                _pets.Add(new Pet
                {
                    ClienteID = (int)row["ClienteID"],
                    Dueño = (string)row["Dueño"],
                    Mascota = (string)row["Mascota"],
                    MascotaID = (int)row["MascotaID"]
                });
            }
            
            MascotaAutoCompleteBox.ItemsSource = _pets;
            MascotaAutoCompleteBox.IsEnabled = true;
            MascotaAutoCompleteBox.IsTextCompletionEnabled = true;
        }

        private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close(false);
        }
    }
}