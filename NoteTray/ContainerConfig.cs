using Autofac;
using NoteTrayLib.Services;

namespace NoteTray;

public static class ContainerConfig
{
    public static IContainer CreateContainer()
    {
        ContainerBuilder builder = new();

        // Configure the DI container
        builder.RegisterType<MainWindow>();
        builder.RegisterType<DirectoryManagerService>();
        builder.RegisterType<SQLiteDatabaseService>().As<IDatabaseService>()
            .WithParameter("databaseFileName", "notetray.db");
        builder.RegisterType<EditorManagerService>();
        builder.RegisterType<UserPreferenceService>().SingleInstance();
        builder.RegisterType<NoteListViewModel>();
        builder.RegisterType<FileTrackerService>().SingleInstance();
        builder.RegisterType<FileChangeWatcher>().WithParameter("includeSubdirectories", true).SingleInstance()
            .AutoActivate(); // Since this runs in the background, we want it to autostart with the app and run for the entire duration
        builder.RegisterType<StartupScanService>();
        builder.RegisterType<FirstTimeSetupService>();
        builder.RegisterType<LuceneFullTextSearchService>().As<IFullTextSearchService>().WithParameter("indexName", "searchindex").SingleInstance();
        builder.RegisterType<StartupService>().AutoActivate(); // do some stuff at startup

        return builder.Build();
    }
}