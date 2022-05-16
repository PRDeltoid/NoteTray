using System.Windows;
using Autofac;
using NoteTrayLib;

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
            ContainerBuilder builder = new ContainerBuilder();
            
            // Configure the DI container
            builder.RegisterType<MainWindow>();
            builder.RegisterType<DirectoryService>();
            
            // Finish building the container
            _container = builder.Build();
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            MainWindow mainForm = _container.Resolve<MainWindow>();
            mainForm.Show();
        }
    }
}