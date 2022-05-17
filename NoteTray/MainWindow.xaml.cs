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

            // Get the user's base note directory
            // If no preference exists, use the User Profile directory
            if (userPrefs.TryGetPreference("basePath", out string notesBasePath) == false)
            {
                string userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                userPrefs.SetPreference("basePath", userProfilePath);
                notesBasePath = userProfilePath;
            }

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