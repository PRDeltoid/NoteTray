using Autofac;
using NoteTrayLib.Services;

namespace NoteTray;

public static class ContainerConfig
{
    public static IContainer CreateContainer()
    {
        ContainerBuilder builder = new ContainerBuilder();
            
        // Configure the DI container
        builder.RegisterType<MainWindow>();
        builder.RegisterType<DirectoryManagerService>();
        builder.RegisterType<SQLiteDatabaseService>().As<IDatabaseService>().WithParameter("databaseFileName", "notetray.db");
        builder.RegisterType<EditorManagerService>();
        builder.RegisterType<UserPreferenceService>();
        
        return builder.Build();
    }
}