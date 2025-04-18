using Avalonia.Controls;
using System.Collections.ObjectModel;
using ProyectoBD2.Models;
using System;
using System.Data;
using System.Linq;
using ProyectoBD2.Services;
using ProyectoBD2.Windows;

namespace ProyectoBD2.Views
{
    public partial class AppointmentsView : UserControl
    {
        private ObservableCollection<Appointment>? _appointments;
        private ObservableCollection<string>? _areas;

        public AppointmentsView()
        {
            InitializeComponent();
            AppointmentDatePicker.SelectedDate = DateTime.Now.Date;
            LoadAppointments();
            LoadAreas();

            DepartmentComboBox.SelectionChanged += (s, e) => FilterAppointments();
            SearchTextBox.TextChanged += (s, e) => FilterAppointments();
            AppointmentDatePicker.Initialized += (s, e) => LoadAppointments();
            AppointmentDatePicker.SelectedDateChanged += (s, e) => LoadAppointments();

            AddAppointmentButton.Click += async (s, e) => await AddAppointment();
            EditAppointmentButton.Click += async (s, e) => await EditAppointment();
            DeleteAppointmentButton.Click += (s, e) => DeleteAppointment();
        }

        private void LoadAreas()
        {
            _areas = ["Todas"];
            var data = AppointmentsService.FindAreas();
            foreach (DataRow row in data.Rows)
            {
                _areas.Add((string)row["Area"]);
            }

            DepartmentComboBox.ItemsSource = _areas;
        }

        private void LoadAppointments()
        {
            try
            {
                int? serviceTypeId = null;
                const string queryMode = "Citas";
                var date = AppointmentDatePicker.SelectedDate!.Value.ToString("yyyy-MM-dd");
                _appointments = [];

                var data = AppointmentsService.FindAll(serviceTypeId, queryMode, date);

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

                AppointmentsDataGrid.ItemsSource = _appointments;
                Console.WriteLine($"Loaded {data.Rows.Count} appointments");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading appointments: {ex.Message}");
                AppointmentsDataGrid.ItemsSource = null;
            }
        }

        private void FilterAppointments()
        {
            var searchText = SearchTextBox.Text?.ToLower() ?? "";
            var selectedArea = DepartmentComboBox.SelectedItem as string ?? "Todas";

            if (_appointments == null) return;

            var filteredAppointments = _appointments.Where(a =>
                (selectedArea == "Todas" || a.TipoServicio!.Contains(selectedArea)) &&
                (string.IsNullOrEmpty(searchText) ||
                 a.Cliente!.Contains(searchText, StringComparison.CurrentCultureIgnoreCase) ||
                 a.Mascota!.Contains(searchText, StringComparison.CurrentCultureIgnoreCase) ||
                 a.Servicio!.Contains(searchText, StringComparison.CurrentCultureIgnoreCase) ||
                 a.TipoServicio!.Contains(searchText, StringComparison.CurrentCultureIgnoreCase))
            ).ToList();

            AppointmentsDataGrid.ItemsSource = filteredAppointments;
        }

        private async System.Threading.Tasks.Task AddAppointment()
        {
            var dialog = new AppointmentDialog();

            var topLevel = TopLevel.GetTopLevel(this);
            var result = await dialog.ShowDialog<bool?>(topLevel as Window);
        }

        private async System.Threading.Tasks.Task EditAppointment()
        {
        }

        private void DeleteAppointment()
        {
            var selectedAppointment = AppointmentsDataGrid.SelectedItem as Appointment;
            if (selectedAppointment == null)
                return;

            _appointments?.Remove(selectedAppointment);

            FilterAppointments();
        }
    }
}