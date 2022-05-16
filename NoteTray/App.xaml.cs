using System.Windows;
using Autofac;
using Serilog;
using Serilog.Core;

namespace NoteTray
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IContainer _container;

        public App()
        {
            _container = ContainerConfig.CreateContainer();
            using Logger log = new LoggerConfiguration()
                .WriteTo.Debug()
                // .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            Log.Logger = log;
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            MainWindow mainForm = _container.Resolve<MainWindow>();
            mainForm.Show();
        }
    }
}