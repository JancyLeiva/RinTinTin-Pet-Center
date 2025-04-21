using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Data;
using System.Linq;
using Avalonia;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using ProyectoBD2.Models;
using ProyectoBD2.Services;

namespace ProyectoBD2.Views
{
    public partial class HostingView : UserControl
    {
        private enum ViewState
        {
            List,
            New,
            Edit,
            Editing
        }

        private ViewState _currentState;

        private int? _currentEstadiaId;
        private Reservation? _selectedReservation;

        private ObservableCollection<Client>? _clients;
        private ObservableCollection<Pet>? _pets;
        private ObservableCollection<Reservation>? _reservations;
        private ObservableCollection<Room>? _rooms;

        public HostingView()
        {
            InitializeComponent();
            LoadClients();

            NewHostingButton.Click += NewHostingButton_Click;
            CancelButton.Click += CancelButton_Click;
            SaveButton.Click += SaveButton_Click;
            EditHostingButton.Click += EditHostingButton_Click;
            DeleteHostingButton.Click += DeleteHostingButton_Click;

            DateFilter.SelectedDate = DateTime.Today;
            DateFilter.SelectedDateChanged += (s, a) => LoadHostings();

            ClienteAutoCompleteBox.ValueMemberBinding = new Binding("Nombre");
            ClienteAutoCompleteBox.SelectionChanged += LoadMascotas;

            SearchTextBox.TextChanged += (s, a) => FilterReservations();

            PetComboBox.IsEnabled = false;
            HostingDataGrid.SelectionChanged += HostingDataGrid_SelectionChanged;
            EntryDatePicker.SelectedDateChanged += (s, a) => LoadRooms();
            ExitDatePicker.SelectedDateChanged += (s, a) => LoadRooms();
            RoomComboBox.SelectionChanged += (s, a) => UpdateEstimatedPrice();
            SpecialFoodCheck.IsCheckedChanged += (s, a) => UpdateEstimatedPrice();
            DailyWalkCheck.IsCheckedChanged += (s, a) => UpdateEstimatedPrice();
            GroomingCheck.IsCheckedChanged += (s, a) => UpdateEstimatedPrice();
            MedicationCheck.IsCheckedChanged += (s, a) => UpdateEstimatedPrice();

            LoadHostings();

            _currentState = ViewState.List;
            UpdateViewState();
        }

        private void HostingDataGridAutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Cancel = e.PropertyName switch
            {
                "EstadiaId" or "EstadoActual" or "ServicioAlimentacionEspecial" or "ServicioPaseoDiario"
                    or "ServicioBanoCepillado" or "ServicioMedicamento" or "FechaReserva" => true,
                _ => e.Cancel
            };
        }

        private void LoadHostings()
        {
            try
            {
                _reservations = [];
                var date = DateFilter.SelectedDate!.Value.ToString("yyyy-MM-dd");
                var data = DataServices.FindReservationsByDate(date);

                foreach (DataRow row in data.Rows)
                {
                    _reservations.Add(new Reservation
                    {
                        EstadiaId = row["EstadiaID"] == DBNull.Value ? null : (int)row["EstadiaID"],
                        HabitacionId = row["HabitacionID"] == DBNull.Value ? null : (int)row["HabitacionID"],
                        DescripcionHabitacion = row["DescripcionHabitacion"] == DBNull.Value
                            ? null
                            : (string)row["DescripcionHabitacion"],
                        NombreMascota = row["NombreMascota"] == DBNull.Value ? null : (string)row["NombreMascota"],
                        NombreCliente = row["NombreCliente"] == DBNull.Value ? null : (string)row["NombreCliente"],
                        Telefono = row["Telefono"] == DBNull.Value ? null : (string)row["Telefono"],
                        FechaIngreso = row["FechaIngreso"] == DBNull.Value ? null : (DateTime)row["FechaIngreso"],
                        FechaSalida = row["FechaSalida"] == DBNull.Value ? null : (DateTime)row["FechaSalida"],
                        EstadoActual = row["EstadoActual"] == DBNull.Value ? null : (string)row["EstadoActual"],
                        Observaciones = row["Observaciones"] == DBNull.Value ? null : (string)row["Observaciones"],
                        ServicioAlimentacionEspecial = row["ServicioAlimentacionEspecial"] == DBNull.Value
                            ? null
                            : (int)row["ServicioAlimentacionEspecial"],
                        ServicioPaseoDiario = row["ServicioPaseoDiario"] == DBNull.Value
                            ? null
                            : (int)row["ServicioPaseoDiario"],
                        ServicioBanoCepillado = row["ServicioBanoCepillado"] == DBNull.Value
                            ? null
                            : (int)row["ServicioBanoCepillado"],
                        ServicioMedicamento = row["ServicioMedicamento"] == DBNull.Value
                            ? null
                            : (int)row["ServicioMedicamento"],
                        FechaReserva = row["FechaReserva"] == DBNull.Value ? null : (DateTime)row["FechaReserva"]
                    });
                }

                HostingDataGrid.ItemsSource = _reservations;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading hostings: {ex.Message}");
            }
        }

        private void LoadClients()
        {
            _clients = [];
            const string busqueda = "";
            var data = DataServices.FindClientsOnAppointments(busqueda);
            Console.Write(data);

            foreach (DataRow row in data.Rows)
            {
                _clients.Add(new Client
                {
                    Nombre = row["Nombre"] == DBNull.Value ? null : (string)row["Nombre"],
                    NumIdentidad = row["NumIdentidad"] == DBNull.Value ? null : (string)row["NumIdentidad"],
                });
            }

            ClienteAutoCompleteBox.ItemsSource = _clients;
        }

        private void LoadMascotas(object? sender, SelectionChangedEventArgs e)
        {
            _pets = [];
            if (ClienteAutoCompleteBox.SelectedItem is not Client identidadCliente)
            {
                PetComboBox.IsEnabled = false;
                PetComboBox.ItemsSource = null;
                return;
            }

            var data = DataServices.FindPetsOnAppointments(identidadCliente.NumIdentidad);

            foreach (DataRow row in data.Rows)
            {
                _pets.Add(new Pet
                {
                    ClienteId = (int)row["ClienteID"],
                    Dueño = (string)row["Dueño"],
                    Nombre = (string)row["Mascota"],
                    MascotaId = (int)row["MascotaID"]
                });
            }

            PetComboBox.ItemsSource = _pets;
            PetComboBox.IsEnabled = true;
        }

        private void LoadRooms()
        {
            if (EntryDatePicker.SelectedDate == null || ExitDatePicker.SelectedDate == null ||
                EntryDatePicker.SelectedDate > ExitDatePicker.SelectedDate)
            {
                RoomComboBox.IsEnabled = false;
                return;
            }

            _rooms = [];
            var startDate = EntryDatePicker.SelectedDate?.ToString("yyyy-MM-dd");
            var endDate = ExitDatePicker.SelectedDate?.ToString("yyyy-MM-dd");
            var data = DataServices.FindRoomsAvailableByDate(startDate, endDate);

            foreach (DataRow row in data.Rows)
            {
                _rooms.Add(new Room
                {
                    HabitacionId = row["HabitacionID"] == DBNull.Value ? null : (int)row["HabitacionID"],
                    Descripcion = row["Descripcion"] == DBNull.Value ? null : (string)row["Descripcion"],
                    Estado = row["Estado"] == DBNull.Value ? null : (string)row["Estado"]
                });
            }

            RoomComboBox.ItemsSource = _rooms;
            RoomComboBox.IsEnabled = true;
        }

        private void UpdateViewState()
        {
            switch (_currentState)
            {
                case ViewState.List:
                    NoSelectionPanel.IsVisible = true;
                    FormPanel.IsVisible = false;
                    break;
                case ViewState.New:
                    FormTitle.Text = "Nueva Reserva";
                    ClienteAutoCompleteBox.IsEnabled = true;
                    NoSelectionPanel.IsVisible = false;
                    FormPanel.IsVisible = true;
                    SaveButton.Content = "Crear Reserva";
                    SaveButton.IsVisible = true;
                    CancelButton.IsVisible = true;
                    EditHostingButton.IsVisible = false;
                    DeleteHostingButton.IsVisible = false;
                    RoomTextBlock.Text = "Habitación";
                    SetFormFieldsReadOnly(false);
                    ClearForm();
                    break;
                case ViewState.Edit:
                    ClienteAutoCompleteBox.IsEnabled = false;
                    FormTitle.Text = "Detalles de Reserva";
                    NoSelectionPanel.IsVisible = false;
                    FormPanel.IsVisible = true;
                    SaveButton.IsVisible = false;
                    CancelButton.IsVisible = false;
                    EditHostingButton.IsVisible = true;
                    DeleteHostingButton.IsVisible = true;
                    SetFormFieldsReadOnly(true);
                    break;
                case ViewState.Editing:
                    ClienteAutoCompleteBox.IsEnabled = false;
                    FormTitle.Text = "Editar Reserva";
                    SaveButton.Content = "Guardar Cambios";
                    SaveButton.IsVisible = true;
                    CancelButton.IsVisible = true;
                    EditHostingButton.IsVisible = false;
                    DeleteHostingButton.IsVisible = false;
                    SetFormFieldsReadOnly(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetFormFieldsReadOnly(bool readOnly)
        {
            PetComboBox.IsEnabled = true;
            EntryDatePicker.IsEnabled = true;
            ExitDatePicker.IsEnabled = true;
            RoomComboBox.IsEnabled = true;
            SpecialFoodCheck.IsEnabled = true;
            DailyWalkCheck.IsEnabled = true;
            GroomingCheck.IsEnabled = true;
            MedicationCheck.IsEnabled = true;

            if (readOnly)
            {
                PetComboBox.IsHitTestVisible = false;
                EntryDatePicker.IsHitTestVisible = false;
                ExitDatePicker.IsHitTestVisible = false;
                RoomComboBox.IsHitTestVisible = false;
                SpecialFoodCheck.IsHitTestVisible = false;
                DailyWalkCheck.IsHitTestVisible = false;
                GroomingCheck.IsHitTestVisible = false;
                MedicationCheck.IsHitTestVisible = false;
            }
            else
            {
                PetComboBox.IsHitTestVisible = true;
                EntryDatePicker.IsHitTestVisible = true;
                ExitDatePicker.IsHitTestVisible = true;
                RoomComboBox.IsHitTestVisible = true;
                SpecialFoodCheck.IsHitTestVisible = true;
                DailyWalkCheck.IsHitTestVisible = true;
                GroomingCheck.IsHitTestVisible = true;
                MedicationCheck.IsHitTestVisible = true;
            }

            NotesTextBox.IsReadOnly = readOnly;
        }

        private void ClearForm()
        {
            ClienteAutoCompleteBox.Text = string.Empty;
            PetComboBox.SelectedIndex = -1;
            EntryDatePicker.SelectedDate = DateTime.Today;
            ExitDatePicker.SelectedDate = DateTime.Today.AddDays(1);
            RoomComboBox.SelectedIndex = -1;
            SpecialFoodCheck.IsChecked = false;
            DailyWalkCheck.IsChecked = false;
            GroomingCheck.IsChecked = false;
            MedicationCheck.IsChecked = false;
            NotesTextBox.Text = string.Empty;
            PriceTextBlock.Text = "L. 0.00";
        }

        private void NewHostingButton_Click(object? sender, RoutedEventArgs e)
        {
            HostingDataGrid.SelectedItem = null;
            _currentState = ViewState.New;
            UpdateViewState();
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_currentState == ViewState.Editing && _currentEstadiaId.HasValue)
            {
                LoadHostings();

                var reservationToReselect = _reservations?.FirstOrDefault(r => r.EstadiaId == _currentEstadiaId);

                if (reservationToReselect != null)
                {
                    HostingDataGrid.SelectedItem = reservationToReselect;
                    return;
                }
            }

            _currentState = _currentState == ViewState.Editing ? ViewState.Edit : ViewState.List;
            UpdateViewState();
        }

        private void SaveButton_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var validationMessage = ValidateReservationInput();
                if (!string.IsNullOrEmpty(validationMessage))
                {
                    ShowErrorMessage(validationMessage);
                    return;
                }

                var selectedPet = PetComboBox.SelectedItem as Pet;
                var selectedRoom = RoomComboBox.SelectedItem as Room;

                var petId = selectedPet?.MascotaId;
                var roomId = selectedRoom?.HabitacionId;
                var entryDate = EntryDatePicker.SelectedDate?.ToString("yyyy-MM-dd");
                var exitDate = ExitDatePicker.SelectedDate?.ToString("yyyy-MM-dd");
                var specialFood = SpecialFoodCheck.IsChecked == true ? 1 : 0;
                var dailyWalk = DailyWalkCheck.IsChecked == true ? 1 : 0;
                var grooming = GroomingCheck.IsChecked == true ? 1 : 0;
                var medication = MedicationCheck.IsChecked == true ? 1 : 0;
                var notes = NotesTextBox.Text;

                int? savedEstadiaId = null;

                if (_currentState == ViewState.New)
                {
                    DataServices.CreateReservation(petId, roomId, entryDate, exitDate, specialFood, dailyWalk, grooming,
                        medication, notes);

                    _currentState = ViewState.List;
                }
                else if (_currentState == ViewState.Editing)
                {
                    savedEstadiaId = _currentEstadiaId;

                    DataServices.UpdateReservation(_currentEstadiaId, petId, roomId, entryDate, exitDate, specialFood,
                        dailyWalk, grooming, medication, notes);

                    _currentState = ViewState.Edit;
                }

                LoadHostings();

                if (savedEstadiaId.HasValue)
                {
                    var updatedItem = _reservations?.FirstOrDefault(r => r.EstadiaId == savedEstadiaId);
                    if (updatedItem != null)
                    {
                        HostingDataGrid.SelectedItem = updatedItem;
                    }
                }

                UpdateViewState();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving hosting: {ex.Message}");
                ShowErrorMessage($"Error al guardar la reserva: {ex.Message}");
            }
        }

        private string ValidateReservationInput()
        {
            if (ClienteAutoCompleteBox.SelectedItem is not Client)
            {
                return "Debe seleccionar un cliente válido.";
            }

            if (PetComboBox.SelectedItem is not Pet)
            {
                return "Debe seleccionar una mascota.";
            }

            if (!EntryDatePicker.SelectedDate.HasValue)
            {
                return "Debe seleccionar una fecha de ingreso.";
            }

            if (!ExitDatePicker.SelectedDate.HasValue)
            {
                return "Debe seleccionar una fecha de salida.";
            }

            if (EntryDatePicker.SelectedDate > ExitDatePicker.SelectedDate)
            {
                return "La fecha de ingreso no puede ser posterior a la fecha de salida.";
            }

            if (_currentState == ViewState.New && EntryDatePicker.SelectedDate < DateTime.Today)
            {
                return "La fecha de ingreso no puede estar en el pasado para nuevas reservas.";
            }

            return RoomComboBox.SelectedItem is not Room ? "Debe seleccionar una habitación disponible." : string.Empty;
        }

        private void ShowErrorMessage(string message)
        {
            Console.WriteLine($"ERROR: {message}");

            var messageBox = new Window
            {
                Title = "Error",
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new StackPanel
                {
                    Margin = new Thickness(20),
                    Children =
                    {
                        new TextBlock
                        {
                            Text = message,
                            TextWrapping = TextWrapping.Wrap,
                            MaxWidth = 400,
                            Margin = new Thickness(0, 0, 0, 15)
                        },
                        new Button
                        {
                            Content = "Aceptar",
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Width = 100
                        }
                    }
                }
            };

            if (messageBox.Content is StackPanel sp && sp.Children[1] is Button btn)
            {
                btn.Click += (_, _) => messageBox.Close();
            }

            messageBox.ShowDialog((VisualRoot as Window)!);
        }

        private void UpdateEstimatedPrice()
        {
            var entryDate = EntryDatePicker.SelectedDate;
            var exitDate = ExitDatePicker.SelectedDate;

            if (!entryDate.HasValue || !exitDate.HasValue || _rooms == null)
                return;

            var days = (exitDate.Value - entryDate.Value).Days;
            var basePrice = days * 500;

            var additionalServices = 0.0;
            if (SpecialFoodCheck.IsChecked == true) additionalServices += days * 250;
            if (DailyWalkCheck.IsChecked == true) additionalServices += days * 250;
            if (GroomingCheck.IsChecked == true) additionalServices += 500;
            if (MedicationCheck.IsChecked == true) additionalServices += days * 250;

            var totalPrice = basePrice + additionalServices;
            PriceTextBlock.Text = $"L. {totalPrice:F2}";
        }

        private void FilterReservations()
        {
            var filterText = SearchTextBox.Text?.ToLower() ?? string.Empty;
            var filteredReservations = _reservations?.Where(r =>
                r.NombreCliente?.ToLower().Contains(filterText, StringComparison.CurrentCultureIgnoreCase) == true ||
                r.NombreMascota?.ToLower().Contains(filterText, StringComparison.CurrentCultureIgnoreCase) == true ||
                r.DescripcionHabitacion?.ToLower().Contains(filterText, StringComparison.CurrentCultureIgnoreCase) ==
                true).ToList();

            HostingDataGrid.ItemsSource = filteredReservations;
        }

        private void HostingDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (HostingDataGrid.SelectedItem == null) return;
            _selectedReservation = HostingDataGrid.SelectedItem as Reservation;
            _currentEstadiaId = _selectedReservation?.EstadiaId;

            RoomTextBlock.Text =
                $"Habitación (Actual: {_selectedReservation?.HabitacionId} - {_selectedReservation?.DescripcionHabitacion})";

            ClienteAutoCompleteBox.Text = _selectedReservation?.NombreCliente;

            var client = _clients?.FirstOrDefault(c => c.Nombre == _selectedReservation?.NombreCliente);
            if (client != null)
            {
                ClienteAutoCompleteBox.SelectedItem = client;

                var clientId = _pets?.FirstOrDefault()?.ClienteId;
                if (clientId.HasValue)
                {
                    var pet = _pets?.FirstOrDefault(p => p.Nombre == _selectedReservation?.NombreMascota);
                    if (pet != null)
                    {
                        PetComboBox.SelectedItem = pet;
                    }
                }
            }

            EntryDatePicker.SelectedDate = _selectedReservation?.FechaIngreso;
            ExitDatePicker.SelectedDate = _selectedReservation?.FechaSalida;

            LoadRooms();

            NotesTextBox.Text = _selectedReservation?.Observaciones;

            SpecialFoodCheck.IsChecked = _selectedReservation?.ServicioAlimentacionEspecial == 1;
            DailyWalkCheck.IsChecked = _selectedReservation?.ServicioPaseoDiario == 1;
            GroomingCheck.IsChecked = _selectedReservation?.ServicioBanoCepillado == 1;
            MedicationCheck.IsChecked = _selectedReservation?.ServicioMedicamento == 1;

            _currentState = ViewState.Edit;
            UpdateViewState();
        }

        private void EditHostingButton_Click(object? sender, RoutedEventArgs e)
        {
            _currentState = ViewState.Editing;
            UpdateViewState();
        }

        private void DeleteHostingButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_selectedReservation == null) return;

            DataServices.DeleteReservation(_selectedReservation.EstadiaId);
            LoadHostings();
            _currentState = ViewState.List;
            UpdateViewState();
        }
    }
}