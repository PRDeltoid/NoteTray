using System.ComponentModel;

namespace NoteTrayLib.Services;

public class UserPreferenceService : INotifyPropertyChanged
{
	private const string BASE_PATH = "basePath";
	private const string EDIT_COMMAND_TEMPLATE = "editCommandTemplate";
	private const string NOTE_FILTER_FILTER = "noteFileFilter";
	private const string FIRST_RUN_FLAG = "firstRunFlag";

	private readonly UserPreferenceDbService _prefDbService;

	public UserPreferenceService(UserPreferenceDbService prefDbService)
	{
		_prefDbService = prefDbService;
	}
	public string BasePath
	{
		get => TryGetOrNull<string>(BASE_PATH);
		set => Set(BASE_PATH, value);
	}

	public string EditorCommand
	{
		get => TryGetOrNull<string>(EDIT_COMMAND_TEMPLATE);
		set => Set(EDIT_COMMAND_TEMPLATE, value);
	}
		
	public string NoteFileFilter
	{
		get => TryGetOrNull<string>(NOTE_FILTER_FILTER);
		set => Set(NOTE_FILTER_FILTER, value);
	}
	
	public bool? FirstRunFlag
	{
		get => TryGetStructOrNull<bool>(FIRST_RUN_FLAG);
		set => Set(FIRST_RUN_FLAG, value.ToString());
	}

	private void Set(string prefName, object val)
	{
		_prefDbService.SetPreference(prefName, val);
		OnPropertyChanged(prefName);
	}

	private T TryGetOrNull<T>(string prefName) where T : class
	{
		if (_prefDbService.TryGetPreference(prefName, out T returnVal))
		{
			return returnVal;
		}
		else
		{
			return null;
		}
	}
	
	/// <summary>
	/// Special implementation needed for accepting structs and returning nullable structs
	/// </summary>
	private T? TryGetStructOrNull<T>(string prefName) where T : struct
	{
		if (_prefDbService.TryGetPreference(prefName, out T returnVal))
		{
			return returnVal;
		}
		else
		{
			return null;
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual void OnPropertyChanged(string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}