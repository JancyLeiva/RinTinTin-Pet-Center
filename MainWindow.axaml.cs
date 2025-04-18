using System.Linq;
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

        private AppointmentsView? _appointmentsView;
        private ClientsView? _clientsView;
        private UserControl? _reportsView;

        private Panel? _viewContainer;

        public MainWindow()
        {
            InitializeComponent();
            SetupNavigation();
            InitializeViews();

            _currentActiveButton = DashboardButton;
            HighlightActiveButton(_currentActiveButton);

            ShowView(_appointmentsView);
        }

        private void InitializeViews()
        {
            _viewContainer = new Grid();
            MainContent.Content = _viewContainer;

            _appointmentsView = new AppointmentsView();
            _clientsView = new ClientsView();

            _viewContainer.Children.Add(_appointmentsView);
            _viewContainer.Children.Add(_clientsView);

            foreach (var child in _viewContainer.Children.OfType<Control>())
            {
                child.IsVisible = false;
            }
        }

        private void ShowView(Control? view)
        {
            if (view == null || _viewContainer == null) return;

            foreach (var child in _viewContainer.Children.OfType<Control>())
            {
                child.IsVisible = false;
            }

            view.IsVisible = true;
        }

        private void SetupNavigation()
        {
            DashboardButton.Click += (s, e) => NavigateToModule("Appointments", DashboardButton);
            PeopleListButton.Click += (s, e) => NavigateToModule("Clients", PeopleListButton);
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
                    ShowView(_appointmentsView);
                    break;
                case "Clients":
                    ShowView(_clientsView);
                    break;
                case "Reports":
                    if (_reportsView == null)
                    {
                        _reportsView = new UserControl();
                        _viewContainer?.Children.Add(_reportsView);
                    }

                    ShowView(_reportsView);
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