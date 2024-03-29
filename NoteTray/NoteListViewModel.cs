﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using NoteTray.Commands;
using NoteTrayLib.Extensions;
using NoteTrayLib.Models;
using NoteTrayLib.Services;
using Serilog;

namespace NoteTray;

public class NoteListViewModel : INotifyPropertyChanged  
{
    private readonly DirectoryManagerService _directoryService;
    private readonly EditorManagerService _editorService;
    private readonly IFullTextSearchService _searchService;

    private NoteListItem _selectedNote;

    private bool _showingSearchResults = false;
    private string _searchString;

    public ICommand ClearSearchCommand { get; }
    public ICommand PerformSearchCommand { get; }

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

    public string SearchString
    {
        get => _searchString;
        set
        {
            _searchString = value;
            OnPropertyChanged(nameof(SearchString));
        }
    }

    public NoteListViewModel(DirectoryManagerService directoryService, EditorManagerService editorService, FileChangeWatcher fileChangeWatcher, IFullTextSearchService searchService)
    {
        _directoryService = directoryService;
        _editorService = editorService;
        _searchService = searchService;
        
        // Command setup
        ClearSearchCommand = new ClearSearchCommand(ClearSearch);
        PerformSearchCommand = new PerformSearchCommand(PerformSearch);

        // If the SelectedNote property changes, run the Item Selection code
        PropertyChanged += OnItemSelected;
        
        // Bind to the search service so we can update the note list when a search is performed
        _searchService.SearchResultsAvailable += SearchResultsAvailable;
        
        fileChangeWatcher.FileAdded += FileChangeWatcherOnFileAdded;
        fileChangeWatcher.FileRemoved += FileChangeWatcherOnFileRemoved;
    }

    private void FileChangeWatcherOnFileRemoved(object sender, TrackedFileModel e)
    {
        // If the file being removed is in the directory we are viewing, remove it to the note list
        if (e.Path == _directoryService.CurrentDirectory && _showingSearchResults == false)
        {
            // remove any files where the filename and path match
            RemoveFromNoteList(e);
        }
    }

    private void FileChangeWatcherOnFileAdded(object sender, TrackedFileModel e)
    {
        // If the file being added is in the directory we are viewing, add it to the note list
        if (e.Path == _directoryService.CurrentDirectory && _showingSearchResults == false)
        {
            AddToNoteList(new NoteListItem() { FullPath = e.FullPath, IsDirectory = false, Name = e.FileName});
        }
    }

    private void AddToNoteList(NoteListItem itemToAdd)
    {
        // ObservableCollection can only be modied from the same thread it was created so we must use a dispatcher here
        App.Current.Dispatcher.Invoke(delegate
        {
            NoteList.Add(itemToAdd);
        }); 
    }

    private void RemoveFromNoteList(TrackedFileModel itemToRemove)
    {
        // ObservableCollection can only be modied from the same thread it was created so we must use a dispatcher here
        App.Current.Dispatcher.Invoke(delegate
        {
            // remove any files where the filename and path match
            NoteList.Remove(x => x.Name == itemToRemove.FileName && x.FullPath == itemToRemove.FullPath);
        }); 
    }

    private void SearchResultsAvailable(object sender, SearchResultEventArgs e)
    {
        Log.Debug("Search Results Available");
        NoteList.Clear(); 
        foreach (SearchResultModel result in e.Results)
        {
            NoteListItem note = new NoteListItem()
            {
                FullPath = result.FullPath,
                IsDirectory = false, // search only returns files for now
                Name = result.Name
            };
            AddToNoteList(note);
        }
        _showingSearchResults = true;
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
            _editorService.OpenInEditor(SelectedNote.FullPath);
        }
    }

    private void ClearSearch()
    {
        SearchString = "";
        // If we're showing search results, go back to regular directory results instead
        if (_showingSearchResults)
        {
            Log.Debug("Switching note list view from search results to directory results");
            UpdateNotesList();
        }
    }

    private void PerformSearch()
    {
        _searchService.Search(SearchString); 
    }
    
    public Task UpdateNotesList()
    {
        Log.Debug("Updating NoteListViewModel NoteList");
        return Task.Run(() =>
        {
            NoteList.Clear();
            // If we are not at the root, show the parent directory to allow the user to move up
            if (_directoryService.IsRootDirectory == false)
            {
                // Format the name of the item so it reads ".."
                AddToNoteList(new NoteListItem() { FullPath = _directoryService.GetParent().FullPath, IsDirectory = true, Name = ".." });
            }

            // Load a list of directories and files
            foreach (NoteListItem dirItem in _directoryService.GetChildDirectories())
            {
                AddToNoteList(dirItem);
            }

            foreach (NoteListItem fileitem in _directoryService.GetChildFiles())
            {
                AddToNoteList(fileitem);
            }

            _showingSearchResults = false;
            OnPropertyChanged(nameof(NoteList));
        });
    }
    
    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
}