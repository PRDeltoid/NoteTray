using System;
using System.Windows;
using System.Windows.Input;

namespace NoteTray.Commands;

public class OpenPreferencesCommand : ICommand
{
	private readonly PreferencesWindow.Factory _windowFactory;

	public OpenPreferencesCommand(PreferencesWindow.Factory windowFactory)
	{
		_windowFactory = windowFactory;
	}

	public bool CanExecute(object parameter)
	{
		return true;
	}

	public void Execute(object parameter)
	{
		if (parameter is not Window parent) throw new Exception("OpenPreferencesCommand requires Execute parameter be of type 'Window'");
		_windowFactory.Invoke(parent).Show();
	}

	public event EventHandler CanExecuteChanged;
}