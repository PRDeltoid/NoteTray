using Autofac;
using NoteTrayLib;

namespace NoteTray;

public class ContainerConfig
{
    public static IContainer CreateContainer()
    {
        ContainerBuilder builder = new ContainerBuilder();
            
        // Configure the DI container
        builder.RegisterType<MainWindow>();
        builder.RegisterType<DirectoryService>();
        
        return builder.Build();
    }
}