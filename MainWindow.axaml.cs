using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using ProyectoBD2.Views;
using ProyectoBD2.Windows;

namespace ProyectoBD2
{
    public partial class MainWindow : Window
    {
        private Button? _currentActiveButton;
        private string _currentModule = "Appointments"; // Changed default module

        public MainWindow()
        {
            InitializeComponent();
            SetupNavigation();

            // Set Appointments as the initially active module
            _currentActiveButton = DashboardButton;
            HighlightActiveButton(_currentActiveButton);
            
            // Load the appointments view by default
            MainContent.Content = new AppointmentsView();
        }

        private void SetupNavigation()
        {
            // Conectar los eventos de click para cada botón de navegación
            DashboardButton.Click += (s, e) => NavigateToModule("Appointments", DashboardButton);
            PeopleListButton.Click += (s, e) => NavigateToModule("PeopleList", PeopleListButton);
            ReportsButton.Click += (s, e) => NavigateToModule("Reports", ReportsButton);
            
            // Setup logout button event
            LogoutButton.Click += (s, e) => LogoutUser();
        }

        private void NavigateToModule(string moduleName, Button clickedButton)
        {
            // Skip if already on the same module
            if (_currentModule == moduleName)
                return;

            _currentModule = moduleName;

            // Reset previous button style and set new active button
            ResetButtonHighlight(_currentActiveButton);
            _currentActiveButton = clickedButton;
            HighlightActiveButton(_currentActiveButton);

            // Cargar el UserControl correspondiente al módulo
            switch (moduleName)
            {
                case "Appointments":
                    MainContent.Content = new AppointmentsView();
                    break;
                case "PeopleList":
                    // MainContent.Content = new PeopleListView();
                    break;
                case "Reports":
                    // MainContent.Content = new ReportsView();
                    break;
            }
        }

        private void LogoutUser()
        {
            // Create and show the login window
            var loginWindow = new LoginView();
            loginWindow.Show();
            
            // Close the current main window
            this.Close();
        }

        private void HighlightActiveButton(Button? button)
        {
            if (button != null)
            {
                // Visual indication that this is the active module
                button.Background = new SolidColorBrush(Color.Parse("#206EA9"));
                button.Opacity = 1;
            }
        }

        private void ResetButtonHighlight(Button? button)
        {
            if (button != null)
            {
                // Reset to default style
                button.Background = new SolidColorBrush(Colors.Transparent);
                button.Opacity = 0.8;
            }
        }

        public string GetCurrentModule()
        {
            return _currentModule;
        }
    }
}