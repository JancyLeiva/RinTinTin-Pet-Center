using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace ProyectoBD2.Windows;

public partial class LoginWindow : Window
{
    private TextBox? _usernameTextBox;
    private TextBox? _passwordTextBox;

    public LoginWindow()
    {
        InitializeComponent();
            
        _usernameTextBox = this.FindControl<TextBox>("UsernameTextBox");
        _passwordTextBox = this.FindControl<TextBox>("PasswordTextBox");
        var loginButton = this.FindControl<Button>("LoginButton");
            
        loginButton.Click += OnLoginButtonClick;
    }

    private void OnLoginButtonClick(object? sender, RoutedEventArgs routedEventArgs)
    {
        if (!string.IsNullOrEmpty(_usernameTextBox?.Text) && 
            !string.IsNullOrEmpty(_passwordTextBox?.Text))
        {
            // Login successful
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}