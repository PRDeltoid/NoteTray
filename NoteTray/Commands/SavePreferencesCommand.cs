using System;
using System.Windows.Input;
using NoteTrayLib.Services;

namespace NoteTray.Commands;

public class SavePreferencesCommand : ICommand
{
	private readonly PreferencesViewModel _viewModel;
	private readonly UserPreferenceService _userPrefs;

	public SavePreferencesCommand(PreferencesViewModel viewModel, UserPreferenceService userPrefs)
	{
		_viewModel = viewModel;
		_userPrefs = userPrefs;
	}
	
	public bool CanExecute(object parameter)
	{
		return true;
	}

	public void Execute(object parameter)
	{
		_userPrefs.BasePath = _viewModel.BasePath;
		_userPrefs.EditorCommand = _viewModel.EditorCommand;
		_userPrefs.NoteFileFilter = _viewModel.NoteFileFilter;
	}

	public event EventHandler CanExecuteChanged;
}