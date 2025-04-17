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
        private string _currentModule = "Appointments";

        public MainWindow()
        {
            InitializeComponent();
            SetupNavigation();

            _currentActiveButton = DashboardButton;
            HighlightActiveButton(_currentActiveButton);
            
            MainContent.Content = new AppointmentsView();
        }

        private void SetupNavigation()
        {
            DashboardButton.Click += (s, e) => NavigateToModule("Appointments", DashboardButton);
            PeopleListButton.Click += (s, e) => NavigateToModule("PeopleList", PeopleListButton);
            ReportsButton.Click += (s, e) => NavigateToModule("Reports", ReportsButton);
            LogoutButton.Click += (s, e) => LogoutUser();
        }

        private void NavigateToModule(string moduleName, Button clickedButton)
        {
            if (_currentModule == moduleName)
                return;

            _currentModule = moduleName;

            ResetButtonHighlight(_currentActiveButton);
            _currentActiveButton = clickedButton;
            HighlightActiveButton(_currentActiveButton);

            switch (moduleName)
            {
                case "Appointments":
                    MainContent.Content = new AppointmentsView();
                    break;
                case "PeopleList":
                case "Reports":
                    break;
            }
        }

        private void LogoutUser()
        {
            var loginWindow = new LoginView();
            loginWindow.Show();
            
            this.Close();
        }

        private static void HighlightActiveButton(Button? button)
        {
            if (button == null) return;
            button.Background = new SolidColorBrush(Color.Parse("#206EA9"));
            button.Opacity = 1;
        }

        private static void ResetButtonHighlight(Button? button)
        {
            if (button == null) return;
            button.Background = new SolidColorBrush(Colors.Transparent);
            button.Opacity = 0.8;
        }

        public string GetCurrentModule()
        {
            return _currentModule;
        }
    }
}