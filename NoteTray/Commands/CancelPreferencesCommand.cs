using System;
using System.Windows.Input;
using NoteTrayLib.Services;

namespace NoteTray.Commands;

public class CancelPreferencesCommand : ICommand
{
	private readonly PreferencesViewModel _viewModel;
	private readonly UserPreferenceService _userPrefs;

	public CancelPreferencesCommand(PreferencesViewModel viewModel, UserPreferenceService userPrefs)
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
		_viewModel.BasePath = _userPrefs.BasePath;
		_viewModel.NoteFileFilter = _userPrefs.NoteFileFilter;
		_viewModel.EditorCommand = _userPrefs.EditorCommand;
	}

	public event EventHandler CanExecuteChanged;
}