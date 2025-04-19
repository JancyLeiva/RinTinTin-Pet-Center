using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.ObjectModel;
using System.Data;
using ProyectoBD2.Models;
using ProyectoBD2.Services;

namespace ProyectoBD2.Views;

public partial class ClientsView : UserControl
{
    private ObservableCollection<Client>? _clients;
    private ObservableCollection<Pet>? _pets;
    private Client? _newClient;
    private ObservableCollection<string>? _newPets;
    private enum ViewState
    {
        NoSelection,
        ViewingClient,
        CreatingClient
    }
    
    private ViewState _currentState = ViewState.NoSelection;
    
    public ClientsView()
    {
        InitializeComponent();
        LoadClients();

        SearchTextBox.TextChanged += FilterClients;
        ClientsDataGrid.SelectionChanged += ClientsDataGrid_SelectionChanged;
        AddClientButton.Click += AddClientButton_Click;
        CancelNewClientButton.Click += CancelNewClientButton_Click;
        ContinueToMascotasButton.Click += ContinueToMascotasButton_Click;
        BackToClientDetailsButton.Click += BackToClientDetailsButton_Click;
        SaveNewClientButton.Click += SaveNewClientButton_Click;
        // AddNewPetButton.Click += AddNewPetButton_Click;
        CreatePetsTab.Tapped += ContinueToMascotasButton_Click;
        
        NewPetsListBox.ItemsSource = _pets;
        UpdateViewState();
    }

    private void LoadClients()
    {
        _clients = [];
        var data = DataServices.FindAllClients();

        foreach (DataRow row in data.Rows)
        {
            _clients.Add(new Client
            {
                ClienteId = (int)row["ClienteID"],
                Nombre = (string)row["Nombre"],
                Telefono = (string)row["Telefono"],
                Correo = (string)row["Correo"],
                Direccion = (string)row["Direccion"],
                TelefonoAdicional = (string)row["TelefonoAdicional"],
                NumIdentidad = (string)row["NumIdentidad"],
                Activo = (bool)row["Activo"],
            });
        }
        
        ClientsDataGrid.ItemsSource = _clients;
    }

    private void FilterClients(object? sender, TextChangedEventArgs? e)
    {
        var filterText = SearchTextBox.Text?.ToLower() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(filterText))
        {
            ClientsDataGrid.ItemsSource = _clients;
            return;
        }

        var filteredClients = new ObservableCollection<Client>();
        if (_clients != null)
            foreach (var client in _clients)
            {
                if (client.Nombre?.ToLower().Contains(filterText, StringComparison.CurrentCultureIgnoreCase) == true ||
                    client.NumIdentidad?.ToLower().Contains(filterText, StringComparison.CurrentCultureIgnoreCase) == true)
                {
                    filteredClients.Add(client);
                }
            }

        ClientsDataGrid.ItemsSource = filteredClients;
    }
    
    private void ClientsDataGridAutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        e.Cancel = e.PropertyName switch
        {
            "ClienteId" or "NumIdentidad" or "Activo" => true,
            _ => e.Cancel
        };
    }
    private void PetsDataGridAutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        e.Cancel = e.PropertyName switch
        {
            "ClienteId" or "MascotaId" or "Activo" or "Dueño" => true,
            _ => e.Cancel
        };
    }
    
    private void ClientsDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs? e)
    {
        _pets = [];
        if (ClientsDataGrid.SelectedItem is not Client selectedClient)
        {
            _currentState = ViewState.NoSelection;
            UpdateViewState();
            return;
        }
        
        var selectedClientId = selectedClient.ClienteId;
        var data = DataServices.FindPetByClientId(selectedClientId);

        foreach (DataRow row in data.Rows)
        {
            _pets.Add(new Pet
            {
                ClienteId = (int)row["ClienteID"],
                MascotaId = (int)row["MascotaID"],
                Nombre = (string)row["Nombre"],
                Especie = (string)row["Especie"],
                Raza = (string)row["Raza"],
                Peso = (decimal)row["Peso"],
                Edad = (int)row["Edad"],
                Color = (string)row["Color"],
                Descripcion = (string)row["Descripcion"],
                Activo = (bool)row["Activo"],
            });
        }
        
        PetsDataGrid.ItemsSource = _pets;
        
        NumIdentidadClienteTextBox.Text = selectedClient.NumIdentidad;
        NombreClienteTextBox.Text = selectedClient.Nombre;
        TelefonoClienteTextBox.Text = selectedClient.Telefono;
        CorreoClienteTextBox.Text = selectedClient.Correo ?? "";
        DireccionClienteTextBox.Text = selectedClient.Direccion ?? "";
        TelefonoAdicionalClienteTextBox.Text = selectedClient.TelefonoAdicional ?? "";
        
        _currentState = ViewState.ViewingClient;
        UpdateViewState();
    }
    
    private void AddClientButton_Click(object? sender, RoutedEventArgs? e)
    {
        if (ClientsDataGrid.SelectedItem != null) ClientsDataGrid.SelectedItem = null;
        
        _currentState = ViewState.CreatingClient;
        UpdateViewState();
        
        NumIdentidadClienteNuevoTextBox.Text = string.Empty;
        NombreClienteNuevoTextBox.Text = string.Empty;
        TelefonoClienteNuevoTextBox.Text = string.Empty;
        CorreoClienteNuevoTextBox.Text = string.Empty;
        DireccionClienteNuevoTextBox.Text = string.Empty;
        TelefonoAdicionalClienteNuevoTextBox.Text = string.Empty;
        
        CreateClientTabControl.SelectedIndex = 0;
        CreatePetsTab.IsEnabled = false;
    }
    
    private void CancelNewClientButton_Click(object? sender, RoutedEventArgs? e)
    {
        _currentState = ClientsDataGrid.SelectedItem != null ? ViewState.ViewingClient : ViewState.NoSelection;
        UpdateViewState();
    }
    
    private void ContinueToMascotasButton_Click(object? sender, RoutedEventArgs? e)
    {
        if(!CreatePetsTab.IsEnabled) CreatePetsTab.IsEnabled = true;
        CreateClientTabControl.SelectedIndex = 1;

        _newClient = new Client
        {
            NumIdentidad = NumIdentidadClienteNuevoTextBox.Text?.Trim(),
            Nombre = NombreClienteNuevoTextBox.Text?.Trim(),
            Telefono = TelefonoClienteNuevoTextBox.Text?.Trim(),
            Correo = CorreoClienteNuevoTextBox.Text?.Trim(),
            Direccion = DireccionClienteNuevoTextBox.Text?.Trim(),
            TelefonoAdicional = TelefonoAdicionalClienteNuevoTextBox.Text?.Trim(),
        };
        
        Console.WriteLine(_newClient);
    }
    
    private void BackToClientDetailsButton_Click(object? sender, RoutedEventArgs? e)
    {
        CreateClientTabControl.SelectedIndex = 0;
    }
    
    private void SaveNewClientButton_Click(object? sender, RoutedEventArgs? e)
    {
        _currentState = ViewState.NoSelection;
        UpdateViewState();
    }
    
    // private void AddNewPetButton_Click(object? sender, RoutedEventArgs? e)
    // {
    //     var petName = NewPetNameTextBox.Text?.Trim() ?? string.Empty;
    //     var petSpecies = (NewPetSpeciesComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "No especificado";
    //     var petBreed = NewPetBreedTextBox.Text?.Trim() ?? string.Empty;
    //     var petAge = NewPetAgeNumeric.Value;
    //
    //     if (string.IsNullOrWhiteSpace(petName)) return;
    //     var petDisplay = $"{petName} - {petSpecies} - {petBreed} - {petAge} años";
    //     // _newPets.Add(petDisplay);
    //         
    //     NewPetNameTextBox.Text = string.Empty;
    //     NewPetSpeciesComboBox.SelectedIndex = -1;
    //     NewPetBreedTextBox.Text = string.Empty;
    //     NewPetAgeNumeric.Value = 0;
    // }
    
    private void UpdateViewState()
    {
        switch (_currentState)
        {
            case ViewState.NoSelection:
                NoSelectionPanel.IsVisible = true;
                ViewClientTabControl.IsVisible = false;
                CreateClientTabControl.IsVisible = false;
                break;
                
            case ViewState.ViewingClient:
                NoSelectionPanel.IsVisible = false;
                ViewClientTabControl.IsVisible = true;
                CreateClientTabControl.IsVisible = false;
                break;
                
            case ViewState.CreatingClient:
                NoSelectionPanel.IsVisible = false;
                ViewClientTabControl.IsVisible = false;
                CreateClientTabControl.IsVisible = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}