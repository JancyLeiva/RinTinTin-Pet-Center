using System;
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

public partial class ProveedoresView : UserControl
{
    private ObservableCollection<Provider>? _proveedores;
    private Provider? _originalProveedorData;
    private Provider? _newProveedor;

    private enum ViewState
    {
        NoSelection,
        ViewingProveedor,
        CreatingProveedor,
        EditingProveedor
    }

    private ViewState _currentState = ViewState.NoSelection;

    public ProveedoresView()
    {
        InitializeComponent();

        // Configurar evento para botón de nuevo proveedor
        NuevoProveedorButton.Click += NuevoProveedorButton_Click;

        // Configurar el evento para personalizar las columnas del DataGrid
        ProveedoresDataGrid.AutoGeneratingColumn += ProveedoresDataGridAutoGeneratingColumn;
        ProveedoresDataGrid.SelectionChanged += ProveedoresDataGrid_SelectionChanged;

        // Cargar datos de proveedores al inicializar
        CargarProveedores();
    }

    private void CargarProveedores()
    {
        try
        {
            _proveedores = new ObservableCollection<Provider>();
            var data = OtrosServicios.AllProveedores();

            foreach (DataRow row in data.Rows)
            {
                _proveedores.Add(new Provider
                {
                    ProveedorId = row["ProveedorID"] == DBNull.Value ? null : (int?)row["ProveedorID"],
                    CodigoProveedor = row["CodigoProveedor"] == DBNull.Value ? null : (string)row["CodigoProveedor"],
                    Nombre = row["Nombre"] == DBNull.Value ? null : (string)row["Nombre"],
                    Contacto = row["Contacto"] == DBNull.Value ? null : (string)row["Contacto"],
                    Numero = row["Numero"] == DBNull.Value ? null : (string)row["Numero"],
                    Direccion = row["Direccion"] == DBNull.Value ? null : (string)row["Direccion"],
                    RTN = row["RTN"] == DBNull.Value ? null : (string)row["RTN"]
                });
            }

            ProveedoresDataGrid.ItemsSource = _proveedores;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar proveedores: {ex.Message}");
        }
    }

    private void ProveedoresDataGridAutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        if (!(sender is DataGrid dataGrid))
            return;

        // Ocultar columnas específicas si es necesario
        if (e.PropertyName == "ProveedorID")
        {
            e.Cancel = true;
        }

        // Añadir columna de acciones al final
        if (sender is DataGrid grid && e.PropertyName == "RTN")
        {
            Dispatcher.UIThread.Post(() =>
            {
                // Verificar si ya existe una columna de acciones
                foreach (var column in dataGrid.Columns)
                {
                    if (column is DataGridTemplateColumn templateColumn &&
                        templateColumn.Header?.ToString() == "Acciones")
                        return;
                }
                
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
                        // Panel para los botones
                        var panel = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Spacing = 5,
                            HorizontalAlignment = HorizontalAlignment.Center
                        };

                        // Botón de editar
                        var editButton = new Button
                        {
                            Content = "Editar",
                            Background = new SolidColorBrush(Color.Parse("#0275d8")),
                            Foreground = Brushes.White,
                            Padding = new Thickness(8, 4, 8, 4),
                            CommandParameter = item
                        };
                        editButton.Click += EditProveedor_Click;

                        // Botón de eliminar
                        var deleteButton = new Button
                        {
                            Content = "Eliminar",
                            Background = new SolidColorBrush(Color.Parse("#dc3545")),
                            Foreground = Brushes.White,
                            Padding = new Thickness(8, 4, 8, 4),
                            CommandParameter = item
                        };
                        deleteButton.Click += DeleteProveedor_Click;

                        // Agregar botones al panel
                        panel.Children.Add(editButton);
                        panel.Children.Add(deleteButton);
                        return panel;
                    })
                };

                dataGrid.Columns.Add(actionColumn);
            });
        }
    }

    private void ProveedoresDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ProveedoresDataGrid.SelectedItem is not Provider selectedProveedor)
        {
            _currentState = ViewState.NoSelection;
            return;
        }

        _currentState = ViewState.ViewingProveedor;
    }

    private async void NuevoProveedorButton_Click(object? sender, RoutedEventArgs e)
    {
        // Crear formulario para nuevo proveedor
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

        // Contacto
        var contactoPanel = new StackPanel();
        contactoPanel.Children.Add(new TextBlock { Text = "Contacto:" });
        var contactoTextBox = new TextBox();
        contactoPanel.Children.Add(contactoTextBox);
        formPanel.Children.Add(contactoPanel);

        // Número
        var numeroPanel = new StackPanel();
        numeroPanel.Children.Add(new TextBlock { Text = "Número:" });
        var numeroTextBox = new TextBox();
        numeroPanel.Children.Add(numeroTextBox);
        formPanel.Children.Add(numeroPanel);

        // Dirección
        var direccionPanel = new StackPanel();
        direccionPanel.Children.Add(new TextBlock { Text = "Dirección:" });
        var direccionTextBox = new TextBox();
        direccionPanel.Children.Add(direccionTextBox);
        formPanel.Children.Add(direccionPanel);

        // RTN
        var rtnPanel = new StackPanel();
        rtnPanel.Children.Add(new TextBlock { Text = "RTN:" });
        var rtnTextBox = new TextBox();
        rtnPanel.Children.Add(rtnTextBox);
        formPanel.Children.Add(rtnPanel);

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
            Title = "Nuevo Provider",
            Width = 450,
            Height = 450,
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
                string contacto = contactoTextBox.Text?.Trim() ?? string.Empty;
                string numero = numeroTextBox.Text?.Trim() ?? string.Empty;
                string direccion = direccionTextBox.Text?.Trim() ?? string.Empty;
                string rtn = rtnTextBox.Text?.Trim() ?? string.Empty;

                // Validar campos obligatorios
                if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(contacto))
                {
                    await MostrarMensajeError("El nombre y el contacto son obligatorios.");
                    return;
                }

                Console.WriteLine($"Creando proveedor: {nombre}, {contacto}, {numero}, {direccion}, {rtn}");

                // Crear proveedor y obtener resultado
                var resultado = OtrosServicios.CrearProveedor(nombre, contacto, numero, direccion, rtn);
                
                // Mostrar mensaje de éxito
                await MostrarMensaje("Éxito", "Provider creado correctamente.");

                // Actualizar lista de proveedores
                CargarProveedores();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear proveedor: {ex.Message}");
                await MostrarMensajeError($"No se pudo crear el proveedor: {ex.Message}");
            }
        }
    }

    private async void EditProveedor_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Provider proveedor)
        {
            // Verificar que el código no sea nulo
            if (string.IsNullOrEmpty(proveedor.CodigoProveedor))
            {
                return;
            }

            // Guardar datos originales para referencia
            _originalProveedorData = new Provider
            {
                ProveedorId = proveedor.ProveedorId,
                CodigoProveedor = proveedor.CodigoProveedor,
                Nombre = proveedor.Nombre,
                Contacto = proveedor.Contacto,
                Numero = proveedor.Numero,
                Direccion = proveedor.Direccion,
                RTN = proveedor.RTN
            };

            // Crear formulario para editar proveedor
            var formPanel = new StackPanel
            {
                Spacing = 10,
                Width = 400,
                Margin = new Thickness(20)
            };

            // Nombre
            var nombrePanel = new StackPanel();
            nombrePanel.Children.Add(new TextBlock { Text = "Nombre:" });
            var nombreTextBox = new TextBox { Text = proveedor.Nombre ?? string.Empty };
            nombrePanel.Children.Add(nombreTextBox);
            formPanel.Children.Add(nombrePanel);

            // Contacto
            var contactoPanel = new StackPanel();
            contactoPanel.Children.Add(new TextBlock { Text = "Contacto:" });
            var contactoTextBox = new TextBox { Text = proveedor.Contacto ?? string.Empty };
            contactoPanel.Children.Add(contactoTextBox);
            formPanel.Children.Add(contactoPanel);

            // Número
            var numeroPanel = new StackPanel();
            numeroPanel.Children.Add(new TextBlock { Text = "Número:" });
            var numeroTextBox = new TextBox { Text = proveedor.Numero ?? string.Empty };
            numeroPanel.Children.Add(numeroTextBox);
            formPanel.Children.Add(numeroPanel);

            // Dirección
            var direccionPanel = new StackPanel();
            direccionPanel.Children.Add(new TextBlock { Text = "Dirección:" });
            var direccionTextBox = new TextBox { Text = proveedor.Direccion ?? string.Empty };
            direccionPanel.Children.Add(direccionTextBox);
            formPanel.Children.Add(direccionPanel);

            // RTN
            var rtnPanel = new StackPanel();
            rtnPanel.Children.Add(new TextBlock { Text = "RTN:" });
            var rtnTextBox = new TextBox { Text = proveedor.RTN ?? string.Empty };
            rtnPanel.Children.Add(rtnTextBox);
            formPanel.Children.Add(rtnPanel);

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
                Title = "Editar Provider",
                Width = 450,
                Height = 450,
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
                    string contacto = contactoTextBox.Text?.Trim() ?? string.Empty;
                    string numero = numeroTextBox.Text?.Trim() ?? string.Empty;
                    string direccion = direccionTextBox.Text?.Trim() ?? string.Empty;
                    string rtn = rtnTextBox.Text?.Trim() ?? string.Empty;

                    // Validar campos obligatorios
                    if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(contacto))
                    {
                        await MostrarMensajeError("El nombre y el contacto son obligatorios.");
                        return;
                    }

                    Console.WriteLine($"Editando proveedor: Código={proveedor.CodigoProveedor}, {nombre}, {contacto}, {numero}, {direccion}, {rtn}");

                    // Editar proveedor y obtener resultado
                    var resultado = OtrosServicios.EditarProveedor(
                        proveedor.CodigoProveedor,
                        nombre,
                        contacto,
                        numero,
                        direccion,
                        rtn
                    );

                    // Mostrar mensaje de éxito
                    await MostrarMensaje("Éxito", "Provider actualizado correctamente.");

                    // Actualizar lista de proveedores
                    CargarProveedores();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al editar proveedor: {ex.Message}");
                    await MostrarMensajeError($"No se pudo editar el proveedor: {ex.Message}");
                }
            }
        }
    }

    private async void DeleteProveedor_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Provider proveedor)
        {
            // Verificar que el código no sea nulo
            if (string.IsNullOrEmpty(proveedor.CodigoProveedor))
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
                Text = $"¿Está seguro que desea eliminar al proveedor {proveedor.Nombre}?",
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
                    OtrosServicios.EliminarProveedor(proveedor.CodigoProveedor);

                    // Mostrar mensaje de éxito
                    await MostrarMensaje("Éxito", "Provider eliminado correctamente.");

                    // Actualizar la lista de proveedores
                    CargarProveedores();
                }
                catch (Exception ex)
                {
                    await MostrarMensajeError($"No se pudo eliminar el proveedor: {ex.Message}");
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
        CargarProveedores();
    }
}