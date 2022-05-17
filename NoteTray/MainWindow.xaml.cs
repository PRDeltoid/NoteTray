using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NoteTrayLib;
using NoteTrayLib.Models;
using Serilog;

namespace NoteTray
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DirectoryService _directoryService;

        public MainWindow(DirectoryService directoryService, UserPreferenceService userPrefs)
        {
            _directoryService = directoryService;
            InitializeComponent();

            // Get the user's base note directory
            // If no preference exists, use the User Profile directory
            if (userPrefs.TryGetPreference("basePath", out string notesBasePath) == false)
            {
                string userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                userPrefs.SetPreference("basePath", userProfilePath);
                notesBasePath = userProfilePath;
            }

            // Load a list of directories and files
            foreach (NoteListItem dirItem in DirectoryService.GetChildDirectories(notesBasePath))
            {
                lstNoteFiles.Items.Add(dirItem);
            }
            foreach (NoteListItem fileitem in DirectoryService.GetChildFiles(notesBasePath))
            {
                lstNoteFiles.Items.Add(fileitem);
            }

            // Remove placeholder "Search" text when the user selects the searchbox
            txtSearchbox.GotFocus += RemoveText;
            txtSearchbox.LostFocus += AddText;
            
            // Move into directories on click
            lstNoteFiles.SelectionChanged += ItemSelected;
        }

        private void ItemSelected(object sender, SelectionChangedEventArgs e)
        {
            Log.Debug("Item selected: {e}", e.AddedItems.Cast<NoteListItem>().First().FullPath);
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