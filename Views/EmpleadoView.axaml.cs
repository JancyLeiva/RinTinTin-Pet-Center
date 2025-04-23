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

public partial class EmpleadoView : UserControl
{
    private ObservableCollection<Empleado>? _empleados;
    private Empleado? _originalEmpleadoData;
    private Empleado? _newEmpleado;

    private enum ViewState
    {
        NoSelection,
        ViewingEmpleado,
        CreatingEmpleado,
        EditingEmpleado
    }

    private ViewState _currentState = ViewState.NoSelection;

    public EmpleadoView()
    {
        InitializeComponent();

        // Configurar evento para botón de nuevo empleado
        NuevoEmpleadoButton.Click += NuevoEmpleadoButton_Click;

        // Configurar el evento para personalizar las columnas del DataGrid
        EmpleadosDataGrid.AutoGeneratingColumn += EmpleadosDataGridAutoGeneratingColumn;
        EmpleadosDataGrid.SelectionChanged += EmpleadosDataGrid_SelectionChanged;

        // Cargar datos de empleados al inicializar
        CargarEmpleados();
    }

    private void CargarEmpleados()
    {
        try
        {
            _empleados = new ObservableCollection<Empleado>();
            var data = OtrosServicios.AllEmpleados();

            foreach (DataRow row in data.Rows)
            {
                _empleados.Add(new Empleado
                {
                    EmpleadoID = row["EmpleadoID"] == DBNull.Value ? null : (int?)row["EmpleadoID"],
                    CodigoEmpleado = row["CodigoEmpleado"] == DBNull.Value ? null : (string)row["CodigoEmpleado"],
                    Nombre = row["Nombre"] == DBNull.Value ? null : (string)row["Nombre"],
                    Identificacion = row["Identificacion"] == DBNull.Value ? null : (string)row["Identificacion"],
                    Puesto = row["Puesto"] == DBNull.Value ? null : (string)row["Puesto"],
                    DepartamentoID = row["DepartamentoID"] == DBNull.Value ? null : (int?)row["DepartamentoID"],
                    Telefono = row["Telefono"] == DBNull.Value ? null : (string)row["Telefono"]
                });
            }

            EmpleadosDataGrid.ItemsSource = _empleados;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar empleados: {ex.Message}");
        }
    }

    private void EmpleadosDataGridAutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        if (!(sender is DataGrid dataGrid))
            return;

        // Ocultar columnas específicas si es necesario
        if (e.PropertyName == "DepartamentoID")
        {
            e.Cancel = true;
        }

        // Añadir columna de acciones al final
        if (sender is DataGrid grid && e.PropertyName == "Telefono")
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
                        editButton.Click += EditEmpleado_Click;

                        // Botón de eliminar
                        var deleteButton = new Button
                        {
                            Content = "Eliminar",
                            Background = new SolidColorBrush(Color.Parse("#dc3545")),
                            Foreground = Brushes.White,
                            Padding = new Thickness(8, 4, 8, 4),
                            CommandParameter = item
                        };
                        deleteButton.Click += DeleteEmpleado_Click;

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

    private void EmpleadosDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (EmpleadosDataGrid.SelectedItem is not Empleado selectedEmpleado)
        {
            _currentState = ViewState.NoSelection;
            return;
        }

        _currentState = ViewState.ViewingEmpleado;
    }

   private async void NuevoEmpleadoButton_Click(object? sender, RoutedEventArgs e)
{
    // Crear formulario para nuevo empleado
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

    // Identificación
    var identificacionPanel = new StackPanel();
    identificacionPanel.Children.Add(new TextBlock { Text = "Identificación:" });
    var identificacionTextBox = new TextBox();
    identificacionPanel.Children.Add(identificacionTextBox);
    formPanel.Children.Add(identificacionPanel);

    // Puesto
    var puestoPanel = new StackPanel();
    puestoPanel.Children.Add(new TextBlock { Text = "Puesto:" });
    var puestoTextBox = new TextBox();
    puestoPanel.Children.Add(puestoTextBox);
    formPanel.Children.Add(puestoPanel);

    // Departamento
    var departamentoPanel = new StackPanel();
    departamentoPanel.Children.Add(new TextBlock { Text = "Departamento ID:" });
    var departamentoTextBox = new TextBox { Text = "1" }; // Valor por defecto
    departamentoPanel.Children.Add(departamentoTextBox);
    formPanel.Children.Add(departamentoPanel);

    // Teléfono
    var telefonoPanel = new StackPanel();
    telefonoPanel.Children.Add(new TextBlock { Text = "Teléfono:" });
    var telefonoTextBox = new TextBox();
    telefonoPanel.Children.Add(telefonoTextBox);
    formPanel.Children.Add(telefonoPanel);

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
        Title = "Nuevo Empleado",
        Width = 450,
        Height = 400,
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
            string identificacion = identificacionTextBox.Text?.Trim() ?? string.Empty;
            string puesto = puestoTextBox.Text?.Trim() ?? string.Empty;
            
            // Para el departamentoID, usamos un valor por defecto si no se puede convertir
            int departamentoID = 1;
            int.TryParse(departamentoTextBox.Text, out departamentoID);
            
            string telefono = telefonoTextBox.Text?.Trim() ?? string.Empty;

            // Validar campos obligatorios
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(identificacion))
            {
                await MostrarMensajeError("El nombre y la identificación son obligatorios.");
                return;
            }

            Console.WriteLine($"Creando empleado: {nombre}, {identificacion}, {puesto}, {departamentoID}, {telefono}");

            // Crear empleado y obtener resultado
            var resultado = OtrosServicios.CrearEmpleado(nombre, identificacion, puesto, departamentoID, telefono);
            
            // Verificar si se obtuvo algún resultado
            if (resultado != null && resultado.Rows.Count > 0)
            {
                Console.WriteLine("Empleado creado con éxito");
                // Mostrar mensaje de éxito
                await MostrarMensaje("Éxito", "Empleado creado correctamente.");
            }
            else
            {
                Console.WriteLine("No se obtuvo respuesta al crear el empleado");
                await MostrarMensajeError("No se obtuvo respuesta al crear el empleado");
            }

            // Actualizar lista de empleados
            CargarEmpleados();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al crear empleado: {ex.Message}");
            await MostrarMensajeError($"No se pudo crear el empleado: {ex.Message}");
        }
    }
}

// Método auxiliar para mostrar mensajes de éxito
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

    private void EditEmpleado_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Empleado empleado)
        {
            Console.WriteLine($"Editar empleado: {empleado.EmpleadoID}");

            // Guardar datos originales para posible cancelación
            _originalEmpleadoData = new Empleado
            {
                EmpleadoID = empleado.EmpleadoID,
                CodigoEmpleado = empleado.CodigoEmpleado,
                Nombre = empleado.Nombre,
                Identificacion = empleado.Identificacion,
                Puesto = empleado.Puesto,
                DepartamentoID = empleado.DepartamentoID,
                Telefono = empleado.Telefono
            };

            _currentState = ViewState.EditingEmpleado;

            // Aquí se implementaría la lógica para mostrar un formulario de edición
        }
    }

  private async void DeleteEmpleado_Click(object? sender, RoutedEventArgs e)
{
    if (sender is Button button && button.CommandParameter is Empleado empleado)
    {
        // Verificar que el ID no sea nulo
        if (empleado.EmpleadoID == null)
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
            Text = $"¿Está seguro que desea eliminar al empleado {empleado.Nombre}?",
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
                OtrosServicios.EliminarEmpleado(empleado.EmpleadoID);

                // Actualizar la lista de empleados
                CargarEmpleados();
            }
            catch (Exception ex)
            {
                await MostrarMensajeError($"No se pudo eliminar el empleado: {ex.Message}");
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

    // Método para refrescar los datos después de una operación CRUD
    public void RefrescarDatos()
    {
        CargarEmpleados();
    }
}