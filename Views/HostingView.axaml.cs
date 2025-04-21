using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Data;
using Avalonia.Data;
using ProyectoBD2.Models;
using ProyectoBD2.Services;

namespace ProyectoBD2.Views
{
    public partial class HostingView : UserControl
    {
        private enum ViewState { List, New, Edit, Editing }
        private ViewState _currentState;

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

            PetComboBox.IsEnabled = false;
            HostingDataGrid.SelectionChanged += HostingDataGrid_SelectionChanged;
            EntryDatePicker.SelectedDateChanged += (s,a) => LoadRooms();
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
                        DescripcionHabitacion = row["DescripcionHabitacion"] == DBNull.Value ? null : (string)row["DescripcionHabitacion"],
                        NombreMascota = row["NombreMascota"] == DBNull.Value ? null : (string)row["NombreMascota"],
                        NombreCliente = row["NombreCliente"] == DBNull.Value ? null : (string)row["NombreCliente"],
                        Telefono = row["Telefono"] == DBNull.Value ? null : (string)row["Telefono"],
                        FechaIngreso = row["FechaIngreso"] == DBNull.Value ? null : (DateTime)row["FechaIngreso"],
                        FechaSalida = row["FechaSalida"] == DBNull.Value ? null : (DateTime)row["FechaSalida"],
                        EstadoActual = row["EstadoActual"] == DBNull.Value ? null : (string)row["EstadoActual"],
                        Observaciones = row["Observaciones"] == DBNull.Value ? null : (string)row["Observaciones"],
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
                    NoSelectionPanel.IsVisible = false;
                    FormPanel.IsVisible = true;
                    SaveButton.Content = "Crear Reserva";
                    SaveButton.IsVisible = true;
                    CancelButton.IsVisible = true;
                    EditHostingButton.IsVisible = false;
                    DeleteHostingButton.IsVisible = false;
                    SetFormFieldsReadOnly(false);
                    ClearForm();
                    break;
                case ViewState.Edit:
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
            ClienteAutoCompleteBox.IsEnabled = !readOnly;
            PetComboBox.IsEnabled = !readOnly && ClienteAutoCompleteBox.SelectedItem is Client;
            EntryDatePicker.IsEnabled = !readOnly;
            ExitDatePicker.IsEnabled = !readOnly;
            RoomComboBox.IsEnabled = !readOnly && EntryDatePicker.SelectedDate != null && ExitDatePicker.SelectedDate != null && EntryDatePicker.SelectedDate < ExitDatePicker.SelectedDate;
            SpecialFoodCheck.IsEnabled = !readOnly;
            DailyWalkCheck.IsEnabled = !readOnly;
            GroomingCheck.IsEnabled = !readOnly;
            MedicationCheck.IsEnabled = !readOnly;
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
            PriceTextBlock.Text = "$0.00";
        }

        private void NewHostingButton_Click(object? sender, RoutedEventArgs e)
        {
            _currentState = ViewState.New;
            UpdateViewState();
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            _currentState = ViewState.List;
            UpdateViewState();
        }

        private void SaveButton_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
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
                
                if (_currentState == ViewState.New)
                {
                    DataServices.CreateReservation(petId, roomId, entryDate, exitDate, specialFood, dailyWalk, grooming, medication, notes);
                }
                // else
                // {
                //     DataServices.UpdateReservation(petId, roomId, entryDate, exitDate, specialFood, dailyWalk, grooming, medication, notes);
                // }
                
                LoadHostings();
                
                _currentState = ViewState.List;
                UpdateViewState();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving hosting: {ex.Message}");
            }
        }

        private void UpdateEstimatedPrice()
        {
            var entryDate = EntryDatePicker.SelectedDate;
            var exitDate = ExitDatePicker.SelectedDate;
            
            if (!entryDate.HasValue || !exitDate.HasValue || _rooms == null)
                return;
                
            var days = (exitDate.Value - entryDate.Value).Days;
            var basePrice = days * 50;
            
            var additionalServices = 0.0;
            if (SpecialFoodCheck.IsChecked == true) additionalServices += days * 500;
            if (DailyWalkCheck.IsChecked == true) additionalServices += days * 500;
            if (GroomingCheck.IsChecked == true) additionalServices += 500;
            if (MedicationCheck.IsChecked == true) additionalServices += days * 500;
            
            var totalPrice = basePrice + additionalServices;
            PriceTextBlock.Text = $"L. {totalPrice:F2}";
        }

        private void SearchButton_Click(object? sender, RoutedEventArgs e)
        {
            // Implement search logic
            var searchText = SearchTextBox.Text;
            var status = StatusFilter.SelectedIndex;
            var date = DateFilter.SelectedDate;
            
            // Call data service with filters
            // var filteredHostings = DataServices.FindHostings(searchText, status, date);
            // HostingDataGrid.ItemsSource = filteredHostings.DefaultView;
        }

        private void HostingDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (HostingDataGrid.SelectedItem is DataRowView selectedRow)
            {
                _currentState = ViewState.Edit;
                UpdateViewState();
            }
        }
        
        private void EditHostingButton_Click(object? sender, RoutedEventArgs e)
        {
            _currentState = ViewState.Editing;
            UpdateViewState();
        }

        private void DeleteHostingButton_Click(object? sender, RoutedEventArgs e)
        {
            // Implementar lógica para eliminar la reserva
            // Mostrar diálogo de confirmación
            // Si confirma, eliminar y regresar a ViewState.List
        }
    }
}