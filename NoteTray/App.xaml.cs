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
            using Logger log = new LoggerConfiguration()
                .WriteTo.Debug()
                #if DEBUG
                .MinimumLevel.Debug()
                #endif
                .CreateLogger();
            Log.Logger = log;
            _container = ContainerConfig.CreateContainer();
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            MainWindow mainForm = _container.Resolve<MainWindow>();
            mainForm.Show();
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            Log.CloseAndFlush();
        }
    }
}