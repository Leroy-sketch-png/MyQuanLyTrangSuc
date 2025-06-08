// MyQuanLyTrangSuc.View.MainNavigationWindow.xaml.cs
using System.Windows;
using System.Windows.Input;
using MyQuanLyTrangSuc.ViewModel; // Import your ViewModel namespace

namespace MyQuanLyTrangSuc.View
{
    public partial class MainNavigationWindow : Window
    {
        public MainNavigationWindow()
        {
            InitializeComponent();

            // Set DataContext here
            // Ensure Initialize is called once, passing 'this' window instance
            if (MainNavigationWindowLogic.Initialize(this))
            {
                this.DataContext = MainNavigationWindowLogic.Instance;
            }
            else
            {
                // If it's already initialized, just set the DataContext
                this.DataContext = MainNavigationWindowLogic.Instance;
            }

            // Call Authentification right after setting DataContext
            // The ViewModel will then notify the UI via OnPropertyChanged for CurrentUserPrincipal
            MainNavigationWindowLogic.Instance.Authentification();
        }

        private void Loaded_HomePage(object sender, RoutedEventArgs e)
        {
            // Now that DataContext is set and Authentification run,
            // we can execute the initial navigation command from the ViewModel.
            if (this.DataContext is MainNavigationWindowLogic viewModel)
            {
                // This is a common way to trigger an initial command on load
                viewModel.NavigateToDashboardCommand.Execute(null);
            }
        }

        // Keep existing Window drag/maximize logic if needed
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private bool isMaximized = false;
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (isMaximized)
                {
                    this.WindowState = WindowState.Normal;
                    this.Width = 1080;
                    this.Height = 720;
                    isMaximized = false;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;
                    isMaximized = true;
                }
            }
        }

    }
}