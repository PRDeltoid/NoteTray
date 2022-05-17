using Autofac;
using NoteTrayLib;
using NoteTrayLib.Services;

namespace NoteTray;

public static class ContainerConfig
{
    public static IContainer CreateContainer()
    {
        ContainerBuilder builder = new ContainerBuilder();
            
        // Configure the DI container
        builder.RegisterType<MainWindow>();
        builder.RegisterType<DirectoryService>();
        builder.RegisterType<SQLiteDatabaseService>().As<IDatabaseService>().WithParameter("databaseFileName", "notetray.db");
        builder.RegisterType<UserPreferenceService>();
        
        return builder.Build();
    }
}