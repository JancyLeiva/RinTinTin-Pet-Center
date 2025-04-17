using Avalonia.Controls;
using System.Collections.ObjectModel;
using ProyectoBD2.Models;
using System;
using System.Data;
using ProyectoBD2.Services;

namespace ProyectoBD2.Views
{
    public partial class AppointmentsView : UserControl
    {
        private ObservableCollection<Appointment>? _appointments;

        public AppointmentsView()
        {
            InitializeComponent();
            LoadAppointments();

            // Set up event handlers
            // DepartmentComboBox.SelectionChanged += (s, e) => FilterAppointments();
            // SearchTextBox.TextChanged += (s, e) => FilterAppointments();

            // AddAppointmentButton.Click += async (s, e) => await AddAppointment();
            // EditAppointmentButton.Click += async (s, e) => await EditAppointment();
            // DeleteAppointmentButton.Click += (s, e) => DeleteAppointment();
        }

        private void LoadAppointments()
        {
            try
            {
                int? serviceTypeId = null;
                const string queryMode = "Citas";
                const string? date = "2025-04-19";
                _appointments = [];
        
                var appointmentsData = AppointmentsService.FindAll(serviceTypeId, queryMode, date);

                foreach (DataRow row in appointmentsData.Rows)
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
                Console.WriteLine($"Loaded {appointmentsData.Rows.Count} appointments");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading appointments: {ex.Message}");
                AppointmentsDataGrid.ItemsSource = null;
            }
        }

        // private void LoadSampleData()
        // {
        //     _appointments = new ObservableCollection<Appointment>
        //     {
        //         new Appointment { 
        //             CitaID = 1, 
        //             ClienteID = 1, 
        //             ClienteNombre = "Juan Pérez", 
        //             MascotaID = 1, 
        //             MascotaNombre = "Rocky (Labrador)", 
        //             Estado = "Confirmada", 
        //             ServicioID = 1, 
        //             ServicioNombre = "Consulta General",
        //             FechaInicio = new DateTime(2024, 6, 15, 10, 0, 0),
        //             FechaFin = new DateTime(2024, 6, 15, 10, 30, 0),
        //             EsEmergencia = false
        //         },
        //         new Appointment { 
        //             CitaID = 2, 
        //             ClienteID = 2, 
        //             ClienteNombre = "María López", 
        //             MascotaID = 0, 
        //             MascotaNombre = "N/A", 
        //             Estado = "Pendiente", 
        //             ServicioID = 3, 
        //             ServicioNombre = "Corte de Pelo",
        //             FechaInicio = new DateTime(2024, 6, 15, 11, 30, 0),
        //             FechaFin = new DateTime(2024, 6, 15, 12, 15, 0),
        //             EsEmergencia = false
        //         },
        //         new Appointment { 
        //             CitaID = 3, 
        //             ClienteID = 3, 
        //             ClienteNombre = "Carlos Ruiz", 
        //             MascotaID = 2, 
        //             MascotaNombre = "Luna (Gato)", 
        //             Estado = "Confirmada", 
        //             ServicioID = 2, 
        //             ServicioNombre = "Vacunación",
        //             FechaInicio = new DateTime(2024, 6, 16, 9, 15, 0),
        //             FechaFin = new DateTime(2024, 6, 16, 10, 15, 0),
        //             EsEmergencia = true
        //         }
        //     };
        //
        //     AppointmentsDataGrid.ItemsSource = _appointments;
        // }

        // private void FilterAppointments()
        // {
        //     // Get the search text
        //     string searchText = SearchTextBox.Text?.ToLower() ?? "";
        //     
        //     // Get the selected department (this will need to be adjusted based on your new filtering criteria)
        //     var selectedItem = DepartmentComboBox.SelectedItem as ComboBoxItem;
        //     string department = selectedItem?.Content.ToString() ?? "Todos";
        //     
        //     // Create a filtered collection - adjust filtering based on new fields
        //     var filteredAppointments = _appointments.Where(a =>
        //         (department == "Todos" || 
        //          (department == "Veterinaria" && a.ServicioNombre.Contains("Consulta") || a.ServicioNombre.Contains("Vacunación")) ||
        //          (department == "Salón de Belleza" && (a.ServicioNombre.Contains("Corte") || a.ServicioNombre.Contains("Peluquería") || a.ServicioNombre.Contains("Manicure")))) &&
        //         (string.IsNullOrEmpty(searchText) ||
        //          a.ClienteNombre.ToLower().Contains(searchText) ||
        //          a.MascotaNombre.ToLower().Contains(searchText) ||
        //          a.ServicioNombre.ToLower().Contains(searchText))
        //     ).ToList();
        //     
        //     // Update the DataGrid
        //     AppointmentsDataGrid.ItemsSource = filteredAppointments;
        // }

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