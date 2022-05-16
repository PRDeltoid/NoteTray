using System;
using System.Windows;
using NoteTrayLib;

namespace NoteTray
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DirectoryService _directoryService;

        public MainWindow(DirectoryService directoryService)
        {
            _directoryService = directoryService;
            InitializeComponent();
            // Remove placeholder "Search" text when the user selects the searchbox
            txtSearchbox.GotFocus += RemoveText;
            txtSearchbox.LostFocus += AddText;

            const string notesBasePath = "C:\\Users\\Taylor";
            // Load a list of directories and files
            foreach (string dirName in DirectoryService.GetChildDirectories(notesBasePath))
            {
                lstNoteFiles.Items.Add(dirName + "\\");
            }
            foreach (string fileName in DirectoryService.GetChildFiles(notesBasePath))
            {
                lstNoteFiles.Items.Add(fileName);
            }
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