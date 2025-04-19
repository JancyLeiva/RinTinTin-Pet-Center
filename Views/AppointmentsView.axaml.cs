using Avalonia.Controls;
using System.Collections.ObjectModel;
using ProyectoBD2.Models;
using System;
using System.Data;
using System.Linq;
using Avalonia.VisualTree;
using ProyectoBD2.DataAccess;
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
        
        private void AppointmentsDataGridAutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "CitaId") e.Cancel = true;
        }

        private void LoadAreas()
        {
            _areas = ["Todas"];
            var data = DataServices.FindAreas();
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
                var date = AppointmentDatePicker.SelectedDate!.Value.ToString("yyyy-MM-dd");
                _appointments = [];

                var data = DataServices.FindAppointmentsByDate(date);

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
            var result = await dialog.ShowDialog<bool?>((topLevel as Window)!);
            if (result == true) LoadAppointments();
        }

        private async System.Threading.Tasks.Task EditAppointment()
        {
            if (AppointmentsDataGrid.SelectedItem is not Appointment selectedAppointment) return;

            var dialog = new AppointmentDialog(selectedAppointment);
        
            var result = await dialog.ShowDialog<bool>((this.GetVisualRoot() as Window)!);
        
            if (result) LoadAppointments();
        }

        private void DeleteAppointment()
        {
            try
            {
                if (AppointmentsDataGrid.SelectedItem is not Appointment selectedAppointment) return;
                DataServices.DeleteAppointment(selectedAppointment.CitaId);
                LoadAppointments();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}