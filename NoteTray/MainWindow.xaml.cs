using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using NoteTray.Commands;
using Serilog;
using WindowGrabberLib;

namespace NoteTray
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly WindowSnapper _windowSnapper;
        private readonly WindowGrabber _grabber;
        private readonly WindowInteropHelper _windowHelper;

        public ICommand OpenPreferencesCommand { get; }
        
        public MainWindow(NoteListViewModel viewModel, OpenPreferencesCommand preferencesCommand)
        {
            DataContext = viewModel;
            Task.Run(viewModel.UpdateNotesList);
            OpenPreferencesCommand = preferencesCommand;
            Log.Debug("Main Window Showing");
            // Window Snapper is used to attach this window to the side of another process and keep it there
            // This is used when the ViewModel opens an editor process
            _windowSnapper = new WindowSnapper(this);
            // Window Grabber is used to allow the user to select what process they want to snap to
            _grabber = new WindowGrabber();
            InitializeComponent();
            
            // Helps get the handle of this window.
            // The usual method of using Process.GetCurrentProcess() returns a different process than the main window
            _windowHelper = new WindowInteropHelper(this);
        }

        private void Exit_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void Dock_OnClick(object sender, RoutedEventArgs e)
        {
            // Ask the user to select a window and then snap to it
            // Pass this window's handle so we can ignore it in the grabber
            _windowSnapper.Attach(await _grabber.GrabWindowAsync(_windowHelper.Handle));
        }
        
        private void Preferences_OnClick(object sender, RoutedEventArgs e)
        {
            OpenPreferencesCommand.Execute(this);
        }

        private void Settings_MouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Button button) return;

            // Manually load context since this isn't done automatically when left clicking
            button.ContextMenu.DataContext = button.DataContext;
            // Show context menu
            button.ContextMenu.IsOpen = true;
        }
    }
}