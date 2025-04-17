using Avalonia.Controls;
using System.Collections.ObjectModel;
using ProyectoBD2.Models;
using System;
using System.Data;
using System.Linq;
using ProyectoBD2.Services;

namespace ProyectoBD2.Views
{
    public partial class AppointmentsView : UserControl
    {
        private ObservableCollection<Appointment>? _appointments;
        private ObservableCollection<string>? _areas;

        public AppointmentsView()
        {
            InitializeComponent();
            LoadAppointments();
            LoadAreas();

            // Set up event handlers
            DepartmentComboBox.SelectionChanged += (s, e) => FilterAppointments();
            SearchTextBox.TextChanged += (s, e) => FilterAppointments();

            // AddAppointmentButton.Click += async (s, e) => await AddAppointment();
            // EditAppointmentButton.Click += async (s, e) => await EditAppointment();
            // DeleteAppointmentButton.Click += (s, e) => DeleteAppointment();
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
                const string? date = "2025-04-19";
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

        // private async System.Threading.Tasks.Task AddAppointment()
        // {
        //     // Create a new dialog for adding an appointment
        //     var dialog = new AppointmentDialog();
        //
        //     // Show the dialog as a modal and get the result
        //     var topLevel = TopLevel.GetTopLevel(this);
        //     var result = await dialog.ShowDialog<bool?>(topLevel as Window);
        //
        //     // If the dialog was confirmed and we have a result appointment
        //     if (result == true && dialog.ResultAppointment != null)
        //     {
        //         // Generate a new ID (in a real app, this would come from the database)
        //         dialog.ResultAppointment.CitaID = _appointments.Count > 0 ? 
        //             _appointments.Max(a => a.CitaID) + 1 : 1;
        //
        //         // Add the new appointment to the collection
        //         _appointments.Add(dialog.ResultAppointment);
        //
        //         // Refresh the filtered view
        //         FilterAppointments();
        //     }
        // }

        // private async System.Threading.Tasks.Task EditAppointment()
        // {
        //     // Get the selected appointment
        //     var selectedAppointment = AppointmentsDataGrid.SelectedItem as Appointment;
        //     if (selectedAppointment == null)
        //         return;
        //
        //     // Create a dialog for editing, initialized with the selected appointment
        //     var dialog = new AppointmentDialog(selectedAppointment)
        //     {
        //         DataContext = selectedAppointment
        //     };
        //
        //     // Show the dialog as a modal and get the result
        //     var topLevel = TopLevel.GetTopLevel(this);
        //     var result = await dialog.ShowDialog<bool?>(topLevel as Window);
        //
        //     // If the dialog was confirmed and we have a result appointment
        //     if (result == true && dialog.ResultAppointment != null)
        //     {
        //         // Find the original appointment in the collection
        //         var originalAppointment = _appointments.FirstOrDefault(a => a.CitaID == selectedAppointment.CitaID);
        //         if (originalAppointment != null)
        //         {
        //             // Update the appointment data
        //             int index = _appointments.IndexOf(originalAppointment);
        //             _appointments[index] = dialog.ResultAppointment;
        //
        //             // Refresh the filtered view
        //             FilterAppointments();
        //         }
        //     }
        // }

        // private void DeleteAppointment()
        // {
        //     // Get the selected appointment
        //     var selectedAppointment = AppointmentsDataGrid.SelectedItem as Appointment;
        //     if (selectedAppointment == null)
        //         return;
        //
        //     // Remove the appointment from the collection
        //     _appointments.Remove(selectedAppointment);
        //
        //     // Refresh the filtered view
        //     FilterAppointments();
        // }
    }
}