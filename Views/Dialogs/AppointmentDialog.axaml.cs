using Avalonia.Controls;
using ProyectoBD2.Models;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using Avalonia.Data;
using ProyectoBD2.Services;

namespace ProyectoBD2.Windows
{
    public partial class AppointmentDialog : Window
    {
        private Appointment? _appointment;
        private bool _isEditMode;

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

        public AppointmentDialog(Appointment appointmentToEdit)
        {
            InitializeComponent();
            LoadClients();
            LoadServices();

            _appointment = appointmentToEdit;
            _isEditMode = true;

            ClienteAutoCompleteBox.ValueMemberBinding = new Binding("Nombre");
            // ClienteAutoCompleteBox.FilterMode = AutoCompleteFilterMode.Contains;
            ClienteAutoCompleteBox.IsEnabled = false;
            EstadoComboBox.ItemsSource = _estados;
            EstadoStackPanel.IsVisible = true;

            FechaDatePicker.SelectedDateChanged += (s, e) => LoadHorarios();
            ClienteAutoCompleteBox.SelectionChanged += (s, e) => LoadMascotas();
            ServicioComboBox.SelectionChanged += (s, e) => LoadHorarios();
            MascotaComboBox.SelectionChanged += (s, e) => LoadHorarios();

            LoadAppointmentData();

            if (!_appointment.EsEmergencia) return;
            FechaDatePicker.IsEnabled = false;
            HoraComboBox.IsEnabled = false;
        }

        private void LoadAppointmentData()
        {
            if (_appointment == null) return;

            Title = "Editar Cita";

            FechaDatePicker.SelectedDate = _appointment.FechaInicio.Date;

            foreach (var client in _clients!)
            {
                if (client.Nombre != _appointment.Cliente) continue;
                ClienteAutoCompleteBox.SelectedItem = client;
                break;
            }

            LoadMascotas();

            foreach (var pet in _pets!)
            {
                if (pet.Mascota != _appointment.Mascota) continue;
                MascotaComboBox.SelectedItem = pet;
                break;
            }

            foreach (var service in _services!)
            {
                if (!service.Servicio!.Contains(_appointment.TipoServicio!) ||
                    !service.Servicio.Contains(_appointment.Servicio!)) continue;
                ServicioComboBox.SelectedItem = service;
                break;
            }

            LoadHorarios();

            EsEmergenciaCheckBox.IsChecked = _appointment.EsEmergencia;

            var statusIndex = Array.IndexOf(_estados.ToArray(), _appointment.Estado);
            if (statusIndex >= 0) EstadoComboBox.SelectedIndex = statusIndex;
        }

        private void LoadClients()
        {
            _clients = [];
            const string busqueda = "";
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

                if (_appointment == null || !_isEditMode || _appointment.FechaInicio.ToString("HH:mm") != hora ||
                    _appointment.Mascota != selectedPet) continue;
                _horasDisponibles.Add(hora);
                HoraComboBox.SelectedItem = hora;
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
            HoraComboBox.IsEnabled = EsEmergenciaCheckBox.IsChecked != true;
        }

        private void SaveButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (ClienteAutoCompleteBox.SelectedItem == null || MascotaComboBox.SelectedItem == null ||
                ServicioComboBox.SelectedItem == null || ((FechaDatePicker.SelectedDate == null ||
                HoraComboBox.SelectedItem == null) && !EsEmergenciaCheckBox.IsChecked == true))
            {
                return;
            }

            var citaId = _appointment?.CitaId ?? 0;
            var identidadCliente = ((Client)ClienteAutoCompleteBox.SelectedItem).NumIdentidad ?? "";
            var mascotaId = ((Pet)MascotaComboBox.SelectedItem).MascotaID ?? 0;
            var estado = EstadoComboBox.SelectedItem?.ToString() ?? "Pendiente";
            var servicioId = ((ServiceType)ServicioComboBox.SelectedItem).ServicioId ?? 0;
            var esEmergencia = EsEmergenciaCheckBox.IsChecked == true ? 1 : 0;
            var fechaInicio = EsEmergenciaCheckBox.IsChecked == true
                ? DateTime.Now
                : FechaDatePicker.SelectedDate!.Value.Date.Add(TimeSpan.Parse(HoraComboBox.SelectedItem!.ToString()!));
            
            if (!_isEditMode)
            {
                AppointmentsService.CreateAppointment(identidadCliente, mascotaId, estado, servicioId, fechaInicio,
                    esEmergencia);
                Close(true);
                return;
            }

            if (_appointment == null || !_isEditMode) return;
            AppointmentsService.UpdateAppointment(citaId, mascotaId,
                estado, servicioId, fechaInicio, esEmergencia);
            Close(true);
        }

        private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close(false);
        }
    }
}