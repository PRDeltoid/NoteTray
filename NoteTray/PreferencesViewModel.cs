using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using NoteTray.Commands;
using NoteTrayLib.Services;

namespace NoteTray;

public class PreferencesViewModel : INotifyPropertyChanged  
{
	private readonly UserPreferenceService _userPrefs;
	private string _basePath;
	private string _fileFilter;
	private string _editorCommand;

	public ICommand SaveChangesCommand { get; }
	public ICommand CancelChangesCommand { get; }
	
	#region Preference Properties
	public string BasePath
	{
		get => _basePath;
		set {  _basePath = value; OnPropertyChanged(nameof(BasePath));}
	}

	public string EditorCommand
	{
		get => _editorCommand;
		set { _editorCommand = value; OnPropertyChanged(nameof(EditorCommand));}
	}
		
	public string NoteFileFilter
	{
		get => _fileFilter;
		set {  _fileFilter = value; OnPropertyChanged(nameof(NoteFileFilter));}
	}
	#endregion

	public PreferencesViewModel(UserPreferenceService userPrefs)
	{
		_userPrefs = userPrefs;
		SaveChangesCommand = new SavePreferencesCommand(this, userPrefs);
		CancelChangesCommand = new CancelPreferencesCommand(this, userPrefs);
		
		// Load starting values
		BasePath = _userPrefs.BasePath;
		NoteFileFilter = _userPrefs.NoteFileFilter;
		EditorCommand = _userPrefs.EditorCommand;
	}
	
	#region INotifyPropertyChanged
	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
	#endregion
}