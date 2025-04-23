using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using ProyectoBD2.Models;
using ProyectoBD2.Services;

namespace ProyectoBD2.Views;

public partial class ArticulosView : UserControl
{
    private ObservableCollection<Article>? _articulos;
    private Article? _originalArticuloData;
    private Article? _newArticulo;

    // Lista de clasificaciones disponibles
    private readonly List<string> _clasificaciones = new List<string>
    {
        "Insumo para servicio",
        "Venta",
        "Venta e insumo para servicio"
    };

    private enum ViewState
    {
        NoSelection,
        ViewingArticulo,
        CreatingArticulo,
        EditingArticulo
    }

    private ViewState _currentState = ViewState.NoSelection;

    public ArticulosView()
    {
        InitializeComponent();

        // Configurar evento para botón de nuevo artículo
        NuevoArticuloButton.Click += NuevoArticuloButton_Click;

        // Configurar el evento para personalizar las columnas del DataGrid
        ArticulosDataGrid.AutoGeneratingColumn += ArticulosDataGridAutoGeneratingColumn;
        ArticulosDataGrid.SelectionChanged += ArticulosDataGrid_SelectionChanged;

        // Cargar datos de artículos al inicializar
        CargarArticulos();
    }

    private void CargarArticulos()
    {
        try
        {
            _articulos = new ObservableCollection<Article>();
            var data = OtrosServicios.AllArticulos();

            foreach (DataRow row in data.Rows)
            {
                _articulos.Add(new Article
                {
                    ArticuloId = row["ArticuloId"] == DBNull.Value ? null : (int?)row["ArticuloId"],
                    Nombre = row["Nombre"] == DBNull.Value ? null : (string)row["Nombre"]
                });
            }

            ArticulosDataGrid.ItemsSource = _articulos;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar artículos: {ex.Message}");
        }
    }

    private void ArticulosDataGridAutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        if (!(sender is DataGrid dataGrid))
            return;

        // Añadir columna de acciones al final
        if (sender is DataGrid grid && e.PropertyName == "Nombre")
        {
            Dispatcher.UIThread.Post(() =>
            {
                // Verificar si ya existe una columna de acciones
                var hasActionColumn = grid.Columns.Any(c =>
                    c is DataGridTemplateColumn column &&
                    column.Header?.ToString() == "Acciones");

                if (hasActionColumn) grid.Columns.Remove(grid.Columns.First(c => c.Header?.ToString() == "Acciones"));

                // Crear columna de acciones
                var actionColumn = new DataGridTemplateColumn
                {
                    Header = "Acciones",
                    Width = DataGridLength.Auto,
                    CellTemplate = new FuncDataTemplate<object>((item, _) =>
                    {
                        if (item is not Article articulo)
                            return new TextBlock { Text = "Error" };

                        // Crear panel para botones
                        var panel = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Spacing = 8,
                            Margin = new Thickness(5)
                        };

                        // Botón Editar
                        var editButton = new Button
                        {
                            Content = "Editar",
                            CommandParameter = articulo
                        };
                        editButton.Click += EditArticulo_Click;

                        // Botón Eliminar
                        var deleteButton = new Button
                        {
                            Content = "Eliminar",
                            CommandParameter = articulo,
                            Foreground = Brushes.White,
                            Background = new SolidColorBrush(Color.Parse("#dc3545"))
                        };
                        deleteButton.Click += DeleteArticulo_Click;

                        panel.Children.Add(editButton);
                        panel.Children.Add(deleteButton);

                        return panel;
                    })
                };

                dataGrid.Columns.Add(actionColumn);
            });
        }
    }

    private void ArticulosDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ArticulosDataGrid.SelectedItem is not Article selectedArticulo)
        {
            _currentState = ViewState.NoSelection;
            return;
        }

        _currentState = ViewState.ViewingArticulo;
    }

    private async void NuevoArticuloButton_Click(object? sender, RoutedEventArgs e)
    {
        // Crear formulario para nuevo artículo
        var formPanel = new StackPanel
        {
            Spacing = 10,
            Width = 400,
            Margin = new Thickness(20)
        };

        // Nombre
        var nombrePanel = new StackPanel();
        nombrePanel.Children.Add(new TextBlock { Text = "Nombre:" });
        var nombreTextBox = new TextBox();
        nombrePanel.Children.Add(nombreTextBox);
        formPanel.Children.Add(nombrePanel);

        // Existencia Mínima
        var existenciaPanel = new StackPanel();
        existenciaPanel.Children.Add(new TextBlock { Text = "Existencia Mínima:" });
        var existenciaTextBox = new TextBox { Text = "0" };
        existenciaPanel.Children.Add(existenciaTextBox);
        formPanel.Children.Add(existenciaPanel);

        // Clasificación (ComboBox)
        var clasificacionPanel = new StackPanel();
        clasificacionPanel.Children.Add(new TextBlock { Text = "Clasificación:" });
        var clasificacionComboBox = new ComboBox
        {
            ItemsSource = _clasificaciones,
            SelectedIndex = 0,
            Width = 300,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        clasificacionPanel.Children.Add(clasificacionComboBox);
        formPanel.Children.Add(clasificacionPanel);

        // Botones
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 20, 0, 0)
        };

        var guardarButton = new Button
        {
            Content = "Guardar",
            Background = new SolidColorBrush(Color.Parse("#28a745")),
            Foreground = Brushes.White
        };

        var cancelarButton = new Button
        {
            Content = "Cancelar"
        };

        buttonPanel.Children.Add(guardarButton);
        buttonPanel.Children.Add(cancelarButton);
        formPanel.Children.Add(buttonPanel);

        // Crear ventana de diálogo
        var dialogWindow = new Window
        {
            Title = "Nuevo Artículo",
            Width = 450,
            Height = 300,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = formPanel,
            SizeToContent = SizeToContent.Height
        };

        // Usar TaskCompletionSource para esperar la respuesta
        var tcs = new TaskCompletionSource<bool>();

        guardarButton.Click += (_, _) =>
        {
            tcs.SetResult(true);
            dialogWindow.Close();
        };

        cancelarButton.Click += (_, _) =>
        {
            tcs.SetResult(false);
            dialogWindow.Close();
        };

        // También manejar el cierre de la ventana (X)
        dialogWindow.Closed += (_, _) =>
        {
            if (!tcs.Task.IsCompleted)
                tcs.SetResult(false);
        };

        // Mostrar ventana modal
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
        {
            dialogWindow.ShowDialog(topLevel as Window);
        }
        else
        {
            await dialogWindow.ShowDialog(null);
        }

        // Esperar el resultado
        bool resultOk = await tcs.Task;

        if (resultOk)
        {
            try
            {
                // Obtener valores del formulario
                string nombre = nombreTextBox.Text?.Trim() ?? string.Empty;
                
                // Para la existencia mínima, usamos un valor por defecto si no se puede convertir
                int existenciaMinima = 0;
                int.TryParse(existenciaTextBox.Text, out existenciaMinima);

                string clasificacion = clasificacionComboBox.SelectedItem?.ToString() ?? _clasificaciones[0];

                // Validar campos obligatorios
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    await MostrarMensajeError("El nombre es obligatorio.");
                    return;
                }

                Console.WriteLine($"Creando artículo: {nombre}, {existenciaMinima}, {clasificacion}");

                // Crear artículo y obtener resultado
                var resultado = OtrosServicios.CrearArticulo(nombre, existenciaMinima, clasificacion);

                // Mostrar mensaje de éxito
                await MostrarMensaje("Éxito", "Artículo creado correctamente.");

                // Actualizar lista de artículos
                CargarArticulos();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear artículo: {ex.Message}");
                await MostrarMensajeError($"No se pudo crear el artículo: {ex.Message}");
            }
        }
    }

    private async void EditArticulo_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Article articulo)
        {
            // Verificar que el ID no sea nulo
            if (articulo.ArticuloId == null)
            {
                return;
            }

            // Obtener datos completos del artículo
            try
            {
                // Guardar datos originales para referencia
                _originalArticuloData = new Article
                {
                    ArticuloId = articulo.ArticuloId,
                    Nombre = articulo.Nombre
                    // Los otros campos no están disponibles en el grid
                };

                // Crear formulario para editar artículo
                var formPanel = new StackPanel
                {
                    Spacing = 10,
                    Width = 400,
                    Margin = new Thickness(20)
                };

                // Nombre
                var nombrePanel = new StackPanel();
                nombrePanel.Children.Add(new TextBlock { Text = "Nombre:" });
                var nombreTextBox = new TextBox { Text = articulo.Nombre ?? string.Empty };
                nombrePanel.Children.Add(nombreTextBox);
                formPanel.Children.Add(nombrePanel);

                // Existencia Mínima
                var existenciaPanel = new StackPanel();
                existenciaPanel.Children.Add(new TextBlock { Text = "Existencia Mínima:" });
                var existenciaTextBox = new TextBox { Text = "0" }; // Valor por defecto
                existenciaPanel.Children.Add(existenciaTextBox);
                formPanel.Children.Add(existenciaPanel);

                // Clasificación (ComboBox)
                var clasificacionPanel = new StackPanel();
                clasificacionPanel.Children.Add(new TextBlock { Text = "Clasificación:" });
                var clasificacionComboBox = new ComboBox
                {
                    ItemsSource = _clasificaciones,
                    SelectedIndex = 0,
                    Width = 300,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                clasificacionPanel.Children.Add(clasificacionComboBox);
                formPanel.Children.Add(clasificacionPanel);

                // Botones
                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 10,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 20, 0, 0)
                };

                var guardarButton = new Button
                {
                    Content = "Guardar",
                    Background = new SolidColorBrush(Color.Parse("#28a745")),
                    Foreground = Brushes.White
                };

                var cancelarButton = new Button
                {
                    Content = "Cancelar"
                };

                buttonPanel.Children.Add(guardarButton);
                buttonPanel.Children.Add(cancelarButton);
                formPanel.Children.Add(buttonPanel);

                // Crear ventana de diálogo
                var dialogWindow = new Window
                {
                    Title = "Editar Artículo",
                    Width = 450,
                    Height = 300,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Content = formPanel,
                    SizeToContent = SizeToContent.Height
                };

                // Usar TaskCompletionSource para esperar la respuesta
                var tcs = new TaskCompletionSource<bool>();

                guardarButton.Click += (_, _) =>
                {
                    tcs.SetResult(true);
                    dialogWindow.Close();
                };

                cancelarButton.Click += (_, _) =>
                {
                    tcs.SetResult(false);
                    dialogWindow.Close();
                };

                // También manejar el cierre de la ventana (X)
                dialogWindow.Closed += (_, _) =>
                {
                    if (!tcs.Task.IsCompleted)
                        tcs.SetResult(false);
                };

                // Mostrar ventana modal
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel != null)
                {
                    dialogWindow.ShowDialog(topLevel as Window);
                }
                else
                {
                    await dialogWindow.ShowDialog(null);
                }

                // Esperar el resultado
                bool resultOk = await tcs.Task;

                if (resultOk)
                {
                    try
                    {
                        // Obtener valores del formulario
                        string nombre = nombreTextBox.Text?.Trim() ?? string.Empty;
                        
                        // Para la existencia mínima, usamos un valor por defecto si no se puede convertir
                        int existenciaMinima = 0;
                        int.TryParse(existenciaTextBox.Text, out existenciaMinima);

                        string clasificacion = clasificacionComboBox.SelectedItem?.ToString() ?? _clasificaciones[0];

                        // Validar campos obligatorios
                        if (string.IsNullOrWhiteSpace(nombre))
                        {
                            await MostrarMensajeError("El nombre es obligatorio.");
                            return;
                        }

                        Console.WriteLine($"Editando artículo: ID={articulo.ArticuloId}, {nombre}, {existenciaMinima}, {clasificacion}");

                        // Editar artículo y obtener resultado
                        var resultado = OtrosServicios.EditarArticulo(articulo.ArticuloId, nombre, existenciaMinima, clasificacion);

                        // Mostrar mensaje de éxito
                        await MostrarMensaje("Éxito", "Artículo actualizado correctamente.");

                        // Actualizar lista de artículos
                        CargarArticulos();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al editar artículo: {ex.Message}");
                        await MostrarMensajeError($"No se pudo editar el artículo: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al preparar edición: {ex.Message}");
                await MostrarMensajeError($"No se pudo preparar la edición: {ex.Message}");
            }
        }
    }

    private async void DeleteArticulo_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Article articulo)
        {
            // Verificar que el ID no sea nulo
            if (articulo.ArticuloId == null)
            {
                return;
            }

            // Crear contenedor principal para el diálogo
            var panel = new StackPanel
            {
                Spacing = 15,
                Width = 300,
                Margin = new Thickness(20)
            };

            // Mensaje
            var messageText = new TextBlock
            {
                Text = $"¿Está seguro que desea eliminar el artículo {articulo.Nombre}?",
                TextWrapping = TextWrapping.Wrap
            };
            panel.Children.Add(messageText);

            // Botones
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };

            var eliminarButton = new Button
            {
                Content = "Eliminar",
                Background = new SolidColorBrush(Color.Parse("#dc3545")),
                Foreground = Brushes.White
            };

            var cancelarButton = new Button
            {
                Content = "Cancelar"
            };

            buttonPanel.Children.Add(eliminarButton);
            buttonPanel.Children.Add(cancelarButton);
            panel.Children.Add(buttonPanel);

            // Crear ventana de diálogo
            var confirmDialog = new Window
            {
                Title = "Confirmar eliminación",
                Content = panel,
                Width = 350,
                Height = 160,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                SizeToContent = SizeToContent.Height
            };

            // Usar TaskCompletionSource para esperar la respuesta
            var tcs = new TaskCompletionSource<bool>();

            eliminarButton.Click += (_, _) =>
            {
                tcs.SetResult(true);
                confirmDialog.Close();
            };

            cancelarButton.Click += (_, _) =>
            {
                tcs.SetResult(false);
                confirmDialog.Close();
            };

            // También manejar el cierre de la ventana (X)
            confirmDialog.Closed += (_, _) =>
            {
                if (!tcs.Task.IsCompleted)
                    tcs.SetResult(false);
            };

            // Mostrar ventana modal
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel != null)
            {
                confirmDialog.ShowDialog(topLevel as Window);
            }
            else
            {
                await confirmDialog.ShowDialog(null);
            }

            // Esperar el resultado
            bool confirmed = await tcs.Task;

            if (confirmed)
            {
                try
                {
                    // Llamar al método de eliminación
                    OtrosServicios.EliminarArticulo(articulo.ArticuloId);

                    // Mostrar mensaje de éxito
                    await MostrarMensaje("Éxito", "Artículo eliminado correctamente.");

                    // Actualizar la lista de artículos
                    CargarArticulos();
                }
                catch (Exception ex)
                {
                    await MostrarMensajeError($"No se pudo eliminar el artículo: {ex.Message}");
                }
            }
        }
    }

    private async Task MostrarMensajeError(string mensaje)
    {
        // Crear contenedor principal para el diálogo
        var panel = new StackPanel
        {
            Spacing = 15,
            Width = 350,
            Margin = new Thickness(20)
        };

        // Mensaje
        var messageText = new TextBlock
        {
            Text = mensaje,
            TextWrapping = TextWrapping.Wrap
        };
        panel.Children.Add(messageText);

        // Botón Aceptar
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 10, 0, 0)
        };

        var okButton = new Button { Content = "Aceptar" };
        buttonPanel.Children.Add(okButton);
        panel.Children.Add(buttonPanel);

        // Crear ventana de diálogo
        var messageDialog = new Window
        {
            Title = "Error",
            Content = panel,
            Width = 400,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            SizeToContent = SizeToContent.Height
        };

        okButton.Click += (_, _) => messageDialog.Close();

        // Mostrar ventana modal
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
        {
            messageDialog.ShowDialog(topLevel as Window);
        }
        else
        {
            await messageDialog.ShowDialog(null);
        }
    }

    private async Task MostrarMensaje(string titulo, string mensaje)
    {
        // Crear contenedor principal para el diálogo
        var panel = new StackPanel
        {
            Spacing = 15,
            Width = 350,
            Margin = new Thickness(20)
        };

        // Mensaje
        var messageText = new TextBlock
        {
            Text = mensaje,
            TextWrapping = TextWrapping.Wrap
        };
        panel.Children.Add(messageText);

        // Botón Aceptar
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 10, 0, 0)
        };

        var okButton = new Button { Content = "Aceptar" };
        buttonPanel.Children.Add(okButton);
        panel.Children.Add(buttonPanel);

        // Crear ventana de diálogo
        var messageDialog = new Window
        {
            Title = titulo,
            Content = panel,
            Width = 400,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            SizeToContent = SizeToContent.Height
        };

        okButton.Click += (_, _) => messageDialog.Close();

        // Mostrar ventana modal
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null)
        {
            messageDialog.ShowDialog(topLevel as Window);
        }
        else
        {
            await messageDialog.ShowDialog(null);
        }
    }

    // Método para refrescar los datos después de una operación CRUD
    public void RefrescarDatos()
    {
        CargarArticulos();
    }
}