using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ProyectoBD2.Models;
using ProyectoBD2.Services;

namespace ProyectoBD2.Views;

public partial class StockView : UserControl
{
    private ObservableCollection<Stock>? _stockItems;
    private List<string> _clasificaciones = new List<string> { "Todos", "Venta", "InsumoServicio", "VentaYServicio" };
    private Dictionary<string, string> _clasificacionesMap = new Dictionary<string, string>
    {
        { "Todos", "" },
        { "Venta", "Venta" },
        { "InsumoServicio", "Insumo para servicio" },
        { "VentaYServicio", "Venta e insumo para servicio" }
    };

    public StockView()
    {
        InitializeComponent();

        // Configurar el ComboBox con las opciones de clasificación
        ClasificacionComboBox.ItemsSource = _clasificaciones;
        ClasificacionComboBox.SelectedIndex = 0;  // Seleccionar "Todos" por defecto
        
        // Configurar el evento para personalizar las columnas del DataGrid
        StockDataGrid.AutoGeneratingColumn += StockDataGrid_AutoGeneratingColumn;
        
        // Configurar eventos
        BuscarArticuloTextBox.PropertyChanged += (sender, args) =>
        {
            if (args.Property == TextBox.TextProperty)
            {
                FilterData();
            }
        };

        ClasificacionComboBox.SelectionChanged += (sender, args) =>
        {
            FilterData();
        };

        EstadoCriticoCheckBox.PropertyChanged += (sender, args) =>
        {
            if (args.Property == CheckBox.IsCheckedProperty)
            {
                FilterData();
            }
        };

        ActualizarButton.Click += (sender, args) =>
        {
            CargarDatos();
        };

        // Cargar datos iniciales
        CargarDatos();
    }
    
    private void StockDataGrid_AutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Stock.NombreProveedor):
                e.Column.Header = "Nombre de Proveedor";
                break;
            case nameof(Stock.Clasificacion):
                e.Column.Header = "Clasificación";
                break;
            case nameof(Stock.ExistenciaActual):
                e.Column.Header = "Existencia Actual";
                break;
            case nameof(Stock.ExistenciaMinima):
                e.Column.Header = "Existencia Mínima";
                break;
            case nameof(Stock.Estado):
                e.Column.Header = "Estado";
                break;
        }
    }

    private void CargarDatos()
    {
        try
        {
            _stockItems = new ObservableCollection<Stock>();
            var data = OtrosServicios.GetNivelesStock();
            
            Console.WriteLine($"Filas devueltas: {data.Rows.Count}");
            
            foreach (DataRow row in data.Rows)
            {
                // Imprimir los nombres de las columnas para depuración
                if (_stockItems.Count == 0)
                {
                    foreach (DataColumn column in data.Columns)
                    {
                        Console.WriteLine($"Columna: {column.ColumnName}");
                    }
                }
                
                var stock = new Stock
                {
                    NombreProveedor = row["Nombre"]?.ToString(),
                    Clasificacion = row["ClasificacionUsoArticulo"]?.ToString(),
                    ExistenciaActual = row["ExistenciaActual"] == DBNull.Value ? null : Convert.ToDecimal(row["ExistenciaActual"]),
                    ExistenciaMinima = row["ExistenciaMinima"] == DBNull.Value ? null : Convert.ToDecimal(row["ExistenciaMinima"]),
                    Estado = row["Estado"]?.ToString()
                };
                
                _stockItems.Add(stock);
            }

            // Asignar directamente al DataGrid y aplicar filtros
            FilterData();
            
            Console.WriteLine($"Datos cargados: {_stockItems.Count} registros");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar datos de stock: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
        }
    }

    private void FilterData()
    {
        if (_stockItems == null) return;

        var filteredCollection = new List<Stock>(_stockItems);

        // Filtrar por nombre si hay texto en la búsqueda
        var searchText = BuscarArticuloTextBox.Text?.Trim().ToLower();
        if (!string.IsNullOrEmpty(searchText))
        {
            filteredCollection = filteredCollection.Where(s => 
                s.NombreProveedor?.ToLower().Contains(searchText) == true).ToList();
        }

        // Filtrar por clasificación si no es "Todos"
        var selectedClasificacion = ClasificacionComboBox.SelectedItem?.ToString();
        if (!string.IsNullOrEmpty(selectedClasificacion) && selectedClasificacion != "Todos")
        {
            // Usar el mapa para convertir el valor mostrado al valor real en los datos
            var clasificacionFilter = _clasificacionesMap[selectedClasificacion];
            filteredCollection = filteredCollection.Where(s => 
                s.Clasificacion == clasificacionFilter).ToList();
        }

        // Filtrar por estado crítico si el checkbox está marcado
        if (EstadoCriticoCheckBox.IsChecked == true)
        {
            filteredCollection = filteredCollection.Where(s => 
                s.Estado == "Sin stock").ToList();
        }

        // Actualizar el DataGrid
        StockDataGrid.ItemsSource = new ObservableCollection<Stock>(filteredCollection);
        
        Console.WriteLine($"Filtro aplicado: {filteredCollection.Count} registros mostrados");
    }

    // Método para refrescar los datos
    public void RefrescarDatos()
    {
        CargarDatos();
    }
}