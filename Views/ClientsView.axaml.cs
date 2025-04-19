using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.ObjectModel;

namespace ProyectoBD2.Views;

public partial class ClientsView : UserControl
{
    private enum ViewState
    {
        NoSelection,
        ViewingClient,
        CreatingClient
    }
    
    private ViewState _currentState = ViewState.NoSelection;
    private ObservableCollection<string> _newPets = new ObservableCollection<string>();
    
    public ClientsView()
    {
        InitializeComponent();
        
        ClientsDataGrid.SelectionChanged += ClientsDataGrid_SelectionChanged;
        AddClientButton.Click += AddClientButton_Click;
        CancelNewClientButton.Click += CancelNewClientButton_Click;
        ContinueToMascotasButton.Click += ContinueToMascotasButton_Click;
        BackToClientDetailsButton.Click += BackToClientDetailsButton_Click;
        SaveNewClientButton.Click += SaveNewClientButton_Click;
        AddNewPetButton.Click += AddNewPetButton_Click;
        
        NewPetsListBox.ItemsSource = _newPets;
        UpdateViewState();
    }
    
    private void ClientsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ClientsDataGrid.SelectedItem == null) return;
        _currentState = ViewState.ViewingClient;
        UpdateViewState();
    }
    
    private void AddClientButton_Click(object sender, RoutedEventArgs e)
    {
        _currentState = ViewState.CreatingClient;
        UpdateViewState();
        
        NewClientIdTextBox.Text = string.Empty;
        NewClientNameTextBox.Text = string.Empty;
        NewClientPhoneTextBox.Text = string.Empty;
        NewClientEmailTextBox.Text = string.Empty;
        NewClientAddressTextBox.Text = string.Empty;
        
        _newPets.Clear();
        
        CreateClientTabControl.SelectedIndex = 0;
        CreateMascotasTab.IsEnabled = false;
    }
    
    private void CancelNewClientButton_Click(object sender, RoutedEventArgs e)
    {
        _currentState = ClientsDataGrid.SelectedItem != null ? ViewState.ViewingClient : ViewState.NoSelection;
        UpdateViewState();
    }
    
    private void ContinueToMascotasButton_Click(object sender, RoutedEventArgs e)
    {
        CreateMascotasTab.IsEnabled = true;
        CreateClientTabControl.SelectedIndex = 1;
    }
    
    private void BackToClientDetailsButton_Click(object sender, RoutedEventArgs e)
    {
        CreateClientTabControl.SelectedIndex = 0;
    }
    
    private void SaveNewClientButton_Click(object sender, RoutedEventArgs e)
    {
        _currentState = ViewState.NoSelection;
        UpdateViewState();
    }
    
    private void AddNewPetButton_Click(object sender, RoutedEventArgs e)
    {
        var petName = NewPetNameTextBox.Text?.Trim() ?? string.Empty;
        var petSpecies = (NewPetSpeciesComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "No especificado";
        var petBreed = NewPetBreedTextBox.Text?.Trim() ?? string.Empty;
        var petAge = NewPetAgeNumeric.Value;

        if (string.IsNullOrWhiteSpace(petName)) return;
        var petDisplay = $"{petName} - {petSpecies} - {petBreed} - {petAge} años";
        _newPets.Add(petDisplay);
            
        NewPetNameTextBox.Text = string.Empty;
        NewPetSpeciesComboBox.SelectedIndex = -1;
        NewPetBreedTextBox.Text = string.Empty;
        NewPetAgeNumeric.Value = 0;
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