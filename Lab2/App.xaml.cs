// App.xaml.cs
using System.Windows;

namespace Lab2
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow mainWindow = new MainWindow();
            mainWindow.DataContext = new ViewModels.MainViewModel();
            mainWindow.Show();
        }
    }
}
