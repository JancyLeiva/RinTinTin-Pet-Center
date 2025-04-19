using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ProyectoBD2.Services;
using System;
using System.Threading.Tasks;

namespace ProyectoBD2.Windows
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
            PasswordTextBox.RevealPassword = false;
            PasswordTextBox.PasswordChar = '•';
            EyeIcon.Data = Geometry.Parse(
                "M12,9.0046246 C14.209139,9.0046246 16,10.7954856 16,13.0046246 C16,15.2137636 14.209139,17.0046246 12,17.0046246 C9.790861,17.0046246 8,15.2137636 8,13.0046246 C8,10.7954856 9.790861,9.0046246 12,9.0046246 Z M12,10.5046246 C10.6192881,10.5046246 9.5,11.6239127 9.5,13.0046246 C9.5,14.3853365 10.6192881,15.5046246 12,15.5046246 C13.3807119,15.5046246 14.5,14.3853365 14.5,13.0046246 C14.5,11.6239127 13.3807119,10.5046246 12,10.5046246 Z M12,5.5 C16.613512,5.5 20.5960869,8.65000641 21.7011157,13.0643865 C21.8017,13.4662019 21.557504,13.8734775 21.1556885,13.9740618 C20.7538731,14.0746462 20.3465976,13.8304502 20.2460132,13.4286347 C19.3071259,9.67795854 15.9213644,7 12,7 C8.07693257,7 4.69009765,9.68026417 3.75285786,13.4331499 C3.65249525,13.8350208 3.24535455,14.0794416 2.84348365,13.979079 C2.44161275,13.8787164 2.19719198,13.4715757 2.29755459,13.0697048 C3.4006459,8.65271806 7.38448293,5.5 12,5.5 Z");

            UsernameTextBox.Text = "k.martinez";
            PasswordTextBox.Text = "Rinti123!";

            LoginButton.Click += OnLoginButtonClick;
            TogglePasswordButton.Click += OnTogglePasswordClick;

            UsernameTextBox.TextChanged += (s, e) => HideError();
            PasswordTextBox.TextChanged += (s, e) => HideError();

            UsernameTextBox.KeyDown += OnKeyDown;
            PasswordTextBox.KeyDown += OnKeyDown;
        }

        private void OnTogglePasswordClick(object? sender, RoutedEventArgs e)
        {
            PasswordTextBox.RevealPassword = !PasswordTextBox.RevealPassword;

            if (!PasswordTextBox.RevealPassword)
            {
                EyeIcon.Data = Geometry.Parse(
                    "M12,9.0046246 C14.209139,9.0046246 16,10.7954856 16,13.0046246 C16,15.2137636 14.209139,17.0046246 12,17.0046246 C9.790861,17.0046246 8,15.2137636 8,13.0046246 C8,10.7954856 9.790861,9.0046246 12,9.0046246 Z M12,10.5046246 C10.6192881,10.5046246 9.5,11.6239127 9.5,13.0046246 C9.5,14.3853365 10.6192881,15.5046246 12,15.5046246 C13.3807119,15.5046246 14.5,14.3853365 14.5,13.0046246 C14.5,11.6239127 13.3807119,10.5046246 12,10.5046246 Z M12,5.5 C16.613512,5.5 20.5960869,8.65000641 21.7011157,13.0643865 C21.8017,13.4662019 21.557504,13.8734775 21.1556885,13.9740618 C20.7538731,14.0746462 20.3465976,13.8304502 20.2460132,13.4286347 C19.3071259,9.67795854 15.9213644,7 12,7 C8.07693257,7 4.69009765,9.68026417 3.75285786,13.4331499 C3.65249525,13.8350208 3.24535455,14.0794416 2.84348365,13.979079 C2.44161275,13.8787164 2.19719198,13.4715757 2.29755459,13.0697048 C3.4006459,8.65271806 7.38448293,5.5 12,5.5 Z");
                return;
            }

            EyeIcon.Data = Geometry.Parse(
                "M2,5.27L3.28,4L20,20.72L18.73,22L15.65,18.92C14.5,19.3 13.28,19.5 12,19.5C7,19.5 2.73,16.39 1,12C1.69,10.24 2.79,8.69 4.19,7.46L2,5.27M12,9A3,3 0 0,1 15,12C15,12.35 14.94,12.69 14.83,13L11,9.17C11.31,9.06 11.65,9 12,9M12,4.5C17,4.5 21.27,7.61 23,12C22.18,14.08 20.79,15.88 19,17.19L17.58,15.76C18.94,14.82 20.06,13.54 20.82,12C19.17,8.64 15.76,6.5 12,6.5C10.91,6.5 9.84,6.68 8.84,7L7.3,5.47C8.74,4.85 10.33,4.5 12,4.5M3.18,12C4.83,15.36 8.24,17.5 12,17.5C12.69,17.5 13.37,17.43 14,17.29L11.72,15C10.29,14.85 9.15,13.71 9,12.28L5.6,8.87C4.61,9.72 3.78,10.78 3.18,12Z");
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TryLogin();
            }
        }

        private void OnLoginButtonClick(object? sender, RoutedEventArgs e)
        {
            TryLogin();
        }

        private async void TryLogin()
        {
            try
            {
                var usuario = UsernameTextBox.Text?.Trim() ?? "";
                var contrasena = PasswordTextBox.Text ?? "";

                if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contrasena))
                {
                    ShowError("Debe ingresar usuario y contraseña.");
                    return;
                }

                SetLoadingState(true);

                try
                {
                    await Task.Delay(1000);
                    var authenticated = AuthService.AutenticarUsuario(usuario, contrasena);

                    if (authenticated)
                    {
                        var roleInfo = "";
                        if (SessionService.EsAdministrador) roleInfo = "Administrador";
                        else if (SessionService.EsVeterinario) roleInfo = "Veterinario";
                        else if (SessionService.EsRecepcionista) roleInfo = "Recepcionista";

                        Console.WriteLine($"Usuario autenticado como: {roleInfo}");

                        var mainWindow = new MainWindow();
                        mainWindow.Show();
                        Close();
                    }
                    else
                    {
                        ShowError("Credenciales incorrectas.");
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Error al iniciar sesión: {ex.Message}");
                }
                finally
                {
                    SetLoadingState(false);
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorPanel.IsVisible = true;
        }

        private void HideError()
        {
            ErrorPanel.IsVisible = false;
        }

        private void SetLoadingState(bool isLoading)
        {
            LoadingBar.IsVisible = isLoading;
            LoginButton.IsEnabled = !isLoading;
            UsernameTextBox.IsEnabled = !isLoading;
            PasswordTextBox.IsEnabled = !isLoading;
            TogglePasswordButton.IsEnabled = !isLoading;
        }
    }
}