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

            foreach (string dirName in _directoryService.GetChildDirectories("C:\\Users\\Taylor"))
            {
                lstNoteFiles.Items.Add(dirName + "\\");
            }
            foreach (string fileName in _directoryService.GetChildFiles("C:\\Users\\Taylor"))
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