using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ProyectoBD2.Models;
using System;
using System.Collections.Generic;

namespace ProyectoBD2.Windows
{
    public partial class AppointmentDialog : Window
    {
        public Appointment ResultAppointment { get; private set; }
        private bool _isEditMode;
        
        // Sample data - in a real application, these would come from the database
        private List<(int id, string name)> _clientes = new List<(int id, string name)>
        {
            (1, "Juan Pérez"),
            (2, "María López"),
            (3, "Carlos Ruiz"),
            (4, "Ana González")
        };
        
        private List<(int id, string name)> _mascotas = new List<(int id, string name)>
        {
            (1, "Rocky (Labrador)"),
            (2, "Luna (Gato)"),
            (3, "Max (Bulldog)"),
            (4, "Coco (Poodle)")
        };
        
        private List<(int id, string name)> _servicios = new List<(int id, string name)>
        {
            (1, "Consulta General"),
            (2, "Vacunación"),
            (3, "Corte de Pelo"),
            (4, "Manicure"),
            (5, "Baño y Peluquería")
        };

        public AppointmentDialog()
        {
            InitializeComponent();
            _isEditMode = false;
            SetupControls();
            LoadSampleData();
        }

        public AppointmentDialog(Appointment existingAppointment)
        {
            InitializeComponent();
            _isEditMode = true;
            
            LoadSampleData();
            PopulateForm(existingAppointment);
            SetupControls();
        }

        private void LoadSampleData()
        {
            // Populate ComboBoxes with sample data
            var clienteCombo = this.FindControl<ComboBox>("ClienteComboBox");
            var mascotaCombo = this.FindControl<ComboBox>("MascotaComboBox");
            var servicioCombo = this.FindControl<ComboBox>("ServicioComboBox");
            
            // Load clientes
            foreach (var cliente in _clientes)
            {
                clienteCombo.Items.Add(new ComboBoxItem { Content = cliente.name, Tag = cliente.id });
            }
            
            // Load mascotas
            foreach (var mascota in _mascotas)
            {
                mascotaCombo.Items.Add(new ComboBoxItem { Content = mascota.name, Tag = mascota.id });
            }
            
            // Load servicios
            foreach (var servicio in _servicios)
            {
                servicioCombo.Items.Add(new ComboBoxItem { Content = servicio.name, Tag = servicio.id });
            }
        }

        private void SetupControls()
        {
            var saveButton = this.FindControl<Button>("SaveButton");
            var cancelButton = this.FindControl<Button>("CancelButton");

            // Set title based on mode
            this.Title = _isEditMode ? "Editar Cita" : "Nueva Cita";
            
            // Set defaults
            var estadoCombo = this.FindControl<ComboBox>("EstadoComboBox");
            if (estadoCombo.SelectedIndex == -1)
                estadoCombo.SelectedIndex = 0;

            // Set today's date as default for date pickers
            this.FindControl<DatePicker>("FechaInicioPicker").SelectedDate = DateTime.Today;
            this.FindControl<DatePicker>("FechaFinPicker").SelectedDate = DateTime.Today;

            // Setup button event handlers
            saveButton.Click += SaveButton_Click;
            cancelButton.Click += CancelButton_Click;
        }

        private void PopulateForm(Appointment appointment)
        {
            // Find and select the correct client
            var clienteCombo = this.FindControl<ComboBox>("ClienteComboBox");
            for (int i = 0; i < clienteCombo.Items.Count; i++)
            {
                var item = clienteCombo.Items[i] as ComboBoxItem;
                if (item != null && ((int)item.Tag) == appointment.ClienteID)
                {
                    clienteCombo.SelectedIndex = i;
                    break;
                }
            }
            
            // Find and select the correct mascota
            var mascotaCombo = this.FindControl<ComboBox>("MascotaComboBox");
            for (int i = 0; i < mascotaCombo.Items.Count; i++)
            {
                var item = mascotaCombo.Items[i] as ComboBoxItem;
                if (item != null && ((int)item.Tag) == appointment.MascotaID)
                {
                    mascotaCombo.SelectedIndex = i;
                    break;
                }
            }
            
            // Find and select the correct servicio
            var servicioCombo = this.FindControl<ComboBox>("ServicioComboBox");
            for (int i = 0; i < servicioCombo.Items.Count; i++)
            {
                var item = servicioCombo.Items[i] as ComboBoxItem;
                if (item != null && ((int)item.Tag) == appointment.ServicioID)
                {
                    servicioCombo.SelectedIndex = i;
                    break;
                }
            }
            
            // Set estado
            var estadoCombo = this.FindControl<ComboBox>("EstadoComboBox");
            for (int i = 0; i < estadoCombo.Items.Count; i++)
            {
                var item = estadoCombo.Items[i] as ComboBoxItem;
                if (item != null && item.Content.ToString() == appointment.Estado)
                {
                    estadoCombo.SelectedIndex = i;
                    break;
                }
            }
            
            // Set dates and times
            this.FindControl<DatePicker>("FechaInicioPicker").SelectedDate = new DateTimeOffset(appointment.FechaInicio);
            this.FindControl<TimePicker>("HoraInicioPicker").SelectedTime = appointment.FechaInicio.TimeOfDay;

            this.FindControl<DatePicker>("FechaFinPicker").SelectedDate = new DateTimeOffset(appointment.FechaFin);
            this.FindControl<TimePicker>("HoraFinPicker").SelectedTime = appointment.FechaFin.TimeOfDay;
            
            // Set emergency checkbox
            this.FindControl<CheckBox>("EsEmergenciaCheckBox").IsChecked = appointment.EsEmergencia;
        }

        private void SaveButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
{
    try
    {
        // Get values from form controls
        var clienteItem = this.FindControl<ComboBox>("ClienteComboBox").SelectedItem as ComboBoxItem;
        var mascotaItem = this.FindControl<ComboBox>("MascotaComboBox").SelectedItem as ComboBoxItem;
        var servicioItem = this.FindControl<ComboBox>("ServicioComboBox").SelectedItem as ComboBoxItem;
        var estadoItem = this.FindControl<ComboBox>("EstadoComboBox").SelectedItem as ComboBoxItem;

        // Get dates and times (with null-conditional operators to safely access values)
        var fechaInicio = this.FindControl<DatePicker>("FechaInicioPicker").SelectedDate ?? DateTimeOffset.Now;
        var horaInicio = this.FindControl<TimePicker>("HoraInicioPicker").SelectedTime ?? TimeSpan.Zero;

        var fechaFin = this.FindControl<DatePicker>("FechaFinPicker").SelectedDate ?? DateTimeOffset.Now;
        var horaFin = this.FindControl<TimePicker>("HoraFinPicker").SelectedTime ?? TimeSpan.Zero;

        // Combine date and time into DateTime, converting from DateTimeOffset to DateTime
        var fechaInicioCompleta = fechaInicio.Date.Add(horaInicio);
        var fechaFinCompleta = fechaFin.Date.Add(horaFin);

        // Get emergency status
        var esEmergencia = this.FindControl<CheckBox>("EsEmergenciaCheckBox").IsChecked ?? false;

        // Create result appointment
        ResultAppointment = new Appointment
        {
            CitaID = _isEditMode ? ((Appointment)this.DataContext).CitaID : 0,
            ClienteID = clienteItem != null ? (int)clienteItem.Tag : 0,
            ClienteNombre = clienteItem?.Content.ToString() ?? "",
            MascotaID = mascotaItem != null ? (int)mascotaItem.Tag : 0,
            MascotaNombre = mascotaItem?.Content.ToString() ?? "",
            ServicioID = servicioItem != null ? (int)servicioItem.Tag : 0,
            ServicioNombre = servicioItem?.Content.ToString() ?? "",
            Estado = estadoItem?.Content.ToString() ?? "Pendiente",
            FechaInicio = fechaInicioCompleta,
            FechaFin = fechaFinCompleta,
            EsEmergencia = esEmergencia
        };

        // Close with success result
        this.Close(true);
    }
    catch (Exception ex)
    {
        // In a real application, you would handle this error more gracefully
        Console.WriteLine($"Error saving appointment: {ex.Message}");
        this.Close(false);
    }
}

        private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Close with cancel result
            this.Close(false);
        }
    }
}