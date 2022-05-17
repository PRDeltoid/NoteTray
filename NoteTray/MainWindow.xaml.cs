using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NoteTrayLib.Models;
using NoteTrayLib.Services;
using Serilog;

namespace NoteTray
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DirectoryManagerService _directoryService;

        public MainWindow(DirectoryManagerService directoryService, UserPreferenceService userPrefs)
        {
            _directoryService = directoryService;
            InitializeComponent();

            UpdateNotesList();

            // Remove placeholder "Search" text when the user selects the searchbox
            txtSearchbox.GotFocus += RemoveText;
            txtSearchbox.LostFocus += AddText;
            
            // Move into directories on click
            lstNoteFiles.SelectionChanged += ItemSelected;
        }

        private void UpdateNotesList()
        {
            lstNoteFiles.Items.Clear();
            // If we are not at the root, show the parent directory to allow the user to move up
            if (_directoryService.IsRootDirectory == false)
            {
                // Format the name of the item so it reads ".."
                lstNoteFiles.Items.Add(new NoteListItem() { FullPath = _directoryService.GetParent().FullPath, IsDirectory = true, Name = ".."});
            }
            
            // Load a list of directories and files
            foreach (NoteListItem dirItem in _directoryService.GetChildDirectories())
            {
                lstNoteFiles.Items.Add(dirItem);
            }
            foreach (NoteListItem fileitem in _directoryService.GetChildFiles())
            {
                lstNoteFiles.Items.Add(fileitem);
            } 
        }

        private void ItemSelected(object sender, SelectionChangedEventArgs e)
        {
            // Exit early if no items are selected. This tends to happen when the list is cleared and items are re-added
            if (e.AddedItems.Count == 0) return;
            // Since our list is single-select only, we can safely use First() and not worry about losing results
            NoteListItem clickedItem = e.AddedItems.Cast<NoteListItem>().First();
            Log.Debug("Item selected: {fullPath}", clickedItem.FullPath);
            
            // Exit early if the clicked item is not a directory
            if (clickedItem.IsDirectory)
            {
                _directoryService.SetCurrentDirectory(clickedItem.FullPath);
                UpdateNotesList();
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