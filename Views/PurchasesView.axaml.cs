using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using ProyectoBD2.Models;
using ProyectoBD2.Services;
using Avalonia.Media;
using Avalonia.Layout;

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
        SetupDataGridEvents();
        
        SearchPurchasesTextBox.TextChanged += (s, e) => FilterPurchases(SearchPurchasesTextBox.Text!);

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

        InitializeButtons();

        CreateButton.Click += (s, e) => CreatePurchase();
        SaveButton.Click += (s, e) => SavePurchase();
        // DeletePurchaseButton.Click += (s, e) => DeletePurchase();
        AddArticleButton.Click += (s, e) => AddArticleToGrid();

        // Initial state
        SetViewMode(ViewMode.Viewing);
    }

    private void InitializeButtons()
    {
        CancelButton.Click += (s, e) =>
        {
            if (_currentMode == ViewMode.Editing && PurchasesHistoryDataGrid.SelectedItem is Purchase selectedPurchase)
            {
                LoadPurchaseDetails(selectedPurchase);
            }
            else
            {
                _purchaseItems?.Clear();
            }

            SetViewMode(ViewMode.Viewing);
        };
    }
    
    private void FilterPurchases(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            PurchasesHistoryDataGrid.ItemsSource = _purchases;
            return;
        }

        var filteredPurchases = _purchases?
            .Where(p => 
                (p.CodigoCompra?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (p.Proveedor?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (p.Total?.ToString()?.Contains(searchText) ?? false) ||
                (p.Estado?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (p.Fecha?.ToString("dd/MM/yyyy")?.Contains(searchText) ?? false))
            .ToList();

        PurchasesHistoryDataGrid.ItemsSource = filteredPurchases;
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
                Estado = row["Estado"] == DBNull.Value ? null : (string)row["Estado"],
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

        ViewPanel.IsVisible = mode == ViewMode.Viewing;
        EditCreatePanel.IsVisible = mode != ViewMode.Viewing;
        EditCreateTitle.Text = mode == ViewMode.Creating ? "Nueva Compra" : "Editar Compra";

        if (mode == ViewMode.Creating)
        {
            PurchasesHistoryDataGrid.SelectedItem = null;
            _purchaseItems?.Clear();
            SupplierComboBox.SelectedItem = null;
            PurchaseDatePicker.SelectedDate = DateTime.Now;
            StatusComboBox.SelectedIndex = 0;
        }

        if (mode == ViewMode.Editing && PurchasesHistoryDataGrid.SelectedItem is Purchase selectedPurchase)
        {
            LoadPurchaseDetails(selectedPurchase);
        }

        SetupPurchaseItemsDataGrid();
        PurchaseItemsDataGrid.ItemsSource = null;
        PurchaseItemsDataGrid.ItemsSource = _purchaseItems;

        CreateButton.IsVisible = mode == ViewMode.Creating;
        SaveButton.IsVisible = mode == ViewMode.Editing;
        CancelButton.IsVisible = mode != ViewMode.Viewing;
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
            Binding = new Binding("Cantidad"),
        };
        PurchaseItemsDataGrid.Columns.Add(cantidadColumn);

        var precioColumn = new DataGridTextColumn
        {
            Header = "Precio",
            Binding = new Binding("Precio"),
        };
        PurchaseItemsDataGrid.Columns.Add(precioColumn);

        var descuentoColumn = new DataGridTextColumn
        {
            Header = "Descuento",
            Binding = new Binding("Descuento"),
        };
        PurchaseItemsDataGrid.Columns.Add(descuentoColumn);

        var impuestoColumn = new DataGridTextColumn
        {
            Header = "Impuesto",
            Binding = new Binding("Impuesto"),
        };
        PurchaseItemsDataGrid.Columns.Add(impuestoColumn);

        var deleteColumn = new DataGridTemplateColumn
        {
            Header = "Eliminar",
            CellTemplate = new FuncDataTemplate<PurchaseItem>((purchaseItem, _) =>
            {
                var pathData = Geometry.Parse(
                    "M24,7.25 C27.1017853,7.25 29.629937,9.70601719 29.7458479,12.7794443 L29.75,13 L37,13 C37.6903559,13 38.25,13.5596441 38.25,14.25 C38.25,14.8972087 37.7581253,15.4295339 37.1278052,15.4935464 L37,15.5 L35.909,15.5 L34.2058308,38.0698451 C34.0385226,40.2866784 32.1910211,42 29.9678833,42 L18.0321167,42 C15.8089789,42 13.9614774,40.2866784 13.7941692,38.0698451 L12.09,15.5 L11,15.5 C10.3527913,15.5 9.8204661,15.0081253 9.75645361,14.3778052 L9.75,14.25 C9.75,13.6027913 10.2418747,13.0704661 10.8721948,13.0064536 L11,13 L18.25,13 C18.25,9.82436269 20.8243627,7.25 24,7.25 Z M33.4021054,15.5 L14.5978946,15.5 L16.2870795,37.8817009 C16.3559711,38.7945146 17.116707,39.5 18.0321167,39.5 L29.9678833,39.5 C30.883293,39.5 31.6440289,38.7945146 31.7129205,37.8817009 L33.4021054,15.5 Z M27.25,20.75 C27.8972087,20.75 28.4295339,21.2418747 28.4935464,21.8721948 L28.5,22 L28.5,33 C28.5,33.6903559 27.9403559,34.25 27.25,34.25 C26.6027913,34.25 26.0704661,33.7581253 26.0064536,33.1278052 L26,33 L26,22 C26,21.3096441 26.5596441,20.75 27.25,20.75 Z M20.75,20.75 C21.3972087,20.75 21.9295339,21.2418747 21.9935464,21.8721948 L22,22 L22,33 C22,33.6903559 21.4403559,34.25 20.75,34.25 C20.1027913,34.25 19.5704661,33.7581253 19.5064536,33.1278052 L19.5,33 L19.5,22 C19.5,21.3096441 20.0596441,20.75 20.75,20.75 Z M24,9.75 C22.2669685,9.75 20.8507541,11.1064548 20.7551448,12.8155761 L20.75,13 L27.25,13 C27.25,11.2050746 25.7949254,9.75 24,9.75 Z");

                var path = new Path
                {
                    Data = pathData,
                    Fill = new SolidColorBrush(Colors.White),
                    Width = 16,
                    Height = 16,
                    Stretch = Stretch.Uniform
                };

                var button = new Button
                {
                    Content = path,
                    Background = new SolidColorBrush(Colors.Red),
                    Foreground = new SolidColorBrush(Colors.White),
                    IsEnabled = _currentMode != ViewMode.Viewing,
                    Width = 40,
                    Height = 30,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(2)
                };

                button.Click += (s, e) =>
                {
                    if (_currentMode == ViewMode.Viewing) return;
                    _purchaseItems?.Remove(purchaseItem);
                    PurchaseItemsDataGrid.ItemsSource = _purchaseItems;
                };

                return button;
            })
        };

        PurchaseItemsDataGrid.Columns.Add(deleteColumn);
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

        _purchases?.Clear();
        LoadPurchaseHistory();

        _purchaseItems?.Clear();

        SupplierComboBox.SelectedItem = null;
        PurchaseDatePicker.SelectedDate = DateTime.Now;
        StatusComboBox.SelectedIndex = 0;
        ArticleAutoCompleteBox.SelectedItem = null;
        ArticleAutoCompleteBox.Text = string.Empty;

        SetViewMode(ViewMode.Viewing);
    }

    private void LoadPurchaseDetails(Purchase purchase)
    {
        if (purchase.CompraId == null) return;

        var data = DataServices.FindPurchaseDetails(purchase.CompraId);

        _purchaseItems?.Clear();

        foreach (DataRow row in data.Rows)
        {
            _purchaseItems?.Add(new PurchaseItem
            {
                ArticuloId = row["ArticuloID"] == DBNull.Value ? null : (int?)row["ArticuloID"],
                NombreArticulo = row["NombreArticulo"] == DBNull.Value ? null : (string)row["NombreArticulo"],
                Cantidad = row["Cantidad"] == DBNull.Value ? null : (int?)row["Cantidad"],
                Precio = row["Precio"] == DBNull.Value ? null : (decimal?)row["Precio"],
                Descuento = row["Descuento"] == DBNull.Value ? null : (decimal?)row["Descuento"],
                Impuesto = row["Impuesto"] == DBNull.Value ? null : (decimal?)row["Impuesto"],
            });
        }

        var provider = _providers?.FirstOrDefault(p => p.Nombre == purchase.Proveedor);
        if (provider != null)
        {
            SupplierComboBox.SelectedItem = provider;
        }

        PurchaseDatePicker.SelectedDate = purchase.Fecha;
        StatusComboBox.SelectedItem = purchase.Estado;
    }

    private void SavePurchase()
    {
        if (SupplierComboBox.SelectedItem is not Provider selectedProvider ||
            PurchasesHistoryDataGrid.SelectedItem is not Purchase selectedPurchase)
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
            row["ArticuloID"] = item.ArticuloId ?? 0;
            row["NombreArticulo"] = item.NombreArticulo ?? string.Empty;
            row["Cantidad"] = item.Cantidad ?? 0;
            row["Precio"] = item.Precio ?? 0;
            row["Descuento"] = item.Descuento ?? 0;
            row["Impuesto"] = item.Impuesto ?? 0;
            detailsTable.Rows.Add(row);
        }

        if (PurchaseDatePicker.SelectedDate == null) return;
        var selectedStatus = StatusComboBox.SelectedItem as string;

        DataServices.UpdatePurchase(
            selectedPurchase.CompraId,
            selectedProvider.ProveedorId,
            PurchaseDatePicker.SelectedDate,
            selectedStatus,
            detailsTable);

        _purchases?.Clear();
        LoadPurchaseHistory();

        SetViewMode(ViewMode.Viewing);
    }

    private void SetupDataGridEvents()
    {
        PurchasesHistoryDataGrid.SelectionChanged += (s, e) =>
        {
            if (PurchasesHistoryDataGrid.SelectedItem is Purchase selectedPurchase &&
                _currentMode == ViewMode.Viewing)
            {
                LoadPurchaseDetails(selectedPurchase);
            }
        };
    }
}