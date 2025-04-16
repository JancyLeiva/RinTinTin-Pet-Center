using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ProyectoBD2.Services;
using System.Threading.Tasks;
using System;

namespace ProyectoBD2.Windows
{
    public partial class LoginView : Window
    {
        private TextBox? _usernameTextBox; private TextBox? _passwordTextBox;

    public LoginView()
        {
            InitializeComponent();

            _usernameTextBox = this.FindControl<TextBox>("UsernameTextBox");
            _passwordTextBox = this.FindControl<TextBox>("PasswordTextBox");
            var loginButton = this.FindControl<Button>("LoginButton");

            loginButton.Click += OnLoginButtonClick;
        }

        private async void OnLoginButtonClick(object? sender, RoutedEventArgs e)
        {
            string usuario = _usernameTextBox?.Text?.Trim() ?? "";
            string contrasena = _passwordTextBox?.Text ?? "";

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contrasena))
            {
                await ShowMessage("Debe ingresar usuario y contrase�a.");
                return;
            }

            try
            {
                var (esValido, roles) = AuthService.ValidarCredenciales(usuario, contrasena);

                if (esValido)
                {
                    SessionService.Usuario = usuario; SessionService.Roles = roles;

                    await ShowMessage($"Bienvenido, {usuario}.\nRoles: {roles}");

                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    await ShowMessage("Credenciales incorrectas.");
                }
            }
            catch (Exception ex)
            {
                await ShowMessage($"Error: {ex.Message}");
            }
        }

        private async Task ShowMessage(string message)
        {
            var dialog = new Window { 
                Title = "Mensaje", Width = 300, Height = 150, 
                Content = new StackPanel { Margin = new Thickness(20), 
                    Children = { new TextBlock { Text = message, 
                        TextWrapping = TextWrapping.Wrap }, new Button { 
                            Content = "Aceptar", Margin = new Thickness(0, 20, 0, 0), 
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center } } } 
            };

            var button = ((dialog.Content as StackPanel)?.Children[1]) as Button;
            button!.Click += (_, _) => dialog.Close();

            await dialog.ShowDialog(this);
            return;
        }
    }
}