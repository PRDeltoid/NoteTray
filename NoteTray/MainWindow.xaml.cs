using System;
using System.ComponentModel;
using System.Windows;
using Serilog;

namespace NoteTray
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly WindowSnapper _windowSnapper;

        public MainWindow(NoteListViewModel viewModel)
        {
            DataContext = viewModel;
            // Window Snapper is used to attach this window to the side of another process and keep it there
            // This is used when the ViewModel opens an editor process
            _windowSnapper = new WindowSnapper(this);
            InitializeComponent();
            
            // Enable window snapping to the viewmodel's Process property
            viewModel.PropertyChanged += OnProcessPropertyChanged;

            // Remove placeholder "Search" text when the user selects the searchbox
            txtSearchbox.GotFocus += RemoveText;
            txtSearchbox.LostFocus += AddText;
        }

        private void OnProcessPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "EditorProcess") return;
            NoteListViewModel viewModel = sender as NoteListViewModel;
            
            Log.Debug("Attaching to editor process: {0}",viewModel.EditorProcess.MainWindowTitle);
            _windowSnapper.Attach(viewModel.EditorProcess.MainWindowHandle);
        }

        private void RemoveText(object sender, EventArgs e)
        {
            if (txtSearchbox.Text == "Search...")
            {
                txtSearchbox.Text = "";
            }
        }

        private void AddText(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearchbox.Text))
                txtSearchbox.Text = "Search...";
        }
    }
}