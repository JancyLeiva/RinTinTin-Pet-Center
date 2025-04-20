using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Interactivity;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using Avalonia.Layout;
using Avalonia.Threading;
using Microsoft.IdentityModel.Tokens;
using ProyectoBD2.Models;
using ProyectoBD2.Services;

namespace ProyectoBD2.Views;

public partial class ClientsView : UserControl
{
    private ObservableCollection<Client>? _clients;
    private ObservableCollection<Pet>? _pets;
    private Client? _originalClientData;
    private Client? _newClient;
    private ObservableCollection<Pet>? _newPets;

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
        BackToNewClientFormButton.Click += BackToNewClientFormButton_Click;
        SaveNewClientButton.Click += SaveNewClientButton_Click;
        AddPetButton.Click += (a, s) => AddPet_Click();
        AddNewPetButton.Click += AddNewPetButton_Click;
        CreatePetsTab.Tapped += ContinueToMascotasButton_Click;
        PetsDataGrid.CellEditEnded += PetsDataGrid_CellEditEnded;

        EditClientButton.Click += EditClientButton_Click;
        DeleteClientButton.Click += DeleteClientButton_Click;
        CancelEditButton.Click += CancelEditButton_Click;
        SaveClientButton.Click += SaveClientButton_Click;

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
                ClienteId = row["ClienteID"] == DBNull.Value ? null : (int?)row["ClienteID"],
                Nombre = row["Nombre"] == DBNull.Value ? null : (string)row["Nombre"],
                Telefono = row["Telefono"] == DBNull.Value ? null : (string)row["Telefono"],
                Correo = row["Correo"] == DBNull.Value ? null : (string)row["Correo"],
                Direccion = row["Direccion"] == DBNull.Value ? null : (string)row["Direccion"],
                TelefonoAdicional = row["TelefonoAdicional"] == DBNull.Value ? null : (string)row["TelefonoAdicional"],
                NumIdentidad = row["NumIdentidad"] == DBNull.Value ? null : (string)row["NumIdentidad"],
                Activo = row["Activo"] == DBNull.Value ? null : (bool?)row["Activo"],
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
                    client.NumIdentidad?.ToLower().Contains(filterText, StringComparison.CurrentCultureIgnoreCase) ==
                    true)
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

        if (sender is DataGrid grid && e.PropertyName == "Descripcion")
        {
            Dispatcher.UIThread.Post(() =>
            {
                var hasActionColumn = grid.Columns.Any(c =>
                    c is DataGridTemplateColumn column &&
                    column.Header?.ToString() == "Acciones");

                if (hasActionColumn) grid.Columns.Remove(grid.Columns.First(c => c.Header?.ToString() == "Acciones"));

                var actionColumn = new DataGridTemplateColumn
                {
                    Header = "Acciones",
                    Width = DataGridLength.Auto,
                    CellTemplate = new FuncDataTemplate<Pet>((pet, namescope) =>
                    {
                        var trashIcon = new PathIcon
                        {
                            Data = StreamGeometry.Parse(
                                "M6.5 1h3a.5.5 0 0 1 .5.5v1h-4v-1a.5.5 0 0 1 .5-.5ZM11 2.5v-1A1.5 1.5 0 0 0 9.5 0h-3A1.5 1.5 0 0 0 5 1.5v1H2.506a.58.58 0 0 0-.01 0H1.5a.5.5 0 0 0 0 1h.538l.853 10.66A2 2 0 0 0 4.885 16h6.23a2 2 0 0 0 1.994-1.84l.853-10.66h.538a.5.5 0 0 0 0-1h-.995a.59.59 0 0 0-.01 0H11Zm1.958 1-.846 10.58a1 1 0 0 1-.997.92h-6.23a1 1 0 0 1-.997-.92L3.042 3.5h9.916Zm-7.487 1a.5.5 0 0 1 .528.47l.5 8.5a.5.5 0 0 1-.998.06L5 5.03a.5.5 0 0 1 .47-.53Zm5.058 0a.5.5 0 0 1 .47.53l-.5 8.5a.5.5 0 1 1-.998-.06l.5-8.5a.5.5 0 0 1 .528-.47ZM8 4.5a.5.5 0 0 1 .5.5v8.5a.5.5 0 0 1-1 0V5a.5.5 0 0 1 .5-.5Z"),
                            Width = 16,
                            Height = 16,
                            Foreground = Brushes.White
                        };

                        var button = new Button
                        {
                            Content = trashIcon,
                            Background = new SolidColorBrush(Color.Parse("#DC3545")),
                            Foreground = Brushes.White,
                            Padding = new Thickness(8, 4, 8, 4),
                            HorizontalAlignment = HorizontalAlignment.Center
                        };

                        button.Click += (s, args) => DeletePet_Click(pet);

                        return button;
                    })
                };

                grid.Columns.Add(actionColumn);
            });
        }
    }

    private void NewPetsDataGridAutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        e.Cancel = e.PropertyName switch
        {
            "ClienteId" or "MascotaId" or "Activo" or "Dueño" => true,
            _ => e.Cancel
        };

        if (sender is DataGrid grid && e.PropertyName == "Descripcion")
        {
            Dispatcher.UIThread.Post(() =>
            {
                var hasActionColumn = grid.Columns.Any(c =>
                    c is DataGridTemplateColumn column &&
                    column.Header?.ToString() == "Acciones");

                if (hasActionColumn) grid.Columns.Remove(grid.Columns.First(c => c.Header?.ToString() == "Acciones"));

                var actionColumn = new DataGridTemplateColumn
                {
                    Header = "Acciones",
                    Width = DataGridLength.Auto,
                    CellTemplate = new FuncDataTemplate<Pet>((pet, namescope) =>
                    {
                        var trashIcon = new PathIcon
                        {
                            Data = StreamGeometry.Parse(
                                "M6.5 1h3a.5.5 0 0 1 .5.5v1h-4v-1a.5.5 0 0 1 .5-.5ZM11 2.5v-1A1.5 1.5 0 0 0 9.5 0h-3A1.5 1.5 0 0 0 5 1.5v1H2.506a.58.58 0 0 0-.01 0H1.5a.5.5 0 0 0 0 1h.538l.853 10.66A2 2 0 0 0 4.885 16h6.23a2 2 0 0 0 1.994-1.84l.853-10.66h.538a.5.5 0 0 0 0-1h-.995a.59.59 0 0 0-.01 0H11Zm1.958 1-.846 10.58a1 1 0 0 1-.997.92h-6.23a1 1 0 0 1-.997-.92L3.042 3.5h9.916Zm-7.487 1a.5.5 0 0 1 .528.47l.5 8.5a.5.5 0 0 1-.998.06L5 5.03a.5.5 0 0 1 .47-.53Zm5.058 0a.5.5 0 0 1 .47.53l-.5 8.5a.5.5 0 1 1-.998-.06l.5-8.5a.5.5 0 0 1 .528-.47ZM8 4.5a.5.5 0 0 1 .5.5v8.5a.5.5 0 0 1-1 0V5a.5.5 0 0 1 .5-.5Z"),
                            Width = 16,
                            Height = 16,
                            Foreground = Brushes.White
                        };

                        var button = new Button
                        {
                            Content = trashIcon,
                            Background = new SolidColorBrush(Color.Parse("#DC3545")),
                            Foreground = Brushes.White,
                            Padding = new Thickness(8, 4, 8, 4),
                            HorizontalAlignment = HorizontalAlignment.Center
                        };

                        button.Click += (s, args) =>
                        {
                            if (_newPets == null) return;
                            _newPets.Remove(pet);
                            NewPetsDataGrid.ItemsSource = _newPets;
                        };

                        return button;
                    })
                };

                grid.Columns.Add(actionColumn);
            });
        }
    }

    private void AddPet_Click()
    {
        var clienteId = ClientsDataGrid.SelectedItem is Client selectedClient
            ? selectedClient.ClienteId
            : null;

        if (clienteId == null) return;
        if (NombreMascotaTextBox == null || EspecieMascotaTextBox == null || RazaMascotaTextBox == null ||
            ColorMascotaTextBox == null ||
            PesoMascotaNumeric == null || EdadMascotaNumeric == null ||
            DescripcionMascotaTextBox == null) return;
        DataServices.CreatePet(
            NombreMascotaTextBox.Text,
            EspecieMascotaTextBox.Text,
            RazaMascotaTextBox.Text,
            PesoMascotaNumeric.Value,
            (int)EdadMascotaNumeric.Value!,
            ColorMascotaTextBox.Text,
            DescripcionMascotaTextBox.Text,
            clienteId);

        LoadPetsByClient(clienteId);
    }

    private void DeletePet_Click(Pet pet)
    {
        if (pet.MascotaId == null) return;

        try
        {
            DataServices.DeletePet(pet.MascotaId);
            LoadPetsByClient(pet.ClienteId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar mascota: {ex.Message}");
        }
    }

    private static void PetsDataGrid_CellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        if (e.Row.DataContext is not Pet pet) return;
        try
        {
            DataServices.UpdatePet(
                pet.MascotaId,
                pet.Nombre,
                pet.Especie,
                pet.Raza,
                pet.Peso,
                pet.Edad,
                pet.Color,
                pet.Descripcion
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar mascota: {ex.Message}");
        }
    }

    private void LoadPetsByClient(int? clientId)
    {
        _pets = [];
        var data = DataServices.FindPetByClientId(clientId);

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
    }

    private void ClientsDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs? e)
    {
        if (ClientsDataGrid.SelectedItem is not Client selectedClient)
        {
            _currentState = ViewState.NoSelection;
            UpdateViewState();
            return;
        }

        var selectedClientId = selectedClient.ClienteId;
        LoadPetsByClient(selectedClientId);

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
        _newClient = new Client();
        _newPets = [];
        if (ClientsDataGrid.SelectedItem != null) ClientsDataGrid.SelectedItem = null;

        _currentState = ViewState.CreatingClient;
        UpdateViewState();

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
        if (NombreClienteNuevoTextBox == null || TelefonoClienteNuevoTextBox == null ||
            CorreoClienteNuevoTextBox == null || DireccionClienteNuevoTextBox == null ||
            NumIdentidadClienteNuevoTextBox == null) return;

        if (!CreatePetsTab.IsEnabled) CreatePetsTab.IsEnabled = true;
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

    private void BackToNewClientFormButton_Click(object? sender, RoutedEventArgs? e)
    {
        CreateClientTabControl.SelectedIndex = 0;
    }

    private void SaveNewClientButton_Click(object? sender, RoutedEventArgs? e)
    {
        if (_newClient == null) return;
        var result = DataServices.CreateClient(
            _newClient.Nombre,
            _newClient.NumIdentidad,
            _newClient.Telefono,
            _newClient.Correo,
            _newClient.Direccion,
            _newClient.TelefonoAdicional);

        var clienteId = (int)result.Rows[0]["ClienteID"];

        if (!_newPets.IsNullOrEmpty())
        {
            foreach (var pet in _newPets!)
            {
                DataServices.CreatePet(
                    pet.Nombre,
                    pet.Especie,
                    pet.Raza,
                    pet.Peso,
                    pet.Edad,
                    pet.Color,
                    pet.Descripcion,
                    clienteId);
            }
        }

        LoadClients();

        _currentState = ViewState.NoSelection;
        UpdateViewState();
    }

    private void AddNewPetButton_Click(object? sender, RoutedEventArgs? e)
    {
        if (NombreClienteTextBox == null || EspecieMascotaNuevaTextBox == null || RazaMascotaNuevaTextBox == null ||
            ColorMascotaNuevaTextBox == null ||
            PesoMascotaNuevaNumeric == null || EdadMascotaNuevaNumeric == null ||
            DescripcionMascotaNuevaTextBox == null) return;

        _newPets?.Add(new Pet
        {
            Nombre = NombreMascotaNuevaTextBox.Text,
            Especie = EspecieMascotaNuevaTextBox.Text,
            Raza = RazaMascotaNuevaTextBox.Text,
            Color = ColorMascotaNuevaTextBox.Text,
            Peso = PesoMascotaNuevaNumeric.Value,
            Edad = (int)EdadMascotaNuevaNumeric.Value!,
            Descripcion = DescripcionMascotaNuevaTextBox.Text
        });

        NombreMascotaNuevaTextBox.Text = string.Empty;
        EspecieMascotaNuevaTextBox.Text = string.Empty;
        RazaMascotaNuevaTextBox.Text = string.Empty;
        ColorMascotaNuevaTextBox.Text = string.Empty;
        PesoMascotaNuevaNumeric.Value = null;
        EdadMascotaNuevaNumeric.Value = null;
        DescripcionMascotaNuevaTextBox.Text = string.Empty;

        NewPetsDataGrid.ItemsSource = _newPets;
    }

    private void EditClientButton_Click(object? sender, RoutedEventArgs e)
    {
        if (ClientsDataGrid.SelectedItem is Client selectedClient)
        {
            _originalClientData = new Client
            {
                ClienteId = selectedClient.ClienteId,
                Nombre = selectedClient.Nombre,
                NumIdentidad = selectedClient.NumIdentidad,
                Telefono = selectedClient.Telefono,
                Correo = selectedClient.Correo,
                Direccion = selectedClient.Direccion,
                TelefonoAdicional = selectedClient.TelefonoAdicional,
                Activo = selectedClient.Activo
            };
        }

        NombreClienteTextBox.IsReadOnly = false;
        NumIdentidadClienteTextBox.IsReadOnly = false;
        TelefonoClienteTextBox.IsReadOnly = false;
        CorreoClienteTextBox.IsReadOnly = false;
        DireccionClienteTextBox.IsReadOnly = false;
        TelefonoAdicionalClienteTextBox.IsReadOnly = false;

        EditClientButton.IsVisible = false;
        DeleteClientButton.IsVisible = false;
        SaveClientButton.IsVisible = true;
        CancelEditButton.IsVisible = true;
    }

    private void DeleteClientButton_Click(object? sender, RoutedEventArgs e)
    {
        if (ClientsDataGrid.SelectedItem is not Client selectedClient) return;
        DataServices.DeleteClient(selectedClient.ClienteId);
        LoadClients();
        ClientsDataGrid.SelectedItem = null;
        _currentState = ViewState.NoSelection;
        UpdateViewState();
    }

    private void CancelEditButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_originalClientData != null)
        {
            NombreClienteTextBox.Text = _originalClientData.Nombre;
            NumIdentidadClienteTextBox.Text = _originalClientData.NumIdentidad;
            TelefonoClienteTextBox.Text = _originalClientData.Telefono;
            CorreoClienteTextBox.Text = _originalClientData.Correo ?? "";
            DireccionClienteTextBox.Text = _originalClientData.Direccion ?? "";
            TelefonoAdicionalClienteTextBox.Text = _originalClientData.TelefonoAdicional ?? "";
        }

        SetClientDetailsReadOnly(true);

        EditClientButton.IsVisible = true;
        DeleteClientButton.IsVisible = true;
        SaveClientButton.IsVisible = false;
        CancelEditButton.IsVisible = false;
    }

    private void SaveClientButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_originalClientData == null) return;

        var clienteId = _originalClientData.ClienteId ?? 0;

        DataServices.UpdateClient(
            _originalClientData.ClienteId,
            NombreClienteTextBox.Text,
            NumIdentidadClienteTextBox.Text,
            TelefonoClienteTextBox.Text,
            CorreoClienteTextBox.Text,
            DireccionClienteTextBox.Text,
            TelefonoAdicionalClienteTextBox.Text
        );

        SetClientDetailsReadOnly(true);

        EditClientButton.IsVisible = true;
        DeleteClientButton.IsVisible = true;
        SaveClientButton.IsVisible = false;
        CancelEditButton.IsVisible = false;

        LoadClients();
        Dispatcher.UIThread.Post(() =>
        {
            if (clienteId <= 0 || _clients == null) return;
            var clientToSelect = _clients.FirstOrDefault(c => c.ClienteId == clienteId);
            if (clientToSelect != null)
            {
                ClientsDataGrid.SelectedItem = clientToSelect;
            }
        });
    }

    private void SetClientDetailsReadOnly(bool isReadOnly)
    {
        NombreClienteTextBox.IsReadOnly = isReadOnly;
        NumIdentidadClienteTextBox.IsReadOnly = isReadOnly;
        TelefonoClienteTextBox.IsReadOnly = isReadOnly;
        CorreoClienteTextBox.IsReadOnly = isReadOnly;
        DireccionClienteTextBox.IsReadOnly = isReadOnly;
        TelefonoAdicionalClienteTextBox.IsReadOnly = isReadOnly;
    }

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