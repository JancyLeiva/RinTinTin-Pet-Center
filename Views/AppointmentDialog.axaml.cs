using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ProyectoBD2.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using ProyectoBD2.Services;

namespace ProyectoBD2.Windows
{
    public partial class AppointmentDialog : Window
    {
        private ObservableCollection<Client>? _clients;
        private ObservableCollection<Appointment>? _appointments;
        private ObservableCollection<ServiceType>? _services;
        private ObservableCollection<Pet>? _pets;
        private ObservableCollection<string>? _horasDisponibles;
        private readonly ObservableCollection<string> _estados = ["Pendiente", "En curso", "Finalizada", "Cancelada"];

        private readonly ObservableCollection<string> _horasHabiles =
            ["08:00", "09:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00", "16:00", "17:00"];

        public AppointmentDialog()
        {
            InitializeComponent();
            LoadClients();
            LoadServices();
            ClienteAutoCompleteBox.ValueMemberBinding = new Binding("Nombre");
            ClienteAutoCompleteBox.FilterMode = AutoCompleteFilterMode.Contains;
            EstadoComboBox.ItemsSource = _estados;
            EstadoComboBox.SelectedIndex = 0;
            EstadoStackPanel.IsVisible = false;
            HoraComboBox.IsEnabled = false;
            MascotaComboBox.IsEnabled = false;
            FechaDatePicker.SelectedDateChanged += (s, e) => LoadHorarios();
            ClienteAutoCompleteBox.SelectionChanged += (s, e) => LoadMascotas();
            ServicioComboBox.SelectionChanged += (s, e) => LoadHorarios();
            MascotaComboBox.SelectionChanged += (s, e) => LoadHorarios();
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

            if (FechaDatePicker.SelectedDate == null)
            {
                HoraComboBox.ItemsSource = _horasDisponibles;
                HoraComboBox.IsEnabled = false;
                return;
            }

            var date = FechaDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd");

            if (ServicioComboBox.SelectedItem is not ServiceType selectedService)
            {
                HoraComboBox.ItemsSource = _horasDisponibles;
                HoraComboBox.IsEnabled = false;
                return;
            }

            var data = AppointmentsService.FindAll(date);

            foreach (DataRow row in data.Rows)
            {
                _appointments.Add(new Appointment
                {
                    CitaId = (int)row["CitaID"],
                    Cliente = (string)row["Cliente"],
                    Mascota = (string)row["Mascota"],
                    TipoServicio = (string)row["TipoServicio"],
                    Servicio = (string)row["Servicio"],
                    FechaInicio = (DateTime)row["FechaInicio"],
                    Estado = (string)row["Estado"],
                    EsEmergencia = Convert.ToInt32(row["EsEmergencia"]) == 1
                });
            }

            var selectedTipoServicio = selectedService.Servicio?.Split(" -")[0];
            var selectedPet = (MascotaComboBox.SelectionBoxItem as Pet)?.Mascota;
            var selectedClient = (ClienteAutoCompleteBox.SelectedItem as Client)?.Nombre;

            foreach (var hora in _horasHabiles)
            {
                var appointmentsAtHour = _appointments
                    .Where(a =>
                        a.FechaInicio.ToString("HH:mm") == hora)
                    .ToList();

                var appointmentsAtHourAndServiceType =
                    appointmentsAtHour.Where(a => a.TipoServicio == selectedTipoServicio).ToList();

                var appointmentsAtHourAndPet = appointmentsAtHour
                    .Any(a => a.Mascota == selectedPet && a.Cliente == selectedClient);

                if (appointmentsAtHourAndServiceType.Count < 3 && !appointmentsAtHourAndPet)
                    _horasDisponibles.Add(hora);
            }

            HoraComboBox.ItemsSource = _horasDisponibles;
            HoraComboBox.IsEnabled = _horasDisponibles.Count > 0;
        }

        private void LoadMascotas()
        {
            _pets = [];
            if (ClienteAutoCompleteBox.SelectedItem is not Client identidadCliente)
            {
                MascotaComboBox.IsEnabled = false;
                MascotaComboBox.ItemsSource = null;
                return;
            }

            var data = AppointmentsService.FindPets(identidadCliente.NumIdentidad);

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

            MascotaComboBox.ItemsSource = _pets;
            MascotaComboBox.IsEnabled = true;
        }

        private void LoadServices()
        {
            _services = [];
            var data = AppointmentsService.FindServices();

            foreach (DataRow row in data.Rows)
            {
                _services.Add(new ServiceType
                {
                    ServicioId = (int)row["ServicioID"],
                    Servicio = (string)row["TipoServicio_Servicio"]
                });
            }

            ServicioComboBox.ItemsSource = _services;
        }

        private void isEmergencyCheckBox_Checked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            FechaDatePicker.IsEnabled = EsEmergenciaCheckBox.IsChecked != true;
        }

        private void SaveButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (ClienteAutoCompleteBox.SelectedItem == null || MascotaComboBox.SelectedItem == null ||
                ServicioComboBox.SelectedItem == null || FechaDatePicker.SelectedDate == null ||
                HoraComboBox.SelectedItem == null)
            {
                return;
            }

            var identidadCliente = ((Client)ClienteAutoCompleteBox.SelectedItem).NumIdentidad ?? "";
            var mascotaId = ((Pet)MascotaComboBox.SelectedItem).MascotaID ?? 0;
            var estado = EstadoComboBox.SelectedItem?.ToString();
            var servicioId = ((ServiceType)ServicioComboBox.SelectedItem).ServicioId ?? 0;
            var esEmergencia = EsEmergenciaCheckBox.IsChecked == true ? 1 : 0;
            var fechaInicio = EsEmergenciaCheckBox.IsChecked == true
                ? DateTime.Now
                : FechaDatePicker.SelectedDate!.Value.Date.Add(TimeSpan.Parse(HoraComboBox.SelectedItem.ToString()!));
            Console.WriteLine(identidadCliente);
            Console.WriteLine(mascotaId);
            Console.WriteLine(estado);
            Console.WriteLine(servicioId);
            Console.WriteLine(fechaInicio);
            Console.WriteLine(esEmergencia);
            AppointmentsService.CreateAppointment(identidadCliente, mascotaId, estado!, servicioId, fechaInicio,
                esEmergencia);
        }

        private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close(false);
        }
    }
}