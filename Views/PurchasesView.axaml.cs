using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.IdentityModel.Tokens;
using ProyectoBD2.Models;
using ProyectoBD2.Services;

namespace ProyectoBD2.Views;

public partial class PurchasesView : UserControl
{
    private readonly ObservableCollection<Provider>? _providers = [];
    private readonly ObservableCollection<Article>? _articles = [];
    // private readonly ObservableCollection<Purchases>? _purchases = [];
    
    private enum ViewMode
    {
        Viewing,
        Creating,
        Editing
    }
    
    private ViewMode _currentMode = ViewMode.Viewing;

    public PurchasesView()
    {
        InitializeComponent();
        LoadProviders();
        LoadArticles();
        
        // PurchaseItemsDataGrid.ItemsSource = _purchaseItems;
        ArticleComboBox.ItemsSource = _articles;
        SupplierComboBox.ItemsSource = _providers;
        SupplierComboBox.DisplayMemberBinding = new Binding("Nombre");
        ArticleComboBox.DisplayMemberBinding = new Binding("Nombre");
        PurchaseDatePicker.SelectedDate = DateTime.Now;
    
        // Set up event handlers
        NewPurchaseButton.Click += (s, e) => SetViewMode(ViewMode.Creating);
        EditPurchaseButton.Click += (s, e) => SetViewMode(ViewMode.Editing);
        CancelButton.Click += (s, e) => SetViewMode(ViewMode.Viewing);
        // CreateButton.Click += (s, e) => CreatePurchase();
        // SaveButton.Click += (s, e) => SavePurchase();
        // DeletePurchaseButton.Click += (s, e) => DeletePurchase();
        AddArticleButton.Click += (s, e) => AddArticleToGrid();
    
        // Initial state
        SetViewMode(ViewMode.Viewing);
    }
    
    private void LoadProviders()
    {
        var data = DataServices.FindProviders();

        foreach (DataRow row in data.Rows)
        {
            _providers?.Add(new Provider
            {
                ProveedorId = row["ProveedorID"] == DBNull.Value ? null : (int?)row["ProveedorID"],
                Nombre = row["Nombre"] == DBNull.Value ? null : (string)row["Nombre"],
            });
        }
        
        SupplierComboBox.ItemsSource = _providers;
    }

    private void LoadArticles()
    {
        var data = DataServices.FindArticles();

        foreach (DataRow row in data.Rows)
        {
            _articles?.Add(new Article
            {
                ArticuloId = row["ArticuloID"] == DBNull.Value ? null : (int?)row["ArticuloID"],
                Nombre = row["Nombre"] == DBNull.Value ? null : (string)row["Nombre"],
            });
        }
        
        ArticleComboBox.ItemsSource = _articles;
    }
    
    private void SetViewMode(ViewMode mode)
    {
        _currentMode = mode;
    
        // Update UI based on mode
        ViewPanel.IsVisible = mode == ViewMode.Viewing;
        EditCreatePanel.IsVisible = mode != ViewMode.Viewing;
    
        // Set title
        EditCreateTitle.Text = mode == ViewMode.Creating ? "Nueva Compra" : "Editar Compra";
    
        // Set button visibility
        CreateButton.IsVisible = mode == ViewMode.Creating;
        SaveButton.IsVisible = mode == ViewMode.Editing;
        CancelButton.IsVisible = mode != ViewMode.Viewing;
        StatusComboBox.IsVisible = mode == ViewMode.Editing;
    
        // Reset form if creating
        if (mode == ViewMode.Creating)
        {
            // _purchaseItems.Clear();
            SupplierComboBox.SelectedItem = null;
            PurchaseDatePicker.SelectedDate = DateTime.Now;
        }
    
        // Load data if editing
        // if (mode == ViewMode.Editing && PurchasesHistoryDataGrid.SelectedItem is Purchase selectedPurchase)
        // {
        //     // _currentPurchase = selectedPurchase;
        //     // Load purchase details
        //     // LoadPurchaseDetails(selectedPurchase);
        // }
    }

    private void AddArticleToGrid()
    {
        if (ArticleComboBox.SelectedItem is Article selectedArticle)
        {
            // Add logic to add item to grid with default values
            // _purchaseItems.Add(new PurchaseItem
            // {
            //     ArticuloId = selectedArticle.ArticuloId,
            //     ArticuloNombre = selectedArticle.Nombre,
            //     Cantidad = 1,
            //     PrecioUnitario = 0,
            //     Descuento = 0,
            //     Impuesto = 0
            // });
        }
    }
}