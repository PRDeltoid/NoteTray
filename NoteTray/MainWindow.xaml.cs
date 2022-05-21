using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NoteTrayLib.Services;
using Serilog;

namespace NoteTray
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IFullTextSearchService _searchService;
        private readonly WindowSnapper _windowSnapper;
        
        public MainWindow(NoteListViewModel viewModel, StartupScanService startupScanner, FirstTimeSetupService firstTimeSetup, IFullTextSearchService searchService)
        {
            _searchService = searchService;
            // Initial setup
            // TODO: Should either one of these be moved elsewhere? AutoActivate maybe? StartUp glue class with autoactivate?
            firstTimeSetup.Setup();
            startupScanner.Scan();
            
            DataContext = viewModel;
            // Window Snapper is used to attach this window to the side of another process and keep it there
            // This is used when the ViewModel opens an editor process
            _windowSnapper = new WindowSnapper(this);
            InitializeComponent();
            
            // Enable window snapping to the viewmodel's Process property
            viewModel.PropertyChanged += OnProcessPropertyChanged;
        }

        private void OnProcessPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "EditorProcess") return;
            if (sender is not NoteListViewModel viewModel) return;
            
            Log.Debug("Attaching to editor process: {0}",viewModel.EditorProcess.MainWindowTitle);
            _windowSnapper.Attach(viewModel.EditorProcess.MainWindowHandle);
        }

        private void Exit_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Preferences_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Settings_MouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Button button) return;
            
            // Manually load context since this isn't done automatically when left clicking
            button.ContextMenu.DataContext = button.DataContext;
            // Show context menu
            button.ContextMenu.IsOpen = true;
        }
        
        private void Search_EnterPressed(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            e.Handled = true;
            Log.Debug("Enter pressed in search box");
            
            _searchService.Search(txtSearchbox.Text);
        }
    }
}