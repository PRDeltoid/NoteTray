using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using NoteTrayLib.Models;
using NoteTrayLib.Services;
using Serilog;

namespace NoteTray;

public class NoteListViewModel : INotifyPropertyChanged  
{
    private readonly DirectoryManagerService _directoryService;
    private readonly EditorManagerService _editorService;

    private NoteListItem _selectedNote;

    private Process _editorProcess;
    
    public Process EditorProcess
    {
        get => _editorProcess;
        set
        {
            _editorProcess = value;
            OnPropertyChanged(nameof(EditorProcess));
        }
    }

    public ObservableCollection<NoteListItem> NoteList { get; } = new ObservableCollection<NoteListItem>();

    public NoteListItem SelectedNote
    {
        get => _selectedNote;
        set
        {
            _selectedNote = value;
            OnPropertyChanged(nameof(SelectedNote));
        }
    }

    public NoteListViewModel(DirectoryManagerService directoryService, EditorManagerService editorService, IFullTextSearchService searchService)
    {
        _directoryService = directoryService;
        _editorService = editorService;

        // If the SelectedNote property changes, run the Item Selection code
        PropertyChanged += OnItemSelected;
        
        // Bind to the search service so we can update the note list when a search is performed
        searchService.SearchResultsAvailable += SearchResultsAvailable;
            
        UpdateNotesList();
    }

    private void SearchResultsAvailable(object sender, SearchResultEventArgs e)
    {
        NoteList.Clear(); 
        foreach (SearchResultModel result in e.Results)
        {
            NoteListItem note = new NoteListItem()
            {
                FullPath = result.FullPath,
                IsDirectory = false, // search only returns files for now
                Name = result.Name
            };
            NoteList.Add(note); 
        }
        OnPropertyChanged(nameof(NoteList));
    }

    private void OnItemSelected(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != "SelectedNote" || SelectedNote == null) return;
        Log.Debug("Item selected: {fullPath}", SelectedNote.FullPath);
            
        // If it's a directory, navigate to it
        // If it's a file, open it
        if (SelectedNote.IsDirectory)
        {
            _directoryService.SetCurrentDirectory(SelectedNote.FullPath);
            UpdateNotesList();
        }
        else
        {
            EditorProcess = _editorService.OpenInEditor(SelectedNote.FullPath);
        }
    }
    
    private void UpdateNotesList()
    {
        NoteList.Clear();
        // If we are not at the root, show the parent directory to allow the user to move up
        if (_directoryService.IsRootDirectory == false)
        {
            // Format the name of the item so it reads ".."
            NoteList.Add(new NoteListItem() { FullPath = _directoryService.GetParent().FullPath, IsDirectory = true, Name = ".."});
        }
            
        // Load a list of directories and files
        foreach (NoteListItem dirItem in _directoryService.GetChildDirectories())
        {
            NoteList.Add(dirItem);
        }
        foreach (NoteListItem fileitem in _directoryService.GetChildFiles())
        {
            NoteList.Add(fileitem);
        }

        OnPropertyChanged(nameof(NoteList));
    }
    
    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
}