using System;
using System.Collections.ObjectModel;
using System.Data;
using Avalonia.Controls;
using Avalonia.Data;
using ProyectoBD2.Models;
using ProyectoBD2.Services;

namespace ProyectoBD2.Views;

public partial class PurchasesView : UserControl
{
    private readonly ObservableCollection<Provider>? _providers = [];
    private readonly ObservableCollection<Article>? _articles = [];
    private readonly ObservableCollection<PurchaseItem>? _purchaseItems = [];
    private readonly ObservableCollection<string> _statuses =
    [
        "Pendiente",
        "Finalizada"
    ];
    private readonly ObservableCollection<Purchase>? _purchases = [];
    
    private Article? _selectedArticle = new Article();
    
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
        LoadPurchaseHistory();
        
        SetupPurchaseItemsDataGrid();
        
        PurchaseItemsDataGrid.ItemsSource = _purchaseItems;
        ArticleAutoCompleteBox.ItemsSource = _articles;
        ArticleAutoCompleteBox.ValueMemberBinding = new Binding("Nombre");
        SupplierComboBox.ItemsSource = _providers;
        SupplierComboBox.DisplayMemberBinding = new Binding("Nombre");
        PurchaseDatePicker.SelectedDate = DateTime.Now;
        StatusComboBox.ItemsSource = _statuses;
        StatusComboBox.SelectedIndex = 0;

        NewPurchaseButton.Click += (s, e) => SetViewMode(ViewMode.Creating);
        EditPurchaseButton.Click += (s, e) => SetViewMode(ViewMode.Editing);
        CancelButton.Click += (s, e) => SetViewMode(ViewMode.Viewing);
        
        CreateButton.Click += (s, e) => CreatePurchase();
        // SaveButton.Click += (s, e) => SavePurchase();
        // DeletePurchaseButton.Click += (s, e) => DeletePurchase();
        AddArticleButton.Click += (s, e) => AddArticleToGrid();
    
        // Initial state
        SetViewMode(ViewMode.Viewing);
    }

    private void LoadPurchaseHistory()
    {
        var data = DataServices.FindPurchasesHistory();

        foreach (DataRow row in data.Rows)
        {
            _purchases?.Add(new Purchase
            {
                CompraId = row["CompraID"] == DBNull.Value ? null : (int?)row["CompraID"],
                CodigoCompra = row["CodigoCompra"] == DBNull.Value ? null : (string)row["CodigoCompra"],
                Proveedor = row["Proveedor"] == DBNull.Value ? null : (string)row["Proveedor"],
                Fecha = row["Fecha"] == DBNull.Value ? null : (DateTime?)row["Fecha"],
                Total = row["Total"] == DBNull.Value ? null : (decimal?)row["Total"],
            });
        }
        
        PurchasesHistoryDataGrid.ItemsSource = _purchases;
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
        
        ArticleAutoCompleteBox.ItemsSource = _articles;
    }
    
    private void SetViewMode(ViewMode mode)
    {
        _currentMode = mode;
    
        // Update UI based on mode
        ViewPanel.IsVisible = mode == ViewMode.Viewing;
        EditCreatePanel.IsVisible = mode != ViewMode.Viewing;
    
        // Set title
        EditCreateTitle.Text = mode == ViewMode.Creating ? "Nueva Compra" : "Editar Compra";
        
        PurchasesHistoryDataGrid.SelectedItem = mode == ViewMode.Creating ? null : PurchasesHistoryDataGrid.SelectedItem;
    
        // Set button visibility
        CreateButton.IsVisible = mode == ViewMode.Creating;
        SaveButton.IsVisible = mode == ViewMode.Editing;
        CancelButton.IsVisible = mode != ViewMode.Viewing;
    
        // Reset form if creating
        if (mode != ViewMode.Creating) return;
        // _purchaseItems.Clear();
        SupplierComboBox.SelectedItem = null;
        PurchaseDatePicker.SelectedDate = DateTime.Now;

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
        if (ArticleAutoCompleteBox.SelectedItem is not Article selectedArticle) return;
        _selectedArticle = selectedArticle;
        _purchaseItems?.Add(new PurchaseItem
        {
            ArticuloId = _selectedArticle?.ArticuloId,
            NombreArticulo = _selectedArticle?.Nombre,
        });
        
        PurchaseItemsDataGrid.ItemsSource = _purchaseItems;
    }
    
    private void SetupPurchaseItemsDataGrid()
    {
        PurchaseItemsDataGrid.Columns.Clear();
    
        var articuloIdColumn = new DataGridTextColumn
        {
            Header = "ArticuloID",
            Binding = new Binding("ArticuloId"),
            IsReadOnly = true
        };
        PurchaseItemsDataGrid.Columns.Add(articuloIdColumn);
    
        var nombreArticuloColumn = new DataGridTextColumn
        {
            Header = "NombreArticulo",
            Binding = new Binding("NombreArticulo"),
            IsReadOnly = true
        };
        PurchaseItemsDataGrid.Columns.Add(nombreArticuloColumn);
    
        var cantidadColumn = new DataGridTextColumn
        {
            Header = "Cantidad",
            Binding = new Binding("Cantidad")
        };
        PurchaseItemsDataGrid.Columns.Add(cantidadColumn);
    
        var precioColumn = new DataGridTextColumn
        {
            Header = "Precio",
            Binding = new Binding("Precio")
        };
        PurchaseItemsDataGrid.Columns.Add(precioColumn);
    
        var descuentoColumn = new DataGridTextColumn
        {
            Header = "Descuento",
            Binding = new Binding("Descuento")
        };
        PurchaseItemsDataGrid.Columns.Add(descuentoColumn);
    
        var impuestoColumn = new DataGridTextColumn
        {
            Header = "Impuesto",
            Binding = new Binding("Impuesto")
        };
        PurchaseItemsDataGrid.Columns.Add(impuestoColumn);
    }

    private void CreatePurchase()
    {
        if (SupplierComboBox.SelectedItem is not Provider selectedProvider)
        {
            return;
        }
        
        var detailsTable = new DataTable();
        
        detailsTable.Columns.Add("ArticuloID", typeof(int));
        detailsTable.Columns.Add("NombreArticulo", typeof(string));
        detailsTable.Columns.Add("Cantidad", typeof(int));
        detailsTable.Columns.Add("Precio", typeof(decimal));
        detailsTable.Columns.Add("Descuento", typeof(decimal));
        detailsTable.Columns.Add("Impuesto", typeof(decimal));
        
        foreach (var item in _purchaseItems!)
        {
            var row = detailsTable.NewRow();
            row["ArticuloID"] = item.ArticuloId;
            row["NombreArticulo"] = item.NombreArticulo;
            row["Cantidad"] = item.Cantidad;
            row["Precio"] = item.Precio;
            row["Descuento"] = item.Descuento;
            row["Impuesto"] = item.Impuesto;
            detailsTable.Rows.Add(row);
        }

        if (PurchaseDatePicker.SelectedDate == null) return;
        var data = DataServices.CreatePurchase(selectedProvider.ProveedorId,
            PurchaseDatePicker.SelectedDate, detailsTable);
        Console.Write(data);
    }
}