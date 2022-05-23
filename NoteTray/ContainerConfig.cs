using Autofac;
using NoteTray.Commands;
using NoteTrayLib.Services;

namespace NoteTray;

public static class ContainerConfig
{
    public static IContainer CreateContainer()
    {
        ContainerBuilder builder = new();

        // Configure the DI container
        
        // NoteTray Config
        builder.RegisterType<MainWindow>();
        builder.RegisterType<NoteListViewModel>();
        builder.RegisterType<OpenPreferencesCommand>();
        builder.RegisterType<PreferencesViewModel>();
        builder.RegisterType<PreferencesWindow>();
        
        // NoteTrayLib Config
        builder.RegisterType<DirectoryManagerService>();
        builder.RegisterType<SQLiteDatabaseService>().As<IDatabaseService>()
            .WithParameter("databaseFileName", "notetray.db");
        builder.RegisterType<EditorManagerService>();
        builder.RegisterType<UserPreferenceDbService>().SingleInstance();
        builder.RegisterType<FileTrackerService>().SingleInstance();
        builder.RegisterType<FileChangeWatcher>().SingleInstance()
            .WithParameter("includeSubdirectories", true);
            // .AutoActivate(); // Since this runs in the background, we want it to autostart with the app and run for the entire duration
        builder.RegisterType<StartupScanService>();
        builder.RegisterType<FirstTimeSetupService>();
        builder.RegisterType<LuceneFullTextSearchService>().As<IFullTextSearchService>().SingleInstance()
            .WithParameter("indexName", "searchindex");
        builder.RegisterType<StartupService>().AutoActivate(); // do some stuff at startup
        builder.RegisterType<UserPreferenceService>().SingleInstance();
        
        return builder.Build();
    }
}